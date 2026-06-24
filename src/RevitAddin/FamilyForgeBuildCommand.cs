using System;
using System.IO;
#if NET48
using System.Web.Script.Serialization;
#else
using System.Text.Json;
#endif

namespace Symetri.FamilyForge.RevitAddin;

public sealed class FamilyForgeBuildCommand
{
    public FamilyForgeBuildResult BuildFromRecipeFile(string recipePath)
    {
        if (string.IsNullOrWhiteSpace(recipePath))
        {
            return FamilyForgeBuildResult.Fail("Recipe path is required.");
        }

        if (!File.Exists(recipePath))
        {
            return FamilyForgeBuildResult.Fail($"Recipe file was not found: {recipePath}");
        }

        FamilyForgeRecipe? recipe;
        try
        {
            var json = File.ReadAllText(recipePath);
#if NET48
            recipe = new JavaScriptSerializer().Deserialize<FamilyForgeRecipe>(json);
#else
            recipe = JsonSerializer.Deserialize<FamilyForgeRecipe>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
#endif
        }
        catch (Exception ex)
        {
            return FamilyForgeBuildResult.Fail($"Recipe could not be read: {ex.Message}");
        }

        if (recipe is null)
        {
            return FamilyForgeBuildResult.Fail("Recipe file did not contain a valid recipe.");
        }

        var validator = new FamilyForgeRecipePreflight();
        var preflight = validator.Validate(recipe);
        if (!preflight.CanBuild)
        {
            return preflight;
        }

        // Revit API implementation target:
        // 1. Resolve family template from recipe.Family.Category and recipe.Family.Hosting.
        // 2. Create family parameters.
        // 3. Create materials and material parameters.
        // 4. Create reference planes.
        // 5. Build each supported geometry primitive in a transaction.
        // 6. Apply materials, subcategories, formulas, and constraints.
        // 7. Write QA report next to the recipe.
        return FamilyForgeBuildResult.Success(
            "Preflight passed. Revit API geometry build is ready to implement.");
    }
}

public sealed class FamilyForgeRecipePreflight
{
    public FamilyForgeBuildResult Validate(FamilyForgeRecipe recipe)
    {
        var result = FamilyForgeBuildResult.Success("Preflight passed.");

        if (recipe.SchemaVersion != "0.1")
        {
            result.AddError($"Unsupported schema version: {recipe.SchemaVersion}");
        }

        if (recipe.Family.Hosting != "NonHosted")
        {
            result.AddWarning(
                $"Hosting '{recipe.Family.Hosting}' is not part of the first builder MVP.");
        }

        foreach (var geometry in recipe.Geometry)
        {
            if (geometry.Type is not ("rectangularExtrusion" or "voidRectangularExtrusion" or "cylinder"))
            {
                result.AddError($"Unsupported geometry type on '{geometry.Id}': {geometry.Type}");
            }
        }

        if (recipe.Qa.Status != "approved_for_build")
        {
            result.AddWarning(
                $"Recipe QA status is '{recipe.Qa.Status}'. Human review is recommended before delivery.");
        }

        foreach (var question in recipe.ClarifyingQuestions)
        {
            result.AddWarning($"Open question: {question}");
        }

        return result;
    }
}
