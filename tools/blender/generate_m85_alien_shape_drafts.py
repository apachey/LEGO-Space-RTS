"""Bounded T082 source-informed geometry drafts; NOT shipping roster models.

Run in existing Blender 4.4+: --background --python <this file> -- <output-dir>.
The dimensions are director-accepted physical comparison envelopes, not Godot
world units or authoritative footprints. No image-generation retry is involved.
"""
from __future__ import annotations

import hashlib
import json
import math
from pathlib import Path
import sys

import bpy
from mathutils import Vector

STUD = 0.008
PLATE = 0.0032
STATUS = "PREPRODUCTION_SHAPE_DRAFT_DIMENSIONS_ONLY_ACCEPTED"


def surface(name, gray):
    result = bpy.data.materials.new(name)
    result.diffuse_color = (gray, gray, gray, 1)
    result.use_nodes = True
    shader = result.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (gray, gray, gray, 1)
    shader.inputs["Roughness"].default_value = 0.55
    return result


def attach(obj, name, root, material, component):
    obj.name = name
    obj.parent = root
    obj.data.materials.append(material)
    obj["component"] = component
    obj["evidence_status"] = "NEW_ADAPTATION_SOURCE_INFORMED_NOT_LITERAL_PART"
    return obj


def mesh(name, vertices, faces, root, material, component="head"):
    data = bpy.data.meshes.new(name)
    data.from_pydata([tuple(v * STUD for v in point) for point in vertices], [], faces)
    data.update()
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    attach(obj, name, root, material, component)
    # Recalculate normals on closed meshes; all shape drafts stay faceted.
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.normals_make_consistent(inside=False)
    bpy.ops.object.mode_set(mode="OBJECT")
    obj.select_set(False)
    return obj


def slab(name, outline, low, high, root, material, component="head"):
    count = len(outline)
    vertices = [(x, y, low) for x, y in outline] + [(x, y, high) for x, y in outline]
    faces = [tuple(reversed(range(count))), tuple(range(count, 2 * count))]
    faces += [(i, (i + 1) % count, (i + 1) % count + count, i + count) for i in range(count)]
    return mesh(name, vertices, faces, root, material, component)


def box(name, size, position, root, material, component="head"):
    x, y, z = position
    w, d, h = size
    return slab(name, [(x-w/2, y-d/2), (x+w/2, y-d/2),
                       (x+w/2, y+d/2), (x-w/2, y+d/2)], z-h/2, z+h/2,
                root, material, component)


def taper(name, lower, upper, root, material, component="head"):
    count = len(lower)
    assert count == len(upper)
    faces = [tuple(reversed(range(count))), tuple(range(count, 2*count))]
    faces += [(i, (i+1)%count, (i+1)%count+count, i+count) for i in range(count)]
    return mesh(name, lower+upper, faces, root, material, component)


def rod(name, start, end, radius, root, material, component="head", vertices=12):
    a, b = Vector(start)*STUD, Vector(end)*STUD
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius*STUD,
                                       depth=(b-a).length)
    obj = bpy.context.object
    attach(obj, name, root, material, component)
    obj.location = (a+b)/2
    obj.rotation_euler = (b-a).to_track_quat("Z", "Y").to_euler()
    obj.select_set(False)
    return obj


def conduit(name, points, root, material, component):
    data = bpy.data.curves.new(name, "CURVE")
    data.dimensions = "3D"
    data.resolution_u = 3
    data.bevel_depth = 0.13*STUD
    data.bevel_resolution = 0
    spline = data.splines.new("BEZIER")
    spline.bezier_points.add(len(points)-1)
    for control, position in zip(spline.bezier_points, points):
        control.co = Vector(position)*STUD
        control.handle_left_type = control.handle_right_type = "AUTO"
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    attach(obj, name, root, material, component)
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.convert(target="MESH")
    obj.select_set(False)
    return obj


def root(name, x):
    obj = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(obj)
    obj.location.x = x
    obj["status"] = STATUS
    obj["metres_per_comparison_stud"] = STUD
    obj["gameplay_scale_mapping"] = "NOT_DEFINED_BY_THIS_DRAFT"
    return obj


def worker(asset, mats):
    shell, support, living = mats
    # Ground clearance is excluded from accepted body height (8 plates).
    lower = [(-3,-2.5,.4),(-2.2,-4,.4),(2.2,-4,.4),(3,-2.5,.4),
             (2,4,.4),(-2,4,.4)]
    upper = [(x*.9,y*.9,1) for x,y,z in lower]
    taper("Worker_LoadBearingDeck", lower, upper, asset, support, "body")
    for sign, side in [(-1,"Left"),(1,"Right")]:
        outline=[(.5,-3.6),(2.2,-4),(3,-2.5),(2.1,3.2),(.65,4)]
        low=[(sign*x,y,1) for x,y in outline]
        high=[(sign*x*.9,y,3.6 if y<0 else 2.1) for x,y in outline]
        taper(f"Worker_{side}_ShellValve", low, high, asset, shell, "body")
        rod(f"Worker_{side}_ValveRoot", (sign*.5,-2.7,1.5),
            (sign*1.3,-2.7,1.5), .3, asset, support,"body")
        conduit(f"Worker_{side}_ProtectedLivingLink",
                [(sign*.25,-2.4,1.35),(sign*.25,0,1.65),(sign*.7,2.6,1.3)],
                asset,living,"body")
    box("Worker_InternalSpine",(.55,6.2,.65),(0,-.3,1.35),asset,support,"body")
    box("Worker_ToolRoot",(2.4,1,.7),(0,3.5,1.3),asset,support,"tool")
    for sign, side in [(-1,"Left"),(1,"Right")]:
        outline=[(sign*.55,3.9),(sign*1.2,4.2),(sign*.95,6),
                 (sign*.2,5.65),(sign*.45,5.2)]
        slab(f"Worker_{side}_UtilityJaw",outline,.95,1.65,asset,shell,"tool")
    # Four large connection studs, not rows of decorative microgreebles.
    for x in [-1.15,1.15]:
        for y in [-2.7,-1.7]:
            rod(f"Worker_Connector_{x}_{y}",(x,y,3.3),(x,y,3.5),.3,
                asset,support,"body")


def foundation(asset, mats):
    shell, support, living = mats
    for index,(sx,sy) in enumerate([(-1,-1),(1,-1),(-1,1),(1,1)]):
        box(f"Base_ContactPad_{index}",(3,3,.4),(sx*4.5,sy*4.5,.2),
            asset,support,"base")
        # Broad rigid support roots connect pad to core; no walking joints.
        low=[(sx*1.1,sy*1.1,.8),(sx*2.2,sy*.8,.8),
             (sx*5.1,sy*3.8,.4),(sx*3.8,sy*5.1,.4)]
        high=[(x,y,z+1) for x,y,z in low]
        taper(f"Base_FixedShellRoot_{index}",low,high,asset,shell,"base")
    box("Base_SupportSpine",(3,3,2.4),(0,0,1.6),asset,support,"base")
    low=[(-3,-2.2,.8),(-2.2,-3,.8),(2.2,-3,.8),(3,-2.2,.8),
         (3,2.2,.8),(2.2,3,.8),(-2.2,3,.8),(-3,2.2,.8)]
    high=[(x*.75,y*.75,3.1) for x,y,z in low]
    taper("Base_CentralCapsule",low,high,asset,shell,"base")
    rod("Base_HeadInterface",(0,0,2.9),(0,0,3.5),1.2,asset,support,"base")
    for sign in [-1,1]:
        conduit(f"Base_ProtectedLink_{sign}",[(sign*1.7,2.1,2.3),
                (sign*2.1,2.5,1.9),(sign*2.8,2.8,1.5)],asset,living,"base")


def ground_head(asset, mats):
    shell,support,living=mats
    rod("Ground_HeadSpine",(0,0,3.4),(0,0,4.8),.7,asset,support)
    for index in range(3):
        a=index*2*math.pi/3+math.pi/2
        outer=[(4.9*math.cos(a-.78+j*1.56/8),
                4.9*math.sin(a-.78+j*1.56/8)) for j in range(9)]
        inner=[(3*math.cos(a-.78+j*1.56/8),
                3*math.sin(a-.78+j*1.56/8)) for j in reversed(range(9))]
        slab(f"Ground_SourceInformedCrescent_{index}",outer+inner,4.8,6,
             asset,shell)
        c,s=math.cos(a),math.sin(a)
        rod(f"Ground_CrownRoot_{index}",(c*.5,s*.5,4.3),(c*3.6,s*3.6,5.2),
            .4,asset,support)
        rod(f"Ground_HorizontalEmitter_{index}",(c*4.2,s*4.2,5.4),
            (c*5.5,s*5.5,5.4),.35,asset,support)
        rod(f"Ground_EmitterLens_{index}",(c*5.5,s*5.5,5.4),
            (c*5.75,s*5.75,5.4),.4,asset,living)
        for offset in [-.38,.38]:
            q=a+offset
            x,y=3.6*math.cos(q),3.6*math.sin(q)
            rod(f"Ground_Connector_{index}_{offset}",(x,y,6-.25),
                (x,y,6),.3,asset,support)


def air_head(asset,mats):
    shell,support,living=mats
    rod("Air_HeadSpine",(0,0,3.4),(0,0,7.6),.7,asset,support)
    # Source-informed 7692 blade silhouette, reduced to a single broad panel
    # per side. These are authored proxies, not scaled literal LEGO parts.
    profile=[(-1.8,3.5),(-2,7.2),(-1.3,9.5),(-.3,10.4),
             (1.2,9.8),(2.7,7.5),(1.7,4.4)]
    for sign,side in [(-1,"Left"),(1,"Right")]:
        vertices=[(sign*x,y,z) for x in [1.2,1.95] for y,z in profile]
        n=len(profile)
        faces=[tuple(reversed(range(n))),tuple(range(n,2*n))]
        faces += [(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
        mesh(f"Air_{side}_SourceInformedBlade",vertices,faces,asset,shell)
        rod(f"Air_{side}_BladeRoot",(0,0,4),(sign*1.5,0,4),.4,asset,support)
    box("Air_TripleEmitterRoot",(2.1,2.1,.5),(0,.2,7.5),asset,support)
    for index,(x,y) in enumerate([(-.6,-.4),(.6,-.4),(0,.85)]):
        rod(f"Air_UpwardEmitter_{index}",(x,y,7.5),(x,y,9),.25,asset,support)
        rod(f"Air_EmitterLens_{index}",(x,y,9),(x,y,9.4),.3,asset,living)


def geometry(asset, component=None):
    return [obj for obj in asset.children if obj.type=="MESH" and
            (component is None or obj.get("component")==component)]


def dimensions(asset,component=None):
    points=[obj.matrix_local @ v.co for obj in geometry(asset,component)
            for v in obj.data.vertices]
    low=[min(v[i] for v in points) for i in range(3)]
    high=[max(v[i] for v in points) for i in range(3)]
    return [(high[i]-low[i])/STUD for i in range(3)]


def digest(asset):
    records=[]
    for obj in geometry(asset,"base"):
        records.append({"name":obj.name.split(".")[0],
                        "vertices":[[round(v,9) for v in obj.matrix_local @ p.co]
                                    for p in obj.data.vertices],
                        "faces":[list(p.vertices) for p in obj.data.polygons]})
    return hashlib.sha256(json.dumps(sorted(records,key=lambda a:a["name"]),
                         sort_keys=True,separators=(",",":")).encode()).hexdigest()


def validate(assets):
    bpy.context.view_layer.update()
    records={}
    expected={"Servitor":(6,10,8*PLATE/STUD),"GroundPulse":(12,12,15*PLATE/STUD),
              "AirLance":(12,12,26*PLATE/STUD)}
    for name,asset in assets.items():
        measured=dimensions(asset)
        target=expected[name]
        for value,limit in zip(measured,target):
            if abs(value-limit)>1e-4:
                raise AssertionError(f"{name} bound {measured} != {target}")
        if name=="Servitor":
            body=dimensions(asset,"body")
            if any(abs(a-b)>1e-4 for a,b in zip(body,(6,8,3.2))):
                raise AssertionError(f"Worker body mismatch: {body}")
        records[name]={"bounds_studs":measured,"mesh_modules":len(geometry(asset)),
                       "triangles":sum(len(p.vertices)-2 for obj in geometry(asset)
                                       for p in obj.data.polygons),"status":STATUS}
    a,b=digest(assets["GroundPulse"]),digest(assets["AirLance"])
    if a!=b:
        raise AssertionError("Defense foundations differ")
    if len([o for o in geometry(assets["GroundPulse"]) if "Crescent" in o.name])!=3:
        raise AssertionError("Ground head must retain exactly three shell lobes")
    for name,prefix,upward in [("GroundPulse","Ground_HorizontalEmitter_",False),
                               ("AirLance","Air_UpwardEmitter_",True)]:
        emitters=[obj for obj in geometry(assets[name]) if obj.name.startswith(prefix)]
        if len(emitters)!=3:
            raise AssertionError(f"Missing draft emitter cluster: {name}")
        for emitter in emitters:
            z=(emitter.matrix_local.to_3x3() @ Vector((0,0,1))).normalized().z
            if (upward and z<.99999) or (not upward and abs(z)>1e-5):
                raise AssertionError(f"Incorrect draft firing axis: {emitter.name}")
    records["shared_foundation_sha256"]=a
    records["checks"]={"accepted_envelopes":"PASS","worker_body_tool_separation":"PASS",
                       "identical_defense_foundations":"PASS","three_ground_lobes":"PASS",
                       "ground_horizontal_air_upward_axes":"PASS"}
    return records


def self_tests(assets):
    """Negative regressions mutate only newly created disposable geometry."""
    tests=[("worker_envelope",next(o for o in geometry(assets["Servitor"])
                                   if o.name=="Worker_LoadBearingDeck"),"scale",Vector((1.1,1,1))),
           ("shared_base",next(o for o in geometry(assets["AirLance"],"base")
                               if o.name.startswith("Base_CentralCapsule")),
            "location",Vector((.001,0,0))),
           ("three_lobes",next(o for o in geometry(assets["GroundPulse"]) if "Crescent" in o.name),
            "name","Ground_UnapprovedReplacementPanel"),
           ("air_axis",next(o for o in geometry(assets["AirLance"]) if "Air_UpwardEmitter_" in o.name),
            "rotation_euler",Vector((math.pi/2,0,0)))]
    validate(assets)
    for name,obj,property_name,value in tests:
        old=getattr(obj,property_name)
        saved=old if isinstance(old,str) else old.copy()
        try:
            setattr(obj,property_name,value)
            try:
                validate(assets)
            except AssertionError:
                print(f"DRAFT REGRESSION: PASS rejects {name}")
            else:
                raise RuntimeError(f"Draft regression missed {name}")
        finally:
            setattr(obj,property_name,saved)
        validate(assets)
    print("DRAFT SELF TESTS: PASS 5/5 (baseline + four negative guards)")


def text(name,caption,position,size,camera,mat):
    data=bpy.data.curves.new(name,"FONT")
    data.body=caption
    data.size=size
    data.align_x="CENTER"
    obj=bpy.data.objects.new(name,data)
    bpy.context.collection.objects.link(obj)
    obj.location=position
    obj.rotation_euler=camera.rotation_euler
    obj.data.materials.append(mat)
    return obj


def render_setup():
    scene=bpy.context.scene
    scene.render.engine="CYCLES"
    scene.cycles.device="CPU"
    scene.cycles.samples=32
    scene.cycles.use_denoising=True
    scene.render.resolution_x=1800
    scene.render.resolution_y=1050
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format="PNG"
    scene.view_settings.view_transform="Standard"
    scene.world.use_nodes=True
    scene.world.node_tree.nodes["Background"].inputs[0].default_value=(.78,.78,.78,1)
    scene.world.node_tree.nodes["Background"].inputs[1].default_value=.45
    bpy.ops.object.camera_add()
    camera=bpy.context.object
    camera.data.type="ORTHO"
    camera.data.ortho_scale=.54
    camera.data.lens=50
    scene.camera=camera
    bpy.ops.object.light_add(type="AREA",location=(-.15,.1,.45))
    # Physical millimetre-scale subjects need a proportionate light; a normal
    # metre-scale product light clips every grayscale material to white.
    bpy.context.object.data.energy=.6
    bpy.context.object.data.shape="DISK"
    bpy.context.object.data.size=.45
    return camera


def aim(camera,position,target):
    camera.location=position
    delta=Vector(target)-camera.location
    if abs(delta.x)<1e-8 and abs(delta.y)<1e-8:
        # Keep the same left-to-right subject order in the overhead board.
        camera.rotation_euler=(0,0,math.pi)
    else:
        camera.rotation_euler=delta.to_track_quat("-Z","Y").to_euler()


def main():
    marker=sys.argv.index("--")
    if sys.argv[marker+1]=="--audit-existing":
        bpy.ops.wm.open_mainfile(filepath=str(Path(sys.argv[marker+2]).resolve()))
        assets={name:bpy.data.objects[f"Draft_{name}"]
                for name in ["Servitor","GroundPulse","AirLance"]}
        print("SAVED ALIEN DRAFT AUDIT: PASS "+json.dumps(validate(assets)["checks"],sort_keys=True))
        return
    self_test=sys.argv[marker+1]=="--self-test"
    out=None if self_test else Path(sys.argv[marker+1]).resolve()
    # Explicitly bounded task-owned outputs; refuse to overwrite historical work.
    outputs=[] if self_test else [out/n for n in ["alien_shape_drafts_v1.blend","geometry_audit.json",
                             "alien_shape_drafts_three_quarter.png","alien_shape_drafts_top.png"]]
    if any(p.exists() for p in outputs):
        raise RuntimeError("Draft outputs already exist; choose a new version directory")
    if out is not None: out.mkdir(parents=True,exist_ok=True)
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    mats=(surface("Draft_BroadArmor",.48),surface("Draft_LoadBearingSupport",.2),
          surface("Draft_ProtectedLivingInterface",.66))
    assets={"Servitor":root("Draft_Servitor",.13),
            "GroundPulse":root("Draft_GroundPulse",0),
            "AirLance":root("Draft_AirLance",-.13)}
    worker(assets["Servitor"],mats)
    for name in ["GroundPulse","AirLance"]: foundation(assets[name],mats)
    ground_head(assets["GroundPulse"],mats)
    air_head(assets["AirLance"],mats)
    audit=validate(assets)
    if self_test:
        self_tests(assets)
        return
    # A schematic minifigure sized to the previously disclosed ~40 mm guide.
    ref=root("Reference_SchematicMinifigure",.235)
    box("Ref_LegLeft",(.7,1,1.65),(-.4,0,.825),ref,mats[1],"reference")
    box("Ref_LegRight",(.7,1,1.65),(.4,0,.825),ref,mats[1],"reference")
    box("Ref_Torso",(1.7,1,1.85),(0,0,2.575),ref,mats[0],"reference")
    rod("Ref_Head",(0,0,3.5),(0,0,5),.55,ref,mats[2],"reference")
    for x in [-1.05,1.05]: box(f"Ref_Arm_{x}",(.35,.7,1.6),(x,0,2.5),ref,mats[0],"reference")
    plate=root("Reference_4x6Plate",.235)
    plate.location.y=.065
    box("Ref_Plate",(4,6,.4),(0,0,.2),plate,mats[0],"reference")
    for x in [-1.5,-.5,.5,1.5]:
        for y in [-2.5,-1.5,-.5,.5,1.5,2.5]:
            rod(f"Ref_Stud_{x}_{y}",(x,y,.4),(x,y,.6),.3,plate,mats[1],"reference")
    floor=root("Reference_Floor",0)
    box("Floor",(200,200,.2),(0,0,-.14),floor,surface("Draft_Backdrop",.92),"reference")
    camera=render_setup()
    label_mat=surface("Draft_Label",.08)
    for name,asset in assets.items():
        text(f"Label_{name}",{"Servitor":"1  SERVITOR\n6 x 8 studs / 8 plates",
             "GroundPulse":"2  GROUND PULSE\n12 x 12 studs / 15 plates",
             "AirLance":"3  AIR LANCE\n12 x 12 studs / 26 plates"}[name],
             (asset.location.x,.095,.018),.006,camera,label_mat)
    text("Label_Reference","~40 mm\nschematic",(.235,.035,.014),.0045,camera,label_mat)
    text("Label_Plate","4 x 6 studs",(.235,.102,.012),.0045,camera,label_mat)
    for view,pos,target in [("three_quarter",(-.12,.55,.39),(.045,0,.04)),
                            ("top",(.045,0,.65),(.045,0,0))]:
        aim(camera,pos,target)
        for obj in bpy.context.scene.objects:
            if obj.type=="FONT": obj.rotation_euler=camera.rotation_euler
        bpy.context.scene.render.filepath=str(out/f"alien_shape_drafts_{view}.png")
        bpy.ops.render.render(write_still=True)
    aim(camera,(-.12,.55,.39),(.045,0,.04))
    for obj in bpy.context.scene.objects:
        if obj.type=="FONT": obj.rotation_euler=camera.rotation_euler
    bpy.ops.wm.save_as_mainfile(filepath=str(out/"alien_shape_drafts_v1.blend"))
    audit["blend_sha256"]=hashlib.sha256((out/"alien_shape_drafts_v1.blend").read_bytes()).hexdigest()
    audit["images"]={p.name:hashlib.sha256(p.read_bytes()).hexdigest()
                     for p in out.glob("*.png")}
    (out/"geometry_audit.json").write_text(json.dumps(audit,indent=2)+"\n")
    print("ALIEN SHAPE DRAFTS: PASS "+json.dumps(audit["checks"],sort_keys=True))


if __name__=="__main__": main()
