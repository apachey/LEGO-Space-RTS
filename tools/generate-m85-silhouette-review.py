#!/usr/bin/env python3
"""Generate deterministic T082 silhouette and cross-roster review artifacts."""

from __future__ import annotations

import argparse
import hashlib
import html
import json
import random
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
ROSTER = ROOT / "Content/Presentation/SuperScout/roster_identity_baseline.json"
CONCEPTS = ROOT / "Content/Presentation/SuperScout/silhouette_concepts.json"
CONTENT = ROOT / "Content/PrototypeEntities.json"
CONTRACT_PATHS = (
    ROOT / "Content/Presentation/SuperScout/rock_raiders_production_contracts.json",
    ROOT / "Content/Presentation/SuperScout/astronauts_production_contracts.json",
    ROOT / "Content/Presentation/SuperScout/aliens_production_contracts.json",
    ROOT / "Content/Presentation/SuperScout/martians_production_contracts.json",
)
OUTPUT = ROOT / "Docs/Development/M85SuperScout/Silhouettes"

FACTION_LABELS = {
    "RockRaiders": "Rock Raiders",
    "Astronauts": "Astronauts",
    "Aliens": "Aliens",
    "Martians": "Martians",
}
FACTION_SLUGS = {
    "RockRaiders": "rock_raiders",
    "Astronauts": "astronauts",
    "Aliens": "aliens",
    "Martians": "martians",
}
FOOTPRINT_CELLS = {"Tiny": 0.75, "Small": 1.25, "Medium": 1.85, "Large": 2.70, "Huge": 4.00}
VIEW_SCALE = {24: 1.0, 44: 24.0 / 44.0, 72: 24.0 / 72.0}
NOOP_MOBILITY = {"planted"}
NOOP_FRAMES = {"open", "sealed", "low", "curved"}


def esc(value: object) -> str:
    return html.escape(str(value), quote=True)


def polygon(points: str, fill: str = "#050505") -> str:
    return f'<polygon points="{points}" fill="{fill}"/>'


def rect(x: float, y: float, width: float, height: float, radius: float = 0, fill: str = "#050505") -> str:
    return f'<rect x="{x:g}" y="{y:g}" width="{width:g}" height="{height:g}" rx="{radius:g}" fill="{fill}"/>'


def ellipse(cx: float, cy: float, rx: float, ry: float, fill: str = "#050505") -> str:
    return f'<ellipse cx="{cx:g}" cy="{cy:g}" rx="{rx:g}" ry="{ry:g}" fill="{fill}"/>'


def circle(cx: float, cy: float, radius: float, fill: str = "#050505") -> str:
    return f'<circle cx="{cx:g}" cy="{cy:g}" r="{radius:g}" fill="{fill}"/>'


def stroke_line(x1: float, y1: float, x2: float, y2: float, width: float, color: str = "#050505") -> str:
    return (
        f'<line x1="{x1:g}" y1="{y1:g}" x2="{x2:g}" y2="{y2:g}" '
        f'stroke="{color}" stroke-width="{width:g}" stroke-linecap="round"/>'
    )


def base_shapes(core: str, view: str) -> list[str]:
    if core == "person":
        return [circle(0, -24, 12), rect(-11, -13, 22, 31, 4), polygon("-11,14 -2,14 -5,43 -17,43"), polygon("2,14 11,14 17,43 5,43")]
    if core in {"sled", "pod", "node", "landed_craft"}:
        return [polygon("-36,14 -28,-16 0,-30 31,-14 40,17 17,31 -23,29")]
    if core == "twin_hull":
        return [polygon("-39,34 -31,-38 -12,-41 -10,30"), polygon("10,30 12,-41 31,-38 39,34"), rect(-18, -3, 36, 23, 4)]
    if core in {"vehicle", "convoy", "platform"}:
        length = 70 if core == "convoy" else 57
        return [polygon(f"-34,{length/2:g} -40,-18 -27,-{length/2:g} 27,-{length/2:g} 40,-18 34,{length/2:g}")]
    if core in {"walker", "installation"}:
        if view == "top":
            return [polygon("-27,-31 25,-31 34,-6 22,19 -22,19 -34,-6"), rect(-18, 10, 36, 22, 5)]
        return [polygon("-30,-29 29,-29 35,-5 20,17 -20,17 -35,-5"), rect(-21, 8, 42, 23, 5)]
    if core in {"aircraft", "transformer"}:
        return [polygon("0,-46 13,-20 39,-5 42,11 15,8 10,39 0,47 -10,39 -15,8 -42,11 -39,-5 -13,-20")]
    if core == "disc":
        return [ellipse(0, 0, 45, 39), polygon("-36,-18 -50,3 -38,28 0,36 38,28 50,3 36,-18")]
    if core in {"facility", "cradle"}:
        return [polygon("-42,31 -45,-22 -29,-39 25,-42 43,-24 39,34 10,43 -28,40")]
    if core == "barrier":
        return [polygon("-48,-13 41,-19 49,15 -39,22")]
    if core == "tower":
        return [polygon("-18,39 -24,24 -13,-31 -5,-44 8,-44 15,-30 24,24 17,39")]
    if core == "pad":
        return [polygon("-46,36 -44,-38 30,-44 47,-18 44,39")]
    if core == "ring":
        return [ellipse(0, 0, 44, 40), ellipse(0, 0, 23, 20, "#ffffff")]
    if core == "core":
        return [polygon("0,-42 31,-17 38,22 0,43 -38,22 -31,-17")]
    if core == "drum":
        return [ellipse(0, -23, 29, 13), rect(-29, -23, 58, 51), ellipse(0, 28, 29, 13)]
    if core == "tube_span":
        return [rect(-48, -15, 96, 30, 15)]
    return []


def mobility_shapes(mobility: str, view: str) -> list[str]:
    shapes: list[str] = []
    if mobility in {"feet2", "feet2_long", "feet2_large"}:
        foot = 15 if mobility == "feet2_large" else 11
        leg = 11 if mobility == "feet2_long" else 8
        shapes += [stroke_line(-17, 8, -24, 38, leg), stroke_line(17, 8, 24, 38, leg), ellipse(-26, 40, foot, 6), ellipse(26, 40, foot, 6)]
    elif mobility in {"wheels2_large", "wheels4", "wheels4_large", "wheels6", "wheels_many", "wheels3"}:
        radius = 13 if "large" in mobility else 9
        if mobility == "wheels2_large":
            positions = [(-36, 4), (36, 4)]
        elif mobility == "wheels3":
            positions = [(0, -38), (-34, 25), (34, 25)]
        elif mobility in {"wheels6", "wheels_many"}:
            positions = [(-38, -25), (-40, 5), (-36, 31), (38, -25), (40, 5), (36, 31)]
        else:
            positions = [(-39, -25), (-39, 25), (39, -25), (39, 25)]
        shapes += [circle(x, y, radius) for x, y in positions]
    elif mobility == "tracks":
        shapes += [rect(-45, -31, 14, 67, 7), rect(31, -31, 14, 67, 7)]
    elif mobility in {"legs3", "legs4", "legs_many", "radial_feet", "clamps4", "braces"}:
        points = [(-35, 35), (35, 35), (-36, -30), (36, -30)]
        if mobility == "legs3":
            points = [(0, -43), (-38, 34), (38, 34)]
        elif mobility == "legs_many":
            points += [(-48, 2), (48, 2)]
        elif mobility == "braces":
            points = [(-42, 32), (42, 32)]
        for x, y in points:
            shapes += [stroke_line(x * 0.55, y * 0.45, x, y, 7), ellipse(x, y, 10, 5)]
    elif mobility in {"two_runners", "side_tubes", "couplers2"}:
        if mobility == "two_runners":
            shapes += [rect(-39, -42, 13, 83, 6), rect(26, -42, 13, 83, 6)]
        elif mobility == "side_tubes":
            shapes += [rect(-34, -41, 11, 84, 6), rect(23, -41, 11, 84, 6)]
        else:
            shapes += [ellipse(-45, 0, 13, 20), ellipse(45, 0, 13, 20)]
    elif mobility in {"twin_rotors", "short_wings", "swept_wings", "high_wings", "wide_wings", "swept_plate", "wing_halves"}:
        if mobility == "twin_rotors":
            shapes += [circle(-42, 0, 25), circle(42, 0, 25), circle(-42, 0, 8, "#ffffff"), circle(42, 0, 8, "#ffffff")]
        elif mobility == "wide_wings":
            shapes += [polygon("-5,-16 -49,-8 -48,14 -8,8"), polygon("5,-16 49,-8 48,14 8,8")]
        elif mobility == "high_wings":
            shapes += [polygon("-8,-25 -47,-4 -43,10 -7,2"), polygon("8,-25 47,-4 43,10 7,2")]
        elif mobility in {"swept_wings", "swept_plate"}:
            shapes += [polygon("-5,-23 -45,5 -39,21 -8,7"), polygon("5,-23 45,5 39,21 8,7")]
        elif mobility == "wing_halves":
            shapes += [polygon("-4,-31 -47,-9 -40,28 -7,11"), polygon("4,-31 47,-9 40,28 7,11")]
        else:
            shapes += [polygon("-6,-14 -35,-3 -31,12 -7,6"), polygon("6,-14 35,-3 31,12 7,6")]
    elif mobility == "hover":
        shapes += [ellipse(-25, 29, 15, 7), ellipse(25, 29, 15, 7)]
    return shapes


def frame_shapes(frame: str) -> list[str]:
    shapes: list[str] = []
    if frame in {"gantry", "heavy_gantry", "clean_gantry", "open_gantry"}:
        width = 10 if frame == "heavy_gantry" else 7
        shapes += [rect(-42, -35, width, 73, 3), rect(42 - width, -35, width, 73, 3), rect(-42, -39, 84, width, 3)]
    elif frame == "split_towers":
        shapes += [polygon("-45,38 -43,-31 -19,-45 -13,35"), polygon("13,35 19,-45 43,-31 45,38"), rect(-23, -42, 46, 8, 3)]
    elif frame in {"open_complex", "open_network"}:
        shapes += [rect(-45, 25, 31, 13, 3), rect(15, -39, 25, 14, 3), stroke_line(-28, 25, 26, -25, 7)]
    elif frame in {"reinforced_ring", "radial", "radial_shell"}:
        shapes += [ellipse(0, 0, 43, 39), ellipse(0, 0, 27, 23, "#ffffff")]
        if frame == "radial_shell":
            shapes += [rect(-7, -45, 14, 90)]
    elif frame in {"side_pods", "modules", "tripartite"}:
        shapes += [rect(-43, -15, 17, 38, 5), rect(26, -15, 17, 38, 5)]
        if frame == "modules":
            shapes += [rect(-19, 24, 38, 16, 4)]
    elif frame in {"bridge", "raised_platform"}:
        shapes += [rect(-39, -31, 14, 63, 5), rect(25, -31, 14, 63, 5), rect(-27, -9, 54, 18, 4)]
    elif frame in {"siege_split", "split_shell"}:
        shapes += [polygon("-47,-30 -11,-22 -16,31 -44,39"), polygon("47,-30 11,-22 16,31 44,39")]
    elif frame in {"high_cab", "rear_cab", "wide_cockpit", "wedge_torso"}:
        shapes += [polygon("-25,4 -20,-31 20,-31 29,4")]
    elif frame == "turret":
        shapes += [circle(0, -7, 22), rect(-9, -35, 18, 29, 4)]
    elif frame == "mast":
        shapes += [rect(-6, -43, 12, 80), polygon("-25,37 25,37 16,27 -16,27")]
    elif frame == "directional":
        shapes += [polygon("0,-48 14,-25 6,-28 6,39 -6,39 -6,-28 -14,-25")]
    elif frame == "asymmetric":
        shapes += [rect(-43, -28, 17, 45, 4), circle(31, 21, 15)]
    elif frame in {"articulated", "deploying", "hinged", "unfolding"}:
        shapes += [circle(-27, 6, 8), circle(27, 6, 8)]
    elif frame in {"feed_line", "transfer_line"}:
        shapes += [rect(-42, 18, 84, 13, 4), polygon("-45,12 -31,5 -31,38 -45,31")]
    elif frame == "service_frame":
        shapes += [rect(-40, -31, 10, 65, 3), rect(30, -31, 10, 65, 3), stroke_line(-30, -25, 30, -9, 7)]
    elif frame == "work_rig":
        shapes += [rect(18, -35, 17, 58, 4), stroke_line(26, -31, -2, -45, 7), rect(-34, 17, 24, 19, 3)]
    elif frame == "suspension":
        shapes += [stroke_line(-31, -18, -42, 29, 6), stroke_line(31, -18, 42, 29, 6)]
    elif frame == "suspended":
        shapes += [stroke_line(-37, -25, -25, 27, 6), stroke_line(37, -25, 25, 27, 6), rect(-24, 20, 48, 9, 3)]
    elif frame == "pedestal":
        shapes += [polygon("-27,39 -18,10 18,10 27,39"), rect(-13, -30, 26, 43, 4)]
    elif frame == "eccentric":
        shapes += [rect(17, -44, 18, 67, 4), circle(-20, 17, 18), stroke_line(-9, 8, 19, -22, 7)]
    elif frame == "exposed_base":
        shapes += [polygon("-39,37 -31,21 31,21 39,37"), circle(-22, 26, 7, "#ffffff"), circle(22, 26, 7, "#ffffff")]
    elif frame == "handling_body":
        shapes += [rect(-37, -18, 74, 37, 5), rect(-27, 18, 54, 17, 4)]
    elif frame == "tube_base":
        shapes += [rect(-41, 20, 82, 17, 7), circle(-34, 28, 5, "#ffffff"), circle(34, 28, 5, "#ffffff")]
    elif frame == "transparent_span":
        shapes += [rect(-44, -19, 88, 38, 18), rect(-37, -10, 74, 20, 10, "#ffffff")]
    return shapes


def hero_shapes(hero: str) -> list[str]:
    shapes: list[str] = []
    if hero in {"hand_tool", "field_pack_tool"}:
        shapes += [stroke_line(12, -2, 34, 31, 7), polygon("28,26 45,39 38,45 23,32")]
        if hero == "field_pack_tool":
            shapes += [rect(-19, -10, 9, 27, 3)]
    elif hero in {"scanner_front", "sensor_bar"}:
        shapes += [stroke_line(0, -23, 0, -43, 6), rect(-23, -48, 46, 8, 4)]
    elif hero == "twin_saws":
        shapes += [circle(-25, -34, 14), circle(25, -34, 14), circle(-25, -34, 5, "#ffffff"), circle(25, -34, 5, "#ffffff")]
    elif hero in {"long_drill", "armored_drill"}:
        shapes += [polygon("0,-52 -13,-28 13,-28"), rect(-8, -31, 16, 25, 3)]
    elif hero == "scoop":
        shapes += [polygon("-43,-48 43,-48 31,-25 -31,-25"), stroke_line(-28, -25, -18, -4, 6), stroke_line(28, -25, 18, -4, 6)]
    elif hero == "rear_thrusters":
        shapes += [circle(-25, 41, 12), circle(25, 41, 12)]
    elif hero in {"cargo_cradle", "rover_cradle", "suspended_cradle"}:
        shapes += [rect(-24, 9, 48, 31, 5), rect(-15, 16, 30, 17, 3, "#ffffff")]
    elif hero == "command_crane":
        shapes += [stroke_line(11, 24, 31, -37, 7), stroke_line(31, -37, 47, -14, 6), circle(47, -10, 5)]
    elif hero in {"crusher_stack", "three_tiers"}:
        shapes += [rect(-19, -42, 38, 18, 4), rect(-25, -18, 50, 18, 4), rect(-31, 6, 62, 21, 4)]
    elif hero == "engine_exhaust":
        shapes += [rect(-26, -27, 52, 38, 5), rect(-20, -44, 9, 20, 4), rect(11, -44, 9, 20, 4)]
    elif hero in {"repair_arms", "mixed_dock_arms", "unequal_overhead_arms", "feed_arms"}:
        shapes += [stroke_line(-38, -31, -12, 4, 7), stroke_line(38, -25, 15, 9, 9), circle(-10, 7, 6), circle(14, 12, 7)]
    elif hero in {"tool_tower", "tower_drill"}:
        shapes += [rect(-9, -48, 18, 63, 4), polygon("0,-58 -17,-39 17,-39")]
    elif hero in {"crystal_chamber", "faceted_crystal", "raised_core", "central_core"}:
        shapes += [polygon("0,-34 18,-11 13,25 0,39 -13,25 -18,-11")]
    elif hero == "ram_face":
        shapes += [polygon("-48,-18 48,-18 37,7 -37,7")]
    elif hero in {"tracking_cutter", "replaceable_turret", "pivot_weapon"}:
        shapes += [circle(0, -31, 16), rect(-5, -52, 10, 24, 3)]
    elif hero == "module_bay":
        shapes += [rect(-23, -8, 46, 35, 4), rect(-14, -1, 28, 20, 3, "#ffffff")]
    elif hero == "single_jet":
        shapes += [rect(-10, -39, 20, 74, 8), circle(0, 36, 13)]
    elif hero == "solar_wings":
        shapes += [rect(-48, -18, 34, 36, 2), rect(14, -18, 34, 36, 2), stroke_line(-9, 0, 9, 0, 7)]
    elif hero == "compact_canopy":
        shapes += [ellipse(0, -13, 12, 20)]
    elif hero == "folding_wings":
        shapes += [polygon("-9,-8 -48,-27 -42,24 -10,12"), polygon("9,-8 48,-27 42,24 10,12"), circle(-11, 3, 6), circle(11, 3, 6)]
    elif hero == "harvest_head":
        shapes += [rect(-35, -48, 70, 20, 5), polygon("-33,-50 -23,-61 -12,-50 0,-61 12,-50 23,-61 33,-50")]
    elif hero == "claw_and_gun":
        shapes += [stroke_line(-18, -5, -46, -33, 8), circle(-48, -35, 10), stroke_line(18, -7, 44, -39, 7)]
    elif hero == "command_spine":
        shapes += [rect(-9, -46, 18, 88, 5), rect(-18, -23, 36, 22, 4)]
    elif hero == "a_frame_tower":
        shapes += [stroke_line(-26, 32, -5, -46, 10), stroke_line(26, 32, 5, -46, 10), rect(-16, -12, 32, 10, 3)]
    elif hero == "module_racks":
        shapes += [rect(-42, -27, 16, 54, 3), rect(26, -17, 16, 44, 3)]
    elif hero == "assembly_rails":
        shapes += [rect(-31, -39, 9, 75, 3), rect(22, -39, 9, 75, 3)]
    elif hero == "service_arms_beacon":
        shapes += [stroke_line(-35, 21, -12, -3, 6), stroke_line(35, 21, 12, -3, 6), circle(39, -35, 8)]
    elif hero == "drill_receiver":
        shapes += [polygon("0,-48 -15,-28 15,-28"), rect(-30, -20, 60, 18, 3)]
    elif hero == "single_manipulator":
        shapes += [stroke_line(8, -6, 38, 26, 8), circle(40, 29, 8)]
    elif hero == "twin_arches":
        shapes += [stroke_line(-27, 21, -17, -27, 7), stroke_line(27, 21, 17, -27, 7), stroke_line(-17, -27, 0, -39, 7), stroke_line(17, -27, 0, -39, 7)]
    elif hero == "twin_prongs":
        shapes += [polygon("-36,9 -27,-52 -14,-20 -16,14"), polygon("36,9 27,-52 14,-20 16,14")]
    elif hero == "rotating_nose":
        shapes += [polygon("0,-52 -18,-28 18,-28"), circle(0, -20, 11)]
    elif hero == "open_channel_bays":
        shapes += [rect(-11, -43, 22, 86, 3, "#ffffff"), rect(-6, -36, 12, 27, 2)]
    elif hero == "opposed_couplers":
        shapes += [polygon("-46,-16 -10,-27 -15,-5 -43,8"), polygon("46,-16 10,-27 15,-5 43,8"), circle(0, 7, 13)]
    elif hero == "shallow_operator_wedge":
        shapes += [polygon("-27,-22 27,-22 18,2 -18,2")]
    elif hero == "asymmetric_rear":
        shapes += [circle(-26, 34, 13), rect(17, 23, 22, 22, 4)]
    elif hero == "orange_nozzle_cluster":
        shapes += [circle(-18, -39, 8), circle(0, -45, 9), circle(18, -39, 8)]
    elif hero == "pneumatic_lift":
        shapes += [circle(0, 22, 24), circle(0, 22, 13, "#ffffff"), rect(-15, -28, 30, 30, 5)]
    elif hero == "offset_pedestal":
        shapes += [rect(11, -37, 22, 51, 5), circle(22, -39, 13)]
    elif hero == "drill_and_claw":
        shapes += [stroke_line(-22, -3, -44, 27, 8), polygon("-48,37 -56,20 -40,26"), stroke_line(22, -4, 43, 29, 8), circle(45, 32, 9)]
    elif hero == "twin_emitter_arms":
        shapes += [stroke_line(-20, -5, -49, 30, 9), stroke_line(20, -5, 49, 30, 9), rect(-55, 26, 15, 20, 5), rect(40, 26, 15, 20, 5)]
    elif hero in {"crane_claw", "crane_intake"}:
        shapes += [stroke_line(-15, 20, 29, -43, 8), stroke_line(29, -43, 49, -18, 7), circle(48, -13, 9)]
    elif hero == "tube_pump_towers":
        shapes += [circle(-13, 3, 23), rect(17, -44, 18, 69, 4), rect(-43, -23, 17, 49, 4)]
    elif hero == "tube_junction":
        shapes += [circle(0, 0, 22), rect(-48, -7, 96, 14, 7), rect(-7, -47, 14, 94, 7)]
    elif hero == "route_drum_arms":
        shapes += [circle(0, -24, 18), stroke_line(-9, -14, -39, 12, 7), stroke_line(9, -14, 40, 4, 7)]
    elif hero == "paddle_arm":
        shapes += [stroke_line(-9, 24, 20, -19, 12), stroke_line(20, -19, 39, -40, 10), rect(30, -51, 25, 16, 4)]
    elif hero == "tracking_twin_arms":
        shapes += [stroke_line(-6, -24, -33, -43, 7), stroke_line(6, -24, 33, -43, 7), rect(-43, -49, 18, 10, 3), rect(25, -49, 18, 10, 3)]
    elif hero == "hypersled_path":
        shapes += [rect(-38, -7, 76, 14, 7), polygon("18,-13 42,0 18,13")]
    return shapes


def negative_shapes(negative: str) -> list[str]:
    if negative in {"cockpit", "rider_gap", "passenger_gap", "cab_gap", "core_gap"}:
        return [ellipse(0, -3, 11, 16, "#ffffff")]
    if negative in {"cargo_gap", "module_gap", "central_dock", "process_gap", "chamber_gap", "conductor_gap"}:
        return [rect(-14, -3, 28, 30, 5, "#ffffff")]
    if negative in {"leg_gap", "leg_gaps", "stance_gap"}:
        return [polygon("-12,5 12,5 19,43 -19,43", "#ffffff")]
    if negative in {"drive_through", "wide_exit", "huge_exit", "walker_exit", "shuttle_lanes", "launch_lane"}:
        return [rect(-19, -2, 38, 46, 4, "#ffffff")]
    if negative in {"material_channel", "launch_channel", "rock_channel", "machinery_channel"}:
        return [rect(-8, -43, 16, 84, 3, "#ffffff")]
    if negative in {"fork_gap", "tube_bore"}:
        return [rect(-12, -45, 24, 74, 8, "#ffffff")]
    if negative in {"wing_gap", "wing_notches", "deck_notches", "pod_gaps", "module_gaps", "hinge_gaps"}:
        return [polygon("-8,-15 0,-31 8,-15 5,11 0,21 -5,11", "#ffffff")]
    if negative in {"arm_gap", "brace_gaps", "axle_gaps", "service_gap", "machine_gap", "clamp_gaps", "head_gap"}:
        return [ellipse(0, 14, 10, 15, "#ffffff")]
    if negative in {"tube_and_air_lanes", "worker_and_tube_exits", "tube_throats", "seam_entrances", "crew_lanes"}:
        return [rect(-10, -44, 20, 36, 4, "#ffffff"), rect(-36, 12, 72, 17, 5, "#ffffff")]
    if negative == "panel_gap":
        return [rect(-8, -22, 16, 44, 3, "#ffffff")]
    if negative == "underslung_gap":
        return [rect(-14, 6, 28, 30, 4, "#ffffff")]
    if negative == "core_notch":
        return [polygon("-10,-9 0,-22 10,-9 7,12 -7,12", "#ffffff")]
    return []


def render_profile(profile: dict, x: float, y: float, scale: float, view: str = "game") -> str:
    width, height, length = profile["ratios"]
    if view == "front":
        sx, sy = width, height
    elif view == "side":
        sx, sy = length, height
    elif view == "top":
        sx, sy = width, length
    else:
        sx, sy = width, max(0.35, length * 0.62 + height * 0.38)
    largest = max(sx, sy)
    sx = scale * sx / largest
    sy = scale * sy / largest
    parts = mobility_shapes(profile["mobility"], view)
    parts += base_shapes(profile["core"], view)
    parts += frame_shapes(profile["frame"])
    parts += hero_shapes(profile["hero"])
    parts += negative_shapes(profile["negative"])
    return f'<g transform="translate({x:g} {y:g}) scale({sx:g} {sy:g})">{"".join(parts)}</g>'


def svg_document(width: int, height: int, title: str, body: str) -> str:
    return (
        '<?xml version="1.0" encoding="UTF-8"?>\n'
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">\n'
        f'<title>{esc(title)}</title>\n'
        '<rect width="100%" height="100%" fill="#ffffff"/>\n'
        '<style>text{font-family:Arial,Helvetica,sans-serif}.title{font-size:28px;font-weight:700;fill:#141518}.sub{font-size:14px;fill:#5e626a}.code{font-size:14px;font-weight:700;letter-spacing:1px;fill:#34373d}.name{font-size:12px;font-weight:700;fill:#17181b}.meta{font-size:10px;fill:#656a72}.label{font-size:11px;font-weight:700;fill:#3d4148}</style>\n'
        f'{body}\n</svg>\n'
    )


def blind_codes(assets: list[dict]) -> list[tuple[str, dict]]:
    shuffled = list(assets)
    random.Random(85082).shuffle(shuffled)
    return [(f"S{index:02d}", asset) for index, asset in enumerate(shuffled, 1)]


def blind_board(camera_width: int, coded_assets: list[tuple[str, dict]], profiles: dict[str, dict]) -> str:
    columns, cell_w, cell_h = 11, 330, 280
    rows = (len(coded_assets) + columns - 1) // columns
    width, height = columns * cell_w + 80, rows * cell_h + 130
    body = [f'<text x="40" y="45" class="title">T082 blind silhouettes — {camera_width}-cell camera</text>']
    body.append('<text x="40" y="72" class="sub">No names, factions, roles, colours, icons or selection rings. Record the asset you believe each S-code represents.</text>')
    for index, (code, asset) in enumerate(coded_assets):
        col, row = index % columns, index // columns
        x, y = 40 + col * cell_w, 95 + row * cell_h
        body.append(f'<rect x="{x}" y="{y}" width="300" height="250" rx="8" fill="none" stroke="#d9dbe0"/>')
        footprint = asset["footprint"]
        icon_scale = 0.64 * FOOTPRINT_CELLS[footprint] * VIEW_SCALE[camera_width]
        body.append(render_profile(profiles[asset["stableId"]], x + 150, y + 113, icon_scale, "game"))
        body.append(f'<text x="{x + 150}" y="{y + 232}" text-anchor="middle" class="code">{code}</text>')
    return svg_document(width, height, f"T082 blind silhouettes {camera_width} cells", "\n".join(body))


def blind_board_page(
    camera_width: int,
    coded_assets: list[tuple[str, dict]],
    profiles: dict[str, dict],
    page: int,
    total_pages: int,
) -> str:
    columns, cell_w, cell_h = 6, 250, 240
    rows = (len(coded_assets) + columns - 1) // columns
    width, height = columns * cell_w + 80, rows * cell_h + 130
    body = [f'<text x="40" y="45" class="title">T082 blind silhouettes — {camera_width}-cell camera — page {page}/{total_pages}</text>']
    body.append('<text x="40" y="72" class="sub">No names, factions, roles, colours, icons or selection rings. Use the same S-code across all camera widths.</text>')
    for index, (code, asset) in enumerate(coded_assets):
        col, row = index % columns, index // columns
        x, y = 40 + col * cell_w, 95 + row * cell_h
        body.append(f'<rect x="{x}" y="{y}" width="220" height="210" rx="8" fill="none" stroke="#d9dbe0"/>')
        footprint = asset["footprint"]
        icon_scale = 0.48 * FOOTPRINT_CELLS[footprint] * VIEW_SCALE[camera_width]
        body.append(render_profile(profiles[asset["stableId"]], x + 110, y + 92, icon_scale, "game"))
        body.append(f'<text x="{x + 110}" y="{y + 194}" text-anchor="middle" class="code">{code}</text>')
    return svg_document(width, height, f"T082 blind silhouettes {camera_width} cells page {page}", "\n".join(body))


def proportion_board(faction: str, assets: list[dict], profiles: dict[str, dict]) -> str:
    columns, cell_w, cell_h = 3, 470, 220
    rows = (len(assets) + columns - 1) // columns
    width, height = columns * cell_w + 80, rows * cell_h + 130
    body = [f'<text x="40" y="45" class="title">T082 {esc(FACTION_LABELS[faction])} — front / side / top proportion board</text>']
    body.append('<text x="40" y="72" class="sub">Labeled production-planning view. Shared footprint scale; black concept geometry only.</text>')
    for index, asset in enumerate(assets):
        col, row = index % columns, index // columns
        x, y = 40 + col * cell_w, 96 + row * cell_h
        body.append(f'<rect x="{x}" y="{y}" width="440" height="190" rx="8" fill="none" stroke="#d9dbe0"/>')
        body.append(f'<text x="{x + 16}" y="{y + 24}" class="name">{esc(asset["displayName"])}</text>')
        body.append(f'<text x="{x + 424}" y="{y + 24}" text-anchor="end" class="meta">{esc(asset["kind"])} · {esc(asset["footprint"])}</text>')
        scale = 0.30 * FOOTPRINT_CELLS[asset["footprint"]]
        for view_index, view in enumerate(("front", "side", "top")):
            cx = x + 76 + view_index * 144
            body.append(render_profile(profiles[asset["stableId"]], cx, y + 105, scale, view))
            body.append(f'<text x="{cx}" y="{y + 176}" text-anchor="middle" class="label">{view.upper()}</text>')
    return svg_document(width, height, f"T082 {FACTION_LABELS[faction]} proportion board", "\n".join(body))


def scale_lineup(assets: list[dict], profiles: dict[str, dict]) -> str:
    grouped: dict[str, list[dict]] = {name: [] for name in FOOTPRINT_CELLS}
    for asset in assets:
        grouped[asset["footprint"]].append(asset)
    width, height = 2100, 1130
    body = ['<text x="40" y="45" class="title">T082 full-roster relative footprint and scale lineup</text>']
    body.append('<text x="40" y="72" class="sub">Labeled audit view. A single cell-to-pixel rule is applied inside each row; profile ratios remain asset-specific.</text>')
    y = 120
    for footprint, row_assets in grouped.items():
        body.append(f'<text x="40" y="{y + 20}" class="label">{footprint.upper()}</text>')
        body.append(f'<line x1="135" y1="{y + 92}" x2="2060" y2="{y + 92}" stroke="#b8bbc2"/>')
        spacing = 1900 / max(1, len(row_assets))
        scale = 0.25 * FOOTPRINT_CELLS[footprint]
        for index, asset in enumerate(row_assets):
            cx = 150 + spacing * (index + 0.5)
            body.append(render_profile(profiles[asset["stableId"]], cx, y + 55, scale, "game"))
            short = asset["displayName"] if len(asset["displayName"]) <= 25 else asset["displayName"][:23] + "…"
            body.append(f'<text x="{cx}" y="{y + 112}" text-anchor="middle" class="meta">{esc(short)}</text>')
        y += 200
    return svg_document(width, height, "T082 relative scale lineup", "\n".join(body))


def building_skyline(buildings: list[dict], profiles: dict[str, dict]) -> str:
    columns, cell_w, cell_h = 4, 420, 220
    rows = (len(buildings) + columns - 1) // columns
    width, height = columns * cell_w + 80, rows * cell_h + 130
    body = ['<text x="40" y="45" class="title">T082 building skyline audit</text>']
    body.append('<text x="40" y="72" class="sub">Side elevation at shared footprint scale. Entrances and channels remain white negative space.</text>')
    for index, asset in enumerate(buildings):
        col, row = index % columns, index // columns
        x, y = 40 + col * cell_w, 95 + row * cell_h
        body.append(f'<rect x="{x}" y="{y}" width="390" height="190" rx="8" fill="none" stroke="#d9dbe0"/>')
        body.append(f'<text x="{x + 14}" y="{y + 23}" class="name">{esc(asset["displayName"])}</text>')
        body.append(f'<text x="{x + 376}" y="{y + 23}" text-anchor="end" class="meta">{esc(FACTION_LABELS[asset["faction"]])} · {esc(asset["footprint"])}</text>')
        scale = 0.30 * FOOTPRINT_CELLS[asset["footprint"]]
        body.append(render_profile(profiles[asset["stableId"]], x + 195, y + 102, scale, "side"))
        body.append(f'<line x1="{x + 30}" y1="{y + 162}" x2="{x + 360}" y2="{y + 162}" stroke="#8f939b"/>')
    return svg_document(width, height, "T082 building skyline audit", "\n".join(body))


def building_matrix(buildings: list[dict], profiles: dict[str, dict], definitions: dict[str, dict], contracts: dict[str, dict]) -> str:
    lines = [
        "# M8.5 T082 — building skyline, entrance, exit and network matrix",
        "",
        "This is a labeled cross-roster planning audit. It does not replace the blind silhouette review and does not approve production modeling. The runtime production-exit envelope is quoted exactly; the visual lane is a concept obligation, not new gameplay.",
        "",
        "| Building | Skyline obligation | Entrance / negative-space obligation | Authoritative production exit | Visible network / service connection | Draft audit |",
        "|---|---|---|---|---|---|",
    ]
    for asset in buildings:
        stable_id = asset["stableId"]
        definition = definitions[stable_id]
        contract = contracts[stable_id]
        exit_data = definition.get("productionExit")
        if exit_data:
            exit_text = f"{exit_data['width']}×{exit_data['depth']} cells; clears `{exit_data['largestFootprint']}`"
        else:
            exit_text = "None — no production exit"
        sockets = [
            socket for socket in contract["sockets"]
            if any(token in socket for token in ("Link", "Tube", "Network", "Resource", "Service", "Worksite", "ProductionExit"))
        ]
        connection_text = ", ".join(f"`{socket}`" for socket in sockets) if sockets else "No explicit network/service socket"
        negative = profiles[stable_id]["negative"].replace("_", " ")
        audit = "READY FOR DIRECTOR REVIEW" if negative not in {"none", ""} else "HOLD — no readable opening"
        lines.append(
            f"| {asset['displayName']} | {asset['silhouetteThesis']} | Preserve **{negative}** as readable white space; supports may not close it. | {exit_text} | {connection_text} | `{audit}` |"
        )
    lines += [
        "",
        "## Interpretation rules",
        "",
        "- A production building must visually reserve the same direction and capacity represented by its authoritative exit envelope.",
        "- A non-production building must not grow a false hangar-sized opening merely to look busy.",
        "- Tube, resource, service and worksite sockets are presentation hookups only; gameplay connectivity remains authoritative outside the model.",
        "- White cuts on the skyline board are deliberate identity-bearing openings, not transparent materials or missing geometry.",
    ]
    return "\n".join(lines) + "\n"


def answer_key(coded_assets: list[tuple[str, dict]]) -> str:
    lines = [
        "# M8.5 T082 — blind silhouette answer key",
        "",
        "Open this only after recording the blind identifications. The same fixed S-codes are used on the 24-, 44- and 72-cell boards, so recognition loss can be compared directly.",
        "",
        "| Code | Asset | Faction | Kind | Footprint |",
        "|---|---|---|---|---|",
    ]
    for code, asset in coded_assets:
        lines.append(f"| `{code}` | {asset['displayName']} | {FACTION_LABELS[asset['faction']]} | {asset['kind']} | {asset['footprint']} |")
    return "\n".join(lines) + "\n"


def readme_text() -> str:
    return """# M8.5 T082 — cross-roster silhouette review

## V1 result — rejected

The game director recognized 0 of 66 assets on the first 24-cell blind sheets on 2026-09-11. The deterministic primitive generator reduced specific LEGO construction to category-level icons, so every `blind_*_cells.svg` V1 sheet is rejected evidence. Do not continue its 44- or 72-cell review and do not use `BLIND_REVIEW_KEY.md` to reinterpret the result as a partial pass.

The retained V1 files are a reproducible failed experiment and diagnostic baseline, not finished art, gameplay authority or an accepted production input.

## Active next step

`PilotV2/blind_pilot_v2.png` is the self-contained review board for four source-derived concept renders before any second 66-asset corpus is produced. It preserves source construction instead of composing assets from generic rectangles, circles and wedges. Its separate answer key must remain closed until the game director records all four identifications. The SVG is retained only as an editable layout source because some viewers do not resolve its linked PNG files.

## Rejected V1 review order — historical only

1. Open both `blind_24_cells_page_1.svg` and `blind_24_cells_page_2.svg` without opening the answer key. Write down the asset name you believe matches every S-code.
2. Repeat with both 44-cell pages, then both 72-cell pages. Do not use faction, role or footprint hints. The unsplit `blind_*_cells.svg` files are retained as full-roster overview sheets.
3. Open `BLIND_REVIEW_KEY.md` and mark wrong, uncertain or indistinguishable codes.
4. Use the faction proportion boards, scale lineup and building skyline/access matrix to diagnose the exact missing distinction.

Passing still requires game-director identification or explicit acceptance of a documented mitigation. The generator intentionally never marks a review complete by itself.

## What the sheets do and do not prove

- The 24/44/72 boards apply one camera-ratio reduction and one canonical footprint-class scale rule to all 66 assets.
- The proportion boards compare front, side and top concept ratios using labels because they are diagnosis tools, not blind tests.
- The building skyline board treats identity-bearing entrances, production lanes and Tube/network gaps as negative space.
- These SVGs do not prove final LEGO construction, material readability, animation, LOD implementation or gameplay-camera integration. Those remain later production gates.
"""


def expected_outputs() -> dict[Path, str]:
    roster = json.loads(ROSTER.read_text(encoding="utf-8"))
    concepts = json.loads(CONCEPTS.read_text(encoding="utf-8"))
    content = json.loads(CONTENT.read_text(encoding="utf-8"))
    assets = roster["assets"]
    profiles = {profile["stableId"]: profile for profile in concepts["profiles"]}
    roster_ids = {asset["stableId"] for asset in assets}
    if len(profiles) != len(concepts["profiles"]) or set(profiles) != roster_ids:
        raise ValueError("silhouette profiles must cover every roster asset exactly once")
    required_fields = {"stableId", "ratios", "core", "mobility", "hero", "frame", "negative"}
    fingerprints: dict[str, str] = {}
    for stable_id, profile in profiles.items():
        if set(profile) != required_fields:
            raise ValueError(f"{stable_id} silhouette profile fields drifted")
        ratios = profile["ratios"]
        if len(ratios) != 3 or any(not isinstance(value, (int, float)) or value <= 0 for value in ratios):
            raise ValueError(f"{stable_id} needs three positive width/height/length ratios")
        if not base_shapes(profile["core"], "game"):
            raise ValueError(f"{stable_id} uses an unsupported core")
        if not mobility_shapes(profile["mobility"], "game") and profile["mobility"] not in NOOP_MOBILITY:
            raise ValueError(f"{stable_id} uses an unsupported mobility silhouette")
        if not hero_shapes(profile["hero"]):
            raise ValueError(f"{stable_id} uses an unsupported hero silhouette")
        if not frame_shapes(profile["frame"]) and profile["frame"] not in NOOP_FRAMES:
            raise ValueError(f"{stable_id} uses an unsupported frame silhouette")
        if not negative_shapes(profile["negative"]):
            raise ValueError(f"{stable_id} uses an unsupported negative-space silhouette")
        fingerprint = render_profile(profile, 0, 0, 1, "game")
        if fingerprint in fingerprints:
            raise ValueError(f"{stable_id} duplicates the concept silhouette for {fingerprints[fingerprint]}")
        fingerprints[fingerprint] = stable_id
    coded_assets = blind_codes(assets)
    contracts: dict[str, dict] = {}
    for path in CONTRACT_PATHS:
        data = json.loads(path.read_text(encoding="utf-8"))
        contracts.update({asset["stableId"]: asset for asset in data["assets"]})
    definitions = {building["stableId"]: building for building in content["buildingDefinitions"]}
    buildings = [asset for asset in assets if asset["kind"] == "Infrastructure"]
    outputs = {
        OUTPUT / "README.md": readme_text(),
        OUTPUT / "BLIND_REVIEW_KEY.md": answer_key(coded_assets),
        OUTPUT / "relative_scale_lineup.svg": scale_lineup(assets, profiles),
        OUTPUT / "building_skyline.svg": building_skyline(buildings, profiles),
        OUTPUT / "building_access_network_matrix.md": building_matrix(buildings, profiles, definitions, contracts),
    }
    for camera_width in concepts["cameraWidthsCells"]:
        outputs[OUTPUT / f"blind_{camera_width}_cells.svg"] = blind_board(camera_width, coded_assets, profiles)
        page_size = (len(coded_assets) + 1) // 2
        pages = (coded_assets[:page_size], coded_assets[page_size:])
        for page_index, page_assets in enumerate(pages, 1):
            outputs[OUTPUT / f"blind_{camera_width}_cells_page_{page_index}.svg"] = blind_board_page(
                camera_width, page_assets, profiles, page_index, len(pages)
            )
    for faction in FACTION_LABELS:
        faction_assets = [asset for asset in assets if asset["faction"] == faction]
        outputs[OUTPUT / f"proportions_{FACTION_SLUGS[faction]}.svg"] = proportion_board(faction, faction_assets, profiles)
    return outputs


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", action="store_true", help="fail when generated artifacts are missing or stale")
    args = parser.parse_args()
    outputs = expected_outputs()
    if args.check:
        stale = [str(path.relative_to(ROOT)) for path, text in outputs.items() if not path.exists() or path.read_text(encoding="utf-8") != text]
        if stale:
            print("M8.5 SILHOUETTES: FAIL missing or stale: " + ", ".join(stale))
            return 1
        digest = hashlib.sha256("".join(outputs[path] for path in sorted(outputs)).encode("utf-8")).hexdigest()[:16]
        print(f"M8.5 SILHOUETTES: PASS artifacts={len(outputs)} digest={digest}")
        return 0
    OUTPUT.mkdir(parents=True, exist_ok=True)
    for path, text in outputs.items():
        path.write_text(text, encoding="utf-8")
    print(f"Generated {len(outputs)} T082 silhouette-review artifacts in {OUTPUT.relative_to(ROOT)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
