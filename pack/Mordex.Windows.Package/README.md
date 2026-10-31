# Mordex Components

![Mordex Components: themed WinForms controls for .NET 10](https://raw.githubusercontent.com/ehsanenaloo/Mordex-Components/main/assets/screenshots/hero.png)

WinForms controls for .NET 10 with Visual Studio designer support.

The package has a data grid, ribbon, navigation bar, task pane, date picker, scheduler, timeline, docking panels, command bars, tab controls and basic controls. It includes the out-of-process designer, so the controls appear in the Visual Studio toolbox.

## Screenshots

![Ribbon, data grid, scheduler, timeline, docking and more](https://raw.githubusercontent.com/ehsanenaloo/Mordex-Components/main/assets/screenshots/controls.png)

![The ribbon and the data grid in six of the 35 themes](https://raw.githubusercontent.com/ehsanenaloo/Mordex-Components/main/assets/screenshots/themes.png)

## Themes

All controls follow one theme. There are 35 built-in themes: Windows 11 and 10, Visual Studio 2022, Office 365, Chrome and Firefox in light and dark, and older looks such as Windows Classic, Windows XP and Office 2003 to 2010.

```csharp
using Mordex.Windows.Common;

MxThemeManager.DefaultTheme = MxThemeManager.Find("Windows 11 Dark");
```

## Links

- Source and issues: https://github.com/ehsanenaloo/Mordex-Components
- Changes: https://github.com/ehsanenaloo/Mordex-Components/blob/main/CHANGELOG.md

Free and open source under the MIT License.
