using UnityEngine;
using BepInEx.Configuration;

public class EventComponent : MonoBehaviour
{
    private KeyCode? cachedInfiniteStamina;
    private KeyCode? cachedFreezeAfflictions;
    private KeyCode? cachedNoWeight;
    private KeyCode? cachedUnlimitedItemUses;
    private KeyCode? cachedSpeedMod;
    private KeyCode? cachedJumpMod;
    private KeyCode? cachedClimbMod;
    private KeyCode? cachedVineClimbMod;
    private KeyCode? cachedRopeClimbMod;
    private KeyCode? cachedTeleportToPing;
    private KeyCode? cachedFlyMod;
    private KeyCode? cachedShowPlayerMarkers;
    private KeyCode? cachedShowCoordOverlay;

    private string lastInfiniteStamina;
    private string lastFreezeAfflictions;
    private string lastNoWeight;
    private string lastUnlimitedItemUses;
    private string lastSpeedMod;
    private string lastJumpMod;
    private string lastClimbMod;
    private string lastVineClimbMod;
    private string lastRopeClimbMod;
    private string lastTeleportToPing;
    private string lastFlyMod;
    private string lastShowPlayerMarkers;
    private string lastShowCoordOverlay;

    private void Update()
    {
        var movement = GameHelpers.GetMovementComponent();
        if (movement != null)
        {
            if (ConfigManager.SpeedMod.Value)
                ConstantFields.GetMovementModifierField()?.SetValue(movement, ConfigManager.SpeedAmount.Value);

            if (ConfigManager.JumpMod.Value)
            {
                ConstantFields.GetJumpGravityField()?.SetValue(movement, ConfigManager.JumpAmount.Value);

                if (ConfigManager.NoFallDmg.Value)
                    ConstantFields.GetFallDamageTimeField()?.SetValue(movement, 999f);
            }
        }

        var character = GameHelpers.GetCharacterComponent();
        if (character != null)
        {
            if (ConfigManager.InfiniteStamina.Value)
                ConstantFields.GetInfiniteStaminaProperty()?.SetValue(character, true);

            if (ConfigManager.LockStatus.Value)
                ConstantFields.GetStatusLockProperty()?.SetValue(character, true);
        }

        var climb = GameHelpers.GetClimbingComponent();
        if (climb != null && ConfigManager.ClimbMod.Value)
        {
            ConstantFields.GetClimbSpeedModField()?.SetValue(climb, ConfigManager.ClimbAmount.Value);
        }

        var vine = GameHelpers.GetVineClimbComponent();
        if (vine != null && ConfigManager.VineClimbMod.Value)
        {
            ConstantFields.GetVineClimbSpeedModField()?.SetValue(vine, ConfigManager.VineClimbAmount.Value);
        }

        var rope = GameHelpers.GetRopeClimbComponent();
        if (rope != null && ConfigManager.RopeClimbMod.Value)
        {
            ConstantFields.GetRopeClimbSpeedModField()?.SetValue(rope, ConfigManager.RopeClimbAmount.Value);
        }

        HandleKeybinds();
    }

    private void HandleKeybinds()
    {
        ToggleKeybind(ConfigManager.KeybindInfiniteStamina, ConfigManager.InfiniteStamina, ref cachedInfiniteStamina, ref lastInfiniteStamina);
        ToggleKeybind(ConfigManager.KeybindFreezeAfflictions, ConfigManager.LockStatus, ref cachedFreezeAfflictions, ref lastFreezeAfflictions);
        ToggleKeybind(ConfigManager.KeybindNoWeight, ConfigManager.NoWeight, ref cachedNoWeight, ref lastNoWeight);
        ToggleKeybind(ConfigManager.KeybindUnlimitedItemUses, ConfigManager.UnlimitedItemUses, ref cachedUnlimitedItemUses, ref lastUnlimitedItemUses);
        ToggleKeybind(ConfigManager.KeybindSpeedMod, ConfigManager.SpeedMod, ref cachedSpeedMod, ref lastSpeedMod);
        ToggleKeybind(ConfigManager.KeybindJumpMod, ConfigManager.JumpMod, ref cachedJumpMod, ref lastJumpMod);
        ToggleKeybind(ConfigManager.KeybindClimbMod, ConfigManager.ClimbMod, ref cachedClimbMod, ref lastClimbMod);
        ToggleKeybind(ConfigManager.KeybindVineClimbMod, ConfigManager.VineClimbMod, ref cachedVineClimbMod, ref lastVineClimbMod);
        ToggleKeybind(ConfigManager.KeybindRopeClimbMod, ConfigManager.RopeClimbMod, ref cachedRopeClimbMod, ref lastRopeClimbMod);
        ToggleKeybind(ConfigManager.KeybindTeleportToPing, ConfigManager.TeleportToPing, ref cachedTeleportToPing, ref lastTeleportToPing);
        ToggleKeybind(ConfigManager.KeybindFlyMod, ConfigManager.FlyMod, ref cachedFlyMod, ref lastFlyMod);
        ToggleKeybind(ConfigManager.KeybindShowPlayerMarkers, ConfigManager.ShowPlayerMarkers, ref cachedShowPlayerMarkers, ref lastShowPlayerMarkers);
        ToggleKeybind(ConfigManager.KeybindShowCoordOverlay, ConfigManager.ShowCoordOverlay, ref cachedShowCoordOverlay, ref lastShowCoordOverlay);
    }

    private void ToggleKeybind(ConfigEntry<string> keybind, ConfigEntry<bool> toggle, ref KeyCode? cachedKey, ref string lastValue)
    {
        if (keybind.Value != lastValue)
        {
            lastValue = keybind.Value;
            KeyCode parsed;
            if (string.IsNullOrEmpty(keybind.Value) || keybind.Value == "None")
                cachedKey = null;
            else if (System.Enum.TryParse(keybind.Value, true, out parsed))
                cachedKey = parsed;
            else
                cachedKey = null;
        }

        if (cachedKey.HasValue && Input.GetKeyDown(cachedKey.Value))
        {
            toggle.Value = !toggle.Value;
        }
    }
}
