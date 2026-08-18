# PeakMod V0.1

A feature-rich quality-of-life and utility mod for **PEAK (v2.2.a)** built on BepInEx. PeakMod adds player enhancements, inventory tools, stage teleportation, badge unlocking, world manipulation, and lobby control in a clean Fullblack ImGui interface.

> **Disclaimer:** This mod is provided **as-is** for fun and educational/personal use. It targets a specific build of PEAK (v2.2.a) and **will not always be updated** when features break or the game changes. Don't expect ongoing maintenance — contributions are welcome though!

## Repository layout

This repo is both the **install** and the **source**:

```
PeakMod-src/
├── BepInEx/plugins/PeakMod.dll   <-- prebuilt plugin, ready to install
├── PeakMod/                      <-- full C# source (open source, build it yourself!)
├── PeakMod.csproj / PeakMod.sln  <-- Visual Studio project
└── README.md
```

Everything here is open source so you can build the DLL yourself and verify exactly what it does before using it — no need to trust a prebuilt binary blindly.

## Requirements

- **PEAK v2.2.a** (Steam)
- [BepInEx 5.4.23.3](https://github.com/BepInEx/BepInEx/releases) (x64)
- [DearImGuiInjection](https://github.com/panda-lang/DearImGuiInjection) (required — the GUI will not render without it)
- .NET Framework 4.x runtime (included with the game)
- Only required for building from source: the game assemblies (`Assembly-CSharp.dll`, `UnityEngine*.dll`) and a .NET Framework 4.7.x compiler

## Installation (prebuilt DLL)

1. Install **BepInEx 5.4.23.3** into your PEAK game folder by extracting it into the game directory and running the game once to generate the folder structure.
2. Drop the **DearImGuiInjection** plugin into `BepInEx\plugins\` (build it for the PEAK/Unity version you run; it must be one of the `*NotRepeated`/standalone variants).
3. Copy `BepInEx\plugins\PeakMod.dll` from this repo into your game's `BepInEx\plugins\` folder.
4. Launch the game. Open the mod menu by pressing `Z` (see Controls / Usage to change it).
5. If present, delete the older config at `BepInEx\config\com.thelocaladmin.peakmod.cfg` when upgrading between versions.

## Features

- **Player** - Infinite stamina, freeze afflictions, no-weight, no-fog, unlimited item uses, no-status toggles (eat/injury/cold/poison/curse/hot/spores/petrify/etc.) grouped in a "Statuses" dropdown, speed/jump/climb/rope/vine/fly modifiers, no fall damage, teleport to ping, custom teleportation.
- **Items** - Real-time inventory slot editing (assign any item), recharge item charges, searchable item list.
- **Lobby** - Player list, revive/kill selected, warp to / warp to me, spawn Scoutmaster for a player (host only).
- **World** - Find and interact with nearby containers/luggage (auto-refreshing), open all nearby containers, warp to luggage.
- **Stages** - Teleport to any mountain stage, from Beach to Peak.
- **Achievements** - Unlock all badges and grant ascent levels.

## Controls / Usage

- **Menu hotkey:** The default key is **`Z`**. To change it:
  1. Close the game.
  2. Open `BepInEx\config\iDeathHD.DearImGuiInjection.cfg`.
  3. Under `[Keybinds]`, change the line `CursorVisibility = Z` to your preferred key (e.g. `CursorVisibility = N`, `CursorVisibility = Insert`). Use any value from the acceptable-keys list in that file.
  4. Save the file and launch the game.
- Host-related actions (spawn Scoutmaster, etc.) only work if you are the host/MasterClient — the game enforces this on the RPC layer.

## Building

Build with the .NET Framework 4.7.x compiler (or newer) referencing the game's assemblies. The project uses HarmonyX for runtime patching and DearImGuiInjection for UI integration. See the included project source under `PeakMod/`.

## Credits

- Penswer – for insight and guidance
- BepInEx team – for the modding framework
- DearImGuiInjection – for seamless UI integration
- HarmonyX – for runtime patching support

## License / Legal

Not affiliated with or endorsed by the developers of PEAK. Use responsibly at your own risk. Singleplayer and casual MP use only.