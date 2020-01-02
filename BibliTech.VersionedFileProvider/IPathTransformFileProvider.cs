using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BibliTech.VersionedFileProvider
{
    public interface IPathTransformFileProvider : IFileProvider
    {

        string TransformSubpath(string subpath);

    }
}
