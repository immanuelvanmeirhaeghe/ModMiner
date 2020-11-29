using Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModMiner
{
    public enum MessageType
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// ModMiner is a mod for Green Hell
    /// that allows a player to spawn charcoal, stones, obsidian, iron and gold sacks.
    /// The ores will be added to the player inventory.
	/// Enable the mod UI by pressing Home.
    /// </summary>
    public class ModMiner : MonoBehaviour
    {
        private static ModMiner Instance;
        private static readonly string ModName = nameof(ModMiner);
        private static readonly float ModScreenTotalWidth = 500f;
        private static readonly float ModScreenTotalHeight = 150f;
        private static readonly float ModScreenMinWidth = 50f;
        private static readonly float ModScreenMaxWidth = 550f;
        private static readonly float ModScreenMinHeight = 50f;
        private static readonly float ModScreenMaxHeight = 200f;
        private static float ModScreenStartPositionX { get; set; } = (Screen.width - ModScreenMaxWidth) % ModScreenTotalWidth;
        private static float ModScreenStartPositionY { get; set; } = (Screen.height - ModScreenMaxHeight) % ModScreenTotalHeight;
        private static bool IsMinimized { get; set; } = false;
        private bool ShowUI = false;

        public static List<ItemID> MiningItemIDs { get; set; }
            = new List<ItemID> {
                ItemID.pickaxe,
                ItemID.metal_pickaxe,
                ItemID.moneybag,
                ItemID.iron_ore_stone,
                ItemID.Obsidian_Stone,
                ItemID.Stone,
                ItemID.Charcoal
            };

        private bool MiningIsUnlocked = false;
        public static Rect ModMinerScreen = new Rect(ModScreenStartPositionX, ModScreenStartPositionY, ModScreenTotalWidth, ModScreenTotalHeight);

        private static ItemsManager LocalItemsManager;
        private static HUDManager LocalHUDManager;
        private static Player LocalPlayer;

        public static string AddedToInventoryMessage(int count, ItemInfo itemInfo) => $"Added {count} x {itemInfo.GetNameToDisplayLocalized()} to inventory.";
        public static string PermissionChangedMessage(string permission) => $"Permission to use mods and cheats in multiplayer was {permission}!";
        public static string HUDBigInfoMessage(string message, MessageType messageType, Color? headcolor = null)
            => $"<color=#{ (headcolor != null ? ColorUtility.ToHtmlStringRGBA(headcolor.Value) : ColorUtility.ToHtmlStringRGBA(Color.red))  }>{messageType}</color>\n{message}";

        private static string CountStack = "1";
        private static string CountCharcoal = "1";
        private static string CountStone = "1";
        private static string CountObsidian = "1";
        private static string CountIron = "1";

        public bool IsModActiveForMultiplayer { get; private set; }
        public bool IsModActiveForSingleplayer => ReplTools.AmIMaster();

        public void Start()
        {
            ModManager.ModManager.onPermissionValueChanged += ModManager_onPermissionValueChanged;
        }

        private void ModManager_onPermissionValueChanged(bool optionValue)
        {
            IsModActiveForMultiplayer = optionValue;
            ShowHUDBigInfo(
                          (optionValue ?
                            HUDBigInfoMessage(PermissionChangedMessage($"granted"), MessageType.Info, Color.green)
                            : HUDBigInfoMessage(PermissionChangedMessage($"revoked"), MessageType.Info, Color.yellow))
                            );
        }

        public ModMiner()
        {
            useGUILayout = true;
            Instance = this;
        }

        public static ModMiner Get()
        {
            return Instance;
        }

        public void ShowHUDBigInfo(string text)
        {
            string header = $"{ModName} Info";
            string textureName = HUDInfoLogTextureType.Count.ToString();
            HUDBigInfo hudBigInfo = (HUDBigInfo)LocalHUDManager.GetHUD(typeof(HUDBigInfo));
            HUDBigInfoData.s_Duration = 2f;
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
                LocalPlayer.BlockMoves();
                LocalPlayer.BlockRotation();
                LocalPlayer.BlockInspection();
            }
            else
            {
                LocalPlayer.UnblockMoves();
                LocalPlayer.UnblockRotation();
                LocalPlayer.UnblockInspection();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Home))
            {
                if (!ShowUI)
                {
                    InitData();
                    EnableCursor(true);
                }
                ToggleShowUI();
                if (!ShowUI)
                {
                    EnableCursor(false);
                }
            }
        }

        private void ToggleShowUI()
        {
            ShowUI = !ShowUI;
        }

        private void OnGUI()
        {
            if (ShowUI)
            {
                InitData();
                InitSkinUI();
                InitWindow();
            }
        }

        private void UnlockMining()
        {
            if (!MiningIsUnlocked)
            {
                foreach (ItemID itemIdToUnlock in MiningItemIDs)
                {
                    LocalItemsManager.UnlockItemInNotepad(itemIdToUnlock);
                    LocalItemsManager.UnlockItemInfo(itemIdToUnlock.ToString());
                }
                MiningIsUnlocked = true;
            }
        }

        private void InitWindow()
        {
            int wid = GetHashCode();
            ModMinerScreen = GUILayout.Window(wid, ModMinerScreen, InitModMinerScreen, $"{ModName}", GUI.skin.window);
        }

        private void InitData()
        {
            LocalItemsManager = ItemsManager.Get();
            LocalPlayer = Player.Get();
            LocalHUDManager = HUDManager.Get();
            UnlockMining();
        }

        private void InitModMinerScreen(int windowID)
        {
            using (var modContentScope = new GUILayout.VerticalScope(
                                                                                                        GUI.skin.box,
                                                                                                        GUILayout.ExpandWidth(true),
                                                                                                        GUILayout.MinWidth(ModScreenMinWidth),
                                                                                                        GUILayout.MaxWidth(ModScreenMaxWidth),
                                                                                                        GUILayout.ExpandHeight(true),
                                                                                                        GUILayout.MinHeight(ModScreenMinHeight),
                                                                                                        GUILayout.MaxHeight(ModScreenMaxHeight)))
            {
                ScreenMenuBox();
                if (!IsMinimized)
                {
                    OresBox();
                    GoldBox();
                    CharcoalBox();
                    StonesBox();
                    ObsidianBox();
                    IronBox();
                }
            }
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 10000f));
        }

        private void ObsidianBox()
        {
            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("How many obsidian?: ", GUI.skin.label);
                CountObsidian = GUILayout.TextField(CountObsidian, GUI.skin.textField, GUILayout.MaxWidth(50f));
                if (GUILayout.Button("Get obsidian", GUI.skin.button, GUILayout.MaxWidth(200f)))
                {
                    OnClickGetObsidianButton();
                }
            }
        }

        private void StonesBox()
        {
            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("How many stones?: ", GUI.skin.label);
                CountStone = GUILayout.TextField(CountStone, GUI.skin.textField, GUILayout.MaxWidth(50f));
                if (GUILayout.Button("Get stones", GUI.skin.button, GUILayout.MaxWidth(200f)))
                {
                    OnClickGetStoneButton();
                }
            }
        }

        private void CharcoalBox()
        {
            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("How many charcoal?: ", GUI.skin.label);
                CountCharcoal = GUILayout.TextField(CountCharcoal, GUI.skin.textField, GUILayout.MaxWidth(50f));
                if (GUILayout.Button("Get charcoal", GUI.skin.button, GUILayout.MaxWidth(200f)))
                {
                    OnClickGetCharcoalButton();
                }
            }
        }

        private void GoldBox()
        {
            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                if (GUILayout.Button("Get gold", GUI.skin.button))
                {
                    OnClickGetGoldButton();
                }
            }
        }

        private void OresBox()
        {
            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("How many ores of each type?: ", GUI.skin.label);
                CountStack = GUILayout.TextField(CountStack, GUI.skin.textField, GUILayout.MaxWidth(50f));
                if (GUILayout.Button("Get ore stack", GUI.skin.button, GUILayout.MaxWidth(200f)))
                {
                    OnClickGetStackButton();
                }
            }
        }

        private void IronBox()
        {
            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("How many iron?: ", GUI.skin.label);
                CountIron = GUILayout.TextField(CountIron, GUI.skin.textField, GUILayout.MaxWidth(50f));
                if (GUILayout.Button("Get iron", GUI.skin.button, GUILayout.MaxWidth(200f)))
                {
                    OnClickGetIronButton();
                }
            }
        }

        private void ScreenMenuBox()
        {
            if (GUI.Button(new Rect(ModMinerScreen.width - 40f, 0f, 20f, 20f), "-", GUI.skin.button))
            {
                CollapseWindow();
            }

            if (GUI.Button(new Rect(ModMinerScreen.width - 20f, 0f, 20f, 20f), "X", GUI.skin.button))
            {
                CloseWindow();
            }
        }

        private void CollapseWindow()
        {
            if (!IsMinimized)
            {
                ModScreenStartPositionX = ModMinerScreen.x;
                ModScreenStartPositionY = ModMinerScreen.y;
                ModMinerScreen.Set(ModMinerScreen.x, ModMinerScreen.y, ModScreenMinWidth, ModScreenMinHeight);
                IsMinimized = true;
            }
            else
            {
                ModMinerScreen.Set(ModScreenStartPositionX, ModScreenStartPositionY, ModScreenTotalWidth, ModScreenTotalHeight);
                IsMinimized = false;
            }
            InitWindow();
        }

        private void CloseWindow()
        {
            ShowUI = false;
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
                ModAPI.Log.Write($"[{ModName}:{nameof(OnClickGetStackButton)}] throws exception: \n{exc.Message}");
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
                ModAPI.Log.Write($"[{ModName}:{nameof(OnClickGetGoldButton)}] throws exception:\n{exc.Message}");
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
                ModAPI.Log.Write($"[{ModName}:{nameof(OnClickGetCharcoalButton)}] throws exception:\n{exc.Message}");
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
                ModAPI.Log.Write($"[{ModName}:{nameof(OnClickGetStoneButton)}] throws exception:\n{exc.Message}");
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
                ModAPI.Log.Write($"[{ModName}:{nameof(OnClickGetObsidianButton)}] throws exception:\n{exc.Message}");
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
                ModAPI.Log.Write($"[{ModName}:{nameof(OnClickGetIronButton)}] throws exception:\n{exc.Message}");
            }
        }

        public void AddMoneyBagToInventory(int count = 1)
        {
            try
            {
                ItemInfo moneybagInfo = LocalItemsManager.GetInfo(ItemID.moneybag);
                for (int i = 0; i < count; i++)
                {
                    LocalPlayer.AddItemToInventory(moneybagInfo.m_ID.ToString());
                }
                ShowHUDBigInfo(HUDBigInfoMessage(AddedToInventoryMessage(count, moneybagInfo), MessageType.Info, Color.green));
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}:{nameof(AddMoneyBagToInventory)}] throws exception:\n{exc.Message}");
            }
        }

        public void AddIronToInventory(int count = 1)
        {
            try
            {
                ItemInfo ironInfo = LocalItemsManager.GetInfo(ItemID.iron_ore_stone);
                for (int i = 0; i < count; i++)
                {
                    LocalPlayer.AddItemToInventory(ironInfo.m_ID.ToString());
                }
                ShowHUDBigInfo(HUDBigInfoMessage(AddedToInventoryMessage(count, ironInfo), MessageType.Info, Color.green));
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}:{nameof(AddIronToInventory)}] throws exception:\n{exc.Message}");
            }
        }

        public void AddObsidianToInventory(int count = 1)
        {
            try
            {
                ItemInfo obsidianInfo = LocalItemsManager.GetInfo(ItemID.Obsidian_Stone);
                for (int i = 0; i < count; i++)
                {
                    LocalPlayer.AddItemToInventory(obsidianInfo.m_ID.ToString());
                }
                ShowHUDBigInfo(HUDBigInfoMessage(AddedToInventoryMessage(count, obsidianInfo), MessageType.Info, Color.green));
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}:{nameof(AddObsidianToInventory)}] throws exception:\n{exc.Message}");
            }
        }

        public void AddStoneToInventory(int count = 1)
        {
            try
            {
                ItemInfo stoneInfo = LocalItemsManager.GetInfo(ItemID.Stone);
                for (int i = 0; i < count; i++)
                {
                    LocalPlayer.AddItemToInventory(stoneInfo.m_ID.ToString());
                }
                ShowHUDBigInfo(HUDBigInfoMessage(AddedToInventoryMessage(count, stoneInfo), MessageType.Info, Color.green));
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}:{nameof(AddStoneToInventory)}] throws exception:\n{exc.Message}");
            }
        }

        public void AddCharcoalToInventory(int count = 1)
        {
            try
            {
                ItemInfo charcoalInfo = LocalItemsManager.GetInfo(ItemID.Charcoal);
                for (int i = 0; i < count; i++)
                {
                    LocalPlayer.AddItemToInventory(charcoalInfo.m_ID.ToString());
                }
                ShowHUDBigInfo(HUDBigInfoMessage(AddedToInventoryMessage(count, charcoalInfo), MessageType.Info, Color.green));
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}:{nameof(AddCharcoalToInventory)}] throws exception:\n{exc.Message}");
            }
        }
    }
}
