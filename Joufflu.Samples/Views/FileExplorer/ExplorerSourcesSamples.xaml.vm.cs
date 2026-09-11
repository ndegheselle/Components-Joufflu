using System.IO;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Feedback;
using Joufflu.FileExplorer.Sources;

namespace Joufflu.Samples.Views.FileExplorer;

public class ExplorerSourcesSamplesViewModel : ObservableObject
{
    /// <summary>Directory the watched sample writes into, emptied by the button of the sample.</summary>
    private readonly string watchedDirectoryPath = Path.Combine(Path.GetTempPath(), "Joufflu.Samples.Watcher");

    private int created;

    /// <summary>The disk, read once : the explorer displays what was there when a directory was opened.</summary>
    public IExplorerSource Source { get; private set; }

    /// <summary>The disk, kept in step with it : what the buttons write shows up without a refresh.</summary>
    public IExplorerSource WatcherSource { get; private set; }

    /// <summary>A source of the application, see <see cref="VirtualFilesSource"/>.</summary>
    public IExplorerSource VirtualSource { get; private set; }

    /// <summary>Writes into the watched directory, behind the back of the explorer.</summary>
    public ICommand CreateFileCommand { get; }
    public ICommand CreateDirectoryCommand { get; }
    public ICommand ClearCommand { get; }

    public string FileSystemSourceCode =>
        """
        // toasts is an injected IToastService, null is accepted
        public IExplorerSource Source { get; } = new FileSystemSource(@"C:\Projects", toasts);
        // ...
        await Source.Open();
        """;

    public string FileSystemWatcherSourceCode =>
        """
        // Same source, watching the disk : dispose it to stop watching.
        public IExplorerSource Source { get; } = new FileSystemWatcherSource(@"C:\Projects", toasts);
        // ...
        await Source.Open();
        """;

    public string VirtualNodesCode =>
        """
        // A node type of the application : it carries a path, and a state no file of the disk has.
        // An IExplorerNode and not an IExplorerFile : it has no size, so the size column stays empty on its row.
        public class VirtualFile : ObservableObject, IExplorerNode
        {
            public string Path { get; set; }
            public string Name { get; }
            public DateTime ModifiedAt { get; }
            public IExplorerDirectory? Parent { get; set; }

            public bool IsPinned { get => isPinned; set => SetProperty(ref isPinned, value); }
            private bool isPinned;

            public VirtualFile(IExplorerDirectory parent, string name)
            {
                Parent = parent;
                Name = name;
                Path = System.IO.Path.Combine(parent.Path, name);
                ModifiedAt = parent.ModifiedAt;
            }
        }

        // A source hands its own nodes over along with the ones it reads.
        public class VirtualFilesSource : FileSystemSource
        {
            protected override void LoadDirectory(IExplorerDirectory directory, int depth)
            {
                base.LoadDirectory(directory, depth);
                directory.Children.Add(GetVirtualFile(directory));
            }

            // Nothing to hand over to the shell : a virtual file is opened by the application itself.
            public override Task Open(IExplorerNode node) { ... }
        }

        <fileExplorer:Explorer Source="{Binding VirtualSource}">
            <fileExplorer:Explorer.Resources>
                <conv:TypeConverter x:Key="NodeTypeConverter" />

                <!-- Visual of the virtual nodes, implicit as the ones of the library are -->
                <DataTemplate DataType="{x:Type local:VirtualFile}">
                    <StackPanel Orientation="Horizontal" toolkit:Spacing.Gap="4">
                        <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.StickyNote}" />
                        <TextBlock Text="{Binding Name}" />
                    </StackPanel>
                </DataTemplate>

                <!-- Context menu keyed on the node type, replaces the one of the files nowhere else -->
                <DataTemplate x:Key="{base:ContextMenuTemplateKey local:VirtualFile}">
                    <ContextMenu>
                        <MenuItem Header="Pinned" IsCheckable="True"
                                  IsChecked="{Binding Node.IsPinned, Mode=TwoWay}" />
                        <MenuItem Header="Copy path" Command="{Binding Source.CopyPathCommand}"
                                  CommandParameter="{Binding Node}" />
                    </ContextMenu>
                </DataTemplate>
            </fileExplorer:Explorer.Resources>
            <fileExplorer:Explorer.ExtraColumns>
                <GridViewColumn Header="Pinned" Width="70">
                    <GridViewColumn.CellTemplate>
                        <DataTemplate>
                            <!-- The column is shared by every row : only the virtual nodes show a box -->
                            <CheckBox IsChecked="{Binding IsPinned, Mode=OneWay}" IsHitTestVisible="False">
                                <CheckBox.Style>
                                    <Style TargetType="CheckBox" BasedOn="{StaticResource {x:Type CheckBox}}">
                                        <Setter Property="Visibility" Value="Collapsed" />
                                        <Style.Triggers>
                                            <DataTrigger Value="{x:Type local:VirtualFile}"
                                                         Binding="{Binding Converter={StaticResource NodeTypeConverter}}">
                                                <Setter Property="Visibility" Value="Visible" />
                                            </DataTrigger>
                                        </Style.Triggers>
                                    </Style>
                                </CheckBox.Style>
                            </CheckBox>
                        </DataTemplate>
                    </GridViewColumn.CellTemplate>
                </GridViewColumn>
            </fileExplorer:Explorer.ExtraColumns>
        </fileExplorer:Explorer>
        """;

    public ExplorerSourcesSamplesViewModel(IToastService toasts)
    {
        Source = new FileSystemSource(Directory.GetCurrentDirectory(), toasts);
        Source.Open();

        Directory.CreateDirectory(watchedDirectoryPath);
        WatcherSource = new FileSystemWatcherSource(watchedDirectoryPath, toasts);
        WatcherSource.Open();

        VirtualSource = new VirtualFilesSource(Directory.GetCurrentDirectory(), toasts);
        VirtualSource.Open();

        CreateFileCommand = new RelayCommand(
            () => File.WriteAllText(Path.Combine(watchedDirectoryPath, $"File {++created}.txt"), "Written outside of the explorer."));

        CreateDirectoryCommand = new RelayCommand(
            () => Directory.CreateDirectory(Path.Combine(watchedDirectoryPath, $"Folder {++created}")));

        ClearCommand = new RelayCommand(() =>
        {
            foreach (string path in Directory.EnumerateFiles(watchedDirectoryPath))
                File.Delete(path);

            foreach (string path in Directory.EnumerateDirectories(watchedDirectoryPath))
                Directory.Delete(path, true);
        });
    }
}
