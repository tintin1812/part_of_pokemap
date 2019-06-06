using System.Collections;
using System.Collections.Generic;
using AON.RpgMapEditor;
using FairyGUI;
using UnityEngine;

public class PropertysGame
{
    public class PropertyChar
    {
        public static PropertyChar CreateFromSlug(SerializablePropertys.Property propertyBase, string slug)
        {
            PropertyChar pc = new PropertyChar();
            pc.Slug = slug;
            pc.PropertyBase = propertyBase;
            return pc;
        }

        public string Slug;

        // public SerializablePropertys.Property PropertyBase;
        public SerializablePropertys.Property PropertyBase { get; set; }

        public int EquipSlot = -1;

        public int Count = 1;

        public bool IsActive = false;

        public string NameUI
        {
            get
            {
                if (PropertyBase == null)
                {
                    return "Item not found";
                }
                else if (!string.IsNullOrEmpty(PropertyBase.Name))
                {
                    return PropertyBase.Name;
                }
                else if (!string.IsNullOrEmpty(Slug))
                {
                    return Slug;
                }
                return "Item name is NULL";
            }
        }
    }

    private static PropertysGame _instance = null;

    public static PropertysGame Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PropertysGame();
            }
            return _instance;
        }
    }

    public static void ResetCache()
    {
        _instance = null;
    }
    //Data
    public List<PropertyChar> PropertysChar = new List<PropertyChar>();
    private bool ShouldUpdateGUIContent = true;
    private int countNewItem = 0;

    public PropertyChar AddItem(SerializablePropertys propertys, string slug)
    {
        var v_slug = propertys.PropertyBySlug(slug);
        if (v_slug == null)
        {
            InputFieldHelper.Instance.ShowNoti(string.Format("Item {0} not found (check propertys list)", slug));
            return null;
        }
        ShouldUpdateGUIContent = true;
        countNewItem++;
        if (v_slug.StackInBag)
        {
            for (int i = 0; i < PropertysChar.Count; i++)
            {
                var p = PropertysChar[i];
                if (p.Slug == slug)
                {
                    p.Count = p.Count + 1;
                    return p;
                }
            }
        }
        var p_new = PropertyChar.CreateFromSlug(v_slug, slug);
        PropertysChar.Add(p_new);
        return p_new;
    }

    public bool IsHaveItem(string slug)
    {
        for (int i = 0; i < PropertysChar.Count; i++)
        {
            var p = PropertysChar[i];
            if (p.Slug == slug)
            {
                return true;
            }
        }
        return false;
    }

    public PropertyChar GetOutfit(Item item)
    {
        for (int i = 0; i < PropertysChar.Count; i++)
        {
            var p = PropertysChar[i];
            if (p.PropertyBase.IsOutfit && p.PropertyBase.RefSlug == item.SlugGlobal)
            {
                return p;
            }
        }
        return null;
    }

    public PropertyChar GetPet(SerializablePropertys.Property property)
    {
        for (int i = 0; i < PropertysChar.Count; i++)
        {
            var p = PropertysChar[i];
            if (p.PropertyBase.IsPet && p.PropertyBase == property)
            {
                return p;
            }
        }
        return null;
    }

    public PropertyChar GetCertificates(SerializablePropertys.Property property)
    {
        for (int i = 0; i < PropertysChar.Count; i++)
        {
            var p = PropertysChar[i];
            if (p.PropertyBase.IsCertificates && p.PropertyBase == property)
            {
                return p;
            }
        }
        return null;
    }

    public void UsingItem(PropertyChar p)
    {
        if (p.Count > 0)
        {
            p.Count = p.Count - 1;
        }
        FlagAction.DoFlagAction(TriggerGame.Instance.WorldFlag, AutoTileMap_Editor.Instance.MapsData.ListFlagAction, p.PropertyBase.ActionUsing);
        if (p.Count <= 0)
        {
            PropertysChar.Remove(p);
        }
        ShouldUpdateGUIContent = true;
    }

    public void Equip(CharGame charGame, PropertyChar p)
    {
        if (!p.PropertyBase.IsOutfit)
        {
            InputFieldHelper.Instance.ShowNoti("ItemEquip it not the Outfit");
            return;
        }
        if (p.PropertyBase.RefSlug == null || p.PropertyBase.RefSlug == "")
        {
            InputFieldHelper.Instance.ShowNoti("ItemEquip empty");
            return;
        }
        var item = AutoTileMap_Editor.Instance.ItemCharData.FetchItemByGlobalSlug(p.PropertyBase.RefSlug);
        if (item == null)
        {
            InputFieldHelper.Instance.ShowNoti("ItemEquip not found");
            return;
        }
        if (charGame == null)
        {
            charGame = AutoTileMap_Editor.Instance.Agent.GetComponentInChildren<CharGame>();
        }
        if (charGame.SlugChar != item.SlugChar)
        {
            InputFieldHelper.Instance.ShowNoti("Your character can't use this item.");
            return;
        }
        ShopGame.Instance.UnTryCostume(charGame);
        UnEquip(charGame, item.ItemType);
        CheckItemBeforeEquip(charGame, item.ItemType);
        string error = "";
        if (charGame.AddEquipment(item, ref error) == false)
        {
            // m_tip = error;
            return;
        }
        p.EquipSlot = (int)item.ItemType;
        ShouldUpdateGUIContent = true;
    }

    public void CheckItemBeforeEquip(CharGame charGame, Item.EItemType itemType){
        if(itemType == Item.EItemType.hair){
            UnEquip(charGame, Item.EItemType.accessories);
        }else if(itemType == Item.EItemType.accessories){
            UnEquip(charGame, Item.EItemType.hair);
        }
    }

    public bool UnEquip(CharGame charGame, Item.EItemType t)
    {
        int slot = (int)t;
        for (int i = 0; i < PropertysChar.Count; i++)
        {
            var pCheck = PropertysChar[i];
            if (pCheck.EquipSlot == slot)
            {
                if (UnEquip(charGame, pCheck) == false)
                {
                    InputFieldHelper.Instance.ShowNoti("Can't UnEquip which item same slot.");
                    return true;
                }
            }
        }
        return false;
    }

    public bool UnEquip(CharGame charGame, PropertyChar p)
    {
        if (p.PropertyBase.RefSlug == null || p.PropertyBase.RefSlug == "")
        {
            InputFieldHelper.Instance.ShowNoti("ItemEquip empty");
            return false;
        }
        var item = AutoTileMap_Editor.Instance.ItemCharData.FetchItemByGlobalSlug(p.PropertyBase.RefSlug);
        if (item == null)
        {
            InputFieldHelper.Instance.ShowNoti("ItemEquip not found");
            return false;
        }
        if (charGame == null)
        {
            charGame = AutoTileMap_Editor.Instance.Agent.GetComponentInChildren<CharGame>();
        }
        ShopGame.Instance.UnTryCostume(charGame);
        charGame.RemoveEquipment(item);
        p.EquipSlot = -1;
        ShouldUpdateGUIContent = true;
        return true;
    }

    public void UnEquipAll()
    {
        CharGame charGame = AutoTileMap_Editor.Instance.Agent.GetComponentInChildren<CharGame>();
        foreach (var p in PropertysChar)
        {
            if (p.EquipSlot >= 0)
            {
                UnEquip(charGame, p);
            }
        }
        UnquipCurrentPet();
    }

    public void EquipPet(PropertyChar p)
    {
        if (!p.PropertyBase.IsPet)
        {
            InputFieldHelper.Instance.ShowNoti("Item it not the Pet");
            return;
        }
        UnquipCurrentPet();
        p.IsActive = true;
        BasicMecanimControl basicMecanimControl = AutoTileMap_Editor.Instance.Agent.GetComponentInChildren<BasicMecanimControl>();
        basicMecanimControl.Pet = "pets/" +  p.PropertyBase.RefSlug;
    }

    public void UnquipCurrentPet()
    {
        for (int i = 0; i < PropertysChar.Count; i++)
        {
            var pCheck = PropertysChar[i];
            if (pCheck.IsActive && pCheck.PropertyBase != null && pCheck.PropertyBase.IsPet)
            {
                pCheck.IsActive = false;
            }
        }
        BasicMecanimControl basicMecanimControl = AutoTileMap_Editor.Instance.Agent.GetComponentInChildren<BasicMecanimControl>();
        basicMecanimControl.Pet = "";
    }

    //----------GUI in game---------//
    /*
	private GUISkin GUISkinIngame = Resources.Load("GUI/InGame") as GUISkin;
	private Texture TexSelected = Resources.Load("GUI/Icon/selarrow") as Texture;
	public void OnGUI() {
		// Debug.Log("TriggerGame OnGUI");
		var lastSkin = GUI.skin;
		GUI.skin = GUISkinIngame;
		if(m_show){
			OnGUIItem();
		}else
		{
			string strBag = countNewItem == 0 ? "Bag" : "Bag (" + countNewItem + " new)";
			float w_title = GUI.skin.button.CalcSize( new GUIContent(strBag)).x + 4;

			var rectBagBt = new Rect(Screen.width - 6 - w_title, 16, w_title, DefineAON.GAME_Height_Label);

			GameGui.IgnoreMouseByBox( rectBagBt);
			
			if(GUI.Button( rectBagBt, strBag)){
				m_show = true;
				indextItemSeleted = 0;
				m_scrollPos = Vector2.zero;
				ShouldUpdateGUIContent = true;
			}
		}
		GUI.skin = lastSkin;
	}
	
	private bool m_show = false;
	public bool IsShow{
		get{ return m_show;}
	}
	private int indextItemSeleted = 0;
	private Vector2 m_scrollPos = Vector2.zero;
	private GUIContent[] GUI_Propertys;
	private int countNewItem = 0;
	private string m_tip = "";
	public void OnGUIItem() {
		float offsetBoxOut = 8;
		float w = 500;
		float h = 300;
		Rect rect = new Rect(Screen.width - w - 20 , 20, w , h);
		{
			Rect rectBox = new Rect(rect.x - offsetBoxOut, rect.y - offsetBoxOut, rect.width + offsetBoxOut * 2, rect.height + offsetBoxOut * 2);
			GUI.Box( rectBox, "");
			GameGui.IgnoreMouseByBox( rectBox);
		}

		GUI.Label( new Rect(rect.x + 6, rect.y + 6, 60, DefineAON.GAME_Height_Label),"Bag");
		if(GUI.Button(new Rect(rect.x + rect.width - 76, rect.y + 4, 60, DefineAON.GAME_Height_Label),"Close")){
			m_show = false;
			countNewItem = 0;
			return;
		}
		float offsetBoxIn = 4;
		float top = 44;
		float bot = 110;
		float offset_x_list = 10;
		var rectView = new Rect(rect.x + offset_x_list + offsetBoxIn, rect.y + top + offsetBoxIn, rect.width - offset_x_list - offset_x_list - offsetBoxIn * 2, rect.height - top - bot - offsetBoxIn * 2);
		OnGUIItems(rectView);
		if(indextItemSeleted >= 0 && indextItemSeleted < PropertysChar.Count){
			OnGUIItemInfo(new Rect(rect.x + offset_x_list + offsetBoxIn, rect.y + rect.height - bot + 20, rect.width - offset_x_list - offset_x_list - offsetBoxIn * 2, bot - 20));
		}
	}
	private void OnGUIItems( Rect rectView){
		GUI.Box(rectView, "");
		if(PropertysChar.Count == 0){
			// var listStyleLabel = GUI.skin.label;
			// float h_title = listStyleLabel.CalcHeight( g, widthContent) + 8 + 8;
			GUI.Label(rectView, new GUIContent("Empty"));
		}else
		{
			if(ShouldUpdateGUIContent){
				ShouldUpdateGUIContent = false;
				GUI_Propertys = new GUIContent[PropertysChar.Count];
				for( int i = 0; i < PropertysChar.Count; i++){
					var p = PropertysChar[i];
					string v = "";
					if(p.PropertyBase.Name == ""){
						v = p.Slug;
					}else if(p.Count == 1){
						v = p.PropertyBase.Name;
					}else{
						v = string.Format("{0} x{1}", p.PropertyBase.Name, p.Count);
					}
					if(p.EquipSlot >= 0){
						v += " (equip)";
					}
					GUI_Propertys[i] = new GUIContent(v);
				}
			}
			if(indextItemSeleted >= GUI_Propertys.Length){
				indextItemSeleted = GUI_Propertys.Length - 1;
			}
			var rectContent = new Rect(rectView.x, rectView.y, rectView.width - 16, GUI_Propertys.Length * 32f);
			// GUI.skin = GUISkinGrid;
			m_scrollPos = GUI.BeginScrollView(rectView, m_scrollPos, rectContent, false, false);
			var indextItemNext = GUI.SelectionGrid( new Rect(rectContent.x + 14, rectContent.y, rectContent.width - 14, rectContent.height), indextItemSeleted, GUI_Propertys, 1, GUISkinIngame.label);
			if(indextItemNext != indextItemSeleted){
				indextItemSeleted = indextItemNext;
				m_tip = "";
			}
			GUI.DrawTexture( new Rect(rectView.x, rectView.y + indextItemSeleted * 32f + 5, 12, 20), TexSelected);
			GUI.EndScrollView();
		}
	}
	private void OnGUIItemInfo( Rect rect){
		GUI.Box( rect, "");
		var p = PropertysChar[indextItemSeleted];
		float yGui = rect.y;
		float xGui = rect.x + 8;
		float wBtRight = 80;
		float wLeft = rect.width - 16 - wBtRight;
		if(!string.IsNullOrEmpty(p.PropertyBase.Des)){
			GUI.Label(new Rect( xGui, yGui, rect.width - 8, 64), p.PropertyBase.Des);
		}
		yGui = rect.y + rect.height - 34;
		float yRightBt = rect.x + rect.width - 16 - wBtRight;
		if(p.EquipSlot >= 0){
			// On Equip
			if(GUI.Button(new Rect(yRightBt, yGui, wBtRight, DefineAON.GAME_Height_Label), "Un equip")){
				UnEquip( null, p);
			}
		}else
		{
			if(p.PropertyBase.CanEquip){
				if(GUI.Button(new Rect(yRightBt, yGui, wBtRight, DefineAON.GAME_Height_Label), "Equip")){
					Equip( null, p);
				}
			}
			yGui -= 32f;
			if(p.PropertyBase.CanUsing){
				if(GUI.Button(new Rect(yRightBt, yGui, wBtRight, DefineAON.GAME_Height_Label), "Using")){
					UsingItem( p);
				}
			}
		}
		yGui = rect.y + rect.height - 34;
		if(m_tip != null && m_tip.Length > 0){
			GUI.Label(new Rect(xGui, yGui, wLeft, 32), m_tip);
		}
	}
	*/

    //-------------------//
    ControlPropertys controlPropertys = null;
    public bool IsShow
    {
        get
        {
            if (controlPropertys == null)
                return false;
            return controlPropertys.contentPane.visible;
        }
    }

    private void _controlPropertys_Init()
    {
        if (controlPropertys != null){
            controlPropertys.Dispose();
            controlPropertys = null;
        }
        controlPropertys = new ControlPropertys();
    }
    private void _controlPropertys_Dispose()
    {
        if (controlPropertys != null)
        {
            controlPropertys.Dispose();
            controlPropertys = null;
        }
    }

    //----------Outfits---------//
    public void ShowOutfits(GComponent lastPopup, CharGame charGame, SerializablePropertys propertys)
    {
        ItemChar itemChar = charGame.DataChar;

        var pm_choise = new QuickControlList();

        for (int i = 0; i < itemChar.ItemByType.Length; i++)
        {
            ItemChar.Items items = itemChar.ItemByType[i];
            if (items.ItemList != null && items.ItemList.Length > 0)
            {
                Item.EItemType eItemType = (Item.EItemType)i;
                pm_choise.AddBt(Item.StrEItemType[i], (EventContext context) =>
                {
                    this._showOutfits2(charGame, pm_choise.contentPane, eItemType, items, propertys);
                });
            }
        }
        pm_choise.AddBt("Cancel", (EventContext context) =>
        {
            pm_choise.Dispose();
        });
        pm_choise.SetParent(InputFieldHelper.Instance.PopUp);
        lastPopup.visible = false;
        pm_choise.SetOnDispose(() =>
        {
            lastPopup.visible = true;
        });
    }

    private void _showOutfits2(CharGame charGame, GComponent lastPopup, Item.EItemType eItemType, ItemChar.Items items, SerializablePropertys propertys)
    {
        _controlPropertys_Init();
        lastPopup.visible = false;
        controlPropertys._btClose.onClick.Clear();
        controlPropertys._btClose.onClick.Add(() =>
        {
            _controlPropertys_Dispose();
            lastPopup.visible = true;
        });
        controlPropertys.SetTitle(Item.StrEItemType[(int)eItemType]);
        controlPropertys.ResetListItem();
        controlPropertys.ResetSelectItem();
        _showOutfits3(charGame, items, propertys);
        _refreshListOutfits(items);
        controlPropertys.ShowOn(InputFieldHelper.Instance.PopUp);
    }

    private void _showOutfits3(CharGame charGame, ItemChar.Items items, SerializablePropertys propertys)
    {
        var l = items.ItemList;
        for (int i = 0; i < l.Length; i++)
        {
            Item item = l[i];
            PropertyChar propertyOwn = GetOutfit(item);
            var bt = controlPropertys.AddItem(item.Slug, () =>
            {
                _showOutfits4(charGame, items, item, propertyOwn);
            });
            _setCellOutfits(i, propertyOwn);
        }
    }

    private void _refreshListOutfits(ItemChar.Items items)
    {
        var l = items.ItemList;
        for (int i = 0; i < l.Length; i++)
        {
            Item item = l[i];
            PropertyChar property = GetOutfit(item);
            _setCellOutfits(i, property);
        }
    }

    private void _setCellOutfits(int index, PropertyChar property)
    {
        bool isOwn = property != null;
        bool isEquip = property != null && property.EquipSlot != -1;
        controlPropertys.SetItemCaption(index, isOwn, isEquip);
    }

    private void _showOutfits4(CharGame charGame, ItemChar.Items items, Item item, PropertyChar property)
    {
        var tryItem = AutoTileMap_Editor.Instance.ItemCharData.FetchItemByGlobalSlug(item.SlugGlobal);
        if (tryItem == null)
        {
            InputFieldHelper.Instance.ShowNoti("Item not found in database");
            return;
        }

        controlPropertys.ResetSelectItem();

        controlPropertys.LoadModelItem(tryItem.ItemPrefab);

        if (tryItem.SlugChar != charGame.SlugChar)
        {
            controlPropertys.SetItemDes("Your character can't use this item.");
            return;
        }

        if (property == null)
        {
            controlPropertys.SetItemName(item.Slug);
            controlPropertys.SetItemDes("You don't own this item.");
            return;
        }
        else
        {
            controlPropertys.SetItemName(property.NameUI);
        }
        if (property.EquipSlot == -1)
        {
            controlPropertys.AddAction("Equip", () =>
            {
                this.Equip(charGame, property);
                this._showOutfits4(charGame, items, item, property);
                this._refreshListOutfits(items);
            });
        }
        else
        {
            controlPropertys.SetItemDes("Item is equipped");
            controlPropertys.AddAction("Unequip", () =>
            {
                this.UnEquip(charGame, property);
                this._showOutfits4(charGame, items, item, property);
                this._refreshListOutfits(items);
            });
        }
    }


    //----------Items---------//
    public void ShowItems(GComponent lastPopup)
    {
        _controlPropertys_Init();
        lastPopup.visible = false;
        controlPropertys._btClose.onClick.Clear();
        controlPropertys._btClose.onClick.Add(() =>
        {
            _controlPropertys_Dispose();
            lastPopup.visible = true;
        });
        controlPropertys.SetTitle("Items");
        controlPropertys.ListItem.selectedIndex = -1;
        _refreshListItems();
        controlPropertys.ShowOn(InputFieldHelper.Instance.PopUp);
    }

    private void _refreshListItems()
    {
        int lastSelectedIndex = controlPropertys.ListItem.selectedIndex;
        controlPropertys.ResetListItem();
        controlPropertys.ResetSelectItem();
        for (int i = 0; i < PropertysChar.Count; i++)
        {
            PropertyChar property = PropertysChar[i];
            if (property.PropertyBase.IsItem)
            {
                var bt = controlPropertys.AddItem(property.NameUI, () =>
                {
                    _showItems2(property);
                });
                controlPropertys.SetItemUsing(i, property.PropertyBase.StackInBag, property.Count);
            }
        }
        controlPropertys.SetSelectedIndex(lastSelectedIndex);
    }

    private void _showItems2(PropertyChar property)
    {

        controlPropertys.ResetSelectItem();

        controlPropertys.SetItemName(property.NameUI);

        controlPropertys.LoadImageItem(property.PropertyBase.RefIcon);

        if (property.EquipSlot != -1)
        {
            controlPropertys.SetItemDes("Item is equipped");
            return;
        }

        controlPropertys.SetItemDes(property.PropertyBase.Des);

        controlPropertys.AddAction("Use", () =>
        {
            _onUsingItem(property);
        });
    }

    private void _onUsingItem(PropertyChar property)
    {
        controlPropertys.contentPane.visible = false;
        string name_item = property.NameUI;
        // string last_text = InputFieldHelper.Instance.LastTextChatBottom();
        string text = string.Format("{0} used!", name_item);
        text += "\n" + property.PropertyBase.Des;
        text += "\n" + property.PropertyBase.ActionUsing;
        InputFieldHelper.Instance.ShowChatBottom(text, true, (TypingEffectByLine ty) =>
        {
            controlPropertys.contentPane.visible = true;
            InputFieldHelper.Instance.HideChatBottom();
            this.UsingItem(property);
            _refreshListItems();
        });
    }

    //----------Certificates---------//
    public void ShowCertificates(GComponent lastPopup, SerializablePropertys propertys)
    {
        _controlPropertys_Init();
        lastPopup.visible = false;
        controlPropertys._btClose.onClick.Clear();
        controlPropertys._btClose.onClick.Add(() =>
        {
            // controlPropertys.Hide();
            _controlPropertys_Dispose();
            lastPopup.visible = true;
        });
        controlPropertys.SetTitle("Certificates");
        controlPropertys.ListItem.selectedIndex = -1;
        _refreshCertificates(propertys);
        controlPropertys.ShowOn(InputFieldHelper.Instance.PopUp);
    }

    private void _refreshCertificates(SerializablePropertys propertys)
    {
        int lastSelectedIndex = controlPropertys.ListItem.selectedIndex;
        controlPropertys.ResetListItem();
        controlPropertys.ResetSelectItem();
        for (int i = 0; i < propertys.Count; i++)
        {
            var propertyData = propertys.PropertyByIndex(i);
            if (propertyData.IsCertificates)
            {
                PropertyChar propertyOwn = GetCertificates(propertyData);
                var bt = controlPropertys.AddItem(propertyData.Name, () =>
                {
                    _showCertificates(propertyData, propertyOwn);
                });
                controlPropertys.SetItemOwn(bt, propertyOwn != null);
            }
        }
        controlPropertys.SetSelectedIndex(lastSelectedIndex);
    }

    private void _showCertificates(SerializablePropertys.Property propertyData, PropertyChar propertyOwn)
    {

        controlPropertys.ResetSelectItem();

        controlPropertys.SetItemName(propertyData.Name);

        controlPropertys.SetItemDes(propertyData.Des);

        controlPropertys.LoadImageItem(propertyData.RefIcon);
    }

    //----------Pets---------//

    public void ShowPets(GComponent lastPopup, SerializablePropertys propertys)
    {
        _controlPropertys_Init();
        lastPopup.visible = false;
        controlPropertys._btClose.onClick.Clear();
        controlPropertys._btClose.onClick.Add(() =>
        {
            // controlPropertys.Hide();
            _controlPropertys_Dispose();
            lastPopup.visible = true;
        });
        controlPropertys.SetTitle("Pets");
        controlPropertys.ListItem.selectedIndex = -1;
        _refreshPets(propertys, true);
        controlPropertys.ShowOn(InputFieldHelper.Instance.PopUp);
    }

    private void _refreshPets(SerializablePropertys propertys, bool isBeginShow)
    {
        int lastSelectedIndex = controlPropertys.ListItem.selectedIndex;
        controlPropertys.ResetListItem();
        controlPropertys.ResetSelectItem();
        for (int i = 0; i < propertys.Count; i++)
        {
            var propertyData = propertys.PropertyByIndex(i);
            if (propertyData.IsPet)
            {
                PropertyChar propertyOwn = GetPet(propertyData);
                var bt = controlPropertys.AddItem(propertyData.Name, () =>
                {
                    _showPet(propertys, propertyData, propertyOwn);
                });
                bool isOwn = propertyOwn != null;
                bool isEquip = false;
                if (propertyOwn != null)
                {
                    isEquip = propertyOwn.IsActive;
                }
                controlPropertys.SetItemCaption(bt, isOwn, isEquip);
                if (isBeginShow && isEquip)
                {
                    lastSelectedIndex = i;
                }
            }
        }
        controlPropertys.SetSelectedIndex(lastSelectedIndex);
    }

    private void _showPet(SerializablePropertys propertys, SerializablePropertys.Property propertyData, PropertyChar propertyOwn)
    {
        controlPropertys.ResetSelectItem();

        controlPropertys.SetItemName(propertyData.Name);

        controlPropertys.SetItemDes(propertyData.Des);

        if (string.IsNullOrEmpty(propertyData.RefSlug))
        {
            InputFieldHelper.Instance.ShowNoti("Item model is null");
            return;
        }
        controlPropertys.LoadModelItem("pets/" + propertyData.RefSlug);

        if (propertyOwn != null)
        {
            bool isEquip = propertyOwn.IsActive;
            if (!isEquip)
            {
                controlPropertys.AddAction("Equip", () =>
                {
                    // basicMecanimControl.Pet = propertyData.RefSlug;
                    EquipPet(propertyOwn);
                    _refreshPets(propertys, false);
                });
            }
            else
            {
                controlPropertys.SetItemDes("Item is equipped");
                controlPropertys.AddAction("Unequip", () =>
                {
                    // basicMecanimControl.Pet = "";
                    UnquipCurrentPet();
                    _refreshPets(propertys, false);
                });
            }
        }
    }
}
