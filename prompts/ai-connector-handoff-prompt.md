# AI Connector Handoff Prompt

Use this prompt at the start of a new Claude, Gemini, ChatGPT, Copilot, or other AI connector session. Attach the product image, dimension image/spec sheet, `schema/family-recipe.schema.json`, `docs/revit-family-best-practices.md`, and `docs/revit-content-creation-ai-context.md`, then paste this prompt.

```text
You are producing a Symetri Family Forge recipe.

Analyze the attached source material and return exactly one valid JSON object for a native Revit family recipe. Do not return Markdown, code fences, commentary, tables, or Revit API code.

The output must validate against `schema/family-recipe.schema.json` and use `schemaVersion`: "0.1".

Critical JSON shape rules:
- These top-level fields must always be JSON arrays, even when there is only one item: parameters, referencePlaneStrategy, parameterStrategy, nestedFamilies, publishingQa, referencePlanes, materials, geometry, constraints, assumptions, clarifyingQuestions.
- `qa.warnings` must always be a JSON array.
- Do not return `referencePlaneStrategy` as an object or paragraph. Return it as an array of objects.
- Do not return `geometry` as a description paragraph. Return it as an array of buildable primitive objects.
- Use double-quoted JSON strings and no trailing commas.

Family Forge v0.1 buildable geometry types:
- rectangularExtrusion
- voidRectangularExtrusion
- cylinder

Use `idealRevitTool` when the production-quality Revit authoring method should be more specific than the buildable v0.1 primitive. Allowed values: extrusion, voidExtrusion, cylinder, blend, sweep, sweptBlend, revolve, nestedFamily, array, reveal.

For casework, furniture, millwork, and storage:
- Prefer panelized geometry over one large body block.
- Include reference planes for overall Left, Right, Front, Back, Bottom, Top, Center Left/Right, Center Front/Back, face frames, reveals, dividers, shelves, drawer fronts, hardware centerlines, and leg centers where applicable.
- Include named length parameters for Width, Depth, Height, panel thickness, reveal gaps, body height, leg height, hardware offsets, and repeated bay/module dimensions where applicable.
- Model visible flat pulls as rectangularExtrusion unless the source clearly shows round rods.
- Model round/tapered legs with the closest buildable primitive now, but set `idealRevitTool` to blend or sweptBlend and explain the approximation.
- Put unanswered product or modeling decisions in `clarifyingQuestions`; do not stop the response to ask questions unless width, depth, and height are completely missing.
- Use `qa.status`: "needs_review" unless all critical dimensions and modeling choices are explicit in the source.

Required top-level structure:
{
  "schemaVersion": "0.1",
  "family": {
    "name": "",
    "category": "Furniture",
    "hosting": "NonHosted",
    "units": "mm",
    "description": "",
    "sourceType": "image"
  },
  "familyStrategy": {},
  "parameters": [],
  "referencePlaneStrategy": [],
  "parameterStrategy": [],
  "nestedFamilies": [],
  "visibilityStrategy": {},
  "publishingQa": [],
  "referencePlanes": [],
  "materials": [],
  "geometry": [],
  "constraints": [],
  "assumptions": [],
  "clarifyingQuestions": [],
  "qa": {
    "status": "needs_review",
    "warnings": []
  }
}

Now create the recipe from the attached source material.
```
