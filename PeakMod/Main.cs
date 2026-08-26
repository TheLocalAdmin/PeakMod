using BepInEx;
using ImGuiNET;
using DearImGuiInjection;
using DearImGuiInjection.BepInEx;
using UnityEngine;
using System.Xml;
using System.Reflection;
using System;
using BepInEx.Configuration;
using HarmonyLib;
using Photon.Pun;
using System.Collections.Generic;

[BepInDependency(DearImGuiInjection.Metadata.GUID)]
[BepInPlugin("com.thelocaladmin.peakmod", "PeakMod V0.2.0 by TheLocalAdmin", "0.2.0")]

public class PeakMod : BaseUnityPlugin
{
    // Menu
    private bool styleApplied = false;
    private int selectedTab = 1;

    private void ApplyCustomStyle()
    {
        var style = ImGui.GetStyle();
        var colors = style.Colors;

        var bgBlack = new System.Numerics.Vector4(0.04f, 0.04f, 0.04f, 1.00f);
        var windowBlack = new System.Numerics.Vector4(0.06f, 0.06f, 0.06f, 1.00f);
        var panelBlack = new System.Numerics.Vector4(0.09f, 0.09f, 0.09f, 1.00f);
        var fieldBlack = new System.Numerics.Vector4(0.12f, 0.12f, 0.12f, 1.00f);
        var fieldHover = new System.Numerics.Vector4(0.17f, 0.17f, 0.17f, 1.00f);
        var fieldActive = new System.Numerics.Vector4(0.20f, 0.20f, 0.20f, 1.00f);
        var textWhite = new System.Numerics.Vector4(0.92f, 0.92f, 0.92f, 1.00f);
        var textDim = new System.Numerics.Vector4(0.55f, 0.55f, 0.55f, 1.00f);
        var accent = new System.Numerics.Vector4(0.93f, 0.62f, 0.06f, 1.00f);      // amber accent
        var accentHover = new System.Numerics.Vector4(1.00f, 0.72f, 0.20f, 1.00f);
        var accentActive = new System.Numerics.Vector4(0.75f, 0.48f, 0.03f, 1.00f);
        var borders = new System.Numerics.Vector4(0.20f, 0.20f, 0.20f, 1.00f);

        colors[(int)ImGuiCol.WindowBg] = windowBlack;
        colors[(int)ImGuiCol.ChildBg] = panelBlack;
        colors[(int)ImGuiCol.PopupBg] = fieldBlack;
        colors[(int)ImGuiCol.Border] = borders;
        colors[(int)ImGuiCol.TitleBg] = bgBlack;
        colors[(int)ImGuiCol.TitleBgActive] = bgBlack;
        colors[(int)ImGuiCol.TitleBgCollapsed] = bgBlack;
        colors[(int)ImGuiCol.Text] = textWhite;
        colors[(int)ImGuiCol.TextDisabled] = textDim;
        colors[(int)ImGuiCol.CheckMark] = accent;
        colors[(int)ImGuiCol.FrameBg] = fieldBlack;
        colors[(int)ImGuiCol.FrameBgHovered] = fieldHover;
        colors[(int)ImGuiCol.FrameBgActive] = fieldActive;
        colors[(int)ImGuiCol.TableHeaderBg] = fieldBlack;
        colors[(int)ImGuiCol.TableBorderStrong] = borders;
        colors[(int)ImGuiCol.TableBorderLight] = borders;
        colors[(int)ImGuiCol.TableRowBg] = bgBlack;
        colors[(int)ImGuiCol.TableRowBgAlt] = windowBlack;
        colors[(int)ImGuiCol.Button] = fieldBlack;
        colors[(int)ImGuiCol.ButtonHovered] = accent;
        colors[(int)ImGuiCol.ButtonActive] = accentActive;
        colors[(int)ImGuiCol.Header] = fieldBlack;
        colors[(int)ImGuiCol.HeaderHovered] = fieldHover;
        colors[(int)ImGuiCol.HeaderActive] = fieldActive;
        colors[(int)ImGuiCol.Separator] = borders;
        colors[(int)ImGuiCol.SeparatorHovered] = accent;
        colors[(int)ImGuiCol.SeparatorActive] = accentHover;
        colors[(int)ImGuiCol.ScrollbarBg] = bgBlack;
        colors[(int)ImGuiCol.ScrollbarGrab] = fieldHover;
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = fieldActive;
        colors[(int)ImGuiCol.ScrollbarGrabActive] = accent;
        colors[(int)ImGuiCol.SliderGrab] = accent;
        colors[(int)ImGuiCol.SliderGrabActive] = accentHover;
        colors[(int)ImGuiCol.NavHighlight] = accent;
        colors[(int)ImGuiCol.TextSelectedBg] = new System.Numerics.Vector4(0.93f, 0.62f, 0.06f, 0.30f);

        style.WindowRounding = 6f;
        style.FrameRounding = 4f;
        style.ChildRounding = 4f;
        style.FrameBorderSize = 1.0f;
        style.GrabRounding = 4f;
        style.WindowPadding = new System.Numerics.Vector2(6, 6);
        style.CellPadding = new System.Numerics.Vector2(4, 4);
        style.ItemSpacing = new System.Numerics.Vector2(4, 4);
        style.WindowBorderSize = 1.0f;
        style.PopupBorderSize = 1.0f;
    }
    private void Awake()
    {
        Logger.LogInfo("PeakMod V0.2.0 by TheLocalAdmin - Mod Initialized");
        this.gameObject.AddComponent<EventComponent>();
    }

    private void OnEnable()
    {
        Logger.LogInfo("[PeakMod] OnEnable called");

        Globals.itemSearchBuffers = new string[3] { "", "", "" };
        ConfigManager.Init(Config, Logger);
        DearImGuiInjection.DearImGuiInjection.Render += MyUI;
        DearImGuiInjection.DearImGuiInjection.Render += DrawPlayerMarkers;
        DearImGuiInjection.DearImGuiInjection.Render += DrawCoordOverlay;
        DearImGuiInjection.DearImGuiInjection.Render += DrawLuggageESP;

        // Initialize Harmony
        var harmony = new Harmony("com.thelocaladmin.peakmod");
        harmony.PatchAll();
        Logger.LogInfo("Harmony patches applied.");
    }

    private void OnDisable()
    {
        Logger.LogInfo("[PeakMod] OnDisable called");
        DearImGuiInjection.DearImGuiInjection.Render -= MyUI;
        DearImGuiInjection.DearImGuiInjection.Render -= DrawPlayerMarkers;
        DearImGuiInjection.DearImGuiInjection.Render -= DrawCoordOverlay;
        DearImGuiInjection.DearImGuiInjection.Render -= DrawLuggageESP;
    }

    private void DrawPlayerMarkers()
    {
        try
        {
            if (!ConfigManager.ShowPlayerMarkers.Value)
                return;

            Camera cam = Camera.main;
            if (cam == null)
                return;

            Character local = Character.localCharacter;
            if (local == null)
                return;

            if (Globals.allPlayers.Count == 0)
                Utilities.RefreshPlayerList();

            var drawList = ImGui.GetBackgroundDrawList();
            var display = ImGui.GetIO().DisplaySize;

            for (int i = 0; i < Globals.allPlayers.Count; i++)
            {
                Character other = Globals.allPlayers[i];
                if (other == null || other == local)
                    continue;

                if (other.Ghost != null)
                    continue;

                Vector3 head;
                try { head = other.Head; } catch { continue; }

                Vector3 screen = cam.WorldToScreenPoint(head);
                if (screen.z < 0f)
                    continue;

                float distance = Vector3.Distance(local.Head, head);
                string name = "Player";
                try { name = other.characterName; } catch { }

                string label = $"{name} - {distance:F0}m";
                var pos = new System.Numerics.Vector2(screen.x, display.Y - screen.y);

                uint col = 0xFFF2B207;
                System.Numerics.Vector2 textSize = ImGui.CalcTextSize(label);
                drawList.AddRectFilled(
                    new System.Numerics.Vector2(pos.X - 2f, pos.Y - 15f),
                    new System.Numerics.Vector2(pos.X + textSize.X + 4f, pos.Y + textSize.Y - 2f),
                    0xA0000000);
                drawList.AddText(pos, col, label);
            }
        }
        catch (Exception ex)
        {
            ConfigManager.Logger.LogError("[PeakMod] DrawPlayerMarkers Exception: " + ex);
        }
    }

    private void DrawCoordOverlay()
    {
        try
        {
            if (!Globals.showCoordOverlay)
                return;

            Character local = Character.localCharacter;
            if (local == null)
                return;

            var drawList = ImGui.GetBackgroundDrawList();
            var display = ImGui.GetIO().DisplaySize;
            float startY = 10f;
            float lineH = 18f;
            uint bgCol = 0xC0000000;
            uint textCol = 0xFFF2B207;
            uint playerCol = 0xFF00CCFF;
            uint luggageCol = 0xFF00FF88;

            Vector3 localPos = local.Head;
            string localLabel = $"YOU: X={localPos.x:F1} Y={localPos.y:F1} Z={localPos.z:F1}";
            System.Numerics.Vector2 textSize = ImGui.CalcTextSize(localLabel);
            drawList.AddRectFilled(
                new System.Numerics.Vector2(10f, startY - 2f),
                new System.Numerics.Vector2(12f + textSize.X + 8f, startY + textSize.Y + 2f),
                bgCol);
            drawList.AddText(new System.Numerics.Vector2(14f, startY), textCol, localLabel);
            startY += lineH;

            if (Globals.allPlayers.Count == 0)
                Utilities.RefreshPlayerList();

            for (int i = 0; i < Globals.allPlayers.Count; i++)
            {
                Character other = Globals.allPlayers[i];
                if (other == null || other == local)
                    continue;
                if (other.Ghost != null)
                    continue;

                Vector3 otherPos;
                try { otherPos = other.Head; } catch { continue; }

                string name = "Player";
                try { name = other.characterName; } catch { }
                float dist = Vector3.Distance(localPos, otherPos);
                string label = $"{name}: X={otherPos.x:F1} Y={otherPos.y:F1} Z={otherPos.z:F1} [{dist:F0}m]";
                textSize = ImGui.CalcTextSize(label);
                drawList.AddRectFilled(
                    new System.Numerics.Vector2(10f, startY - 2f),
                    new System.Numerics.Vector2(12f + textSize.X + 8f, startY + textSize.Y + 2f),
                    bgCol);
                drawList.AddText(new System.Numerics.Vector2(14f, startY), playerCol, label);
                startY += lineH;
            }

            for (int i = 0; i < Globals.luggageObject.Count && i < 5; i++)
            {
                var lug = Globals.luggageObject[i];
                if (lug == null) continue;

                Vector3 lugPos = lug.Center();
                float dist = Vector3.Distance(localPos, lugPos);
                string lugName = lug.displayName ?? "Container";
                string label = $"{lugName}: X={lugPos.x:F1} Y={lugPos.y:F1} Z={lugPos.z:F1} [{dist:F0}m]";
                textSize = ImGui.CalcTextSize(label);
                drawList.AddRectFilled(
                    new System.Numerics.Vector2(10f, startY - 2f),
                    new System.Numerics.Vector2(12f + textSize.X + 8f, startY + textSize.Y + 2f),
                    bgCol);
                drawList.AddText(new System.Numerics.Vector2(14f, startY), luggageCol, label);
                startY += lineH;
            }
        }
        catch (Exception ex)
        {
            ConfigManager.Logger.LogError("[PeakMod] DrawCoordOverlay Exception: " + ex);
        }
    }

    private void DrawLuggageESP()
    {
        try
        {
            if (!ConfigManager.LuggageESP.Value)
                return;

            Character local = Character.localCharacter;
            if (local == null)
                return;

            Camera cam = Camera.main;
            if (cam == null)
                return;

            Utilities.EnsureLuggageListInitialized();

            uint color = ParseHexColor(ConfigManager.LuggageESPColor.Value);
            var drawList = ImGui.GetBackgroundDrawList();

            for (int i = 0; i < Globals.luggageObject.Count; i++)
            {
                var lug = Globals.luggageObject[i];
                if (lug == null) continue;

                Vector3 lugPos = lug.Center();
                float distance = Vector3.Distance(local.Head, lugPos);

                Vector3 screenCenter = cam.WorldToScreenPoint(lugPos);
                if (screenCenter.z < 0f) continue;

                var display = ImGui.GetIO().DisplaySize;
                float screenX = screenCenter.x;
                float screenY = display.Y - screenCenter.y;

                float boxSize = Mathf.Clamp(800f / distance, 8f, 60f);

                var min = new System.Numerics.Vector2(screenX - boxSize, screenY - boxSize);
                var max = new System.Numerics.Vector2(screenX + boxSize, screenY + boxSize);

                drawList.AddRect(min, max, color, 0f, ImDrawFlags.None, 2f);

                string lugName = lug.displayName ?? "Container";
                string label = $"{lugName} [{distance:F0}m]";
                var textSize = ImGui.CalcTextSize(label);
                var textPos = new System.Numerics.Vector2(screenX - textSize.X / 2f, min.Y - textSize.Y - 2f);
                drawList.AddRectFilled(
                    new System.Numerics.Vector2(textPos.X - 2f, textPos.Y - 1f),
                    new System.Numerics.Vector2(textPos.X + textSize.X + 2f, textPos.Y + textSize.Y + 1f),
                    0xA0000000);
                drawList.AddText(textPos, color, label);
            }
        }
        catch (Exception ex)
        {
            ConfigManager.Logger.LogError("[PeakMod] DrawLuggageESP Exception: " + ex);
        }
    }

    private uint ParseHexColor(string hex)
    {
        try
        {
            hex = hex.Replace("#", "").Trim();
            if (hex.Length == 6)
            {
                byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                return (uint)(0xFF000000 | (uint)(r << 16) | (uint)(g << 8) | (uint)b);
            }
        }
        catch { }
        return 0xFF00FF00;
    }

    void DrawCheckbox(ConfigEntry<bool> config, string label, Action<bool> mainThreadAction = null)
    {
        bool value = config.Value;
        if (ImGui.Checkbox(label, ref value))
        {
            config.Value = value;
            Logger.LogInfo($"[Menu] {label} toggled to {(value ? "ON" : "OFF")}");

            if (mainThreadAction != null)
            {
                UnityMainThreadDispatcher.Enqueue(() => mainThreadAction.Invoke(value));
            }
        }
    }

    void DrawSliderFloat(ConfigEntry<float> config, string label, float min, float max, string format = "%.2f")
    {
        float value = config.Value;
        if (ImGui.SliderFloat(label, ref value, min, max, format))
            config.Value = value;
    }

    bool DrawSearchableCombo(string label, ref int selectedIndex, List<string> items, ref string searchBuffer)
    {
        bool changed = false;

        // Draw input field
        string inputId = $"Search##{label}";
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - 4);
        ImGui.InputText("##" + inputId, ref searchBuffer, 100);
        ImGui.PopItemWidth();

        // Draw custom placeholder if input is empty and not active
        if (string.IsNullOrEmpty(searchBuffer) && !ImGui.IsItemActive())
        {
            var pos = ImGui.GetItemRectMin();
            ImGui.SameLine();
            ImGui.SetCursorScreenPos(pos + new System.Numerics.Vector2(4, 2));
            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.55f, 0.55f, 0.55f, 1.00f));
            ImGui.TextUnformatted("Search items...");
            ImGui.PopStyleColor();
        }

        if (ImGui.BeginCombo(label, selectedIndex >= 0 && selectedIndex < items.Count ? items[selectedIndex] : "None"))
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (!string.IsNullOrEmpty(searchBuffer) &&
                    !items[i].ToLower().Contains(searchBuffer.ToLower()))
                    continue;

                bool isSelected = (selectedIndex == i);
                if (ImGui.Selectable($"{items[i]}##{i}", isSelected))
                {
                    selectedIndex = i;
                    changed = true;
                }

                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }

        return changed;
    }

    void DrawToolTip(string text)
    {
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");

        if (ImGui.IsItemHovered())
        {
            ImGui.PushStyleColor(ImGuiCol.PopupBg, new System.Numerics.Vector4(0.12f, 0.12f, 0.12f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.92f, 0.92f, 0.92f, 1.00f));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.0f);

            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(450.0f);
            ImGui.TextUnformatted(text);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();

            ImGui.PopStyleVar();
            ImGui.PopStyleColor(2);
        }
    }

    private void MyUI()
    {
        try
        {
            // Only draw the menu when the ImGui cursor is visible (toggled with Insert)
            if (!DearImGuiInjection.DearImGuiInjection.IsCursorVisible)
                return;

            if (!styleApplied)
            {
                ApplyCustomStyle();
                styleApplied = true;
            }

            // Set window position and size
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(20, 20), ImGuiCond.Once);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(540, 340), ImGuiCond.Once);

            if (ImGui.Begin("PeakMod V0.2.0 by TheLocalAdmin##Main", ImGuiWindowFlags.NoCollapse))
            {
                // Sidebar
                ImGui.BeginChild("Sidebar", new System.Numerics.Vector2(90, 0), true);
                ImGui.Dummy(new System.Numerics.Vector2(4, 2));
                string[] sidebarItems = { "PLAYER", "ITEMS", "SPAWN", "LOBBY", "WORLD", "STAGES", "ACHIEVE", "HOST", "PROFILE", "ABOUT" };
                for (int i = 0; i < sidebarItems.Length; i++)
                {
                    bool isSelected = (selectedTab == i + 1);
                    string label = sidebarItems[i];

                    var textColor = isSelected
                        ? new System.Numerics.Vector4(0.96f, 0.70f, 0.16f, 1.0f)
                        : new System.Numerics.Vector4(0.85f, 0.85f, 0.85f, 1.00f);

                    ImGui.PushStyleColor(ImGuiCol.Text, textColor);

                    float textWidth = ImGui.CalcTextSize(label).X;
                    float availableWidth = ImGui.GetContentRegionAvail().X;
                    float offsetX = (availableWidth - textWidth) * 0.5f;
                    ImGui.SetCursorPosX(offsetX);

                    if (ImGui.Selectable(label, isSelected))
                        selectedTab = i + 1;

                    ImGui.PopStyleColor();
                }

                ImGui.EndChild();

                // Main content area
                ImGui.SameLine();
                ImGui.BeginChild("MainArea");

                // Player
                if (selectedTab == 1)
                {
                    // Sub-tabs
                    ImGui.Dummy(new System.Numerics.Vector2(4, 2));
                    if (ImGui.Button("Self Mods##subtab0"))
                        Globals.teamSubTab = 0;
                    ImGui.SameLine();
                    if (ImGui.Button("Team##subtab1"))
                        Globals.teamSubTab = 1;
                    ImGui.SameLine();
                    ImGui.Dummy(new System.Numerics.Vector2(4, 2));

                    ImGui.Separator();

                    if (Globals.teamSubTab == 0)
                    {
                        // Self Mods sub-tab
                        float fullWidth = ImGui.GetContentRegionAvail().X;
                        float halfWidth = fullWidth / 2f;

                        ImGui.BeginChild("PlayerColumn", new System.Numerics.Vector2(halfWidth, 0), true);
                        ImGui.Indent(4.0f);
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                        ImGui.Dummy(new System.Numerics.Vector2(4, 2));
                        if (ImGui.CollapsingHeader("Self Mods##SelfMods", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            DrawCheckbox(ConfigManager.InfiniteStamina, "Infinite Stamina", (val) =>
                            {
                                var character = GameHelpers.GetCharacterComponent();
                                var prop = ConstantFields.GetInfiniteStaminaProperty();
                                if (character != null && prop != null)
                                    prop.SetValue(character, val);
                            });
                            ImGui.SameLine();
                            DrawToolTip("Prevents stamina from decreasing, allowing unlimited sprinting and actions.");

                            DrawCheckbox(ConfigManager.LockStatus, "Freeze Afflictions", (val) =>
                            {
                                var character = GameHelpers.GetCharacterComponent();
                                var prop = ConstantFields.GetStatusLockProperty();
                                if (character != null && prop != null)
                                    prop.SetValue(character, val);
                            });
                            ImGui.SameLine();
                            DrawToolTip("Prevents your statuses from changing.");

                            DrawCheckbox(ConfigManager.NoWeight, "No Weight");
                            ImGui.SameLine();
                            DrawToolTip("Disables weight penalties from carried items and backpack.");

                            DrawCheckbox(ConfigManager.NoFog, "No Fog");
                            ImGui.SameLine();
                            DrawToolTip("Removes the in-world fog.");
                            DrawCheckbox(ConfigManager.UnlimitedItemUses, "Unlimited Item Uses");
                            ImGui.SameLine();
                            DrawToolTip("Items never run out of uses.");

                            DrawCheckbox(ConfigManager.SpeedMod, "Change Speed", (val) =>
                            {
                                var movement = GameHelpers.GetMovementComponent();
                                var field = ConstantFields.GetMovementModifierField();
                                if (movement != null && field != null)
                                    field.SetValue(movement, ConfigManager.SpeedAmount.Value);
                            });
                            ImGui.SameLine();
                            DrawToolTip("Overrides your character's movement speed with a custom multiplier.");

                            DrawCheckbox(ConfigManager.JumpMod, "Change Jump", (val) =>
                            {
                                var movement = GameHelpers.GetMovementComponent();
                                var jumpField = ConstantFields.GetJumpGravityField();
                                var fallField = ConstantFields.GetFallDamageTimeField();
                                if (movement != null && jumpField != null)
                                    jumpField.SetValue(movement, ConfigManager.JumpAmount.Value);
                                if (movement != null && fallField != null)
                                    fallField.SetValue(movement, ConfigManager.NoFallDmg.Value ? 999f : 1.5f);
                            });
                            ImGui.SameLine();
                            DrawToolTip("Modifies jump height, allowing higher or lower jumps depending on your settings.");

                            DrawCheckbox(ConfigManager.ClimbMod, "Change Climb", (val) =>
                            {
                                var climb = GameHelpers.GetClimbingComponent();
                                var field = ConstantFields.GetClimbSpeedModField();
                                if (climb != null && field != null)
                                    field.SetValue(climb, ConfigManager.ClimbAmount.Value);
                            });
                            ImGui.SameLine();
                            DrawToolTip("Adjusts the speed at which you climb ladders and surfaces.");

                            DrawCheckbox(ConfigManager.VineClimbMod, "Change Vine Climb", (val) =>
                            {
                                var vine = GameHelpers.GetVineClimbComponent();
                                var field = ConstantFields.GetVineClimbSpeedModField();
                                if (vine != null && field != null)
                                    field.SetValue(vine, ConfigManager.VineClimbAmount.Value);
                            });
                            ImGui.SameLine();
                            DrawToolTip("Changes climbing speed specifically for vines.");

                            DrawCheckbox(ConfigManager.RopeClimbMod, "Change Rope Climb", (val) =>
                            {
                                var rope = GameHelpers.GetRopeClimbComponent();
                                var field = ConstantFields.GetRopeClimbSpeedModField();
                                if (rope != null && field != null)
                                    field.SetValue(rope, ConfigManager.RopeClimbAmount.Value);
                            });
                            ImGui.SameLine();
                            DrawToolTip("Modifies climbing speed when using ropes or rope-based obstacles.");

                            DrawCheckbox(ConfigManager.TeleportToPing, "Teleport to Ping");
                            ImGui.SameLine();
                            DrawToolTip("Teleports your character to the pinged location on the map.");

                            DrawCheckbox(ConfigManager.FlyMod, "Fly Mode", FlyPatch.SetFlying);
                            ImGui.SameLine();
                            DrawToolTip("Allows free movement in all directions while ignoring gravity.");

                            DrawCheckbox(ConfigManager.ShowPlayerMarkers, "Show Player Markers");
                            ImGui.SameLine();
                            DrawToolTip("Draws markers with the name and distance of every nearby player on your screen.");
                        }
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                        if (ImGui.CollapsingHeader("Teleport##PlayerTeleport", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            ImGui.InputFloat("X", ref Globals.teleportX);
                            ImGui.InputFloat("Y", ref Globals.teleportY);
                            ImGui.InputFloat("Z", ref Globals.teleportZ);

                            if (ImGui.Button("Teleport to coords"))
                            {
                                Logger.LogInfo($"[PeakMod] Requested to X:{Globals.teleportX} Y:{Globals.teleportY} Z:{Globals.teleportZ}");
                                Utilities.TeleportToCoords(Globals.teleportX, Globals.teleportY, Globals.teleportZ);
                            }
                        }
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                        if (ImGui.CollapsingHeader("Statuses##PlayerStatus", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            ImGui.Text("No Status:");
                            DrawCheckbox(ConfigManager.NoEat, "No Eat");
                            ImGui.SameLine();
                            DrawToolTip("You never get hungry and don't need to eat.");
                            DrawCheckbox(ConfigManager.NoInjury, "No Injury");
                            DrawCheckbox(ConfigManager.NoCold, "No Cold");
                            DrawCheckbox(ConfigManager.NoPoison, "No Poison");
                            DrawCheckbox(ConfigManager.NoCurse, "No Curse");
                            DrawCheckbox(ConfigManager.NoDrowsy, "No Drowsy");
                            DrawCheckbox(ConfigManager.NoHot, "No Heat");
                            DrawCheckbox(ConfigManager.NoSpores, "No Spores");
                            DrawCheckbox(ConfigManager.NoPetrify, "No Petrify");
                            DrawCheckbox(ConfigManager.NoRagdoll, "No Ragdoll");
                            ImGui.SameLine();
                            DrawToolTip("Prevents your character from falling over / going limp.");
                        }
                        ImGui.EndChild();
                        ImGui.Unindent();
                        ImGui.SameLine();
                        ImGui.BeginChild("PlayerDetailsColumn", new System.Numerics.Vector2(halfWidth - 10, 0), true);
                        ImGui.Indent(4.0f);
                        ImGui.Dummy(new System.Numerics.Vector2(4, 2));
                        if (ImGui.CollapsingHeader("Details", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            if (ConfigManager.JumpMod.Value)
                            {
                                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                                DrawCheckbox(ConfigManager.NoFallDmg, "No Fall Dmg");
                                DrawSliderFloat(ConfigManager.JumpAmount, "##jump_amt", 10.0f, 500.0f, "Jump Mult: %.2f");
                            }

                            if (ConfigManager.SpeedMod.Value)
                            {
                                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                                DrawSliderFloat(ConfigManager.SpeedAmount, "##speed_amt", 1.0f, 20.0f, "Move Speed: %.2f");
                            }

                            if (ConfigManager.ClimbMod.Value)
                            {
                                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                                DrawSliderFloat(ConfigManager.ClimbAmount, "##climb_amt", 1.0f, 20.0f, "Climb Speed: %.2f");
                            }

                            if (ConfigManager.VineClimbMod.Value)
                            {
                                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                                DrawSliderFloat(ConfigManager.VineClimbAmount, "##vine_climb_amt", 1.0f, 20.0f, "Vine Speed: %.2f");
                            }

                            if (ConfigManager.RopeClimbMod.Value)
                            {
                                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                                DrawSliderFloat(ConfigManager.RopeClimbAmount, "##rope_climb_amt", 1.0f, 20.0f, "Rope Speed: %.2f");
                            }
                            if (ConfigManager.FlyMod.Value)
                            {
                                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                                DrawSliderFloat(ConfigManager.FlySpeed, "##fly_speed", 10f, 100f, "Fly Speed: %.2f");
                                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                                DrawSliderFloat(ConfigManager.FlyAcceleration, "##fly_acceleration", 10f, 300f, "Fly Acceleration: %.2f");
                            }
                        }
                        ImGui.Unindent();
                        ImGui.EndChild();
                    }
                    else if (Globals.teamSubTab == 1)
                    {
                        // Team sub-tab
                        float fullWidth = ImGui.GetContentRegionAvail().X;
                        float halfWidth = fullWidth / 2f;

                        if (Globals.allPlayers.Count == 0)
                            Utilities.RefreshPlayerList();

                        // Left: Player List
                        ImGui.BeginChild("Team_PlayerList", new System.Numerics.Vector2(halfWidth, 0), true);
                        ImGui.Indent(4.0f);
                        ImGui.Dummy(new System.Numerics.Vector2(4, 2));

                        if (ImGui.CollapsingHeader("Team##TeamPlayers", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                            if (ImGui.BeginCombo("Select Teammate", Globals.teamTargetPlayer >= 0 && Globals.teamTargetPlayer < Globals.playerNames.Count
                                ? Globals.playerNames[Globals.teamTargetPlayer]
                                : "None"))
                            {
                                for (int i = 0; i < Globals.playerNames.Count; i++)
                                {
                                    bool isSelected = (Globals.teamTargetPlayer == i);
                                    if (ImGui.Selectable($"{Globals.playerNames[i]}##team_{i}", isSelected))
                                    {
                                        Globals.teamTargetPlayer = i;
                                    }
                                    if (isSelected)
                                        ImGui.SetItemDefaultFocus();
                                }
                                ImGui.EndCombo();
                            }
                            ImGui.Dummy(new System.Numerics.Vector2(4, 2));

                            ImGui.Separator();
                            ImGui.Text("All Teammates");

                            if (Globals.teamTargetPlayer >= 0 && Globals.teamTargetPlayer < Globals.allPlayers.Count)
                            {
                                Character target = Globals.allPlayers[Globals.teamTargetPlayer];
                                ImGui.Text($"Selected: {Globals.playerNames[Globals.teamTargetPlayer]}");
                            }
                            else
                            {
                                ImGui.TextDisabled("No teammate selected.");
                            }
                        }

                        ImGui.Dummy(new System.Numerics.Vector2(4, 2));
                        if (ImGui.Button("Refresh Team List"))
                            Utilities.RefreshPlayerList();

                        ImGui.Unindent();
                        ImGui.EndChild();

                        // Right: Team Actions
                        ImGui.SameLine();
                        ImGui.BeginChild("Team_Actions", new System.Numerics.Vector2(halfWidth - 10, 0), true);
                        ImGui.Indent(4.0f);
                        ImGui.Dummy(new System.Numerics.Vector2(0, 4));

                        if (ImGui.CollapsingHeader("Team Actions##TeamActions", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            if (Globals.teamTargetPlayer >= 0 && Globals.teamTargetPlayer < Globals.allPlayers.Count)
                            {
                                ImGui.Text($"Target: {Globals.playerNames[Globals.teamTargetPlayer]}");
                                ImGui.Separator();

                                // Teleport actions
                                if (ImGui.Button("Warp To Teammate"))
                                {
                                    int oldSelected = Globals.selectedPlayer;
                                    Globals.selectedPlayer = Globals.teamTargetPlayer;
                                    Utilities.WarpToSelectedPlayer();
                                    Globals.selectedPlayer = oldSelected;
                                }

                                ImGui.SameLine();
                                if (ImGui.Button("Warp Teammate To Me"))
                                {
                                    int oldSelected = Globals.selectedPlayer;
                                    Globals.selectedPlayer = Globals.teamTargetPlayer;
                                    Utilities.WarpSelectedPlayerToMe();
                                    Globals.selectedPlayer = oldSelected;
                                }

                                ImGui.Dummy(new System.Numerics.Vector2(4, 4));
                                ImGui.Separator();
                                ImGui.Text("Give Status to Teammate");

                                if (Globals.teamStatusNames.Count == 0)
                                    Utilities.RefreshHostStatuses();

                                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                                if (ImGui.BeginCombo("Status Type##team", Globals.teamStatusType >= 0 && Globals.teamStatusType < Globals.teamStatusNames.Count
                                    ? Globals.teamStatusNames[Globals.teamStatusType]
                                    : "None"))
                                {
                                    for (int i = 0; i < Globals.teamStatusNames.Count; i++)
                                    {
                                        bool isSel = Globals.teamStatusType == i;
                                        if (ImGui.Selectable($"{Globals.teamStatusNames[i]}##teamstatus_{i}", isSel))
                                        {
                                            Globals.teamStatusType = i;
                                        }
                                    }
                                    ImGui.EndCombo();
                                }

                                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                                float amt = Globals.teamStatusAmount;
                                if (ImGui.InputFloat("##team_status_amount", ref amt, 0.1f, 1f, "%.2f"))
                                    Globals.teamStatusAmount = amt;
                                ImGui.SameLine();
                                DrawToolTip("Amount of the status to apply.");

                                if (ImGui.Button("Give Status"))
                                {
                                    int oldSelected = Globals.selectedPlayer;
                                    Globals.selectedPlayer = Globals.teamTargetPlayer;
                                    if (Globals.teamStatusType >= 0 && Globals.teamStatusType < Globals.teamStatusTypes.Count)
                                    {
                                        Utilities.HostGiveStatusToSelected(
                                            (CharacterAfflictions.STATUSTYPE)Globals.teamStatusTypes[Globals.teamStatusType],
                                            Globals.teamStatusAmount);
                                    }
                                    Globals.selectedPlayer = oldSelected;
                                }
                                ImGui.SameLine();
                                DrawToolTip("Applies the selected status effect to the teammate.");

                                ImGui.Dummy(new System.Numerics.Vector2(4, 4));
                                ImGui.Separator();
                                ImGui.Text("Give Self Mods to Teammate");

                                ImGui.Dummy(new System.Numerics.Vector2(4, 2));

                                if (ImGui.Button("Infinite Stamina"))
                                    Utilities.ApplyInfiniteStaminaToPlayer(Globals.teamTargetPlayer);
                                ImGui.SameLine();
                                if (ImGui.Button("Freeze Afflictions"))
                                    Utilities.ApplyFreezeAfflictionsToPlayer(Globals.teamTargetPlayer);

                                if (ImGui.Button("Speed"))
                                    Utilities.ApplySpeedToPlayer(Globals.teamTargetPlayer);
                                ImGui.SameLine();
                                if (ImGui.Button("Jump"))
                                    Utilities.ApplyJumpToPlayer(Globals.teamTargetPlayer);

                                if (ImGui.Button("Fly Mode"))
                                    Utilities.ApplyFlyModeToPlayer(Globals.teamTargetPlayer);
                                ImGui.SameLine();
                                if (ImGui.Button("Climb"))
                                    Utilities.ApplyClimbToPlayer(Globals.teamTargetPlayer);

                                if (ImGui.Button("Clear All Statuses"))
                                    Utilities.ApplyAllStatusesToPlayer(Globals.teamTargetPlayer);

                                ImGui.Dummy(new System.Numerics.Vector2(4, 4));
                                ImGui.Separator();
                                ImGui.Text("Team Utilities");

                                if (ImGui.Button("Revive Teammate"))
                                {
                                    int oldSelected = Globals.selectedPlayer;
                                    Globals.selectedPlayer = Globals.teamTargetPlayer;
                                    Utilities.ReviveSelectedPlayer();
                                    Globals.selectedPlayer = oldSelected;
                                }

                                ImGui.SameLine();
                                if (ImGui.Button("Kill Teammate"))
                                {
                                    int oldSelected = Globals.selectedPlayer;
                                    Globals.selectedPlayer = Globals.teamTargetPlayer;
                                    Utilities.KillSelectedPlayer();
                                    Globals.selectedPlayer = oldSelected;
                                }
                            }
                            else
                            {
                                ImGui.Text("No teammate selected.");
                                ImGui.TextWrapped("Select a teammate from the list on the left to see available actions.");
                            }
                        }

                        ImGui.Unindent();
                        ImGui.EndChild();
                    }
                }
                // Items
                else if (selectedTab == 2)
                {
                    if (Globals.itemNames.Count == 0)
                    {
                        Utilities.UpdateItems();
                    }

                    List<(int slot, int itemIndex)> assignQueue = new List<(int slot, int itemIndex)>();

                    ImGui.Indent(4.0f);
                    ImGui.Dummy(new System.Numerics.Vector2(4, 2));

                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                    if (ImGui.BeginTable("InventorySlots", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                    {
                        ImGui.TableSetupColumn("Slot 1");
                        ImGui.TableSetupColumn("Slot 2");
                        ImGui.TableSetupColumn("Slot 3");
                        ImGui.TableHeadersRow();

                        ImGui.TableNextRow();

                        for (int slot = 0; slot < 3; slot++)
                        {
                            ImGui.TableSetColumnIndex(slot);
                            ImGui.PushID(slot); // Single PushID per slot

                            string currentItemName = "None";

                            if (Player.localPlayer?.itemSlots != null &&
                                Player.localPlayer.itemSlots.Length > slot &&
                                Player.localPlayer.itemSlots[slot]?.prefab != null)
                            {
                                currentItemName = Player.localPlayer.itemSlots[slot].prefab.GetName();
                            }

                            ImGui.Text($"Item {slot + 1}:");
                            ImGui.SameLine();
                            ImGui.Text(currentItemName);
                            ImGui.Spacing();

                            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);

                            // Detect if the value actually changed
                            int selected = Globals.selectedItems[slot];
                            if (DrawSearchableCombo($"##Combo{slot}", ref selected, Globals.itemNames, ref Globals.itemSearchBuffers[slot]))
                            {
                                Globals.selectedItems[slot] = selected;
                                assignQueue.Add((slot, selected));
                            }

                            ImGui.SameLine();
                            DrawToolTip("Search and assign any available item to this slot.");

                            ImGui.Spacing();

                            ConfigEntry<float> rechargeAmountConfig;
                            switch (slot)
                            {
                                case 0:
                                    rechargeAmountConfig = ConfigManager.RechargeAmountSlot1;
                                    break;
                                case 1:
                                    rechargeAmountConfig = ConfigManager.RechargeAmountSlot2;
                                    break;
                                case 2:
                                    rechargeAmountConfig = ConfigManager.RechargeAmountSlot3;
                                    break;
                                default:
                                    rechargeAmountConfig = ConfigManager.RechargeAmountSlot1;
                                    break;
                            }

                            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                            DrawSliderFloat(rechargeAmountConfig, $"##recharge_mount##{slot}", 0f, 100f, "Charge: %.1f");

                            if (ImGui.Button($"Recharge##{slot}"))
                            {
                                Utilities.RechargeInventorySlot(slot, rechargeAmountConfig.Value);
                            }
                            ImGui.SameLine();
                            DrawToolTip("Set how much to recharge the item's charges when clicking 'Recharge'.");

                            ImGui.PopID(); // Pop slot ID
                        }

                        ImGui.EndTable();
                    }

                    foreach (var (slot, itemIndex) in assignQueue)
                    {
                        Utilities.AssignInventoryItem(slot, itemIndex);
                    }

                    ImGui.Dummy(new System.Numerics.Vector2(4, 2));
                    if (ImGui.Button("Refresh Item List"))
                        Utilities.UpdateItems();
                    ImGui.SameLine();
                    DrawToolTip("Reloads the list of available items in case something was missed or updated.");

                    ImGui.Unindent();
                }
                // Spawn
                else if (selectedTab == 3)
                {
                    if (Globals.itemNames.Count == 0)
                    {
                        Utilities.UpdateItems();
                    }

                    if (Globals.allPlayers.Count == 0)
                    {
                        Utilities.RefreshPlayerList();
                    }

                    ImGui.Indent(4.0f);
                    ImGui.Dummy(new System.Numerics.Vector2(4, 2));

                    // Target selector
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                    if (ImGui.BeginCombo("Spawn Target", Globals.selectedPlayer >= 0 && Globals.selectedPlayer < Globals.playerNames.Count
                        ? Globals.playerNames[Globals.selectedPlayer]
                        : "None"))
                    {
                        for (int i = 0; i < Globals.playerNames.Count; i++)
                        {
                            bool isSel = Globals.selectedPlayer == i;
                            if (ImGui.Selectable(Globals.playerNames[i], isSel))
                            {
                                Globals.selectedPlayer = i;
                            }
                        }
                        ImGui.EndCombo();
                    }
                    DrawToolTip("Whose hand the item will be spawned into. Works for any player as a non-host.");

                    // Item picker
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                    if (ImGui.BeginCombo("Item to Spawn", Globals.selectedSpawnItem >= 0 && Globals.selectedSpawnItem < Globals.itemNames.Count
                        ? Globals.itemNames[Globals.selectedSpawnItem]
                        : "None"))
                    {
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                        ImGui.InputText("##spawn_item_search", ref Globals.spawnItemSearch, 64);
                        for (int i = 0; i < Globals.itemNames.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(Globals.spawnItemSearch) &&
                                Globals.itemNames[i].IndexOf(Globals.spawnItemSearch, System.StringComparison.OrdinalIgnoreCase) < 0)
                                continue;
                            bool isSel = Globals.selectedSpawnItem == i;
                            if (ImGui.Selectable(Globals.itemNames[i], isSel))
                            {
                                Globals.selectedSpawnItem = i;
                            }
                        }
                        ImGui.EndCombo();
                    }

                    ImGui.Dummy(new System.Numerics.Vector2(4, 4));

                    if (ImGui.Button("Spawn In Own Hand"))
                    {
                        Utilities.SpawnItemInHand(Globals.selectedSpawnItem);
                    }
                    DrawToolTip("Spawns the selected item into YOUR hand (any client).");

                    if (ImGui.Button("Spawn In Selected Hand"))
                    {
                        Utilities.SpawnItemInSelectedHand(Globals.selectedSpawnItem);
                    }
                    DrawToolTip("Spawns the selected item into the selected player's hand (any client).");
                }
                // Lobby
                else if (selectedTab == 4)
                {
                    float fullWidth = ImGui.GetContentRegionAvail().X;
                    float halfWidth = fullWidth / 2f;

                    if (Globals.allPlayers.Count == 0)
                    {
                        Utilities.RefreshPlayerList();
                    }

                    // Left: Player List
                    ImGui.BeginChild("Lobby_PlayerList", new System.Numerics.Vector2(halfWidth, 0), true);
                    ImGui.Indent(4.0f);
                    ImGui.Dummy(new System.Numerics.Vector2(4, 2));
                    if (ImGui.CollapsingHeader("Lobby Players", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);

                        if (ImGui.BeginCombo("Select Player", Globals.selectedPlayer >= 0 && Globals.selectedPlayer < Globals.playerNames.Count
                            ? Globals.playerNames[Globals.selectedPlayer]
                            : "None"))
                        {
                            for (int i = 0; i < Globals.playerNames.Count; i++)
                            {
                                bool isSelected = (Globals.selectedPlayer == i);
                                if (ImGui.Selectable($"{Globals.playerNames[i]}##{i}", isSelected))
                                {
                                    Globals.selectedPlayer = i;
                                }

                                if (isSelected)
                                    ImGui.SetItemDefaultFocus();
                            }
                            ImGui.EndCombo();
                        }
                        ImGui.Dummy(new System.Numerics.Vector2(4, 4));
                        ImGui.Separator();
                        ImGui.Text("All Players");

                        if (ImGui.Button("Revive All"))
                            Utilities.ReviveAllPlayers();

                        ImGui.SameLine();
                        if (ImGui.Button("Kill All"))
                        {
                            Utilities.KillAllPlayers();
                        }

                        bool excludeSelf = Globals.excludeSelfFromAllActions;
                        if (ImGui.Checkbox("Exclude Self from Kill All##KillAll", ref excludeSelf))
                            Globals.excludeSelfFromAllActions = excludeSelf;

                        if (ImGui.Button("Warp All To Me"))
                            Utilities.WarpAllPlayersToMe();
                    }

                    ImGui.Dummy(new System.Numerics.Vector2(4, 2));
                    if (ImGui.Button("Refresh Players List"))
                        Utilities.RefreshPlayerList();
                    ImGui.SameLine();
                    DrawToolTip("Manually reloads the list of players in case it wasn't updated automatically.");

                    ImGui.Unindent();
                    ImGui.EndChild();

                    // Right: Player Actions
                    ImGui.SameLine();
                    ImGui.BeginChild("Lobby_PlayerActions", new System.Numerics.Vector2(halfWidth - 10, 0), true);
                    ImGui.Indent(4.0f);
                    ImGui.Dummy(new System.Numerics.Vector2(0, 4));
                    if (ImGui.CollapsingHeader("Actions", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        if (Globals.selectedPlayer >= 0 && Globals.selectedPlayer < Globals.allPlayers.Count)
                        {
                            if (ImGui.Button("Revive"))
                                Utilities.ReviveSelectedPlayer();

                            ImGui.SameLine();
                            if (ImGui.Button("Kill"))
                                Utilities.KillSelectedPlayer();

                            if (ImGui.Button("Warp To"))
                                Utilities.WarpToSelectedPlayer();

                            ImGui.SameLine();
                            if (ImGui.Button("Warp To Me"))
                                Utilities.WarpSelectedPlayerToMe();

                            ImGui.Dummy(new System.Numerics.Vector2(4, 2));
                            ImGui.Separator();
                            ImGui.Text("Special Actions");

                            if (ImGui.Button("Spawn Scoutmaster"))
                            {
                                Utilities.SpawnScoutmasterForPlayer(Globals.selectedPlayer);
                            }
                            ImGui.SameLine();
                            DrawToolTip("Spawns a Scoutmaster near the selected player. Only works for host. Forces aggro.");
                        }
                        else
                        {
                            ImGui.Text("No player selected.");
                        }
                    }

                    ImGui.Unindent();
                    ImGui.EndChild();
                }
                // World
                else if (selectedTab == 5)
                {
                    float fullWidth = ImGui.GetContentRegionAvail().X;
                    float halfWidth = fullWidth / 2f;

                    Utilities.EnsureLuggageListInitialized();

                    // Left: Luggage List
                    ImGui.BeginChild("World_LuggageList", new System.Numerics.Vector2(halfWidth, 0), true);
                    ImGui.Indent(4.0f);
                    ImGui.Dummy(new System.Numerics.Vector2(4, 2));

                    if (ImGui.CollapsingHeader("Containers", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);

                        // Always show the combo, even if the list is empty
                        string selectedLabel = Globals.selectedLuggageIndex >= 0 && Globals.selectedLuggageIndex < Globals.luggageLabels.Count
                            ? Globals.luggageLabels[Globals.selectedLuggageIndex]
                            : "None";

                        if (ImGui.BeginCombo("Select Container", selectedLabel))
                        {
                            if (Globals.luggageLabels.Count > 0)
                            {
                                for (int i = 0; i < Globals.luggageLabels.Count; i++)
                                {
                                    bool isSelected = (Globals.selectedLuggageIndex == i);
                                    if (ImGui.Selectable($"{Globals.luggageLabels[i]}##{i}", isSelected))
                                    {
                                        Globals.selectedLuggageIndex = i;
                                    }

                                    if (isSelected)
                                        ImGui.SetItemDefaultFocus();
                                }
                            }
                            else
                            {
                                ImGui.TextDisabled("No containers found.");
                            }

                            ImGui.EndCombo();
                        }

                        ImGui.Dummy(new System.Numerics.Vector2(4, 2));
                        if (ImGui.Button("Refresh Luggage List"))
                        {
                            Utilities.hasInitializedLuggageList = false;
                            Utilities.RefreshLuggageList();
                        }
                        ImGui.SameLine();
                        DrawToolTip("Reloads the list of luggage within 300m of your position.");

                        ImGui.Dummy(new System.Numerics.Vector2(4, 4));
                        ImGui.Separator();
                        ImGui.Text("All Nearby Containers");

                        if (ImGui.Button("Open All Nearby"))
                        {
                            Utilities.OpenAllNearbyLuggage();
                        }
                    }

                    ImGui.Unindent();
                    ImGui.EndChild();

                    // Right: Luggage Actions
                    ImGui.SameLine();
                    ImGui.BeginChild("World_LuggageActions", new System.Numerics.Vector2(halfWidth - 10, 0), true);
                    ImGui.Indent(4.0f);
                    ImGui.Dummy(new System.Numerics.Vector2(0, 4));

                    if (ImGui.CollapsingHeader("Actions", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        if (Globals.selectedLuggageIndex >= 0 && Globals.selectedLuggageIndex < Globals.luggageLabels.Count)
                        {
                            string label = Globals.luggageLabels[Globals.selectedLuggageIndex];

                            if (ImGui.Button("Warp To Luggage"))
                            {
                                Logger.LogInfo($"[PeakMod] Warp requested for index {Globals.selectedLuggageIndex} - {label}");
                                Vector3 luggageCoords = Globals.luggageObject[Globals.selectedLuggageIndex].Center();
                                luggageCoords.y += 1.5f;

                                Utilities.TeleportToCoords(luggageCoords.x, luggageCoords.y, luggageCoords.z);
                            }

                            if (ImGui.Button("Open Luggage"))
                            {
                                Utilities.OpenLuggage(Globals.selectedLuggageIndex);
                            }
                        }
                        else
                        {
                            ImGui.Text("No luggage selected.");
                        }
                    }

                    if (ImGui.CollapsingHeader("Luggage ESP", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        DrawCheckbox(ConfigManager.LuggageESP, "Show Luggage Boxes");
                        ImGui.SameLine();
                        DrawToolTip("Draws glowing boxes around all nearby luggage on your screen.");

                        if (ConfigManager.LuggageESP.Value)
                        {
                            string colorHex = ConfigManager.LuggageESPColor.Value;
                            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                            ImGui.InputText("Box Color (hex)", ref colorHex, 6);
                            ConfigManager.LuggageESPColor.Value = colorHex;
                            DrawToolTip("RGB hex color for the ESP boxes (e.g. 00FF00 = green, FF0000 = red).");
                        }
                    }

                    ImGui.Unindent();
                    ImGui.EndChild();
                }
                // Stages
                else if (selectedTab == 6)
                {
                    ImGui.Indent(4.0f);
                    ImGui.Dummy(new System.Numerics.Vector2(4, 2));

                    if (ImGui.CollapsingHeader("Jump to Stage", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGui.TextWrapped("Teleport the lobby to any mountain stage. Works from host; otherwise the host is asked to teleport everyone.");

                        ImGui.Dummy(new System.Numerics.Vector2(4, 4));

                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 20);

                        string selectedLabel = Globals.selectedSegmentIndex >= 0 && Globals.selectedSegmentIndex < Globals.segmentNames.Length
                            ? Globals.segmentNames[Globals.selectedSegmentIndex]
                            : "None";

                        if (ImGui.BeginCombo("Select Stage", selectedLabel))
                        {
                            for (int i = 0; i < Globals.segmentNames.Length; i++)
                            {
                                bool isSelected = (Globals.selectedSegmentIndex == i);
                                if (ImGui.Selectable($"{Globals.segmentNames[i]}##{i}", isSelected))
                                {
                                    Globals.selectedSegmentIndex = i;
                                }

                                if (isSelected)
                                    ImGui.SetItemDefaultFocus();
                            }
                            ImGui.EndCombo();
                        }

                        ImGui.Dummy(new System.Numerics.Vector2(4, 4));

                        if (ImGui.Button("Jump to Selected Stage"))
                        {
                            Utilities.TeleportToSegment(Globals.selectedSegmentIndex);
                        }
                        ImGui.SameLine();
                        DrawToolTip("Teleports all players (including you) to the selected mountain stage.");

                        if (Globals.selectedSegmentIndex == 2 || Globals.selectedSegmentIndex == 5)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1.0f, 0.75f, 0.15f, 1.0f));
                            ImGui.TextWrapped("[!] NOTE: Alpine and Peak teleports drop you in the air near the spawn point. Turn on Fly Mode in the PLAYER tab if you get stuck.");
                            ImGui.PopStyleColor();
                        }

                        ImGui.Dummy(new System.Numerics.Vector2(4, 4));
                        ImGui.Separator();

                        try
                        {
                            Segment current = MapHandler.CurrentSegmentNumber;
                            ImGui.Text($"Current Stage: {current}");
                        }
                        catch
                        {
                            ImGui.Text("Current Stage: unknown");
                        }
                    }

                    ImGui.Unindent();
                }
                // Achievements
                else if (selectedTab == 7)
                {
                    // Full width badge list
                    ImGui.BeginChild("BadgeListPanel", new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X, 0), true);
                    ImGui.Indent(4.0f);
                    ImGui.Dummy(new System.Numerics.Vector2(4, 2));

                    if (ImGui.CollapsingHeader("Badges##Achievements", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGui.TextWrapped("Search the badge list and unlock or remove them.");

                        ImGui.Dummy(new System.Numerics.Vector2(4, 4));

                        if (Globals.badgeNames.Count == 0)
                            Utilities.RefreshBadges();

                        ImGui.Text("Search Badges");
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                        ImGui.InputText("##badge_search", ref Globals.badgeSearch, 100);
                        ImGui.SameLine();
                        DrawToolTip("Type to filter the badge list.");

                        ImGui.Dummy(new System.Numerics.Vector2(4, 4));

                        ImGui.BeginChild("BadgeList", new System.Numerics.Vector2(0, 200), true);
                        for (int i = 0; i < Globals.badgeNames.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(Globals.badgeSearch) &&
                                !Globals.badgeNames[i].ToLower().Contains(Globals.badgeSearch.ToLower()))
                                continue;

                            ACHIEVEMENTTYPE type = Globals.badges[i];
                            bool unlocked = Utilities.IsBadgeUnlocked(type);

                            ImGui.PushStyleColor(ImGuiCol.Text, unlocked
                                ? new System.Numerics.Vector4(0.45f, 0.75f, 0.40f, 1.0f)
                                : new System.Numerics.Vector4(0.75f, 0.75f, 0.75f, 1.0f));

                            if (ImGui.Button($"Unlock##{Globals.badgeNames[i]}"))
                            {
                                Utilities.UnlockBadge(type);
                            }
                            ImGui.SameLine();
                            ImGui.Text($"{(unlocked ? "OWNED" : "LOCKED")}  {Globals.badgeNames[i]}");
                            ImGui.PopStyleColor();
                        }
                        ImGui.EndChild();

                        ImGui.Dummy(new System.Numerics.Vector2(4, 4));

                        if (ImGui.Button("Unlock All Badges"))
                        {
                            Utilities.UnlockAllBadges();
                        }

                        ImGui.Dummy(new System.Numerics.Vector2(4, 4));
                        ImGui.Separator();
                        ImGui.Text("Ascent Level");

                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 20);
                        int ascentVal = Globals.ascentLevel;
                        if (ImGui.SliderInt("##ascent_level", ref ascentVal, 1, 8, "Ascent Level: %d"))
                            Globals.ascentLevel = ascentVal;

                        ImGui.Dummy(new System.Numerics.Vector2(4, 2));
                        if (ImGui.Button("Grant Ascent Level"))
                        {
                            Utilities.GrantAscentLevel(Globals.ascentLevel);
                        }
                        ImGui.SameLine();
                        DrawToolTip("Grants the selected ascent milestone badge for your current run.");
                    }

                    ImGui.Unindent();
                    ImGui.EndChild();

                    ImGui.Unindent();
                    ImGui.EndChild();
                }
                // Host Only
                else if (selectedTab == 8)
                {
                    if (Globals.allPlayers.Count == 0)
                    {
                        Utilities.RefreshPlayerList();
                    }

                    if (Globals.hostStatusNames.Count == 0)
                    {
                        Utilities.RefreshHostStatuses();
                    }

                    ImGui.Indent(4.0f);
                    ImGui.Dummy(new System.Numerics.Vector2(4, 2));

                    ImGui.TextWrapped("These actions only work when YOU are the session host (master client). They use server-gated RPCs.");
                    ImGui.Spacing();

                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                    if (ImGui.BeginCombo("Host Target", Globals.selectedPlayer >= 0 && Globals.selectedPlayer < Globals.playerNames.Count
                        ? Globals.playerNames[Globals.selectedPlayer]
                        : "None"))
                    {
                        for (int i = 0; i < Globals.playerNames.Count; i++)
                        {
                            bool isSel = Globals.selectedPlayer == i;
                            if (ImGui.Selectable(Globals.playerNames[i], isSel))
                            {
                                Globals.selectedPlayer = i;
                            }
                        }
                        ImGui.EndCombo();
                    }

                    ImGui.Dummy(new System.Numerics.Vector2(4, 4));

                    if (ImGui.Button("Kick Selected Player"))
                    {
                        Utilities.HostKickSelected();
                    }
                    DrawToolTip("Kicks the selected player from the session (host only).");

                    ImGui.Separator();
                    ImGui.Text("Give Status (Host)");

                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                    if (ImGui.BeginCombo("Status Type", Globals.selectedHostStatusIndex >= 0 && Globals.selectedHostStatusIndex < Globals.hostStatusNames.Count
                        ? Globals.hostStatusNames[Globals.selectedHostStatusIndex]
                        : "None"))
                    {
                        for (int i = 0; i < Globals.hostStatusNames.Count; i++)
                        {
                            bool isSel = Globals.selectedHostStatusIndex == i;
                            if (ImGui.Selectable(Globals.hostStatusNames[i], isSel))
                            {
                                Globals.selectedHostStatusIndex = i;
                            }
                        }
                        ImGui.EndCombo();
                    }

                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                    float amt = Globals.hostStatusAmount;
                    if (ImGui.InputFloat("##host_status_amount", ref amt, 1f, 5f, "%.1f"))
                        Globals.hostStatusAmount = amt;
                    ImGui.SameLine();
                    DrawToolTip("Amount of the status to add to the target.");

                    if (ImGui.Button("Give Status To Selected"))
                    {
                        Utilities.HostGiveStatusToSelected(
                            (CharacterAfflictions.STATUSTYPE)Globals.hostStatusTypes[Globals.selectedHostStatusIndex],
                            Globals.hostStatusAmount);
                    }

                    ImGui.Separator();
                    ImGui.Text("Remove Slot (Host)");

                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                    int slotIdx = Globals.hostRemoveSlot;
                    if (ImGui.SliderInt("##host_remove_slot", ref slotIdx, 0, 8, "Slot: %d"))
                        Globals.hostRemoveSlot = slotIdx;

                    if (ImGui.Button("Remove Item From Slot"))
                    {
                        Utilities.HostRemoveSlotSelected(Globals.hostRemoveSlot);
                    }
                    DrawToolTip("Removes the item in the chosen slot from the selected player's inventory (host only).");

                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                    int itemIdx2 = Globals.selectedSpawnItem;
                    if (ImGui.BeginCombo("Item to Place", itemIdx2 >= 0 && itemIdx2 < Globals.itemNames.Count
                        ? Globals.itemNames[itemIdx2]
                        : "None"))
                    {
                        for (int i = 0; i < Globals.itemNames.Count; i++)
                        {
                            bool isSel = itemIdx2 == i;
                            if (ImGui.Selectable(Globals.itemNames[i], isSel))
                            {
                                Globals.selectedSpawnItem = i;
                            }
                        }
                        ImGui.EndCombo();
                    }
                    if (ImGui.Button("Fill Slot With Item (Host)"))
                    {
                        Utilities.HostFillSelectedSlot(Globals.hostRemoveSlot, Globals.selectedSpawnItem);
                    }
                    DrawToolTip("Places the chosen item into the selected slot of the selected player's inventory (host only).");

                    ImGui.Separator();
                    ImGui.Text("Character Actions (Host)");

                    if (ImGui.Button("Pass Out Selected"))
                    {
                        Utilities.HostPassOutSelected();
                    }
                    DrawToolTip("Causes the selected player to pass out (host only).");

                    if (ImGui.Button("Zombify Selected"))
                    {
                        Utilities.HostZombifySelected();
                    }
                    DrawToolTip("Turns the selected player into a zombie (host only).");

                    ImGui.Separator();
                    ImGui.Text("Backpack (Host)");

                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                    int invSlot = 0;
                    ImGui.SliderInt("##host_inv_slot", ref invSlot, 0, 7, "Inventory Slot: %d");
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 4);
                    int packSlot = 0;
                    ImGui.SliderInt("##host_backpack_slot", ref packSlot, 0, 15, "Backpack Pocket: %d");

                    if (ImGui.Button("Move Slot To Backpack (Host)"))
                    {
                        Utilities.HostAddToBackpackSelected(invSlot, packSlot);
                    }
                    DrawToolTip("Moves the item in the chosen inventory slot to the selected player's backpack (host only).");
                }
                // Profile
                else if (selectedTab == 9)
                {
                    ImGui.Indent(4.0f);
                    ImGui.Dummy(new System.Numerics.Vector2(4, 2));

                    if (ImGui.CollapsingHeader("Player Profile", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGui.TextWrapped("Save or load all of your PLAYER tab options (self mods, statuses, fly/speed/jump settings). These options only affect YOUR character.");

                        ImGui.Dummy(new System.Numerics.Vector2(4, 4));

                        ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.20f, 0.45f, 0.25f, 1.0f));
                        if (ImGui.Button("Save Current Player Profile"))
                        {
                            Utilities.SavePlayerProfile();
                        }
                        ImGui.PopStyleColor();
                        DrawToolTip("Writes all PLAYER tab options to a profile file on disk.");

                        if (ImGui.Button("Load Saved Player Profile"))
                        {
                            Utilities.LoadPlayerProfile();
                        }
                        DrawToolTip("Restores all PLAYER tab options from the saved profile and applies them.");

                        ImGui.Dummy(new System.Numerics.Vector2(4, 4));
                        ImGui.Separator();

                        ImGui.TextWrapped("The profile is saved to: BepInEx/config/PeakModPlayerProfile.json\nOnly PLAYER tab options are stored - inventory, spawn, host and stage settings are not part of a profile.");
                    }

                    ImGui.Unindent();
                }
                // About
                else if (selectedTab == 10)
                {
                    ImGui.Indent(4.0f);
                    ImGui.Dummy(new System.Numerics.Vector2(4, 2));

                    ImGui.Text("PeakMod V0.2.0 by TheLocalAdmin");
                    ImGui.Separator();
                    ImGui.Text("Version: 0.2.0");
                    ImGui.Text("Author: TheLocalAdmin");

                    ImGui.Spacing();
                    ImGui.TextWrapped("A feature-rich quality-of-life and utility mod for PEAK: player enhancements, inventory tools, stage teleportation, badge unlocking, world manipulation, and lobby control in a clean Fullblack ImGui interface.");

                    ImGui.Spacing();
                    ImGui.Text("Key Features:");
                    ImGui.BulletText("Infinite stamina and affliction immunity");
                    ImGui.BulletText("Adjustable movement: speed, jump, and climb mods");
                    ImGui.BulletText("Real-time inventory editing and recharge");
                    ImGui.BulletText("Teleport to any mountain stage (Beach to Peak)");
                    ImGui.BulletText("Unlock all badges and grant ascent levels");
                    ImGui.BulletText("Player-to-player warp, revive, and kill tools");
                    ImGui.BulletText("Spawn any item into any player's hand (works as non-host)");
                    ImGui.BulletText("Host tab: kick, status-giving, slot editing, pass-out, zombify, backpack control");
                    ImGui.BulletText("Custom teleportation and ping-based movement");
                    ImGui.BulletText("Coordinate overlay (press M) showing all player and luggage positions");
                    ImGui.BulletText("Badge management: unlock all badges and ascent levels");
                    ImGui.BulletText("Vanish mode (press V): invisible, fly, and coordinate overlay");
                    ImGui.BulletText("Team tab: apply self mods, give status, and teleport teammates");
                    ImGui.BulletText("Luggage ESP with configurable glowing boxes");
                    ImGui.BulletText("Stylized Fullblack UI with tabbed interface");

                    ImGui.Spacing();
                    ImGui.Text("Special Thanks:");
                    ImGui.BulletText("Penswer for insight, and guidance");
                    ImGui.BulletText("BepInEx team for the modding framework");
                    ImGui.BulletText("DearImGuiInjection for seamless UI integration");
                    ImGui.BulletText("HarmonyX for runtime patching support");

                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.TextWrapped("This mod is provided as-is for educational and personal use. Not affiliated with or endorsed by the developers of PEAK. Use responsibly.");

                    ImGui.Unindent();
                }

                ImGui.EndChild();
            }

            ImGui.End();
        }
        catch (Exception ex)
        {
            ConfigManager.Logger.LogError("[UI ERROR] Exception in MyUI: " + ex);
        }
    }
}