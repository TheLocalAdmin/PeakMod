# PeakMod V0.3

A feature-rich quality-of-life and utility mod for **PEAK** built on BepInEx + DearImGuiInjection.

## Features

- **Player Mods** - Infinite stamina, freeze afflictions, no-weight, no-fog (local only), unlimited item uses, no-status toggles, speed/jump/climb/vine/rope/fly modifiers, no fall damage, teleport to ping, custom teleportation.
- **Inventory** - Real-time slot editing (assign any item), recharge item charges, searchable item list.
- **Spawn** - Spawn any item into any player's hand (works as non-host).
- **Lobby** - Player list, revive/kill, warp to/warp to me, spawn Scoutmaster (host only). Safe respawn with ground detection.
- **World** - Find and interact with nearby containers/luggage, open all nearby, warp to luggage, luggage ESP with configurable glowing boxes.
- **Stages** - Teleport to any mountain stage (Beach to Peak).
- **Achievements** - Unlock all badges and grant ascent levels.
- **Host Only** - Kick, give any status, remove/fill inventory slots, pass out, zombify, backpack control.
- **Team Tab** - Apply self mods (infinite stamina, freeze, speed, jump, climb, fly) to teammates, give status effects, clear all statuses, warp to/from teammate.
- **Vanish Mode** - Press V to go invisible, enable fly, and show coordinate overlay.
- **Coordinate Overlay** - Press M to show your position, all players with distance, and nearby containers.
- **Profile** - Save/load all PLAYER tab options to a JSON file.

## Installation

### Via Thunderstore Mod Manager (Recommended)
1. Install [Thunderstore Mod Manager](https://get.thunderstore.io/) or [r2modman](https://thunderstore.io/c/peak/p/ebkr/r2modman/)
2. Search for **PeakMod** in the PEAK community
3. Click Download - BepInEx and DearImGuiInjection will be installed automatically

### Manual Install
1. Install [BepInEx](https://github.com/BepInEx/BepInEx/releases) ([Thunderstore](https://thunderstore.io/c/peak/p/BepInEx/BepInExPack_PEAK/)) (x64) into your PEAK game folder
2. Run the game once to generate the BepInEx folder structure
3. Download [DearImGuiInjection](https://thunderstore.io/c/peak/p/penswer/DearImGuiInjection/) and copy `DearImGuiInjection.dll` into `BepInEx/plugins/`
4. Copy `PeakMod.dll` into `BepInEx/plugins/`
5. Launch the game and press **Fn + Insert** to open the menu

## Controls

- **Fn + Insert** - Open/close mod menu
- **M** - Toggle coordinate overlay
- **V** - Toggle vanish mode (invisible + fly + coords)

> **Note:** You must close the menu (press Fn + Insert again) before you can move your character.

### Changing the Menu Key
1. Close the game
2. Open `BepInEx/config/iDeathHD.DearImGuiInjection.cfg`
3. Under `[Keybinds]`, change `CursorVisibility = Insert` to your preferred key
4. Save and launch

## Dependencies

- [BepInExPack PEAK](https://thunderstore.io/c/peak/p/BepInEx/BepInExPack_PEAK/) (installed automatically via Thunderstore)
- [DearImGuiInjection](https://thunderstore.io/c/peak/p/penswer/DearImGuiInjection/) (installed automatically via Thunderstore)

## Links

- **[GitHub Repository](https://github.com/TheLocalAdmin/PeakMod)** — Source code, issues, and releases
- [Bug Reports](https://github.com/TheLocalAdmin/PeakMod/issues)

## Credits

- [Penswer](https://github.com/Penswer/DearImGuiInjection) - for insight, guidance, and DearImGuiInjection
- [BepInEx](https://github.com/BepInEx/BepInEx) - for the modding framework
- [DearImGuiInjection](https://thunderstore.io/c/peak/p/penswer/DearImGuiInjection/) - for seamless UI integration
- [HarmonyX](https://github.com/pardeike/Harmony) - for runtime patching support

## Disclaimer

Not affiliated with or endorsed by the developers of PEAK. Use responsibly.
