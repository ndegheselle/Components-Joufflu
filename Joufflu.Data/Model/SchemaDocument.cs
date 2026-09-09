using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NJsonSchema;

namespace Joufflu.Data;

/// <summary>
/// A schema being written : the tree of what it describes, and the JSON Schema it amounts to.
/// <para>
/// The schema round-trips as text and <see cref="ToJson"/> gives back <see langword="null"/> for
/// one describing nothing, so a caller storing "no schema" as null keeps storing null.
/// </para>
/// </summary>
public partial class SchemaDocument : ObservableObject
{
    /// <summary>The shape as a whole, which is what the author edits.</summary>
    public SchemaNode Root { get; }

    /// <summary>What the document could not read, null when it read what it was given.</summary>
    public string? LoadError { get; private set; }

    /// <summary>Raised whenever anything in the tree changes, so a host can write the schema back.</summary>
    public event Action? Changed;

    private SchemaDocument(SchemaNode root)
    {
        Root = root;
        Adopt(root);
    }

    /// <summary>
    /// A document editing [json] as a schema. Text that is not a schema is reported through
    /// <see cref="LoadError"/> and edited as an empty object rather than dropped on the floor.
    /// </summary>
    public static SchemaDocument Create(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new SchemaDocument(new SchemaNode(EnumDataKind.Object) { IsExpanded = true });

        try
        {
            JsonSchema schema = JsonSchema.FromJsonAsync(json).Result;
            return new SchemaDocument(SchemaNode.From(string.Empty, schema, isRequired: false, []));
        }
        catch (Exception exception)
        {
            return new SchemaDocument(new SchemaNode(EnumDataKind.Object) { IsExpanded = true })
            {
                LoadError = Unwrap(exception).Message
            };
        }
    }

    /// <summary>A document editing [schema].</summary>
    public static SchemaDocument Create(JsonSchema? schema)
        => schema == null
            ? new SchemaDocument(new SchemaNode(EnumDataKind.Object) { IsExpanded = true })
            : new SchemaDocument(SchemaNode.From(string.Empty, schema, isRequired: false, []));

    /// <summary>What the author has described, as the schema it is.</summary>
    public JsonSchema ToSchema() => Root.ToSchema();

    /// <summary>
    /// What the author has described, <see langword="null"/> when they have described nothing : an
    /// object with no property says the same thing as no schema at all, and is not worth storing.
    /// </summary>
    public string? ToJson()
    {
        if (Root.Kind == EnumDataKind.Object && Root.Properties.Count == 0 && string.IsNullOrWhiteSpace(Root.Description))
            return null;

        return ToSchema().ToJson();
    }

    /// <summary>Take [node] and everything under it into the document, and follow their changes.</summary>
    private void Adopt(SchemaNode node)
    {
        foreach (SchemaNode inner in node.Walk())
        {
            inner.PropertyChanged -= OnNodeChanged;
            inner.PropertyChanged += OnNodeChanged;
            inner.Properties.CollectionChanged -= OnPropertiesChanged;
            inner.Properties.CollectionChanged += OnPropertiesChanged;
        }
    }

    private void OnPropertiesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (SchemaNode added in e.NewItems?.OfType<SchemaNode>() ?? [])
            Adopt(added);

        Refresh();
    }

    private void OnNodeChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SchemaNode.IsExpanded))
            return;

        // A kind change puts a new item in place, which has to be followed like the rest.
        if (e.PropertyName == nameof(SchemaNode.Item) && sender is SchemaNode { Item: SchemaNode item })
            Adopt(item);

        Refresh();
    }

    private void Refresh() => Changed?.Invoke();

    /// <summary>
    /// What actually went wrong. The schema is parsed by blocking on the asynchronous read, so a
    /// failure arrives wrapped and the wrapper says nothing useful.
    /// </summary>
    private static Exception Unwrap(Exception exception)
        => exception is AggregateException aggregate && aggregate.InnerException != null
            ? aggregate.InnerException
            : exception;
}
