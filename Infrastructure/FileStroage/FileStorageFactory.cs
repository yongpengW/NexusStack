using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.FileStroage
{
    public class FileStorageFactory(IServiceScopeFactory scopeFactory, IOptionsSnapshot<StorageOptions> storageOptions) : IFileStorageFactory, IScopedDependency
    {
        public virtual IFileStorage GetStorage()
        {
            var storageType = storageOptions.Value.Type;
            return GetStorage(storageType);
        }

        public virtual IFileStorage GetStorage(FileStorageType storageType)
        {
            using var scope = scopeFactory.CreateScope();

            var storage = scope.ServiceProvider.GetServices<IFileStorage>().FirstOrDefault(a => a.StorageType == storageType);

            if (storage == null)
            {
                throw new Exception($"暂不支持 {Enum.GetName<FileStorageType>(storageType)}");
            }

            return storage;
        }
    }
}
