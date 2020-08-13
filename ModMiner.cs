using Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModMiner
{
    /// <summary>
    /// ModMiner
    /// </summary>
    class ModMiner : MonoBehaviour
    {
        private static ModMiner s_Instance;

        private bool showUI = false;

        private static ItemsManager itemsManager;

        private static Player player;

        public bool IsModMinerActive = false;

        private static string m_CountStack = "1";
        private static string m_CountCharcoal = "1";
        private static string m_CountStone = "1";
        private static string m_CountObsidian = "1";
        private static string m_CountIron = "1";

        /// <summary>
        /// ModAPI required security check to enable this mod feature.
        /// </summary>
        /// <returns></returns>
        public bool IsLocalOrHost => ReplTools.AmIMaster() || !ReplTools.IsCoopEnabled();

        public ModMiner()
        {
            IsModMinerActive = true;
            s_Instance = this;
        }

        public static ModMiner Get()
        {
            return s_Instance;
        }

        private void Update()
        {
            if (IsLocalOrHost && Input.GetKeyDown(KeyCode.Home))
            {
                if (!showUI)
                {
                    itemsManager = ItemsManager.Get();

                    player = Player.Get();

                    EnableCursor(true);
                }
                // toggle menu
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
                InitModUI();
            }
        }

        private static void InitData()
        {
            itemsManager = ItemsManager.Get();

            player = Player.Get();

            InitSkinUI();
        }

        private void InitModUI()
        {
            GUI.Box(new Rect(10f, 10f, 450f, 150f), "ModMiner UI - Press HOME to open/close", GUI.skin.window);

            GUI.Label(new Rect(30f, 30f, 200f, 20f), "How many ores per type?: ", GUI.skin.label);
            m_CountStack = GUI.TextField(new Rect(250f, 30f, 20f, 20f), m_CountStack, GUI.skin.textField);
            if (GUI.Button(new Rect(280f, 30f, 150f, 20f), "Get ore stack", GUI.skin.button))
            {
                OnClickGetStackButton();
                showUI = false;
                EnableCursor(false);
            }

            if (GUI.Button(new Rect(280f, 50f, 150f, 20f), "Get gold", GUI.skin.button))
            {
                OnClickGetGoldButton();
                showUI = false;
                EnableCursor(false);
            }

            GUI.Label(new Rect(30f, 70f, 200f, 20f), "How many charcoal: ", GUI.skin.label);
            m_CountCharcoal = GUI.TextField(new Rect(250f, 70f, 20f, 20f), m_CountCharcoal, GUI.skin.textField);
            if (GUI.Button(new Rect(280f, 70f, 150f, 20f), "Get charcoal", GUI.skin.button))
            {
                OnClickGetCharcoalButton();
                showUI = false;
                EnableCursor(false);
            }

            GUI.Label(new Rect(30f, 90f, 200f, 20f), "How many stones: ", GUI.skin.label);
            m_CountStone = GUI.TextField(new Rect(250f, 90f, 20f, 20f), m_CountStone, GUI.skin.textField);
            if (GUI.Button(new Rect(280f, 90f, 150f, 20f), "Get stones", GUI.skin.button))
            {
                OnClickGetStoneButton();
                showUI = false;
                EnableCursor(false);
            }

            GUI.Label(new Rect(30f, 110f, 200f, 20f), "How many obsidian: ", GUI.skin.label);
            m_CountObsidian = GUI.TextField(new Rect(250f, 110f, 20f, 20f), m_CountObsidian, GUI.skin.textField);
            if (GUI.Button(new Rect(280f, 110f, 150f, 20f), "Get obsidian", GUI.skin.button))
            {
                OnClickGetObsidianButton();
                showUI = false;
                EnableCursor(false);
            }

            GUI.Label(new Rect(30f, 130f, 200f, 20f), "How many iron: ", GUI.skin.label);
            m_CountIron = GUI.TextField(new Rect(250f, 130f, 20f, 20f), m_CountIron, GUI.skin.textField);
            if (GUI.Button(new Rect(280f, 130f, 150f, 20f), "Get iron", GUI.skin.button))
            {
                OnClickGetIronButton();
                showUI = false;
                EnableCursor(false);
            }
        }

        public static void OnClickGetStackButton()
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

        public static void OnClickGetGoldButton()
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

        public static void OnClickGetCharcoalButton()
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

        public static void OnClickGetStoneButton()
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

        public static void OnClickGetObsidianButton()
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

        public static void OnClickGetIronButton()
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

        public static void OnClickGetDynamiteButton()
        {
            try
            {
                AddDynamiteToInventory();
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(OnClickGetDynamiteButton)}] throws exception: {exc.Message}");
            }
        }

        public static void AddMoneyBagToInventory(int count = 1)
        {
            try
            {
                itemsManager.UnlockItemInNotepad(ItemID.moneybag);
                itemsManager.UnlockItemInfo(ItemID.moneybag.ToString());
                for (int i = 0; i < count; i++)
                {
                    player.AddItemToInventory(ItemID.moneybag.ToString());
                }

                ShowHUDBigInfo($"Added {count} x Gold sack to inventory", "Mod Miner Info", HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(AddMoneyBagToInventory)}] throws exception: {exc.Message}");
            }
        }

        public static void AddDynamiteToInventory(int count = 1)
        {
            try
            {
                Item detonator = itemsManager.GetInfo(ItemID.QuestConstruction_Detonator.ToString()).m_Item;
                itemsManager.UnlockItemInNotepad(detonator.m_Info.m_ID);
                itemsManager.UnlockItemInfo(detonator.m_Info.m_ID.ToString());
                for (int i = 0; i < count; i++)
                {
                    player.AddItemToInventory(detonator.m_Info.m_ID.ToString());
                }

                ShowHUDBigInfo($"Added {count} x dynamite to inventory", "Mod Miner Info", HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(AddDynamiteToInventory)}] throws exception: {exc.Message}");
            }
        }

        public static void AddIronToInventory(int count = 1)
        {
            try
            {
                itemsManager.UnlockItemInNotepad(ItemID.iron_ore_stone);
                itemsManager.UnlockItemInfo(ItemID.iron_ore_stone.ToString());
                for (int i = 0; i < count; i++)
                {
                    player.AddItemToInventory(ItemID.iron_ore_stone.ToString());
                }

                ShowHUDBigInfo($"Added {count} x {itemsManager.GetInfo(ItemID.iron_ore_stone).GetNameToDisplayLocalized()} to inventory", "Mod Miner Info", HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(AddIronToInventory)}] throws exception: {exc.Message}");
            }
        }

        public static void AddObsidianToInventory(int count = 1)
        {
            try
            {
                itemsManager.UnlockItemInNotepad(ItemID.Obsidian_Stone);
                itemsManager.UnlockItemInfo(ItemID.Obsidian_Stone.ToString());
                for (int i = 0; i < count; i++)
                {
                    player.AddItemToInventory(ItemID.Obsidian_Stone.ToString());
                }

                ShowHUDBigInfo($"Added {count} x {itemsManager.GetInfo(ItemID.Obsidian_Stone).GetNameToDisplayLocalized()} to inventory", "Mod Miner Info", HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(AddObsidianToInventory)}] throws exception: {exc.Message}");
            }
        }

        public static void AddStoneToInventory(int count = 1)
        {
            try
            {
                itemsManager.UnlockItemInNotepad(ItemID.Stone);
                itemsManager.UnlockItemInfo(ItemID.Stone.ToString());
                for (int i = 0; i < count; i++)
                {
                    player.AddItemToInventory(ItemID.Stone.ToString());
                }

                ShowHUDBigInfo($"Added {count} x {itemsManager.GetInfo(ItemID.Stone).GetNameToDisplayLocalized()} to inventory", "Mod Miner Info", HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(AddStoneToInventory)}] throws exception: {exc.Message}");
            }
        }

        public static void AddCharcoalToInventory(int count = 1)
        {
            try
            {
                itemsManager.UnlockItemInNotepad(ItemID.Charcoal);
                itemsManager.UnlockItemInfo(ItemID.Charcoal.ToString());
                for (int i = 0; i < count; i++)
                {
                    player.AddItemToInventory(ItemID.Charcoal.ToString());
                }

                ShowHUDBigInfo($"Added {count} x {itemsManager.GetInfo(ItemID.Charcoal).GetNameToDisplayLocalized()} to inventory", "Mod Miner Info", HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(AddCharcoalToInventory)}] throws exception: {exc.Message}");
            }
        }

        public static void ShowHUDBigInfo(string text, string header, string textureName)
        {
            HUDManager hUDManager = HUDManager.Get();

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

        private static void InitSkinUI()
        {
            GUI.skin = ModAPI.Interface.Skin;
        }

        private static void EnableCursor(bool enabled = false)
        {
            CursorManager.Get().ShowCursor(enabled, false);
            player = Player.Get();

            if (enabled)
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
    }
}
