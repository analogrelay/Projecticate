using System;
using System.IO;

namespace Projecticate
{
    public class FileBasicInfo
    {
        public bool IsDirectory { get; }
        public long FileSize { get; }
        public DateTime CreationTime { get; }
        public DateTime LastAccessTime { get; }
        public DateTime LastWriteTime { get; }
        public DateTime ChangeTime { get; }
        public FileAttributes FileAttributes { get; }

        public FileBasicInfo(bool isDirectory, long fileSize, DateTime creationTime, DateTime lastAccessTime, DateTime lastWriteTime, DateTime changeTime, FileAttributes fileAttributes)
        {
            IsDirectory = isDirectory;
            FileSize = fileSize;
            CreationTime = creationTime;
            LastAccessTime = lastAccessTime;
            LastWriteTime = lastWriteTime;
            ChangeTime = changeTime;
            FileAttributes = fileAttributes;
        }

        public static FileBasicInfo Directory(DateTime creationTime, DateTime lastAccessTime, DateTime lastWriteTime, DateTime changeTime) =>
            new FileBasicInfo(isDirectory: true, fileSize: 0, creationTime, lastAccessTime, lastWriteTime, creationTime, FileAttributes.Normal);

        public static FileBasicInfo File(long size, DateTime creationTime, DateTime lastAccessTime, DateTime lastWriteTime, DateTime changeTime) =>
            new FileBasicInfo(isDirectory: false, size, creationTime, lastAccessTime, lastWriteTime, creationTime, FileAttributes.Normal);

        public static FileBasicInfo File(long size, DateTime creationTime, DateTime lastAccessTime, DateTime lastWriteTime, DateTime changeTime, FileAttributes attributes) =>
            new FileBasicInfo(isDirectory: false, size, creationTime, lastAccessTime, lastWriteTime, creationTime, attributes);
    }
}