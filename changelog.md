# Version 0.7.0

- Add the `Joufflu.Data` package : JSON Schema as two trees rather than a raw text box. `DataEditor` fills a value in against a schema, one row per value edited by the widget its kind calls for, with a property the schema does not require left out until it is ticked — to a schema an absent property and a property holding "" are not the same thing. `SchemaEditor` writes the schema itself, and `SchemaView` shows one read only. Any field can hold a reference to be resolved later instead of a literal, whatever its declared type, picked from what the caller offers. Schemas are NJsonSchema `JsonSchema` objects, so one derived from a .NET type round-trips without a conversion layer, and a shape the editors cannot model — a schema saying nothing of a type, one referencing itself — falls back on the JSON it is rather than being lost. Replaces the never released `Joufflu.Data.Shared` generic element model, which conflated the shape with the values and no longer built
- Add `OverlayViewModel`, a base for the content of a modal overlay : it holds the `OverlayOptions` the overlay is shown with, so `IOverlayService.Show` takes nothing but the content, it comes with a `CancelCommand`, and it closes itself rather than whatever sits on top of the stack. `OverlayViewModel<TResult>` adds what the overlay is awaited for — a `Result` set by validating it, and a static `ShowAsync` handing that result back, the default of the type when the overlay was dismissed. Replaces the `CloseTop(true)` / `CloseTop(false)` pair every awaited overlay was writing by hand
- Add `IOverlayService.Close(object content, bool? result)`, closing the overlay showing a given content rather than the top one : an overlay opening another one of its own kind can now close itself whichever is displayed. **Breaking** for anything implementing `IOverlayService` outside the library, `OverlayService` implementing it already

# Version 0.6.5

- Give the text inputs — `TextBox`, `PasswordBox` and `FormatTextBox` — their own `Dimensions.InputPadding` keys (`Xs`/`Sm`/`Md`/`Lg`), half the control padding's horizontal so they read tighter than a button, covering every size variant where the base text input padding used to be fixed whatever the size. Set on a plain `Padding` setter, so a consumer local value or style overrides it; the theme customizer keeps them in lockstep with the control padding rather than editing them on their own
- Replace `Derive.BorderSides`, `Derive.MarginSides` and `Derive.Corners` by `Derive.BorderThicknessFactor`, `Derive.MarginFactor` and `Derive.CornerRadiusFactor` : a `Thickness` or a `CornerRadius` multiplying the derived value side by side, so `0` still drops a side and `1` keeps it, while any other value scales it — a doubled margin or a halved radius no longer needs its own dimension. Drops the `ThicknessSides` and `Corners` flags enums
- Let the header of a `TreeViewItem` fill the width of the row it is highlighted on, so what is seen as the item is what answers to the mouse — a drag from a `DragSource` in the item template included. Its height follows `VerticalContentAlignment`, still centered by default : a header covering the row entirely sets it to `Stretch` and centers its own content

# Version 0.6.3

- Pass a `DropData` to `DropTarget.Command` rather than the bare `IDataObject` : it is an `IDataObject` of the dragged data, so the commands taking one keep working, and it adds where the pointer is (`Position`) on the element the drop landed on (`Target`), for the targets placing what they receive
- Offer the data wrapped by `DragSource.Data` under the base classes and the interfaces of its type too, so a target can ask for what the data is instead of having to know the exact kind it is given

# Version 0.5.1

- Add a standard confirm overlay, `IOverlayService.Confirm`, with an `EnumConfirmationType` colouring its confirm button
- Add `DropTarget.Command`, turning any element into a drop target
- Add `DragSource.Data`, turning any element into a drag source, with `DragSource.AllowedEffects` and `DragSource.IsDragging`
- Add a `FullContainer` to simplify content placement when using `AllowContentOverTitleBar`
- Support `Paging` without a known total
- Move `Dropdown` from `Joufflu.Navigation` to `Joufflu.Inputs` (namespace `Joufflu.Inputs.Controls`)
- Turn `Dropdown` from a wrapper control into attached properties (`Dropdown.Popup`, `Dropdown.Placement`, `Dropdown.PopupStyle`, offsets) set on a `ToggleButton` you own, so the button accepts any `ToggleButton` style and attached property — `Sizing.IsSquare` included — with nothing to forward. Replaces `Header`, `ButtonStyle` and `PopupPlacement`. Adds `Dropdown.CloseOnClick`, dismissing the popup when a button inside it is clicked. `Dropdown.PopupStyle` now styles the `DropdownPopupHost` drawing the popup chrome, so its padding, background, border and radius are reachable — a `Popup` itself has none of those
- Remove the `Dimensions.BorderThicknessRight` key, now owned by `NavigationMenu` which was its only user
- Add the `Derive.BorderThickness` and `Derive.CornerRadius` attached properties, deriving per-side thicknesses and per-corner radii from a scalar dimension so they follow a runtime theme change
- Derive the partial borders and radii of `NavigationMenu`, `TabControl`, `GroupBox`, `DataGrid` and `ListView` through `Derive`, dropping the `NavigationMenuBorderThickness` key

# Version 0.4.0

- Change the navigation to use types instead of string keys
- Split `OverlayContainer` into separate overlay and `ToastContainer` containers
- Move the tooltip into the `Joufflu` core package

# Version 0.2.0

- Add the `Joufflu.FileExplorer` package : `Explorer`, `ExplorerList`, `ExplorerTree` and `ExplorerControlBar` sharing an `IExplorerSource`, with node visuals and context menus keyed on the node type, drag and drop, keyboard shortcuts, and file operations handed over to the Windows shell
- Add the `xl` control size and its `ControlFontSizeXl` dimension
- Size `FontIcon` from the design system instead of a fixed value
- Improve the toasts look, with a progress bar of their remaining duration
- Restyle the native `ListView` and `TreeView` (rounded border, centered cell content)
- Add `MoreVisualTreeHelper.FindSelfOrParent`, and a logical tree fallback to its parent lookup

# Version 0.1.2

- Move `Badge`, `Spinner`, toasts and the tooltip attached properties out of the core `Joufflu` package into a new `Joufflu.Feedback` package (namespace `Joufflu.Feedback.Controls`)

# Version 0.1.1

- Add tooltip
- Add soft and outline button styles
- Improve theme manager custom themes handling
- Improve `ThemedWindow` handling of `AllowContentOverTitleBar`

# Version 0.1.0

- First version