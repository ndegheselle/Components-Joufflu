using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Feedback;
using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.Helpers;

namespace Joufflu.FileExplorer.Sources
{
    /// <summary>
    /// The files and the directories under a directory of this machine.
    /// </summary>
    /// <remarks>
    /// Copying, moving and deleting are handed over to the shell (through <see cref="ShellFileOperation"/>), so they
    /// come with the progress window, the "replace or skip" prompt and the recycle bin of the Windows explorer rather
    /// than with an implementation of our own. A whole selection is handed over in one call, which is what keeps the
    /// windows of the shell to one for the batch.
    /// </remarks>
    public partial class FileSystemSource : ObservableObject, IExplorerSource
    {
        /// <summary>
        /// Levels of sub directories loaded along with a directory, so that the tree shows an expander without
        /// reading the disk again.
        /// </summary>
        protected const int LoadDepth = 2;

        /// <summary>Characters a name is refused for, as the Windows explorer lists them.</summary>
        private const string InvalidNameCharacters = @"\ / : * ? "" < > |";

        private readonly string rootDirectoryPath;
        private readonly IToastService? toasts;

        [ObservableProperty]
        private IExplorerDirectory? root;
        [ObservableProperty]
        private IExplorerDirectory? current;

        ICommand IExplorerSource.RenameCommand => RenameCommand;
        ICommand IExplorerSource.RemoveCommand => RemoveCommand;
        ICommand IExplorerSource.CreateDirectoryCommand => CreateDirectoryCommand;

        ICommand IExplorerSource.OpenCommand => OpenCommand;
        ICommand IExplorerSource.OpenInExplorerCommand => OpenInExplorerCommand;
        ICommand IExplorerSource.CopyPathCommand => CopyPathCommand;
        ICommand IExplorerSource.OpenWithDefaultCommand => OpenWithDefaultCommand;

        ICommand IExplorerSource.CopyCommand => CopyCommand;
        ICommand IExplorerSource.CutCommand => CutCommand;
        ICommand IExplorerSource.PasteCommand => PasteCommand;

        public FileSystemSource(string rootDirectoryPath, IToastService? toasts)
        {
            this.rootDirectoryPath = rootDirectoryPath;
            this.toasts = toasts;
        }

        #region Open

        public virtual Task Open()
        {
            Root = new FileSystemDirectory(new DirectoryInfo(rootDirectoryPath), null);
            return Open(Root);
        }

        /// <summary>
        /// Open a node : a directory is loaded and made the <see cref="Current"/> one, a file is opened by the
        /// system. A derived source overrides it for the node types of its own, the other ones being left to it.
        /// </summary>
        [RelayCommand]
        public virtual async Task Open(IExplorerNode node)
        {
            if (node is IExplorerDirectory directory)
            {
                await OpenDirectory(directory, LoadDepth);
            }
            else if (node is IExplorerFile file)
            {
                await OpenFile(file);
            }
        }

        [RelayCommand]
        public void OpenInExplorer(IExplorerNode node)
        {
            // "/select," so that a directory is shown inside its parent instead of being opened.
            StartProcess(new ProcessStartInfo("explorer.exe", $"/select,\"{node.Path}\"") { UseShellExecute = true });
        }

        [RelayCommand]
        public void OpenWithDefault(IExplorerFile file)
        {
            // UseShellExecute resolves the application associated with the file
            StartProcess(new ProcessStartInfo(file.Path) { UseShellExecute = true });
        }

        /// <summary>
        /// Open a directory by loading all it's childrens and setting it as <see cref="Current"/>.
        /// Recursif on <paramref name="depth"/> sub directories.
        /// </summary>
        protected virtual Task OpenDirectory(IExplorerDirectory directory, int depth)
        {
            LoadDirectory(directory, depth);
            Current = directory;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Read a directory into its children, recursif on <paramref name="depth"/> sub directories, without making
        /// it the <see cref="Current"/> one. Also used to reload a directory the shell just modified.
        /// </summary>
        protected virtual void LoadDirectory(IExplorerDirectory directory, int depth)
        {
            directory.Children.Clear();
            var dirInfo = new DirectoryInfo(directory.Path);
            foreach (var entry in dirInfo.EnumerateFileSystemInfos())
            {
                IExplorerNode node = CreateNode(entry, directory);
                directory.Children.Add(node);

                if (depth > 0 && node is IExplorerDirectory subDirectory)
                    LoadDirectory(subDirectory, depth - 1);
            }
        }

        /// <summary>
        /// Node displaying a file system entry. Every node of the source goes through it, so a derived source
        /// overrides it to display nodes of its own.
        /// </summary>
        protected virtual IExplorerNode CreateNode(FileSystemInfo entry, IExplorerDirectory? parent)
            => entry is DirectoryInfo directory
                ? new FileSystemDirectory(directory, parent)
                : new FileSystemFile((FileInfo)entry, parent);

        /// <summary>
        /// Open the file with the default programm.
        /// </summary>
        protected virtual Task OpenFile(IExplorerFile file)
        {
            OpenWithDefault(file);
            return Task.CompletedTask;
        }

        #endregion

        #region Copy / Paste

        [RelayCommand]
        public void Copy(IEnumerable<IExplorerNode> nodes) => SetClipboard(nodes, isMove: false);

        [RelayCommand]
        public void Cut(IEnumerable<IExplorerNode> nodes) => SetClipboard(nodes, isMove: true);

        /// <summary>
        /// Copy or move the nodes held by the clipboard into a directory, <see cref="Current"/> when none is given.
        /// </summary>
        /// <remarks>
        /// Whether the nodes have been copied or cut is read from the clipboard itself, see
        /// <see cref="ExplorerClipboard"/> : the file explorer of Windows writes it there as well, so a cut made in
        /// one is pasted as a move in the other.
        /// </remarks>
        [RelayCommand(CanExecute = nameof(CanPaste))]
        public async Task Paste(IExplorerDirectory target)
        {
            IReadOnlyList<string> paths = ExplorerClipboard.GetPaths(out bool isMove);
            if (paths.Count == 0)
                return;

            await Transfer(paths, target, isMove);
        }

        public bool CanPaste(IExplorerDirectory target)
        {
            IReadOnlyList<string> paths = ExplorerClipboard.GetPaths(out bool isMove);
            return paths.Count > 0;
        }

        /// <summary>
        /// Copy or move paths into a directory, through the shell : the standard progress window of Windows is
        /// displayed, along with the "replace or skip" prompt when a name is already taken, and cancelling it leaves
        /// everything in place.
        /// </summary>
        /// <param name="paths">Files and directories to transfer, the ones that don't exist anymore are ignored.</param>
        /// <param name="target">Directory the paths are transferred into.</param>
        /// <param name="isMove">True to move the paths, false to copy them.</param>
        public async Task Transfer(IReadOnlyList<string> paths, IExplorerDirectory target, bool isMove)
        {
            string targetPath = target.Path;
            List<string> sources = [];
            List<string> destinations = [];

            foreach (string source in paths.Where(path => CanTransfer(path, targetPath)))
            {
                string name = Path.GetFileName(Path.TrimEndingDirectorySeparator(source));
                string destination = Path.Combine(targetPath, name);

                if (PathsEqual(source, destination))
                {
                    // Cut and pasted where it already is : nothing to do, where the shell would report that the
                    // source and the destination are the same.
                    if (isMove)
                        continue;

                    // Pasted next to itself : the shell refuses to copy a node onto itself, so the copy is named
                    // "File - Copy.ext" the way Windows does.
                    destination = Path.Combine(targetPath, GetCopyName(targetPath, name, destinations));
                }

                sources.Add(source);
                destinations.Add(destination);
            }

            if (sources.Count == 0)
                return;

            try
            {
                ShellFileOperation.Transfer(sources, destinations, isMove);
            }
            catch (Exception exception)
            {
                toasts?.Error(exception.Message);
            }

            // A move also empties the directories the paths are coming from, when they are loaded.
            List<IExplorerDirectory?> changed = [target];
            if (isMove)
                changed.AddRange(sources.Select(source => FindDirectory(Path.GetDirectoryName(source))));

            Refresh(changed);
        }

        /// <summary>
        /// A path that doesn't exist anymore can't be transferred, and a directory can't be transferred into itself
        /// or into one of its own sub directories.
        /// </summary>
        private static bool CanTransfer(string path, string targetPath)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
                return false;

            return !IsSameOrAncestor(path, targetPath);
        }

        private void SetClipboard(IEnumerable<IExplorerNode> nodes, bool isMove)
        {
            string[] paths = [.. nodes.Select(node => node.Path)];
            if (paths.Length == 0)
                return;

            if (!ExplorerClipboard.TrySetPaths(paths, isMove))
            {
                toasts?.Error("The clipboard is not available.");
                return;
            }

            toasts?.Info(isMove ? $"{paths.Length} element(s) cut." : $"{paths.Length} element(s) copied.");
        }

        #endregion

        #region Misc

        [RelayCommand]
        public void CopyPath(IExplorerNode node)
        {
            Clipboard.SetText(node.Path);
            toasts?.Info("Path copied to clipboard.");
        }

        /// <summary>
        /// Give a node the name it has been renamed to. An empty or an unchanged name does nothing, the edition having
        /// been given up.
        /// </summary>
        [RelayCommand]
        public void Rename(ExplorerNodeRename rename)
        {
            string name = rename.Name.Trim();
            if (string.IsNullOrEmpty(name) || name == rename.Node.Name)
                return;

            // Checked here, where the shell only reports a refusal as a code of its own.
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                toasts?.Error($"A name cannot contain any of the following characters : {InvalidNameCharacters}");
                return;
            }

            try
            {
                // Moved onto its new path, the rename going through the shell as the other operations do : a name
                // already taken displays the same prompt as in the Windows explorer.
                ShellFileOperation.Transfer(
                    [rename.Node.Path],
                    [Path.Combine(Path.GetDirectoryName(rename.Node.Path) ?? "", name)],
                    isMove: true);
            }
            catch (Exception exception)
            {
                toasts?.Error(exception.Message);
            }

            Refresh([rename.Node.Parent]);
        }

        /// <summary>
        /// Send nodes to the recycle bin, through the shell : the standard progress window and confirmation of
        /// Windows are displayed, and cancelling them leaves the nodes in place.
        /// </summary>
        [RelayCommand]
        public async Task Remove(IEnumerable<IExplorerNode> nodes)
        {
            List<IExplorerNode> removed = [.. nodes];
            if (removed.Count == 0)
                return;

            try
            {
                ShellFileOperation.Delete([.. removed.Select(node => node.Path)]);
            }
            catch (Exception exception)
            {
                toasts?.Error(exception.Message);
            }

            Refresh(removed.Select(node => node.Parent));
        }

        /// <summary>
        /// Create a "New folder" in a directory, <see cref="Current"/> when none is given.
        /// </summary>
        [RelayCommand]
        public void CreateDirectory(IExplorerDirectory? parent)
        {
            parent ??= Current;
            if (parent == null)
                return;

            try
            {
                Directory.CreateDirectory(Path.Combine(parent.Path, GetNewDirectoryName(parent.Path)));
            }
            catch (Exception exception)
            {
                toasts?.Error(exception.Message);
                return;
            }

            Refresh([parent]);
        }
        #endregion

        #region Helpers

        /// <summary>
        /// Reload the directories whose content changed, ignoring the ones that aren't part of the loaded tree.
        /// </summary>
        protected void Refresh(IEnumerable<IExplorerDirectory?> directories)
        {
            foreach (IExplorerDirectory directory in directories.OfType<IExplorerDirectory>().Distinct())
            {
                if (Directory.Exists(directory.Path))
                    LoadDirectory(directory, LoadDepth);
            }
        }

        /// <summary>
        /// Loaded directory displaying <paramref name="path"/>, null when it isn't part of the loaded tree.
        /// </summary>
        protected IExplorerDirectory? FindDirectory(string? path)
        {
            if (Root == null || string.IsNullOrEmpty(path))
                return null;

            string searched = path;
            return Find(Root);

            IExplorerDirectory? Find(IExplorerDirectory directory)
            {
                if (PathsEqual(directory.Path, searched))
                    return directory;

                foreach (IExplorerDirectory child in directory.Children.OfType<IExplorerDirectory>())
                {
                    IExplorerDirectory? found = Find(child);
                    if (found != null)
                        return found;
                }

                return null;
            }
        }

        private void StartProcess(ProcessStartInfo startInfo)
        {
            try
            {
                Process.Start(startInfo)?.Dispose();
            }
            catch (Exception exception)
            {
                toasts?.Error(exception.Message);
            }
        }

        /// <summary>
        /// "File - Copy.ext", then "File - Copy (2).ext" while the name is taken, the way Windows names a copy made
        /// next to its source.
        /// </summary>
        /// <param name="directoryPath">Directory the copy is made in.</param>
        /// <param name="name">Name of the node being copied.</param>
        /// <param name="taken">
        /// Destinations of the same batch, none of which exists yet : the whole batch is named before the shell
        /// creates any of it, so a name already given to another copy has to be skipped as well.
        /// </param>
        private static string GetCopyName(string directoryPath, string name, IReadOnlyList<string> taken)
        {
            string bareName = Path.GetFileNameWithoutExtension(name);
            string extension = Path.GetExtension(name);

            for (int index = 1; index < int.MaxValue; index++)
            {
                string number = index > 1 ? $" ({index})" : "";
                string candidate = $"{bareName} - Copy{number}{extension}";
                string candidatePath = Path.Combine(directoryPath, candidate);
                if (!Exists(candidatePath) && !taken.Any(other => PathsEqual(other, candidatePath)))
                    return candidate;
            }

            return name;
        }

        /// <summary>
        /// "New folder", then "New folder (2)" while the name is taken, the way Windows does.
        /// </summary>
        private static string GetNewDirectoryName(string parentPath)
        {
            const string baseName = "New folder";

            for (int index = 1; index < int.MaxValue; index++)
            {
                string candidate = index > 1 ? $"{baseName} ({index})" : baseName;
                if (!Exists(Path.Combine(parentPath, candidate)))
                    return candidate;
            }

            return baseName;
        }

        private static bool Exists(string path) => File.Exists(path) || Directory.Exists(path);

        /// <summary>
        /// Whether two paths designate the same node, the file system of Windows being case insensitive.
        /// </summary>
        protected static bool PathsEqual(string left, string right)
            => string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Whether <paramref name="candidate"/> is <paramref name="path"/> itself or one of its parents.
        /// </summary>
        /// <remarks>
        /// Compared on whole segments, by ending both with a separator : a plain StartsWith would report "C:\foo2" as
        /// being inside "C:\foo".
        /// </remarks>
        protected static bool IsSameOrAncestor(string candidate, string path)
            => (Normalize(path) + Path.DirectorySeparatorChar).StartsWith(
                Normalize(candidate) + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase);

        private static string Normalize(string path) => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

        #endregion
    }
}