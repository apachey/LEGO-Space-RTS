#!/usr/bin/env python3
"""Assemble existing T082 images without replacing historical boards or artwork."""

from __future__ import annotations

import argparse
import hashlib
import html
import json
from pathlib import Path
import tempfile

ROOT = Path(__file__).resolve().parents[1]
BASE = ROOT / "Docs/Development/M85SuperScout/Silhouettes/FullV2"
OUTPUT = BASE / "CurrentComparisonV1"
PENDING_UNITS = {
    "unit.astronauts.mobile_mining_platform",
    "unit.astronauts.mt51_claw_tank",
    "unit.astronauts.mt101_armored_drilling_unit",
}


def read(path: Path) -> dict:
    return json.loads(path.read_text(encoding="utf-8"))


def selections() -> list[dict]:
    roster = read(ROOT / "Content/Presentation/SuperScout/roster_identity_baseline.json")
    generation = read(BASE / "generation_manifest.json")
    selected = {}
    for asset in generation["assets"]:
        selected[asset["stableId"]] = {
            "stableId": asset["stableId"], "file": str((BASE / asset["output"]).relative_to(ROOT)),
            "sha256": asset["outputSha256"], "status": "HISTORICAL_BASE_CONTEXT_ONLY",
            "origin": "FullV2/generation_manifest.json:assets",
        }
    for asset in generation["revisionCandidates"]:
        selected[asset["stableId"]] = {
            "stableId": asset["stableId"], "file": str((BASE / asset["output"]).relative_to(ROOT)),
            "sha256": asset["outputSha256"], "status": asset["status"],
            "origin": "FullV2/generation_manifest.json:revisionCandidates",
        }
    # A completed adaptation, never the isolated official-donor study.
    frontier = next(a for a in generation["composedDesignCandidates"]
                    if a["stableId"] == "building.ast.frontier_extraction_station")
    selected[frontier["stableId"]] = {
        "stableId": frontier["stableId"], "file": str((BASE / frontier["output"]).relative_to(ROOT)),
        "sha256": frontier["outputSha256"], "status": "DIRECTOR_ACCEPTED_CORRECTION_CANDIDATE",
        "origin": "Docs/Development/M85_T082_COMPOSED_REVIEW_AND_BIOMECHANICAL_CONFLICT.md:director item 2",
    }
    composition = read(ROOT / "Content/Presentation/SuperScout/completed_composition_review.json")
    for asset in composition["selections"]:
        selected[asset["stableId"]] = {k: asset[k] for k in ("stableId", "file", "sha256", "status")}
        selected[asset["stableId"]]["origin"] = "completed_composition_review.json"
    alien_dir = ROOT / "ArtSource/M85/Preproduction/AlienCompletedAppearanceV1"
    alien = read(alien_dir / "completed_appearance_manifest.json")
    defense_alternate = None
    for asset in alien["assets"]:
        view = {
            "stableId": asset["stable_id"], "file": str((alien_dir / asset["output"]).relative_to(ROOT)),
            "sha256": asset["sha256"], "status": alien["status"],
            "configuration": asset["configuration"], "origin": "completed_appearance_manifest.json",
        }
        if asset["configuration"] == "air_lance":
            defense_alternate = view
        else:
            selected[asset["stable_id"]] = view
    mx = read(ROOT / "Content/Presentation/SuperScout/mx71_localized_appearance_review.json")
    selected[mx["stableId"]] = {k: mx[k] for k in ("stableId", "file", "sha256", "status")}
    selected[mx["stableId"]]["origin"] = "mx71_localized_appearance_review.json"
    claw_dir = ROOT / "ArtSource/M85/Preproduction/ClawTankArmCorrectionV2"
    claw = read(claw_dir / "edit_manifest.json")
    selected[claw["stableId"]] = {
        "stableId": claw["stableId"], "file": str((claw_dir / claw["output"]["file"]).relative_to(ROOT)),
        "sha256": claw["output"]["sha256"], "status": claw["status"],
        "origin": "ClawTankArmCorrectionV2/edit_manifest.json",
    }
    # Native MT is not promoted to a finished appearance. Keep old raster as
    # explicitly unresolved context; never ask the director to approve a blockout.
    selected["unit.astronauts.mt101_armored_drilling_unit"]["status"] = "UNRESOLVED_CONTEXT_NOT_FINISHED_APPEARANCE"
    assets = roster["assets"]
    if len(assets) != 66 or len(selected) != 66 or set(selected) != {a["stableId"] for a in assets}:
        raise ValueError("current comparison requires the exact 66-asset roster")
    result = []
    for index, asset in enumerate(assets, 1):
        record = dict(selected[asset["stableId"]])
        record.update(code=f"C{index:02d}", name=asset["displayName"], faction=asset["faction"],
                      priorityPending=asset["stableId"] in PENDING_UNITS)
        if record["stableId"] == "building.ali.etx_defense_node":
            if defense_alternate is None:
                raise ValueError("current comparison lost the accepted alternate defense head")
            record["alternate"] = defense_alternate
        for view in (record, record.get("alternate")):
            if view is None:
                continue
            image = ROOT / view["file"]
            if image.read_bytes()[:8] != b"\x89PNG\r\n\x1a\n" or hashlib.sha256(image.read_bytes()).hexdigest() != view["sha256"]:
                raise ValueError(f"selected image changed: {view['file']}")
        result.append(record)
    return result


def gallery(records: list[dict]) -> str:
    cards = []
    for record in records:
        views = [record] + ([record["alternate"]] if "alternate" in record else [])
        images = []
        for view in views:
            # Gallery is six levels below the repository root.
            src = "../../../../../../" + view["file"]
            images.append(f'<a href="{html.escape(src)}"><img loading="lazy" src="{html.escape(src)}" alt="{html.escape(record["name"])}"></a>')
        pending = ' data-pending="true"' if record["priorityPending"] else ''
        caption = html.escape(record["status"].replace("_", " "))
        cards.append(f'<article{pending}><h2>{record["code"]} — {html.escape(record["name"])}</h2>'
                     f'<p>{html.escape(record["faction"])} · {caption}</p>{"".join(images)}</article>')
    return '''<!doctype html><html lang="uk"><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>T082 — актуальні зображення</title><style>
body{margin:0;background:#eceef1;color:#17202b;font:16px system-ui}header{padding:24px;max-width:1200px;margin:auto}h1{margin:0 0 14px}p{line-height:1.5}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(min(100%,420px),1fr));gap:18px;padding:18px}
article{background:white;border:1px solid #c9ced6;border-radius:10px;padding:16px}h2{font-size:19px;margin:0}article p{font-size:12px;color:#495461;overflow-wrap:anywhere}img{display:block;width:100%;height:auto}button{padding:10px 16px;border-radius:6px;border:1px solid #748293;background:white;font:inherit;cursor:pointer}body.pending article:not([data-pending]){display:none}a{color:inherit}
</style><header><h1>T082 — актуальний порівняльний набір</h1>
<p>66 позицій реєстру, 67 зображень із двома головами однієї оборонної споруди. Тут використано вже збережені виправлення, а не повторні генерації. Натисни на зображення для повного розміру.</p>
<p>Це огляд із назвами, не сліпий тест і не перевірка масштабу в грі. Старі базові картинки залишені лише як контекст, не як автоматично погоджений вигляд. MT-101 ще потребує завершеного вигляду; його старий контекст не є пропозицією погодити технічну геометрію. У Mothership досі не погоджені оператори та рельєф замість друку.</p>
<button id="filter" type="button" aria-pressed="false">Лише три відкриті пріоритетні юніти</button></header><main>''' + "".join(cards) + '''</main><script>
document.getElementById('filter').addEventListener('click',function(){const pending=document.body.classList.toggle('pending');this.setAttribute('aria-pressed',String(pending));this.textContent=pending?'Показати всі 66 позицій':'Лише три відкриті пріоритетні юніти';});
</script></html>'''


def write_outputs(destination: Path) -> None:
    records = selections()
    destination.mkdir(parents=True, exist_ok=True)
    (destination / "index.html").write_text(gallery(records), encoding="utf-8")
    manifest = {"schema": 1, "task": "T082", "gate": "DIAGNOSTIC",
                "state": "CURRENT_COMPARISON_NOT_BLIND_OR_PRODUCTION_ACCEPTANCE",
                "assets": 66, "views": 67, "productionAccepted": False, "canonImpact": "NONE",
                "priorityPending": sorted(PENDING_UNITS), "selections": records}
    (destination / "selection_manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    if args.check:
        with tempfile.TemporaryDirectory(prefix="m85-current-comparison-") as temp:
            destination = Path(temp)
            write_outputs(destination)
            for name in ("index.html", "selection_manifest.json"):
                if not (OUTPUT / name).is_file() or (OUTPUT / name).read_bytes() != (destination / name).read_bytes():
                    raise SystemExit("current comparison is stale")
    else:
        write_outputs(OUTPUT)
    print("M85 CURRENT COMPARISON: PASS assets=66 views=67 gate=DIAGNOSTIC productionAccepted=false")


if __name__ == "__main__":
    main()
