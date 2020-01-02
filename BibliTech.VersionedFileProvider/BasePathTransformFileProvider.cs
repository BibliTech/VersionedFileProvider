using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace BibliTech.VersionedFileProvider
{
    public abstract class BasePathTransformFileProvider : IPathTransformFileProvider
    {


        IFileProvider underlyingFileProvider;

        public BasePathTransformFileProvider(IFileProvider underlyingFileProvider)
        {
            this.underlyingFileProvider = underlyingFileProvider;
        }

        public BasePathTransformFileProvider(string root)
            : this(new PhysicalFileProvider(root))
        { }

        public abstract string TransformSubpath(string subpath);

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return this.underlyingFileProvider.GetDirectoryContents(
                this.TransformSubpath(subpath));
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return this.underlyingFileProvider.GetFileInfo(
                this.TransformSubpath(subpath));
        }

        public IChangeToken Watch(string filter)
        {
            return this.underlyingFileProvider.Watch(filter);
        }
    }
}
