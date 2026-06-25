using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Forms = System.Windows.Forms;

namespace Symetri.FamilyForge.RevitAddin;

[Transaction(TransactionMode.Manual)]
public sealed class BuildFamilyFromRecipeCommand : IExternalCommand
{
    public Result Execute(
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements)
    {
        using var dialog = new Forms.OpenFileDialog
        {
            Title = "Select Symetri Family Forge recipe",
            Filter = "Family Forge recipe (*.json)|*.json|All files (*.*)|*.*",
            Multiselect = false
        };

        if (dialog.ShowDialog() != Forms.DialogResult.OK)
        {
            return Result.Cancelled;
        }

        var buildCommand = new FamilyForgeBuildCommand();
        var result = buildCommand.BuildFromRecipeFile(
            dialog.FileName,
            commandData.Application);

        var summary = FormatResult(result);
        Autodesk.Revit.UI.TaskDialog.Show("Symetri Family Forge", summary);

        if (!result.CanBuild)
        {
            message = string.Join(Environment.NewLine, result.Errors);
            return Result.Succeeded;
        }

        return Result.Succeeded;
    }

    private static string FormatResult(FamilyForgeBuildResult result)
    {
        var lines = new List<string>
        {
            result.Message,
            string.Empty
        };

        if (!string.IsNullOrWhiteSpace(result.OutputPath))
        {
            lines.Add("Output Family:");
            lines.Add(result.OutputPath);
            lines.Add(string.Empty);
        }

        if (!string.IsNullOrWhiteSpace(result.QaReportPath))
        {
            lines.Add("QA Report:");
            lines.Add(result.QaReportPath);
            lines.Add(string.Empty);
        }

        if (!string.IsNullOrWhiteSpace(result.FeedbackReportPath))
        {
            lines.Add("Interim Recipe Feedback Report:");
            lines.Add(result.FeedbackReportPath);
            lines.Add(string.Empty);
        }

        if (result.Errors.Any())
        {
            lines.Add("Errors:");
            lines.AddRange(result.Errors.Take(5).Select(item => "- " + item));
            lines.Add(string.Empty);
        }

        if (result.Warnings.Any())
        {
            lines.Add($"Warnings: {result.Warnings.Count}");
            lines.AddRange(result.Warnings.Take(5).Select(item => "- " + item));
            if (result.Warnings.Count > 5)
            {
                lines.Add($"- See reports for {result.Warnings.Count - 5} more warning(s).");
            }
        }
        else
        {
            lines.Add("No warnings.");
        }

        return string.Join(
            Environment.NewLine,
            lines.Where(line => !string.IsNullOrWhiteSpace(line)));
    }
}
