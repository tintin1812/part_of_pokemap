using System;
using System.Collections;
using System.Collections.Generic;
using AON.RpgMapEditor;
using UnityEngine;

public class ComboBoxHelper {

	private static ComboBoxHelper _instance = null;
	public static ComboBoxHelper Instance{ 
		get{
			if(_instance == null){
				_instance = new ComboBoxHelper();
			}
			return _instance;
		}
	}
	public static void ResetInstance(){
		Debug.Log("ComboBoxHelper ResetInstance");
		_instance = null;
		ComboBox.ResetInstance();
	}
	
	private ComboBoxHelper(){
		_showIndexScope = PlayerPrefs.GetInt("Setting_ShowIndexScope", 0) == 1;
	}

	private ComboBox _filer3DList = null;
	private int _filer3DIdRef = -99;
	public ComboBox Filer3DList(AutoTileMap autoTileMap, int idRef){
		if(_filer3DIdRef != idRef){
			_filer3DIdRef = idRef;
			_filer3DList = null;
		}
		if(_filer3DList == null){
			var modelList = autoTileMap.Tileset.Filer3DListByIdRef(idRef);
			_filer3DList = ComboBox.CreateComboBox(0, modelList, "Pick model filler...");
		}
		return _filer3DList;
	}

	private ComboBox _interiorList = null;
	public ComboBox InteriorList(AutoTileMap autoTileMap){
		if(_interiorList == null){
			var modelList = autoTileMap.Tileset.InteriorList;
			_interiorList = ComboBox.CreateComboBox(0, modelList, "Pick model interior...");
		}
		return _interiorList;
	}

	private ComboBox _houseListModel = null;
	public ComboBox HouseListModel(AutoTileMap autoTileMap){
		if(_houseListModel == null){
			var modelList = autoTileMap.Tileset.HouseList;
			_houseListModel = ComboBox.CreateComboBox(0, modelList, "Pick model house...");
		}
		return _houseListModel;
	}
	
	private ComboBox _mapList = null;
	public void UpdateDataMapList( AutoTileMap autoTileMap){
		var data = autoTileMap.MapsData.Maps;
		var comboBoxMapList = new GUIContent[data.Count];
		for (int i = 0; i < data.Count; ++i)
		{
			comboBoxMapList[i] = new GUIContent("Map: " + i.ToString());
		}
		_mapList = new ComboBox(new Rect(0, 0, 150, 20), comboBoxMapList);
	}
	public ComboBox MapList( AutoTileMap autoTileMap){
		if(_mapList == null){
			UpdateDataMapList( autoTileMap);
		}
		return _mapList;
	}

	/*
	private ComboBox _NPCList = null;
	public void ResetDataNPCList(){
		_NPCList = null;
	}
	private void UpdateDataNPCList( AutoTileMap autoTileMap){
		var data = autoTileMap.MapSelect.NPCData;
		var g = new GUIContent[data.Count];
		for (int i = 0; i < data.Count; ++i)
		{
			g[i] = new GUIContent( string.Format( "NPC ({0}) {1}", i,  data[i].NameNpc));
		}
		_NPCList = new ComboBox(new Rect(0, 0, 150, 20), g.Length > 0 ? g[0] : null, g);
	}
	public ComboBox NPCList( AutoTileMap autoTileMap){
		if(_NPCList == null){
			UpdateDataNPCList( autoTileMap);
		}
		return _NPCList;
	}
	*/
	public ComboBox NPCList( AutoTileMapSerializeData map){
		return TypeObj( map, eSlotAonTypeObj.Person);
	}

	// private ComboBox _HouseListMap = null;
	// public void ResetDataHouseListMap(){
		// _HouseListMap = null;
	// }
	// private void UpdateDataHouseListMap( AutoTileMap autoTileMap){
	// 	var data = autoTileMap.MapSelect.HouseData;
	// 	var g = new GUIContent[data.Count];
	// 	for (int i = 0; i < data.Count; ++i)
	// 	{
	// 		g[i] = new GUIContent( string.Format( "House ({0}) {1}", i,  data[i].NameHouse));
	// 	}
	// 	_HouseListMap = new ComboBox(new Rect(0, 0, 150, 20), g.Length > 0 ? g[0] : null, g);
	// }
	public ComboBox HouseListMap( AutoTileMapSerializeData map){
		return TypeObj( map, eSlotAonTypeObj.House);
	}

	public ComboBox WarpListMap( AutoTileMapSerializeData map){
		return TypeObj( map, eSlotAonTypeObj.Warps);
	}

	private ComboBox _formatScope = null;
	public ComboBox FormatScope(){
		if(_formatScope == null){
			string[] data = Enum.GetNames (typeof(ScriptGui.EFormatScope));
			_formatScope = ComboBox.CreateComboBox(0, data, "Pick format scope...");
		}
		return _formatScope;
	}

	private ComboBox _scopeList = null;
	private bool _showIndexScope = false;
	public bool ShowIndexScope{
		get{
			return _showIndexScope;
		}
		set{
			if(_showIndexScope != value){
				_showIndexScope = value;
				PlayerPrefs.SetInt("Setting_ShowIndexScope", _showIndexScope ? 1 : 0);
				ResetDataScopeList();
			}
		}
	}
	
	public void ResetDataScopeList(){
		_scopeList = null;
	}

	private void UpdateDataScopeList( List<ScriptGui.ActionData> data){
		var comboBoxList = new GUIContent[data.Count];
		for (int i = 0; i < data.Count; ++i)
		{
			// string name = (actionData.ContainsKey("name") && actionData["name"] != null ? actionData["name"] : "");
			if(i == 0){
				comboBoxList[i] = new GUIContent( "NULL");
			}else{
				ScriptGui.ActionData actionData = data[i];
				string name = actionData.Name;
				if(_showIndexScope){
					comboBoxList[i] = new GUIContent( string.Format("{0}. {1}", i, name));
				}else
				{
					comboBoxList[i] = new GUIContent( name);
				}
			}
		}
		if(_scopeList == null){
			_scopeList = new ComboBox(new Rect(0, 0, 150, 20), comboBoxList);
		}else
		{
			_scopeList.ListContent = comboBoxList;	
		}
	}
	public ComboBox ScopeList( List<ScriptGui.ActionData> data){
		// if(_scopeList == null){
			// UpdateDataScopeList( data);	
		// }
		UpdateDataScopeList( data);
		return _scopeList;
	}
	public void UpdateScopeName( int i, string s){
		if( i > 0 && i < _scopeList.ListContent.Length){
			if(_showIndexScope){
				_scopeList.ListContent[i] = new GUIContent( string.Format("{0}. {1}", i, s));	
			}else
			{
				_scopeList.ListContent[i] = new GUIContent(s);
			}
		}
	}

	// private ComboBox _ECharacter = null;
	// public ComboBox ECharacter(){
	// 	if(_ECharacter == null){
	// 		string[] data = Enum.GetNames (typeof(ScriptGui.ECharacter));
	// 		_ECharacter = ComboBox.CreateComboBox(0, data, "Pick ...");
	// 	}
	// 	return _ECharacter;
	// }

	private ComboBox _NPCModel = null;
	public ComboBox NPCModel(AutoTileMap autoTileMap){
		if(_NPCModel == null){
			var modelList = autoTileMap.Tileset.NPCModel;
			_NPCModel = ComboBox.CreateComboBox(0, modelList, "Pick model NPC...");
		}
		return _NPCModel;
	}

	/*
	private ComboBox _script = null;
	public void ResetDataScriptList(){
		_script = null;
	}
	public ComboBox Scripts(AutoTileMap autoTileMap){
		if(_script == null){
			List<AutoTileMapSerializeData.Script> ScriptData = autoTileMap.MapSelect.ScriptData;
			var comboBoxList = new GUIContent[ScriptData.Count];
			for (int i = 0; i < ScriptData.Count; ++i)
			{
				var s = ScriptData[i];
				comboBoxList[i] = new GUIContent(string.Format("({0}) {1}", i.ToString(), s.Name()));
			}
			_script = new ComboBox(new Rect(0, 0, 150, 20), comboBoxList[0], comboBoxList, new GUIContent("Pick Script..."));
		}
		return _script;
	}
	 */
	public ComboBox Scripts(AutoTileMapSerializeData map){
		return TypeObj( map, eSlotAonTypeObj.Script);
	}

	private ComboBox _NPCAction = null;
	public ComboBox NPCAction(){
		if(_NPCAction == null){
			string[] data = Enum.GetNames (typeof(ScriptGui.NPCAction.EAction));
			_NPCAction = ComboBox.CreateComboBox(0, data, "Pick action...");
		}
		return _NPCAction;
	}

	private ComboBox _MainCharAction = null;
	public ComboBox MainCharAction(){
		if(_MainCharAction == null){
			string[] data = Enum.GetNames (typeof(ScriptGui.MainCharAction.EAction));
			_MainCharAction = ComboBox.CreateComboBox(0, data, "Pick action...");
		}
		return _MainCharAction;
	}

	private ComboBox _flagAction = null;
	public ComboBox FlagAction(List<FlagAction> data){
		if(_flagAction == null){
			_flagAction = new ComboBox(Rect.zero, new GUIContent[0]);
		}
		if(data == null || data.Count == 0){
			_flagAction.UpdateContentLength(0);
		}else
		{
			_flagAction.UpdateContentLength(data.Count);
			for( int i = 0; i < data.Count; i ++){
				if(string.IsNullOrEmpty(data[i].Name)){
					_flagAction.ListContent[i].text = "NULL";
				}else
				{
					_flagAction.ListContent[i].text = data[i].Name;	
				}
			}
		}
		return _flagAction;
	}
	
	/*
	private ComboBox _actionData = null;
	public ComboBox ActionData(List<ScriptGui.ActionData> data){
		if(_actionData == null){
			_actionData = new ComboBox(Rect.zero, new GUIContent[0]);
		}
		_actionData.UpdateContentLength(data.Count);
		for( int i = 0; i < data.Count; i ++){
			_actionData.ListContent[i].text = data[i].name;
		}
		return _actionData;
	}
	*/

	private ComboBox _AnimationNPC = null;
	public ComboBox AnimationNPC(AutoTileMap autoTileMap){
		if(_AnimationNPC == null){
			var modelList = autoTileMap.Tileset.NPCModel;
			_AnimationNPC = ComboBox.CreateComboBox(0, modelList, "Pick model NPC...");
		}
		return _AnimationNPC;
	}

	private Dictionary< eSlotAonTypeObj, ComboBox> _CacheCType = new Dictionary< eSlotAonTypeObj, ComboBox>();
	public void ResetTypeObj( eSlotAonTypeObj typeObj){
		if(_CacheCType.ContainsKey(typeObj) == true){
			_CacheCType[typeObj] = null;
		}
	}

	public ComboBox TypeObj( AutoTileMapSerializeData map, eSlotAonTypeObj typeObj){
		if(_CacheCType.ContainsKey(typeObj) == false || _CacheCType[typeObj] == null){
			ComboBox c = CreateByTypeObj( map, typeObj);
			_CacheCType[typeObj] = c;
			return c;
		}
		return _CacheCType[typeObj];
	}

	private ComboBox CreateByTypeObj( AutoTileMapSerializeData map, eSlotAonTypeObj typeObj){
		if(typeObj == eSlotAonTypeObj.House){
			var count = map.OverlayCountAt(typeObj);
			var g = new GUIContent[count];
			for (int i = 0; i < count; ++i)
			{
				var t = map.GetOverlayByIdxRef(i, typeObj);
				g[i] = new GUIContent( string.Format( "({0}) {1}", i,  t.Name()));
			}
			var c = new ComboBox(new Rect(0, 0, 150, 20), g);
			return c;
		}else{
			var count = map.TriggerCountAt(typeObj);
			var g = new GUIContent[count];
			for (int i = 0; i < count; ++i)
			{
				var t = map.GetTriggerByIdxRef(i, typeObj);
				g[i] = new GUIContent( string.Format( "({0}) {1}", i,  t.Name()));
			}
			var c = new ComboBox(new Rect(0, 0, 150, 20), g);
			return c;
		}
	}

	private ComboBox _Number = null;
	public ComboBox Number(int count){
		if(_Number != null && _Number.ListContent.Length != count){
			_Number = null;
		}
		if(_Number == null){
			var g = new GUIContent[count];
			for (int i = 0; i < count; ++i)
			{
				g[i] = new GUIContent( i.ToString());
			}
			_Number = new ComboBox(new Rect(0, 0, 150, 20), g);
		}
		return _Number;
	}

	private ComboBox _Operation = null;
	public ComboBox Operation(){
		if(_Operation == null){
			string[] data = ScriptGui.StrEOperation;
			_Operation = ComboBox.CreateComboBox(0, data, "");
		}
		return _Operation;
	}

	private ComboBox _ECompare = null;
	public ComboBox ECompare(){
		if(_ECompare == null){
			string[] data = ScriptGui.StrECompare;
			_ECompare = ComboBox.CreateComboBox(0, data, "");
		}
		return _ECompare;
	}

	private ComboBox _StringN = null;
	public ComboBox StringN(string[] data){
		if(_StringN == null){
			_StringN = new ComboBox(Rect.zero, new GUIContent[0]);
		}
		if(data == null || data.Length == 0){
			_StringN.UpdateContentLength(0);
		}else
		{
			_StringN.UpdateContentLength(data.Length);
			for( int i = 0; i < data.Length; i ++){
				_StringN.ListContent[i].text = data[i];
			}
		}
		return _StringN;
	}

	public int IndextOfStringN ( string[] data, string key){
		if(key == null || key == ""){
			return -1;
		}
		for(int k = 0; k < data.Length; k++){
			if(data[k] == key){
				return k;
			}
		}
		return -1;
	}

	private ComboBox _Choise_Hightlight = null;
	public ComboBox Choise_Hightlight(){
		if(_Choise_Hightlight == null){
			string[] data = ScriptGui.MsgboxChoise.StrTypeHighlight;
			_Choise_Hightlight = ComboBox.CreateComboBox(0, data, "");
		}
		return _Choise_Hightlight;
	}
}
