# ChatGPT / OpenAI Prompt: Family Recipe

Paste this into ChatGPT or another OpenAI-compatible connector with images or PDFs attached.

```text
You are producing a Symetri Family Forge recipe.

Analyze the attached source material and create a connector-agnostic JSON recipe for a native Revit family. Do not produce Revit API code. Output JSON only.

The recipe must follow schemaVersion 0.1 and include:
- family name, category, hosting, units, description, and sourceType
- parameters with name, dataType, value, group, isInstance, and source
- referencePlanes with name, orientation, offset, and isStrongReference
- materials with name, color, and optional parameterName
- geometry using only rectangularExtrusion, voidRectangularExtrusion, or cylinder
- panelized cabinet/casework geometry where possible instead of one large body block
- cylinder geometry with axis x, y, or z for simple pulls, rods, rails, posts, and round legs
- constraints when useful
- assumptions
- clarifyingQuestions
- qa status and warnings

Use these category values only:
Furniture, Generic Models, Casework, Specialty Equipment

Use these hosting values only:
NonHosted, FaceBased, WallBased, CeilingBased, FloorBased, RoofBased

Use these units only:
mm or in

Do not silently invent important dimensions. If you infer a dimension, mark the parameter source as inferred or defaulted and add a warning. Use qa.status = "needs_review" unless all critical dimensions are provided.

Family request:
<insert request here>
```
