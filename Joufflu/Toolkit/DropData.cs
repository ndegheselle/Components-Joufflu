using System.Windows;

namespace Joufflu.Toolkit;

/// <summary>
/// The dragged data as it reaches a drop target : what the source carried, and where the pointer is
/// on the target, for the drops that land somewhere rather than just on something (a canvas placing
/// what is dropped under the cursor, a list inserting it at an index...).
/// <para>
/// It is what <see cref="DropTarget.CommandProperty"/> is given, as an
/// <see cref="IDataObject"/> delegating to the dragged data : a command only interested in the data
/// itself keeps taking an <see cref="IDataObject"/> and reads it as usual, one that places what it
/// receives casts to <see cref="DropData"/> to also get the <see cref="Position"/>.
/// </para>
/// </summary>
public sealed class DropData : IDataObject
{
    /// <summary>Data the drag carries, as the source gave it.</summary>
    public IDataObject Data { get; }

    /// <summary>
    /// Element the drop happens on, the one holding <see cref="DropTarget.CommandProperty"/>, so
    /// <see cref="Position"/> can be translated to whatever the target draws into.
    /// </summary>
    public UIElement Target { get; }

    /// <summary>Position of the pointer, relative to <see cref="Target"/>.</summary>
    public Point Position { get; }

    public DropData(IDataObject data, UIElement target, Point position)
    {
        Data = data;
        Target = target;
        Position = position;
    }

    #region Dragged data

    public object? GetData(string format) => Data.GetData(format);

    public object? GetData(Type format) => Data.GetData(format);

    public object? GetData(string format, bool autoConvert) => Data.GetData(format, autoConvert);

    public bool GetDataPresent(string format) => Data.GetDataPresent(format);

    public bool GetDataPresent(Type format) => Data.GetDataPresent(format);

    public bool GetDataPresent(string format, bool autoConvert) => Data.GetDataPresent(format, autoConvert);

    public string[] GetFormats() => Data.GetFormats();

    public string[] GetFormats(bool autoConvert) => Data.GetFormats(autoConvert);

    public void SetData(object data) => Data.SetData(data);

    public void SetData(string format, object data) => Data.SetData(format, data);

    public void SetData(Type format, object data) => Data.SetData(format, data);

    public void SetData(string format, object data, bool autoConvert) => Data.SetData(format, data, autoConvert);

    #endregion
}
