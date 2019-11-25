using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace BibliTech.VersionedFileProvider
{

    public class VersionedFileProvider : IFileProvider, IDisposable
    {

        public string RootFolder { get; private set; }
        public Regex VersionRegex { get; set; } = new Regex("^v[0-9]+");

        public VersionedFileProvider(string rootFolder) : this(rootFolder, ExclusionFilters.Sensitive) { }

        public VersionedFileProvider(string rootFolder, ExclusionFilters filters)
        {
            this.RootFolder = rootFolder;

            _filters = filters;
            _fileWatcherFactory = () => CreateFileWatcher();
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var finalPath = this.RemoveVersionedSubpaths(subpath);

            var folder = Path.Combine(this.RootFolder, finalPath);
            return new VersionedDirectoryContent(folder);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var finalPath = this.RemoveVersionedSubpaths(subpath);

            var file = Path.Combine(this.RootFolder, finalPath);
            var result = new VersionedFileInfo(file);

            return result;
        }

        string RemoveVersionedSubpaths(string subpath)
        {
            var splits = subpath.Split('/');
            var removed = splits.Where(q => !string.IsNullOrEmpty(q) && !this.VersionRegex.IsMatch(q));

            return string.Join('/', removed);
        }

        #region Watcher code

        // Copy from source code of PhysicalFileProvider
        // https://github.com/aspnet/Extensions/blob/master/src/FileProviders/Physical/src/PhysicalFileProvider.cs

        private static readonly char[] _pathSeparators = new[]
        {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};

        private readonly Func<PhysicalFilesWatcher> _fileWatcherFactory;
        private PhysicalFilesWatcher _fileWatcher;
        private bool _fileWatcherInitialized;
        private object _fileWatcherLock = new object();

        private bool? _usePollingFileWatcher;
        private bool? _useActivePolling;
        private readonly ExclusionFilters _filters;

        private const string PollingEnvironmentKey = "DOTNET_USE_POLLING_FILE_WATCHER";

        internal PhysicalFilesWatcher CreateFileWatcher()
        {
            var root = EnsureTrailingSlash(Path.GetFullPath(this.RootFolder));
            return new PhysicalFilesWatcher(root, new FileSystemWatcher(root), UsePollingFileWatcher, _filters)
            {
                //UseActivePolling = UseActivePolling,
            };
        }

        private void ReadPollingEnvironmentVariables()
        {
            var environmentValue = Environment.GetEnvironmentVariable(PollingEnvironmentKey);
            var pollForChanges = string.Equals(environmentValue, "1", StringComparison.Ordinal) ||
                string.Equals(environmentValue, "true", StringComparison.OrdinalIgnoreCase);

            _usePollingFileWatcher = pollForChanges;
            _useActivePolling = pollForChanges;
        }

        public bool UsePollingFileWatcher
        {
            get
            {
                if (_fileWatcher != null)
                {
                    return false;
                }
                if (_usePollingFileWatcher == null)
                {
                    ReadPollingEnvironmentVariables();
                }
                return _usePollingFileWatcher ?? false;
            }
            set
            {
                if (_fileWatcher != null)
                {
                    throw new InvalidOperationException($"Cannot modify {nameof(UsePollingFileWatcher)} once file watcher has been initialized.");
                }
                _usePollingFileWatcher = value;
            }
        }

        internal PhysicalFilesWatcher FileWatcher
        {
            get
            {
                return LazyInitializer.EnsureInitialized(
                    ref _fileWatcher,
                    ref _fileWatcherInitialized,
                    ref _fileWatcherLock,
                    _fileWatcherFactory);
            }
            set
            {
                Debug.Assert(!_fileWatcherInitialized);

                _fileWatcherInitialized = true;
                _fileWatcher = value;
            }
        }

        public IChangeToken Watch(string filter)
        {
            if (filter == null || HasInvalidFilterChars(filter))
            {
                return NullChangeToken.Singleton;
            }

            // Relative paths starting with leading slashes are okay
            filter = filter.TrimStart(_pathSeparators);

            return FileWatcher.CreateFileChangeToken(filter);
        }

        private static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars()
            .Where(c => c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar).ToArray();
        private static readonly char[] _invalidFilterChars = _invalidFileNameChars
            .Where(c => c != '*' && c != '|' && c != '?').ToArray();
        static bool HasInvalidFilterChars(string path)
        {
            return path.IndexOfAny(_invalidFilterChars) != -1;
        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            _fileWatcher?.Dispose();
        }

        ~VersionedFileProvider() => Dispose(false);

        internal static string EnsureTrailingSlash(string path)
        {
            if (!string.IsNullOrEmpty(path) &&
                path[path.Length - 1] != Path.DirectorySeparatorChar)
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }

        #endregion

    }

}
