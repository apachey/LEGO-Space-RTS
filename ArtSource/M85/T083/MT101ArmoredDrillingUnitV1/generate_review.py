#!/usr/bin/env python3
"""Generate the MT-101 T083 director review HTML and a matching PNG plate."""
from __future__ import annotations

import argparse
import hashlib
import json
import tempfile
from io import BytesIO
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[4]
OUT = ROOT / "Docs/Development/M85UnitDesign/Batch02Astronauts"
SOURCE = ROOT / "ArtSource/M85/Preproduction/MT101SourceRebuildV1/mt101_source_rebuild_rev5.png"
MODULES = ROOT / "ArtSource/M85/Preproduction/MT101ControlledAppearanceV1/reference_7699_book2_page43.png"
TARGET_CROP = ROOT / "ArtSource/M85/T083/MT101ArmoredDrillingUnitV1/mt101_t082_accepted_completed_appearance_target.png"
EVIDENCE = ROOT / "ArtSource/M85/T083/MT101ArmoredDrillingUnitV1/source_manifest.json"
HTML_OUT = OUT / "mt101_armored_drilling_unit_director_review_20260925_v1.html"
PNG_OUT = OUT / "mt101_armored_drilling_unit_director_review_20260925_v1.png"
SPEC = "mt101_armored_drilling_unit_design_spec_20260925_v1.md"
STABLE_ID = "unit.astronauts.mt101_armored_drilling_unit"

TITLE = "MT-101 Armored Drilling Unit"
ALT = "Затверджений джерельний образ LEGO 7699: біло-помаранчева шестиколісна важка машина з передньою кабіною, буром, окремим верхнім пускачем і пристикованим заднім літальним модулем."
TARGET_REL = "../../../../ArtSource/M85/T083/MT101ArmoredDrillingUnitV1/mt101_t082_accepted_completed_appearance_target.png"
FULL_SOURCE_REL = "../../../../ArtSource/M85/Preproduction/MT101ControlledAppearanceV1/reference_7699_book2_cover.png"
MODULES_REL = "../../../../ArtSource/M85/Preproduction/MT101ControlledAppearanceV1/reference_7699_book2_page43.png"
MANIFEST_REL = "../../../../ArtSource/M85/T083/MT101ArmoredDrillingUnitV1/source_manifest.json"


def html_doc() -> str:
    return f'''<!doctype html>
<html lang="uk"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>{TITLE} — директорський огляд T083</title>
<style>
:root{{--ink:#222b2c;--muted:#536164;--line:#c8d1ce;--paper:#edf0ec;--white:#fff;--orange:#e45c25;--pale:#f8e9df;--graphite:#30403f;--green:#496c5c}}
*{{box-sizing:border-box}}body{{margin:0;background:var(--paper);color:var(--ink);font:15px/1.4 -apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif}}main{{max-width:1320px;margin:auto;padding:18px 24px 22px}}
header{{background:var(--white);border-top:6px solid var(--orange);border-bottom:1px solid var(--line);padding:13px 18px}}.eyebrow{{font-size:11px;font-weight:750;letter-spacing:.08em;color:var(--graphite)}}h1{{font-size:30px;line-height:1.05;margin:5px 0}}.subtitle{{margin:0 0 9px;color:var(--muted)}}
.tags{{display:flex;gap:7px;flex-wrap:wrap}}.tag{{padding:4px 9px;border:1px solid var(--line);background:#f8faf8;font-size:12px;font-weight:650}}.tag.mode{{background:var(--pale);border-color:#e5ab8d;color:#743a22}}
.status{{display:flex;justify-content:space-between;gap:12px;border-top:1px solid #e1e6e3;padding-top:8px;margin-top:9px;font-size:12px;font-weight:800}}.status strong{{color:var(--green)}}.status span{{color:#743a22}}
section{{margin-top:10px}}h2{{font-size:16px;margin:0 0 5px}}.hero{{display:grid;grid-template-columns:minmax(0,1.5fr) minmax(315px,.82fr);gap:10px;align-items:start}}.target{{background:#fff;border:1px solid var(--line);border-top:4px solid var(--graphite)}}.target-head{{padding:7px 11px 0;display:flex;justify-content:space-between;align-items:baseline;gap:8px}}.target-head p{{margin:0;color:var(--muted);font-size:11px}}.target img{{display:block;width:100%;height:min(47vh,480px);object-fit:contain;background:#e6f8e3}}.target-foot{{padding:6px 11px;border-top:1px solid var(--line);font-size:11px;color:#394b4b}}
.side{{display:grid;gap:7px}}.card{{background:var(--white);border:1px solid var(--line);padding:8px 10px}}.card ul{{padding-left:17px;margin:3px 0 0}}.card li{{margin:2px 0;font-size:13px;line-height:1.32}}.checklist{{border-left:4px solid var(--orange);background:#fffaf6}}.checklist ul{{list-style:none;padding:0}}.checklist li{{position:relative;padding-left:18px}}.checklist li:before{{content:"□";position:absolute;left:0;color:var(--graphite);font-weight:750}}.module{{display:grid;grid-template-columns:78px 1fr;gap:8px;align-items:center;background:#e7eeeb;border:1px solid var(--line);padding:5px 7px}}.module img{{width:78px;height:74px;object-fit:cover;object-position:center 58%;background:#fff}}.module p{{font-size:10px;line-height:1.3;margin:0}}.module strong{{display:inline;margin-right:5px;color:var(--graphite)}}
.sources{{border-top:1px solid var(--line);padding-top:8px}}.links{{display:flex;flex-wrap:wrap;gap:5px 14px;font-size:11px}}.links a{{color:#245879;text-underline-offset:2px}}.note{{font-size:10px;color:var(--muted);margin:5px 0 0}}details{{margin-top:8px;background:#e7ece9;border:1px solid var(--line);padding:6px 9px;font-size:10px;color:var(--muted)}}summary{{font-weight:750;color:#43545a;cursor:pointer}}details p{{margin:5px 0 0}}
@media(max-width:900px){{main{{padding:9px}}h1{{font-size:25px}}.hero{{grid-template-columns:minmax(0,1.2fr) minmax(280px,.8fr)}}.target img{{height:min(55vh,420px)}}.status{{flex-direction:row}}}}
@media(max-width:620px){{main{{padding:7px}}header{{padding:10px 12px}}h1{{font-size:22px}}.hero{{grid-template-columns:1fr}}.target img{{height:auto;max-height:300px}}.side{{grid-template-columns:1fr 1fr;gap:5px}}.side .checklist{{grid-column:1/-1}}.module{{grid-template-columns:55px 1fr}}.module img{{width:55px;height:58px}}.status{{font-size:10px}}.target-head{{align-items:flex-start;flex-direction:column}}}}
</style></head><body><main>
<header><div class="eyebrow">M8.5 · T083 · Astronauts — Mission Systems</div><h1>{TITLE}</h1>
<p class="subtitle">Зовнішній вигляд важкої машини LEGO 7699</p>
<div class="tags"><span class="tag">Фракція: Astronauts</span><span class="tag">Роль: важка штурмова машина</span><span class="tag mode">SOURCE_LOCKED · без змін зовнішнього вигляду</span></div>
<div class="status"><strong>ДИЗАЙН ПРИЙНЯТО · 25.09.2026</strong><span>T084 — НЕ РОЗПОЧАТО</span></div></header>
<section class="hero"><div class="target"><div class="target-head"><h2>Production Design Target</h2><p>Завершений вигляд із повітряним модулем · джерельний образ T082</p></div>
<a href="{FULL_SOURCE_REL}" aria-label="Відкрити повну офіційну сторінку LEGO 7699"><img src="{TARGET_REL}" alt="{ALT}"></a>
<div class="target-foot">Затверджений раніше джерельний образ MT-101; пристикований задній літальний модуль показано повністю. Обкладинка та інструкції LEGO — у джерелах нижче.</div></div>
<div class="side"><article class="card"><h2>Видимі адаптації</h2><p style="margin:3px 0 0"><strong>Видимих адаптацій немає.</strong></p><p class="note">Офіційна модель уже задає потрібний вигляд. Ігровий контактний бур не змінює геометрію target.</p></article>
<article class="card checklist"><h2>Прийняте рішення</h2><ul><li>Повний силует: шестиколісне шасі, бур, верхній пускач і пристикований літальний модуль.</li><li>Прийнято пропорції та біло-помаранчеву Mission Systems конструкцію.</li><li>Мінібайк залишається у відсіку заднього модуля за джерелом LEGO.</li></ul></article></div></section>
<section class="sources"><h2>Джерела</h2><div class="links"><a href="https://www.lego.com/en-us/service/buildinginstructions/7699">LEGO · набір 7699</a><a href="https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517776.pdf">Офіційна інструкція · книга 1 (PDF)</a><a href="https://www.lego.com/cdn/product-assets/product.bi.core.pdf/4517777.pdf">Офіційна інструкція · книга 2 (PDF)</a><a href="../../M85SuperScout/Matrices/astronauts_source_audit.md">Аудит джерела T082</a></div>
<details><summary>Технічні деталі</summary><p>Stable ID: <code>{STABLE_ID}</code> · <a href="{SPEC}">Design Spec</a> · <a href="{MANIFEST_REL}">Маніфест джерел і контрольних сум</a> · <a href="mt101_armored_drilling_unit_director_review_20260925_v1.png">Відкрити PNG цього огляду</a>.</p><p>Директор прийняв T083 Production Design Target 25.09.2026. T084 авторизовано для MT-101, але виробниче моделювання ще не розпочато.</p></details></section>
</main></body></html>'''


def font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont | ImageFont.ImageFont:
    choices = [
        "/System/Library/Fonts/Supplemental/Arial Bold.ttf" if bold else "/System/Library/Fonts/Supplemental/Arial.ttf",
        "/System/Library/Fonts/Supplemental/Helvetica Neue.ttc",
        "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf" if bold else "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
    ]
    for choice in choices:
        if Path(choice).exists():
            return ImageFont.truetype(choice, size)
    return ImageFont.load_default()


def wrap(draw: ImageDraw.ImageDraw, text: str, fnt: ImageFont.ImageFont, width: int) -> list[str]:
    words, lines, line = text.split(), [], ""
    for word in words:
        candidate = f"{line} {word}".strip()
        if draw.textbbox((0, 0), candidate, font=fnt)[2] <= width:
            line = candidate
        else:
            if line:
                lines.append(line)
            line = word
    if line:
        lines.append(line)
    return lines


def draw_paragraph(draw, xy, text, fnt, fill, width, gap=7):
    x, y = xy
    for line in wrap(draw, text, fnt, width):
        draw.text((x, y), line, font=fnt, fill=fill)
        y += fnt.size + gap
    return y


def review_png(destination: Path) -> None:
    W, H = 1800, 2180
    paper, ink, muted = "#edf0ec", "#222b2c", "#536164"
    orange, graphite, line, pale = "#e45c25", "#30403f", "#c8d1ce", "#f8e9df"
    im = Image.new("RGB", (W, H), paper)
    d = ImageDraw.Draw(im)
    regular, bold = font(28), font(28, True)
    header = (70, 52, W-70, 290)
    d.rounded_rectangle(header, radius=8, fill="#ffffff", outline=line, width=2)
    d.rectangle((70, 52, W-70, 62), fill=orange)
    d.text((100, 82), "M8.5 · T083 · ASTRONAUTS — MISSION SYSTEMS", font=font(22, True), fill=graphite)
    d.text((100, 116), TITLE, font=font(48, True), fill=ink)
    d.text((100, 174), "Зовнішній вигляд важкої машини LEGO 7699", font=font(28), fill=muted)
    d.rounded_rectangle((100, 220, 635, 261), radius=4, fill="#f8faf8", outline=line, width=2)
    d.text((118, 225), "Фракція: Astronauts  ·  Роль: важка штурмова", font=font(19, True), fill=ink)
    d.rounded_rectangle((655, 220, 1110, 261), radius=4, fill=pale, outline="#e5ab8d", width=2)
    d.text((672, 225), "SOURCE_LOCKED · без видимих змін", font=font(20, True), fill="#743a22")
    d.text((1160, 227), "ДИЗАЙН ПРИЙНЯТО · 25.09.2026", font=font(22, True), fill="#496c5c")
    d.text((1470, 227), "T084 — НЕ РОЗПОЧАТО", font=font(20, True), fill="#743a22")

    # Main target image: previously accepted source-led T082 appearance.
    bx = (70, 320, 1160, 1445)
    d.rounded_rectangle(bx, radius=7, fill="#ffffff", outline=line, width=2)
    d.rectangle((70, 320, 1160, 328), fill=graphite)
    d.text((100, 350), "Production Design Target", font=font(30, True), fill=ink)
    d.text((100, 393), "Завершений вигляд із повітряним модулем · джерельний образ T082", font=font(21), fill=muted)
    target = Image.open(BytesIO(target_crop_bytes())).convert("RGB")
    target.thumbnail((1010, 850), Image.Resampling.LANCZOS)
    tx, ty = 110 + (1010-target.width)//2, 435 + (850-target.height)//2
    im.paste(target, (tx, ty))
    d.line((92, 1305, 1138, 1305), fill=line, width=2)
    draw_paragraph(d, (100, 1320), "Затверджений раніше джерельний образ MT-101; пристикований задній літальний модуль показано повністю. Офіційна обкладинка та інструкції — у джерелах.", font(17), graphite, 1010, gap=4)

    # Compact decision column.
    rx0, rx1 = 1190, 1730
    d.rounded_rectangle((rx0, 320, rx1, 500), radius=7, fill="#ffffff", outline=line, width=2)
    d.text((rx0+20, 338), "Видимі адаптації", font=font(25, True), fill=ink)
    d.text((rx0+20, 382), "Видимих адаптацій немає.", font=font(21, True), fill=graphite)
    draw_paragraph(d, (rx0+20, 426), "Контактний бур не змінює геометрію target.", font(17), muted, 490, gap=4)

    d.rounded_rectangle((rx0, 515, rx1, 1050), radius=7, fill="#fffaf6", outline=line, width=2)
    d.rectangle((rx0, 515, rx0+8, 1050), fill=orange)
    d.text((rx0+24, 532), "Прийняте рішення", font=font(23, True), fill=ink)
    checks = [
        "Повний силует: шестиколісне шасі, бур, верхній пускач і пристикований літальний модуль.",
        "Прийнято пропорції та біло-помаранчеву Mission Systems конструкцію.",
        "Мінібайк залишається у відсіку заднього модуля за джерелом LEGO.",
    ]
    y = 585
    for text in checks:
        d.text((rx0+24, y), "□", font=font(23, True), fill=graphite)
        y = draw_paragraph(d, (rx0+58, y+2), text, font(18), ink, 450, gap=4) + 13

    d.rounded_rectangle((70, 1475, W-70, 1605), radius=7, fill="#ffffff", outline=line, width=2)
    d.text((96, 1490), "Джерела", font=font(23, True), fill=ink)
    draw_paragraph(d, (96, 1530), "LEGO 7699 · офіційні інструкції, книги 1 і 2. T082 фіксує вкладений мінібайк у задньому кораблі.", font(18), muted, 1580, gap=4)
    d.rounded_rectangle((70, 1620, W-70, 1715), radius=7, fill="#e7ece9", outline=line, width=2)
    d.text((96, 1635), "Технічні деталі", font=font(21, True), fill=graphite)
    draw_paragraph(d, (96, 1670), f"Stable ID: {STABLE_ID} · Директор прийняв T083 25.09.2026; T084 авторизовано, але не розпочато.", font(16), muted, 1580, gap=3)
    d.text((70, 1755), "Директор прийняв цей Production Design Target 25.09.2026.", font=font(20, True), fill=graphite)
    im.save(destination, format="PNG", optimize=False)


def target_crop_bytes() -> bytes:
    buffer = BytesIO()
    with Image.open(SOURCE) as source:
        source.convert("RGB").save(buffer, format="PNG", optimize=False)
    return buffer.getvalue()


def validate_inputs() -> None:
    manifest = json.loads(EVIDENCE.read_text(encoding="utf-8"))
    if manifest["stableId"] != STABLE_ID or manifest["targetMode"] != "SOURCE_LOCKED":
        raise SystemExit("FAIL: source manifest identity/mode mismatch")
    for item in manifest["pageRasters"]:
        path = ROOT / item["file"]
        if not path.is_file():
            raise SystemExit(f"FAIL: missing source raster {item['file']}")
        digest = hashlib.sha256(path.read_bytes()).hexdigest()
        if digest != item["sha256"]:
            raise SystemExit(f"FAIL: source raster SHA-256 mismatch: {item['file']}")
    crop = manifest["derivedTarget"]
    if crop.get("sourceFile") != str(SOURCE.relative_to(ROOT)) or crop.get("cropBoxPixels") is not None:
        raise SystemExit("FAIL: target crop provenance mismatch")
    expected_crop = target_crop_bytes()
    if hashlib.sha256(expected_crop).hexdigest() != crop.get("sha256"):
        raise SystemExit("FAIL: deterministic target crop SHA-256 mismatch")
    if not TARGET_CROP.is_file() or TARGET_CROP.read_bytes() != expected_crop:
        raise SystemExit("FAIL: target crop is absent or differs from source framing")
    registry = json.loads((ROOT / "Docs/Development/M85UnitDesign/registry.json").read_text(encoding="utf-8"))
    record = next((x for x in registry["assets"] if x["stableId"] == STABLE_ID), None)
    if record is None or not record.get("directorAccepted") or not record.get("productionAuthorized"):
        raise SystemExit("FAIL: registry identity missing or accepted state mismatch")
    if record.get("status") != "DESIGN_ACCEPTED_T084_AUTHORIZED" or record.get("productionDesignTarget", {}).get("state") != "DIRECTOR_ACCEPTED":
        raise SystemExit("FAIL: registry director acceptance status mismatch")
    from html.parser import HTMLParser

    class LocalLinks(HTMLParser):
        def __init__(self):
            super().__init__()
            self.paths: list[str] = []
        def handle_starttag(self, tag, attrs):
            for key, value in attrs:
                if key in ("href", "src") and value and not value.startswith(("https://", "http://", "#", "mailto:")):
                    self.paths.append(value)
    parser = LocalLinks()
    parser.feed(html_doc())
    for link in parser.paths:
        target = (OUT / link.split("#", 1)[0]).resolve()
        if not target.is_file():
            raise SystemExit(f"FAIL: local report link is missing: {link}")
    copy = html_doc()
    if "ДИЗАЙН ПРИЙНЯТО · 25.09.2026" not in copy or "T084 — НЕ РОЗПОЧАТО" not in copy:
        raise SystemExit("FAIL: required director acceptance states are missing")


def generate(directory: Path) -> tuple[Path, Path]:
    html_path, png_path = directory / HTML_OUT.name, directory / PNG_OUT.name
    html_path.write_bytes((html_doc() + "\n").encode("utf-8"))
    review_png(png_path)
    return html_path, png_path


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", action="store_true", help="verify exact HTML/PNG regeneration and package gates")
    args = parser.parse_args()
    if not args.check:
        TARGET_CROP.write_bytes(target_crop_bytes())
    validate_inputs()
    if args.check:
        with tempfile.TemporaryDirectory(prefix="mt101-t083-") as tmp:
            html_path, png_path = generate(Path(tmp))
            if html_path.read_bytes() != HTML_OUT.read_bytes():
                raise SystemExit("FAIL: director review HTML differs from regeneration")
            if png_path.read_bytes() != PNG_OUT.read_bytes():
                raise SystemExit("FAIL: director review PNG differs from regeneration")
        print("PASS: MT-101 source hashes, registry state, HTML and PNG regeneration")
    else:
        generate(OUT)
        print(f"Generated {HTML_OUT.relative_to(ROOT)}")
        print(f"Generated {PNG_OUT.relative_to(ROOT)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
