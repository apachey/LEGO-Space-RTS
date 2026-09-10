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
SOURCE_ANALYSIS_POLICY = ROOT / "Content/Presentation/SuperScout/source_analysis_policy.json"
ROCK_RAIDERS_EVIDENCE = ROOT / "Content/Presentation/SuperScout/rock_raiders_source_evidence.json"
ASTRONAUTS_EVIDENCE = ROOT / "Content/Presentation/SuperScout/astronauts_source_evidence.json"
ALIENS_EVIDENCE = ROOT / "Content/Presentation/SuperScout/aliens_source_evidence.json"
MARTIANS_EVIDENCE = ROOT / "Content/Presentation/SuperScout/martians_source_evidence.json"
ROCK_RAIDERS_CONTRACTS = ROOT / "Content/Presentation/SuperScout/rock_raiders_production_contracts.json"
ASTRONAUTS_CONTRACTS = ROOT / "Content/Presentation/SuperScout/astronauts_production_contracts.json"
ALIENS_CONTRACTS = ROOT / "Content/Presentation/SuperScout/aliens_production_contracts.json"
MARTIANS_CONTRACTS = ROOT / "Content/Presentation/SuperScout/martians_production_contracts.json"
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
CONTRACT_STATES = {"SOURCE_VERIFIED", "CANON_DERIVED_ADAPTATION", "SOURCE_BOUNDED_PROVISIONAL"}
MATERIAL_ROLES = {"Body", "Accent", "Tool", "Rubber", "Glass", "Signal", "Lamp", "Neutral"}


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
    expected_official_audited: int,
    expected_archival_audited: int,
    expected_gaps: int,
) -> tuple[int, int, int]:
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

    official_audited_sources = 0
    archival_audited_sources = 0
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
            official_audited_sources += 1
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
        elif record.get("evidenceState") == "ARCHIVAL_PDF_VISUALLY_AUDITED":
            archival_audited_sources += 1
            pdfs = record["instructionPdfs"]
            if not pdfs or not record["constructionRanges"]:
                fail(f"{label} source {set_id} archival-PDF state lacks PDF or page ranges")
            if instruction_by_id[set_id]["state"] != "NO_OFFICIAL_PDF_LOCATED":
                fail(f"{label} source {set_id} archival-PDF state disagrees with instruction index")
            archival_urls = instruction_by_id[set_id].get("archivalEvidenceUrls", [])
            if [pdf["url"] for pdf in pdfs] != archival_urls:
                fail(f"{label} source {set_id} archival PDF list disagrees with instruction index")
            for pdf in pdfs:
                parsed = urlparse(pdf.get("url", ""))
                if parsed.scheme != "https" or not parsed.netloc:
                    fail(f"{label} source {set_id} has invalid archival PDF URL")
                if not isinstance(pdf.get("pageCount"), int) or pdf["pageCount"] <= 0:
                    fail(f"{label} source {set_id} has invalid archival page count")
                sha256 = pdf.get("sha256", "")
                if len(sha256) != 64 or any(character not in "0123456789abcdef" for character in sha256):
                    fail(f"{label} source {set_id} has invalid archival PDF SHA-256")
                require_nonempty(pdf, "provenance", f"{label} source {set_id} archival PDF")
            for page_range in record["constructionRanges"]:
                require_nonempty(page_range, "pages", f"{label} source {set_id} archival page range")
                require_nonempty(page_range, "evidence", f"{label} source {set_id} archival page range")
        elif record.get("evidenceState") == "ARCHIVAL_PRODUCT_VISUALLY_AUDITED":
            archival_audited_sources += 1
            if record["instructionPdfs"] or record["constructionRanges"]:
                fail(f"{label} source {set_id} product-audit state contains instruction evidence")
            references = record.get("referenceUrls", [])
            if not references or len(references) != len(set(references)):
                fail(f"{label} source {set_id} has missing or duplicate archival product references")
            if references != instruction_by_id[set_id].get("archivalEvidenceUrls", []):
                fail(f"{label} source {set_id} product references disagree with instruction index")
            for reference in references:
                parsed = urlparse(reference)
                if parsed.scheme != "https" or not parsed.netloc:
                    fail(f"{label} source {set_id} has invalid archival product URL")
            if not any(value.startswith("VERIFIED") for value in record["viewCoverage"].values()):
                fail(f"{label} source {set_id} product audit has no verified view")
        elif record.get("evidenceState") == "ARCHIVAL_GAP":
            evidence_gaps += 1
            if record["instructionPdfs"] or record["constructionRanges"]:
                fail(f"{label} source {set_id} archival gap contains unverified PDF evidence")
            if instruction_by_id[set_id]["state"] != "NO_OFFICIAL_PDF_LOCATED":
                fail(f"{label} source {set_id} gap disagrees with instruction index")
        else:
            fail(f"{label} source {set_id} has invalid evidence state")

    if (
        official_audited_sources != expected_official_audited
        or archival_audited_sources != expected_archival_audited
        or evidence_gaps != expected_gaps
    ):
        fail(
            f"{label} evidence expected {expected_official_audited} official audits, "
            f"{expected_archival_audited} archival audits and {expected_gaps} gaps, found "
            f"{official_audited_sources}/{archival_audited_sources}/{evidence_gaps}"
        )
    return official_audited_sources, archival_audited_sources, evidence_gaps


def validate_source_analysis_policy(policy: dict) -> str:
    if policy.get("schemaVersion") != 1 or policy.get("task") != "T082":
        fail("source-analysis policy schema/task mismatch")
    if policy.get("status") != "ACTIVE_SOURCE_ANALYSIS_POLICY":
        fail("source-analysis policy must remain active")
    required_order = policy.get("requiredAnalysisOrder", [])
    if len(required_order) != 6 or len(required_order) != len(set(required_order)):
        fail("source-analysis policy needs six distinct ordered safeguards")
    expected_source_states = {
        "releasedOfficialSetOrSubassembly",
        "officialPromotionalModelOrMaterial",
        "officialCombinationOrAlternateBuild",
        "officialUnreleasedModel",
        "officialGameModelOrAnimation",
        "alternativeOfficialVersion",
        "fanMoc",
    }
    if set(policy.get("sourceUsePolicy", {})) != expected_source_states:
        fail("source-analysis policy source classifications drifted")
    if policy["sourceUsePolicy"]["fanMoc"] != "EXCLUDED_FROM_CURRENT_T082_SOURCE_POOL":
        fail("fan MOCs must remain excluded from the current T082 source pool")
    method_ids = [method.get("id") for method in policy.get("candidateConstructionMethods", [])]
    if method_ids != [
        "DIRECT_SET_ADAPTATION",
        "OFFICIAL_SUBASSEMBLY_DERIVATION",
        "OFFICIAL_CROSS_SET_COMBINATION",
        "OFFICIAL_GAME_RECOMPOSITION",
        "FACTION_GRAMMAR_SYNTHESIS",
        "FUNCTION_FIRST_SOURCE_CLADDING",
    ]:
        fail("source-analysis policy construction methods drifted")
    if any(not method.get("description") for method in policy["candidateConstructionMethods"]):
        fail("source-analysis policy has an unexplained construction method")
    proposal_fields = policy.get("composedDesignProposalFields", [])
    if len(proposal_fields) != 10 or len(proposal_fields) != len(set(proposal_fields)):
        fail("composed-design proposal contract is incomplete")
    game_titles = [game.get("title") for game in policy.get("gameReferencePool", [])]
    if game_titles != ["LEGO Rock Raiders", "CrystAlien Conflict", "LEGO Battles (Nintendo DS)"]:
        fail("required official-game reference pool drifted")
    for game in policy["gameReferencePool"]:
        parsed = urlparse(game.get("referenceUrl", ""))
        if parsed.scheme != "https" or not parsed.netloc:
            fail(f"game reference {game.get('title')} has invalid URL")
        require_nonempty(game, "state", f"game reference {game.get('title')}")
        require_nonempty(game, "allowedUse", f"game reference {game.get('title')}")
    case_ids = [case.get("caseId") for case in policy.get("caseStudies", [])]
    if case_ids != [
        "RR_1277_COMPLETE_SET",
        "RR_4930_COMPLETE_SET",
        "LOM_7302_TOPOLOGY_CORRECTION",
        "MM_7691_SINGLE_UNIT_DECOMPOSITION",
        "CAC_TRAINING_CAMP_RECOMPOSITION",
    ]:
        fail("source-analysis policy case-study set drifted")
    for case in policy["caseStudies"]:
        require_nonempty(case, "finding", f"case study {case.get('caseId')}")
        require_nonempty(case, "productionConsequence", f"case study {case.get('caseId')}")
        require_nonempty(case, "evidenceUrls", f"case study {case.get('caseId')}")
    correction = policy.get("knownAuditCorrection", {})
    if correction.get("stableId") != "unit.martians.worker_robot":
        fail("Worker Robot audit correction is missing")
    for field in ("incorrectClaim", "rootCause", "preventiveGuard"):
        require_nonempty(correction, field, "Worker Robot audit correction")
    require_nonempty(policy, "packetNotice", "source-analysis policy")
    return policy["packetNotice"]


def validate_production_contracts(
    contracts: dict, assets: list[dict], faction: str, label: str, expected_count: int
) -> tuple[int, int]:
    if contracts.get("schemaVersion") != 1 or contracts.get("task") != "T082":
        fail(f"{label} production-contract schema/task mismatch")
    if contracts.get("faction") != faction:
        fail(f"{label} production-contract faction mismatch")
    if contracts.get("status") != "FACTION_CONTRACT_DRAFT_COMPLETE_HOLD_FOR_ROSTER_REVIEW":
        fail(f"{label} production contracts must retain the roster-review HOLD")

    shared = contracts.get("sharedMaterialPlan", {})
    if set(shared.get("masterMaterialRoles", [])) != MATERIAL_ROLES:
        fail(f"{label} production contracts drift from the accepted material roles")
    texture_specs = shared.get("reusableTextureFamilies", [])
    texture_ids = [spec.get("id") for spec in texture_specs]
    if len(texture_ids) != len(set(texture_ids)) or len(texture_ids) < 4:
        fail(f"{label} reusable texture families are missing or duplicated")
    for spec in texture_specs:
        identity = f"{label} texture {spec.get('id')}"
        for field in (
            "purpose", "channels", "resolution", "texelDensity", "tiling",
            "lodFallback", "provenance", "state",
        ):
            require_nonempty(spec, field, identity)
        if spec["state"] != "SPECIFIED_NOT_AUTHORED":
            fail(f"{identity} incorrectly implies authored/accepted texture work")
    require_nonempty(shared, "globalRules", f"{label} shared material plan")

    records = contracts.get("assets", [])
    ids = [record.get("stableId") for record in records]
    expected_ids = {asset["stableId"] for asset in assets if asset["faction"] == faction}
    if len(ids) != len(set(ids)) or set(ids) != expected_ids:
        fail(f"{label} production contracts must cover all {expected_count} faction assets exactly once")

    provisional = 0
    for record in records:
        stable_id = record["stableId"]
        state = record.get("contractState")
        if state not in CONTRACT_STATES:
            fail(f"{stable_id} has invalid production-contract state")
        provisional += state == "SOURCE_BOUNDED_PROVISIONAL"
        semantic = record.get("semanticParts", [])
        if len(semantic) < 3 or len(semantic) != len(set(semantic)):
            fail(f"{stable_id} needs at least three distinct semantic parts")
        construction = record.get("construction", {})
        for field in ("loadPath", "modules", "adaptationBoundary"):
            require_nonempty(construction, field, f"{stable_id} construction contract")
        motion = record.get("motion", {})
        for field in ("locomotion", "plantedContact", "pivots", "beats"):
            require_nonempty(motion, field, f"{stable_id} motion contract")
        if len(motion["pivots"]) < 2 or len(motion["beats"]) < 4:
            fail(f"{stable_id} needs at least two pivots and four state/animation beats")
        pivot_names = []
        for pivot in motion["pivots"]:
            for field in ("name", "parent", "motion", "driver"):
                require_nonempty(pivot, field, f"{stable_id} pivot")
            if not pivot["name"].startswith("Pivot_"):
                fail(f"{stable_id} pivot {pivot['name']} violates the T081 naming contract")
            parent = pivot["parent"]
            if not parent.startswith("Asset_") and parent not in pivot_names:
                fail(f"{stable_id} pivot {pivot['name']} has an unresolved parent node {parent}")
            pivot_names.append(pivot["name"])
        if len(pivot_names) != len(set(pivot_names)):
            fail(f"{stable_id} duplicates a named pivot")
        materials = record.get("materials", {})
        for field in ("geometryMustCarry", "materialRoles", "textureFamilies", "bespokeTextures"):
            if field not in materials:
                fail(f"{stable_id} material contract has no {field}")
        if len(materials["geometryMustCarry"]) < 3:
            fail(f"{stable_id} needs at least three geometry-retained forms")
        if not set(materials["materialRoles"]).issubset(MATERIAL_ROLES):
            fail(f"{stable_id} uses an unknown material role")
        if not set(materials["textureFamilies"]).issubset(set(texture_ids)):
            fail(f"{stable_id} references an unknown reusable texture family")
        sockets = record.get("sockets", [])
        if len(sockets) != len(set(sockets)) or not {"Socket_Selection", "Socket_Health"}.issubset(sockets):
            fail(f"{stable_id} has missing or duplicate common sockets")
        if any(not socket.startswith("Socket_") for socket in sockets):
            fail(f"{stable_id} violates the T081 socket naming contract")
        require_nonempty(record, "unresolved", f"{stable_id} decision ledger")
    if provisional != 0:
        fail(f"{label} production contracts retain {provisional} source-bounded provisional drafts")
    return len(records), provisional


def main() -> None:
    for path in (
        MANIFEST, LEDGER, INSTRUCTION_INDEX, SOURCE_ANALYSIS_POLICY, ROCK_RAIDERS_EVIDENCE, ASTRONAUTS_EVIDENCE,
        ALIENS_EVIDENCE,
        MARTIANS_EVIDENCE,
        ROCK_RAIDERS_CONTRACTS, ASTRONAUTS_CONTRACTS, ALIENS_CONTRACTS, MARTIANS_CONTRACTS,
        CONFUSION, CONTENT, GENERATOR,
    ):
        if not path.is_file():
            fail(f"missing {path.relative_to(ROOT)}")

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
    content = json.loads(CONTENT.read_text(encoding="utf-8"))
    packet_notice = validate_source_analysis_policy(source_analysis_policy)
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
        archival_urls = record.get("archivalEvidenceUrls", [])
        if not isinstance(archival_urls, list) or len(archival_urls) != len(set(archival_urls)):
            fail(f"instruction source {set_id} has invalid or duplicate archival evidence URLs")
        for archival_url in archival_urls:
            parsed = urlparse(archival_url)
            if parsed.scheme != "https" or not parsed.netloc:
                fail(f"instruction source {set_id} has invalid archival evidence URL")
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

    audited_sources, rock_raiders_archival_audits, evidence_gaps = validate_source_evidence(
        rock_raiders_evidence,
        assets,
        instruction_by_id,
        "RockRaiders",
        "Rock Raiders",
        "SOURCE_AUDIT_COMPLETE_WITH_ARCHIVAL_EVIDENCE",
        7,
        2,
        0,
    )
    astronauts_audited, astronauts_archival_audits, astronauts_gaps = validate_source_evidence(
        astronauts_evidence,
        assets,
        instruction_by_id,
        "Astronauts",
        "Astronauts",
        "SOURCE_AUDIT_COMPLETE",
        18,
        0,
        0,
    )
    aliens_audited, aliens_archival_audits, aliens_gaps = validate_source_evidence(
        aliens_evidence,
        assets,
        instruction_by_id,
        "Aliens",
        "Aliens",
        "SOURCE_AUDIT_COMPLETE",
        8,
        0,
        0,
    )
    martians_audited, martians_archival_audits, martians_gaps = validate_source_evidence(
        martians_evidence,
        assets,
        instruction_by_id,
        "Martians",
        "Martians",
        "SOURCE_AUDIT_COMPLETE_WITH_TWO_GAPS",
        8,
        0,
        2,
    )
    rock_raiders_contract_count, rock_raiders_provisional_contracts = validate_production_contracts(
        rock_raiders_contracts, assets, "RockRaiders", "Rock Raiders", 16
    )
    astronauts_contract_count, astronauts_provisional_contracts = validate_production_contracts(
        astronauts_contracts, assets, "Astronauts", "Astronauts", 21
    )
    aliens_contract_count, aliens_provisional_contracts = validate_production_contracts(
        aliens_contracts, assets, "Aliens", "Aliens", 12
    )
    martians_contract_count, martians_provisional_contracts = validate_production_contracts(
        martians_contracts, assets, "Martians", "Martians", 17
    )
    martian_contracts_by_id = {
        record["stableId"]: record for record in martians_contracts["assets"]
    }
    worker_contract = martian_contracts_by_id["unit.martians.worker_robot"]
    if "exactly two" not in worker_contract["construction"]["loadPath"]:
        fail("Worker Robot production contract no longer protects the corrected biped topology")
    aero_skiff_contract = martian_contracts_by_id["unit.martians.aero_skiff"]
    if "1195 lacks modelable construction evidence" not in aero_skiff_contract["construction"]["adaptationBoundary"]:
        fail("Aero Skiff production contract no longer bounds the 1195 evidence gap")
    tube_link_contract = martian_contracts_by_id["building.mar.aero_tube_link"]
    if "3750 has no modelable interior evidence" not in tube_link_contract["construction"]["adaptationBoundary"]:
        fail("Aero Tube Link production contract no longer bounds the 3750 evidence gap")
    contract_count = (
        rock_raiders_contract_count
        + astronauts_contract_count
        + aliens_contract_count
        + martians_contract_count
    )
    provisional_contracts = (
        rock_raiders_provisional_contracts
        + astronauts_provisional_contracts
        + aliens_provisional_contracts
        + martians_provisional_contracts
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

    matrix_dir = ROOT / "Docs/Development/M85SuperScout/Matrices"
    for slug, label in (
        ("rock_raiders", "Rock Raiders"),
        ("astronauts", "Astronauts"),
        ("aliens", "Aliens"),
        ("martians", "Martians"),
    ):
        expected_matrix_headings = {
            f"{slug}_semantic_construction.md": f"# M8.5 T082 — {label} semantic-construction matrix",
            f"{slug}_motion_socket.md": f"# M8.5 T082 — {label} motion and socket matrix",
            f"{slug}_material_texture.md": f"# M8.5 T082 — {label} material and texture-needs matrix",
        }
        for filename, expected_heading in expected_matrix_headings.items():
            heading = (matrix_dir / filename).read_text(encoding="utf-8").splitlines()[0]
            if heading != expected_heading:
                fail(f"{filename} has stale or cross-faction heading")

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
        if packet_notice not in packet:
            fail(f"{path.name} is missing the source-decomposition policy notice")
    contracted_packets = sum("- Contract state: `" in path.read_text(encoding="utf-8") for path in packet_paths)
    if contracted_packets != contract_count:
        fail(f"expected {contract_count} contract-enriched packets, found {contracted_packets}")

    evidence_ids_by_faction = {
        "RockRaiders": {record["setId"] for record in rock_raiders_evidence["sources"]},
        "Astronauts": {record["setId"] for record in astronauts_evidence["sources"]},
        "Aliens": {record["setId"] for record in aliens_evidence["sources"]},
        "Martians": {record["setId"] for record in martians_evidence["sources"]},
    }
    for asset in assets:
        packet_path = packet_dir / (asset["stableId"].replace(".", "_") + ".md")
        packet = packet_path.read_text(encoding="utf-8")
        expected_evidence_ids = set(asset["sourceSets"]) & evidence_ids_by_faction.get(asset["faction"], set())
        if packet.count("### Source audit [") != len(expected_evidence_ids):
            fail(f"{packet_path.name} has cross-faction or missing source-evidence blocks")
        for set_id in expected_evidence_ids:
            if f"### Source audit [{asset['faction']}:{set_id}]" not in packet:
                fail(f"{packet_path.name} is missing source evidence for {set_id}")

    primary = sum(source["verification"] == "PRIMARY_VERIFIED" for source in sources)
    archival = len(sources) - primary
    if primary != direct_pdf_sources or archival != archival_sources:
        fail("source-ledger confidence totals disagree with instruction index")
    print(
        "M8.5 SUPER SCOUT: PASS "
        f"assets={len(assets)} units=35 infrastructure=31 sources={len(sources)} "
        f"primaryVerified={primary} archival={archival} directPdfs={direct_pdf_sources} "
        f"rockRaidersOfficialAudits={audited_sources} rockRaidersArchivalAudits={rock_raiders_archival_audits} rockRaidersGaps={evidence_gaps} "
        f"astronautsAudited={astronauts_audited} astronautsArchivalAudits={astronauts_archival_audits} astronautsGaps={astronauts_gaps} "
        f"aliensAudited={aliens_audited} aliensArchivalAudits={aliens_archival_audits} aliensGaps={aliens_gaps} "
        f"martiansAudited={martians_audited} martiansArchivalAudits={martians_archival_audits} martiansGaps={martians_gaps} "
        f"contracts={contract_count} provisionalContracts={provisional_contracts} "
        f"packets=66 confusionPairs={len(pairs)} state=HOLD"
    )


if __name__ == "__main__":
    main()
