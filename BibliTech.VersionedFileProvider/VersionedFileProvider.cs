using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BibliTech.VersionedFileProvider
{
    public class VersionedFileProvider : BasePathTransformFileProvider
    {
        public VersionedFileProvider(IFileProvider underlyingFileProvider) : base(underlyingFileProvider)
        {
        }

        public VersionedFileProvider(string root) : base(root)
        {
        }

        public Regex VersionRegex { get; set; } = new Regex(@"^v[0-9\.]+");

        public override string TransformSubpath(string path)
        {
            var pathSegments = path.Split('/');

            return string.Join(
                '/',
                pathSegments
                    .Where(q => !this.VersionRegex.IsMatch(q)));
        }

    }
}
