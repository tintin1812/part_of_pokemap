using System.Collections;
using System.Collections.Generic;
using AON.RpgMapEditor;
using UnityEngine;
using UnityEngine.AI;

public class ScriptGame {

	private TriggerDetail Detail{ get; set;}
	private string Key{ get; set;}
	public ScriptGame(TriggerDetail t){
		Detail = t;
		Key = t.Hash;
	}

	private ScriptGui.ScriptYaml _scriptTarget{ get; set;}
	public ScriptGui.ScriptYaml Script{
		get{
			return _scriptTarget;
		}
		set{
			if(_scriptTarget == null){
				_scriptTarget = value;
				// _flagsYaml = null;
			}else if(_scriptTarget != value){
				Debug.LogError("Script is change");
			}
		}
	}

	private Flags _flagsYaml = null;
	public Flags Flags{
		get{
			if(_flagsYaml == null){
				_flagsYaml = TriggerGame.Instance.ScriptFlag( Detail, Script);
			}
			return _flagsYaml;
		}
	}

	private void ResetValueFlag(){
		// Reset flag
		if(_flagsYaml != null && _scriptTarget != null && _scriptTarget.FlagsYaml != null && _scriptTarget.FlagReset != null){
			var baseFlags = _scriptTarget.FlagsYaml;
			var data = _flagsYaml;
			foreach( string k in _scriptTarget.FlagReset){
				if(!baseFlags.ContainsKey(k) || !data.ContainsKey(k))
					continue;
				var d = baseFlags[k];
				data[k] = d;
			}
		}
	}

	private Flags FlagByTarget(int target){
		if( target == (int) ScriptGui.FlagTarget.World){
			return TriggerGame.Instance.WorldFlag;
		}
		if( target == (int) ScriptGui.FlagTarget.Map){
			return TriggerGame.Instance.MapFlag(IdxMap);
		}
		// Scirpt
		return Flags;
	}

	private List<FlagAction> ListFlagActionByTarget(int target){
		if(target == (int)ScriptGui.FlagTarget.World){
			return AutoTileMap_Editor.Instance.MapsData.ListFlagAction;
		}
		if(target == (int)ScriptGui.FlagTarget.Map){
			return AutoTileMap_Editor.Instance.MapAt(IdxMap).ListFlagAction;
		}
		return Script.FlagActions;
	}
	
	private int mScopeIndext = 0;
	private ScriptGui.NPCAction mActionWait = null;
	private bool mIsRun = false;
	public bool IsRun{
		get{return mIsRun;}
	}
	
	private bool mIsMain = false;
	public bool IsMain{
		get{return mIsMain;}
	}

	// With Normal NpcTarget is NPC start; With Main NpcTarget is NPC talker
	private GameObject _OBJTarget_Cache = null;
	private GameObject OBJTarget{
		set{
			_OBJTarget_Cache = null;
		}
		get{
			if(_OBJTarget_Cache == null){
				var t = AutoTileMap_Editor.Instance.GetNPC(Detail);
				if(t != null){
					_OBJTarget_Cache = t.gameObject;
				}
			}
			return _OBJTarget_Cache;
		}
	}
	public GameObject Talker{
		get{
			if(!mIsMain){
				return null;
			}
			return OBJTarget;
		}
	}

	private int IdxMap{ get; set;}
	public int IdxScript{ get; set;}

	public delegate void OnEndAction( ScriptGame s);
	public OnEndAction mOnEndAction = null;
	
	public void StartNormal( int idxMap, int idxScript, ScriptGui.ScriptYaml script, GameObject npcTarget, OnEndAction onEndAction = null){
		// if(mIsRun){
		// 	Debug.Log("Can't start action when running");
		// 	return;
		// }
		Debug.Log("Start Normal Action " + Key + " ,idxScript: " + idxScript);
		IdxMap = idxMap;
		IdxScript = idxScript;
		Script = script;
		OBJTarget = npcTarget;
		mIsMain = false;
		mOnEndAction = onEndAction;
		mActionWait = null;
		ResetValueFlag();
		StartScope();
	}

	public void StartOnMain( int idxMap, int idxScript, ScriptGui.ScriptYaml script, GameObject npcTarget, OnEndAction onEndAction = null){
		if(mIsRun){
			Debug.Log("Can't start action when running");
			return;
		}
		// if(mIsRun && !mIsMain){
		// 	Debug.Log("Stop normal before run main");
		// 	ForceEnd();
		// }
		Debug.Log("Start Main Action " + Key + " , idxScript: " + idxScript);
		IdxMap = idxMap;
		IdxScript = idxScript;
		Script = script;
		OBJTarget = npcTarget;
		mIsMain = true;
		InputFieldHelper.Instance.HideMenu();
		mOnEndAction = ( ScriptGame s) =>{
			// UI
			InputFieldHelper.Instance.HidePopupAction();
			InputFieldHelper.Instance.HideChatBottom();
			InputFieldHelper.Instance.ShowMenu();
			AutoTileMap_Editor.Instance.ResetMainCamWithAni(()=>{
				onEndAction(s);
			});
		};
		ResetValueFlag();
		StartScope();
	}

	private void StartScope(){
		if(_scriptTarget.Begin != null){
			mScopeIndext = _scriptTarget.ListActions().IndexOf(_scriptTarget.Begin);
			if( mScopeIndext == 0){
				mScopeIndext = 1;
			}
		}else
		{
			mScopeIndext = 1;
		}
		if(mScopeIndext > 0 && mScopeIndext < _scriptTarget.ListActions().Count){
			mIsRun = true;
			DoAction( mScopeIndext, 0);
		}else
		{
			EndAction();	
		}
	}

	private void DoAction( int scopeIndext, int countStack){
		if(countStack > 24){
			InputFieldHelper.Instance.ShowNoti( string.Format( "Error at Script ({0}): : Over stack scope call!", IdxScript));
			EndAction();
			return;
		}
		ScriptGui.ActionData action = _scriptTarget.ActionDataAt(scopeIndext);
		switch( action.Format){
			case ScriptGui.EFormatScope.End:
			{
				if(mIsMain){
					Debug.Log("DoAction: End");
				}
				EndAction();
				return;
			}
			case ScriptGui.EFormatScope.Check:
			{
				if(mIsMain){
					Debug.Log("DoAction: Check");
				}
				var flagTarget = FlagByTarget( action.Check.Target);
				string flag = (action.Check.Flag != null ? action.Check.Flag : "");
				if( flag == "" || flagTarget == null || flagTarget.ContainsKey(flag) == false){
					InputFieldHelper.Instance.ShowNoti( string.Format( "Error at Script ({0}): Not found flag {1}", IdxScript, flag));
					EndAction();
					return;
				}
				if(ScriptGui.Compare_A_B(flagTarget[flag], action.Check.Value, (ScriptGui.ECompare)action.Check.Compare)){
					var t = action.Check.Right;
					NextActionTo(t, countStack + 1);
				}else{
					if(action.Check.SubCheck != null){
						for (int i = 0; i < action.Check.SubCheck.Count; i++)
						{
							var subCheck = action.Check.SubCheck[i];
							if(subCheck == null){
								continue;
							}
							if(ScriptGui.Compare_A_B(flagTarget[flag], subCheck.Value, (ScriptGui.ECompare)subCheck.Compare)){
								NextActionTo( subCheck.Right, countStack + 1);
								return;
							}
						}
					}
					var t = action.Check.Wrong;
					NextActionTo(t, countStack + 1);
				}
				return;
			}
			case ScriptGui.EFormatScope.Set:
			{
				if(mIsMain){
					Debug.Log("DoAction: Set");
				}
				var flagTarget = FlagByTarget( action.Set.Target);
				List<FlagAction> flagActions = ListFlagActionByTarget( action.Set.Target);
				FlagAction.DoFlagAction( flagTarget, flagActions, action.Set.Action);
				var t = action.Set.Next;
				NextActionTo(t, countStack + 1);
				return;
			}
			case ScriptGui.EFormatScope.MsgboxChat:
			{
				if(!mIsMain){
					InputFieldHelper.Instance.ShowNoti( string.Format( "Error at Script ({0}): Can't using Msg Box Chat on Sub Script", IdxScript));
					EndAction();
					return;
				}
				if(TriggerGame.Instance.MSG.ScriptGameMSG == null || TriggerGame.Instance.MSG.ScriptGameMSG == this){
					Debug.Log("MsgboxChat, Name: " + action.Name + ", Index: " + scopeIndext);
					if(OBJTarget != null && OBJTarget.activeSelf){
						AutoTileMap_Editor.Instance.RequestAgentFaceToNpc(OBJTarget, ()=>{
							TriggerGame.Instance.StartMsg( this, action, countStack);
						});
					}else
					{
						TriggerGame.Instance.StartMsg( this, action, countStack);
					}
				}else{
					EndAction();
				}
				return;
			}
			case ScriptGui.EFormatScope.NPC:
			{
				var npc = action.NPCAction;
				DoActionNPC(scopeIndext, countStack, npc);
				return;
			}
			case ScriptGui.EFormatScope.MainChar:
			{
				var mainAction = action.MainCharAction;
				if(!mIsMain){
					InputFieldHelper.Instance.ShowNoti( string.Format( "Error at Script ({0}): Can't control MainChar on Sub Script", IdxScript));
					EndAction();
					return;
				}
				switch(mainAction.Action){
					case ScriptGui.MainCharAction.EAction.WaitMoveToPos:
					{
						InputFieldHelper.Instance.HidePopupAction();
						InputFieldHelper.Instance.HideChatBottom();
						AutoTileMap_Editor.Instance.ResetMainCamWithAni(()=>{
							bool success = TriggerGame.Instance.WaitGoToCheckPoint( this, mainAction.x, mainAction.y, () =>{
								TriggerGame.Instance.SetTip("");
								// Next action
								var t = mainAction.Next;
								NextActionTo(t, 0);
							});
							if(!success){
								EndAction();
							}
						});
						break;
					}
					case ScriptGui.MainCharAction.EAction.WaitMoveToHouse:
					{
						if(mainAction.IdHouse < 0){
							EndAction();
							return;
						}
						InputFieldHelper.Instance.HidePopupAction();
						InputFieldHelper.Instance.HideChatBottom();
						AutoTileMap_Editor.Instance.ResetMainCamWithAni(()=>{
							bool success = TriggerGame.Instance.WaitEnterHouse( this, mainAction, (int houseIdx) => {
								// Next action
								TriggerGame.Instance.SetTip("");
								var t = mainAction.Next;
								NextActionTo(t, 0);
							});
							if(!success){
								EndAction();
							}
						});
						
						break;
					}
					case ScriptGui.MainCharAction.EAction.WaitInteractionsNPC:
					{
						if(mainAction.IdNpc < 0){
							EndAction();
							return;
						}
						InputFieldHelper.Instance.HidePopupAction();
						InputFieldHelper.Instance.HideChatBottom();
						AutoTileMap_Editor.Instance.ResetMainCamWithAni(()=>{
							bool success = TriggerGame.Instance.WaitInteractionNPC( this, mainAction, (int r) => {
								// Next action
								TriggerGame.Instance.SetTip("");
								var t = mainAction.Next;
								NextActionTo(t, 0);
								return true;
							});
							if(!success){
								EndAction();
							}
						});
						
						break;
					}
					case ScriptGui.MainCharAction.EAction.RewardItem:
					{	
						PropertysGame.Instance.AddItem( AutoTileMap_Editor.Instance.MapsData.Propertys, mainAction.SlugItem);
						TriggerGame.Instance.SetTip("");
						var t = mainAction.Next;
						NextActionTo(t, 0);
						return;
					}
					case ScriptGui.MainCharAction.EAction.CheckItem:
					{
						if(PropertysGame.Instance.IsHaveItem( mainAction.SlugItem)){
							NextActionTo( mainAction.Next, 0);
						}else
						{
							NextActionTo( mainAction.Wrong, 0);
						}
						return;
					}
					case ScriptGui.MainCharAction.EAction.BuyItem:
					{	
						SerializablePackages.Package package = AutoTileMap_Editor.Instance.MapsData.Packages.PackageBySlug( mainAction.SlugPackage);
						if(package == null){
							EndAction();
						}else
						{
							// TriggerGame.Instance.StopMainChar();
							TriggerGame.Instance.ResetLogChat();
							var lastAction = action;
							ShopGame.Instance.ShowWithData( AutoTileMap_Editor.Instance.MapsData.Propertys, package, () => {
								// Close dialog
								NextActionTo( mainAction.Wrong, 0);
							},
							(ControlShop pm) =>{
								// If buy something
								if(mainAction.Next == null || mainAction.Next == lastAction){
									// Try buy orther
									InputFieldHelper.Instance.ShowChatBottom( "Continue shopping?", false, (TypingEffectByLine ty) => {
										pm.contentPane.visible = true;
									});
								}else
								{
									NextActionTo( mainAction.Next, 0);	
								}
							});
						}
						return;
					}
					case ScriptGui.MainCharAction.EAction.WarpTo:
					{
						EndAction();
						var triggerRef = mainAction.IdWarp;
						if( triggerRef >= 0 && triggerRef < AutoTileMap_Editor.Instance.MapSelect.WarpsData.Count){
							var w = AutoTileMap_Editor.Instance.MapSelect.WarpsData[triggerRef];
							Debug.Log("GoTo: " + w.map + "_" + w.x + "_" + w.y);
							AutoTileMap_Editor.Instance.WarpsTo(w.map, w.x, w.y);
							// AutoTileMap_Editor.Instance.SetModePlay(true, w.map);
						}
						return;
					}
					default:
					{
						EndAction();
						return;
					}
				}
				if(mainAction.Tip != ""){
					TriggerGame.Instance.SetTip( mainAction.Tip);
				}
				break;
			}
			default:
			{
				EndAction();
				break;
			}
		}
	}

	private void DoActionNPC( int scopeIndext, int countStack, ScriptGuiBase.NPCAction npc){
		if(mActionWait != null){
			Debug.LogError("Have mActionWait");
			return;
		}
		mActionWait = null;
		if(npc.Action == ScriptGui.NPCAction.EAction.Show){
			if(npc.UsingNPCTarget){
				if(!OBJTarget){
					EndAction();
					InputFieldHelper.Instance.ShowNoti( string.Format( "Error at Script ({0}): : Not found NPC on target", IdxScript));
					return;
				}
				OBJTarget.gameObject.SetActive(true);
				AutoTileMap_Editor.Instance.WarpTo(OBJTarget.transform, npc.x, npc.y);
			}else{
				TriggerDetail detail = new TriggerDetail(){
					refMap = IdxMap,
					typeObj = eSlotAonTypeObj.Person,
					refId = npc.IdNpc,
					isCreateFromMap = false,
					x = npc.x,
					y = npc.y
				};
				if(AutoTileMap_Editor.Instance.ShowNPC( detail) == null){
					EndAction();
					return;	
				}
			}
			var t = npc.Next;
			NextActionTo(t, countStack + 1);
			return;
		}
		GameObject npcOBJ = GetNPCTarget(npc);
		if(npcOBJ == null){
			if(mIsMain){
				var t = npc.Next;
				NextActionTo(t, countStack + 1);
			}else
			{
				EndAction();
			}
			return;
		}
		if(!npcOBJ.activeSelf){
			var t = npc.Next;
			NextActionTo(t, countStack + 1);
			return;
		}
		if(npcOBJ == OBJTarget){
			CLEAR_ALL_ACTION_NPC(npcOBJ);
		}else
		{
			if(mIsMain){
				//Pause other NPC
				ScriptGame s_from_npc = TriggerGame.Instance.GetScriptGameCache_NPC_Begin(npcOBJ, false);
				TriggerGame.Instance.AddScriptNeedResume(s_from_npc);
			}
		}
		switch( npc.Action){
			case ScriptGui.NPCAction.EAction.Show:
			{
				// No Check
				break;
			}
			case ScriptGui.NPCAction.EAction.Hide:
			{
				if(npc.UsingNPCTarget == false){
					AutoTileMap_Editor.Instance.RemoveNPC( npc.IdNpc);
				}else{
					if(OBJTarget != null){
						OBJTarget.SetActive(false);
					}
				}
				break;
			}
			case ScriptGui.NPCAction.EAction.Move:
			{
				// AutoTileMap_Editor.Instance.AddNPC( npc.IdNpc, npc.x, npc.y, true);
				// GameObject npcOBJ = GetNPCTarget(npc);
				if(npcOBJ != null && npcOBJ.activeSelf){
					mActionWait = npc;
					var high = AutoTileMap_Editor.Instance.MapSelect.GetHighRef(npc.x, npc.y) * 0.5f;
					var to = new Vector3((0.5f + npc.x) * AutoTileMap_Editor.Instance.CellSize.x , high + 1, -(0.5f + npc.y) * AutoTileMap_Editor.Instance.CellSize.y);
					var c = npcOBJ.GetComponent<NavMeshAgentCallback>();
					if(c == null){
						c = npcOBJ.AddComponent<NavMeshAgentCallback>();
					}
					c.WalkTo( 1, to, ( NavMeshAgent nav ) => {
						mActionWait = null;
						var t = npc.Next;
						this.NextActionTo(t, 0);
					});
				}
				break;
			}
			case ScriptGui.NPCAction.EAction.LookAtCharacter:
			{
				// GameObject npcOBJ = GetNPCTarget(npc);
				if(npcOBJ != null && npcOBJ.activeSelf){
					bool requestLockBack = mIsMain;
					if(NpcLookatMainCallback.InstanceOf(npcOBJ).LookTo( AutoTileMap_Editor.Instance.Agent, requestLockBack, (NpcLookatMainCallback n) => {
						mActionWait = null;
						var t = npc.Next;
						this.NextActionTo(t, 0);
					})){
						mActionWait = npc;
					}else
					{
						var t = npc.Next;
						this.NextActionTo(t, 0);
					}
				}
				break;
			}
			case ScriptGui.NPCAction.EAction.Animation_Talk:
			{
				// GameObject npcOBJ = GetNPCTarget(npc);
				if(npcOBJ != null && npcOBJ.activeSelf){
					var b = npcOBJ.GetComponent<BasicMecanimControl>();
					if(b != null){
						mActionWait = npc;
						b.TalkWithAni((BasicMecanimControl bb)=>{
							mActionWait = null;
							var t = npc.Next;
							this.NextActionTo(t, 0);
						});
					}
				}
				break;
			}
			case ScriptGui.NPCAction.EAction.Animation_Face_Down:
			{
				doNpcFaceTo( npc, new Vector3( 0, 0 , -1));
				return;
			}
			case ScriptGui.NPCAction.EAction.Animation_Face_Up:
			{
				doNpcFaceTo( npc, new Vector3( 0, 0 , 1));
				return;
			}
			case ScriptGui.NPCAction.EAction.Animation_Face_Left:
			{
				doNpcFaceTo( npc, new Vector3( -1, 0 , 0));
				return;
			}
			case ScriptGui.NPCAction.EAction.Animation_Face_Right:
			{
				doNpcFaceTo( npc, new Vector3( 1, 0 , 0));
				return;
			}
			default:
			{
				EndAction();
				return;
			}
		}
		if(mActionWait == null){
			var t = npc.Next;
			NextActionTo(t, countStack + 1);
		}
	}

	/*
	private void ForceEnd(){
		if(mActionWait != null){
			var autoTileMap = AutoTileMap_Editor.Instance;
			var npc = mActionWait;
			GameObject npcOBJ = npc.UsingNPCTarget ? mNpcTarget : autoTileMap.GetNPC(npc.IdNpc);
			if( npcOBJ != null && npcOBJ.activeSelf){
				// npcOBJ.GetComponent<NavMeshAgent>().ResetPath();
				var c = npcOBJ.GetComponent<NavMeshAgentCallback>();
				if(c != null){
					c.ClearDestination();
				}
			}
		}
		EndAction();
	}
	*/

	public void PauseAction(){
		if(!mIsRun){
			return;
		}
		mIsRun = false;
		Debug.Log("ScriptGame Pause " + Key);
		if(mActionWait != null){
			GameObject npcOBJ = GetNPCTarget(mActionWait);
			CLEAR_ALL_ACTION_NPC(npcOBJ);
		}
	}

	public void ResumeAction(){
		if(mIsRun){
			return;
		}
		mIsRun = true;
		Debug.Log("ScriptGame Resume " + Key);
		if(mActionWait != null){
			ScriptGuiBase.NPCAction action = mActionWait;
			mActionWait = null;
			DoActionNPC(mScopeIndext, 0, action);
		}
	}

	public void EndAction() {
		Debug.Log("ScriptGame End " + Key);
		mActionWait = null;
		mIsRun = false;
		if(mOnEndAction != null){
			var callBack = mOnEndAction;
			mOnEndAction = null;
			callBack(this);
		}
	}

	public void Update(){
		/*
		if(mActionWait != null){
			var npc = mActionWait;
			var autoTileMap = AutoTileMap_Editor.Instance;
			GameObject npcOBJ = npc.UsingNPCTarget ? mNpcTarget : autoTileMap.GetNPC(npc.IdNpc);
			if( npcOBJ != null && npcOBJ.activeSelf){
				if(npc.Action == ScriptGui.NPCAction.EAction.LookAtCharacter){
					// npcOBJ.transform.LookAt(autoTileMap.Agent.transform, Vector3.up);
					Vector3 direction = (autoTileMap.Agent.transform.position - npcOBJ.transform.position).normalized;
					Quaternion lookRotation = Quaternion.LookRotation(direction);
					//Char
					float angleChar = 0;
					if(mIsMain){
						Quaternion i_lookRotation = Quaternion.LookRotation((npcOBJ.transform.position - autoTileMap.Agent.transform.position).normalized);
						autoTileMap.Agent.transform.rotation = Quaternion.Slerp(autoTileMap.Agent.transform.rotation, i_lookRotation, Time.deltaTime * 5);
						angleChar = Quaternion.Angle( autoTileMap.Agent.transform.rotation, i_lookRotation );
					}
					//NPC
					npcOBJ.transform.rotation = Quaternion.Slerp(npcOBJ.transform.rotation, lookRotation, Time.deltaTime * 5);
					var angleNPC = Quaternion.Angle( npcOBJ.transform.rotation, lookRotation );
					if(angleChar <= 1 && angleNPC <= 1){
						mActionWait = null;
						if(mIsMain){
							TriggerGame.Instance.ResumeMainChar();
						}
					}
				}
			}else{
				mActionWait = null;
			}
			if(mActionWait == null){
				var t = npc.Next;
				NextActionTo(t, 0);
			}
		}
		*/
	}

	// public void OnItemEnter( GameObject itemTarget, string itemName){
	// 	if(itemName == "WaitMoveToPos"){
	// 		ScriptGui.ActionData action = scriptTarget.ActionDataAt(scopeIndext);
	// 		if(action == null
	// 		|| action.Format != ScriptGui.EFormatScope.MainChar
	// 		|| action.MainCharAction.Action != ScriptGui.MainCharAction.EAction.WaitMoveToPos){
	// 			return;
	// 		}
	// 		GameObject.Destroy(itemTarget);
	// 		// Next action
	// 		var t = action.MainCharAction.Next;
	// 		NextActionTo(t, 0);
	// 	}
	// }

	public bool NextActionTo( ScriptGui.ActionData t, int countStack){
		mScopeIndext = (t == null ? -1: _scriptTarget.ListActions().IndexOf(t));
		if(mScopeIndext > 0 && mScopeIndext < _scriptTarget.ListActions().Count){
			DoAction( mScopeIndext, countStack);
			return true;
		}else{
			EndAction();
		}
		return false;
	}

	public delegate void OnCallBack( ScriptGame s);
	// private IEnumerator Wait(float time, OnCallBack calback){
	// 	yield return new WaitForSeconds(time);
	// 	calback( this);
	// 	yield break;
	// }

	private GameObject GetNPCTarget( ScriptGui.NPCAction npc) {
		GameObject npcOBJ = npc.UsingNPCTarget ? OBJTarget : (mIsMain ? AutoTileMap_Editor.Instance.GetNPC(npc.IdNpc) : null);
		// GameObject npcOBJ = null;
		// if(npc.UsingNPCTarget){
		// 	var tTarget =  AutoTileMap_Editor.Instance.GetNPC(Detail);
		// 	if(tTarget){
		// 		npcOBJ = tTarget.gameObject;
		// 	}
		// }
		// if(!npcOBJ){
		// 	npcOBJ = AutoTileMap_Editor.Instance.GetNPC(npc.IdNpc);
		// }
		if(npcOBJ == null){
			if(npc.UsingNPCTarget){
				InputFieldHelper.Instance.ShowNoti( string.Format( "Error at Script ({0}): : Not found NPC on target", IdxScript));
			}else
			{
				InputFieldHelper.Instance.ShowNoti( string.Format( "Error at Script ({0}): : Not found NPC ({1})", IdxScript, npc.IdNpc));
			}
		}
		return npcOBJ;
	}

	private void doNpcFaceTo( ScriptGui.NPCAction npc, Vector3 to){
		GameObject npcOBJ = GetNPCTarget(npc);
		if(npcOBJ != null && npcOBJ.activeSelf){
			mActionWait = npc;
			NpcLookatDCallback.InstanceOf(npcOBJ).LookTo( to, (NpcLookatDCallback n) => {
				mActionWait = null;
				this.NextActionTo(npc.Next, 0);
			});
		}
	}

	public static void CLEAR_ALL_ACTION_NPC(GameObject npcOBJ){
		if( npcOBJ != null && npcOBJ.activeSelf){
			var cMove = npcOBJ.GetComponent<NavMeshAgentCallback>();
			if(cMove != null){
				cMove.ClearDestination();
			}
			var cLockAtD = npcOBJ.GetComponent<NpcLookatDCallback>();
			if(cLockAtD != null){
				cLockAtD.StopLock();
			}
			var cLockAtMain = npcOBJ.GetComponent<NpcLookatMainCallback>();
			if(cLockAtMain != null){
				cLockAtMain.StopLock();
			}
			var cTalk = npcOBJ.GetComponent<BasicMecanimControl>();
			if(cTalk != null){
				cTalk.ClearTalk();
			}
		}
	}
}
