using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Data;
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
    /// <summary>
    /// The shape the value is filled in against. Derived from a .NET type, which is how a real
    /// schema usually turns up.
    /// </summary>
    public JsonSchema OrderSchema { get; } = JsonSchema.FromType<Order>();

    /// <summary>The value being filled in, written back by the editor on every edit.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValuePreview))]
    private string? _valueJson;

    /// <summary>The schema being written, written back by the editor on every edit.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SchemaPreview))]
    private string? _schemaJson =
        """
        {
          "type": "object",
          "properties": {
            "Name": { "type": "string", "description": "How the thing is called" },
            "Count": { "type": "integer" }
          },
          "required": [ "Name" ]
        }
        """;

    /// <summary>
    /// What a caller may read a value from instead of typing one. The editor writes the token as a
    /// string where the value would go, and resolves nothing itself.
    /// </summary>
    public IReadOnlyList<DataReference> References { get; } =
    [
        new DataReference("previous", "$previous", "{ }", EnumDataKind.Object)
        {
            Children =
            {
                new DataReference("Customer", "$previous.Customer", "\"Ada\"", EnumDataKind.String),
                new DataReference("Total", "$previous.Total", "42", EnumDataKind.Integer),
            }
        },
        new DataReference("shared", "$shared", "{ }", EnumDataKind.Object)
        {
            Children = { new DataReference("Tenant", "$shared.Tenant", "\"acme\"", EnumDataKind.String) }
        },
    ];

    public string ValuePreview => ValueJson ?? "(nothing filled in)";

    public string SchemaPreview => SchemaJson ?? "(describes nothing)";

    public string DataCode =>
        "<data:DataEditor\n" +
        "    Schema=\"{Binding OrderSchema}\"\n" +
        "    Json=\"{Binding ValueJson, Mode=TwoWay}\"\n" +
        "    References=\"{Binding References}\" />";

    public string SchemaCode =>
        "<data:SchemaEditor Json=\"{Binding SchemaJson, Mode=TwoWay}\" />\n" +
        "\n" +
        "// and the same schema, read only\n" +
        "<data:SchemaView SchemaJson=\"{Binding SchemaJson}\" />";
}
