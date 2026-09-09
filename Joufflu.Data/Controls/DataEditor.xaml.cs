using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using NJsonSchema;

namespace Joufflu.Data.Controls;

/// <summary>
/// Fill a value in against a schema : one row per value the schema describes, each edited by the
/// widget its kind calls for rather than as the JSON it is stored as.
/// <para>
/// Give it a <see cref="Schema"/> or the <see cref="SchemaJson"/> it is stored as, and bind
/// <see cref="Json"/> to where the value lives — it is written back on every edit, and comes back
/// <see langword="null"/> when nothing was filled in.
/// </para>
/// <para>
/// Any field can hold one of <see cref="References"/> instead of a literal, whatever its declared
/// type : a reference stands where a value stands and is only resolved later, by whoever offered
/// the references. Leave the collection empty and the affordance never appears.
/// </para>
/// </summary>
public partial class DataEditor : UserControl
{
    public static readonly DependencyProperty SchemaProperty = DependencyProperty.Register(
        nameof(Schema), typeof(JsonSchema), typeof(DataEditor),
        new PropertyMetadata(null, (d, _) => ((DataEditor)d).Rebuild()));

    public static readonly DependencyProperty SchemaJsonProperty = DependencyProperty.Register(
        nameof(SchemaJson), typeof(string), typeof(DataEditor),
        new PropertyMetadata(null, (d, e) => ((DataEditor)d).Schema = Parse(e.NewValue as string)));

    public static readonly DependencyProperty JsonProperty = DependencyProperty.Register(
        nameof(Json), typeof(string), typeof(DataEditor),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (d, e) => ((DataEditor)d).OnJsonChanged((string?)e.NewValue)));

    public static readonly DependencyProperty ReferencesProperty = DependencyProperty.Register(
        nameof(References), typeof(IEnumerable<DataReference>), typeof(DataEditor),
        new PropertyMetadata(null, (d, _) => ((DataEditor)d).Rebuild()));

    public static readonly DependencyProperty ReferencePrefixProperty = DependencyProperty.Register(
        nameof(ReferencePrefix), typeof(string), typeof(DataEditor),
        new PropertyMetadata("$", (d, _) => ((DataEditor)d).Rebuild()));

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        nameof(IsReadOnly), typeof(bool), typeof(DataEditor), new PropertyMetadata(false));

    public static readonly DependencyProperty EmptyTextProperty = DependencyProperty.Register(
        nameof(EmptyText), typeof(string), typeof(DataEditor),
        new PropertyMetadata("No shape is expected, so there is nothing to fill in."));

    private static readonly DependencyPropertyKey DocumentPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Document), typeof(DataDocument), typeof(DataEditor), new PropertyMetadata(null));

    /// <summary>The value being filled in, written by the control.</summary>
    public static readonly DependencyProperty DocumentProperty = DocumentPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey IsEmptyPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsEmpty), typeof(bool), typeof(DataEditor), new PropertyMetadata(true));

    /// <summary>Whether there is no schema at all to fill anything in against.</summary>
    public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;

    /// <summary>The shape the value is filled in against.</summary>
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

    /// <summary>The value as it is stored, written back on every edit.</summary>
    public string? Json
    {
        get => (string?)GetValue(JsonProperty);
        set => SetValue(JsonProperty, value);
    }

    /// <summary>What a field may hold in place of a literal, empty when nothing may.</summary>
    public IEnumerable<DataReference>? References
    {
        get => (IEnumerable<DataReference>?)GetValue(ReferencesProperty);
        set => SetValue(ReferencesProperty, value);
    }

    /// <summary>
    /// What a reference is written as, so a stored value holding one is read back as a reference
    /// rather than as the text it is. Empty turns the whole feature off.
    /// </summary>
    public string ReferencePrefix
    {
        get => (string)GetValue(ReferencePrefixProperty);
        set => SetValue(ReferencePrefixProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>What to say when there is no schema, which is not the same thing everywhere.</summary>
    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public DataDocument? Document => (DataDocument?)GetValue(DocumentProperty);

    public bool IsEmpty => (bool)GetValue(IsEmptyProperty);

    /// <summary>
    /// The value as the one root of the tree : a TreeView takes a collection, and there is only
    /// ever one value.
    /// </summary>
    public ObservableCollection<DataNode> Roots { get; } = [];

    /// <summary>Every reference on offer, groups flattened out, for the pickers to filter.</summary>
    public ObservableCollection<DataReference> FlatReferences { get; } = [];

    /// <summary>Whether anything may be held in place of a literal at all.</summary>
    public bool AllowsReferences => FlatReferences.Count > 0;

    /// <summary>
    /// Whether the control is the one writing <see cref="Json"/>, in which case reading it back is
    /// not a caller handing over another value and must not rebuild the tree under the reader.
    /// </summary>
    private bool _isWriting;

    public DataEditor()
    {
        InitializeComponent();
        Rebuild();
    }

    private void OnJsonChanged(string? json)
    {
        if (_isWriting)
            return;

        Document?.Load(json);
    }

    private void Rebuild()
    {
        if (Document != null)
            Document.Changed -= OnDocumentChanged;

        FlatReferences.Clear();
        foreach (DataReference reference in References ?? [])
        {
            foreach (DataReference found in reference.For(EnumDataKind.Any))
                FlatReferences.Add(found);
        }

        DataDocument document = DataDocument.Create(Schema, Json, ReferencePrefix);
        document.Changed += OnDocumentChanged;

        SetValue(DocumentPropertyKey, document);
        SetValue(IsEmptyPropertyKey, Schema == null);

        Roots.Clear();
        if (document.Root != null)
            Roots.Add(document.Root);

        document.Validate();
    }

    private void OnDocumentChanged()
    {
        if (Document == null)
            return;

        _isWriting = true;
        try
        {
            Json = Document.ToJson();
        }
        finally
        {
            _isWriting = false;
        }

        Document.Validate();
    }

    /// <summary>
    /// Check what is currently filled in against the schema, handing each field what is held
    /// against it. Returns the errors as a whole, empty when there is nothing to hold.
    /// </summary>
    public IReadOnlyList<DataValidationError> Validate()
        => Document?.Validate() ?? [];

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

/// <summary>
/// The references a field of a given kind may hold : a field takes what resolves to its kind, and
/// anything at all when either side says nothing of a type.
/// </summary>
public class ReferencesForKindConverter : System.Windows.Data.IMultiValueConverter
{
    public static readonly ReferencesForKindConverter Default = new();

    public object Convert(object[] values, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not EnumDataKind kind || values[1] is not IEnumerable<DataReference> references)
            return Array.Empty<DataReference>();

        return references
            .Where(x => kind == EnumDataKind.Any || x.Kind == EnumDataKind.Any || x.Kind == kind)
            .ToList();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Which widget edits a value : one template per <see cref="EnumDataKind"/>, the object and the
/// array holding nothing of their own.
/// </summary>
public class DataNodeTemplateSelector : DataTemplateSelector
{
    public DataTemplate? StringTemplate { get; set; }
    public DataTemplate? IntegerTemplate { get; set; }
    public DataTemplate? NumberTemplate { get; set; }
    public DataTemplate? BooleanTemplate { get; set; }
    public DataTemplate? DateTemplate { get; set; }
    public DataTemplate? TimeTemplate { get; set; }
    public DataTemplate? EnumerationTemplate { get; set; }
    public DataTemplate? RawTemplate { get; set; }
    public DataTemplate? NoneTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is not DataNode node)
            return base.SelectTemplate(item, container);

        return node switch
        {
            DataRawNode => RawTemplate,
            DataObjectNode or DataArrayNode => NoneTemplate,
            _ => node.Kind switch
            {
                EnumDataKind.Enumeration => EnumerationTemplate,
                EnumDataKind.Integer => IntegerTemplate,
                EnumDataKind.Number => NumberTemplate,
                EnumDataKind.Boolean => BooleanTemplate,
                EnumDataKind.DateTime or EnumDataKind.Date => DateTemplate,
                EnumDataKind.Time or EnumDataKind.Duration => TimeTemplate,
                _ => StringTemplate
            }
        };
    }
}
