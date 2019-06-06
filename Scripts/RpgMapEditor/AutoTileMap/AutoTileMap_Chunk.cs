using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AON.RpgMapEditor
{
	public class AutoTileMap_Chunk : AutoTileMap_Base {

		// [Serializable]
        public class MapLayer
        {
            public bool Visible = true;
            public string Name = "layer";
            public eSlotAonTypeLayer LayerType = eSlotAonTypeLayer.Ground;
            public string SortingLayer = "Default";
            public int SortingOrder = 0;
            public float Depth = 0;
            /// <summary>
            /// Index of TileLayers with tiles of this layer. Used only to be able to rearrange elements using ReorderableList in AutoTileMapEditor.
            /// </summary>
            public int TileLayerIdx = -1;
        }

		[NonSerialized]
        private TileChunkPool _tileChunkPoolNode;
        public TileChunkPool TileChunkPoolNode
        {
            get
            {
                return _tileChunkPoolNode;
            }
        }

		public TileChunkPool InitChunk()
		{
			string nodeName = name + "_" + MapIdxSelect;
			GameObject obj = GameObject.Find(nodeName);
			if (obj == null) obj = new GameObject();
			obj.name = nodeName;
			_tileChunkPoolNode = obj.AddComponent<TileChunkPool>();
			return _tileChunkPoolNode;
		}

		public void DestroyChunk()
        {
            if (_tileChunkPoolNode != null)
            {
                _tileChunkPoolNode.name = "";
                DestroyImmediate(_tileChunkPoolNode.gameObject);
                _tileChunkPoolNode = null;
            }
        }

		void Update()
        {
            if (!IsInitialized)
            {
                return;
            }
            UpdateChunks();
        }

		public void UpdateChunks()
        {
            if (_tileChunkPoolNode != null)
            {
                _tileChunkPoolNode.UpdateChunks();
            }
        }

		public void MarkLayerChunksForUpdate(int tileLayerIdx)
        {
            _tileChunkPoolNode.MarkLayerChunksForUpdate(tileLayerIdx);
        }

		protected void SetLightActive(bool b){
            WindowLight[] a = _tileChunkPoolNode.GetComponentsInChildren<WindowLight>();
            for (int i = 0; i < a.Length; i++)
            {
                a[i].SetLightActive(b);
            }
        }

		// AutoTile
		public const int k_emptyTileId = -1;
        public const int k_outofboundsTileId = -2;
		public List<MapLayer> MapLayers = null;
        public List<AutoTile[,]> TileLayers;
		private bool m_isPlayMode;
        public bool IsPlayMode
        {
            get { 
				return m_isPlayMode; 
			}
			set{
				m_isPlayMode = value;
			}
        }

		protected void InitMapLayers(){
			MapLayers = new List<MapLayer>();
			TileLayers = new List<AutoTile[,]>();
		}

		public bool IsInitialized { get { return TileLayers != null && TileLayers.Count > 0; } }

		public AutoTile GetAutoTile(int gridX, int gridY, int iLayer)
        {
            if (IsValidAutoTilePos(gridX, gridY) && iLayer < MapLayers.Count)
            {
                // AutoTile autoTile = TileLayers[MapLayers[iLayer].TileLayerIdx][gridX + gridY * MapTileWidth];
                AutoTile autoTile = TileLayers[iLayer][gridX , gridY];
                if (autoTile != null)
                {
                    return autoTile;
                }
                return new AutoTile()
                {
                    Id = k_emptyTileId,
                    TileX = gridX,
                    TileY = gridY,
                    Layer = iLayer
                };
            }
            return new AutoTile()
            {
                Id = k_outofboundsTileId,
                TileX = gridX,
                TileY = gridY,
                Layer = iLayer
            };
        }

		public int GetLayerCount()
        {
            return MapLayers.Count;
        }

		public void ClearLayer(MapLayer mapLayer)
        {
            for (int i = 0; i < TileLayers[mapLayer.TileLayerIdx].Length; ++i)
            {
                for (int gridX = 0; gridX < MapTileWidth; gridX++)
                {
                    for (int gridY = 0; gridY < MapTileHeight; gridY++)
                    {
                        AutoTile autoTile = TileLayers[mapLayer.TileLayerIdx][ gridX, gridY];
                        if (autoTile != null)
                        {
                            autoTile.Id = -1;
                        }
                    }
                }
            }
        }

		public void CreateChunkLayersData()
        {
            if (MapLayers.Count > 0)
            {
                TileChunkPoolNode.InitLayers();
                TileChunkPoolNode.UpdateLayersData( m_isPlayMode);
            }
        }

		public void MarkLayerChunksForUpdate(MapLayer mapLayer)
        {
            MarkLayerChunksForUpdate(mapLayer.TileLayerIdx);
        }
        
        public void ResetTileChunkPool()
        {
            foreach (MapLayer mapLayer in MapLayers)
            {
                MarkLayerChunksForUpdate(mapLayer);
            }
        }

		public bool SetAutoTile(int gridX, int gridY, int tileId, int iLayer, bool checkRefreshTile)
        {
            if (!IsValidAutoTilePos(gridX, gridY) || iLayer >= MapLayers.Count)
            {
                return false;
            }

            bool tileHasChange = false;
            {
                // int idx = gridX + gridY * MapTileWidth;
                // AutoTile autoTile = TileLayers[MapLayers[iLayer].TileLayerIdx][idx];
                AutoTile autoTile = TileLayers[iLayer][gridX, gridY];
                if (autoTile == null)
                {
                    autoTile = new AutoTile();
                    // TileLayers[MapLayers[iLayer].TileLayerIdx][idx] = autoTile;
                    TileLayers[iLayer][gridX, gridY] = autoTile;
                }
                tileHasChange = autoTile.Id != tileId;
                autoTile.Id = tileId;
                autoTile.TileX = gridX;
                autoTile.TileY = gridY;
                autoTile.Layer = iLayer;
                if (checkRefreshTile && tileHasChange)
                {
                    for (int xf = -1; xf < 2; ++xf)
                    {
                        for (int yf = -1; yf < 2; ++yf)
                        {
                            RefreshTile(gridX + xf, gridY + yf, iLayer);
                        }
                    }
                }
            }
            return tileHasChange;
        }
        
        public void RefreshAllTiles()
        {
            TileChunkPoolNode.MarkUpdatedAllTile();
        }

		public void RefreshTile(int gridX, int gridY, int iLayer)
        {
            AutoTile autoTile = GetAutoTile(gridX, gridY, iLayer);
            RefreshTile(autoTile);
        }

		public void RefreshTile(AutoTile autoTile)
        {
            if (autoTile == null) return;
            TileChunkPoolNode.MarkUpdatedTile(autoTile.TileX, autoTile.TileY, autoTile.Layer);
        }
        // Create Door, NPC
        public IEnumerator RefreshTileTrigger()
        {
            if (IsPlayMode)
            {
                TileChunkPoolNode.RefreshTileTrigger();
            }
            yield break;
        }

		// ADDON MAP
		private int[,] mapHighWorldX2 = null;
        private int w2;
        private int h2;
        public int getHighX2(int x, int y)
        {
            if (x < 0 || x >= mapHighWorldX2.GetLength(0))
            {
                return -1;
            }
            if (y < 0 || y >= mapHighWorldX2.GetLength(1))
            {
                return -1;
            }
            return mapHighWorldX2[x, y];
        }
        public void setHighX2(int x, int y, int v)
        {
            if (x < 0 || x >= mapHighWorldX2.GetLength(0))
            {
                return;
            }
            if (y < 0 || y >= mapHighWorldX2.GetLength(1))
            {
                return;
            }
            mapHighWorldX2[x, y] = v;
        }

        protected void LoadHigh()
        {
            int mapWidth = MapTileWidth;
            int mapHeight = MapTileHeight;
            w2 = (mapWidth) * 2;
            h2 = (mapHeight) * 2;
            mapHighWorldX2 = new int[w2, h2];
            for (int tx = 0; tx < mapWidth; ++tx)
            {
                for (int ty = 0; ty < mapHeight; ++ty)
                {
                    // MapSelect.GetHighRef(tx, ty);
                    bool isHasPaint = GetAutoTile(tx, ty, 0).Id >= 0;
                    int h = -1;
                    if (isHasPaint)
                    {
                        h = MapSelect.GetHighRef(tx, ty);
                        if (h < 0)
                        {
                            h = 0;
                        }
                    }
                    int tx2 = tx * 2;
                    int ty2 = ty * 2;
                    mapHighWorldX2[tx2, ty2] = h;
                    mapHighWorldX2[tx2 + 1, ty2] = h;
                    mapHighWorldX2[tx2, ty2 + 1] = h;
                    mapHighWorldX2[tx2 + 1, ty2 + 1] = h;
                }
            }
            if (true)
            {//Noi suy
                int offset = 0;
                for (int h = 8; h >= 2; h--)
                {
                    for (int x = offset; x < w2 - offset; x++)
                    {
                        for (int y = offset; y < h2 - offset; y++)
                        {
                            int v = getHighX2(x, y);
                            if (v == h)
                            {
                                {
                                    //[4] 4 2 2 0 0 -> [4] 4 3 2 1 0
                                    int[] a = { v, v - 2, v - 2, v - 4, v - 4 };
                                    SetHighHelp(x, y, a, 2, v - 1, 4, v - 3);
                                }
                                // int v_low_1 = v - 1;
                                // int v_low_2 = v - 2;
                                // int v_low_3 = v - 3;
                                // int v_low_4 = v - 4;
                                // Case [4] 2 2 0 0 -> [4] 3 2 0 0
                                // SetHighHelp(x, y, v - 2, v - 2, v - 4, v - 4, 1, v - 1);
                                // Case 4 3 1 1 -> 4 3 2 1
                                // SetHighHelp(x, y, v - 1, v - 3, v - 3, 2, v - 2);
                                // Case [2] 1 1 0 -> [2] 1 0 0
                                // SetHighHelp(x, y, v - 2, v - 2, v - 2, 1, v - 1);

                                //Fix BR
                                // FixOffset( x, y, 1, 1, v);
                                // FixOffset( x, y, -1, 1, v);
                                // FixOffset( x, y, 1, -1, v);
                                // FixOffset( x, y, -1, -1, v);

                                /*
                                bool isX0Y0 = (getHighX2( x - 1, y) == v_half
                                            && getHighX2( x, y - 1) == v_half
                                            && getHighX2( x - 1, y - 1) == v_low);
                                bool isX0Y1 = (getHighX2( x - 1, y) == v_half
                                            && getHighX2( x, y + 1) == v_half
                                            && getHighX2( x - 1, y + 1) == v_low);
                                bool isX1Y0 = (getHighX2( x + 1, y) == v_half
                                            && getHighX2( x, y - 1) == v_half
                                            && getHighX2( x + 1, y - 1) == v_low);
                                bool isX1Y1 = (getHighX2( x + 1, y) == v_half
                                            && getHighX2( x, y + 1) == v_half
                                            && getHighX2( x + 1, y + 1) == v_low);
                                if(isX0Y0){
                                    setHighX2(x - 1, y - 1, v_half);
                                }
                                if(isX0Y1){
                                    setHighX2(x - 1, y + 1, v_half);
                                }
                                if(isX1Y0){
                                    setHighX2(x + 1, y - 1, v_half);
                                }
                                if(isX1Y1){
                                    setHighX2(x + 1, y + 1, v_half);
                                }
                                */
                                //Check DOUBLE_EXT_CORNER
                                // {
                                //     CheckDOUBLE_EXT_CORNER( NS, x, y, mapHighWorldX2);
                                // }
                            }
                        }
                    }
                }
                /*
                for(int NS = 8; NS >= 1; NS--){
                    for ( int x = 1; x < w2 - 1; x++ ){
                        for ( int y = 1; y < h2 - 1; y++ ){
                            int v = mapHighWorldX2[ x, y];
                            if(v == NS){
                                CheckDOUBLE_EXT_CORNER( NS, x, y, mapHighWorldX2);
                            }
                        }
                    }
                }
                */
            }
            //Fill AB
            // mapAB_X_WorldX2 = new int[w2 ,h2];
            // for ( int y = 0; y < h2; y++ ){
            //     int lastX = 0;
            //     int lastValue = mapHighWorldX2[ lastX, y];
            //     for ( int x = 0; x < w2; x++ ){
            //         if(lastValue == mapHighWorldX2[x, y]){
            //             mapAB_X_WorldX2[x, y] = lastX;
            //         }else{
            //             lastX = x;
            //             lastValue = mapHighWorldX2[x, y];
            //             mapAB_X_WorldX2[x, y] = lastX;
            //         }
            //     }
            // }
        }

        //
        private void SetHighHelp(int x, int y, int[] a, int offset, int v, int offset2, int v2)
        {
            if (getHighX2(x + 1, y) == a[0]
                && getHighX2(x + 2, y) == a[1]
                && getHighX2(x + 3, y) == a[2]
                && getHighX2(x + 4, y) == a[3]
                && getHighX2(x + 5, y) == a[4])
            {
                setHighX2(x + offset, y, v);
                if (offset2 > 0)
                    setHighX2(x + offset2, y, v2);
            }

            if (getHighX2(x - 1, y) == a[0]
                && getHighX2(x - 2, y) == a[1]
                && getHighX2(x - 3, y) == a[2]
                && getHighX2(x - 4, y) == a[3]
                && getHighX2(x - 5, y) == a[4])
            {
                setHighX2(x - offset, y, v);
                if (offset2 > 0)
                    setHighX2(x - offset2, y, v2);
            }

            if (getHighX2(x, y + 1) == a[0]
                && getHighX2(x, y + 2) == a[1]
                && getHighX2(x, y + 3) == a[2]
                && getHighX2(x, y + 4) == a[3]
                && getHighX2(x, y + 5) == a[4])
            {
                setHighX2(x, y + offset, v);
                if (offset2 > 0)
                    setHighX2(x, y + offset2, v2);
            }

            if (getHighX2(x, y - 1) == a[0]
                && getHighX2(x, y - 2) == a[1]
                && getHighX2(x, y - 3) == a[2]
                && getHighX2(x, y - 4) == a[3]
                && getHighX2(x, y - 5) == a[4])
            {
                setHighX2(x, y - offset, v);
                if (offset2 > 0)
                    setHighX2(x, y - offset2, v2);
            }
        }

        // private void FixOffset(int x, int y, int offsetX, int offsetY, int v){
        //     if(
        //     getHighX2( x + offsetX, y + offsetY) == v
        //     && getHighX2( x + offsetX, y) == v
        //     && getHighX2( x, y + offsetY) == v

        //     && getHighX2( x + offsetX + offsetX, y) == v - 2
        //     && getHighX2( x + offsetX + offsetX, y + offsetY) == v - 2

        //     && getHighX2( x + offsetX + offsetX, y + offsetY + offsetY) == v - 2

        //     && getHighX2( x + offsetX, y + offsetY + offsetY) == v - 2
        //     && getHighX2( x + offsetX, y + offsetX) == v - 2
        //     ){
        //         setHighX2(x + offsetX, y, v - 1);
        //         setHighX2(x , y + offsetY, v - 1);
        //         setHighX2(x + offsetX, y + offsetY, v - 1);
        //     }
        // }

        private bool[,] mapWaterInGame = null;
        public bool IsWaterInGame(int x, int y)
        {
            if (x < 0 || x >= mapWaterInGame.GetLength(0))
            {
                return false;
            }
            if (y < 0 || y >= mapWaterInGame.GetLength(1))
            {
                return false;
            }
            return mapWaterInGame[x, y];
        }

        private void SetWaterInGame(int x, int y, bool b)
        {
            if (x < 0 || x >= mapWaterInGame.GetLength(0))
            {
                return;
            }
            if (y < 0 || y >= mapWaterInGame.GetLength(1))
            {
                return;
            }
            mapWaterInGame[x, y] = b;
        }

        protected void LoadWater()
        {
            mapWaterInGame = new bool[MapTileWidth, MapTileHeight];
            for (int tx = 0; tx < MapTileWidth; ++tx)
            {
                for (int ty = 0; ty < MapTileHeight; ++ty)
                {
                    // MapSelect.GetHighRef(tx, ty);
                    if (GetAutoTile(tx, ty, 0).Id == DefineAON.IdSlot_Water)
                    {
                        SetWaterInGame(tx, ty, true);
                        bool isTop = GetAutoTile(tx, ty + 1, 0).Id >= 0;
                        bool isBot = GetAutoTile(tx, ty - 1, 0).Id >= 0;
                        bool isRight = GetAutoTile(tx + 1, ty, 0).Id >= 0;
                        bool isLeft = GetAutoTile(tx - 1, ty, 0).Id >= 0;
                        if (isRight)
                        {
                            SetWaterInGame(tx + 1, ty, true);
                        }
                        if (isLeft)
                        {
                            SetWaterInGame(tx - 1, ty, true);
                        }
                        if (isTop)
                        {
                            SetWaterInGame(tx, ty + 1, true);
                        }
                        if (isBot)
                        {
                            SetWaterInGame(tx, ty - 1, true);
                        }
                        bool isTR = isTop && isRight;
                        bool isTL = isTop && isLeft;
                        bool isBR = isBot && isRight;
                        bool isBL = isBot && isLeft;
                        if (isTR)
                        {
                            SetWaterInGame(tx + 1, ty + 1, true);
                        }
                        if (isTL)
                        {
                            SetWaterInGame(tx - 1, ty + 1, true);
                        }
                        if (isBR)
                        {
                            SetWaterInGame(tx + 1, ty - 1, true);
                        }
                        if (isBL)
                        {
                            SetWaterInGame(tx - 1, ty - 1, true);
                        }
                    }
                }
            }
        }

        private void CheckDOUBLE_EXT_CORNER(int h, int x, int y, int[,] mapHighWorldX2)
        {
            if (mapHighWorldX2[x + 1, y] >= h
                && mapHighWorldX2[x - 1, y] >= h
                && mapHighWorldX2[x, y + 1] >= h
                && mapHighWorldX2[x, y - 1] >= h)
            {
                bool TR = mapHighWorldX2[x + 1, y - 1] >= h;
                bool TL = mapHighWorldX2[x - 1, y - 1] >= h;
                bool BR = mapHighWorldX2[x + 1, y + 1] >= h;
                bool BL = mapHighWorldX2[x - 1, y + 1] >= h;
                int count = 0;
                count += TR ? 1 : 0;
                count += TL ? 1 : 0;
                count += BR ? 1 : 0;
                count += BL ? 1 : 0;
                if (count == 2)
                {
                    if (mapHighWorldX2[x + 1, y - 1] < h)
                    {
                        mapHighWorldX2[x + 1, y - 1] = h;
                    }
                    if (mapHighWorldX2[x - 1, y - 1] < h)
                    {
                        mapHighWorldX2[x - 1, y - 1] = h;
                    }
                    if (mapHighWorldX2[x + 1, y + 1] < h)
                    {
                        mapHighWorldX2[x + 1, y + 1] = h;
                    }
                    if (mapHighWorldX2[x - 1, y + 1] < h)
                    {
                        mapHighWorldX2[x - 1, y + 1] = h;
                    }
                }
            }
        }

		public void ResetTerrain()
        {
            if (MapLayers != null)
            {
                int x0 = MapTileWidth / 2 - 4;
                int x1 = MapTileWidth - x0;
                int y0 = MapTileHeight / 2 - 4;
                int y1 = MapTileHeight - y0;
                foreach (MapLayer mapLayer in MapLayers)
                {
                    if (mapLayer.LayerType == eSlotAonTypeLayer.Ground)
                    {
                        for (int gridX = 0; gridX < MapTileWidth; gridX++)
                        {
                            for (int gridY = 0; gridY < MapTileHeight; gridY++)
                            {
                                int idDefause = -1;
                                if (gridX >= x0 && gridX <= x1 && gridY >= y0 && gridY <= y1)
                                {
                                    idDefause = 0;
                                }
                                TileLayers[mapLayer.TileLayerIdx][gridX , gridY] = new AutoTile()
                                {
                                    Id = idDefause,
                                    TileX = gridX,
                                    TileY = gridY
                                };
                            }
                        }
                    }
                    else
                    {
                        for (int gridX = 0; gridX < MapTileWidth; gridX++)
                        {
                            for (int gridY = 0; gridY < MapTileHeight; gridY++)
                            {
                                TileLayers[mapLayer.TileLayerIdx][gridX , gridY] = null;
                            }
                        }
                    }
                }
            }
        }

        public bool IsEmptyTerrain()
        {
            if (MapLayers != null)
            {
                int idDefause = -1;
                foreach (MapLayer mapLayer in MapLayers)
                {
                    if (mapLayer.LayerType == eSlotAonTypeLayer.Ground)
                    {
                        for (int gridX = 0; gridX < MapTileWidth; gridX++)
                        {
                            for (int gridY = 0; gridY < MapTileHeight; gridY++)
                            {
                                var t = TileLayers[mapLayer.TileLayerIdx][gridX, gridY];
                                if (t != null && t.Id != idDefause)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    else if(mapLayer.LayerType == eSlotAonTypeLayer.Overlay)
                    {
                        for (int gridX = 0; gridX < MapTileWidth; gridX++)
                        {
                            for (int gridY = 0; gridY < MapTileHeight; gridY++)
                            {
                                var t = TileLayers[mapLayer.TileLayerIdx][gridX, gridY];
                                if (t != null && t.Id != -1)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
	}
}