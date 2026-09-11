using System.Windows;
﻿using System.Windows.Input;
using System.Windows.Markup;
using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.Sources;

namespace Joufflu.FileExplorer.Controls.Base
{
    public enum MenuScope { Single, Multiple, None }

    /// <summary>
    /// Resource key of the context menu template of a data type.
    /// Inherits <see cref="ComponentResourceKey"/> because the templates live in the theme dictionary of the library
    /// (Themes/Generic.xaml) : a resource lookup only reaches a theme dictionary for a Type or a ComponentResourceKey
    /// key, any other key would only be found by a consumer merging the dictionaries into its own resources.
    /// </summary>
    public sealed class ContextMenuTemplateKey : ComponentResourceKey, IEquatable<ContextMenuTemplateKey>
    {
        /// <summary>Type name declared in XAML, resolved in <see cref="ProvideValue"/>.</summary>
        private readonly string? _typeName;

        public ContextMenuTemplateKey()
        {
            // Points the lookup at the theme dictionary of this assembly.
            TypeInTargetAssembly = typeof(ContextMenuTemplateKey);
        }

        public ContextMenuTemplateKey(Type dataType) : this() { DataType = dataType; }

        public ContextMenuTemplateKey(string typeName) : this() { _typeName = typeName; }

        public Type? DataType { get; private set; }

        public MenuScope Scope { get; set; } = MenuScope.Single;

        public override object ProvideValue(IServiceProvider sp)
        {
            // Resolves the type name declared in XAML
            if (DataType == null && _typeName != null)
                DataType = (sp.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver)?.Resolve(_typeName);

            return this;
        }

        public bool Equals(ContextMenuTemplateKey? o) => o is not null && o.DataType == DataType && o.Scope == Scope;
        public override bool Equals(object? o) => Equals(o as ContextMenuTemplateKey);
        public override int GetHashCode() => HashCode.Combine(DataType, Scope);
    }

    /// <summary>
    /// A control displaying explorer nodes, for what belongs to it rather than to its <see cref="IExplorerSource"/> :
    /// the edition of a name happens in the control the user started it in, so that another control displaying the
    /// same node doesn't open a box of its own.
    /// </summary>
    public interface IExplorerUi
    {
        /// <summary>
        /// Node whose name is being edited, null while none is. The control replaces the name of that node with an
        /// editable one, so that a rename is typed where the node is displayed.
        /// </summary>
        IExplorerNode? RenamedNode { get; }

        /// <summary>
        /// Starts the edition of the name of the node given as a parameter, null giving up the one in progress. Ended
        /// by the control itself, which hands a validated name over to <see cref="IExplorerSource.RenameCommand"/>.
        /// </summary>
        ICommand RenamingCommand { get; }
    }

    /// <summary>
    /// Data context of the context menus of an <see cref="ExplorerList"/>, gives access to the commands of the
    /// loader and to the nodes the menu was opened on.
    /// </summary>
    public class ExplorerMenuContext
    {
        public IExplorerSource? Source { get; }

        /// <summary>
        /// Control the menu was opened in, null for one that has no rename UI of its own.
        /// </summary>
        public IExplorerUi? Ui { get; }

        /// <summary>
        /// Every selected node, the menu was opened on the first one.
        /// </summary>
        public IReadOnlyList<IExplorerNode> Nodes { get; }

        /// <summary>
        /// Node the menu was opened on, null when multiple nodes are selected.
        /// </summary>
        public IExplorerNode? Node => Nodes.Count == 1 ? Nodes[0] : null;

        public ExplorerMenuContext(IExplorerSource? source, IExplorerUi? ui, IReadOnlyList<IExplorerNode> nodes)
        {
            Ui = ui;
            Source = source;
            Nodes = nodes;
        }
    }
}
