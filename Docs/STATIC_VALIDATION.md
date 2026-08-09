# Static validation

The repository includes:

```bash
python Tools/Validation/validate_phase10.py
```

It checks source-level invariants that do not require Godot or the .NET runtime, including:

- native Godot project/scene presence;
- Godot 4.7.1 .NET SDK pin;
- Forward+ project feature;
- no legacy Unity `Game/` host;
- portable SimCore engine boundary;
- no authoritative `float`/`double` token under SimCore runtime;
- 20 Hz tick constant;
- 160×160 / 320×320 map constants;
- HPA cluster size 10;
- 12-tick reservation horizon;
- 16-order queue capacity;
- 128 selection foundation;
- Godot InputMap and M2 command bindings;
- compiled-data loader presence;
- map/content source integrity;
- canonical M2 Rock Raider footprint/speed baselines;
- engineering-only Huge profile;
- HeadlessSim command-line surface;
- lightweight delimiter sanity across C# files.

Static validation is necessary but not sufficient. It cannot prove C# compilation, Godot API compatibility, deterministic runtime equality or performance.
