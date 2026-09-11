using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;

namespace Joufflu.Data;

/// <summary>
/// A value being filled in against a schema : the tree the schema describes, what it currently
/// holds, and what the schema holds against it.
/// <para>
/// The value round-trips as text and <see cref="ToJson"/> gives back <see langword="null"/> for an
/// empty one : a caller storing "nothing" as null keeps storing null rather than "{}".
/// </para>
/// <para>
/// A schema that cannot be read, or a value that cannot be fitted to one, falls back on
/// <see cref="RawJson"/> — the text is kept and edited as it is rather than lost.
/// </para>
/// </summary>
public partial class DataDocument : ObservableObject
{
    /// <summary>What the schema describes, null while the document is raw.</summary>
    public DataNode? Root { get; }

    public JsonSchema? Schema { get; }

    /// <summary>
    /// The value as text, only while the schema could not be used. Editing it is the escape hatch
    /// out of a shape that does not fit.
    /// </summary>
    [ObservableProperty]
    private string? _rawJson;

    /// <summary>Whether the value is edited as text rather than against the schema.</summary>
    public bool IsRaw => Root == null;

    /// <summary>What the document could not do with what it was given, empty when it could.</summary>
    public string? LoadError { get; private set; }

    /// <summary>
    /// What a reference is written as. A value starting with it is held as a reference rather than
    /// as the text it is; empty turns the whole feature off, so a caller offering no reference
    /// never sees one of its values turn into one.
    /// </summary>
    public string ReferencePrefix { get; }

    /// <summary>Raised whenever anything in the tree changes, so a host can write the value back.</summary>
    public event Action? Changed;

    /// <summary>Every node of the tree, the root first. Empty while the document is raw.</summary>
    public IEnumerable<DataNode> Nodes => Root?.Walk() ?? [];

    private bool _isLoading;

    private DataDocument(JsonSchema? schema, string referencePrefix)
    {
        Schema = schema;
        ReferencePrefix = referencePrefix;

        if (schema == null)
            return;

        try
        {
            Root = DataNode.Build(string.Empty, schema, isRequired: true, []);
            Adopt(Root);
        }
        catch (Exception exception)
        {
            // A schema that cannot be walked describes nothing usable : the value is edited as the
            // text it is rather than against a tree built out of half of it.
            Root = null;
            LoadError = exception.Message;
        }
    }

    /// <summary>
    /// A document filling [json] into what [schema] describes. Without a schema, or with one that
    /// cannot be walked, the value is edited as text.
    /// </summary>
    public static DataDocument Create(JsonSchema? schema, string? json, string referencePrefix = "$")
    {
        var document = new DataDocument(schema, referencePrefix);
        document.Load(json);
        return document;
    }

    /// <summary>Display [json], whatever it holds and whether or not it fits the schema.</summary>
    public void Load(string? json)
    {
        _isLoading = true;
        try
        {
            RawJson = json;

            if (Root == null)
                return;

            JToken? token = null;
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    token = JToken.Parse(json);
                }
                catch (JsonException exception)
                {
                    LoadError = exception.Message;
                }
            }

            Root.Load(token);
        }
        finally
        {
            _isLoading = false;
        }

        Refresh();
    }

    /// <summary>
    /// What the document currently holds, <see langword="null"/> when it holds nothing : an empty
    /// value stays empty rather than becoming an empty object.
    /// </summary>
    public string? ToJson(Formatting formatting = Formatting.Indented)
    {
        if (Root == null)
            return string.IsNullOrWhiteSpace(RawJson) ? null : RawJson;

        JToken? token = Root.ToToken();
        if (token == null)
            return null;

        // An object with nothing written in it is nothing, not "{}" : the reader filled none of it.
        if (token is JObject { Count: 0 })
            return null;

        return token.ToString(formatting);
    }

    /// <summary>Whether [text] reads as a reference rather than as the text it is.</summary>
    public bool IsReference(string? text)
        => ReferencePrefix.Length > 0 && text?.StartsWith(ReferencePrefix, StringComparison.Ordinal) == true;

    /// <summary>
    /// Check what the document holds against its schema and hand each node what is held against
    /// it, returning the errors as a whole.
    /// <para>
    /// A node holding a reference is not checked, and neither is anything under it : a reference
    /// stands for a value only whoever resolves it knows, so the shape says nothing about it yet.
    /// </para>
    /// </summary>
    public IReadOnlyList<DataValidationError> Validate()
    {
        foreach (DataNode node in Nodes)
            node.Errors = [];

        List<DataValidationError> found = [];

        foreach (DataNode node in Nodes)
        {
            if (node is DataRawNode raw && raw.ParseError() is string message)
                found.Add(new DataValidationError(node.Path, message));
        }

        if (Schema != null && Root != null && !HoldsReference())
        {
            JToken? token = Root.ToToken();
            if (token != null)
            {
                foreach (ValidationError error in Schema.Validate(token))
                    found.Add(new DataValidationError(PathOf(error), $"{error.Path} : {error.Kind}"));
            }
        }

        foreach (IGrouping<string, DataValidationError> group in found.GroupBy(x => x.Path))
        {
            DataNode? node = Nodes.FirstOrDefault(x => x.Path == group.Key);
            if (node != null)
                node.Errors = [.. group.Select(x => x.Message)];
        }

        return found;
    }

    /// <summary>Whether anything in the tree is a reference, which suspends the checking.</summary>
    public bool HoldsReference() => Nodes.Any(x => x.IsReference && !string.IsNullOrWhiteSpace(x.Reference));

    /// <summary>
    /// A validation error read as the path this document names its nodes by. NJsonSchema writes
    /// <c>#/orders[0].label</c>; the tree knows <c>orders[0].label</c>.
    /// </summary>
    private static string PathOf(ValidationError error)
        => (error.Path ?? string.Empty).TrimStart('#', '/').Replace("/", ".");

    /// <summary>Take [node] and everything under it into the document, and follow their changes.</summary>
    internal void Adopt(DataNode node)
    {
        foreach (DataNode inner in node.Walk())
        {
            inner.Document = this;
            inner.PropertyChanged -= OnNodeChanged;
            inner.PropertyChanged += OnNodeChanged;
        }
    }

    internal void Refresh()
    {
        if (_isLoading)
            return;

        Changed?.Invoke();
    }

    private void OnNodeChanged(object? sender, PropertyChangedEventArgs e)
    {
        // The errors are written by the checking itself, HasErrors and ErrorsText with them :
        // following any of them would have the checking ask for another one, endlessly.
        if (e.PropertyName is nameof(DataNode.Errors)
            or nameof(DataNode.HasErrors)
            or nameof(DataNode.ErrorsText)
            or nameof(DataNode.IsExpanded))
            return;

        Refresh();
    }
}

/// <summary>One thing wrong with a value, and where in it.</summary>
public record DataValidationError(string Path, string Message)
{
    public override string ToString() => Path.Length > 0 ? $"{Path} : {Message}" : Message;
}
