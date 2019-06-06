using System.Collections;
using System.Collections.Generic;
using AON.RpgMapEditor;
using FairyGUI;
using UnityEngine;
using UnityEngine.AI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class TriggerMapData{

	public int MapIdx{ get; set;}
	private AutoTileMapSerializeData MapData{
		get{
			return AutoTileMap_Editor.Instance.MapAt(MapIdx);
		}
	}

	private Dictionary<int, ScriptGui.ScriptYaml> _cacheYaml = null;
	public ScriptGui.ScriptYaml GetOnCache(int triggerRef){

		if( triggerRef < 0 || triggerRef >= MapData.ScriptData.Count){
			return null;
		}
		ScriptGui.ScriptYaml r = null;
		if( _cacheYaml == null){
			_cacheYaml = new Dictionary<int, ScriptGui.ScriptYaml>();
		}
		if(_cacheYaml.ContainsKey(triggerRef)){
			r = _cacheYaml[triggerRef];
		}else{
			try{
				var scriptData = MapData.ScriptData;
				var script = scriptData[triggerRef];
				// var deserializer = new DeserializerBuilder()
				// .WithNamingConvention(new CamelCaseNamingConvention())
				// .IgnoreUnmatchedProperties()
				// .Build();
				// var s = deserializer.Deserialize<ScriptGui.ScriptYaml>(script.YamlData);
				// s.IdToObj();
				// r = s;
				script.ScriptYaml.IdToObj();
				r = script.ScriptYaml;
				// if(script.YamlData != null){
				// 	r = (ScriptGui.ScriptYaml)UtilsAON.DeepCopy(script.ScriptYaml);
				// }
			}
			catch{
			}
			if( r != null){
				_cacheYaml.Add(triggerRef, r);
			}
		}
		return r;
	}

	private ScriptGame GetScriptGameCache( ref Dictionary<string, ScriptGame> _s, TriggerDetail t, bool forceCreate){
		if(_s == null){
			_s = new Dictionary<string, ScriptGame>();
		}
		string key = t.Hash;
		if(_s.ContainsKey(key)){
			return _s[key];
		}
		if(forceCreate == false){
			return null;
		}
		var s = new ScriptGame( t);
		_s.Add(key, s);
		return s;
	}
	private Dictionary<string, ScriptGame> _s_npc_begin = null;
	private Dictionary<string, ScriptGame> _s_npc_ingame = null;
	private Dictionary<string, ScriptGame> _s_script = null;
	public ScriptGame GetScriptGameCache_NPC_Begin(TriggerDetail t, bool forceCreate){
		return GetScriptGameCache(ref _s_npc_begin, t, forceCreate);
	}
	public ScriptGame GetScriptGameCache_NPC_Ingame(TriggerDetail t, bool forceCreate){
		return GetScriptGameCache(ref _s_npc_ingame, t, forceCreate);
	}
	public ScriptGame GetScriptGameCache_Script(TriggerDetail t, bool forceCreate){
		return GetScriptGameCache(ref _s_script, t, forceCreate);
	}
	

	private Dictionary<string, Flags> _Flags;
	public Flags MapFlag(string hash, ScriptGui.ScriptYaml script){
		if(_Flags == null){
			_Flags = new Dictionary<string, Flags>();
		}
		if(!_Flags.ContainsKey(hash)){
			if(script.FlagsYaml != null){
				_Flags[hash] = script.FlagsYaml.Clone();
			}else
			{
				_Flags[hash] = null;
			}
			
		}
		return _Flags[hash];
	}
}

public class TriggerGameData{

	private Flags _worldFlag;
	public Flags WorldFlag{
		get{
			if(_worldFlag == null){
				_worldFlag = AutoTileMap_Editor.Instance.MapsData.FlagWorld.Clone();
			}
			return _worldFlag;
		}
	}
	
	private Dictionary<int, Flags> _mapFlag;
	public Flags MapFlag(int mapIdx){
		if(_mapFlag == null){
			_mapFlag = new Dictionary<int, Flags>();
		}
		if(_mapFlag.ContainsKey(mapIdx) == false){
			_mapFlag[mapIdx] = AutoTileMap_Editor.Instance.MapAt(mapIdx).FlagMap.Clone();;
		}
		return _mapFlag[mapIdx];
	}

	public Flags ScriptFlag(TriggerDetail triggerDetal, ScriptGui.ScriptYaml script){
		return MapDataAt(triggerDetal.refMap).MapFlag(triggerDetal.Hash, script);
	}

	public string NpcCurrentFaceTo{get;set;}
	private ScriptGame _s_main = null;
	public ScriptGame ScriptMain{
		set{
			_s_main = value;
			// Reset when assign ScriptMain
			NpcCurrentFaceTo = "";
		}
		get{
			// if(_s_main == null){
			// 	_s_main = new ScriptGame("Main");
			// }
			return _s_main;
		}
	}
	public bool IsScriptMainRunning{
		get{
			if(_s_main == null){
				return false;
			}
			return _s_main.IsRun;
		}
	}
	private List<TriggerDetail> _listNpcUsingInMainScript;
	protected List<TriggerDetail> ListNpcUsingInMainScript{
		set{
			_listNpcUsingInMainScript = value;
		}
		get{
			if(_listNpcUsingInMainScript == null){
				_listNpcUsingInMainScript = new List<TriggerDetail>();
			}
			return _listNpcUsingInMainScript;
		}
	}
	protected bool IsHaveListNpcUsingInMainScript{
		get{
			return _listNpcUsingInMainScript != null;
		}
	}
	
	private Dictionary<int, TriggerMapData> _mapData;
	private TriggerMapData MapDataAt(int mapIdx){
		if(_mapData == null){
			_mapData = new Dictionary<int, TriggerMapData>();
		}
		if(_mapData.ContainsKey(mapIdx) == false){
			_mapData[mapIdx] = new TriggerMapData(){
				MapIdx = mapIdx
			};
		}
		return _mapData[mapIdx];
	}

	protected ScriptGui.ScriptYaml GetRawScript(int mapIdx, int triggerRef){
		return MapDataAt(mapIdx).GetOnCache(triggerRef);
	}
	
	public ScriptGame GetScriptGameCache_NPC_Begin( GameObject obj, bool forceCreate){
		TriggerDetailBehaviour r = obj.GetComponent<TriggerDetailBehaviour>();
		return MapDataAt(r.Data.refMap).GetScriptGameCache_NPC_Begin(r.Data, forceCreate);
	}

	public ScriptGame GetScriptGameCache_NPC_Ingame( GameObject obj, bool forceCreate){
		TriggerDetailBehaviour r = obj.GetComponent<TriggerDetailBehaviour>();
		return MapDataAt(r.Data.refMap).GetScriptGameCache_NPC_Ingame(r.Data, forceCreate);
	}
	public ScriptGame GetScriptGameCache_NPC_Ingame( TriggerDetailBehaviour r, bool forceCreate){
		return MapDataAt(r.Data.refMap).GetScriptGameCache_NPC_Ingame(r.Data, forceCreate);
	}

	public ScriptGame GetScriptGameCache_Script( GameObject obj, bool forceCreate){
		TriggerDetailBehaviour r = obj.GetComponent<TriggerDetailBehaviour>();
		return MapDataAt(r.Data.refMap).GetScriptGameCache_Script(r.Data, forceCreate);
	}
}

public delegate void OnMainCharWasToCheckPoint();
public class PackageWaitCheckPoint{
	public ScriptGame ScriptGame;
	public int X;
	public int Y;
	public OnMainCharWasToCheckPoint Callback;
};

public class TriggerGame : TriggerGameData{
	
	//--------  Static -------------//
	private static TriggerGame _instance = null;
		
	public static TriggerGame Instance{ 
		get{
			if(_instance == null){
				_instance = new TriggerGame();
			}
			return _instance;
		}
	}

	public static void ResetCache(){
		Debug.Log("TriggerGame ResetCache");
		_instance = null;
	}
	
	//--------  Non static -------------//

	private ConversationGame _MSG_Helper;
    public ConversationGame MSG{
		get { 
			if(_MSG_Helper == null){
				_MSG_Helper = new ConversationGame();
			}
			return _MSG_Helper;
		}
	}
	
	private GUISkin GUISkinIngame = Resources.Load("GUI/InGame") as GUISkin;

	//Show Tip OnGUI in Game
	private string mTip = "";

    private PackageWaitCheckPoint mPackageWaitCheckPoint = null;

	// Using when Wait Interaction to Door House
	public delegate void OnMainCharEnterHouse( int houseRef);
    private OnMainCharEnterHouse mOnWaitMainCharEnterHouse = null;
	private int houseRefWait = -1;

	// Using when Wait Interaction to Npc
    public delegate bool OnInteractionNPC( int refNPCTarget);
    private OnInteractionNPC mOnInteractionNPC = null;
	private string mNPCRefWait = "";

	// Using when Show button Talk to Npc
	// private string mBtTextWait = "";
	private GameObject mTargetWait = null;
	private int mIdxScriptWait = -1;

	// Using OnGUI in Game
	private Vector2 mScrollPos = Vector2.zero;
	private bool mShowDebug = false;
	private ScriptGui.FlagTarget mTabFlagDebug = ScriptGui.FlagTarget.Script;

	public void OnScriptEnter( GameObject scriptTarget, int triggerRef){
		if( IsScriptMainRunning || triggerRef < 0 || triggerRef >= AutoTileMap_Editor.Instance.MapSelect.ScriptData.Count){
			return;
		}
		int mapIdx = AutoTileMap_Editor.Instance.MapIdxSelect;
		var script = GetRawScript( mapIdx, triggerRef);
		if(script == null){
			return;
		}
		// Stop MainChar
		var nav = AutoTileMap_Editor.Instance.Agent.GetComponent<NavMeshAgentCallback>();
		nav.ResetPath();
		//
		ResetLogChat();
		ScriptMain = GetScriptGameCache_Script(scriptTarget, true);
		ScriptMain.StartOnMain( mapIdx, triggerRef, script, null, (ScriptGame scriptGame) => {
			ResumeAndClearScriptWait();
			ClearNpcHasAdd();
			ScriptMain = null;
		});
	}

	public void StartMsg(ScriptGame scriptGame, ScriptGui.ActionData msg, int countStack){
		MSG.StartMsg(scriptGame, msg, countStack);
	}

	public void OnNPCEnter( GameObject npcTarget, int refNPCTarget){
		// Check Wait InteractionNPC
		if(mOnInteractionNPC != null && !string.IsNullOrEmpty(mNPCRefWait) && npcTarget.name.IndexOf(mNPCRefWait) == 0){
			var action = mOnInteractionNPC;
			mOnInteractionNPC = null;
			mNPCRefWait = null;
			if( action( refNPCTarget)){
				return;
			}
		}
		if(IsScriptMainRunning || mTargetWait != null){
			return;
		}
		if(refNPCTarget >= 0 && refNPCTarget < AutoTileMap_Editor.Instance.MapSelect.NPCData.Count){
			// if(nav.hasPath == false){
			// 	return;
			// }
			var npc = AutoTileMap_Editor.Instance.MapSelect.NPCData[refNPCTarget];
			if(npc.RunScript && npc.IdxScript >= 0 && npc.IdxScript < AutoTileMap_Editor.Instance.MapSelect.ScriptData.Count){
				string btTextWait;
				if(npc.nameNpc != null && npc.nameNpc.Length > 0){
					btTextWait = "Talk to " + npc.nameNpc;
				}else
				{
					btTextWait = "Talk";
				}
				mTargetWait = npcTarget;
				mIdxScriptWait = npc.IdxScript;
				CreateObjInteractionNPC(npcTarget);
				// Pause target
				int mapIdx = AutoTileMap_Editor.Instance.MapIdxSelect;
				ScriptGame s_from_npc = GetScriptGameCache_NPC_Begin( npcTarget, false);
				AddScriptNeedResume(s_from_npc);
				// Lock to NPC
				NpcLookatDCallback.InstanceOf(npcTarget).LookTo(AutoTileMap_Editor.Instance.Agent.transform, null);
				// Stop MainChar
				var nav = AutoTileMap_Editor.Instance.Agent.GetComponent<NavMeshAgentCallback>();
				nav.ResetPath();
				//
				InputFieldHelper.Instance.Show_Menu_BtTalk(btTextWait, () => {
					ActiveScriptGameWait();
				});
			}
		}
	}

	public void ActiveScriptGameWait(){
		if(IsScriptMainRunning){
			return;
		}
		int mapIdx = AutoTileMap_Editor.Instance.MapIdxSelect;
		var script = GetRawScript( mapIdx, mIdxScriptWait);
		if(script == null){
			return;
		}
		//Pause target
		ScriptGame s_from_npc = GetScriptGameCache_NPC_Begin( mTargetWait, false);
		AddScriptNeedResume(s_from_npc);
		//
		var target = mTargetWait;
		var tdxScript = mIdxScriptWait;
		// Reset Wait
		mTargetWait = null;
		mIdxScriptWait = -1;
		ResetObjInteraction();
		ResetLogChat();
		InputFieldHelper.Instance.Hide_Menu_BtTalk();
		ScriptMain = GetScriptGameCache_NPC_Ingame( target, true);
		ScriptMain.StartOnMain( mapIdx, tdxScript, script, target, (ScriptGame scriptGame) => {
			ResumeAndClearScriptWait();
			ClearNpcHasAdd();
			ScriptMain = null;
		});
	}

	/*
	public void OnNPCInHouseEnter( int refNPCTarget){
		if(mOnInteractionNPC != null && refNPCTarget == mNPCRefWait){
			var c = mOnInteractionNPC;
			mOnInteractionNPC = null;
			if(c( refNPCTarget)){
				return;
			}
		}
		if(refNPCTarget >= 0 && refNPCTarget < AutoTileMap_Editor.Instance.MapSelect.NPCData.Count){
			var nav = AutoTileMap_Editor.Instance.Agent.GetComponent<NavMeshAgent>();
			if(nav.hasPath == false){
				return;
			}
			var npc = AutoTileMap_Editor.Instance.MapSelect.NPCData[refNPCTarget];
			if(npc.RunScript && npc.IdxScript >= 0 && npc.IdxScript < AutoTileMap_Editor.Instance.MapSelect.ScriptData.Count){
				// OnScriptEnter( npc.IdxScript);
				var script = GetOnCache( npc.IdxScript);
				if(script != null){
					ScriptGame s = GetScriptGameCache(AutoTileMap_Editor.Instance.Agent.transform);
					if(s.IsRun == false){
						//Begin script
						mScriptMain = s;
						s.StartOnMain( npc.IdxScript, script, null, (ScriptGame scriptGame) => {
							mScriptMain = null;
						});
					}
				}
			}
		}
	}
	*/

	public void OnNPCExit(GameObject npcTarget){
		if(npcTarget != mTargetWait){
			return;
		}
		if(!IsScriptMainRunning){
			//If is not main ScriptActive -> Check and resume Move & Lock for NPC
			//Resume target
			int mapIdx = AutoTileMap_Editor.Instance.MapIdxSelect;
			ScriptGame s_from_npc = GetScriptGameCache_NPC_Begin(mTargetWait, false);
			RemoveScriptWaitResume(s_from_npc);
		}
		// Reset Wait
		mTargetWait = null;
		mIdxScriptWait = -1;
		ResetObjInteraction();
		InputFieldHelper.Instance.Hide_Menu_BtTalk();
	}

	// public void OnItemEnter( GameObject itemTarget, string itemName){
	// 	Debug.Log("OnItemEnter: " + itemName);
	// 	if(itemName == "WaitMoveToPos"){
	// 		//Get Main ScriptGame
	// 		ScriptGame s = GetScriptGameCache(AutoTileMap_Editor.Instance.Agent.transform);
	// 		if(s.Script != null){
	// 			s.OnItemEnter( itemTarget, itemName);
	// 		}
	// 	}
	// }

	public void CallBack_CreateNPC( TriggerDetail detail, int idNpc, GameObject npcTarget, NPC npcData, int tileX, int tileY){
		if (detail.isCreateFromMap)
		{
			if(npcData.StartScript && npcData.IdxStartScript >= 0 && npcData.IdxStartScript < AutoTileMap_Editor.Instance.MapSelect.ScriptData.Count){
				int mapIdx = AutoTileMap_Editor.Instance.MapIdxSelect;
				var script = GetRawScript( mapIdx, npcData.IdxStartScript);
				if(script != null){
					ScriptGame s = GetScriptGameCache_NPC_Begin(npcTarget, true);
					s.StartNormal(mapIdx, npcData.IdxStartScript, script, npcTarget);
				}
			}
		}else
		{
			ListNpcUsingInMainScript.Add(detail);
		}

		if(npcData.RunScript && npcData.IdxScript >= 0 && npcData.IdxScript < AutoTileMap_Editor.Instance.MapSelect.ScriptData.Count){
			int mapIdx = AutoTileMap_Editor.Instance.MapIdxSelect;
			var script = GetRawScript( mapIdx, npcData.IdxScript);
			if(script != null){
				var interaction = npcTarget.GetComponent<InteractionCheck>();
				if (interaction == null){
					interaction = npcTarget.AddComponent<InteractionCheck>();
					interaction.Init();
				}
				var flags = TriggerGame.Instance.ScriptFlag( detail, script);
				if(flags != null){
					flags.AddEventListener_RemoveByFlag(ScriptGuiBase.ScriptYaml.Key_ShowInteractions,()=>{
						interaction.IsShowInteractions = ScriptGuiBase.ScriptYaml.IsShowInteractions(flags);
					});
				}
			}
		}

		// Check Wait InteractionNPC
		if(mOnInteractionNPC != null && !string.IsNullOrEmpty(mNPCRefWait) && npcTarget.name.IndexOf(mNPCRefWait) == 0){
			CreateObjInteractionNPC(npcTarget);
		}
	}

	public void CallBack_AffterWarps(){
		if(IsHaveListNpcUsingInMainScript){
			for (int i = 0; i < ListNpcUsingInMainScript.Count; i++)
			{
				var detail = ListNpcUsingInMainScript[i];
				AutoTileMap_Editor.Instance.ShowNPC(detail);
			}
		}
		if(mPackageWaitCheckPoint != null){
			CreateWaitGoToCheckPoint(mPackageWaitCheckPoint);
		}
	}

	public void OnGUI() {
		// Debug.Log("TriggerGame OnGUI");
		var lastSkin = AONGUI.skin;
		AONGUI.skin = GUISkinIngame;
		// mConversationGame.OnGUIChat();
		OnGUITip();
		// OnGUIWaitInteraction();
		AONGUI.skin = lastSkin;
	}
	
	public void OnGUITip() {
		if(mTip != null && mTip != ""){
			float w = 400;
			float widthContent = w - 16;
			var listStyleLabel = AONGUI.skin.label;
			GUIContent g = new GUIContent(mTip);
			float h_title = listStyleLabel.CalcHeight( g, widthContent) + 8 + 8;
			float h = h_title + 16;
			Rect rect = new Rect((Screen.width - w) / 2, Screen.height - 24f - h, w, h);
			AONGUI.Box( rect, "");
			AONGUI.Label(new Rect(rect.x + 8, rect.y + 8, widthContent, h_title), g);
		}
	}

	/*
	public void OnGUIWaitInteraction() {
		if(mScriptGameWait != null){

			string str = mBtTextWait;

			float w_title = GUI.skin.button.CalcSize( new GUIContent(str)).x + 4;

			var rectBt = new Rect(Screen.width - 16 - w_title, Screen.height - 16 - DefineAON.GAME_Height_Label,  w_title, DefineAON.GAME_Height_Label);

			GameGui.IgnoreMouseByBox( rectBt);

			if(GUI.Button( rectBt, str) || Input.GetKey(KeyCode.T)){
				ActiveScriptGameWait();
			}
		}
	}
	*/

	public void OnUpdate(){
		// if(mCacheGame == null)
		// 	return;
		// foreach(var v in mCacheGame.Values){
		// 	v.Update();
		// }
	}
	
	public void OnGUIDebug( TilesetAON tilesetAON) {
		if(!mShowDebug){
			var rectBt = new Rect(8, Screen.height - 8 - DefineAON.GUI_Height_Button, 60, DefineAON.GUI_Height_Button);
			GameGui.SetRectIgnore(rectBt);
			AONGUI.Button( rectBt, "Debug", ()=> {
				mShowDebug = true;
			});
			return;
		}
		float w = 380;
		float limitHeight = 350;
		Rect rect = new Rect( 8, Screen.height - limitHeight - 8, w, limitHeight);
		AONGUI.Box( rect, "" );
		GameGui.SetRectIgnore(rect);
		string[] d = ScriptGui.StrFlagTarget;
		int current_target = (int)mTabFlagDebug;
		rect.x = rect.x + 8f;
		rect.width = rect.width - 16f;
		AONGUI.SelectionGrid(new Rect( rect.x , rect.y + 6, rect.width, 26), current_target, d, d.Length, tilesetAON.ListStyleGrid, (int next) => {
			mTabFlagDebug = (ScriptGui.FlagTarget) next;
		});
		
		rect.y = rect.y + 32f;
		rect.height = rect.height - 64f;
		if(mTabFlagDebug == ScriptGui.FlagTarget.World){
			FlagGui.OnGuiDebug( WorldFlag, rect, limitHeight);
		}else if(mTabFlagDebug == ScriptGui.FlagTarget.Map){
			int mapIdx = AutoTileMap_Editor.Instance.MapIdxSelect;
			FlagGui.OnGuiDebug( MapFlag(mapIdx), rect, limitHeight);	
		}else //mFlagDebug == ScriptGui.FlagTarget.Script
		{
			if(IsScriptMainRunning){
				FlagGui.OnGuiDebug(ScriptMain.Flags, rect, limitHeight);
			}else{
				AONGUI.Label(new Rect(rect.x, rect.y + DefineAON.GUI_Y_Label, 150, DefineAON.GUI_Height_Label), "No script target");
			}
		}
		float yGui = Screen.height - 36;
		float xGui = 12;
		{
			var rectBt = new Rect(xGui, yGui, 50, 24);
			AONGUI.Button( rectBt, "Hide", ()=>{
				mShowDebug = false;
			});
			xGui += 54;
		}
		{
			var rectBt = new Rect(xGui, yGui, 50, 24);
			AONGUI.Button( rectBt, AutoTileMap_Editor.Instance.DayNight.Hour.ToString() + "h +1", () => {
				AutoTileMap_Editor.Instance.UpdateTime1Hour();
			});
			xGui += 54;
		}
		{
			var rectBt = new Rect(xGui, yGui, 50, 24);
			AONGUI.Button( rectBt, "Base", () => {
				AutoTileMap_Editor.Instance.NextBase();
			});
			xGui += 54;
		}
		{
			var rectBt = new Rect(xGui, yGui, 50, 24);
			AONGUI.Button( rectBt, "Skin", () => {
				AutoTileMap_Editor.Instance.NextSkin();
			});
			xGui += 54;
		}
		{
			var rectBt = new Rect(xGui, yGui, 50, 24);
			AONGUI.Button( rectBt, "Cos", () => {
				AutoTileMap_Editor.Instance.NextCostume();
			});
			xGui += 54;
		}
		{
			var rectBt = new Rect(xGui, yGui, 100, 24);
			AONGUI.Toggle(rectBt, !AutoTileMap_Editor.Instance.CamControler.canControl, " Lock cam", (bool b) => {
				AutoTileMap_Editor.Instance.CamControler.canControl = b;
			});
			xGui += 104;
		}
	}
	
	public bool IsMainChar_Wait_To_Interaction(){
		if(mPackageWaitCheckPoint != null
		|| mOnWaitMainCharEnterHouse != null
		|| mOnInteractionNPC != null){
			return true;
		}
		return false;
	}
	public bool WaitGoToCheckPoint( ScriptGame scriptGame, int x, int y, OnMainCharWasToCheckPoint callBack){
		if(IsMainChar_Wait_To_Interaction()){
			return false;
		}
		var packageWaitCheckPoint = new PackageWaitCheckPoint{
			ScriptGame = scriptGame,
			X = x,
			Y = y,
			Callback = callBack
		};
		return CreateWaitGoToCheckPoint(packageWaitCheckPoint);
	}
	public bool CreateWaitGoToCheckPoint( PackageWaitCheckPoint data){
		mPackageWaitCheckPoint = data;
		GameObject checkPoint = AutoTileMap_Editor.Instance.CreateItem( "OBJ/CheckPoint", data.X, data.Y);
		if(checkPoint == null){
			return false;
		}
		checkPoint.name = "Item_MainCharAction_WaitMoveToPos";
		var col = checkPoint.AddComponent<BoxCollider>();
		col.center = new Vector3(0, -0.5f, 0);
		col.isTrigger =true;
		checkPoint.AddComponent<CollisionCallback>().mOnMainCharEnter = (Collider c) => {
			if(checkPoint != null){
				GameObject.Destroy(checkPoint);
			}
			var action = mPackageWaitCheckPoint.Callback;
			mPackageWaitCheckPoint = null;
			action();
		};
		return true;
	}

	public void OnHouseEnter( int houseRef){
		if(mOnWaitMainCharEnterHouse != null && houseRefWait == houseRef){
			var action = mOnWaitMainCharEnterHouse;
			mOnWaitMainCharEnterHouse = null;
			action( houseRef);
		}
	}

	public bool WaitEnterHouse( ScriptGame scriptGame, ScriptGui.MainCharAction data, OnMainCharEnterHouse callBack){
		if(IsMainChar_Wait_To_Interaction()){
			return false;
		}
		GameObject prefab = Resources.Load<GameObject>("OBJ/CheckPoint");
		if(prefab == null){
			return false;
		}
		List<GameObject> checkPointList = new List<GameObject>();
		{
			// Add CheckPoint
			var triggerNode = AutoTileMap_Editor.Instance.TriggerNode;
			string hash = "House_" + data.IdHouse;
			for ( int i = 0; i < triggerNode.childCount; i++){
				var c = triggerNode.GetChild(i);
				Debug.Log(c.name);
				if(c.name.IndexOf(hash) >= 0){
					var obj = GameObject.Instantiate(prefab);
					obj.transform.parent = c;
					obj.transform.localRotation = Quaternion.identity;
					obj.transform.localPosition = new Vector3( 0 , 0, 0);
					checkPointList.Add( obj);
				}
			}
		}
		if(checkPointList.Count == 0){
			return false;
		}
		houseRefWait = data.IdHouse;
		mOnWaitMainCharEnterHouse = ( int houseRef) => {
			while(checkPointList.Count > 0){
				var o = checkPointList[0];
				checkPointList.RemoveAt(0);
				o.transform.parent = null;
				GameObject.Destroy(o);
			}
			callBack(houseRef);
		};
		return true;
	}

	public bool WaitInteractionNPC( ScriptGame scriptGame, ScriptGui.MainCharAction data, OnInteractionNPC callBack){
		if(IsMainChar_Wait_To_Interaction()){
			return false;
		}
		ResetObjInteraction();
		mNPCRefWait = eSlotAonTypeObj.Person + "_" + data.IdNpc + "_" + AutoTileMap_Editor.Instance.MapIdxSelect;
		CreateObjInteractionNPC( mNPCRefWait);
		mOnInteractionNPC = ( int refNPCTarget) => {
			ResetObjInteraction();
			return callBack( refNPCTarget);
		};
		return true;
	}

	private List<GameObject> mCheckPointList = null;
	private void CreateObjInteractionNPC( string hash){
		if(mCheckPointList == null){
			mCheckPointList = new List<GameObject>();
		}
		{ // Add CheckPoint
			GameObject prefab = Resources.Load<GameObject>("OBJ/CheckPoint_HighLight");
			var triggerNode = AutoTileMap_Editor.Instance.TriggerNode;
			// string hash1 = "Person_" + idNpc + "_FromTile";
			// string hash2 = "Person_" + idNpc;
			// string hash = "Person_" + idNpc;
			for ( int i = 0; i < triggerNode.childCount; i++){
				var c = triggerNode.GetChild(i);
				// if(c.name == hash1 || c.name == hash2){
				if(c.name.IndexOf(hash) == 0){
					var obj = GameObject.Instantiate(prefab);
					obj.transform.parent = c;
					obj.transform.localRotation = Quaternion.identity;
					obj.transform.localPosition = new Vector3( 0 , 0, 0);
					mCheckPointList.Add(obj);
				}
			}
		}
	}
	private void CreateObjInteractionNPC( GameObject target)
	{
		if(mCheckPointList == null){
			mCheckPointList = new List<GameObject>();
		}
		GameObject prefab = Resources.Load<GameObject>("OBJ/CheckPoint_HighLight");
		var obj = GameObject.Instantiate(prefab);
		obj.transform.parent = target.transform;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localPosition = new Vector3( 0 , 0, 0);
		mCheckPointList.Add( obj);
	}

	private void ResetObjInteraction(){
		if( mCheckPointList == null){
			return;
		}
		while(mCheckPointList.Count > 0){
			var o = mCheckPointList[0];
			mCheckPointList.RemoveAt(0);
			if(o != null){
				o.transform.parent = null;
				GameObject.Destroy(o);
			}
		}
		mCheckPointList = null;
	}


	public void SetTip( string tip){
		mTip = tip;
	}

	public void ResetLogChat(){
		MSG.ResetLogChat();
	}

	// public delegate void OnCallBack( TriggerGame g);
	// private IEnumerator DoDelay1Frame(OnCallBack calback){
	// 	yield return null;
	// 	calback( this);
	// 	yield break;
	// }

	public int CurrentCoin{
		get{
			return WorldFlag["Coin"];
		}
	}

	private List<ScriptGame> mScriptsWaitResume = new List<ScriptGame>();

	public void AddScriptNeedResume(ScriptGame s){
		if(s != null && s.IsRun){
			s.PauseAction();
			mScriptsWaitResume.Add(s);
		}
	}

	private void RemoveScriptWaitResume(ScriptGame s){
		if(s != null){
			mScriptsWaitResume.Remove(s);
			s.ResumeAction();
		}
	}

	private void ResumeAndClearScriptWait(){
		foreach (var s in mScriptsWaitResume)
		{
			s.ResumeAction();
		}
		mScriptsWaitResume.Clear();
	}

	private void ClearNpcHasAdd(){
		if(IsHaveListNpcUsingInMainScript){
			for (int i = 0; i < ListNpcUsingInMainScript.Count; i++)
			{
				var detail = ListNpcUsingInMainScript[i];
				AutoTileMap_Editor.Instance.RemoveNPC(detail);
			}
			ListNpcUsingInMainScript = null;
		}
	}
}
