# Test 001: Wood Storage Cabinet

## Source

![Wood storage cabinet source image](source-image.png)

## Recipe

Use this recipe in Revit:

```text
examples\image-to-recipe-tests\001-wood-storage-cabinet\family_recipe.generated.json
```

## Expected First-Pass Output

The current builder should generate a native Revit Furniture family with:

- Panelized cabinet body with side, top, bottom, and back panels.
- Left double-door front.
- Right four-drawer front.
- Front frame rails and stiles.
- Simple rectangular brass-colored pulls matching the source image.
- Four simple rectangular leg placeholders.

## Known Simplifications

- Overall product dimensions were supplied: 42 in wide, 18 in deep, 45 in high.
- Internal component proportions are inferred from the image.
- Wood grain is represented only by material color.
- Tapered round legs should be blends, but are simplified in the current builder.
- Pulls should be sweeps with end returns, but are simplified in the current builder.
- Front frame bevels should be sweep/profile geometry, but are approximated.
- Drawer/door reveals should be minimal, approximately 1/8 in to 1/4 in.
- The family is not parametric yet.

## Test Instructions

1. Restart Revit 2026 after installing the latest add-in.
2. Run `External Tools > Symetri Family Forge - Build Family From Recipe`.
3. Select `family_recipe.generated.json`.
4. Confirm the generated `.rfa` is saved under this test folder's `generated` subfolder.
5. Compare the resulting family against `source-image.png`.
6. Record gaps or observations in `qa-notes.md`.
