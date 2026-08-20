"""Generate the reproducible M7 Style Lab drill-rig proxy as a glTF binary.

Run with Blender 4.4+:
  Blender --background --python tools/blender/generate_m7_style_unit.py -- \
    GodotClient/Assets/M7/raider_drill_rig.glb \
    ArtSource/M7/raider_drill_rig.blend

The model is deliberately a real-time style carrier rather than final unit art.
Godot replaces the placeholder materials according to semantic node names.
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


def make_material(name: str, color: tuple[float, float, float, float], metallic: float, roughness: float) -> bpy.types.Material:
    material = bpy.data.materials.new(name)
    material.diffuse_color = color
    material.use_nodes = True
    principled = material.node_tree.nodes.get("Principled BSDF")
    principled.inputs["Base Color"].default_value = color
    principled.inputs["Metallic"].default_value = metallic
    principled.inputs["Roughness"].default_value = roughness
    if color[3] < 0.999:
        principled.inputs["Alpha"].default_value = color[3]
        material.surface_render_method = "DITHERED"
    return material


def parent_local(obj: bpy.types.Object, parent: bpy.types.Object | None, location: tuple[float, float, float]) -> bpy.types.Object:
    obj.parent = parent
    obj.location = location
    return obj


def empty(name: str, location: tuple[float, float, float], parent: bpy.types.Object | None = None) -> bpy.types.Object:
    obj = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(obj)
    obj.empty_display_type = "PLAIN_AXES"
    obj.empty_display_size = 0.32
    return parent_local(obj, parent, location)


def assign_material(obj: bpy.types.Object, material: bpy.types.Material) -> None:
    obj.data.materials.append(material)


def beveled_box(
    name: str,
    size: tuple[float, float, float],
    location: tuple[float, float, float],
    material: bpy.types.Material,
    parent: bpy.types.Object | None,
    rotation: tuple[float, float, float] = (0.0, 0.0, 0.0),
    bevel: float = 0.08,
) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cube_add(size=1.0)
    obj = bpy.context.object
    obj.name = name
    obj.scale = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.rotation_euler = rotation
    parent_local(obj, parent, location)
    assign_material(obj, material)
    if bevel > 0.0:
        modifier = obj.modifiers.new("EdgeBevel", "BEVEL")
        modifier.width = bevel
        modifier.segments = 2
        modifier.limit_method = "ANGLE"
    return obj


def cylinder(
    name: str,
    radius: float,
    depth: float,
    location: tuple[float, float, float],
    material: bpy.types.Material,
    parent: bpy.types.Object | None,
    rotation: tuple[float, float, float] = (0.0, 0.0, 0.0),
    vertices: int = 16,
    bevel: float = 0.035,
) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth)
    obj = bpy.context.object
    obj.name = name
    obj.rotation_euler = rotation
    parent_local(obj, parent, location)
    assign_material(obj, material)
    if bevel > 0.0:
        modifier = obj.modifiers.new("EdgeBevel", "BEVEL")
        modifier.width = bevel
        modifier.segments = 2
        modifier.limit_method = "ANGLE"
    return obj


def cone(
    name: str,
    radius_a: float,
    radius_b: float,
    depth: float,
    location: tuple[float, float, float],
    material: bpy.types.Material,
    parent: bpy.types.Object | None,
    rotation: tuple[float, float, float],
    vertices: int = 16,
) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=radius_a, radius2=radius_b, depth=depth)
    obj = bpy.context.object
    obj.name = name
    obj.rotation_euler = rotation
    parent_local(obj, parent, location)
    assign_material(obj, material)
    modifier = obj.modifiers.new("EdgeBevel", "BEVEL")
    modifier.width = min(radius_a, radius_b if radius_b > 0.0 else radius_a) * 0.08
    modifier.segments = 2
    modifier.limit_method = "ANGLE"
    return obj


def wedge(
    name: str,
    width: float,
    length: float,
    low_height: float,
    high_height: float,
    location: tuple[float, float, float],
    material: bpy.types.Material,
    parent: bpy.types.Object | None,
) -> bpy.types.Object:
    x = width * 0.5
    y = length * 0.5
    vertices = [
        (-x, -y, 0.0), (x, -y, 0.0), (x, y, 0.0), (-x, y, 0.0),
        (-x, -y, low_height), (x, -y, low_height), (x, y, high_height), (-x, y, high_height),
    ]
    faces = [
        (0, 1, 2, 3), (4, 7, 6, 5), (0, 4, 5, 1),
        (1, 5, 6, 2), (2, 6, 7, 3), (3, 7, 4, 0),
    ]
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    parent_local(obj, parent, location)
    assign_material(obj, material)
    modifier = obj.modifiers.new("EdgeBevel", "BEVEL")
    modifier.width = 0.06
    modifier.segments = 2
    modifier.limit_method = "ANGLE"
    return obj


def add_wheel(name: str, location: tuple[float, float, float], chassis: bpy.types.Object, rubber: bpy.types.Material, tool: bpy.types.Material) -> None:
    pivot = empty(f"Pivot_Wheel_{name}", location, chassis)
    cylinder(f"Rubber_Wheel_{name}", 0.63, 0.42, (0.0, 0.0, 0.0), rubber, pivot, rotation=(0.0, math.pi * 0.5, 0.0), vertices=20, bevel=0.055)
    cylinder(f"Tool_WheelHub_{name}", 0.31, 0.45, (0.0, 0.0, 0.0), tool, pivot, rotation=(0.0, math.pi * 0.5, 0.0), vertices=16, bevel=0.025)


def build_model() -> bpy.types.Object:
    body = make_material("M7_Body", (0.025, 0.40, 0.38, 1.0), 0.0, 0.32)
    accent = make_material("M7_Accent", (0.92, 0.57, 0.04, 1.0), 0.0, 0.38)
    neutral = make_material("M7_Neutral", (0.15, 0.17, 0.18, 1.0), 0.0, 0.50)
    tool = make_material("M7_Tool", (0.48, 0.53, 0.57, 1.0), 0.84, 0.28)
    rubber = make_material("M7_Rubber", (0.018, 0.022, 0.024, 1.0), 0.0, 0.82)
    glass = make_material("M7_Glass", (0.09, 0.22, 0.19, 0.48), 0.0, 0.17)
    signal = make_material("M7_Signal", (0.95, 0.22, 0.03, 0.72), 0.0, 0.16)
    lamp = make_material("M7_Lamp", (0.31, 1.0, 0.21, 0.88), 0.0, 0.12)

    root = empty("RaiderDrillRig", (0.0, 0.0, 0.0))
    chassis = empty("Pivot_Suspension", (0.0, 0.0, 0.0), root)

    beveled_box("Neutral_Chassis", (3.65, 4.95, 0.46), (0.0, 0.15, 1.04), neutral, chassis, bevel=0.12)
    beveled_box("Body_MainDeck", (3.18, 3.25, 0.52), (0.0, 0.46, 1.48), body, chassis, bevel=0.13)
    beveled_box("Body_RearHousing", (2.95, 1.22, 0.92), (0.0, 1.82, 1.86), body, chassis, bevel=0.15)
    wedge("Body_LeftNose", 1.38, 1.62, 0.28, 0.72, (-0.78, -1.72, 1.36), body, chassis)
    wedge("Body_RightNose", 1.38, 1.62, 0.28, 0.72, (0.78, -1.72, 1.36), body, chassis)
    beveled_box("Neutral_CenterSpine", (0.44, 2.68, 0.42), (0.0, -0.33, 1.88), neutral, chassis, bevel=0.07)

    wedge("Glass_Canopy", 1.72, 1.50, 0.18, 0.70, (0.0, 0.60, 2.00), glass, chassis)
    beveled_box("Neutral_CanopyFrameFront", (1.82, 0.12, 0.12), (0.0, -0.12, 2.18), neutral, chassis, bevel=0.025)
    beveled_box("Neutral_CanopyFrameRear", (1.82, 0.12, 0.12), (0.0, 1.34, 2.57), neutral, chassis, bevel=0.025)
    beveled_box("Neutral_CanopyFrameCenter", (0.10, 1.42, 0.12), (0.0, 0.60, 2.38), neutral, chassis, bevel=0.02)

    for side, x in (("L", -1.92), ("R", 1.92)):
        beveled_box(f"Rubber_TrackPod_{side}", (0.70, 4.40, 0.68), (x, 0.08, 0.76), rubber, chassis, bevel=0.14)
        beveled_box(f"Body_TrackGuard_{side}", (0.52, 3.92, 0.34), (x, 0.14, 1.20), body, chassis, bevel=0.10)
        for index, y in enumerate((-1.35, 0.0, 1.35)):
            add_wheel(f"{side}{index + 1}", (x + (-0.15 if side == "L" else 0.15), y, 0.72), chassis, rubber, tool)

    for side, x in (("L", -1.42), ("R", 1.42)):
        cylinder(f"Tool_Hydraulic_{side}", 0.13, 1.35, (x, -1.18, 1.92), tool, chassis, rotation=(math.pi * 0.5, 0.0, 0.0), vertices=12, bevel=0.02)
        beveled_box(f"Accent_HazardRail_{side}", (0.13, 1.52, 0.18), (x, 1.44, 2.38), accent, chassis, bevel=0.04)
        cylinder(f"Tool_Exhaust_{side}", 0.17, 1.05, (x, 2.05, 2.55), tool, chassis, vertices=12, bevel=0.025)
        cylinder(f"Neutral_ExhaustCap_{side}", 0.23, 0.18, (x, 2.05, 3.02), neutral, chassis, vertices=12, bevel=0.025)

    for index, x in enumerate((-1.08, -0.36, 0.36, 1.08)):
        cylinder(f"Body_Stud_{index + 1}", 0.15, 0.10, (x, 1.86, 2.36), body, chassis, vertices=16, bevel=0.02)

    beveled_box("Accent_FrontHazard_L", (0.62, 0.12, 0.24), (-0.85, -2.39, 1.47), accent, chassis, rotation=(0.0, 0.0, -0.30), bevel=0.03)
    beveled_box("Accent_FrontHazard_R", (0.62, 0.12, 0.24), (0.85, -2.39, 1.47), accent, chassis, rotation=(0.0, 0.0, 0.30), bevel=0.03)
    cylinder("Signal_Amber_L", 0.17, 0.10, (-1.30, -2.36, 1.75), signal, chassis, rotation=(math.pi * 0.5, 0.0, 0.0), vertices=16, bevel=0.02)
    cylinder("Signal_Amber_R", 0.17, 0.10, (1.30, -2.36, 1.75), signal, chassis, rotation=(math.pi * 0.5, 0.0, 0.0), vertices=16, bevel=0.02)
    cylinder("Lamp_Work_Green", 0.19, 0.12, (0.0, 2.46, 2.35), lamp, chassis, rotation=(math.pi * 0.5, 0.0, 0.0), vertices=16, bevel=0.02)

    drill = empty("Pivot_Drill", (0.0, -2.44, 1.12), chassis)
    cylinder("Tool_DrillBearing", 0.58, 0.52, (0.0, -0.20, 0.0), tool, drill, rotation=(math.pi * 0.5, 0.0, 0.0), vertices=20, bevel=0.055)
    cone("Tool_DrillSegment_A", 0.56, 0.43, 0.74, (0.0, -0.82, 0.0), tool, drill, (math.pi * 0.5, 0.0, 0.0), 20)
    cone("Tool_DrillSegment_B", 0.45, 0.31, 0.70, (0.0, -1.52, 0.0), tool, drill, (math.pi * 0.5, 0.0, 0.0), 20)
    cone("Tool_DrillSegment_C", 0.33, 0.18, 0.62, (0.0, -2.17, 0.0), tool, drill, (math.pi * 0.5, 0.0, 0.0), 20)
    cone("Tool_DrillTip", 0.20, 0.02, 0.52, (0.0, -2.73, 0.0), tool, drill, (math.pi * 0.5, 0.0, 0.0), 16)

    return root


def evaluated_triangle_count() -> int:
    depsgraph = bpy.context.evaluated_depsgraph_get()
    count = 0
    for obj in bpy.context.scene.objects:
        if obj.type != "MESH":
            continue
        evaluated = obj.evaluated_get(depsgraph)
        mesh = evaluated.to_mesh()
        mesh.calc_loop_triangles()
        count += len(mesh.loop_triangles)
        evaluated.to_mesh_clear()
    return count


def export_glb(path: str, blend_path: str) -> None:
    os.makedirs(os.path.dirname(path), exist_ok=True)
    os.makedirs(os.path.dirname(blend_path), exist_ok=True)
    # The tracked source is itself reproducible; do not leave untracked .blend1
    # backups beside it every time the generator is verified.
    bpy.context.preferences.filepaths.save_version = 0
    bpy.ops.wm.save_as_mainfile(filepath=blend_path)
    bpy.ops.export_scene.gltf(
        filepath=path,
        export_format="GLB",
        export_apply=True,
        export_yup=True,
        export_materials="EXPORT",
        export_cameras=False,
        export_lights=False,
        export_extras=True,
    )


def main() -> None:
    path, blend_path = output_paths()
    reset_scene()
    build_model()
    triangles = evaluated_triangle_count()
    if not 7_500 <= triangles <= 20_000:
        raise RuntimeError(f"M7 style-unit triangle budget violated: {triangles}")
    export_glb(path, blend_path)
    print(f"M7 STYLE UNIT: PASS path={path} triangles={triangles} objects={len(bpy.context.scene.objects)}")


if __name__ == "__main__":
    main()
