using System.Collections;
using System.Collections.Generic;
using AON.RpgMapEditor;
using FairyGUI;
using UnityEngine;

public class ConversationGame {

	public class MLog
	{
		public string text;
		public string talker;
	}

	public class MChoise
	{
		public string text;
		public string talker;
		public ScriptGui.MsgboxChoise choise;
	}
	// Using for during Show MSG
	private ScriptGame mScriptGameMSG = null;
	public ScriptGame ScriptGameMSG{
		get{
			return mScriptGameMSG;
		}
	}
	private GUISkin GUISkinIngame = Resources.Load("GUI/InGame") as GUISkin;
    private ScriptGui.ActionData mMsgboxChat = null;
	private int mIndextChat = 0;
	private int mLimitLog = 32;
	private List<MLog> mMsgLog = new List<MLog>();
	private List<MLog> mMsgChat = null;
	private List<MChoise> mMsgChoise = null;
	private int mNextIdxChoiseInHighlight = -1;

	private string GetHashChoise( ScriptGui.MsgboxChoise choise){
		var dataCheck = mMsgboxChat.MsgboxChat[mIndextChat];
		return string.Format("{0}_{1}_{2}", mMsgboxChat.id, mIndextChat, dataCheck.MsgboxChoise.IndexOf(choise));
	}
	private string GetHashScope( ScriptGui.ActionData scope){
		// return string.Format("{0}_{1}_{2}", scope.id, mIndextChat, dataCheck.MsgboxChoise.IndexOf(choise));
		return scope.id.ToString();
	}
	private List<string> mChoiseIgnore = new List<string>();

	public void ResetLogChat(){
		// Reset choiseIgnore for MsgChat
		Debug.Log("Reset choiseIgnore for MsgChat");
		mChoiseIgnore = new List<string>();
		mMsgLog = new List<MLog>();
		mMsgChat = null;
	}

	public void StartMsg(ScriptGame scriptGame, ScriptGui.ActionData msg, int countStack){
		Debug.Log("StartMsg");
		mScriptGameMSG = scriptGame;
		mMsgboxChat = msg;
		mIndextChat = 0;
		// StopMainChar();
		SetupChoise(countStack);
	}
	
	public void EndMsg(){
		Debug.Log("EndMsg");
		var scriptGameMSG = mScriptGameMSG;
		mScriptGameMSG = null;
		mMsgboxChat = null;
		// ResumeMainChar();
		scriptGameMSG.EndAction();
	}

	// public void StopMainChar(){
	// 	TriggerGame.Instance.StopMainChar();
	// }

	// public void ResumeMainChar(){
	// 	TriggerGame.Instance.ResumeMainChar();
	// }
	
	private void SetupChoise(int countStack){
		mMsgChoise = null;
		mNextIdxChoiseInHighlight = -1;
		if(mMsgboxChat == null || mMsgboxChat.MsgboxChat.Count <= 0 || mIndextChat < 0 || mIndextChat >= mMsgboxChat.MsgboxChat.Count){
			EndMsg();
			return;
		}

		if(mMsgboxChat.RandomOnce){
			var hashScope = GetHashScope(mMsgboxChat);
			if(mChoiseIgnore.IndexOf(hashScope) >= 0){
				Debug.LogError("This scope has ignore, can't add twine");
				return;
			}else
			{
				Debug.Log("Ignore: " + hashScope);
				mChoiseIgnore.Add(hashScope);
			}
		}


		if( mMsgChat != null && mMsgChat.Count > 0){
			for (int i = 0; i < mMsgChat.Count; i++)
			{
				mMsgLog.Add(mMsgChat[i]);
			}
			RefreshLog();
		}
			
		// if(indextChat != msgboxChat.MsgboxChat.Count -1){
		// 	return;
		// }
		mMsgChat = new List<MLog>();
		mMsgChoise = new List<MChoise>();
		while(mIndextChat < mMsgboxChat.MsgboxChat.Count){
			var dataCheck = mMsgboxChat.MsgboxChat[mIndextChat];
			if(dataCheck == null){
				mIndextChat++;
				continue;
			}
			List<ScriptGui.MsgboxChoise> choiseHasFiller = null;
			if(dataCheck.Group >= 1 && dataCheck.MsgboxChoise != null && dataCheck.MsgboxChoise.Count > 0){
				//Random in group
				choiseHasFiller = new List<ScriptGui.MsgboxChoise>();
				if(dataCheck.Group > 1){
					int begin_insert = -1;
					{
						var choiseForRan = new List<ScriptGui.MsgboxChoise>();
						for( int b = 0; b < dataCheck.MsgboxChoise.Count; b += dataCheck.Group){
							var choise = dataCheck.MsgboxChoise[b];
							string hashChoise = GetHashChoise(choise);
							if(mChoiseIgnore.IndexOf( hashChoise) >= 0){
								continue;
							}
							if(choise.Rate <= 0){
								choise.Rate = 1;
								// continue;
							}
							if(choise.Next != null){
								var hashScope = GetHashScope(choise.Next);
								if(mChoiseIgnore.IndexOf(hashScope) >= 0){
									continue;
								}
							}
							choiseForRan.Add(choise);
						}
						if(choiseForRan.Count > 0){
							var totalRate = 0;
							for( int b = 0; b < choiseForRan.Count; b++){
								var choise = choiseForRan[b];
								int rate = choise.Rate;
								totalRate += rate;
							}
							if(totalRate > 0){
								int ran = Random.Range( 0, totalRate);
								totalRate = 0;
								for( int b = 0; b < choiseForRan.Count; b++){
									var choise = choiseForRan[b];
									int rate = choise.Rate;
									totalRate += rate;
									if(ran < totalRate){
										begin_insert = dataCheck.MsgboxChoise.IndexOf(choise);
										break;
									}
								}
							}
						}
					}
					if(begin_insert >= 0){
						begin_insert = begin_insert - (begin_insert % dataCheck.Group);
						for( int o = 0; o < dataCheck.Group; o++){
							int at = o + begin_insert;
							if(at >= 0 && at < dataCheck.MsgboxChoise.Count){
								choiseHasFiller.Add(dataCheck.MsgboxChoise[at]);
							}
						}
						if(dataCheck.RandomOnce) {
							var choiseWillIgnore = dataCheck.MsgboxChoise[begin_insert];
							var hashChoise = GetHashChoise(choiseWillIgnore);
							if(mChoiseIgnore.IndexOf(hashChoise) >= 0){
								Debug.LogError("This choise has ignore, can't add twine");
							}else
							{
								Debug.Log("Ignore: " + hashChoise);
								mChoiseIgnore.Add(hashChoise);
							}
						}
					}
				}else{ // if dataCheck.Group == 1
					if(dataCheck.Random){
                        // Check rate
                        ScriptGui.MsgboxChoise ranChoise = null;
						var choiseForRan = new List<ScriptGui.MsgboxChoise>();
						for( int b = 0; b < dataCheck.MsgboxChoise.Count; b++){
							var choise = dataCheck.MsgboxChoise[b];
							var hashChoise = GetHashChoise( choise);
							if(mChoiseIgnore.IndexOf(hashChoise) >= 0){
								continue;
							}
							if(choise.Rate <= 0){
								choise.Rate = 1;
								// continue;
							}
							if(choise.Next != null){
								var hashScope = GetHashScope(choise.Next);
								if(mChoiseIgnore.IndexOf(hashScope) >= 0){
									continue;
								}
							}
							choiseForRan.Add(choise);
						}
						var totalRate = 0;
						for( int b = 0; b < choiseForRan.Count; b++){
							var choise = choiseForRan[b];
							int rate = choise.Rate;
							totalRate += rate;
						}
						if(totalRate > 0){
							int ran = Random.Range( 0, totalRate);
							totalRate = 0;
							for( int b = 0; b < choiseForRan.Count; b++){
								var choise = choiseForRan[b];
								int rate = choise.Rate;
								totalRate += rate;
								if(ran < totalRate){
									ranChoise = choise;
									break;
								}
							}
						}
						if(ranChoise != null){
							choiseHasFiller.Add(ranChoise);
							if(dataCheck.RandomOnce) {
								ScriptGui.MsgboxChoise choiseWillIgnore = ranChoise;
								string hashChoise = GetHashChoise( choiseWillIgnore);
								if(mChoiseIgnore.IndexOf( hashChoise) >= 0){
									Debug.LogError("This choise has ignore, can't add twine");
								}else
								{
									Debug.Log("Ignore: " + hashChoise);
									mChoiseIgnore.Add( hashChoise);
								}
							}
						}
					}else {
						choiseHasFiller.AddRange(dataCheck.MsgboxChoise);
					}
				}
			}
			
			// if( dataCheck != null && dataCheck.Value != null && dataCheck.Value != ""){
			// 	contentTiles.Add(new GUIContent(dataCheck.Value));
			//
			// Ignore Scope
			if(choiseHasFiller != null && choiseHasFiller.Count > 0){
				for( int i = choiseHasFiller.Count - 1; i >= 0; i--){
					var c = choiseHasFiller[i];
					if(c.Next == null){
						continue;
					}
					var hashScope = GetHashScope(c.Next);
					if(mChoiseIgnore.IndexOf(hashScope) >= 0){
						choiseHasFiller.RemoveAt(i);
					}
				}
			}
			if(choiseHasFiller != null && choiseHasFiller.Count > 0){
				if(dataCheck.Group > 1){
					foreach(var c in choiseHasFiller){
						mMsgChoise.Add( new MChoise(){
							choise = c
						});
					}
				}else { // Group == 1
					bool isHaveNextScope = false;
					foreach( var c in choiseHasFiller){
						if(c != null && c.Next != null && mScriptGameMSG.Script.ListActions().IndexOf(c.Next) != 0){
							isHaveNextScope = true;
							break;
						}
					}
					if(isHaveNextScope){
						foreach( var c in choiseHasFiller){
							if(c != null){
								mMsgChoise.Add(new MChoise(){
									choise = c
								});
							}
						}
					}else{
						foreach( var c in choiseHasFiller){
							if(c != null && c.Value != null && c.Value != ""){
								mMsgChat.Add(new MLog(){
									text = c.Value
								});
							}
						}
					}
				}
			}
			if(mMsgChoise.Count > 0){
				// Check shuffle
				if(mMsgChoise.Count > 1){
					if(dataCheck.Shuffle){
						List<MChoise> listShuffle = new List<MChoise>();
						while(mMsgChoise.Count > 0){
							int r = Random.Range( 0, mMsgChoise.Count);
							listShuffle.Add(mMsgChoise[r]);
							mMsgChoise.RemoveAt(r);
						}
						mMsgChoise = listShuffle;
					}
				}
				break;
			}
			if(mIndextChat >= mMsgboxChat.MsgboxChat.Count - 1){
				break;
			}
			bool haveContent = mMsgChat.Count > 0 || mMsgChoise.Count > 0;
			if(haveContent && dataCheck.BreakLine){
				break;
			}
			mIndextChat++;
		}
		if(mMsgChat.Count == 0 && mMsgChoise.Count == 0){
			//Don't have content
			EndMsg();
			return;
		}
		if(mMsgChat.Count == 0 && mMsgChoise.Count == 1){
			//Auto next if just have one Choise
			var c = mMsgChoise[0];
			if(c.choise.Value == null || c.choise.Value == ""){
				GoNextChoise( c.choise, countStack + 1);
				return;
			}
		}
		// Get talker
		for (int i = 0; i < mMsgChat.Count; i++)
		{
			var log = mMsgChat[i];
			var content = log.text;
			var startI = content.IndexOf(":");
			if(startI >= 0 && startI + 1 < content.Length){
				log.talker = content.Substring(0, startI);
				content = content.Substring(startI + 1, content.Length - startI - 1);
				log.text = content.Trim();
			}
		}
		for (int i = 0; i < mMsgChoise.Count; i++)
		{
			var c = mMsgChoise[i];
			// Get talker
			var content = c.choise.Value;
			var startI = content.IndexOf(":");
			if(startI >= 0 && startI + 1 < content.Length){
				c.talker = content.Substring(0, startI);
				content = content.Substring(startI + 1, content.Length - startI - 1);
				c.text = content.Trim();
			}else
			{
				c.text = content;
			}
		}
		ShowDialog();
	}
	
	public void OnGUIChat() {
		if(mMsgboxChat != null) {
			if(mIndextChat < 0 || mIndextChat >= mMsgboxChat.MsgboxChat.Count){
				EndMsg();
				return;
			}
			float w = 400;
			var listStyleLabel = AONGUI.skin.label;
			var listStyleButton = AONGUI.skin.button;
			float calcHeight = 0;
			float widthContent = w - 16;

			List<float> h_Titles = new List<float>();
			List<GUIContent> contentChoises = new List<GUIContent>();
			List<float> h_Choises = new List<float>();

			// calcHeight
			calcHeight += 8;
			for (int i = 0; i < mMsgLog.Count; i++)
			{
				MLog g = mMsgLog[i];
				float h_title = listStyleLabel.CalcHeight( new GUIContent(g.text), widthContent) + 8;
				calcHeight += h_title;
				h_Titles.Add( h_title);
				calcHeight += 4;
			}
			if(mMsgChat.Count > 0){
				foreach( MLog g in mMsgChat){
					float h_title = listStyleLabel.CalcHeight( new GUIContent(g.text), widthContent) + 8;
					calcHeight += h_title;
					h_Titles.Add( h_title);
					calcHeight += 4;
				}
			}
			bool isHaveButonNext = false;
			float h_next = 0;
			GUIContent g_next = null;
			if(mMsgChoise != null && mMsgChoise.Count > 0){
				foreach(var c in mMsgChoise){
					// GUIContent g_c = new GUIContent(c.Value != "" ? c.Value : ( c.Next != null ? "Next" : "Close"));
					GUIContent g_c = new GUIContent(c.choise.Value != "" ? c.choise.Value : ( c.choise.Next != null ? "Next" : ""));
					float h_c = 0;
					int to_scope = (c.choise.Next == null ? -1: mScriptGameMSG.Script.ListActions().IndexOf(c.choise.Next));
					if(to_scope == -1){
						h_c = listStyleLabel.CalcHeight( g_c, widthContent);
					}else{
						h_c = listStyleButton.CalcHeight( g_c, widthContent);
						isHaveButonNext = true;
					}
					contentChoises.Add(g_c);
					h_Choises.Add(h_c);
					calcHeight += 8;
					calcHeight += h_c;
					calcHeight += 8;
				}
			}
			if( isHaveButonNext == false){
				g_next = new GUIContent((mIndextChat >= mMsgboxChat.MsgboxChat.Count - 1) ? "Close" : "Next");
				h_next = listStyleButton.CalcHeight( g_next, widthContent);
				calcHeight += 8;
				calcHeight += h_next;
				calcHeight += 8;
			}
			// Render
			// Rect rect = new Rect((Screen.width - w) / 2, Screen.height - 24f - calcHeight, w, calcHeight);
			Rect rect = new Rect((Screen.width - w) / 2, 24f, w, calcHeight);
			bool touchOnDialog = false;
			if(mNextIdxChoiseInHighlight == -1){
				AONGUI.Box( rect, "");	
			}else{
				AONGUI.Button( rect, "", GUISkinIngame.box, () => {
					touchOnDialog = true;
				});
			}
			float yGui = rect.y;
			yGui += 8;
			int count_title = 0;
			for (int i = 0; i < mMsgLog.Count; i++)
			{
				MLog g = mMsgLog[i];
				float h_title = h_Titles[count_title];
				count_title++;
				AONGUI.Label(new Rect(rect.x + 8, yGui, widthContent, h_title), new GUIContent(g.text));
				yGui += h_title;
				yGui += 4;
			}
			if(mMsgChat.Count > 0){
				for( int j = 0; j < mMsgChat.Count; j++){
					float h_title = h_Titles[count_title];
					count_title++;
					AONGUI.Label(new Rect(rect.x + 8, yGui, widthContent, h_title), mMsgChat[j].text);
					yGui += h_title;
					yGui += 4;
				}
			}
			if(mMsgChoise != null && mMsgChoise.Count > 0){
				var dataCheck = mMsgboxChat.MsgboxChat[mIndextChat];
				bool isCheckHighlight = mMsgChoise.Count > 0 && dataCheck.Highlight;
				for(int idxChoise = 0; idxChoise < mMsgChoise.Count; idxChoise++){
					var c = mMsgChoise[idxChoise];
					int to_scope = (c.choise.Next == null ? -1: mScriptGameMSG.Script.ListActions().IndexOf(c.choise.Next));
					GUIContent g_c = contentChoises[idxChoise];
					float h_c = h_Choises[idxChoise];
					if(to_scope == -1){
						AONGUI.Label(new Rect(rect.x + 8, yGui, widthContent, h_c + 8), g_c);
					}else{
						var r = new Rect(rect.x + 8, yGui, widthContent, h_c + 8);
						if(c.choise.Value == ""){
							r = new Rect(rect.x + rect.width - 58, yGui, 50, h_c + 8);
						}
						if(isCheckHighlight && mNextIdxChoiseInHighlight != -1){
							if(!touchOnDialog){
								OnGuiShowHighlight( idxChoise, r, g_c, c.choise);
							} else{
								if(idxChoise == mNextIdxChoiseInHighlight){
									//Save Log with Right value
									ScriptGui.MsgboxChoise choiseRight = null;
									for(int idxR = 0; idxR < mMsgChoise.Count; idxR++){
										var cCheck = mMsgChoise[idxR];
										if((ScriptGui.MsgboxChoise.ETypeHighlight)cCheck.choise.TypeHighlight == ScriptGui.MsgboxChoise.ETypeHighlight.Full_Green){
											choiseRight = cCheck.choise;
											break;
										}
									}
									if(choiseRight != null && !string.IsNullOrEmpty(choiseRight.Value)){
										mMsgLog.Add( new MLog(){
											text = choiseRight.Value
										});
									}else if(!string.IsNullOrEmpty(c.choise.Value)){
										mMsgLog.Add( new MLog(){
											text = c.choise.Value
										});
									}
									RefreshLog();
									//End Save Log
									mNextIdxChoiseInHighlight = -1;
									GoNextChoise( c.choise);
									return;
								}
							}
						}else {
							AONGUI.Button(r, g_c, () => {
								// Not show Highlight
								if(isCheckHighlight){
									// Set show Highlight
									mNextIdxChoiseInHighlight = idxChoise;
								}else{
									if(g_c.text != null && g_c.text != "" && g_c.text != "Close" && g_c.text != "Next"){
										mMsgLog.Add(new MLog(){
											text = g_c.text
										});
										RefreshLog();
									}
									GoNextChoise( c.choise);
								}
							});
						}
					}
					// if(choise.Next != null && scriptTarget.Main.IndexOf(choise.Next)))
					yGui += 8;
					yGui += h_c;
					yGui += 8;
				}
			}
			if(isHaveButonNext == false){
				AONGUI.Button(new Rect(rect.x + rect.width - 68, yGui, 60, h_next + 8), g_next, () => {
					mIndextChat++;
					if(mIndextChat >= mMsgboxChat.MsgboxChat.Count){
						EndMsg();
					}else{
						SetupChoise(0);
					}
				});
			}
			return;
		}
	}

	private void OnGuiShowHighlight( int idxChoise, Rect r, GUIContent g_c, ScriptGui.MsgboxChoise choise){
		ScriptGui.MsgboxChoise.ETypeHighlight type = (ScriptGui.MsgboxChoise.ETypeHighlight)choise.TypeHighlight;
		bool isPick = idxChoise == mNextIdxChoiseInHighlight;
		bool isRight = type == ScriptGui.MsgboxChoise.ETypeHighlight.Full_Green;
		if(isPick){
			if(isRight){
				AONGUI.Label(r, g_c, GUISkinIngame.customStyles[3]);
				AONGUI.DrawTexture(new Rect(r.x + r.width - 32, r.y + r.height - 32, 32, 32), GUISkinIngame.customStyles[3].hover.background);
			}else
			{
				if(type == ScriptGui.MsgboxChoise.ETypeHighlight.Words_Red && !string.IsNullOrEmpty(choise.ContentHighlight)){
					var style = GUISkinIngame.customStyles[0];
					var red = GUISkinIngame.customStyles[2];
					var b = choise.Value.IndexOf(choise.ContentHighlight);
					if(b >= 0){
						// var s1 = choise.Value.Substring(0, b);
						// var c1 = new GUIContent(s1);
						var p_sup = style.GetCursorPixelPosition( r, g_c, b);
						var s_sup = style.CalcSize(new GUIContent(choise.ContentHighlight));
						AONGUI.Label(new Rect(p_sup.x - 5, p_sup.y - 2, s_sup.x, s_sup.y),"", red);
						// GUI.Label(r, g_c, style);
					}
				}
				AONGUI.Label(r, g_c, GUISkinIngame.customStyles[4]);
				AONGUI.DrawTexture(new Rect(r.x + r.width - 32, r.y + r.height - 32, 32, 32), GUISkinIngame.customStyles[4].hover.background);
			}
		}else
		{
			if(isRight){
				AONGUI.Label(r, g_c, GUISkinIngame.customStyles[1]);
			}else{
				AONGUI.Label(r, g_c, GUISkinIngame.customStyles[5]);
			}
		}
	}

	private void RefreshLog(){
		if(mMsgLog.Count > mLimitLog){
			mMsgLog.RemoveRange(0, mMsgLog.Count - mLimitLog);
		}
	}

	private void GoNextChoise(ScriptGui.MsgboxChoise choise, int countStack = 0){
		// UI
		InputFieldHelper.Instance.HidePopupAction();
		// InputFieldHelper.Instance.HideChatBottom();
		// Logic
		var scriptGameMSG = mScriptGameMSG;
		// ResumeMainChar();
		mScriptGameMSG = null;
		mMsgboxChat = null;
		//Check Action Flag
		if(choise.Action != null && choise.Action.Key != null
		&& scriptGameMSG.Script.FlagActions.IndexOf(choise.Action) > 0 
		&& scriptGameMSG.Flags != null 
		&& scriptGameMSG.Flags.ContainsKey(choise.Action.Key)){
			FlagAction.DoFlagAction( scriptGameMSG.Flags, scriptGameMSG.Script.FlagActions, choise.Action);
		}
		scriptGameMSG.NextActionTo( choise.Next, countStack);
	}

	private void ShowDialog(){
		if(mMsgboxChat == null) {
			return;
		}
		if(mIndextChat < 0 || mIndextChat >= mMsgboxChat.MsgboxChat.Count){
			EndMsg();
			return;
		}
		
		if(mMsgChat.Count > 0){
			PrintDialog(0, () => {
				EndDialog();
			});
			return;
		}
		EndDialog();
	}

	private void PrintDialog( int indext, EventCallback0 onComplete){
		if(indext >= mMsgChat.Count){
			onComplete();
			return;	
		}
		GameObject objListener = null;
		GameObject objTalker = null;
		GetTalkerObj(mMsgChat[indext].talker, ref objListener, ref objTalker);
		InputFieldHelper.Instance.ShowChatBottomWithArrow( objListener, objTalker, mMsgChat[indext].text, (TypingEffectByLine ty) => {
			PrintDialog(indext + 1, onComplete);
		});
	}

	private void GetTalkerObj(string talker, ref GameObject objListener,ref GameObject objTalker){
		if(string.IsNullOrEmpty(talker)){
			return;
		}
		talker = talker.Trim().ToLower();
		// Debug.Log(talker);
		if( talker == "learner"){
			objListener = mScriptGameMSG.Talker;
			objTalker = AutoTileMap_Editor.Instance.Agent;
		}else if(talker == "npc")
		{
			objListener = AutoTileMap_Editor.Instance.Agent;
			objTalker = mScriptGameMSG.Talker;
		}
		if(objTalker != null){
			var b = objTalker.GetComponent<BasicMecanimControl>();
			if(b != null){
				b.TriggerTalk();
			}
		}
	}

	private void EndDialog(){
		if(mMsgChoise.Count == 1){
			var c = mMsgChoise[0];
			if(string.IsNullOrEmpty(c.choise.Value)){
				GoNextChoise(c.choise);
				return;
			}
		}
		if(mMsgChoise.Count > 0){
			PrintChoise();
		}else
		{
			EndMsg();
		}
	}

	private void PrintChoise(){
		if(mMsgChoise.Count > 0){
			string urlPopup_bt = "ui://BlueSkin/Button_choise_w";
        	UIObjectFactory.SetPackageItemExtension( urlPopup_bt, typeof(GButton_TypingEffect));
			var popup = InputFieldHelper.Instance.ShowPopupAction_Conversation();
			popup.contentPane.width = 450;
			bool isOffsetOnRight = false;
			for (int i = 0; i < mMsgChoise.Count; i++)
			{
				var c = mMsgChoise[i];
				if(!isOffsetOnRight && !string.IsNullOrEmpty(c.choise.ContentHighlight)){
					isOffsetOnRight = true;
				}
				// var t = c.Value != "" ? c.Value : ( c.Next != null ? "Next" : "");
				var t = c.text;
				int idxChoise = i;
				var bt = popup.AddItemWithUrl(urlPopup_bt, t, (EventContext context) =>{
					this._onChoise( popup, idxChoise, c.choise);
				});
				bt.GetController("c1").SetSelectedIndex(0);
				PreItemShowHighlight(c, bt);
			}
			popup.ResizeWidthMinFromItems(isOffsetOnRight);
			popup.ResizeHeightToFix();
			{ 
				//Animation Show
				float delay = 0f;
				for (int i = 0; i < popup.itemCount; i++)
				{
					GObject obj = popup.ItemAt(i);
					if(obj != null && obj is GButton_TypingEffect){
						GButton_TypingEffect gBt = (GButton_TypingEffect)obj;
						// gBt.PlayTypingEffect();
						gBt.PlayEffect(delay);
						delay += 0.2f;
					}
				}
			}
			//Disable scroll
			if(popup.list.scrollPane != null){
				popup.list.scrollPane.touchEffect = false;
			}
			{
				//AddArrowTalker
				GameObject objListener = null;
				GameObject objTalker = null;
				GetTalkerObj(mMsgChoise[0].talker, ref objListener, ref objTalker);
				var _c = popup.contentPane;
				var _mainView = _c.parent;
				
				float posFrameLeft = 10;
				float posFrameRight = _mainView.width - _c.width - 10;
				float posFrameTop = 86;
				float posFrameBot = _mainView.height - _c.height - 86;
		
				InputFieldHelper.AddArrowTalker(objListener, objTalker, popup.contentPane, posFrameTop, posFrameBot, posFrameLeft, posFrameRight);
			}
		}
	}

	private void _onChoise(ControlList popup, int nextIdxChoise, ScriptGui.MsgboxChoise cNext){
		var dataCheck = mMsgboxChat.MsgboxChat[mIndextChat];
		bool isCheckHighlight = mMsgChoise.Count > 0 && dataCheck.Highlight;
		if(!isCheckHighlight){
			if(cNext.Value != null && cNext.Value != "" && cNext.Value != "Close" && cNext.Value != "Next"){
				mMsgLog.Add(new MLog(){
					text = cNext.Value
				});
				RefreshLog();
			}
			GoNextChoise(cNext);
		}else
		{
			HightlightChoise(popup, nextIdxChoise, cNext);
		}
	}

	private void HightlightChoise(ControlList popup, int nextIdxChoise, ScriptGui.MsgboxChoise cNext){
		for (int i = 0; i < mMsgChoise.Count; i++)
		{
			var c = mMsgChoise[i];
			RefreshItemShowHighlight(i, nextIdxChoise, c, (GButton) popup.list.GetChildAt(i));
		}
		popup.contentPane.onClick.Add(() => {
			popup.contentPane.onClick.Clear();
			GoNextChoise(cNext);
		});
	}

	private void PreItemShowHighlight(MChoise c, GButton bt){
		if(!string.IsNullOrEmpty(c.choise.ContentHighlight)){
			var b = c.text.IndexOf(c.choise.ContentHighlight);
			if(b >= 0){
				var tf = bt.GetChild("title").asRichTextField;
				string raw = c.text;
				string replace = c.choise.ContentHighlight;
				raw = raw.Replace(replace, "[color=#000000][u]" + replace + "[/u][/color]");
				tf.text = raw;
			}
		}
	}

	private void RefreshItemShowHighlight(int idxChoise, int nextIdxChoise, MChoise c, GButton bt){
		ScriptGui.MsgboxChoise.ETypeHighlight type = (ScriptGui.MsgboxChoise.ETypeHighlight)c.choise.TypeHighlight;
		bool isPick = idxChoise == nextIdxChoise;
		bool isRight = type == ScriptGui.MsgboxChoise.ETypeHighlight.Full_Green;
		if(isPick){
			if(isRight){
				bt.GetController("c1").SetSelectedPage("p_r");
			}else
			{
				bt.GetController("c1").SetSelectedPage("p_w");
			}
		}else
		{
			if(isRight){
				bt.GetController("c1").SetSelectedPage("j_r");
			}else{
				bt.GetController("c1").SetSelectedPage("j_w");
			}
		}
		if(!isPick){
			bt.text = c.text;
		}else if(!string.IsNullOrEmpty(c.choise.ContentHighlight)){
			if(type == ScriptGui.MsgboxChoise.ETypeHighlight.Full_Green){
				var b = c.text.IndexOf(c.choise.ContentHighlight);
				if(b >= 0){
					var tf = bt.GetChild("title").asRichTextField;
					string raw = c.text;
					string replace = c.choise.ContentHighlight;
					raw = raw.Replace(replace, "[color=#4c9d5f][u]" + replace + "[/u][/color]");
					tf.text = raw;
				}
			}else
			{
				var b = c.text.IndexOf(c.choise.ContentHighlight);
				if(b >= 0){
					var tf = bt.GetChild("title").asRichTextField;
					string raw = c.text;
					string replace = c.choise.ContentHighlight;
					raw = raw.Replace(replace, "[color=#FF3300][u]" + replace + "[/u][/color]");
					tf.text = raw;
				}
			}
		}
		bt.grayed = true;
		bt.selected = false;
	}
}
