# Thunderstore release

Run:

```powershell
.\build.ps1 -Package
```

Upload the single ZIP in `dist/`. The matching unpacked directory is retained beside it for inspection.

Before release, update the version in:

- `DadsPermadeath.csproj`
- `src/AssemblyInfo.cs`
- `src/DadsPermadeathPlugin.cs`
- `package/manifest.json`
- `CHANGELOG.md`
