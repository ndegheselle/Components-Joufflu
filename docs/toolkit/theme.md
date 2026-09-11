---
title: Theme
parent: Toolkit
nav_order: 5
---

# Theme

Joufflu ships two colour palettes — `Themes/Light.xaml` and `Themes/Dark.xaml` — with the
same keys, different values. `ThemeManager` swaps the active one at runtime through a single
merged dictionary; since every control reads its colours through `DynamicResource`, the whole
UI re-themes live.

The keys themselves — what each colour and dimension is used for — are listed in
[Design tokens](tokens.html).

Themes are selected **by name**. Three are always available — `System`, `Light` and `Dark` —
and you can register your own palettes so they become selectable alongside the built-ins.

## App setup

Merge only the control styles in `App.xaml`. Do **not** hardcode a theme dictionary — the
manager inserts the matching Light/Dark dictionary ahead of the styles at runtime.

```xml
<!-- App.xaml -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Resources.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

Initialize the manager once at startup, before the first window is shown:

```csharp
// App.xaml.cs — OnStartup
ThemeManager.Instance.Initialize();
```

## ThemeManager

Owns the active theme. Set `Theme` (a name) from anywhere and the UI re-themes live:

```csharp
using Joufflu.Themes;

ThemeManager.Instance.Theme = ThemeManager.Dark;   // "System" / "Light" / "Dark" / any registered name

bool isDark = ThemeManager.Instance.IsDark;         // the theme actually on screen
```

`ThemeManager.System`, `.Light` and `.Dark` are string constants for the built-in names. In the
`System` theme the manager reads the Windows apps theme and keeps following it, so the UI tracks
the OS light/dark switch automatically.

The manager does **not** persist the selection — that is left to the app, since each app stores its
settings differently (see [Persisting the theme](#persisting-the-theme)).

## Registering a custom theme

A custom theme is just a resource dictionary with the same keys as `Light.xaml` / `Dark.xaml`
(see [Customize theme](customize-theme.html) to generate one). Register it by name — from a pack
`Uri` or an in-memory `ResourceDictionary` — and it becomes selectable through `Theme`, appearing
in the `Themes` list next to the built-ins:

```csharp
// Register BEFORE Initialize() so a restored custom selection can be applied on launch.
ThemeManager.Instance.Register(
    "Ocean",
    new Uri("pack://application:,,,/MyApp;component/Themes/Ocean.xaml"),
    isDark: true);   // isDark surfaces through IsDark while the theme is selected

ThemeManager.Instance.Initialize();

// Later, select it like any other theme:
ThemeManager.Instance.Theme = "Ocean";
```

Reusing an existing name replaces that theme; `Unregister(name)` removes a custom one (the built-ins
cannot be removed). Registering the currently-selected theme re-applies it live — handy when the
restored selection is a custom theme registered after `Initialize()`.

## Persisting the theme

`ThemeManager` does not save the selected theme — each app owns its settings and usually wants to
fold the theme into the same store as its other preferences. Making the choice survive a restart is
two small steps:

1. **Restore** the saved name before `Initialize()`, so the right theme is shown from the first frame.
2. **Save** on every change: `ThemeManager` is an `ObservableObject`, so subscribe to `PropertyChanged`
   and write the name whenever `Theme` changes.

```csharp
// App.xaml.cs — OnStartup, after registering custom themes and before Initialize().
string? saved = LoadThemeSetting();                 // your own settings store
if (!string.IsNullOrWhiteSpace(saved))
    ThemeManager.Instance.Theme = saved;

ThemeManager.Instance.PropertyChanged += (_, e) =>
{
    if (e.PropertyName == nameof(ThemeManager.Theme))
        SaveThemeSetting(ThemeManager.Instance.Theme);
};

ThemeManager.Instance.Initialize();
```

`LoadThemeSetting`/`SaveThemeSetting` are whatever your app already uses (a JSON file, the registry,
`Settings.settings`, …). The sample app persists a small JSON file under `%AppData%` — see its
`App.xaml.cs` for the full working example.

## Building a theme switcher

No dedicated switcher control — bind any UI directly to `ThemeManager.Instance.Theme`. Map each
theme name to a `RadioButton` with the enum-to-boolean converter (it compares by value, so it works
with the theme-name strings too):

```xml
xmlns:themes="clr-namespace:Joufflu.Themes;assembly=Joufflu"

<RadioButton Content="System"
    IsChecked="{Binding Theme, Source={x:Static themes:ThemeManager.Instance},
        Converter={StaticResource EnumMatch},
        ConverterParameter=System, Mode=TwoWay}" />
<RadioButton Content="Light"
    IsChecked="{Binding Theme, Source={x:Static themes:ThemeManager.Instance},
        Converter={StaticResource EnumMatch},
        ConverterParameter=Light, Mode=TwoWay}" />
<RadioButton Content="Dark"
    IsChecked="{Binding Theme, Source={x:Static themes:ThemeManager.Instance},
        Converter={StaticResource EnumMatch},
        ConverterParameter=Dark, Mode=TwoWay}" />
<!-- Registered custom themes slot in the same way -->
<RadioButton Content="Ocean"
    IsChecked="{Binding Theme, Source={x:Static themes:ThemeManager.Instance},
        Converter={StaticResource EnumMatch},
        ConverterParameter=Ocean, Mode=TwoWay}" />
```

See the **Theme** page in the sample gallery for the full working example.
