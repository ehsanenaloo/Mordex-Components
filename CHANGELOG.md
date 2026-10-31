# Changelog

All notable changes to this project are listed here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project uses [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [6.0.0] - 2026-10-31

### Added

- One theme system for all controls. The theme manager has 35 themes: Windows 11 and 10, Visual Studio 2022, Office 365, Chrome and Firefox (light and dark where the product has both), and the older looks Windows Classic, Windows XP, Office XP, Office 2003, 2007 and 2010, and Visual Studio 2005 and 2010. A theme sets colors and also the style of faces, edges, corners, scroll bars and glyphs. A switch that follows the Windows light or dark setting is off by default.
- Every control uses the default theme without any setting: buttons, check boxes, combo boxes, tabs, status bar, data grid and editors, scheduler, calendar, timeline, date picker, navigation bar, task pane, ribbon, dock manager, command bars and menus. Scroll bars of the ribbon follow the theme too.
- Standard Windows parts inside the controls, such as the text box of a combo box, take the dark or light look of the theme.
- Printing with a dark theme prints dark text on white paper.
- A themed window frame, `MxThemedFormFrame`, draws the caption and the border of a form in the style of the theme: Luna for Windows XP, glass for Office 2007, 3D for Windows Classic. The ribbon application button follows the theme too: glass orb, file tab or flat button.
- `MxTabControl.TabsInCaption` draws the tabs, their close buttons and an add-tab button in the title bar with the Chrome and Firefox themes. `AddTabClick` and `ShowAddTabButton` go with it.
- `MxCheckBox` and `MxRadioButton` have a `DisabledFormatStyle`. A themed button draws gradient backgrounds from its state styles. Check boxes and radio buttons draw their image or icon.
- The tabs in the title bar are exposed to screen readers, are mirrored in right-to-left windows, honor `ShowCloseButton` and a disabled tab control, and a press that ends outside the title bar is finished or canceled correctly.
- A control can draw into the caption of a themed frame through `MxThemedFormFrame.CaptionExtension`. The ribbon uses it to put the application orb and the quick access bar in the title bar in the Office 2007 look. Office 2007 and 2010 frames draw lit glass caption buttons, and Chrome and Firefox frames take the frame colors of the browsers.
- The older looks use their own fonts (Tahoma 8 for Windows XP and Office 2003, Microsoft Sans Serif 8.25 for Windows Classic) when you did not set a font, and draw their own check boxes, drop-down buttons, menus, tabs, scroll bars and headers.
- The demo is a full product demo with 26 pages, a ribbon shell, a theme gallery with all themes and a theme list on every page. It shows every control of the suite.
- Tooltips of the suite and the dialogs (About, recurrence, field chooser, command bar customization) draw with the default theme and follow it while they are open. `ThemedToolTip` and `ControlThemer.Attach(form)` do the same for your own tooltips and forms.
- `MxMessageBox` shows a message box in the default theme, with the same buttons, icons and results as the Windows message box. The messages of the suite use it. `ControlThemer.AttachDialog(form)` gives your own dialog the theme and the themed window frame, and `ThemedToolTip.PaintBox` draws a tooltip box in your own tip window.
- `ThemeRole.ToolbarText` is the text color on the toolbar background. The ribbon and the command bars use it. A theme that leaves it empty uses its normal text color.
- In the Chrome and Firefox themes each tab of a tab control has its own close button.
- Themed window frames draw the ribbon quick access bar in the title bar and have a soft shadow.
- In the Office 2003 and 2007 themes, selected grid rows and list items have the orange gradient of those products. List boxes, list views and tree views follow the older looks.
- The NuGet package lists its MIT license and a link to the project.

### Changed

- Breaking: the old look settings are gone. Remove `VisualStyle`, `VisualStyleAreas`, `ThemedAreas`, `OfficeColorScheme`, `OfficeCustomColor`, `Office2007ColorScheme`, `ColorScheme`, `VisualStyleManager` and `UseThemes` from your code and choose a theme with the theme manager. Saved layouts that still contain these values load without error.
- The calendar, the scheduler, the timeline and the data grid paint short texts much faster. Grid pages paint 17 to 34 percent faster in the bench.
- The scheduler loads, clears and lays out many appointments faster. The data grid adds many rows, collapses groups and sizes columns faster.
- Theme changes reach the controls on the interface thread, and one control that fails does not stop the others.
- The standard theme follows changes of the Windows colors while the program runs.
- Timeline range queries keep the items in start order, so a query over a short range takes a fraction of a millisecond with 5,000 items.
- Caption buttons of the themed window frame draw thin glyphs in the Windows 10 and 11 style frames and sharp pixel glyphs in the Windows Classic and Windows XP frames.
- Office 365 Dark Gray puts the ribbon and the command bars on a light gray card with black text, like the original.
- Windows 10 Dark, Chrome Dark and Visual Studio 2010 use colors measured on pictures of the real products.
- The Office 2007 ribbon tabs, text and quick access bar use the colors of each Office 2007 scheme.
- Under a dark theme, standard buttons, combo boxes and the list view header are dark too.
- A themed status bar inside a themed panel paints the status bar color of the theme.
- A color picker takes the size of its swatches as soon as it is created, so its swatches are no longer cut off.

### Removed

- The Office 2003, 2007 and 2010, Visual Studio and Windows XP drawing code of the controls. These looks are themes now.

### Security

- Images in saved layouts must be PNG, BMP, JPEG or GIF. Other formats, such as metafiles, are refused.

### Fixed

- In the scheduler, a back color that you set on the date headers, the time bar or the all-day area is drawn over the theme, and a black appointment outline that you set is drawn black.
- A calendar that cannot paint shows an error in the control instead of opening a message box on every paint.
- Status bar progress with a minimum above zero draws the right fill, and the percentage text of a progress bar reads on both sides of the fill edge.
- A custom accent from `WithAccent` gets the text color with the better contrast. Previewing a theme no longer grows the theme list, and theme changes reach each window on its own thread.
- Scheduler: an empty closing range no longer blocks resizing, and a resize may end at the start of closed time. Totals of a data table no longer fail for Count, Min and Max after an edit.
- A bound image written to a grid cell no longer breaks later drawing with the same mask color. Ribbon fonts are no longer disposed while a style still uses them. Text drawn in a batch keeps the translation of the Graphics, and a transparent text color draws nothing.
- A disabled gallery item no longer becomes checked when it is clicked.
- `MxButton.BackColor` now paints the button face. Tabs in the title bar follow a panel that moves to another form, and an accessible tab of a removed page does nothing. The ribbon title bar parts attach when the ribbon is created or its caption bar setting changes, not only after a theme change.
- Excel export no longer writes characters that XML does not allow, so Excel opens the file.
- Texts with letter pairs that the system kerns, such as "Tr" and "AV", keep their exact look in the fast text path.
- Text boxes and number boxes in dark themes no longer stay white when a page is shown after the theme changed.
- The scroll bars and check glyphs take their size from the DPI of the window, not from the DPI of the system.
- A pop-up menu that opens with an animation keeps its shadow.
- The data grid no longer crashes with `RecordNavigator` turned on.
- Controls with child controls no longer crash the visual style manager.
- Images with a width different from their height now keep the right transparency.
- The ribbon header no longer loops forever when a visual style is missing.
- Animations in the task pane and navigation bar no longer freeze the window.
- Controls now release their graphics objects, streams and timers when they are disposed.
- The scheduler no longer recalculates all appointments on every paint.
- Grouping now puts equal keys in one group, also when text differs only in case.
- The "not between" filter on columns with many values now checks each value.
- Comparing numbers, text and true/false values gives the same order in both directions.
- Number values that match an enum flag combination now convert to that enum.
- Loading a grid layout while an edit is not valid now throws an error and does not change the grid.
- The grid no longer stays stuck after an error while a column header is dropped.
- Recurring appointments that last more than one day now return only the items in the requested range.
- Weekly recurrence no longer fails near the first and last dates .NET supports.
- Tabs with a selected style paint correctly when they are less than 6 pixels high.
- Screen readers now get the right ribbon MDI minimize and restore buttons, in screen order.
- Layout values that are not valid true/false, Guid or time text keep the default value.
- Large grid exports with many different styles are much faster.
- Long numbers, such as 19-digit IDs and decimals with more than 15 digits, keep all their digits in Excel.
- Grid layouts now keep the row style, a column set header style at index 0, fractional font sizes and a DBNull check box value.
- The grid date filter now keeps values late in the day.
- The grid no longer stays stuck in edit or key handling after an event handler throws.
- Auto-sizing a column with a header image and no image list no longer crashes.
- Printing the grid no longer clips the background to the screen area.
- The single card view now raises the MouseWheel event.
- The numeric editor no longer shows scientific notation in General format, reads back percent values on integer types correctly and reports decimal overflow.
- The integer up-down control stays at its limits near the ends of the integer range.
- Selecting a cell again after its column was hidden no longer throws.
- Lists with the same item twice or with null items now get one row for each item.
- Child columns whose names end in "Asc" or "Desc" now work in relations.
- Outlook date groups now work with non-Gregorian calendars and are rebuilt when the day changes.
- Standard deviation over many rows no longer overflows.
- Sorted rows that tie with another row are no longer lost, and case-sensitive groups move rows whose value changes case.
- Input masks no longer drop text that does not fit the mask.
- Conditions and formats on child tables now use the data manager culture.
- Scheduler layouts now keep the to and from texts and the background image draw mode. Older layouts still load.
- The recurrence dialog can move a range forward, keeps the pattern unchanged when a value is not valid, and no longer resets monthly and yearly values when it opens.
- Cancelling a change to an occurrence no longer keeps the new end time or owner, and a cancelled move no longer creates an exception.
- "Last weekend day" and "last weekday" recurrences now pick the last matching day of the month.
- Scheduler printing uses the requested time label font style and no longer changes the day font.
- The free time search no longer treats a slot that ends where an appointment starts as busy, and handles many appointments without running out of stack.
- Format styles in the edit controls, task pane and navigation bar no longer dispose a font the program assigned, and they release the fonts they create.
- Layouts now keep fractional font sizes and alpha modes in format styles, the task pane owner-drawn area, the navigation bar groups style, ribbon group and gallery item super tips, and the selected ribbon tab. Older layout files still load.
- Ribbon contextual tabs stay active while their control has focus, and loading a layout file without a current layout keeps the loaded tabs.
- Ribbon key tips end when the ribbon is disposed, and Esc from a drop-down in a button group goes back one level.
- Disposing a ribbon that hosts a merged ribbon no longer disposes the merged content.
- The timeline works with non-Gregorian cultures, refreshes the current time marker every minute, and places month and year intervals correctly when the interval count is above one.
- Timeline working hour exceptions with a recurrence pattern no longer overflow the stack.
- The timeline calendar bolds the right item dates, skips February 29 in other years and uses the new font for week numbers.
- Recurrence dialogs in the timeline and scheduler leave the pattern unchanged when you press Cancel.
- The date picker keeps values inside MinDate and MaxDate, ignores paste when read-only, keeps quoted text in custom formats and leaves the null state when a value is set.
- Layout merging numbers inherited collection items correctly, and dates in layouts keep milliseconds and kind.
- The filter builder accepts a value change that only changes case, and releases its table when disposed.
- The scroll bar thumb follows the mouse to the last value, and the progress bar draws large ranges and right-to-left images correctly.
- Combo box items keep their original order and indexes after moves, and deleting the selected row clears the selection.
- Buttons are no longer drawn pressed after PerformClick, and combo boxes raise MouseLeave once.
- Tab pages without a name keep their own place when a layout loads.
- The navigation bar no longer throws when a selection change is cancelled, and EnsureVisible no longer hangs.
- In Visual Studio, designer actions now support undo, and moving the mouse over a ribbon, status bar or grid no longer opens undo steps.
- Timeline time scale rows above the bottom row now step by their own interval count, and an hour step is one hour.
- The printed and on-screen timeline agree on the place of month and year intervals with a count above one.
- Very large day or week counts in the timeline no longer overflow, and the mouse wheel moves one whole interval.
- The themed scroll bar no longer throws when it has no appearance set, and the pressed thumb looks the same in both directions.
- Re-docking a floating panel no longer leaves an empty panel group behind.
- A dock style set on a floating panel is now read back and used when the panel is docked again.
- Copy, cut and paste in the date picker no longer throw when another program holds the clipboard, and cut keeps the value when the copy fails. The grid mask editor and the edit menu also cope with a busy clipboard.
- In Visual Studio, ribbon and command bar edits are now reported before the change, so undo restores the earlier state, and saving a form no longer changes the dock layout.
- The track bar no longer freezes when it is very small and shows tick marks.
- A copy of a copied format style no longer keeps a font that was already disposed.
- Setting the font picker's DropListFont to its current value no longer breaks the drop list font. Setting it to null goes back to the automatic font.
- Child lists of a relation now match rows the same way as the DataRelation (table locale, case rule and trailing spaces), also when a key column name contains a comma.
- Comparing values of mixed types is now exact and gives the same result in both orders, for example a number with a boolean, or a large integer with a double.
- Converting an integer to a flags enum with a 64-bit base type no longer throws an overflow error.
- Grouping by several columns now follows each column's CompareIgnoreCase setting.
- The standard deviation total is now correct for values with a large common offset.
- IsWorkingTime and FindFreeTimeSlot accept a time span that ends exactly at the end of working hours, even when it does not fill a whole slot.
- FindFreeTimeSlot no longer returns a start before the proposed date or after the MaxDate day. A start on the MaxDate day is allowed. It returns DateTime.MaxValue when there is no free start.
- FindFreeTimeSlot is much faster on calendars with many back-to-back appointments, and it and IsWorkingTime no longer throw for very long durations near the last supported date.
- The grid filter row no longer throws when you filter a date column on 31 December 9999.
- Grid export now stops when it is cancelled while the workbook is being written. After a write error or a cancel, nothing more is written to the stream.
- Converting a value of one enum type to another enum type no longer crashes the application.
- A failed commit of a related child row no longer stops later child rows from moving to the right parent list.
- A failed add of a new row, a filter comparison that throws, or a source list that fails while rows load no longer leaves the grid ignoring later list changes, filter updates or row loads.
- Track bar: the thumb position and mouse input now work across the full integer range. When the control is too small to show a track, mouse input no longer changes the value.
- Grid format styles no longer dispose a font set by the user when drawing or printing at a scaled size, and no longer leak fonts they build after `Reset`.
- Finding a free time slot in working time no longer freezes the scheduler when no working time can hold the requested duration, and it is faster in calendars with many recurring appointments.
- Finding free time after a very long appointment no longer slows down when recurring appointments exist, and a search that cannot fit with a working-hour pattern that repeats over many months or years now ends quickly.
- Working time checks that cross midnight, dragging or resizing an appointment, and timeline working time checks now follow the working-hour exceptions and rules of each day they cover.
- A failed font build in a grid format style no longer leaves the style with a disposed font.
- Grid format styles now dispose the fonts they build when the font is replaced or reset, scaled print fonts no longer leak, and the font copy made for custom cell editors no longer leaks.
- If starting an edit on the new row fails, the grid now removes the item it added, so trying again does not leave an extra item in the data source. The same holds when setting the parent key of a new child row in a DataSet relation fails.
- Grouping by custom group rows no longer leaves the grid stuck loading when building a group row fails.
- An error in a RowExpanded handler or a ColumnsChanged handler, or in starting an edit on a row, no longer stops the grid from responding to later expand, group or sort changes or to source updates for that row.
- The scheduler and timeline now check a time span that crosses midnight with the working hours of each day, and only for the time that was asked for.
- Dragging an appointment checks only the time it covers against the working hours.
- A closing working hour range now also closes a slot or step that it covers only in part.
- Searching for free working time stops sooner when the calendar never fits, such as many rules whose days never meet. When the rules have no short period, the search covers 400 years after the last dated rule.
- The date picker and calendar follow a change of the thread culture on the next paint.
- Timeline items keep their bar styles when a layout is loaded.
- A working hour range with the same start and end no longer closes the time around it.
- The scheduler paints a closing hour range without its own style as non-working time.
- Timeline items keep their styles when the control rebinds its data.
- The task pane paints correctly when printed to a bitmap, the dock tab highlight always follows the selected panel, and the status bar has a default height.
- Resizing an appointment that ends where closed time starts is allowed, and the timeline no longer fails on the last day of the calendar.
- New rows in a self-referencing table are cancelled in the list that created them.
- Style fonts reload correctly when a grid layout is applied to an existing style, and the style of a custom cell editor is released when the edit ends.

### Changed

- Grid format style font overrides (name, size, bold, italic, underline, strikeout) now apply the same way on screen and in scaled printing, and clearing an override brings back the original font. The `Font` property returns the font that was set, without the overrides.
- Grid format styles reject a print scale of zero, below zero or not a number, and a font size that is not a finite number.
- Filters, value lists and self-referencing keys now compare values in the declared type of the field. Text is parsed only into that type, with the same formats as before. On a text column a number is compared as text, and on a column of type Object text is not parsed. Enums of different types with the same number are equal, an enum column matches a member name, and a decimal column accepts the currency symbol.
- `TaskPaneGroupCollection.MoveAt` now moves the group to the index and shifts the groups in between. It used to swap two groups.
- `TaskPaneGroupCollection.Insert` throws `ArgumentNullException` for a null group and `ArgumentOutOfRangeException` for an index outside the collection.
- Dates in layout files are now written in round-trip format. Text that ends in "Z" now loads as UTC.
- Inserting a combo box item checks its arguments like adding one does.
- Grid export no longer keeps every cell in memory. Rows are buffered as text, and large exports use a temporary file that is deleted at the end.
- Grid export writes nothing to the stream when it fails or is cancelled before writing starts. A failure or cancel after that leaves an incomplete workbook in the stream.
- Navigation, task pane and ribbon tags can now load every type that the layout type policy allows, such as enums and points.
- `LayoutTypePolicy.Register` throws `ArgumentException` for generic types.
- The color list in exported workbooks now has a fixed order.
- `GridExporter.Export` throws `ArgumentNullException` for a null stream.

### Added

- `Mordex.Windows.Common.DesignEditScope` reports a designer change before it is made.
- `GridExporter.Export(Stream, CancellationToken)` lets you cancel a grid export.
- `GridColumn.ExportValueKind` sets whether a column is exported to Excel as numbers or as exact text.
- `LayoutTypePolicy` lets an application add its own data types to layout files.
- `LayoutImportLimits` sets the size, depth and image limits for layout files.
- `LayoutTypePolicy.ConvertOrKeepText` converts a saved layout value to its saved type when the policy allows it.

### Removed

- `Mordex.Windows.Ribbon.Design.DesignHelper.Designer_SetDirty`, and `Mordex.Windows.UI.Design.DesignHelper.RemoveObsoleteGroups` and `GetPanelInfo`. Use `DesignEditScope` and `DesignHelper.GetSavedPanelInfo`.

### Security

- Layout files can only create types from a known list, and layout XML is read with DTD processing off.
- Layout type names no longer match on name prefixes and never load an assembly.
- Layout files are rejected when they are too large, too deep or have too many elements, and large embedded images are skipped.
- Command bar and tab layouts read from an XML reader no longer process a DTD.
- Tags and values in tab, command bar, docking, combo box, navigation, task pane and ribbon layouts now load only the types that the layout type policy allows. Other types stay as text.
- Icons in a layout now follow the image size and pixel limits. An icon over a limit is skipped.

## [5.2.4]

Earlier published version.
