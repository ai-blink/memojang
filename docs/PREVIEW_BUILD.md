# Preview Build

Memojang preview builds are folder publishes for Windows x64.

```powershell
dotnet publish .\src\Memojang.App\Memojang.App.csproj -p:PublishProfile=PreviewFolder
```

Output:

```text
artifacts\publish\Memojang-preview\Memojang.App.exe
```

Smoke check:

```powershell
Start-Process .\artifacts\publish\Memojang-preview\Memojang.App.exe
```

The preview build is framework-dependent and expects the matching .NET desktop runtime to be installed.
