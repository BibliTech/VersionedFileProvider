using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BibliTech.VersionedFileProvider
{
    public class VersionedFileProvider : IFileProvider
    {

        public Regex VersionRegex { get; set; } = new Regex(@"^v[0-9\.]+");

        IFileProvider underlyingFileProvider;

        public VersionedFileProvider(IFileProvider underlyingFileProvider)
        {
            this.underlyingFileProvider = underlyingFileProvider;
        }

        public VersionedFileProvider(string root) 
            : this(new PhysicalFileProvider(root))
        { }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return this.underlyingFileProvider.GetDirectoryContents(
                this.TransformPath(subpath));
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return this.underlyingFileProvider.GetFileInfo(
                this.TransformPath(subpath));
        }

        public IChangeToken Watch(string filter)
        {
            return this.underlyingFileProvider.Watch(
                filter);
        }

        string TransformPath(string path)
        {
            var pathSegments = path.Split('/');

            return string.Join(
                '/',
                pathSegments
                    .Where(q => !this.VersionRegex.IsMatch(q)));
        }

    }
}
