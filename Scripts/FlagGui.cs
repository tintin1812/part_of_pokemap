using System;
using System.Collections;
using System.Collections.Generic;
using AON.RpgMapEditor;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class FlagGui
{

    private static FlagGui _instance = null;

    public static FlagGui Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new FlagGui();
            }
            return _instance;
        }
    }

    private FlagGui() { }

    private static float heighTextField = 25f;

    private Flags flagsCurrent = null;
    private ComboBox _comboBoxFlags = null;

    public delegate void OnChargeKeyDelegate(string last_key, string next_key);

    public void ResetCombobox()
    {
        _comboBoxFlags = null;
    }

    public ComboBox UpdateFlagsData(Flags flags, string currentKey)
    {
        // DeserializerBuilder(data);
        if (flagsCurrent != flags)
        {
            flagsCurrent = flags;
            _comboBoxFlags = null;
        }
        if (_comboBoxFlags == null)
        {
            GUIContent[] comboBoxList = null;
            if (flagsCurrent != null)
            {
                var keys = flagsCurrent.Keys;
                comboBoxList = new GUIContent[keys.Count];
                int i = 0;
                foreach (string key in keys)
                {
                    comboBoxList[i] = new GUIContent(key);
                    i++;
                }
            }
            else
            {
                comboBoxList = new GUIContent[0];
            }
            _comboBoxFlags = new ComboBox(new Rect(0, 0, 150, 20), comboBoxList);
        }
        if (currentKey == null || currentKey == "")
        {
            _comboBoxFlags.Empty = "NULL";
            _comboBoxFlags.SelectedItemIndex = -1;
        }
        else if (flagsCurrent != null)
        {
            var keys = flagsCurrent.Keys;
            int index_key = IndexOfKey(currentKey);
            if (index_key != -1)
            {
                _comboBoxFlags.SelectedItemIndex = index_key;
            }
            else
            {
                _comboBoxFlags.Empty = currentKey + " (not found)";
                _comboBoxFlags.SelectedItemIndex = index_key;
            }
        }
        return _comboBoxFlags;
    }

    public int IndexOfKey(string k)
    {
        if (flagsCurrent == null)
            return -1;
        var keys = flagsCurrent.Keys;
        int i = 0;
        foreach (string key in keys)
        {
            if (k == key)
            {
                return i;
            }
            i++;
        }
        return -1;
    }

    public string KeyFromIndex(int indext)
    {
        if (flagsCurrent == null)
        {
            return "";
        }
        var keys = flagsCurrent.Keys;
        int i = 0;
        foreach (string key in keys)
        {
            if (i == indext)
            {
                return key;
            }
            i++;
        }
        return "";
    }

    /*
	public void SerializerBuilder(){
		if(currentData != null && IsHasDataUpdate){
			try{
				var serializer = new SerializerBuilder().Build();
				var yaml = serializer.Serialize(flagsYaml);
				if(yaml != null){
					currentData.Flags = yaml;
				}
			}catch{}
			IsHasDataUpdate = false;
			_comboBoxFlags = null;
			Debug.Log("TriggerGui.SerializerBuilderScript()");
		}
	}
	*/

    /*
	public bool FlagOnGUI(AutoTileMapSerializeData data, ref float yGui, Rect rect, AutoTileMap autoTileMap, OnChargeKeyDelegate OnChargeKey){
		DeserializerBuilder(data);
		yGui += 7;
		GUI.Label(new Rect(rect.x + 4, yGui, rect.width, 25), "Edit Flags");
		if( GUI.Button( new Rect(rect.x + 140, yGui, rect.width - 140, 25), "Code Flags") ){
			SerializerBuilder();
			InputFieldHelper.Instance.Show(data.Flags, (string content) => {
				if(data.Flags != content){
					data.Flags = content;
					currentData = null;
					flagsYaml = null;
				}
			});
			return false;
		}
		yGui += 32f;
		
		bool isUpdate = DisOnGUI(ref flagsYaml, ref yGui, rect, autoTileMap, OnChargeKey);
		if(isUpdate){
			IsHasDataUpdate = true;
			// var serializer = new SerializerBuilder().Build();
			// var yaml = serializer.Serialize(flagsYaml);
			// if(yaml != null){
			// 	script.flags = yaml;
			// }
		}
		return false;
	}
	*/

    private static string NameAdd = "";
    public static void DisOnGUI(Flags flagsYaml, ref float yGui, Rect rect, string[] lockKey = null, GUIStyle backgroundTop = null, string title = "Flag edit :", List<string> resetList = null)
    {
        if (backgroundTop != null)
        {
            float height_top = 68;
            AONGUI.Box(new Rect(rect.x, rect.y, rect.width, height_top), "", backgroundTop);
        }

        AONGUI.Label(new Rect(rect.x + 4, yGui + DefineAON.GUI_Y_Label, rect.width, DefineAON.GUI_Height_Label), title);
        yGui += 32f;

        if (flagsYaml == null)
        {
            return;
        }
        //Check key lock
        if (flagsYaml.CheckLockKey(lockKey))
        {
            AONGUIBehaviour.AONGUI_ReDrawAll();
            return;
        }
        var keys = flagsYaml.Keys;
        {
            //Add Key
            float xGui = rect.x + 4;
            AONGUI.Label(new Rect(xGui, yGui + DefineAON.GUI_Y_Label, 90, DefineAON.GUI_Height_Label), "Slug flag");
            xGui += 94;
            AONGUI.TextField(new Rect(xGui, yGui + DefineAON.GUI_Y_TextField, 200, DefineAON.GUI_Height_TextField), NameAdd, (string text) => {
                NameAdd = text;
            });
            xGui += 204;
            if (NameAdd.Length == 0)
            {
            }
            else
            {
                bool isUnique = true;
                foreach (string key in keys)
                {
                    if (key == NameAdd)
                    {
                        isUnique = false;
                        break;
                    }
                }
                if (isUnique)
                {
                    AONGUI.Button(new Rect(xGui, yGui + DefineAON.GUI_Y_Button, 40, DefineAON.GUI_Height_Button), "Add", () => {
                        flagsYaml.Add(NameAdd, 0);
                        NameAdd = "";
                        //Check clear cache comboBoxFlags
                        if (_instance != null && _instance.flagsCurrent == flagsYaml)
                        {
                            _instance.ResetCombobox();
                        }
                    });
                }
                else
                {
                    AONGUI.Label(new Rect(xGui, yGui + DefineAON.GUI_Y_Label, 200, DefineAON.GUI_Height_Label), "Slug should be unique");
                }
            }
            yGui += 32f;
        }
        yGui += 16f;
        int count = 0;
        foreach (string key in keys)
        {
            bool lockKeySub = false;
            if (lockKey != null && count < lockKey.Length)
            {
                lockKeySub = true;
            }
            if (key != "")
            {
                KeyValueOnGUI(flagsYaml, key, ref yGui, rect, lockKeySub, resetList, count);
            }
            count++;
        }
    }

    public static void KeyValueOnGUI(Flags flagsYaml, string key, ref float yGui, Rect rect, bool lockKey, List<string> resetList, int index)
    {
        int value = flagsYaml.ContainsKey(key) ? flagsYaml[key] : 0;
        float _w2 = 50;
        float _w3 = 60;
        float _wReset = resetList == null ? 0 : 100;
        float _w = (rect.width - _w2 - _w3 - _wReset - 8) / 2;
        float xGui = rect.x + 4;
        AONGUI.Label(new Rect(xGui, yGui + 32 - heighTextField, _w, heighTextField), key);
        xGui += _w;
        AONGUI.Label(new Rect(xGui + _w2 / 2, yGui, _w2, 32), "=");
        xGui += _w2;
        AONGUI.TextField(new Rect(xGui, yGui + 32 - heighTextField, _w, heighTextField), value.ToString(), 25, (string text) => {
            flagsYaml[key] = UtilsAON.StrToIntDef(text);
        });
        xGui += _w;
        if (lockKey == false)
        {
            AONGUI.Button(new Rect(xGui, yGui + 32 - heighTextField, _w3, heighTextField), "Remove", () => {
                flagsYaml.Remove(key);
                //Check clear cache comboBoxFlags
                if (_instance != null && _instance.flagsCurrent == flagsYaml)
                {
                    _instance.ResetCombobox();
                }
            });
        }
        xGui += _w3;
        if (resetList != null)
        {
            if (index == 0)
            {
                AONGUI.Label(new Rect(xGui + _wReset - 120, yGui - 18 + DefineAON.GUI_Y_Label, 120, DefineAON.GUI_Height_Label), "Reset when interact");
            }
            bool isReset = resetList.IndexOf(key) >= 0;
            string v = " Reset";
            AONGUI.Toggle(new Rect(xGui + _wReset - 60, yGui + 8 + DefineAON.GUI_Y_Label, 60, DefineAON.GUI_Height_Label), isReset, v, (bool resetNext) => {
                if (resetNext)
                {
                    resetList.Add(key);
                }
                else
                {
                    resetList.Remove(key);
                }
            });
        }
        // xGui += _wReset;
        yGui += 32f;
    }

    // OnGUI
    private static Vector2 m_scrollPos = Vector2.zero;
    public static void OnGuiDebug(Flags flags, Rect rect, float limitHeight)
    {
        if(flags == null){
            AONGUI.Label(new Rect(rect.x, rect.y + DefineAON.GUI_Y_Label, 150, DefineAON.GUI_Height_Label), "Empty");
            return;
        }
        var keys = flags.Keys;
        // float w = 200;
        float contentHeight = 24f * (keys.Count) + 8;
        if (contentHeight < limitHeight)
        {
            limitHeight = contentHeight;
            if (limitHeight < 100)
                limitHeight = 100;
        }

        // Rect rect = new Rect( 8, Screen.height - limitHeight - 8, w, limitHeight);
        // GUI.Box( rect, "" );
        // GUI.DrawTexture( rect, GUI.skin.box.normal.background, ScaleMode.StretchToFill);

        bool hasScroll = contentHeight > limitHeight;
        rect.width = rect.width - 16f;

        if (hasScroll)
        {
            Rect view = new Rect(rect.x, rect.y,
                    rect.width + 16f, rect.height);
            Rect listRect = new Rect(rect.x, rect.y,
                    rect.width, contentHeight);
            AONGUI.BeginScrollView(view, m_scrollPos, listRect, false, false, (Vector2 v) => {
                m_scrollPos = v;
            });
            // GUI.Box(listRect, "");
        }

        float yGui = rect.y + 4;
        foreach (string key in keys)
        {
            AONGUI.Label(new Rect(rect.x, yGui, rect.width - 8, 24f), string.Format("{0} : {1}", key, flags[key]));
            yGui += 24f;
        }

        if (hasScroll)
        {
            AONGUI.EndScrollView();
        }
    }
}
