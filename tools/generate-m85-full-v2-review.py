#!/usr/bin/env python3
"""Build self-contained blind-review PNG boards for the T082 Full V2 corpus."""

from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import random
import tempfile

from PIL import Image, ImageChops, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[1]
ROSTER = ROOT / "Content/Presentation/SuperScout/roster_identity_baseline.json"
CORPUS = ROOT / "Docs/Development/M85SuperScout/Silhouettes/FullV2"
MANIFEST = CORPUS / "generation_manifest.json"
OUTPUT = CORPUS / "Review"
CAMERA_WIDTHS = (24, 44, 72)
FOOTPRINT_PIXELS_AT_24 = {
    "Tiny": 100,
    "Small": 150,
    "Medium": 205,
    "Large": 285,
    "Huge": 390,
}
FONT_PATH = Path("/System/Library/Fonts/Supplemental/Arial.ttf")
BOARD_SEED = 85083


def load_font(size: int) -> ImageFont.FreeTypeFont | ImageFont.ImageFont:
    if FONT_PATH.is_file():
        return ImageFont.truetype(str(FONT_PATH), size)
    return ImageFont.load_default()


def content_crop(image: Image.Image) -> Image.Image:
    rgb = image.convert("RGB")
    difference = ImageChops.difference(rgb, Image.new("RGB", rgb.size, "white")).convert("L")
    mask = difference.point(lambda value: 255 if value > 10 else 0)
    bounds = mask.getbbox()
    if bounds is None:
        raise ValueError("render is blank")
    return rgb.crop(bounds)


def coded_assets() -> list[tuple[str, dict, dict]]:
    roster = json.loads(ROSTER.read_text(encoding="utf-8"))
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    assets_by_id = {asset["stableId"]: asset for asset in roster["assets"]}
    renders_by_id = {record["stableId"]: record for record in manifest["assets"]}
    if len(assets_by_id) != 66 or set(renders_by_id) != set(assets_by_id):
        raise ValueError("Full V2 review requires exactly 66 rendered roster assets")
    ordered = list(assets_by_id.values())
    random.Random(BOARD_SEED).shuffle(ordered)
    return [
        (f"V{index:02d}", asset, renders_by_id[asset["stableId"]])
        for index, asset in enumerate(ordered, 1)
    ]


def render_board(
    destination: Path,
    camera_width: int,
    page: int,
    page_assets: list[tuple[str, dict, dict]],
) -> None:
    columns = 6
    rows = (len(page_assets) + columns - 1) // columns
    card_width, card_height = 540, 420
    margin_x, header_height, footer = 40, 130, 35
    width = margin_x * 2 + columns * card_width
    height = header_height + rows * card_height + footer
    board = Image.new("RGB", (width, height), "white")
    draw = ImageDraw.Draw(board)
    title_font = load_font(46)
    subtitle_font = load_font(25)
    code_font = load_font(32)
    draw.text((40, 25), f"T082 Full V2 blind review — {camera_width}-cell camera — page {page}/2", fill="#16181b", font=title_font)
    draw.text((40, 82), "No names, factions, colours or role hints. The same V-code is used at every camera width.", fill="#5b6068", font=subtitle_font)

    for index, (code, asset, record) in enumerate(page_assets):
        col, row = index % columns, index // columns
        x = margin_x + col * card_width
        y = header_height + row * card_height
        draw.rounded_rectangle((x + 10, y + 10, x + card_width - 10, y + card_height - 10), radius=12, outline="#d7d9de", width=3)
        render_path = CORPUS / record["output"]
        with Image.open(render_path) as source:
            cropped = content_crop(source)
        target = max(18, round(FOOTPRINT_PIXELS_AT_24[asset["footprint"]] * 24 / camera_width))
        scale = min(target / cropped.width, target / cropped.height)
        size = (max(1, round(cropped.width * scale)), max(1, round(cropped.height * scale)))
        thumbnail = cropped.resize(size, Image.Resampling.LANCZOS)
        px = x + (card_width - thumbnail.width) // 2
        py = y + 34 + (card_height - 105 - thumbnail.height) // 2
        board.paste(thumbnail, (px, py))
        code_bounds = draw.textbbox((0, 0), code, font=code_font)
        code_width = code_bounds[2] - code_bounds[0]
        draw.text((x + (card_width - code_width) // 2, y + card_height - 64), code, fill="#292c31", font=code_font)

    destination.parent.mkdir(parents=True, exist_ok=True)
    board.save(destination, format="PNG", optimize=True)


def write_outputs(destination: Path) -> None:
    coded = coded_assets()
    page_size = (len(coded) + 1) // 2
    pages = (coded[:page_size], coded[page_size:])
    destination.mkdir(parents=True, exist_ok=True)
    for camera_width in CAMERA_WIDTHS:
        for page, page_assets in enumerate(pages, 1):
            render_board(
                destination / f"blind_{camera_width}_cells_page_{page}.png",
                camera_width,
                page,
                page_assets,
            )

    key_lines = [
        "# M8.5 T082 — Full V2 blind-review answer key",
        "",
        "Open only after recording the 24/44/72-cell identifications. Full V2 remains HOLD until game-director review.",
        "",
        "| Code | Stable ID | Asset | Faction | Kind | Footprint |",
        "|---|---|---|---|---|---|",
    ]
    for code, asset, _ in coded:
        key_lines.append(
            f"| `{code}` | `{asset['stableId']}` | {asset['displayName']} | {asset['faction']} | {asset['kind']} | {asset['footprint']} |"
        )
    (destination / "BLIND_REVIEW_KEY.md").write_text("\n".join(key_lines) + "\n", encoding="utf-8")

    artifact_names = ["BLIND_REVIEW_KEY.md"] + [
        f"blind_{camera}_cells_page_{page}.png"
        for camera in CAMERA_WIDTHS
        for page in (1, 2)
    ]
    records = [
        {"file": name, "sha256": hashlib.sha256((destination / name).read_bytes()).hexdigest()}
        for name in sorted(artifact_names)
    ]
    review_manifest = {
        "schemaVersion": 1,
        "task": "T082",
        "corpus": "SOURCE_DERIVED_FULL_ROSTER_V2",
        "state": "HOLD_FOR_GAME_DIRECTOR_BLIND_REVIEW",
        "boardSeed": BOARD_SEED,
        "cameraWidthsCells": list(CAMERA_WIDTHS),
        "pagesPerWidth": 2,
        "assets": 66,
        "files": records,
    }
    (destination / "review_manifest.json").write_text(
        json.dumps(review_manifest, indent=2) + "\n", encoding="utf-8"
    )


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    if not args.check:
        write_outputs(OUTPUT)
        print(f"Wrote Full V2 blind review to {OUTPUT}")
        return

    with tempfile.TemporaryDirectory(prefix="m85-full-v2-review-") as temp_dir:
        candidate = Path(temp_dir)
        write_outputs(candidate)
        expected = {path.name: path.read_bytes() for path in candidate.iterdir() if path.is_file()}
        actual = {path.name: path.read_bytes() for path in OUTPUT.iterdir() if path.is_file()} if OUTPUT.is_dir() else {}
        if expected != actual:
            raise SystemExit("Full V2 blind-review artifacts are stale; regenerate them")
    print("M8.5 FULL V2 REVIEW: PASS boards=6 assets=66 cameraWidths=24,44,72 state=HOLD")


if __name__ == "__main__":
    main()
