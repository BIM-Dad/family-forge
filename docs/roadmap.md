# Symetri Family Forge Roadmap

## Phase 0: Definition

Status: Started

- Name the initiative: Symetri Family Forge.
- Define the connector-agnostic recipe contract.
- Create first opportunity brief and architecture notes.
- Prepare sample prompts and recipes.

## Phase 1: Recipe MVP

Goal: prove that multiple AI connectors can produce the same valid recipe format.

Deliverables:

- `family-recipe.schema.json` v0.1.
- Revit family best-practices standard for AI recipe generation.
- Imported Revit content creation training context for AI recipe generation.
- Recipe strategy sections for family strategy, reference plane strategy, parameter strategy, nested family candidates, visibility, and publishing QA.
- Generic prompt pack.
- Claude prompt variant.
- ChatGPT / OpenAI prompt variant.
- Gemini prompt variant.
- Example wardrobe recipe.
- Local validator.

Success criteria:

- At least three AI connectors can produce valid JSON for the same family request.
- Validator catches missing dimensions, unknown materials, duplicate parameters, and unsupported geometry.
- A BIM specialist can review and edit the recipe without model-code knowledge.

## Phase 2: Revit Builder Prototype

Goal: build native Revit geometry from a validated recipe.

Deliverables:

- Multi-version Revit add-in command: `Build Family From Recipe`.
- Revit 2024 build target using .NET Framework 4.8.
- Revit 2025-2026 build targets using `net8.0-windows`.
- Revit 2027 build target using `net10.0-windows`.
- No-open-project command visibility through the Revit add-in manifest.
- First-pass family document creation from installed Revit templates.
- Family template mapping.
- Rectangular extrusion builder.
- Cylinder builder for visibly round rods, rails, posts, round legs, and cylindrical hardware.
- Panelized furniture/casework recipe pattern.
- Material creation and assignment.
- Build QA report.
- Parameter creation.
- Reference plane creation.

Success criteria:

- Build add-in binaries for Revit 2024, 2025, 2026, and 2027.
- Load the command in at least Revit 2024 and the newest installed Revit version.
- Create one working non-hosted furniture family from the wardrobe sample recipe.
- Flex width, depth, and height parameters without breaking geometry.
- Generate a QA report listing warnings and assumptions.

## Phase 2.5: Furniture Fidelity Primitives

Goal: move from recognizable massing to BIM-authored furniture logic.

Deliverables:

- Blend primitive for tapered legs and tapered posts.
- Sweep primitive for pulls, rails, rods, and bevel/profile trim.
- Reveal/gap parameter support for doors, drawers, panels, and face frames.
- Recessed panel relationship for doors and drawer fronts.
- Face-frame pattern for cabinet/casework recipes.
- Prompt instructions that separate ideal Revit modeling intent from current buildable approximation.

Success criteria:

- Test 001 rebuild uses blends for legs.
- Test 001 rebuild uses sweeps for pulls with end returns.
- Test 001 front frame reads as profiled trim instead of stacked rectangular blocks.

## Phase 2.6: Constraint And Nesting Foundation

Goal: make generated families behave like authored Revit families instead of unassociated solids.

Deliverables:

- Revit family parameter creation.
- Reference plane creation from recipe.
- Geometry association/locking strategy.
- Hosted panel placement from reference planes.
- Nested family placement for pulls, legs, hinges, and repeated hardware.
- QA warnings for unassociated geometry.

Success criteria:

- Test 001 includes Width, Depth, Height, Panel Thickness, Reveal Gap, and Hardware Offset as Revit family parameters.
- Test 001 geometry is controlled from named reference planes where supported.
- Pulls are represented as nested hardware families or a documented nested-family placeholder.

## Phase 3: Service Pilot

Goal: use the tool internally on real client-like source material.

Deliverables:

- Three sample families from different source types.
- Internal QA checklist.
- Time comparison against manual family creation.
- Known failure library.
- Client-safe positioning note.

Success criteria:

- First-pass modeling time is reduced.
- Human correction points are clear and repeatable.
- Output quality is acceptable for internal draft use.

## Phase 3.5: Recipe Viewer And Preflight Review

Goal: validate model intent before Revit creates a family.

Deliverables:

- Browser recipe viewer for `family_recipe.json`.
- Initial local dependency-free viewer in `viewer/index.html`.
- Three-dimensional preview of supported primitives.
- Parameter sliders/inputs for recipe parameters.
- Open-question panel from `clarifyingQuestions`.
- Recipe feedback report for AI prompt and JSON improvement.
- Builder-gap panel that separates unsupported builder features from recipe problems.
- Export of revised JSON for the Revit add-in.

Success criteria:

- A BIM specialist can resolve open questions before launching Revit.
- Prompt-improvement notes are captured before `.rfa` generation.
- Revit-side reports become technical QA/QC reports, not the primary design-intent review surface.

## Phase 4: Client-Facing Evaluation

Goal: decide whether to expose the workflow as a client-facing toolkit.

Deliverables:

- Browser recipe review UI.
- Preview workflow.
- Open-question and recipe-feedback workflow.
- Upload and prompt flow.
- Guardrails and disclaimers.
- Support model.
- Pricing/service packaging recommendation.

Success criteria:

- Client can generate a draft family without understanding the schema.
- System blocks unsupported requests cleanly.
- Symetri retains quality control and brand confidence.
