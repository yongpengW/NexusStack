using AutoMapper;
using Ardalis.Specification;
using Microsoft.AspNetCore.StaticFiles;
using NexusStack.Core.Dtos.Files;
using NexusStack.Core.Services.Interfaces;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Exceptions;
using NexusStack.Infrastructure.FileStroage;
using NexusStack.Infrastructure.SnowFlake;
using NexusStack.Infrastructure.Utils;
using NexusStack.Infrastructure.Video;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using X.PagedList;
using File = NexusStack.Core.Entities.SystemManagement.File;
using LinqKit;

namespace NexusStack.Core.Services.SystemManagement
{
    /// <summary>
    /// 文件服务
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="mapper"></param>
    /// <param name="storageFactory"></param>
    /// <param name="contentTypeProvider"></param>
    /// <param name="httpClientFactory"></param>
    public class FileService(MainContext dbContext, IMapper mapper, IFileStorageFactory storageFactory, IContentTypeProvider contentTypeProvider, IHttpClientFactory httpClientFactory) : ServiceBase<File>(dbContext, mapper), IFileService, IScopedDependency
    {
        public Task<byte[]> GetContentAsync(string url)
        {
            var idStr = Regex.Match(url, "[0-9]{19}").Value;

            if (long.TryParse(idStr, out var id))
            {
                return GetContentAsync(id);
            }

            throw new ErrorCodeException(-1, "文件未找到");
        }

        public async Task<byte[]> GetContentAsync(File file)
        {
            return await storageFactory.GetStorage().GetAsync(file.Path);
        }

        public async Task<byte[]> GetContentAsync(long id)
        {
            var file = await GetByIdAsync(id);
            return await GetContentAsync(file);
        }

        public Task<File> GetFileByUrlAsync(string path)
        {
            // 使用正则匹配 19 位雪花 Id
            var id = Regex.Match(path, "[0-9]{19}").Value;
            return GetByIdAsync(id);
        }

        public Task<List<File>> GetFilesByIds(string ids)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetTempFilePathAsync(File file)
        {
            // 判断本地是否有该文件，如果有则直接返回文件路径，没有则将文件下载到本地
            var tempPath = App.GetTempPath();
            var tempFilePath = Path.Combine(tempPath, $"{file.Id}{file.Extension}");

            if (System.IO.File.Exists(tempFilePath))
            {
                return tempFilePath;
            }

            // 如果是开发模式，则直接下载该文件到本地环境
            if (Debugger.IsAttached)
            {
                var client = httpClientFactory.CreateClient();
                var downloadBytes = await client.GetByteArrayAsync(file.Url);

                using var tempStream = new FileStream(tempFilePath, FileMode.Create);
                await tempStream.WriteAsync(downloadBytes);

                return tempFilePath;
            }

            var bytes = await GetContentAsync(file);
            using var stream = new FileStream(tempFilePath, FileMode.Create);
            await stream.WriteAsync(bytes);

            return tempFilePath;
        }

        public async Task<string> GetTempFilePathAsync(long id)
        {
            var file = await GetByIdAsync(id);
            return await GetTempFilePathAsync(file);
        }

        public async Task<string> GetTempFilePathAsync(string url)
        {
            var idStr = Regex.Match(url, "[0-9]{19}").Value;

            if (long.TryParse(idStr, out var id))
            {
                return await GetTempFilePathAsync(id);
            }

            throw new ErrorCodeException(-1, "文件未找到");
        }

        public async Task<File> UploadAsync(Stream stream, string originName, long originalFileId = 0, bool transcode = true)
        {
            var bytes = new byte[stream.Length];
            stream.Position = 0;
            await stream.ReadAsync(bytes, 0, bytes.Length);
            return await UploadAsync(bytes, originName, originalFileId, transcode);
        }

        public async Task<File> UploadAsync(byte[] bytes, string originName, long originalFileId = 0, bool transcode = true)
        {
            var storage = storageFactory.GetStorage();
            var file = new File
            {
                Size = bytes.Length,
                StorageType = storage.StorageType,
                Extension = Path.GetExtension(originName),
                Name = originName,
                OriginalId = originalFileId,
            };

            // 是否需要计算文件 MD5
            using var hasher = HashAlgorithm.Create("MD5");
            var hash = BitConverter.ToString(hasher.ComputeHash(bytes)).Replace("-", "").ToLower();

            // 根据 Hash 值判断文件是否已存在，如果该文件已存在，直接返回该文件信息
            var exists = await GetAsync(a => a.Hash == hash);
            if (exists is not null)
            {
                return exists;
            }

            var relativePath = $"{DateTime.Now.ToString("yyyy/MM/dd", DateTimeFormatInfo.InvariantInfo)}/{file.Id}{file.Extension}";
            file.Path = relativePath;

            if (!contentTypeProvider.TryGetContentType(originName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var fileType = contentType.Split("/")[0] switch
            {
                "text" => FileType.Document,
                "image" => FileType.Picture,
                "audio" => FileType.Audio,
                "video" => FileType.Video,
                _ => FileType.Other
            };

            file.Hash = hash;
            file.Type = fileType;

            if (fileType == FileType.Video && transcode)
            {
                var tempFilePath = Path.Combine(App.GetTempPath(), $"{SnowFlake.Instance.NextId()}{Path.GetExtension(originName)}");

                using var writer = new FileStream(tempFilePath, FileMode.Create);
                await writer.WriteAsync(bytes, 0, bytes.Length);
                writer.Close();

                var isH264Codec = await VideoHelper.IsH264Codec(tempFilePath);

                // 如果视频不是 h264 编码，需要进行转码
                if (!isH264Codec)
                {
                    // 新建源文件对象，并上传源文件
                    var originalFile = await UploadAsync(bytes, originName, transcode: false);
                    originalFile.State = FileState.Disabled;

                    // 更新文件状态为禁用
                    await UpdateAsync(originalFile);

                    file.OriginalId = originalFile.Id;

                    // 发布转码事件
                    //this.eventPublisher.Publish(new VideoTranscodeEvent
                    //{
                    //    OriginalId = originalFile.Id,
                    //    TargetId = file.Id,
                    //});

                    //// 将视频的路径指向转码中视频的路径
                    //file.Path = App.Options<VideoTranscodeOptions>().DefaultVideoPath ?? "";
                    file.Url = $"{storage.GetAbsolutePath(relativePath)}?type=video";
                    file.State = FileState.Transcoding;
                }

                // 删除临时文件
                System.IO.File.Delete(tempFilePath);
            }

            if (file.Url.IsNullOrEmpty())
            {
                var url = await storage.UploadAsync(bytes, relativePath);
                if (fileType == FileType.Video)
                {
                    var fileTypeString = GetFileTypeString(fileType);
                    file.Url = $"{url}?type={fileTypeString}";
                }
                else
                {
                    file.Url = url;
                }

                file.State = FileState.Normal;
            }

            file.ContentType = contentType;

            return await InsertAsync(file);
        }

        public Task<File> UploadAsync(string localPath, string originName = "", long originalFileId = 0, bool transcode = true)
        {
            throw new NotImplementedException();
        }

        public Task<File> UploadFromUrlAsync(string url, string originName = "")
        {
            throw new NotImplementedException();
        }

        private static string GetFileTypeString(FileType type)
        {
            return type switch
            {
                FileType.Picture => "image",
                FileType.Video => "video",
                FileType.Audio => "audio",
                FileType.Document => "text",
                _ => ""
            };
        }

        public async Task<IPagedList<ExportFileDto>> GetExportFiles(ExportFileQueryDto model)
        {
            var spec = Specifications<File>.Create();
            spec.Query.Where(item => !item.IsDeleted);
            if (!string.IsNullOrWhiteSpace(model.FileName))
            {
                model.FileName = model.FileName.Trim();
                spec.Query.Where(e => e.Name.Contains(model.FileName));
            }
            if (model.State.HasValue)
            {
                if (model.State == (int)ExportState.Success)
                    spec.Query.Where(e => e.State == FileState.Success);
                else if (model.State == (int)ExportState.Failure)
                    spec.Query.Where(e => e.State == FileState.Fail);
                else
                {
                    DateTime now = DateTime.Now;
                    spec.Query.Where(e => now > e.UpdatedAt.AddDays(30));
                }
            }
            if (model.DateRanges != null && model.DateRanges.Count == 1)
            {
                var dates = model.DateRanges[0].Split(',').Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
                if (dates.Count == 2)
                {
                    DateTime startTime = DateTime.Parse(dates[0]);
                    DateTime endTime = DateTime.Parse(dates[1]);
                    spec.Query.Where(item => item.CreatedAt.Date >= startTime && item.CreatedAt.Date <= endTime);
                }

            }
            spec.Query.OrderByDescending(item => item.CreatedAt);
            var data = await GetPagedListAsync<ExportFileDto>(spec, model.Page, model.Limit);
            data.ForEach(e => {
                e.ExpireDate = e.UpdatedAt.AddDays(30);
                if (DateTime.Now > e.ExpireDate)
                    e.StateName = "已过期";
                else
                    e.StateName = e.State == (int)FileState.Success ? "导出成功" : "导出失败";
                e.Percent = e.State == (int)FileState.Success ? 100 : 0;
            });

            return data;
        }
    }
}
