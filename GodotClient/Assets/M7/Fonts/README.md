# M7 HUD fonts

These unmodified static TrueType fonts are vendored for deterministic HUD
rendering. Runtime code should load them through `HudTypographyLibrary` rather
than referring to filenames throughout the UI.

| Role | File | Official upstream | Pinned revision | SHA-256 |
| --- | --- | --- | --- | --- |
| Headings | `Oxanium-SemiBold.ttf` | `sevmeyer/oxanium` | commit `a8f39e0c71186190027a093e9001459410192d1e` | `e2d77ec4ee67b0152166adf5d6393360550a012c2066e0d4589053e14a733cdc` |
| Body | `IBMPlexSans-Regular.ttf` | `IBM/plex` | tag `@ibm/plex-sans@1.1.0`, commit `1da12f02587b630c07e92692d21492d722f53614` | `975dcda37d80f038dcd143c22e33ca2d97a0cc5a929aace1c749153b0fe1afa5` |
| Values / emphasis | `IBMPlexSans-Medium.ttf` | `IBM/plex` | tag `@ibm/plex-sans@1.1.0`, commit `1da12f02587b630c07e92692d21492d722f53614` | `331c8639d7598b2cde62a911a71db195e30cb655cd6bdf2e324a7e984955f907` |

## Exact sources

- Oxanium SemiBold:
  `https://raw.githubusercontent.com/sevmeyer/oxanium/a8f39e0c71186190027a093e9001459410192d1e/fonts/ttf/Oxanium-SemiBold.ttf`
- IBM Plex Sans Regular:
  `https://raw.githubusercontent.com/IBM/plex/1da12f02587b630c07e92692d21492d722f53614/packages/plex-sans/fonts/complete/ttf/IBMPlexSans-Regular.ttf`
- IBM Plex Sans Medium:
  `https://raw.githubusercontent.com/IBM/plex/1da12f02587b630c07e92692d21492d722f53614/packages/plex-sans/fonts/complete/ttf/IBMPlexSans-Medium.ttf`
- Oxanium license:
  `https://raw.githubusercontent.com/sevmeyer/oxanium/a8f39e0c71186190027a093e9001459410192d1e/OFL.txt`
- IBM Plex license:
  `https://raw.githubusercontent.com/IBM/plex/1da12f02587b630c07e92692d21492d722f53614/LICENSE.txt`

The IBM tag is annotated; tag object `036e2d2a727bd6a378f69b7cde8d8dd20c472dae`
resolves to source commit `1da12f02587b630c07e92692d21492d722f53614`.

## Licenses and attribution

- Oxanium is licensed under the SIL Open Font License 1.1. Copyright 2019
  The Oxanium Project Authors. The exact upstream license is preserved in
  `Oxanium-OFL.txt`.
- IBM Plex Sans is licensed under the SIL Open Font License 1.1. Copyright
  2017 IBM Corp., with Reserved Font Name "Plex". The exact upstream license
  is preserved in `IBM-Plex-OFL.txt`.

The font binaries have not been renamed internally, subsetted, converted or
otherwise modified.
