# LEGO SPACE RTS — CODEX COMPLEXITY GUARDRAILS

**Status:** DEVELOPMENT POLICY  
**Authority:** Phase 09B — Movement Architecture & Prototype-Gate Amendment

This file translates Phase 09B's complexity policy into practical implementation-agent behavior.

It does not replace `AGENTS.md` or `Docs/Canon/`.

## Mandatory rules for implementation agents

### Two materially different attempts maximum
If two materially different implementation approaches fail the same blocker, stop coding and request architecture review.

### New subsystem = stop
If a normal task unexpectedly requires a new persistent coordinator, scheduler, planner, graph layer, manager, solver, traffic-control layer or equivalent subsystem, stop before implementing it and request explicit architecture approval.

### Research before custom algorithm
Before building a custom solution to a standard game-development problem, inspect mature established approaches and primary documentation/source repositories, compare them to project constraints, and explain why custom work is necessary.

### Scope growth = new task
A bounded task may not silently grow into an engine subsystem. If task scope changes category, stop and request a new approved task.

### Gate classification
Every meaningful benchmark/stress gate must be labeled `BLOCKING_NOW`, `BLOCKING_LATER`, or `DIAGNOSTIC`.

### Prototype value
Prototype milestones prioritize functionality the player can actually encounter and evaluate now. Production-scale edge cases do not automatically block the nearest playable milestone.

### Benchmark does not dictate architecture
If passing one benchmark would require a major architectural expansion, stop and review both the benchmark's milestone role and the architecture before coding.

### Approval-spam reduction
Repeated safe repository-local commands such as build, test, verify, headless Godot, and project-owned scripts should use the project's approved permission mechanism where available. Dangerous, destructive, system-wide, dependency-install or history-rewriting actions still require explicit approval.

### Architecture-stop reporting
When stopping because of these rules, explicitly report `ARCHITECTURE REVIEW REQUIRED` and do not continue coding in that direction.
