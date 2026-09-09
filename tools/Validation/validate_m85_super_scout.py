#!/usr/bin/env python3
"""Validate the T082 full-roster identity baseline and generated packets."""

from __future__ import annotations

from collections import Counter
import json
from pathlib import Path
import subprocess
import sys
from urllib.parse import urlparse


ROOT = Path(__file__).resolve().parents[2]
MANIFEST = ROOT / "Content/Presentation/SuperScout/roster_identity_baseline.json"
LEDGER = ROOT / "Content/Presentation/SuperScout/source_ledger.json"
INSTRUCTION_INDEX = ROOT / "Content/Presentation/SuperScout/source_instruction_index.json"
ROCK_RAIDERS_EVIDENCE = ROOT / "Content/Presentation/SuperScout/rock_raiders_source_evidence.json"
ASTRONAUTS_EVIDENCE = ROOT / "Content/Presentation/SuperScout/astronauts_source_evidence.json"
CONFUSION = ROOT / "Content/Presentation/SuperScout/confusion_register.json"
CONTENT = ROOT / "Content/PrototypeEntities.json"
GENERATOR = ROOT / "tools/generate-m85-super-scout-packets.py"

CLASSIFICATIONS = {
    "OFFICIAL_DIRECT": "OFFICIAL-DIRECT",
    "OFFICIAL_ADAPTED": "OFFICIAL-ADAPTED",
    "COMPOSITE_ADAPTED": "COMPOSITE-ADAPTATION",
    "NEW_GAME_CONTENT": "NEW GAME CONTENT",
}

EXPECTED_FACTIONS = {
    "RockRaiders": 16,
    "Astronauts": 21,
    "Aliens": 12,
    "Martians": 17,
}

REQUIRED_PACKET_SECTIONS = [
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

VIEW_COVERAGE_KEYS = {
    "front", "rear", "leftRight", "top", "threeQuarter", "undersideInterior", "mechanism",
}
VIEW_COVERAGE_STATES = {"VERIFIED", "PARTIAL", "MISSING", "NOT_APPLICABLE"}


def fail(message: str) -> None:
    print(f"M8.5 SUPER SCOUT: FAIL {message}", file=sys.stderr)
    raise SystemExit(1)


def require_nonempty(record: dict, field: str, identity: str) -> None:
    value = record.get(field)
    if value is None or value == "" or value == []:
        fail(f"{identity} has no {field}")


def validate_source_evidence(
    evidence: dict,
    assets: list[dict],
    instruction_by_id: dict[str, dict],
    faction: str,
    label: str,
    expected_status: str,
    expected_audited: int,
    expected_gaps: int,
) -> tuple[int, int]:
    if evidence.get("schemaVersion") != 1 or evidence.get("task") != "T082":
        fail(f"{label} evidence schema/task mismatch")
    if evidence.get("faction") != faction:
        fail(f"{label} evidence faction mismatch")
    if evidence.get("status") != expected_status:
        fail(f"{label} evidence status mismatch")

    evidence_records = evidence.get("sources", [])
    evidence_ids = [record.get("setId") for record in evidence_records]
    faction_source_ids = {
        set_id
        for asset in assets if asset["faction"] == faction
        for set_id in asset["sourceSets"]
    }
    if len(evidence_ids) != len(set(evidence_ids)) or set(evidence_ids) != faction_source_ids:
        fail(f"{label} evidence must cover its mapped source set exactly once")

    audited_sources = 0
    evidence_gaps = 0
    for record in evidence_records:
        set_id = record["setId"]
        for field in ("instructionPdfs", "constructionRanges", "viewCoverage", "findings", "openGaps"):
            if field not in record:
                fail(f"{label} source {set_id} has no {field}")
        if set(record["viewCoverage"]) != VIEW_COVERAGE_KEYS:
            fail(f"{label} source {set_id} has incomplete view/mechanism coverage")
        for value in record["viewCoverage"].values():
            if value.split(" ", 1)[0] not in VIEW_COVERAGE_STATES:
                fail(f"{label} source {set_id} has invalid coverage state {value}")
        require_nonempty(record, "findings", f"{label} source {set_id}")
        require_nonempty(record, "openGaps", f"{label} source {set_id}")
        if record.get("evidenceState") == "OFFICIAL_PDF_VISUALLY_AUDITED":
            audited_sources += 1
            pdfs = record["instructionPdfs"]
            if not pdfs or not record["constructionRanges"]:
                fail(f"{label} source {set_id} audited state lacks PDFs or page ranges")
            if [pdf["url"] for pdf in pdfs] != instruction_by_id[set_id]["pdfUrls"]:
                fail(f"{label} source {set_id} PDF list disagrees with instruction index")
            for pdf in pdfs:
                if not isinstance(pdf.get("pageCount"), int) or pdf["pageCount"] <= 0:
                    fail(f"{label} source {set_id} has invalid page count")
                sha256 = pdf.get("sha256", "")
                if len(sha256) != 64 or any(character not in "0123456789abcdef" for character in sha256):
                    fail(f"{label} source {set_id} has invalid PDF SHA-256")
            for page_range in record["constructionRanges"]:
                require_nonempty(page_range, "pages", f"{label} source {set_id} page range")
                require_nonempty(page_range, "evidence", f"{label} source {set_id} page range")
        elif record.get("evidenceState") == "ARCHIVAL_GAP":
            evidence_gaps += 1
            if record["instructionPdfs"] or record["constructionRanges"]:
                fail(f"{label} source {set_id} archival gap contains unverified PDF evidence")
            if instruction_by_id[set_id]["state"] != "NO_OFFICIAL_PDF_LOCATED":
                fail(f"{label} source {set_id} gap disagrees with instruction index")
        else:
            fail(f"{label} source {set_id} has invalid evidence state")

    if audited_sources != expected_audited or evidence_gaps != expected_gaps:
        fail(
            f"{label} evidence expected {expected_audited} audited sources and "
            f"{expected_gaps} gaps, found {audited_sources}/{evidence_gaps}"
        )
    return audited_sources, evidence_gaps


def main() -> None:
    for path in (
        MANIFEST, LEDGER, INSTRUCTION_INDEX, ROCK_RAIDERS_EVIDENCE, ASTRONAUTS_EVIDENCE,
        CONFUSION, CONTENT, GENERATOR,
    ):
        if not path.is_file():
            fail(f"missing {path.relative_to(ROOT)}")

    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    ledger = json.loads(LEDGER.read_text(encoding="utf-8"))
    instruction_index = json.loads(INSTRUCTION_INDEX.read_text(encoding="utf-8"))
    rock_raiders_evidence = json.loads(ROCK_RAIDERS_EVIDENCE.read_text(encoding="utf-8"))
    astronauts_evidence = json.loads(ASTRONAUTS_EVIDENCE.read_text(encoding="utf-8"))
    confusion = json.loads(CONFUSION.read_text(encoding="utf-8"))
    content = json.loads(CONTENT.read_text(encoding="utf-8"))
    if manifest.get("schemaVersion") != 1 or manifest.get("task") != "T082":
        fail("identity baseline schema/task mismatch")
    if manifest.get("corpusStatus") != "IN_PROGRESS_IDENTITY_BASELINE":
        fail("identity baseline must not imply completed T082 acceptance")
    if manifest.get("cameraWidthsCells") != [24, 44, 72]:
        fail("canonical Super Scout camera widths drifted")

    assets = manifest.get("assets", [])
    if len(assets) != 66:
        fail(f"expected 66 assets, found {len(assets)}")
    ids = [asset.get("stableId") for asset in assets]
    if len(set(ids)) != len(ids):
        fail("duplicate stable IDs in identity baseline")
    if Counter(asset.get("kind") for asset in assets) != Counter({"Unit": 35, "Infrastructure": 31}):
        fail("identity baseline is not the canonical 35-unit/31-infrastructure roster")
    if Counter(asset.get("faction") for asset in assets) != Counter(EXPECTED_FACTIONS):
        fail("identity baseline faction counts drifted")

    runtime_assets = {
        entity["stableId"]: entity
        for entity in content.get("entities", [])
        if entity.get("stableId", "").startswith(("unit.", "building."))
    }
    if set(ids) != set(runtime_assets):
        missing = sorted(set(runtime_assets) - set(ids))
        extra = sorted(set(ids) - set(runtime_assets))
        fail(f"roster mismatch missing={missing} extra={extra}")

    sources = ledger.get("sources", [])
    if ledger.get("schemaVersion") != 1 or ledger.get("task") != "T082":
        fail("source ledger schema/task mismatch")
    source_ids = [source.get("setId") for source in sources]
    if len(source_ids) != len(set(source_ids)):
        fail("duplicate set IDs in source ledger")
    source_by_id = {source["setId"]: source for source in sources}
    used_sources: set[str] = set()

    for asset in assets:
        stable_id = asset["stableId"]
        runtime = runtime_assets[stable_id]
        for field in ("displayName", "kind", "faction", "role", "footprint", "sourceClassification", "sourceSets", "silhouetteThesis", "identityAnchors", "researchState"):
            require_nonempty(asset, field, stable_id)
        expected_kind = "Infrastructure" if runtime["selectableKind"] == "Building" else "Unit"
        if asset["kind"] != expected_kind:
            fail(f"{stable_id} kind differs from runtime roster")
        if asset["faction"] != runtime["faction"] or asset["footprint"] != runtime["footprint"]:
            fail(f"{stable_id} faction or footprint differs from runtime roster")
        expected_classification = CLASSIFICATIONS[runtime["sourceClassification"]]
        if asset["sourceClassification"] != expected_classification:
            fail(f"{stable_id} source classification differs from runtime roster")
        anchors = asset["identityAnchors"]
        if not 3 <= len(anchors) <= 7 or len(set(anchors)) != len(anchors):
            fail(f"{stable_id} needs three-to-seven distinct identity anchors")
        if asset["researchState"] != "IDENTITY_BASELINE":
            fail(f"{stable_id} incorrectly implies research completion")
        for set_id in asset["sourceSets"]:
            if set_id not in source_by_id:
                fail(f"{stable_id} cites unknown source set {set_id}")
            used_sources.add(set_id)

    if used_sources != set(source_ids):
        fail(f"source ledger has unused records: {sorted(set(source_ids) - used_sources)}")
    for source in sources:
        set_id = source["setId"]
        for field in ("title", "theme", "year", "officialInstructionsUrl", "inventoryUrl", "verification", "evidenceUse"):
            require_nonempty(source, field, f"source {set_id}")
        official = urlparse(source["officialInstructionsUrl"])
        inventory = urlparse(source["inventoryUrl"])
        if official.scheme != "https" or official.netloc != "www.lego.com" or not official.path.endswith(f"/{set_id}"):
            fail(f"source {set_id} has invalid official instruction URL")
        if inventory.scheme != "https" or inventory.netloc != "www.bricklink.com":
            fail(f"source {set_id} has invalid inventory URL")
        if source["verification"] not in {"PRIMARY_VERIFIED", "CANON_VERIFIED_ARCHIVAL"}:
            fail(f"source {set_id} has invalid confidence state")

    if instruction_index.get("schemaVersion") != 1 or instruction_index.get("task") != "T082":
        fail("instruction index schema/task mismatch")
    if instruction_index.get("status") != "IN_PROGRESS_DIRECT_PDF_INDEX":
        fail("instruction index must not imply complete visual evidence")
    instruction_records = instruction_index.get("records", [])
    instruction_ids = [record.get("setId") for record in instruction_records]
    if len(instruction_ids) != len(set(instruction_ids)) or set(instruction_ids) != set(source_ids):
        fail("instruction index must cover every source exactly once")
    instruction_by_id = {record["setId"]: record for record in instruction_records}
    direct_pdf_sources = 0
    archival_sources = 0
    for set_id, record in instruction_by_id.items():
        pdf_urls = record.get("pdfUrls")
        if not isinstance(pdf_urls, list) or len(pdf_urls) != len(set(pdf_urls)):
            fail(f"instruction source {set_id} has invalid or duplicate PDF URLs")
        for pdf_url in pdf_urls:
            parsed = urlparse(pdf_url)
            if (
                parsed.scheme != "https"
                or parsed.netloc != "www.lego.com"
                or not parsed.path.startswith("/cdn/product-assets/product.bi.core.pdf/")
                or not parsed.path.endswith(".pdf")
            ):
                fail(f"instruction source {set_id} has invalid official PDF URL")
        source = source_by_id[set_id]
        if record.get("state") == "DIRECT_PDF_LOCATED":
            direct_pdf_sources += 1
            if not pdf_urls or source["verification"] != "PRIMARY_VERIFIED":
                fail(f"instruction source {set_id} direct-PDF state disagrees with source ledger")
        elif record.get("state") == "NO_OFFICIAL_PDF_LOCATED":
            archival_sources += 1
            if pdf_urls or source["verification"] != "CANON_VERIFIED_ARCHIVAL":
                fail(f"instruction source {set_id} archival state disagrees with source ledger")
        else:
            fail(f"instruction source {set_id} has invalid state")

    audited_sources, evidence_gaps = validate_source_evidence(
        rock_raiders_evidence,
        assets,
        instruction_by_id,
        "RockRaiders",
        "Rock Raiders",
        "SOURCE_AUDIT_COMPLETE_WITH_TWO_GAPS",
        7,
        2,
    )
    astronauts_audited, astronauts_gaps = validate_source_evidence(
        astronauts_evidence,
        assets,
        instruction_by_id,
        "Astronauts",
        "Astronauts",
        "SOURCE_AUDIT_COMPLETE",
        18,
        0,
    )

    if confusion.get("schemaVersion") != 1 or confusion.get("task") != "T082":
        fail("confusion register schema/task mismatch")
    if confusion.get("status") != "IN_PROGRESS_CANONICAL_PAIR_BASELINE":
        fail("confusion register must not imply completed blind review")
    if confusion.get("blindReviewState") != "PENDING_24_44_72_SILHOUETTES":
        fail("confusion register must retain the open silhouette-review gate")
    pairs = confusion.get("pairs", [])
    if len(pairs) < 30:
        fail(f"expected at least 30 canonical confusion pairs, found {len(pairs)}")
    pair_keys: set[tuple[str, str]] = set()
    internal_pairs = 0
    cross_faction_pairs = 0
    for pair in pairs:
        left, right = pair.get("left"), pair.get("right")
        if left not in runtime_assets or right not in runtime_assets or left == right:
            fail(f"invalid confusion pair {left}/{right}")
        key = tuple(sorted((left, right)))
        if key in pair_keys:
            fail(f"duplicate confusion pair {left}/{right}")
        pair_keys.add(key)
        differences = pair.get("differences", [])
        if len(differences) != 3 or len(set(differences)) != 3:
            fail(f"confusion pair {left}/{right} needs exactly three distinct differences")
        require_nonempty(pair, "risk", f"confusion pair {left}/{right}")
        if pair.get("state") != "BASELINE":
            fail(f"confusion pair {left}/{right} incorrectly implies completed review")
        if runtime_assets[left]["faction"] == runtime_assets[right]["faction"]:
            internal_pairs += 1
        else:
            cross_faction_pairs += 1
    if internal_pairs == 0 or cross_faction_pairs == 0:
        fail("confusion register must cover internal and cross-faction risks")

    generated = subprocess.run(
        [sys.executable, str(GENERATOR), "--check"],
        cwd=ROOT,
        text=True,
        capture_output=True,
        check=False,
    )
    if generated.returncode != 0:
        fail(generated.stderr.strip() or generated.stdout.strip())

    packet_dir = ROOT / "Docs/Development/M85SuperScout/Packets"
    packet_paths = sorted(packet_dir.glob("*.md"))
    if len(packet_paths) != 66:
        fail(f"expected 66 generated packet files, found {len(packet_paths)}")
    for path in packet_paths:
        packet = path.read_text(encoding="utf-8")
        for section in REQUIRED_PACKET_SECTIONS:
            if packet.count(section) != 1:
                fail(f"{path.name} missing or duplicates {section}")
        if "**State:** `HOLD`" not in packet or "**Approving reviewer:** game director, not yet requested" not in packet:
            fail(f"{path.name} could be mistaken for an accepted T082 packet")

    evidence_ids_by_faction = {
        "RockRaiders": {record["setId"] for record in rock_raiders_evidence["sources"]},
        "Astronauts": {record["setId"] for record in astronauts_evidence["sources"]},
    }
    for asset in assets:
        packet_path = packet_dir / (asset["stableId"].replace(".", "_") + ".md")
        packet = packet_path.read_text(encoding="utf-8")
        expected_evidence_ids = set(asset["sourceSets"]) & evidence_ids_by_faction.get(asset["faction"], set())
        if packet.count(" source audit\n") != len(expected_evidence_ids):
            fail(f"{packet_path.name} has cross-faction or missing source-evidence blocks")
        for set_id in expected_evidence_ids:
            if f"### Set {set_id} source audit" not in packet:
                fail(f"{packet_path.name} is missing source evidence for {set_id}")

    primary = sum(source["verification"] == "PRIMARY_VERIFIED" for source in sources)
    archival = len(sources) - primary
    if primary != direct_pdf_sources or archival != archival_sources:
        fail("source-ledger confidence totals disagree with instruction index")
    print(
        "M8.5 SUPER SCOUT: PASS "
        f"assets={len(assets)} units=35 infrastructure=31 sources={len(sources)} "
        f"primaryVerified={primary} archival={archival} directPdfs={direct_pdf_sources} "
        f"rockRaidersAudited={audited_sources} rockRaidersGaps={evidence_gaps} "
        f"astronautsAudited={astronauts_audited} astronautsGaps={astronauts_gaps} packets=66 "
        f"confusionPairs={len(pairs)} state=HOLD"
    )


if __name__ == "__main__":
    main()
