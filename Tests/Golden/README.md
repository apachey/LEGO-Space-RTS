# Deterministic golden artifacts

The Phase 10 golden scenario uses checkpoints at ticks 250, 500, 1000, 1500, 2000 and 3000.

No expected hash file is fabricated in the artifact-generation environment because no .NET runtime is available there. On the first trusted build, generate `phase10-v0.1.hashes` with `HeadlessSim --golden-manifest-out`, commit it, and use `--golden-manifest-in` for later regression runs. A deliberate authoritative simulation change requires an explicit golden update and review.
