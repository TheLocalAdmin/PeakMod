# PeakMod V0.2.0 (Beta)

A feature-rich quality-of-life and utility mod for **PEAK** built on BepInEx + DearImGuiInjection.

> **Disclaimer:** This mod is provided **as-is** for fun and educational/personal use. It targets a specific build of PEAK and **will not always be updated** when features break or the game changes. Don't expect ongoing maintenance — contributions are welcome though!

## Quick Install (Recommended)

### Via Thunderstore Mod Manager
1. Install [Thunderstore Mod Manager](https://get.thunderstore.io/) or [r2modman](https://thunderstore.io/c/peak/p/ebkr/r2modman/)
2. Search for **PeakMod** in the PEAK community
3. Click Download — BepInEx and DearImGuiInjection are installed automatically

### Via GitHub Release (One Download)
1. Download `PeakMod-Release.zip` from [Releases](https://github.com/TheLocalAdmin/PeakMod/releases)
2. Extract the zip into your **PEAK game folder** (where `PEAK.exe` is located)
3. Launch the game once to let BepInEx generate config files
4. Press **Z** to open the mod menu

### Manual Install
1. Install [BepInEx 5.4.23.3](https://github.com/BepInEx/BepInEx/releases) (x64) into your PEAK game folder
2. Run the game once to generate the BepInEx folder structure
3. Download [DearImGuiInjection](https://thunderstore.io/c/peak/p/penswer/DearImGuiInjection/) and copy `DearImGuiInjection.dll` into `BepInEx/plugins/`
4. Copy `PeakMod.dll` into `BepInEx/plugins/`
5. Launch the game and press **Z**

## Features

- **Player Mods** — Infinite stamina, freeze afflictions, no-weight, no-fog (local only), unlimited item uses, no-status toggles (eat/injury/cold/poison/curse/hot/spores/petrify/ragdoll — all local only), speed/jump/climb/vine/rope/fly modifiers, no fall damage, teleport to ping, custom teleportation.
- **Inventory** — Real-time slot editing (assign any item), recharge item charges, searchable item list.
- **Spawn** — Spawn any item into any player's hand (works as non-host via client-legal RPCs).
- **Lobby** — Player list, revive/kill, warp to/warp to me, spawn Scoutmaster (host only). Safe respawn with ground detection (no more airport fly-ups!).
- **World** — Find and interact with nearby containers/luggage, open all nearby, warp to luggage, luggage ESP with configurable glowing boxes.
- **Stages** — Teleport to any mountain stage (Beach to Peak).
- **Achievements** — Unlock all badges and grant ascent levels.
- **Host Only** — Kick, give any status, remove/fill inventory slots, pass out, zombify, backpack control. Only works when you are the session host (MasterClient).
- **Team Tab** — Apply self mods (infinite stamina, freeze, speed, jump, climb, fly) to teammates, give status effects, clear all statuses, warp to/from teammate.
- **Vanish Mode** — Press V to go invisible, enable fly, and show coordinate overlay.
- **Coordinate Overlay** — Press M to show your position, all players with distance, and nearby containers.
- **Profile** — Save/load all PLAYER tab options to `BepInEx/config/PeakModPlayerProfile.json`.

## Controls

| Key | Action |
|-----|--------|
| **Z** | Open/close mod menu |
| **M** | Toggle coordinate overlay |
| **V** | Toggle vanish mode (invisible + fly + coords) |

### Changing the Menu Key
1. Close the game
2. Open `BepInEx/config/iDeathHD.DearImGuiInjection.cfg`
3. Under `[Keybinds]`, change `CursorVisibility = Z` to your preferred key
4. Save and launch

## What's New in V0.2.0 (Beta)

- **Fixed:** No Ragdoll now only affects your character (was affecting all players)
- **Fixed:** No Fog now only affects your view (was global for all players)
- **Fixed:** Respawn at Airport no longer sends players thousands of blocks up
- **New:** Coordinate overlay (press M) shows all player and luggage positions
- **New:** Vanish mode (press V) — invisible, fly, and coords
- **New:** Team tab — apply self mods, give status effects, warp teammates
- **New:** Luggage ESP with configurable glowing boxes (World tab)
- **New:** Badge management — unlock all badges and ascent levels
- **Removed:** Time freeze/override (was unstable)
- **Improved:** Safe respawn with ground raycast detection
- **Improved:** Simplified installation — single zip download or Thunderstore

## Building from Source

Build with .NET Framework 4.7.x referencing the game's assemblies. The project uses HarmonyX for runtime patching and DearImGuiInjection for UI integration.

```
cd PeakMod
dotnet msbuild PeakMod.csproj -t:Build -p:Configuration=Release
```

Output: `PeakMod/bin/Release/PeakMod.dll`

## Repository Layout

```
PeakMod/
├── BepInEx/plugins/PeakMod.dll    ← prebuilt plugin, ready to install
├── PeakMod/                       ← full C# source (open source)
├── thunderstore/                  ← Thunderstore package files
├── README.md
└── ...
```

## Credits

- [Penswer](https://github.com/Penswer/DearImGuiInjection) — for insight, guidance, and DearImGuiInjection
- [BepInEx](https://github.com/BepInEx/BepInEx) — for the modding framework
- [DearImGuiInjection](https://thunderstore.io/c/peak/p/penswer/DearImGuiInjection/) — for seamless UI integration
- [HarmonyX](https://github.com/pardeike/Harmony) — for runtime patching support

## License / Legal

Not affiliated with or endorsed by the developers of PEAK. Use responsibly at your own risk.
