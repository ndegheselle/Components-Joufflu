using System.Collections.ObjectModel;

namespace Joufflu.Data;

/// <summary>
/// Something a field can hold in place of a literal : a token the consumer resolves later, offered
/// as a tree so a caller can group what it hands over.
/// <para>
/// The library knows nothing of what a token means — it writes <see cref="Token"/> into the value
/// as a string and leaves the resolving to whoever asked for the editor.
/// </para>
/// </summary>
public class DataReference
{
    /// <summary>How the reference reads in the picker.</summary>
    public string Name { get; }

    /// <summary>
    /// What is written into the value when this reference is picked. Empty on a row that only
    /// groups others, which cannot be picked.
    /// </summary>
    public string Token { get; }

    /// <summary>
    /// What the token stands for, in one line, so the shape is readable without expanding it.
    /// </summary>
    public string Preview { get; }

    /// <summary>The kind the token resolves to, so a field only offers what it can hold.</summary>
    public EnumDataKind Kind { get; }

    public ObservableCollection<DataReference> Children { get; } = [];

    /// <summary>Whether this row only groups others rather than standing for a value.</summary>
    public bool IsGroup => Token.Length == 0;

    public DataReference(string name, string token, string preview = "", EnumDataKind kind = EnumDataKind.Any)
    {
        Name = name;
        Token = token;
        Preview = preview;
        Kind = kind;
    }

    /// <summary>A row standing for a group of references rather than for one.</summary>
    public static DataReference Group(string name) => new(name, string.Empty);

    /// <summary>
    /// The references of this subtree that a field of [kind] can hold, groups walked through. A
    /// field taking anything takes every reference, and so does a reference resolving to anything :
    /// neither of them knows enough to rule the other out.
    /// </summary>
    public IEnumerable<DataReference> For(EnumDataKind kind)
    {
        if (!IsGroup && (kind == EnumDataKind.Any || Kind == EnumDataKind.Any || Kind == kind))
            yield return this;

        foreach (DataReference child in Children)
        {
            foreach (DataReference found in child.For(kind))
                yield return found;
        }
    }

    public override string ToString() => Token.Length > 0 ? Token : Name;
}
