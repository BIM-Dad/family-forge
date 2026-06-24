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

## Human BIM Review Notes

The preferred Revit modeling strategy is more specific than the first generated recipe:

- The legs should be tapered round solids. In Revit, these should be modeled as blends rather than rectangular posts.
- The cabinet carcass should be modeled as separate side panels: left, right, top, bottom, and back.
- The front frame should be modeled as a sweep with a profile that has a slight bevel sloping toward the cabinet center.
- The central dividing panel should be modeled inside the cabinet, with a face panel that sits slightly back from the front face.
- Door panels should sit back inside the internal faceplate, not flush with the outermost front frame.
- Pulls appear to be cylindrical or rounded bar pulls with end returns back to the door/drawer face. A better Revit model would use sweeps, not straight cylinders or rectangular blocks.
- Reveals/gaps between doors, drawers, panels, and faceplates are minimal, approximately 1/8 in to 1/4 in.

The current v0.1 builder cannot yet create blends, sweeps, bevel profiles, or parametric reveals. The recipe should therefore distinguish between:

- `intendedModelingStrategy`: how a BIM author would ideally model the element.
- buildable v0.1 geometry: the simplified rectangular extrusion or cylinder approximation currently available.

## Assumptions

Measured overall dimensions were provided after the first image pass:

- Width: 42 in / 1066.8 mm
- Depth: 18 in / 457.2 mm
- Overall height: 45 in / 1143 mm

Internal dimensions are still inferred:

- Body height: 990.6 mm
- Leg height: 152.4 mm
- Frame thickness: 38.1 mm

The current Revit builder supports rectangular extrusions and simple cylinders. Angled/tapered blends, sweep profiles, wood grain, bevels, shadows, and true drawer/door gaps are still approximated.

## Test Goal

This test checks whether an image-derived recipe can create a recognizable native Revit family using the current Symetri Family Forge v0.1 builder.
