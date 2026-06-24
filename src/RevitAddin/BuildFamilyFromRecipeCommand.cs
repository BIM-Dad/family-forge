using System;
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
        var lines = new[]
        {
            result.Message,
            string.Empty,
            result.Errors.Any() ? "Errors:" : string.Empty,
            string.Join(Environment.NewLine, result.Errors.Select(item => "- " + item)),
            string.Empty,
            result.Warnings.Any() ? "Warnings:" : "No warnings.",
            string.Join(Environment.NewLine, result.Warnings.Select(item => "- " + item))
        };

        return string.Join(
            Environment.NewLine,
            lines.Where(line => !string.IsNullOrWhiteSpace(line)));
    }
}
