using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using NJsonSchema;

namespace Joufflu.Data.Controls;

/// <summary>
/// A JSON Schema as a tree of the values it describes, read only : each value read by the glyph of
/// its kind rather than by its declaration, because a shape is what a reader needs in front of them
/// and the schema it is written as is not.
/// <para>
/// Give it a <see cref="Schema"/>, or the <see cref="SchemaJson"/> it is stored as — most callers
/// hold the text rather than the schema. Text that is not a schema displays as none rather than as
/// an error : whoever edits it is where that is reported.
/// </para>
/// </summary>
public partial class SchemaView : UserControl
{
    public static readonly DependencyProperty SchemaProperty = DependencyProperty.Register(
        nameof(Schema), typeof(JsonSchema), typeof(SchemaView),
        new PropertyMetadata(null, (d, _) => ((SchemaView)d).Rebuild()));

    public static readonly DependencyProperty SchemaJsonProperty = DependencyProperty.Register(
        nameof(SchemaJson), typeof(string), typeof(SchemaView),
        new PropertyMetadata(null, (d, e) => ((SchemaView)d).Schema = Parse(e.NewValue as string)));

    public static readonly DependencyProperty EmptyTextProperty = DependencyProperty.Register(
        nameof(EmptyText), typeof(string), typeof(SchemaView),
        new PropertyMetadata("No shape is expected."));

    private static readonly DependencyPropertyKey IsEmptyPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsEmpty), typeof(bool), typeof(SchemaView), new PropertyMetadata(true));

    /// <summary>
    /// Whether no schema was given at all. A schema describing nothing is not empty : it says that
    /// anything goes, which is worth reading.
    /// </summary>
    public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;

    public JsonSchema? Schema
    {
        get => (JsonSchema?)GetValue(SchemaProperty);
        set => SetValue(SchemaProperty, value);
    }

    /// <summary>The schema as it is stored. Setting it parses it into <see cref="Schema"/>.</summary>
    public string? SchemaJson
    {
        get => (string?)GetValue(SchemaJsonProperty);
        set => SetValue(SchemaJsonProperty, value);
    }

    /// <summary>What to say when there is no schema, which is not the same thing everywhere.</summary>
    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public bool IsEmpty => (bool)GetValue(IsEmptyProperty);

    /// <summary>The schema as a single root standing for the shape as a whole.</summary>
    public ObservableCollection<SchemaNode> Entries { get; } = [];

    public SchemaView()
    {
        InitializeComponent();
    }

    private void Rebuild()
    {
        Entries.Clear();
        if (Schema != null)
        {
            SchemaNode root = SchemaNode.From(string.Empty, Schema, isRequired: false, []);
            root.IsExpanded = true;
            Entries.Add(root);
        }

        SetValue(IsEmptyPropertyKey, Schema == null);
    }

    /// <summary>[json] as a schema, null when it holds none or when what it holds is not one.</summary>
    private static JsonSchema? Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSchema.FromJsonAsync(json).Result;
        }
        catch
        {
            return null;
        }
    }
}
