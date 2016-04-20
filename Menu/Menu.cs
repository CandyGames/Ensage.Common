﻿#region LICENSE

/*
 Copyright 2014 - 2014 LeagueSharp
 Menu.cs is part of LeagueSharp.Common.
 
 LeagueSharp.Common is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.
 
 LeagueSharp.Common is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with LeagueSharp.Common. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

namespace Ensage.Common.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;

    using Ensage.Common.Extensions;
    using Ensage.Common.Menu.Draw;
    using Ensage.Common.Objects;

    using SharpDX;

    using Color = SharpDX.Color;

    /// <summary>
    ///     The menu.
    /// </summary>
    public class Menu
    {
        #region Static Fields

        /// <summary>
        ///     The root.
        /// </summary>
        public static readonly Menu Root = new Menu("Menu Settings", "Menu Settings");

        /// <summary>
        ///     The item dictionary.
        /// </summary>
        public static Dictionary<string, MenuItem> ItemDictionary;

        /// <summary>
        ///     The menu position dictionary.
        /// </summary>
        public static Dictionary<string, Vector2> menuPositionDictionary = new Dictionary<string, Vector2>();

        /// <summary>
        ///     The root menus.
        /// </summary>
        public static Dictionary<string, Menu> RootMenus = new Dictionary<string, Menu>();

        /// <summary>
        ///     The texture dictionary.
        /// </summary>
        public static Dictionary<string, DotaTexture> TextureDictionary;

        /// <summary>
        ///     The loaded.
        /// </summary>
        private static bool loaded;

        /// <summary>
        ///     The new message type.
        /// </summary>
        private static StringList newMessageType;

        #endregion

        #region Fields

        /// <summary>
        ///     The children.
        /// </summary>
        public List<Menu> Children = new List<Menu>();

        /// <summary>
        ///     The color.
        /// </summary>
        public Color Color;

        /// <summary>
        ///     The display name.
        /// </summary>
        public string DisplayName;

        /// <summary>
        ///     The is root menu.
        /// </summary>
        public bool IsRootMenu;

        /// <summary>
        ///     The items.
        /// </summary>
        public List<MenuItem> Items = new List<MenuItem>();

        /// <summary>
        ///     The name.
        /// </summary>
        public string Name;

        /// <summary>
        ///     The parent.
        /// </summary>
        public Menu Parent;

        /// <summary>
        ///     The show text with texture.
        /// </summary>
        public bool ShowTextWithTexture;

        /// <summary>
        ///     The style.
        /// </summary>
        public FontStyle Style;

        /// <summary>
        ///     The texture name.
        /// </summary>
        public string TextureName;

        /// <summary>
        ///     The cached menu count.
        /// </summary>
        private int cachedMenuCount = 2;

        /// <summary>
        ///     The unique id.
        /// </summary>
        private string uniqueId;

        /// <summary>
        ///     The visible.
        /// </summary>
        private bool visible;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="Menu" /> class.
        /// </summary>
        static Menu()
        {
            if (MenuVariables.DragAndDropDictionary == null)
            {
                MenuVariables.DragAndDropDictionary = new Dictionary<string, DragAndDrop>();
            }

            TextureDictionary = new Dictionary<string, DotaTexture>();
            ItemDictionary = new Dictionary<string, MenuItem>();
            var positionMenu = new Menu("MenuPosition", "menuPosition");
            positionMenu.AddItem(
                new MenuItem("positionX", "Position X").SetValue(
                    new Slider((int)MenuSettings.BasePosition.X, 10, Drawing.Height)));
            positionMenu.AddItem(
                new MenuItem("positionY", "Position Y").SetValue(
                    new Slider((int)MenuSettings.BasePosition.Y, (int)(HUDInfo.ScreenSizeY() * 0.06), Drawing.Width)));
            MenuSettings.BasePosition = new Vector2(
                positionMenu.Item("positionX").GetValue<Slider>().Value, 
                positionMenu.Item("positionY").GetValue<Slider>().Value);
            Root.AddSubMenu(positionMenu);
            Root.AddItem(new MenuItem("pressKey", "Menu hold key").SetValue(new KeyBind(16, KeyBindType.Press)));
            Root.AddItem(new MenuItem("toggleKey", "Menu toggle key").SetValue(new KeyBind(118, KeyBindType.Toggle)));
            Root.AddItem(new MenuItem("showMessage", "Show OnLoad message: ").SetValue(true));
            var message =
                Root.AddItem(
                    new MenuItem("messageType", "Show the message in: ").SetValue(
                        new StringList(new[] { "SideLog", "Chat", "Console" })));
            Root.AddItem(
                new MenuItem("EnsageSharp.Common.IncreaseSize", "Size increase: ").SetValue(new Slider(0, 0, 25)))
                .SetTooltip("Increases size of text and boxes");
            Root.AddItem(
                new MenuItem("EnsageSharp.Common.TooltipDuration", "Tooltip Notification Duration").SetValue(
                    new Slider(1500, 0, 5000)));
            Root.AddItem(
                new MenuItem("EnsageSharp.Common.BlockKeys", "Block player inputs for KeyBinds: ").SetValue(true));
            Root.AddItem(
                new MenuItem("FontInfo", "Press F5 after your change").SetFontStyle(FontStyle.Bold, Color.Yellow));
            loaded = false;
            newMessageType = Root.Item("messageType").GetValue<StringList>();
            CommonMenu.MenuConfig.AddSubMenu(Root);
            Events.OnLoad += Events_OnLoad;
            Events.OnClose += (sender, args) => { loaded = false; };
            message.ValueChanged += MessageValueChanged;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Menu" /> class.
        /// </summary>
        /// <param name="displayName">
        ///     The display name.
        /// </param>
        /// <param name="name">
        ///     The name.
        /// </param>
        /// <param name="isRootMenu">
        ///     The is root menu.
        /// </param>
        /// <param name="textureName">
        ///     The texture name.
        /// </param>
        /// <param name="showTextWithTexture">
        ///     The show text with texture.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public Menu(
            string displayName, 
            string name, 
            bool isRootMenu = false, 
            string textureName = null, 
            bool showTextWithTexture = false)
        {
            this.DisplayName = displayName;
            this.Name = name;
            this.IsRootMenu = isRootMenu;
            this.Style = FontStyle.Regular;
            this.Color = Color.White;
            this.TextureName = textureName;
            this.ShowTextWithTexture = showTextWithTexture;
            if (textureName != null && !TextureDictionary.ContainsKey(textureName))
            {
                if (textureName.Contains("npc_dota_hero_"))
                {
                    TextureDictionary.Add(
                        textureName, 
                        Drawing.GetTexture(
                            "materials/ensage_ui/heroes_horizontal/" + textureName.Substring("npc_dota_hero_".Length)
                            + ".vmat"));
                }
                else if (textureName.Contains("item_"))
                {
                    TextureDictionary.Add(
                        textureName, 
                        Drawing.GetTexture(
                            "materials/ensage_ui/items/" + textureName.Substring("item_".Length) + ".vmat"));
                }
                else
                {
                    TextureDictionary.Add(
                        textureName, 
                        Drawing.GetTexture("materials/ensage_ui/spellicons/" + textureName + ".vmat"));
                }
            }

            if (isRootMenu)
            {
                AppDomain.CurrentDomain.DomainUnload += delegate { this.SaveAll(); };
                AppDomain.CurrentDomain.ProcessExit += delegate { this.SaveAll(); };
                Events.OnClose += delegate { this.SaveAll(); };

                var rootName = Assembly.GetCallingAssembly().GetName().Name + "." + name;

                if (RootMenus.ContainsKey(rootName))
                {
                    throw new ArgumentException("Root Menu [" + rootName + "] with the same name exists", "name");
                }

                RootMenus.Add(rootName, this);
            }
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="Menu" /> class.
        /// </summary>
        ~Menu()
        {
            var rootName = Assembly.GetCallingAssembly().GetName().Name + "." + this.Name;
            if (RootMenus.ContainsKey(rootName))
            {
                RootMenus.Remove(rootName);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the children menu width.
        /// </summary>
        internal int ChildrenMenuWidth
        {
            get
            {
                var result = this.Children.Select(item => item.NeededWidth).Concat(new[] { 0 }).Max();

                return this.Items.Select(item => item.NeededWidth).Concat(new[] { result }).Max();
            }
        }

        /// <summary>
        ///     Gets the height.
        /// </summary>
        internal int Height
        {
            get
            {
                return MenuSettings.MenuItemHeight;
            }
        }

        /// <summary>
        ///     Gets the menu count.
        /// </summary>
        internal int MenuCount
        {
            get
            {
                var n = this.DisplayName + this.Name + "Common.Menu.CacheCount";
                if (this.Parent != null)
                {
                    n += this.Parent.Name;
                }

                if (!Utils.SleepCheck(n))
                {
                    return this.cachedMenuCount;
                }

                var globalMenuList = MenuGlobals.MenuState;
                var i = 0;
                var result = 0;

                foreach (var item in globalMenuList)
                {
                    if (item == this.uniqueId)
                    {
                        result = i;
                        break;
                    }

                    i++;
                }

                this.cachedMenuCount = result;
                Utils.Sleep(2000, n);
                return result;
            }
        }

        /// <summary>
        ///     Gets the my base position.
        /// </summary>
        internal Vector2 MyBasePosition
        {
            get
            {
                if (this.IsRootMenu || this.Parent == null)
                {
                    return MenuSettings.BasePosition + (this.MenuCount * new Vector2(0, MenuSettings.MenuItemHeight))
                           + new Vector2(5, 0);
                }

                return this.Parent.MyBasePosition + new Vector2(5, 0);
            }
        }

        /// <summary>
        ///     Gets the needed width.
        /// </summary>
        internal int NeededWidth
        {
            get
            {
                var n = this.Name + this.DisplayName + "Width";
                if (!Utils.SleepCheck(n))
                {
                    return (int)menuPositionDictionary[n].X;
                }

                var bonus = 0;
                if (this.TextureName == null || this.ShowTextWithTexture)
                {
                    bonus +=
                        (int)
                        Drawing.MeasureText(
                            MultiLanguage._(this.DisplayName), 
                            "Arial", 
                            new Vector2((float)(this.Height * 0.55), 100), 
                            FontFlags.None).X + 2;
                }

                if (this.TextureName != null)
                {
                    var tName = this.TextureName;
                    if (tName.Contains("npc_dota_hero"))
                    {
                        bonus += 15;
                    }
                    else if (tName.Contains("item_"))
                    {
                        bonus += -4;
                    }
                    else
                    {
                        bonus += -4;
                    }
                }

                var arrow = Math.Max((int)(HUDInfo.GetHpBarSizeY() * 2.5), 17);
                if (5 + arrow + bonus < (float)(MenuSettings.MenuItemWidth - (MenuSettings.MenuItemHeight * 0.3)))
                {
                    arrow = 4;
                }

                if (!menuPositionDictionary.ContainsKey(n))
                {
                    menuPositionDictionary.Add(n, new Vector2(this.Height + bonus + arrow));
                }
                else
                {
                    menuPositionDictionary[n] = new Vector2(this.Height + bonus + arrow);
                }

                Utils.Sleep(20000, n);

                return this.Height + bonus + arrow;
            }
        }

        /// <summary>
        ///     Gets the position.
        /// </summary>
        internal Vector2 Position
        {
            get
            {
                var n = this.Name + this.DisplayName;
                if (this.Parent != null)
                {
                    n += this.Parent.Name;
                }

                if (!Utils.SleepCheck(n))
                {
                    return menuPositionDictionary[n];
                }

                int xOffset;

                if (this.Parent != null)
                {
                    xOffset = (int)(this.Parent.Position.X + this.Parent.Width + 1);
                }
                else
                {
                    xOffset = (int)this.MyBasePosition.X;
                }

                var pos = new Vector2(0, this.MyBasePosition.Y) + new Vector2(xOffset, 0)
                          + (this.YLevel * new Vector2(0, MenuSettings.MenuItemHeight));
                if (!menuPositionDictionary.ContainsKey(n))
                {
                    menuPositionDictionary.Add(n, pos);
                }
                else
                {
                    menuPositionDictionary[n] = pos;
                }

                Utils.Sleep(20000, n);
                return pos;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether visible.
        /// </summary>
        internal bool Visible
        {
            get
            {
                if (!MenuSettings.DrawMenu)
                {
                    return false;
                }

                return this.IsRootMenu || this.visible;
            }

            set
            {
                this.visible = value;

                // Hide all the children
                if (this.visible)
                {
                    return;
                }

                foreach (var schild in this.Children)
                {
                    schild.Visible = false;
                }

                foreach (var sitem in this.Items)
                {
                    sitem.Visible = false;
                }
            }
        }

        /// <summary>
        ///     Gets the width.
        /// </summary>
        internal int Width
        {
            get
            {
                return this.Parent != null ? this.Parent.ChildrenMenuWidth : MenuSettings.MenuItemWidth;
            }
        }

        /// <summary>
        ///     Gets the x level.
        /// </summary>
        internal int XLevel
        {
            get
            {
                var result = 0;
                var m = this;
                while (m.Parent != null)
                {
                    m = m.Parent;
                    result++;
                }

                return result;
            }
        }

        /// <summary>
        ///     Gets the y level.
        /// </summary>
        internal int YLevel
        {
            get
            {
                if (this.IsRootMenu || this.Parent == null)
                {
                    return 0;
                }

                return this.Parent.YLevel + this.Parent.Children.TakeWhile(test => test.Name != this.Name).Count();
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The get menu.
        /// </summary>
        /// <param name="assemblyname">
        ///     The assemblyname.
        /// </param>
        /// <param name="menuname">
        ///     The menuname.
        /// </param>
        /// <returns>
        ///     The <see cref="Menu" />.
        /// </returns>
        public static Menu GetMenu(string assemblyname, string menuname)
        {
            var menu = RootMenus.FirstOrDefault(x => x.Key == assemblyname + "." + menuname).Value;
            return menu;
        }

        /// <summary>
        ///     The get value globally.
        /// </summary>
        /// <param name="assemblyname">
        ///     The assemblyname.
        /// </param>
        /// <param name="menuname">
        ///     The menuname.
        /// </param>
        /// <param name="itemname">
        ///     The itemname.
        /// </param>
        /// <param name="submenu">
        ///     The submenu.
        /// </param>
        /// <returns>
        ///     The <see cref="MenuItem" />.
        /// </returns>
        public static MenuItem GetValueGlobally(
            string assemblyname, 
            string menuname, 
            string itemname, 
            string submenu = null)
        {
            var menu = RootMenus.FirstOrDefault(x => x.Key == assemblyname + "." + menuname).Value;

            if (submenu != null)
            {
                menu = menu.SubMenu(submenu);
            }

            var menuitem = menu.Item(itemname);

            return menuitem;
        }

        /// <summary>
        ///     The send message.
        /// </summary>
        /// <param name="key">
        ///     The key.
        /// </param>
        /// <param name="message">
        ///     The message.
        /// </param>
        public static void SendMessage(uint key, Utils.WindowsMessages message)
        {
            foreach (var menu in RootMenus)
            {
                menu.Value.OnReceiveMessage(message, Game.MouseScreenPosition, key);
            }
        }

        /// <summary>
        ///     The add item.
        /// </summary>
        /// <param name="item">
        ///     The item.
        /// </param>
        /// <returns>
        ///     The <see cref="MenuItem" />.
        /// </returns>
        public MenuItem AddItem(MenuItem item)
        {
            item.Parent = this;
            item.Visible = (this.Children.Count > 0 && this.Children[0].Visible)
                           || (this.Items.Count > 0 && this.Items[0].Visible);
            this.Items.Add(item);
            if (item.ValueType == MenuValueType.HeroToggler)
            {
                if (item.GetValue<HeroToggler>().UseEnemyHeroes && item.GetValue<HeroToggler>().Dictionary.Count < 5)
                {
                    var dict = item.GetValue<HeroToggler>().Dictionary;
                    var sdict = item.GetValue<HeroToggler>().SValuesDictionary;
                    var heroes =
                        Heroes.GetByTeam(ObjectManager.LocalHero.GetEnemyTeam())
                            .Where(x => x != null && x.IsValid && !dict.ContainsKey(x.StoredName()))
                            .ToList();

                    foreach (var x in
                        heroes)
                    {
                        item.GetValue<HeroToggler>()
                            .Add(
                                x.StoredName(), 
                                sdict.ContainsKey(x.StoredName())
                                    ? sdict[x.StoredName()]
                                    : item.GetValue<HeroToggler>().DefaultValues);
                    }

                    item.SetValue(
                        new HeroToggler(
                            item.GetValue<HeroToggler>().Dictionary, 
                            true, 
                            false, 
                            item.GetValue<HeroToggler>().DefaultValues));
                }
                else if (item.GetValue<HeroToggler>().UseAllyHeroes && item.GetValue<HeroToggler>().Dictionary.Count < 4)
                {
                    var dict = item.GetValue<HeroToggler>().Dictionary;
                    var sdict = item.GetValue<HeroToggler>().SValuesDictionary;
                    var heroes =
                        Heroes.GetByTeam(ObjectManager.LocalHero.Team)
                            .Where(x => x != null && x.IsValid && !dict.ContainsKey(x.StoredName()))
                            .ToList();

                    foreach (var x in heroes)
                    {
                        item.GetValue<HeroToggler>()
                            .Add(
                                x.StoredName(), 
                                sdict.ContainsKey(x.StoredName())
                                    ? sdict[x.StoredName()]
                                    : item.GetValue<HeroToggler>().DefaultValues);
                    }

                    item.SetValue(
                        new HeroToggler(
                            item.GetValue<HeroToggler>().Dictionary, 
                            false, 
                            true, 
                            item.GetValue<HeroToggler>().DefaultValues));
                }
            }

            return item;
        }

        /// <summary>
        ///     The add sub menu.
        /// </summary>
        /// <param name="subMenu">
        ///     The sub menu.
        /// </param>
        /// <returns>
        ///     The <see cref="Menu" />.
        /// </returns>
        public Menu AddSubMenu(Menu subMenu)
        {
            subMenu.Parent = this;
            subMenu.Visible = this.Children.Count > 0 && this.Children[0].Visible;
            this.Children.Add(subMenu);
            return subMenu;
        }

        /// <summary>
        ///     The add to main menu.
        /// </summary>
        public void AddToMainMenu()
        {
            this.InitMenuState(Assembly.GetCallingAssembly().GetName().Name);
            AppDomain.CurrentDomain.DomainUnload += (sender, args) => this.UnloadMenuState();
            Drawing.OnDraw += this.Drawing_OnDraw;
            ObjectManager.OnAddEntity += this.ObjectMgr_OnAddEntity;
            Game.OnWndProc += this.Game_OnWndProc;
            DelayAction.Add(500, this.SetHeroTogglers);
        }

        /// <summary>
        ///     The item.
        /// </summary>
        /// <param name="name">
        ///     The name.
        /// </param>
        /// <param name="makeChampionUniq">
        ///     The make champion uniq.
        /// </param>
        /// <returns>
        ///     The <see cref="MenuItem" />.
        /// </returns>
        public MenuItem Item(string name, bool makeChampionUniq = false)
        {
            if (makeChampionUniq)
            {
                name = ObjectManager.LocalHero.StoredName() + name;
            }

            MenuItem tempItem;
            if (ItemDictionary.TryGetValue(this.Name + name, out tempItem))
            {
                return tempItem;
            }

            tempItem = this.Items.FirstOrDefault(x => x.Name == name)
                       ?? (from subMenu in this.Children where subMenu.Item(name) != null select subMenu.Item(name))
                              .FirstOrDefault();
            return tempItem;
        }

        /// <summary>
        ///     The remove from main menu.
        /// </summary>
        public void RemoveFromMainMenu()
        {
            try
            {
                var rootName = Assembly.GetCallingAssembly().GetName().Name + "." + this.Name;
                if (RootMenus.ContainsKey(rootName))
                {
                    RootMenus.Remove(rootName);
                    Drawing.OnDraw -= this.Drawing_OnDraw;
                    Game.OnWndProc -= this.Game_OnWndProc;
                    this.UnloadMenuState();
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        ///     The remove sub menu.
        /// </summary>
        /// <param name="name">
        ///     The name.
        /// </param>
        public void RemoveSubMenu(string name)
        {
            var subMenu = this.Children.FirstOrDefault(x => x.Name == name);
            if (subMenu == null)
            {
                return;
            }

            subMenu.Parent = null;
            this.Children.Remove(subMenu);
        }

        /// <summary>
        ///     The set font style.
        /// </summary>
        /// <param name="fontStyle">
        ///     The font style.
        /// </param>
        /// <param name="fontColor">
        ///     The font color.
        /// </param>
        /// <returns>
        ///     The <see cref="Menu" />.
        /// </returns>
        public Menu SetFontStyle(FontStyle fontStyle = FontStyle.Regular, Color? fontColor = null)
        {
            this.Style = fontStyle;
            this.Color = fontColor ?? Color.White;

            return this;
        }

        /// <summary>
        ///     The sub menu.
        /// </summary>
        /// <param name="name">
        ///     The name.
        /// </param>
        /// <returns>
        ///     The <see cref="Menu" />.
        /// </returns>
        public Menu SubMenu(string name)
        {
            // Search in submenus and if it doesn't exist add it.
            var subMenu = this.Children.FirstOrDefault(sm => sm.Name == name);
            return subMenu ?? this.AddSubMenu(new Menu(name, name));
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The drawing_ on draw.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        internal void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame)
            {
                return;
            }

            if (!this.Visible)
            {
                return;
            }

            DotaTexture abg;
            const string ABgName = "menu_button.vmat_c";

            if (!TextureDictionary.TryGetValue(ABgName, out abg))
            {
                abg = Drawing.GetTexture("materials/ensage_ui/ensagemenu/" + ABgName);
                TextureDictionary.Add(ABgName, abg);
            }

            MenuUtils.DrawBoxBordered(
                this.Position.X, 
                this.Position.Y, 
                this.Width, 
                this.Height, 
                1, 
                abg, 
                new Color(15, 10, 0, 255));
            Drawing.DrawRect(this.Position, new Vector2(this.Width, this.Height), new Color(10, 10, 0, 200));

            var textSize = Drawing.MeasureText(
                MultiLanguage._(this.DisplayName), 
                "Arial", 
                new Vector2((float)(this.Height * 0.55), 100), 
                FontFlags.AntiAlias);
            var textPos = this.Position + new Vector2(5, (float)((this.Height * 0.5) - (textSize.Y * 0.5)));
            var bonusWidth = 0;
            if (this.TextureName == null)
            {
                Drawing.DrawText(
                    MultiLanguage._(this.DisplayName), 
                    textPos, 
                    new Vector2((float)(this.Height * 0.55), 100), 
                    this.Color, 
                    FontFlags.AntiAlias | FontFlags.Additive | FontFlags.Custom);
            }
            else
            {
                var tName = this.TextureName;
                if (tName.Contains("npc_dota_hero"))
                {
                    Drawing.DrawRect(
                        this.Position + new Vector2(3, 3), 
                        new Vector2(this.Height + 13, this.Height - 6), 
                        TextureDictionary[tName]);
                    Drawing.DrawRect(
                        this.Position + new Vector2(2, 2), 
                        new Vector2(this.Height + 15, this.Height - 4), 
                        Color.Black, 
                        true);
                    bonusWidth = this.Height + 17;
                }
                else if (tName.Contains("item_"))
                {
                    Drawing.DrawRect(
                        this.Position + new Vector2(3, 3), 
                        new Vector2(this.Height + (float)(this.Height * 0.16), this.Height - 6), 
                        TextureDictionary[tName]);
                    Drawing.DrawRect(
                        this.Position + new Vector2(2, 2), 
                        new Vector2(this.Height - 4, this.Height - 4), 
                        Color.Black, 
                        true);
                    bonusWidth = this.Height - 2;
                }
                else
                {
                    Drawing.DrawRect(
                        this.Position + new Vector2(3, 3), 
                        new Vector2(this.Height - 6, this.Height - 6), 
                        TextureDictionary[tName]);
                    Drawing.DrawRect(
                        this.Position + new Vector2(2, 2), 
                        new Vector2(this.Height - 4, this.Height - 4), 
                        Color.Black, 
                        true);
                    bonusWidth = this.Height - 2;
                }

                if (this.ShowTextWithTexture)
                {
                    Drawing.DrawText(
                        MultiLanguage._(this.DisplayName), 
                        textPos + new Vector2(bonusWidth, 0), 
                        new Vector2((float)(this.Height * 0.55), 100), 
                        this.Color, 
                        FontFlags.AntiAlias | FontFlags.Additive | FontFlags.Custom);
                }
            }

            Drawing.DrawRect(
                new Vector2(this.Position.X, this.Position.Y), 
                new Vector2(this.Width, this.Height), 
                (this.Children.Count > 0 && this.Children[0].Visible) || (this.Items.Count > 0 && this.Items[0].Visible)
                    ? (Utils.IsUnderRectangle(
                        Game.MouseScreenPosition, 
                        this.Position.X, 
                        this.Position.Y, 
                        this.Width, 
                        this.Height)
                           ? new Color(100, 100, 100, 20)
                           : new Color(50, 50, 50, 20))
                    : (Utils.IsUnderRectangle(
                        Game.MouseScreenPosition, 
                        this.Position.X, 
                        this.Position.Y, 
                        this.Width, 
                        this.Height)
                           ? new Color(50, 50, 50, 20)
                           : new Color(0, 0, 0, 180)));

            if (5 + textSize.X + bonusWidth < (float)(this.Width - (this.Height * 0.3)))
            {
                DotaTexture arrow;
                const string Arrowname = "ulti_nomana.vmat_c";

                if (!TextureDictionary.TryGetValue(Arrowname, out arrow))
                {
                    arrow = Drawing.GetTexture("materials/ensage_ui/other/" + Arrowname);
                    TextureDictionary.Add(Arrowname, arrow);
                }

                DotaTexture arrow2;
                const string Arrowname2 = "ulti_cooldown.vmat_c";

                if (!TextureDictionary.TryGetValue(Arrowname2, out arrow2))
                {
                    arrow2 = Drawing.GetTexture("materials/ensage_ui/other/" + Arrowname2);
                    TextureDictionary.Add(Arrowname2, arrow2);
                }

                DotaTexture arrow3;
                const string Arrowname3 = "ulti_ready.vmat_c";

                if (!TextureDictionary.TryGetValue(Arrowname3, out arrow3))
                {
                    arrow3 = Drawing.GetTexture("materials/ensage_ui/other/" + Arrowname3);
                    TextureDictionary.Add(Arrowname3, arrow3);
                }

                var size = new Vector2((float)(this.Height * 0.50), (float)(this.Height * 0.45));
                Drawing.DrawRect(
                    this.Position
                    + new Vector2(
                          (float)(this.Width - (this.Height * 0.35) - (size.X * 0.6)), 
                          (float)((this.Height * 0.5) - (size.Y * 0.5))), 
                    size, 
                    (this.Children.Count > 0 && this.Children[0].Visible)
                    || (this.Items.Count > 0 && this.Items[0].Visible)
                        ? arrow3
                        : (Utils.IsUnderRectangle(
                            Game.MouseScreenPosition, 
                            this.Position.X, 
                            this.Position.Y, 
                            this.Width, 
                            this.Height)
                               ? arrow
                               : arrow2));
            }

            // Draw the menu submenus
            foreach (var child in this.Children.Where(child => child.Visible))
            {
                child.Drawing_OnDraw(args);
            }

            // Draw the items
            for (var i = this.Items.Count - 1; i >= 0; i--)
            {
                var item = this.Items[i];
                if (item.Visible)
                {
                    item.Drawing_OnDraw();
                }
            }
        }

        /// <summary>
        ///     The game_ on wnd proc.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        internal void Game_OnWndProc(WndEventArgs args)
        {
            if (!Game.IsInGame)
            {
                return;
            }

            this.OnReceiveMessage((Utils.WindowsMessages)args.Msg, Game.MouseScreenPosition, (uint)args.WParam, args);
        }

        /// <summary>
        ///     The is inside.
        /// </summary>
        /// <param name="position">
        ///     The position.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        internal bool IsInside(Vector2 position)
        {
            return Utils.IsUnderRectangle(position, this.Position.X, this.Position.Y, this.Width, this.Height);
        }

        /// <summary>
        ///     The on receive message.
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="cursorPos">
        ///     The cursor pos.
        /// </param>
        /// <param name="key">
        ///     The key.
        /// </param>
        /// <param name="args">
        ///     The args.
        /// </param>
        internal void OnReceiveMessage(
            Utils.WindowsMessages message, 
            Vector2 cursorPos, 
            uint key, 
            WndEventArgs args = null)
        {
            // Spread the message to the menu's children recursively
            foreach (var item in this.Items)
            {
                item.OnReceiveMessage(message, cursorPos, key, args);

                // Console.WriteLine(args != null && item.IsInside(cursorPos));
            }

            foreach (var child in this.Children)
            {
                child.OnReceiveMessage(message, cursorPos, key, args);
            }

            if (!this.Visible)
            {
                return;
            }

            // Handle the left clicks on the menus to hide or show the submenus.
            if (message != Utils.WindowsMessages.WM_LBUTTONDOWN)
            {
                return;
            }

            if (this.IsRootMenu && this.Visible)
            {
                if (cursorPos.X - MenuSettings.BasePosition.X < MenuSettings.MenuItemWidth)
                {
                    var n = (int)(cursorPos.Y - MenuSettings.BasePosition.Y) / MenuSettings.MenuItemHeight;
                    if (this.MenuCount != n)
                    {
                        foreach (var schild in this.Children)
                        {
                            schild.Visible = false;
                        }

                        foreach (var sitem in this.Items)
                        {
                            sitem.Visible = false;
                        }
                    }
                }
            }

            if (!this.IsInside(cursorPos))
            {
                return;
            }

            if (!this.IsRootMenu && this.Parent != null)
            {
                // Close all the submenus in the level 
                foreach (var child in this.Parent.Children.Where(child => child.Name != this.Name))
                {
                    foreach (var schild in child.Children)
                    {
                        schild.Visible = false;
                    }

                    foreach (var sitem in child.Items)
                    {
                        sitem.Visible = false;
                    }
                }
            }

            // Hide or Show the submenus.
            foreach (var child in this.Children)
            {
                child.Visible = !child.Visible;
            }

            // Hide or Show the items.
            foreach (var item in this.Items)
            {
                item.Visible = !item.Visible;
            }
        }

        /// <summary>
        ///     The recursive save all.
        /// </summary>
        /// <param name="dics">
        ///     The dics.
        /// </param>
        internal void RecursiveSaveAll(ref Dictionary<string, Dictionary<string, byte[]>> dics)
        {
            foreach (var child in this.Children)
            {
                child.RecursiveSaveAll(ref dics);
            }

            foreach (var item in this.Items)
            {
                item.SaveToFile(ref dics);
            }
        }

        /// <summary>
        ///     The save all.
        /// </summary>
        internal void SaveAll()
        {
            var dic = new Dictionary<string, Dictionary<string, byte[]>>();
            this.RecursiveSaveAll(ref dic);

            foreach (var dictionary in dic)
            {
                var dicToSave = SavedSettings.Load(dictionary.Key) ?? new Dictionary<string, byte[]>();

                foreach (var entry in dictionary.Value)
                {
                    dicToSave[entry.Key] = entry.Value;
                }

                SavedSettings.Save(dictionary.Key, dicToSave);
            }
        }

        /// <summary>
        ///     The set hero togglers.
        /// </summary>
        internal void SetHeroTogglers()
        {
            foreach (var child in this.Children)
            {
                child.SetHeroTogglers();
            }

            foreach (var item in this.Items.Where(item => item.ValueType == MenuValueType.HeroToggler))
            {
                if (item.GetValue<HeroToggler>().UseEnemyHeroes && item.GetValue<HeroToggler>().Dictionary.Count < 5)
                {
                    var dict = item.GetValue<HeroToggler>().Dictionary;
                    var sdict = item.GetValue<HeroToggler>().SValuesDictionary;
                    var heroes =
                        Heroes.GetByTeam(ObjectManager.LocalHero.GetEnemyTeam())
                            .Where(x => x != null && x.IsValid && !dict.ContainsKey(x.StoredName()))
                            .ToList();

                    foreach (var x in
                        heroes)
                    {
                        item.GetValue<HeroToggler>()
                            .Add(
                                x.StoredName(), 
                                sdict.ContainsKey(x.StoredName())
                                    ? sdict[x.StoredName()]
                                    : item.GetValue<HeroToggler>().DefaultValues);
                    }

                    item.SetValue(
                        new HeroToggler(
                            item.GetValue<HeroToggler>().Dictionary, 
                            true, 
                            false, 
                            item.GetValue<HeroToggler>().DefaultValues));
                }
                else if (item.GetValue<HeroToggler>().UseAllyHeroes && item.GetValue<HeroToggler>().Dictionary.Count < 4)
                {
                    var dict = item.GetValue<HeroToggler>().Dictionary;
                    var sdict = item.GetValue<HeroToggler>().SValuesDictionary;
                    var heroes =
                        Heroes.GetByTeam(ObjectManager.LocalHero.Team)
                            .Where(x => x != null && x.IsValid && !dict.ContainsKey(x.StoredName()))
                            .ToList();

                    foreach (var x in heroes)
                    {
                        item.GetValue<HeroToggler>()
                            .Add(
                                x.StoredName(), 
                                sdict.ContainsKey(x.StoredName())
                                    ? sdict[x.StoredName()]
                                    : item.GetValue<HeroToggler>().DefaultValues);
                    }

                    item.SetValue(
                        new HeroToggler(
                            item.GetValue<HeroToggler>().Dictionary, 
                            false, 
                            true, 
                            item.GetValue<HeroToggler>().DefaultValues));
                }
            }
        }

        /// <summary>
        ///     The events_ on load.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private static void Events_OnLoad(object sender, EventArgs e)
        {
            if (loaded)
            {
                return;
            }

            var console = newMessageType.SelectedIndex == 2;

            if (Root.Item("showMessage").GetValue<bool>() && !console)
            {
                var msg =
                    "<font face='Verdana' color='#ff7700'>[</font>Menu Hotkeys<font face='Verdana' color='#ff7700'>]</font> Press: <font face='Verdana' color='#ff7700'>"
                    + Utils.KeyToText(Root.Item("toggleKey").GetValue<KeyBind>().Key)
                    + "</font> Hold: <font face='Verdana' color='#ff7700'>"
                    + Utils.KeyToText(Root.Item("pressKey").GetValue<KeyBind>().Key) + "</font>";
                Game.PrintMessage(
                    msg, 
                    newMessageType.SelectedIndex == 2 || newMessageType.SelectedIndex == 0
                        ? MessageType.LogMessage
                        : MessageType.ChatMessage);
            }
            else if (console && Root.Item("showMessage").GetValue<bool>())
            {
                var msg = @"[Menu Hotkeys] Press: " + Utils.KeyToText(Root.Item("toggleKey").GetValue<KeyBind>().Key)
                          + @" Hold: " + Utils.KeyToText(Root.Item("pressKey").GetValue<KeyBind>().Key);
                Console.WriteLine(msg);
            }

            loaded = true;
        }

        /// <summary>
        ///     The message value changed.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private static void MessageValueChanged(object sender, OnValueChangeEventArgs e)
        {
            loaded = false;
            newMessageType = e.GetNewValue<StringList>();
            Events_OnLoad(null, null);
        }

        /// <summary>
        ///     The init menu state.
        /// </summary>
        /// <param name="assemblyName">
        ///     The assembly name.
        /// </param>
        private void InitMenuState(string assemblyName)
        {
            this.uniqueId = assemblyName + "." + this.Name;

            var globalMenuList = MenuGlobals.MenuState;

            if (globalMenuList == null)
            {
                globalMenuList = new List<string>();
            }

            while (globalMenuList.Contains(this.uniqueId))
            {
                this.uniqueId += ".";
            }

            globalMenuList.Add(this.uniqueId);

            MenuGlobals.MenuState = globalMenuList;
        }

        /// <summary>
        ///     The object mgr_ on add entity.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        private void ObjectMgr_OnAddEntity(EntityEventArgs args)
        {
            DelayAction.Add(
                2000, 
                () =>
                    {
                        var hero = args.Entity as Hero;
                        if (hero != null)
                        {
                            this.SetHeroTogglers();
                        }
                    });
        }

        /// <summary>
        ///     The unload menu state.
        /// </summary>
        private void UnloadMenuState()
        {
            var globalMenuList = MenuGlobals.MenuState;
            globalMenuList.Remove(this.uniqueId);
            MenuGlobals.MenuState = globalMenuList;
        }

        #endregion
    }
}