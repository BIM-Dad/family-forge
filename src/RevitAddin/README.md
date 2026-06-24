# Revit Add-in Scaffold

This folder contains the starting point for the Symetri Family Forge Revit builder.

The target Revit versions are 2024, 2025, 2026, and 2027.

Target framework matrix:

| Revit Version | Target Framework |
| --- | --- |
| 2024 | `net48` |
| 2025 | `net8.0-windows` |
| 2026 | `net8.0-windows` |
| 2027 | `net10.0-windows` |

## Command

`Build Family From Recipe`

Expected flow:

1. User selects a validated `family_recipe.json`.
2. Add-in runs recipe preflight.
3. Add-in opens or creates the correct family template.
4. Add-in creates parameters and materials.
5. Add-in creates reference planes.
6. Add-in creates supported native geometry primitives.
7. Add-in writes a QA report next to the recipe.
8. User saves the generated `.rfa`.

Current command state:

- Loads in Revit as an external command.
- Prompts for a recipe JSON file.
- Runs recipe preflight.
- Reports warnings and errors in a Revit task dialog.
- Native geometry creation is the next implementation step.

## MVP Builder

The first implementation should support:

- Non-hosted Furniture and Generic Models.
- Length and material parameters.
- Rectangular extrusions.
- Simple subcategory assignment.
- Material assignment.
- Warning report for unsupported items.

## Revit API Notes

Family creation needs real Revit API references and a selected family template. The project resolves `RevitAPI.dll` and `RevitAPIUI.dll` from:

```text
C:\Program Files\Autodesk\Revit <version>
```

Build all supported versions from the repository root:

```powershell
.\tools\Build-RevitAddin.ps1
```

Or build a single version:

```powershell
.\tools\Build-RevitAddin.ps1 -RevitVersions 2026
```

The build script expects all four Revit versions to be installed under `C:\Program Files\Autodesk`.
