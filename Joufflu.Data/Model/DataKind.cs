using NJsonSchema;

namespace Joufflu.Data;

/// <summary>
/// What a schema describes, as the editors need it : one kind per widget rather than the flags
/// and formats a schema says it with.
/// </summary>
public enum EnumDataKind
{
    /// <summary>A schema saying nothing of a type : anything goes, so it is edited as raw JSON.</summary>
    Any,
    Object,
    Array,
    /// <summary>A value the schema restricts to a list, whatever the type of that list.</summary>
    Enumeration,
    String,
    Integer,
    Number,
    Boolean,
    DateTime,
    Date,
    Time,
    Duration
}

/// <summary>
/// Reading a <see cref="JsonSchema"/> as an <see cref="EnumDataKind"/>, and back.
/// </summary>
public static class DataKind
{
    /// <summary>
    /// What [schema] describes. An enumeration wins over its type, and a string is read by its
    /// format : "date-time" is not edited the way free text is.
    /// <para>
    /// <see cref="JsonObjectType"/> is a flags enum, so a value allowed to be a string or a null
    /// is both : the first flag that names a widget wins, object and array first since they are
    /// the ones holding something.
    /// </para>
    /// </summary>
    public static EnumDataKind Of(JsonSchema? schema)
    {
        if (schema == null)
            return EnumDataKind.Any;

        JsonSchema actual = schema.ActualSchema;
        if (actual.IsEnumeration)
            return EnumDataKind.Enumeration;

        JsonObjectType type = actual.Type;
        if (type.HasFlag(JsonObjectType.Object))
            return EnumDataKind.Object;
        if (type.HasFlag(JsonObjectType.Array))
            return EnumDataKind.Array;
        if (type.HasFlag(JsonObjectType.Boolean))
            return EnumDataKind.Boolean;
        if (type.HasFlag(JsonObjectType.Integer))
            return EnumDataKind.Integer;
        if (type.HasFlag(JsonObjectType.Number))
            return EnumDataKind.Number;
        if (type.HasFlag(JsonObjectType.String))
        {
            return actual.Format switch
            {
                "date-time" => EnumDataKind.DateTime,
                "date" => EnumDataKind.Date,
                "time" => EnumDataKind.Time,
                "duration" or "time-span" => EnumDataKind.Duration,
                _ => EnumDataKind.String
            };
        }

        // A schema declaring no type at all, or only "null" : nothing to build a widget from.
        return EnumDataKind.Any;
    }

    /// <summary>
    /// [kind] as a schema declares it : the type it is, and the format telling it from the other
    /// values of that type. Null for <see cref="EnumDataKind.Any"/>, which declares nothing.
    /// </summary>
    public static (JsonObjectType Type, string? Format) Declare(EnumDataKind kind) => kind switch
    {
        EnumDataKind.Object => (JsonObjectType.Object, null),
        EnumDataKind.Array => (JsonObjectType.Array, null),
        EnumDataKind.Enumeration => (JsonObjectType.String, null),
        EnumDataKind.String => (JsonObjectType.String, null),
        EnumDataKind.Integer => (JsonObjectType.Integer, null),
        EnumDataKind.Number => (JsonObjectType.Number, null),
        EnumDataKind.Boolean => (JsonObjectType.Boolean, null),
        EnumDataKind.DateTime => (JsonObjectType.String, "date-time"),
        EnumDataKind.Date => (JsonObjectType.String, "date"),
        EnumDataKind.Time => (JsonObjectType.String, "time"),
        EnumDataKind.Duration => (JsonObjectType.String, "duration"),
        _ => (JsonObjectType.None, null)
    };

    /// <summary>How [schema] reads : what it holds, and whatever a format or a list adds to it.</summary>
    public static string Label(JsonSchema? schema)
    {
        if (schema == null)
            return "anything";

        JsonSchema actual = schema.ActualSchema;
        List<string> parts = [TypeName(actual.Type)];

        if (!string.IsNullOrEmpty(actual.Format))
            parts.Add($"({actual.Format})");

        if (actual.IsEnumeration)
            parts.Add($"one of {actual.Enumeration.Count}");

        return string.Join(" ", parts);
    }

    private static string TypeName(JsonObjectType type) => type switch
    {
        JsonObjectType.None => "anything",
        JsonObjectType.Object => "object",
        JsonObjectType.Array => "array",
        JsonObjectType.String => "string",
        JsonObjectType.Integer => "integer",
        JsonObjectType.Number => "number",
        JsonObjectType.Boolean => "boolean",
        JsonObjectType.Null => "null",
        // A value allowed to be of several types, which the flags say.
        _ => string.Join(" or ", $"{type}".Split(',').Select(x => x.Trim().ToLowerInvariant())),
    };
}
