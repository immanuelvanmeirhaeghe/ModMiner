using Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModMiner
{
    /// <summary>
    /// ModMiner is a mod for Green Hell
    /// that allows a player to spawn charcoal, stones, obsidian, iron and gold sacks.
    /// The ores will be added to the player inventory.
	/// Enable the mod UI by pressing Home.
    /// </summary>
    public class ModMiner : MonoBehaviour
    {
        private static ModMiner s_Instance;

		private static readonly string ModName = nameof(ModMiner);

        private bool showUI = false;

        public static List<ItemID> ItemIdsToUnlock { get; set; } = new List<ItemID> { ItemID.moneybag, ItemID.iron_ore_stone, ItemID.Obsidian_Stone, ItemID.Stone, ItemID.Charcoal };

        private bool m_ItemsUnlocked = false;

        public Rect ModMinerScreen = new Rect(10f, 10f, 450f, 150f);

        private static ItemsManager itemsManager;
        private static HUDManager hUDManager;
        private static Player player;

        private static string CountStack = "1";
        private static string CountCharcoal = "1";
        private static string CountStone = "1";
        private static string CountObsidian = "1";
        private static string CountIron = "1";

        private bool _isActiveForMultiplayer;
        public bool IsModActiveForMultiplayer
        {
            get => _isActiveForMultiplayer;
            set => _isActiveForMultiplayer = FindObjectOfType(typeof(ModManager.ModManager)) != null && ModManager.ModManager.AllowModsForMultiplayer;
        }

        private bool _isActiveForSingleplayer;
        public bool IsModActiveForSingleplayer
        {
            get => _isActiveForSingleplayer;
            set => _isActiveForSingleplayer = ReplTools.AmIMaster();
        }

        public ModMiner()
        {
            useGUILayout = true;
            s_Instance = this;
        }

        public static ModMiner Get()
        {
            return s_Instance;
        }

        public void ShowHUDBigInfo(string text, string header, string textureName)
        {
            HUDBigInfo hudBigInfo = (HUDBigInfo)hUDManager.GetHUD(typeof(HUDBigInfo));
            HUDBigInfoData hudBigInfoData = new HUDBigInfoData
            {
                m_Header = header,
                m_Text = text,
                m_TextureName = textureName,
                m_ShowTime = Time.time
            };
            hudBigInfo.AddInfo(hudBigInfoData);
            hudBigInfo.Show(true);
        }

        private void InitSkinUI()
        {
            GUI.skin = ModAPI.Interface.Skin;
        }

        private void EnableCursor(bool blockPlayer = false)
        {
            CursorManager.Get().ShowCursor(blockPlayer, false);

            if (blockPlayer)
            {
                player.BlockMoves();
                player.BlockRotation();
                player.BlockInspection();
            }
            else
            {
                player.UnblockMoves();
                player.UnblockRotation();
                player.UnblockInspection();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Home))
            {
                if (!showUI)
                {
                    InitData();
                    EnableCursor(true);
                }
                showUI = !showUI;
                if (!showUI)
                {
                    EnableCursor(false);
                }
            }
        }

        private void OnGUI()
        {
            if (showUI)
            {
                InitData();
                InitSkinUI();
                InitWindow();
            }
        }

        private void UnlockItemInfos()
        {
            foreach (ItemID itemID in ItemIdsToUnlock)
            {
                itemsManager.UnlockItemInNotepad(itemID);
                itemsManager.UnlockItemInfo(itemID.ToString());
            }
            m_ItemsUnlocked = true;
        }

        private void InitWindow()
        {
            int wid = GetHashCode();
            ModMinerScreen = GUILayout.Window(wid, ModMinerScreen, InitModMinerScreen, $"{ModName}", GUI.skin.window);
        }

        private void InitData()
        {
            itemsManager = ItemsManager.Get();
            player = Player.Get();
            hUDManager = HUDManager.Get();

            if (!m_ItemsUnlocked)
            {
                UnlockItemInfos();
            }
        }

        private void InitModMinerScreen(int windowID)
        {
            using (var verticalScope = new GUILayout.VerticalScope(GUI.skin.box))
            {
                if (GUI.Button(new Rect(430f, 0f, 20f, 20f), "X", GUI.skin.button))
                {
                    CloseWindow();
                }

                using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label("How many ores per type: ", GUI.skin.label);
                    CountStack = GUILayout.TextField(CountStack, GUI.skin.textField, GUILayout.MaxWidth(50f));
                    if (GUILayout.Button("Get ore stack", GUI.skin.button, GUILayout.MinWidth(100f), GUILayout.MaxWidth(200f)))
                    {
                        OnClickGetStackButton();
                        CloseWindow();
                    }
                }

                using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    if (GUILayout.Button("Get gold", GUI.skin.button))
                    {
                        OnClickGetGoldButton();
                        CloseWindow();
                    }
                }

                using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label("How many charcoal: ", GUI.skin.label);
                    CountCharcoal = GUILayout.TextField(CountCharcoal, GUI.skin.textField, GUILayout.MaxWidth(50f));
                    if (GUILayout.Button("Get charcoal", GUI.skin.button, GUILayout.MinWidth(100f), GUILayout.MaxWidth(200f)))
                    {
                        OnClickGetCharcoalButton();
                        CloseWindow();
                    }
                }

                using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label("How many stones: ", GUI.skin.label);
                    CountStone = GUILayout.TextField(CountStone, GUI.skin.textField, GUILayout.MaxWidth(50f));
                    if (GUILayout.Button("Get stones", GUI.skin.button, GUILayout.MinWidth(100f), GUILayout.MaxWidth(200f)))
                    {
                        OnClickGetStoneButton();
                        CloseWindow();
                    }
                }

                using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label("How many obsidian: ", GUI.skin.label);
                    CountObsidian = GUILayout.TextField(CountObsidian, GUI.skin.textField, GUILayout.MaxWidth(50f));
                    if (GUILayout.Button("Get obsidian", GUI.skin.button, GUILayout.MinWidth(100f), GUILayout.MaxWidth(200f)))
                    {
                        OnClickGetObsidianButton();
                        CloseWindow();
                    }
                }

                using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label("How many iron: ", GUI.skin.label);
                    CountIron = GUILayout.TextField(CountIron, GUI.skin.textField, GUILayout.MaxWidth(50f));
                    if (GUILayout.Button("Get iron", GUI.skin.button, GUILayout.MinWidth(100f), GUILayout.MaxWidth(200f)))
                    {
                        OnClickGetIronButton();
                        CloseWindow();
                    }
                }
            }
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 10000f));
        }

        private void CloseWindow()
        {
            showUI = false;
            EnableCursor(false);
        }

        private void OnClickGetStackButton()
        {
            try
            {
                AddCharcoalToInventory(Int32.Parse(CountStack));
                AddStoneToInventory(Int32.Parse(CountStack));
                AddObsidianToInventory(Int32.Parse(CountStack));
                AddIronToInventory(Int32.Parse(CountStack));
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(OnClickGetStackButton)}] throws exception: {exc.Message}");
            }
        }

        private void OnClickGetGoldButton()
        {
            try
            {
                AddMoneyBagToInventory();
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(OnClickGetGoldButton)}] throws exception: {exc.Message}");
            }
        }

        private void OnClickGetCharcoalButton()
        {
            try
            {
                AddCharcoalToInventory(Int32.Parse(CountCharcoal));
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(OnClickGetCharcoalButton)}] throws exception: {exc.Message}");
            }
        }

        private void OnClickGetStoneButton()
        {
            try
            {
                AddStoneToInventory(Int32.Parse(CountStone));
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(OnClickGetStoneButton)}] throws exception: {exc.Message}");
            }
        }

        private void OnClickGetObsidianButton()
        {
            try
            {
                AddObsidianToInventory(Int32.Parse(CountObsidian));
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(OnClickGetObsidianButton)}] throws exception: {exc.Message}");
            }
        }

        private void OnClickGetIronButton()
        {
            try
            {
                AddIronToInventory(Int32.Parse(CountIron));
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(OnClickGetIronButton)}] throws exception: {exc.Message}");
            }
        }

        public void AddMoneyBagToInventory(int count = 1)
        {
            try
            {
                ItemInfo moneybagInfo = itemsManager.GetInfo(ItemID.moneybag);
                for (int i = 0; i < count; i++)
                {
                    player.AddItemToInventory(moneybagInfo.m_ID.ToString());
                }
                ShowHUDBigInfo(
                                    $"Added {count} x {moneybagInfo.GetNameToDisplayLocalized()} to inventory",
                                    $"{ModName} Info",
                                    HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(AddMoneyBagToInventory)}] throws exception: {exc.Message}");
            }
        }

        public void AddIronToInventory(int count = 1)
        {
            try
            {
                ItemInfo ironInfo = itemsManager.GetInfo(ItemID.iron_ore_stone);
                for (int i = 0; i < count; i++)
                {
                    player.AddItemToInventory(ironInfo.m_ID.ToString());
                }
                ShowHUDBigInfo(
                    $"Added {count} x {ironInfo.GetNameToDisplayLocalized()} to inventory",
                    $"{ModName} Info",
                    HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(AddIronToInventory)}] throws exception: {exc.Message}");
            }
        }

        public void AddObsidianToInventory(int count = 1)
        {
            try
            {
                ItemInfo obsidianInfo = itemsManager.GetInfo(ItemID.Obsidian_Stone);
                for (int i = 0; i < count; i++)
                {
                    player.AddItemToInventory(obsidianInfo.m_ID.ToString());
                }
                ShowHUDBigInfo(
                    $"Added {count} x {obsidianInfo.GetNameToDisplayLocalized()} to inventory",
                    $"{ModName} Info",
                    HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(AddObsidianToInventory)}] throws exception: {exc.Message}");
            }
        }

        public void AddStoneToInventory(int count = 1)
        {
            try
            {
                ItemInfo stoneInfo = itemsManager.GetInfo(ItemID.Stone);
                for (int i = 0; i < count; i++)
                {
                    player.AddItemToInventory(stoneInfo.m_ID.ToString());
                }
                ShowHUDBigInfo(
                    $"Added {count} x {stoneInfo.GetNameToDisplayLocalized()} to inventory",
                    $"{ModName} Info",
                    HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(AddStoneToInventory)}] throws exception: {exc.Message}");
            }
        }

        public void AddCharcoalToInventory(int count = 1)
        {
            try
            {
                ItemInfo charcoalInfo = itemsManager.GetInfo(ItemID.Charcoal);
                for (int i = 0; i < count; i++)
                {
                    player.AddItemToInventory(charcoalInfo.m_ID.ToString());
                }
                ShowHUDBigInfo(
                     $"Added {count} x {charcoalInfo.GetNameToDisplayLocalized()} to inventory",
                     $"{ModName} Info",
                     HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(AddCharcoalToInventory)}] throws exception: {exc.Message}");
            }
        }
    }
}
