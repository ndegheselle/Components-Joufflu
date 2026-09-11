using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Themes;

public class ThemeSamplesViewModel : ObservableObject
{
    public string SwitcherCode =>
        "<!-- Bind any control to ThemeManager.Instance.Theme (a name). Here RadioButtons -->\n" +
        "<!-- using EnumMatchToBooleanConverter to map each theme name to a choice.       -->\n" +
        "<RadioButton Content=\"System\"\n" +
        "    IsChecked=\"{Binding Theme, Source={x:Static themes:ThemeManager.Instance},\n" +
        "        Converter={StaticResource EnumMatch},\n" +
        "        ConverterParameter=System, Mode=TwoWay}\" />\n" +
        "<RadioButton Content=\"Light\" IsChecked=\"{Binding Theme, ... ConverterParameter=Light ...}\" />\n" +
        "<RadioButton Content=\"Dark\"  IsChecked=\"{Binding Theme, ... ConverterParameter=Dark ...}\" />\n" +
        "<!-- Custom themes register a name and slot in the same way -->\n" +
        "<RadioButton Content=\"Ocean\" IsChecked=\"{Binding Theme, ... ConverterParameter=Ocean ...}\" />";

    public string ManagerCode =>
        "// 1. (Optional) Register custom themes BEFORE Initialize so a persisted\n" +
        "//    custom selection can be restored on launch:\n" +
        "ThemeManager.Instance.Register(\n" +
        "    \"Ocean\",\n" +
        "    new Uri(\"pack://application:,,,/MyApp;component/Themes/Ocean.xaml\"),\n" +
        "    isDark: true);\n\n" +
        "// 2. At startup (App.OnStartup), before showing any window:\n" +
        "ThemeManager.Instance.Initialize();\n\n" +
        "// 3. Switch theme from anywhere by name:\n" +
        "ThemeManager.Instance.Theme = ThemeManager.Dark;   // Light / Dark / System / \"Ocean\"\n\n" +
        "// Themes lists every selectable name; the choice is persisted and restored.\n" +
        "// In System the app follows the Windows theme live.\n" +
        "bool isDark = ThemeManager.Instance.IsDark;";

    public string SetupCode =>
        "<!-- App.xaml: merge the control styles; the active theme -->\n" +
        "<!-- dictionary is inserted at runtime by Initialize().    -->\n" +
        "<Application.Resources>\n" +
        "    <ResourceDictionary>\n" +
        "        <ResourceDictionary.MergedDictionaries>\n" +
        "            <ResourceDictionary Source=\"pack://application:,,,/Joufflu;component/Resources.xaml\" />\n" +
        "        </ResourceDictionary.MergedDictionaries>\n" +
        "    </ResourceDictionary>\n" +
        "</Application.Resources>";
}
