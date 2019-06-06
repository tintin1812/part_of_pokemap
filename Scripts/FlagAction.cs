using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using AON.RpgMapEditor;

public class FlagAction{
		
	public FlagAction(){
		Name = "";
		Key = "";
		Operation = 0;
		Value = 0;
	}

	[YamlMember(Alias = "name", ApplyNamingConventions = false)]
	public string Name { get; set; }

	[YamlMember(Alias = "k", ApplyNamingConventions = false)]
	public string Key { get; set; }

	[YamlMember(Alias = "o", ApplyNamingConventions = false)]
	public int Operation { get; set; }

	[YamlMember(Alias = "v", ApplyNamingConventions = false)]
	public int Value { get; set; }

	[YamlMember(Alias = "n", ApplyNamingConventions = false)]
	public string Next { get; set; }

	[YamlMember(Alias = "a", ApplyNamingConventions = false)]
	public string KeyA { get; set; }

	[YamlMember(Alias = "b", ApplyNamingConventions = false)]
	public string KeyB { get; set; }
	
	[System.Serializable]
	public class SerializableFlagAction{
		public string Name = "";
		public string Key = "";
		public int Operation = 0;
		public int Value = 0;
		public string Next = "";

		public FlagAction FlagAction{
			get{
				var d = new FlagAction();
				d.Name = this.Name;
				d.Key = this.Key;
				d.Operation = this.Operation;
				d.Value = this.Value;
				d.Next = this.Next;
				return d;
			}
			set{
				this.Name = value.Name;
				this.Key = value.Key;
				this.Operation = value.Operation;
				this.Value = value.Value;
				this.Next = value.Next;
			}
		}
	}
	
	public static FlagAction FindFlagAction ( List<FlagAction> flagAction, string key){
		for(int k = 0; k < flagAction.Count; k++){
			FlagAction f = flagAction[k];
			if(f.Name == key){
				return f;
			}
		}
		return null;
	}

	public static int IndextFlagAction ( List<FlagAction> flagAction, string key){
		if(flagAction == null){
			return -1;
		}
		if(key == null || key == ""){
			return -1;
		}
		for(int k = 0; k < flagAction.Count; k++){
			FlagAction f = flagAction[k];
			if(f.Name == key){
				return k;
			}
		}
		return -1;
	}

	public static void DoFlagAction ( Flags flagsYaml, List<FlagAction> flagAction, string key){
		var action = FindFlagAction( flagAction, key);
		if( action == null){
			return;
		}
		DoFlagAction( flagsYaml, flagAction, action);
	}

	public static void DoFlagAction ( Flags flagsYaml, List<FlagAction> flagAction, FlagAction action){
		if(flagsYaml == null)
			return;
		flagsYaml.ActionOperation(action);
		// Check Next ( add)
		DoFlagActionLoop( flagsYaml, flagAction, action.Next, 0);
	}

	public static void DoFlagActionLoop(Flags flagsYaml, List<FlagAction> flagAction, string key, int loop){
		if(key == null || key == ""){
			return;
		}
		if(loop >= 8){
			return;
		}
		var action = FindFlagAction( flagAction, key);
		if( action == null){
			return;
		}
		flagsYaml.ActionOperation(action);
		DoFlagActionLoop( flagsYaml, flagAction, action.Next, loop + 1);
	}


	private static string NameAdd = "";
	public static bool OnGUIFlagActionList( ref float yGui, ref bool waitUI, Rect rect, List<FlagAction> flagAction, Flags flagsYaml, GUIStyle backgroundTop){
		if( backgroundTop != null){
			float height_top = 68;
			AONGUI.Box(new Rect(rect.x, rect.y, rect.width, height_top), "", backgroundTop);
		}
		AONGUI.Label(new Rect(rect.x + 4, yGui + DefineAON.GUI_Y_Label, rect.width, DefineAON.GUI_Height_Label), "Action flag edit :");
		yGui += 32f;
		{
			//Add Key
			float xGui = rect.x + 4;
			AONGUI.Label(new Rect( xGui, yGui + DefineAON.GUI_Y_Label, 90, DefineAON.GUI_Height_Label ), "Slug action");
			xGui += 94;
			AONGUI.TextField(new Rect( xGui, yGui + DefineAON.GUI_Y_TextField, 200, DefineAON.GUI_Height_TextField ), NameAdd, (string text) => {
				NameAdd = text;
			});
			xGui += 204;
			if(NameAdd.Length == 0){
			}else
			{
				bool isUnique = (IndextFlagAction(flagAction, NameAdd) == -1);
				
				if(isUnique){
					AONGUI.Button( new Rect(xGui, yGui + DefineAON.GUI_Y_Button, 40, DefineAON.GUI_Height_Button), "Add", () => {
						flagAction.Add(new FlagAction(){
							Name = NameAdd
						});
						NameAdd = "";
					});
				}else
				{
					AONGUI.Label(new Rect( xGui, yGui + DefineAON.GUI_Y_Label, 200, DefineAON.GUI_Height_Label ), "Slug should be unique");
				}
			}
			yGui += 32f;
		}
		float size_sub_remove = 70;
		float size_next = 120;
		float size_o = 80;
		float size_v = 50;
		float size_text = (rect.width - size_sub_remove - size_v - size_o - size_next - 8) / 2;
		for(int k = 0; k < flagAction.Count; k++){
			if(k == 0){
				yGui += 32f;
				continue;
			}
			float xGui = rect.x + 4;
			FlagAction f = flagAction[k];
			/*
			if(k > 0){
				var name_next = GUI.TextField(new Rect(xGui, yGui + DefineAON.GUI_Y_TextField, size_text - 4, DefineAON.GUI_Height_TextField), f.Name, 25);
				if(name_next != f.Name){
					f.Name = name_next;
					return true;
				}
			}else{
				GUI.Label(new Rect(xGui, yGui + DefineAON.GUI_Y_TextField, size_text - 4, DefineAON.GUI_Height_TextField), f.Name);
			}
			*/
			if(k == 1){
				AONGUI.Label( new Rect(xGui, yGui - 25, size_next, 24), "Slug:");
			}
			AONGUI.Label(new Rect(xGui, yGui + DefineAON.GUI_Y_TextField, size_text - 4, DefineAON.GUI_Height_TextField), f.Name);
			xGui += size_text;
			if(k == 1){
				AONGUI.Label( new Rect(xGui, yGui - 25, size_next, 24), "Flag:");
			}
			{
				var comboBoxFlags = FlagGui.Instance.UpdateFlagsData( flagsYaml, f.Key);
				int index_key = FlagGui.Instance.IndexOfKey(f.Key);
				comboBoxFlags.SelectedItemIndex = index_key;
				comboBoxFlags.Rect.x = xGui;
				comboBoxFlags.Rect.y = yGui;
				comboBoxFlags.Rect.width = size_text - 4;
				comboBoxFlags.Rect.height = 32f;
				// float limitHeight = rect.height - yGui - 32;
				float limitHeight = 32 * 6f;
				comboBoxFlags.Show( limitHeight, k.ToString(), (int flagNext) => {
					f.Key = FlagGui.Instance.KeyFromIndex(flagNext);
				});
				if(comboBoxFlags.IsDropDownWithHash(k.ToString())){
					yGui += limitHeight;
					waitUI = true;
					return false;
				}
			}
			xGui += size_text;
			{ //Operation
				int current = f.Operation;
				var comboBoxOperation = ComboBoxHelper.Instance.Operation();
				comboBoxOperation.SelectedItemIndex = current;
				comboBoxOperation.Rect.x = xGui;
				comboBoxOperation.Rect.y = yGui;
				comboBoxOperation.Rect.width = size_o - 4;
				comboBoxOperation.Rect.height = 32f;
				float limitHeight = 32f * comboBoxOperation.ListContent.Length;
				comboBoxOperation.Show( limitHeight, k.ToString(), false, (int next) => {
					f.Operation = next;
				});
				if( comboBoxOperation.IsDropDownWithHash(k.ToString())){
					yGui += limitHeight;
					waitUI = true;
					return false;
				}
			}
			float xGuiOperation = xGui;
			xGui += size_o;
			bool isHasInputValue = f.Operation < (int)AON.RpgMapEditor.ScriptGui.EOperation.Add_A_B;
			{
				if(isHasInputValue){
					AONGUI.TextField(new Rect(xGui, yGui + DefineAON.GUI_Y_TextField, size_v - 10, DefineAON.GUI_Height_TextField), f.Value.ToString(), 25, (string text) =>{
						f.Value = UtilsAON.StrToIntDef(text);	
					});
				}
				xGui += size_v;
				{
					//Next ( add) Action
					if(k == 1){
						AONGUI.Label( new Rect(xGui + 10, yGui - 25, size_next, 24), "Add action:");
					}
					var combobox = ComboBoxHelper.Instance.FlagAction( flagAction);
					var hash = k.ToString();
					int currentFlagAction = FlagAction.IndextFlagAction( flagAction, f.Next);
					if(currentFlagAction == -1){
						if(f.Next != null && f.Next != ""){
							combobox.Empty = f.Next + " (not found)";
						}else
						{
							combobox.Empty = "NULL";
						}
					}
					combobox.SelectedItemIndex = currentFlagAction;
					combobox.Rect.x = xGui + 10;
					combobox.Rect.y = yGui + 2;
					combobox.Rect.width = size_next - 20;
					combobox.Rect.height = 32f;
					float limitHeight = 32f * 6;
					combobox.Show( limitHeight, hash, (int nextAction) => {
						f.Next = flagAction[nextAction].Name;
					});
					if(combobox.IsDropDownWithHash( hash)){
						yGui += limitHeight;
						waitUI = true;
						return false;
					}
					xGui += size_next;
				}
				AONGUI.Button( new Rect(xGui, yGui + DefineAON.GUI_Y_Button, size_sub_remove - 4, DefineAON.GUI_Height_Button), "Remove", () => {
					flagAction.RemoveAt(k);
				});
			}
			
			/*
			xGui += size_sub_e;
			if(GUI.Button( new Rect(xGui, yGui + DefineAON.GUI_Y_Button, size_sub_e - 4, DefineAON.GUI_Height_Button), " + ")){
				FlagAction.Insert(k + 1, new FlagAction());
				return true;	
			}
			*/
			
			if(!isHasInputValue){
				yGui += 32f;
				// float size_ab = (rect.x + rect.width - xGuiOperation - size_sub_remove) / 2;
				float size_ab = size_o + size_v;
				xGui = xGuiOperation;
				AONGUI.Label(new Rect(xGui - size_text, yGui, size_text, DefineAON.GUI_Height_Label), "= " + AON.RpgMapEditor.ScriptGui.StrEOperation[f.Operation]);
				AONGUI.Label(new Rect(xGui - 25, yGui, 20, DefineAON.GUI_Height_Label), "a =");
				{
					string hash = "a" + k;
					var comboBoxFlags = FlagGui.Instance.UpdateFlagsData( flagsYaml, f.KeyA);
					int index_a = FlagGui.Instance.IndexOfKey(f.KeyA);
					comboBoxFlags.SelectedItemIndex = index_a;
					comboBoxFlags.Rect.x = xGui;
					comboBoxFlags.Rect.y = yGui;
					comboBoxFlags.Rect.width = size_ab;
					comboBoxFlags.Rect.height = 32f;
					// float limitHeight = rect.height - yGui - 32;
					float limitHeight = 32 * 6f;
					comboBoxFlags.Show( limitHeight, hash, (int flagNext) => {
						f.KeyA = FlagGui.Instance.KeyFromIndex(flagNext);
					});
					if(comboBoxFlags.IsDropDownWithHash(hash)){
						yGui += limitHeight;
						waitUI = true;
						return false;
					}
				}
				// xGui += size_ab;
				yGui += 32f;
				AONGUI.Label(new Rect(xGui - 25, yGui, 20, DefineAON.GUI_Height_Label), "b = ");
				{
					string hash = "b" + k;
					var comboBoxFlags = FlagGui.Instance.UpdateFlagsData( flagsYaml, f.KeyB);
					int index_b = FlagGui.Instance.IndexOfKey(f.KeyB);
					comboBoxFlags.SelectedItemIndex = index_b;
					comboBoxFlags.Rect.x = xGui;
					comboBoxFlags.Rect.y = yGui;
					comboBoxFlags.Rect.width = size_ab;
					comboBoxFlags.Rect.height = 32f;
					// float limitHeight = rect.height - yGui - 32;
					float limitHeight = 32 * 6f;
					comboBoxFlags.Show( limitHeight, hash, (int flagNext) => {
						f.KeyB = FlagGui.Instance.KeyFromIndex(flagNext);
					});
					if(comboBoxFlags.IsDropDownWithHash(hash)){
						yGui += limitHeight;
						waitUI = true;
						return false;
					}
					// var if_a_next = GUI.TextField(new Rect(rect.x + xGui, yGui + 32 - heighTextField, _w, heighTextField), if_a, 25);
				}
				yGui += 32f;
				yGui += 16f;
			}else
			{
				yGui += 32f;
			}
		}
		return false;
	}
}
