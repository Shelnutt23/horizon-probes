# Horizon Probes — Prototype Skeleton (Unity 6.2)

This folder contains a Unity‑ready starter project implementing Phase 0 Week 1–2 backbone:
- Core loop scaffolding (dispatch → sim → resolve)
- Deterministic RNG + tick simulation at 1s cadence
- Mission service with offline simulation (8h cap)
- Economy service and simple resource bundle
- JSON content slice (biomes, chassis, modules, missions, tech)
- Deterministic sector generator stub (24 nodes, one biome: Asteroid Belt)
- Minimal Service Locator + SaveSystem stubs
- Editor‑agnostic (no 3rd‑party deps)

> Recommended Unity: **Unity 6.2 (6000.2.6f2)**

## How to use
1. Create a new empty Unity 2D/URP project.
2. Copy this archive’s `Assets/` into your project’s `Assets/`.
3. In a new scene, add an empty `GameObject` and attach `GameBootstrap` and `HUDController`.
4. Press Play and iterate. Hook into your own UI as needed.

Generated on: 2025-11-06T09:49:40.841408Z
