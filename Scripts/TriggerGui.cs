using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AON.RpgMapEditor
{
	public class TriggerGui {
		private static TriggerGui _instance = null;
		
		public static TriggerGui Instance{ 
			get{
				if(_instance == null){
					_instance = new TriggerGui();
				}
				return _instance;
			}
		}

		private TriggerGui(){}
		
		private float heighTextField = 25f;

		#region Warp
		public bool WarpOnGUI( Warps warp, Rect rect, AutoTileMap autoTileMap, TilesetAON tilesetAON){
			bool hasUpdateUI = false;
			float left = 80f;
			// Input Name
			float yGui = rect.y + 8f;
			
			AONGUI.Label(new Rect(rect.x, yGui, rect.width, 32f), string.Format("Edit Warp {0}", warp.NameWarps));
			yGui += 32;

			AONGUI.Label(new Rect(rect.x, yGui, left, 32), "Name: ");
			AONGUI.TextField(new Rect(rect.x + left, yGui + 32 - heighTextField, rect.width - left, heighTextField), warp.NameWarps, 25, (string text) => {
				warp.NameWarps = text;
			});
			yGui += 32;
			// Pick Map
			AONGUI.Label(new Rect(rect.x, yGui, left, 32), "Map: ");
			var ComboBoxMap = ComboBoxHelper.Instance.MapList( autoTileMap);
			ComboBoxMap.SelectedItemIndex = warp.map;
			ComboBoxMap.Rect.x = rect.x + left;
			ComboBoxMap.Rect.y = yGui;
			ComboBoxMap.Rect.width = rect.width - left;
			ComboBoxMap.Rect.height = 32f;
			ComboBoxMap.Show( rect.height - yGui - 32, (int selectedMap) => {
				warp.map = selectedMap;
			});
			yGui += 32f;
			if(ComboBoxMap.IsDropDownListVisible){
				return hasUpdateUI;
			}
			if(warp.map != -1){
				AONGUI.Label(new Rect(rect.x+ 4f, yGui + 4f, rect.width - 8f, 32 - 8f), "x: " + warp.x);
				yGui += 16f;
				AONGUI.Label(new Rect(rect.x+ 4f, yGui + 4f, rect.width - 8f, 32 - 8f), "y: " + warp.y);
				yGui += 32f;
				AONGUI.Button( new Rect(rect.x, yGui, rect.width, 32), "Select position on map...", () => {
					tilesetAON.PickPosOnMap(warp.map, warp.x, warp.y, (PickMapAON p, int _x, int _y) => {
						warp.x = _x;
						warp.y = _y;
					});
				});
			}
			return hasUpdateUI;
		}
		#endregion

		#region NPC
		public bool NPCOnGUI(NPC npc, Rect rect, AutoTileMap autoTileMap, TilesetAON tilesetAON){
			bool hasUpdateUI = false;
			float left = 80f;
			// Input Name
			float yGui = rect.y + 8f;
			
			AONGUI.Label(new Rect(rect.x, yGui, rect.width, 32f), string.Format("Edit NPC {0}", npc.NameNPC));
			yGui += 32;

			AONGUI.Label(new Rect(rect.x, yGui, left, 32), "Name: ");
			AONGUI.TextField(new Rect(rect.x + left, yGui + 32 - heighTextField, rect.width - left, heighTextField), npc.NameNPC, 25, (string text) => {
				npc.NameNPC = text;
			});
			yGui += 32;
			#region Art
			{
				AONGUI.Label(new Rect(rect.x, yGui, left, 32), "Model: ");
				var comboBoxNPCModel = ComboBoxHelper.Instance.NPCModel(autoTileMap);
				comboBoxNPCModel.SelectedItemIndex = npc.IdxArt;
				comboBoxNPCModel.Rect.x = rect.x + left;
				comboBoxNPCModel.Rect.y = yGui;
				comboBoxNPCModel.Rect.width = rect.width - left;
				comboBoxNPCModel.Rect.height = 32f;
				comboBoxNPCModel.Show( rect.height - yGui - 32, (int selectedArt) => {
					npc.IdxArt = selectedArt;
				});
				if(comboBoxNPCModel.IsDropDownListVisible){
					return hasUpdateUI;
				}
			}
			yGui += 32f;
			#endregion
			yGui += 16f;
			#region Script
			AONGUI.Toggle(new Rect(rect.x, yGui, rect.width, 32), npc.StartScript, "Run Script when Start map", (bool b) =>{
				npc.StartScript = b;
			});
			yGui += 32f;
			if(npc.StartScript){
				if(autoTileMap.MapSelect.ScriptData.Count == 0){
					AONGUI.Label(new Rect(rect.x, yGui, left, 32), "Script: NULL");
					yGui += 32f;
					AONGUI.Button(new Rect(rect.x, yGui, rect.width, 32), "Create new script", () => {
						npc.IdxStartScript = autoTileMap.MapSelect.CreateNewScript();
					});
				}else{
					AONGUI.Label(new Rect(rect.x, yGui, left, 32), "Script: ");
					var comboBox = ComboBoxHelper.Instance.Scripts(autoTileMap.MapSelect);
					comboBox.SelectedItemIndex = npc.IdxStartScript;
					comboBox.Rect.x = rect.x + left;
					comboBox.Rect.y = yGui;
					comboBox.Rect.width = rect.width - left;
					comboBox.Rect.height = 32f;
					string hash = "Start";
					if(comboBox.IsDropDownWithHash(hash)){
						AONGUI.Button(new Rect(rect.x, rect.y + rect.height - 64, rect.width, 32), "Create new script", () => {
							npc.IdxStartScript = autoTileMap.MapSelect.CreateNewScript();
						});
					}
					comboBox.Show( rect.height - yGui - 64, hash, (int selectedScript) => {
						npc.IdxStartScript = selectedScript;
					});
					if(comboBox.IsDropDownWithHash(hash)){
						return hasUpdateUI;
					}
				}
				yGui += 32f;
				if(npc.IdxStartScript >= 0 && npc.IdxStartScript < autoTileMap.MapSelect.ScriptData.Count){
					var w = (rect.width - left) / 2;
					AONGUI.Button(new Rect(rect.x + rect.width - w, yGui, w, 28), "Edit Script", () => {
						tilesetAON.TriggerShowMoreInfo = autoTileMap.MapSelect.ScriptData[npc.IdxStartScript];
					});
				}
			}
			yGui += 32f;
			yGui += 16f;
			AONGUI.Toggle(new Rect(rect.x, yGui, rect.width, 32), npc.RunScript, "Run Script when Talking", ( bool b) => {
				npc.RunScript = b;
			});
			yGui += 32f;
			if(npc.RunScript){
				if(autoTileMap.MapSelect.ScriptData.Count == 0){
					AONGUI.Label(new Rect(rect.x, yGui, left, 32), "Script: NULL");
					yGui += 32f;
					AONGUI.Button(new Rect(rect.x, yGui, rect.width, 32), "Create new script", () => {
						npc.IdxScript = autoTileMap.MapSelect.CreateNewScript();
					});
				}else{
					AONGUI.Label(new Rect(rect.x, yGui, left, 32), "Script: ");
					var comboBox = ComboBoxHelper.Instance.Scripts(autoTileMap.MapSelect);
					comboBox.SelectedItemIndex = npc.IdxScript;
					comboBox.Rect.x = rect.x + left;
					comboBox.Rect.y = yGui;
					comboBox.Rect.width = rect.width - left;
					comboBox.Rect.height = 32f;
					string hash = "Talk";
					if(comboBox.IsDropDownWithHash(hash)){
						AONGUI.Button(new Rect(rect.x, rect.y + rect.height - 64, rect.width, 32), "Create new script", () => {
							npc.IdxScript = autoTileMap.MapSelect.CreateNewScript();
						});
					}
					comboBox.Show( rect.height - yGui - 64, hash, (int selectedScript) => {
						npc.IdxScript = selectedScript;
					});
					if(comboBox.IsDropDownWithHash(hash)){
						return hasUpdateUI;
					}
				}
				yGui += 32f;
				if(npc.IdxScript >= 0 && npc.IdxScript < autoTileMap.MapSelect.ScriptData.Count){
					var w = (rect.width - left) / 2;
					AONGUI.Button(new Rect(rect.x + rect.width - w, yGui, w, 28), "Edit Script", () => {
						tilesetAON.TriggerShowMoreInfo = autoTileMap.MapSelect.ScriptData[npc.IdxScript];
					});
				}
				yGui += 32f;
			}
			#endregion
			return hasUpdateUI;
		}
		#endregion

	}
}
