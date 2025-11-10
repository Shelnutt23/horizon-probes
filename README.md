# Horizon Probes — Unity 6.2

A prototype for a vector/HD space strategy (dispatch → sim → resolve), built for **Unity 6.2 (6000.2.6f2)**.

## Project layout
- `Assets/Code` — Core/Sim/UI/Editor scripts
- `Assets/Resources/Content` — JSON content (biomes, missions, tech)
- `Assets/Art/*` — Backgrounds, UI, Fonts
- `Assets/Scenes/Main.unity` — Auto-setup scene (via HorizonProbes → Auto-Setup)

## Local setup
1. Unity: **Edit → Project Settings → Editor**
   - Version Control: **Visible Meta Files**
   - Asset Serialization: **Force Text**
2. Install **Git LFS** and run:
   ```bash
   git lfs install
   git lfs track "*.psd" "*.tga" "*.tif" "*.tiff" "*.wav" "*.aif" "*.aiff" "*.mp3" "*.mp4" "*.mov" "*.avi" "*.fbx" "*.blend"
   ```
3. Optional (recommended): Unity SmartMerge (UnityYAMLMerge)
   ```bash
   # macOS
   git config merge.unityyamlmerge.name "Unity SmartMerge (UnityYAMLMerge)"
   git config merge.unityyamlmerge.driver "/Applications/Unity/Hub/Editor/6000.2.6f2/Unity.app/Contents/Tools/UnityYAMLMerge" merge -p %O %A %B %P
   # Windows
   # git config merge.unityyamlmerge.driver "C:/Program Files/Unity/Hub/Editor/6000.2.6f2/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p %O %A %B %P
   ```

## First commit
```bash
git init
git add .
git commit -m "Horizon Probes: initial Unity 6.2 setup"
git remote add origin https://github.com/<you>/<horizon-probes>.git
git branch -M main
git push -u origin main
```

## Contributing guidelines
- One person edits a scene at a time (or split into sub-prefabs).
- Feature branches: `feat/x`, `fix/x`, `chore/x`.
- Keep JSON content small and modular for easier diffs.

Generated: 2025-11-10T08:39:55.985158Z
