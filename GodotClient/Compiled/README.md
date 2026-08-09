# Generated runtime data

Canonical regeneration command:

```bash
dotnet run --project tools/ContentCompiler -- Content/PrototypeEntities.json Content/Maps/DEV_FirstControllableRTS.map.json GodotClient/Compiled
```

`*.contentbin` and `*.mapbin` are deterministic build products and are never hand-edited. Source JSON remains authoritative.

The binaries included in this artifact were pre-generated from the documented binary format in an environment without a C# runtime so the native Godot loader has concrete baseline files. They are **not certified compiler output** until the command above is run with the C# `ContentCompiler` and its round-trip validation succeeds. Phase 10 acceptance explicitly requires that regeneration.
