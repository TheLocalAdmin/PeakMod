using UnityEngine;

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

        HandleFog();
        HandleCoordOverlay();
        HandleVanishMode();
    }

    private bool wasFogEnabled = true;

    private void HandleFog()
    {
        if (ConfigManager.NoFog.Value)
        {
            if (RenderSettings.fog)
            {
                wasFogEnabled = true;
                RenderSettings.fog = false;
            }
        }
        else
        {
            if (wasFogEnabled && !RenderSettings.fog)
            {
                RenderSettings.fog = true;
            }
        }
    }

    private void HandleCoordOverlay()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Globals.showCoordOverlay = !Globals.showCoordOverlay;
        }
    }

    private void HandleVanishMode()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            ConfigManager.VanishMode.Value = !ConfigManager.VanishMode.Value;
        }

        if (ConfigManager.VanishMode.Value)
        {
            // Enable fly mode
            if (!ConfigManager.FlyMod.Value)
                ConfigManager.FlyMod.Value = true;

            // Enable coord overlay
            Globals.showCoordOverlay = true;

            // Make invisible via renderer
            var character = GameHelpers.GetCharacterComponent();
            if (character != null)
            {
                var renderers = character.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                {
                    if (r != null && r.enabled)
                        r.enabled = false;
                }
            }
        }
        else
        {
            // Restore renderers when vanishing is turned off
            var character = GameHelpers.GetCharacterComponent();
            if (character != null)
            {
                var renderers = character.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                {
                    if (r != null && !r.enabled)
                        r.enabled = true;
                }
            }
        }
    }
}
