using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AON.RpgMapEditor
{
	public class OverlayGui {

		private static OverlayGui _instance = null;
		
		public static OverlayGui Instance{ 
			get{
				if(_instance == null){
					_instance = new OverlayGui();
				}
				return _instance;
			}
		}

		private OverlayGui(){}

		private int currentIdNpcTager = 0;
		public bool HouseOnGUI(House house, Rect rect, AutoTileMap autoTileMap, TilesetAON tilesetAON){

			bool hasUpdateUI = false;
			float left = 80f;
			// Input Name
			float yGui = rect.y + 8f;
			float heighTextField = 20f;
			
			AONGUI.Label(new Rect(rect.x, yGui, rect.width, 32f), string.Format("Edit House {0}", house.NameHouse));
			yGui += 32;
			
			AONGUI.Label(new Rect(rect.x, yGui, left, 32), "Name: ");
			AONGUI.TextField(new Rect(rect.x + left, yGui + 32 - heighTextField, rect.width - left, heighTextField), house.NameHouse, 25, (string text) => {
				house.NameHouse = text;
			});
			yGui += 32;
			#region Art
			{
				AONGUI.Label(new Rect(rect.x, yGui, left, 32), "House: ");
				var comboBoxHouseList = ComboBoxHelper.Instance.HouseListModel(autoTileMap);
				comboBoxHouseList.SelectedItemIndex = house.IdxArt;
				comboBoxHouseList.Rect.x = rect.x + left;
				comboBoxHouseList.Rect.y = yGui;
				comboBoxHouseList.Rect.width = rect.width - left;
				comboBoxHouseList.Rect.height = 32f;
				comboBoxHouseList.Show( rect.height - yGui - 32, (int selectedArt) => {
					house.IdxArt = selectedArt;
					//
					int tx = tilesetAON.TileShowMoreInfo.TileX;
					int ty = tilesetAON.TileShowMoreInfo.TileY;
					// var slot = autoTileMap.Tileset.GetSlot(tilesetAON.TileShowMoreInfo.Id);
					autoTileMap.TileChunkPoolNode.GetTileChunk( tx, ty, (int)tilesetAON.TileShowMoreInfo.Layer).RefreshTileOverlay( tx, ty, true);
				});
				yGui += 32f;
				if(comboBoxHouseList.IsDropDownListVisible){
					return hasUpdateUI;
				}
			}
			#endregion
			if(house.IdxArt == -1){
				return hasUpdateUI;
			}
			#region Go In
			if( tilesetAON.TileShowMoreInfo != null){
				int xOffsetIn = 0, yOffsetIn = 0;
				int tx = tilesetAON.TileShowMoreInfo.TileX;
				int ty = tilesetAON.TileShowMoreInfo.TileY;
				var rotateRef = autoTileMap.MapSelect.GetRotateRef(tx, ty) % 360;
				house.GetOffsetFromRotate(ref xOffsetIn,ref yOffsetIn, rotateRef);
				AONGUI.Label(new Rect(rect.x+ 4f, yGui + 4f, rect.width - 8f, 32 - 8f), string.Format("Go in at: ({0} ,{1})", xOffsetIn, yOffsetIn));
				yGui += 32f;
				AONGUI.Button( new Rect(rect.x, yGui, rect.width, 32), "Pick go in", () => {
					tilesetAON.PickPosOnMap(autoTileMap.MapIdxSelect, tilesetAON.TileShowMoreInfo.TileX + xOffsetIn, tilesetAON.TileShowMoreInfo.TileY + yOffsetIn, (PickMapAON p, int _x, int _y) => {
						// house.XOffsetIn = _x - tilesetAON.TileShowMoreInfo.TileX;
						// house.YOffsetIn = _y - tilesetAON.TileShowMoreInfo.TileY;
						house.SetOffsetFromRotate( _x - tilesetAON.TileShowMoreInfo.TileX, _y - tilesetAON.TileShowMoreInfo.TileY, rotateRef);
					});
				});
				yGui += 32f;
			}
			#endregion //Door
			#region Interior
			{
				AONGUI.Label(new Rect(rect.x, yGui, left, 32), "Interior: ");
				var comboBoxInteriorList = ComboBoxHelper.Instance.InteriorList( autoTileMap);
				comboBoxInteriorList.SelectedItemIndex = house.IdxInterior;
				comboBoxInteriorList.Rect.x = rect.x + left;
				comboBoxInteriorList.Rect.y = yGui;
				comboBoxInteriorList.Rect.width = rect.width - left;
				comboBoxInteriorList.Rect.height = 32f;
				comboBoxInteriorList.Show( rect.height - yGui - 32, (int selectedInterior) => {
					house.IdxInterior = selectedInterior;
				});
				yGui += 32f;
				if(comboBoxInteriorList.IsDropDownListVisible){
					return hasUpdateUI;
				}
			}
			#endregion
			if(house.IdxInterior == -1){
				return hasUpdateUI;
			}
			#region Go Out
			// GUI.Label(new Rect(rect.x+ 4f, yGui + 4f, rect.width - 8f, 32 - 8f), string.Format("Go out at: {0}", house.OffsetOut.ToString()));
			// yGui += 32f;
			// GUI.Label(new Rect(rect.x+ 4f, yGui + 4f, rect.width - 8f, 32 - 8f), string.Format("Cam: {0}", house.CamOut.ToString()));
			// yGui += 32f;
			AONGUI.Button( new Rect(rect.x, yGui, rect.width, 32), "Pick go out and camera", () => {
				tilesetAON.PickPosOnInterior( house.IdxInterior, house.OffsetOut , house.CamOut, ( TilesetAON t, Vector3 p, Vector3 cam) => {
					house.OffsetOut = p;
					house.CamOut = cam;
				});
			});
			yGui += 32f;
			#endregion //Door
			// yGui += 16f;
			if(house.NpcInHouses.Count == 0){
				AONGUI.Button( new Rect(rect.x, yGui, rect.width, 32), "Add NPC Inhouse", () => {
					house.NpcInHouses.Add(new House.NpcInHouse());
					currentIdNpcTager = 0;
				});
				return false;
			}
			AONGUI.Label(new Rect(rect.x, yGui, left, 28), "Slot NPC: ");
			var comboBoxCount = ComboBoxHelper.Instance.Number(house.NpcInHouses.Count);
			comboBoxCount.SelectedItemIndex = currentIdNpcTager;
			comboBoxCount.Rect.x = rect.x + left;
			comboBoxCount.Rect.y = yGui;
			comboBoxCount.Rect.width = rect.width - left;
			comboBoxCount.Rect.height = 32f;
			if(comboBoxCount.IsDropDownListVisible){
				AONGUI.Button(new Rect(rect.x + left, rect.y + rect.height - 64, rect.width - left, 28), "Add Slot", () => {
					house.NpcInHouses.Add(new House.NpcInHouse());
					currentIdNpcTager = house.NpcInHouses.Count - 1;
				});
			}
			comboBoxCount.Show( rect.height - yGui - 64, (int next) => {
				currentIdNpcTager = next;
			});
			if(comboBoxCount.IsDropDownListVisible){
				return hasUpdateUI;
			}
			yGui += 32f;
			if(currentIdNpcTager >= 0 && currentIdNpcTager < house.NpcInHouses.Count){
				if(HouseOnGUIWithNPC(house, house.NpcInHouses[currentIdNpcTager], rect, autoTileMap, tilesetAON, ref yGui)){
					return hasUpdateUI;
				}
			}
			return hasUpdateUI;
		}

		public bool HouseOnGUIWithNPC(House house, House.NpcInHouse npc, Rect rect, AutoTileMap autoTileMap, TilesetAON tilesetAON, ref float yGui)
		{
			{
				float left = 80f;
				AONGUI.Label(new Rect(rect.x, yGui, left, 28), "Npc ref:");
				var comboBox = ComboBoxHelper.Instance.NPCList( autoTileMap.MapSelect);
				comboBox.SelectedItemIndex = npc.IdxNPC;
				comboBox.Rect.x = rect.x + left;
				comboBox.Rect.y = yGui;
				comboBox.Rect.width = rect.width - left;
				comboBox.Rect.height = 32f;
				if(comboBox.IsDropDownListVisible){
					AONGUI.Button(new Rect(rect.x + left, rect.y + rect.height - 64, rect.width - left, 28), "Add NPC", () => {
						npc.IdxNPC = autoTileMap.MapSelect.CreateNewNPC();
					});
					return true;
				}
				comboBox.Show( rect.height - yGui - 64, (int selectedNpc) => {
					npc.IdxNPC = selectedNpc;
				});
				yGui += 32f;
				if(comboBox.IsDropDownListVisible){
					return true;
				}
			}
			if(npc.IdxNPC >= 0 && npc.IdxNPC < autoTileMap.MapSelect.NPCData.Count){
				AONGUI.Button( new Rect(rect.x + rect.width - 100, yGui, 100, 28), "Edit NPC", () => {
					tilesetAON.TriggerShowMoreInfo = autoTileMap.MapSelect.NPCData[npc.IdxNPC];
				});
			}
			yGui += 32f;

			AONGUI.Button( new Rect(rect.x, yGui, rect.width, 28), "Setup pos and face NPC", () => {
				tilesetAON.PickPosOnInterior( house.IdxInterior, npc.NPC_OffsetOut , npc.NPC_CamOut, ( TilesetAON t, Vector3 p, Vector3 cam) => {
					npc.NPC_OffsetOut = p;
					npc.NPC_CamOut = cam;
				});
			});
			yGui += 32f;
			return false;
		}
	}
}