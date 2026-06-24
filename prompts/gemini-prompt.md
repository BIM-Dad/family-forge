# Gemini Prompt: Family Recipe

Use this when Gemini is reviewing source images, product pages, PDFs, or sketches.

```text
Create a Symetri Family Forge JSON recipe from the attached source material.

Important: output JSON only. The JSON is a recipe for a Revit builder, not code.

Use schemaVersion "0.1".

Allowed family categories:
- Furniture
- Generic Models
- Casework
- Specialty Equipment

Allowed hosting:
- NonHosted
- FaceBased
- WallBased
- CeilingBased
- FloorBased
- RoofBased

Allowed geometry types:
- rectangularExtrusion
- voidRectangularExtrusion
- cylinder

For cabinets and casework, prefer separate panels over one large body mass. Use cylinder geometry with axis x, y, or z only when the source visibly shows round rods, rails, posts, round legs, or cylindrical hardware. Use rectangularExtrusion for flat bar pulls and squared hardware.

Include family metadata, parameters, reference planes, materials, geometry, assumptions, clarifyingQuestions, and qa.

Mark inferred or defaulted values clearly. If important dimensions are missing, ask questions in clarifyingQuestions and set qa.status to "needs_review".

Family request:
<insert request here>
```
