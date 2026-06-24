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

Measured overall dimensions were provided after the first image pass:

- Width: 42 in / 1066.8 mm
- Depth: 18 in / 457.2 mm
- Overall height: 45 in / 1143 mm

Internal dimensions are still inferred:

- Body height: 990.6 mm
- Leg height: 152.4 mm
- Frame thickness: 38.1 mm

The current Revit builder supports rectangular extrusions and simple cylinders. Angled legs, wood grain, bevels, shadows, and true drawer/door gaps are still approximated.

## Test Goal

This test checks whether an image-derived recipe can create a recognizable native Revit family using the current Symetri Family Forge v0.1 builder.
