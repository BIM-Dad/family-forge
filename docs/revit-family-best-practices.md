# Symetri Family Forge Revit Family Best Practices

Use this guide when creating or reviewing a Symetri Family Forge recipe. The goal is not only to make geometry appear in Revit. The goal is to create maintainable native Revit families that follow BIM authoring practices.

## Core Principles

- Model intent first, geometry second.
- Use reference planes as the controlling framework.
- Associate geometry to reference planes wherever possible.
- Drive dimensions with named parameters.
- Prefer nested families for repeated or reusable parts.
- Keep categories, subcategories, and materials organized.
- Keep the family as simple as the required level of detail allows.
- Never silently invent critical dimensions.
- Record assumptions, inferred values, and future modeling intent in the recipe.

## Reference Planes

Every recipe should define a reference framework before geometry:

- Left
- Right
- Front
- Back
- Bottom
- Top
- Center Left/Right
- Center Front/Back when useful
- Important internal dividers, shelves, rails, or face-frame locations

Geometry should be described relative to reference planes or named parameters, not as disconnected arbitrary solids.

For cabinet and furniture families, add planes for:

- Side panel inside faces.
- Top and bottom panel faces.
- Door/drawer reveal lines.
- Drawer stack divisions.
- Face-frame front and recessed panel planes.
- Hardware centerlines.
- Leg centerlines.

## Parameters

Recipes should include parameters for the dimensions a BIM user is expected to control.

Recommended baseline:

- Width
- Depth
- Height
- Material parameters
- Panel Thickness
- Reveal Gap
- Door/Drawer Front Thickness
- Hardware Offset
- Leg Height

Use type parameters for product families with fixed catalog sizes. Use instance parameters only when the project user should adjust each placed instance.

## Geometry Association

Avoid unassociated geometry. A good generated family should eventually:

- Lock side panels to Left/Right/Front/Back/Top/Bottom reference planes.
- Lock top and bottom panels to body height planes.
- Align door and drawer fronts to reveal/reference planes.
- Place hardware from hardware centerline planes.
- Place legs from leg centerline planes.

The current builder may not support all locks yet. When a recipe uses simplified geometry, include `intent` explaining what should be constrained later.

## Nested Families

Use nested families for repeated or reusable elements.

Good candidates:

- Pulls and handles.
- Hinges.
- Knobs.
- Legs and feet.
- Casters.
- Drawer modules.
- Shelf pins or brackets.

For the wood storage cabinet test, the pulls should eventually be nested hardware families or sweep-based nested families. The main cabinet family should host and position them with reference planes and parameters.

## Cabinets And Casework

Do not model a cabinet as one solid block unless it is only a massing placeholder.

Preferred structure:

- Left side panel.
- Right side panel.
- Top panel.
- Bottom panel.
- Back panel.
- Interior dividers.
- Shelves.
- Face frame or front frame.
- Door fronts.
- Drawer fronts.
- Hardware.
- Legs or base.

Panels should have thickness. Doors and drawers should have reveal gaps and a setback from the outer frame where visible.

## Reveals And Setbacks

For furniture, doors and drawers are often not flush with the outermost frame.

Recipes should identify:

- Front frame plane.
- Door/drawer front plane.
- Reveal gap between panels.
- Setback from face frame.
- Drawer stack spacing.

When source dimensions are unknown, use conservative defaults and mark them as inferred:

- Reveal Gap: 3 mm to 6 mm, roughly 1/8 in to 1/4 in.
- Door/Drawer Front Thickness: 18 mm to 25 mm.
- Face frame depth/projection: infer from image, then mark for review.

## Sweeps And Blends

Use the right Revit modeling technique in `intent`, even when the v0.1 builder must approximate it.

Examples:

- Tapered round legs should be blends or swept blends.
- Pulls with end returns should be sweeps or nested sweep families.
- Beveled face frames should be sweeps with a profile.
- Rails, rods, and round posts may be cylinders or sweeps depending on complexity.

If the current builder cannot create the ideal form, the recipe should still preserve the ideal modeling intent.

## Materials And Subcategories

Use clear material and subcategory assignments.

Recommended furniture subcategories:

- Case Panels
- Doors
- Drawers
- Face Frame
- Hardware
- Legs
- Shelves
- Interior Panels

Material parameters should be exposed when users may need to change finishes.

## Level Of Detail

Do not overmodel by default.

Suggested levels:

- Coarse: bounding volume or simplified panel massing.
- Medium: panelized body, fronts, major hardware, legs.
- Fine: bevels, sweep profiles, nested hardware, connectors, detailed reveals.

The recipe should state which level it is targeting.

## AI Recipe Requirements

Every AI-generated recipe should include:

- Provided dimensions.
- Inferred dimensions.
- `familyStrategy` describing template, category, hosting, reusable/loadable-family intent, LOD target, scheduling intent, and rendering intent.
- `referencePlaneStrategy` describing the reference framework before geometry.
- `parameterStrategy` describing which named parameters should drive which geometry or nested parts.
- `nestedFamilies` for reusable handles, pulls, legs, hinges, modules, or hardware.
- `visibilityStrategy` describing coarse, medium, fine, plan/RCP, and subcategory behavior.
- `publishingQa` listing final delivery checks.
- Assumptions.
- Clarifying questions.
- QA warnings.
- Future modeling intent for geometry that should become blends, sweeps, nested families, constraints, or parametric relationships.

The AI should not pretend that approximate geometry is finished BIM content.

## Current Builder Gap List

The builder needs these capabilities to better match this standard:

- Create Revit family parameters.
- Create and name reference planes.
- Associate/lock geometry to reference planes.
- Add formulas and equality constraints.
- Create blends.
- Create sweeps and sweep profiles.
- Support nested family placement.
- Support reveal/setback patterns.
- Create type catalogs or multiple family types.
