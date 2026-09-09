using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NJsonSchema;

namespace Joufflu.Data;

/// <summary>
/// One value of a schema being written : what it is called, what it may hold, and whatever the
/// author says about it.
/// <para>
/// One class rather than one per kind : the author changes what a value holds by picking another
/// kind, and a node that had to be replaced in its parent every time would make that a move rather
/// than an edit. What a kind does not use is simply not shown.
/// </para>
/// </summary>
public partial class SchemaNode : ObservableObject
{
    /// <summary>
    /// The property this value is declared as. Empty at the root and under an array, neither of
    /// which is declared as anything.
    /// </summary>
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsObject), nameof(IsArray), nameof(IsValue), nameof(IsEnumeration))]
    private EnumDataKind _kind;

    /// <summary>What the author says of the value, shown to whoever fills it in.</summary>
    [ObservableProperty]
    private string? _description;

    /// <summary>Whether a caller leaving the value out is not describing this shape.</summary>
    [ObservableProperty]
    private bool _isRequired;

    /// <summary>What the value holds when a caller gives none, empty when it has no default.</summary>
    [ObservableProperty]
    private string? _default;

    /// <summary>Seeded so the shape as a whole comes open.</summary>
    [ObservableProperty]
    private bool _isExpanded;

    public bool IsObject => Kind == EnumDataKind.Object;
    public bool IsArray => Kind == EnumDataKind.Array;
    public bool IsEnumeration => Kind == EnumDataKind.Enumeration;

    /// <summary>Whether the value is one the author describes on its own rather than through others.</summary>
    public bool IsValue => !IsObject && !IsArray;

    /// <summary>What an object holds, empty for anything else.</summary>
    public ObservableCollection<SchemaNode> Properties { get; } = [];

    /// <summary>What an array holds, null for anything else.</summary>
    [ObservableProperty]
    private SchemaNode? _item;

    /// <summary>What an enumeration restricts the value to, one per line.</summary>
    [ObservableProperty]
    private string? _options;

    public SchemaNode? Parent { get; internal set; }

    /// <summary>The root and the item of an array are not declared as a property of anything.</summary>
    public bool IsNamed => Parent?.Kind == EnumDataKind.Object;

    /// <summary>What the tree shows under this node.</summary>
    public IEnumerable<SchemaNode> Children => Kind switch
    {
        EnumDataKind.Object => Properties,
        EnumDataKind.Array => Item == null ? [] : [Item],
        _ => []
    };

    public SchemaNode(EnumDataKind kind = EnumDataKind.Object)
    {
        _kind = kind;
        if (kind == EnumDataKind.Array)
            _item = new SchemaNode(EnumDataKind.String) { Parent = this };
    }

    /// <summary>
    /// Follow the author picking another kind : what the previous one held is dropped, and what the
    /// new one needs is put in place. An array always holds something, even before it is described.
    /// </summary>
    partial void OnKindChanged(EnumDataKind value)
    {
        if (value != EnumDataKind.Object)
            Properties.Clear();

        if (value == EnumDataKind.Array)
            Item ??= new SchemaNode(EnumDataKind.String) { Parent = this };
        else
            Item = null;

        if (value != EnumDataKind.Enumeration)
            Options = null;

        IsExpanded = true;
        OnPropertyChanged(nameof(Children));
    }

    [RelayCommand]
    private void AddProperty()
    {
        Properties.Add(new SchemaNode(EnumDataKind.String)
        {
            Parent = this,
            Name = UniqueName("property")
        });

        IsExpanded = true;
        OnPropertyChanged(nameof(Children));
    }

    [RelayCommand]
    private void RemoveProperty(SchemaNode? property)
    {
        if (property != null && Properties.Remove(property))
            OnPropertyChanged(nameof(Children));
    }

    /// <summary>A property is read by its name within its object, so no two may share one.</summary>
    private string UniqueName(string baseName)
    {
        if (Properties.All(x => x.Name != baseName))
            return baseName;

        int index = 2;
        while (Properties.Any(x => x.Name == $"{baseName} {index}"))
            index++;
        return $"{baseName} {index}";
    }

    /// <summary>What this node declares, as the schema it is.</summary>
    public JsonSchema ToSchema()
    {
        (JsonObjectType type, string? format) = DataKind.Declare(Kind);
        var schema = new JsonSchema { Type = type, Format = format };

        if (!string.IsNullOrWhiteSpace(Description))
            schema.Description = Description;

        switch (Kind)
        {
            case EnumDataKind.Object:
                foreach (SchemaNode property in Properties)
                {
                    if (string.IsNullOrWhiteSpace(property.Name))
                        continue;

                    JsonSchemaProperty declared = property.ToSchemaProperty();
                    declared.IsRequired = property.IsRequired;
                    schema.Properties[property.Name] = declared;
                }
                break;

            case EnumDataKind.Array:
                schema.Item = Item?.ToSchema() ?? new JsonSchema();
                break;

            case EnumDataKind.Enumeration:
                foreach (string option in SplitOptions())
                    schema.Enumeration.Add(option);
                break;
        }

        if (!string.IsNullOrWhiteSpace(Default))
            schema.Default = Default;

        return schema;
    }

    /// <summary>
    /// The same as <see cref="ToSchema"/>, as the property of an object. NJsonSchema keeps the two
    /// apart : only a property carries whether it is required.
    /// </summary>
    private JsonSchemaProperty ToSchemaProperty()
    {
        var property = new JsonSchemaProperty();
        JsonSchema declared = ToSchema();

        property.Type = declared.Type;
        property.Format = declared.Format;
        property.Description = declared.Description;
        property.Default = declared.Default;
        property.Item = declared.Item;

        foreach ((string name, JsonSchemaProperty inner) in declared.Properties)
            property.Properties[name] = inner;

        foreach (object? option in declared.Enumeration)
            property.Enumeration.Add(option);

        return property;
    }

    /// <summary>The options as they were typed, one per line, the blank ones left out.</summary>
    private IEnumerable<string> SplitOptions()
        => (Options ?? string.Empty)
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>
    /// [schema] as a node to edit. [walking] guards a schema referencing itself : what is already
    /// open higher up is left as the kind it is rather than opened again.
    /// </summary>
    internal static SchemaNode From(string name, JsonSchema? schema, bool isRequired, HashSet<JsonSchema> walking)
    {
        JsonSchema? actual = schema?.ActualSchema;
        var node = new SchemaNode(DataKind.Of(actual))
        {
            Name = name,
            IsRequired = isRequired,
            Description = actual?.Description,
            Default = actual?.Default?.ToString()
        };

        if (actual == null)
            return node;

        if (node.Kind == EnumDataKind.Enumeration)
            node.Options = string.Join(Environment.NewLine, actual.Enumeration.Where(x => x != null));

        bool holdsChildren = node.Kind is EnumDataKind.Object or EnumDataKind.Array;
        if (!holdsChildren || !walking.Add(actual))
            return node;

        try
        {
            if (node.Kind == EnumDataKind.Object)
            {
                foreach ((string property, JsonSchemaProperty declared) in actual.ActualProperties)
                {
                    SchemaNode child = From(property, declared, declared.IsRequired, walking);
                    child.Parent = node;
                    node.Properties.Add(child);
                }
            }
            else
            {
                SchemaNode item = From(string.Empty, actual.Item, isRequired: false, walking);
                item.Parent = node;
                node.Item = item;
            }
        }
        finally
        {
            walking.Remove(actual);
        }

        node.IsExpanded = true;
        return node;
    }

    /// <summary>Every node of this subtree, itself first.</summary>
    public IEnumerable<SchemaNode> Walk()
    {
        yield return this;
        foreach (SchemaNode child in Children)
        {
            foreach (SchemaNode found in child.Walk())
                yield return found;
        }
    }
}
