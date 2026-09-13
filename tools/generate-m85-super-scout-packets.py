#!/usr/bin/env python3
"""Generate the reviewable T082 identity-baseline packet set."""

from __future__ import annotations

import argparse
import csv
import io
import json
from pathlib import Path
import random
import sys


ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "Content/Presentation/SuperScout/roster_identity_baseline.json"
LEDGER = ROOT / "Content/Presentation/SuperScout/source_ledger.json"
INSTRUCTION_INDEX = ROOT / "Content/Presentation/SuperScout/source_instruction_index.json"
SOURCE_ANALYSIS_POLICY = ROOT / "Content/Presentation/SuperScout/source_analysis_policy.json"
ROCK_RAIDERS_EVIDENCE = ROOT / "Content/Presentation/SuperScout/rock_raiders_source_evidence.json"
ASTRONAUTS_EVIDENCE = ROOT / "Content/Presentation/SuperScout/astronauts_source_evidence.json"
ALIENS_EVIDENCE = ROOT / "Content/Presentation/SuperScout/aliens_source_evidence.json"
MARTIANS_EVIDENCE = ROOT / "Content/Presentation/SuperScout/martians_source_evidence.json"
ROCK_RAIDERS_CONTRACTS = ROOT / "Content/Presentation/SuperScout/rock_raiders_production_contracts.json"
ASTRONAUTS_CONTRACTS = ROOT / "Content/Presentation/SuperScout/astronauts_production_contracts.json"
ALIENS_CONTRACTS = ROOT / "Content/Presentation/SuperScout/aliens_production_contracts.json"
ALIEN_BIOMECHANICAL_POLICY = ROOT / "Content/Presentation/SuperScout/alien_biomechanical_policy.json"
MARTIANS_CONTRACTS = ROOT / "Content/Presentation/SuperScout/martians_production_contracts.json"
CONFUSION = ROOT / "Content/Presentation/SuperScout/confusion_register.json"
SILHOUETTES = ROOT / "Content/Presentation/SuperScout/silhouette_concepts.json"
OUTPUT = ROOT / "Docs/Development/M85SuperScout/Packets"
INDEX = ROOT / "Docs/Development/M85SuperScout/PACKET_INDEX.md"
MATRIX = ROOT / "Docs/Development/M85SuperScout/Matrices/identity_source_matrix.csv"
CONFUSION_MATRIX = ROOT / "Docs/Development/M85SuperScout/Matrices/confusion_register.md"
SOURCE_INDEX_MATRIX = ROOT / "Docs/Development/M85SuperScout/Matrices/source_instruction_index.csv"
ROCK_RAIDERS_AUDIT = ROOT / "Docs/Development/M85SuperScout/Matrices/rock_raiders_source_audit.md"
ASTRONAUTS_AUDIT = ROOT / "Docs/Development/M85SuperScout/Matrices/astronauts_source_audit.md"
ALIENS_AUDIT = ROOT / "Docs/Development/M85SuperScout/Matrices/aliens_source_audit.md"
MARTIANS_AUDIT = ROOT / "Docs/Development/M85SuperScout/Matrices/martians_source_audit.md"
ROCK_RAIDERS_CONSTRUCTION = ROOT / "Docs/Development/M85SuperScout/Matrices/rock_raiders_semantic_construction.md"
ROCK_RAIDERS_MOTION = ROOT / "Docs/Development/M85SuperScout/Matrices/rock_raiders_motion_socket.md"
ROCK_RAIDERS_MATERIAL = ROOT / "Docs/Development/M85SuperScout/Matrices/rock_raiders_material_texture.md"
ASTRONAUTS_CONSTRUCTION = ROOT / "Docs/Development/M85SuperScout/Matrices/astronauts_semantic_construction.md"
ASTRONAUTS_MOTION = ROOT / "Docs/Development/M85SuperScout/Matrices/astronauts_motion_socket.md"
ASTRONAUTS_MATERIAL = ROOT / "Docs/Development/M85SuperScout/Matrices/astronauts_material_texture.md"
ALIENS_CONSTRUCTION = ROOT / "Docs/Development/M85SuperScout/Matrices/aliens_semantic_construction.md"
ALIENS_MOTION = ROOT / "Docs/Development/M85SuperScout/Matrices/aliens_motion_socket.md"
ALIENS_MATERIAL = ROOT / "Docs/Development/M85SuperScout/Matrices/aliens_material_texture.md"
MARTIANS_CONSTRUCTION = ROOT / "Docs/Development/M85SuperScout/Matrices/martians_semantic_construction.md"
MARTIANS_MOTION = ROOT / "Docs/Development/M85SuperScout/Matrices/martians_motion_socket.md"
MARTIANS_MATERIAL = ROOT / "Docs/Development/M85SuperScout/Matrices/martians_material_texture.md"

FACTION_RULES = {
    "RockRaiders": (
        "Dark turquoise and industrial gray with black, light gray, hazard-yellow and restrained tool metal.",
        "Do not turn the asset into a conventional tank, APC, artillery piece or realistic modern vehicle. Its industrial purpose must read first.",
    ),
    "Astronauts": (
        "Field Systems retain rugged white/light-gray/medium-blue construction; Mission Systems retain clean white/orange/black construction. Shared identity comes from insignia and interfaces, not shape averaging.",
        "Do not blend the two source lineages into generic white sci-fi or add military forms unsupported by the mapped expedition function.",
    ),
    "Aliens": (
        "Black and bright lime armored biomechanical craft with skeletal supports, living conduits and disciplined translucent-neon-green energy or crystal elements.",
        "Do not substitute generic insects, hives, unrelated tentacles, contemporary human robots or black-neon towers for source-grounded biomechanical craft (Phase 02A).",
    ),
    "Martians": (
        "Blue and sand-red with translucent-neon-green accents, open platforms and visibly articulated mechanics.",
        "Do not make the asset Alien-lite, a smooth energy object or a joke contraption. Pumps, tubes, legs, clamps and platforms carry identity.",
    ),
}

SECTION_TITLES = [
    "## A. Identity and authority",
    "## B. Reference board",
    "## C. Recognition contract",
    "## D. Construction contract",
    "## E. Material and texture contract",
    "## F. State and animation contract",
    "## G. Presentation hookups",
    "## H. Insight and decision ledger",
    "## I. Build handoff",
]


def slug(stable_id: str) -> str:
    return stable_id.replace(".", "_") + ".md"


def source_evidence_block(
    set_ids: list[str], evidence: dict[tuple[str, str], dict], faction: str
) -> str:
    blocks = []
    for set_id in set_ids:
        record = evidence.get((faction, set_id))
        if record is None:
            continue
        ranges = "\n".join(
            f"  - Evidence pages {page_range['pages']}: {page_range['evidence']}"
            for page_range in record["constructionRanges"]
        ) or "  - No construction-page range is available for this evidence type."
        evidence_links = []
        for number, pdf in enumerate(record.get("instructionPdfs", []), start=1):
            provenance = pdf.get("provenance", "official instruction PDF")
            evidence_links.append(f"[{provenance} {number}]({pdf['url']})")
        for number, url in enumerate(record.get("referenceUrls", []), start=1):
            evidence_links.append(f"[archival product reference {number}]({url})")
        evidence_link_text = ", ".join(evidence_links) or "no viewable evidence link recorded"
        coverage = "; ".join(
            f"{name}={value}" for name, value in record["viewCoverage"].items()
        )
        findings = "\n".join(f"  - {finding}" for finding in record["findings"])
        gaps = "\n".join(f"  - {gap}" for gap in record["openGaps"])
        blocks.append(
            f"### Source audit [{faction}:{set_id}]\n\n"
            f"- Evidence state: `{record['evidenceState']}`\n"
            f"- Evidence links: {evidence_link_text}\n"
            f"- Construction map:\n{ranges}\n"
            f"- View/mechanism coverage: {coverage}\n"
            f"- Verified findings:\n{findings}\n"
            f"- Remaining evidence gaps:\n{gaps}"
        )
    if not blocks:
        return "`PENDING` — this source family has not yet received its visual PDF/page-range audit."
    return "\n\n".join(blocks)


def production_contract_sections(contract: dict | None, texture_specs: dict[str, dict]) -> dict[str, str]:
    if contract is None:
        return {
            "construction": (
                "- Hero geometry must preserve every recognition anchor above.\n"
                "- Support geometry must explain how hero masses connect, carry load and articulate.\n"
                "- Micro geometry may enrich close view but may not become required for recognition.\n"
                "- Exact chassis/load path, repeated modules, mounting logic, scale ratios and approved adaptations: `HOLD — SOURCE DECOMPOSITION REQUIRED`."
            ),
            "material": (
                "- Silhouette, openings, major panel breaks, moving joints and LEGO connection logic remain geometry.\n"
                "- Surface channels may carry controlled color masks, roughness, emission, decals and non-structural relief only.\n"
                "- Required reusable and bespoke texture sets, resolution, tiling, texel density, LOD fallback and import settings: `HOLD — TEXTURE-NEEDS AUDIT REQUIRED`.\n"
                "- Baked lighting, fake silhouette structure and illegible micro-noise are prohibited."
            ),
            "motion": (
                "- Applicable idle, locomotion/operation, work, attack, production, repair, transform/deploy, disabled, damage and destruction beats: `HOLD — MECHANISM EVIDENCE REQUIRED`.\n"
                "- Every moving assembly must receive a named pivot, parent, axis/path, rest/extreme poses and authoritative presentation driver.\n"
                "- Animation may communicate gameplay state but never decide gameplay timing."
            ),
            "hookups": (
                "- `Socket_Selection` and `Socket_Health` are mandatory.\n"
                "- Tool, weapon, projectile, VFX, lamp and audio sockets follow only from verified function.\n"
                "- Cargo, passenger, service, production-exit or network sockets apply where the canonical role requires them.\n"
                "- Identification Tile, icon silhouette, portrait camera and reduced-presentation fallback: `HOLD — PRESENTATION AUDIT REQUIRED`."
            ),
            "insight": (
                "- Verified fact: stable identity, faction, role, footprint, source classification and mapped source family.\n"
                "- Canon-derived interpretation: silhouette thesis and identity anchors above.\n"
                "- Unknown: exact multi-angle construction, articulation, material ratios, texture inventory and confusion mitigation until the remaining audits are complete.\n"
                "- Consequential contradictions: none recorded at identity-baseline stage."
            ),
        }

    semantic = "\n".join(f"  - {value}" for value in contract["semanticParts"])
    construction = contract["construction"]
    geometry = "\n".join(f"  - {value}" for value in contract["materials"]["geometryMustCarry"])
    roles = ", ".join(f"`{value}`" for value in contract["materials"]["materialRoles"])
    texture_lines = []
    for texture_id in contract["materials"]["textureFamilies"]:
        spec = texture_specs[texture_id]
        texture_lines.append(
            f"  - `{texture_id}` — {spec['purpose']} Channels: {spec['channels']} "
            f"Resolution: {spec['resolution']}; texel density: {spec['texelDensity']}; "
            f"tiling: {spec['tiling']}; LOD fallback: {spec['lodFallback']} "
            f"Provenance/state: {spec['provenance']} `{spec['state']}`."
        )
    bespoke = contract["materials"]["bespokeTextures"]
    bespoke_text = "; ".join(bespoke) if bespoke else "none required in this faction draft"
    motion = contract["motion"]
    pivot_rows = "\n".join(
        f"| `{pivot['name']}` | {pivot['parent']} | {pivot['motion']} | {pivot['driver']} |"
        for pivot in motion["pivots"]
    )
    beats = "\n".join(f"  - {value}" for value in motion["beats"])
    sockets = ", ".join(f"`{value}`" for value in contract["sockets"])
    unresolved = "\n".join(f"  - {value}" for value in contract["unresolved"])
    return {
        "construction": (
            f"- Contract state: `{contract['contractState']}`. This is an internally checked draft, not game-director approval.\n"
            f"- Semantic part map:\n{semantic}\n"
            f"- Structural load path: {construction['loadPath']}\n"
            f"- Repeated modules / connection grammar: {construction['modules']}\n"
            f"- Source-faithful versus adapted boundary: {construction['adaptationBoundary']}"
        ),
        "material": (
            f"- Geometry must carry:\n{geometry}\n"
            f"- Accepted master-material roles: {roles}.\n"
            f"- Reusable texture requirements:\n" + "\n".join(texture_lines) + "\n"
            f"- Bespoke texture requirements: {bespoke_text}.\n"
            "- Baked lighting, fake silhouette structure, per-part texture phase resets and illegible micro-noise remain prohibited."
        ),
        "motion": (
            f"- Locomotion / operation: {motion['locomotion']}\n"
            f"- Planted/contact rule: {motion['plantedContact']}\n\n"
            "| Pivot | Parent | Axis/path and rest-to-extreme motion | Presentation driver |\n"
            "|---|---|---|---|\n"
            f"{pivot_rows}\n\n"
            f"- Required beats:\n{beats}\n"
            "- Animation consumes authoritative state and never decides gameplay timing or results."
        ),
        "hookups": (
            f"- Required presentation sockets: {sockets}.\n"
            "- These sockets are presentation references only and never own targeting, collision, movement, transport or production truth.\n"
            "- Identification Tile placement, icon silhouette, portrait camera and reduced-presentation fallback remain `HOLD — PRESENTATION AUDIT REQUIRED`."
        ),
        "insight": (
            "- Verified fact: stable identity, source evidence and the source-supported assemblies cited above.\n"
            "- Canon-derived interpretation: gameplay function, adaptation boundary, contact behavior and presentation drivers are explicitly labeled in the contract.\n"
            f"- Remaining source/design decisions:\n{unresolved}\n"
            "- Cross-roster silhouette and game-director review remain open; this contract does not authorize production modeling."
        ),
    }


def packet_text(
    asset: dict,
    sources: dict[str, dict],
    pairs: list[dict],
    instruction_index: dict[str, dict],
    evidence: dict[tuple[str, str], dict],
    contract: dict | None,
    texture_specs: dict[str, dict],
    packet_notice: str,
    review_code: str,
) -> str:
    palette, forbidden = FACTION_RULES[asset["faction"]]
    biological_evidence = ""
    if asset["faction"] == "Aliens":
        policy = json.loads(ALIEN_BIOMECHANICAL_POLICY.read_text(encoding="utf-8"))
        biological_evidence = policy["packetNotice"] + "\n\n" + "\n".join(
            f"- [{source['id']}]({source['url']}) — {source['evidenceKind']}: {source['finding']}"
            for source in policy["sources"]
        ) + "\n\n" + "\n".join(f"- {rule}" for rule in policy["presentationRules"])
    if contract is None:
        packet_state = "IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE"
        confidence = "verified canonical identity; construction confidence remains bounded by the source verification shown below."
        build_handoff = (
            "1. Verify and cite the complete multi-angle source board.\n"
            "2. Decompose primary masses and negative spaces from orthogonal evidence.\n"
            "3. Resolve LEGO load path, connection grammar and moving mechanism.\n"
            "4. Complete material/texture and state/animation contracts.\n"
            "5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review."
        )
    else:
        packet_state = "FACTION_CONTRACT_DRAFT — HOLD FOR SILHOUETTE/ROSTER/DIRECTOR REVIEW"
        confidence = (
            "verified canonical identity and faction-internal construction/motion/material draft; "
            "source-bounded decisions remain explicit below."
        )
        build_handoff = (
            "1. Retain the audited evidence and every explicit adaptation boundary.\n"
            "2. Greybox hero masses, openings and structural load path from the semantic map.\n"
            "3. Validate named pivots, contacts and sockets in the real gameplay camera.\n"
            "4. Author only the specified reusable textures after human material review.\n"
            "5. Apply the game-director-approved source-derived method from the 4/4 Pilot V2 result, then run a new complete 24/44/72-cell blind review."
        )
    source_rows = []
    for set_id in asset["sourceSets"]:
        source = sources[set_id]
        instruction = instruction_index[set_id]
        pdf_links = "<br>".join(
            f"[official PDF {number}]({url})"
            for number, url in enumerate(instruction["pdfUrls"], start=1)
        ) or "no direct official PDF located"
        archival_links = "<br>".join(
            f"[archival evidence {number}]({url})"
            for number, url in enumerate(instruction.get("archivalEvidenceUrls", []), start=1)
        )
        if archival_links:
            pdf_links += f"<br>{archival_links}"
        source_rows.append(
            f"| {set_id} — {source['title']} | [LEGO instructions]({source['officialInstructionsUrl']})<br>{pdf_links} | "
            f"[inventory]({source['inventoryUrl']}) | {source['verification']} | {source['evidenceUse']} |"
        )
    evidence_block = source_evidence_block(asset["sourceSets"], evidence, asset["faction"])
    if contract is not None:
        open_question = (
            "The faction-internal construction, motion, socket and material draft is recorded below. "
            "Its unresolved decisions and the complete-roster silhouette/director gates must be cleared "
            "before this packet can leave HOLD."
        )
    elif any((asset["faction"], set_id) in evidence for set_id in asset["sourceSets"]):
        open_question = (
            "Source-view coverage and construction-critical page ranges are recorded for the audited "
            "sources below. Asset-specific adaptation boundaries still must be resolved "
            "before this packet can leave HOLD."
        )
    else:
        open_question = (
            "Exact source-view coverage, construction-critical page ranges and every adaptation boundary "
            "must be recorded before this packet can leave HOLD."
        )
    anchors = "\n".join(f"- {anchor}" for anchor in asset["identityAnchors"])
    related = []
    for pair in pairs:
        if asset["stableId"] not in {pair["left"], pair["right"]}:
            continue
        other = pair["right"] if pair["left"] == asset["stableId"] else pair["left"]
        related.append(
            f"- `{other}` — {pair['risk']} Mitigations: "
            + " / ".join(pair["differences"])
        )
    confusion = "\n".join(related) if related else "- `PENDING` — no nearest-neighbor pair has been assigned yet."
    authority = "\n".join(
        [
            "- `Docs/Canon/00_CANON_SET_REGISTRY.md`",
            "- `Docs/Canon/03_UNIT_BUILDING_ROSTER.md`",
            "- `Docs/Canon/09C_FULL_CONTENT_AND_PRESENTATION_PRODUCTION_AMENDMENT.md`",
            "- `Content/PrototypeEntities.json`",
        ]
    )
    contract_sections = production_contract_sections(contract, texture_specs)
    return f"""# {asset['displayName']} — T082 Super Scout packet

**Stable ID:** `{asset['stableId']}`

**Packet state:** `{packet_state}`

**This is not a design approval or production-model authorization.**

{SECTION_TITLES[0]}

- Faction: `{asset['faction']}`
- Kind: `{asset['kind']}`
- Gameplay role: {asset['role']}
- Authoritative footprint: `{asset['footprint']}`
- Source classification: `{asset['sourceClassification']}`
- Approved source sets/motifs: {', '.join(asset['sourceSets'])}
- Current confidence: {confidence}

Authoritative references:

{authority}

Open question: {open_question}

{SECTION_TITLES[1]}

| Source | Primary evidence | Inventory / archival check | Confidence | Intended use |
|---|---|---|---|---|
{chr(10).join(source_rows)}

{evidence_block}{chr(10) + chr(10) + biological_evidence if biological_evidence else ''}

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

**Source-decomposition rule:** {packet_notice}

{SECTION_TITLES[2]}

**Silhouette thesis:** {asset['silhouetteThesis']}

Non-removable identity anchors:

{anchors}

- Rejected V1 blind-review code: `{review_code}`. Historical failed boards: [24-cell](../Silhouettes/blind_24_cells.svg), [44-cell](../Silhouettes/blind_44_cells.svg), [72-cell](../Silhouettes/blind_72_cells.svg). The game director recognized 0/66 at 24 cells; this primitive concept is not an approved model or accepted evidence.
- Palette and material hierarchy: {palette}
- Forbidden genericization: {forbidden}
- Nearest-confusion baseline:

{confusion}

{SECTION_TITLES[3]}

{contract_sections['construction']}

{SECTION_TITLES[4]}

{contract_sections['material']}

{SECTION_TITLES[5]}

{contract_sections['motion']}

{SECTION_TITLES[6]}

{contract_sections['hookups']}

{SECTION_TITLES[7]}

{contract_sections['insight']}

{SECTION_TITLES[8]}

{build_handoff}

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
"""


def index_text(assets: list[dict], contract_ids: set[str]) -> str:
    rows = []
    for asset in assets:
        rel = f"Packets/{slug(asset['stableId'])}"
        state = "FACTION_CONTRACT_DRAFT / HOLD" if asset["stableId"] in contract_ids else "IDENTITY_BASELINE / HOLD"
        rows.append(
            f"| [{asset['displayName']}]({rel}) | `{asset['stableId']}` | {asset['faction']} | "
            f"{asset['kind']} | {asset['footprint']} | {state} |"
        )
    return f"""# M8.5 T082 — Super Scout packet index

This generated index covers every canonical buildable unit and infrastructure entry. Identity, sources, semantic construction, mechanisms and texture needs exist. The first cross-roster primitive-silhouette draft failed game-director blind review at 0/66 and is rejected. The source-derived Pilot V2 then passed 4/4, approving that method for a new complete corpus. Every packet deliberately remains `HOLD` until the complete 24/44/72-cell review passes.

| Asset | Stable ID | Faction | Kind | Footprint | State |
|---|---|---|---|---|---|
{chr(10).join(rows)}
"""


def matrix_text(assets: list[dict], contract_ids: set[str]) -> str:
    output = io.StringIO(newline="")
    writer = csv.writer(output, lineterminator="\n")
    writer.writerow([
        "stable_id", "display_name", "faction", "kind", "role", "footprint",
        "source_classification", "source_sets", "silhouette_thesis", "identity_anchors", "state",
    ])
    for asset in assets:
        writer.writerow([
            asset["stableId"], asset["displayName"], asset["faction"], asset["kind"],
            asset["role"], asset["footprint"], asset["sourceClassification"],
            ";".join(asset["sourceSets"]), asset["silhouetteThesis"],
            ";".join(asset["identityAnchors"]),
            "FACTION_CONTRACT_DRAFT_HOLD" if asset["stableId"] in contract_ids else "IDENTITY_BASELINE_HOLD",
        ])
    return output.getvalue()


def confusion_text(pairs: list[dict], assets: dict[str, dict]) -> str:
    rows = []
    for pair in pairs:
        left = assets[pair["left"]]["displayName"]
        right = assets[pair["right"]]["displayName"]
        differences = "<br>".join(f"{index + 1}. {value}" for index, value in enumerate(pair["differences"]))
        rows.append(f"| {left} | {right} | {pair['risk']} | {differences} | {pair['state']} |")
    return f"""# M8.5 T082 — confusion register

This register combines the canon-derived baseline with hypotheses exposed by the rejected first 24/44/72-cell primitive boards. The game director recognized 0/66 at 24 cells, then identified all four source-derived Pilot V2 renders correctly. These hypotheses remain provisional until the new complete source-derived corpus is reviewed.

| Left | Right | Why they may be confused | Required visible differences | State |
|---|---|---|---|---|
{chr(10).join(rows)}
"""


def source_index_text(records: list[dict], sources: dict[str, dict]) -> str:
    output = io.StringIO(newline="")
    writer = csv.writer(output, lineterminator="\n")
    writer.writerow([
        "set_id", "title", "verification", "direct_pdf_count", "direct_pdf_urls",
        "archival_evidence_count", "archival_evidence_urls", "state",
    ])
    for record in records:
        source = sources[record["setId"]]
        writer.writerow([
            record["setId"], source["title"], source["verification"],
            len(record["pdfUrls"]), ";".join(record["pdfUrls"]),
            len(record.get("archivalEvidenceUrls", [])),
            ";".join(record.get("archivalEvidenceUrls", [])), record["state"],
        ])
    return output.getvalue()


def source_audit_text(
    records: list[dict], sources: dict[str, dict], faction: str, faction_label: str
) -> str:
    evidence = {(faction, record["setId"]): record for record in records}
    blocks = []
    for record in records:
        title = sources[record["setId"]]["title"]
        blocks.append(
            f"## {record['setId']} — {title}\n\n"
            f"{source_evidence_block([record['setId']], evidence, faction)}"
        )
    return f"""# M8.5 T082 — {faction_label} source-evidence audit

This generated review records what the available official instructions or labeled archival evidence actually prove, which views remain partial or missing, and where gameplay adaptation still must be explicit. The PDFs and rendered review sheets are temporary research material and are not redistributed in the repository.

""" + "\n\n".join(blocks) + "\n"


def semantic_construction_matrix_text(
    contracts: list[dict], assets: dict[str, dict], faction_label: str
) -> str:
    rows = []
    for contract in contracts:
        asset = assets[contract["stableId"]]
        parts = "<br>".join(contract["semanticParts"])
        construction = contract["construction"]
        rows.append(
            f"| {asset['displayName']} | `{contract['contractState']}` | {parts} | "
            f"{construction['loadPath']} | {construction['modules']} | {construction['adaptationBoundary']} |"
        )
    return f"""# M8.5 T082 — {faction_label} semantic-construction matrix

This generated matrix converts the audited source evidence and locked gameplay roles into buildable faction-internal drafts. It does not approve production modeling; every row remains subject to the full-roster silhouette and game-director review.

| Asset | Contract state | Identity-bearing semantic parts | Structural load path | Modules / connections | Adaptation boundary |
|---|---|---|---|---|---|
""" + "\n".join(rows) + "\n"


def motion_socket_matrix_text(
    contracts: list[dict], assets: dict[str, dict], faction_label: str
) -> str:
    rows = []
    for contract in contracts:
        asset = assets[contract["stableId"]]
        motion = contract["motion"]
        pivots = "<br>".join(
            f"`{pivot['name']}` — {pivot['parent']}; {pivot['motion']}; driver: {pivot['driver']}"
            for pivot in motion["pivots"]
        )
        beats = "<br>".join(motion["beats"])
        sockets = ", ".join(f"`{value}`" for value in contract["sockets"])
        rows.append(
            f"| {asset['displayName']} | {motion['locomotion']} | {motion['plantedContact']} | "
            f"{pivots} | {beats} | {sockets} |"
        )
    return f"""# M8.5 T082 — {faction_label} motion and socket matrix

This generated matrix names the buildable mechanical causes, contacts, pivots and presentation attachment points for the {faction_label} draft. Animation consumes authoritative gameplay state; it never decides results or timing.

| Asset | Locomotion / operation | Planted/contact rule | Named pivots | Required beats | Presentation sockets |
|---|---|---|---|---|---|
""" + "\n".join(rows) + "\n"


def material_texture_matrix_text(
    contracts: list[dict], assets: dict[str, dict], shared_plan: dict, faction_label: str
) -> str:
    family_rows = []
    for spec in shared_plan["reusableTextureFamilies"]:
        family_rows.append(
            f"| `{spec['id']}` | {spec['purpose']} | {spec['channels']} | {spec['resolution']} | "
            f"{spec['texelDensity']} | {spec['tiling']} | {spec['lodFallback']} | {spec['provenance']} | `{spec['state']}` |"
        )
    asset_rows = []
    for contract in contracts:
        asset = assets[contract["stableId"]]
        material = contract["materials"]
        geometry = "<br>".join(material["geometryMustCarry"])
        roles = ", ".join(f"`{value}`" for value in material["materialRoles"])
        reusable = ", ".join(f"`{value}`" for value in material["textureFamilies"])
        bespoke = "<br>".join(material["bespokeTextures"]) or "None required in this faction draft"
        asset_rows.append(
            f"| {asset['displayName']} | {geometry} | {roles} | {reusable} | {bespoke} |"
        )
    rules = "\n".join(f"- {value}" for value in shared_plan["globalRules"])
    return f"""# M8.5 T082 — {faction_label} material and texture-needs matrix

The accepted M7 role-authored material family remains authoritative. These are production requirements, not generated texture assets and not permission to bake structural detail into maps.

## Shared rules

{rules}

## Reusable texture families

| ID | Purpose | Channels | Resolution | Texel density | Tiling | LOD fallback | Provenance | State |
|---|---|---|---|---|---|---|---|---|
{chr(10).join(family_rows)}

## Per-asset needs

| Asset | Geometry must carry | Master-material roles | Reusable texture families | Bespoke textures |
|---|---|---|---|---|
{chr(10).join(asset_rows)}
"""


def expected_files() -> dict[Path, str]:
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    ledger = json.loads(LEDGER.read_text(encoding="utf-8"))
    instruction_index = json.loads(INSTRUCTION_INDEX.read_text(encoding="utf-8"))
    source_analysis_policy = json.loads(SOURCE_ANALYSIS_POLICY.read_text(encoding="utf-8"))
    rock_raiders_evidence = json.loads(ROCK_RAIDERS_EVIDENCE.read_text(encoding="utf-8"))
    astronauts_evidence = json.loads(ASTRONAUTS_EVIDENCE.read_text(encoding="utf-8"))
    aliens_evidence = json.loads(ALIENS_EVIDENCE.read_text(encoding="utf-8"))
    martians_evidence = json.loads(MARTIANS_EVIDENCE.read_text(encoding="utf-8"))
    rock_raiders_contracts = json.loads(ROCK_RAIDERS_CONTRACTS.read_text(encoding="utf-8"))
    astronauts_contracts = json.loads(ASTRONAUTS_CONTRACTS.read_text(encoding="utf-8"))
    aliens_contracts = json.loads(ALIENS_CONTRACTS.read_text(encoding="utf-8"))
    martians_contracts = json.loads(MARTIANS_CONTRACTS.read_text(encoding="utf-8"))
    confusion = json.loads(CONFUSION.read_text(encoding="utf-8"))
    silhouette_concepts = json.loads(SILHOUETTES.read_text(encoding="utf-8"))
    sources = {source["setId"]: source for source in ledger["sources"]}
    instructions = {record["setId"]: record for record in instruction_index["records"]}
    evidence_records = [
        (rock_raiders_evidence["faction"], record)
        for record in rock_raiders_evidence["sources"]
    ] + [
        (astronauts_evidence["faction"], record)
        for record in astronauts_evidence["sources"]
    ] + [
        (aliens_evidence["faction"], record)
        for record in aliens_evidence["sources"]
    ] + [
        (martians_evidence["faction"], record)
        for record in martians_evidence["sources"]
    ]
    evidence = {
        (faction, record["setId"]): record
        for faction, record in evidence_records
    }
    if len(evidence) != len(evidence_records):
        raise ValueError("duplicate faction/source keys across source-evidence audits")
    assets = {asset["stableId"]: asset for asset in manifest["assets"]}
    shuffled_assets = list(manifest["assets"])
    random.Random(85082).shuffle(shuffled_assets)
    review_codes = {
        asset["stableId"]: f"S{index:02d}"
        for index, asset in enumerate(shuffled_assets, 1)
    }
    if {profile["stableId"] for profile in silhouette_concepts["profiles"]} != set(assets):
        raise ValueError("silhouette concepts do not cover the packet roster")
    contract_documents = [
        rock_raiders_contracts, astronauts_contracts, aliens_contracts, martians_contracts
    ]
    production_contracts = {
        contract["stableId"]: contract
        for document in contract_documents
        for contract in document["assets"]
    }
    texture_specs = {
        spec["id"]: spec
        for document in contract_documents
        for spec in document["sharedMaterialPlan"]["reusableTextureFamilies"]
    }
    result = {
        INDEX: index_text(manifest["assets"], set(production_contracts)),
        MATRIX: matrix_text(manifest["assets"], set(production_contracts)),
        CONFUSION_MATRIX: confusion_text(confusion["pairs"], assets),
        SOURCE_INDEX_MATRIX: source_index_text(instruction_index["records"], sources),
        ROCK_RAIDERS_AUDIT: source_audit_text(
            rock_raiders_evidence["sources"], sources, "RockRaiders", "Rock Raiders"
        ),
        ASTRONAUTS_AUDIT: source_audit_text(
            astronauts_evidence["sources"], sources, "Astronauts", "Astronauts"
        ),
        ALIENS_AUDIT: source_audit_text(
            aliens_evidence["sources"], sources, "Aliens", "Aliens"
        ),
        MARTIANS_AUDIT: source_audit_text(
            martians_evidence["sources"], sources, "Martians", "Martians"
        ),
        ROCK_RAIDERS_CONSTRUCTION: semantic_construction_matrix_text(
            rock_raiders_contracts["assets"], assets, "Rock Raiders"
        ),
        ROCK_RAIDERS_MOTION: motion_socket_matrix_text(
            rock_raiders_contracts["assets"], assets, "Rock Raiders"
        ),
        ROCK_RAIDERS_MATERIAL: material_texture_matrix_text(
            rock_raiders_contracts["assets"], assets,
            rock_raiders_contracts["sharedMaterialPlan"], "Rock Raiders"
        ),
        ASTRONAUTS_CONSTRUCTION: semantic_construction_matrix_text(
            astronauts_contracts["assets"], assets, "Astronauts"
        ),
        ASTRONAUTS_MOTION: motion_socket_matrix_text(
            astronauts_contracts["assets"], assets, "Astronauts"
        ),
        ASTRONAUTS_MATERIAL: material_texture_matrix_text(
            astronauts_contracts["assets"], assets,
            astronauts_contracts["sharedMaterialPlan"], "Astronauts"
        ),
        ALIENS_CONSTRUCTION: semantic_construction_matrix_text(
            aliens_contracts["assets"], assets, "Aliens"
        ),
        ALIENS_MOTION: motion_socket_matrix_text(
            aliens_contracts["assets"], assets, "Aliens"
        ),
        ALIENS_MATERIAL: material_texture_matrix_text(
            aliens_contracts["assets"], assets,
            aliens_contracts["sharedMaterialPlan"], "Aliens"
        ),
        MARTIANS_CONSTRUCTION: semantic_construction_matrix_text(
            martians_contracts["assets"], assets, "Martians"
        ),
        MARTIANS_MOTION: motion_socket_matrix_text(
            martians_contracts["assets"], assets, "Martians"
        ),
        MARTIANS_MATERIAL: material_texture_matrix_text(
            martians_contracts["assets"], assets,
            martians_contracts["sharedMaterialPlan"], "Martians"
        ),
    }
    for asset in manifest["assets"]:
        result[OUTPUT / slug(asset["stableId"])] = packet_text(
            asset, sources, confusion["pairs"], instructions, evidence,
            production_contracts.get(asset["stableId"]), texture_specs,
            source_analysis_policy["packetNotice"],
            review_codes[asset["stableId"]],
        )
    return result


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    expected = expected_files()
    if args.check:
        failures = []
        for path, content in expected.items():
            if not path.is_file() or path.read_text(encoding="utf-8") != content:
                failures.append(str(path.relative_to(ROOT)))
        if failures:
            print("M8.5 SUPER SCOUT PACKETS: FAIL stale or missing generated files:", file=sys.stderr)
            for failure in failures:
                print(f"- {failure}", file=sys.stderr)
            raise SystemExit(1)
        print("M8.5 SUPER SCOUT PACKETS: PASS packets=66 matrices=19 contracts=66 state=HOLD")
        return

    OUTPUT.mkdir(parents=True, exist_ok=True)
    for path, content in expected.items():
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(content, encoding="utf-8")
    print("M8.5 SUPER SCOUT PACKETS: GENERATED packets=66 matrices=19 contracts=66 state=HOLD")


if __name__ == "__main__":
    main()
