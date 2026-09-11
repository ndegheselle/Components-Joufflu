using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;

namespace Joufflu.Data;

/// <summary>
/// One value a schema describes, as it is being filled in : what it is called, what it may hold,
/// and what it currently holds.
/// <para>
/// A node is built from the schema and keeps that shape — the schema is what says which widget
/// edits it. Whatever it holds it can hold a <see cref="Reference"/> instead : a reference stands
/// where a value stands, at any depth and whatever the declared type, being resolved later.
/// </para>
/// </summary>
public abstract partial class DataNode : ObservableObject
{
    /// <summary>The schema this node was built from, its references already resolved.</summary>
    public JsonSchema? Schema { get; }

    public EnumDataKind Kind { get; }

    /// <summary>The property this node is declared as, "[i]" under an array, empty at the root.</summary>
    public string Name { get; private set; }

    /// <summary>What the schema says of the value, empty when it says nothing.</summary>
    public string Description { get; }

    public bool HasDescription => Description.Length > 0;

    /// <summary>What the schema declares the value to be, as the reader is told it.</summary>
    public string TypeLabel { get; }

    /// <summary>Whether a caller leaving the value out is not describing the shape.</summary>
    public bool IsRequired { get; }

    public DataNode? Parent { get; internal set; }

    /// <summary>Where the node sits in the value, e.g. <c>orders[0].label</c>.</summary>
    public string Path => Parent == null || Parent.Path.Length == 0
        ? Name
        : Name.StartsWith('[') ? Parent.Path + Name : $"{Parent.Path}.{Name}";

    /// <summary>Whether the value is a reference to be resolved rather than a literal.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLiteral))]
    private bool _isReference;

    /// <summary>What the reference reads, e.g. "$previous.Value". Only read while <see cref="IsReference"/>.</summary>
    [ObservableProperty]
    private string? _reference;

    public bool IsLiteral => !IsReference;

    /// <summary>Seeded so the shape as a whole comes open and the reader opens the rest.</summary>
    [ObservableProperty]
    private bool _isExpanded;

    /// <summary>What is held against the value as it currently stands, empty while nothing is.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrors), nameof(ErrorsText))]
    private IReadOnlyList<string> _errors = [];

    public bool HasErrors => Errors.Count > 0;

    public string ErrorsText => string.Join(Environment.NewLine, Errors);

    public virtual IEnumerable<DataNode> Children => [];

    /// <summary>
    /// Add an item, for the values holding as many as the reader adds. Null everywhere else, so a
    /// row that cannot grow shows no button rather than a dead one.
    /// </summary>
    public virtual IRelayCommand? AddItemCommand => null;

    /// <summary>
    /// Take this value out of what holds it, for an item of an array. Null everywhere else : a
    /// property is declared by the schema and stays, holding null rather than being removed.
    /// </summary>
    public IRelayCommand? RemoveItemCommand => (Parent as DataArrayNode)?.RemoveCommand;

    /// <summary>
    /// Hold nothing : whatever was filled in is dropped, so the value is written as the null it
    /// now is rather than as the empty value of its kind.
    /// </summary>
    [RelayCommand]
    private void SetNull()
    {
        IsReference = false;
        Reference = null;
        Clear();
    }

    /// <summary>Drop what the node holds, each kind holding its own thing.</summary>
    protected virtual void Clear() { }

    /// <summary>The document this node hangs under, which owns what a reference looks like.</summary>
    internal DataDocument? Document { get; set; }

    protected DataNode(string name, JsonSchema? schema, bool isRequired)
    {
        Name = name;
        Schema = schema?.ActualSchema;
        Kind = DataKind.Of(schema);
        IsRequired = isRequired;
        Description = Schema?.Description ?? string.Empty;
        TypeLabel = DataKind.Label(schema);
    }

    /// <summary>What the node holds, <see langword="null"/> when it holds nothing to be written.</summary>
    public JToken? ToToken()
    {
        // A reference is written as the string it is : what it points at is only known once
        // something resolves it, which is not this editor's business.
        if (IsReference)
            return string.IsNullOrWhiteSpace(Reference) ? null : new JValue(Reference);

        return ToLiteral();
    }

    protected abstract JToken? ToLiteral();

    /// <summary>Display [token] in this node, whatever it holds. Nothing leaves the value out.</summary>
    public void Load(JToken? token)
    {
        if (token == null)
        {
            IsReference = false;
            Reference = null;
            LoadLiteral(null);
            return;
        }

        if (token.Type == JTokenType.String && Document?.IsReference(token.Value<string>()) == true)
        {
            IsReference = true;
            Reference = token.Value<string>();
            return;
        }

        IsReference = false;
        LoadLiteral(token);
    }

    protected abstract void LoadLiteral(JToken? token);

    /// <summary>Every node of this subtree, itself first.</summary>
    public IEnumerable<DataNode> Walk()
    {
        yield return this;
        foreach (DataNode child in Children)
        {
            foreach (DataNode found in child.Walk())
                yield return found;
        }
    }

    internal void Rename(string name)
    {
        Name = name;
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Path));
    }

    /// <summary>
    /// Build the node [schema] describes, holding what the schema defaults to.
    /// <para>
    /// [walking] holds the schemas already being described : a schema is a graph and can reference
    /// itself, while what is edited is a tree, so a value already open higher up is not opened
    /// again — it is edited as raw JSON instead.
    /// </para>
    /// </summary>
    internal static DataNode Build(string name, JsonSchema? schema, bool isRequired, HashSet<JsonSchema> walking)
    {
        JsonSchema? actual = schema?.ActualSchema;
        EnumDataKind kind = DataKind.Of(actual);
        bool holdsChildren = kind is EnumDataKind.Object or EnumDataKind.Array;

        if (actual != null && holdsChildren && !walking.Add(actual))
            return new DataRawNode(name, actual, isRequired);

        try
        {
            return kind switch
            {
                EnumDataKind.Object => new DataObjectNode(name, actual!, isRequired, walking),
                EnumDataKind.Array => new DataArrayNode(name, actual!, isRequired, walking),
                EnumDataKind.Any => new DataRawNode(name, actual, isRequired),
                _ => new DataValueNode(name, actual, isRequired),
            };
        }
        finally
        {
            if (actual != null && holdsChildren)
                walking.Remove(actual);
        }
    }
}

/// <summary>
/// A value the schema describes on its own : a string, a number, a date, one of a list.
/// </summary>
public partial class DataValueNode : DataNode
{
    [ObservableProperty] private object? _value;

    /// <summary>What the schema restricts the value to, empty when it restricts nothing.</summary>
    public IReadOnlyList<DataOption> Options { get; }

    public DataValueNode(string name, JsonSchema? schema, bool isRequired) : base(name, schema, isRequired)
    {
        Options = OptionsOf(Schema);
        _value = Default();
    }

    /// <summary>
    /// What [schema] restricts the value to, each option named. A schema derived from a .NET enum
    /// restricts the value to the numbers behind the names and puts the names beside them, so an
    /// option shows as what it was written as rather than as the number it is stored as.
    /// </summary>
    private static IReadOnlyList<DataOption> OptionsOf(JsonSchema? schema)
    {
        if (schema?.IsEnumeration != true)
            return [];

        object?[] values = [.. schema.Enumeration];
        string[] names = [.. schema.EnumerationNames];

        return
        [
            .. values
                .Select((value, index) => new DataOption(value, index < names.Length ? names[index] : $"{value}"))
                .Where(x => x.Value != null)
        ];
    }

    protected override void Clear() => Value = null;

    protected override JToken? ToLiteral() => Value == null ? JValue.CreateNull() : JToken.FromObject(Value);

    protected override void LoadLiteral(JToken? token)
    {
        if (token == null)
        {
            Value = Default();
            return;
        }

        Value = Kind switch
        {
            EnumDataKind.Boolean => token.Type == JTokenType.Boolean ? token.Value<bool>() : Default(),
            EnumDataKind.Integer => token.Type is JTokenType.Integer or JTokenType.Float ? token.Value<long>() : Default(),
            EnumDataKind.Number => token.Type is JTokenType.Integer or JTokenType.Float ? token.Value<double>() : Default(),
            EnumDataKind.DateTime or EnumDataKind.Date => AsDate(token) ?? Default(),
            EnumDataKind.Time or EnumDataKind.Duration => AsSpan(token) ?? Default(),
            _ => token.Type == JTokenType.Null ? null : token.ToString(),
        };
    }

    private static object? AsDate(JToken token)
        => DateTime.TryParse(token.ToString(), out DateTime parsed) ? parsed : null;

    private static object? AsSpan(JToken token)
        => TimeSpan.TryParse(token.ToString(), out TimeSpan parsed) ? parsed : null;

    /// <summary>What the value starts as : the schema default, or the empty value of its kind.</summary>
    private object? Default()
    {
        if (Schema?.Default != null)
            return Schema.Default;

        if (Options.Count > 0)
            return Options[0].Value;

        return Kind switch
        {
            EnumDataKind.Boolean => false,
            EnumDataKind.Integer => 0L,
            EnumDataKind.Number => 0d,
            EnumDataKind.DateTime or EnumDataKind.Date => DateTime.Now,
            EnumDataKind.Time or EnumDataKind.Duration => TimeSpan.Zero,
            _ => string.Empty,
        };
    }
}

/// <summary>An object, holding one node per property the schema declares.</summary>
public partial class DataObjectNode : DataNode
{
    public ObservableCollection<DataNode> Properties { get; } = [];

    public override IEnumerable<DataNode> Children => Properties;

    internal DataObjectNode(string name, JsonSchema schema, bool isRequired, HashSet<JsonSchema> walking)
        : base(name, schema, isRequired)
    {
        foreach ((string property, JsonSchemaProperty declared) in schema.ActualProperties)
        {
            DataNode node = Build(property, declared, declared.IsRequired, walking);
            node.Parent = this;
            Properties.Add(node);
        }

        IsExpanded = true;
    }

    protected override void Clear()
    {
        foreach (DataNode property in Properties)
            property.SetNullCommand.Execute(null);
    }

    protected override JToken? ToLiteral()
    {
        var written = new JObject();
        foreach (DataNode property in Properties)
        {
            if (property.ToToken() is JToken token)
                written[property.Name] = token;
        }
        return written;
    }

    protected override void LoadLiteral(JToken? token)
    {
        JObject? values = token as JObject;
        foreach (DataNode property in Properties)
            property.Load(values?[property.Name]);
    }
}

/// <summary>
/// An array, holding as many nodes as the reader adds : a schema describes what an array holds
/// rather than how many, so every item is built from the one schema it declares.
/// </summary>
public partial class DataArrayNode : DataNode
{
    public ObservableCollection<DataNode> Items { get; } = [];

    public override IEnumerable<DataNode> Children => Items;

    public override IRelayCommand? AddItemCommand => AddCommand;

    /// <summary>The schema every item is built from, null when the array declares none.</summary>
    private readonly JsonSchema? _item;

    internal DataArrayNode(string name, JsonSchema schema, bool isRequired, HashSet<JsonSchema> walking)
        : base(name, schema, isRequired)
    {
        _item = schema.Item;
    }

    [RelayCommand]
    private void Add()
    {
        Insert(Items.Count, null);
        IsExpanded = true;
    }

    [RelayCommand]
    private void Remove(DataNode? item)
    {
        if (item == null || !Items.Remove(item))
            return;

        Renumber();
        Document?.Refresh();
    }

    private void Insert(int index, JToken? value)
    {
        // A guard of its own : an array holds nothing until an item is added, so a schema
        // referencing itself through one is opened a level at a time by the reader rather than all
        // at once. Each item is a fresh walk and cannot loop on its own.
        DataNode node = Build($"[{index}]", _item, isRequired: true, []);
        node.Parent = this;
        Document?.Adopt(node);
        node.Load(value);

        Items.Insert(index, node);
        Renumber();
        Document?.Refresh();
    }

    /// <summary>An item is read by its position, so the ones after a removal are named again.</summary>
    private void Renumber()
    {
        for (int index = 0; index < Items.Count; index++)
            Items[index].Rename($"[{index}]");
    }

    protected override void Clear()
    {
        Items.Clear();
        Document?.Refresh();
    }

    protected override JToken? ToLiteral()
    {
        var written = new JArray();
        foreach (DataNode item in Items)
            written.Add(item.ToToken() ?? JValue.CreateNull());
        return written;
    }

    protected override void LoadLiteral(JToken? token)
    {
        Items.Clear();
        if (token is not JArray values)
            return;

        foreach (JToken value in values)
            Insert(Items.Count, value);
    }
}

/// <summary>
/// A value no schema describes closely enough to build a widget for : edited as the JSON it is.
/// This is what a schema saying nothing gets, and what a schema referencing itself gets once the
/// tree has already opened it.
/// </summary>
public partial class DataRawNode : DataNode
{
    /// <summary>The value as text, so what does not fit a shape is still editable.</summary>
    [ObservableProperty]
    private string? _json;

    public DataRawNode(string name, JsonSchema? schema, bool isRequired) : base(name, schema, isRequired)
    { }

    protected override void Clear() => Json = null;

    protected override JToken? ToLiteral()
    {
        if (string.IsNullOrWhiteSpace(Json))
            return null;

        try
        {
            return JToken.Parse(Json);
        }
        catch (JsonException)
        {
            // Kept as it was typed : the document reports it, and dropping it would lose the work.
            return null;
        }
    }

    protected override void LoadLiteral(JToken? token)
        => Json = token == null ? null : token.ToString(Formatting.Indented);

    /// <summary>What is wrong with the text, null while it reads as JSON.</summary>
    public string? ParseError()
    {
        if (string.IsNullOrWhiteSpace(Json))
            return null;

        try
        {
            JToken.Parse(Json);
            return null;
        }
        catch (JsonException exception)
        {
            return exception.Message;
        }
    }
}

/// <summary>One of the values a schema restricts a value to, as it is stored and as it reads.</summary>
public record DataOption(object? Value, string Name)
{
    public override string ToString() => Name;
}
