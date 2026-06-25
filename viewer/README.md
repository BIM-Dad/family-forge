# Symetri Family Forge Recipe Viewer

This is a local preflight viewer for `family_recipe.json` files.

## Purpose

Use the viewer before creating a Revit family. It is the place to validate recipe intent, open questions, inferred/defaulted dimensions, and builder gaps.

The Revit add-in should remain focused on technical QA/QC of the generated `.rfa`.

## Run

Open `viewer/index.html` in a browser and use **Open Recipe JSON**.

The **Load Cabinet Sample** button works when the repository is served by a local static web server from the repository root.

## Current Scope

- Loads Family Forge recipe JSON.
- Shows family metadata, parameters, geometry, open questions, and builder gaps.
- Provides parameter sliders for local preview only.
- Draws an isometric preview of supported rectangular and cylindrical recipe primitives.
- Copies recipe feedback text for the next AI prompt or JSON revision.

## Not Yet Scope

- It does not create Revit geometry.
- It does not guarantee Revit parametric behavior.
- It does not save edited JSON yet.
- It does not preview true Revit sweeps, blends, nested families, or constraints yet.
