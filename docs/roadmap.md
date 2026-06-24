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

## Phase 4: Client-Facing Evaluation

Goal: decide whether to expose the workflow as a client-facing toolkit.

Deliverables:

- Browser recipe review UI.
- Preview workflow.
- Upload and prompt flow.
- Guardrails and disclaimers.
- Support model.
- Pricing/service packaging recommendation.

Success criteria:

- Client can generate a draft family without understanding the schema.
- System blocks unsupported requests cleanly.
- Symetri retains quality control and brand confidence.
