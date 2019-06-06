using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Linq;
using YamlDotNet.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System;

namespace AON.RpgMapEditor
{
    /// <summary>
    /// ScriptableObject containing map data
    /// </summary>
    public class AutoTileMapData : ScriptableObject
    {
        // private int seleted = -1;
        // public int SeletedMapIndex{
        // 	get{
        // 		return seleted;
        // 	}
        // 	set{
        // 		seleted = value;
        // 	}
        // }
        // public AutoTileMapSerializeData SeletedMap{
        // 	get{
        // 		if(seleted < 0 || seleted >=  Maps.Count){
        // 			CreateAutoTileMapData();
        // 		}
        // 		return Maps[seleted];
        // 	}
        // }

        public SerializableGame dataGame = new SerializableGame();

        public int MapIndex{
            get{
                return dataGame.MapIndex;
            }
            set{
                dataGame.MapIndex = value;
            }
        }

        public List<AutoTileMapSerializeData> Maps = new List<AutoTileMapSerializeData>();
        // public SerializableFlag RawFlagWorld = new SerializableFlag();
        // public List<FlagAction.SerializableFlagAction> RawFlagAction = new List<FlagAction.SerializableFlagAction>();
        public SerializablePropertys Propertys
        {
            get
            {
                return dataGame.Propertys;
            }
        }
        public SerializablePackages Packages
        {
            get
            {
                return dataGame.Packages;
            }
        }

        public void UpdateRaw()
        {
            dataGame.UpdateFrom(this);
        }

        public void ApplyRaw()
        {
            dataGame.ApplyTo(this);
        }

        public static string[] LockFlagWorld = {
            "Coin",
            "Level",
            "Stamina",
            "Reputation"
        };
        [System.NonSerialized]
        public Flags FlagWorld = null;

        [System.NonSerialized]
        public List<FlagAction> ListFlagAction = null;

        public void CheckAndInit()
        {
            if (Maps.Count == 0)
            {
                Maps.Add(CreateExampleMap());
            }
            if (FlagWorld == null)
            {
                ApplyRaw();
            }
        }

        public void CreateAutoTileMapData()
        {
            Maps = new List<AutoTileMapSerializeData>();
            Maps.Add(CreateExampleMap());
            // seleted = 0;
        }

        private AutoTileMapSerializeData CreateExampleMap()
        {
            var data = new AutoTileMapSerializeData();
            // data.TileMapWidth = 32;
            // data.TileMapHeight = 32;
            data.TileMapWidth = 128;
            data.TileMapHeight = 128;
            //TODO: refactor this
            // List<int> tiles0 = Enumerable.Repeat(-1, data.TileMapWidth * data.TileMapHeight).ToList();
            // List<int> tiles1 = Enumerable.Repeat(-1, data.TileMapWidth * data.TileMapHeight).ToList();
            // List<int> tiles2 = Enumerable.Repeat(-1, data.TileMapWidth * data.TileMapHeight).ToList();
            List<int> tiles = new List<int>(2);
            tiles.Add(-(data.TileMapWidth * data.TileMapHeight));
            tiles.Add(-1);
            data.TileData = new List<TileLayer>()
            {
                new TileLayer(){ Tiles = tiles, Visible = true, Name = "Ground", LayerType = eSlotAonTypeLayer.Ground, Depth = 1f},
                new TileLayer(){ Tiles = tiles, Visible = true, Name = "Overlay", LayerType = eSlotAonTypeLayer.Overlay, Depth = 0.5f},
                new TileLayer(){ Tiles = tiles, Visible = true, Name = "Trigger", LayerType = eSlotAonTypeLayer.Trigger, Depth = -1f}
            };
            data.StartX = data.TileMapWidth / 2;
            data.StartY = data.TileMapHeight / 2;
            return data;
        }

        public void AddMap()
        {
            Maps.Add(CreateExampleMap());
        }

        public string GetDataWorld(bool isIncludeMap)
        {
            //Update RawFlag
            UpdateRaw();
            if (isIncludeMap)
            {
                dataGame.Map = Maps;
            }
            else
            {
                dataGame.Map = null;
            }
            // string json = JsonUtility.ToJson(dataGame, true);
            // return json;

            string json = UtilsAON.SerializeObject(dataGame);

            return json;
        }

        public bool LoadDataWorld(string data, bool isIncludeMap)
        {
            dataGame = UtilsAON.DeserializeObject<SerializableGame>(data);
            
            if (dataGame == null)
            {
                return false;
            }
            if (isIncludeMap)
            {
                if (dataGame.Map == null || dataGame.Map.Count <= 0)
                {
                    return false;
                }
                Maps = dataGame.Map;
            }
            ApplyRaw();
            return true;
        }
    }
}
