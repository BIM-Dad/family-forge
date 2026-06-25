# Claude Prompt: Family Recipe

Use this when Claude has the product image, sketch, or PDF available.

```text
I need a Symetri Family Forge recipe for a native Revit family.

Please inspect the attached source material and produce JSON only. Do not include Markdown, commentary, or Revit API code.

The output must be a connector-agnostic recipe that another tool can validate and use to build native Revit geometry.

Follow the Symetri Family Forge Revit family best-practices standard and training context: use docs/revit-content-creation-ai-context.md as source context and docs/revit-family-best-practices.md as the operational checklist. Model reference planes first, named parameters, associated geometry intent, nested families for reusable hardware, clean subcategories/materials, and clear notes when the current v0.1 geometry is only an approximation.

Rules:
1. Use schemaVersion "0.1".
2. Keep the result inspectable and conservative.
3. Do not silently invent critical dimensions.
4. If dimensions are missing, add clarifyingQuestions and warnings.
5. Use simple primitives first: rectangularExtrusion, voidRectangularExtrusion, cylinder.
6. For cabinets/casework, prefer separate panels over one large body mass.
7. Use cylinder axis x, y, or z only when the source visibly shows round rods, rails, posts, round legs, or cylindrical hardware.
8. Use rectangularExtrusion for flat bar pulls and squared hardware.
9. In geometry intent and notes, identify the ideal BIM modeling strategy when the v0.1 geometry is only an approximation.
10. Keep geometry type buildable, but include idealRevitTool when the correct Revit authoring method should be extrusion, voidExtrusion, cylinder, blend, sweep, sweptBlend, revolve, nestedFamily, array, or reveal.
11. Include approximationReason whenever idealRevitTool is more advanced than the current buildable primitive.
12. Call out elements that should later become blends, sweeps, swept blends, reveals, recessed panels, or nested hardware families.
13. Include reference planes for bounds, centerlines, reveals, hardware positions, face frames, dividers, shelves, and leg centers where applicable.
14. Include Width, Depth, and Height parameters when applicable.
15. Include material, panel thickness, reveal gap, hardware offset, and leg height parameters where applicable.
16. Include materials and subcategories.
17. Include familyStrategy with template, category reason, hosting reason, reusability/loadable-family rationale, lodTarget, scheduling intent, and rendering intent.
18. Include referencePlaneStrategy, parameterStrategy, nestedFamilies, visibilityStrategy, and publishingQa.
19. Include assumptions and qa warnings.
20. Set qa.status to "needs_review" unless the source material fully defines the family.

Family request:
<insert request here>
```
