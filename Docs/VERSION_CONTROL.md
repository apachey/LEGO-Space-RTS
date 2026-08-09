# Version control baseline — Godot

## Commit

Commit authored text/source:

- C#;
- `.csproj` / `.sln`;
- `project.godot`;
- `.tscn` / `.tres` / `.res` only when intentionally authored/source-controlled;
- JSON source content/maps;
- deterministic compiled M2 runtime data when the release baseline requires an immediately runnable package;
- documentation/tests/tooling.

## Do not commit

- `.godot/` imported/editor cache;
- `.mono/`;
- `bin/`, `obj/`;
- IDE caches;
- logs;
- local benchmark captures;
- normal build/export outputs;
- user-local editor configuration.

Godot source scene/project files are text-first and should receive normal review/diff treatment.

## Deterministic generated-content policy

Human-editable JSON is authoritative. `*.contentbin`/`*.mapbin` are compiler products. For this M2 implementation package they are included so the Godot project has a runnable compiled-data baseline; any source JSON modification requires regeneration and review of content hash/validation output.

Do not hand-edit compiled binaries.

## Git LFS

Use Git LFS later for genuinely large binary source assets such as high-resolution layered art, audio masters, large DCC files, baked cinematics or other multi-megabyte binary authoring sources.

Do not place normal C#, JSON, Godot scene text, small textures/icons or these small deterministic simulation binaries under LFS by default.
