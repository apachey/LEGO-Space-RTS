"""Finished exterior concept renders, not shipping/integrated roster assets.

Uses the existing native draft builders and preserves their dimension guards.
No new free-form image attempt or physical-part/source-verification claim.
Blender --background --python-exit-code 1 --python <script> -- <new-output-dir>
"""
from __future__ import annotations

import hashlib
import json
import math
from pathlib import Path
import sys

import bpy
from mathutils import Vector

sys.path.insert(0, str(Path(__file__).resolve().parent))
import generate_m85_alien_shape_drafts as draft

STATUS = "COMPLETED_EXTERIOR_CONCEPT_REQUIRES_DIRECTOR_REVIEW"


def material(name, color, roughness=.3, transmission=0):
    result = bpy.data.materials.new(name)
    result.use_nodes = True
    result.diffuse_color = (*color, 1)
    shader = result.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*color, 1)
    shader.inputs["Roughness"].default_value = roughness
    shader.inputs["Transmission Weight"].default_value = transmission
    shader.inputs["IOR"].default_value = 1.46
    result["role"] = name
    result["acceptance"] = "SOURCE_FAMILY_PALETTE_PROPOSED_DISTRIBUTION"
    return result


def set_material(obj, mat):
    obj.data.materials.clear()
    obj.data.materials.append(mat)


def remove_geometry(obj):
    data = obj.data
    bpy.data.objects.remove(obj, do_unlink=True)
    if data.users == 0:
        bpy.data.meshes.remove(data)


def shell_valve(asset, sign, side, armor):
    # One broad molded curved shell per side, not a stack of armor strips.
    sections = [(-4, .42, 2, 2.8), (-3, .4, 2.85, 3.6),
                (-1, .38, 2.75, 3.45), (1, .38, 2.45, 2.9),
                (3, .45, 2.05, 2.1), (4, .6, 1.65, 1.75)]
    vertices = []
    for y, inner, outer, high in sections:
        vertices.extend([(sign*inner,y,.95), (sign*outer,y,.95),
                         (sign*outer,y,max(1.25,high-.65)),
                         (sign*(inner+.2),y,high)])
    faces = [(3,2,1,0), tuple(range(len(vertices)-4,len(vertices)))]
    for section in range(len(sections)-1):
        start=section*4
        for corner in range(4):
            faces.append((start+corner,start+(corner+1)%4,
                          start+4+(corner+1)%4,start+4+corner))
    return draft.mesh(f"Worker_{side}_ShellValve",vertices,faces,
                      asset,armor,"body")


def finish_worker(asset, mats):
    armor, neutral, lime, living, bone, glass = mats
    for obj in list(draft.geometry(asset)):
        if "ShellValve" in obj.name:
            side="Left" if "Left" in obj.name else "Right"
            remove_geometry(obj)
            shell_valve(asset,-1 if side=="Left" else 1,side,armor)
        elif "ProtectedLiving" in obj.name:
            set_material(obj,living)
        elif "InternalSpine" in obj.name:
            set_material(obj,bone)
        elif "UtilityJaw" in obj.name:
            set_material(obj,lime)
        else:
            set_material(obj,neutral)
    for sign,side in [(-1,"Left"),(1,"Right")]:
        draft.conduit(f"Worker_{side}_LimeShellLip",
                      [(sign*.7,-3.3,3.4),(sign*.66,-1,3.3),
                       (sign*.68,1,2.75),(sign*.75,3.2,1.95)],
                      asset,lime,"body")
        # Root collar visibly joins the articulated shell and spine.
        draft.rod(f"Worker_{side}_RootCollar",(sign*.55,-2.7,1.5),
                  (sign*.9,-2.7,1.5),.37,asset,lime,"body")
    # Two broad internal support vertebrae; no dense ribbed-machine decoration.
    for index,y in enumerate([-1.5,.7]):
        draft.box(f"Worker_ProtectedSupport_{index}",(.7,.45,.7),
                  (0,y,1.45),asset,bone,"body")
    draft.box("Worker_ClosedRearInterface",(.68,.48,.65),(0,-3.1,2.35),
              asset,glass,"body")


def finish_foundation(asset, mats):
    armor,neutral,lime,living,bone,glass=mats
    for obj in draft.geometry(asset,"base"):
        if "ProtectedLink" in obj.name:
            set_material(obj,living)
        elif "SupportSpine" in obj.name:
            set_material(obj,bone)
        elif "ContactPad" in obj.name or "HeadInterface" in obj.name:
            set_material(obj,neutral)
        else:
            set_material(obj,armor)
    # Four receiving plates connect to the existing fixed pads and roots.
    for index,(sx,sy) in enumerate([(-1,-1),(1,-1),(-1,1),(1,1)]):
        outline=[(sx*3.7,sy*4.1),(sx*4.4,sy*3.6),
                 (sx*5.1,sy*4.3),(sx*4.3,sy*5.1)]
        draft.slab(f"Base_RootReceivingPlate_{index}",outline,.43,.65,
                   asset,lime,"base")
        draft.rod(f"Base_ConnectionStud_{index}",(sx*4.2,sy*4.3,.6),
                  (sx*4.2,sy*4.3,.82),.3,asset,neutral,"base")
    # Two deliberately exposed protected interfaces, not a glass cockpit.
    for sign in [-1,1]:
        draft.box(f"Base_RecessedInterface_{sign}",(.65,.25,.6),
                  (sign*1.4,2.51,2.15),asset,glass,"base")


def finish_ground(asset,mats):
    armor,neutral,lime,living,bone,glass=mats
    for obj in draft.geometry(asset):
        if obj.get("component")=="base":
            continue
        set_material(obj,glass if "EmitterLens" in obj.name else
                     bone if "HeadSpine" in obj.name else
                     armor if "Crescent" in obj.name else neutral)
    for index in range(3):
        angle=index*2*math.pi/3+math.pi/2
        points=[(3.05*math.cos(angle-.7+j*1.4/8),
                 3.05*math.sin(angle-.7+j*1.4/8),5.82) for j in range(9)]
        draft.conduit(f"Ground_CrownInnerLip_{index}",points,
                      asset,lime,"head")
        c,s=math.cos(angle),math.sin(angle)
        draft.rod(f"Ground_EmitterCollar_{index}",(c*4.75,s*4.75,5.4),
                  (c*5.1,s*5.1,5.4),.44,asset,lime)


def finish_air(asset,mats):
    armor,neutral,lime,living,bone,glass=mats
    for obj in draft.geometry(asset):
        if obj.get("component")=="base":
            continue
        set_material(obj,glass if "EmitterLens" in obj.name else
                     bone if "HeadSpine" in obj.name else
                     armor if "Blade" in obj.name else neutral)
    for sign,side in [(-1,"Left"),(1,"Right")]:
        draft.conduit(f"Air_{side}_BladeInnerLip",
                      [(sign*1.25,1.6,4.8),(sign*1.25,2.1,6.2),
                       (sign*1.25,1.9,7.8),(sign*1.25,.8,9.4)],
                      asset,lime,"head")
        draft.rod(f"Air_{side}_RootCollar",(sign*.9,0,4),
                  (sign*1.4,0,4),.46,asset,lime)
    for index,(x,y) in enumerate([(-.6,-.4),(.6,-.4),(0,.85)]):
        draft.rod(f"Air_EmitterCollar_{index}",(x,y,8.5),(x,y,8.9),
                  .34,asset,lime)


def finish_edges(assets):
    # Non-destructive sub-millimetre molded edges. Bounds are checked on both
    # base geometry and evaluated geometry, not assumed to survive modifiers.
    for asset in assets.values():
        for obj in draft.geometry(asset):
            if not any(part in obj.name for part in
                       ["LoadBearingDeck","ShellValve","UtilityJaw",
                        "SourceInformedBlade"]):
                bevel=obj.modifiers.new("MoldedEdge","BEVEL")
                bevel.width=.045*draft.STUD
                bevel.segments=2
                bevel.affect="EDGES"
                bevel.limit_method="ANGLE"
                bevel.angle_limit=.42
            normal=obj.modifiers.new("MoldedFaceNormals","WEIGHTED_NORMAL")
            normal.keep_sharp=True


def validate(assets):
    result=draft.validate(assets)
    depsgraph=bpy.context.evaluated_depsgraph_get()
    for name,asset in assets.items():
        source=draft.dimensions(asset)
        points=[]
        for obj in draft.geometry(asset):
            evaluated=obj.evaluated_get(depsgraph)
            data=evaluated.to_mesh()
            try:
                points.extend(obj.matrix_local @ v.co for v in data.vertices)
            finally:
                evaluated.to_mesh_clear()
        actual=[(max(p[i] for p in points)-min(p[i] for p in points))/draft.STUD
                for i in range(3)]
        if any(abs(a-b)>1e-4 for a,b in zip(actual,source)):
            raise AssertionError(f"Finished evaluated envelope changed: {name} {actual}")
        roles={mat.name for obj in draft.geometry(asset) for mat in obj.data.materials}
        if not {"Body_AlienArmor","Accent_OpaqueLime","Glass_GreenInterface"} <= roles:
            raise AssertionError(f"Missing exterior material roles: {name}")
        for obj in draft.geometry(asset):
            for mat in obj.data.materials:
                shader=mat.node_tree.nodes.get("Principled BSDF")
                if shader.inputs["Emission Strength"].default_value!=0:
                    raise AssertionError(f"Unapproved persistent glow: {mat.name}")
        result[name]["evaluated_bounds_studs"]=actual
        result[name]["status"]=STATUS
    result["checks"].update({"evaluated_finished_envelopes":"PASS",
                             "source_family_material_roles":"PASS",
                             "no_permanent_emission":"PASS"})
    return result


def setup():
    scene=bpy.context.scene
    scene.render.engine="CYCLES"
    scene.cycles.device="CPU"
    scene.cycles.samples=48
    scene.cycles.use_denoising=True
    scene.render.resolution_x=1500
    scene.render.resolution_y=1200
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format="PNG"
    scene.view_settings.view_transform="AgX"
    scene.world.use_nodes=True
    scene.world.node_tree.nodes["Background"].inputs[0].default_value=(.8,.85,1,1)
    scene.world.node_tree.nodes["Background"].inputs[1].default_value=.35
    floor=draft.root("Concept_Backdrop",0)
    draft.box("Concept_Floor",(200,200,.2),(0,0,-.14),floor,
              material("Backdrop",(.8,.83,.86),.7),"reference")
    for name,position,power,size in [("Key",(-.12,.16,.22),.22,.16),
                                     ("Fill",(.18,.05,.16),.1,.18),
                                     ("Rim",(0,-.17,.18),.16,.12)]:
        bpy.ops.object.light_add(type="AREA",location=position)
        lamp=bpy.context.object
        lamp.name=name
        lamp.data.energy=power
        lamp.data.size=size
        lamp.rotation_euler=(-lamp.location).to_track_quat("-Z","Y").to_euler()
    bpy.ops.object.camera_add()
    camera=bpy.context.object
    camera.data.type="ORTHO"
    scene.camera=camera
    return camera


def main():
    args=sys.argv[sys.argv.index("--")+1:]
    if args[0]=="--audit-existing":
        bpy.ops.wm.open_mainfile(filepath=str(Path(args[1]).resolve()))
        assets={name:bpy.data.objects[f"Concept_{name}"] for name in
                ["Servitor","GroundPulse","AirLance"]}
        print("FINISHED CONCEPT SAVED AUDIT: PASS "+json.dumps(validate(assets)["checks"]))
        return
    self_test=args[0]=="--self-test"
    out=None if self_test else Path(args[0]).resolve()
    names=["alien_finished_concepts_v1.blend","appearance_audit.json"]+[
        f"{name}_{view}.png" for name in ["servitor","ground_pulse","air_lance"]
        for view in ["finished","top"]]
    if out is not None:
        if any((out/name).exists() for name in names):
            raise RuntimeError("Use a new version directory; historical outputs are protected")
        out.mkdir(parents=True,exist_ok=True)
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    mats=(material("Body_AlienArmor",(.012,.018,.017)),
          material("Neutral_ConnectedStructure",(.11,.13,.13),.4),
          material("Accent_OpaqueLime",(.39,.64,.012)),
          material("Neutral_ProtectedLivingLink",(.035,.24,.08),.4),
          material("Neutral_ProtectedSupport",(.55,.58,.37),.45),
          material("Glass_GreenInterface",(.33,.75,.015),.18,.55))
    # Builders expect shell, support, living in this exact order.
    builders=(mats[0],mats[1],mats[3])
    assets={name:draft.root(f"Concept_{name}",0) for name in
            ["Servitor","GroundPulse","AirLance"]}
    draft.worker(assets["Servitor"],builders)
    finish_worker(assets["Servitor"],mats)
    for name in ["GroundPulse","AirLance"]:
        draft.foundation(assets[name],builders)
        finish_foundation(assets[name],mats)
    draft.ground_head(assets["GroundPulse"],builders)
    finish_ground(assets["GroundPulse"],mats)
    draft.air_head(assets["AirLance"],builders)
    finish_air(assets["AirLance"],mats)
    finish_edges(assets)
    audit=validate(assets)
    if self_test:
        draft.self_tests(assets)
        mat=mats[2]
        shader=mat.node_tree.nodes.get("Principled BSDF")
        shader.inputs["Emission Strength"].default_value=1
        try:
            validate(assets)
        except AssertionError:
            print("FINISHED REGRESSION: PASS rejects permanently emissive lime")
        else:
            raise RuntimeError("Permanent-emission guard did not reject mutation")
        finally:
            shader.inputs["Emission Strength"].default_value=0
        validate(assets)
        print("FINISHED CONCEPT SELF TESTS: PASS 6/6")
        return
    camera=setup()
    for name,slug in [("Servitor","servitor"),("GroundPulse","ground_pulse"),
                      ("AirLance","air_lance")]:
        for other,asset in assets.items():
            for obj in draft.geometry(asset):
                obj.hide_render=other!=name
        camera.data.ortho_scale=.115 if name=="Servitor" else .155
        target=(0,.008,.013 if name=="Servitor" else .035)
        for view,position in [("finished",(-.12,.2,.15)),("top",(0,.008,.3))]:
            draft.aim(camera,position,target)
            bpy.context.scene.render.filepath=str(out/f"{slug}_{view}.png")
            bpy.ops.render.render(write_still=True)
    for asset in assets.values():
        for obj in draft.geometry(asset):
            obj.hide_render=False
    bpy.ops.wm.save_as_mainfile(filepath=str(out/"alien_finished_concepts_v1.blend"))
    audit["files_sha256"]={name:hashlib.sha256((out/name).read_bytes()).hexdigest()
                            for name in names if name!="appearance_audit.json"}
    (out/"appearance_audit.json").write_text(json.dumps(audit,indent=2)+"\n")
    print("FINISHED ALIEN CONCEPTS: PASS "+json.dumps(audit["checks"]))


if __name__=="__main__":
    main()
