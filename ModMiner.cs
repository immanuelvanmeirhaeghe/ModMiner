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

        private static string m_Count = "1";

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
            if (Input.GetKeyDown(KeyCode.Home))
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
            GUI.Box(new Rect(10f, 10f, 380f, 100f), "ModMiner UI", GUI.skin.window);

            GUI.Label(new Rect(30f, 30f, 150f, 20f), "How many ores per type?: ", GUI.skin.label);
            m_Count = GUI.TextField(new Rect(200f, 30f, 20f, 20f), m_Count, GUI.skin.textField);
            if (GUI.Button(new Rect(230f, 30f, 150f, 20f), "Get ore stack", GUI.skin.button))
            {
                OnClickGetStackButton();
                showUI = false;
                EnableCursor(false);
            }

            if (GUI.Button(new Rect(230f, 50f, 150f, 20f), "Get gold", GUI.skin.button))
            {
                OnClickGetGoldButton();
                showUI = false;
                EnableCursor(false);
            }
        }

        public static void OnClickGetStackButton()
        {
            try
            {
                AddCharcoalToInventory(Int32.Parse(m_Count));
                AddStoneToInventory(Int32.Parse(m_Count));
                AddObsidianToInventory(Int32.Parse(m_Count));
                AddIronToInventory(Int32.Parse(m_Count));
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

        private static void AddMoneyBagToInventory(int count = 1)
        {
            itemsManager.UnlockItemInNotepad(ItemID.moneybag);
            itemsManager.UnlockItemInfo(ItemID.moneybag.ToString());
            for (int i = 0; i < count; i++)
            {
                player.AddItemToInventory(ItemID.moneybag.ToString());
            }

            ShowHUDBigInfo($"Added {count} x gold bag to inventory", "Mod Miner Info", HUDInfoLogTextureType.Count.ToString());
        }

        private static void AddIronToInventory(int count = 1)
        {
            try
            {
                itemsManager.UnlockItemInNotepad(ItemID.iron_ore_stone);
                itemsManager.UnlockItemInfo(ItemID.iron_ore_stone.ToString());
                for (int i = 0; i < count; i++)
                {
                    player.AddItemToInventory(ItemID.iron_ore_stone.ToString());
                }

                ShowHUDBigInfo($"Added {count} x iron ore to inventory", "Mod Miner Info", HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(AddIronToInventory)}] throws exception: {exc.Message}");
            }
        }

        private static void AddObsidianToInventory(int count = 1)
        {
            try
            {
                itemsManager.UnlockItemInNotepad(ItemID.Obsidian_Stone);
                itemsManager.UnlockItemInfo(ItemID.Obsidian_Stone.ToString());
                for (int i = 0; i < count; i++)
                {
                    player.AddItemToInventory(ItemID.Obsidian_Stone.ToString());
                }

                ShowHUDBigInfo($"Added {count} x obsidian ore to inventory", "Mod Miner Info", HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(AddObsidianToInventory)}] throws exception: {exc.Message}");
            }
        }

        private static void AddStoneToInventory(int count = 1)
        {
            itemsManager.UnlockItemInNotepad(ItemID.Stone);
            itemsManager.UnlockItemInfo(ItemID.Stone.ToString());
            for (int i = 0; i < count; i++)
            {
                player.AddItemToInventory(ItemID.Stone.ToString());
            }

            ShowHUDBigInfo($"Added {count} x stone to inventory", "Mod Miner Info", HUDInfoLogTextureType.Count.ToString());
        }

        private static void AddCharcoalToInventory(int count = 1)
        {
            itemsManager.UnlockItemInNotepad(ItemID.Charcoal);
            itemsManager.UnlockItemInfo(ItemID.Charcoal.ToString());
            for (int i = 0; i < count; i++)
            {
                player.AddItemToInventory(ItemID.Charcoal.ToString());
            }

            ShowHUDBigInfo($"Added {count} x charcoal to inventory", "Mod Miner Info", HUDInfoLogTextureType.Count.ToString());
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
