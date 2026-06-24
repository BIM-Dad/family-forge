# Revit Add-in Scaffold

This folder contains the intended starting point for the Symetri Family Forge Revit builder.

The files are a scaffold, not a compiled add-in yet. They document the command shape and core responsibilities so the prototype can move quickly into a dedicated Revit add-in repository or the artifact library.

## Command

`Build Family From Recipe`

Expected flow:

1. User selects a validated `family_recipe.json`.
2. Add-in opens or creates the correct family template.
3. Add-in creates parameters and materials.
4. Add-in creates reference planes.
5. Add-in creates supported native geometry primitives.
6. Add-in writes a QA report next to the recipe.
7. User saves the generated `.rfa`.

## MVP Builder

The first implementation should support:

- Non-hosted Furniture and Generic Models.
- Length and material parameters.
- Rectangular extrusions.
- Simple subcategory assignment.
- Material assignment.
- Warning report for unsupported items.

## Revit API Notes

Family creation needs real Revit API references and a selected family template. The command should be implemented in a Revit add-in project, then connected to this recipe schema.

