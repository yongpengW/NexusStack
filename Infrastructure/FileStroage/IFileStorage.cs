using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.FileStroage
{
    public interface IFileStorage
    {
        /// <summary>
        /// 存储类型
        /// </summary>
        FileStorageType StorageType { get; }

        string GetAbsolutePath(string relativePath);

        /// <summary>
        /// 获取文件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<byte[]> GetAsync(string key);

        /// <summary>
        /// 上传文件
        /// 上传中心上传文件
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="ossFolderName"></param>
        /// <param name="fileName"></param>
        /// <param name="isOverride">是否覆盖上传</param>
        /// <returns></returns>
        Task<Result<string>> UploadAsync(Stream stream, string ossFolderName, string fileName, bool isOverride = false);

        /// <summary>
        /// 获取文件的预签名地址
        /// 上传中心获取文件的预签名地址
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string GeneratePresignedUri(string key);

        Task<string> UploadAsync(byte[] bytes, string key);

        string GetDownloadCenterUri(string key);

        byte[] GetDownloadCenterFile(string key);

        /// <summary>
        /// 检查文件是否存在
        /// 上传中心检查文件是否存在
        /// </summary>
        /// <param name="fileKey"></param>
        /// <returns></returns>
        bool DoesObjectExist(string fileKey);

        /// <summary>
        /// 删除文件
        /// 上传中心删除文件
        /// </summary>
        /// <param name="fileKey"></param>
        void DeleteObject(string fileKey);

        /// <summary>
        /// 获取文件
        /// 从上传中心获取文件
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        byte[] GetFile(string bucket, string key);

        /// <summary>
        /// 从配置中心获取文件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        byte[] GetConfigFile(string key);
        /// <summary>
        /// 上传文件
        /// 上传配置地址（微信支付秘钥）上传文件
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="ossFolderName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task<Result<string>> UploadConfigAsync(Stream stream, string ossFolderName, string fileName);
        /// <summary>
        /// 获取文件的预签名地址
        /// 配置地址（微信支付秘钥）获取文件的预签名地址
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string GeneratePresignedConfigUri(string key);
    }
}
