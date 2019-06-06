using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AON.RpgMapEditor
{
	public class AutoTileMap_Base : MonoBehaviour {

		private Vector2 m_cellSize = new Vector2(1, 1);
        public Vector2 CellSize
        {
            get
            {
                return m_cellSize;
            }
			set{
				m_cellSize = value;	
			}
        }

		protected AutoTileMapData m_mapData;
        public AutoTileMapData MapsData
        {
            get
            {
                return m_mapData;
            }
        }

        protected int m_mapIndex = 0;
        public int MapIdxSelect
        {
            get
            {
                return m_mapIndex;
            }
        }

		public AutoTileMapSerializeData MapSelect
        {
            get
            {
                return m_mapData.Maps[MapIdxSelect];
            }
        }

		public bool IsValidAutoTilePos(int gridX, int gridY)
        {
            return !(gridX < 0 || gridX >= MapSelect.TileMapWidth || gridY < 0 || gridY >= MapSelect.TileMapHeight);
        }

		public virtual void LoadMapsData( AutoTileMapData mapData, int mapIdx){
            m_mapData = mapData;
            m_mapIndex = mapIdx;
        }

        public int MapTileWidth { 
            get {
                return MapSelect.TileMapWidth; 
            } 
        }

        public int MapTileHeight { 
            get {
                return MapSelect.TileMapHeight; 
            } 
        }
	}
}