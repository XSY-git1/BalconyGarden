# BalconyGarden

Unity2D casual balcony planting prototype.

## Current Status

`MVP_01` has proved the core planting loop.

`MVP_02` is planned in `MVP_02.md`.

Implemented:

- 5 horizontal balcony pot slots
- Small pot occupying 1 slot
- Long pot occupying 2 slots
- Slot occupancy and selected pot behavior
- Daisy, Tomato, and Mystery seed data
- Daisy, Tomato, and Mystery plant data
- Seed inventory
- Real-world-time plant growth stages
- Automatic in-play growth refresh
- Mature plant harvest
- Local JSON save/load through PlayerPrefs
- Temporary debug UI for testing
- Basic player HUD for pot selection, seed selection, planting, harvesting, and inventory display

Intentionally not included yet:

- Bird system
- Album or specimen collection
- Rare traits
- Shop
- Ads
- WeChat SDK
- Cloud save

## Unity Version

Open with Unity `2022.3.62f1c1`.

## Test Scene

Main scene:

```text
Assets/Scenes/SampleScene.unity
```

Basic MVP check:

1. Enter Play Mode.
2. Use the Garden panel to select a pot type.
3. Click an empty slot to place a pot.
4. Use the Garden panel to select a seed and plant it into a pot.
5. Wait for the plant to mature or use short test growth times.
6. Harvest the mature plant.
7. Save, clear layout, then load to confirm data restores.
8. Stop and restart Play Mode to confirm saved plants keep growing by real time.

## Git Notes

The repository keeps Unity source files, scenes, prefabs, settings, and `.meta` files.

Generated or local-only files are ignored, including:

- `Library/`
- `Temp/`
- `Logs/`
- `UserSettings/`
- `.vs/`
- `.vscode/`
- generated project files such as `*.csproj` and `*.sln`
- common secret/key file patterns
- source-art and build-output patterns
