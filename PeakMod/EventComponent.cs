using UnityEngine;
using BepInEx.Configuration;

public class EventComponent : MonoBehaviour
{
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
        ToggleKeybind(ConfigManager.KeybindInfiniteStamina, ConfigManager.InfiniteStamina);
        ToggleKeybind(ConfigManager.KeybindFreezeAfflictions, ConfigManager.LockStatus);
        ToggleKeybind(ConfigManager.KeybindNoWeight, ConfigManager.NoWeight);
        ToggleKeybind(ConfigManager.KeybindUnlimitedItemUses, ConfigManager.UnlimitedItemUses);
        ToggleKeybind(ConfigManager.KeybindSpeedMod, ConfigManager.SpeedMod);
        ToggleKeybind(ConfigManager.KeybindJumpMod, ConfigManager.JumpMod);
        ToggleKeybind(ConfigManager.KeybindClimbMod, ConfigManager.ClimbMod);
        ToggleKeybind(ConfigManager.KeybindVineClimbMod, ConfigManager.VineClimbMod);
        ToggleKeybind(ConfigManager.KeybindRopeClimbMod, ConfigManager.RopeClimbMod);
        ToggleKeybind(ConfigManager.KeybindTeleportToPing, ConfigManager.TeleportToPing);
        ToggleKeybind(ConfigManager.KeybindFlyMod, ConfigManager.FlyMod);
        ToggleKeybind(ConfigManager.KeybindShowPlayerMarkers, ConfigManager.ShowPlayerMarkers);
        ToggleKeybind(ConfigManager.KeybindShowCoordOverlay, ConfigManager.ShowCoordOverlay);
    }

    private void ToggleKeybind(ConfigEntry<string> keybind, ConfigEntry<bool> toggle)
    {
        if (string.IsNullOrEmpty(keybind.Value) || keybind.Value == "None")
            return;

        KeyCode key;
        if (System.Enum.TryParse(keybind.Value, true, out key) && Input.GetKeyDown(key))
        {
            toggle.Value = !toggle.Value;
        }
    }
}
