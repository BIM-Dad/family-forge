# ChatGPT / OpenAI Prompt: Family Recipe

Paste this into ChatGPT or another OpenAI-compatible connector with images or PDFs attached.

```text
You are producing a Symetri Family Forge recipe.

Analyze the attached source material and create a connector-agnostic JSON recipe for a native Revit family. Do not produce Revit API code. Output JSON only.

Follow the Symetri Family Forge Revit family best-practices standard and training context: use docs/revit-content-creation-ai-context.md as source context and docs/revit-family-best-practices.md as the operational checklist. Model reference planes first, named parameters, associated geometry intent, nested families for reusable hardware, clean subcategories/materials, and clear notes when the current v0.1 geometry is only an approximation.

The recipe must follow schemaVersion 0.1 and include:
- family name, category, hosting, units, description, and sourceType
- familyStrategy with template, category reason, hosting reason, reusability/loadable-family rationale, lodTarget, scheduling intent, and rendering intent
- parameters with name, dataType, value, group, isInstance, and source
- referencePlaneStrategy describing controlling planes and what they control
- parameterStrategy describing user-facing parameters and the geometry or nested families they should drive
- referencePlanes with name, orientation, offset, and isStrongReference
- materials with name, color, and optional parameterName
- nestedFamilies for reusable parts such as pulls, handles, legs, hinges, and repeated modules
- visibilityStrategy for coarse/medium/fine behavior, plan/RCP behavior, and subcategory use
- publishingQa with checks required before client delivery
- geometry using only rectangularExtrusion, voidRectangularExtrusion, or cylinder
- geometry type must remain the current buildable primitive, but geometry may include idealRevitTool when the correct Revit authoring method should be extrusion, voidExtrusion, cylinder, blend, sweep, sweptBlend, revolve, nestedFamily, array, or reveal
- approximationReason whenever idealRevitTool is more advanced than the current buildable primitive
- panelized cabinet/casework geometry where possible instead of one large body block
- cylinder geometry with axis x, y, or z only when the source visibly shows round rods, posts, rails, round legs, or cylindrical hardware
- rectangularExtrusion geometry for flat bar pulls and squared hardware
- intent and notes describing the ideal BIM modeling strategy when v0.1 geometry is only an approximation
- explicit notes for elements that should later become blends, sweeps, swept blends, reveals, recessed panels, or nested hardware families
- reference planes for overall bounds, centerlines, reveals, hardware positions, face frames, dividers, shelves, and leg centers where applicable
- named parameters for dimensions, materials, panel thickness, reveal gaps, hardware offsets, and leg heights when applicable
- constraints when useful
- assumptions
- clarifyingQuestions
- qa status and warnings

Critical JSON shape rule:
- Always return these top-level fields as JSON arrays, even when there is only one item: parameters, referencePlaneStrategy, parameterStrategy, nestedFamilies, publishingQa, referencePlanes, materials, geometry, constraints, assumptions, clarifyingQuestions.
- Always return qa.warnings as a JSON array.
- Do not return referencePlaneStrategy, parameterStrategy, geometry, or clarifyingQuestions as paragraphs or single objects.

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
