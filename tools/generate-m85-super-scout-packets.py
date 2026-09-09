#!/usr/bin/env python3
"""Generate the reviewable T082 identity-baseline packet set."""

from __future__ import annotations

import argparse
import csv
import io
import json
from pathlib import Path
import sys


ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "Content/Presentation/SuperScout/roster_identity_baseline.json"
LEDGER = ROOT / "Content/Presentation/SuperScout/source_ledger.json"
INSTRUCTION_INDEX = ROOT / "Content/Presentation/SuperScout/source_instruction_index.json"
ROCK_RAIDERS_EVIDENCE = ROOT / "Content/Presentation/SuperScout/rock_raiders_source_evidence.json"
ASTRONAUTS_EVIDENCE = ROOT / "Content/Presentation/SuperScout/astronauts_source_evidence.json"
ALIENS_EVIDENCE = ROOT / "Content/Presentation/SuperScout/aliens_source_evidence.json"
CONFUSION = ROOT / "Content/Presentation/SuperScout/confusion_register.json"
OUTPUT = ROOT / "Docs/Development/M85SuperScout/Packets"
INDEX = ROOT / "Docs/Development/M85SuperScout/PACKET_INDEX.md"
MATRIX = ROOT / "Docs/Development/M85SuperScout/Matrices/identity_source_matrix.csv"
CONFUSION_MATRIX = ROOT / "Docs/Development/M85SuperScout/Matrices/confusion_register.md"
SOURCE_INDEX_MATRIX = ROOT / "Docs/Development/M85SuperScout/Matrices/source_instruction_index.csv"
ROCK_RAIDERS_AUDIT = ROOT / "Docs/Development/M85SuperScout/Matrices/rock_raiders_source_audit.md"
ASTRONAUTS_AUDIT = ROOT / "Docs/Development/M85SuperScout/Matrices/astronauts_source_audit.md"
ALIENS_AUDIT = ROOT / "Docs/Development/M85SuperScout/Matrices/aliens_source_audit.md"

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
        "Black and bright lime with dark mechanics and disciplined translucent-neon-green energy or crystal elements.",
        "Do not use insect bodies, biological tissue, nests, tentacles or generic black-neon towers. Construction must remain craft-derived and mechanical.",
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
            f"  - PDF pages {page_range['pages']}: {page_range['evidence']}"
            for page_range in record["constructionRanges"]
        ) or "  - No official construction-page range is available."
        coverage = "; ".join(
            f"{name}={value}" for name, value in record["viewCoverage"].items()
        )
        findings = "\n".join(f"  - {finding}" for finding in record["findings"])
        gaps = "\n".join(f"  - {gap}" for gap in record["openGaps"])
        blocks.append(
            f"### Source audit [{faction}:{set_id}]\n\n"
            f"- Evidence state: `{record['evidenceState']}`\n"
            f"- Construction map:\n{ranges}\n"
            f"- View/mechanism coverage: {coverage}\n"
            f"- Verified findings:\n{findings}\n"
            f"- Remaining evidence gaps:\n{gaps}"
        )
    if not blocks:
        return "`PENDING` — this source family has not yet received its visual PDF/page-range audit."
    return "\n\n".join(blocks)


def packet_text(
    asset: dict,
    sources: dict[str, dict],
    pairs: list[dict],
    instruction_index: dict[str, dict],
    evidence: dict[tuple[str, str], dict],
) -> str:
    palette, forbidden = FACTION_RULES[asset["faction"]]
    source_rows = []
    for set_id in asset["sourceSets"]:
        source = sources[set_id]
        instruction = instruction_index[set_id]
        pdf_links = "<br>".join(
            f"[official PDF {number}]({url})"
            for number, url in enumerate(instruction["pdfUrls"], start=1)
        ) or "no direct official PDF located"
        source_rows.append(
            f"| {set_id} — {source['title']} | [LEGO instructions]({source['officialInstructionsUrl']})<br>{pdf_links} | "
            f"[inventory]({source['inventoryUrl']}) | {source['verification']} | {source['evidenceUse']} |"
        )
    evidence_block = source_evidence_block(asset["sourceSets"], evidence, asset["faction"])
    if any((asset["faction"], set_id) in evidence for set_id in asset["sourceSets"]):
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
    return f"""# {asset['displayName']} — T082 Super Scout packet

**Stable ID:** `{asset['stableId']}`

**Packet state:** `IDENTITY_BASELINE — HOLD FOR MULTI-ANGLE EVIDENCE`

**This is not a design approval or production-model authorization.**

{SECTION_TITLES[0]}

- Faction: `{asset['faction']}`
- Kind: `{asset['kind']}`
- Gameplay role: {asset['role']}
- Authoritative footprint: `{asset['footprint']}`
- Source classification: `{asset['sourceClassification']}`
- Approved source sets/motifs: {', '.join(asset['sourceSets'])}
- Current confidence: verified canonical identity; construction confidence remains bounded by the source verification shown below.

Authoritative references:

{authority}

Open question: {open_question}

{SECTION_TITLES[1]}

| Source | Primary evidence | Inventory / archival check | Confidence | Intended use |
|---|---|---|---|---|
{chr(10).join(source_rows)}

{evidence_block}

Any `PARTIAL` or `MISSING` view remains an explicit gap. One flattering three-quarter image is never sufficient.

{SECTION_TITLES[2]}

**Silhouette thesis:** {asset['silhouetteThesis']}

Non-removable identity anchors:

{anchors}

- Near / standard / far silhouette thumbnails: `PENDING 24/44/72-CELL BOARD`.
- Palette and material hierarchy: {palette}
- Forbidden genericization: {forbidden}
- Nearest-confusion baseline:

{confusion}

{SECTION_TITLES[3]}

- Hero geometry must preserve every recognition anchor above.
- Support geometry must explain how hero masses connect, carry load and articulate.
- Micro geometry may enrich close view but may not become required for recognition.
- Exact chassis/load path, repeated modules, mounting logic, scale ratios and approved adaptations: `HOLD — SOURCE DECOMPOSITION REQUIRED`.

{SECTION_TITLES[4]}

- Silhouette, openings, major panel breaks, moving joints and LEGO connection logic remain geometry.
- Surface channels may carry controlled color masks, roughness, emission, decals and non-structural relief only.
- Required reusable and bespoke texture sets, resolution, tiling, texel density, LOD fallback and import settings: `HOLD — TEXTURE-NEEDS AUDIT REQUIRED`.
- Baked lighting, fake silhouette structure and illegible micro-noise are prohibited.

{SECTION_TITLES[5]}

- Applicable idle, locomotion/operation, work, attack, production, repair, transform/deploy, disabled, damage and destruction beats: `HOLD — MECHANISM EVIDENCE REQUIRED`.
- Every moving assembly must receive a named pivot, parent, axis/path, rest/extreme poses and authoritative presentation driver.
- Animation may communicate gameplay state but never decide gameplay timing.

{SECTION_TITLES[6]}

- `Socket_Selection` and `Socket_Health` are mandatory.
- Tool, weapon, projectile, VFX, lamp and audio sockets follow only from verified function.
- Cargo, passenger, service, production-exit or network sockets apply where the canonical role requires them.
- Identification Tile, icon silhouette, portrait camera and reduced-presentation fallback: `HOLD — PRESENTATION AUDIT REQUIRED`.

{SECTION_TITLES[7]}

- Verified fact: stable identity, faction, role, footprint, source classification and mapped source family.
- Canon-derived interpretation: silhouette thesis and identity anchors above.
- Unknown: exact multi-angle construction, articulation, material ratios, texture inventory and confusion mitigation until the remaining audits are complete.
- Consequential contradictions: none recorded at identity-baseline stage.

{SECTION_TITLES[8]}

1. Verify and cite the complete multi-angle source board.
2. Decompose primary masses and negative spaces from orthogonal evidence.
3. Resolve LEGO load path, connection grammar and moving mechanism.
4. Complete material/texture and state/animation contracts.
5. Produce 24/44/72-cell black silhouettes and run the cross-roster confusion audit.

**State:** `HOLD`

**Approving reviewer:** game director, not yet requested for this packet.
"""


def index_text(assets: list[dict]) -> str:
    rows = []
    for asset in assets:
        rel = f"Packets/{slug(asset['stableId'])}"
        rows.append(
            f"| [{asset['displayName']}]({rel}) | `{asset['stableId']}` | {asset['faction']} | "
            f"{asset['kind']} | {asset['footprint']} | IDENTITY_BASELINE / HOLD |"
        )
    return f"""# M8.5 T082 — Super Scout packet index

This generated index covers every canonical buildable unit and infrastructure entry. The current pass locks identity, source family and non-removable silhouette anchors. It deliberately remains `HOLD` until multi-angle evidence, construction, mechanism, texture and cross-roster silhouette audits are complete.

| Asset | Stable ID | Faction | Kind | Footprint | State |
|---|---|---|---|---|---|
{chr(10).join(rows)}
"""


def matrix_text(assets: list[dict]) -> str:
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
            ";".join(asset["identityAnchors"]), "IDENTITY_BASELINE_HOLD",
        ])
    return output.getvalue()


def confusion_text(pairs: list[dict], assets: dict[str, dict]) -> str:
    rows = []
    for pair in pairs:
        left = assets[pair["left"]]["displayName"]
        right = assets[pair["right"]]["displayName"]
        differences = "<br>".join(f"{index + 1}. {value}" for index, value in enumerate(pair["differences"]))
        rows.append(f"| {left} | {right} | {pair['risk']} | {differences} | {pair['state']} |")
    return f"""# M8.5 T082 — confusion-register baseline

This is the canon-derived first pass for the most obvious internal and cross-faction confusion pairs. It is not the final blind silhouette audit; additional pairs may be discovered after the 24/44/72-cell boards exist.

| Left | Right | Why they may be confused | Required visible differences | State |
|---|---|---|---|---|
{chr(10).join(rows)}
"""


def source_index_text(records: list[dict], sources: dict[str, dict]) -> str:
    output = io.StringIO(newline="")
    writer = csv.writer(output, lineterminator="\n")
    writer.writerow(["set_id", "title", "verification", "direct_pdf_count", "direct_pdf_urls", "state"])
    for record in records:
        source = sources[record["setId"]]
        writer.writerow([
            record["setId"], source["title"], source["verification"],
            len(record["pdfUrls"]), ";".join(record["pdfUrls"]), record["state"],
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

This generated review records what the official instruction PDFs actually prove, which views remain partial or missing, and where gameplay adaptation still must be explicit. The PDFs and rendered review sheets are temporary research material and are not redistributed in the repository.

""" + "\n\n".join(blocks) + "\n"


def expected_files() -> dict[Path, str]:
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    ledger = json.loads(LEDGER.read_text(encoding="utf-8"))
    instruction_index = json.loads(INSTRUCTION_INDEX.read_text(encoding="utf-8"))
    rock_raiders_evidence = json.loads(ROCK_RAIDERS_EVIDENCE.read_text(encoding="utf-8"))
    astronauts_evidence = json.loads(ASTRONAUTS_EVIDENCE.read_text(encoding="utf-8"))
    aliens_evidence = json.loads(ALIENS_EVIDENCE.read_text(encoding="utf-8"))
    confusion = json.loads(CONFUSION.read_text(encoding="utf-8"))
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
    ]
    evidence = {
        (faction, record["setId"]): record
        for faction, record in evidence_records
    }
    if len(evidence) != len(evidence_records):
        raise ValueError("duplicate source IDs across source-evidence audits")
    assets = {asset["stableId"]: asset for asset in manifest["assets"]}
    result = {
        INDEX: index_text(manifest["assets"]),
        MATRIX: matrix_text(manifest["assets"]),
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
    }
    for asset in manifest["assets"]:
        result[OUTPUT / slug(asset["stableId"])] = packet_text(
            asset, sources, confusion["pairs"], instructions, evidence
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
        print("M8.5 SUPER SCOUT PACKETS: PASS packets=66 matrices=6 state=HOLD")
        return

    OUTPUT.mkdir(parents=True, exist_ok=True)
    for path, content in expected.items():
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(content, encoding="utf-8")
    print("M8.5 SUPER SCOUT PACKETS: GENERATED packets=66 matrices=6 state=HOLD")


if __name__ == "__main__":
    main()
