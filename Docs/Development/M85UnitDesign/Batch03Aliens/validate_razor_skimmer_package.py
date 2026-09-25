#!/usr/bin/env python3
"""Check the Razor Skimmer T083 review package without changing it."""
import hashlib
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[4]
REVIEW = ROOT / "Docs/Development/M85UnitDesign/Batch03Aliens/razor_skimmer_director_review_20260925_v2.html"
SPEC = ROOT / "Docs/Development/M85UnitDesign/Batch03Aliens/razor_skimmer_design_spec_20260925_v2.md"
REGISTRY = ROOT / "Docs/Development/M85UnitDesign/registry.json"
PACKET = ROOT / "Docs/Development/M85SuperScout/Packets/unit_aliens_razor_skimmer.md"
ART = ROOT / "ArtSource/M85/T083/RazorSkimmerV1"
TARGET = ART / "razor_skimmer_production_target_20260925_v3.png"


def check(condition, message):
    if not condition:
        raise SystemExit(f"FAIL: {message}")


registry = json.loads(REGISTRY.read_text())
entry = next(a for a in registry["assets"] if a["stableId"] == "unit.aliens.razor_skimmer")
target = entry["productionDesignTarget"]
review = REVIEW.read_text()
spec = SPEC.read_text()
source_manifest = json.loads((ART / "source_evidence_manifest.json").read_text())
target_manifest = json.loads((ART / "production_target_manifest.json").read_text())

check(entry["status"] == "DESIGN_ACCEPTED_T084_AUTHORIZED" and entry.get("acceptanceDate") == "2026-09-26", "unit director acceptance is recorded for the approval date")
check(entry["directorAccepted"] is True and entry["productionAuthorized"] is True, "accepted visual target authorizes later T084, but not its start")
check(target["mode"] == "ORIGINAL_EXTENDED" and target["state"] == "DIRECTOR_ACCEPTED", "target mode and accepted state")
check(target["reviewReady"] is True and target["artifact"].endswith(REVIEW.relative_to(ROOT).as_posix()), "review target points to this page")
check(registry["authoredProposals"] == sum(a.get("status") != "QUEUED_NOT_AUTHORED" for a in registry["assets"]), "registry authored count")
check(registry["directorAccepted"] == sum(bool(a.get("directorAccepted")) for a in registry["assets"]), "registry accepted count")
check(registry["productionModels"] == 0, "T084 production models remain unstarted")
check(PACKET.is_file() and SPEC.is_file(), "T082 packet and T083 Design Spec exist")
check([s["setId"] for s in source_manifest["sources"]] == ["7645", "7692", "7697"], "exact T082 donor sets")
check(all(s["officialPdfBook1"].startswith("https://www.lego.com/cdn/") for s in source_manifest["sources"]), "official LEGO instructions are linked")
digest = hashlib.sha256(TARGET.read_bytes()).hexdigest()
check(digest == target_manifest["sha256"], "Production Design Target hash")
check(target_manifest["target"] == TARGET.name and target_manifest["mode"] == "ORIGINAL_EXTENDED", "target manifest identity")
check(target_manifest["status"] == "DIRECTOR_ACCEPTED" and "ПРИЙНЯТО ДИРЕКТОРОМ · 2026-09-26" in review and "T084 — НЕ РОЗПОЧАТО" in review, "accepted visual target and unstarted T084 are visible")
check("Рішення директора" in spec and "прийняв 2026-09-26" in spec, "acceptance decision is recorded")
check("ORIGINAL_EXTENDED" in review and "Razor Skimmer" in review, "review identity and mode")
check("Alien Jet — повітряний юніт" in review, "review distinguishes its role from Alien Jet")
check("<details>" in review and "Технічні деталі" in review, "technical details are collapsed")
check("SOURCE_LOCKED" not in review, "target mode remains ORIGINAL_EXTENDED")
check("напрям 3" in spec and "відкритим V-проміжком" in spec, "specification records the selected direction")
check("не підтверджено" in spec and "Studio" in spec, "unverified part/build limitation is explicit")
for url in re.findall(r'(?:href|src)="([^"]+)"', review):
    if url.startswith(("http://", "https://", "#")):
        continue
    check((REVIEW.parent / url).resolve().is_file(), f"broken local review link: {url}")

print("PASS: Razor Skimmer T083 identity, director acceptance, T084-not-started state, source manifests, target hash, appearance summary, and local review links")
