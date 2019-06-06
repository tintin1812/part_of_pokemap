using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using AON.RpgMapEditor;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Newtonsoft.Json;
using System;

namespace AON.RpgMapEditor
{
	[System.Serializable]
	public class MetadataChunk
	{
		public static string k_version = "1.2.4";//TODO: change this after each update!

		public string version = k_version; 
		public bool compressedTileData = true;

		public bool IsVersionAboveOrEqual(string versionToCompare)
		{
			string[] curVersion = version.Split('.');
			string[] compareVersion = versionToCompare.Split('.');
			for( int i = 0; i < curVersion.Length && i < compareVersion.Length; ++i )
			{
				if (System.Convert.ToInt32(curVersion[i]) < System.Convert.ToInt32(compareVersion[i]))
					return false;
			}
			return compareVersion.Length >= curVersion.Length;
		}
	}

	[System.Serializable]
	public class TileLayer
	{
		public List<int> Tiles;
		public bool Visible = true;
		public string Name;
		public eSlotAonTypeLayer LayerType;
		public string SortingLayer = "Default"; // sorting layer
		public int SortingOrder = 0; // sorting order
		public float Depth;
	}
	
	[System.Serializable]
	public class Pos
	{
		public int map = 0;
		public int x = -1;
		public int y = -1;
		public bool ShowGUI( Rect rect){
			return false;
		}
	}

	[System.Serializable]
	public class Trigger{
		public virtual string Name(){
			return "";
		}

		//Return true if had change Trigger Ref
		public virtual bool ShowGUI( Rect rect, AutoTileMap autoTileMap, TilesetAON tilesetAON, ref bool isShowMoreInfo, AComponent_Button.OnClick onCloseDialog){
			// GUI.Box( rect, "Edit Trigger");
			return false;
		}
	}

	[System.Serializable]
	public class Warps : Trigger
	{
		public int map = -1;
		public int x = 0;
		public int y = 0;
		// public int toWarp = -1;

		public string nameWarps = "";
		
		public override string Name(){
			return nameWarps;
		}
		public string NameWarps{
			get{
				return nameWarps;
			}
			set{
				ComboBoxHelper.Instance.ResetTypeObj(eSlotAonTypeObj.Warps);
				nameWarps = value;
			}
		}

		//Return true if had change Trigger Ref
		public override bool ShowGUI( Rect rect, AutoTileMap autoTileMap, TilesetAON tilesetAON, ref bool isShowMoreInfo, AComponent_Button.OnClick onCloseDialog){
			return TriggerGui.Instance.WarpOnGUI( this, rect, autoTileMap, tilesetAON);
		}
	}

	[System.Serializable]
	public class Signposts : Trigger
	{
		public string info; 
	}

	[System.Serializable]
	public class NPC : Trigger
	{
		public string nameNpc = "";

		public int IdxArt = -1;
		
		public int IdxScript = -1;

		public bool RunScript = false;

		public int IdxStartScript = -1;

		public bool StartScript = false;

		public override string Name(){
			return nameNpc;
		}

		public string NameNPC{
			get{
				return nameNpc;
			}
			set{
				ComboBoxHelper.Instance.ResetTypeObj(eSlotAonTypeObj.Person);
				nameNpc = value;
			}
		}

		//Return true if had change Trigger Ref
		public override bool ShowGUI( Rect rect, AutoTileMap autoTileMap, TilesetAON tilesetAON, ref bool isShowMoreInfo, AComponent_Button.OnClick onCloseDialog){
			return TriggerGui.Instance.NPCOnGUI( this, rect, autoTileMap, tilesetAON);
		}
	}

	[System.Serializable]
	public class Script : Trigger
	{
		// [SerializeField][JsonIgnore]
		// private string data = null;

		[System.NonSerialized]
		public ScriptGuiBase.ScriptYaml _scriptYaml;

		// Load for Old version
		public string Data{
			get{
				return null;
			}
			set{
				if(!string.IsNullOrEmpty(value)){
					try
					{
						var deserializer = new DeserializerBuilder()
						.WithNamingConvention(new CamelCaseNamingConvention())
						.IgnoreUnmatchedProperties()
						.Build();
						this._scriptYaml = deserializer.Deserialize<ScriptGui.ScriptYaml>(value);
						if(this._scriptYaml != null){
							this._scriptYaml.IdToObj();
						}
					}
					catch (Exception exception)
					{
						Debug.Log(exception);
					}
				}
			}
		}

		[JsonIgnore]
		public string YamlData{
			get{
				// if(string.IsNullOrEmpty(data) && _scriptYaml != null){
				if(_scriptYaml != null){
					_scriptYaml.ObjToID();
					var serializer = new SerializerBuilder().Build();
					var data = serializer.Serialize(_scriptYaml);
					_scriptYaml.IdToObj();
					return data;
				}
				return null;
			}
			set{
				if(!string.IsNullOrEmpty(value)){
					var deserializer = new DeserializerBuilder()
					.WithNamingConvention(new CamelCaseNamingConvention())
					.IgnoreUnmatchedProperties()
					.Build();
					this._scriptYaml = deserializer.Deserialize<ScriptGui.ScriptYaml>(value);
					if(this._scriptYaml != null){
						this._scriptYaml.IdToObj();
					}
				}
			}
		}
		
		public ScriptGuiBase.ScriptYaml ScriptYaml{
			get{
				if(UtilsAON.IsDuringSerializeObject){
					if(_scriptYaml == null){
						return null;
					}
					_scriptYaml.ObjToID();
					var serializer = new SerializerBuilder().Build();
					var data = serializer.Serialize(_scriptYaml);

					var deserializer = new DeserializerBuilder()
						.WithNamingConvention(new CamelCaseNamingConvention())
						.IgnoreUnmatchedProperties()
						.Build();
					var clone = deserializer.Deserialize<ScriptGui.ScriptYaml>(data);

					_scriptYaml.IdToObj();
					
					return clone;
				}
				// if(_scriptYaml != null){
				// 	_scriptYaml.IdToObj();
				// }
				return _scriptYaml;
			}
			set{
				_scriptYaml = value;
				if(this._scriptYaml != null){
					this._scriptYaml.IdToObj();
				}
			}
		}

		public string nameScript = "";
		
		public override string Name(){
			return nameScript;
		}
		
		[JsonIgnore]
		public string NameScript{
			get{
				return nameScript;
			}
			set{
				ComboBoxHelper.Instance.ResetTypeObj(eSlotAonTypeObj.Script);
				nameScript = value;
			}
		}

		//Return true if had change Trigger Ref
		public override bool ShowGUI( Rect rect, AutoTileMap autoTileMap, TilesetAON tilesetAON, ref bool isShowMoreInfo, AComponent_Button.OnClick onCloseDialog){
			return ScriptGui.Instance.ScriptOnGUI( this, rect, autoTileMap, tilesetAON, ref isShowMoreInfo, onCloseDialog);
		}
	}
	
	[System.Serializable]
	public class Overlay{
		public virtual string Name(){
			return "";
		}

		//Return true if had change Trigger Ref
		public virtual bool ShowGUI( Rect rect, AutoTileMap autoTileMap, TilesetAON tilesetAON){
			// GUI.Box( rect, "Edit Overlay");
			return false;
		}
	}

	[System.Serializable]
	public class House : Overlay
	{
		public string nameHouse = "";
		public int IdxArt = 0;
		public int IdxInterior = 0;
		public int XOffsetIn = 0;
		public int YOffsetIn = 0;
		public SerializableVector3 OffsetOut = Vector3.zero;
		public SerializableVector3 CamOut = Vector3.zero;
		//Npc inhouse
		// public int IdxNPC = -1;
		// public Vector3 NPC_OffsetOut = Vector3.zero;
		// public Vector3 NPC_CamOut = Vector3.zero;

		public List<NpcInHouse> NpcInHouses = new List<NpcInHouse>();

		public override string Name(){
			return nameHouse;
		}
		public string NameHouse{
			get{
				return nameHouse;
			}
			set{
				ComboBoxHelper.Instance.ResetTypeObj(eSlotAonTypeObj.House);
				nameHouse = value;
			}
		}

		[System.Serializable]
		public class NpcInHouse{
			public int IdxNPC = -1;
			public SerializableVector3 NPC_OffsetOut = Vector3.zero;
			public SerializableVector3 NPC_CamOut = Vector3.zero;                
		}
		//Return true if had change Trigger Ref
		public override bool ShowGUI( Rect rect, AutoTileMap autoTileMap, TilesetAON tilesetAON){
			return OverlayGui.Instance.HouseOnGUI( this, rect, autoTileMap, tilesetAON);
		}

		public void GetOffsetFromRotate(ref int x, ref int y, int r){
			if(r >= 0 && r < 90){
				//Bot
				x = YOffsetIn;
				y = XOffsetIn;
			}else if(r >= 90 && r < 180){
				//Right
				x = XOffsetIn;
				y = YOffsetIn;
			}else if(r >= 180 && r < 270){
				//Top
				x = YOffsetIn;
				y = -XOffsetIn;
			}else{
				//Left
				x = -XOffsetIn;
				y = YOffsetIn;
			}
		}

		public void SetOffsetFromRotate(int xOffset, int yOffset, int r){
			if(r >= 0 && r < 90){
				//Bot
				YOffsetIn = xOffset;
				XOffsetIn = yOffset;
			}else if(r >= 90 && r < 180){
				//Right
				XOffsetIn = xOffset;
				YOffsetIn = yOffset;
			}else if(r >= 180 && r < 270){
				//Top
				YOffsetIn = xOffset;
				XOffsetIn = -yOffset;
			}else{
				//Left
				XOffsetIn = -xOffset;
				YOffsetIn = yOffset;
			}
		}
	}

	[System.Serializable]
	public class  AutoTileMapSerializeData_Base {
		public MetadataChunk Metadata = new MetadataChunk(); // 1
		public int TileMapWidth; // 2
		public int TileMapHeight; // 3

        public int StartX = 30; // 3.1
        public int StartY = 30; // 3.2

        public List<TileLayer> TileData = new List<TileLayer>(); // 4
        
        public List<Warps> WarpsData = new List<Warps>(); // 5
        
        public List<Signposts> SignpostsData = new List<Signposts>(); // 6

        public List<Script> ScriptData = new List<Script>(); // 7

        [System.NonSerialized]
        public int[,] TriggerLink = null; // 8
        
        [System.NonSerialized]
        public int[,] OverlayLink = null; // 9

        public List<int> TriggerLink_C = new List<int>(); // 10
        
        public List<int> OverlayLink_C = new List<int>(); // 11
        
        public List<House> HouseData = new List<House>(); // 12
    
        [System.NonSerialized]
        public int[,] High = null; // 13
        
        public List<int> High_C = new List<int>(); // 14

        public List<NPC> NPCData = new List<NPC>(); // 15

        [System.NonSerialized]
        public int[,] OverlayRotate = null; // 16

        public List<int> OverlayRotate_C = new List<int>(); // 17

		// [System.NonSerialized]
		public Flags FlagMap = new Flags(); // 18

        // public SerializableFlag RawFlagMap = new SerializableFlag(); // 19

        [System.NonSerialized]
		public List<FlagAction> ListFlagAction = new List<FlagAction>(); //20

		public List<FlagAction.SerializableFlagAction> RawFlagAction = new List<FlagAction.SerializableFlagAction>(); // 21
	}
}