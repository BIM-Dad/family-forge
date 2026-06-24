# Gemini Prompt: Family Recipe

Use this when Gemini is reviewing source images, product pages, PDFs, or sketches.

```text
Create a Symetri Family Forge JSON recipe from the attached source material.

Important: output JSON only. The JSON is a recipe for a Revit builder, not code.

Follow the Symetri Family Forge Revit family best-practices standard and training context: use docs/revit-content-creation-ai-context.md as source context and docs/revit-family-best-practices.md as the operational checklist. Model reference planes first, named parameters, associated geometry intent, nested families for reusable hardware, clean subcategories/materials, and clear notes when the current v0.1 geometry is only an approximation.

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

In geometry intent and notes, describe the ideal BIM modeling strategy when the v0.1 geometry is only an approximation. Call out elements that should later become blends, sweeps, swept blends, reveals, recessed panels, or nested hardware families. Include reference planes and named parameters for bounds, centerlines, reveals, hardware positions, face frames, dividers, shelves, and leg centers where applicable.

Include family metadata, familyStrategy, parameters, referencePlaneStrategy, parameterStrategy, reference planes, materials, nestedFamilies, visibilityStrategy, publishingQa, geometry, assumptions, clarifyingQuestions, and qa.

Mark inferred or defaulted values clearly. If important dimensions are missing, ask questions in clarifyingQuestions and set qa.status to "needs_review".

Family request:
<insert request here>
```
