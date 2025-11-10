# Contributing

## Branching
- Use feature branches: `feat/<name>`, `fix/<name>`, `chore/<name>`.
- Small PRs with clear descriptions.

## Scenes
- Prefer one editor at a time per scene.
- If multiple people must touch UI, extract sub-canvases as prefabs.

## Code style
- C# files in `Assets/Code/...`
- Avoid target-typed `new()` in shared code; keep Unity 6 compatibility.

## Reviews
- Include before/after screenshots for UI.
- Note any changes to JSON content schema.
