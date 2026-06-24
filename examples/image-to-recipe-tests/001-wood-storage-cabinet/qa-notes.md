# QA Notes: Wood Storage Cabinet

## Test Status

The generated family is a strong proof of the end-to-end pipeline, but it is not yet a production-quality family.

## What Worked

- Source image was converted into a structured recipe.
- Recipe validated successfully.
- Revit add-in generated a native `.rfa`.
- Cabinet composition is recognizable.
- Panelized body is better than the first single-block mass.
- Materials and subcategories make the intent readable.

## Intent Corrections

- The legs are tapered round legs and should be modeled as blends.
- The front frame is not just rectangular trim. It should be a sweep with a bevel/profile sloping toward the center.
- The central divider should have depth relationship: inner divider plus a face panel set back from the outer front plane.
- Door and drawer panels should be recessed within the face frame.
- Pulls should be modeled as sweeps with end returns back to the panel, not as plain cylinders or rectangular blocks.
- Reveals should be minimal, roughly 1/8 in to 1/4 in.

## Prompt Lesson

The AI should not only identify visible parts. It should infer the likely Revit modeling technique and separate ideal BIM intent from the current builder's simplified geometry.

## Next Builder Needs

- Blend primitive for tapered legs.
- Sweep primitive with profile support.
- Reveal/gap parameter.
- Front-frame or face-frame pattern.
- Recessed panel relationship.
- Pull-with-returns pattern.

