using System.IO;
using System.Windows.Threading;
using Joufflu.Feedback;
using Joufflu.FileExplorer.Data;

namespace Joufflu.FileExplorer.Sources
{
    /// <summary>
    /// A <see cref="FileSystemSource"/> that keeps its nodes in step with the disk : a file or a directory created,
    /// deleted, renamed or moved outside of the explorer shows up in it without a manual refresh.
    /// </summary>
    /// <remarks>
    /// Only the loaded nodes are maintained, an event about a directory that has never been opened being dropped : it
    /// will be read from the disk when it is opened. The content of a file isn't watched, a node only carrying the
    /// size and the date it has been read with.
    /// </remarks>
    public class FileSystemWatcherSource : FileSystemSource, IDisposable
    {
        /// <summary>
        /// Dispatcher of the thread the source has been built on : the watcher raises its events on a thread of its
        /// own, while the nodes are bound to the UI.
        /// </summary>
        private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

        private FileSystemWatcher? watcher;
        private bool isDisposed;

        public FileSystemWatcherSource(string rootDirectoryPath, IToastService? toasts)
            : base(rootDirectoryPath, toasts)
        { }

        public override async Task Open()
        {
            await base.Open();
            StartWatching();
        }

        public void Dispose()
        {
            isDisposed = true;
            StopWatching();
            GC.SuppressFinalize(this);
        }

        #region Watcher

        private void StartWatching()
        {
            StopWatching();

            if (Root == null || !Directory.Exists(Root.Path))
                return;

            watcher = new FileSystemWatcher(Root.Path)
            {
                IncludeSubdirectories = true,
                // Only the existence of the nodes is watched : a node carries no live content, so the writes into a
                // file are of no use here and would only raise events by the hundred while one is being copied.
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
            };

            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            watcher.Error += OnError;
            watcher.EnableRaisingEvents = true;
        }

        private void StopWatching()
        {
            if (watcher == null)
                return;

            watcher.EnableRaisingEvents = false;
            watcher.Created -= OnCreated;
            watcher.Deleted -= OnDeleted;
            watcher.Renamed -= OnRenamed;
            watcher.Error -= OnError;
            watcher.Dispose();
            watcher = null;
        }

        /// <summary>
        /// Runs an update of the nodes on the thread the source belongs to, the watcher raising its events on one of
        /// its own.
        /// </summary>
        private void Dispatch(Action update)
        {
            if (isDisposed)
                return;

            dispatcher.InvokeAsync(() =>
            {
                // Disposed while the update was waiting for the dispatcher.
                if (!isDisposed)
                    update();
            });
        }

        private void OnCreated(object sender, FileSystemEventArgs e) => Dispatch(() => Add(e.FullPath));

        private void OnDeleted(object sender, FileSystemEventArgs e) => Dispatch(() => Remove(e.FullPath));

        private void OnRenamed(object sender, RenamedEventArgs e) => Dispatch(() =>
        {
            // A rename and a move are the same event : the node is dropped where it was and read again where it is
            // now, rather than having its path patched, a renamed directory holding children whose paths are stale.
            Remove(e.OldFullPath, keepCurrent: true);
            Add(e.FullPath);

            // The opened directory was the renamed one or one of its children : it is opened again under its new path.
            if (Current != null && IsSameOrAncestor(e.OldFullPath, Current.Path))
                OpenPath(e.FullPath + Current.Path[e.OldFullPath.Length..]);
        });

        /// <summary>
        /// The watcher lost events, its buffer having overflowed : the whole loaded tree is read again, no single
        /// change being known anymore.
        /// </summary>
        private void OnError(object sender, ErrorEventArgs e) => Dispatch(() =>
        {
            if (Root == null)
                return;

            string currentPath = Current?.Path ?? Root.Path;
            Refresh([Root]);
            OpenPath(currentPath);

            // A watcher that reported an error has stopped watching, a new one takes over.
            StartWatching();
        });

        #endregion

        #region Nodes

        /// <summary>
        /// Adds the node of a path to the directory holding it, when that directory is loaded and doesn't display it
        /// already : an operation made by the source itself has added its own nodes before the event arrives.
        /// </summary>
        private void Add(string path)
        {
            IExplorerDirectory? parent = FindDirectory(Path.GetDirectoryName(path));
            if (parent == null || parent.Children.Any(child => PathsEqual(child.Path, path)))
                return;

            // Created then deleted while the event was on its way.
            FileSystemInfo? entry = GetEntry(path);
            if (entry == null)
                return;

            IExplorerNode node = CreateNode(entry, parent);
            parent.Children.Add(node);

            if (node is IExplorerDirectory directory)
                LoadDirectory(directory, LoadDepth);
        }

        /// <summary>
        /// Drops the node of a path from the directory holding it.
        /// </summary>
        /// <param name="path">Path of the node to drop.</param>
        /// <param name="keepCurrent">
        /// True to leave the opened directory alone, a rename opening it again under its new path itself.
        /// </param>
        private void Remove(string path, bool keepCurrent = false)
        {
            IExplorerDirectory? parent = FindDirectory(Path.GetDirectoryName(path));
            if (parent == null)
                return;

            foreach (IExplorerNode node in parent.Children.Where(child => PathsEqual(child.Path, path)).ToList())
                parent.Children.Remove(node);

            // The opened directory has been deleted, along with the nodes it was displaying : the directory that held
            // it is opened instead, the way the Windows explorer does.
            if (!keepCurrent && Current != null && IsSameOrAncestor(path, Current.Path))
                OpenPath(parent.Path);
        }

        /// <summary>
        /// Opens the directory of a path, reading the disk down to it : a directory that has just appeared isn't
        /// loaded yet. Nothing is opened for a path outside of the root or that doesn't exist anymore.
        /// </summary>
        private void OpenPath(string path)
        {
            if (Root == null || !IsSameOrAncestor(Root.Path, path) || !Directory.Exists(path))
                return;

            IExplorerDirectory directory = Root;
            while (!PathsEqual(directory.Path, path))
            {
                IExplorerDirectory? child = FindChild(directory);
                if (child == null)
                {
                    // Not loaded yet : the directory is read again, and the walk down goes on through what it holds.
                    LoadDirectory(directory, 0);
                    child = FindChild(directory);

                    if (child == null)
                        break;
                }

                directory = child;
            }

            _ = Open(directory);

            IExplorerDirectory? FindChild(IExplorerDirectory parent)
                => parent.Children.OfType<IExplorerDirectory>().FirstOrDefault(node => IsSameOrAncestor(node.Path, path));
        }

        private static FileSystemInfo? GetEntry(string path)
        {
            if (Directory.Exists(path))
                return new DirectoryInfo(path);

            return File.Exists(path) ? new FileInfo(path) : null;
        }

        #endregion
    }
}
