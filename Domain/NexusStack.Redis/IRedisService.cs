using CSRedis;
using NexusStack.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Redis
{
    public interface IRedisService
    {
        /// <summary>
        /// 查看服务是否运行
        /// </summary>
        /// <returns></returns>
        bool PingAsync();

        /// <summary>
        /// 根据key获取缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// 设置指定key的缓存值(不过期)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<bool> SetAsync(string key, object value);

        /// <summary>
        /// 设置指定key的缓存值(可设置过期时间和Nx、Xx)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        /// <param name="exists"></param>
        /// <returns></returns>
        Task<bool> SetAsync(string key, object value, TimeSpan expire, RedisExistence? exists = null);

        /// <summary>
        /// 设置指定key的缓存值(可设置过期秒数和Nx、Xx)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireSeconds">过期时间单位为秒</param>
        /// <param name="exists"></param>
        /// <returns></returns>
        Task<bool> SetAsync(string key, object value, int expireSeconds = -1, RedisExistence? exists = null);

        /// <summary>
        /// 删除Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<long> DeleteAsync(string key);


        Task<dynamic> ScanAsync(PagedQueryModelBase model);

        /// <summary>
        /// 添加集合成员
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expireSeconds"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        Task<long> SAddAsync(string key, int expireSeconds = -1, params string[] members);

        /// <summary>
        /// 删除集合中的指定成员
        /// </summary>
        /// <param name="key"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        Task<long> SRemAsync(string key, params string[] members);

        /// <summary>
        /// 获取集合中的所有成员
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<string[]> SMembersAsync(string key);

        /// <summary>
        /// 判断指定成员是否是集合的成员
        /// </summary>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        Task<bool> SIsMemberAsync(string key, string member);

        /// <summary>
        /// 添加哈希
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// /// <param name="expireSeconds"></param>
        /// <returns></returns>
        Task<bool> HSetAsync(string key, string field, object value, int expireSeconds = -1);

        /// <summary>
        /// 获取哈希指定值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        Task<T> HGetAsync<T>(string key, string field);

        /// <summary>
        /// 获取哈希所有值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<Dictionary<string, T>> HGetAllAsync<T>(string key);

        /// <summary>
        /// 获取哈希所有值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> HGetAllAsync(string key);

        /// <summary>
        /// 删除哈希值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        Task<long> HDelAsync(string key, params string[] fields);

        /// <summary>
        /// 判断哈希值是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        Task<bool> HExistsAsync(string key, string field);
    }
}
