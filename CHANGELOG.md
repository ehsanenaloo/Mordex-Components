# Changelog

All notable changes to this project are listed here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project uses [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [6.0.0] - 2026-10-31

First public release.

### Added

- A theme system for all controls, with 35 built-in themes. They cover Windows 11 and 10, Visual Studio 2022, Office 365, Chrome and Firefox, and older looks from Windows Classic to Office 2010.
- `MxThemedFormFrame` draws the window frame in the style of the theme. With the Chrome and Firefox themes, `MxTabControl.TabsInCaption` puts the tabs in the title bar.
- `MxMessageBox` and `ControlThemer` show messages, dialogs and tooltips in the theme.
- A demo app with a page for every control. Run it with `--smoke` to check that every control loads.

### Changed

- Breaking: the old look settings are gone. Remove `VisualStyle`, `VisualStyleAreas`, `ThemedAreas`, `OfficeColorScheme`, `OfficeCustomColor`, `Office2007ColorScheme`, `ColorScheme`, `VisualStyleManager` and `UseThemes` from your code, and pick a theme with `MxThemeManager`. Old saved layouts still load.
- The data grid, scheduler, calendar and timeline paint faster.

### Security

- Layout files are treated as untrusted. They load only known types, read XML with DTD processing off, and have limits on size and images.
