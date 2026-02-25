using Microsoft.Extensions.Options;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Exceptions;
using NexusStack.Infrastructure.Models;
using NexusStack.Infrastructure.Options;
using NexusStack.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.FileStroage
{
    /// <summary>
    /// 本地文件存储
    /// </summary>
    public class LocalFileStorage(IOptionsSnapshot<StorageOptions> storageOptions, IOptionsSnapshot<CommonOptions> commonOptions) : IFileStorage, IScopedDependency
    {
        public FileStorageType StorageType => FileStorageType.Local;

        public async Task<string> UploadAsync(byte[] bytes, string key)
        {
            var fileSavePath = Path.Combine(GetStorageBaseDirectory(), key);
            Directory.CreateDirectory(Path.GetDirectoryName(fileSavePath));

            using var file = File.Create(fileSavePath);
            await file.WriteAsync(bytes);

            return GetAbsolutePath(key);
        }

        public string GeneratePresignedUri(string key)
        {
            return GetAbsolutePath(key);
        }

        public byte[] GetFile(string bucket, string key)
        {
            var filePath = Path.Combine(GetStorageBaseDirectory(), key);
            if (!File.Exists(filePath))
            {
                throw new ErrorCodeException(-1, $"文件[{filePath}]不存在");
            }
            return File.ReadAllBytes(filePath);
        }

        public byte[] GetConfigFile(string key)
        {
            throw new NotImplementedException();
        }

        public async Task<byte[]> GetAsync(string key)
        {
            var filePath = Path.Combine(GetStorageBaseDirectory(), key);

            if (!File.Exists(filePath))
            {
                throw new ErrorCodeException(-1, $"文件[{filePath}]不存在");
            }

            return await File.ReadAllBytesAsync(filePath);
        }

        private string GetStorageBaseDirectory()
        {
            var basePath = storageOptions.Value.Path;

            if (basePath.IsNullOrEmpty())
            {
                return Path.Combine(AppContext.BaseDirectory, "uploads");
            }

            if (basePath.StartsWith("/"))
            {
                return basePath;
            }

            return Path.Combine(AppContext.BaseDirectory, basePath);
        }

        public virtual string GetAbsolutePath(string relativePath)
        {
            var host = commonOptions.Value.Host;

            if (host.IsNullOrEmpty())
            {
                return relativePath;
            }

            return $"{host}/static/{relativePath}";
        }

        public Task<Result<string>> UploadAsync(Stream stream, string ossFolderName, string fileName, bool isOverride = false)
        {
            throw new NotImplementedException();
        }

        public bool DoesObjectExist(string fileKey)
        {
            throw new NotImplementedException();
        }

        public void DeleteObject(string fileKey)
        {
            throw new NotImplementedException();
        }

        public Task<Result<string>> UploadConfigAsync(Stream stream, string ossFolderName, string fileName)
        {
            throw new NotImplementedException();
        }

        public string GeneratePresignedConfigUri(string key)
        {
            throw new NotImplementedException();
        }

        Task<string> IFileStorage.UploadAsync(byte[] bytes, string key)
        {
            throw new NotImplementedException();
        }

        public string GetDownloadCenterUri(string key)
        {
            throw new NotImplementedException();
        }

        public byte[] GetDownloadCenterFile(string key)
        {
            throw new NotImplementedException();
        }
    }
}
