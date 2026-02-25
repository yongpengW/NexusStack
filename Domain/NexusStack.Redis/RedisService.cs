using CSRedis;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace NexusStack.Redis
{
    public class RedisService : IRedisService, IScopedDependency
    {
        public async Task<long> DeleteAsync(string key)
        {
            return await RedisHelper.DelAsync(key);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            return await RedisHelper.GetAsync<T>(key);
        }

        public bool PingAsync()
        {
            return RedisHelper.Ping();
        }

        public async Task<dynamic> ScanAsync(PagedQueryModelBase model)
        {
            List<string> list = new List<string>();

            //根据model.Keyword进行模糊匹配
            var scanResult = await RedisHelper.ScanAsync(model.Page, $"*{model.Keyword}*", model.Limit);
            list.AddRange(scanResult.Items);

            var values = await RedisHelper.MGetAsync(list.ToArray());

            var resultDictionary = list.Zip(values, (key, value) => new { key, value })
                                            .ToDictionary(item => item.key, item => item.value);
            dynamic result = new ExpandoObject();
            result.Items = resultDictionary;
            result.Cursor = scanResult.Cursor;  // 下一次要通过这个Cursor获取下一页的keys
            return result;
        }

        public async Task<bool> SetAsync(string key, object value)
        {
            return await RedisHelper.SetAsync(key, value);
        }

        public async Task<bool> SetAsync(string key, object value, TimeSpan expire, RedisExistence? exists = null)
        {
            return await RedisHelper.SetAsync(key, value, expire, exists);
        }

        public async Task<bool> SetAsync(string key, object value, int expireSeconds = -1, RedisExistence? exists = null)
        {
            return await RedisHelper.SetAsync(key, value, expireSeconds, exists);
        }


        public async Task<long> SAddAsync(string key, int expireSeconds = -1, params string[] members)
        {
            var count = await RedisHelper.SAddAsync(key, members);
            await RedisHelper.ExpireAsync(key, expireSeconds);
            return count;
        }

        public async Task<long> SRemAsync(string key, params string[] members)
        {
            return await RedisHelper.SRemAsync(key, members);
        }

        public async Task<string[]> SMembersAsync(string key)
        {
            return await RedisHelper.SMembersAsync(key);
        }

        public async Task<bool> SIsMemberAsync(string key, string member)
        {
            return await RedisHelper.SIsMemberAsync(key, member);
        }

        public async Task<bool> HSetAsync(string key, string field, object value, int expireSeconds = -1)
        {
            await RedisHelper.HSetAsync(key, field, value);
            return await RedisHelper.ExpireAsync(key, expireSeconds);
        }

        public async Task<T> HGetAsync<T>(string key, string field)
        {
            return await RedisHelper.HGetAsync<T>(key, field);
        }

        public async Task<Dictionary<string, string>> HGetAllAsync(string key)
        {
            return await RedisHelper.HGetAllAsync(key);
        }

        public async Task<Dictionary<string, T>> HGetAllAsync<T>(string key)
        {
            return await RedisHelper.HGetAllAsync<T>(key);
        }

        public async Task<long> HDelAsync(string key, params string[] fields)
        {
            return await RedisHelper.HDelAsync(key, fields);
        }

        public async Task<bool> HExistsAsync(string key, string field)
        {
            return await RedisHelper.HExistsAsync(key, field);
        }
    }
}
