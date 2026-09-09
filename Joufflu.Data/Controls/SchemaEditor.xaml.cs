using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using NJsonSchema;

namespace Joufflu.Data.Controls;

/// <summary>
/// Write a JSON Schema as a tree : one row per value, holding what it is called, what it may hold,
/// whether it is required and what the author says of it.
/// <para>
/// The schema round-trips through <see cref="Json"/>, so a caller storing it as text binds that and
/// nothing else. A schema describing nothing writes back <see langword="null"/> rather than "{}" :
/// storing "no schema" as null keeps storing null.
/// </para>
/// </summary>
public partial class SchemaEditor : UserControl
{
    public static readonly DependencyProperty JsonProperty = DependencyProperty.Register(
        nameof(Json), typeof(string), typeof(SchemaEditor),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (d, e) => ((SchemaEditor)d).OnJsonChanged((string?)e.NewValue)));

    private static readonly DependencyPropertyKey DocumentPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Document), typeof(SchemaDocument), typeof(SchemaEditor), new PropertyMetadata(null));

    /// <summary>The tree being edited, written by the control.</summary>
    public static readonly DependencyProperty DocumentProperty = DocumentPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey ErrorPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Error), typeof(string), typeof(SchemaEditor), new PropertyMetadata(null));

    /// <summary>What could not be read of what was given, written by the control.</summary>
    public static readonly DependencyProperty ErrorProperty = ErrorPropertyKey.DependencyProperty;

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        nameof(IsReadOnly), typeof(bool), typeof(SchemaEditor), new PropertyMetadata(false));

    /// <summary>The schema as it is stored, written back on every edit.</summary>
    public string? Json
    {
        get => (string?)GetValue(JsonProperty);
        set => SetValue(JsonProperty, value);
    }

    public SchemaDocument? Document => (SchemaDocument?)GetValue(DocumentProperty);

    public string? Error => (string?)GetValue(ErrorProperty);

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>The kinds a value can be given, which is every one a schema can declare.</summary>
    public static IReadOnlyList<EnumDataKind> Kinds { get; } = Enum.GetValues<EnumDataKind>();

    /// <summary>
    /// The shape as a whole, as the one root of the tree : a TreeView takes a collection, and there
    /// is only ever one schema.
    /// </summary>
    public ObservableCollection<SchemaNode> Roots { get; } = [];

    /// <summary>
    /// Whether the control is the one writing <see cref="Json"/>, in which case reading it back is
    /// not a caller handing over another schema and must not rebuild the tree under the author.
    /// </summary>
    private bool _isWriting;

    public SchemaEditor()
    {
        InitializeComponent();
        Load(null);
    }

    private void OnJsonChanged(string? json)
    {
        if (_isWriting)
            return;

        Load(json);
    }

    private void Load(string? json)
    {
        if (Document != null)
            Document.Changed -= OnDocumentChanged;

        SchemaDocument document = SchemaDocument.Create(json);
        document.Changed += OnDocumentChanged;

        SetValue(DocumentPropertyKey, document);
        SetValue(ErrorPropertyKey, document.LoadError);

        Roots.Clear();
        Roots.Add(document.Root);
    }

    private void OnDocumentChanged()
    {
        if (Document == null)
            return;

        _isWriting = true;
        try
        {
            Json = Document.ToJson();
            SetValue(ErrorPropertyKey, null);
        }
        catch (Exception exception)
        {
            SetValue(ErrorPropertyKey, exception.Message);
        }
        finally
        {
            _isWriting = false;
        }
    }

    /// <summary>The schema as it currently stands, for a caller wanting it parsed.</summary>
    public JsonSchema? ToSchema() => Document?.ToSchema();
}
