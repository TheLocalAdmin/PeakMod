# PeakMod V0.3.1

A feature-rich quality-of-life and utility mod for **PEAK v2.3.a** built on BepInEx + DearImGuiInjection.

## Features

- **Player Mods** - Infinite stamina, freeze afflictions, no-weight, unlimited item uses, no-status toggles, speed/jump/climb/vine/rope/fly modifiers, no fall damage, teleport to ping.
- **Inventory** - Real-time slot editing (assign any item), recharge item charges, searchable item list.
- **Spawn** - Spawn any item into any player's hand (works as non-host).
- **Lobby** - Player list, revive/kill, warp to/warp to me, teleport players to custom coordinates, spawn Scoutmaster (host only). Safe respawn with ground detection.
- **World** - Find and interact with nearby containers/luggage, open all nearby, warp to luggage, luggage ESP with configurable glowing boxes.
- **Stages** - Teleport to any mountain stage (Beach to Peak).
- **Achievements** - Unlock all badges and grant ascent levels.
- **Host Only** - Kick, give any status, remove/fill inventory slots, pass out, zombify, backpack control.
- **Coordinate Overlay** - Toggle via checkbox or custom keybind to show your position, all players, and nearby containers.
- **Custom Keybinds** - Assign keys for fly mode and coordinate overlay that save to your profile.
- **Profile** - Save/load all PLAYER tab options including custom keybinds.

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
- **Custom** - Any self mod can be bound to a key (see Keybinds below)

> **Note:** You must close the menu (press Fn + Insert again) before you can move your character.

### Changing the Menu Key
1. Close the game
2. Open `BepInEx/config/iDeathHD.DearImGuiInjection.cfg`
3. Under `[Keybinds]`, change `CursorVisibility = Insert` to your preferred key
4. Save and launch

### Custom Keybinds
Keybinds are set in `BepInEx/config/com.thelocaladmin.peakmod.cfg` under `[Keybinds]`. Set any key name or `None` to disable.

```ini
[Keybinds]
; set a key name (e.g. F5, Alpha1, Keypad0) or None to disable
FlyMod = F5
ShowCoordOverlay = None
; ...etc for all self mods
```

**Key names (case-sensitive):**
- **F keys:** `F1` through `F24`
- **Number row:** `Alpha0` through `Alpha9`
- **Numpad:** `Keypad0` through `Keypad9`, `KeypadPeriod`, `KeypadDivide`, `KeypadMultiply`, `KeypadMinus`, `KeypadPlus`, `KeypadEnter`, `KeypadEquals`
- **Letters:** `A` through `Z` (uppercase)
- **Navigation:** `Insert`, `Delete`, `Home`, `End`, `PageUp`, `PageDown`
- **Arrows:** `UpArrow`, `DownArrow`, `LeftArrow`, `RightArrow`
- **Mouse:** `Mouse0` through `Mouse6`
- **Other:** `Space`, `Return`, `Escape`, `Tab`, `Backspace`, `Comma`, `Period`, `Slash`, `Backslash`, `Minus`, `Equals`, `LeftBracket`, `RightBracket`, `Semicolon`, `Quote`, `BackQuote`
- **Modifiers:** `LeftShift`, `RightShift`, `LeftControl`, `RightControl`, `LeftAlt`, `RightAlt`
- **Disable:** `None`

Full list and guide: [GitHub Keybinds Reference](https://github.com/TheLocalAdmin/PeakMod#custom-keybinds)

## Dependencies

- [BepInExPack PEAK](https://thunderstore.io/c/peak/p/BepInEx/BepInExPack_PEAK/) (installed automatically via Thunderstore)
- [DearImGuiInjection](https://thunderstore.io/c/peak/p/penswer/DearImGuiInjection/) (installed automatically via Thunderstore)

## Links

- **[GitHub Repository](https://github.com/TheLocalAdmin/PeakMod)** — Source code, issues, releases, and full keybind docs
- [Bug Reports](https://github.com/TheLocalAdmin/PeakMod/issues)

## Credits

- [Penswer](https://github.com/Penswer/DearImGuiInjection) - for insight, guidance, and DearImGuiInjection
- [BepInEx](https://github.com/BepInEx/BepInEx) - for the modding framework
- [DearImGuiInjection](https://thunderstore.io/c/peak/p/penswer/DearImGuiInjection/) - for seamless UI integration
- [HarmonyX](https://github.com/pardeike/Harmony) - for runtime patching support

## Disclaimer

Not affiliated with or endorsed by the developers of PEAK. Use responsibly.
