using Microsoft.Extensions.FileProviders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BibliTech.VersionedFileProvider
{
    internal class VersionedDirectoryContent : IDirectoryContents
    {

        public string FolderPath { get; private set; }

        public VersionedDirectoryContent(string folderPath)
        {
            this.FolderPath = folderPath;
        }

        public bool Exists => Directory.Exists(this.FolderPath);

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            var files = Directory.GetFiles(this.FolderPath);
            var folders = Directory.GetDirectories(this.FolderPath);

            return folders.Concat(files)
                .Select(q => new VersionedFileInfo(q) as IFileInfo)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
