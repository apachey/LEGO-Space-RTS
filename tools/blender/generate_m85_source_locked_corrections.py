"""T082 source-locked, finished monochrome review models; no runtime imports.

Existing Blender: --background --python-exit-code 1 --python THIS -- OUTPUT.
Comparison coordinates are source-informed estimates, NOT measured LEGO parts,
approved gameplay footprints, or literal/production-ready set reconstruction.
This is the controlled native approach recommended after two free-form failures.
"""
from __future__ import annotations

import hashlib
import json
import math
from pathlib import Path
import sys

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import bpy
from mathutils import Vector
import generate_m85_alien_shape_drafts as d

STATUS = "CONTROLLED_SOURCE_LOCKED_APPEARANCE_REQUIRES_DIRECTOR_REVIEW"
S = d.STUD
SOURCES = {
    "MT101": ["https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517776.pdf",
              "https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517777.pdf"],
    "MX71": ["https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4524070.pdf"],
}


def group(name, parent=None, kind="structure", anchor=(0, 0, 0)):
    obj = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(obj)
    obj.parent = parent
    obj["kind"] = kind
    obj["anchor_studs"] = list(anchor)
    obj["status"] = STATUS
    return obj


def studs(name, positions, parent, mat):
    for i, (x, y, z) in enumerate(positions):
        d.rod(f"{name}_Stud_{i}", (x, y, z), (x, y, z + .18), .29, parent, mat)


def wheel(name, x, y, z, radius, width, parent, mats, kind):
    shell, dark, light, glass = mats
    w = group(name, parent, kind, (x, y, z))
    w["radius_studs"] = radius
    d.rod(name + "_Tyre", (x-width/2, y, z), (x+width/2, y, z), radius,
          w, shell, vertices=32)
    # Broad source-like transverse tread blocks, not fine surface greebles.
    for i in range(12):
        a = i*math.tau/12
        p = (x, y + (radius-.10)*math.sin(a), z + (radius-.10)*math.cos(a))
        tread = d.box(name + f"_Tread_{i}", (width+.08, radius*.35, .24), p, w, dark)
        # The shared box helper stores vertices in comparison coordinates.
        # Move the local origin to the tread centre before rotating its mesh.
        for vertex in tread.data.vertices:
            vertex.co -= Vector(p)*S
        tread.location = Vector(p)*S
        tread.rotation_euler.x = -a
    for sign in (-1, 1):
        rimx = x + sign*(width/2+.06)
        d.rod(name + f"_Rim_{sign}", (rimx, y, z), (rimx+sign*.12, y, z),
              radius*.64, w, dark, vertices=24)
        for i in range(8):
            a = i*math.tau/8
            d.rod(name + f"_Spoke_{sign}_{i}", (rimx+sign*.15, y, z),
                  (rimx+sign*.15, y+radius*.55*math.sin(a), z+radius*.55*math.cos(a)),
                  .12 if radius > 3 else .075, w, light)
        d.rod(name + f"_Hub_{sign}", (rimx, y, z), (rimx+sign*.25, y, z),
              radius*.23, w, light, vertices=16)
    return w


def canopy(name, parent, front, rear, width, mats):
    shell, dark, light, glass = mats
    fy, fz = front
    ry, rz = rear
    c = group(name, parent, "closed_cockpit")
    outline = [(-width, fy, fz), (width, fy, fz),
               (width, ry, fz), (-width, ry, fz)]
    top = [(-width*.76, fy, fz+.5), (width*.76, fy, fz+.5),
           (width*.85, ry, rz), (-width*.85, ry, rz)]
    d.taper(name + "_ClosedGlazing", outline, top, c, glass)
    for sign in (-1, 1):
        d.rod(name + f"_Frame_{sign}", (sign*width*.8, fy, fz+.5),
              (sign*width*.85, ry, rz), .17, c, light)
    d.rod(name + "_RearFrame", (-width*.85, ry, rz), (width*.85, ry, rz), .2, c, dark)
    d.rod(name + "_NoseFrame", (-width*.76, fy, fz+.5),
          (width*.76, fy, fz+.5), .18, c, dark)
    return c


def drill(name, start, end, radius, parent, mats):
    shell, dark, light, glass = mats
    d.rod(name + "_Shaft", start, end, radius*.22, parent, dark)
    a, b = Vector(start), Vector(end)
    axis = (b-a).normalized()
    rotation = axis.to_track_quat("Z", "Y")
    # Coarse stepped helical flutes: one tool, no cannon fused into its tip.
    for i in range(20):
        t = i/21
        p = a.lerp(b, t)
        r = radius*(1-t*.65)
        vertices = []
        for dz in (-.13, .13):
            for j in range(4):
                angle = i*.67 + j*math.pi/2
                v = p + rotation @ Vector((r*math.cos(angle), r*math.sin(angle), dz))
                vertices.append(tuple(v))
        d.mesh(name + f"_Flight_{i}", vertices,
               [(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],
               parent, light)


def mt101(asset, mats):
    shell, dark, light, glass = mats
    chassis = group("MT101_PermanentChassis", asset, "main_chassis")
    d.box("MT101_LoadFrame", (7, 23, 1.2), (0, -.4, 5.6), chassis, dark)
    for sign in (-1, 1):
        for i, y in enumerate((8, 0, -8)):
            wheel(f"MT101_Wheel_{sign}_{i}", sign*7, y, 4, 4, 3.1,
                  chassis, mats, "main_contact_wheel")
            d.rod(f"MT101_Suspension_{sign}_{i}", (sign*2.8,y,6.2),
                  (sign*7,y,4), .44, chassis, dark)
            d.rod(f"MT101_Damper_{sign}_{i}", (sign*5.6,y,4.4),
                  (sign*4.8,y,8), .29, chassis, light)
        low=[(sign*2.4,11,5.2),(sign*4.4,10,5.2),
             (sign*4.2,2.3,5.7),(sign*2.4,2.3,5.7)]
        high=[(x,y,z+(1 if y>9 else 5.5)) for x,y,z in low]
        d.taper(f"MT101_CabinShoulder_{sign}", low, high, chassis, light)
        # Curved large white donor panel, kept as a single clean shell.
        profile=[(-3,7),(-3,10),(-2,11.6),(0,12),(2,11.6),(3,10),(3,7)]
        verts=[(sign*3.1,y,z) for y,z in profile]+[(sign*4.5,y,z) for y,z in profile]
        n=len(profile)
        faces=[tuple(range(n)),tuple(range(n,2*n))]
        faces += [(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
        d.mesh(f"MT101_CurvedShoulder_{sign}", verts, faces, chassis, light)
        studs(f"MT101_Shoulder_{sign}", [(sign*3.8,y,11.9) for y in (-1,0,1)], chassis, shell)
        d.rod(f"MT101_FrontLamp_{sign}", (sign*2.8,11,5.6),
              (sign*2.8,11.5,5.6), .8, chassis, light, vertices=24)
        d.rod(f"MT101_FrontLampLens_{sign}", (sign*2.8,11.5,5.6),
              (sign*2.8,11.6,5.6), .52, chassis, glass, vertices=24)
    cabin = canopy("MT101_PermanentCockpit", chassis, (11.6,5.8), (3,11.6), 2.1, mats)
    cabin["permanent"] = True
    d.rod("MT101_CanopySpine", (0,11.6,6.35), (0,3,11.8), .21, cabin, dark)
    gun = group("MT101_UpperBallLauncher", chassis, "upper_gun", (0,-.4,12.5))
    d.rod("MT101_GunPivot", (0,-.4,10.8), (0,-.4,13), .62, gun, dark)
    d.box("MT101_GunBody", (2.8,4,1.1), (0,.8,13.3), gun, dark)
    for sign in (-1,1):
        d.rod(f"MT101_LauncherFork_{sign}", (sign*1.2,-.5,13.4),
              (sign*1.2,3.8,13.4), .24, gun, dark)
    d.rod("MT101_LauncherHoop", (-1.2,3.8,13.4), (1.2,3.8,13.4), .24, gun, dark)
    bpy.ops.mesh.primitive_uv_sphere_add(segments=16, ring_count=8, radius=.82*S,
                                        location=(0,2.4*S,13.4*S))
    d.attach(bpy.context.object,"MT101_LauncherBall",gun,glass,"gun")
    tool = group("MT101_ArticulatedDrill", chassis, "separate_drill", (-3.5,-2,9.5))
    points=[(-3.5,-2,9.5),(-7,-1,15),(-11.5,6,15)]
    for i in range(2):
        d.rod(f"MT101_DrillBoom_{i}", points[i], points[i+1], .58, tool, dark)
        d.rod(f"MT101_DrillElbow_{i}", tuple(Vector(points[i])-Vector((.55,0,0))),
              tuple(Vector(points[i])+Vector((.55,0,0))), .9, tool, shell)
    d.rod("MT101_DrillMotor", (-11.5,6,15), (-11.5,6,12.5), 1.2, tool, shell)
    drill("MT101_ContactAuger", (-11.5,6,12.5), (-11.5,6,4), 1.15, tool, mats)
    ship = group("MT101_DockedRearSpacecraft", asset, "docked_rear_craft")
    d.box("MT101_DockingCollar", (3,1,.6), (0,-4,10.1), chassis, dark)
    d.slab("MT101_RearSpacecraftDeck", [(-2.4,-3),(2.4,-3),(2.4,-14),(-2.4,-14)],
           10.45,11.1, ship, light)
    canopy("MT101_RearCraftCockpit", ship, (-4,11.1), (-8,12.8), 1.5, mats)
    for sign in (-1,1):
        d.rod(f"MT101_RearEquipmentTube_{sign}", (sign*3.4,-6,11),
              (sign*3.4,-12,11), 1, ship, shell, vertices=24)
        d.slab(f"MT101_RearWing_{sign}", [(sign*2,-8),(sign*7,-14),
                (sign*6,-16),(sign*2,-14)], 10.5,10.9,ship,dark)
        d.mesh(f"MT101_RearFin_{sign}",[(sign*5,-11,10.9),(sign*5,-15,10.9),
               (sign*5,-15,16.8)],[(0,1,2)],ship,dark)
    return asset


def mx71(asset, mats):
    shell, dark, light, glass = mats
    frame=group("MX71_Airframe",asset,"airframe")
    d.box("MX71_LongSpine", (4,31,1.4), (0,-4.5,12),frame,dark)
    d.slab("MX71_SpinePanels",[(-2,9),(2,9),(2,-16),(-2,-16)],12.7,13.4,frame,light)
    canopy("MX71_ClosedForwardCockpit",frame,(17,7.5),(7,14),2.2,mats)
    for sign in (-1,1):
        d.rod(f"MX71_CabinRail_{sign}",(sign*2.5,16,7.8),(sign*2.5,7,14.1),.35,frame,dark)
        low=[(sign*3,8,12),(sign*10.5,7,8.2),(sign*10.5,2.5,8.2),(sign*3,2.5,12)]
        high=[(x,y,z+.45) for x,y,z in low]
        d.taper(f"MX71_SlopedFrontWing_{sign}",low,high,frame,light)
        studs(f"MX71_Wing_{sign}",[(sign*x,y,12.45-(x-3)*.51) for x in (4,6,8,10)
                                   for y in (3.5,5,6.5)],frame,shell)
        d.rod(f"MX71_ShoulderDish_{sign}",(sign*4.4,8,12.2),(sign*5.1,8,12.2),1.2,frame,shell,vertices=24)
        for pair,x,y,z in [("Inner",3.6,10,10.3),("Outer",8.6,6.2,7.6)]:
            gun=group(f"MX71_{pair}Emitter_{sign}",frame,"airframe_emitter",(sign*x,y,z))
            gun["axis"]=[0,1,0]
            gun["pair"]=pair
            d.rod(gun.name+"_Mount",(sign*x,y-1,z),(sign*x,y,z),.55,gun,dark)
            d.rod(gun.name+"_ShortBarrel",(sign*x,y,z),(sign*x,y+2.3,z),.38,gun,shell)
            d.rod(gun.name+"_Tip",(sign*x,y+2.3,z),(sign*x,y+2.65,z),.48,gun,glass)
        d.rod(f"MX71_SideEquipment_{sign}",(sign*4.9,-5,11),(sign*4.9,-14,11),1.12,frame,shell,vertices=24)
        for y in (-5.8,-7.4,-9,-10.6,-12.2,-13.7):
            d.rod(f"MX71_TubeCollar_{sign}_{y}",(sign*4.9,y,11),
                  (sign*4.9,y-.15,11),1.17,frame,light,vertices=24)
        d.box(f"MX71_TailBoom_{sign}",(1.5,9,.8),(sign*6,-17,12),frame,dark)
        d.rod(f"MX71_RearEngine_{sign}",(sign*6,-17,12.6),(sign*6,-21,12.6),2.15,frame,light,vertices=32)
        d.rod(f"MX71_EngineRecess_{sign}",(sign*6,-21,12.6),(sign*6,-21.1,12.6),1.6,frame,dark,vertices=32)
        d.rod(f"MX71_EngineCore_{sign}",(sign*6,-21.1,12.6),(sign*6,-21.2,12.6),.95,frame,shell,vertices=24)
        d.mesh(f"MX71_VerticalTailFin_{sign}",[(sign*6,-17,14),(sign*6,-22,14),
                (sign*6,-22,20)],[(0,1,2)],frame,light)
        d.conduit(f"MX71_SourceHose_{sign}",[(sign*1.6,1,13.6),
                  (sign*3.2,-1,15),(sign*4.8,-5,12)],frame,dark,"hose")
        for y in (4,-7):
            d.rod(f"MX71_LandingLeg_{sign}_{y}",(sign*3.3,y,11),
                  (sign*3.3,y,5.3),.24,frame,dark)
            d.rod(f"MX71_LandingPad_{sign}_{y}",(sign*3.3,y,5.2),
                  (sign*3.3,y,5.4),.85,frame,shell,vertices=24)
    studs("MX71_Spine",[(x,y,13.4) for x in (-1,1) for y in (-14,-12,-10,0,2)],frame,shell)
    cargo=group("MX71_CloseDockedSourceRover",asset,"source_payload")
    d.box("MX71_PayloadFrame",(4.8,13,1),(0,0,4.3),cargo,dark)
    for sign in (-1,1):
        for i,y in enumerate((5,0,-5)):
            wheel(f"MX71_PayloadWheel_{sign}_{i}",sign*2.8,y,3.2,1.6,1.1,
                  cargo,mats,"payload_contact_wheel")
    canopy("MX71_SourcePayloadCockpit",cargo,(7,4.8),(1,8.4),1.9,mats)
    d.box("MX71_PayloadRearHousing",(4.3,5,3),(0,-3,6),cargo,light)
    drill("MX71_PayloadHorizontalAuger",(0,7,4.1),(0,13,4.1),.55,cargo,mats)
    dock=group("MX71_LoadCradle",frame,"load_cradle")
    d.box("MX71_CloseDockInterface",(3.8,2.4,2.2),(0,-3,9),dock,dark)
    return asset


def descendants(root):
    return list(root.children_recursive)


def controls(assets):
    result={}
    for name,root in assets.items():
        nodes=descendants(root)
        result[name]=[{"name":o.name,"kind":o["kind"],
                       "parent":o.parent.name,"anchor":list(o["anchor_studs"]),
                       "radius":o.get("radius_studs"),"axis":list(o["axis"]) if "axis" in o else None,
                       "pair":o.get("pair"),"permanent":o.get("permanent",False)}
                      for o in nodes if "kind" in o]
    return result


def validate(c):
    mt,mx=c["MT101"],c["MX71"]
    wheels=[o for o in mt if o["kind"]=="main_contact_wheel"]
    assert len(wheels)==6 and {tuple(o["anchor"]) for o in wheels}=={
        (s*7,y,4) for s in (-1,1) for y in (8,0,-8)},"six distinct symmetric MT contacts"
    assert all(o["anchor"][2]==o["radius"] for o in wheels),"MT contact plane"
    cabin=next(o for o in mt if o["name"]=="MT101_PermanentCockpit")
    assert cabin["permanent"] and cabin["kind"]=="closed_cockpit" and cabin["parent"]=="MT101_PermanentChassis","permanent closed MT cabin"
    assert len([o for o in mt if o["kind"]=="docked_rear_craft"])==1,"connected rear craft"
    gun=next(o for o in mt if o["kind"]=="upper_gun")
    tool=next(o for o in mt if o["kind"]=="separate_drill")
    assert gun["name"]!=tool["name"] and gun["parent"]==tool["parent"]=="MT101_PermanentChassis","independent tool roots"
    guns=[o for o in mx if o["kind"]=="airframe_emitter"]
    assert len(guns)==4,"four MX airframe emitters"
    for pair,x,y,z in [("Inner",3.6,10,10.3),("Outer",8.6,6.2,7.6)]:
        p=[o for o in guns if o["pair"]==pair]
        assert len(p)==2 and {tuple(o["anchor"]) for o in p}=={(-x,y,z),(x,y,z)},"two mirrored emitter pairs"
    assert all(o["axis"]==[0,1,0] for o in guns),"forward emitter axes"
    assert len([o for o in mx if o["kind"]=="source_payload"])==1,"source rover retained"
    assert len([o for o in mx if o["kind"]=="payload_contact_wheel"])==6,"six payload contacts, not aircraft guns"
    assert len([o for o in mx if o["kind"]=="load_cradle"])==1,"close docking interface"


def negative_tests(c):
    import copy
    cases=[("lost_MT_wheel","MT101","MT101_Wheel_-1_0","kind","missing"),
           ("lost_permanent_cabin","MT101","MT101_PermanentCockpit","permanent",False),
           ("cabin_detached_with_ship","MT101","MT101_PermanentCockpit","parent","MT101_DockedRearSpacecraft"),
           ("fused_tool_mount","MT101","MT101_ArticulatedDrill","parent","MT101_UpperBallLauncher"),
           ("asymmetric_MX_gun","MX71","MX71_OuterEmitter_-1","anchor",[-9,6.2,7.6]),
           ("missing_MX_gun","MX71","MX71_InnerEmitter_1","kind","missing"),
           ("rearward_MX_gun","MX71","MX71_InnerEmitter_-1","axis",[0,-1,0]),
           ("missing_rover","MX71","MX71_CloseDockedSourceRover","kind","missing"),
           ("missing_cradle","MX71","MX71_LoadCradle","kind","missing")]
    validate(c)
    for name,asset,node,key,value in cases:
        bad=copy.deepcopy(c)
        next(o for o in bad[asset] if o["name"]==node)[key]=value
        try:
            validate(bad)
        except (AssertionError,StopIteration):
            print("SOURCE CONTROL REGRESSION: PASS rejects",name)
        else:
            raise RuntimeError("Guard missed "+name)
    return [name for name,*_ in cases]


def scene_audit(assets):
    """Check actual saved mesh transforms, not only their labelled anchors."""
    c = controls(assets)
    validate(c)
    for w in [a for a in c["MT101"] if a["kind"] == "main_contact_wheel"]:
        tyre = bpy.data.objects[w["name"] + "_Tyre"]
        actual = tyre.matrix_world.translation / S
        assert (actual-Vector(w["anchor"])).length < .001, "real tyre centre differs from control"
        minimum = min((tyre.matrix_world @ Vector(v)).z for v in tyre.bound_box)/S
        assert abs(minimum) < .001, "real MT tyre does not contact ground"
    for gun in [a for a in c["MX71"] if a["kind"] == "airframe_emitter"]:
        barrel = bpy.data.objects[gun["name"] + "_ShortBarrel"]
        axis = barrel.matrix_world.to_quaternion() @ Vector((0,0,1))
        assert (axis-Vector((0,1,0))).length < .001, "actual barrel not forward"
        start = barrel.matrix_world.translation/S - Vector((0,1.15,0))
        assert (start-Vector(gun["anchor"])).length < .001, "actual barrel not at mirrored mount"
    cabin = bpy.data.objects["MT101_PermanentCockpit_ClosedGlazing"]
    assert len(cabin.data.polygons)==6 and cabin.parent.name=="MT101_PermanentCockpit", "closed permanent glazing lost"
    assert bpy.data.objects["MT101_DrillMotor"].parent.name=="MT101_ArticulatedDrill", "real drill mount fused"
    assert bpy.data.objects["MT101_GunBody"].parent.name=="MT101_UpperBallLauncher", "real gun mount fused"
    assert bpy.data.objects["MX71_CloseDockInterface"].parent.name=="MX71_LoadCradle", "real load cradle lost"
    print("SAVED SOURCE SCENE AUDIT: PASS actual tyre positions/contacts, four gun axes/mounts, closed cabin and independent tools/cradle")


def main():
    if "--audit" in sys.argv:
        source=Path(sys.argv[sys.argv.index("--audit")+1]).resolve()
        bpy.ops.wm.open_mainfile(filepath=str(source))
        assets={name:bpy.data.objects[name] for name in ("MT101","MX71")}
        bpy.context.view_layer.update()
        scene_audit(assets)
        negative_tests(controls(assets))
        if "--details" in sys.argv:
            scene=bpy.context.scene
            camera=scene.camera
            for name,filename,position,target,scale in [
                ("MT101","mt101_finished_overhead.png",(0,0,.55),(0,0,.05),.36),
                ("MX71","mx71_finished_front.png",(0,.60,.12),(0,0,.09),.28)]:
                destination=source.parent/filename
                if destination.exists():
                    raise RuntimeError("Refusing to overwrite retained detail view")
                for other,asset in assets.items():
                    for obj in descendants(asset):
                        obj.hide_render=other!=name
                d.aim(camera,position,target)
                camera.data.ortho_scale=scale
                scene.render.filepath=str(destination)
                bpy.ops.render.render(write_still=True)
        return
    output=Path(sys.argv[sys.argv.index("--")+1]).resolve()
    output.mkdir(parents=True,exist_ok=True)
    targets=[output/n for n in ("source_locked_corrections.blend","source_locked_audit.json",
                               "mt101_appearance.png","mx71_appearance.png")]
    if any(p.exists() for p in targets):
        raise RuntimeError("Refusing to overwrite retained review outputs")
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    mats=tuple(d.surface(name,v) for name,v in [("Matte mid shell",.32),("Matte black structure",.055),
                                               ("Matte light shell",.67),("Smoked closed glazing",.13)])
    assets={name:group(name,kind="review_asset") for name in ("MT101","MX71")}
    mt101(assets["MT101"],mats)
    mx71(assets["MX71"],mats)
    for obj in bpy.data.objects:
        if obj.type=="MESH":
            bevel=obj.modifiers.new("Small moulded edge","BEVEL")
            bevel.width=.00055
            bevel.segments=2
    bpy.ops.mesh.primitive_plane_add(size=200*S,location=(0,0,-.001))
    floor=bpy.context.object
    floor.name="ReviewGround"
    floor.data.materials.append(d.surface("Neutral ground",.88))
    scene=bpy.context.scene
    scene.render.engine="CYCLES"
    scene.cycles.samples=48
    scene.cycles.use_denoising=True
    scene.render.resolution_x=1500
    scene.render.resolution_y=1200
    scene.render.resolution_percentage=100
    scene.world.color=(.3,.3,.3)
    scene.view_settings.view_transform="AgX"
    bpy.ops.object.camera_add()
    camera=bpy.context.object
    camera.data.type="ORTHO"
    camera.data.clip_start=.001
    scene.camera=camera
    for name,pos,power,size in [("Key",(-.3,.2,.55),3,.4),
                                ("Fill",(.35,.1,.4),1.5,.3),("Rim",(0,-.35,.5),3,.3)]:
        bpy.ops.object.light_add(type="AREA",location=pos)
        lamp=bpy.context.object
        lamp.name=name
        lamp.data.energy=power
        lamp.data.shape="DISK"
        lamp.data.size=size
        d.aim(lamp,pos,(0,0,.07))
    c=controls(assets)
    bpy.context.view_layer.update()
    scene_audit(assets)
    guards=negative_tests(c)
    bpy.ops.wm.save_as_mainfile(filepath=str(targets[0]))
    images={}
    for name,filename,position,target,scale in [
        ("MT101","mt101_appearance.png",(-.38,.45,.29),(0,-.015,.065),.34),
        ("MX71","mx71_appearance.png",(-.38,.49,.28),(0,-.03,.082),.41)]:
        for other,asset in assets.items():
            for obj in descendants(asset):
                obj.hide_render=other!=name
        d.aim(camera,position,target)
        camera.data.ortho_scale=scale
        scene.render.filepath=str(output/filename)
        bpy.ops.render.render(write_still=True)
        images[name]={"file":filename,"sha256":hashlib.sha256((output/filename).read_bytes()).hexdigest()}
    audit={"schema":1,"status":STATUS,"productionAccepted":False,"canonImpact":"NONE",
           "reviewRoles":{"MT101":"UNREVIEWED_NATIVE_CANDIDATE",
                          "MX71":"INTERNAL_LAYOUT_ONLY_NOT_FINAL_APPEARANCE"},
           "stableIds":{"MT101":"unit.astronauts.mt101_armored_drilling_unit",
                        "MX71":"unit.astronauts.mx71_recon_dropship"},
           "approach":"single controlled native source-informed reconstruction, not free-form image retry",
           "dimensions":"estimated comparison studs; not exact part geometry or approved game scale",
           "sources":SOURCES,"controls":c,"negativeGuards":guards,"images":images,
           "nativeSource":{"file":targets[0].name,"sha256":hashlib.sha256(targets[0].read_bytes()).hexdigest()},
           "limitations":["Native control checks do not prove visibility in a raster",
                          "No physical buildability, exact part count, game import, animation or production approval",
                          "Source rover is visual payload, not a new roster unit or transport rule"]}
    targets[1].write_text(json.dumps(audit,indent=2)+"\n")
    print("SOURCE LOCKED CORRECTIONS: PASS baseline and",len(guards),"negative guards")


if __name__=="__main__":
    main()
