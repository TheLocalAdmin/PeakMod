using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;

public static class ConfigManager
{
    public static ManualLogSource Logger;

    // Cheats
    public static ConfigEntry<bool> InfiniteStamina;
    public static ConfigEntry<bool> FlyMod;
    public static ConfigEntry<float> FlySpeed;
    public static ConfigEntry<float> FlyAcceleration;

    // Afflictions
    public static ConfigEntry<bool> LockStatus;
    public static ConfigEntry<bool> NoWeight;
    public static ConfigEntry<bool> NoEat;
    public static ConfigEntry<bool> NoInjury;
    public static ConfigEntry<bool> NoCold;
    public static ConfigEntry<bool> NoPoison;
    public static ConfigEntry<bool> NoHot;
    public static ConfigEntry<bool> NoCurse;
    public static ConfigEntry<bool> NoDrowsy;
    public static ConfigEntry<bool> NoSpores;
    public static ConfigEntry<bool> NoPetrify;
    public static ConfigEntry<bool> NoRagdoll;

    // Cheats
    public static ConfigEntry<bool> UnlimitedItemUses;

    // Character Toggles
    public static ConfigEntry<bool> SpeedMod;
    public static ConfigEntry<bool> JumpMod;
    public static ConfigEntry<bool> NoFallDmg;
    public static ConfigEntry<bool> ClimbMod;
    public static ConfigEntry<bool> VineClimbMod;
    public static ConfigEntry<bool> RopeClimbMod;

    // Character Amounts
    public static ConfigEntry<float> SpeedAmount;
    public static ConfigEntry<float> JumpAmount;
    public static ConfigEntry<float> ClimbAmount;
    public static ConfigEntry<float> VineClimbAmount;
    public static ConfigEntry<float> RopeClimbAmount;

    // Inventory Recharge
    public static ConfigEntry<float> RechargeAmountSlot1;
    public static ConfigEntry<float> RechargeAmountSlot2;
    public static ConfigEntry<float> RechargeAmountSlot3;

    // Teleport
    public static ConfigEntry<bool> TeleportToPing;

    // Markers
    public static ConfigEntry<bool> ShowPlayerMarkers;

    // Luggage ESP
    public static ConfigEntry<bool> LuggageESP;
    public static ConfigEntry<string> LuggageESPColor;

    // Coord Overlay
    public static ConfigEntry<bool> ShowCoordOverlay;

    // Keybinds (config-file only, stored as strings to avoid BepInEx enum spam)
    public static ConfigEntry<string> KeybindInfiniteStamina;
    public static ConfigEntry<string> KeybindFreezeAfflictions;
    public static ConfigEntry<string> KeybindNoWeight;
    public static ConfigEntry<string> KeybindUnlimitedItemUses;
    public static ConfigEntry<string> KeybindSpeedMod;
    public static ConfigEntry<string> KeybindJumpMod;
    public static ConfigEntry<string> KeybindClimbMod;
    public static ConfigEntry<string> KeybindVineClimbMod;
    public static ConfigEntry<string> KeybindRopeClimbMod;
    public static ConfigEntry<string> KeybindTeleportToPing;
    public static ConfigEntry<string> KeybindFlyMod;
    public static ConfigEntry<string> KeybindShowPlayerMarkers;
    public static ConfigEntry<string> KeybindShowCoordOverlay;

    public static void Init(ConfigFile config, ManualLogSource logger)
    {
        Logger = logger;
        Utilities.Logger = logger;

        // Cheats
        InfiniteStamina = config.Bind("Cheats", "InfiniteStamina", false);
        TeleportToPing = config.Bind("Cheats", "TeleportToPing", false);
        FlyMod = config.Bind("Cheats", "Fly Mod", false);
        FlySpeed = config.Bind("Cheats", "Fly Speed", 100f);
        FlyAcceleration = config.Bind("Cheats", "Fly Acceleration", 300f);

        // Afflictions
        LockStatus = config.Bind("Afflictions", "LockStatus", false);
        NoWeight = config.Bind("Afflictions", "NoWeight", false);
        NoEat = config.Bind("Afflictions", "NoEat", false);
        NoInjury = config.Bind("Afflictions", "NoInjury", false);
        NoCold = config.Bind("Afflictions", "NoCold", false);
        NoPoison = config.Bind("Afflictions", "NoPoison", false);
        NoHot = config.Bind("Afflictions", "NoHot", false);
        NoCurse = config.Bind("Afflictions", "NoCurse", false);
        NoDrowsy = config.Bind("Afflictions", "NoDrowsy", false);
        NoSpores = config.Bind("Afflictions", "NoSpores", false);
        NoPetrify = config.Bind("Afflictions", "NoPetrify", false);
        NoRagdoll = config.Bind("Afflictions", "NoRagdoll", false);

        // Cheats
        UnlimitedItemUses = config.Bind("Cheats", "UnlimitedItemUses", false);
        ShowPlayerMarkers = config.Bind("Cheats", "ShowPlayerMarkers", true);

        // Luggage ESP
        LuggageESP = config.Bind("World", "LuggageESP", false);
        LuggageESPColor = config.Bind("World", "LuggageESPColor", "00FF00");

        // Coord Overlay
        ShowCoordOverlay = config.Bind("UI", "ShowCoordOverlay", false);

        // Keybinds — set a key name (e.g. F5, Alpha1, Keypad0) or None to disable
        KeybindInfiniteStamina = config.Bind("Keybinds", "InfiniteStamina", "None");
        KeybindFreezeAfflictions = config.Bind("Keybinds", "FreezeAfflictions", "None");
        KeybindNoWeight = config.Bind("Keybinds", "NoWeight", "None");
        KeybindUnlimitedItemUses = config.Bind("Keybinds", "UnlimitedItemUses", "None");
        KeybindSpeedMod = config.Bind("Keybinds", "SpeedMod", "None");
        KeybindJumpMod = config.Bind("Keybinds", "JumpMod", "None");
        KeybindClimbMod = config.Bind("Keybinds", "ClimbMod", "None");
        KeybindVineClimbMod = config.Bind("Keybinds", "VineClimbMod", "None");
        KeybindRopeClimbMod = config.Bind("Keybinds", "RopeClimbMod", "None");
        KeybindTeleportToPing = config.Bind("Keybinds", "TeleportToPing", "None");
        KeybindFlyMod = config.Bind("Keybinds", "FlyMod", "None");
        KeybindShowPlayerMarkers = config.Bind("Keybinds", "ShowPlayerMarkers", "None");
        KeybindShowCoordOverlay = config.Bind("Keybinds", "ShowCoordOverlay", "None");

        // Character Toggles
        SpeedMod = config.Bind("Character", "SpeedMod", false);
        JumpMod = config.Bind("Character", "JumpMod", false);
        NoFallDmg = config.Bind("Character", "NoFallDmg", false);
        ClimbMod = config.Bind("Character", "ClimbMod", false);
        VineClimbMod = config.Bind("Character", "VineClimbMod", false);
        RopeClimbMod = config.Bind("Character", "RopeClimbMod", false);

        // Character Amounts
        SpeedAmount = config.Bind("Character", "SpeedAmount", 1.0f);
        JumpAmount = config.Bind("Character", "JumpAmount", 10.0f);
        ClimbAmount = config.Bind("Character", "ClimbAmount", 1.0f);
        VineClimbAmount = config.Bind("Character", "VineClimbAmount", 1.0f);
        RopeClimbAmount = config.Bind("Character", "RopeClimbAmount", 1.0f);

        // Inventory
        RechargeAmountSlot1 = config.Bind("Inventory", "RechargeAmountSlot1", 100f);
        RechargeAmountSlot2 = config.Bind("Inventory", "RechargeAmountSlot2", 100f);
        RechargeAmountSlot3 = config.Bind("Inventory", "RechargeAmountSlot3", 100f);

        Logger.LogInfo("[PeakMod][ConfigManager] Config Loaded.");
    }
}
