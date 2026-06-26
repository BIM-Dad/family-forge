# Recipe Viewer

Open `viewer/index.html` in a browser and use **Open Recipe JSON**.

Example recipes now live in the opportunity package instead of inside the implementation repo:

```text
P:\_IRIS-Artifact Library\00_Opportunities\Symetri_Family_Forge\examples
```

Start with `wardrobe_3bay.recipe.json` or `image-to-recipe-tests\001-wood-storage-cabinet\family_recipe.generated.json`.

## Controls

- Drag: orbit the model.
- Shift+drag: pan the view.
- Mouse wheel: zoom.
- Reset View: restore the default camera.

Parameter sliders are preview-only. They update geometry in the browser so the recipe can be reviewed before Revit build, but they do not write JSON yet.

The viewer applies proportional scaling from the original recipe baseline for numeric X/Y/Z origins and dimensions. Expression-driven values still resolve directly from the active parameter values. This keeps the preview coherent when overall width, depth, height, or body-height sliders are adjusted, but it is not a substitute for true Revit constraints.

The viewer also reports common AI connector JSON shape mistakes. If a section such as `geometry` or `referencePlaneStrategy` is returned as a single object instead of an array, the viewer may recover enough to display it, but the feedback panel should still be used to correct the recipe before Revit build.
