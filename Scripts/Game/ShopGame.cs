using System.Collections;
using System.Collections.Generic;
using AON.RpgMapEditor;
using FairyGUI;
using UnityEngine;

public class ShopGame
{

    private static ShopGame _instance = null;

    public static ShopGame Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ShopGame();
            }
            return _instance;
        }
    }

    public static void ResetCache()
    {
        _instance = null;
    }

    public delegate void CallBackClose();
    public delegate void CallBackBuy(ControlShop pm);
    // private CallBack mOnCloseShop = null;
    private CallBackBuy mOnBuy = null;

    // public delegate void OnCloseShop();
    // private OnCloseShop mOnCloseShop = null;

    /*
	private GUISkin GUISkinGrid = Resources.Load("GUI/InGame") as GUISkin;
	private Texture TexSelected = Resources.Load("GUI/Icon/selarrow") as Texture;
	private bool m_show = false;
	public bool IsShow{
		get{ return m_show;}
	}
	private GUIContent[] m_GUI_Items_Left;
	private GUIContent[] m_GUI_Items_Right;
	private Vector2 m_scrollPos = Vector2.zero;
	*/

    private SerializablePropertys m_dataRaw = null;
    private SerializablePackages.Package m_package = null;
    private Item m_itemRawTry = null;
    private Item m_itemTryOn = null;
    private int m_indextItemSeleted = -1;

    private ControlShop _controlShop = null;
    public bool IsShow
    {
        get
        {
            if (_controlShop == null)
                return false;
            return _controlShop.contentPane.visible;
        }
    }

    private void _controlShop_Dispose()
    {
        if (_controlShop != null)
        {
            _controlShop.Dispose();
            _controlShop = null;
        }
    }

    /*
	public void OnGUI() {
		if(!m_show)
			return;
		var lastSkin = GUI.skin;
		GUI.skin = GUISkinGrid;
		OnGUIIn();
		GUI.skin = lastSkin;
	}

	public void OnGUIIn() {
		float offsetBoxOut = 8;
		float w = 500;
		float h = 300;
		Rect rect = new Rect((Screen.width - w) - 20, (Screen.height - h) - 20, w , h);
		{
			Rect rectBox = new Rect(rect.x - offsetBoxOut, rect.y - offsetBoxOut, rect.width + offsetBoxOut * 2, rect.height + offsetBoxOut * 2);
			GUI.Box( rectBox, "", GUISkinGrid.box);
			GameGui.IgnoreMouseByBox( rectBox);
		}
		GUI.Label( new Rect(rect.x + 6, rect.y + 6, 60, DefineAON.GAME_Height_Label),"Shop", GUISkinGrid.label);
		if(GUI.Button(new Rect(rect.x + rect.width - 76, rect.y + 4, 60, DefineAON.GAME_Height_Label),"Close", GUISkinGrid.button)){
			UnTryCostume(null);
			m_show = false;
			if(mOnCloseShop != null){
				var callback = mOnCloseShop;
				mOnCloseShop = null;
				callback();
			}
			return;
		}
		float offsetBoxIn = 4;
		float top = 44;
		float bot = 110;
		float offset_x_list = 10;
		var rectView = new Rect(rect.x + offset_x_list + offsetBoxIn, rect.y + top + offsetBoxIn, rect.width - offset_x_list - offset_x_list - offsetBoxIn * 2, rect.height - top - bot - offsetBoxIn * 2);
		OnGUIItems(rectView);
		if(m_indextItemSeleted >= 0 && m_indextItemSeleted < m_package.propertys.Count){
			OnGUIItemInfo(new Rect(rect.x + offset_x_list + offsetBoxIn, rect.y + rect.height - bot + 20, rect.width - offset_x_list - offset_x_list - offsetBoxIn * 2, bot - 20));
		}
	}

	private void OnGUIItems( Rect rectView){
		GUI.Box(rectView, "", GUISkinGrid.box);
		if(m_GUI_Items_Left.Length == 0){
			// var listStyleLabel = GUI.skin.label;
			// float h_title = listStyleLabel.CalcHeight( g, widthContent) + 8 + 8;
			GUI.Label(rectView, new GUIContent("Empty"), GUISkinGrid.label);
		}else
		{
			if(m_indextItemSeleted >= m_GUI_Items_Left.Length){
				m_indextItemSeleted = m_GUI_Items_Left.Length - 1;
			}
			var rectContent = new Rect(rectView.x, rectView.y, rectView.width - 16, m_GUI_Items_Left.Length * 32f);
			m_scrollPos = GUI.BeginScrollView(rectView, m_scrollPos, rectContent, false, false);
			var indextItemNext = GUI.SelectionGrid( new Rect(rectContent.x + 14, rectContent.y, rectContent.width - 14, rectContent.height), m_indextItemSeleted, m_GUI_Items_Left, 1, GUISkinGrid.label);
			if(indextItemNext != m_indextItemSeleted){
				m_indextItemSeleted = indextItemNext;
			}
			GUI.DrawTexture( new Rect(rectView.x, rectView.y + m_indextItemSeleted * 32f + 5, 12, 20), TexSelected);
			var StyleRightText = GUISkinGrid.customStyles[6];
			GUI.SelectionGrid( new Rect(rectContent.x + 14, rectContent.y, rectContent.width - 14, rectContent.height), m_indextItemSeleted, m_GUI_Items_Right, 1, StyleRightText);
			GUI.EndScrollView();
		}
	}

	private void OnGUIItemInfo( Rect rect){
		GUI.Box( rect, "", GUISkinGrid.box);
		var slug = m_package.propertys[m_indextItemSeleted];
		var payBy = m_package.payBys[m_indextItemSeleted];
		var propertyData = m_dataRaw.PropertyBySlug(slug);
		float yGui = rect.y;
		if(propertyData == null){
			GUI.Label(new Rect(rect.x + 8, yGui, rect.width - 8, 32), "Item not found in database", GUISkinGrid.label);
			return;
		}
		// GUI.Label(new Rect(rect.x + 8, yGui, rect.width - 8, 32), propertyData.Name, GUISkinGrid.label);
		// yGui += 32f;
		if(!string.IsNullOrEmpty(propertyData.Des)){
			GUI.Label(new Rect(rect.x + 8, yGui, rect.width - 8, 64), propertyData.Des, GUISkinGrid.label);
		}
		float wBtRight = 80;
		float yRightBt = rect.x + rect.width - 16 - wBtRight;
		yGui = rect.y + rect.height - 34;
		if(GUI.Button(new Rect(yRightBt, yGui, wBtRight, DefineAON.GAME_Height_Label), "Buy", GUISkinGrid.button)){
			PropertysGame.Instance.AddItem( m_dataRaw, slug);
			return;
		}
		OnGUIItemEquip( rect, propertyData);
	}

	private void OnGUIItemEquip( Rect rect, SerializablePropertys.Property property){
		if(!property.CanEquip)
			return;
		float yGui = rect.y + rect.height - 34;
		float xGui = rect.x + 8;
		float wBtRight = 80;
		float wLeft = rect.width - 16;
		float yRightBt = rect.x + rect.width - 16 - wBtRight;
		if(string.IsNullOrEmpty(property.ItemEquip)){
			GUI.Label(new Rect(xGui, yGui, wLeft, DefineAON.GAME_Height_Label), "Item Equip empty", GUISkinGrid.label);
			return;
		}
		var item = AutoTileMap_Editor.Instance.ItemCharData.FetchItemByGlobalSlug(property.ItemEquip);
		if(item == null){
			GUI.Label(new Rect(xGui, yGui, wLeft, DefineAON.GAME_Height_Label), "Item Equip not found in database", GUISkinGrid.label);
			return;
		}
		var charGame = AutoTileMap_Editor.Instance.Agent.GetComponentInChildren<CharGame>();
		if(item.SlugChar != charGame.SlugChar){
			GUI.Label(new Rect(xGui, yGui, wLeft, DefineAON.GAME_Height_Label), "Warning: Your character can't use this item.", GUISkinGrid.label);
			return;
		}
		Item currentItem = charGame.GetCurrentItem(item.ItemType);
		if(currentItem != null && item.SlugGlobal == currentItem.SlugGlobal){
			if(m_itemTryOn != null && currentItem.SlugGlobal == m_itemTryOn.SlugGlobal){
				GUI.Label(new Rect(xGui, yGui, wLeft, DefineAON.GAME_Height_Label), "Costumes is trying", GUISkinGrid.label);
				yGui -= 32f;
				if(GUI.Button(new Rect(yRightBt, yGui, wBtRight, DefineAON.GAME_Height_Label), "UnTry")){
					UnTryCostume(charGame);
					return;
				}
			}else
			{
				GUI.Label(new Rect(xGui, yGui, wLeft, DefineAON.GAME_Height_Label), "Costumes are in use", GUISkinGrid.label);
				return;
			}
		}else
		{
			yGui -= 32f;
			if(GUI.Button(new Rect(yRightBt, yGui, wBtRight, DefineAON.GAME_Height_Label), "Try on")){
				UnTryCostume(charGame);
				TryCostume(charGame, currentItem, item);
				return;
			}	
		}
	}

	public void ShowWithDataOnGUi(SerializablePropertys dataRaw, SerializablePackages.Package package, OnCloseShop onCloseShop){
		m_show = true;
		mOnCloseShop = onCloseShop;
		m_dataRaw = dataRaw;
		m_package = package;
		m_indextItemSeleted = 0;
		var propertys = m_package.propertys;
		m_GUI_Items_Left = new GUIContent[propertys.Count];
		m_GUI_Items_Right = new GUIContent[propertys.Count];
		for( int i = 0; i < propertys.Count; i++){
			var slug = propertys[i];
			if(string.IsNullOrEmpty(slug)){
				m_GUI_Items_Left[i] = new GUIContent("Item is empty");
			}else
			{
				var propertyData = m_dataRaw.PropertyBySlug(slug);
				if(propertyData == null){
					m_GUI_Items_Left[i] = new GUIContent("Item not found");
				}else if(!string.IsNullOrEmpty(propertyData.Name)){
					m_GUI_Items_Left[i] = new GUIContent(propertyData.Name);
				}else if(!string.IsNullOrEmpty(slug)){
					m_GUI_Items_Left[i] = new GUIContent(slug);
				}else
				{
					m_GUI_Items_Left[i] = new GUIContent("Name is NULL");
				}	
			}
			var payBy = m_package.payBys[i];
			if(payBy == null){
				m_GUI_Items_Right[i] = new GUIContent("NULL");
			}else
			{
				m_GUI_Items_Right[i] = new GUIContent(payBy.coin.ToString() + " $");	
			}
		}
	}

	*/

    private void TryCostume(CharGame charGame, Item raw, Item next)
    {
        PropertysGame.Instance.CheckItemBeforeEquip(charGame, next.ItemType);
        string error = "";
        if (charGame.AddEquipment(next, ref error) == false)
        {
            return;
        }
        m_itemRawTry = raw;
        m_itemTryOn = next;
    }

    public void UnTryCostume(CharGame charGame)
    {
        if (m_itemTryOn == null)
        {
            return;
        }
        if (charGame == null)
        {
            charGame = AutoTileMap_Editor.Instance.Agent.GetComponentInChildren<CharGame>();
        }
        if (m_itemRawTry == null)
        {
            charGame.RemoveEquipment(m_itemTryOn);
        }
        else
        {
            PropertysGame.Instance.CheckItemBeforeEquip(charGame, m_itemRawTry.ItemType);
            string error = "";
            if (charGame.AddEquipment(m_itemRawTry, ref error) == false)
            {
                return;
            }
        }
        m_itemRawTry = null;
        m_itemTryOn = null;
    }

    public void ShowWithData(SerializablePropertys dataRaw, SerializablePackages.Package package, CallBackClose onCloseShop, CallBackBuy onBuy)
    {
        // ShowWithDataOnGUi(dataRaw, package, onCloseShop);
        // Repair data
        // mOnCloseShop = onCloseShop;
        mOnBuy = onBuy;
        m_dataRaw = dataRaw;
        m_package = package;
        m_indextItemSeleted = 0;
        var propertys = m_package.data;
        string[] m_s_Left = new string[propertys.Count];
        string[] m_s_Right = new string[propertys.Count];
        for (int i = 0; i < propertys.Count; i++)
        {
            var slug = propertys[i].property;
            if (string.IsNullOrEmpty(slug))
            {
                m_s_Left[i] = "Item is empty";
            }
            else
            {
                var propertyData = m_dataRaw.PropertyBySlug(slug);
                m_s_Left[i] = NameOf(propertyData, slug);
            }
            var payBy = m_package.data[i].payBy;
            if (payBy == null)
            {
                m_s_Right[i] = "NULL";
            }
            else
            {
                m_s_Right[i] = payBy.coin.ToString() + " " + DefineAON.CoinName;
            }
        }
        // UI
        _controlShop_Dispose();
        if (_controlShop == null)
        {
            _controlShop = new ControlShop();
            _controlShop._btClose.onClick.Add(() =>
            {
                // _controlShop.Hide();
                _controlShop_Dispose();
                UnTryCostume(null);
                if (onCloseShop != null)
                {
                    var callback = onCloseShop;
                    onCloseShop = null;
                    callback();
                }
            });
        }
        else
        {
            _controlShop.ResetListItem();
        }
        _controlShop.SetTitle(m_package.Name);
        for (int i = 0; i < m_s_Left.Length; i++)
        {
            int ii = i;
            var bt = _controlShop.AddItem(m_s_Left[i], m_s_Right[i], () =>
            {
                SetSelectItem(ii);
            });
        }
        m_indextItemSeleted = -1;
        RefreshDataSelect();
        _controlShop.ShowOn(InputFieldHelper.Instance.PopUp);
    }

    private string NameOf(SerializablePropertys.Property property, string slug)
    {
        if (property == null)
        {
            return "Item not found";
        }
        else if (!string.IsNullOrEmpty(property.Name))
        {
            return property.Name;
        }
        else if (!string.IsNullOrEmpty(slug))
        {
            return slug;
        }
        return "Item name is NULL";
    }

    private void RefreshDataSelect()
    {
        _controlShop.ResetSelectItem();
        GTween.Kill(this);
        GTween.DelayedCall(0.02f).SetTarget(this).OnComplete(() =>
        {
            SetSelectItem(m_indextItemSeleted);
        });
    }

    private void SetSelectItem(int ixdex)
    {
        m_indextItemSeleted = ixdex;
        _controlShop.ResetSelectItem();
        if (ixdex == -1)
        {
            _controlShop.SetItemName("No item selected");
            return;
        }
        Debug.Log("SetSelectItem " + ixdex);
        var slug = m_package.data[ixdex].property;
        var payBy = m_package.data[ixdex].payBy;
        var property = m_dataRaw.PropertyBySlug(slug);
        if (property == null)
        {
            InputFieldHelper.Instance.ShowNoti("Item not found in database");
            return;
        }
        _controlShop.SetItemName(NameOf(property, slug));
        _controlShop.SetItemDes(property.Des);
        if (property.IsOutfit)
        {
            if(string.IsNullOrEmpty(property.RefSlug)){
                InputFieldHelper.Instance.ShowNoti("Item model is null");
                return;
            }
            var tryItem = AutoTileMap_Editor.Instance.ItemCharData.FetchItemByGlobalSlug(property.RefSlug);
            if (tryItem == null)
            {
                InputFieldHelper.Instance.ShowNoti("Item model not found in database");
                return;
            }
            _controlShop.LoadModelItem(tryItem.ItemPrefab);
            var charGame = AutoTileMap_Editor.Instance.Agent.GetComponentInChildren<CharGame>();
            if (tryItem.SlugChar != charGame.SlugChar)
            {
                _controlShop.SetItemDes("Your character can't use this item.");
                return;
            }
            if (PropertysGame.Instance.IsHaveItem(slug))
            {
                _controlShop.SetItemDes("You already own this item.");
                return;
            }
            Item rawItem = charGame.GetCurrentItem(tryItem.ItemType);
            if (rawItem != null && tryItem.SlugGlobal == rawItem.SlugGlobal)
            {
                if (m_itemTryOn != null && rawItem.SlugGlobal == m_itemTryOn.SlugGlobal)
                {
                    _controlShop.SetItemDes("Costumes is trying");
                    _controlShop.AddAction("UnTry", () =>
                    {
                        UnTryCostume(charGame);
                        RefreshDataSelect();
                    });
                }
                else
                {
                    _controlShop.SetItemDes("Costumes are in use");
                }
            }
            else
            {
                _controlShop.AddAction("Try on", () =>
                {
                    _tryOnItem(charGame, rawItem, tryItem, slug, property, payBy);
                });
            }
            _controlShop.AddAction("Buy", () =>
            {
                _onBuy(slug, property, payBy);
            });
        }
        else if (property.IsCertificates)
        {
            if (PropertysGame.Instance.IsHaveItem(slug))
            {
                _controlShop.SetItemDes("You already own this item.");
                return;
            }
            _controlShop.AddAction("Buy", () =>
            {
                Debug.Log("Buy");
                _onBuy(slug, property, payBy);
            });
        }
        else if (property.IsPet)
        {
            if(string.IsNullOrEmpty(property.RefSlug)){
                InputFieldHelper.Instance.ShowNoti("Item model is null");
                return;
            }
            if (PropertysGame.Instance.IsHaveItem(slug))
            {
                _controlShop.SetItemDes("You already own this item.");
                return;
            }
            _controlShop.LoadModelItem("pets/" + property.RefSlug);
            
            _controlShop.AddAction("Buy", () =>
            {
                _onBuy(slug, property, payBy);
            });
        }
        else
        {
            _controlShop.AddAction("Buy", () =>
            {
                _onBuy(slug, property, payBy);
            });
        }
        if (_controlShop.IsLoadModel())
        {
            _controlShop.LoadImageItem(null);
        }
        else
        {
            _controlShop.LoadImageItem(property.RefIcon);
        }

    }

    private void _tryOnItem(CharGame charGame, Item rawItem, Item tryItem, string slug, SerializablePropertys.Property property, SerializablePackages.PayBy payBy)
    {
        _controlShop.contentPane.visible = false;
        {
            // Cam
            var cam = AutoTileMap_Editor.Instance.CamControler;
            float _camXAngle_to = cam.camXAngle;
            float _y_to = cam.yCam;
            // float _y_to = 0;
            float _distance_to = cam.minDistance;
            Vector3 _target_to = charGame.transform.position;
            cam.MoveCamTo(_camXAngle_to, _y_to, _distance_to, _target_to, () =>
            {
                // cam.canControl = true;
                // cam.target = charGame.transform;
            });
        }
        UnTryCostume(charGame);
        TryCostume(charGame, rawItem, tryItem);
        {
            var b = AutoTileMap_Editor.Instance.Agent.GetComponent<BasicMecanimControl>();
            if (b != null)
            {
                b.TriggerTalk();
            }
        }
        var pm_choise = new QuickControlList();
        pm_choise.AddBt("Untry", (EventContext context) =>
        {
            // Debug.Log("Click Cancel");
            pm_choise.Dispose();
            UnTryCostume(charGame);
            _controlShop.contentPane.visible = true;
        });
        pm_choise.AddBt("Buy", (EventContext context) =>
        {
            // Debug.Log("Click Buy");
            pm_choise.Dispose();
            UnTryCostume(charGame);
            _onBuy(slug, property, payBy);
        });
        pm_choise.SetParent(InputFieldHelper.Instance.PopUp);
        // Hide chatBottom
        var chatBottom = InputFieldHelper.Instance.GComponent_ChatBottom;
        if (chatBottom != null && chatBottom.visible)
        {
            chatBottom.visible = false;
            pm_choise.SetOnDispose(() =>
            {
                chatBottom.visible = true;
            });
        }
    }


    private void _onBuy(string slug, SerializablePropertys.Property property, SerializablePackages.PayBy payBy)
    {
        if(payBy.coin <= 0){
            //Free
            _onBuy1(slug, property,payBy);
            return;
        }
        int currentCoin = TriggerGame.Instance.WorldFlag["Coin"];
        if(currentCoin < payBy.coin){
            string name_item = NameOf(property, slug);
            string text = string.Format("Sorry, {0} costs ${1} {2}, you don't have enough!", name_item, payBy.coin, DefineAON.CoinName);
            _controlShop.contentPane.visible = false;
            InputFieldHelper.Instance.ShowChatBottom(text, true, (TypingEffectByLine ty) =>
            {
                _controlShop.contentPane.visible = true;   
            });
        }else
        {
            TriggerGame.Instance.WorldFlag.DoAdd("Coin", -payBy.coin);
            _onBuy1(slug, property,payBy);
        }
    }

    private void _onBuy1(string slug, SerializablePropertys.Property property, SerializablePackages.PayBy payBy)
    {
        _controlShop.contentPane.visible = false;
        string name_item = NameOf(property, slug);
        // string last_text = InputFieldHelper.Instance.LastTextChatBottom();
        string text = string.Format("{0} costs ${1} {2}", name_item, payBy.coin, DefineAON.CoinName);
        text += "\nThank you very much for your purchase!";
        // text += string.Format("\nThe {0} was putted in your Bag!", name_item);
        InputFieldHelper.Instance.ShowChatBottom(text, true, (TypingEffectByLine ty) =>
        {
            PropertysGame.PropertyChar pChar = PropertysGame.Instance.AddItem(m_dataRaw, slug);
            RefreshDataSelect();
            if (property.IsOutfit || property.IsPet)
            {
                _onBuy2(name_item, slug, property, pChar);
            }
            else
            {
                if (mOnBuy != null)
                {
                    mOnBuy(_controlShop);
                }
            }
        });
    }

    private void _onBuy2(string name_item, string slug, SerializablePropertys.Property property, PropertysGame.PropertyChar pChar)
    {
        InputFieldHelper.Instance.ShowChatBottom("Do you want to equip now?", false, (TypingEffectByLine ty) =>
        {
            var pm_choise = new QuickControlList();
            pm_choise.AddBt("Equip", (EventContext context) =>
            {
                pm_choise.Dispose();
                _onEquipNow(name_item, slug, property, pChar);
            });
            pm_choise.AddBt("Later", (EventContext context) =>
            {
                pm_choise.Dispose();
                _onEquipLate(name_item, slug, property);
            });
            pm_choise.SetParent(InputFieldHelper.Instance.PopUp);
        });
    }

    private void _onEquipNow(string name_item, string slug, SerializablePropertys.Property property, PropertysGame.PropertyChar pChar)
    {
        if(property.IsOutfit){
            var charGame = AutoTileMap_Editor.Instance.Agent.GetComponentInChildren<CharGame>();
            PropertysGame.Instance.Equip(charGame, pChar);
        }else if(property.IsPet){
            PropertysGame.Instance.EquipPet(pChar);
        }

        string text = string.Format("{0} is equipped!", name_item);

        InputFieldHelper.Instance.ShowChatBottom(text, true, (TypingEffectByLine ty) =>
        {
            if (mOnBuy != null)
            {
                mOnBuy(_controlShop);
            }
        });
    }

    private void _onEquipLate(string name_item, string slug, SerializablePropertys.Property property)
    {
        string text = string.Format("{0} has been added in your bag!", name_item);
        InputFieldHelper.Instance.ShowChatBottom(text, true, (TypingEffectByLine ty) =>
        {
            if (mOnBuy != null)
            {
                mOnBuy(_controlShop);
            }
        });
    }

}
