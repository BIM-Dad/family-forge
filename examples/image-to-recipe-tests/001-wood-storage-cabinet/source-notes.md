# Image To Recipe Test 001: Wood Storage Cabinet

Source: `source-image.png`

## Observed Intent

The image shows a freestanding mid-century style wood storage cabinet.

Visible features:

- Rectangular cabinet body.
- Left storage bay with two tall front doors.
- Right bay with four stacked drawers.
- Wood case/frame with darker side panels.
- Brass or dark metal horizontal drawer pulls.
- Vertical pull on the left doors.
- Four angled/tapered legs.

## Assumptions

No measured dimensions were provided with the image, so the first recipe uses default dimensions:

- Width: 1200 mm
- Depth: 450 mm
- Body height: 1200 mm
- Leg height: 180 mm
- Overall height: 1380 mm

The current Revit builder only supports rectangular extrusions, so angled legs, wood grain, bevels, shadows, and true drawer/door gaps are approximated with simple box geometry.

## Test Goal

This test checks whether an image-derived recipe can create a recognizable native Revit family using the current Symetri Family Forge v0.1 builder.

