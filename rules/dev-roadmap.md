# Dev Roadmap

| Milestone | Status | Evidence |
|---|---|---|
| Initialize app-dev workflow docs and templates | Done | This init run; validated with `git diff --check`, secret scan, and plan validator |
| Scaffold .NET 10 WPF solution | Done | Commit `8e3ab8e`; `dotnet build .\Memojang.sln`; `dotnet test .\Memojang.sln` |
| Implement Notepad-like minimal shell | Done | `MainWindow` menu/editor/status bar, core shortcuts, and find/replace panel; `dotnet build .\Memojang.sln`; `dotnet test .\Memojang.sln` |
| Add basic file editing commands | Done | New/open/save/save-as, dirty-state reset, line-ending detection, file association startup open, `.txt`/`.md` save defaults, and shell-open smoke for `.txt`; `dotnet build .\Memojang.sln`; focused startup tests |
| Add save-before-discard prompts and app smoke pass | Done | Dirty new/open/exit prompts; process-start smoke run; `dotnet build .\Memojang.sln`; `dotnet test .\Memojang.sln` |
| Add Markdown preview foundation | Done | View toggle; heading/paragraph/pipe-table preview; parser tests; process-start smoke run |
| Add Markdown table helper | Done | Starter table insertion, add-row helper, source-preserving command tests |
| Add capture exclusion privacy mode | In progress | Privacy toggle, honest status text, and Win32 tooltip detail implemented; Snipping Tool/OBS/Recall manual matrix still needed |
| Package V1 preview build | Done | `PreviewFolder.pubxml`; `dotnet publish`; published exe smoke run |
| Add localization and theme foundation | Done | Resource-backed UI text, Windows culture selection with English fallback, Arabic RTL flow, and system/light/dark dictionaries; build and tests passed |
| Polish custom shell chrome and toolbar | Done | Tab-like title row, compact menu row, menu contrast fixes, window drag controls, flatter tab/menu borders, themed scrollbars, capture-exclusion toggle, readable dark preview, Korean capture-exclusion wording, and basic Markdown toolbar edit commands implemented; `dotnet build`; `dotnet test`; app smoke |
| Add Markdown document mode and image assets | Done | `.txt`/`.md` mode detection, status display, preview debounce, Markdown image preview/placeholders, sidecar image copy/link insertion, and tests; `dotnet build`; `dotnet test`; app smoke; `git diff --check` |
| Harden save/image review findings | Done | Save and image attachment IO/access failures now show localized errors, unsaved image insertion forces Markdown Save As defaults, Text mode hides/disables Markdown-specific UI, and custom chrome exposes system window commands/menu access; `dotnet build .\Memojang.sln -c Release`; `dotnet test .\Memojang.sln -c Release --no-build`; `git diff --check` |
| Add Memojang app icon | Done | `Assets\Memojang.ico` configured as executable and window icon; WPF resource inclusion fixed after shell-open startup failure; `dotnet build`; focused tests; `.txt` shell-open smoke |
| Fix custom menu popup theme contrast | Done | Theme resources now load before `MainWindow`, menu popup brushes are explicit for light/dark themes, and a Memojang-owned menu item template prevents unreadable default popup colors; `dotnet build .\Memojang.sln --artifacts-path artifacts\build-menu-template2 -p:UseAppHost=false`; `dotnet test .\Memojang.sln --artifacts-path artifacts\test-menu-theme -p:UseAppHost=false`; launch smoke |
| Add quick text size, dark mode, and zoom controls | Done | Top toolbar text-size quick buttons and dark-mode toggle, plus bottom status-bar zoom slider/readout; `dotnet build .\src\Memojang.App\Memojang.App.csproj -o %TEMP%\memojang-codex-build`; `dotnet test .\tests\Memojang.Tests\Memojang.Tests.csproj -o %TEMP%\memojang-codex-test`; `git diff --check` |
