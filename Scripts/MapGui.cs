using System;
using System.Collections;
using System.Collections.Generic;
using AON.RpgMapEditor;
using UnityEngine;

public class MapGui {

	private static MapGui _instance = null;
			
	public static MapGui Instance{ 
		get{
			if(_instance == null){
				_instance = new MapGui();
			}
			return _instance;
		}
	}

	private MapGui(){}

	private enum EMenuLeft : int{
		General = 0,
		FlagMap = 1,
		Action = 2,
		Warp = 3,
		NPC = 4,
		Script
	}
	private static string[] StrMenuLeft = Enum.GetNames (typeof(EMenuLeft));
	private EMenuLeft mMenuLeft = EMenuLeft.General;

	public void MapOnGUI( TilesetAON tilesetAON, AutoTileMapSerializeData d, AutoTileMap autoTileMap, AComponent_Button.OnClick closeDialog){
		float W_ScopeList = 200;
		float yGui = 0f;
		Rect rectMenuLeft = new Rect( 0, 0, W_ScopeList, Screen.height);
		AONGUI.Box(rectMenuLeft, "", tilesetAON.ListStyleBlack);
		{
			Rect rect = new Rect( 0, 0, W_ScopeList, Screen.height);
			yGui = 4f;
			AONGUI.Box(rect, "", tilesetAON.ListStyleBlack);
			AONGUI.Button( new Rect(4, yGui, 45, 26), "Back", closeDialog);
			AONGUI.Label( new Rect(54, yGui, W_ScopeList - 54, 26), "Map edit");
			yGui += 32f;
			OnGUIMenuLeft(tilesetAON, ref yGui, rectMenuLeft);
		}
		Rect rectContent = new Rect( W_ScopeList, 0, Screen.width - W_ScopeList, Screen.height);
		AONGUI.Box(rectContent, "");
		yGui = 0f;

		if(mMenuLeft == EMenuLeft.General){
			OnGuiGeneral( tilesetAON, autoTileMap, rectContent);
		}else if(mMenuLeft == EMenuLeft.FlagMap){
			FlagGui.DisOnGUI( d.FlagMap, ref yGui, rectContent, null, tilesetAON.ListStyleBlack2, "Flag map edit :");
		}else if(mMenuLeft == EMenuLeft.Action){
			// OnGUITriggerList(tilesetAON, d, eSlotAonTypeObj.Warps, DefineAON.IdSlot_Warps, ref yGui, rectContent);
			if(d.ListFlagAction == null){
				d.ListFlagAction = new List<FlagAction>();
			}
			if(d.ListFlagAction.Count == 0){
				d.ListFlagAction.Add( new FlagAction());
			}
			d.ListFlagAction[0].Name = "";
			d.ListFlagAction[0].Key = "";
			d.ListFlagAction[0].Value = 0;
			Rect rect = new Rect( W_ScopeList, 0, Screen.width - W_ScopeList, Screen.height);
			bool isWaitUI = false;
			if(FlagAction.OnGUIFlagActionList( ref yGui, ref isWaitUI, rectContent, d.ListFlagAction, d.FlagMap, tilesetAON.ListStyleBlack2)){
				return;
			}
		}else if(mMenuLeft == EMenuLeft.Warp){
			OnGUITriggerList(tilesetAON, d, eSlotAonTypeObj.Warps, DefineAON.IdSlot_Warps, ref yGui, rectContent);
		}else if(mMenuLeft == EMenuLeft.NPC){
			OnGUITriggerList(tilesetAON, d, eSlotAonTypeObj.Person, DefineAON.IdSlot_NPC, ref yGui, rectContent);
		}else if(mMenuLeft == EMenuLeft.Script){
			OnGUITriggerList(tilesetAON, d, eSlotAonTypeObj.Script, DefineAON.IdSlot_Script, ref yGui, rectContent);
		}
		// float yGui = 0f;
		// bool isUpdateFlag = FlagGui.DisOnGUI( ref d.FlagMap, ref yGui, rect, null, tilesetAON.ListStyleBlack2, "Flag map edit :");
	}

	private void OnGuiGeneral(TilesetAON tilesetAON, AutoTileMap autoTileMap, Rect rect){
		float yBeginGui = rect.y + 4f;
		AONGUI.Label(new Rect(rect.x+ 10f, yBeginGui, 200, DefineAON.GUI_Height_Label), "Map " + autoTileMap.MapIdxSelect);
		// yBeginGui += 32;
		AONGUI.Button( new Rect(rect.x + rect.width - 220, yBeginGui, 200, DefineAON.GUI_Height_Label), "Reset map", () => {
			tilesetAON._isShowDialogClearMap = true;
		});
		yBeginGui += 32;
		AONGUI.Label(new Rect(rect.x+ 10f, yBeginGui, rect.width - 8f, DefineAON.GUI_Height_Label), string.Format("Start at: ({0}, {1})", autoTileMap.MapSelect.StartX, autoTileMap.MapSelect.StartY));
		yBeginGui += 32;
		AONGUI.Button( new Rect(rect.x + 10f, yBeginGui, 200, DefineAON.GUI_Height_Button), "Pick start position", () => {
			tilesetAON.PickPosOnMap(autoTileMap.MapIdxSelect, autoTileMap.MapSelect.StartX, autoTileMap.MapSelect.StartY, (PickMapAON p, int _x, int _y) => {
				autoTileMap.MapSelect.StartX = _x;
				autoTileMap.MapSelect.StartY = _y;
			});
		});
	}

	private bool OnGUIMenuLeft( TilesetAON tilesetAON, ref float yGui, Rect rect){
		// Flag
		// yGui += 16;
		int current = (int)mMenuLeft;
		float h = StrMenuLeft.Length * 32;
		AONGUI.SelectionGrid(new Rect( rect.x, yGui, rect.width, h), current, StrMenuLeft, 1, tilesetAON.ListStyleGrid, (int next) => {
			mMenuLeft = (EMenuLeft) next;
			refTrigger = null;
		});
		yGui += h;
		return false;
	}

	private List<AutoTile> refTrigger = null;
	private int currentIdxTrigger = -1;
	private void OnGUITriggerList( TilesetAON tilesetAON, AutoTileMapSerializeData d, eSlotAonTypeObj e, int idSlotTrigger, ref float yGui, Rect rect){
		float W_ScopeList = 200;
		AONGUI.Label(new Rect( rect.x + 4, yGui + DefineAON.GUI_Y_Label, W_ScopeList, DefineAON.GUI_Height_Label ), e.ToString() + " list :");
		yGui += 32f;
		d.TriggerCountAt(e);
		var comboBoxTrigger = ComboBoxHelper.Instance.TypeObj( d, e);
		comboBoxTrigger.Rect.x = rect.x;
		comboBoxTrigger.Rect.y = yGui;
		comboBoxTrigger.Rect.width = W_ScopeList;
		comboBoxTrigger.Rect.height = 32f;
		comboBoxTrigger.SelectedItemIndex = currentIdxTrigger;
		comboBoxTrigger.Show( rect.height - yGui - 32f, "defause", true, false, (int idxTrigger) => {
			currentIdxTrigger = comboBoxTrigger.SelectedItemIndex;
			refTrigger = null;
		});
		
		var autoTileMap = AutoTileMap_Editor.Instance;
		if(autoTileMap == null || autoTileMap.MapSelect != d){
			return;
		}
		if(comboBoxTrigger.IsDropDownListVisible){
			//Create new
			AONGUI.Button( new Rect(rect.x, rect.y + rect.height - 32f, W_ScopeList, DefineAON.GUI_Height_Button), "Create new "  + e.ToString(), ()=>{
				autoTileMap.MapSelect.CreateNewTrigger(e);
			});
		}
		Trigger trigger = d.GetTriggerByIdxRef(currentIdxTrigger, e);
		if(trigger == null){
			return;
		}
		yGui = 4f;
		rect.x = rect.x + W_ScopeList + 10;
		if(string.IsNullOrEmpty(trigger.Name())){
			AONGUI.Label(new Rect(rect.x, yGui, rect.width, 32), "Name: NULL");
		}else
		{
			AONGUI.Label(new Rect(rect.x, yGui, rect.width, 32), "Name: " + trigger.Name());	
		}
		yGui += 32f;
		
		if(refTrigger == null){
			refTrigger = new List<AutoTile>();
			var triggerLink = d.TriggerLink;
			var tileMapWidth = autoTileMap.MapTileWidth;
			var tileMapHeight = autoTileMap.MapTileHeight;
			int layerTrigger = (int)eSlotAonTypeLayer.Trigger;
			for( int x = 0; x < tileMapWidth; x++){
				for (int y = 0; y < tileMapHeight; y++)
				{
					var a = autoTileMap.GetAutoTile(x,y, layerTrigger);
					if(a == null){
						continue;
					}
					if(a.Id < 0){
						continue;
					}
					int idSlot = a.Id;
					if(idSlot != idSlotTrigger){
						continue;
					}
					if(currentIdxTrigger != d.GetTriggerRef(x,y)){
						continue;
					}
					refTrigger.Add(a);
				}
			}
		}
		AONGUI.Label(new Rect(rect.x, yGui, rect.width, 32), "Reference on map: " + refTrigger.Count);
		yGui += 32f;
		AONGUI.Label(new Rect(rect.x, yGui, rect.width, 32), "----Edit----");
		yGui += 32f;
		if(e == eSlotAonTypeObj.Script){
			yGui += 16f;
			AONGUI.Button( new Rect(rect.x, yGui, 100, DefineAON.GUI_Height_Label), "Edit", () => {
				tilesetAON.TriggerShowMoreInfo = trigger;
			});
		}else
		{
			bool isShowMoreInfo = true;
			trigger.ShowGUI(new Rect(rect.x, yGui, rect.width, rect.y + rect.height - yGui), AutoTileMap_Editor.Instance, tilesetAON, ref isShowMoreInfo, null);
		}
	}
}
