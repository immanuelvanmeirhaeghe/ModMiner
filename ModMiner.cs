using Enums;
using ModMiner.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace ModMiner
{
    /// <summary>
    /// ModMiner is a mod for Green Hell
    /// that allows a player to spawn charcoal, stones, obsidian, iron and gold sacks.
    /// The ores will be added to the player inventory.
    /// Press Keypad7 (default) or the key configurable in ModAPI to open the main mod screen.
    /// </summary>
    public class ModMiner : MonoBehaviour
    {
        private static ModMiner Instance;

        private static readonly string ModName = nameof(ModMiner);
        private static readonly float ModScreenTotalWidth = 500f;
        private static readonly float ModScreenTotalHeight = 150f;
        private static readonly float ModScreenMinWidth = 450f;
        private static readonly float ModScreenMaxWidth = 550f;
        private static readonly float ModScreenMinHeight = 50f;
        private static readonly float ModScreenMaxHeight = 200f;
        private static float ModScreenStartPositionX { get; set; } = Screen.width / 7f;
        private static float ModScreenStartPositionY { get; set; } = 0f;
        private static bool IsMinimized { get; set; } = false;
        private bool ShowUI = false;
        private Color DefaultGuiColor = GUI.color;

        private static bool IsScaling { get; set; } = false;

        public static List<ItemID> MiningItemIDs { get; set; }
            = new List<ItemID> {
                ItemID.pickaxe,
                ItemID.metal_pickaxe,
                ItemID.moneybag,
                ItemID.iron_ore_stone,
                ItemID.Obsidian_Stone,
                ItemID.Stone,
                ItemID.Charcoal,
                ItemID.Campfire_ash
            };

        public bool MiningUnlocked { get; set; } = false;
        public static Rect ModMinerScreen = new Rect(ModScreenStartPositionX, ModScreenStartPositionY, ModScreenTotalWidth, ModScreenTotalHeight);

        private static ItemsManager LocalItemsManager;
        private static CursorManager LocalCursorManager;
        private static HUDManager LocalHUDManager;
        private static Player LocalPlayer;

        private void HandleException(Exception exc, string methodName)
        {
            string info = $"[{ModName}:{methodName}] throws exception:\n{exc.Message}";
            ModAPI.Log.Write(info);
            ShowHUDBigInfo(HUDBigInfoMessage(info, MessageType.Error, Color.red));
        }
        public static string AlreadyUnlockedMining()
            => $"All mining items were already unlocked!";
        public static string OnlyForSinglePlayerOrHostMessage()
            => $"Only available for single player or when host. Host can activate using ModManager.";
        public static string AddedToInventoryMessage(int count, ItemInfo itemInfo)
            => $"Added {count} x {itemInfo.GetNameToDisplayLocalized()} to inventory.";
        public static string PermissionChangedMessage(string permission, string reason)
            => $"Permission to use mods and cheats in multiplayer was {permission} because {reason}.";
        public static string HUDBigInfoMessage(string message, MessageType messageType, Color? headcolor = null)
            => $"<color=#{ (headcolor != null ? ColorUtility.ToHtmlStringRGBA(headcolor.Value) : ColorUtility.ToHtmlStringRGBA(Color.red))  }>{messageType}</color>\n{message}";

        public static string CountStack { get; set; } = "1";
        public static string CountCharcoal { get; set; } = "1";
        public static string CountStone { get; set; } = "1";
        public static string CountObsidian { get; set; } = "1";
        public static string CountIron { get; set; } = "1";

        public bool IsModActiveForMultiplayer { get; private set; } = false;
        public bool IsModActiveForSingleplayer => ReplTools.AmIMaster();

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

        public void ShowHUDInfoLog(string itemID, string localizedTextKey)
        {
            var localization = GreenHellGame.Instance.GetLocalization();
            HUDMessages hUDMessages = (HUDMessages)LocalHUDManager.GetHUD(typeof(HUDMessages));
            hUDMessages.AddMessage(
                $"{localization.Get(localizedTextKey)}  {localization.Get(itemID)}"
                );
        }

        private static readonly string RuntimeConfigurationFile = Path.Combine(Application.dataPath.Replace("GH_Data", "Mods"), "RuntimeConfiguration.xml");
        private static KeyCode ModKeybindingId { get; set; } = KeyCode.Keypad7;
        private KeyCode GetConfigurableKey(string buttonId)
        {
            KeyCode configuredKeyCode = default;
            string configuredKeybinding = string.Empty;

            try
            {
                if (File.Exists(RuntimeConfigurationFile))
                {
                    using (var xmlReader = XmlReader.Create(new StreamReader(RuntimeConfigurationFile)))
                    {
                        while (xmlReader.Read())
                        {
                            if (xmlReader["ID"] == ModName)
                            {
                                if (xmlReader.ReadToFollowing(nameof(Button)) && xmlReader["ID"] == buttonId)
                                {
                                    configuredKeybinding = xmlReader.ReadElementContentAsString();
                                }
                            }
                        }
                    }
                }

                configuredKeybinding = configuredKeybinding?.Replace("NumPad", "Keypad").Replace("Oem", "");

                configuredKeyCode = (KeyCode)(!string.IsNullOrEmpty(configuredKeybinding)
                                                            ? Enum.Parse(typeof(KeyCode), configuredKeybinding)
                                                            : GetType()?.GetProperty(buttonId)?.GetValue(this));
                return configuredKeyCode;
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(GetConfigurableKey));
                configuredKeyCode = (KeyCode)(GetType()?.GetProperty(buttonId)?.GetValue(this));
                return configuredKeyCode;
            }
        }

        public void Start()
        {
            ModManager.ModManager.onPermissionValueChanged += ModManager_onPermissionValueChanged;
            ModKeybindingId = GetConfigurableKey(nameof(ModKeybindingId));
        }

        private void ModManager_onPermissionValueChanged(bool optionValue)
        {
            string reason = optionValue ? "the game host allowed usage" : "the game host did not allow usage";
            IsModActiveForMultiplayer = optionValue;

            ShowHUDBigInfo(
                          optionValue ?
                            HUDBigInfoMessage(PermissionChangedMessage($"granted", $"{reason}"), MessageType.Info, Color.green)
                            : HUDBigInfoMessage(PermissionChangedMessage($"revoked", $"{reason}"), MessageType.Info, Color.yellow)
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

        private void InitSkinUI()
        {
            GUI.skin = ModAPI.Interface.Skin;
        }

        private void EnableCursor(bool blockPlayer = false)
        {
           LocalCursorManager.ShowCursor(blockPlayer, false);

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
            if (Input.GetKeyDown(ModKeybindingId))
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
            if (!MiningUnlocked)
            {
                foreach (ItemID itemIdToUnlock in MiningItemIDs)
                {
                    LocalItemsManager.UnlockItemInNotepad(itemIdToUnlock);
                    LocalItemsManager.UnlockItemInfo(itemIdToUnlock.ToString());
                }
                MiningUnlocked = true;
            }
        }

        private void InitWindow()
        {
            int wid = GetHashCode();
            ModMinerScreen = GUILayout.Window(
                                                                                        wid,
                                                                                        ModMinerScreen,
                                                                                        InitModMinerScreen,
                                                                                        ModName,
                                                                                        GUI.skin.window,
                                                                                        GUILayout.ExpandWidth(true),
                                                                                        GUILayout.MinWidth(ModScreenMinWidth),
                                                                                        GUILayout.MaxWidth(ModScreenMaxWidth),
                                                                                        GUILayout.ExpandHeight(true),
                                                                                        GUILayout.MinHeight(ModScreenMinHeight),
                                                                                        GUILayout.MaxHeight(ModScreenMaxHeight)
                                                                                    );
        }

        private void InitData()
        {
            LocalItemsManager = ItemsManager.Get();
            LocalPlayer = Player.Get();
            LocalHUDManager = HUDManager.Get();
            LocalCursorManager = CursorManager.Get();
            UnlockMining();
        }

        private void InitModMinerScreen(int windowID)
        {
            ModScreenStartPositionX = ModMinerScreen.x;
            ModScreenStartPositionY = ModMinerScreen.y;

            using (var modContentScope = new GUILayout.VerticalScope(GUI.skin.box))
            {
                ScreenMenuBox();
                if (!IsMinimized)
                {
                    ModOptionsBox();
                    UnlockMiningItemsBox();
                }
            }
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 10000f));
        }

        private void ModOptionsBox()
        {
            if (IsModActiveForSingleplayer || IsModActiveForMultiplayer)
            {
                using (var optionsScope = new GUILayout.VerticalScope(GUI.skin.box))
                {
                    GUILayout.Label($"To toggle the mod main UI, press [{ModKeybindingId}]", GUI.skin.label);
                    MultiplayerOptionBox();
                }
            }
            else
            {
                OnlyForSingleplayerOrWhenHostBox();
            }
        }

        private void OnlyForSingleplayerOrWhenHostBox()
        {
            using (var infoScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUI.color = Color.yellow;
                GUILayout.Label(OnlyForSinglePlayerOrHostMessage(), GUI.skin.label);
            }
        }

        private void MultiplayerOptionBox()
        {
            try
            {
                using (var multiplayeroptionsScope = new GUILayout.VerticalScope(GUI.skin.box))
                {
                    GUILayout.Label("Multiplayer options: ", GUI.skin.label);
                    string multiplayerOptionMessage = string.Empty;
                    if (IsModActiveForSingleplayer || IsModActiveForMultiplayer)
                    {
                        GUI.color = Color.green;
                        if (IsModActiveForSingleplayer)
                        {
                            multiplayerOptionMessage = $"you are the game host";
                        }
                        if (IsModActiveForMultiplayer)
                        {
                            multiplayerOptionMessage = $"the game host allowed usage";
                        }
                        _ = GUILayout.Toggle(true, PermissionChangedMessage($"granted", multiplayerOptionMessage), GUI.skin.toggle);
                    }
                    else
                    {
                        GUI.color = Color.yellow;
                        if (!IsModActiveForSingleplayer)
                        {
                            multiplayerOptionMessage = $"you are not the game host";
                        }
                        if (!IsModActiveForMultiplayer)
                        {
                            multiplayerOptionMessage = $"the game host did not allow usage";
                        }
                        _ = GUILayout.Toggle(false, PermissionChangedMessage($"revoked", $"{multiplayerOptionMessage}"), GUI.skin.toggle);
                    }
                }
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(MultiplayerOptionBox));
            }
        }

        private void UnlockMiningItemsBox()
        {
            if (IsModActiveForSingleplayer || IsModActiveForMultiplayer)
            {
                using (var miningritemsScope = new GUILayout.VerticalScope(GUI.skin.box))
                {
                    GUI.color = DefaultGuiColor;
                    GUILayout.Label($"Mining tools and ores are unlocked: ", GUI.skin.label);
                    OresBox();
                    GoldBox();
                    CharcoalBox();
                    StonesBox();
                    ObsidianBox();
                    IronBox();
                }
            }
            else
            {
                OnlyForSingleplayerOrWhenHostBox();
            }
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
                ModMinerScreen = new Rect(ModScreenStartPositionX, ModScreenStartPositionY, ModScreenTotalWidth, ModScreenMinHeight);
                IsMinimized = true;
            }
            else
            {
                ModMinerScreen = new Rect(ModScreenStartPositionX, ModScreenStartPositionY, ModScreenTotalWidth, ModScreenTotalHeight);
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
                int countStack = ValidMinMax(CountStack);
                if (countStack > 0)
                {
                    AddCharcoalToInventory(countStack);
                    AddStoneToInventory(countStack);
                    AddObsidianToInventory(countStack);
                    AddIronToInventory(countStack);
                }
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(OnClickGetStackButton));
            }
        }

        private void OnClickGetGoldButton()
        {
            try
            {
                int countMoneybag = ValidMinMax("1");
                if (countMoneybag > 0)
                {
                    AddMoneyBagToInventory(countMoneybag);
                }
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(OnClickGetGoldButton));
            }
        }

        private void OnClickGetCharcoalButton()
        {
            try
            {
                int countCharcoal = ValidMinMax(CountCharcoal);
                if (countCharcoal > 0)
                {
                    AddCharcoalToInventory(countCharcoal);
                }
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(OnClickGetCharcoalButton));
            }
        }

        private void OnClickGetStoneButton()
        {
            try
            {
                int countStone = ValidMinMax(CountStone);
                if (countStone > 0)
                {
                    AddStoneToInventory(countStone);
                }
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(OnClickGetStoneButton));
            }
        }

        private void OnClickGetObsidianButton()
        {
            try
            {
                int countObsidian = ValidMinMax(CountObsidian);
                if (countObsidian > 0)
                {
                    AddObsidianToInventory(countObsidian);
                }
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(OnClickGetObsidianButton));
            }
        }

        private void OnClickGetIronButton()
        {
            try
            {
                int countIron = ValidMinMax(CountIron);
                if (countIron > 0)
                {
                    AddIronToInventory(countIron);
                }
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(OnClickGetIronButton));
            }
        }

        private int ValidMinMax(string countToValidate)
        {
            if (int.TryParse(countToValidate, out int count))
            {
                if (count <= 0)
                {
                    count = 1;
                }
                if (count > 5)
                {
                    count = 5;
                }
                return count;
            }
            else
            {
                ShowHUDBigInfo(HUDBigInfoMessage($"Invalid input {countToValidate}: please input numbers only - min. 1 and max. 5", MessageType.Error, Color.red));
                return -1;
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
                HandleException(exc, nameof(AddMoneyBagToInventory));
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
                HandleException(exc, nameof(AddIronToInventory));
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
                HandleException(exc, nameof(AddObsidianToInventory));
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
                HandleException(exc, nameof(AddStoneToInventory));
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
                HandleException(exc, nameof(AddCharcoalToInventory));
            }
        }
    }
}
