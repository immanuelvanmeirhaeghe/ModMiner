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
            GUI.Box(new Rect(10f, 10f, 450f, 100f), "ModMiner UI", GUI.skin.window);

            GUI.Label(new Rect(30f, 30f, 200f, 20f), "How many ores per type?: ", GUI.skin.label);
            m_Count = GUI.TextField(new Rect(250f, 30f, 20f, 20f), m_Count, GUI.skin.textField);
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

            if (GUI.Button(new Rect(280f, 80f, 150f, 20f), "Get dynamite", GUI.skin.button))
            {
                OnClickGetDynamiteButton();
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
                int total = 0;

                List<ItemInfo> dynamites = itemsManager.GetAllInfosOfType(ItemType.Dynamite);
                foreach (ItemInfo dynamiteItemInfo in dynamites)
                {
                    total++;
                    itemsManager.UnlockItemInNotepad(dynamiteItemInfo.m_ID);
                    itemsManager.UnlockItemInfo(dynamiteItemInfo.m_ID.ToString());
                    for (int i = 0; i < count; i++)
                    {
                        player.AddItemToInventory(dynamiteItemInfo.m_ID.ToString());
                    }
                }

                ShowHUDBigInfo($"Added {total} x dynamite to inventory", "Mod Miner Info", HUDInfoLogTextureType.Count.ToString());
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
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(AddObsidianToInventory)}] throws exception: {exc.Message}");
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
                ModAPI.Log.Write($"[{nameof(ModMiner)}.{nameof(ModMiner)}:{nameof(AddObsidianToInventory)}] throws exception: {exc.Message}");
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
