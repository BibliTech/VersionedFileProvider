using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BibliTech.VersionedFileProvider
{
    internal class VersionedFileInfo : IFileInfo
    {

        public string FilePath { get; private set; }
        public FileInfo FileInfo { get; private set; }
        public DirectoryInfo DirectoryInfo { get; private set; }

        public VersionedFileInfo(string filePath)
        {
            this.FilePath = filePath;
            this.FileInfo = new FileInfo(filePath);
            this.DirectoryInfo = new DirectoryInfo(filePath);
        }

        public bool Exists => File.Exists(this.FilePath) || Directory.Exists(this.FilePath);

        public bool IsDirectory => Directory.Exists(this.FilePath);

        public DateTimeOffset LastModified => new DateTimeOffset(this.FileInfo.LastWriteTimeUtc, TimeSpan.Zero);

        public long Length => this.FileInfo.Length;

        public string Name => this.FileInfo.Name;

        public string PhysicalPath => this.FilePath;

        public Stream CreateReadStream()
        {
            return File.OpenRead(this.FilePath);
        }
    }
}
