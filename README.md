<p align="center">
  <img src="icon.png" alt="PeakMod Logo" width="256" height="256" />
</p>

<h1 align="center">PeakMod V0.3.1</h1>

<p align="center">
  A feature-rich quality-of-life and utility mod for <b>PEAK</b> built on BepInEx + DearImGuiInjection.
</p>

> **Disclaimer:** This mod is provided **as-is** for fun and educational/personal use. It targets a specific build of PEAK and **will not always be updated** when features break or the game changes. Don't expect ongoing maintenance — contributions are welcome though!

## Install

### Thunderstore (Recommended)
1. Install [Thunderstore Mod Manager](https://get.thunderstore.io/) or [r2modman](https://thunderstore.io/c/peak/p/ebkr/r2modman/)
2. Search for **PeakMod** in the PEAK community
3. Click Download — BepInEx and DearImGuiInjection are installed automatically

### GitHub (Manual)
1. Install [BepInEx](https://github.com/BepInEx/BepInEx/releases) (x64) into your PEAK game folder
2. Run the game once to generate the BepInEx folder structure
3. Download [DearImGuiInjection](https://thunderstore.io/c/peak/p/penswer/DearImGuiInjection/) and copy `DearImGuiInjection.dll` into `BepInEx/plugins/`
4. Download `PeakMod.dll` from [Releases](https://github.com/TheLocalAdmin/PeakMod/releases/tag/v0.2.0) and copy it into `BepInEx/plugins/`
5. Launch the game and press **Fn + Insert** to open the menu

## Features

- **Player Mods** — Infinite stamina, freeze afflictions, no-weight, unlimited item uses, no-status toggles (eat/injury/cold/poison/curse/hot/spores/petrify/ragdoll — all local only), speed/jump/climb/vine/rope/fly modifiers, no fall damage, teleport to ping.
- **Inventory** — Real-time slot editing (assign any item), recharge item charges, searchable item list.
- **Spawn** — Spawn any item into any player's hand (works as non-host via client-legal RPCs).
- **Lobby** — Player list, revive/kill, warp to/warp to me, teleport players to custom coordinates, spawn Scoutmaster (host only). Safe respawn with ground detection (no more airport fly-ups!).
- **World** — Find and interact with nearby containers/luggage, open all nearby, warp to luggage, luggage ESP with configurable glowing boxes.
- **Stages** — Teleport to any mountain stage (Beach to Peak).
- **Achievements** — Unlock all badges and grant ascent levels.
- **Host Only** — Kick, give any status, remove/fill inventory slots, pass out, zombify, backpack control. Only works when you are the session host (MasterClient).
- **Coordinate Overlay** — Toggle via checkbox or custom keybind to show your position, all players with distance, and nearby containers.
- **Custom Keybinds** — Assign keys for fly mode and coordinate overlay that save to your profile.
- **Profile** — Save/load all PLAYER tab options including custom keybinds to `BepInEx/config/PeakModPlayerProfile.json`.

## Controls

| Key | Action |
|-----|--------|
| **Fn + Insert** | Open/close mod menu |
| **Custom** | Fly mode toggle, coordinate overlay (set in PLAYER tab) |

> **Note:** You must close the menu (press Fn + Insert again) before you can move your character.

### Changing the Menu Key
1. Close the game
2. Open `BepInEx/config/iDeathHD.DearImGuiInjection.cfg`
3. Under `[Keybinds]`, change `CursorVisibility = Insert` to your preferred key
4. Save and launch

## What's New in V0.3.1

- **Removed:** Vanish mode, No Fog, Team tab — streamlined to focus on core features
- **New:** Coordinate overlay is now a checkbox in Self Mods (no fixed keybind)
- **New:** Custom keybinds for fly mode and coordinate overlay (saves to profile)
- **Moved:** Teleport-to-coordinates moved from Player to Lobby tab — teleport selected players to specific coordinates
- **Updated:** About tab with current feature list

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
├── icon.png
├── README.md
└── ...
```

## Credits

- [Penswer](https://github.com/Penswer/DearImGuiInjection) ([Thunderstore](https://thunderstore.io/c/peak/p/penswer/DearImGuiInjection/)) — for insight, guidance, and DearImGuiInjection
- [BepInEx](https://github.com/BepInEx/BepInEx) ([Thunderstore](https://thunderstore.io/c/peak/p/BepInEx/BepInExPack_PEAK/)) — for the modding framework
- [DearImGuiInjection](https://thunderstore.io/c/peak/p/penswer/DearImGuiInjection/) — for seamless UI integration
- [HarmonyX](https://github.com/pardeike/Harmony) — for runtime patching support

## License / Legal

Not affiliated with or endorsed by the developers of PEAK. Use responsibly at your own risk.
