# Revit Content Creation Best Practices - AI Context

**Purpose:** Use this document as the standing context for AI assistance related to Revit family/content creation. It captures Gerardo's preferred approach from a multi-session Revit content creation training series.

**How the AI should use this:**  
When asked to help create, troubleshoot, document, or improve Revit content, use these standards as the default decision framework. Do not treat Revit family creation as pure geometry modeling. Treat it as a balance of usability, project performance, documentation quality, scheduling needs, rendering needs, and content governance.

---

## 1. Core Philosophy

### 1.1 The family should serve the project outcome

The goal of a Revit family is not to model every physical detail. The goal is to support the intended project workflow.

Default priorities:

1. **Correct placement and hosting behavior**
2. **Reliable dimensions and constraints**
3. **Clear plan/RCP/elevation/section representation**
4. **Useful parameters for users, schedules, tags, and visibility**
5. **Reasonable model performance**
6. **Clean materials and rendering behavior when needed**
7. **Reusable content when the condition is likely to appear again**

A family is successful when it supports the drawings, schedules, coordination, and visualization needs without adding unnecessary complexity.

### 1.2 Avoid over-modeling

Do not model hidden internal details, hollow interiors, extra edges, unnecessary bevels, or small construction features unless they are needed for:

- Documentation
- Scheduling
- Coordination
- Rendering
- Fabrication-level information explicitly required by the project

Revit processes geometry and edges. Extra geometry adds model weight and can slow projects down. Detail level should match the project's use case, fee, and modeling expectations.

**Default rule:** If the detail does not affect drawings, schedules, coordination, or rendering, simplify it.

### 1.3 Model the level of detail intentionally

Before giving family-building guidance, ask or infer the intended use:

- Is this a placeholder?
- Is it for design visualization?
- Is it for construction documentation?
- Is it for scheduling or procurement?
- Is it for repeat use across projects?
- Does it need to cut a host?
- Does it need to render accurately?
- Does it need to be tagged or scheduled?

The level of modeling should align with the answer.

---

## 2. Family Type Decision Rules

### 2.1 Loadable family vs model-in-place

Use a **loadable family** when:

- The element will be used more than once.
- The element could be reused on another project.
- The content may belong in a library.
- Parameters, types, schedules, visibility controls, or nesting are needed.
- The geometry may be copied or repeated.

Use **model-in-place** only when:

- The condition is truly one-off.
- It is highly project-specific.
- It is unlikely to ever be reused.
- It needs to be modeled directly from surrounding project geometry and will not be copied around.

**Avoid copying model-in-place elements.**  
Repeated model-in-place geometry increases model weight because each copy carries the geometry again. A loadable family is loaded once, and additional instances are comparatively lighter.

### 2.2 System family vs component family

Revit includes both system families and component/loadable families.

System families include things like:

- Walls
- Floors
- Roofs
- Ceilings
- Stairs
- Railings
- Wall sweeps
- Some embedded profile-based elements

Component/loadable families are saved as `.rfa` files and can be loaded across projects.

When working with system-family-related content, remember that many system families rely on nested component families such as profiles, balusters, panels, and trims.

### 2.3 Wall sweep vs separate family/model-in-place

Use a wall sweep embedded in a wall type only when the condition belongs on every instance of that wall type.

Use a separate element when:

- The condition only occurs on some walls.
- Embedding it would force too many wall types.
- It would make project management harder.
- Users need independent control over placement or extent.

The guiding question is:

> Does embedding this reduce management, or does it create more things to manage?

### 2.4 Generic Model as a flexible starting point

Generic Model is often a good starting template because it is flexible and not overloaded with assumptions.

Use Generic Model when:

- The content does not need category-specific behavior at the start.
- You want a clean, minimal template.
- You plan to set the category intentionally afterward.
- You want to avoid inherited constraints that cannot be deleted.

Then set the family category intentionally.

### 2.5 Host-based templates

Choose a host-based template only when host behavior is actually needed.

Use a face-based, wall-based, ceiling-based, roof-based, or floor-based family when:

- The family must cut a host.
- The family must behave relative to a host in a specific way.
- The host behavior is central to the family.

Do not choose a host-based template just because an object happens to be near or on a surface.

A non-hosted or work-plane-based family can often still be placed on surfaces. Host-based is mainly necessary when the family must cut or depend on the host.

### 2.6 Face-based vs ceiling-based light fixtures

For light fixtures, avoid automatically choosing the Lighting Fixture template without considering its constraints.

Reasons to be cautious:

- Some lighting templates include built-in light source reference behavior that may be too restrictive.
- Geometry can accidentally get tied to the light source reference plane.
- If the light source is trapped inside solid geometry, rendering may fail or emit no light.
- A generic or nested approach can provide better control.

Preferred strategy for complex light fixtures:

- Separate the fixture into nested components.
- Allow the mounting piece to follow a ceiling or face when needed.
- Allow the pendant body or chain to remain vertical when needed.
- Keep the light source adjustable and not trapped in solid geometry.

### 2.7 Template choice is hard to reverse

Family template selection matters because some template behavior cannot be removed later.

The category can often be changed after the family is created, but host/template behavior and built-in reference conditions may not be cleanly reversible.

When unsure, start from a simpler template and build only the behavior needed.

---

## 3. Reference Plane Strategy

### 3.1 Build the skeleton first

A good Revit family starts with a reference-plane framework. Geometry should follow the framework.

Preferred workflow:

1. Define the family origin.
2. Define main horizontal and vertical reference planes.
3. Name important reference planes.
4. Set reference strength where useful.
5. Add dimensional constraints between reference planes.
6. Label dimensions with parameters.
7. Lock geometry to reference planes, not to other geometry.
8. Flex/test the family before adding more complexity.

### 3.2 Keep origin consistent

The family origin matters for:

- Placement
- Alignment
- Nesting
- Mirroring
- Locking
- Predictable instance behavior

When building nested families, make sure their origin planes are consistent and meaningful. Use origin planes when aligning nested components in the parent family.

### 3.3 Reference planes should represent design intent

Reference planes should not be random construction clutter. They should represent meaningful controls such as:

- Overall width
- Overall depth
- Overall height
- Centerline
- Back of object
- Face of host
- Mounting center
- Clearance line
- Top/bottom control
- Interior offset
- Array start/end
- Panel boundary
- Cut boundary

### 3.4 Avoid geometry-to-geometry constraints

Do not lock geometry directly to other geometry as the normal workflow.

Bad pattern:

- Solid edge locked directly to another solid edge
- Nested component locked directly to another nested component
- Geometry pulling geometry with no governing reference plane

Preferred pattern:

- Geometry A locks to reference plane.
- Geometry B locks to the same reference plane.
- The reference plane is controlled by dimensions/parameters.

This makes the family more stable and easier to troubleshoot.

### 3.5 Use reference strength intentionally

Reference planes may be set as strong, weak, not a reference, etc.

Use reference behavior to control what users can snap to and align to after the family is loaded into a project.

For nested families, reference planes also define the usable snap/alignment points in the parent family. If a nested family does not offer the expected alignment point, check the nested family's reference planes and their reference settings.

### 3.6 Curtain wall panel caution

Curtain wall panel families are driven by curtain grid boundaries. The left/right/top/bottom reference planes are often controlled by the host curtain wall grid.

Do not add independent width/height constraints to those boundary planes if they are meant to flex with curtain grid cells.

You can add internal reference planes for frame thickness, reveals, offsets, or panel design features, but the outer boundaries should remain driven by the curtain wall grid.

---

## 4. Parameters

### 4.1 Create parameters from the thing being controlled when possible

When creating a parameter for a dimension, select the dimension and create the parameter from the label control. This helps Revit assign the correct data type.

Example:

- Dimension -> Label -> Create Parameter = length parameter

This reduces accidental creation of the wrong parameter data type.

### 4.2 Type vs instance

Use **type parameters** when the value defines a repeatable type and should remain consistent across all instances of that type.

Use **instance parameters** when the value may vary per placed instance.

Common instance candidates:

- Mounting height
- Adjustable offsets
- Finish/material overrides, when project-specific
- Visibility toggles that may vary per placement
- One-off dimensional adjustments
- Host/project-specific conditions

Common type candidates:

- Nominal width/height/depth
- Standard product sizes
- Standard trim/profile options
- Standard door panel types
- Default offsets
- Standard detail configurations

### 4.3 Do not drive type parameters with instance parameters

A type parameter should not depend on an instance parameter. If a formula is driven by an instance value, the resulting parameter usually needs to be an instance parameter as well.

### 4.4 Parameter grouping

Group user-facing parameters where people expect to find them.

Hide or reduce clutter by moving non-user-facing parameters to less prominent groups, often **Other**, especially when they are:

- Formula-driven
- Internal controls
- Not meant to be edited
- Intermediate values
- Reporting values
- Nested control parameters

A family with too many visible parameters becomes harder for users to understand and easier to misuse.

### 4.5 Naming parameters

Use clear, readable parameter names. Avoid symbols that can cause formula issues.

Good examples:

- Width
- Depth
- Height
- Mounting Height
- Fixture Finish
- Glass Material
- Light Bulb Material
- Rough Opening Offset
- Number of Cushions
- Cushion Width
- Array Count

Avoid:

- Names with unnecessary punctuation
- Ambiguous abbreviations
- Parameter names that use math operators like `+`, `-`, or `/` unless there is a strong reason

If a parameter name contains unusual symbols, Revit formulas may require the name in brackets.

### 4.6 Shared parameters

Use shared parameters when a value must be:

- Tagged
- Scheduled consistently across projects
- Used in office standards
- Coordinated with title blocks, schedules, or downstream workflows

Use family parameters for internal controls that do not need to be tagged or scheduled outside the family.

### 4.7 Reporting parameters

Use reporting parameters when a dimension should read existing geometry and report that value back for formulas or coordination.

Use with care:

- Reporting parameters are useful for measured geometry logic.
- They can help formulas self-update.
- They may introduce complexity if used as a substitute for a cleaner reference framework.
- Confirm whether the value needs to be scheduleable/taggable and whether it should be shared.

---

## 5. Formulas and Logic

### 5.1 Use formulas to reduce manual user burden

Use formulas when one setting should logically control another.

Examples:

- Cushion Width = Bench Length / Number of Cushions
- Flat Stock Trim = not(Profile Trim)
- Swing Jamb = not(Pocket Door)
- Pocket Jamb = Pocket Door
- Rough Width = Width + Rough Opening Offset controls
- Rough Height = Height + floor/undercut/rough-opening controls

The goal is to prevent users from needing to check or update multiple related values manually.

### 5.2 Boolean formulas

Use Yes/No parameters to control visibility and mutually exclusive options.

Common patterns:

```text
Flat Stock Trim = not(Profile Trim)
Pocket Jamb = Pocket Door
Swing Door = not(Pocket Door)
```

This keeps users from showing duplicate or conflicting geometry.

### 5.3 Formula syntax caution

If Revit gives a formula error such as inconsistent units or cannot interpret a parameter, check:

- Parameter names
- Brackets around names with symbols
- Data type
- Type vs instance mismatch
- Units
- Whether the formula references a value that is not available in that context

### 5.4 Intermediate parameters

Use intermediate parameters when it makes the formula more readable.

Example:

```text
Cushion Width = Bench Length / Number of Cushions
Cushion Width Half = Cushion Width / 2
```

This makes constraints easier to label and troubleshoot.

### 5.5 Rounding and dimensional precision

Do not automatically force precise fabrication rounding into the family unless the project actually needs that. Sometimes rounding is better handled in schedules or documentation rather than geometry.

Before adding advanced rounding/trigonometry logic, ask:

- Is this needed for documentation?
- Is this needed for procurement?
- Is this needed for fabrication?
- Will it make the family too fragile?
- Is the project model intended to carry this level of precision?

---

## 6. Arrays

### 6.1 Use arrays only when the repeated behavior is intentional

Use arrays for elements like:

- Shelves
- Cushions
- Chain links
- Repeated panels
- Repeated fixtures
- Repeated hardware
- Patterned components

Avoid arrays when manual placement or simpler nested types would be more stable.

### 6.2 Group and associate

When creating a parametric array, use **Group and Associate** so that the array count can be parameterized.

### 6.3 First-to-second vs first-to-last

Understand the array driving method.

Use **first-to-second** when:

- You want to control spacing between repeated items.
- The count grows by adding more elements at the same spacing.
- The final location is allowed to move.

Use **first-to-last** when:

- You want the array to fill a fixed length.
- The first and last elements are constrained.
- Intermediate elements should distribute between them.

### 6.4 Do not constrain disappearing elements

If an array is driven by the second item and you constrain the last item, reducing the array count can remove the constrained element and break the family.

Constrain the correct controlling elements based on the array behavior.

### 6.5 Arrays and nested families

Nested array elements should have reliable reference planes and origins. If an arrayed nested family cannot align or lock correctly, check:

- Whether the nested family has useful reference planes
- Whether the reference planes are set as references
- Whether Work Plane-Based is enabled when needed
- Whether Always Vertical is appropriate
- Whether the nested geometry was modeled in plan or elevation
- Whether the nested family origin is meaningful

---

## 7. Nested Families

### 7.1 Use nested families to separate behavior

Use nested families when different parts of the object need different behavior.

Examples:

- A pendant light canopy follows a sloped ceiling, while the chain/body stays vertical.
- A door family contains selectable nested door panel types.
- A chain family arrays a nested chain link.
- A window family uses nested casing/trim components.
- A cabinet or bench uses repeated nested components.

Nesting lets each part have its own reference framework, visibility, type options, and behavior.

### 7.2 Map nested parameters to parent parameters

When loading a nested family into a parent family, map important nested parameters to parent parameters.

Typical mappings:

- Width
- Height
- Depth
- Thickness
- Material
- Visibility
- Type selector
- Offset
- Mounting height
- Array count

If a nested component does not flex when the parent flexes, check whether its parameters are mapped correctly.

### 7.3 Use type selector parameters for nested options

For families like doors, use a nested family type parameter when users need to choose between design options.

Example:

- Door Panel Type = Flush / Flat / Raised / Traditional / TS2020 / TS2010, etc.

This can reduce the number of separate family files and keep related options in one controlled parent family.

### 7.4 Save useful nested family edits back to the library

If a nested family is improved and the change may help others, save/publish it to the managed content library workflow rather than keeping it only inside one project or parent family.

---

## 8. Geometry Modeling Tools

### 8.1 Choose the simplest stable tool

Use the simplest modeling method that achieves the needed result.

Typical uses:

| Tool | Best Use |
|---|---|
| Extrusion | Straight, constant-depth shapes |
| Blend | Tapered or transitioning shapes |
| Revolve | Lathe-like objects, bulbs, cylinders, rounded caps, knobs |
| Sweep | A profile following a path |
| Swept Blend | A profile changing along a path |
| Void Extrusion | Cutting simple openings |
| Void Sweep / Void Blend | Cutting along paths or tapered shapes |

There is often more than one valid way to build a family. Prefer the one that is easiest to control, explain, and flex.

### 8.2 Extrusions

Use extrusions for straightforward block-like or planar geometry.

Keep them constrained to reference planes when they need to flex.

### 8.3 Blends

Use blends for tapered shapes, angled glass, lantern bodies, flared elements, or transitions between two profiles.

Blends can include internal and external loops to create thickness without needing a separate cut, when appropriate.

### 8.4 Revolves

Use revolves for circular or lathe-like objects.

Examples:

- Light bulb
- Socket
- Cylindrical connector
- Knob
- Rounded cap
- Spherical or partially spherical forms

Draw the axis intentionally. The axis does not close the sketch; the sketch profile still needs to be valid.

### 8.5 Sweeps

Use sweeps for profiles along paths.

Examples:

- Trim
- Casing
- Chains
- Tubes
- Rings
- Decorative edges

Use loaded profiles when standard profiles already exist. Do not redraw standard trim profiles unnecessarily.

### 8.6 Swept blends

Use swept blends when a shape follows a path and transitions between two profiles.

Be aware of limitations:

- Swept blends can be less forgiving.
- They may not support complex multi-segment paths cleanly.
- They may require matching profiles segment by segment.
- They are less flexible than freeform modeling tools in Rhino or similar software.

### 8.7 Voids

Use voids to cut geometry when it is simpler than modeling each piece independently.

Be cautious:

- Very small cuts can fail.
- Complex intersecting cuts can fail.
- Self-intersecting voids can fail.
- Too many voids can make the family harder to maintain.

When a void fails, adjust the geometry and look for the threshold that causes Revit to fail.

### 8.8 Join geometry only when appropriate

Joining geometry can make pieces behave as one solid, but it also shares properties.

When geometry is joined, it may share:

- Material behavior
- Visibility settings
- Detail level behavior
- Selection/graphics behavior

Do not join parts that need separate materials, visibility, or detail control.

If separate control is needed, keep geometry unjoined or unjoin it.

---

## 9. Visibility, Graphics, and Representation

### 9.1 Do not rely only on 3D geometry for plan/RCP graphics

For many families, especially light fixtures, furniture, and symbolic objects, the plan or RCP representation should be intentional.

Use:

- Symbolic lines
- Masking regions when appropriate
- Visibility settings
- Detail-level control
- Subcategories

A light fixture in RCP usually needs a symbol, not a literal projection of all modeled geometry.

### 9.2 Use family visibility controls

Family visibility settings affect whether geometry appears in:

- Plan/RCP
- Front/back
- Left/right
- 3D
- Coarse
- Medium
- Fine

Use these controls intentionally.

Example:

- Show simplified fixture body at coarse.
- Show detailed bulb/socket only at fine.
- Hide glass in plan if it creates distracting lines.
- Show symbolic RCP representation instead of modeled fixture edges.

### 9.3 Use detail levels deliberately

Detail levels are not just project view settings. Family geometry can be assigned to coarse, medium, or fine.

Use this to reduce visual clutter and improve performance.

Suggested default:

- **Coarse:** simplified representation
- **Medium:** normal documentation representation
- **Fine:** detailed modeling/rendering representation

### 9.4 Use Preview while testing family visibility

Use the family preview toggle to see how the family will behave when loaded into a project.

Test visibility at:

- Coarse
- Medium
- Fine
- Plan
- RCP
- Elevation
- 3D

### 9.5 Use subcategories for graphic control

Create and assign subcategories when users may need to control linework or visibility separately.

Examples:

- Frame
- Glass
- Hardware
- Symbol
- Clearance
- Hidden reference
- Trim
- Shade
- Chain
- Panel

Subcategories make content more manageable in project visibility/graphics settings.

---

## 10. Materials

### 10.1 Use material parameters

Do not hard-code materials when users may need to adjust finishes.

Use material parameters for:

- Fixture Finish
- Glass Material
- Light Bulb Material
- Frame Material
- Panel Material
- Hardware Material
- Seat Material
- Casework Finish

This avoids requiring users to edit the family just to change a finish.

### 10.2 Separate materials by separating geometry where needed

If two parts need different materials, they need separate geometry or carefully managed unjoined geometry.

Example:

- Light bulb glass should not share the same material as the socket.
- Fixture body should not share the same material as glass.
- Trim should be independently controllable if finish varies.

### 10.3 Rendering considerations

For light fixtures:

- Keep the light source from being trapped inside opaque geometry.
- Use glass or emissive materials where appropriate.
- Do not over-model internal light components unless visible or needed for rendering.

---

## 11. Content Sourcing and Management

### 11.1 Preferred search order for existing content

Before creating new content from scratch, search in this order:

1. Managed office library, such as Kinship
2. Managed project-indexed content
3. Out-of-the-box Autodesk/Revit library
4. Known internal project examples
5. Trusted manufacturer content
6. Reputable BIM content sites
7. Custom family creation

### 11.2 Avoid unvetted content sources

Avoid or be cautious with open/unvetted family sites such as RevitCity. Some content may be usable, but the risk is higher for:

- Poorly built families
- Corrupt content
- Bad categories
- Excessive file size
- Over-modeled geometry
- Bad parameters
- Broken constraints
- Unreliable materials
- Unwanted imports

Preferred external sources include vetted manufacturer sites and reputable BIM content platforms such as ARCAT, BIMobject, and BIMsmith, while still reviewing content before use.

### 11.3 Manufacturer content

If a specific product is being used:

- Search the manufacturer website.
- Check if Revit content is available.
- If not publicly available, consider contacting the manufacturer.
- Review the downloaded content before loading it broadly into projects.

Manufacturer families are not automatically good families. Always inspect category, file size, parameters, visibility, geometry, and materials.

### 11.4 Push useful edits back to the library

When a family is improved and likely useful again:

- Save it properly.
- Add it to the managed library process.
- Use pending approval where required.
- Notify the responsible library/content manager.
- Avoid keeping improvements hidden in personal or project-only locations.

### 11.5 Avoid private content stashes

Discourage users from keeping personal hidden libraries. Reusable content should become part of the governed library so the team benefits and standards remain consistent.

---

## 12. File Hygiene

### 12.1 Remove reference images and PDFs

If images or PDFs were imported for tracing, remove them when modeling is done.

Deleting the placed image is not enough. Also remove it from **Manage Images** so it is no longer embedded in the family file.

Leaving unused images/PDFs increases file size.

### 12.2 Remove temporary lines

Remove temporary modeling guides before publishing.

Check for and delete:

- Model lines used as layout guides
- Symbolic lines used only for tracing
- Imported DWG/PDF/image remnants
- Temporary reference geometry that users should not see

Use selection filters to isolate model lines or symbolic lines when cleaning up.

### 12.3 Delete backup files before sharing

Revit creates backup files when saving families. Delete unnecessary backup files before sharing or publishing content.

### 12.4 Save with a useful preview

Family thumbnails/previews often come from the last saved view.

Before publishing:

- Open a clean 3D or preview view.
- Set the view to show the family clearly.
- Save from that view.
- Confirm the thumbnail/preview is useful in the content manager.

This matters for content browsing tools such as Kinship.

### 12.5 Keep file size reasonable

A family should not carry unnecessary:

- Imports
- Images
- DWGs
- Hidden geometry
- Overly detailed manufacturer meshes
- Extra types
- Unused nested families
- Redundant materials
- Excessive voids or edges

---

## 13. Family Build Workflow

Use this workflow when advising on or building a new family.

### Step 1 - Define purpose

Document:

- What is the object?
- Where will it be used?
- Is it one-off or reusable?
- Does it need to be scheduled?
- Does it need to be tagged?
- Does it need to render?
- Does it need to cut a host?
- Does it need symbolic plan/RCP representation?
- What level of detail is appropriate?

### Step 2 - Choose family strategy

Decide:

- Loadable family or model-in-place
- Template
- Category
- Host behavior
- Work plane behavior
- Whether nesting is needed
- Whether shared parameters are needed

### Step 3 - Create reference framework

Set up:

- Origin
- Centerlines
- Width/depth/height planes
- Important offsets
- Host face/reference
- Mounting references
- Array start/end references
- Named reference planes
- Parameterized dimensions

### Step 4 - Add primary geometry

Use the simplest geometry tools:

- Extrusions for simple forms
- Blends for tapered forms
- Revolves for rotational forms
- Sweeps for trim/profile/path forms
- Voids only where needed

Constrain geometry to reference planes.

### Step 5 - Add parameters

Add:

- Dimensions
- Type/instance controls
- Material parameters
- Visibility controls
- Formula-driven values
- Nested family type selectors
- Array count/spacing parameters where needed

Group and name parameters cleanly.

### Step 6 - Add representation controls

Set:

- Plan/RCP graphics
- Symbolic lines
- Detail level behavior
- Visibility by view direction
- Subcategories
- Materials

### Step 7 - Flex and test

Test:

- Minimum size
- Maximum size
- Typical sizes
- Type switching
- Instance overrides
- Visibility toggles
- Material changes
- Host behavior
- Cutting behavior
- Nesting behavior
- Array count changes
- Schedule/tag behavior if applicable

### Step 8 - Clean up and publish

Clean:

- Temporary lines
- Imported images/PDFs
- Backup files
- Unused types
- Unused materials
- Unused nested content
- Bad previews

Then save and publish through the managed library workflow.

---

## 14. Troubleshooting Logic

### 14.1 If a family breaks when flexed

Check:

1. Are geometry elements locked to each other instead of reference planes?
2. Are reference planes moving in conflicting ways?
3. Is an array element disappearing?
4. Is a type parameter being driven by an instance parameter?
5. Are formulas using the wrong data type or units?
6. Are nested parameters not mapped?
7. Are constraints too tight at small sizes?
8. Are voids self-intersecting?
9. Are joined elements sharing properties unexpectedly?
10. Is the family using a template with hidden assumptions?

### 14.2 If nested geometry does not resize

Check:

- Width/height/depth parameters in the nested family
- Whether nested parameters are instance or type
- Whether parent parameters are mapped
- Whether the nested family is locked to parent reference planes
- Whether reference planes are available as references
- Whether the nested family has a consistent origin

### 14.3 If something will not align

Check:

- Whether the target reference plane is set to Not a Reference
- Whether the nested family origin/reference planes are correct
- Whether geometry was modeled in the expected orientation
- Whether Work Plane-Based or Always Vertical settings are correct
- Whether the element is being placed on the correct work plane

### 14.4 If rendering light does not work

Check:

- Is the light source inside opaque geometry?
- Is the bulb material transparent/emissive as needed?
- Is the fixture body blocking the light?
- Is the light source controlled independently from geometry?
- Is the lighting template too restrictive?

### 14.5 If the family is too heavy

Check for:

- Embedded images/PDFs
- Imported CAD
- Over-modeled geometry
- Too many voids
- Excessive manufacturer detail
- Unused nested families
- Backup files
- Unused types/materials
- Detailed elements shown at all detail levels

---

## 15. AI Response Standards

When responding to family/content creation requests, the AI should provide answers in Gerardo's preferred practical format.

### 15.1 Default response structure

For a new family request, respond with:

1. **Recommended family strategy**
   - Template
   - Category
   - Host/work plane behavior
   - Loadable vs model-in-place
   - Whether nesting is needed

2. **Reference plane plan**
   - Origin
   - Main controls
   - Named reference planes
   - Alignment/locking strategy

3. **Parameter plan**
   - Type parameters
   - Instance parameters
   - Material parameters
   - Visibility parameters
   - Shared parameter needs
   - Formula parameters

4. **Geometry plan**
   - Which modeling tools to use
   - What not to model
   - Where voids are appropriate
   - Where symbolic lines should replace geometry

5. **Visibility/graphics plan**
   - Plan/RCP representation
   - Coarse/medium/fine behavior
   - Subcategories
   - Material handling

6. **Testing checklist**
   - Flexing
   - Host behavior
   - Schedule/tag behavior
   - Visibility
   - Materials
   - File hygiene

### 15.2 Avoid giving only generic Revit advice

Do not simply say "create a family and add parameters." Provide a practical modeling path with decision rules.

### 15.3 Favor stability over cleverness

If a highly parametric solution would be fragile, recommend a simpler family with clear limits.

### 15.4 Be explicit about when complexity is not worth it

Call out when a requested family is likely being over-modeled or over-parametrized.

Use language such as:

- "This is possible, but I would not make it parametric unless the project needs that value for documentation or scheduling."
- "I would keep this as a simpler type-driven family instead of adding formulas."
- "This is better handled in a schedule or type naming than by driving geometry."
- "This should be a loadable family if it will be reused or copied."

### 15.5 When asked to help Codex or another AI build something

Provide Markdown instructions with:

- Goal
- Existing behavior
- Desired behavior
- Revit constraints
- Parameter names
- File/family assumptions
- Implementation steps
- Edge cases
- Testing checklist
- Acceptance criteria

---

## 16. Family Publishing QA Checklist

Before a family is added to a shared library, verify the following.

### Template and category

- [ ] Correct template/host behavior
- [ ] Correct family category
- [ ] Correct work plane behavior
- [ ] Correct cutting behavior if applicable
- [ ] Correct schedule/tag behavior if applicable

### Reference framework

- [ ] Origin is meaningful
- [ ] Main reference planes are named
- [ ] Geometry is locked to reference planes, not geometry
- [ ] Reference strengths are intentional
- [ ] Family flexes without warnings

### Parameters

- [ ] Parameters are named clearly
- [ ] Type vs instance is appropriate
- [ ] Formula parameters work
- [ ] Material parameters are exposed where useful
- [ ] Internal parameters are grouped out of the user's way
- [ ] Shared parameters are used where tagging/scheduling requires them

### Geometry

- [ ] Geometry is simplified to the required level
- [ ] Voids are stable
- [ ] Joined geometry shares properties intentionally
- [ ] No hidden unnecessary detail
- [ ] No excessive manufacturer mesh/detail

### Visibility and graphics

- [ ] Plan/RCP representation is correct
- [ ] Elevation/section representation is correct
- [ ] Coarse/medium/fine behavior is correct
- [ ] Symbolic lines are clean
- [ ] Subcategories are assigned
- [ ] Glass/transparent elements do not clutter documentation views

### Materials

- [ ] Materials are parameterized where needed
- [ ] Finish controls are usable
- [ ] Rendering-critical materials behave properly
- [ ] Light sources are not blocked by opaque geometry

### File hygiene

- [ ] Imported images/PDFs removed from Manage Images
- [ ] Temporary model/symbolic lines removed
- [ ] Backup files deleted before sharing
- [ ] Thumbnail/preview is clean
- [ ] File size is reasonable
- [ ] Useful edits are submitted to the managed content library

---

## 17. Source Training Sessions Synthesized

This guide was synthesized from the following Revit Content Creation training transcripts:

- 2026-01-22 - Revit content creation introduction, family types, templates, categories, reference planes
- 2026-01-23 - Modeling tools, solids/voids, lantern/light fixture approach
- 2026-01-29 - Detail levels, visibility graphics, materials, symbolic representation, cleanup
- 2026-01-30 - Pendant chain, sweeps, nested link family, arrays, reference behavior
- 2026-02-12 - Door families, nested panels, formulas, type selectors, content sourcing
- 2026-02-19 - Arrays/formulas wrap-up, model-in-place guidance, content management, window/content examples

---

## 18. Copy/Paste AI Context Block

Use the following as a compact prompt block when giving another AI standing context:

```markdown
You are assisting with Revit family and content creation. Follow Gerardo's Revit content creation standards:

- A family should serve drawings, schedules, coordination, rendering, and reuse needs without over-modeling.
- Always begin with the intended use, level of detail, schedule/tag needs, rendering needs, host/cut needs, and reuse potential.
- Prefer loadable families for anything repeated, copied, or reusable. Use model-in-place only for true one-off, project-specific conditions.
- Generic Model is often the safest flexible starting point; set category intentionally. Use host-based templates only when host cutting or host-specific behavior is required.
- Build the reference-plane skeleton first. Name important planes. Use a meaningful origin. Lock geometry to reference planes, not geometry-to-geometry.
- Create parameters from the dimension/material/visibility item being controlled when possible. Choose type vs instance intentionally. Do not drive type parameters with instance parameters.
- Use formulas for logical relationships and to prevent users from manually coordinating dependent settings. Keep formulas readable with intermediate parameters.
- Use arrays only when repeat behavior is intentional. Understand first-to-second vs first-to-last. Do not constrain array elements that may disappear.
- Use nested families to separate behavior, reuse parts, and support selectable types. Map nested parameters to parent parameters.
- Choose the simplest stable geometry tool: extrusion, blend, revolve, sweep, swept blend, and voids as appropriate. Avoid hidden detail and excessive edges.
- Join geometry only when shared material/visibility behavior is desired. Unjoin when parts need separate materials or detail-level controls.
- Use symbolic lines, subcategories, and visibility/detail-level settings for clean plan/RCP/elevation behavior. Do not rely on detailed 3D geometry for documentation graphics.
- Use material parameters for finishes, glass, bulbs, frames, hardware, and other user-controlled materials.
- Clean up families before publishing: remove imported images/PDFs from Manage Images, delete temporary lines, delete backups, save with a clean preview, and keep file size reasonable.
- Search managed libraries first, then indexed project content, then out-of-box content, then trusted manufacturers/reputable BIM sites. Avoid unvetted content where possible.
- Publish useful reusable improvements back to the managed content library workflow rather than keeping them in personal/project-only stashes.
- When giving guidance, provide: family strategy, reference plane plan, parameter plan, geometry plan, visibility/material plan, testing checklist, and acceptance criteria.
```
