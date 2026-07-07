# File Association Startup Retro

Date: 2026-07-07

## Summary

Windows `.txt` default-app launch initially appeared to fail because Memojang did not show a window after selecting it as the default app. The first assumption was that file-path argument handling or synchronous file loading was the issue. That was only part of the risk. The actual blocker was a WPF startup exception: `MainWindow.xaml` referenced `Assets/Memojang.ico`, but the `.ico` file was not included as a WPF `Resource`.

## Root Cause

`MainWindow.xaml` added:

```xml
Icon="Assets/Memojang.ico"
```

The project file set `ApplicationIcon`, but did not include the icon as a WPF resource. The executable icon and the WPF window icon are separate concerns. `ApplicationIcon` embeds the executable icon; WPF still needs the XAML-referenced icon to be available as an application resource.

Startup log captured the failure:

```text
System.IO.IOException: 'assets/memojang.ico' 리소스가 없습니다.
```

## Fix

The project now includes:

```xml
<ApplicationIcon>Assets\Memojang.ico</ApplicationIcon>

<ItemGroup>
  <Resource Include="Assets\Memojang.ico" />
  <None Include="Assets\Memojang.png" />
</ItemGroup>
```

Startup file loading was also made more resilient:

- Show and activate the main window before opening a startup document.
- Load startup documents at `DispatcherPriority.ApplicationIdle`.
- Use async file reading for startup document load.
- Write startup diagnostics to `%LOCALAPPDATA%\Memojang\startup.log`.

## Verification

- `dotnet build .\Memojang.sln`
- `dotnet test .\tests\Memojang.Tests\Memojang.Tests.csproj -p:OutDir=C:\ai\projects\memojang\artifacts\test-bin\icon-resource-startup\`
- Windows shell-open smoke: `Start-Process <sample.txt>`
- Log confirmed:

```text
MainWindow shown
Startup document path=...
OpenDocument async success ...
```

## Reflection

The failure came from treating "icon applied" as visually done after seeing the executable icon, without verifying WPF resource loading through the same launch path the user would use. The correct habit is to test the exact user path after any shell integration change:

- If adding an executable icon, verify both executable icon and window icon.
- If adding file association support, use Windows shell open, not only direct `exe "file.txt"` invocation.
- If a UI launch fails silently, add startup diagnostics before guessing.
- Do not stop at registry/default-app state; confirm process, visible window, and document load.

This is the same class of mistake as the earlier generated-icon source fidelity issue: the surface looked plausible, but the artifact path actually used by the product was different. Future work should verify the actual product path before declaring success.
