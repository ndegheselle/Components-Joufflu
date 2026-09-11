using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Themes;
using Microsoft.Win32;
using JBrushes = Joufflu.Brushes;
using JColors = Joufflu.Colors;
using JDimensions = Joufflu.Dimensions;

namespace Joufflu.Samples.Views.Themes;

/// <summary>A single editable theme colour, wired to a live resource key.</summary>
public class ThemeColorEntry : ObservableObject
{
    private readonly Action<ThemeColorEntry> _onChanged;

    public string Label { get; }

    /// <summary>The <c>Joufflu.Colors</c> accessor name, e.g. <c>PrimaryColor</c>.</summary>
    public string ResourceName { get; }

    public ComponentResourceKey Key { get; }

    private Color _color;
    public Color Color
    {
        get => _color;
        set
        {
            if (SetProperty(ref _color, value))
                _onChanged(this);
        }
    }

    public ThemeColorEntry(string label, string resourceName, ComponentResourceKey key, Color color, Action<ThemeColorEntry> onChanged)
    {
        Label = label;
        ResourceName = resourceName;
        Key = key;
        _color = color;
        _onChanged = onChanged;
    }

    /// <summary>Sets the colour without re-applying it to resources (used when seeding/resetting).</summary>
    public void SetColorSilently(Color color) => SetProperty(ref _color, color, nameof(Color));
}

/// <summary>A single editable numeric dimension (radius, spacing, height, font size…).</summary>
public class ThemeDimensionEntry : ObservableObject
{
    private readonly Action<ThemeDimensionEntry> _onChanged;

    public string Label { get; }

    /// <summary>Identifier used to route the value to the right resource key(s).</summary>
    public string ResourceName { get; }

    public double Minimum { get; }

    public double Maximum { get; }

    public string Unit { get; }

    private double _value;
    public double Value
    {
        get => _value;
        set
        {
            // Snap to whole numbers — every dimension here is an integer count of DIPs.
            double snapped = Math.Round(value);
            if (SetProperty(ref _value, snapped))
                _onChanged(this);
        }
    }

    public ThemeDimensionEntry(string label, string resourceName, double min, double max, double value, Action<ThemeDimensionEntry> onChanged, string unit = "px")
    {
        Label = label;
        ResourceName = resourceName;
        Minimum = min;
        Maximum = max;
        _value = value;
        _onChanged = onChanged;
        Unit = unit;
    }

    public void SetValueSilently(double value) => SetProperty(ref _value, Math.Round(value), nameof(Value));
}

/// <summary>
/// One size step of a <see cref="ThemeScaleEntry"/>: the resource it feeds and the ratio its value
/// is derived at from the scale's base value.
/// </summary>
public class ThemeScaleStep : ObservableObject
{
    /// <summary>Size suffix shown to the user, e.g. <c>xs</c>.</summary>
    public string Label { get; }

    /// <summary>Identifier used to route the derived value to its resource key.</summary>
    public string ResourceName { get; }

    /// <summary>Multiplier applied to the scale's base value, <c>1</c> for the step the base is read from.</summary>
    public double Ratio { get; }

    private double _value;
    /// <summary>The derived value (horizontal amount for a padding scale), in DIPs.</summary>
    public double Value
    {
        get => _value;
        private set => SetProperty(ref _value, value);
    }

    private double _vertical;
    /// <summary>The derived vertical amount; padding scales only.</summary>
    public double Vertical
    {
        get => _vertical;
        private set => SetProperty(ref _vertical, value);
    }

    private string _display = "";
    /// <summary>The exact derived value, formatted for the preview next to the slider.</summary>
    public string Display
    {
        get => _display;
        private set => SetProperty(ref _display, value);
    }

    public ThemeScaleStep(string label, string resourceName, double ratio)
    {
        Label = label;
        ResourceName = resourceName;
        Ratio = ratio;
    }

    /// <summary>Derives this step from <paramref name="value"/> (and <paramref name="vertical"/> when paired).</summary>
    internal void Derive(double value, double? vertical, string unit)
    {
        Value = Math.Round(value * Ratio);
        Vertical = vertical is null ? 0 : Math.Round(vertical.Value * Ratio);
        Display = vertical is null ? $"{Num(Value)} {unit}" : $"{Num(Value)},{Num(Vertical)} {unit}";
    }

    private static string Num(double value)
        => value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
}

/// <summary>
/// A family of dimensions (control height, font size, control padding…) driven by a single base
/// value: every size step is derived from it by a fixed ratio, so one slider — two for the paddings,
/// one per axis — replaces one slider per size.
/// </summary>
public class ThemeScaleEntry : ObservableObject
{
    private readonly Action<ThemeScaleEntry> _onChanged;

    public string Label { get; }

    public double Minimum { get; }

    public double Maximum { get; }

    public string Unit { get; }

    /// <summary>Steps derived from the base value, smallest first.</summary>
    public ObservableCollection<ThemeScaleStep> Steps { get; }

    /// <summary>True when the scale drives two axes (a padding), so a second slider is shown.</summary>
    public bool HasVertical => _verticalBase is not null;

    /// <summary>Axis marker shown next to the base slider, empty for a single-axis scale.</summary>
    public string AxisLabel => HasVertical ? "H" : "";

    /// <summary>Label of the base slider — the size step the base value is the exact value of.</summary>
    public string BaseLabel { get; }

    private double _base;
    /// <summary>The value every step is derived from (the <c>md</c> step's own value).</summary>
    public double Base
    {
        get => _base;
        set
        {
            if (SetProperty(ref _base, Math.Round(value)))
                Refresh();
        }
    }

    private double? _verticalBase;
    /// <summary>The vertical base value; padding scales only.</summary>
    public double VerticalBase
    {
        get => _verticalBase ?? 0;
        set
        {
            if (_verticalBase is null || Math.Round(value) == _verticalBase)
                return;
            _verticalBase = Math.Round(value);
            OnPropertyChanged();
            Refresh();
        }
    }

    public ThemeScaleEntry(
        string label,
        string baseLabel,
        double min,
        double max,
        double @base,
        double? verticalBase,
        Action<ThemeScaleEntry> onChanged,
        string unit,
        params ThemeScaleStep[] steps)
    {
        Label = label;
        BaseLabel = baseLabel;
        Minimum = min;
        Maximum = max;
        _base = Math.Round(@base);
        _verticalBase = verticalBase is null ? null : Math.Round(verticalBase.Value);
        _onChanged = onChanged;
        Unit = unit;
        Steps = new ObservableCollection<ThemeScaleStep>(steps);
        DeriveSteps();
    }

    /// <summary>Re-seeds the base values without pushing them back into the live resources.</summary>
    public void SetBaseSilently(double @base, double? verticalBase = null)
    {
        SetProperty(ref _base, Math.Round(@base), nameof(Base));
        if (_verticalBase is not null && verticalBase is not null)
        {
            _verticalBase = Math.Round(verticalBase.Value);
            OnPropertyChanged(nameof(VerticalBase));
        }
        DeriveSteps();
    }

    private void Refresh()
    {
        DeriveSteps();
        _onChanged(this);
    }

    private void DeriveSteps()
    {
        foreach (var step in Steps)
            step.Derive(_base, _verticalBase, Unit);
    }
}

public class ThemeColorGroup
{
    public string Header { get; }
    public ObservableCollection<ThemeColorEntry> Colors { get; }
    public ThemeColorGroup(string header, ObservableCollection<ThemeColorEntry> colors)
    {
        Header = header;
        Colors = colors;
    }
}

public class ThemeDimensionGroup
{
    public string Header { get; }
    public ObservableCollection<ThemeDimensionEntry> Dimensions { get; }
    public ThemeDimensionGroup(string header, ObservableCollection<ThemeDimensionEntry> dimensions)
    {
        Header = header;
        Dimensions = dimensions;
    }
}

/// <summary>
/// Drives the "Customize theme" page: edits live colour/dimension resources application-wide
/// (so the whole gallery becomes the preview) and emits a drop-in ResourceDictionary.
/// </summary>
public class ThemeCustomizerViewModel : ObservableObject
{
    // When true, entry changes are seeding/resetting and must not push back into resources.
    private bool _suppress;

    private readonly List<ThemeColorEntry> _allColors = new();
    private readonly List<ThemeDimensionEntry> _allDimensions = new();
    private readonly List<ThemeScaleEntry> _allScales = new();

    public ObservableCollection<ThemeColorGroup> ColorGroups { get; } = new();
    public ObservableCollection<ThemeDimensionGroup> DimensionGroups { get; } = new();
    /// <summary>Size families driven by a single base value (control height, font size, control padding).</summary>
    public ObservableCollection<ThemeScaleEntry> Scales { get; } = new();

    /// <summary>Selectable preset palettes, sourced from the themes registered with <see cref="ThemeManager"/>.</summary>
    public IReadOnlyList<ThemePreset> Presets { get; }

    private ThemePreset? _selectedPreset;
    /// <summary>The currently selected preset; assigning a non-null value applies its palette.</summary>
    public ThemePreset? SelectedPreset
    {
        get => _selectedPreset;
        set
        {
            if (SetProperty(ref _selectedPreset, value) && value is not null && !_suppress)
                ApplyPreset(value);
        }
    }

    private string _generatedXaml = "";
    public string GeneratedXaml
    {
        get => _generatedXaml;
        private set => SetProperty(ref _generatedXaml, value);
    }

    public ICommand ResetCommand { get; }
    public ICommand CopyCommand { get; }
    public ICommand SaveCommand { get; }

    public ThemeCustomizerViewModel()
    {
        BuildColorGroups();
        BuildDimensionGroups();
        BuildScales();

        // Built after the colour groups so _allColors tells us which keys to read from each theme.
        Presets = BuildPresets();

        ResetCommand = new RelayCommand(Reset);
        CopyCommand = new RelayCommand(Copy);
        SaveCommand = new RelayCommand(Save);

        RegenerateXaml();
    }

    #region Definitions

    private void BuildColorGroups()
    {
        ThemeColorGroup Group(string header, params (string label, string name, ComponentResourceKey key)[] items)
        {
            var colors = new ObservableCollection<ThemeColorEntry>();
            foreach (var (label, name, key) in items)
            {
                var entry = new ThemeColorEntry(label, name, key, ReadColor(key), OnColorChanged);
                colors.Add(entry);
                _allColors.Add(entry);
            }
            return new ThemeColorGroup(header, colors);
        }

        ColorGroups.Add(Group("Surface",
            ("Background", "BackgroundColor", JColors.BackgroundColor),
            ("Background 100 (elevated)", "Background100Color", JColors.Background100Color),
            ("Background 200 (selected)", "Background200Color", JColors.Background200Color),
            ("Border", "BorderColor", JColors.BorderColor),
            ("Border 100", "Border100Color", JColors.Border100Color)));

        ColorGroups.Add(Group("Text",
            ("Foreground", "ForegroundColor", JColors.ForegroundColor),
            ("Foreground 100", "Foreground100Color", JColors.Foreground100Color),
            ("Foreground 200", "Foreground200Color", JColors.Foreground200Color)));

        ColorGroups.Add(Group("Primary",
            ("Primary", "PrimaryColor", JColors.PrimaryColor),
            ("Primary 100 (hover)", "Primary100Color", JColors.Primary100Color),
            ("Primary content", "PrimaryContentColor", JColors.PrimaryContentColor)));

        ColorGroups.Add(Group("Secondary",
            ("Secondary", "SecondaryColor", JColors.SecondaryColor),
            ("Secondary 100 (hover)", "Secondary100Color", JColors.Secondary100Color),
            ("Secondary content", "SecondaryContentColor", JColors.SecondaryContentColor)));

        ColorGroups.Add(Group("Success",
            ("Success", "SuccessColor", JColors.SuccessColor),
            ("Success 100 (hover)", "Success100Color", JColors.Success100Color),
            ("Success content", "SuccessContentColor", JColors.SuccessContentColor)));

        ColorGroups.Add(Group("Info",
            ("Info", "InfoColor", JColors.InfoColor),
            ("Info 100 (hover)", "Info100Color", JColors.Info100Color),
            ("Info content", "InfoContentColor", JColors.InfoContentColor)));

        ColorGroups.Add(Group("Warning",
            ("Warning", "WarningColor", JColors.WarningColor),
            ("Warning 100 (hover)", "Warning100Color", JColors.Warning100Color),
            ("Warning content", "WarningContentColor", JColors.WarningContentColor)));

        ColorGroups.Add(Group("Danger",
            ("Danger", "DangerColor", JColors.DangerColor),
            ("Danger 100 (hover)", "Danger100Color", JColors.Danger100Color),
            ("Danger content", "DangerContentColor", JColors.DangerContentColor)));
    }

    private void BuildDimensionGroups()
    {
        ThemeDimensionGroup Group(string header, params ThemeDimensionEntry[] items)
        {
            var dims = new ObservableCollection<ThemeDimensionEntry>();
            foreach (var entry in items)
            {
                dims.Add(entry);
                _allDimensions.Add(entry);
            }
            return new ThemeDimensionGroup(header, dims);
        }

        ThemeDimensionEntry Dim(string label, string name, double min, double max) =>
            new(label, name, min, max, ReadDouble(DimensionKey(name)), OnDimensionChanged);

        DimensionGroups.Add(Group("Shape",
            Dim("Corner radius", "Radius", 0, 24),
            Dim("Border thickness", "Thickness", 0, 4),
            Dim("Spacing", "Spacing", 0, 32)));
    }

    /// <summary>
    /// Declares the size families. Each is driven by its <c>md</c> value; the other steps keep a fixed
    /// ratio to it, chosen so the shipped defaults come out exactly at the shipped base.
    /// Height and padding stop at <c>lg</c> — the toolkit defines no <c>xl</c> key for them.
    /// </summary>
    private void BuildScales()
    {
        void Scale(string label, string baseLabel, double min, double max, double @base, double? verticalBase, string unit, params ThemeScaleStep[] steps)
        {
            var entry = new ThemeScaleEntry(label, baseLabel, min, max, @base, verticalBase, OnScaleChanged, unit, steps);
            Scales.Add(entry);
            _allScales.Add(entry);
        }

        Scale("Control height", "md", 16, 72, ReadDouble(JDimensions.HeightMd), null, "px",
            new ThemeScaleStep("xs", "HeightXs", 0.75),
            new ThemeScaleStep("sm", "HeightSm", 0.875),
            new ThemeScaleStep("md", "HeightMd", 1),
            new ThemeScaleStep("lg", "HeightLg", 1.25));

        Scale("Font size", "md", 8, 32, ReadDouble(JDimensions.FontSizeMd), null, "px",
            new ThemeScaleStep("xs", "FontSizeXs", 0.85),
            new ThemeScaleStep("sm", "FontSizeSm", 0.92),
            new ThemeScaleStep("md", "FontSizeMd", 1),
            new ThemeScaleStep("lg", "FontSizeLg", 1.23),
            new ThemeScaleStep("xl", "FontSizeXl", 1.85));

        // ControlPadding thicknesses are symmetric (Left==Right, Top==Bottom), so one base per axis.
        // The text input paddings are derived from these at half the horizontal, not edited on their own.
        Thickness padding = ReadThickness(JDimensions.PaddingMd);
        Scale("Control padding", "md", 0, 40, padding.Left, padding.Top, "px",
            new ThemeScaleStep("xs", "PaddingXs", 0.5),
            new ThemeScaleStep("sm", "PaddingSm", 0.75),
            new ThemeScaleStep("md", "PaddingMd", 1),
            new ThemeScaleStep("lg", "PaddingLg", 1.5));
    }

    /// <summary>
    /// Builds a preset per concrete theme registered with <see cref="ThemeManager"/> (<c>System</c> is
    /// skipped — it is a resolver, not a palette). Each preset's colours are read straight from the
    /// theme's dictionary so the list never drifts from what the app can actually apply.
    /// </summary>
    private IReadOnlyList<ThemePreset> BuildPresets()
    {
        var presets = new List<ThemePreset>();
        foreach (string name in ThemeManager.Instance.Themes)
        {
            ResourceDictionary? dictionary = ThemeManager.Instance.GetDictionary(name);
            if (dictionary is null)
                continue;

            var colors = new Dictionary<string, Color>();
            foreach (var entry in _allColors)
            {
                if (dictionary[entry.Key] is Color color)
                    colors[entry.ResourceName] = color;
            }

            // Only offer themes that define the whole editable palette — ThemePreset needs every key.
            if (colors.Count == _allColors.Count)
                presets.Add(new ThemePreset(name, colors));
        }
        return presets;
    }

    #endregion

    #region Live application

    /// <summary>Applies a preset palette: seeds the editors and pushes every colour to the live resources.</summary>
    private void ApplyPreset(ThemePreset preset)
    {
        _suppress = true;
        try
        {
            var res = Application.Current.Resources;
            foreach (var entry in _allColors)
            {
                if (!preset.Colors.TryGetValue(entry.ResourceName, out Color color))
                    continue;
                entry.SetColorSilently(color);
                res[entry.Key] = color;
                res[BrushKey(entry.ResourceName)] = new SolidColorBrush(color);
            }
        }
        finally
        {
            _suppress = false;
        }

        RegenerateXaml();
    }

    private void OnColorChanged(ThemeColorEntry entry)
    {
        if (_suppress)
            return;
        // A manual edit no longer matches any preset — drop the highlight without re-applying.
        SetProperty(ref _selectedPreset, null, nameof(SelectedPreset));
        var res = Application.Current.Resources;
        res[entry.Key] = entry.Color;
        // The semantic brushes bind their Color via DynamicResource, but each brush lives in the
        // same merged theme dictionary as its colour and resolves it there before reaching this
        // app-level override. Override the derived brush explicitly so the preview moves
        // (same reason ApplyDimension overrides the derived Thickness/CornerRadius keys).
        res[BrushKey(entry.ResourceName)] = new SolidColorBrush(entry.Color);
        RegenerateXaml();
    }

    private void OnDimensionChanged(ThemeDimensionEntry entry)
    {
        if (_suppress)
            return;
        ApplyDimension(entry);
        RegenerateXaml();
    }

    private void OnScaleChanged(ThemeScaleEntry entry)
    {
        if (_suppress)
            return;
        ApplyScale(entry);
        RegenerateXaml();
    }

    /// <summary>Horizontal ratio the text input paddings keep to the control paddings they follow.</summary>
    private const double InputPaddingHorizontalFactor = 0.5;

    /// <summary>Ratio of <see cref="Dimensions.HeightEmbedded"/> to the control height scale's base (md) value.</summary>
    private const double CloseHeightFactor = 0.625;

    /// <summary>Pushes every derived step of a scale to its live resource.</summary>
    private static void ApplyScale(ThemeScaleEntry entry)
    {
        var res = Application.Current.Resources;
        foreach (var step in entry.Steps)
        {
            if (entry.HasVertical)
                res[PaddingKey(step.ResourceName)] = new Thickness(step.Value, step.Vertical, step.Value, step.Vertical);
            else
                res[DimensionKey(step.ResourceName)] = step.Value;
        }

        // The input paddings are not edited on their own: they follow the control paddings at half the horizontal.
        if (IsControlPadding(entry))
            foreach (var step in entry.Steps)
                res[InputPaddingKey(step.ResourceName)] = InputPaddingFrom(step);

        // The close-button height is not edited on its own: it follows the control height's base value.
        if (IsControlHeight(entry))
            res[JDimensions.HeightEmbedded] = Math.Round(entry.Base * CloseHeightFactor);
    }

    private static bool IsControlPadding(ThemeScaleEntry entry)
        => entry.Steps.Count > 0 && entry.Steps[0].ResourceName.StartsWith("Padding", StringComparison.Ordinal);

    private static bool IsControlHeight(ThemeScaleEntry entry)
        => entry.Steps.Count > 0 && entry.Steps[0].ResourceName.StartsWith("Height", StringComparison.Ordinal);

    /// <summary>The input padding thickness derived from a control padding step.</summary>
    private static Thickness InputPaddingFrom(ThemeScaleStep controlPaddingStep)
    {
        double h = controlPaddingStep.Value * InputPaddingHorizontalFactor;
        return new Thickness(h, controlPaddingStep.Vertical, h, controlPaddingStep.Vertical);
    }

    /// <summary>The input padding key a control padding step feeds (e.g. <c>PaddingSm</c> → <c>InputPaddingSm</c>).</summary>
    private static ComponentResourceKey InputPaddingKey(string controlPaddingName)
        => PaddingKey("Input" + controlPaddingName);

    private void ApplyDimension(ThemeDimensionEntry entry)
    {
        var res = Application.Current.Resources;
        double v = entry.Value;
        switch (entry.ResourceName)
        {
            // These base doubles feed a derived Thickness/CornerRadius built with StaticResource,
            // so the derived key must be overridden explicitly for the live preview to move.
            case "Radius":
                res[JDimensions.Radius] = v;
                res[JDimensions.CornerRadius] = new CornerRadius(v);
                break;
            case "Thickness":
                res[JDimensions.Thickness] = v;
                res[JDimensions.BorderThickness] = new Thickness(v);
                break;
            case "Spacing":
                res[JDimensions.Spacing] = v;
                res[JDimensions.SpacingThickness] = new Thickness(v);
                break;
            default:
                res[DimensionKey(entry.ResourceName)] = v;
                break;
        }
    }

    #endregion

    #region Commands

    private void Reset()
    {
        _suppress = true;
        try
        {
            var res = Application.Current.Resources;

            // Drop every override so lookups fall back to the merged theme dictionary…
            foreach (var color in _allColors)
            {
                res.Remove(color.Key);
                res.Remove(BrushKey(color.ResourceName));
            }
            foreach (var key in AllDimensionKeys())
                res.Remove(key);
            foreach (var step in _allScales.SelectMany(scale => scale.Steps))
                res.Remove(ScaleKey(step.ResourceName));
            // Input paddings are derived from the control paddings, so drop their overrides too.
            foreach (var step in _allScales.Where(IsControlPadding).SelectMany(scale => scale.Steps))
                res.Remove(InputPaddingKey(step.ResourceName));

            // …then re-seed the editors from those restored values.
            foreach (var color in _allColors)
                color.SetColorSilently(ReadColor(color.Key));
            foreach (var dim in _allDimensions)
                dim.SetValueSilently(ReadDouble(DimensionKey(dim.ResourceName)));
            foreach (var scale in _allScales)
            {
                // Re-seed from the restored md step, the one the base value is read from.
                string mdName = scale.Steps.First(step => step.Ratio == 1).ResourceName;
                if (scale.HasVertical)
                {
                    Thickness t = ReadThickness(PaddingKey(mdName));
                    scale.SetBaseSilently(t.Left, t.Top);
                }
                else
                {
                    scale.SetBaseSilently(ReadDouble(DimensionKey(mdName)));
                }
            }
        }
        finally
        {
            _suppress = false;
        }

        SelectedPreset = null;
        RegenerateXaml();
    }

    private void Copy()
    {
        try
        {
            Clipboard.SetText(GeneratedXaml);
        }
        catch
        {
            // Clipboard can transiently fail if another process holds it; ignore.
        }
    }

    private void Save()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Save theme dictionary",
            Filter = "XAML resource dictionary (*.xaml)|*.xaml|All files (*.*)|*.*",
            FileName = "Theme.xaml",
            DefaultExt = ".xaml",
            AddExtension = true,
        };

        if (dialog.ShowDialog() == true)
            File.WriteAllText(dialog.FileName, GeneratedXaml);
    }

    #endregion

    #region Resource helpers

    private static Color ReadColor(ComponentResourceKey key)
        => Application.Current.TryFindResource(key) is Color color ? color : System.Windows.Media.Colors.Magenta;

    private static double ReadDouble(ComponentResourceKey key)
        => Application.Current.TryFindResource(key) is double value ? value : 0d;

    private static Thickness ReadThickness(ComponentResourceKey key)
        => Application.Current.TryFindResource(key) is Thickness value ? value : new Thickness(0);

    /// <summary>The brush key derived from a colour's accessor name (e.g. <c>PrimaryColor</c> → <c>PrimaryBrush</c>).</summary>
    private static ComponentResourceKey BrushKey(string colorName)
        => new(typeof(JBrushes), colorName.Replace("Color", "Brush"));

    private static ComponentResourceKey DimensionKey(string name) => name switch
    {
        "Radius" => JDimensions.Radius,
        "Thickness" => JDimensions.Thickness,
        "Spacing" => JDimensions.Spacing,
        "HeightXs" => JDimensions.HeightXs,
        "HeightSm" => JDimensions.HeightSm,
        "HeightMd" => JDimensions.HeightMd,
        "HeightLg" => JDimensions.HeightLg,
        "HeightEmbedded" => JDimensions.HeightEmbedded,
        "FontSizeXs" => JDimensions.FontSizeXs,
        "FontSizeSm" => JDimensions.FontSizeSm,
        "FontSizeMd" => JDimensions.FontSizeMd,
        "FontSizeLg" => JDimensions.FontSizeLg,
        "FontSizeXl" => JDimensions.FontSizeXl,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown dimension"),
    };

    private static ComponentResourceKey PaddingKey(string name) => name switch
    {
        "PaddingXs" => JDimensions.PaddingXs,
        "PaddingSm" => JDimensions.PaddingSm,
        "PaddingMd" => JDimensions.PaddingMd,
        "PaddingLg" => JDimensions.PaddingLg,
        "InputPaddingXs" => JDimensions.InputPaddingXs,
        "InputPaddingSm" => JDimensions.InputPaddingSm,
        "InputPaddingMd" => JDimensions.InputPaddingMd,
        "InputPaddingLg" => JDimensions.InputPaddingLg,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown padding"),
    };

    /// <summary>The key of a scale step, whichever kind of resource it feeds.</summary>
    private static ComponentResourceKey ScaleKey(string name)
        => name.StartsWith("Padding", StringComparison.Ordinal) ? PaddingKey(name) : DimensionKey(name);

    private IEnumerable<ComponentResourceKey> AllDimensionKeys()
    {
        yield return JDimensions.Radius;
        yield return JDimensions.CornerRadius;
        yield return JDimensions.Thickness;
        yield return JDimensions.BorderThickness;
        yield return JDimensions.Spacing;
        yield return JDimensions.SpacingThickness;
        yield return JDimensions.HeightEmbedded;
        foreach (var dim in _allDimensions)
        {
            if (dim.ResourceName is "Radius" or "Thickness" or "Spacing")
                continue;
            yield return DimensionKey(dim.ResourceName);
        }
    }

    #endregion

    #region XAML generation

    private double DimValue(string name) => _allDimensions.First(d => d.ResourceName == name).Value;

    private void RegenerateXaml()
    {
        var sb = new StringBuilder();
        sb.AppendLine("<ResourceDictionary");
        sb.AppendLine("    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
        sb.AppendLine("    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
        sb.AppendLine("    xmlns:joufflu=\"clr-namespace:Joufflu;assembly=Joufflu\"");
        sb.AppendLine("    xmlns:system=\"clr-namespace:System;assembly=mscorlib\">");
        sb.AppendLine();

        // Colors
        sb.AppendLine("    <!--  Colors  -->");
        foreach (var entry in _allColors)
            sb.AppendLine($"    <Color x:Key=\"{{x:Static joufflu:Colors.{entry.ResourceName}}}\">{ToHex(entry.Color)}</Color>");
        sb.AppendLine();

        // Brushes (one per colour; brush name mirrors the colour name)
        sb.AppendLine("    <!--  Brushes  -->");
        foreach (var entry in _allColors)
        {
            string brushName = entry.ResourceName.Replace("Color", "Brush");
            sb.AppendLine(
                $"    <SolidColorBrush x:Key=\"{{x:Static joufflu:Brushes.{brushName}}}\" Color=\"{{DynamicResource {{x:Static joufflu:Colors.{entry.ResourceName}}}}}\" />");
        }
        sb.AppendLine();

        AppendDimensions(sb);

        sb.Append("</ResourceDictionary>");
        GeneratedXaml = sb.ToString();
    }

    private void AppendDimensions(StringBuilder sb)
    {
        double thickness = DimValue("Thickness");
        double radius = DimValue("Radius");
        double spacing = DimValue("Spacing");

        sb.AppendLine("    <!--  Dimensions  -->");
        sb.AppendLine($"    <system:Double x:Key=\"{{x:Static joufflu:Dimensions.Thickness}}\">{Num(thickness)}</system:Double>");
        sb.AppendLine($"    <Thickness x:Key=\"{{x:Static joufflu:Dimensions.BorderThickness}}\">{Num(thickness)}</Thickness>");
        sb.AppendLine();
        sb.AppendLine($"    <system:Double x:Key=\"{{x:Static joufflu:Dimensions.Radius}}\">{Num(radius)}</system:Double>");
        sb.AppendLine($"    <CornerRadius x:Key=\"{{x:Static joufflu:Dimensions.CornerRadius}}\">{Num(radius)}</CornerRadius>");
        sb.AppendLine();
        sb.AppendLine($"    <system:Double x:Key=\"{{x:Static joufflu:Dimensions.Spacing}}\">{Num(spacing)}</system:Double>");
        sb.AppendLine($"    <Thickness x:Key=\"{{x:Static joufflu:Dimensions.SpacingThickness}}\">{Num(spacing)}</Thickness>");
        sb.AppendLine();

        // Every size step is emitted with its derived value, so the dictionary stays a drop-in.
        foreach (var scale in _allScales)
        {
            sb.AppendLine($"    <!--  {scale.Label}  -->");
            foreach (var step in scale.Steps)
            {
                sb.AppendLine(scale.HasVertical
                    ? $"    <Thickness x:Key=\"{{x:Static joufflu:Dimensions.{step.ResourceName}}}\">{Num(step.Value)},{Num(step.Vertical)}</Thickness>"
                    : $"    <system:Double x:Key=\"{{x:Static joufflu:Dimensions.{step.ResourceName}}}\">{Num(step.Value)}</system:Double>");
            }
            sb.AppendLine();

            // The input paddings ride along with the control paddings, at half the horizontal.
            if (IsControlPadding(scale))
            {
                sb.AppendLine("    <!--  Input padding (control padding at half the horizontal)  -->");
                foreach (var step in scale.Steps)
                {
                    Thickness input = InputPaddingFrom(step);
                    sb.AppendLine($"    <Thickness x:Key=\"{{x:Static joufflu:Dimensions.{"Input" + step.ResourceName}}}\">{Num(input.Left)},{Num(input.Top)}</Thickness>");
                }
                sb.AppendLine();
            }

            // The close-button height rides along with the control height's base value.
            if (IsControlHeight(scale))
            {
                sb.AppendLine("    <!--  Close button height (0.625 of the control height's base value)  -->");
                sb.AppendLine($"    <system:Double x:Key=\"{{x:Static joufflu:Dimensions.HeightEmbedded}}\">{Num(Math.Round(scale.Base * CloseHeightFactor))}</system:Double>");
                sb.AppendLine();
            }
        }
    }

    private static string ToHex(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    private static string Num(double value)
        => value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

    #endregion
}
