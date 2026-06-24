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
7. Add-in saves the generated `.rfa` to a `generated` folder next to the recipe.
8. Add-in opens/activates the saved `.rfa` in Revit.
9. Add-in writes a QA report next to the recipe.

Current command state:

- Loads in Revit as an external command.
- Prompts for a recipe JSON file.
- Runs recipe preflight.
- Reports warnings and errors in a Revit task dialog.
- Can run without an open project because the manifest uses `AlwaysVisible`.
- Creates a new family document from the installed Revit family template.
- Builds first-pass native rectangular extrusions from the recipe.
- Saves the family to `generated\<family name>.rfa` beside the selected recipe.
- Opens/activates the saved family file in Revit for review.
- Creates recipe materials, subcategories, and a QA report next to the recipe.

## MVP Builder

The first implementation should support:

- Non-hosted Furniture and Generic Models.
- Rectangular extrusions.
- Simple subcategory assignment.
- Material assignment.
- Warning report for unsupported items.

Current limitations:

- Length parameters are read from the recipe to size the first geometry pass, but Revit family parameters and constraints are not created yet.
- Only `rectangularExtrusion` geometry is built. Voids, cylinders, formulas, and flexing behavior are still pending.
- The generated family is saved automatically. Existing output files with the same family name are overwritten.

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
