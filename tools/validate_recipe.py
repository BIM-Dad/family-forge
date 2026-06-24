#!/usr/bin/env python3
"""Validate a Symetri Family Forge recipe.

This script intentionally uses only the Python standard library for the
cross-field checks. If the optional `jsonschema` package is installed, it also
validates against the schema file.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[1]
DEFAULT_SCHEMA = ROOT / "schema" / "family-recipe.schema.json"


def load_json(path: Path) -> dict[str, Any]:
    with path.open("r", encoding="utf-8") as handle:
        data = json.load(handle)
    if not isinstance(data, dict):
        raise ValueError(f"{path} must contain a JSON object.")
    return data


def validate_json_schema(recipe: dict[str, Any], schema_path: Path) -> list[str]:
    try:
        import jsonschema  # type: ignore
    except ImportError:
        return [
            "Optional package 'jsonschema' is not installed; skipped formal schema validation."
        ]

    schema = load_json(schema_path)
    validator = jsonschema.Draft202012Validator(schema)
    errors = sorted(validator.iter_errors(recipe), key=lambda item: list(item.path))

    messages: list[str] = []
    for error in errors:
        location = ".".join(str(part) for part in error.path) or "<root>"
        messages.append(f"{location}: {error.message}")
    return messages


def collect_cross_field_warnings(recipe: dict[str, Any]) -> list[str]:
    warnings: list[str] = []

    parameters = recipe.get("parameters", [])
    parameter_names = [item.get("name") for item in parameters if isinstance(item, dict)]
    parameter_name_set = {name for name in parameter_names if isinstance(name, str)}

    duplicates = sorted(
        name for name in parameter_name_set if parameter_names.count(name) > 1
    )
    for name in duplicates:
        warnings.append(f"Duplicate parameter name: {name}")

    required_dimensions = {"Width", "Depth", "Height"}
    missing_dimensions = sorted(required_dimensions - parameter_name_set)
    for name in missing_dimensions:
        warnings.append(f"Missing recommended dimension parameter: {name}")

    for parameter in parameters:
        if not isinstance(parameter, dict):
            continue
        name = parameter.get("name", "<unnamed>")
        source = parameter.get("source")
        value = parameter.get("value")
        if source in {"inferred", "defaulted"}:
            warnings.append(f"Parameter '{name}' is {source} and needs review.")
        if parameter.get("dataType") == "length" and isinstance(value, (int, float)):
            if value <= 0:
                warnings.append(f"Length parameter '{name}' must be greater than zero.")

    materials = recipe.get("materials", [])
    material_names = {item.get("name") for item in materials if isinstance(item, dict)}

    for geometry in recipe.get("geometry", []):
        if not isinstance(geometry, dict):
            continue
        geometry_id = geometry.get("id", "<unknown>")
        material_name = geometry.get("material")
        geometry_type = geometry.get("type")
        axis = geometry.get("axis", "z")
        if material_name not in material_names:
            warnings.append(
                f"Geometry '{geometry_id}' references unknown material '{material_name}'."
            )
        if geometry_type == "cylinder" and axis not in {"x", "y", "z"}:
            warnings.append(
                f"Geometry '{geometry_id}' uses cylinder axis '{axis}', expected x, y, or z."
            )

        dimensions = geometry.get("dimensions", {})
        if isinstance(dimensions, dict):
            for axis in ("width", "depth", "height"):
                value = dimensions.get(axis)
                if isinstance(value, str) and not _is_expression_reference(
                    value, parameter_name_set
                ):
                    warnings.append(
                        f"Geometry '{geometry_id}' {axis} references unknown expression '{value}'."
                    )
                if isinstance(value, (int, float)) and value <= 0:
                    warnings.append(
                        f"Geometry '{geometry_id}' {axis} must be greater than zero."
                    )

    qa = recipe.get("qa", {})
    if isinstance(qa, dict) and qa.get("status") == "approved_for_build":
        inferred = [
            item.get("name")
            for item in parameters
            if isinstance(item, dict) and item.get("source") in {"inferred", "defaulted"}
        ]
        if inferred:
            warnings.append(
                "Recipe is approved_for_build but includes inferred/defaulted parameters: "
                + ", ".join(str(name) for name in inferred)
            )

    return warnings


def _is_expression_reference(value: str, parameter_names: set[str]) -> bool:
    if value in parameter_names:
        return True

    tokens = re.findall(r"[A-Za-z][A-Za-z0-9_ ]*", value)
    if not tokens:
        return False

    compact_names = {name.replace(" ", "") for name in parameter_names}
    compact_value = value.replace(" ", "")
    return any(name in compact_value for name in compact_names)


def main() -> int:
    parser = argparse.ArgumentParser(description="Validate a Family Forge recipe.")
    parser.add_argument("recipe", type=Path, help="Path to family recipe JSON.")
    parser.add_argument(
        "--schema",
        type=Path,
        default=DEFAULT_SCHEMA,
        help="Path to family recipe JSON schema.",
    )
    args = parser.parse_args()

    try:
        recipe = load_json(args.recipe)
    except Exception as exc:
        print(f"ERROR: Could not read recipe: {exc}", file=sys.stderr)
        return 2

    schema_messages = validate_json_schema(recipe, args.schema)
    cross_field_warnings = collect_cross_field_warnings(recipe)

    errors = [
        message
        for message in schema_messages
        if not message.startswith("Optional package")
    ]
    notices = [
        message
        for message in schema_messages
        if message.startswith("Optional package")
    ]

    if errors:
        print("Schema validation failed:")
        for message in errors:
            print(f"- {message}")
        return 1

    print("Schema validation passed.")

    for notice in notices:
        print(f"Notice: {notice}")

    if cross_field_warnings:
        print("BIM QA warnings:")
        for warning in cross_field_warnings:
            print(f"- {warning}")
    else:
        print("No BIM QA warnings found.")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
