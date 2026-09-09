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


def fail(message: str) -> None:
    print(f"M8.5 SUPER SCOUT: FAIL {message}", file=sys.stderr)
    raise SystemExit(1)


def require_nonempty(record: dict, field: str, identity: str) -> None:
    value = record.get(field)
    if value is None or value == "" or value == []:
        fail(f"{identity} has no {field}")


def main() -> None:
    for path in (MANIFEST, LEDGER, CONFUSION, CONTENT, GENERATOR):
        if not path.is_file():
            fail(f"missing {path.relative_to(ROOT)}")

    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    ledger = json.loads(LEDGER.read_text(encoding="utf-8"))
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

    primary = sum(source["verification"] == "PRIMARY_VERIFIED" for source in sources)
    archival = len(sources) - primary
    print(
        "M8.5 SUPER SCOUT: PASS "
        f"assets={len(assets)} units=35 infrastructure=31 sources={len(sources)} "
        f"primaryVerified={primary} archival={archival} packets=66 "
        f"confusionPairs={len(pairs)} state=HOLD"
    )


if __name__ == "__main__":
    main()
