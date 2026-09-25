#!/usr/bin/env python3
"""Focused integrity checks for the accepted MT-201 T083 design package."""
import json
from html.parser import HTMLParser
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BASE = ROOT / "Docs/Development/M85UnitDesign/Batch02Astronauts"
REGISTRY = ROOT / "Docs/Development/M85UnitDesign/registry.json"
HTML = BASE / "mt201_ultra_drill_walker_director_review_20260925_v1.html"
SPEC = BASE / "mt201_ultra_drill_walker_design_spec_20260925_v1.md"
PROPOSAL = BASE / "mt201_ultra_drill_walker_proposal_20260925_v1.md"
MANIFEST = BASE / "mt201_ultra_drill_walker_source_evidence_20260925_v1.json"
STABLE_ID = "unit.astronauts.mt201_ultra_drill_walker"


def check(condition, message):
    if not condition:
        raise SystemExit(f"FAIL: {message}")


class ReviewParser(HTMLParser):
    def __init__(self):
        super().__init__()
        self.text = []
        self.hrefs = []
        self.images = []

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if tag == "a" and attrs.get("href"):
            self.hrefs.append(attrs["href"])
        if tag == "img" and attrs.get("src"):
            self.images.append(attrs["src"])

    def handle_data(self, data):
        self.text.append(data)


registry = json.loads(REGISTRY.read_text())
manifest = json.loads(MANIFEST.read_text())
review = HTML.read_text()
spec = SPEC.read_text()
proposal = PROPOSAL.read_text()
parsed = ReviewParser()
parsed.feed(review)
entries = [asset for asset in registry["assets"] if asset.get("stableId") == STABLE_ID]
check(len(entries) == 1, "registry contains exactly one MT-201 entry")
entry = entries[0]
counted = sum(asset.get("status") != "QUEUED_NOT_AUTHORED" for asset in registry["assets"])
accepted = sum(bool(asset.get("directorAccepted")) for asset in registry["assets"])
check(registry.get("target") == 35 and registry.get("authoredProposals") == counted, "registry authored-proposal counter matches entries")
check(registry.get("directorAccepted") == accepted, "registry acceptance counter matches entries")
check(entry.get("status") == "DESIGN_ACCEPTED_T084_AUTHORIZED", "MT-201 is director-accepted")
check(entry.get("directorAccepted") is True and entry.get("productionAuthorized") is True, "MT-201 acceptance and T084 authorization are recorded")
check(entry.get("review") == "Batch02Astronauts/mt201_ultra_drill_walker_director_review_20260925_v1.html", "unique MT-201 review artifact is registered")
target = entry.get("productionDesignTarget", {})
check(target.get("mode") == "SOURCE_LOCKED" and target.get("state") == "DIRECTOR_ACCEPTED", "source-locked target is director-accepted")
check(target.get("reviewReady") is True and target.get("artifact", "").endswith("mt201_ultra_drill_walker_director_review_20260925_v1.html"), "target artifact identity is correct")
check(target.get("sourceImage") == manifest["completedModelPhoto"]["url"], "registry points to manifested completed-model photo")
check(manifest.get("sourceSet") == "7649" and len(manifest.get("officialSources", [])) >= 3, "official source records are present")
check(manifest["completedModelPhoto"].get("sha256") is None, "remote source image hash is not falsely claimed")
check(all(url in parsed.hrefs for url in [s["url"] for s in manifest["officialSources"]]), "every official source link is present in review")
check(parsed.images == [manifest["completedModelPhoto"]["url"]], "one completed-model image is the sole visible target")
check("<html lang=\"uk\">" in review, "director review declares Ukrainian")
check("Production Design Target" in review and "SOURCE_LOCKED" in review, "target and mode are explicit")
check("ЗАТВЕРДЖЕНО ДИРЕКТОРОМ · 25.09.2026" in review and "T084 — НЕ РОЗПОЧАТО" in review, "accepted review and unstarted production states are explicit")
check("Прийняте рішення" in review and "Прийнято директором · 25.09.2026" in review, "approval decision and date are clear")
check("Видимих адаптацій немає." in review and entry.get("gameAdaptations") == [], "no visible adaptation is stated")
check("Технічні відомості" in review and "<details>" in review, "technical details are secondary")
check("Тримати" not in review, "no stray prohibited draft text")
check("LEGO 7649" in spec and "SOURCE_LOCKED" in spec and "Видимі адаптації" in spec, "Design Spec records source-locked appearance and adaptation boundary")
check("T084" in proposal and "Рішення директора: прийнято" in proposal, "accepted package records the director decision")
check("T084 — НЕ РОЗПОЧАТО" in review and "T084" in spec, "package keeps T084 unstarted")
print("PASS: MT-201 T083 registry, target, source links, Ukrainian decision state and T084 guard.")
