"""Generate the deterministic M8.5 T081 asset-pipeline reference vehicle.

The result is a technical round-trip fixture, not a canonical roster model.
Run with Blender 4.4+ through tools/generate-m85-pipeline-reference.sh.
"""

from __future__ import annotations

import math
import os
import sys

import bpy


def output_paths() -> tuple[str, str]:
    try:
        marker = sys.argv.index("--")
        glb_path = sys.argv[marker + 1]
        blend_path = sys.argv[marker + 2]
    except (ValueError, IndexError) as exc:
        raise RuntimeError("Expected output .glb and .blend paths after --") from exc
    if not glb_path.lower().endswith(".glb") or not blend_path.lower().endswith(".blend"):
        raise RuntimeError("Output paths must end in .glb and .blend")
    return os.path.abspath(glb_path), os.path.abspath(blend_path)


def reset_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.materials):
        for datablock in list(datablocks):
            if datablock.users == 0:
                datablocks.remove(datablock)


def material(name: str, color: tuple[float, float, float, float], metallic: float, roughness: float) -> bpy.types.Material:
    result = bpy.data.materials.new(name)
    result.diffuse_color = color
    result.use_nodes = True
    principled = result.node_tree.nodes.get("Principled BSDF")
    principled.inputs["Base Color"].default_value = color
    principled.inputs["Metallic"].default_value = metallic
    principled.inputs["Roughness"].default_value = roughness
    if color[3] < 0.999:
        principled.inputs["Alpha"].default_value = color[3]
        result.surface_render_method = "DITHERED"
    return result


def attach(obj: bpy.types.Object, parent: bpy.types.Object | None, location: tuple[float, float, float]) -> bpy.types.Object:
    obj.parent = parent
    obj.location = location
    return obj


def empty(name: str, location: tuple[float, float, float], parent: bpy.types.Object | None = None) -> bpy.types.Object:
    obj = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(obj)
    obj.empty_display_type = "PLAIN_AXES"
    obj.empty_display_size = 0.24
    return attach(obj, parent, location)


def box(name: str, size: tuple[float, float, float], location: tuple[float, float, float],
        surface: bpy.types.Material, parent: bpy.types.Object, bevel: float = 0.0) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cube_add(size=1.0)
    obj = bpy.context.object
    obj.name = name
    obj.scale = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    attach(obj, parent, location)
    obj.data.materials.append(surface)
    if bevel > 0.0:
        modifier = obj.modifiers.new("EdgeBevel", "BEVEL")
        modifier.width = bevel
        modifier.segments = 1
        modifier.limit_method = "ANGLE"
    return obj


def cylinder(name: str, radius: float, depth: float, location: tuple[float, float, float],
             surface: bpy.types.Material, parent: bpy.types.Object,
             rotation: tuple[float, float, float] = (0.0, 0.0, 0.0), vertices: int = 12) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth)
    obj = bpy.context.object
    obj.name = name
    obj.rotation_euler = rotation
    attach(obj, parent, location)
    obj.data.materials.append(surface)
    return obj


def cone(name: str, radius: float, depth: float, location: tuple[float, float, float],
         surface: bpy.types.Material, parent: bpy.types.Object, vertices: int) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=radius, radius2=0.04, depth=depth)
    obj = bpy.context.object
    obj.name = name
    obj.rotation_euler = (-math.pi * 0.5, 0.0, 0.0)
    attach(obj, parent, location)
    obj.data.materials.append(surface)
    return obj


def add_close(root: bpy.types.Object, surfaces: dict[str, bpy.types.Material]) -> None:
    box("Neutral_Hero_Chassis_Close", (3.4, 3.7, 0.42), (0.0, -0.05, 0.68), surfaces["neutral"], root, 0.08)
    box("Body_Hero_Hull_Close", (2.9, 2.6, 0.64), (0.0, -0.28, 1.18), surfaces["body"], root, 0.10)
    box("Glass_Hero_Canopy_Close", (1.5, 1.25, 0.52), (0.0, -0.38, 1.82), surfaces["glass"], root, 0.06)
    box("Tool_Hero_DrillMount_Close", (0.82, 0.58, 0.72), (0.0, 1.48, 1.00), surfaces["tool"], root, 0.05)
    cone("Tool_Hero_Drill_Close", 0.43, 1.32, (0.0, 2.18, 1.00), surfaces["tool"], root, 18)
    for side, x in (("Left", -1.60), ("Right", 1.60)):
        for index, y in enumerate((0.95, -0.75)):
            cylinder(f"Rubber_Hero_Wheel{side}{index + 1}_Close", 0.52, 0.34, (x, y, 0.54),
                     surfaces["rubber"], root, (0.0, math.pi * 0.5, 0.0), 18)
    box("Accent_Support_HazardLeft_Close", (0.30, 1.45, 0.20), (-1.08, 0.05, 1.58), surfaces["accent"], root, 0.03)
    box("Accent_Support_HazardRight_Close", (0.30, 1.45, 0.20), (1.08, 0.05, 1.58), surfaces["accent"], root, 0.03)
    box("Neutral_Support_RearFrame_Close", (2.60, 0.42, 0.58), (0.0, -1.52, 1.42), surfaces["neutral"], root, 0.04)
    cylinder("Lamp_Support_WorkLamp_Close", 0.16, 0.12, (0.0, 1.08, 1.82),
             surfaces["lamp"], root, (-math.pi * 0.5, 0.0, 0.0), 12)
    for index, x in enumerate((-0.9, -0.3, 0.3, 0.9)):
        cylinder(f"Body_Micro_Stud{index + 1}_Close", 0.12, 0.10, (x, -1.20, 1.77), surfaces["body"], root, vertices=12)


def add_combat(root: bpy.types.Object, surfaces: dict[str, bpy.types.Material]) -> None:
    box("Neutral_Hero_Chassis_Combat", (3.4, 3.7, 0.44), (0.0, -0.05, 0.69), surfaces["neutral"], root, 0.06)
    box("Body_Hero_Hull_Combat", (2.9, 2.7, 0.78), (0.0, -0.30, 1.28), surfaces["body"], root, 0.08)
    box("Glass_Hero_Canopy_Combat", (1.5, 1.25, 0.50), (0.0, -0.36, 1.84), surfaces["glass"], root, 0.04)
    cone("Tool_Hero_Drill_Combat", 0.46, 1.72, (0.0, 1.95, 1.02), surfaces["tool"], root, 14)
    for side, x in (("Left", -1.60), ("Right", 1.60)):
        cylinder(f"Rubber_Hero_WheelBank{side}_Combat", 0.58, 0.34, (x, 0.05, 0.58),
                 surfaces["rubber"], root, (0.0, math.pi * 0.5, 0.0), 14)
    box("Accent_Support_HazardBand_Combat", (2.30, 0.22, 0.18), (0.0, 1.08, 1.58), surfaces["accent"], root, 0.02)


def add_strategic(root: bpy.types.Object, surfaces: dict[str, bpy.types.Material]) -> None:
    box("Body_Hero_Hull_Strategic", (3.25, 3.65, 1.18), (0.0, -0.04, 1.02), surfaces["body"], root, 0.04)
    box("Glass_Hero_Canopy_Strategic", (1.45, 1.18, 0.42), (0.0, -0.28, 1.78), surfaces["glass"], root, 0.02)
    cone("Tool_Hero_Drill_Strategic", 0.48, 1.76, (0.0, 1.98, 1.00), surfaces["tool"], root, 10)
    cylinder("Rubber_Hero_Locomotion_Strategic", 0.60, 3.55, (0.0, 0.02, 0.58),
             surfaces["rubber"], root, (0.0, math.pi * 0.5, 0.0), 12)


def build_model() -> bpy.types.Object:
    surfaces = {
        "body": material("M85_PaintedHull", (0.04, 0.43, 0.40, 1.0), 0.0, 0.34),
        "accent": material("M85_Accent", (0.96, 0.55, 0.04, 1.0), 0.0, 0.40),
        "neutral": material("M85_DarkMechanic", (0.10, 0.12, 0.13, 1.0), 0.35, 0.48),
        "tool": material("M85_ToolSteel", (0.48, 0.53, 0.57, 1.0), 0.82, 0.28),
        "rubber": material("M85_Rubber", (0.018, 0.022, 0.024, 1.0), 0.0, 0.84),
        "glass": material("M85_Canopy", (0.08, 0.24, 0.21, 0.50), 0.0, 0.16),
        "lamp": material("M85_Lamp", (0.45, 1.0, 0.30, 1.0), 0.0, 0.12),
    }
    root = empty("Asset_PipelineReferenceVehicle", (0.0, 0.0, 0.0))
    root["asset_schema"] = 1
    root["stable_id"] = "pipeline.reference.vehicle"
    root["production_status"] = "PIPELINE_FIXTURE_NOT_ROSTER_ART"
    root["blender_forward"] = "+Y"
    root["godot_forward"] = "-Z"
    root["world_units_per_build_cell"] = 2.0

    close = empty("LOD_Close", (0.0, 0.0, 0.0), root)
    combat = empty("LOD_Combat", (0.0, 0.0, 0.0), root)
    strategic = empty("LOD_Strategic", (0.0, 0.0, 0.0), root)
    add_close(close, surfaces)
    add_combat(combat, surfaces)
    add_strategic(strategic, surfaces)

    empty("Pivot_Suspension", (0.0, 0.0, 0.68), root)
    empty("Pivot_ToolPrimary", (0.0, 1.42, 1.00), root)
    empty("Pivot_Wheel_Left", (-1.60, 0.05, 0.54), root)
    empty("Pivot_Wheel_Right", (1.60, 0.05, 0.54), root)
    empty("Socket_Selection", (0.0, 0.0, 0.0), root)
    empty("Socket_Health", (0.0, -0.10, 2.45), root)
    weapon = empty("Socket_Weapon_Primary", (0.0, 2.82, 1.00), root)
    weapon.rotation_euler = (-math.pi * 0.5, 0.0, 0.0)
    empty("Socket_VFX_Exhaust", (0.0, -1.72, 1.78), root)
    empty("Socket_Audio_Engine", (0.0, -0.72, 1.15), root)
    empty("Socket_Cargo", (0.0, -0.82, 1.36), root)
    return root


def triangle_counts() -> dict[str, int]:
    depsgraph = bpy.context.evaluated_depsgraph_get()
    counts = {"Close": 0, "Combat": 0, "Strategic": 0}
    for obj in bpy.context.scene.objects:
        if obj.type != "MESH":
            continue
        family = next((name for name in counts if obj.name.endswith(f"_{name}")), None)
        if family is None:
            raise RuntimeError(f"Mesh does not carry an LOD suffix: {obj.name}")
        evaluated = obj.evaluated_get(depsgraph)
        mesh = evaluated.to_mesh()
        mesh.calc_loop_triangles()
        counts[family] += len(mesh.loop_triangles)
        evaluated.to_mesh_clear()
    return counts


def export(glb_path: str, blend_path: str) -> None:
    os.makedirs(os.path.dirname(glb_path), exist_ok=True)
    os.makedirs(os.path.dirname(blend_path), exist_ok=True)
    bpy.context.preferences.filepaths.save_version = 0
    bpy.ops.wm.save_as_mainfile(filepath=blend_path)
    bpy.ops.export_scene.gltf(
        filepath=glb_path,
        export_format="GLB",
        export_apply=True,
        export_yup=True,
        export_materials="EXPORT",
        export_cameras=False,
        export_lights=False,
        export_extras=True,
    )


def main() -> None:
    glb_path, blend_path = output_paths()
    reset_scene()
    build_model()
    counts = triangle_counts()
    if not (counts["Close"] > counts["Combat"] > counts["Strategic"] > 0):
        raise RuntimeError(f"LOD triangle counts must strictly decrease: {counts}")
    export(glb_path, blend_path)
    print("M8.5 PIPELINE SOURCE: PASS " + " ".join(f"{key.lower()}={value}" for key, value in counts.items()))


if __name__ == "__main__":
    main()
