using BepInEx.Logging;
using DearImGuiInjection.BepInEx;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zorro.Core.Serizalization;

public static class Utilities
{
    public static ManualLogSource Logger
    {
        get
        {
            if (LoggerInternal == null && ConfigManager.Logger != null)
                LoggerInternal = ConfigManager.Logger;
            return LoggerInternal;
        }
        set => LoggerInternal = value;
    }
    private static ManualLogSource LoggerInternal;

    public static void GetPlayer()
    {
        if (Globals.playerObj == null)
            Globals.playerObj = Player.localPlayer;
    }

    public static void UpdateItems()
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                Globals.items.Clear();
                Globals.itemNames.Clear();
                for (int i = 0; i < 3; i++) Globals.selectedItems[i] = -1;

                ItemDatabase db = ItemDatabase.Instance;
                if (db != null && db.Objects != null && db.Objects.Count > 0)
                {
                    foreach (var item in db.Objects)
                    {
                        if (item == null || string.IsNullOrEmpty(item.GetName()))
                            continue;
                        Globals.items.Add(item);
                        Globals.itemNames.Add(item.GetName());
                    }
                }
                else
                {
                    UnityEngine.Object[] allItems = Resources.FindObjectsOfTypeAll(typeof(Item));
                    foreach (var obj in allItems)
                    {
                        var item = obj as Item;
                        if (item != null && !string.IsNullOrEmpty(item.GetName()))
                        {
                            Globals.items.Add(item);
                            Globals.itemNames.Add(item.GetName());
                        }
                    }
                }

                var indexed = new List<(string name, int idx)>();
                for (int i = 0; i < Globals.itemNames.Count; i++)
                    indexed.Add((Globals.itemNames[i], i));
                indexed.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));

                var sortedItems = new List<Item>();
                var sortedNames = new List<string>();
                foreach (var entry in indexed)
                {
                    sortedItems.Add(Globals.items[entry.idx]);
                    sortedNames.Add(entry.name);
                }
                Globals.items = sortedItems;
                Globals.itemNames = sortedNames;

                Logger.LogInfo($"[PeakMod] Item list loaded: {Globals.itemNames.Count} items (via ItemDatabase).");
            }
            catch (Exception ex)
            {
                ConfigManager.Logger.LogError("[PeakMod] UpdateItems failed: " + ex);
            }
        });
    }

    public static void AssignInventoryItem(int slot, int itemIndex)
    {
        GetPlayer();

        if (Globals.playerObj == null)
        {
            Logger.LogError("[PeakMod] Player is null during inventory operation");
            return;
        }

        if (Globals.playerObj != null &&
            Globals.playerObj.itemSlots != null &&
            Globals.playerObj.itemSlots.Length > slot &&
            itemIndex >= 0 && itemIndex < Globals.items.Count)
        {
            int capturedItemIndex = itemIndex;
            int capturedSlot = slot;
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                var slotData = Globals.playerObj.itemSlots[capturedSlot];
                var prefab = Globals.items[capturedItemIndex];
                if (slotData == null || prefab == null)
                    return;

                slotData.SetItem(prefab, new ItemInstanceData(Guid.NewGuid()));

                ItemInstanceDataHandler.AddInstanceData(slotData.data);

                byte[] syncData = IBinarySerializable.ToManagedArray<InventorySyncData>(
                    new InventorySyncData(
                        Globals.playerObj.itemSlots,
                        Globals.playerObj.backpackSlot,
                        Globals.playerObj.tempFullSlot
                    )
                );

                Globals.playerObj.photonView.RPC("SyncInventoryRPC", RpcTarget.Others, new object[] { syncData, true });
            });
            Logger.LogInfo($"[PeakMod] Assigned {Globals.itemNames[itemIndex]} to slot {slot}");
        }
    }

    public static void RechargeInventorySlot(int slot, float rechargeValue)
    {
        GetPlayer();

        if (Globals.playerObj == null)
        {
            Logger.LogError("[PeakMod] Player is null during inventory operation");
            return;
        }

        if (Globals.playerObj != null &&
            Globals.playerObj.itemSlots != null &&
            Globals.playerObj.itemSlots.Length > slot)
        {
            int capturedSlot = slot;
            float capturedValue = rechargeValue;
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                var itemSlot = Globals.playerObj.itemSlots[capturedSlot];
                if (itemSlot?.data?.data != null)
                {
                    foreach (var kvp in itemSlot.data.data)
                    {
                        if (kvp.Key == DataEntryKey.PetterItemUses)
                        {
                            if (kvp.Value is IntItemData intData)
                            {
                                intData.Value = (int)capturedValue;
                            }
                        }
                        else if (kvp.Key == DataEntryKey.Fuel)
                        {
                            if (kvp.Value is FloatItemData floatData)
                            {
                                floatData.Value = capturedValue;
                            }
                        }
                        else if (kvp.Key == DataEntryKey.UseRemainingPercentage)
                        {
                            if (kvp.Value is FloatItemData floatData)
                            {
                                floatData.Value = capturedValue;
                            }
                        }
                        else if (kvp.Key == DataEntryKey.ItemUses)
                        {
                            if (kvp.Value is OptionableIntItemData intData)
                            {
                                intData.Value = (int)capturedValue;
                            }
                        }
                    }
                }
            });
            Logger.LogInfo($"[PeakMod] Recharged slot {slot} to {rechargeValue}");
        }
    }

    public static void RefreshPlayerList()
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                Globals.allPlayers.Clear();
                Globals.playerNames.Clear();
                Globals.selectedPlayer = -1;

                foreach (var character in Character.AllCharacters)
                {
                    Globals.allPlayers.Add(character);
                    Globals.playerNames.Add(character.characterName);
                }
                Logger.LogInfo($"[PeakMod] Found {Globals.allPlayers.Count} players.");
            }
            catch (Exception ex)
            {
                ConfigManager.Logger.LogError(ex);
            }
        });
    }

    public static void ReviveAllPlayers()
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            foreach (var character in Character.AllCharacters)
            {
                Vector3 revivePos = character.Ghost != null ? character.Ghost.transform.position : character.Head;
                character.photonView.RPC("RPCA_ReviveAtPosition", RpcTarget.All, new object[] {
                    revivePos + new Vector3(0f, 4f, 0f), true, -1
                });
            }
            Logger.LogInfo("[PeakMod] Revive All triggered.");
        });
    }

    public static void KillAllPlayers()
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            foreach (var character in Character.AllCharacters)
            {
                if (Globals.excludeSelfFromAllActions && character.IsLocal)
                    continue;

                character.photonView.RPC("RPCA_Die", RpcTarget.All, new object[0]);
            }

            Logger.LogInfo($"[PeakMod] Kill All triggered. ExcludeSelf: {Globals.excludeSelfFromAllActions}");
        });
    }

    public static void WarpAllPlayersToMe()
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            Vector3 myPos = Character.localCharacter.Head + new Vector3(0f, 4f, 0f);
            foreach (var character in Character.AllCharacters)
            {
                character.photonView.RPC("WarpPlayerRPC", RpcTarget.All, new object[] { myPos, true });
            }
            Logger.LogInfo("[PeakMod] Warp All To Me triggered.");
        });
    }


    public static void ReviveSelectedPlayer()
    {
        if (Globals.selectedPlayer < 0 || Globals.selectedPlayer >= Globals.allPlayers.Count)
            return;

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[Globals.selectedPlayer];
                Vector3 revivePos = target.Ghost != null ? target.Ghost.transform.position : target.Head;
                target.photonView.RPC("RPCA_ReviveAtPosition", RpcTarget.All, new object[] {
                    revivePos + new Vector3(0f, 4f, 0f), true, -1
                });
                Logger.LogInfo($"[PeakMod] Revive requested for player index {Globals.selectedPlayer}");
            }
            catch (Exception ex)
            {
                ConfigManager.Logger.LogError(ex);
            }
        });
    }

    public static void KillSelectedPlayer()
    {
        if (Globals.selectedPlayer < 0 || Globals.selectedPlayer >= Globals.allPlayers.Count)
            return;

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[Globals.selectedPlayer];
                target.photonView.RPC("RPCA_Die", RpcTarget.All, new object[0]);
                Logger.LogInfo($"[PeakMod] Kill requested for player index {Globals.selectedPlayer}");
            }
            catch (Exception ex)
            {
                ConfigManager.Logger.LogError(ex);
            }
        });
    }

    public static void WarpToSelectedPlayer()
    {
        if (Globals.selectedPlayer < 0 || Globals.selectedPlayer >= Globals.allPlayers.Count)
            return;

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[Globals.selectedPlayer];
                Vector3 targetPos = target.Head + new Vector3(0f, 4f, 0f);
                Character.localCharacter.photonView.RPC("WarpPlayerRPC", RpcTarget.All, new object[] {
                targetPos, true
            });
                Logger.LogInfo($"[PeakMod] Warp to requested for player index {Globals.selectedPlayer}");
            }
            catch (Exception ex)
            {
                ConfigManager.Logger.LogError(ex);
            }
        });
    }

    public static void WarpSelectedPlayerToMe()
    {
        if (Globals.selectedPlayer < 0 || Globals.selectedPlayer >= Globals.allPlayers.Count)
            return;

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[Globals.selectedPlayer];
                Vector3 myHead = Character.localCharacter.Head + new Vector3(0f, 4f, 0f);
                target.photonView.RPC("WarpPlayerRPC", RpcTarget.All, new object[] {
                myHead, true
            });
                Logger.LogInfo($"[PeakMod] Warp to me requested for player index {Globals.selectedPlayer}");
            }
            catch (Exception ex)
            {
                ConfigManager.Logger.LogError(ex);
            }
        });
    }

    public static void TeleportToCoords(float x, float y, float z)
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                Character localCharacter = Character.localCharacter;
                if (localCharacter == null || localCharacter.data.dead)
                {
                    Logger.LogWarning("[PeakMod] Local character is null or dead. Aborting teleport.");
                    return;
                }

                PhotonView photonView = localCharacter.photonView;
                if (photonView == null)
                    return;

                Vector3 target = new Vector3(x, y, z);
                photonView.RPC("WarpPlayerRPC", RpcTarget.All, new object[]
                {
                target, true
                });

                ConfigManager.Logger.LogInfo($"[PeakMod] Teleported to {target}");
            }
            catch (Exception ex)
            {
                ConfigManager.Logger.LogError("[PeakMod] Teleport Exception: " + ex);
            }
        });
    }

    public static void TeleportToSegment(int segmentIndex)
    {
        if (segmentIndex < 0 || segmentIndex >= Globals.allSegments.Length)
            return;

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                MapHandler.JumpToSegment(Globals.allSegments[segmentIndex]);
                ConfigManager.Logger.LogInfo($"[PeakMod] Jumping to segment: {Globals.segmentNames[segmentIndex]}");
            }
            catch (Exception ex)
            {
                ConfigManager.Logger.LogError("[PeakMod] TeleportToSegment Exception: " + ex);
            }
        });
    }

    public static void UnlockAllBadges()
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                AchievementManager.UnlockAll();
                ConfigManager.Logger.LogInfo("[PeakMod] Unlocked all badges.");
            }
            catch (Exception ex)
            {
                ConfigManager.Logger.LogError("[PeakMod] UnlockAllBadges Exception: " + ex);
            }
        });
    }

    public static void RefreshBadges()
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                Globals.badges.Clear();
                Globals.badgeNames.Clear();
                foreach (string badgeName in System.Enum.GetNames(typeof(ACHIEVEMENTTYPE)))
                {
                    ACHIEVEMENTTYPE type = (ACHIEVEMENTTYPE)System.Enum.Parse(typeof(ACHIEVEMENTTYPE), badgeName);
                    if (type == ACHIEVEMENTTYPE.NONE)
                        continue;
                    Globals.badges.Add(type);
                    string display = badgeName;
                    if (display.EndsWith("Badge"))
                        display = display.Substring(0, display.Length - "Badge".Length);
                    Globals.badgeNames.Add(display);
                }
            }
            catch (Exception ex)
            {
                ConfigManager.Logger.LogError("[PeakMod] RefreshBadges Exception: " + ex);
            }
        });
    }

    public static void UnlockBadge(ACHIEVEMENTTYPE type)
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                AchievementManager.Grant(type);
                ConfigManager.Logger.LogInfo($"[PeakMod] Granted badge: {type}");
            }
            catch (Exception ex)
            {
                ConfigManager.Logger.LogError("[PeakMod] UnlockBadge Exception: " + ex);
            }
        });
    }

    public static bool IsBadgeUnlocked(ACHIEVEMENTTYPE type)
    {
        try
        {
            return AchievementManager.Instance != null && AchievementManager.Instance.IsAchievementUnlocked(type);
        }
        catch
        {
            return false;
        }
    }

    public static void GrantAscentLevel(int level)
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                AchievementManager.GiveAscentLevel(level);
                ConfigManager.Logger.LogInfo($"[PeakMod] Set ascent level to {level}.");
            }
            catch (Exception ex)
            {
                ConfigManager.Logger.LogError("[PeakMod] GrantAscentLevel Exception: " + ex);
            }
        });
    }

    public static bool hasInitializedLuggageList = false;
    private static float _lastLuggageRefreshTime = -999f;

    public static void EnsureLuggageListInitialized()
    {
        if (!hasInitializedLuggageList)
        {
            hasInitializedLuggageList = true;
            RefreshLuggageList();
        }
        else if (Globals.luggageLabels.Count == 0 && Time.time - _lastLuggageRefreshTime > 2f)
        {
            RefreshLuggageList();
        }
    }


    public static void RefreshLuggageList()
    {
        _lastLuggageRefreshTime = Time.time;
        Globals.luggageLabels.Clear();
        Globals.luggageObject.Clear();
        Globals.selectedLuggageIndex = -1;

        var allLuggage = new List<(Luggage lug, float distance)>();

        foreach (var lug in Luggage.ALL_LUGGAGE)
        {
            if (lug == null) continue;

            float distance = Vector3.Distance(Character.localCharacter.Head, lug.Center());
            if (distance <= 300)
            {
                allLuggage.Add((lug, distance));
            }
        }

        allLuggage.Sort((a, b) => a.distance.CompareTo(b.distance));

        foreach (var (lug, distance) in allLuggage)
        {
            string name = lug.displayName ?? "Unnamed";
            Globals.luggageLabels.Add($"{name} [{distance:F1}m]");
            Globals.luggageObject.Add(lug);
        }

        Logger.LogInfo($"[PeakMod] Luggage refreshed. Found {Globals.luggageLabels.Count} nearby.");
    }

    public static void OpenAllNearbyLuggage()
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            int opened = 0;

            for (int i = 0; i < Globals.luggageObject.Count; i++)
            {
                var luggage = Globals.luggageObject[i];
                if (luggage == null) continue;

                var view = luggage.GetComponent<PhotonView>();
                if (view != null)
                {
                    view.RPC("OpenLuggageRPC", RpcTarget.All, new object[] { true });
                    opened++;
                }
            }

            Logger.LogInfo($"[PeakMod] Requested open for {opened} nearby containers.");
        });
    }

    public static void OpenLuggage(int index)
    {
        if (index < 0 || index >= Globals.luggageObject.Count)
            return;

        var luggage = Globals.luggageObject[index];
        if (luggage == null)
            return;

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                PhotonView view = luggage.GetComponent<PhotonView>();
                if (view != null)
                {
                    view.RPC("OpenLuggageRPC", RpcTarget.All, new object[] { true });
                    Logger.LogInfo($"[PeakMod] Sent OpenLuggageRPC for: {luggage.displayName}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"[PeakMod] Open luggage failed: {ex}");
            }
        });
    }

    public static void SpawnScoutmasterForPlayer(int playerIndex)
    {
        UnityMainThreadDispatcher.Enqueue(async () =>
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Logger.LogWarning("[PeakMod] Only the MasterClient can spawn the Scoutmaster.");
                return;
            }

            if (playerIndex < 0 || playerIndex >= Character.AllCharacters.Count)
            {
                Logger.LogWarning("[PeakMod] Invalid player index.");
                return;
            }

            Character targetCharacter = Character.AllCharacters[playerIndex];
            Vector3 targetPos = targetCharacter.transform.position;
            Vector3 spawnOrigin = targetPos + new Vector3(UnityEngine.Random.Range(-10f, 10f), 25f, UnityEngine.Random.Range(-10f, 10f));
            Vector3 down = Vector3.down;

            if (Physics.Raycast(spawnOrigin, down, out RaycastHit hit, 100f, ~0))
            {
                Vector3 spawnPoint = hit.point + Vector3.up * 1f;
                Quaternion rotation = Quaternion.identity;

                GameObject scoutObj = PhotonNetwork.InstantiateRoomObject("Character_Scoutmaster", spawnPoint, rotation, 0, null);
                var character = scoutObj.GetComponent<Character>();
                if (character != null)
                    character.data.spawnPoint = character.transform;

                await Task.Delay(100);

                var scoutmaster = scoutObj.GetComponent<Scoutmaster>();
                if (scoutmaster != null)
                {
                    try
                    {
                        var method = typeof(Scoutmaster).GetMethod("SetCurrentTarget", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (method != null)
                        {
                            method.Invoke(scoutmaster, new object[] { targetCharacter, 15f });
                            Logger.LogInfo($"[PeakMod] Scoutmaster target set to {targetCharacter.characterName}");
                        }
                        else
                        {
                            Logger.LogWarning("[PeakMod] Reflection failed - method not found.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("[PeakMod] Scoutmaster reflection error: " + ex);
                    }
                }
            }
            else
            {
                Logger.LogWarning("[PeakMod] No valid ground to spawn.");
            }
        });
    }
}