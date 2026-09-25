#!/usr/bin/env python3
"""Focused integrity checks for the ETX Alien Strike T083 proposal only."""
import hashlib
import json
import re
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[4]
REGISTRY = ROOT / "Docs/Development/M85UnitDesign/registry.json"
SPEC = Path(__file__).with_name("etx_alien_strike_design_spec_20260926_v1.md")
REVIEW = Path(__file__).with_name("etx_alien_strike_director_review_20260926_v1.html")
CAPTURE = Path(__file__).with_name("etx_alien_strike_director_review_20260926_v1.png")
MANIFEST = ROOT / "ArtSource/M85/T083/ETXAlienStrikeV1/source_evidence_manifest.json"
TARGET = ROOT / "ArtSource/M85/T083/ETXAlienStrikeV1/etx_alien_strike_production_target_20260926_v1.png"

def check(ok, message):
    if not ok:
        raise SystemExit(f"FAIL: {message}")

registry = json.loads(REGISTRY.read_text())
manifest = json.loads(MANIFEST.read_text())
entry = next(x for x in registry["assets"] if x["stableId"] == "unit.aliens.etx_alien_strike")
first_open = next(x for x in registry["assets"] if not x.get("directorAccepted"))
check(first_open["stableId"] == "unit.aliens.etx_alien_infiltrator" and first_open.get("status") == "QUEUED_NOT_AUTHORED", "next unit remains queued and untouched")
spec, review = SPEC.read_text(), REVIEW.read_text()
check(entry["status"] == "DESIGN_ACCEPTED_T084_AUTHORIZED" and entry["directorAccepted"] and entry["productionAuthorized"], "director acceptance and unit-specific T084 authorization are recorded")
check(registry["authoredProposals"] == sum(x.get("status") != "QUEUED_NOT_AUTHORED" for x in registry["assets"]), "authored proposal count matches registry")
check(registry["directorAccepted"] == sum(bool(x.get("directorAccepted")) for x in registry["assets"]), "accepted count matches registry")
check(entry["productionDesignTarget"]["mode"] == "SOURCE_LOCKED" and entry["productionDesignTarget"]["state"] == "DIRECTOR_ACCEPTED", "correct target mode and accepted state")
check(entry.get("acceptanceGate") == "PRODUCTION_DESIGN_TARGET_GATE" and entry.get("acceptanceDate") == "2026-09-26" and entry.get("acceptanceNote"), "specific director acceptance record")
check(registry["productionModels"] == 0, "no T084 production model has started")
check(entry["gameAdaptations"] == [] and "Видимих адаптацій немає." in review, "no adaptations are invented")
check(manifest["officialInstructionPdf"] == "https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4523183.pdf", "official source PDF reference")
check(manifest["officialBoxArtSourceSha256"] == hashlib.sha256((ROOT / manifest["officialBoxArtLocalSource"]).read_bytes()).hexdigest(), "official box-art source hash")
check(manifest["targetImageSha256"] == hashlib.sha256(TARGET.read_bytes()).hexdigest(), "Production Target hash")
check("TARGET_SHA256_TO_FILL" not in MANIFEST.read_text(), "manifest contains no placeholder hashes")
check(all(x in review for x in ("lang=\"uk\"", "ДИРЕКТОР ПРИЙНЯВ · 26.09.2026", "T084 — НЕ РОЗПОЧАТО", "SOURCE_LOCKED", "Production Design Target")), "director screen records acceptance and keeps T084 unstarted")
check("Прийняте рішення" in review and "Прийнято саме SOURCE_LOCKED" in review, "review shows the accepted visual decision")
check("Показаний поруч ровер" in review and "не входить до цього юніта" in review, "box-art staging is distinguished from the unit target")
check("T084 авторизовано лише для ETX Alien Strike" in spec and "не розпочато" in spec, "Design Spec records unit-only T084 authorization without starting it")
for attr in re.findall(r'(?:href|src)="([^\"]+)"', review):
    if attr.startswith(("https://", "http://", "#")):
        continue
    check((REVIEW.parent / attr).resolve().is_file(), f"broken local review link: {attr}")
for path in (SPEC, REVIEW, CAPTURE, MANIFEST, TARGET):
    check(path.is_file(), f"required artifact exists: {path.name}")
capture = Image.open(CAPTURE)
check(capture.format == "PNG" and capture.size == (1320, 900), "review PNG has expected first-screen dimensions")
print("PASS: ETX Alien Strike director acceptance, SOURCE_LOCKED target, provenance/hash, local links, Ukrainian review and PNG capture")
