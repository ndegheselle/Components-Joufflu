using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Joufflu.Samples.Views.Toolkit;

public class DropTargetSamplesViewModel : ObservableObject
{
    /// <summary>Names of the last accepted files, listed under the drop zone.</summary>
    public ObservableCollection<string> DroppedFiles { get; } = [];

    /// <summary>
    /// Handles the drop, and decides what the zone accepts through its <c>CanExecute</c> : a drag
    /// of files holding nothing but PDFs.
    /// </summary>
    public IRelayCommand DropFilesCommand { get; }

    public DropTargetSamplesViewModel()
    {
        DropFilesCommand = new RelayCommand<IDataObject>(DropFiles, CanDropFiles);
        TakeTagCommand = new RelayCommand<IDataObject>(TakeTag, CanTakeTag);
    }

    // Called on every mouse move of the drag, so it only looks at the paths and never at the files.
    private static bool CanDropFiles(IDataObject? data)
    {
        string[]? files = GetFiles(data);
        return files?.Length > 0 && files.All(IsPdf);
    }

    private void DropFiles(IDataObject? data)
    {
        DroppedFiles.Clear();
        foreach (string file in GetFiles(data) ?? [])
            DroppedFiles.Add(Path.GetFileName(file));
    }

    /// <summary>Paths of a file drag, or <c>null</c> for data that isn't one (a text selection, an image, …).</summary>
    private static string[]? GetFiles(IDataObject? data) => data?.GetData(DataFormats.FileDrop) as string[];

    private static bool IsPdf(string path) => Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    /// <summary>Tags left to drag, each one its own drag source.</summary>
    public ObservableCollection<string> AvailableTags { get; } = ["Design", "Toolkit", "Samples"];

    /// <summary>Tags the drop target took away from <see cref="AvailableTags"/>.</summary>
    public ObservableCollection<string> TakenTags { get; } = [];

    /// <summary>Takes a dragged tag, and refuses anything that isn't one of them.</summary>
    public IRelayCommand TakeTagCommand { get; }

    // Only the tags of this sample : a text selection dragged from elsewhere isn't one of them, and
    // a tag already taken isn't available anymore.
    private bool CanTakeTag(IDataObject? data) => GetTag(data) is string tag && AvailableTags.Contains(tag);

    private void TakeTag(IDataObject? data)
    {
        if (GetTag(data) is not string tag)
            return;

        AvailableTags.Remove(tag);
        TakenTags.Add(tag);
    }

    /// <summary>The dragged tag, or <c>null</c> for data that isn't text at all.</summary>
    private static string? GetTag(IDataObject? data) => data?.GetData(DataFormats.UnicodeText) as string;

    public string DropCode =>
        """
        <!-- AllowDrop and the drag events are handled by the behavior -->
        <Border toolkit:DropTarget.Command="{Binding DropFilesCommand}"
                BorderThickness="{DynamicResource {x:Static joufflu:Dimensions.BorderThickness}}">
            <Border.Style>
                <Style TargetType="Border">
                    <Setter Property="Background" Value="Transparent" />
                    <Style.Triggers>
                        <!-- True only while accepted data hovers: refused files never highlight -->
                        <Trigger Property="toolkit:DropTarget.IsDragOver" Value="True">
                            <Setter Property="Background" Value="{DynamicResource {x:Static joufflu:Brushes.Primary100Brush}}" />
                            <Setter Property="BorderBrush" Value="{DynamicResource {x:Static joufflu:Brushes.PrimaryBrush}}" />
                        </Trigger>
                    </Style.Triggers>
                </Style>
            </Border.Style>
            <TextBlock VerticalAlignment="Center" Text="Drop .pdf files here" />
        </Border>

        // The command's CanExecute is the whole filter: what it refuses can't be dropped
        public IRelayCommand DropFilesCommand { get; }
            = new RelayCommand<IDataObject>(DropFiles, CanDropFiles);

        private static bool CanDropFiles(IDataObject? data)
        {
            string[]? files = data?.GetData(DataFormats.FileDrop) as string[];
            return files?.Length > 0 && files.All(f => Path.GetExtension(f).Equals(".pdf", StringComparison.OrdinalIgnoreCase));
        }

        // Same IDataObject, once the drop landed
        private static void DropFiles(IDataObject? data) { ... }
        """;

    public string DragCode =>
        """
        <!-- The mouse events are handled by the behavior: the drag starts past the system threshold -->
        <Border toolkit:DragSource.Data="{Binding}"
                toolkit:DragSource.AllowedEffects="Move">
            <Border.Style>
                <Style TargetType="Border">
                    <Style.Triggers>
                        <!-- True for the whole drag: the original fades out while it travels -->
                        <Trigger Property="toolkit:DragSource.IsDragging" Value="True">
                            <Setter Property="Opacity" Value="0.4" />
                        </Trigger>
                    </Style.Triggers>
                </Style>
            </Border.Style>
            <TextBlock Text="{Binding}" />
        </Border>

        <!-- AllowedEffects and Effect must agree, here on Move, for the drop to happen -->
        <Border toolkit:DropTarget.Command="{Binding TakeTagCommand}"
                toolkit:DropTarget.Effect="Move" />

        // Data that isn't an IDataObject is wrapped in a DataObject, so a string arrives as text
        private static string? GetTag(IDataObject? data) => data?.GetData(DataFormats.UnicodeText) as string;

        private bool CanTakeTag(IDataObject? data) => GetTag(data) is string tag && AvailableTags.Contains(tag);
        """;
}
