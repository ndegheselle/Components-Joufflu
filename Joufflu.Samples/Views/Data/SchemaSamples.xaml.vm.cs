using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Data.Model;
using NJsonSchema;

namespace Joufflu.Samples.Views.Data;

/// <summary>What the data sample fills in, so the schema is one a real type would produce.</summary>
public enum EnumDelivery
{
    Standard,
    Express,
    Pickup
}

/// <summary>A line of an order, so the sample has an array of objects to fill in.</summary>
public class OrderLine
{
    public string Reference { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>The shape the data sample is filled in against, derived from this very type.</summary>
public class Order
{
    public string Customer { get; set; } = "";
    public DateTime PlacedOn { get; set; }
    public EnumDelivery Delivery { get; set; }
    public bool IsPaid { get; set; }
    public List<OrderLine> Lines { get; set; } = [];
}

public partial class SchemaSamplesViewModel : ObservableObject
{
    [ObservableProperty]
    private DataObject? _node;

    [RelayCommand]
    public void Test()
    {
        var schema = JsonSchema.FromType<Order>();
        Node = (DataObject)DataFactory.ToDataNode(schema);
    }
}
