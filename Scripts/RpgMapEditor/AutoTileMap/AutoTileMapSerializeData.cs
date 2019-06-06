using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace AON.RpgMapEditor
{
    /// <summary>
    /// Map data containing all tiles and the size of the map
    /// </summary>
	[System.Serializable]
	public class AutoTileMapSerializeData : AutoTileMapSerializeData_Base
	{
        #region Trigger
        public int GetTriggerRef( int x, int y){
            return TriggerLink[x,y];
        }

        public int TriggerCountAt( eSlotAonTypeObj typeObj){
            if(typeObj == eSlotAonTypeObj.Warps){
                return WarpsData.Count;
			}else if(typeObj == eSlotAonTypeObj.Signposts){
                return SignpostsData.Count;
			}else if(typeObj == eSlotAonTypeObj.Person){
                return NPCData.Count;
			}else if(typeObj == eSlotAonTypeObj.Script){
				return ScriptData.Count;
			}
            return 0;
        }

        public Trigger CreateNewTrigger( int x, int y, eSlotAonTypeObj typeObj){
            Trigger trigger = null;
            int idGen = -1;
            if(typeObj == eSlotAonTypeObj.Warps){
                idGen = WarpsData.Count;
                WarpsData.Add(new Warps());
                trigger = WarpsData[idGen];
			}else if(typeObj == eSlotAonTypeObj.Signposts){
                idGen = SignpostsData.Count;
                SignpostsData.Add(new Signposts());
                trigger = SignpostsData[idGen];
			}else if(typeObj == eSlotAonTypeObj.Person){
                idGen = NPCData.Count;
                NPCData.Add(new NPC());
                trigger = NPCData[idGen];
                // ComboBoxHelper.Instance.ResetDataNPCList();
			}else if(typeObj == eSlotAonTypeObj.Script){
				idGen = ScriptData.Count;
                ScriptData.Add(new Script());
                trigger = ScriptData[idGen];
                // ComboBoxHelper.Instance.ResetDataScriptList();
			}
            if(trigger != null){
                // CheckInitArray(ref TriggerLink, TileMapWidth, TileMapHeight, -1);
                ComboBoxHelper.Instance.ResetTypeObj( typeObj);
                TriggerLink[x,y] = idGen;
            }
            return trigger;
        }

        public Trigger CreateNewTrigger(eSlotAonTypeObj typeObj){
            Trigger trigger = null;
            int idGen = -1;
            if(typeObj == eSlotAonTypeObj.Warps){
                idGen = WarpsData.Count;
                WarpsData.Add(new Warps());
                trigger = WarpsData[idGen];
			}else if(typeObj == eSlotAonTypeObj.Signposts){
                idGen = SignpostsData.Count;
                SignpostsData.Add(new Signposts());
                trigger = SignpostsData[idGen];
			}else if(typeObj == eSlotAonTypeObj.Person){
                idGen = NPCData.Count;
                NPCData.Add(new NPC());
                trigger = NPCData[idGen];
                // ComboBoxHelper.Instance.ResetDataNPCList();
			}else if(typeObj == eSlotAonTypeObj.Script){
				idGen = ScriptData.Count;
                ScriptData.Add(new Script());
                trigger = ScriptData[idGen];
                // ComboBoxHelper.Instance.ResetDataScriptList();
			}
            return trigger;
        }
        
        public void ResetTriggerRef( int x, int y){
            // CheckInitArray(ref TriggerLink, TileMapWidth, TileMapHeight, -1);
            TriggerLink[x,y] = -1;
        }

        public void SetTriggerRef( int x, int y, int idxTrigger){
            // CheckInitArray(ref TriggerLink, TileMapWidth, TileMapHeight, -1);
            TriggerLink[x,y] = idxTrigger;
        }

        public Trigger GetTrigger( int x, int y, eSlotAonTypeObj typeObj){
            if( x < 0 || x >= TileMapWidth || y < 0 || y >= TileMapHeight)
                return null;
            int idxRef = TriggerLink[x,y];
            if(idxRef < 0){
                return null;
            }
            return GetTriggerByIdxRef( idxRef, typeObj);
        }

        public Trigger GetTriggerByIdxRef( int idxRef, eSlotAonTypeObj typeObj){
            if(idxRef < 0)
                return null;
            if(typeObj == eSlotAonTypeObj.Warps){
                if(idxRef >= WarpsData.Count)
                    return null;
                return WarpsData[idxRef];
			}else if(typeObj == eSlotAonTypeObj.Signposts){
                if(idxRef >= SignpostsData.Count)
                    return null;
                return SignpostsData[idxRef];
			}else if(typeObj == eSlotAonTypeObj.Person){
                if(idxRef >= NPCData.Count)
                    return null;
                return NPCData[idxRef];
			}else if(typeObj == eSlotAonTypeObj.Script){
                if(idxRef >= ScriptData.Count)
                    return null;
                return ScriptData[idxRef];
			}
            return null;
        }
        #endregion //Trigger

        #region Overlay
        public int GetOverlayRef( int x, int y){
            return OverlayLink[x, y];
        }

        public int OverlayCountAt( eSlotAonTypeObj typeObj){
            if(typeObj == eSlotAonTypeObj.House){
                return HouseData.Count;
			}
            return 0;
        }

        public Overlay CreateNewOverlay( int x, int y, eSlotAonTypeObj typeObj){
            Overlay overlay = null;
            int idGen = -1;
            if(typeObj == eSlotAonTypeObj.House){
                idGen = HouseData.Count;
                HouseData.Add(new House());
                overlay = HouseData[idGen];
                ComboBoxHelper.Instance.ResetTypeObj( eSlotAonTypeObj.House);
			}
            if(overlay != null){
                // CheckInitArray(ref OverlayLink, TileMapWidth, TileMapHeight, -1);
                OverlayLink[x,y] = idGen;
            }
            return overlay;
        }
        
        public void ResetOverlayRef( int x, int y){
            // CheckInitArray(ref OverlayLink, TileMapWidth, TileMapHeight, -1);
            OverlayLink[x,y] = -1;
        }

        public void SetOverlayRef( int x, int y, int idxOverlay){
            // CheckInitArray(ref OverlayLink, TileMapWidth, TileMapHeight, -1);
            OverlayLink[x,y] = idxOverlay;
        }

        public Overlay GetOverlay( int x, int y, eSlotAonTypeObj typeObj){
            if( x < 0 || x >= TileMapWidth || y < 0 || y >= TileMapHeight)
                return null;
            int idxRef = OverlayLink[x,y];
            if(idxRef < 0){
                return null;
            }
            return GetOverlayByIdxRef( idxRef, typeObj);
        }

        public Overlay GetOverlayByIdxRef( int idxRef, eSlotAonTypeObj typeObj){
            if(typeObj == eSlotAonTypeObj.House){
                if(idxRef >= HouseData.Count)
                    return null;
                return HouseData[idxRef];
			}
            return null;
        }
        #endregion //Overlay

        public int GetHighRef( int x, int y){
            return High[x,y];
        }
        public bool SetHighRef( int x, int y, int idxHighRef){
            // CheckInitArray(ref High, TileMapWidth, TileMapHeight, 0);
            if(High[x,y] == idxHighRef){
                return false;    
            }
            High[x,y] = idxHighRef;
            return true;
        }
        //
        public int GetRotateRef( int x, int y){
            var v = OverlayRotate[x,y];
            if(v == -1){ // Fix
                v = 0;
            }
            return v;
        }
        
        public bool SetRotateRef( int x, int y, int v){
            // CheckInitArray(ref OverlayRotate, TileMapWidth, TileMapHeight, -1);
            if(OverlayRotate[x,y] == v)
                return false;
            OverlayRotate[x,y] = v;
            return true;
        }
        
        public int CreateNewScript(){
            int idGen = ScriptData.Count;
            ScriptData.Add(new Script());
            ComboBoxHelper.Instance.ResetTypeObj( eSlotAonTypeObj.Script);
            // ComboBoxHelper.Instance.ResetDataScriptList();
            return idGen;
        }

        public int CreateNewNPC(){
            int idGen = NPCData.Count;
            NPCData.Add(new NPC());
            // ComboBoxHelper.Instance.ResetDataNPCList();
            ComboBoxHelper.Instance.ResetTypeObj( eSlotAonTypeObj.Person);
            return idGen;
        }

	 	public void CopyData (AutoTileMapSerializeData mapData)
		{
			Metadata = mapData.Metadata; // 1
			TileMapWidth = mapData.TileMapWidth; // 2
			TileMapHeight = mapData.TileMapHeight; // 3
            StartX = mapData.StartX;
            StartY = mapData.StartY;
			TileData = mapData.TileData; // 4
            WarpsData = mapData.WarpsData; // 5
            SignpostsData = mapData.SignpostsData; // 6
            ScriptData = mapData.ScriptData; // 7
            TriggerLink = mapData.TriggerLink; // 8
            OverlayLink = mapData.OverlayLink; // 9
            TriggerLink_C = mapData.TriggerLink_C; // 10
            OverlayLink_C = mapData.OverlayLink_C; // 11
            HouseData = mapData.HouseData; // 12
            High = mapData.High; // 13
            High_C = mapData.High_C; // 14
            NPCData = mapData.NPCData; // 15
            OverlayRotate = mapData.OverlayRotate; // 16
            OverlayRotate_C = mapData.OverlayRotate_C; // 17
            FlagMap = mapData.FlagMap; // 18
            // RawFlagMap = mapData.RawFlagMap; // 19
            ListFlagAction = mapData.ListFlagAction; // 20
            RawFlagAction = mapData.RawFlagAction; // 21
		}

        public void ClearData(){
            // TileData.Clear();
            StartX = TileMapWidth / 2;
            StartY = TileMapHeight / 2;
            TriggerLink = null;
            WarpsData.Clear();
            SignpostsData.Clear();
            NPCData.Clear();
            ScriptData.Clear();
            OverlayLink = null;
            HouseData.Clear();
            High = null;
            FlagMap = new Flags();
            // RawFlagMap = new SerializableFlag();
            ListFlagAction = new List<FlagAction>();
            RawFlagAction = new List<FlagAction.SerializableFlagAction>();
        }

        /// <summary>
        /// Save the map configuration
        /// </summary>
        /// <param name="_autoTileMap"></param>
        /// <returns></returns>
		public bool SaveData( AutoTileMap _autoTileMap)
		{
            int width = TileMapWidth;
            int height = TileMapHeight;
            // avoid clear map data when auto tile map is not initialized
			if( !_autoTileMap.IsInitialized )
			{
				//Debug.LogError(" Error saving data. Autotilemap is not initialized! Map will not be saved. ");
				return false;
			}

            Metadata.version = MetadataChunk.k_version;
			
			TileData.Clear();
            // TriggerLink.Clear();
            // WarpsData.Clear();
            // SignpostsData.Clear();
            // ScriptData.Clear();

            bool isEnableCompression = true;
			for( int iLayer = 0; iLayer < _autoTileMap.GetLayerCount(); ++iLayer )
			{
                AutoTileMap.MapLayer mapLayer = _autoTileMap.MapLayers[iLayer];
                List<int> tileData = new List<int>(width * height);
                int iTileRepetition = 0;
                int savedTileId = 0;
                int mapWidth = _autoTileMap.MapTileWidth;
                int mapHeight = _autoTileMap.MapTileHeight;
                for (int tile_y = 0; tile_y < height; ++tile_y)
				{
                    for (int tile_x = 0; tile_x < width; ++tile_x)
					{
                        int iType = -1;
                        if (tile_x < mapWidth && tile_y < mapHeight)
                        {
                            // AutoTile autoTile = _autoTileMap.TileLayers[_autoTileMap.MapLayers[iLayer].TileLayerIdx][tile_x + tile_y * mapWidth];
                            AutoTile autoTile = _autoTileMap.TileLayers[iLayer][tile_x , tile_y];
                            iType = autoTile != null? autoTile.Id : -1;
                        }
                        
                        if(isEnableCompression){
                            if( iTileRepetition == 0 )
                            {
                                savedTileId = iType;
                                iTileRepetition = 1;
                            }
                            else
                            {
                                // compression data. All tiles of the same type are store with number of repetitions ( negative number ) and type
                                // ex: 5|5|5|5 --> |-4|5| (4 times 5) ex: -1|-1|-1 --> |-3|-1| ( 3 times -1 )
                                if( iType == savedTileId ) ++iTileRepetition;
                                else
                                {
                                    if( iTileRepetition > 1 )
                                    {
                                        tileData.Add( -iTileRepetition ); // save number of repetition with negative sign
                                    }
                                    if( savedTileId < -1 )
                                    {
                                        Debug.LogError(" Wrong tile id found when compressing the tile layer " + mapLayer.Name);
                                        savedTileId = -1;
                                    }
                                    tileData.Add( savedTileId );
                                    savedTileId = iType;
                                    iTileRepetition = 1;
                                }
                            }
                        }else{
                            tileData.Add( iType ); // save number of repetition with negative sign
                        }
					}
				}
                if(isEnableCompression){
                    // save last tile type found
                    if( iTileRepetition > 1 )
                    {
                        tileData.Add( -iTileRepetition );
                    }
                    tileData.Add( savedTileId );
                }
				// 
                TileData.Add(new TileLayer() 
                { 
                    Tiles = tileData, 
                    Depth = mapLayer.Depth, 
                    LayerType = mapLayer.LayerType, 
                    SortingLayer = mapLayer.SortingLayer,
                    SortingOrder = mapLayer.SortingOrder,
                    Name = mapLayer.Name, 
                    Visible = mapLayer.Visible 
                });
			}
            TileMapWidth = width;
            TileMapHeight = height;

            //Compression Data
            OverlayLink_C = CreateCompressionArray(OverlayLink);
            TriggerLink_C = CreateCompressionArray(TriggerLink);
            High_C = CreateCompressionArray(High);
            OverlayRotate_C = CreateCompressionArray(OverlayRotate);
            // RawFlagMap.Data = FlagMap;
            RawFlagAction = new List<FlagAction.SerializableFlagAction>(ListFlagAction.Count);
			for (int i = 0; i < ListFlagAction.Count; i++)
			{
				var r = new FlagAction.SerializableFlagAction();
				r.FlagAction = ListFlagAction[i];
				RawFlagAction.Add(r);
			}
            Debug.Log("Save with Compression");
			return true;
		}

        /// <summary>
        /// Get this object serialized as an xml string
        /// </summary>
        /// <returns></returns>
		// public string GetXmlString()
		// {
		// 	return UtilsSerialize.Serialize<AutoTileMapSerializeData>(this);
		// }

        /// <summary>
        /// Save this object serialized in an xml file
        /// </summary>
        /// <param name="_filePath"></param>
		public void SaveToFile(string _filePath)
		{
            #if false //XML
			var serializer = new XmlSerializer(typeof(AutoTileMapSerializeData));
			var stream = new FileStream(_filePath, FileMode.Create);
			serializer.Serialize(stream, this);
			stream.Close();
            #else //JSON
            // string json = JsonUtility.ToJson(this, true);
            string json = UtilsAON.SerializeObject(this);
            File.WriteAllText(_filePath, json);
            #endif
		}

        /// <summary>
        /// Create map serialized data from xml file
        /// </summary>
        /// <param name="_filePath"></param>
        /// <returns></returns>
		public static AutoTileMapSerializeData LoadFromFile(string _filePath)
		{
            #if false //XML
			var serializer = new XmlSerializer(typeof(AutoTileMapSerializeData));
			var stream = new FileStream(_filePath, FileMode.Open);
			var obj = serializer.Deserialize(stream) as AutoTileMapSerializeData;
			stream.Close();
			return obj;
            #else // JSON
            // var obj = JsonUtility.FromJson<AutoTileMapSerializeData>(File.ReadAllText(_filePath));
            AutoTileMapSerializeData mapData = UtilsAON.DeserializeObject<AutoTileMapSerializeData>(File.ReadAllText(_filePath));
            return mapData;
            #endif
		}

        public void LoadFromCompression()
        {
            //Compression Data
            int count = TileMapWidth * TileMapHeight;
            OverlayLink = LoadFromCompressionArray(OverlayLink_C, TileMapWidth, TileMapHeight, -1);
            TriggerLink = LoadFromCompressionArray(TriggerLink_C, TileMapWidth, TileMapHeight, -1);
            High = LoadFromCompressionArray(High_C, TileMapWidth, TileMapHeight, 0);
            OverlayRotate = LoadFromCompressionArray(OverlayRotate_C, TileMapWidth, TileMapHeight, 0);
            // FlagMap = RawFlagMap.Data;
            if(RawFlagAction != null){
				ListFlagAction = new List<FlagAction>(RawFlagAction.Count);
				for (int i = 0; i < RawFlagAction.Count; i++)
				{
					ListFlagAction.Add(RawFlagAction[i].FlagAction);
				}
			}
            Debug.Log("Load with Compression");
        }

        /*
        private static void CheckInitArray( ref int[,] array, int w, int h, int defause){
            if(array == null){
                array = new int[w, h];
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        array[x,y] = defause;
                    }   
                }
            }
        }
        */

        /*
        private List<int> CreateCompression(List<int> real){
            if(real == null || real.Count == 0){
                List<int> r = new List<int>();
                return r;
            }
            List<int> tileData = new List<int>(real.Count);
            int iTileRepetition = 0;
            int savedTileId = 0;
            foreach( int v in real) {
                int iType = v;
                if( iTileRepetition == 0 )
                {
                    savedTileId = iType;
                    iTileRepetition = 1;
                }
                else
                {
                    // ex: 5|5|5|5 --> |-4|5| (4 times 5) ex: -1|-1|-1 --> |-3|-1| ( 3 times -1 )
                    if( iType == savedTileId ) ++iTileRepetition;
                    else
                    {
                        if(iTileRepetition == 1 && savedTileId >= 0){
                            tileData.Add( savedTileId );
                        }else
                        {
                            tileData.Add( -iTileRepetition );
                            tileData.Add( savedTileId );
                        }
                        savedTileId = iType;
                        iTileRepetition = 1;
                    }
                }
            }
            if( iTileRepetition > 0 ) {
                if(iTileRepetition == 1 && savedTileId >= 0){
                    tileData.Add( savedTileId );
                }else{
                    tileData.Add( -iTileRepetition );
                    tileData.Add( savedTileId );
                }
            }
            return tileData;
        }
        */

        private static List<int> CreateCompressionArray(int[,] array){
            if(array == null){
                List<int> r = new List<int>();
                return r;
            }
            int w = array.GetLength(0);
            int h = array.GetLength(1);
            int total = w * h;
            int[] real = new int[total];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int idx = y * w + x;
                    real[idx] = array[x,y];
                }
            }
            List<int> tileData = new List<int>();
            int iTileRepetition = 0;
            int savedTileId = 0;
            for (int i = 0; i < total; i++)
            {
                int iType = real[i];
                if( iTileRepetition == 0 )
                {
                    savedTileId = iType;
                    iTileRepetition = 1;
                }
                else
                {
                    // ex: 5|5|5|5 --> |-4|5| (4 times 5) ex: -1|-1|-1 --> |-3|-1| ( 3 times -1 )
                    if( iType == savedTileId ) ++iTileRepetition;
                    else
                    {
                        if(iTileRepetition == 1 && savedTileId >= 0){
                            tileData.Add( savedTileId );
                        }else
                        {
                            tileData.Add( -iTileRepetition );
                            tileData.Add( savedTileId );
                        }
                        savedTileId = iType;
                        iTileRepetition = 1;
                    }
                }
            }
            if( iTileRepetition > 0 ) {
                if(iTileRepetition == 1 && savedTileId >= 0){
                    tileData.Add( savedTileId );
                }else{
                    tileData.Add( -iTileRepetition );
                    tileData.Add( savedTileId );
                }
            }
            return tileData;
        }

        /*
        private static int[] LoadFromCompression(List<int> compression, int totalMapTiles, int valueDefause){
            if(compression == null || compression.Count == 0){
                int[] result_empty = new int[totalMapTiles];
                for (int i = 0; i < totalMapTiles; i++)
                {
                    result_empty[i] = valueDefause;
                }
                return result_empty;
            }
            int[] real = new int[totalMapTiles];
            int count = 0;
            for( int i = 0; i < compression.Count; i++){
                int iType = compression[i];
                if (iType <= -1 && i < compression.Count - 1)
                {
                    int iTileRepetition = -iType;
                    i++;
                    iType = compression[i];
                    for (; iTileRepetition > 0; --iTileRepetition)
                    {
                        if(count >= totalMapTiles){
                            return real;   
                        }
                        real[count] = iType;
                        count++;
                    }
                }else{
                    if(count >= totalMapTiles){
                        return real;   
                    }
                    real[count] = iType;
                    count++;
                }
            }
            if(count < totalMapTiles){
                for (; count < totalMapTiles; count++)
                {
                    real[count] = valueDefause;
                }
            }
            return real;
        }
        */

        private static int[,] LoadFromCompressionArray(List<int> compression, int w, int h, int valueDefause){
            if(compression == null || compression.Count == 0){
                int[,] result_empty = new int[w, h];
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        result_empty[x, y] = valueDefause;
                    }
                }
                return result_empty;
            }
            int totalMapTiles = w * h;
            int[] real = new int[totalMapTiles];
            int count = 0;
            for( int i = 0; i < compression.Count; i++){
                int iType = compression[i];
                if (iType <= -1 && i < compression.Count - 1)
                {
                    int iTileRepetition = -iType;
                    i++;
                    iType = compression[i];
                    for (; iTileRepetition > 0; --iTileRepetition)
                    {
                        if(count < totalMapTiles){
                            real[count] = iType;
                            count++;
                        }
                    }
                }else{
                    if(count < totalMapTiles){
                        real[count] = iType;
                        count++;
                    }
                }
            }
            if(count < totalMapTiles){
                for (; count < totalMapTiles; count++)
                {
                    real[count] = valueDefause;
                    count++;
                }
            }
            int[,] result = new int[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int idx = y * w + x;
                    result[x, y] = real[idx];
                }
            }
            return result;
        }
	}
}
