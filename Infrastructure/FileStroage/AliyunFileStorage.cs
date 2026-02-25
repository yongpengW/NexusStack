using Aliyun.OSS;
using Aliyun.OSS.Common;
using Microsoft.Extensions.Options;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Exceptions;
using NexusStack.Infrastructure.Models;
using NexusStack.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NexusStack.Infrastructure.FileStroage
{
    /// <summary>
    /// 阿里云文件存储
    /// </summary>
    public class AliyunFileStorage(IOptionsSnapshot<AliyunOSSOption> storageOptions) : IFileStorage, IScopedDependency
    {
        public FileStorageType StorageType => FileStorageType.Aliyun;

        public async Task<Result<string>> UploadAsync(Stream stream, string ossFolderName, string fileName, bool isOverride = false)
        {
            //To do 上传的配置准备全部做到门店设置里
            var updateloadRes = new Result<string>();
            string ossPath = string.Empty;
            if (isOverride)
            {
                //采用覆盖上传方式时，不生成唯一文件名 在客户端操作时，先判断文件是否存在，存在先删除旧文件，再上传新文件
                //适应于各个门店文件的上传
                ossPath = $"{ossFolderName}/{fileName}";
            }
            else
            {
                //因为采用生成唯一id作为文件名，每件文件都是唯一保存在oss中 适用于下载中心的文件
                var uniqueName = fileName.GenerateUniqueFileName();
                ossPath = $"{ossFolderName}/{uniqueName}";
            }
            //考虑到门店文件的时效性 需要没有过期时间或者延长过期时间
            //var expireTime = DateTime.Now.AddDays(storageOptions.Value.ExpirationDay);
            var client = new OssClient(storageOptions.Value.UploadCenter.Endpoint, storageOptions.Value.UploadCenter.AccessKeyId, storageOptions.Value.UploadCenter.AccessKeySecret);
            //var result = client.PutObject(storageOptions.Value.BucketName, ossPath, stream, new ObjectMetadata() { ExpirationTime = expireTime });
            var result = client.PutObject(storageOptions.Value.UploadCenter.BucketName, ossPath, stream);
            stream.Dispose();
            if (result.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Upload oss error : {storageOptions.Value.UploadCenter.BucketName}/{ossPath}");
            }
            else
            {
                updateloadRes.Success = true;
                updateloadRes.Data = ossPath; //client.GeneratePresignedUri(storageOptions.Value.BucketName, ossPath, expireTime, SignHttpMethod.Get).ToString();

                //if (storageOptions.Value.IsRemote)
                //{
                //    updateloadRes.Data = updateloadRes.Data.Replace("http", "https");
                //}
            }
            return updateloadRes;
        }

        public string GeneratePresignedUri(string key)
        {
            var client = new OssClient(storageOptions.Value.UploadCenter.Endpoint, storageOptions.Value.UploadCenter.AccessKeyId, storageOptions.Value.UploadCenter.AccessKeySecret);
            var expiration = DateTime.Now.AddDays(storageOptions.Value.UploadCenter.ExpirationDay);
            return client.GeneratePresignedUri(storageOptions.Value.UploadCenter.BucketName, key, expiration, SignHttpMethod.Get).ToString();
        }

        public bool DoesObjectExist(string fileKey)
        {
            var client = new OssClient(storageOptions.Value.UploadCenter.Endpoint, storageOptions.Value.UploadCenter.AccessKeyId, storageOptions.Value.UploadCenter.AccessKeySecret);
            return client.DoesObjectExist(storageOptions.Value.UploadCenter.BucketName, fileKey);
        }

        public void DeleteObject(string fileKey)
        {
            try
            {
                var client = new OssClient(storageOptions.Value.UploadCenter.Endpoint, storageOptions.Value.UploadCenter.AccessKeyId, storageOptions.Value.UploadCenter.AccessKeySecret);
                client.DeleteObject(storageOptions.Value.UploadCenter.BucketName, fileKey);
            }
            catch (OssException ex)
            {
                throw new Exception($"删除文件失败：Failed with error code: {ex.ErrorCode}; Error info: {ex.Message}. \nRequestID:{ex.RequestId}\tHostID:{ex.HostId}");
            }
        }

        public byte[] GetFile(string bucket, string key)
        {
            try
            {
                var client = new OssClient(storageOptions.Value.UploadCenter.Endpoint, storageOptions.Value.UploadCenter.AccessKeyId, storageOptions.Value.UploadCenter.AccessKeySecret);
                var obj = client.GetObject(bucket, key);
                if (obj == null)
                {
                    throw new BusinessException($"未找到文件目录下的[{key}]的文件");
                }
                using (var memoryStream = new MemoryStream())
                {
                    obj.Content.CopyTo(memoryStream);
                    return memoryStream.ToArray();

                }
            }
            catch (OssException ex)
            {
                throw new Exception($"下载文件失败：Failed with error code: {ex.ErrorCode}; Error info: {ex.Message}. \nRequestID:{ex.RequestId}\tHostID:{ex.HostId}");
            }
        }

        public byte[] GetConfigFile(string key)
        {
            try
            {
                var client = new OssClient(storageOptions.Value.ConfigurationCenter.Endpoint, storageOptions.Value.ConfigurationCenter.AccessKeyId, storageOptions.Value.ConfigurationCenter.AccessKeySecret);
                var obj = client.GetObject(storageOptions.Value.ConfigurationCenter.BucketName, key);
                if (obj == null)
                {
                    throw new BusinessException($"未找到文件目录下的[{key}]的文件");
                }
                using (var memoryStream = new MemoryStream())
                {
                    obj.Content.CopyTo(memoryStream);
                    return memoryStream.ToArray();

                }
            }
            catch (OssException ex)
            {
                throw new Exception($"下载文件失败：Failed with error code: {ex.ErrorCode}; Error info: {ex.Message}. \nRequestID:{ex.RequestId}\tHostID:{ex.HostId}");
            }
        }

        public async Task<byte[]> GetAsync(string key)
        {

            throw new NotImplementedException();
        }

        private string GetStorageBaseDirectory()
        {
            throw new NotImplementedException();
        }

        public virtual string GetAbsolutePath(string relativePath)
        {
            throw new NotImplementedException();
        }

        public async Task<string> UploadAsync(byte[] bytes, string key)
        {
            var client = new OssClient(storageOptions.Value.DownloadCenter.Endpoint, storageOptions.Value.DownloadCenter.AccessKeyId, storageOptions.Value.DownloadCenter.AccessKeySecret);
            using var ms = new MemoryStream(bytes);
            var result = client.PutObject(storageOptions.Value.DownloadCenter.BucketName, key, ms);
            if (result.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new BusinessException($"Upload oss error : {storageOptions.Value.DownloadCenter.BucketName}/{key}");
            }

            return string.Empty;
        }

        public string GetDownloadCenterUri(string key)
        {
            var client = new OssClient(storageOptions.Value.DownloadCenter.Endpoint, storageOptions.Value.DownloadCenter.AccessKeyId, storageOptions.Value.DownloadCenter.AccessKeySecret);
            var expiration = DateTime.Now.AddDays(storageOptions.Value.DownloadCenter.ExpirationDay);
            return client.GeneratePresignedUri(storageOptions.Value.DownloadCenter.BucketName, key, expiration, SignHttpMethod.Get).ToString();
        }

        public byte[] GetDownloadCenterFile(string key)
        {
            try
            {
                var client = new OssClient(storageOptions.Value.DownloadCenter.Endpoint, storageOptions.Value.DownloadCenter.AccessKeyId, storageOptions.Value.DownloadCenter.AccessKeySecret);
                var obj = client.GetObject(storageOptions.Value.DownloadCenter.BucketName, key);
                if (obj == null)
                {
                    throw new BusinessException($"未找到文件目录下的[{key}]的文件");
                }
                using (var memoryStream = new MemoryStream())
                {
                    obj.Content.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (OssException ex)
            {
                throw new Exception($"下载文件失败：Failed with error code: {ex.ErrorCode}; Error info: {ex.Message}. \nRequestID:{ex.RequestId}\tHostID:{ex.HostId}");
            }
        }

        public async Task<Result<string>> UploadConfigAsync(Stream stream, string ossFolderName, string fileName)
        {
            //To do 上传的配置准备全部做到门店设置里
            var updateloadRes = new Result<string>();
            string ossPath = string.Empty;


            ossPath = $"{ossFolderName}/{fileName}";

            //考虑到门店文件的时效性 需要没有过期时间或者延长过期时间
            //var expireTime = DateTime.Now.AddDays(storageOptions.Value.ExpirationDay);
            var client = new OssClient(storageOptions.Value.ConfigurationCenter.Endpoint, storageOptions.Value.ConfigurationCenter.AccessKeyId, storageOptions.Value.ConfigurationCenter.AccessKeySecret);
            //var result = client.PutObject(storageOptions.Value.BucketName, ossPath, stream, new ObjectMetadata() { ExpirationTime = expireTime });
            var result = client.PutObject(storageOptions.Value.ConfigurationCenter.BucketName, ossPath, stream);
            stream.Dispose();
            if (result.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Upload oss error : {storageOptions.Value.ConfigurationCenter.BucketName}/{ossPath}");
            }
            else
            {
                updateloadRes.Success = true;
                updateloadRes.Data = ossPath;
            }
            return updateloadRes;
        }


        public string GeneratePresignedConfigUri(string key)
        {
            var client = new OssClient(storageOptions.Value.ConfigurationCenter.Endpoint, storageOptions.Value.ConfigurationCenter.AccessKeyId, storageOptions.Value.ConfigurationCenter.AccessKeySecret);
            var expiration = DateTime.Now.AddDays(storageOptions.Value.ConfigurationCenter.ExpirationDay);
            return client.GeneratePresignedUri(storageOptions.Value.ConfigurationCenter.BucketName, key, expiration, SignHttpMethod.Get).ToString();
        }
    }
}
