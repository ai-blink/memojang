# Dev Roadmap

| Milestone | Status | Evidence |
|---|---|---|
| Initialize app-dev workflow docs and templates | Done | This init run; validated with `git diff --check`, secret scan, and plan validator |
| Scaffold .NET 10 WPF solution | Done | Commit `8e3ab8e`; `dotnet build .\Memojang.sln`; `dotnet test .\Memojang.sln` |
| Implement Notepad-like minimal shell | Done | `MainWindow` menu/editor/status bar and core shortcuts; `dotnet build .\Memojang.sln`; `dotnet test .\Memojang.sln` |
| Add basic file editing commands | Done | New/open/save/save-as, dirty-state reset, line-ending detection; `dotnet build .\Memojang.sln`; `dotnet test .\Memojang.sln` |
| Add save-before-discard prompts and app smoke pass | Done | Dirty new/open/exit prompts; process-start smoke run; `dotnet build .\Memojang.sln`; `dotnet test .\Memojang.sln` |
| Add Markdown preview foundation | Done | View toggle; heading/paragraph/pipe-table preview; parser tests; process-start smoke run |
| Add Markdown table helper | Done | Starter table insertion, add-row helper, source-preserving command tests |
| Add capture exclusion privacy mode | In progress | Privacy toggle, honest status text, and Win32 tooltip detail implemented; Snipping Tool/OBS/Recall manual matrix still needed |
| Package V1 preview build | Done | `PreviewFolder.pubxml`; `dotnet publish`; published exe smoke run |
