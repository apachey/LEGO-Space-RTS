#!/usr/bin/env python3
"""Render a static first-screen PNG companion for the ETX Alien Strike review."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[4]
OUT = Path(__file__).with_name("etx_alien_strike_director_review_20260926_v1.png")
TARGET = ROOT / "ArtSource/M85/T083/ETXAlienStrikeV1/etx_alien_strike_production_target_20260926_v1.png"
W, H = 1320, 900
INK, MUTED, PAPER = "#202420", "#5d665d", "#eef0eb"
PANEL, LINE, DEEP, LIME = "#ffffff", "#cfd5cb", "#222722", "#a8c92a"
FONT_FILE = "/System/Library/Fonts/HelveticaNeue.ttc"

def font(size, bold=False):
    index = 1 if bold else 0
    return ImageFont.truetype(FONT_FILE, size, index=index)

def text(draw, xy, value, size, fill=INK, bold=False):
    draw.text(xy, value, font=font(size, bold), fill=fill)

def main():
    im = Image.new("RGB", (W, H), PAPER)
    d = ImageDraw.Draw(im)
    # Header and state mirror the HTML decision screen.
    text(d, (32, 17), "ВІЗУАЛЬНИЙ ДИЗАЙН · T083", 11, "#536c1c", True)
    text(d, (32, 33), "ETX Alien Strike", 36, INK, True)
    text(d, (33, 75), "Прибульці · повітряний штурмовик споруд · середній розмір", 14, MUTED)
    d.rounded_rectangle((1002, 32, 1144, 61), radius=2, fill=PANEL, outline=LINE)
    text(d, (1014, 40), "ПРИБУЛЬЦІ", 10, INK, True)
    d.rounded_rectangle((1152, 32, 1288, 61), radius=2, fill="#e6edd0", outline="#9cb04e")
    text(d, (1164, 40), "SOURCE_LOCKED", 10, "#405414", True)
    d.line((32, 100, 1288, 100), fill=LINE, width=1)
    d.rectangle((32, 111, 1288, 143), fill=DEEP)
    d.rectangle((32, 111, 37, 143), fill=LIME)
    text(d, (49, 120), "ДИРЕКТОР ПРИЙНЯВ · 26.09.2026", 11, PANEL, True)
    text(d, (1065, 120), "T084 — НЕ РОЗПОЧАТО", 11, "#dfe9c0", True)
    # Dominant target panel.
    x0, y0, x1, y1 = 32, 154, 1288, 704
    d.rectangle((x0, y0, x1, y1), fill=PANEL, outline=LINE, width=1)
    text(d, (48, 165), "Production Design Target", 16, INK, True)
    text(d, (775, 168), "Офіційний завершений LEGO 7693 · ігрових змін не запропоновано", 10, MUTED, True)
    d.line((33, 191, 1287, 191), fill=LINE, width=1)
    d.rectangle((33, 192, 1287, 671), fill="#e4e6e0")
    d.rectangle((33, 192, 825, 671), fill="#d9dcd4")
    source = Image.open(TARGET).convert("RGB")
    source.thumbnail((760, 460), Image.Resampling.LANCZOS)
    im.paste(source, (49 + (760-source.width)//2, 201 + (460-source.height)//2))
    d.line((825, 192, 825, 671), fill=LINE, width=1)
    d.rectangle((850, 224, 954, 246), fill=DEEP)
    text(d, (859, 230), "ЦІЛЬ ДЛЯ T084", 9, PANEL, True)
    text(d, (850, 263), "Зберегти завершений", 18, INK, True)
    text(d, (850, 286), "вигляд LEGO 7693", 18, INK, True)
    text(d, (850, 325), "Це кадр офіційної обкладинки", 12, MUTED)
    text(d, (850, 343), "набору. Ровер поруч — інша модель", 12, MUTED)
    text(d, (850, 361), "із коробки, він не входить до юніта.", 12, MUTED)
    d.line((850, 398, 1268, 398), fill=LINE, width=1)
    for i, line in enumerate(("Вигнутий повітряний силует", "Центральна кабіна й випромінювач", "Парні серпоподібні секції", "Довгі лаймові хвостові лопаті")):
        text(d, (850, 413+i*24), line, 11, INK, i == 0)
    text(d, (48, 680), "Коробкова ілюстрація показує зібране судно, але не визначає точну висоту польоту чи кути шарнірів.", 10, MUTED)
    # Adaptation and approval decision.
    d.rectangle((32, 718, 536, 792), fill=PANEL, outline=LINE)
    d.rectangle((548, 718, 1288, 792), fill="#fbfcf7", outline=LINE)
    d.rectangle((548, 718, 552, 792), fill=LIME)
    text(d, (46, 730), "Видимі адаптації", 12, INK, True)
    text(d, (46, 752), "Видимих адаптацій немає.", 12, INK)
    text(d, (565, 730), "Прийняте рішення", 12, INK, True)
    text(d, (565, 751), "Прийнято саме SOURCE_LOCKED вигляд LEGO 7693:", 12, INK, True)
    text(d, (565, 769), "силует, кабіну з випромінювачем, бокові секції й лаймові лопаті.", 11, INK)
    # Compact support links. Technical material remains below the first screen.
    d.rectangle((32, 805, 1288, 845), fill="#e1e4dd", outline=LINE)
    text(d, (44, 819), "ДЖЕРЕЛА", 9, MUTED, True)
    text(d, (105, 817), "LEGO 7693 та інструкції   ·   офіційний PDF   ·   фото зібраного набору   ·   інвентар", 10, "#405719", True)
    im.save(OUT, "PNG", optimize=True)
    print(f"PASS: wrote {OUT} ({W}x{H})")

if __name__ == "__main__":
    main()
