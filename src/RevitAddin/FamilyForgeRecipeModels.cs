using System.Collections.Generic;

namespace Symetri.FamilyForge.RevitAddin;

public sealed class FamilyForgeRecipe
{
    public string SchemaVersion { get; set; } = "0.1";
    public FamilyInfo Family { get; set; } = new();
    public List<FamilyParameterSpec> Parameters { get; set; } = new();
    public List<ReferencePlaneSpec> ReferencePlanes { get; set; } = new();
    public List<MaterialSpec> Materials { get; set; } = new();
    public List<GeometrySpec> Geometry { get; set; } = new();
    public List<ConstraintSpec> Constraints { get; set; } = new();
    public List<string> Assumptions { get; set; } = new();
    public List<string> ClarifyingQuestions { get; set; } = new();
    public QaSpec Qa { get; set; } = new();
}

public sealed class FamilyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Hosting { get; set; } = string.Empty;
    public string Units { get; set; } = "mm";
    public string Description { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
}

public sealed class FamilyParameterSpec
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public object? Value { get; set; }
    public string Group { get; set; } = string.Empty;
    public bool IsInstance { get; set; }
    public string? Formula { get; set; }
    public string? Source { get; set; }
}

public sealed class ReferencePlaneSpec
{
    public string Name { get; set; } = string.Empty;
    public string Orientation { get; set; } = string.Empty;
    public object? Offset { get; set; }
    public bool IsStrongReference { get; set; }
}

public sealed class MaterialSpec
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#B8B8B8";
    public string? ParameterName { get; set; }
}

public sealed class GeometrySpec
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Subcategory { get; set; }
    public string? Axis { get; set; }
    public string Material { get; set; } = string.Empty;
    public PointSpec Origin { get; set; } = new();
    public DimensionsSpec Dimensions { get; set; } = new();
    public bool Visible { get; set; } = true;
    public string? Notes { get; set; }
}

public sealed class PointSpec
{
    public object? X { get; set; }
    public object? Y { get; set; }
    public object? Z { get; set; }
}

public sealed class DimensionsSpec
{
    public object? Width { get; set; }
    public object? Depth { get; set; }
    public object? Height { get; set; }
}

public sealed class ConstraintSpec
{
    public string Type { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty;
}

public sealed class QaSpec
{
    public string Status { get; set; } = "draft";
    public List<string> Warnings { get; set; } = new();
}
