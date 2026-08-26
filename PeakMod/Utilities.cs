using BepInEx;
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
                Globals.itemPrefabNames.Clear();
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
                        Globals.itemPrefabNames.Add(item.gameObject != null ? item.gameObject.name : item.GetName());
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
                            Globals.itemPrefabNames.Add(item.gameObject != null ? item.gameObject.name : item.GetName());
                        }
                    }
                }

                var indexed = new List<(string name, int idx)>();
                for (int i = 0; i < Globals.itemNames.Count; i++)
                    indexed.Add((Globals.itemNames[i], i));
                indexed.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));

                var sortedItems = new List<Item>();
                var sortedNames = new List<string>();
                var sortedPrefabs = new List<string>();
                foreach (var entry in indexed)
                {
                    sortedItems.Add(Globals.items[entry.idx]);
                    sortedNames.Add(entry.name);
                    sortedPrefabs.Add(Globals.itemPrefabNames[entry.idx]);
                }
                Globals.items = sortedItems;
                Globals.itemNames = sortedNames;
                Globals.itemPrefabNames = sortedPrefabs;

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
                Vector3 revivePos = GetSafeRevivePosition(character);
                character.photonView.RPC("RPCA_ReviveAtPosition", RpcTarget.All, new object[] {
                    revivePos, true, -1
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
                Vector3 revivePos = GetSafeRevivePosition(target);
                target.photonView.RPC("RPCA_ReviveAtPosition", RpcTarget.All, new object[] {
                    revivePos, true, -1
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
                var segment = Globals.allSegments[segmentIndex];

                try
                {
                    MapHandler.JumpToSegment(segment);
                }
                catch (Exception jumpEx)
                {
                    Logger.LogWarning("[PeakMod] JumpToSegment exception (map may still have switched): " + jumpEx);
                }

                WarpLocalToSegmentSpawn(segment);
                Logger.LogInfo($"[PeakMod] Jumping to segment: {Globals.segmentNames[segmentIndex]}");
            }
            catch (Exception ex)
            {
                ConfigManager.Logger.LogError("[PeakMod] TeleportToSegment Exception: " + ex);
            }
        });
    }

    private static void WarpLocalToSegmentSpawn(Segment segment)
    {
        Character localCharacter = Character.localCharacter;
        if (localCharacter == null || localCharacter.photonView == null)
        {
            Logger.LogWarning("[PeakMod] No local character to warp after segment jump.");
            return;
        }

        MapHandler mapHandler = UnityEngine.Object.FindFirstObjectByType<MapHandler>();
        if (mapHandler == null || mapHandler.segments == null)
        {
            Logger.LogWarning("[PeakMod] MapHandler not found, skipping relocation warp.");
            return;
        }

        Vector3 position;
        int idx = (int)segment;
        if (idx >= 5)
            idx -= 1;

        if (segment == Segment.Peak && mapHandler.respawnThePeak != null)
        {
            position = mapHandler.respawnThePeak.position;
        }
        else if (segment == Segment.Void && Peak.VoidBiome.instance != null)
        {
            position = Peak.VoidBiome.instance.GetSpawnPosition(0);
        }
        else if (idx >= 0 && idx < mapHandler.segments.Length && mapHandler.segments[idx] != null)
        {
            var ms = mapHandler.segments[idx];
            GameObject campfire = null;
            try { campfire = ms.segmentCampfire; } catch (Exception) { }
            Transform reconnectSpawn = ms.reconnectSpawnPos;

            if (campfire != null)
                position = campfire.transform.position;
            else if (reconnectSpawn != null)
                position = reconnectSpawn.position;
            else
                position = ms.segmentParent != null ? ms.segmentParent.transform.position : Vector3.zero;
        }
        else
        {
            position = Vector3.zero;
        }

        localCharacter.photonView.RPC("WarpPlayerRPC", RpcTarget.All, new object[] { position, true });
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

    private static Vector3 GetSafeRevivePosition(Character character)
    {
        Vector3 basePos = character.Ghost != null ? character.Ghost.transform.position : character.Head;

        if (Physics.Raycast(basePos + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 200f, ~0))
        {
            return hit.point + Vector3.up * 1.5f;
        }

        if (Physics.Raycast(basePos + Vector3.up * 5f, Vector3.down, out RaycastHit hit2, 50f, ~0))
        {
            return hit2.point + Vector3.up * 1.5f;
        }

        return basePos + new Vector3(0f, 2f, 0f);
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

    public static void RefreshHostStatuses()
    {
        try
        {
            if (Globals.hostStatusTypes.Count == 0)
            {
                foreach (CharacterAfflictions.STATUSTYPE type in Enum.GetValues(typeof(CharacterAfflictions.STATUSTYPE)))
                {
                    Globals.hostStatusTypes.Add((int)type);
                    Globals.hostStatusNames.Add(type.ToString());
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("[PeakMod] RefreshHostStatuses error: " + ex);
        }
    }

    public static void SpawnItemInHand(int itemIndex)
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                if (itemIndex < 0 || itemIndex >= Globals.itemPrefabNames.Count)
                    return;
                var character = Character.localCharacter;
                if (character?.refs?.items == null)
                {
                    Logger.LogWarning("[PeakMod] Local items component not found.");
                    return;
                }
                string prefabName = Globals.itemPrefabNames[itemIndex];
                character.refs.items.photonView.RPC("RPC_SpawnItemInHandMaster", RpcTarget.All, new object[] { prefabName });
                Logger.LogInfo($"[PeakMod] Spawned {Globals.itemNames[itemIndex]} in hand.");
            }
            catch (Exception ex)
            {
                Logger.LogError("[PeakMod] SpawnItemInHand Exception: " + ex);
            }
        });
    }

    public static void SpawnItemInSelectedHand(int itemIndex)
    {
        if (Globals.selectedPlayer < 0 || Globals.selectedPlayer >= Globals.allPlayers.Count)
            return;

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                if (itemIndex < 0 || itemIndex >= Globals.itemPrefabNames.Count)
                    return;
                var target = Globals.allPlayers[Globals.selectedPlayer];
                if (target?.refs?.items == null)
                {
                    Logger.LogWarning("[PeakMod] Target items component not found.");
                    return;
                }
                string prefabName = Globals.itemPrefabNames[itemIndex];
                target.refs.items.photonView.RPC("RPC_SpawnItemInHandMaster", RpcTarget.All, new object[] { prefabName });
                Logger.LogInfo($"[PeakMod] Spawned {Globals.itemNames[itemIndex]} in {Globals.playerNames[Globals.selectedPlayer]}'s hand.");
            }
            catch (Exception ex)
            {
                Logger.LogError("[PeakMod] SpawnItemInSelectedHand Exception: " + ex);
            }
        });
    }

    public static void HostKickSelected()
    {
        if (Globals.selectedPlayer < 0 || Globals.selectedPlayer >= Globals.allPlayers.Count)
            return;

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[Globals.selectedPlayer];
                var player = target?.player;
                if (player == null)
                {
                    Logger.LogWarning("[PeakMod] Target player not found for kick.");
                    return;
                }
                player.photonView.RPC("RPC_GetKicked", RpcTarget.All, new object[0]);
                Logger.LogInfo($"[PeakMod] Kick RPC sent for {Globals.playerNames[Globals.selectedPlayer]} (host only).");
            }
            catch (Exception ex)
            {
                Logger.LogError("[PeakMod] HostKickSelected Exception: " + ex);
            }
        });
    }

    public static void HostGiveStatusToSelected(CharacterAfflictions.STATUSTYPE type, float amount)
    {
        if (Globals.selectedPlayer < 0 || Globals.selectedPlayer >= Globals.allPlayers.Count)
            return;

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[Globals.selectedPlayer];
                var aff = target?.refs?.afflictions;
                if (aff == null)
                {
                    Logger.LogWarning("[PeakMod] Target afflictions not found.");
                    return;
                }

                int index = (int)type;
                int count = Enum.GetValues(typeof(CharacterAfflictions.STATUSTYPE)).Length;
                float[] statuses = new float[count];
                for (int i = 0; i < count; i++)
                    statuses[i] = aff.GetCurrentStatus((CharacterAfflictions.STATUSTYPE)i);
                statuses[index] += amount;

                target.photonView.RPC("RPC_ApplyStatusesFromFloatArray", RpcTarget.All, new object[] { statuses });
                Logger.LogInfo($"[PeakMod] Host gave {Globals.playerNames[Globals.selectedPlayer]} {amount} of {type}.");
            }
            catch (Exception ex)
            {
                Logger.LogError("[PeakMod] HostGiveStatusToSelected Exception: " + ex);
            }
        });
    }

    public static void HostRemoveSlotSelected(int slotIndex)
    {
        if (Globals.selectedPlayer < 0 || Globals.selectedPlayer >= Globals.allPlayers.Count)
            return;

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[Globals.selectedPlayer];
                var player = target?.player;
                if (player == null)
                {
                    Logger.LogWarning("[PeakMod] Target player not found for slot removal.");
                    return;
                }
                player.photonView.RPC("RPCRemoveItemFromSlot", RpcTarget.All, new object[] { (byte)slotIndex });
                Logger.LogInfo($"[PeakMod] Remove-slot RPC sent for {Globals.playerNames[Globals.selectedPlayer]} (host only), slot {slotIndex}.");
            }
            catch (Exception ex)
            {
                Logger.LogError("[PeakMod] HostRemoveSlotSelected Exception: " + ex);
            }
        });
    }

    public static void HostPassOutSelected()
    {
        if (Globals.selectedPlayer < 0 || Globals.selectedPlayer >= Globals.allPlayers.Count)
            return;

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[Globals.selectedPlayer];
                if (target == null)
                {
                    Logger.LogWarning("[PeakMod] Target character not found for pass out.");
                    return;
                }
                target.photonView.RPC("RPCA_PassOut", RpcTarget.All, new object[0]);
                Logger.LogInfo($"[PeakMod] Pass-out RPC sent for {Globals.playerNames[Globals.selectedPlayer]} (host only).");
            }
            catch (Exception ex)
            {
                Logger.LogError("[PeakMod] HostPassOutSelected Exception: " + ex);
            }
        });
    }

    public static void HostZombifySelected()
    {
        if (Globals.selectedPlayer < 0 || Globals.selectedPlayer >= Globals.allPlayers.Count)
            return;

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[Globals.selectedPlayer];
                if (target == null)
                {
                    Logger.LogWarning("[PeakMod] Target character not found for zombify.");
                    return;
                }
                target.photonView.RPC("RPCA_Zombify", RpcTarget.All, new object[0]);
                Logger.LogInfo($"[PeakMod] Zombify RPC sent for {Globals.playerNames[Globals.selectedPlayer]} (host only).");
            }
            catch (Exception ex)
            {
                Logger.LogError("[PeakMod] HostZombifySelected Exception: " + ex);
            }
        });
    }

    public static void HostAddToBackpackSelected(int inventorySlot, int backpackSlot)
    {
        if (Globals.selectedPlayer < 0 || Globals.selectedPlayer >= Globals.allPlayers.Count)
            return;

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[Globals.selectedPlayer];
                var player = target?.player;
                if (player == null)
                {
                    Logger.LogWarning("[PeakMod] Target player not found for backpack add.");
                    return;
                }
                player.photonView.RPC("RPCAddItemToCharacterBackpack", RpcTarget.All, new object[] { player.photonView, (byte)inventorySlot, (byte)backpackSlot });
                Logger.LogInfo($"[PeakMod] Backpack-add RPC sent for {Globals.playerNames[Globals.selectedPlayer]} (host only), slot {inventorySlot} -> pocket {backpackSlot}.");
            }
            catch (Exception ex)
            {
                Logger.LogError("[PeakMod] HostAddToBackpackSelected Exception: " + ex);
            }
        });
    }

    public static void HostFillSelectedSlot(int slotIndex, int itemIndex)
    {
        if (Globals.selectedPlayer < 0 || Globals.selectedPlayer >= Globals.allPlayers.Count)
            return;

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[Globals.selectedPlayer];
                var aff = target?.refs;
                var items = aff?.items;
                var player = target?.player;
                if (player == null || items == null)
                {
                    Logger.LogWarning("[PeakMod] Target player or items not found for slot fill.");
                    return;
                }
                if (itemIndex < 0 || itemIndex >= Globals.items.Count)
                    return;

                var slotData = player.itemSlots != null && slotIndex >= 0 && slotIndex < player.itemSlots.Length
                    ? player.itemSlots[slotIndex]
                    : null;
                if (slotData == null)
                {
                    Logger.LogWarning("[PeakMod] Slot not found on target.");
                    return;
                }

                Item prefab = Globals.items[itemIndex];
                slotData.SetItem(prefab, new ItemInstanceData(Guid.NewGuid()));
                ItemInstanceDataHandler.AddInstanceData(slotData.data);

                byte[] syncData = IBinarySerializable.ToManagedArray<InventorySyncData>(
                    new InventorySyncData(player.itemSlots, player.backpackSlot, player.tempFullSlot)
                );

                player.photonView.RPC("SyncInventoryRPC", RpcTarget.Others, new object[] { syncData, true });
                Logger.LogInfo($"[PeakMod] Host filled slot {slotIndex} for {Globals.playerNames[Globals.selectedPlayer]} with {Globals.itemNames[itemIndex]}.");
            }
            catch (Exception ex)
            {
                Logger.LogError("[PeakMod] HostFillSelectedSlot Exception: " + ex);
            }
        });
    }

    private static string profilePath = System.IO.Path.Combine(Paths.ConfigPath, "PeakModPlayerProfile.json");

    public static void SavePlayerProfile()
    {
        try
        {
            var data = new System.Text.StringBuilder();
            data.Append("{");
            AppendProfileFile(data, "InfiniteStamina", ConfigManager.InfiniteStamina.Value);
            AppendProfileFile(data, "LockStatus", ConfigManager.LockStatus.Value);
            AppendProfileFile(data, "NoWeight", ConfigManager.NoWeight.Value);
            AppendProfileFile(data, "NoFog", ConfigManager.NoFog.Value);
            AppendProfileFile(data, "UnlimitedItemUses", ConfigManager.UnlimitedItemUses.Value);
            AppendProfileFile(data, "SpeedMod", ConfigManager.SpeedMod.Value);
            AppendProfileFile(data, "SpeedAmount", ConfigManager.SpeedAmount.Value);
            AppendProfileFile(data, "JumpMod", ConfigManager.JumpMod.Value);
            AppendProfileFile(data, "JumpAmount", ConfigManager.JumpAmount.Value);
            AppendProfileFile(data, "NoFallDmg", ConfigManager.NoFallDmg.Value);
            AppendProfileFile(data, "ClimbMod", ConfigManager.ClimbMod.Value);
            AppendProfileFile(data, "ClimbAmount", ConfigManager.ClimbAmount.Value);
            AppendProfileFile(data, "VineClimbMod", ConfigManager.VineClimbMod.Value);
            AppendProfileFile(data, "VineClimbAmount", ConfigManager.VineClimbAmount.Value);
            AppendProfileFile(data, "RopeClimbMod", ConfigManager.RopeClimbMod.Value);
            AppendProfileFile(data, "RopeClimbAmount", ConfigManager.RopeClimbAmount.Value);
            AppendProfileFile(data, "TeleportToPing", ConfigManager.TeleportToPing.Value);
            AppendProfileFile(data, "FlyMod", ConfigManager.FlyMod.Value);
            AppendProfileFile(data, "FlySpeed", ConfigManager.FlySpeed.Value);
            AppendProfileFile(data, "FlyAcceleration", ConfigManager.FlyAcceleration.Value);
            AppendProfileFile(data, "NoEat", ConfigManager.NoEat.Value);
            AppendProfileFile(data, "NoInjury", ConfigManager.NoInjury.Value);
            AppendProfileFile(data, "NoCold", ConfigManager.NoCold.Value);
            AppendProfileFile(data, "NoPoison", ConfigManager.NoPoison.Value);
            AppendProfileFile(data, "NoHot", ConfigManager.NoHot.Value);
            AppendProfileFile(data, "NoCurse", ConfigManager.NoCurse.Value);
            AppendProfileFile(data, "NoDrowsy", ConfigManager.NoDrowsy.Value);
            AppendProfileFile(data, "NoSpores", ConfigManager.NoSpores.Value);
            AppendProfileFile(data, "NoPetrify", ConfigManager.NoPetrify.Value);
            AppendProfileFile(data, "NoRagdoll", ConfigManager.NoRagdoll.Value);

            if (data.Length > 1 && data[data.Length - 1] == ',')
                data.Length -= 1;

            data.Append("}");

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(profilePath));
            System.IO.File.WriteAllText(profilePath, data.ToString());
            Logger.LogInfo($"[PeakMod] Player profile saved to {profilePath}");
        }
        catch (Exception ex)
        {
            Logger.LogError("[PeakMod] SavePlayerProfile Exception: " + ex);
        }
    }

    public static void LoadPlayerProfile()
    {
        try
        {
            if (!System.IO.File.Exists(profilePath))
            {
                Logger.LogWarning("[PeakMod] No profile file found to load.");
                return;
            }

            string json = System.IO.File.ReadAllText(profilePath);
            System.Collections.Generic.Dictionary<string, string> map = new System.Collections.Generic.Dictionary<string, string>();
            foreach (var part in json.Trim('{', '}').Split(','))
            {
                if (string.IsNullOrEmpty(part))
                    continue;
                int eq = part.IndexOf(':');
                if (eq <= 0)
                    continue;
                string key = part.Substring(0, eq).Trim().Trim('"');
                string val = part.Substring(eq + 1).Trim().Trim('"');
                map[key] = val;
            }

            ApplyProfileBool(map, "InfiniteStamina", ConfigManager.InfiniteStamina);
            ApplyProfileBool(map, "LockStatus", ConfigManager.LockStatus);
            ApplyProfileBool(map, "NoWeight", ConfigManager.NoWeight);
            ApplyProfileBool(map, "NoFog", ConfigManager.NoFog);
            ApplyProfileBool(map, "UnlimitedItemUses", ConfigManager.UnlimitedItemUses);
            ApplyProfileBool(map, "SpeedMod", ConfigManager.SpeedMod);
            ApplyProfileFloat(map, "SpeedAmount", ConfigManager.SpeedAmount);
            ApplyProfileBool(map, "JumpMod", ConfigManager.JumpMod);
            ApplyProfileFloat(map, "JumpAmount", ConfigManager.JumpAmount);
            ApplyProfileBool(map, "NoFallDmg", ConfigManager.NoFallDmg);
            ApplyProfileBool(map, "ClimbMod", ConfigManager.ClimbMod);
            ApplyProfileFloat(map, "ClimbAmount", ConfigManager.ClimbAmount);
            ApplyProfileBool(map, "VineClimbMod", ConfigManager.VineClimbMod);
            ApplyProfileFloat(map, "VineClimbAmount", ConfigManager.VineClimbAmount);
            ApplyProfileBool(map, "RopeClimbMod", ConfigManager.RopeClimbMod);
            ApplyProfileFloat(map, "RopeClimbAmount", ConfigManager.RopeClimbAmount);
            ApplyProfileBool(map, "TeleportToPing", ConfigManager.TeleportToPing);
            ApplyProfileBool(map, "FlyMod", ConfigManager.FlyMod);
            ApplyProfileFloat(map, "FlySpeed", ConfigManager.FlySpeed);
            ApplyProfileFloat(map, "FlyAcceleration", ConfigManager.FlyAcceleration);
            ApplyProfileBool(map, "NoEat", ConfigManager.NoEat);
            ApplyProfileBool(map, "NoInjury", ConfigManager.NoInjury);
            ApplyProfileBool(map, "NoCold", ConfigManager.NoCold);
            ApplyProfileBool(map, "NoPoison", ConfigManager.NoPoison);
            ApplyProfileBool(map, "NoHot", ConfigManager.NoHot);
            ApplyProfileBool(map, "NoCurse", ConfigManager.NoCurse);
            ApplyProfileBool(map, "NoDrowsy", ConfigManager.NoDrowsy);
            ApplyProfileBool(map, "NoSpores", ConfigManager.NoSpores);
            ApplyProfileBool(map, "NoPetrify", ConfigManager.NoPetrify);
            ApplyProfileBool(map, "NoRagdoll", ConfigManager.NoRagdoll);

            Utilities.ApplyLoadedPlayerSettings();
            Logger.LogInfo("[PeakMod] Player profile loaded.");
        }
        catch (Exception ex)
        {
            Logger.LogError("[PeakMod] LoadPlayerProfile Exception: " + ex);
        }
    }

    private static void AppendProfileFile(System.Text.StringBuilder sb, string key, bool value)
    {
        sb.Append("\"").Append(key).Append("\":").Append(value ? "true" : "false").Append(",");
    }

    private static void AppendProfileFile(System.Text.StringBuilder sb, string key, float value)
    {
        sb.Append("\"").Append(key).Append("\":").Append(value.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",");
    }

    private static void ApplyProfileBool(Dictionary<string, string> map, string key, BepInEx.Configuration.ConfigEntry<bool> entry)
    {
        string raw;
        if (map.TryGetValue(key, out raw))
        {
            bool parsed;
            if (bool.TryParse(raw, out parsed))
                entry.Value = parsed;
        }
    }

    private static void ApplyProfileFloat(Dictionary<string, string> map, string key, BepInEx.Configuration.ConfigEntry<float> entry)
    {
        string raw;
        if (map.TryGetValue(key, out raw))
        {
            float parsed;
            if (float.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out parsed))
                entry.Value = parsed;
        }
    }

    public static void ApplyLoadedPlayerSettings()
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var character = GameHelpers.GetCharacterComponent();
                if (character == null)
                    return;

                var infiniteProp = ConstantFields.GetInfiniteStaminaProperty();
                if (infiniteProp != null)
                    infiniteProp.SetValue(character, ConfigManager.InfiniteStamina.Value);

                var lockProp = ConstantFields.GetStatusLockProperty();
                if (lockProp != null)
                    lockProp.SetValue(character, ConfigManager.LockStatus.Value);

                var movement = GameHelpers.GetMovementComponent();
                var speedField = ConstantFields.GetMovementModifierField();
                if (movement != null && speedField != null)
                    speedField.SetValue(movement, ConfigManager.SpeedMod.Value ? ConfigManager.SpeedAmount.Value : 1f);

                var jumpField = ConstantFields.GetJumpGravityField();
                var fallField = ConstantFields.GetFallDamageTimeField();
                if (movement != null && jumpField != null)
                    jumpField.SetValue(movement, ConfigManager.JumpMod.Value ? ConfigManager.JumpAmount.Value : 10f);
                if (movement != null && fallField != null)
                    fallField.SetValue(movement, ConfigManager.NoFallDmg.Value ? 999f : 1.5f);

                var climb = GameHelpers.GetClimbingComponent();
                var climbField = ConstantFields.GetClimbSpeedModField();
                if (climb != null && climbField != null)
                    climbField.SetValue(climb, ConfigManager.ClimbMod.Value ? ConfigManager.ClimbAmount.Value : 1f);

                var vine = GameHelpers.GetVineClimbComponent();
                var vineField = ConstantFields.GetVineClimbSpeedModField();
                if (vine != null && vineField != null)
                    vineField.SetValue(vine, ConfigManager.VineClimbMod.Value ? ConfigManager.VineClimbAmount.Value : 1f);

                var rope = GameHelpers.GetRopeClimbComponent();
                var ropeField = ConstantFields.GetRopeClimbSpeedModField();
                if (rope != null && ropeField != null)
                    ropeField.SetValue(rope, ConfigManager.RopeClimbMod.Value ? ConfigManager.RopeClimbAmount.Value : 1f);

                FlyPatch.SetFlying(ConfigManager.FlyMod.Value);
            }
            catch (Exception ex)
            {
                Logger.LogError("[PeakMod] ApplyLoadedPlayerSettings Exception: " + ex);
            }
        });
    }

    public static void ApplyFlyModeToPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= Globals.allPlayers.Count) return;
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[playerIndex];
                if (target == null) return;
                FlyPatch.SetFlying(true);
                Logger.LogInfo($"[PeakMod] Applied fly mode to {Globals.playerNames[playerIndex]}.");
            }
            catch (Exception ex) { Logger.LogError("[PeakMod] ApplyFlyModeToPlayer Exception: " + ex); }
        });
    }

    public static void ApplySpeedToPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= Globals.allPlayers.Count) return;
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[playerIndex];
                if (target == null) return;
                var movement = target.GetComponent<CharacterMovement>();
                if (movement != null)
                {
                    var field = ConstantFields.GetMovementModifierField();
                    if (field != null)
                    {
                        float speed = ConfigManager.SpeedMod.Value ? ConfigManager.SpeedAmount.Value : 1f;
                        field.SetValue(movement, speed);
                        Logger.LogInfo($"[PeakMod] Applied speed {speed} to {Globals.playerNames[playerIndex]}.");
                    }
                }
            }
            catch (Exception ex) { Logger.LogError("[PeakMod] ApplySpeedToPlayer Exception: " + ex); }
        });
    }

    public static void ApplyInfiniteStaminaToPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= Globals.allPlayers.Count) return;
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[playerIndex];
                if (target == null) return;
                var prop = ConstantFields.GetInfiniteStaminaProperty();
                if (prop != null)
                {
                    prop.SetValue(target, true);
                    Logger.LogInfo($"[PeakMod] Applied infinite stamina to {Globals.playerNames[playerIndex]}.");
                }
            }
            catch (Exception ex) { Logger.LogError("[PeakMod] ApplyInfiniteStaminaToPlayer Exception: " + ex); }
        });
    }

    public static void ApplyFreezeAfflictionsToPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= Globals.allPlayers.Count) return;
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[playerIndex];
                if (target == null) return;
                var prop = ConstantFields.GetStatusLockProperty();
                if (prop != null)
                {
                    prop.SetValue(target, true);
                    Logger.LogInfo($"[PeakMod] Applied freeze afflictions to {Globals.playerNames[playerIndex]}.");
                }
            }
            catch (Exception ex) { Logger.LogError("[PeakMod] ApplyFreezeAfflictionsToPlayer Exception: " + ex); }
        });
    }

    public static void ApplyNoWeightToPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= Globals.allPlayers.Count) return;
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[playerIndex];
                if (target == null) return;
                target.photonView.RPC("RPC_ApplyStatusesFromFloatArray", RpcTarget.All, new object[] { null });
                Logger.LogInfo($"[PeakMod] Applied no weight to {Globals.playerNames[playerIndex]}.");
            }
            catch (Exception ex) { Logger.LogError("[PeakMod] ApplyNoWeightToPlayer Exception: " + ex); }
        });
    }

    public static void ApplyJumpToPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= Globals.allPlayers.Count) return;
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[playerIndex];
                if (target == null) return;
                var movement = target.GetComponent<CharacterMovement>();
                if (movement != null)
                {
                    var jumpField = ConstantFields.GetJumpGravityField();
                    var fallField = ConstantFields.GetFallDamageTimeField();
                    if (jumpField != null)
                        jumpField.SetValue(movement, ConfigManager.JumpMod.Value ? ConfigManager.JumpAmount.Value : 10f);
                    if (fallField != null)
                        fallField.SetValue(movement, ConfigManager.NoFallDmg.Value ? 999f : 1.5f);
                    Logger.LogInfo($"[PeakMod] Applied jump settings to {Globals.playerNames[playerIndex]}.");
                }
            }
            catch (Exception ex) { Logger.LogError("[PeakMod] ApplyJumpToPlayer Exception: " + ex); }
        });
    }

    public static void ApplyClimbToPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= Globals.allPlayers.Count) return;
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[playerIndex];
                if (target == null) return;

                var climb = target.GetComponent<CharacterClimbing>();
                if (climb != null && ConfigManager.ClimbMod.Value)
                {
                    var field = ConstantFields.GetClimbSpeedModField();
                    if (field != null) field.SetValue(climb, ConfigManager.ClimbAmount.Value);
                }

                var vine = target.GetComponent<CharacterVineClimbing>();
                if (vine != null && ConfigManager.VineClimbMod.Value)
                {
                    var field = ConstantFields.GetVineClimbSpeedModField();
                    if (field != null) field.SetValue(vine, ConfigManager.VineClimbAmount.Value);
                }

                var rope = target.GetComponent<CharacterRopeHandling>();
                if (rope != null && ConfigManager.RopeClimbMod.Value)
                {
                    var field = ConstantFields.GetRopeClimbSpeedModField();
                    if (field != null) field.SetValue(rope, ConfigManager.RopeClimbAmount.Value);
                }

                Logger.LogInfo($"[PeakMod] Applied climb settings to {Globals.playerNames[playerIndex]}.");
            }
            catch (Exception ex) { Logger.LogError("[PeakMod] ApplyClimbToPlayer Exception: " + ex); }
        });
    }

    public static void ApplyAllStatusesToPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= Globals.allPlayers.Count) return;
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                var target = Globals.allPlayers[playerIndex];
                if (target == null) return;
                var aff = target?.refs?.afflictions;
                if (aff == null) return;

                int count = Enum.GetValues(typeof(CharacterAfflictions.STATUSTYPE)).Length;
                float[] statuses = new float[count];
                for (int i = 0; i < count; i++)
                    statuses[i] = aff.GetCurrentStatus((CharacterAfflictions.STATUSTYPE)i);

                statuses[(int)CharacterAfflictions.STATUSTYPE.Hunger] = 0f;
                statuses[(int)CharacterAfflictions.STATUSTYPE.Injury] = 0f;
                statuses[(int)CharacterAfflictions.STATUSTYPE.Cold] = 0f;
                statuses[(int)CharacterAfflictions.STATUSTYPE.Poison] = 0f;
                statuses[(int)CharacterAfflictions.STATUSTYPE.Hot] = 0f;
                statuses[(int)CharacterAfflictions.STATUSTYPE.Curse] = 0f;
                statuses[(int)CharacterAfflictions.STATUSTYPE.Drowsy] = 0f;
                statuses[(int)CharacterAfflictions.STATUSTYPE.Spores] = 0f;
                statuses[(int)CharacterAfflictions.STATUSTYPE.Petrify] = 0f;

                target.photonView.RPC("RPC_ApplyStatusesFromFloatArray", RpcTarget.All, new object[] { statuses });
                Logger.LogInfo($"[PeakMod] Cleared all statuses on {Globals.playerNames[playerIndex]}.");
            }
            catch (Exception ex) { Logger.LogError("[PeakMod] ApplyAllStatusesToPlayer Exception: " + ex); }
        });
    }
}