using System.Collections;
using System.Collections.Generic;
using AON.RpgMapEditor;
using UnityEngine;

public class PropertysGUI {

	private static PropertysGUI _instance = null;
			
	public static PropertysGUI Instance{ 
		get{
			if(_instance == null){
				_instance = new PropertysGUI();
			}
			return _instance;
		}
	}

	// private string SlugCurrentTarget = "";
	private ComboBox comboBoxSlug = new ComboBox(new Rect(200,0,200,32), null, new GUIContent("Not selected"));
	private int slugIndext = -1;

	public ComboBox ComboBoxSlug (SerializablePropertys data){
		comboBoxSlug.UpdateListContent( data == null ? null : data.AllKey);
		return comboBoxSlug;
	}

	public delegate void PickItem(string slug);
	public void PickSlugItem(string hash, SerializablePropertys data, string slugCurrent, float x, float y, float w, ref float yGui, ref bool isWaitUI, PickItem onPick){
		int idProperty = data.IndexOf(slugCurrent);
		var comboBox = PropertysGUI.Instance.ComboBoxSlug(data);
		if (slugCurrent == null || slugCurrent == "")
		{
			comboBox.Empty = "NULL";
		}
		else
		{
			comboBox.Empty = slugCurrent + " (Not found)";
		}
		comboBox.SelectedItemIndex = idProperty;
		comboBox.Rect.x = x;
		comboBox.Rect.y = y;
		comboBox.Rect.width = w;
		comboBox.Rect.height = 32f;
		float limitHeight = 32f * 20;
		comboBox.Show( limitHeight, hash, (int idNext) => {
			onPick(data.AllKey[idNext]);
		});
		if (comboBox.IsDropDownWithHash(hash))
		{
			yGui += limitHeight;
			isWaitUI = true;
			return;
		}
	}

	private string slugNameAdd = "";

	public void OnGUI(SerializablePropertys data, List<FlagAction> listFlagAction, TilesetAON tilesetAON, Rect rect){
		float height_top = 68;
		AONGUI.Box(new Rect(rect.x, rect.y, rect.width, height_top), "", tilesetAON.ListStyleBlack2);
		AONGUI.Button(new Rect( rect.x + rect.width - 170, rect.y + DefineAON.GUI_Y_Button, 160, DefineAON.GUI_Height_Button), "Import full from Resource", () => {
			_importAllFromResource(data);
		});
		if(OnTopMenu( data, tilesetAON, new Rect( rect.x, rect.y, rect.width, rect.height ))){
			return;
		}
		if(OnGuiBot( data, listFlagAction, tilesetAON, new Rect(rect.x, height_top, rect.width, rect.height))){
			return;
		}
	}

	private bool OnTopMenu( SerializablePropertys data, TilesetAON tilesetAON, Rect rect){
		float widthLeft = 200;
		//menu
		float yGui = rect.y;
		AONGUI.Label(new Rect( rect.x + 4, yGui + DefineAON.GUI_Y_Label, widthLeft, DefineAON.GUI_Height_Label ), "Property edit :");
		yGui += 32f;
		float xGui = rect.x + 4;
		AONGUI.Label(new Rect( xGui, yGui + DefineAON.GUI_Y_Label, 90, DefineAON.GUI_Height_Label ), "Slug property");
		xGui += 94;
		AONGUI.TextField(new Rect( xGui, yGui + DefineAON.GUI_Y_TextField, 200, DefineAON.GUI_Height_TextField ), slugNameAdd, (string text) => {
			slugNameAdd = text;
		});
		
		xGui += 204;
		if(slugNameAdd.Length == 0){
			// GUI.Label(new Rect( rect.x, yGui + DefineAON.GUI_Y_Label, 200, DefineAON.GUI_Height_Label ), "Input slug property");
		}else
		{
			bool isUnique = true;
			var keys = data.AllKey;
			for( int i = 0; i < keys.Count; i++){
				if( keys[i] == slugNameAdd){
					isUnique = false;
					break;
				}
			}
			if(isUnique){
				AONGUI.Button( new Rect(xGui, yGui + DefineAON.GUI_Y_Button, 80, DefineAON.GUI_Height_Button), "Add (Enter)", KeyCode.Return, () => {
					data.Add( slugNameAdd);
					slugNameAdd = "";
				});
			}else
			{
				AONGUI.Label(new Rect( xGui, yGui + DefineAON.GUI_Y_Label, widthLeft, DefineAON.GUI_Height_Label ), "Slug should be unique");
			}
		}
		return false;
	}

	private bool OnGuiBot( SerializablePropertys data, List<FlagAction> listFlagAction, TilesetAON tilesetAON, Rect rect){
		if(data == null){
			return false;
		}
		float yGui = rect.y + 4;
		float widthLeft = 200;
		AONGUI.Label(new Rect( rect.x + 4, yGui + DefineAON.GUI_Y_Label, widthLeft, DefineAON.GUI_Height_Label ), "Edit property:");
		yGui += 32f;
		{
			comboBoxSlug.UpdateListContent( data.AllKey);
			comboBoxSlug.Empty = "Not selected";
			comboBoxSlug.SelectedItemIndex = slugIndext;
			comboBoxSlug.Rect.x = rect.x;
			comboBoxSlug.Rect.y = yGui;
			comboBoxSlug.Rect.width = widthLeft;
			comboBoxSlug.Rect.height = 32f;
			comboBoxSlug.Show( rect.height - yGui, "defause", true, false, (int next) => {
				slugIndext = next;
			});
		}
		// if(comboBoxSlug.IsDropDownListVisible){
		// 	return true;
		// }
		yGui = rect.y + 4;
		rect.x = rect.x + widthLeft + 4;
		if( slugIndext < 0 || slugIndext >= data.AllKey.Count){
			return true;
		}

		AONGUI.Button(new Rect( rect.x, yGui + DefineAON.GUI_Y_Button, 120, DefineAON.GUI_Height_Button), "Remove by slug", () => {
			data.Remove(slugIndext);
		});

		SerializablePropertys.Property property = data.PropertyByIndex(slugIndext);
		AONGUI.Button(new Rect( rect.x + 130, yGui + DefineAON.GUI_Y_Button, 120, DefineAON.GUI_Height_Button), "Duplicate by slug", () => {
			var n = data.Copy(slugIndext);
			if(n >= 0){
				slugIndext = n;
			}
		});
		yGui += 32f;
		AONGUI.Label(new Rect( rect.x, yGui + DefineAON.GUI_Y_Label, 40, DefineAON.GUI_Height_Label ), "Name");
		AONGUI.TextField(new Rect( rect.x + 40, yGui + DefineAON.GUI_Y_TextField, widthLeft - 40, DefineAON.GUI_Height_TextField ), property.Name, (string text) => {
			property.Name = text;
		});
		yGui += 32f;

		AONGUI.Label(new Rect( rect.x, yGui + DefineAON.GUI_Y_Label, 40, DefineAON.GUI_Height_Label ), "Des");
		AONGUI.TextField(new Rect( rect.x + 40, yGui + DefineAON.GUI_Y_TextField, widthLeft - 40, DefineAON.GUI_Height_TextField ), property.Des, (string text) => {
			property.Des = text;
		});
		yGui += 32f;
		
		string[] strType = SerializablePropertys.StrEType;
		AONGUI.SelectionGrid(new Rect( rect.x, yGui, rect.width, 24f ), (int)property._Type, strType, strType.Length, tilesetAON.ListStyleGrid, (int next) => {
			var tNext = (SerializablePropertys.EType)next;
			if(tNext != property._Type){
				property._Type = tNext;
			}
		});
		
		yGui += 32f;

		// if(property.IsItem){
		// 	property.StackInBag = GUI.Toggle(new Rect( rect.x, yGui + DefineAON.GUI_Y_Label, widthLeft, DefineAON.GUI_Height_Label ), property.StackInBag, "Can Stack in bag");
		// 	yGui += 32f;
		// }

		// property.CanEquip = GUI.Toggle(new Rect( rect.x, yGui + DefineAON.GUI_Y_Label, widthLeft, DefineAON.GUI_Height_Label ), property.CanEquip, " Can Equip");
		if(property.IsOutfit){
			// float xGui = rect.x + widthLeft + 4;
			float xGui = rect.x;
			AONGUI.Label(new Rect( xGui, yGui + DefineAON.GUI_Y_Label, 60, DefineAON.GUI_Height_Label ), "Item :");
			xGui += 60;
			string[] str = AutoTileMap_Editor.Instance.ItemCharData.StrItemList;
			var combobox = ComboBoxHelper.Instance.StringN( str);
			var hash = "item";
			int currentItem = ComboBoxHelper.Instance.IndextOfStringN( str, property.RefSlug);
			if(currentItem == -1){
				if(property.RefSlug != null && property.RefSlug != ""){
					combobox.Empty = property.RefSlug + " (not found)";
				}else
				{
					combobox.Empty = "NULL";
				}
			}
			combobox.SelectedItemIndex = currentItem;
			combobox.Rect.x = xGui;
			combobox.Rect.y = yGui;
			combobox.Rect.width = widthLeft;
			combobox.Rect.height = 32f;
			float limitHeight = 32f * 6;
			combobox.Show( limitHeight, hash, (int nextItem) => {
				property.RefSlug = str[nextItem];
			});
			if(combobox.IsDropDownWithHash( hash)){
				yGui += limitHeight;
				return false;
			}
			yGui += 32f;
		}
		
		// property.CanUsing = GUI.Toggle(new Rect( rect.x, yGui + DefineAON.GUI_Y_Label, widthLeft, DefineAON.GUI_Height_Label ), property.CanUsing, " Can Using");
		if(property.IsItem){
			// float xGui = rect.x + widthLeft + 4;
			float xGui = rect.x;
			AONGUI.Label(new Rect( xGui, yGui + DefineAON.GUI_Y_Label, 60, DefineAON.GUI_Height_Label ), "Action :");
			xGui += 60;
			var combobox = ComboBoxHelper.Instance.FlagAction( listFlagAction);
			var hash = "property";
			int currentFlagAction = FlagAction.IndextFlagAction( listFlagAction, property.ActionUsing);
			if(currentFlagAction == -1){
				if(property.ActionUsing != null && property.ActionUsing != ""){
					combobox.Empty = property.ActionUsing + " (not found)";
				}else
				{
					combobox.Empty = "NULL";
				}
			}
			combobox.SelectedItemIndex = currentFlagAction;
			combobox.Rect.x = xGui;
			combobox.Rect.y = yGui;
			combobox.Rect.width = widthLeft;
			combobox.Rect.height = 32f;
			float limitHeight = 32f * 6;
			combobox.Show( limitHeight, hash, (int nextAction) => {
				property.ActionUsing = listFlagAction[nextAction].Name;
			});
			if(combobox.IsDropDownWithHash( hash)){
				yGui += limitHeight;
				return false;
			}
			yGui += 32f;
		}
		
		// property.Consume = GUI.Toggle(new Rect( rect.x, yGui + DefineAON.GUI_Y_Label, widthLeft, DefineAON.GUI_Height_Label ), property.Consume, "Consumable");
		// yGui += 32f;

		if(property.IsPet){
			AONGUI.Label(new Rect( rect.x, yGui + DefineAON.GUI_Y_Label, 40, DefineAON.GUI_Height_Label ), "Pet");
			AONGUI.TextField(new Rect( rect.x + 40, yGui + DefineAON.GUI_Y_TextField, widthLeft - 40, DefineAON.GUI_Height_TextField ), property.RefSlug, (string text) => {
				property.RefSlug = text;
			});
			
			AONGUI.Button(new Rect( rect.x + widthLeft + 10, yGui + DefineAON.GUI_Y_TextField, 40, DefineAON.GUI_Height_TextField ), "Pick", () => {
				InputFieldHelper.Instance.ShowPickModel((string topic, string pet) => {
					Debug.Log(topic + "/" + pet);
					property.RefSlug = topic + "/" + pet;
					InputFieldHelper.Instance.HidePickModel();
				});	
			});
			yGui += 32f;	
		}

		if(property.IsItem || property.IsCertificates){
			AONGUI.Label(new Rect( rect.x, yGui + DefineAON.GUI_Y_Label, 40, DefineAON.GUI_Height_Label ), "Icon");
			AONGUI.TextField(new Rect( rect.x + 40, yGui + DefineAON.GUI_Y_TextField, widthLeft - 40, DefineAON.GUI_Height_TextField ), property.RefIcon, (string text) => {
				property.RefIcon = text;
			});
			AONGUI.Button(new Rect( rect.x + widthLeft + 10, yGui + DefineAON.GUI_Y_TextField, 40, DefineAON.GUI_Height_TextField ), "Pick", () => {
				InputFieldHelper.Instance.ShowPickIcon((string topic, string icon) => {
					// Debug.Log(topic + "/" + icon);
					property.RefIcon = topic + "/" + icon;
					InputFieldHelper.Instance.HidePickIcon();
				});
			});
			yGui += 32f;
		}
		return false;
	}

	private void _importAllFromResource(SerializablePropertys property){
		// Pet
		var petlist = PetsDatabase.Instance.PetList;
		foreach (var pets in petlist)
		{
			var topic = pets.topic;
			var d = pets.data;
			foreach (var s in d)
			{
				string key = "pet/" + s;
				var p = property.PropertyBySlug(key);
				if(p == null){
					p = property.Add(key);
				}
				p._Type = SerializablePropertys.EType.Pet;
				p.Name = UtilsAON.NameFormat(s);
				if(string.IsNullOrEmpty(p.Des)){
					p.Des = UtilsAON.DesFormat(s);
				}
				p.RefSlug = topic + "/" + s;
			}
		}
		// Outfit
		// string[] str = AutoTileMap_Editor.Instance.ItemCharData.StrItemList;
		Item[] item = AutoTileMap_Editor.Instance.ItemCharData.ItemList;
		foreach (var i in item)
		{
			string s = i.SlugGlobal;
			string key = "outfit/" + s;
			var p = property.PropertyBySlug(key);
			if(p == null){
				p = property.Add(key);
			}
			p._Type = SerializablePropertys.EType.Outfit;
			p.Name = UtilsAON.NameFormat(i.Slug);
			if(string.IsNullOrEmpty(p.Des)){
				p.Des = UtilsAON.DesFormat(i.Slug);
			}
			p.RefSlug = s;
		}
	}
}
