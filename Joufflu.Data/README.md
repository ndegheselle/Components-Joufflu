# Joufflu.Data

**JSON Schema editing for [Joufflu](https://www.nuget.org/packages/Joufflu).**

Two trees instead of a raw JSON text box: write a schema, and fill a value in against one.
Follows the Joufflu design system and themes with it.

[![Joufflu.Data on NuGet](https://img.shields.io/nuget/v/Joufflu.Data?label=Joufflu.Data&logo=nuget)](https://www.nuget.org/packages/Joufflu.Data)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](https://github.com/ndegheselle/Joufflu/blob/main/LICENSE)

## What's inside

| Piece | Purpose |
|---|---|
| `DataEditor` | Fills a value in against a schema — one row per value, edited by the widget its kind calls for. Any field can hold a *reference* instead of a literal. |
| `SchemaEditor` | Writes a JSON Schema — name, kind, required, description, enum options, per value. |
| `SchemaView` | Shows a schema read-only, as the tree of values it describes. |
| `DataDocument` / `DataNode` | The value being filled in, and what the schema holds against it. |
| `SchemaDocument` / `SchemaNode` | The schema being written, and the `JsonSchema` it amounts to. |

Schemas are [NJsonSchema](https://github.com/RicoSuter/NJsonSchema) `JsonSchema` objects, so a schema
generated from a .NET type with `JsonSchema.FromType<T>()` round-trips through the editors without a
conversion layer.

## Filling a value in

Bind the shape and the value; the value is written back on every edit.

```xml
<data:DataEditor SchemaJson="{Binding ExpectedSchemaJson}"
                 Json="{Binding SettingsJson, Mode=TwoWay}" />
```

`Json` comes back `null` when nothing was filled in, so a caller storing "nothing" as null keeps
storing null rather than `{}`. Every property the schema declares is written; the row's options
menu holds "Set to null" for a value the reader means to leave empty rather than fill in.

Ask for the errors whenever you need them — each field is handed its own, and the call returns
them as a whole:

```csharp
IReadOnlyList<DataValidationError> errors = Editor.Validate();
```

## Writing a schema

```xml
<data:SchemaEditor Json="{Binding InputSchemaJson, Mode=TwoWay}" />
```

`Json` is the JSON Schema document itself, and comes back `null` for a schema describing nothing.

## References

A field can hold a token to be resolved later instead of a literal — a mapping reading
`$previous.Value` rather than a value typed in. Hand the editor what may be read:

```csharp
Editor.References =
[
    new DataReference("previous", "$previous", "{ }", EnumDataKind.Object)
    {
        Children = { new DataReference("Value", "$previous.Value", "42", EnumDataKind.Integer) }
    }
];
```

Each field then offers the references its kind can hold, and writes the token as a string where the
value would go. The library resolves nothing: what a token means is the caller's business.
`ReferencePrefix` (default `$`) is how a stored value holding one is read back as a reference rather
than as the text it is; leave `References` empty and the affordance never appears.

## Shapes that do not fit

A schema saying nothing of a type, and a schema referencing itself past the first time it is opened,
are edited as the JSON they are — the text is kept rather than lost. A schema that cannot be read at
all leaves the whole value as text, and says why through `Error`.
