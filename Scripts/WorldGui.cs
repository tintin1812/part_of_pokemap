using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AON.RpgMapEditor;
using PygmyMonkey.FileBrowser;
using UnityEngine;

public class WorldGui {

	private static WorldGui _instance = null;
			
	public static WorldGui Instance{ 
		get{
			if(_instance == null){
				_instance = new WorldGui();
			}
			return _instance;
		}
	}

	private enum EMenuLeft : int{
		FlagWorld = 0,
		ActionWorld = 1,
		Property = 2,
		Package = 3,
	}
	private static string[] StrMenuLeft = Enum.GetNames (typeof(EMenuLeft));
	private EMenuLeft mMenuLeft = EMenuLeft.FlagWorld;

	public void WorldOnGUI( TilesetAON tilesetAON, AutoTileMapData d, AComponent_Button.OnClick onCloseDialog){
		// Rect rect
		Rect rectFull = new Rect( 0, 0, Screen.width, Screen.height);
		AONGUI.Box(rectFull, "");
		float W_ScopeList = 200;
		{
			Rect rect = new Rect( 0, 0, W_ScopeList, Screen.height);
			float yGui = 4f;
			AONGUI.Box(rect, "", tilesetAON.ListStyleBlack);
			AONGUI.Button( new Rect(4, yGui, 45, 26), "Back", onCloseDialog);
			AONGUI.Label( new Rect(54, yGui, W_ScopeList - 54, 26), "World edit");
			yGui += 32f;
			float w = (W_ScopeList) / 2;
			AONGUI.Button( new Rect(4, yGui, w - 8, 26), "Save", () => {
				string title = "Save world data ( not include map)";
				FileBrowser.SaveFilePanel( title, title, Application.persistentDataPath, "worlddata.json", new string[] { "json"}, null, (bool canceled, string filePath) => {
					if (canceled)
					{
						return;
					}
					File.WriteAllText(filePath, d.GetDataWorld( false));
				});
			});
			AONGUI.Button( new Rect(w + 4, yGui, w - 8, 26), "Load", () => {
				var title = "Load world data ( not include map)";
				var path = Application.persistentDataPath + "/worlddata.json";
				FileBrowser.OpenFilePanel(title, path, new string[] { "json"}, null, (bool canceled, string filePath) => {
					if (canceled)
					{
						return;
					}
					if(File.Exists(filePath)){
						var data = File.ReadAllText(filePath);
						if(d.LoadDataWorld(data, false) == false){
							InputFieldHelper.Instance.ShowNoti("Load error");
						}
					}
				});
			});
			yGui += 4f;
			yGui += 32f;
			OnGUIMenuLeft(tilesetAON, ref yGui, new Rect(0, 0, W_ScopeList, rectFull.height));
		}
		if(mMenuLeft == EMenuLeft.FlagWorld){
			Rect rect = new Rect( W_ScopeList, 0, Screen.width - W_ScopeList, Screen.height);
			float yGui = 0f;
			FlagGui.DisOnGUI(d.FlagWorld, ref yGui, rect, AutoTileMapData.LockFlagWorld, tilesetAON.ListStyleBlack2, "Flag world edit :");
		}else if(mMenuLeft == EMenuLeft.ActionWorld){
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
			float yGui = 0f;
			bool isWaitUI = false;
			if(FlagAction.OnGUIFlagActionList( ref yGui, ref isWaitUI, rect, d.ListFlagAction, d.FlagWorld, tilesetAON.ListStyleBlack2)){
				return;
			}
		}else if(mMenuLeft == EMenuLeft.Property){
			Rect rect = new Rect( W_ScopeList, 0, Screen.width - W_ScopeList, Screen.height);
			PropertysGUI.Instance.OnGUI( d.Propertys, d.ListFlagAction, tilesetAON, rect);
		}else if(mMenuLeft == EMenuLeft.Package){
			Rect rect = new Rect( W_ScopeList, 0, Screen.width - W_ScopeList, Screen.height);
			PackagesGUI.Instance.OnGUI( d.Packages, d.Propertys, d.ListFlagAction, tilesetAON, rect);
		}
	}

	private bool OnGUIMenuLeft( TilesetAON tilesetAON, ref float yGui, Rect rect){
		// Flag
		// yGui += 16;
		int current = (int)mMenuLeft;
		float h = StrMenuLeft.Length * 32;
		AONGUI.SelectionGrid(new Rect( rect.x, yGui, rect.width, h), current, StrMenuLeft, 1, tilesetAON.ListStyleGrid, (int next) => {
			mMenuLeft = (EMenuLeft) next;
		});
		yGui += h;
		return false;
	}
}
