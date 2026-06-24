# Claude Prompt: Family Recipe

Use this when Claude has the product image, sketch, or PDF available.

```text
I need a Symetri Family Forge recipe for a native Revit family.

Please inspect the attached source material and produce JSON only. Do not include Markdown, commentary, or Revit API code.

The output must be a connector-agnostic recipe that another tool can validate and use to build native Revit geometry.

Rules:
1. Use schemaVersion "0.1".
2. Keep the result inspectable and conservative.
3. Do not silently invent critical dimensions.
4. If dimensions are missing, add clarifyingQuestions and warnings.
5. Use simple primitives first: rectangularExtrusion, voidRectangularExtrusion, cylinder.
6. Include Width, Depth, and Height parameters when applicable.
7. Include named reference planes.
8. Include materials and subcategories.
9. Include assumptions and qa warnings.
10. Set qa.status to "needs_review" unless the source material fully defines the family.

Family request:
<insert request here>
```

