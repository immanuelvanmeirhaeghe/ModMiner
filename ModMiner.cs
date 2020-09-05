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

        private bool showUI = false;

        public static List<ItemID> ItemIdsToUnlock { get; set; } = new List<ItemID> { ItemID.moneybag, ItemID.iron_ore_stone, ItemID.Obsidian_Stone, ItemID.Stone, ItemID.Charcoal };

        private bool m_ItemsUnlocked = false;

        public Rect ModMinerScreen = new Rect(10f, 10f, 450f, 150f);

        private static ItemsManager itemsManager;
        private static HUDManager hUDManager;
        private static Player player;

        private static string m_CountStack = "1";
        private static string m_CountCharcoal = "1";
        private static string m_CountStone = "1";
        private static string m_CountObsidian = "1";
        private static string m_CountIron = "1";

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
            ModMinerScreen = GUI.Window(wid, ModMinerScreen, InitModMinerScreen, $"{nameof(ModMiner)}");
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

        private void InitModMinerScreen(int windowId)
        {
            if (GUI.Button(new Rect(440f, 10f, 20f, 20f), "X", GUI.skin.button))
            {
                CloseUi();
            }

            GUI.Label(new Rect(30f, 30f, 200f, 20f), "How many ores per type: ", GUI.skin.label);
            m_CountStack = GUI.TextField(new Rect(250f, 30f, 20f, 20f), m_CountStack, GUI.skin.textField);
            if (GUI.Button(new Rect(280f, 30f, 150f, 20f), "Get ore stack", GUI.skin.button))
            {
                OnClickGetStackButton();
                CloseUi();
            }

            if (GUI.Button(new Rect(280f, 50f, 150f, 20f), "Get gold", GUI.skin.button))
            {
                OnClickGetGoldButton();
                CloseUi();
            }

            GUI.Label(new Rect(30f, 70f, 200f, 20f), "How many charcoal: ", GUI.skin.label);
            m_CountCharcoal = GUI.TextField(new Rect(250f, 70f, 20f, 20f), m_CountCharcoal, GUI.skin.textField);
            if (GUI.Button(new Rect(280f, 70f, 150f, 20f), "Get charcoal", GUI.skin.button))
            {
                OnClickGetCharcoalButton();
                CloseUi();
            }

            GUI.Label(new Rect(30f, 90f, 200f, 20f), "How many stones: ", GUI.skin.label);
            m_CountStone = GUI.TextField(new Rect(250f, 90f, 20f, 20f), m_CountStone, GUI.skin.textField);
            if (GUI.Button(new Rect(280f, 90f, 150f, 20f), "Get stones", GUI.skin.button))
            {
                OnClickGetStoneButton();
                CloseUi();
            }

            GUI.Label(new Rect(30f, 110f, 200f, 20f), "How many obsidian: ", GUI.skin.label);
            m_CountObsidian = GUI.TextField(new Rect(250f, 110f, 20f, 20f), m_CountObsidian, GUI.skin.textField);
            if (GUI.Button(new Rect(280f, 110f, 150f, 20f), "Get obsidian", GUI.skin.button))
            {
                OnClickGetObsidianButton();
                CloseUi();
            }

            GUI.Label(new Rect(30f, 130f, 200f, 20f), "How many iron: ", GUI.skin.label);
            m_CountIron = GUI.TextField(new Rect(250f, 130f, 20f, 20f), m_CountIron, GUI.skin.textField);
            if (GUI.Button(new Rect(280f, 130f, 150f, 20f), "Get iron", GUI.skin.button))
            {
                OnClickGetIronButton();
                CloseUi();
            }

            // Make the windows be draggable.
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        private void CloseUi()
        {
            showUI = false;
            EnableCursor(false);
        }

        private void OnClickGetStackButton()
        {
            try
            {
                AddCharcoalToInventory(Int32.Parse(m_CountStack));
                AddStoneToInventory(Int32.Parse(m_CountStack));
                AddObsidianToInventory(Int32.Parse(m_CountStack));
                AddIronToInventory(Int32.Parse(m_CountStack));
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(OnClickGetStackButton)}] throws exception: {exc.Message}");
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
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(OnClickGetGoldButton)}] throws exception: {exc.Message}");
            }
        }

        private void OnClickGetCharcoalButton()
        {
            try
            {
                AddCharcoalToInventory(Int32.Parse(m_CountCharcoal));
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(OnClickGetCharcoalButton)}] throws exception: {exc.Message}");
            }
        }

        private void OnClickGetStoneButton()
        {
            try
            {
                AddStoneToInventory(Int32.Parse(m_CountStone));
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(OnClickGetStoneButton)}] throws exception: {exc.Message}");
            }
        }

        private void OnClickGetObsidianButton()
        {
            try
            {
                AddObsidianToInventory(Int32.Parse(m_CountObsidian));
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(OnClickGetObsidianButton)}] throws exception: {exc.Message}");
            }
        }

        private void OnClickGetIronButton()
        {
            try
            {
                AddIronToInventory(Int32.Parse(m_CountIron));
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(OnClickGetIronButton)}] throws exception: {exc.Message}");
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
                                    $"Added <color #{ColorUtility.ToHtmlStringRGBA(Color.cyan)}>{count}</color> x <color #{ColorUtility.ToHtmlStringRGBA(Color.magenta)}>{moneybagInfo.GetNameToDisplayLocalized()}</color> to inventory",
                                    $"{nameof(ModMiner)} Info",
                                    HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(AddMoneyBagToInventory)}] throws exception: {exc.Message}");
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
                    $"Added <color #{ColorUtility.ToHtmlStringRGBA(Color.cyan)}>{count}</color> x <color #{ColorUtility.ToHtmlStringRGBA(Color.magenta)}>{ironInfo.GetNameToDisplayLocalized()}</color> to inventory",
                    $"{nameof(ModMiner)} Info",
                    HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(AddIronToInventory)}] throws exception: {exc.Message}");
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
                    $"Added <color #{ColorUtility.ToHtmlStringRGBA(Color.cyan)}>{count}</color> x <color #{ColorUtility.ToHtmlStringRGBA(Color.magenta)}>{obsidianInfo.GetNameToDisplayLocalized()}</color> to inventory",
                    $"{nameof(ModMiner)} Info",
                    HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(AddObsidianToInventory)}] throws exception: {exc.Message}");
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
                    $"Added <color #{ColorUtility.ToHtmlStringRGBA(Color.cyan)}>{count}</color> x <color #{ColorUtility.ToHtmlStringRGBA(Color.magenta)}>{stoneInfo.GetNameToDisplayLocalized()}</color> to inventory",
                    $"{nameof(ModMiner)} Info",
                    HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(AddStoneToInventory)}] throws exception: {exc.Message}");
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
                     $"Added <color #{ColorUtility.ToHtmlStringRGBA(Color.cyan)}>{count}</color> x <color #{ColorUtility.ToHtmlStringRGBA(Color.magenta)}>{charcoalInfo.GetNameToDisplayLocalized()}</color> to inventory",
                     $"{nameof(ModMiner)}  Info",
                     HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(AddCharcoalToInventory)}] throws exception: {exc.Message}");
            }
        }
    }
}
