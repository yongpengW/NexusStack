using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NexusStack.Core.Entities.Schedules;
using NexusStack.Core.Services.Interfaces;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Utils;
using NexusStack.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NexusStack.Core.HostedServices
{
    /// <summary>
    /// 应该为所有种子数据设置一个版本号，以避免每次启动的时候执行
    /// </summary>
    public class ExecuteSeedDataService(IServiceScopeFactory scopeFactory, IServiceProvider services, ILogger<ExecuteSeedDataService> logger, IConfiguration configuration) : BackgroundService
    {
        /// <summary>
        /// 当前配置文件的路径
        /// </summary>
        private string currentConfigPath = null;

        /// <summary>
        /// 更新定时任务记录
        /// </summary>  
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateCronTask(SeedDataTask model)
        {
            using var scope = scopeFactory.CreateScope();
            var seedDataService = scope.ServiceProvider.GetRequiredService<ISeedDataTaskCoreService>();

            await seedDataService.UpdateAsync(model);
        }

        /// <summary>
        /// 获取任务是否需要执行的状态
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool GetExecuteStatus(SeedDataTask model)
        {
            // 为空，则表示数据没有写入在json配置文件中
            if (string.IsNullOrEmpty(currentConfigPath))
            {
                return true;
            }
            else
            {
                // 判断json文件是否保存过，通过最后保存时间来判断
                var file = new FileInfo(currentConfigPath);

                // 将文件的本地时间转换为 UTC，确保与数据库中的时间一致
                var fileLastWriteTimeUtc = new DateTimeOffset(file.LastWriteTime, TimeZoneInfo.Local.GetUtcOffset(file.LastWriteTime)).ToUniversalTime();
                
                // 比较完整的时间戳，而不是只比较时间部分
                if (model.LastWriteTime < fileLastWriteTimeUtc)
                {
                    model.LastWriteTime = fileLastWriteTimeUtc;
                    return true;
                }

                // 如果执行失败，则需要重新执行
                if (model.ExecuteStatus == ExecuteStatus.Fail)
                {
                    return true;
                }
                return false;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation($"开始执行种子数据更新任务");

            var stopwatch = new Stopwatch();

            stopwatch.Start();
            using var scope = scopeFactory.CreateScope();
            var redisDatabaseProvider = scope.ServiceProvider.GetRequiredService<IRedisService>();

            var seedInstants = scope.ServiceProvider.GetServices<ISeedData>();

            var cronTaskCoreService = scope.ServiceProvider.GetRequiredService<ISeedDataTaskCoreService>();

            var list = await cronTaskCoreService.GetListAsync();

            foreach (var seed in seedInstants.OrderBy(a => a.Order))
            {
                var code = seed.GetType().FullName;
                currentConfigPath = seed.ConfigPath;
                var taskId = Guid.NewGuid().ToString();

                var model = list.FirstOrDefault(item => item.Code == code);

                try
                {
                    //判断该任务是否启用，并且是否已同步到数据库，如果没有同步，则要写入数据库
                    if (model is not null && !model.IsEnable)
                    {
                        break; //如果任务已经禁用，则不再执行
                    }

                    if (model is null)
                    {
                        model = await cronTaskCoreService.InsertAsync(new SeedDataTask
                        {
                            Name = DocsHelper.GetTypeComments(seed.GetType().Assembly.GetName().Name, seed.GetType()),
                            Code = code,
                            ConfigPath = currentConfigPath,
                            IsEnable = true
                        });

                    }
                    //判断任务是否需要执行任务
                    if (GetExecuteStatus(model))
                    {
                        logger.LogInformation($"开始执行[{code}]");

                        if (await redisDatabaseProvider.SetAsync(code, taskId, TimeSpan.FromMinutes(5), CSRedis.RedisExistence.Nx))
                        {
                            await seed.ApplyAsync(model);
                            model.ExecuteStatus = ExecuteStatus.Success;
                            model.ExecuteTime = DateTimeOffset.UtcNow;
                        }

                        logger.LogInformation($"[{code}]执行完成");
                    }

                }
                catch (Exception ex)
                {
                    model.ExecuteStatus = ExecuteStatus.Fail;
                    model.ExecuteTime = DateTimeOffset.UtcNow;
                    logger.LogError(ex, ex.Message);
                }
                finally
                {
                    await UpdateCronTask(model);
                    await redisDatabaseProvider.DeleteAsync(code);
                }
            }

            stopwatch.Stop();
            logger.LogInformation($"种子数据更新任务执行完成，耗时:{stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
