using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#if NET48
using System.Web.Script.Serialization;
#else
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

namespace Symetri.FamilyForge.RevitAddin;

public sealed class FamilyForgeBuildCommand
{
    public FamilyForgeBuildResult BuildFromRecipeFile(
        string recipePath,
        UIApplication uiApplication)
    {
        var loadResult = LoadAndPreflightRecipe(recipePath);
        if (!loadResult.Result.CanBuild || loadResult.Recipe is null)
        {
            return loadResult.Result;
        }

        return BuildFamilyDocument(
            loadResult.Recipe,
            uiApplication,
            recipePath,
            loadResult.Result);
    }

    public FamilyForgeBuildResult BuildFromRecipeFile(string recipePath)
    {
        return LoadAndPreflightRecipe(recipePath).Result;
    }

    private static RecipeLoadResult LoadAndPreflightRecipe(string recipePath)
    {
        if (string.IsNullOrWhiteSpace(recipePath))
        {
            return new RecipeLoadResult(
                null,
                FamilyForgeBuildResult.Fail("Recipe path is required."));
        }

        if (!File.Exists(recipePath))
        {
            return new RecipeLoadResult(
                null,
                FamilyForgeBuildResult.Fail($"Recipe file was not found: {recipePath}"));
        }

        FamilyForgeRecipe? recipe;
        try
        {
            var json = File.ReadAllText(recipePath);
#if NET48
            recipe = new JavaScriptSerializer().Deserialize<FamilyForgeRecipe>(json);
#else
            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            serializerOptions.Converters.Add(new SingleOrArrayJsonConverterFactory());
            recipe = JsonSerializer.Deserialize<FamilyForgeRecipe>(
                json,
                serializerOptions);
#endif
        }
        catch (Exception ex)
        {
            return new RecipeLoadResult(
                null,
                FamilyForgeBuildResult.Fail(
                    "Recipe could not be read: "
                    + ex.Message
                    + " Check that top-level collections such as parameters, referencePlaneStrategy, parameterStrategy, nestedFamilies, publishingQa, referencePlanes, materials, geometry, constraints, assumptions, and clarifyingQuestions are JSON arrays."));
        }

        if (recipe is null)
        {
            return new RecipeLoadResult(
                null,
                FamilyForgeBuildResult.Fail("Recipe file did not contain a valid recipe."));
        }

        var validator = new FamilyForgeRecipePreflight();
        var preflight = validator.Validate(recipe);
        if (!preflight.CanBuild)
        {
            return new RecipeLoadResult(recipe, preflight);
        }

        return new RecipeLoadResult(recipe, preflight);
    }

    private static FamilyForgeBuildResult BuildFamilyDocument(
        FamilyForgeRecipe recipe,
        UIApplication uiApplication,
        string recipePath,
        FamilyForgeBuildResult preflight)
    {
        var application = uiApplication.Application;
        var templatePath = FamilyForgeTemplateResolver.ResolveTemplatePath(
            recipe,
            application.VersionNumber);
        if (templatePath is null)
        {
            preflight.AddError(
                $"No supported family template was found for category '{recipe.Family.Category}' and Revit {application.VersionNumber}.");
            return preflight;
        }

        Document familyDocument;
        try
        {
            familyDocument = application.NewFamilyDocument(templatePath);
        }
        catch (Exception ex)
        {
            preflight.AddError($"Could not create family document: {ex.Message}");
            return preflight;
        }

        try
        {
            var builder = new FamilyForgeNativeBuilder(familyDocument, recipe);
            builder.Build();

            var outputPath = ResolveOutputFamilyPath(recipePath, recipe.Family.Name);
            SaveFamilyDocument(familyDocument, outputPath);

            var result = FamilyForgeBuildResult.Success(
                $"Created family '{recipe.Family.Name}' from recipe. Built {builder.BuiltGeometryCount} geometry item(s). Saved to: {outputPath}");

            foreach (var warning in preflight.Warnings)
            {
                result.AddWarning(warning);
            }

            foreach (var warning in builder.Warnings)
            {
                result.AddWarning(warning);
            }

            if (recipe.Qa.Status != "approved_for_build")
            {
                result.AddWarning(
                    $"Recipe QA status is '{recipe.Qa.Status}'. Review before client delivery.");
            }

            var qaReportPath = WriteQaReport(
                recipePath,
                recipe,
                result,
                templatePath,
                outputPath,
                builder.BuiltGeometryCount,
                builder.CreatedParameterCount,
                builder.CreatedReferencePlaneCount);
            var feedbackReportPath = WriteFeedbackReport(recipePath, recipe, result);
            result.SetArtifacts(outputPath, qaReportPath, feedbackReportPath);

            ActivateSavedFamily(uiApplication, familyDocument, outputPath, result);

            return result;
        }
        catch (Exception ex)
        {
            preflight.AddError($"Family build failed: {ex.Message}");
            return preflight;
        }
    }

    private static string ResolveOutputFamilyPath(string recipePath, string familyName)
    {
        var recipeDirectory = Path.GetDirectoryName(recipePath)
            ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var outputDirectory = Path.Combine(recipeDirectory, "generated");
        Directory.CreateDirectory(outputDirectory);

        return Path.Combine(outputDirectory, MakeSafeFileName(familyName) + ".rfa");
    }

    private static void SaveFamilyDocument(Document familyDocument, string outputPath)
    {
        var saveOptions = new SaveAsOptions
        {
            OverwriteExistingFile = true
        };

        familyDocument.SaveAs(outputPath, saveOptions);
    }

    private static void ActivateSavedFamily(
        UIApplication uiApplication,
        Document familyDocument,
        string outputPath,
        FamilyForgeBuildResult result)
    {
        try
        {
            familyDocument.Close(false);
        }
        catch (Exception ex)
        {
            result.AddWarning(
                $"Saved the family, but Revit did not close the temporary build document before activation: {ex.Message}");
        }

        try
        {
            uiApplication.OpenAndActivateDocument(outputPath);
            result.AddWarning("Opened the saved family file in Revit for review.");
        }
        catch (Exception ex)
        {
            result.AddWarning(
                $"Saved the family, but Revit could not activate it automatically: {ex.Message}");
        }
    }

    private static string MakeSafeFileName(string value)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var safe = new string(value
            .Select(character => invalidCharacters.Contains(character) ? '_' : character)
            .ToArray())
            .Trim();

        return string.IsNullOrWhiteSpace(safe) ? "SymetriFamilyForgeFamily" : safe;
    }

    private static string WriteQaReport(
        string recipePath,
        FamilyForgeRecipe recipe,
        FamilyForgeBuildResult preflight,
        string templatePath,
        string outputPath,
        int geometryCount,
        int parameterCount,
        int referencePlaneCount)
    {
        var reportPath = Path.ChangeExtension(recipePath, ".family-forge-qa.md");
        var lines = new List<string>
        {
            "# Symetri Family Forge Technical QA/QC Report",
            string.Empty,
            $"Family: {recipe.Family.Name}",
            $"Schema Version: {recipe.SchemaVersion}",
            $"Template: {templatePath}",
            $"Output Family: {outputPath}",
            $"Geometry Built: {geometryCount}",
            $"Family Parameters Created: {parameterCount}",
            $"Reference Planes Created: {referencePlaneCount}",
            $"Recipe QA Status: {recipe.Qa.Status}",
            string.Empty
        };

        if (recipe.FamilyStrategy is not null)
        {
            lines.Add("## Family Strategy");
            lines.Add($"- Template: {recipe.FamilyStrategy.Template ?? "Unspecified"}");
            lines.Add($"- LOD Target: {recipe.FamilyStrategy.LodTarget ?? "Unspecified"}");
            lines.Add($"- Category Reason: {recipe.FamilyStrategy.CategoryReason ?? "Unspecified"}");
            lines.Add($"- Hosting Reason: {recipe.FamilyStrategy.HostingReason ?? "Unspecified"}");
            lines.Add($"- Loadable Family Reason: {recipe.FamilyStrategy.LoadableFamilyReason ?? "Unspecified"}");
            lines.Add($"- Scheduling Intent: {recipe.FamilyStrategy.ScheduleTagNeeds ?? "Unspecified"}");
            lines.Add($"- Rendering Intent: {recipe.FamilyStrategy.RenderingNeeds ?? "Unspecified"}");
            lines.Add(string.Empty);
        }

        AddStrategySection(lines, "Reference Plane Strategy", recipe.ReferencePlaneStrategy);
        AddStrategySection(lines, "Parameter Strategy", recipe.ParameterStrategy);
        AddNestedFamilySection(lines, recipe.NestedFamilies);
        AddVisibilitySection(lines, recipe.VisibilityStrategy);
        AddStringListSection(lines, "Publishing QA", recipe.PublishingQa);

        var warnings = preflight.Warnings
            .Concat(recipe.Qa.Warnings)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        AddWarningSection(lines, "Open Questions", warnings, IsOpenQuestion);
        AddWarningSection(lines, "Builder Limitations", warnings, IsBuilderLimitation);
        AddWarningSection(lines, "Recipe Review Items", warnings, warning => !IsOpenQuestion(warning) && !IsBuilderLimitation(warning));
        lines.Add("## Assumptions");

        if (recipe.Assumptions.Count == 0)
        {
            lines.Add("- None");
        }
        else
        {
            lines.AddRange(recipe.Assumptions.Select(assumption => "- " + assumption));
        }

        File.WriteAllLines(reportPath, lines);
        return reportPath;
    }

    private static string WriteFeedbackReport(
        string recipePath,
        FamilyForgeRecipe recipe,
        FamilyForgeBuildResult result)
    {
        var reportPath = Path.ChangeExtension(recipePath, ".family-forge-feedback.md");
        var openQuestions = result.Warnings
            .Where(IsOpenQuestion)
            .Select(RemoveWarningPrefix)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var builderLimitations = result.Warnings
            .Where(IsBuilderLimitation)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var lines = new List<string>
        {
            "# Symetri Family Forge Interim Recipe Feedback",
            string.Empty,
            $"Family: {recipe.Family.Name}",
            $"Recipe QA Status: {recipe.Qa.Status}",
            string.Empty,
            "## Use This To Revise The AI Prompt Or JSON",
            "This is an interim preflight artifact. Long term, these items should be resolved in the browser recipe viewer before Revit creates an RFA.",
            string.Empty,
            "## Questions To Answer"
        };

        if (openQuestions.Count == 0)
        {
            lines.Add("- None");
        }
        else
        {
            lines.AddRange(openQuestions.Select(question => "- " + question));
        }

        lines.Add(string.Empty);
        lines.Add("## Suggested Prompt Additions");
        if (openQuestions.Count == 0)
        {
            lines.Add("- No clarifying questions were captured in this run.");
        }
        else
        {
            foreach (var question in openQuestions)
            {
                lines.Add($"- Resolve before JSON export: {question}");
            }
        }

        if (recipe.NestedFamilies.Count > 0)
        {
            foreach (var nestedFamily in recipe.NestedFamilies.Where(item => item.Status is "recommended" or "required"))
            {
                lines.Add(
                    $"- Decide whether `{nestedFamily.Name}` should remain simplified geometry or become a nested family. Purpose: {nestedFamily.Purpose}");
            }
        }

        lines.Add(string.Empty);
        lines.Add("## Builder Gaps To Track Separately");
        if (builderLimitations.Count == 0)
        {
            lines.Add("- None");
        }
        else
        {
            lines.AddRange(builderLimitations.Select(warning => "- " + RemoveWarningPrefix(warning)));
        }

        lines.Add(string.Empty);
        lines.Add("## Current Publishing QA");
        lines.AddRange(recipe.PublishingQa.Count == 0
            ? new[] { "- None" }
            : recipe.PublishingQa.Select(item => "- " + item));

        File.WriteAllLines(reportPath, lines);
        return reportPath;
    }

    private static void AddWarningSection(
        List<string> lines,
        string title,
        IReadOnlyList<string> warnings,
        Func<string, bool> predicate)
    {
        lines.Add("## " + title);
        var matchingWarnings = warnings
            .Where(predicate)
            .Select(RemoveWarningPrefix)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (matchingWarnings.Count == 0)
        {
            lines.Add("- None");
        }
        else
        {
            lines.AddRange(matchingWarnings.Select(warning => "- " + warning));
        }

        lines.Add(string.Empty);
    }

    private static bool IsOpenQuestion(string warning)
    {
        return warning.StartsWith("Open question:", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBuilderLimitation(string warning)
    {
        return ContainsIgnoreCase(warning, "not implemented")
            || ContainsIgnoreCase(warning, "not yet")
            || ContainsIgnoreCase(warning, "current builder")
            || ContainsIgnoreCase(warning, "not associated")
            || ContainsIgnoreCase(warning, "not locked");
    }

    private static bool ContainsIgnoreCase(string value, string searchText)
    {
        return value.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string RemoveWarningPrefix(string warning)
    {
        const string openQuestionPrefix = "Open question:";
        return warning.StartsWith(openQuestionPrefix, StringComparison.OrdinalIgnoreCase)
            ? warning.Substring(openQuestionPrefix.Length).Trim()
            : warning;
    }

    private static void AddStrategySection(List<string> lines, string title, IReadOnlyList<StrategyItemSpec> items)
    {
        lines.Add("## " + title);
        if (items.Count == 0)
        {
            lines.Add("- None");
        }
        else
        {
            foreach (var item in items)
            {
                var drives = string.IsNullOrWhiteSpace(item.Drives) ? string.Empty : $" Drives: {item.Drives}";
                var notes = string.IsNullOrWhiteSpace(item.Notes) ? string.Empty : $" Notes: {item.Notes}";
                lines.Add($"- {item.Name}: {item.Intent}{drives}{notes}");
            }
        }

        lines.Add(string.Empty);
    }

    private static void AddNestedFamilySection(List<string> lines, IReadOnlyList<NestedFamilySpec> nestedFamilies)
    {
        lines.Add("## Nested Family Candidates");
        if (nestedFamilies.Count == 0)
        {
            lines.Add("- None");
        }
        else
        {
            foreach (var nestedFamily in nestedFamilies)
            {
                var mappedParameters = nestedFamily.ParametersToMap.Count == 0
                    ? "none listed"
                    : string.Join(", ", nestedFamily.ParametersToMap);
                lines.Add(
                    $"- {nestedFamily.Name} ({nestedFamily.Status}): {nestedFamily.Purpose} Parameters to map: {mappedParameters}.");
            }
        }

        lines.Add(string.Empty);
    }

    private static void AddVisibilitySection(List<string> lines, VisibilityStrategySpec? visibilityStrategy)
    {
        lines.Add("## Visibility Strategy");
        if (visibilityStrategy is null)
        {
            lines.Add("- None");
        }
        else
        {
            lines.Add($"- Coarse: {visibilityStrategy.Coarse ?? "Unspecified"}");
            lines.Add($"- Medium: {visibilityStrategy.Medium ?? "Unspecified"}");
            lines.Add($"- Fine: {visibilityStrategy.Fine ?? "Unspecified"}");
            lines.Add($"- Plan/RCP: {visibilityStrategy.PlanRcp ?? "Unspecified"}");
            lines.Add("- Subcategories: " + (visibilityStrategy.Subcategories.Count == 0
                ? "None"
                : string.Join(", ", visibilityStrategy.Subcategories)));
        }

        lines.Add(string.Empty);
    }

    private static void AddStringListSection(List<string> lines, string title, IReadOnlyList<string> items)
    {
        lines.Add("## " + title);
        lines.AddRange(items.Count == 0
            ? new[] { "- None" }
            : items.Select(item => "- " + item));
        lines.Add(string.Empty);
    }
}

internal sealed class RecipeLoadResult
{
    public RecipeLoadResult(FamilyForgeRecipe? recipe, FamilyForgeBuildResult result)
    {
        Recipe = recipe;
        Result = result;
    }

    public FamilyForgeRecipe? Recipe { get; }
    public FamilyForgeBuildResult Result { get; }
}

internal static class FamilyForgeTemplateResolver
{
    public static string? ResolveTemplatePath(FamilyForgeRecipe recipe, string revitVersion)
    {
        var basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Autodesk",
            "RVT " + revitVersion,
            "Family Templates",
            "English");

        var fileName = recipe.Family.Category switch
        {
            "Furniture" => "Metric Furniture.rft",
            "Casework" => "Metric Casework.rft",
            "Specialty Equipment" => "Metric Specialty Equipment.rft",
            _ => "Metric Generic Model.rft"
        };

        var path = Path.Combine(basePath, fileName);
        if (File.Exists(path))
        {
            return path;
        }

        var fallback = Path.Combine(basePath, "Metric Generic Model.rft");
        return File.Exists(fallback) ? fallback : null;
    }
}

internal sealed class FamilyForgeNativeBuilder
{
    private const double FeetPerMillimeter = 1.0 / 304.8;
    private const double FeetPerInch = 1.0 / 12.0;

    private readonly Document _document;
    private readonly FamilyForgeRecipe _recipe;
    private readonly Dictionary<string, double> _lengthParameters;
    private readonly Dictionary<string, ElementId> _materials = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, FamilyParameter> _familyParameters = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Category> _subcategories = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _referencePlaneNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _warnings = new();
    private readonly double _familyWidth;
    private readonly double _familyDepth;

    public FamilyForgeNativeBuilder(Document document, FamilyForgeRecipe recipe)
    {
        _document = document;
        _recipe = recipe;
        _lengthParameters = recipe.Parameters
            .Where(parameter => string.Equals(parameter.DataType, "length", StringComparison.OrdinalIgnoreCase))
            .GroupBy(parameter => parameter.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => ConvertLength(group.First().Value, recipe.Family.Units),
                StringComparer.OrdinalIgnoreCase);
        _familyWidth = ResolveLengthParameter("Width", 1000);
        _familyDepth = ResolveLengthParameter("Depth", 500);
    }

    public IReadOnlyList<string> Warnings => _warnings;
    public int BuiltGeometryCount { get; private set; }
    public int CreatedParameterCount { get; private set; }
    public int CreatedReferencePlaneCount { get; private set; }

    public void Build()
    {
        using var transaction = new Transaction(_document, "Build Symetri Family Forge Recipe");
        transaction.Start();

        CreateMaterials();
        CreateFamilyParameters();
        CreateReferencePlanes();
        CreateGeometry();
        AddStrategyWarnings();

        transaction.Commit();
    }

    private void CreateFamilyParameters()
    {
        var manager = _document.FamilyManager;
        EnsureFamilyType(manager);
        CacheExistingFamilyParameters(manager);

        foreach (var parameterSpec in _recipe.Parameters)
        {
            CreateFamilyParameter(manager, parameterSpec);
        }

        foreach (var materialSpec in _recipe.Materials)
        {
            if (string.IsNullOrWhiteSpace(materialSpec.ParameterName))
            {
                continue;
            }

            var parameterSpec = new FamilyParameterSpec
            {
                Name = materialSpec.ParameterName,
                DataType = "material",
                Value = materialSpec.Name,
                Group = "Materials",
                IsInstance = false,
                Source = "recipe.materials"
            };
            CreateFamilyParameter(manager, parameterSpec);
        }
    }

    private void EnsureFamilyType(FamilyManager manager)
    {
        if (manager.CurrentType is not null)
        {
            return;
        }

        try
        {
            manager.NewType(MakeRevitSafeName(_recipe.Family.Name));
        }
        catch (Exception ex)
        {
            _warnings.Add($"Could not create a named family type before parameter creation: {ex.Message}");
        }
    }

    private void CacheExistingFamilyParameters(FamilyManager manager)
    {
        foreach (FamilyParameter parameter in manager.Parameters)
        {
            if (!string.IsNullOrWhiteSpace(parameter.Definition.Name))
            {
                _familyParameters[parameter.Definition.Name] = parameter;
            }
        }
    }

    private void CreateFamilyParameter(FamilyManager manager, FamilyParameterSpec parameterSpec)
    {
        if (string.IsNullOrWhiteSpace(parameterSpec.Name))
        {
            return;
        }

        if (_familyParameters.TryGetValue(parameterSpec.Name, out var existingParameter))
        {
            TrySetFamilyParameterValue(manager, existingParameter, parameterSpec);
            return;
        }

        if (!TryAddFamilyParameter(manager, parameterSpec, out var parameter))
        {
            return;
        }

        _familyParameters[parameterSpec.Name] = parameter;
        CreatedParameterCount++;
        TrySetFamilyParameterValue(manager, parameter, parameterSpec);
    }

    private bool TryAddFamilyParameter(
        FamilyManager manager,
        FamilyParameterSpec parameterSpec,
        out FamilyParameter parameter)
    {
        parameter = null!;

        try
        {
            if (!TryMapParameterType(parameterSpec, out var groupTypeId, out var specTypeId))
            {
                _warnings.Add(
                    $"Skipped family parameter '{parameterSpec.Name}' because data type '{parameterSpec.DataType}' is not implemented in this builder.");
                return false;
            }

            parameter = manager.AddParameter(
                parameterSpec.Name,
                groupTypeId,
                specTypeId,
                parameterSpec.IsInstance);
            return true;
        }
        catch (Exception ex)
        {
            _warnings.Add($"Could not create family parameter '{parameterSpec.Name}': {ex.Message}");
            return false;
        }
    }

    private void TrySetFamilyParameterValue(
        FamilyManager manager,
        FamilyParameter parameter,
        FamilyParameterSpec parameterSpec)
    {
        if (parameterSpec.Value is null)
        {
            return;
        }

        try
        {
            if (string.Equals(parameterSpec.DataType, "length", StringComparison.OrdinalIgnoreCase))
            {
                manager.Set(parameter, ConvertLength(parameterSpec.Value, _recipe.Family.Units));
            }
            else if (string.Equals(parameterSpec.DataType, "material", StringComparison.OrdinalIgnoreCase)
                     && _materials.TryGetValue(Convert.ToString(parameterSpec.Value, CultureInfo.InvariantCulture) ?? string.Empty, out var materialId))
            {
                manager.Set(parameter, materialId);
            }
        }
        catch (Exception ex)
        {
            _warnings.Add($"Could not set value for family parameter '{parameterSpec.Name}': {ex.Message}");
        }
    }

    private static bool TryMapParameterType(
        FamilyParameterSpec parameterSpec,
        out ForgeTypeId groupTypeId,
        out ForgeTypeId specTypeId)
    {
        groupTypeId = MapParameterGroup(parameterSpec.Group, parameterSpec.DataType);
        specTypeId = parameterSpec.DataType.Trim().ToLowerInvariant() switch
        {
            "length" => SpecTypeId.Length,
            "material" => SpecTypeId.Reference.Material,
            _ => new ForgeTypeId()
        };

        return specTypeId != new ForgeTypeId();
    }

    private static ForgeTypeId MapParameterGroup(string group, string dataType)
    {
        if (string.Equals(dataType, "material", StringComparison.OrdinalIgnoreCase))
        {
            return GroupTypeId.Materials;
        }

        return group.Trim().ToLowerInvariant() switch
        {
            "constraints" => GroupTypeId.Constraints,
            "materials" => GroupTypeId.Materials,
            "identity data" => GroupTypeId.IdentityData,
            _ => GroupTypeId.Geometry
        };
    }

    private void CreateReferencePlanes()
    {
        var view = FindReferencePlaneView();
        if (view is null)
        {
            _warnings.Add("Could not create recipe reference planes because no usable family view was found.");
            return;
        }

        foreach (var referencePlane in _recipe.ReferencePlanes)
        {
            CreateReferencePlane(referencePlane, view);
        }
    }

    private Autodesk.Revit.DB.View? FindReferencePlaneView()
    {
        return new FilteredElementCollector(_document)
            .OfClass(typeof(Autodesk.Revit.DB.View))
            .Cast<Autodesk.Revit.DB.View>()
            .Where(view => !view.IsTemplate)
            .OrderBy(view => view.ViewType == Autodesk.Revit.DB.ViewType.FloorPlan ? 0 : 1)
            .FirstOrDefault(view => view.ViewType is Autodesk.Revit.DB.ViewType.FloorPlan
                or Autodesk.Revit.DB.ViewType.CeilingPlan
                or Autodesk.Revit.DB.ViewType.Elevation
                or Autodesk.Revit.DB.ViewType.ThreeD);
    }

    private void CreateReferencePlane(ReferencePlaneSpec referencePlaneSpec, Autodesk.Revit.DB.View view)
    {
        if (string.IsNullOrWhiteSpace(referencePlaneSpec.Name))
        {
            return;
        }

        if (!_referencePlaneNames.Add(referencePlaneSpec.Name))
        {
            _warnings.Add($"Skipped duplicate reference plane '{referencePlaneSpec.Name}'.");
            return;
        }

        var recipeOffset = ResolveLength(referencePlaneSpec.Offset, "referencePlane.offset", referencePlaneSpec.Name);
        var width = _familyWidth;
        var depth = _familyDepth;

        var orientation = referencePlaneSpec.Orientation.Trim().ToLowerInvariant();
        var bubbleEnd = XYZ.Zero;
        var freeEnd = XYZ.BasisX;
        var cutVector = XYZ.BasisZ;

        switch (orientation)
        {
            case "leftright":
                var xOffset = CenterX(recipeOffset);
                bubbleEnd = new XYZ(xOffset, -depth / 2.0, 0);
                freeEnd = new XYZ(xOffset, depth / 2.0, 0);
                cutVector = XYZ.BasisZ;
                break;
            case "frontback":
                var yOffset = CenterY(recipeOffset);
                bubbleEnd = new XYZ(-width / 2.0, yOffset, 0);
                freeEnd = new XYZ(width / 2.0, yOffset, 0);
                cutVector = XYZ.BasisZ;
                break;
            case "vertical":
                bubbleEnd = new XYZ(-width / 2.0, 0, recipeOffset);
                freeEnd = new XYZ(width / 2.0, 0, recipeOffset);
                cutVector = XYZ.BasisY;
                break;
            default:
                _warnings.Add(
                    $"Skipped reference plane '{referencePlaneSpec.Name}' because orientation '{referencePlaneSpec.Orientation}' is not supported.");
                return;
        }

        try
        {
            var referencePlane = _document.FamilyCreate.NewReferencePlane(
                bubbleEnd,
                freeEnd,
                cutVector,
                view);

            referencePlane.Name = MakeRevitSafeName(referencePlaneSpec.Name);
            CreatedReferencePlaneCount++;
        }
        catch (Exception ex)
        {
            _warnings.Add($"Could not create reference plane '{referencePlaneSpec.Name}': {ex.Message}");
        }
    }

    private double ResolveLengthParameter(string parameterName, double fallbackMillimeters)
    {
        return _lengthParameters.TryGetValue(parameterName, out var value)
            ? value
            : ConvertLength(fallbackMillimeters, "mm");
    }

    private double CenterX(double recipeX)
    {
        return recipeX - (_familyWidth / 2.0);
    }

    private double CenterY(double recipeY)
    {
        return recipeY - (_familyDepth / 2.0);
    }

    private void CreateMaterials()
    {
        foreach (var materialSpec in _recipe.Materials)
        {
            var materialId = FindMaterialId(materialSpec.Name);
            if (materialId == ElementId.InvalidElementId)
            {
                materialId = Material.Create(_document, materialSpec.Name);
            }

            var material = _document.GetElement(materialId) as Material;
            if (material is not null)
            {
                material.Color = ParseColor(materialSpec.Color);
            }

            _materials[materialSpec.Name] = materialId;
        }
    }

    private void AddStrategyWarnings()
    {
        if (_recipe.ReferencePlaneStrategy.Count > 0 && CreatedReferencePlaneCount > 0 && BuiltGeometryCount > 0)
        {
            _warnings.Add(
                "Created recipe reference planes, but geometry is not locked/aligned to those planes yet. Treat generated solids as first-pass authored geometry.");
        }

        foreach (var nestedFamily in _recipe.NestedFamilies)
        {
            if (nestedFamily.Status is "recommended" or "required")
            {
                _warnings.Add(
                    $"Nested family '{nestedFamily.Name}' is {nestedFamily.Status} but is not created or placed by the current builder. Purpose: {nestedFamily.Purpose}");
            }
        }

        if (_recipe.ParameterStrategy.Count > 0 && CreatedParameterCount > 0)
        {
            _warnings.Add(
                "Created family parameters from the recipe, but geometry dimensions are not yet associated to those parameters through Revit constraints.");
        }

        if (_recipe.ReferencePlanes.Any(referencePlane => referencePlane.IsStrongReference))
        {
            _warnings.Add(
                "Reference planes were created and named, but strong-reference assignment is not implemented for this Revit API target yet.");
        }

        _warnings.Add(
            "Centered recipe X/Y coordinates on the Revit family origin so default Center Left/Right and Center Front/Back planes remain meaningful.");
        _warnings.Add(
            "Rectangular extrusions are oriented by thinnest dimension so panels use XY, XZ, or YZ sketch planes instead of always extruding from Ref. Level.");
    }

    private void CreateGeometry()
    {
        foreach (var geometry in _recipe.Geometry)
        {
            switch (geometry.Type)
            {
                case "rectangularExtrusion":
                    CreateRectangularExtrusion(geometry);
                    break;
                case "cylinder":
                    CreateCylinder(geometry);
                    break;
                default:
                    _warnings.Add(
                        $"Skipped geometry '{geometry.Id}' because type '{geometry.Type}' is not implemented in the current builder.");
                    break;
            }
        }
    }

    private void CreateRectangularExtrusion(GeometrySpec geometry)
    {
        var origin = ResolvePoint(geometry.Origin);
        var width = ResolveLength(geometry.Dimensions.Width, "width", geometry.Id);
        var depth = ResolveLength(geometry.Dimensions.Depth, "depth", geometry.Id);
        var height = ResolveLength(geometry.Dimensions.Height, "height", geometry.Id);

        if (width <= 0 || depth <= 0 || height <= 0)
        {
            _warnings.Add(
                $"Skipped geometry '{geometry.Id}' because one or more dimensions were not greater than zero.");
            return;
        }

        var extrusionLayout = GetRectangularExtrusionLayout(origin, width, depth, height);
        var profile = CreateRectangularProfile(
            extrusionLayout.Origin,
            extrusionLayout.ProfileXAxis,
            extrusionLayout.ProfileYAxis,
            extrusionLayout.ProfileWidth,
            extrusionLayout.ProfileHeight);

        var profileArray = new CurveArrArray();
        profileArray.Append(profile);

        var sketchPlane = SketchPlane.Create(
            _document,
            Plane.CreateByNormalAndOrigin(extrusionLayout.ExtrusionAxis, extrusionLayout.Origin));

        var extrusion = _document.FamilyCreate.NewExtrusion(
            true,
            profileArray,
            sketchPlane,
            extrusionLayout.ExtrusionDepth);

        FinishForm(extrusion, geometry);
    }

    private void CreateCylinder(GeometrySpec geometry)
    {
        var axis = string.IsNullOrWhiteSpace(geometry.Axis)
            ? "z"
            : geometry.Axis.Trim().ToLowerInvariant();
        var origin = ResolvePoint(geometry.Origin);
        var width = ResolveLength(geometry.Dimensions.Width, "width", geometry.Id);
        var depth = ResolveLength(geometry.Dimensions.Depth, "depth", geometry.Id);
        var height = ResolveLength(geometry.Dimensions.Height, "height", geometry.Id);

        var length = axis switch
        {
            "x" => width,
            "y" => depth,
            _ => height
        };
        var diameter = axis switch
        {
            "x" => Math.Min(depth, height),
            "y" => Math.Min(width, height),
            _ => Math.Min(width, depth)
        };

        if (length <= 0 || diameter <= 0)
        {
            _warnings.Add(
                $"Skipped cylinder '{geometry.Id}' because length or diameter was not greater than zero.");
            return;
        }

        var radius = diameter / 2.0;
        var normal = AxisNormal(axis);
        var xAxis = AxisProfileX(axis);
        var yAxis = AxisProfileY(axis);
        var profile = new CurveArray();
        profile.Append(Arc.Create(origin, radius, 0, Math.PI, xAxis, yAxis));
        profile.Append(Arc.Create(origin, radius, Math.PI, Math.PI * 2.0, xAxis, yAxis));

        var profileArray = new CurveArrArray();
        profileArray.Append(profile);

        var sketchPlane = SketchPlane.Create(
            _document,
            Plane.CreateByNormalAndOrigin(normal, origin));

        var extrusion = _document.FamilyCreate.NewExtrusion(
            true,
            profileArray,
            sketchPlane,
            length);

        FinishForm(extrusion, geometry);
    }

    private void FinishForm(GenericForm form, GeometrySpec geometry)
    {
        ApplyMaterial(form, geometry.Material);
        ApplySubcategory(form, geometry.Subcategory);
        BuiltGeometryCount++;
    }

    private static XYZ AxisNormal(string axis)
    {
        return axis switch
        {
            "x" => XYZ.BasisX,
            "y" => XYZ.BasisY,
            _ => XYZ.BasisZ
        };
    }

    private static XYZ AxisProfileX(string axis)
    {
        return axis switch
        {
            "x" => XYZ.BasisY,
            "y" => XYZ.BasisX,
            _ => XYZ.BasisX
        };
    }

    private static XYZ AxisProfileY(string axis)
    {
        return axis switch
        {
            "x" => XYZ.BasisZ,
            "y" => XYZ.BasisZ,
            _ => XYZ.BasisY
        };
    }

    private ElementId FindMaterialId(string materialName)
    {
        var existingMaterial = new FilteredElementCollector(_document)
            .OfClass(typeof(Material))
            .Cast<Material>()
            .FirstOrDefault(material => string.Equals(
                material.Name,
                materialName,
                StringComparison.OrdinalIgnoreCase));

        return existingMaterial?.Id ?? ElementId.InvalidElementId;
    }

    private static RectangularExtrusionLayout GetRectangularExtrusionLayout(
        XYZ origin,
        double width,
        double depth,
        double height)
    {
        if (width <= depth && width <= height)
        {
            return new RectangularExtrusionLayout(
                origin,
                XYZ.BasisX,
                XYZ.BasisY,
                XYZ.BasisZ,
                depth,
                height,
                width);
        }

        if (depth <= width && depth <= height)
        {
            return new RectangularExtrusionLayout(
                origin,
                XYZ.BasisY,
                XYZ.BasisX,
                XYZ.BasisZ,
                width,
                height,
                depth);
        }

        return new RectangularExtrusionLayout(
            origin,
            XYZ.BasisZ,
            XYZ.BasisX,
            XYZ.BasisY,
            width,
            depth,
            height);
    }

    private static CurveArray CreateRectangularProfile(
        XYZ origin,
        XYZ xAxis,
        XYZ yAxis,
        double width,
        double height)
    {
        var profile = new CurveArray();
        var p1 = origin;
        var p2 = origin + (xAxis * width);
        var p3 = origin + (xAxis * width) + (yAxis * height);
        var p4 = origin + (yAxis * height);

        profile.Append(Line.CreateBound(p1, p2));
        profile.Append(Line.CreateBound(p2, p3));
        profile.Append(Line.CreateBound(p3, p4));
        profile.Append(Line.CreateBound(p4, p1));
        return profile;
    }

    private readonly struct RectangularExtrusionLayout
    {
        public RectangularExtrusionLayout(
            XYZ origin,
            XYZ extrusionAxis,
            XYZ profileXAxis,
            XYZ profileYAxis,
            double profileWidth,
            double profileHeight,
            double extrusionDepth)
        {
            Origin = origin;
            ExtrusionAxis = extrusionAxis;
            ProfileXAxis = profileXAxis;
            ProfileYAxis = profileYAxis;
            ProfileWidth = profileWidth;
            ProfileHeight = profileHeight;
            ExtrusionDepth = extrusionDepth;
        }

        public XYZ Origin { get; }
        public XYZ ExtrusionAxis { get; }
        public XYZ ProfileXAxis { get; }
        public XYZ ProfileYAxis { get; }
        public double ProfileWidth { get; }
        public double ProfileHeight { get; }
        public double ExtrusionDepth { get; }
    }

    private static string MakeRevitSafeName(string value)
    {
        var safe = new string(value
            .Select(character => char.IsLetterOrDigit(character) || character is ' ' or '_' or '-'
                ? character
                : '_')
            .ToArray())
            .Trim();

        return string.IsNullOrWhiteSpace(safe) ? "Family Forge Geometry" : safe;
    }

    private void ApplyMaterial(Element element, string materialName)
    {
        if (!_materials.TryGetValue(materialName, out var materialId))
        {
            _warnings.Add(
                $"Material '{materialName}' was not found for one generated element.");
            return;
        }

        var parameter = element.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
        if (parameter is { IsReadOnly: false })
        {
            parameter.Set(materialId);
        }
    }

    private void ApplySubcategory(GenericForm form, string? subcategoryName)
    {
        if (string.IsNullOrWhiteSpace(subcategoryName))
        {
            return;
        }

        if (!_subcategories.TryGetValue(subcategoryName, out var subcategory))
        {
            var parentCategory = _document.OwnerFamily.FamilyCategory;
            subcategory = parentCategory.SubCategories
                .Cast<Category>()
                .FirstOrDefault(category => string.Equals(
                    category.Name,
                    subcategoryName,
                    StringComparison.OrdinalIgnoreCase))
                ?? _document.Settings.Categories.NewSubcategory(
                    parentCategory,
                    subcategoryName);
            _subcategories[subcategoryName] = subcategory;
        }

        form.Subcategory = subcategory;
    }

    private XYZ ResolvePoint(PointSpec point)
    {
        return new XYZ(
            CenterX(ResolveLength(point.X, "origin.x", "point")),
            CenterY(ResolveLength(point.Y, "origin.y", "point")),
            ResolveLength(point.Z, "origin.z", "point"));
    }

    private double ResolveLength(object? value, string label, string geometryId)
    {
        if (TryGetNumber(value, out var numericValue))
        {
            return ConvertLength(numericValue, _recipe.Family.Units);
        }

        var expression = Convert.ToString(value, CultureInfo.InvariantCulture);
        if (!string.IsNullOrWhiteSpace(expression)
            && _lengthParameters.TryGetValue(expression, out var parameterValue))
        {
            return parameterValue;
        }

        _warnings.Add(
            $"Could not resolve {label} value '{expression}' for geometry '{geometryId}'. Using 0.");
        return 0;
    }

    private static double ConvertLength(object? value, string units)
    {
        if (!TryGetNumber(value, out var numericValue))
        {
            return 0;
        }

        return ConvertLength(numericValue, units);
    }

    private static double ConvertLength(double value, string units)
    {
        return string.Equals(units, "in", StringComparison.OrdinalIgnoreCase)
            ? value * FeetPerInch
            : value * FeetPerMillimeter;
    }

    private static bool TryGetNumber(object? value, out double result)
    {
        result = 0;

        if (value is null)
        {
            return false;
        }

        switch (value)
        {
            case double doubleValue:
                result = doubleValue;
                return true;
            case float floatValue:
                result = floatValue;
                return true;
            case decimal decimalValue:
                result = (double)decimalValue;
                return true;
            case int intValue:
                result = intValue;
                return true;
            case long longValue:
                result = longValue;
                return true;
        }

#if !NET48
        if (value is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Number)
            {
                return jsonElement.TryGetDouble(out result);
            }

            if (jsonElement.ValueKind == JsonValueKind.String
                && double.TryParse(
                    jsonElement.GetString(),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out result))
            {
                return true;
            }
        }
#endif

        return double.TryParse(
            Convert.ToString(value, CultureInfo.InvariantCulture),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out result);
    }

    private static Autodesk.Revit.DB.Color ParseColor(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex) || hex.Length != 7 || hex[0] != '#')
        {
            return new Autodesk.Revit.DB.Color(184, 184, 184);
        }

        return new Autodesk.Revit.DB.Color(
            byte.Parse(hex.Substring(1, 2), NumberStyles.HexNumber),
            byte.Parse(hex.Substring(3, 2), NumberStyles.HexNumber),
            byte.Parse(hex.Substring(5, 2), NumberStyles.HexNumber));
    }
}

#if !NET48
internal sealed class SingleOrArrayJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType
            && typeToConvert.GetGenericTypeDefinition() == typeof(List<>);
    }

    public override JsonConverter CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var itemType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(SingleOrArrayJsonConverter<>).MakeGenericType(itemType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

internal sealed class SingleOrArrayJsonConverter<T> : JsonConverter<List<T>>
{
    public override List<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return new List<T>();
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            return JsonSerializer.Deserialize<List<T>>(ref reader, options) ?? new List<T>();
        }

        var item = JsonSerializer.Deserialize<T>(ref reader, options);
        return item is null ? new List<T>() : new List<T> { item };
    }

    public override void Write(
        Utf8JsonWriter writer,
        List<T> value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (IEnumerable<T>)value, options);
    }
}
#endif

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
