using System.Collections.Generic;
using System.Linq;

namespace Symetri.FamilyForge.RevitAddin;

public sealed class FamilyForgeBuildResult
{
    private readonly List<string> _errors = new();
    private readonly List<string> _warnings = new();

    public string Message { get; private set; }
    public string? OutputPath { get; private set; }
    public string? QaReportPath { get; private set; }
    public string? FeedbackReportPath { get; private set; }
    public IReadOnlyList<string> Errors => _errors;
    public IReadOnlyList<string> Warnings => _warnings;
    public bool CanBuild => !_errors.Any();

    private FamilyForgeBuildResult(string message)
    {
        Message = message;
    }

    public static FamilyForgeBuildResult Success(string message)
    {
        return new FamilyForgeBuildResult(message);
    }

    public static FamilyForgeBuildResult Fail(string message)
    {
        var result = new FamilyForgeBuildResult(message);
        result.AddError(message);
        return result;
    }

    public void AddError(string message)
    {
        _errors.Add(message);
    }

    public void AddWarning(string message)
    {
        _warnings.Add(message);
    }

    public void SetArtifacts(string outputPath, string qaReportPath, string feedbackReportPath)
    {
        OutputPath = outputPath;
        QaReportPath = qaReportPath;
        FeedbackReportPath = feedbackReportPath;
    }
}
