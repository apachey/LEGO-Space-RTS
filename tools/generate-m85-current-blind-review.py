#!/usr/bin/env python3
"""Build the renewed T082 blind-review boards from the current 66 selections."""

from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import random
import tempfile

from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont, ImageOps


ROOT = Path(__file__).resolve().parents[1]
ROSTER = ROOT / "Content/Presentation/SuperScout/roster_identity_baseline.json"
SELECTION = (
    ROOT
    / "Docs/Development/M85SuperScout/Silhouettes/FullV2/CurrentComparisonV1/selection_manifest.json"
)
OUTPUT = ROOT / "Docs/Development/M85SuperScout/Silhouettes/FullV2/CurrentBlindReviewV1"
CAMERA_WIDTHS = (24, 44, 72)
FOOTPRINT_PIXELS_AT_24 = {
    "Tiny": 100,
    "Small": 150,
    "Medium": 205,
    "Large": 285,
    "Huge": 390,
}
FONT_PATH = ROOT / "GodotClient/Assets/M7/Fonts/IBMPlexSans-Medium.ttf"
BOARD_SEED = 85084
CODE_PREFIX = "R"
CROP_CACHE: dict[str, Image.Image] = {}


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def load_font(size: int) -> ImageFont.FreeTypeFont | ImageFont.ImageFont:
    if not FONT_PATH.is_file():
        raise ValueError(f"tracked review font is missing: {FONT_PATH}")
    return ImageFont.truetype(str(FONT_PATH), size)


def expanded(bounds: tuple[int, int, int, int], size: tuple[int, int]) -> tuple[int, int, int, int]:
    left, top, right, bottom = bounds
    width, height = size
    padding = max(4, round(max(right - left, bottom - top) * 0.035))
    return (
        max(0, left - padding),
        max(0, top - padding),
        min(width, right + padding),
        min(height, bottom + padding),
    )


def solve_linear_system(matrix: list[list[float]], vector: list[float]) -> list[float]:
    size = len(vector)
    augmented = [matrix[row][:] + [vector[row]] for row in range(size)]
    for column in range(size):
        pivot = max(range(column, size), key=lambda row: abs(augmented[row][column]))
        if abs(augmented[pivot][column]) < 1e-9:
            raise ValueError("background fit is singular")
        augmented[column], augmented[pivot] = augmented[pivot], augmented[column]
        divisor = augmented[column][column]
        augmented[column] = [value / divisor for value in augmented[column]]
        for row in range(size):
            if row == column:
                continue
            factor = augmented[row][column]
            augmented[row] = [
                augmented[row][index] - factor * augmented[column][index]
                for index in range(size + 1)
            ]
    return [augmented[row][-1] for row in range(size)]


def fitted_studio_background(rgb: Image.Image, border: int) -> Image.Image:
    """Estimate a neutral quadratic studio backdrop from border pixels."""

    width, height = rgb.size
    grayscale = ImageOps.grayscale(rgb)
    step = max(3, round(min(width, height) / 90))
    strip = max(border * 2, round(min(width, height) * 0.055))
    normal = [[0.0] * 6 for _ in range(6)]
    target = [0.0] * 6
    samples = 0
    for y in range(0, height, step):
        ny = 2.0 * y / max(1, height - 1) - 1.0
        for x in range(0, width, step):
            if x >= strip and x < width - strip and y >= strip and y < height - strip:
                continue
            nx = 2.0 * x / max(1, width - 1) - 1.0
            features = (1.0, nx, ny, nx * nx, ny * ny, nx * ny)
            value = float(grayscale.getpixel((x, y)))
            for row in range(6):
                target[row] += features[row] * value
                for column in range(6):
                    normal[row][column] += features[row] * features[column]
            samples += 1
    if samples < 12:
        raise ValueError("insufficient border samples for background fit")
    coefficients = solve_linear_system(normal, target)

    preview_width = min(320, width)
    preview_height = max(1, round(height * preview_width / width))
    pixels = []
    for py in range(preview_height):
        ny = 2.0 * py / max(1, preview_height - 1) - 1.0
        for px in range(preview_width):
            nx = 2.0 * px / max(1, preview_width - 1) - 1.0
            features = (1.0, nx, ny, nx * nx, ny * ny, nx * ny)
            value = round(sum(coefficient * feature for coefficient, feature in zip(coefficients, features)))
            pixels.append(max(0, min(255, value)))
    background = Image.new("L", (preview_width, preview_height))
    background.putdata(pixels)
    return background.resize((width, height), Image.Resampling.BICUBIC)


def content_crop(image: Image.Image) -> Image.Image:
    """Isolate the subject, remove source backdrops and return neutral grayscale."""

    rgb = ImageOps.exif_transpose(image).convert("RGB")
    width, height = rgb.size
    border = max(2, round(min(width, height) * 0.02))
    grayscale = ImageOps.grayscale(rgb)

    border_samples = []
    border_samples.extend(grayscale.crop((0, 0, width, border)).getdata())
    border_samples.extend(grayscale.crop((0, height - border, width, height)).getdata())
    border_samples.extend(grayscale.crop((0, border, border, height - border)).getdata())
    border_samples.extend(grayscale.crop((width - border, border, width, height - border)).getdata())
    mostly_white = sum(value >= 238 for value in border_samples) >= len(border_samples) * 0.72

    if mostly_white:
        difference = ImageChops.difference(rgb, Image.new("RGB", rgb.size, "white")).convert("L")
        mask = difference.point(lambda value: 255 if value > 11 else 0)
    else:
        # Generated concept images sometimes have a smooth grey studio gradient.
        # Fit that neutral backdrop from the border, then preserve only pixels
        # whose luminance or colour meaningfully departs from it.
        background = fitted_studio_background(rgb, border)
        background_rgb = Image.merge("RGB", (background, background, background))
        differences = ImageChops.difference(rgb, background_rgb).split()
        difference = ImageChops.lighter(ImageChops.lighter(differences[0], differences[1]), differences[2])
        mask = difference.point(lambda value: 255 if value > 23 else 0)
        draw = ImageDraw.Draw(mask)
        draw.rectangle((0, 0, width - 1, border), fill=0)
        draw.rectangle((0, height - border - 1, width - 1, height - 1), fill=0)
        draw.rectangle((0, 0, border, height - 1), fill=0)
        draw.rectangle((width - border - 1, 0, width - 1, height - 1), fill=0)
        mask = (
            mask.filter(ImageFilter.MedianFilter(5))
            .filter(ImageFilter.MaxFilter(7))
            .filter(ImageFilter.MinFilter(5))
            .filter(ImageFilter.GaussianBlur(1.2))
        )

    bounds = mask.getbbox()
    if bounds is None:
        raise ValueError("selected review image is blank")
    bounds = expanded(bounds, rgb.size)
    neutral = Image.composite(
        grayscale.convert("RGB"),
        Image.new("RGB", rgb.size, "white"),
        mask,
    )
    return neutral.crop(bounds)


def coded_assets() -> list[tuple[str, dict, dict]]:
    roster = json.loads(ROSTER.read_text(encoding="utf-8"))
    selection = json.loads(SELECTION.read_text(encoding="utf-8"))
    assets = roster.get("assets", [])
    selected = selection.get("selections", [])
    selected_by_id = {record["stableId"]: record for record in selected}
    if (
        len(assets) != 66
        or len(selected) != 66
        or len(selected_by_id) != 66
        or set(selected_by_id) != {asset["stableId"] for asset in assets}
    ):
        raise ValueError("renewed blind review requires the exact current 66-asset selection")

    for record in selected:
        image = ROOT / record["file"]
        if not image.is_file() or sha256(image) != record["sha256"]:
            raise ValueError(f"selected image changed: {record['file']}")

    ordered = list(assets)
    random.Random(BOARD_SEED).shuffle(ordered)
    return [
        (f"{CODE_PREFIX}{index:02d}", asset, selected_by_id[asset["stableId"]])
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
    margin_x, header_height, footer = 40, 140, 35
    width = margin_x * 2 + columns * card_width
    height = header_height + rows * card_height + footer
    board = Image.new("RGB", (width, height), "white")
    draw = ImageDraw.Draw(board)
    title_font = load_font(44)
    subtitle_font = load_font(24)
    code_font = load_font(32)
    draw.text(
        (40, 24),
        f"T082 — оновлений сліпий огляд — {camera_width} клітинок — сторінка {page}/2",
        fill="#16181b",
        font=title_font,
    )
    draw.text(
        (40, 83),
        "R01–R66: той самий порядок у масштабах 24/44/72. Без назв, фракцій, кольорів і ролей.",
        fill="#5b6068",
        font=subtitle_font,
    )

    for index, (code, asset, record) in enumerate(page_assets):
        col, row = index % columns, index // columns
        x = margin_x + col * card_width
        y = header_height + row * card_height
        draw.rounded_rectangle(
            (x + 10, y + 10, x + card_width - 10, y + card_height - 10),
            radius=12,
            outline="#d7d9de",
            width=3,
        )
        image_path = ROOT / record["file"]
        cache_key = image_path.as_posix()
        if cache_key not in CROP_CACHE:
            with Image.open(image_path) as source:
                CROP_CACHE[cache_key] = content_crop(source)
        # Colour is deliberately removed by content_crop so accepted coloured
        # studies do not reveal faction or source family.
        cropped = CROP_CACHE[cache_key]
        target = max(18, round(FOOTPRINT_PIXELS_AT_24[asset["footprint"]] * 24 / camera_width))
        scale = min(target / cropped.width, target / cropped.height)
        size = (max(1, round(cropped.width * scale)), max(1, round(cropped.height * scale)))
        thumbnail = cropped.resize(size, Image.Resampling.LANCZOS)
        px = x + (card_width - thumbnail.width) // 2
        py = y + 34 + (card_height - 105 - thumbnail.height) // 2
        board.paste(thumbnail, (px, py))
        code_bounds = draw.textbbox((0, 0), code, font=code_font)
        code_width = code_bounds[2] - code_bounds[0]
        draw.text(
            (x + (card_width - code_width) // 2, y + card_height - 64),
            code,
            fill="#292c31",
            font=code_font,
        )

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
        "# M8.5 T082 — renewed current blind-review answer key",
        "",
        "WITHHELD: open only after the game director records the blind identifications.",
        "",
        "| Code | Stable ID | Asset | Faction | Kind | Footprint | Selected file |",
        "|---|---|---|---|---|---|---|",
    ]
    for code, asset, record in coded:
        key_lines.append(
            f"| `{code}` | `{asset['stableId']}` | {asset['displayName']} | {asset['faction']} | "
            f"{asset['kind']} | {asset['footprint']} | `{record['file']}` |"
        )
    key_name = "WITHHELD_ANSWER_KEY.md"
    (destination / key_name).write_text("\n".join(key_lines) + "\n", encoding="utf-8")

    board_names = [
        f"blind_{camera}_cells_page_{page}.png"
        for camera in CAMERA_WIDTHS
        for page in (1, 2)
    ]
    records = [
        {"file": name, "sha256": sha256(destination / name)}
        for name in sorted([key_name] + board_names)
    ]
    review_manifest = {
        "schemaVersion": 1,
        "task": "T082",
        "corpus": "CURRENT_SELECTED_FULL_ROSTER_RENEWED_V1",
        "gateClassification": "BLOCKING_NOW",
        "state": "HOLD_FOR_GAME_DIRECTOR_24_CELL_REVIEW",
        "productionAccepted": False,
        "canonImpact": "NONE",
        "boardSeed": BOARD_SEED,
        "codePrefix": CODE_PREFIX,
        "cameraWidthsCells": list(CAMERA_WIDTHS),
        "nextReviewWidthCells": 24,
        "pagesPerWidth": 2,
        "assets": 66,
        "selectedViews": 67,
        "reviewedPrimaryViews": 66,
        "alternateViewsExcluded": 1,
        "grayscale": True,
        "sourceSelectionManifestSha256": sha256(SELECTION),
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
        print(f"Wrote renewed current blind review to {OUTPUT}")
        return

    with tempfile.TemporaryDirectory(prefix="m85-current-blind-review-") as temp_dir:
        candidate = Path(temp_dir)
        write_outputs(candidate)
        expected = {path.name: path.read_bytes() for path in candidate.iterdir() if path.is_file()}
        actual = (
            {path.name: path.read_bytes() for path in OUTPUT.iterdir() if path.is_file()}
            if OUTPUT.is_dir()
            else {}
        )
        if expected != actual:
            raise SystemExit("renewed current blind-review artifacts are stale; regenerate them")
    print("M8.5 CURRENT BLIND REVIEW: PASS boards=6 assets=66 widths=24,44,72 state=HOLD")


if __name__ == "__main__":
    main()
