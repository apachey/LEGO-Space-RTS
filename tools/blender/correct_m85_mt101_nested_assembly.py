"""Bounded MT-101 rear-bay correction of the retained native source.

Comparison dimensions are estimated, not measured official parts or game units.
No roster/runtime imports. Existing front chassis/cabin/tools remain untouched.
"""
from __future__ import annotations

import hashlib
import json
from pathlib import Path
import sys

import bpy
from mathutils import Vector
from mathutils.bvhtree import BVHTree

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import generate_m85_source_locked_corrections as old

S = old.S
ROOT = Path(__file__).resolve().parents[2]
BASE = ROOT / "ArtSource/M85/Preproduction/SourceLockedCorrectionsV1Finished/source_locked_corrections.blend"
STATUS = "INTERNAL_NESTED_ASSEMBLY_CONTROL_NOT_FINAL_APPEARANCE"


def bounds(objects):
    points = [o.matrix_world @ Vector(c) / S for o in objects if o.type == "MESH" for c in o.bound_box]
    return [[min(p[i] for p in points) for i in range(3)],
            [max(p[i] for p in points) for i in range(3)]]


def mesh_tree(obj):
    obj.data.calc_loop_triangles()
    vertices = [obj.matrix_world @ v.co for v in obj.data.vertices]
    triangles = [tuple(t.vertices) for t in obj.data.loop_triangles]
    return BVHTree.FromPolygons(vertices, triangles, all_triangles=True)


def intersections(moving, obstacles):
    return [(a.name, b.name) for a in moving for b in obstacles
            if mesh_tree(a).overlap(mesh_tree(b))]


def preserved_signature():
    # All retained mesh vertices/transforms and parent relationships outside the
    # one removed solid rear deck are locked across this local correction.
    return {o.name: (o.parent.name if o.parent else None,
                     [list(row) for row in o.matrix_world],
                     [list(v.co) for v in o.data.vertices])
            for o in bpy.data.objects if o.type == "MESH" and o.name != "MT101_RearSpacecraftDeck"}


def build():
    ship = bpy.data.objects["MT101_DockedRearSpacecraft"]
    # Direct source parent retained: source model -> rear spacecraft -> bike.
    bpy.data.objects.remove(bpy.data.objects["MT101_RearSpacecraftDeck"], do_unlink=True)
    mats = tuple(bpy.data.materials[n] for n in
                 ("Matte mid shell", "Matte black structure", "Matte light shell", "Smoked closed glazing"))
    shell, dark, light, glass = mats
    old.d.box("MT101_RetainedRearCraftNoseFloor", (4.8, 5, .65), (0, -5.5, 10.775), ship, light)
    for sign in (-1, 1):
        old.d.box(f"MT101_BaySideRail_{sign}", (1, 6.7, 1.25), (sign*2.1, -11, 11.3), ship, light)
        old.studs(f"MT101_BayRail_{sign}", [(sign*2.1,y,11.925) for y in (-8.5,-9.5,-10.5,-11.5,-12.5,-13.5)], ship, shell)
    old.d.box("MT101_BayRearStop", (4.8, .5, .65), (0,-14.1,10.775), ship, dark)
    floor = old.d.box("MT101_BaySupportingFloor", (3.2, 6, .25), (0,-11,10.575), ship, dark)
    floor["bay_support"] = True
    bike = old.group("MT101_ContainedMiniBike", ship, "contained_extractable_mini_bike")
    bike["source_pages"] = "book 1 p8; book 2 p43"
    bike["extraction_axis"] = [0, 0, 1]
    for i,y in enumerate((-9.1,-13.1)):
        wheel = old.group(f"MT101_MiniBikeWheel_{i}", bike, "mini_bike_contact_wheel", (0,y,11.45))
        old.d.rod(wheel.name+"_Tyre", (-1,y,11.45),(1,y,11.45), .75,wheel,shell,vertices=20)
        for sign in (-1,1):
            old.d.rod(wheel.name+f"_Hub_{sign}",(sign*1,y,11.45),(sign*1.04,y,11.45),.36,wheel,dark,vertices=16)
    for sign in (-1,1):
        old.d.box(f"MT101_MiniBikeSideBeam_{sign}", (.28,4.3,.5), (sign*1.25,-11.1,11.6),bike,dark)
    old.d.box("MT101_MiniBikeSeat", (1.6,2.1,.32),(0,-11.2,11.7),bike,light)
    old.d.box("MT101_MiniBikeFeet", (1.4,1,.6),(0,-9.9,12.1),bike,light)
    old.d.box("MT101_MiniBikeReclinedTorso", (1.6,1.3,.8),(0,-11.2,12.3),bike,light)
    old.d.rod("MT101_MiniBikeHumanHelmet", (0,-12.35,12.3),(0,-12.35,13.1),.58,bike,shell,vertices=16)
    old.d.rod("MT101_MiniBikeVisor", (0,-12.05,12.55),(0,-11.93,12.55),.43,bike,glass,vertices=16)
    # Source-like bent narrow hood, not a generic modern motorcycle canopy.
    old.d.box("MT101_MiniBikeHoodTop", (2.6,.4,.28),(0,-12.6,13.6),bike,glass)
    for sign in (-1,1):
        old.d.rod(f"MT101_MiniBikeHoodEdge_{sign}",(sign*1.3,-12.6,13.6),
                  (sign*1.3,-10.4,12.8),.12,bike,glass)
    return bike


def audit(bike):
    mt = bpy.data.objects["MT101"]
    ship = bpy.data.objects["MT101_DockedRearSpacecraft"]
    assert bike.parent == ship and ship.parent == mt, "nested source parents"
    assert len([o for o in bike.children_recursive if o.get("kind") == "mini_bike_contact_wheel"]) == 2, "two bike wheels"
    assert list(bike["extraction_axis"]) == [0,0,1], "source upward extraction axis"
    moving = [o for o in bike.children_recursive if o.type == "MESH"]
    obstacles = [o for o in mt.children_recursive if o.type == "MESH" and o not in moving and not o.get("bay_support")]
    lo,hi = bounds(moving)
    assert lo[0] > -1.6 and hi[0] < 1.6 and lo[1] > -14 and hi[1] < -8, "bike fits estimated central bay"
    base = bike.location.copy()
    for step in range(33):
        bike.location.z = base.z + step*.25*S
        bpy.context.view_layer.update()
        assert not intersections(moving, obstacles), "bike extraction intersects retained mesh"
    bike.location = base
    bpy.context.view_layer.update()
    # Whole carrier lifts with its contained bike; permanent front cabin stays.
    ship_meshes = [o for o in ship.children_recursive if o.type == "MESH"]
    chassis_meshes = [o for o in mt.children_recursive if o.type == "MESH" and o not in ship_meshes]
    ship_base = ship.location.copy()
    for step in range(25):
        ship.location.z = ship_base.z + step*.5*S
        bpy.context.view_layer.update()
        assert not intersections(ship_meshes, chassis_meshes), "carrier separation intersects retained chassis"
    ship.location = ship_base
    bpy.context.view_layer.update()
    return {"sourceParents":["MT101","MT101_DockedRearSpacecraft","MT101_ContainedMiniBike"],
            "bikeBoundsComparisonStuds":[lo,hi], "bayInnerWidthComparisonStuds":3.2,
            "extractionAxis":[0,0,1], "bikeSweepSamples":33,"carrierSweepSamples":25,
            "triangleSurfaceIntersections":0,
            "limitations":"Estimated native envelope/sampled triangle-surface checks, not exact official dimensions, certified LEGO buildability, continuous collision proof or gameplay animation."}


def main():
    if "--audit" in sys.argv:
        bpy.ops.wm.open_mainfile(filepath=str(Path(sys.argv[sys.argv.index("--audit")+1]).resolve()))
        bpy.context.view_layer.update()
        result = audit(bpy.data.objects["MT101_ContainedMiniBike"])
        old.scene_audit({n:bpy.data.objects[n] for n in ("MT101","MX71")})
        print("MT101 NESTED SAVED SCENE: PASS",json.dumps(result))
        return
    output = Path(sys.argv[sys.argv.index("--")+1]).resolve()
    if output.exists():
        raise RuntimeError("Refusing to overwrite retained correction outputs")
    output.mkdir(parents=True)
    bpy.ops.wm.open_mainfile(filepath=str(BASE))
    bpy.context.view_layer.update()
    preserved = preserved_signature()
    bike = build()
    bpy.context.view_layer.update()
    now = preserved_signature()
    assert all(now[k] == v for k,v in preserved.items()), "unrelated retained geometry drifted"
    result = audit(bike)
    old.scene_audit({n:bpy.data.objects[n] for n in ("MT101","MX71")})
    guards=[]
    for label,mutation,restore in [
        ("wrong_bike_parent",lambda:setattr(bike,"parent",bpy.data.objects["MT101"]),lambda:setattr(bike,"parent",bpy.data.objects["MT101_DockedRearSpacecraft"])),
        ("oversized_bike",lambda:setattr(bike,"scale",Vector((2,1,1))),lambda:setattr(bike,"scale",Vector((1,1,1)))),
        ("lost_upward_axis",lambda:bike.__setitem__("extraction_axis",[1,0,0]),lambda:bike.__setitem__("extraction_axis",[0,0,1]))]:
        mutation()
        bpy.context.view_layer.update()
        try:
            audit(bike)
        except AssertionError:
            guards.append(label)
        else:
            raise RuntimeError("Missed guard: "+label)
        finally:
            restore()
            bpy.context.view_layer.update()
    # Save assembled control, never the separated presentation pose.
    for obj in bike.children_recursive:
        if obj.type == "MESH":
            bevel=obj.modifiers.new("Moulded edge","BEVEL")
            bevel.width=.00035
            bevel.segments=2
    source=output/"mt101_nested_assembly.blend"
    bpy.ops.wm.save_as_mainfile(filepath=str(source))
    scene=bpy.context.scene
    scene.cycles.samples=24
    images={}
    for name,pos,target,scale in [
        ("assembled",(-.38,.45,.29),(0,-.015,.065),.34),
        ("rear_bay",(-.20,-.22,.33),(0,-.085,.095),.17),
        ("nested_separation",(-.30,.35,.29),(0,-.02,.13),.40)]:
        mt=bpy.data.objects["MT101"]
        ship=bpy.data.objects["MT101_DockedRearSpacecraft"]
        for obj in bpy.data.objects["MX71"].children_recursive:
            obj.hide_render=True
        for obj in mt.children_recursive:
            obj.hide_render=name=="rear_bay" and obj not in ship.children_recursive and obj!=ship
        if name=="nested_separation":
            ship.location.z=12*S
            bike.location.z=8*S
            bpy.context.view_layer.update()
        old.d.aim(scene.camera,pos,target)
        scene.camera.data.ortho_scale=scale
        scene.render.filepath=str(output/(name+".png"))
        bpy.ops.render.render(write_still=True)
        images[name]={"file":name+".png","sha256":hashlib.sha256((output/(name+".png")).read_bytes()).hexdigest()}
    record={"schema":1,"status":STATUS,"productionAccepted":False,"canonImpact":"NONE",
            "authority":{"baseCommit":"b5c3dd4","message":"+","scope":"Correct source-led nested construction and finish its appearance without independent gameplay or free-form reconstruction."},
            "retainedBaseSha256":hashlib.sha256(BASE.read_bytes()).hexdigest(),
            "preservedMeshCount":len(preserved),"geometryAudit":result,"negativeGuards":guards,
            "nativeSource":{"file":source.name,"sha256":hashlib.sha256(source.read_bytes()).hexdigest()},"images":images}
    (output/"construction_audit.json").write_text(json.dumps(record,indent=2)+"\n")
    print("MT101 NESTED CONSTRUCTION: PASS",len(guards),"negative guards")


if __name__=="__main__":
    main()
