using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.FileStroage
{
    public interface IFileStorageFactory
    {
        IFileStorage GetStorage(FileStorageType storageType);

        IFileStorage GetStorage();
    }
}
