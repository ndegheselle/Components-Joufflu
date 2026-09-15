using System.Collections.ObjectModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NJsonSchema;
namespace Joufflu.Data.Model;

public partial class DataNode : ObservableObject
{
    public JsonSchema Schema { get; set; }

    [ObservableProperty]
    private string? _key;

    public JsonObjectType Type => Schema.Type;

    public DataNode(string? key, JsonSchema schema)
    {
        Key = key;
        Schema = schema;
    }
}


public partial class DataArrayControl
{
    public DataArray Array { get; }
    public DataArrayControl(DataArray array)
    {
        Array = array;
    }

    [RelayCommand]
    public void Add() => Array.Add();
}

public partial class DataArray : DataNode
{
    public JsonSchema Template { get; set; }
    public ObservableCollection<DataNode> Values { get; set; } = [];

    [ObservableProperty]
    private bool _isExpanded = true;
    public CompositeCollection Items { get; }

    public DataArray(string? name, JsonSchema schema) : base(name, schema)
    {
        Template = schema.Item ?? throw new Exception("Schemas with multiple templates are not supported. Only [Item] is supported not [Items].");
        Items = [new CollectionContainer { Collection = Values }, new DataArrayControl(this)];
    }

    /// <summary>
    /// Create a new node from the [Template] and add it to the [Values].
    /// </summary>
    [RelayCommand]
    public void Add()
    {
        var node = Template.ToDataNode($"[{Values.Count}]");
        Values.Add(node);
    }

    [RelayCommand]
    public void Remove(DataNode node)
    {
        Values.Remove(node);
        for (int i = 0; i < Values.Count; i++)
        {
            DataNode n = Values[i];
            n.Key = $"[{i}]";
        }
    }
}

public partial class DataObject : DataNode
{
    public List<DataNode> Properties { get; set; } = [];

    /// <summary>Whether the properties are shown. Open, so the shape is seen right away.</summary>
    [ObservableProperty]
    private bool _isExpanded = true;
    public DataObject(string? name, JsonSchema schema) : base(name, schema)
    {
    }
}

public class DataValue : DataNode
{
    public object? Value { get; set; }
    public DataValue(string? name, JsonSchema schema) : base(name, schema)
    {
    }
}

public static class DataFactory
{
    extension(JsonSchema schema)
    {
        public DataNode ToDataNode(string? name = null)
        {
            schema = schema.ActualSchema;

            if (schema.IsObject)
            {
                var node = new DataObject(name, schema)
                {
                    Properties = schema.ActualProperties.Select(prop => prop.Value.ToDataNode(prop.Key)).ToList()
                };

                return node;
            }
            else if (schema.IsArray)
            {
                return new DataArray(name, schema);
            }

            return new DataValue(name, schema);
        }
    }
}