using AutoMapper;
using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NexusStack.Core.Dtos.ScheduleTasks;
using NexusStack.Core.Entities.Schedules;
using NexusStack.Core.Services.Interfaces;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure.Constants;
using NexusStack.Redis;
using NexusStack.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NexusStack.Core.Schedules
{
    /// <summary>
    /// 实现一个后台定时任务的抽象类
    /// </summary>
    public abstract class CronScheduleService(IServiceScopeFactory serviceFactory) : BackgroundService
    {
        /// <summary>
        /// 定时表达式
        /// </summary>
        protected abstract string Expression { get; set; }

        /// <summary>
        /// 是否单例运行
        /// 如果为 True 在部署多个节点时，使用 Redis 作为分布式锁，限制每次只有一个服务执行
        /// </summary>
        protected abstract bool Singleton { get; }

        /// <summary>
        /// 获取下一次任务执行时间
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset GetNextTime()
        {
            var nextOccurrence = CronExpression.Parse(Expression, CronFormat.IncludeSeconds)
                .GetNextOccurrence(DateTime.UtcNow, TimeZoneInfo.Local);
            
            if (!nextOccurrence.HasValue)
            {
                return DateTimeOffset.MinValue;
            }
            
            // 将本地时间转换为 DateTimeOffset（带本地时区偏移）
            var localDateTimeOffset = new DateTimeOffset(nextOccurrence.Value, TimeZoneInfo.Local.GetUtcOffset(nextOccurrence.Value));
            
            // 返回 UTC 时间，便于后续统一比较
            return localDateTimeOffset.ToUniversalTime();
        }

        protected abstract Task ProcessAsync(CancellationToken cancellationToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var code = this.GetType().FullName;

            // CronFormat.IncludeSeconds 表达式中秒字段必须被指定
            DateTimeOffset nextExcuteTime;
            try
            {
                nextExcuteTime = GetNextTime();
            }
            catch (Exception ex)
            {
                // 如果初始表达式无效，记录错误后退出
                using var scope = serviceFactory.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<CronScheduleService>>();
                logger.LogCritical(ex, $"定时任务 {code} 的Cron表达式无效，服务无法启动");
                return; // 退出，不启动服务
            }

            if (nextExcuteTime != DateTimeOffset.MinValue)
            {
                Console.WriteLine($"{code}任务下次执行时间:{nextExcuteTime.ToLocalTime()}");
            }

            //用于准确测量时间间隔
            Stopwatch stopwatch = new();

            while (!stoppingToken.IsCancellationRequested)
            {
                // 把这个作用域和服务放到循环内部，循环结束便进行释放
                using var scope = serviceFactory.CreateAsyncScope();

                var scheduleTaskService = scope.ServiceProvider.GetRequiredService<IScheduleTaskService>();
                var recordService = scope.ServiceProvider.GetRequiredService<IServiceBase<ScheduleTaskRecord>>();
                var redisService = scope.ServiceProvider.GetRequiredService<IRedisService>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<CronScheduleService>>();
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                var scheduleTask = await redisService.GetAsync<ScheduleTaskExecuteDto>(CoreRedisConstants.ScheduleTaskCache.Format(code));
                if (scheduleTask is null || !scheduleTask.IsEnable)
                {
                    // 延迟重新执行
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                if (DateTimeOffset.UtcNow < nextExcuteTime)
                {
                    // 延迟重新执行
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                var lockName = $"ScheduleTask:{code}.{nextExcuteTime}";
                var scheduleTaskRecord = new ScheduleTaskRecord();
                scheduleTaskRecord.ScheduleTaskId = scheduleTask.Id;
                var executionSkipped = false;
                
                try
                {
                    stopwatch.Restart();
                    scheduleTaskRecord.ExpressionTime = nextExcuteTime;
                    scheduleTaskRecord.ExecuteStartTime = DateTimeOffset.UtcNow;
                    if (!Singleton)
                    {
                        // ConfigureAwait允许你配置异步等待的行为。如果你使用ConfigureAwait(false)，
                        // 则表示你不需要恢复到原始上下文，而是允许异异步操作在任何上下文中继续执行。
                        // 这通常可以提高性能，因为避免了上下文切换的开销。
                        await ProcessAsync(stoppingToken).ConfigureAwait(false);
                    }
                    else
                    {
                        // 在那台服务器上获取到了锁，便在那台服务器上进行执行?
                        if (await redisService.SetAsync(lockName, null, TimeSpan.FromMinutes(1), CSRedis.RedisExistence.Nx))
                        {
                            await ProcessAsync(stoppingToken).ConfigureAwait(false);
                        }
                        else
                        {
                            logger.LogInformation($"定时任务 {code} 执行时未获取到锁，本次放弃执行");
                            executionSkipped = true;
                        }
                    }

                    if (!executionSkipped)
                    {
                        scheduleTask.LastExecuteTime = DateTimeOffset.UtcNow;
                        scheduleTaskRecord.ExecuteEndTime = DateTimeOffset.UtcNow;
                        scheduleTaskRecord.IsSuccess = true;
                        
                        try
                        {
                            await recordService.InsertAsync(scheduleTaskRecord);
                        }
                        catch (Exception recordEx)
                        {
                            logger.LogError(recordEx, $"保存任务执行记录失败: {code}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    scheduleTaskRecord.ExecuteEndTime = DateTimeOffset.UtcNow;
                    scheduleTaskRecord.ErrorMessage = ex.Message;
                    scheduleTaskRecord.IsSuccess = false;
                    
                    try
                    {
                        await recordService.InsertAsync(scheduleTaskRecord);
                    }
                    catch (Exception recordEx)
                    {
                        logger.LogError(recordEx, $"保存任务失败记录时发生异常: {code}");
                    }
                    
                    logger.LogError(ex, $"执行 {code} 任务发生错误");
                }

                // 无论成功还是失败，都要更新下次执行时间（除非未获取到锁）
                if (!executionSkipped)
                {
                    try
                    {
                        // 先检查是否有新的表达式需要更新
                        if (!scheduleTask.Expression.IsNullOrEmpty() && scheduleTask.Expression != this.Expression)
                        {
                            this.Expression = scheduleTask.Expression;
                        }
                        
                        nextExcuteTime = GetNextTime();

                        scheduleTask.Expression = this.Expression;
                        if (nextExcuteTime != DateTimeOffset.MinValue)
                        {
                            scheduleTask.NextExecuteTime = nextExcuteTime;
                        }

                        var schedule = await scheduleTaskService.GetAsync(item => item.Code == code);
                        mapper.Map(scheduleTask, schedule);
                        await scheduleTaskService.UpdateAsync(schedule);
                    }
                    catch (Exception updateEx)
                    {
                        logger.LogError(updateEx, $"更新任务下次执行时间失败: {code}，可能是Cron表达式无效");
                        // 不中断循环，继续运行
                    }
                }
            }
        }
    }
}
