using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PygmyMonkey.FileBrowser;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AON.RpgMapEditor
{
    public class ScriptGui : ScriptGuiBase
    {

        private static ScriptGui _instance = null;

        public static ScriptGui Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ScriptGui();
                }
                return _instance;
            }
        }

        private ScriptGui() { }

        // private float heighTextField = 25f;
        private Vector2 mScrollPosScope = Vector2.zero;
        private Vector2 mScrollPosMsg = Vector2.zero;
        private Vector2 mScrollPosPickScope = Vector2.zero;

        private Script ScriptTarget{get;set;}
        private int ScopeSelect{get;set;}
        // private bool mIsHasUpdateScript = false;
        private string mFocusControlNextFrame = "";

        public void ResetCache()
        {
            Debug.Log("ScriptGui ResetCache");
            ScriptTarget = null;
            // mScriptYaml = null;
            ScopeSelect = 0;
            // mIsHasUpdateScript = false;
        }

        private void UpdateScriptTarget(Script script)
        {
            if (ScriptTarget != script)
            {
                // SaveCurrentYamlToStr(true);
                //Reload data
                // if (script.data == null)
                // {
                //     script.data = "";
                // }
                ScriptTarget = script;
                ScopeSelect = 0;
                // mIsHasUpdateScript = false;
                // mScriptYaml = null;
                // try
                // {
                //     var deserializer = new DeserializerBuilder()
                //     .WithNamingConvention(new CamelCaseNamingConvention())
                //     .IgnoreUnmatchedProperties()
                //     .Build();
                //     mScriptYaml = deserializer.Deserialize<ScriptYaml>(script.data);
                //     mScriptYaml.IdToObj();
                // }
                // catch
                // {
                // }
                ComboBoxHelper.Instance.ResetDataScopeList();
            }
            // Fix scriptYaml null
            if (ScriptTarget.ScriptYaml == null)
            {
                ScriptTarget.ScriptYaml = new ScriptYaml().Init();
            }
            if (ScriptTarget.ScriptYaml.ListActions().Count == 0)
            {
                ScriptTarget.ScriptYaml.ActionDataAdd(new ActionData().Init());
            }
        }

        /*
        public void SaveCurrentYamlToStr(bool isForceUpdate)
        {
            if (mScriptTarget != null && (isForceUpdate || mIsHasUpdateScript))
            {
                var serializer = new SerializerBuilder().Build();
                mScriptYaml.ObjToID();
                var yaml = serializer.Serialize(mScriptYaml);
                mScriptYaml.IdToObj();
                if (yaml != null)
                {
                    mScriptTarget.data = yaml;
                }
                mIsHasUpdateScript = false;
                Debug.Log("SaveCurrentYamlToStr");
            }
        }
         */

        public bool ScriptOnGUI(Script script, Rect rectMenuRight, AutoTileMap autoTileMap, TilesetAON tilesetAON, ref bool isShowMoreInfo, AComponent_Button.OnClick onCloseDialog)
        {
            if(script == null){
                return false;
            }
            isShowMoreInfo = false;
            // Input Name
            // float yGui = rectMenuRight.y + 8f;
            // GUI.Label(new Rect(rectMenuRight.x, yGui + DefineAON.GUI_Y_Label, rectMenuRight.width, DefineAON.GUI_Height_Label), string.Format("Edit Script {0}", script.NameScript));
            // yGui += 32;
            float yGui = 4f;
            UpdateScriptTarget(script);
            // Rect rectFull = new Rect(0, 0, Screen.width - rectMenuRight.width, Screen.height);
            Rect rectFull = new Rect(0, 0, Screen.width, Screen.height);
            AONGUI.Box(rectFull, "");
            yGui = 0;
            float left = 80f;
            bool hasUpdatedata = OnGUIScriptYaml(script, autoTileMap, tilesetAON, yGui, left, rectFull);
            if (hasUpdatedata)
            {
                // mIsHasUpdateScript = true;
            }
            else
            {
                if (mFocusControlNextFrame != "")
                {
                    AONGUI.FocusControl(mFocusControlNextFrame);
                    mFocusControlNextFrame = "";
                }
            }
            yGui = 0;
            if (menuLeft == EMenuLeft.General)
            {
                Rect rectGeneral = new Rect(200, 0, 400, Screen.height);
                float x = rectGeneral.x + 4;
                AONGUI.Label(new Rect(x, yGui + DefineAON.GUI_Y_Label, 40, DefineAON.GUI_Height_Label), "Name: ");
                x += 40;
                AONGUI.TextField(new Rect(x, yGui + DefineAON.GUI_Y_TextField, 306, DefineAON.GUI_Height_TextField), script.NameScript, 25, (string text) => {
                    script.NameScript = text;
                });
                yGui += 32f;
                // x = rectGeneral.x + 4;
                AONGUI.Button(new Rect(x, yGui + DefineAON.GUI_Y_Button, 150, DefineAON.GUI_Height_Button), "Save script data", () => {
                    // SaveCurrentYamlToStr(true);
                    var title = "Save script data";
                    var namefile = string.Format("map{0}_script{1}.txt", autoTileMap.MapIdxSelect, autoTileMap.MapSelect.ScriptData.IndexOf(script));
                    FileBrowser.SaveFilePanel(title, title, Application.persistentDataPath, namefile, new string[] { "txt" }, null, (bool canceled, string filePath) =>
                    {
                        if (canceled)
                        {
                            return;
                        }
                        File.WriteAllText(filePath, script.YamlData);
                    });
                });
                x += 160;
                AONGUI.Button(new Rect(x, yGui + DefineAON.GUI_Y_Button, 150, DefineAON.GUI_Height_Button), "Load script data", () => {
                    // SaveCurrentYamlToStr(true);
                    var title = "Load script data";
                    var namefile = string.Format("map{0}_script{1}.txt", autoTileMap.MapIdxSelect, autoTileMap.MapSelect.ScriptData.IndexOf(script));
                    var path = Application.persistentDataPath + "/" + namefile;
                    FileBrowser.OpenFilePanel(title, path, new string[] { "txt" }, null, (bool canceled, string filePath) =>
                    {
                        if (canceled)
                        {
                            return;
                        }
                        var data = File.ReadAllText(filePath);
                        script.YamlData = data;
                    });
                });
                yGui += 32;
            }
            if(onCloseDialog != null){
                AONGUI.Button(new Rect(4, 4, 150, DefineAON.GUI_Height_Button), "Close edit Script (Esc)", KeyCode.Escape, onCloseDialog);
            }
            return false;
        }

        public enum EMenuLeft : int
        {
            General = 0,
            Flag = 1,
            Action = 2,
            Scope = 3
            // 
        }
        public static string[] StrMenuLeft = Enum.GetNames(typeof(EMenuLeft));
        private EMenuLeft menuLeft = EMenuLeft.General;
        private bool OnGUIMenuLeft(Script script, AutoTileMap autoTileMap, TilesetAON tilesetAON, ScriptYaml scriptYaml, float yGui, Rect rect)
        {
            AONGUI.Box(rect, "", tilesetAON.ListStyleBlack2);
            {
                // Flag
                // yGui += 16;
                int current = (int)menuLeft;
                float h = StrMenuLeft.Length * 32;
                AONGUI.SelectionGrid(new Rect(rect.x, yGui, rect.width, h), current, StrMenuLeft, 1, tilesetAON.ListStyleGrid, (int next) => {
                    menuLeft = (EMenuLeft)next;
                });
                yGui += h;
                // yGui += 16;
            }
            if (menuLeft == EMenuLeft.Scope)
            {
                // Pick ScopeList
                AONGUI.Toggle(new Rect(rect.x + 4, yGui + DefineAON.GUI_Y_Label, rect.width, DefineAON.GUI_Height_Label), ComboBoxHelper.Instance.ShowIndexScope, " Show Index Scope ", (bool b) => {
                    ComboBoxHelper.Instance.ShowIndexScope = b;
                });
                yGui += 32f;
                GUIStyle listStyle = tilesetAON.ListStyleGrid;
                var comboBoxScopeList = ComboBoxHelper.Instance.ScopeList(scriptYaml.ListActions());
                float contentHeight = listStyle.CalcHeight(comboBoxScopeList.ListContent[0], 1.0f) * comboBoxScopeList.ListContent.Length;
                Rect view = new Rect(rect.x, yGui, rect.width, rect.height - yGui);
                Rect listRect = new Rect(rect.x, yGui, rect.width - 16, contentHeight);
                AONGUI.BeginScrollView(view, mScrollPosScope, listRect, false, true, (Vector2 v) => {
                    mScrollPosScope = v;
                });
                AONGUI.SelectionGrid(listRect, ScopeSelect, comboBoxScopeList.ListContent, 1, listStyle, (int scopeSelectNext) => {
                    ScopeSelect = scopeSelectNext;
                });
                AONGUI.EndScrollView();
            }
            return false;
        }

        private bool OnGUIScriptYaml(Script script, AutoTileMap autoTileMap, TilesetAON tilesetAON, float yGui, float left, Rect rect)
        {
            bool hasUpdateData = false;
            ScriptYaml scriptYaml = script.ScriptYaml;
            if (scriptYaml != null)
            {
                if (scriptYaml.ListActions().Count == 0)
                {
                    scriptYaml.ActionDataAdd(new ActionData().Init());
                }
                if (ScopeSelect < 0)
                {
                    ScopeSelect = 0;
                }
                else if (ScopeSelect >= scriptYaml.ListActions().Count)
                {
                    ScopeSelect = scriptYaml.ListActions().Count - 1;
                }
                float W_ScopeList = 200;
                float W_Content = rect.width - W_ScopeList;
                rect.width = W_ScopeList;
                yGui = 48f;
                if (OnGUIMenuLeft(script, autoTileMap, tilesetAON, scriptYaml, yGui, rect))
                {
                    // return true;
                }
                yGui = 0f;
                rect.x = W_ScopeList;
                rect.width = W_Content;
                bool isWaitUI = false;
                float countHeightBegin = yGui;
                float limitHeight = rect.height - yGui;
                float LastContentHeight = LastContentEnd - yGui;
                if (LastContentHeight < limitHeight)
                {
                    LastContentHeight = limitHeight;
                    LastContentEnd = yGui + limitHeight;
                }
                bool hasScroll = LastContentHeight > limitHeight;
                if (hasScroll)
                {
                    rect.width = rect.width - 16f;
                    Rect view = new Rect(rect.x, rect.y + yGui,
                            rect.width + 16f, limitHeight);
                    Rect listRect = new Rect(rect.x, rect.y + yGui,
                            rect.width, LastContentHeight);
                    AONGUI.BeginScrollView(view, mScrollPosMsg, listRect, false, false, (Vector2 v) => {
                        mScrollPosMsg = v;
                    });
                }
                hasUpdateData = OnGUIContent(script, ref yGui, ref isWaitUI, rect, autoTileMap, tilesetAON);
                if (hasScroll)
                {
                    AONGUI.EndScrollView();
                }
                if (ComboBox.IsOnShow() == false || yGui > LastContentEnd)
                {
                    LastContentEnd = yGui;
                }
                if (isWaitUI)
                {
                    return false;
                }
            }
            return hasUpdateData;
        }

        private bool OnGUIContent(Script script, ref float yGui, ref bool isWaitUI, Rect rect, AutoTileMap autoTileMap, TilesetAON tilesetAON)
        {
            var scriptYaml = script.ScriptYaml;
            if (menuLeft == EMenuLeft.Flag)
            {
                // Flag
                if (scriptYaml.FlagsYaml == null)
                {
                    scriptYaml.FlagsYaml = new Flags();
                }
                Flags f = scriptYaml.FlagsYaml;
                // Should have key "ShowInteractions" in Start
                if (scriptYaml.FlagReset == null)
                {
                    scriptYaml.FlagReset = new List<string>();
                }
                FlagGui.DisOnGUI(f, ref yGui, rect, ScriptYaml.LockFlag, tilesetAON.ListStyleBlack2, "Flag script edit :", scriptYaml.FlagReset);
            }
            else if (menuLeft == EMenuLeft.Action)
            {
                // Action Flag
                if (scriptYaml.FlagActions == null)
                {
                    scriptYaml.FlagActions = new List<FlagAction>();
                }
                if (scriptYaml.FlagActions.Count == 0)
                {
                    scriptYaml.FlagActions.Add(new FlagAction());
                }
                scriptYaml.FlagActions[0].Name = "";
                scriptYaml.FlagActions[0].Key = "";
                scriptYaml.FlagActions[0].Value = 0;
                if (FlagAction.OnGUIFlagActionList(ref yGui, ref isWaitUI, new Rect(rect.x, rect.y, rect.width, rect.height), scriptYaml.FlagActions, scriptYaml.FlagsYaml, tilesetAON.ListStyleBlack2))
                {
                    //Update all ref FlagAction in MsgChat
                    scriptYaml.UpdateRefActionFlagInMsgChat();
                    return true;
                }
                if (isWaitUI)
                {
                    return false;
                }
            }
            else if (menuLeft == EMenuLeft.Scope)
            {
                var actionData = scriptYaml.ActionDataAt(ScopeSelect);
                float left = 80f;
                if (OnGUIActionYaml(script, ref isWaitUI, autoTileMap, tilesetAON, ScopeSelect, actionData, ref yGui, left, rect))
                {
                    return true;
                }
            }
            return false;
        }

        private float LastContentEnd = 0;
        private bool OnGUIActionYaml(Script script, ref bool isWaitUI, AutoTileMap autoTileMap, TilesetAON tilesetAON, int indexScope, ActionData actionData, ref float yGui, float left, Rect rect)
        {
            // if(OnPickFlagAction != null){
            // 	var content = ComboBoxHelper.Instance.FlagAction(scriptYaml);
            // 	if(content.Length <= 0){
            // 		GUI.Label(new Rect(rect.x + 4, yGui + DefineAON.GUI_Y_Label, rect.width, DefineAON.GUI_Height_Label), "Don't have flag action");
            // 		if(GUI.Button(new Rect(rect.x + rect.width - 116, yGui + 4f, 100, 24), "Close")){
            // 			OnPickFlagAction = null;
            // 			return true;
            // 		}
            // 		return false;
            // 	}
            // 	GUI.Label(new Rect(rect.x + 4, yGui + DefineAON.GUI_Y_Label, rect.width, DefineAON.GUI_Height_Label), "Pick flag action :");
            // 	if(GUI.Button(new Rect(rect.x + rect.width - 116, yGui + 4f, 100, 24), "Close")){
            // 		OnPickFlagAction = null;
            // 		return true;
            // 	}
            // 	yGui += 32f;
            // 	// Pick ScopeList
            // 	GUIStyle listStyle = ComboBoxHelper.Instance.GUIStyle();
            // 	float contentHeight = listStyle.CalcHeight(content[0], 1.0f) * content.Length;
            // 	Rect listRect = new Rect(rect.x, yGui, rect.width, contentHeight);
            // 	int actionNext = GUI.SelectionGrid(listRect, CurrentFlagActionOnPick, content, 1, listStyle);
            // 	if(CurrentFlagActionOnPick != actionNext){
            // 		OnPickFlagAction( scriptYaml.ActionFlagAt(actionNext), actionNext);
            // 		OnPickFlagAction = null;
            // 		return true;
            // 	}
            // 	// OnPickScope = null;
            // 	return false;
            // }
            var scriptYaml = script.ScriptYaml;
            { // Top
                float xGui2 = rect.x + 8;
                AONGUI.Button(new Rect(xGui2, yGui + DefineAON.GUI_Y_Button, 120, DefineAON.GUI_Height_Button), "Add block bellow", () => {
                    var act = new ActionData().Init();
                    // if(actionData.Name == null || actionData.Name == ""){
                    // 	act.Name = mScriptYaml.ListActions().Count.ToString();
                    // }else{
                    // 	act.Name = (int.Parse(actionData.Name) + 1).ToString();
                    // }
                    scriptYaml.ActionDataInsert(ScopeSelect + 1, act);
                    // scriptYaml.Main.Add( new ActionData().Init());
                    ComboBoxHelper.Instance.ResetDataScopeList();
                    ScopeSelect = ScopeSelect + 1;
                });
                xGui2 += 120 + 16;
                if (ScopeSelect != 0)
                {
                    AONGUI.Button(new Rect(xGui2, yGui + DefineAON.GUI_Y_Button, 120, DefineAON.GUI_Height_Button), "Remove this block", () => {
                        scriptYaml.ActionRemoveAt(ScopeSelect);
                        ComboBoxHelper.Instance.ResetDataScopeList();
                    });
                }
                xGui2 += 120 + 16;
                if (ScopeSelect != 0 )
                {
                    AONGUI.Button(new Rect(xGui2, yGui + DefineAON.GUI_Y_Button, 84, DefineAON.GUI_Height_Button), "Save scope", () => {
                        var title = "Save scope data";
                        var namefile = string.Format("map{0}_script{1}_scope{2}.txt", autoTileMap.MapIdxSelect, autoTileMap.MapSelect.ScriptData.IndexOf(script), ScopeSelect);
                        FileBrowser.SaveFilePanel(title, title, Application.persistentDataPath, namefile, new string[] { "txt" }, null, (bool canceled, string filePath) =>
                        {
                            if (canceled)
                            {
                                return;
                            }
                            scriptYaml.ObjToID();
                            var scopeData = scriptYaml.ActionDataAt(ScopeSelect);
                            var serializer = new SerializerBuilder().Build();
                            var yaml = serializer.Serialize(scopeData);
                            File.WriteAllText(filePath, yaml);
                            //Reload Ref Again
                            scriptYaml.IdToObj();
                        });
                    });
                }
                xGui2 += 84 + 16;
                if (ScopeSelect != 0)
                {
                    AONGUI.Button(new Rect(xGui2, yGui + DefineAON.GUI_Y_Button, 84, DefineAON.GUI_Height_Button), "Load scope", () => {
                        var title = "Load scope data";
                        var namefile = string.Format("map{0}_script{1}_scope{2}.txt", autoTileMap.MapIdxSelect, autoTileMap.MapSelect.ScriptData.IndexOf(script), ScopeSelect);
                        var path = Application.persistentDataPath + "/" + namefile;
                        FileBrowser.OpenFilePanel(title, path, new string[] { "txt" }, null, (bool canceled, string filePath) =>
                        {
                            if (canceled)
                            {
                                return;
                            }
                            try
                            {
                                var data = File.ReadAllText(filePath);
                                var deserializer = new DeserializerBuilder()
                                .WithNamingConvention(new CamelCaseNamingConvention())
                                .IgnoreUnmatchedProperties()
                                .Build();
                                ActionData scopeData = deserializer.Deserialize<ActionData>(data);
                                scriptYaml.SetActionData(ScopeSelect, scopeData);
                                scriptYaml.IdToObj();
                            }
                            catch
                            {
                                InputFieldHelper.Instance.ShowNoti("Can't load scope");
                            }
                        });
                    });
                }
            }
            yGui += 32;
            if (ScopeSelect == 0)
            {
                AONGUI.Label(new Rect(rect.x + 4, yGui + DefineAON.GUI_Y_Label, rect.width, DefineAON.GUI_Height_Label), "Setting");
                yGui += 32;
                // Next
                    AONGUI.Label(new Rect(rect.x + 4, yGui + DefineAON.GUI_Y_Label, left - 4, DefineAON.GUI_Height_Label), "Start :");
                string hash = "scope_start";
                var t = scriptYaml.Begin;
                OnGUiPickScope(hash, t, rect.x + left, yGui, rect.width - left, ref yGui, ref isWaitUI, (ActionData pickScope ) => {
                    scriptYaml.Begin = pickScope; 
                });
            }
            bool hasUpdateData = false;
            int xGui = 4;
            {
                // string name = (actionData.ContainsKey("name") ? actionData["name"] : "");
                string name = (actionData.Name != null ? actionData.Name : "");
                AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, left - 4, DefineAON.GUI_Height_Label), "Name : ");
                AONGUI.TextField(new Rect(rect.x + left, yGui + DefineAON.GUI_Y_TextField, rect.width - left, DefineAON.GUI_Height_TextField), name, 25, (string text) => {
                    actionData.Name = text;
                    ComboBoxHelper.Instance.UpdateScopeName(indexScope, text);
                });
                yGui += 32;
            }
            {
                // Pick format
                // int format = System.Convert.ToInt32(actionData.ContainsKey("format") ? actionData["format"] : "0");
                int format = (int)actionData.Format;
                AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Button, left - 4, DefineAON.GUI_Height_Label), "Format :");
                var ComboBoxFormat = ComboBoxHelper.Instance.FormatScope();
                ComboBoxFormat.SelectedItemIndex = format;
                ComboBoxFormat.Rect.x = rect.x + left;
                ComboBoxFormat.Rect.y = yGui;
                ComboBoxFormat.Rect.width = rect.width - left;
                ComboBoxFormat.Rect.height = 32f;
                ComboBoxFormat.Show(rect.height - yGui - 32, indexScope.ToString(), (int selectedFormat) => {
                    format = selectedFormat;
                    // actionData.format = Enum.ToObject(typeof(EFormatScope), selectedFormat));
                    actionData.Format = (EFormatScope)selectedFormat;
                    hasUpdateData = true;
                });
                if (ComboBoxFormat.IsDropDownWithHash(indexScope.ToString()))
                {
                    isWaitUI = true;
                    return false;
                }
                yGui += 32f;
                if (actionData.Format == EFormatScope.MsgboxChat)
                {
                    AONGUI.Toggle(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, rect.width - xGui, DefineAON.GUI_Height_Label), actionData.RandomOnce, " This scope will show once", (bool b) => {
                        actionData.RandomOnce = b;
                    });
                    yGui += 32f;
                }
                else if (actionData.RandomOnce)
                {
                    actionData.RandomOnce = false;
                    hasUpdateData = true;
                }
                yGui += 16f;
                // Content format
                if (actionData.Format == EFormatScope.End)
                {
                    AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, rect.width - xGui, DefineAON.GUI_Height_Label), "- End scope -");
                    yGui += 32f;
                }
                else if (actionData.Format == EFormatScope.Check)
                {
                    #region EFormatScope.Check
                    yGui += 16;
                    float sizeToHalf = (rect.width - left) / 2;
                    {
                        string[] d = StrFlagTarget;
                        int current_target = actionData.Check.Target;
                        AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, 100, DefineAON.GUI_Height_Label), "Target :");
                        AONGUI.SelectionGrid(new Rect(rect.x + left, yGui + 6, sizeToHalf, 26), current_target, d, d.Length, tilesetAON.ListStyleGrid, (int targetNext) => {
                            actionData.Check.Target = targetNext;
                        });
                        yGui += 32f;
                    }
                    int count_case = 0;
                    if (actionData.Check.SubCheck != null)
                    {
                        count_case = actionData.Check.SubCheck.Count;
                    }
                    {
                        string[] d = StrCountSwitch;
                        AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, 100, DefineAON.GUI_Height_Label), "Total case :");
                        AONGUI.SelectionGrid(new Rect(rect.x + left, yGui + 2, sizeToHalf, 26), count_case, d, d.Length, tilesetAON.ListStyleGrid, (int next) => {
                            if (next == 0)
                            {
                                actionData.Check.SubCheck = null;
                            }
                            else if (actionData.Check.SubCheck == null)
                            {
                                actionData.Check.SubCheck = new List<SubCheck>();
                                for (int i = 0; i < next; i++)
                                {
                                    actionData.Check.SubCheck.Add(new SubCheck().Init());
                                }
                            }
                            else if (next > count_case)
                            {
                                for (int i = count_case; i < next; i++)
                                {
                                    actionData.Check.SubCheck.Add(new SubCheck().Init());
                                }
                            }
                            else
                            { // next < count_to
                                actionData.Check.SubCheck.RemoveRange(next, count_case - next);
                            }
                        });
                        yGui += 32f;
                    }
                    AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, 100, DefineAON.GUI_Height_Label), "Check flag");
                    {
                        float xGui2 = rect.x + left;
                        float _w = sizeToHalf;
                        string if_a = actionData.Check.Flag;
                        Flags flagTarget = FlagByTarget(scriptYaml, autoTileMap, (actionData.Check == null ? (int)FlagTarget.Script : actionData.Check.Target));
                        var comboBoxFlags = FlagGui.Instance.UpdateFlagsData(flagTarget, if_a);
                        int index_a = FlagGui.Instance.IndexOfKey(actionData.Check.Flag);
                        comboBoxFlags.SelectedItemIndex = index_a;
                        comboBoxFlags.Rect.x = xGui2;
                        comboBoxFlags.Rect.y = yGui;
                        comboBoxFlags.Rect.width = _w;
                        comboBoxFlags.Rect.height = 32f;
                        float limitHeight = 32f * 6;
                        comboBoxFlags.Show(limitHeight, indexScope.ToString(), (int flagNext) => {
                            actionData.Check.Flag = FlagGui.Instance.KeyFromIndex(flagNext);
                            hasUpdateData = true;
                        });
                        if (comboBoxFlags.IsDropDownWithHash(indexScope.ToString()))
                        {
                            yGui += limitHeight;
                            isWaitUI = true;
                            return false;
                        }
                    }
                    yGui += 32f;
                    count_case += 2;
                    for (int z = 0; z < count_case; z++)
                    {
                        bool isBegin = z == 0;
                        bool isEnd = z == count_case - 1;
                        SubCheck subCheck = (isEnd || isBegin) ? null : actionData.Check.SubCheck[z - 1];
                        // Switch
                        {
                            // Left switch
                            float xGuiTo = rect.x + left;
                            float xGuiToEnd = xGuiTo + sizeToHalf;
                            if (!isEnd)
                            {
                                int current = subCheck == null ? actionData.Check.Compare : subCheck.Compare;
                                var comboBoxOperation = ComboBoxHelper.Instance.ECompare();
                                comboBoxOperation.SelectedItemIndex = current;
                                comboBoxOperation.Rect.x = xGuiTo;
                                comboBoxOperation.Rect.y = yGui;
                                comboBoxOperation.Rect.width = 30;
                                comboBoxOperation.Rect.height = 32f;
                                float limitHeight = comboBoxOperation.HeightForShowAll();
                                comboBoxOperation.Show(limitHeight, z.ToString(), false, (int next) => {
                                    if (subCheck != null)
                                    {
                                        subCheck.Compare = next;
                                    }
                                    else
                                    {
                                        actionData.Check.Compare = next;
                                    }
                                });
                                if (comboBoxOperation.IsDropDownWithHash(z.ToString()))
                                {
                                    yGui += limitHeight;
                                    isWaitUI = true;
                                    return false;
                                }
                                xGuiTo += 30;
                                int if_b = subCheck == null ? actionData.Check.Value : subCheck.Value;
                                AONGUI.TextField(new Rect(xGuiTo, yGui + DefineAON.GUI_Y_TextField, xGuiToEnd - xGuiTo, DefineAON.GUI_Height_TextField), if_b.ToString(), (string text) => {
                                    var if_b_next = UtilsAON.StrToIntDef(text);
                                    if (subCheck != null)
                                    {
                                        subCheck.Value = if_b_next;
                                    }
                                    else
                                    {
                                        actionData.Check.Value = if_b_next;
                                    }
                                });
                            }
                            else
                            {
                                AONGUI.Label(new Rect(xGuiTo, yGui, 100, DefineAON.GUI_Height_Label), "Defause");
                            }
                        }
                        {
                            float xGuiTo = rect.x + left + sizeToHalf;
                            float xGuiToEnd = xGuiTo + sizeToHalf;
                            AONGUI.Label(new Rect(xGuiTo + 10, yGui, 30, DefineAON.GUI_Height_Label), "to");
                            xGuiTo += 30;
                            var t = isBegin ? actionData.Check.Right : isEnd ? actionData.Check.Wrong : subCheck.Right;
                            OnGUiPickScope(z.ToString(), t, xGuiTo, yGui, xGuiToEnd - xGuiTo, ref yGui, ref isWaitUI, (ActionData pickScope) => {
                                if (isBegin)
                                {
                                    actionData.Check.Right = pickScope;
                                }
                                else if (isEnd)
                                {
                                    actionData.Check.Wrong = pickScope;
                                }
                                else
                                {
                                    subCheck.Right = pickScope;
                                }
                            });
                        }
                        yGui += 32f;
                    }
                    #endregion EFormatScope.Check
                }
                else if (actionData.Format == EFormatScope.Set)
                {
                    #region EFormatScope.Set
                    yGui += 16;
                    {
                        string[] d = StrFlagTarget;
                        int current_target = actionData.Set.Target;
                        AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, 100, DefineAON.GUI_Height_Label), "Target:");
                        AONGUI.SelectionGrid(new Rect(rect.x + left, yGui + 6, 200, 26), current_target, d, d.Length, tilesetAON.ListStyleGrid, (int targetNext) => {
                            actionData.Set.Target = targetNext;
                        });
                        yGui += 32f;
                    }
                    {
                        float size_e = 50;
                        AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, 50, DefineAON.GUI_Height_Label), "Action");
                        List<FlagAction> listFlagAction = ListFlagActionByTarget(scriptYaml, autoTileMap, actionData.Set.Target);
                        string keyAction = actionData.Set.Action;
                        var combobox = ComboBoxHelper.Instance.FlagAction(listFlagAction);
                        var hash = indexScope.ToString();
                        int currentFlagAction = FlagAction.IndextFlagAction(listFlagAction, keyAction);
                        if (currentFlagAction == -1)
                        {
                            if (keyAction != null && keyAction != "")
                            {
                                combobox.Empty = keyAction + " (not found)";
                            }
                            else
                            {
                                combobox.Empty = "NULL";
                            }
                        }
                        combobox.SelectedItemIndex = currentFlagAction;
                        combobox.Rect.x = rect.x + left;
                        combobox.Rect.y = yGui;
                        combobox.Rect.width = rect.width - left - size_e;
                        combobox.Rect.height = 32f;

                        float limitHeight = 32f * 6;
                        combobox.Show(limitHeight, hash, (int nextAction) => {
                            actionData.Set.Action = listFlagAction[nextAction].Name;
                        });
                        if (combobox.IsDropDownWithHash(hash))
                        {
                            yGui += limitHeight;
                            isWaitUI = true;
                            return false;
                        }
                    }
                    yGui += 32f;
                    {
                        // Next
                        AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, 50, DefineAON.GUI_Height_Label), "Next");
                        var t = actionData.Set.Next;
                        OnGUiPickScope("next", t, rect.x + left, yGui, rect.width - left, ref yGui, ref isWaitUI, ( ActionData pickScope) => {
                           actionData.Set.Next = pickScope; 
                        });
                        yGui += 32f;
                    }
                    #endregion EFormatScope.Set
                }
                else if (actionData.Format == EFormatScope.MsgboxChat)
                {
                    #region EFormatScope.MsgboxChat
                    if (actionData.MsgboxChat.Count == 0)
                    {
                        actionData.MsgboxChat.Add(new MsgboxChat().Init());
                    }
                    yGui += 16f;
                    //

                    // float size_e = 70;
                    float size_i = rect.width - 4;
                    float size_rate = 50;
                    float size_sub_e = 30; // Bt (-)
                    float size_sub_e_2 = 80; // Bt (+)
                    float size_sub_t = 240; // Action & Next
                    float size_hightlight = 200;
                    float size_sub_c = size_i - size_sub_e - size_sub_e_2 - size_sub_t - size_rate - size_hightlight;
                    float xHightlight = rect.x + xGui + size_rate + size_sub_c;
                    for (int ii = 0; ii < actionData.MsgboxChat.Count; ii++)
                    {
                        int i = ii;
                        {
                            //Line
                            yGui += 8;
                            AONGUI.Box(new Rect(rect.x, yGui + 8, rect.width, 8), "", tilesetAON.ListStyleBlack);
                            yGui += 16;
                        }
                        var msg = actionData.MsgboxChat[i];
                        {
                            // float xGui0 = rect.x + xGui;
                            // GUI.Box(new Rect(rect.x, yGui, rect.width, 34 + 8), "", tilesetAON.ListStyleBlack);
                            AONGUI.Label(new Rect(rect.x + 10, yGui + DefineAON.GUI_Y_Label, 200, DefineAON.GUI_Height_Label), i.ToString() + ".");
                            // xGui0 += size_l;
                            // {
                            // 	//Edit Value
                            // 	string content = msg.Value;
                            // 	var _content = GUI.TextField(new Rect( xGui0, yGui + 32 - heighTextField, size_i - 4, heighTextField), content);
                            // 	xGui0 += size_i;
                            // 	if(_content != content){
                            // 		msg.Value = _content;
                            // 		return true;
                            // 	}
                            // }
                            yGui += 32f;
                        }
                        if (msg.MsgboxChoise == null)
                        {
                            //Check MsgboxChoise
                            msg.MsgboxChoise = new List<MsgboxChoise>();
                            msg.MsgboxChoise.Add(new MsgboxChoise().Init());
                        }
                        if (msg.Type == MsgboxChat.EType.Chat)
                        {
                            msg.Group = 1;
                            //Clear Next & Action When Chat
                            for (int j = 0; j < msg.MsgboxChoise.Count; j++)
                            {
                                var choise = msg.MsgboxChoise[j];
                                choise.Next = null;
                                choise.Action = null;
                            }
                        }
                        else if (msg.Type == MsgboxChat.EType.Choise)
                        {
                            msg.Group = 1;
                        }
                        else if (msg.Type == MsgboxChat.EType.Group2)
                        {
                            if (msg.Group != 2)
                            {
                                msg.Group = 2;
                            }
                        }
                        else if (msg.Type == MsgboxChat.EType.Group3)
                        {
                            if (msg.Group != 3)
                            {
                                msg.Group = 3;
                            }
                        }
                        else if (msg.Type == MsgboxChat.EType.Group4)
                        {
                            if (msg.Group != 4)
                            {
                                msg.Group = 4;
                            }
                        }
                        if (msg.MsgboxChoise.Count % msg.Group != 0)
                        {
                            if (msg.MsgboxChoise.Count < msg.Group)
                            {
                                int add = msg.Group - (msg.MsgboxChoise.Count % msg.Group);
                                for (; add > 0; add--)
                                {
                                    msg.MsgboxChoise.Add(new MsgboxChoise().Init());
                                }
                            }
                            else
                            {
                                // int removeFrom = (msg.MsgboxChoise.Count % msg.Group);
                                while (msg.MsgboxChoise.Count % msg.Group != 0)
                                {
                                    msg.MsgboxChoise.RemoveAt(msg.MsgboxChoise.Count - 1);
                                }
                            }
                        }
                        {
                            //Pick Type
                            string[] d = MsgboxChat.StrEType;
                            int current_type = (int)msg.Type;
                            Debug.Log("current_type: " + current_type);
                            AONGUI.Label(new Rect(rect.x + 10, yGui + DefineAON.GUI_Y_Label, 100, DefineAON.GUI_Height_Label), "Input:");
                            AONGUI.SelectionGrid(new Rect(rect.x + 100, yGui + 6, rect.width - 100, 26), current_type, d, d.Length, tilesetAON.ListStyleGrid, (int typeNext) => {
                                msg.Type = (MsgboxChat.EType)typeNext;
                                Debug.Log("msg.Type Update: " + msg.Type);
                            });
                            yGui += 32f;
                        }
                        {
                            // Random
                            float xSubGui0 = rect.x + xGui;
                            float xSubGuiTop = xSubGui0 + 4;
                            // if(GUI.Button( new Rect(xSubGui0, yGui + 4, 120, 28), "Clear multiple") ){
                            // 	msg.MsgboxChoise = null;
                            // 	return true;
                            // }
                            if (msg.Group > 1)
                            {
                                AONGUI.Label(new Rect(xSubGuiTop, yGui + DefineAON.GUI_Y_Label, rect.width - xSubGuiTop, DefineAON.GUI_Height_Label), "Random groups");
                                // Force random is true
                                if (msg.Random != true)
                                {
                                    msg.Random = true;
                                }
                            }
                            else
                            {
                                AONGUI.Toggle(new Rect(xSubGuiTop, yGui + 6, 140, 28), msg.Random, msg.Type == MsgboxChat.EType.Chat ? " Random Chat" : " Random Choice", (bool b) => {
                                    msg.Random = b;
                                });
                            }
                            xSubGuiTop += 140;
                            if (msg.Random == true)
                            {
                                AONGUI.Toggle(new Rect(xSubGuiTop, yGui + 6, 140, 28), msg.RandomOnce, " Random Once", (bool b) => {
                                    msg.RandomOnce = b;
                                });
                            }
                            xSubGuiTop += 140;
                            if (msg.Type == MsgboxChat.EType.Chat)
                            {
                                AONGUI.Toggle(new Rect(xSubGuiTop, yGui + 6, 140, 28), msg.BreakLine, " Break Line", (bool b) => {
                                    msg.BreakLine = b;
                                });
                                xSubGuiTop += 140;
                            }
                            if (msg.Type != MsgboxChat.EType.Chat)
                            {
                                AONGUI.Toggle(new Rect(xHightlight, yGui + 6, size_hightlight, 28), msg.Highlight, " Highlight", (bool b) => {
                                    msg.Highlight = b;
                                });
                            }
                            else if (msg.Highlight)
                            {
                                msg.Highlight = false;
                            }
                            if (msg.Type != MsgboxChat.EType.Chat)
                            {
                                AONGUI.Toggle(new Rect(xHightlight + size_hightlight, yGui + 6, size_sub_t, 28), msg.Shuffle, " Shuffle", (bool b) => {
                                    msg.Shuffle = b;
                                });
                            }
                            else if (msg.Shuffle)
                            {
                                msg.Shuffle = false;
                            }
                            yGui += 32f;
                            for (int jj = 0; jj < msg.MsgboxChoise.Count; jj++)
                            {
                                int j = jj;
                                var choise = msg.MsgboxChoise[j];
                                float xGui1 = xSubGui0;
                                bool isBeginGroup = (j % msg.Group == 0);
                                bool isEndGroup = (j % msg.Group == (msg.Group - 1));

                                if (j == 0 || (isBeginGroup && msg.Group > 1))
                                {
                                    yGui += 16f;
                                    if (msg.Group > 1)
                                    {
                                        AONGUI.Box(new Rect(xGui1, yGui, rect.width - 8, msg.Group * 32 + 8), "", tilesetAON.ListStyleBlack2);
                                    }
                                }
                                bool showRate = ((msg.Group > 1) || msg.Random == true);
                                if (showRate)
                                {
                                    if (j == 0)
                                    {
                                        // GUI.Label( new Rect(xGui1 + 4, yGui - 25, 120, 24), msg.Group > 1 ? "Rate group:" : "Rate:");
                                        AONGUI.Label(new Rect(xGui1 + 4, yGui - 25, 120, 24), "Rate:");
                                    }
                                    if (isBeginGroup)
                                    {
                                        var rate = choise.Rate;
                                        AONGUI.TextField(new Rect(xGui1 + 4, yGui + DefineAON.GUI_Y_TextField, size_rate - 8, DefineAON.GUI_Height_TextField), rate.ToString(), (string text) => {
                                            var next = UtilsAON.StrToIntDef(text);
                                            if (next < 1)
                                            {
                                                next = 1;
                                            }
                                            if (next != rate)
                                            {
                                                choise.Rate = next;
                                            }
                                        });
                                    }
                                    xGui1 += size_rate;
                                }
                                var w_value = size_sub_c;
                                // w_value += 60;
                                if (!showRate)
                                {
                                    w_value += size_rate;
                                }
                                if (msg.Type == MsgboxChat.EType.Chat)
                                {
                                    w_value += size_sub_t;
                                }
                                if (!msg.Highlight)
                                {
                                    w_value += size_hightlight;
                                }
                                {

                                    string content = choise.Value;
                                    string hash = i.ToString() + "_" + j.ToString();
                                    AONGUI.SetNextControlName(hash);
                                    AONGUI.TextField(new Rect(xGui1 + 4, yGui + DefineAON.GUI_Y_TextField, w_value - 8, DefineAON.GUI_Height_TextField), content, (string text) => {
                                        choise.Value = text;
                                    });
                                    xGui1 += w_value;
                                }
                                if (msg.Highlight)
                                {
                                    float xH = xHightlight;
                                    float sizeCombobox = 100;
                                    // if (j == 0)
                                    // {
                                    //     GUI.Label(new Rect(xH + 4, yGui - 25, size_hightlight, 24), "Format:");
                                    // }
                                    {
                                        int current = choise.TypeHighlight;
                                        var comboBox = ComboBoxHelper.Instance.Choise_Hightlight();
                                        comboBox.SelectedItemIndex = current;
                                        comboBox.Rect.x = xH;
                                        comboBox.Rect.y = yGui;
                                        comboBox.Rect.width = sizeCombobox;
                                        comboBox.Rect.height = 32f;
                                        float limitHeight = comboBox.HeightForShowAll();
                                        var hash = i.ToString() + "_" + j.ToString();
                                        comboBox.Show(limitHeight, hash, false, (int next) => {
                                            choise.TypeHighlight = next;
                                        });
                                        if (comboBox.IsDropDownWithHash(hash))
                                        {
                                            yGui += limitHeight;
                                            isWaitUI = true;
                                            return false;
                                        }
                                        xH += sizeCombobox;
                                    }
                                    MsgboxChoise.ETypeHighlight t = (MsgboxChoise.ETypeHighlight)choise.TypeHighlight;
                                    if (t == MsgboxChoise.ETypeHighlight.Words_Red || t == MsgboxChoise.ETypeHighlight.Full_Green)
                                    {
                                        AONGUI.TextField(new Rect(xH + 4, yGui + DefineAON.GUI_Y_TextField, size_hightlight - sizeCombobox - 8, DefineAON.GUI_Height_TextField), choise.ContentHighlight, (string text) => {
                                            choise.ContentHighlight = text;
                                        });
                                    }
                                    // else if (t == MsgboxChoise.ETypeHighlight.Full_Green)
                                    // {
                                    //     GUI.Label(new Rect(xH + 4, yGui + DefineAON.GUI_Y_TextField, size_hightlight - sizeCombobox - 8, DefineAON.GUI_Height_TextField), "Full text highlight in Green");
                                    // }
                                    // else
                                    // {
                                    //     GUI.Label(new Rect(xH + 4, yGui + DefineAON.GUI_Y_TextField, size_hightlight - sizeCombobox - 8, DefineAON.GUI_Height_TextField), "Not effect");
                                    // }
                                    xGui1 += size_hightlight;
                                }
                                if (msg.Type != MsgboxChat.EType.Chat)
                                {
                                    //Choise Next
                                    float s_pick = size_sub_t / 2;
                                    {
                                        if (j == 0)
                                        {
                                            AONGUI.Label(new Rect(xGui1 + 4, yGui - 25, s_pick, 24), "Action:");
                                        }
                                        var combobox = ComboBoxHelper.Instance.FlagAction(scriptYaml.FlagActions);
                                        var hash = i.ToString() + "_" + j.ToString();
                                        int currentFlagActionOnPick = (choise.Action == null || scriptYaml.FlagActions == null ? -1 : scriptYaml.FlagActions.IndexOf(choise.Action));
                                        if (currentFlagActionOnPick == -1)
                                        {
                                            if (choise.Action != null && choise.Action.Name != null && choise.Action.Name != "")
                                            {
                                                combobox.Empty = choise.Action.Name + " (not found)";
                                            }
                                            else
                                            {
                                                combobox.Empty = "NULL";
                                            }
                                        }
                                        combobox.SelectedItemIndex = currentFlagActionOnPick;
                                        combobox.Rect.x = xGui1;
                                        combobox.Rect.y = yGui;
                                        combobox.Rect.width = s_pick - 4;
                                        combobox.Rect.height = 32f;
                                        float limitHeight = 32f * 6;
                                        combobox.Show(limitHeight, hash, true, false, rect.x + xGui + 4, rect.x + rect.width, (int nextAction) => {
                                            choise.Action = scriptYaml.FlagActions[nextAction];
                                        });
                                        if (combobox.IsDropDownWithHash(hash))
                                        {
                                            yGui += limitHeight;
                                            return false;
                                        }
                                    }
                                    xGui1 += s_pick;
                                    {
                                        if (j == 0)
                                        {
                                            AONGUI.Label(new Rect(xGui1 + 4, yGui - 25, s_pick, 24), "Next:");
                                        }
                                        var hash = i.ToString() + "_" + j.ToString();
                                        var t = choise.Next;
                                        OnGUiPickScope(hash, t, xGui1, yGui, s_pick, ref yGui, ref isWaitUI, rect.x + xGui + 4, rect.x + rect.width, false, (ActionData pickScope) => {
                                            choise.Next = pickScope;
                                        });
                                    }
                                    xGui1 += s_pick;
                                }
                                if (isEndGroup)
                                {
                                    AONGUI.Button(new Rect(xGui1, yGui + DefineAON.GUI_Y_Button, size_sub_e - 4, DefineAON.GUI_Height_Button), " - ", () =>{
                                        for (int k = 0; k < msg.Group; k++)
                                            msg.MsgboxChoise.RemoveAt(j - msg.Group + 1);
                                        if (msg.MsgboxChoise.Count == 0)
                                        {
                                            msg.MsgboxChoise = null;
                                        }
                                    });
                                }
                                xGui1 += size_sub_e;
                                if (isEndGroup)
                                {
                                    AONGUI.Button(new Rect(xGui1, yGui + DefineAON.GUI_Y_Button, size_sub_e_2 - 4, DefineAON.GUI_Height_Button), " + (Enter)", KeyCode.Return, () => {
                                        for (int k = 0; k < msg.Group; k++)
                                        {
                                            msg.MsgboxChoise.Insert(j + 1, new MsgboxChoise().Init());
                                        }
                                        mFocusControlNextFrame = i.ToString() + "_" + (j + 1).ToString();
                                    });
                                }
                                /*
								string hash = "msg_" + i.ToString() + "_" + j.ToString();
								var t = choise.Next;
								int to_scope = (t == null ? -1: scriptYaml.Main.IndexOf(t));
								var comboBoxScopeList = ComboBoxHelper.Instance.ScopeList(scriptYaml.Main);
								comboBoxScopeList.SelectedItemIndex = to_scope;
								comboBoxScopeList.Rect.x = xGui1;
								comboBoxScopeList.Rect.y = yGui;
								comboBoxScopeList.Rect.width = size_sub_t;
								comboBoxScopeList.Rect.height = 32f;
								int nextScope = comboBoxScopeList.Show( rect.height - yGui - 32, hash);
								if( comboBoxScopeList.IsDropDownWithHash(hash)){
									isWaitUI = true;
									return false;
								}
								if(nextScope != to_scope){
									choise.Next = scriptYaml.Main[nextScope];
									return true;
								}
								*/
                                yGui += 32f;
                            }
                        }
                        yGui += 16f;
                        AONGUI.Button(new Rect(rect.x + 10, yGui + DefineAON.GUI_Y_Button, 100, DefineAON.GUI_Height_Button), "Add above", () => {
                            actionData.MsgboxChat.Insert(i, new MsgboxChat().Init());
                        });
                        
                        AONGUI.Button(new Rect(rect.x + 120, yGui + DefineAON.GUI_Y_Button, 100, DefineAON.GUI_Height_Button), "Add bellow", () => {
                            actionData.MsgboxChat.Insert(i + 1, new MsgboxChat().Init());
                        });
                        
                        AONGUI.Button(new Rect(rect.x + 230, yGui + DefineAON.GUI_Y_Button, 150, DefineAON.GUI_Height_Button), "Remove this paragraph", () => {
                            actionData.MsgboxChat.RemoveAt(i);
                        });
                        
                        yGui += 32f;
                    }
                    {
                        //Line
                        yGui += 8;
                        AONGUI.Box(new Rect(rect.x, yGui + 8, rect.width, 8), "", tilesetAON.ListStyleBlack);
                        yGui += 8;
                    }
                    #endregion
                }
                else if (actionData.Format == EFormatScope.NPC)
                {
                    #region EFormatScope.NPC
                    yGui += 16;
                    yGui += 16;
                    AONGUI.Toggle(new Rect(rect.x, yGui, rect.width, 32), actionData.NPCAction.UsingNPCTarget, "Using NPC on target", (bool b) => {
                        actionData.NPCAction.UsingNPCTarget = b;
                    });
                    yGui += 32;
                    if (actionData.NPCAction.UsingNPCTarget == false)
                    {
                        AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, left - 4, DefineAON.GUI_Height_Label), "NPC");
                        // string hash = "flag";
                        // string if_a = (actionData.Dic.ContainsKey(hash) ? actionData.Dic[hash] : "");
                        if (autoTileMap.MapSelect.NPCData.Count == 0)
                        {
                            AONGUI.Button(new Rect(rect.x + left, yGui + DefineAON.GUI_Y_Button, rect.width - left, DefineAON.GUI_Height_Button), "Add NPC", () => {
                               actionData.NPCAction.IdNpc = autoTileMap.MapSelect.CreateNewNPC(); 
                            });
                        }
                        else
                        {
                            int idNpc = actionData.NPCAction.IdNpc;
                            var comboBox = ComboBoxHelper.Instance.NPCList(autoTileMap.MapSelect);
                            comboBox.SelectedItemIndex = idNpc;
                            comboBox.Rect.x = rect.x + left;
                            comboBox.Rect.y = yGui;
                            comboBox.Rect.width = rect.width - left;
                            comboBox.Rect.height = 32f;
                            if (comboBox.IsDropDownWithHash(indexScope.ToString()))
                            {
                                AONGUI.Button(new Rect(rect.x + left, rect.y + rect.height - 32 + DefineAON.GUI_Y_Button, rect.width - left, DefineAON.GUI_Height_Button), "Add NPC", () => {
                                    actionData.NPCAction.IdNpc = autoTileMap.MapSelect.CreateNewNPC();
                                });
                            }
                            comboBox.Show(rect.height - yGui - 64, indexScope.ToString(), (int idNpcNext) => {
                                actionData.NPCAction.IdNpc = idNpcNext;
                                hasUpdateData = true;
                            });
                            if (comboBox.IsDropDownWithHash(indexScope.ToString()))
                            {
                                isWaitUI = true;
                                return false;
                            }
                        }
                        yGui += 32;
                        if (actionData.NPCAction.IdNpc >= 0 && actionData.NPCAction.IdNpc < autoTileMap.MapSelect.NPCData.Count)
                        {
                            float size_e = 100;
                            AONGUI.Button(new Rect(rect.x + rect.width - size_e + 4, yGui + DefineAON.GUI_Y_Button, size_e - 4, DefineAON.GUI_Height_Button), "Edit NPC", ()=>{
                                tilesetAON.TriggerShowMoreInfo = autoTileMap.MapSelect.NPCData[actionData.NPCAction.IdNpc];
                            });
                        }
                        yGui += 32;
                    }
                    else
                    {
                        yGui += 64;
                    }
                    {
                        AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, left - 4, DefineAON.GUI_Height_Label), "Action");
                        var last = (int)actionData.NPCAction.Action;
                        var comboBox = ComboBoxHelper.Instance.NPCAction();
                        comboBox.SelectedItemIndex = last;
                        comboBox.Rect.x = rect.x + left;
                        comboBox.Rect.y = yGui;
                        comboBox.Rect.width = rect.width - left;
                        comboBox.Rect.height = 32f;
                        comboBox.Show(rect.height - yGui - 32, indexScope.ToString(), (int next) => {
                            actionData.NPCAction.Action = (NPCAction.EAction)next;
                            hasUpdateData = true;
                        });
                        if (comboBox.IsDropDownWithHash(indexScope.ToString()))
                        {
                            isWaitUI = true;
                            return false;
                        }
                        yGui += 32;
                    }
                    if (actionData.NPCAction.Action == NPCAction.EAction.Show || actionData.NPCAction.Action == NPCAction.EAction.Move)
                    {
                        NPCAction npcAction = actionData.NPCAction;
                        AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, left - 4, DefineAON.GUI_Height_Label), "Position");
                        AONGUI.Label(new Rect(rect.x + left, yGui + DefineAON.GUI_Y_Label, 100, DefineAON.GUI_Height_Label), string.Format("( {0} , {1})", npcAction.x, npcAction.y));
                        AONGUI.Button(new Rect(rect.x + left + 100, yGui + DefineAON.GUI_Y_Button, 100, DefineAON.GUI_Height_Button), "Pick", () => {
                            tilesetAON.PickPosOnMap(autoTileMap.MapIdxSelect, actionData.NPCAction.x, actionData.NPCAction.y, (PickMapAON p, int _x, int _y) =>
                            {
                                npcAction.x = _x;
                                npcAction.y = _y;
                                // mIsHasUpdateScript = true;
                            });
                        });
                    }
                    yGui += 32f;
                    {
                        // Next
                        yGui += 32f;
                        AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, 100, DefineAON.GUI_Height_Label), "Next block");
                        string hash = "next";
                        // int to_scope = System.Convert.ToInt32(actionData.Dic.ContainsKey(hash) ? actionData.Dic[hash] : "0");
                        // var t = actionData.GetToAt(hash);
                        OnGUiPickScope(hash, actionData.NPCAction.Next, rect.x + left, yGui, rect.width - left, ref yGui, ref isWaitUI, (ActionData pickScope) => {
                            actionData.NPCAction.Next = pickScope;
                        });
                        yGui += 32f;
                    }
                    #endregion
                }
                else if (actionData.Format == EFormatScope.MainChar)
                {
                    #region EFormatScope.MainChar
                    {
                        AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, left - 4, DefineAON.GUI_Height_Label), "Action");
                        var last = (int)actionData.MainCharAction.Action;
                        var comboBox = ComboBoxHelper.Instance.MainCharAction();
                        comboBox.SelectedItemIndex = last;
                        comboBox.Rect.x = rect.x + left;
                        comboBox.Rect.y = yGui;
                        comboBox.Rect.width = rect.width - left;
                        comboBox.Rect.height = 32f;
                        comboBox.Show(rect.height - yGui - 32, indexScope.ToString(), (int next) => {
                            actionData.MainCharAction.Action = (MainCharAction.EAction)next;
                            hasUpdateData = true;
                        });
                        if (comboBox.IsDropDownWithHash(indexScope.ToString()))
                        {
                            isWaitUI = true;
                            return false;
                        }
                        yGui += 32;
                    }
                    if (actionData.MainCharAction.Action == MainCharAction.EAction.WaitMoveToPos
                    || actionData.MainCharAction.Action == MainCharAction.EAction.WaitMoveToHouse
                    || actionData.MainCharAction.Action == MainCharAction.EAction.WaitInteractionsNPC)
                    {
                        AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, left - 4, DefineAON.GUI_Height_Label), "Tip:");
                        AONGUI.TextField(new Rect(rect.x + left, yGui + DefineAON.GUI_Y_TextField, rect.width - left, DefineAON.GUI_Height_TextField), actionData.MainCharAction.Tip, (string text) => {
                            actionData.MainCharAction.Tip = text;
                        });
                        yGui += 32f;
                    }
                    if (actionData.MainCharAction.Action == MainCharAction.EAction.WaitMoveToPos)
                    {
                        MainCharAction action = actionData.MainCharAction;
                        AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, left - 4, DefineAON.GUI_Height_Label), "Position");
                        AONGUI.Label(new Rect(rect.x + left, yGui + DefineAON.GUI_Y_Label, 100, DefineAON.GUI_Height_Label), string.Format("( {0} , {1})", action.x, action.y));
                        AONGUI.Button(new Rect(rect.x + left + 100, yGui + DefineAON.GUI_Y_Button, 100, DefineAON.GUI_Height_Button), "Pick", () => {
                            tilesetAON.PickPosOnMap(autoTileMap.MapIdxSelect, actionData.MainCharAction.x, actionData.MainCharAction.y, (PickMapAON p, int _x, int _y) =>
                            {
                                action.x = _x;
                                action.y = _y;
                                // mIsHasUpdateScript = true;
                            });
                        });
                        yGui += 32f;
                    }
                    else if (actionData.MainCharAction.Action == MainCharAction.EAction.WaitMoveToHouse)
                    {
                        if (autoTileMap.MapSelect.HouseData.Count == 0)
                        {
                            AONGUI.Label(new Rect(rect.x + left, yGui + DefineAON.GUI_Y_Label, rect.width - left, DefineAON.GUI_Height_Label), "The house empty should be created a house in the map.");
                            yGui += 32f;
                        }
                        else
                        {
                            int idHouse = actionData.MainCharAction.IdHouse;
                            var comboBox = ComboBoxHelper.Instance.HouseListMap(autoTileMap.MapSelect);
                            comboBox.SelectedItemIndex = idHouse;
                            comboBox.Rect.x = rect.x + left;
                            comboBox.Rect.y = yGui;
                            comboBox.Rect.width = rect.width - left;
                            comboBox.Rect.height = 32f;
                            comboBox.Show(rect.height - yGui - 64, indexScope.ToString(), (int idHouseNext) => {
                                actionData.MainCharAction.IdHouse = idHouseNext;
                                hasUpdateData = true;
                            });
                            if (comboBox.IsDropDownWithHash(indexScope.ToString()))
                            {
                                isWaitUI = true;
                                return false;
                            }
                        }
                    }
                    else if (actionData.MainCharAction.Action == MainCharAction.EAction.WaitInteractionsNPC)
                    {
                        if (autoTileMap.MapSelect.NPCData.Count == 0)
                        {
                            AONGUI.Button(new Rect(rect.x + left, yGui + DefineAON.GUI_Y_Label, rect.width - left, DefineAON.GUI_Height_Label), "Add NPC", () => {
                                actionData.MainCharAction.IdNpc = autoTileMap.MapSelect.CreateNewNPC();
                            });
                        }
                        else
                        {
                            int idNpc = actionData.MainCharAction.IdNpc;
                            var comboBox = ComboBoxHelper.Instance.NPCList(autoTileMap.MapSelect);
                            comboBox.SelectedItemIndex = idNpc;
                            comboBox.Rect.x = rect.x + left;
                            comboBox.Rect.y = yGui;
                            comboBox.Rect.width = rect.width - left;
                            comboBox.Rect.height = 32f;
                            if (comboBox.IsDropDownWithHash(indexScope.ToString()))
                            {
                                AONGUI.Button(new Rect(rect.x + left, rect.y + rect.height - 32 + DefineAON.GUI_Y_Button, rect.width - left, DefineAON.GUI_Height_Button), "Add NPC", () => {
                                    actionData.MainCharAction.IdNpc = autoTileMap.MapSelect.CreateNewNPC();
                                });
                            }
                            comboBox.Show(rect.height - yGui - 64, indexScope.ToString(), (int idNpcNext) => {
                                actionData.MainCharAction.IdNpc = idNpcNext;
                                hasUpdateData = true;
                            });
                            if (comboBox.IsDropDownWithHash(indexScope.ToString()))
                            {
                                isWaitUI = true;
                                return false;
                            }
                        }
                        yGui += 32;
                    }
                    else if (actionData.MainCharAction.Action == MainCharAction.EAction.RewardItem)
                    {
                        var propertys = autoTileMap.MapsData.Propertys;
                        if (propertys.Count == 0)
                        {
                            if (actionData.MainCharAction.SlugItem != null && actionData.MainCharAction.SlugItem.Length > 0)
                            {
                                AONGUI.Label(new Rect(rect.x + left, yGui + DefineAON.GUI_Y_Label, rect.width - left, DefineAON.GUI_Height_Label), actionData.MainCharAction.SlugItem + " not found and propertys list is empty.");
                            }
                            else
                            {
                                AONGUI.Label(new Rect(rect.x + left, yGui + DefineAON.GUI_Y_Label, rect.width - left, DefineAON.GUI_Height_Label), "Property list is empty.");
                            }
                        }
                        else
                        {
                            string currentSlug = actionData.MainCharAction.SlugItem;
                            PropertysGUI.Instance.PickSlugItem(indexScope.ToString(), propertys, currentSlug, rect.x + left, yGui, rect.width - left, ref yGui, ref isWaitUI, (string slugPick) => {
                                actionData.MainCharAction.SlugItem = slugPick;
                                hasUpdateData = true;
                            });
                        }
                        yGui += 32;
                    }
                    else if (actionData.MainCharAction.Action == MainCharAction.EAction.CheckItem)
                    {
                        var propertys = autoTileMap.MapsData.Propertys;
                        if (propertys.Count == 0)
                        {
                            if (actionData.MainCharAction.SlugItem != null && actionData.MainCharAction.SlugItem.Length > 0)
                            {
                                AONGUI.Label(new Rect(rect.x + left, yGui + DefineAON.GUI_Y_Label, rect.width - left, DefineAON.GUI_Height_Label), actionData.MainCharAction.SlugItem + " not found and propertys list is empty.");
                            }
                            else
                            {
                                AONGUI.Label(new Rect(rect.x + left, yGui + DefineAON.GUI_Y_Label, rect.width - left, DefineAON.GUI_Height_Label), "Property list is empty.");
                            }
                        }
                        else
                        {
                            string currentSlug = actionData.MainCharAction.SlugItem;
                            PropertysGUI.Instance.PickSlugItem(indexScope.ToString(), propertys, currentSlug, rect.x + left, yGui, rect.width - left, ref yGui, ref isWaitUI, (string slugPick) => {
                                actionData.MainCharAction.SlugItem = slugPick;
                                hasUpdateData = true;
                            });
                        }
                        yGui += 32;
                        {
                            // Have
                            yGui += 32f;
                            AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, 120, DefineAON.GUI_Height_Label), "Have");
                            string hash = "have";
                            OnGUiPickScope(hash, actionData.MainCharAction.Next, rect.x + left, yGui, rect.width - left, ref yGui, ref isWaitUI, (ActionData pickScope) => {
                                actionData.MainCharAction.Next = pickScope;
                            });
                        }
                        {
                            // Dont Have
                            yGui += 32f;
                            AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, 120, DefineAON.GUI_Height_Label), "Dont have");
                            string hash = "dont have";
                            OnGUiPickScope(hash, actionData.MainCharAction.Wrong, rect.x + left, yGui, rect.width - left, ref yGui, ref isWaitUI, (ActionData pickScope) => {
                                actionData.MainCharAction.Wrong = pickScope;
                            });
                        }
                    }
                    else if (actionData.MainCharAction.Action == MainCharAction.EAction.BuyItem)
                    {
                        var packages = autoTileMap.MapsData.Packages;
                        if (packages.Count == 0)
                        {
                            if (!string.IsNullOrEmpty(actionData.MainCharAction.SlugPackage))
                            {
                                AONGUI.Label(new Rect(rect.x + left, yGui + DefineAON.GUI_Y_Label, rect.width - left, DefineAON.GUI_Height_Label), actionData.MainCharAction.SlugPackage + " not found and packages list is empty.");
                            }
                            else
                            {
                                AONGUI.Label(new Rect(rect.x + left, yGui + DefineAON.GUI_Y_Label, rect.width - left, DefineAON.GUI_Height_Label), "Packages list is empty.");
                            }
                        }
                        else
                        {
                            string currentSlug = actionData.MainCharAction.SlugPackage;
                            PackagesGUI.Instance.PickSlugItem(indexScope.ToString(), packages, currentSlug, rect.x + left, yGui, rect.width - left, ref yGui, ref isWaitUI, (string slugPick) => {
                                actionData.MainCharAction.SlugPackage = slugPick;
                                hasUpdateData = true;
                            });
                        }
                        yGui += 32;
                        {
                            // Next
                            AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, 100, DefineAON.GUI_Height_Label), "Buy");
                            string hash = "buy";
                            OnGUiPickScope(hash, actionData.MainCharAction.Next, rect.x + left, yGui, rect.width - left, ref yGui, ref isWaitUI, (ActionData pickScope) => {
                                actionData.MainCharAction.Next = pickScope;
                            });
                            yGui += 32f;
                        }
                        {
                            // Next
                            AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, 100, DefineAON.GUI_Height_Label), "Don't buy");
                            string hash = "notbuy";
                            OnGUiPickScope(hash, actionData.MainCharAction.Wrong, rect.x + left, yGui, rect.width - left, ref yGui, ref isWaitUI, (ActionData pickScope) => {
                                actionData.MainCharAction.Wrong = pickScope;
                            });
                            yGui += 32f;
                        }
                    }else if (actionData.MainCharAction.Action == MainCharAction.EAction.WarpTo)
                    {
                        if (autoTileMap.MapSelect.WarpsData.Count == 0)
                        {
                            AONGUI.Label(new Rect(rect.x + left, yGui + DefineAON.GUI_Y_Label, rect.width - left, DefineAON.GUI_Height_Label), "Warps is empty, should be created a warp on the map.");
                            yGui += 32f;
                        }
                        else
                        {
                            int idWarp = actionData.MainCharAction.IdWarp;
                            var comboBox = ComboBoxHelper.Instance.WarpListMap(autoTileMap.MapSelect);
                            comboBox.SelectedItemIndex = idWarp;
                            comboBox.Rect.x = rect.x + left;
                            comboBox.Rect.y = yGui;
                            comboBox.Rect.width = rect.width - left;
                            comboBox.Rect.height = 32f;
                            comboBox.Show(rect.height - yGui - 64, indexScope.ToString(),(int idWarpNext) => {
                                actionData.MainCharAction.IdWarp = idWarpNext;
                                hasUpdateData = true;
                            });
                            if (comboBox.IsDropDownWithHash(indexScope.ToString()))
                            {
                                isWaitUI = true;
                                return false;
                            }
                        }
                    }
                    if(actionData.MainCharAction.Action != MainCharAction.EAction.CheckItem
                        && actionData.MainCharAction.Action != MainCharAction.EAction.BuyItem
                        && actionData.MainCharAction.Action != MainCharAction.EAction.WarpTo){
                        // Next
                        yGui += 32f;
                        AONGUI.Label(new Rect(rect.x + xGui, yGui + DefineAON.GUI_Y_Label, 100, DefineAON.GUI_Height_Label), "Next");
                        string hash = "next";
                        OnGUiPickScope(hash, actionData.MainCharAction.Next, rect.x + left, yGui, rect.width - left, ref yGui, ref isWaitUI, (ActionData pickScope) => {
                           actionData.MainCharAction.Next = pickScope; 
                        });
                    }
                    #endregion
                }
                // else if(actionData.Format == EFormatScope.Switch){
                // }
            }
            return hasUpdateData;
        }

        private delegate void OnPickActionData( ActionData a);
        private void OnGUiPickScope(string hash, ActionData current, float x, float y, float w, ref float yGui, ref bool isWaitUI, OnPickActionData OnPick = null){
            OnGUiPickScope( hash, current, x, y, w, ref yGui, ref isWaitUI, -1, -1, true, OnPick);
        }
        
        private void OnGUiPickScope(string hash, ActionData current, float x, float y, float w, ref float yGui, ref bool isWaitUI, float xBoxBegin, float xBoxEnd, bool showEdit, OnPickActionData OnPick)
        {
            var scriptYaml = ScriptTarget.ScriptYaml;
            float size_e = 45;
            var t = current;
            int currentScope = (t == null ? -1 : scriptYaml.ListActions().IndexOf(t));
            var comboBoxScopeList = ComboBoxHelper.Instance.ScopeList(scriptYaml.ListActions());
            comboBoxScopeList.SelectedItemIndex = currentScope;
            comboBoxScopeList.Rect.x = x;
            comboBoxScopeList.Rect.y = y;
            comboBoxScopeList.Rect.width = showEdit ? w - size_e - 10 : w - 4;
            comboBoxScopeList.Rect.height = 32f;
            float limitHeight = 32f * 10;
            comboBoxScopeList.Show(limitHeight, hash, true, false, xBoxBegin, xBoxEnd, (int nextScope) => {
                OnPick(scriptYaml.ActionDataAt(nextScope));
            });
            if (comboBoxScopeList.IsDropDownWithHash(hash))
            {
                yGui += limitHeight;
                isWaitUI = true;
                return;
            }
            if (showEdit && currentScope > 0 && currentScope != ScopeSelect)
            {
                AONGUI.Button(new Rect(x + w - size_e, yGui + DefineAON.GUI_Y_Button, size_e, 26), "Edit", () => {
                    ScopeSelect = currentScope;
                });
            }
            return;
        }

        private Flags FlagByTarget(ScriptYaml scriptYaml, AutoTileMap autoTileMap, int target)
        {
            if (target == (int)FlagTarget.World)
            {
                return autoTileMap.MapsData.FlagWorld;
            }
            if (target == (int)FlagTarget.Map)
            {
                return autoTileMap.MapSelect.FlagMap;
            }
            // FlagTarget.Script
            return scriptYaml.FlagsYaml;
        }

        private List<FlagAction> ListFlagActionByTarget(ScriptYaml scriptYaml, AutoTileMap autoTileMap, int target)
        {
            if (target == (int)FlagTarget.World)
            {
                return autoTileMap.MapsData.ListFlagAction;
            }
            if (target == (int)FlagTarget.Map)
            {
                return autoTileMap.MapSelect.ListFlagAction;
            }
            return scriptYaml.FlagActions;
        }

        // private delegate void OnClickButton();
        // private OnClickButton mOnClickButton = null;
        // private void SetNextActionClick(OnClickButton action){
        //     mOnClickButton = action;
        // }
    }
}