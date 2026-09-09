#!/usr/bin/env python3
"""Validate the T081 source record and exported GLB without third-party packages."""

from __future__ import annotations

import json
from pathlib import Path
import struct
import sys


ROOT = Path(__file__).resolve().parents[2]
RECORD = ROOT / "Content/Presentation/Assets/pipeline.reference.vehicle.asset.json"


def fail(message: str) -> None:
    print(f"M8.5 ASSET PIPELINE: FAIL {message}", file=sys.stderr)
    raise SystemExit(1)


def read_glb(path: Path) -> dict:
    data = path.read_bytes()
    if len(data) < 20 or data[:4] != b"glTF":
        fail("runtime asset is not a GLB file")
    version, total_length = struct.unpack_from("<II", data, 4)
    if version != 2 or total_length != len(data):
        fail(f"GLB header mismatch version={version} length={total_length}/{len(data)}")
    json_length, chunk_type = struct.unpack_from("<II", data, 12)
    if chunk_type != 0x4E4F534A:
        fail("first GLB chunk is not JSON")
    return json.loads(data[20:20 + json_length].decode("utf-8").rstrip(" \t\r\n\0"))


def node_map(gltf: dict) -> dict[str, dict]:
    result: dict[str, dict] = {}
    for node in gltf.get("nodes", []):
        name = node.get("name")
        if not name:
            fail("unnamed GLB node")
        if name in result:
            fail(f"duplicate GLB node name: {name}")
        result[name] = node
    return result


def triangle_count(gltf: dict, root_index: int) -> int:
    nodes = gltf["nodes"]
    accessors = gltf.get("accessors", [])
    meshes = gltf.get("meshes", [])
    total = 0
    pending = [root_index]
    while pending:
        node = nodes[pending.pop()]
        pending.extend(node.get("children", []))
        mesh_index = node.get("mesh")
        if mesh_index is None:
            continue
        for primitive in meshes[mesh_index].get("primitives", []):
            if primitive.get("mode", 4) != 4 or "indices" not in primitive:
                fail(f"non-triangle or unindexed primitive below {nodes[root_index].get('name')}")
            count = accessors[primitive["indices"]].get("count", 0)
            if count <= 0 or count % 3:
                fail(f"invalid triangle index count {count}")
            total += count // 3
    return total


def descendants(gltf: dict, root_index: int) -> list[dict]:
    nodes = gltf["nodes"]
    result: list[dict] = []
    pending = list(nodes[root_index].get("children", []))
    while pending:
        node = nodes[pending.pop()]
        result.append(node)
        pending.extend(node.get("children", []))
    return result


def main() -> None:
    if not RECORD.is_file():
        fail(f"missing sidecar {RECORD.relative_to(ROOT)}")
    record = json.loads(RECORD.read_text(encoding="utf-8"))
    if record.get("schemaVersion") != 1:
        fail("sidecar schema must be 1")
    if record.get("productionStatus") != "PIPELINE_FIXTURE_NOT_ROSTER_ART":
        fail("reference fixture could be mistaken for production roster art")
    if record.get("review", {}).get("gameDirectorAcceptance") != "ACCEPTED_2026-09-09":
        fail("T081 game-director acceptance is not recorded in the asset sidecar")
    if record.get("sourceClassification") not in {
        "OFFICIAL-DIRECT", "OFFICIAL-ADAPTED", "COMPOSITE-ADAPTATION", "NEW GAME CONTENT"
    }:
        fail("invalid source classification")
    source = ROOT / record["sourcePath"]
    runtime = ROOT / record["runtimePath"]
    if not source.is_file() or source.stat().st_size == 0:
        fail("editable Blender source is missing")
    if not runtime.is_file() or runtime.stat().st_size == 0:
        fail("exported runtime GLB is missing")

    coords = record["coordinateContract"]
    if coords != {
        "blenderUp": "+Z", "blenderForward": "+Y", "godotUp": "+Y", "godotForward": "-Z",
        "worldUnitsPerBlenderMetre": 1.0, "worldUnitsPerBuildCell": 2.0,
        "rootOrigin": "ground-contact-centre"
    }:
        fail("coordinate contract drift")

    gltf = read_glb(runtime)
    nodes = gltf.get("nodes", [])
    by_name = node_map(gltf)
    root_name = record["rootNode"]
    if root_name not in by_name:
        fail(f"missing root node {root_name}")
    root_node = by_name[root_name]
    for transform_key in ("translation", "rotation", "scale", "matrix"):
        if transform_key in root_node:
            expected = {"translation": [0, 0, 0], "rotation": [0, 0, 0, 1], "scale": [1, 1, 1]}.get(transform_key)
            if expected is None or root_node[transform_key] != expected:
                fail(f"root {transform_key} is not canonical")
    extras = root_node.get("extras", {})
    if extras.get("stable_id") != record["stableId"] or extras.get("asset_schema") != 1:
        fail("root provenance/schema extras did not survive export")
    if extras.get("production_status") != record["productionStatus"]:
        fail("root production status did not survive export")
    if extras.get("world_units_per_build_cell") != 2.0:
        fail("root scale metadata did not survive export")

    index_by_name = {node["name"]: index for index, node in enumerate(nodes)}
    counts: dict[str, int] = {}
    for lod in record["lods"]:
        name, root = lod["name"], lod["root"]
        if root not in by_name:
            fail(f"missing LOD root {root}")
        count = triangle_count(gltf, index_by_name[root])
        if not lod["minimumTriangles"] <= count <= lod["maximumTriangles"]:
            fail(f"{name} triangle count {count} outside sidecar budget")
        counts[name] = count
        geometry = [node["name"] for node in descendants(gltf, index_by_name[root]) if "mesh" in node]
        if not geometry:
            fail(f"{name} has no geometry")
        for node_name in geometry:
            parts = node_name.split("_")
            if len(parts) < 4 or parts[0] not in record["materialRoles"] or parts[1] not in {"Hero", "Support", "Micro"} or parts[-1] != name:
                fail(f"geometry naming contract violation: {node_name}")
            if name == "Strategic" and parts[1] != "Hero":
                fail(f"Strategic LOD contains non-Hero geometry: {node_name}")
            if name == "Combat" and parts[1] == "Micro":
                fail(f"Combat LOD contains Micro geometry: {node_name}")
    if not counts["Close"] > counts["Combat"] > counts["Strategic"] > 0:
        fail(f"LOD triangle counts do not strictly decrease: {counts}")

    for required in record["requiredPivots"] + record["requiredSockets"]:
        if required not in by_name:
            fail(f"required attachment node missing: {required}")
        if "mesh" in by_name[required]:
            fail(f"attachment node must not contain render geometry: {required}")

    materials = {entry.get("name") for entry in gltf.get("materials", [])}
    for required in ["M85_PaintedHull", "M85_Accent", "M85_DarkMechanic", "M85_ToolSteel", "M85_Rubber", "M85_Canopy", "M85_Lamp"]:
        if required not in materials:
            fail(f"semantic material missing: {required}")

    import_path = runtime.with_suffix(runtime.suffix + ".import")
    if not import_path.is_file():
        fail("Godot import record is missing")
    import_text = import_path.read_text(encoding="utf-8")
    for token in [
        'importer="scene"', 'nodes/apply_root_scale=true', 'nodes/root_scale=1.0',
        'meshes/ensure_tangents=true', 'meshes/generate_lods=false',
        'meshes/create_shadow_meshes=true', 'animation/import=true', 'animation/fps=30',
        'animation/remove_immutable_tracks=true', 'import_script/path=""'
    ]:
        if token not in import_text:
            fail(f"Godot import setting missing: {token}")

    print(
        "M8.5 ASSET PIPELINE: PASS "
        f"stableId={record['stableId']} close={counts['Close']} combat={counts['Combat']} "
        f"strategic={counts['Strategic']} pivots={len(record['requiredPivots'])} "
        f"sockets={len(record['requiredSockets'])} materials={len(materials)}"
    )


if __name__ == "__main__":
    main()
