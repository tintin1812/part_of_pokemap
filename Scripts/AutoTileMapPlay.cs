using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System;
using AON.RpgMapEditor;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AutoTileMapPlay : MonoBehaviour {
	#if false
        public const int k_emptyTileId = -1;
        public const int k_outofboundsTileId = -2;

        public delegate void OnMapLoadedDelegate( AutoTileMapPlay autoTileMap );
        /// <summary>
        /// Called when map has been loaded
        /// </summary>
        public OnMapLoadedDelegate OnMapLoaded;

        [Serializable]
        public class MapLayer
        {
            public bool Visible = true;
            public string Name = "layer";
            public eSlotAonType LayerType = eSlotAonType.Ground;
            public string SortingLayer = "Default";
            public int SortingOrder = 0;
            public float Depth = 0;
            /// <summary>
            /// Index of TileLayers with tiles of this layer. Used only to be able to rearrange elements using ReorderableList in AutoTileMapEditor.
            /// </summary>
            public int TileLayerIdx = -1;
        }

        [SerializeField]
        AutoTileset m_autoTileset;
        /// <summary>
        /// Tileset used by this map to draw the tiles
        /// </summary>
		public AutoTileset Tileset
        {
            get { return m_autoTileset; }
            set
            {
                bool isChanged = m_autoTileset != value;
                m_autoTileset = value;
                if (isChanged)
                {
                    LoadMap();
                }
            }
        }

		[SerializeField]
		AutoTileMapData m_mapData;
        public AutoTileMapData MapsData
        {
            get{
                return m_mapData;
            }
            set
			{
				bool isChanged = m_mapData != value;
				m_mapData = value;
				if( isChanged )
				{
					LoadMap();
				}
			}
        }
        /// <summary>
        /// Tile data for this map
        /// </summary>
		public AutoTileMapSerializeData MapSelect
		{ 
			get{ 
                if(m_mapInde >= m_mapData.Maps.Count)
                    return null;
                return m_mapData.Maps[m_mapInde];
            }
		}

        [SerializeField]
        private int m_mapInde = 0;
		public int MapIdxSelect{
			get{
				return m_mapInde;
			}
			set{
				m_mapInde = value;
			}
		}

        /// <summary>
        /// Reference to the Sprite Renderer used to draw the minimap in the editor
        /// </summary>
		// public SpriteRenderer EditorMinimapRender;

        /// <summary>
        /// Minimap texture for this map
        /// </summary>
		public Texture2D MinimapTexture{ get; private set; }

        /// <summary>
        /// Width of this map in tiles
        /// </summary>
		public int MapTileWidth		{ get{ return MapSelect != null? MapSelect.TileMapWidth : 0; } }
        /// <summary>
        /// Height of this map in tiles
        /// </summary>
		public int MapTileHeight 	{ get{ return MapSelect != null? MapSelect.TileMapHeight : 0; } }
        /// <summary>
        /// The size of the tilemap cell in units
        /// </summary>
        public Vector2 CellSize { get { return m_cellSize; } set { m_cellSize = value; } }

        /// <summary>
        /// Main camera used to view this map
        /// </summary>
		public Camera ViewCamera;

        /// <summary>
        /// Component used to edit the map on play
        /// </summary>
		// public AutoTileMapGui AutoTileMapGui{ get; private set; }

        /// <summary>
        /// Speed of animated tiles in frames per second
        /// </summary>
		public float AnimatedTileSpeed = 6f;

        /// <summary>
        /// If true, map collisions will be enabled.
        /// </summary>
		// public bool IsCollisionEnabled = true;

        /// <summary>
        /// If map has been initialized
        /// </summary>
        public bool IsInitialized { get { return TileLayers != null && TileLayers.Count > 0; } }

        /// <summary>
        /// If map is loading
        /// </summary>
        public bool IsLoading { get; private set; }

        //NOTE: can't be inside MapLayer because ReorderableList only copy serialized data, and this is not serializable. Make AutoTile will brake performance.
        public List<AutoTile[]> TileLayers; 

        /// <summary>
        /// Set map visibility
        /// </summary>
		public bool IsVisible
		{
			get{ return m_isVisible; }
			set{ m_isVisible = value;}
		}
		private bool m_isVisible = true;

        /// <summary>
        /// If true, changes made to the map in game will be applied after stop playing and going back to the editor.
        /// If you set this to true while playing and load a different scene with a different map with also this set to true, after going back to the editor,
        /// the scene map will be modified with second map. So be careful.
        /// </summary>
		public bool SaveChangesAfterPlaying = true;

        /// <summary>
        /// If true, display the tile grid when AutoTileMap object is selected in editor
        /// </summary>
        public bool ShowGrid = true;

        public List<MapLayer> MapLayers = null;

		private Texture2D m_minimapTilesTexture;
		// private float m_tileAnim3Frame = 0f;
        // private float m_tileAnim4Frame = 0f;
        private Color[] aTilesColors;
        [SerializeField]
        private Vector2 m_cellSize;
		[SerializeField]
		public TileChunkPool m_tileChunkPoolNode;

        private Texture2D[] m_thumbnailTextures; // thumbnail texture for each sub tileset
        public Texture2D[] ThumbnailTextures{ get{ return m_thumbnailTextures;}}

		void Awake()
		{
			// Destroy( transform.gameObject );
			//Orther
			IsLoading = false;

			if( CanBeInitialized() )
			{
				if( Application.isPlaying && ViewCamera && ViewCamera.name == "SceneCamera" )
				{
					ViewCamera = null;
				}
				LoadMap();
				//StartCoroutine(LoadMapAsync()); // test asynchronous load
				IsVisible = true;
			}
			else
			{
				Debug.LogWarning(" Autotilemap cannot be initialized because Tileset and/or Map Data is missing. Press create button in the inspector to create the missing part or select one.");
			}	
		}

        void OnValidate()
        {
            IsLoading = false;
        }

		void OnDisable()
		{
			if( IsInitialized && SaveChangesAfterPlaying )
			{
				SaveMap();
			}
		}

        public void DestroyChunk(){
            if( m_tileChunkPoolNode != null )
            {
                Destroy(m_tileChunkPoolNode.gameObject);
                m_tileChunkPoolNode = null;
            }
        }

		void OnDestroy()
		{
            DestroyImmediate( m_minimapTilesTexture );
            DestroyImmediate( MinimapTexture );
		}

        /// <summary>
        /// When the game object is selected this will draw the grid
        /// ref: http://wiki.unity3d.com/index.php/2D_Tilemap_Starter_Kit
        /// </summary>
        /// <remarks>Only called when in the Unity editor.</remarks>
        private void OnDrawGizmosSelected()
        {
            if (ShowGrid && Tileset != null)
            {

                // store map width, height and position
                var mapWidth = MapTileWidth * CellSize.x;
                var mapHeight = -MapTileHeight * CellSize.y;
                var position = this.transform.position;
                position.z = Camera.current.transform.position.z + 0.1f;

                // draw layer border
                Gizmos.color = Color.white;
                Gizmos.DrawLine(position, position + new Vector3(mapWidth, 0, 0));
                Gizmos.DrawLine(position, position + new Vector3(0, mapHeight, 0));
                Gizmos.DrawLine(position + new Vector3(mapWidth, 0, 0), position + new Vector3(mapWidth, mapHeight, 0));
                Gizmos.DrawLine(position + new Vector3(0, mapHeight, 0), position + new Vector3(mapWidth, mapHeight, 0));

                // draw tile cells
                Gizmos.color = Color.grey;
                for (float i = 1; i < MapTileWidth; i++)
                {
                    Gizmos.DrawLine(position + new Vector3(i * CellSize.x, 0, 0), position + new Vector3(i * CellSize.x, mapHeight, 0));
                }

                for (float i = 1; i < MapTileHeight; i++)
                {
                    Gizmos.DrawLine(position + new Vector3(0, -i * CellSize.y, 0), position + new Vector3(mapWidth, -i * CellSize.y, 0));
                }
            }
        }

        /// <summary>
        /// Update tile chunks of the map.
        /// </summary>
		public void UpdateChunks()
		{
            if(m_tileChunkPoolNode != null){
                m_tileChunkPoolNode.UpdateChunks();
            }
		}

        /// <summary>
        /// Find the last layer found of provided type or null if not found.
        /// </summary>
        /// <param name="layerType">Layer type to be found</param>
        /// <param name="startIndex">The zero-based starting index of the search</param>
        // public MapLayer FindLastLayer(eLayerType layerType, int startIndex = 0)
        // {
        //     for (int i = MapLayers.Count - 1; i >= startIndex; --i)
        //         if (MapLayers[i].LayerType == layerType) return MapLayers[i];
        //     return null;
        // }

        // public int FindLastLayerIdx(eLayerType layerType, int startIndex = 0)
        // {
        //     for (int i = MapLayers.Count - 1; i >= startIndex; --i)
        //         if (MapLayers[i].LayerType == layerType) return i;
        //     return -1;
        // }

        /// <summary>
        /// Find the first layer found of provided type or null if not found.
        /// </summary>
        /// <param name="layerType">Layer type to be found</param>
        /// <param name="startIndex">The zero-based starting index of the search</param>
        /// <returns></returns>
        // public MapLayer FindFirstLayer(eLayerType layerType, int startIndex = 0)
        // {
        //     for (int i = startIndex; i < MapLayers.Count; ++i)
        //         if (MapLayers[i].LayerType == layerType) return MapLayers[i];
        //     return null;
        // }

        // public int FindFirstLayerIdx(eLayerType layerType, int startIndex = 0)
        // {
        //     for (int i = startIndex; i < MapLayers.Count; ++i)
        //         if (MapLayers[i].LayerType == layerType) return i;
        //     return -1;
        // }

        /// <summary>
        /// Update tile chunk nodes using data from MapLayers. Call this method after changing depth and/or layer in MapLayers.
        /// </summary>
        public void UpdateChunkLayersData()
        {
            if (MapLayers.Count > 0)
            {
                m_tileChunkPoolNode.CreateLayers(MapLayers.Count);
                m_tileChunkPoolNode.UpdateLayersData();
            }
        }

        /// <summary>
        /// Mark all tilechunks of a map layer to be updated
        /// </summary>
        /// <param name="mapLayer"></param>
        public void MarkLayerChunksForUpdate( MapLayer mapLayer )
        {
            m_tileChunkPoolNode.MarkLayerChunksForUpdate(mapLayer.TileLayerIdx);
        }

        /// <summary>
        /// Reset the pool of tile chunks.
        /// </summary>
        public void ResetTileChunkPool()
        {
            // foreach (MapLayer mapLayer in MapLayers){
            //     MarkLayerChunksForUpdate(mapLayer);
            // }
            for( int i = 0; i < MapLayers.Count; i++){
                m_tileChunkPoolNode.MarkUpdatedTile( 0, 0, i);
            }
        }
        
        /// <summary>
        /// Load Map according to MapData.
        /// </summary>
		public void LoadMap()
        {
            Debug.Log("LoadMap");
            if( Tileset != null && m_cellSize == Vector2.zero)
            {
                m_cellSize = new Vector2(Tileset.TileWidth / AutoTileset.PixelToUnits, Tileset.TileHeight / AutoTileset.PixelToUnits);
            }
            if( m_tileChunkPoolNode == null )
            {
                string nodeName = name+" Data";
                GameObject obj = GameObject.Find( nodeName );
                if( obj == null ) obj = new GameObject();
                obj.name = nodeName;
                m_tileChunkPoolNode = obj.AddComponent<TileChunkPool>();
            }
            m_tileChunkPoolNode.Initialize( this );
            IEnumerator coroutine = LoadMapAsync();
            while (coroutine.MoveNext()) ;
        }

        /// <summary>
        /// Load Map asynchronously according to MapData.
        /// </summary>
		public IEnumerator LoadMapAsync()
		{
            if (IsLoading)
            {
                //Debug.LogWarning("Cannot load map while loading");
            }
            else
            {
                IsLoading = true;
                if (Tileset != null && Tileset.AtlasTexture != null)
                {

                    if (MapSelect != null)
                    {

                        IEnumerator coroutine = MapSelect.LoadToMap(this);
                        while (coroutine.MoveNext()) yield return null;

                        coroutine = m_tileChunkPoolNode.UpdateChunksAsync();
                        while (coroutine.MoveNext()) yield return null;

                        //+++ Fog Of War
                        // StopCoroutine(FogOfWarCoroutine());
                        // m_fogOfWarSetRequests.Clear();
                        // m_fogOfWarTilesToUpdate.Clear();
                        //NOTE: finds first fog of war layer, not more than one
                        // m_fogOfWarMapLayer = MapLayers.Find(x => x.LayerType == eLayerType.FogOfWar);
                        // if (m_fogOfWarMapLayer != null && Application.isPlaying)
                        // {
                            //NOTE: for some unknown reasong, starting the fog of war coroutine here, make it to be frozen
                            // when calling "yield return null" for the first time during several seconds. 
                            // This only happens the first time after clearing the fog of war layer. This trick fix the issue.
                        //     m_fogOfWarInitCoroutineOnNextUpdate = true;
                        // }
                        //---

                        //+++free unused resources
                        Resources.UnloadUnusedAssets();
                        System.GC.Collect();
                        //---
                    }
                }

                IsLoading = false;
                if (OnMapLoaded != null)
                {
                    OnMapLoaded(this);
                }
            }
            yield return null;
		}
		
        /// <summary>
        /// Save current map to MapData
        /// </summary>
        /// <returns></returns>
		public bool SaveMap( int width = -1, int height = -1 )
		{
            bool isOk = false;
            if (IsLoading)
            {
                //Debug.LogWarning("Cannot save map while loading");
            }
            else
            {
                //Debug.Log("AutoTileMap:SaveMap");
                if (width < 0) width = MapTileWidth;
                if (height < 0) height = MapTileHeight;
                isOk = MapSelect.SaveData(this, width, height);
#if UNITY_EDITOR
                EditorUtility.SetDirty(this.m_mapData);
                AssetDatabase.SaveAssets();
#endif
            }
            return isOk;
		}

        /// <summary>
        /// Display a load dialog to load a map saved as xml
        /// </summary>
        /// <returns></returns>
		public bool ShowLoadDialog()
		{
	#if UNITY_EDITOR
			string filePath = EditorUtility.OpenFilePanel( "Load tilemap",	"", "json");
			if( filePath.Length > 0 )
			{
				AutoTileMapSerializeData mapData = AutoTileMapSerializeData.LoadFromFile( filePath );
				MapSelect.CopyData( mapData );
				LoadMap();
				return true;
			}
	#else
            /*
			string xml = PlayerPrefs.GetString("XmlMapData", "");
			if( !string.IsNullOrEmpty(xml) )
			{
				AutoTileMapSerializeData mapData = AutoTileMapSerializeData.LoadFromXmlString( xml );
				MapData.Data.CopyData( mapData );
				LoadMap();
				return true;
			}
             */
	#endif
			return false;
		}

        /// <summary>
        /// Display a save dialog to save the current map in xml format
        /// </summary>
		public void ShowSaveDialog()
		{
	#if UNITY_EDITOR
			string filePath = EditorUtility.SaveFilePanel( "Save tilemap",	"",	"map" + ".json", "json");
			if( filePath.Length > 0 )
			{
				SaveMap();
				MapSelect.SaveToFile( filePath );
			}
	#else
            /*
			SaveMap();
			string xml = MapData.Data.GetXmlString( );
			PlayerPrefs.SetString("XmlMapData", xml);
            */
	#endif
		}

        /// <summary>
        /// If map can be initialized
        /// </summary>
        /// <returns></returns>
		public bool CanBeInitialized()
		{
			return Tileset != null && Tileset.AtlasTexture != null && MapSelect != null;
		}

        /// <summary>
        /// Initialize the map
        /// </summary>
		public void Initialize()
		{
			//Debug.Log("AutoTileMap:Initialize");

			if( MapSelect == null )
			{
				Debug.LogError(" AutoTileMap.Initialize called when MapData was null");
			}
			else if( Tileset == null || Tileset.AtlasTexture == null )
			{
				Debug.LogError(" AutoTileMap.Initialize called when Tileset or Tileset.TilesetsAtlasTexture was null");
			}
			else
			{
                //Set the instance if executed in editor where Awake is not called
                if( Instance == null ) Instance = this;

				Tileset.GenerateAutoTileData();

                //TODO: Allow changing minimap offset to allow minimaps smaller than map size
                int minimapWidth = Mathf.Min(MapTileWidth, 2048); //NOTE: 2048 is a maximum safe size for a texture
                int minimapHeigh = Mathf.Min(MapTileHeight, 2048);
                if( MinimapTexture != null )
                {
                    DestroyImmediate( MinimapTexture );
                }
                MinimapTexture = new Texture2D(minimapWidth, minimapHeigh);
				MinimapTexture.anisoLevel = 0;
				MinimapTexture.filterMode = FilterMode.Point;
				MinimapTexture.name = "MiniMap";
                MinimapTexture.hideFlags = HideFlags.DontSave;

                if (m_minimapTilesTexture != null)
                {
                    DestroyImmediate(m_minimapTilesTexture);
                }
				
				_GenerateMinimapTilesTexture();
				
                MapLayers = new List<MapLayer>();
                TileLayers = new List<AutoTile[]>();
				
				// AutoTileMapGui = GetComponent<AutoTileMapGui>();
			}
		}

        /// <summary>
        /// Clean all tiles of the map
        /// </summary>
		public void ClearMap()
		{
			if( MapLayers != null )
			{
                int idDefause = Tileset.SlotAONs.Count > 0 ? 0 : -1;
                foreach (MapLayer mapLayer in MapLayers)
				{
                    if(mapLayer.LayerType == eSlotAonType.Ground){
                        for ( int gridX = 0 ; gridX < MapTileWidth; gridX++){
                            for ( int gridY = 0 ; gridY < MapTileHeight; gridY++){
                                TileLayers[mapLayer.TileLayerIdx][gridX + gridY * MapTileWidth] = new AutoTile() { 
                                    Id = idDefause,
                                    TileX = gridX,
                                    TileY = gridY
                                };
                            }   
                        }
                    }else{
                        for (int i = 0; i < TileLayers[mapLayer.TileLayerIdx].Length; ++i)
                        {
                            TileLayers[mapLayer.TileLayerIdx][i] = null;
                        }
                    }
				}
                /*
                foreach (MapLayer mapLayer in MapLayers)
				{
                    for (int i = 0; i < TileLayers[mapLayer.TileLayerIdx].Length; ++i)
					{
                        TileLayers[mapLayer.TileLayerIdx][i] = null;
					}
				}
                */
			}
            // remove all tile chunks
            m_tileChunkPoolNode.Initialize(this);
		}

        /// <summary>
        /// Clear all tiles of the map layer
        /// </summary>
        /// <param name="mapLayer"></param>
        public void ClearLayer(MapLayer mapLayer)
        {
            for (int i = 0; i < TileLayers[mapLayer.TileLayerIdx].Length; ++i)
            {
                AutoTile autoTile = TileLayers[mapLayer.TileLayerIdx][i];
                if (autoTile != null)
                {
                    autoTile.Id = -1;
                }
            }
        }

		//public int _Debug_SpriteRenderCounter = 0; //debug
		// private int __prevTileAnimFrame = -1;
		void Update () 
		{
			if( !IsInitialized )
			{
				return;
			}
            if(m_tileChunkPoolNode != null){
                m_tileChunkPoolNode.UpdateChunks();
            }
		}

        /// <summary>
        /// Return the number of map layers
        /// </summary>
        /// <returns></returns>
        public int GetLayerCount()
        {
            return MapLayers.Count;
        }
        /// <summary>
        /// Check if the tile position is inside the map
        /// </summary>
        /// <param name="gridX">Tile x position of the map</param>
        /// <param name="gridY">Tile y position of the map</param>
        /// <returns></returns>
		public bool IsValidAutoTilePos( int gridX, int gridY )
		{
            return !(gridX < 0 || gridX >= MapSelect.TileMapWidth || gridY < 0 || gridY >= MapSelect.TileMapHeight);
		}

        private AutoTile m_emptyAutoTile = new AutoTile() { Id = k_emptyTileId };
        
        /// <summary>
        /// Return the AutoTile data for a tile in the provided tile position and layer
        /// </summary>
        /// <param name="gridX">Tile x position of the map</param>
        /// <param name="gridY">Tile y position of the map</param>
        /// <param name="iLayer">Tile layer, see eTileLayer </param>
        /// <returns></returns>
		public AutoTile GetAutoTile( int gridX, int gridY, int iLayer )
		{
            if (IsValidAutoTilePos(gridX, gridY) && iLayer < MapLayers.Count)
            {
                AutoTile autoTile = TileLayers[MapLayers[iLayer].TileLayerIdx][gridX + gridY * MapTileWidth];
                if(autoTile != null){
                    return autoTile;
                }
                return new AutoTile() { 
                    Id = k_emptyTileId,
                    TileX = gridX,
                    TileY = gridY
                };
            }
            return new AutoTile() { 
                Id = k_outofboundsTileId,
                TileX = gridX,
                TileY = gridY
            };
		}

        /// <summary>
        /// Set a tile in the grid coordinates specified and layer ( 0: ground, 1: overground, 2: overlay )
        /// </summary>
        /// <param name="gridX">Tile x position of the map</param>
        /// <param name="gridY">Tile y position of the map</param>
        /// <param name="tileId">This is the id of the tile. You can see it in the editor while editing the map in the top left corner. Use -1 for an empty tile</param>
        /// <param name="iLayer"> Layer where to set the tile ( 0: ground, 1: overground, 2: overlay )</param>
        /// <param name="refreshTile">If tile and neighbors should be refreshed by this method or do it layer</param>
        public void SetAutoTile(int gridX, int gridY, int tileId, int iLayer, bool refreshTile = true)
		{
			if( !IsValidAutoTilePos( gridX, gridY ) || iLayer >= MapLayers.Count )
			{
				return;
			}

            bool tileHasChange = false;
            {
                //Bug Here
                // tileId = Mathf.Clamp(tileId, -1, Tileset.ThumbnailRects.Count - 1);
                int idx = gridX + gridY * MapTileWidth;
                AutoTile autoTile = TileLayers[MapLayers[iLayer].TileLayerIdx][idx];

                if (autoTile == null)
                {
                    autoTile = new AutoTile();
                    TileLayers[MapLayers[iLayer].TileLayerIdx][idx] = autoTile;
                    autoTile.TilePartsType = new eTilePartType[4];
                    autoTile.TilePartsIdx = new int[4];
                }
                // int tilesetIdx = tileId / AutoTileset.k_TilesPerSubTileset;
                tileHasChange = autoTile.Id != tileId;
                autoTile.Id = tileId;
                // autoTile.TilesetIdx = tilesetIdx;
                // autoTile.MappedIdx = tileId < 0 ? -1 : Tileset.AutotileIdxMap[tileId % AutoTileset.k_TilesPerSubTileset];
                autoTile.TileX = gridX;
                autoTile.TileY = gridY;
                autoTile.Layer = iLayer;
                // autoTile.Type = _GetAutoTileType(autoTile);

                // refresh tile and neighbours
                if (refreshTile && tileHasChange)
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
		}

        /// <summary>
        /// Refresh all tiles of the map. 
        /// Used for optimization, when calling SetAutoTile with refreshTile = false, for a big amount of tiles, this can be called later and refresh all at once.
        /// </summary>
        public void RefreshAllTiles()
        {
            for (int i = 0; i < MapLayers.Count; ++i)
            {
                for (int j = 0; j < TileLayers[MapLayers[i].TileLayerIdx].Length; ++j)
                {
                    RefreshTile(TileLayers[MapLayers[i].TileLayerIdx][j]);
                }
            }            
        }

		private int[,] aTileAff = new int[,]
		{
			{2, 0},
			{0, 2},
			{2, 4},
			{2, 2},
			{0, 4},
		};
		
		private int[,] aTileBff = new int[,]
		{
			{3, 0},
			{3, 2},
			{1, 4},
			{1, 2},
			{3, 4},
		};
		
		private int[,] aTileCff = new int[,]
		{
			{2, 1},
			{0, 5},
			{2, 3},
			{2, 5},
			{0, 3},
		};
		
		private int[,] aTileDff = new int[,]
		{
			{3, 1},
			{3, 5},
			{1, 3},
			{1, 5},
			{3, 3},
		};

        /// <summary>
        /// Refresh a tile according to neighbors
        /// </summary>
        /// <param name="gridX">Tile x position of the map</param>
        /// <param name="gridY">Tile y position of the map</param>
        /// <param name="iLayer"> Layer where to set the tile ( 0: ground, 1: overground, 2: overlay )</param>
		public void RefreshTile( int gridX, int gridY, int iLayer )
		{
			AutoTile autoTile = GetAutoTile( gridX, gridY, iLayer );
			RefreshTile( autoTile );
		}

        /// <summary>
        /// Refresh a tile according to neighbors
        /// </summary>
        /// <param name="autoTile">Tile to be refreshed</param>
		public void RefreshTile( AutoTile autoTile )
		{
            #if true //TestBlock
            if (autoTile == null) return;
			m_tileChunkPoolNode.MarkUpdatedTile( autoTile.TileX, autoTile.TileY, autoTile.Layer);
            #else
            if (autoTile == null) return;

			m_tileChunkPoolNode.MarkUpdatedTile( autoTile.TileX, autoTile.TileY, autoTile.Layer);

            // if(autoTile.TilesetIdx >= Tileset.SubTilesets.Count){
            //     return;
            // }
            int TilesetIdx = 0;
            SubTilesetConf tilesetConf = Tileset.SubTilesets[TilesetIdx];
			if( autoTile.Id >= 0 )
			{
                //Create TilePartsIdx
				{
					int gridX = autoTile.TileX;
					int gridY = autoTile.TileY;
					int iLayer = autoTile.Layer;
					int tilePartIdx = 0;
					for( int j = 0; j < 2; ++j )
					{
						for( int i = 0; i < 2; ++i, ++tilePartIdx )
						{
							int tile_x = gridX*2 + i;
							int tile_y = gridY*2 + j;

							int tilePartX = 0;
							int tilePartY = 0;

							eTilePartType tilePartType;
							if (tile_x % 2 == 0 && tile_y % 2 == 0) //A
							{
								tilePartType = _getTileByNeighbours( autoTile.Id, 
								                               GetAutoTile( gridX, gridY-1, iLayer ).Id, //V 
								                               GetAutoTile( gridX-1, gridY, iLayer ).Id, //H 
								                               GetAutoTile( gridX-1, gridY-1, iLayer ).Id  //D
								                               );
								tilePartX = aTileAff[ (int)tilePartType, 0 ];
								tilePartY = aTileAff[ (int)tilePartType, 1 ];
							} 
							else if (tile_x % 2 != 0 && tile_y % 2 == 0) //B
							{
								tilePartType = _getTileByNeighbours( autoTile.Id, 
								                               GetAutoTile( gridX, gridY-1, iLayer ).Id, //V 
								                               GetAutoTile( gridX+1, gridY, iLayer ).Id, //H 
								                               GetAutoTile( gridX+1, gridY-1, iLayer ).Id  //D
								                               );
								tilePartX = aTileBff[ (int)tilePartType, 0 ];
								tilePartY = aTileBff[ (int)tilePartType, 1 ];
							}
							else if (tile_x % 2 == 0 && tile_y % 2 != 0) //C
							{
								tilePartType = _getTileByNeighbours( autoTile.Id, 
								                               GetAutoTile( gridX, gridY+1, iLayer ).Id, //V 
								                               GetAutoTile( gridX-1, gridY, iLayer ).Id, //H 
								                               GetAutoTile( gridX-1, gridY+1, iLayer ).Id  //D
								                               );
								tilePartX = aTileCff[ (int)tilePartType, 0 ];
								tilePartY = aTileCff[ (int)tilePartType, 1 ];
							}
							else //if (tile_x % 2 != 0 && tile_y % 2 != 0) //D
							{
								tilePartType = _getTileByNeighbours( autoTile.Id, 
								                               GetAutoTile( gridX, gridY+1, iLayer ).Id, //V 
								                               GetAutoTile( gridX+1, gridY, iLayer ).Id, //H 
								                               GetAutoTile( gridX+1, gridY+1, iLayer ).Id  //D
								                               );
								tilePartX = aTileDff[ (int)tilePartType, 0 ];
								tilePartY = aTileDff[ (int)tilePartType, 1 ];
							}

							// set the kind of tile, for collision use
							autoTile.TilePartsType[ tilePartIdx ] = tilePartType;

							// int tileBaseIdx = tilesetConf.TilePartOffset[ (int)autoTile.Type ]; // set base tile idx of autoTile tileset
                            // int tileBaseIdx = 0;
                            //NOTE: All tileset have 32 autotiles except the Wall tileset with 48 tiles ( so far it's working because wall tileset is the last one )
							// relativeTileIdx = autoTile.MappedIdx - ((int)autoTile.Type * 32); // relative to owner tileset ( All tileset have 32 autotiles )
                            int relativeTileIdx = autoTile.Id % AutoTileset.k_TilesPerSubTileset; // relative to owner tileset ( All tileset have 32 autotiles )
                            int tx = relativeTileIdx % Tileset.AutoTilesPerRow;
                            int ty = relativeTileIdx / Tileset.AutoTilesPerRow;
                            int tileBaseIdx = tilesetConf.TilePartOffset;
							int tilePartSpriteIdx = tileBaseIdx + ty * (Tileset.AutoTilesPerRow * 4) * 6 + tx * 4 + tilePartY * (Tileset.AutoTilesPerRow * 4) + tilePartX;
                            // int tilePartSpriteIdx = ty * (Tileset.AutoTilesPerRow * 4) * 6 + tx * 4 + tilePartY * (Tileset.AutoTilesPerRow * 4) + tilePartX;
							autoTile.TilePartsIdx[ tilePartIdx ] = tilePartSpriteIdx;
						}
					}
                    // Set Length of tileparts
                    autoTile.TilePartsLength = 4;
				}
			}
            #endif
		}

        /// <summary>
        /// Get the map collision at world position
        /// </summary>
        /// <param name="vPos">World position</param>
        /// <returns></returns>
		public eTileCollisionType GetAutotileCollisionAtPosition( Vector3 vPos )
		{
			vPos -= transform.position;

			// transform to pixel coords
			vPos.y = -vPos.y;

            vPos.x = vPos.x * Tileset.TileWidth / CellSize.x;
            vPos.y = vPos.y * Tileset.TileHeight / CellSize.y;

			if( vPos.x >= 0 && vPos.y >= 0 )
			{
                int tile_x = (int)vPos.x / Tileset.TileWidth;
                int tile_y = (int)vPos.y / Tileset.TileWidth;
                Vector2 vTileOffset = new Vector2((int)vPos.x % Tileset.TileWidth, (int)vPos.y % Tileset.TileHeight);
				for( int iLayer = MapLayers.Count - 1; iLayer >= 0; --iLayer )
				{
                    if (MapLayers[iLayer].LayerType == eSlotAonType.Ground)
                    {
                        eTileCollisionType tileCollType = GetAutotileCollision(tile_x, tile_y, iLayer, vTileOffset);
                        if (tileCollType != eTileCollisionType.EMPTY && tileCollType != eTileCollisionType.OVERLAY) //remove Overlay check???
                        {
                            return tileCollType;
                        }
                    }
				}
			}
			return eTileCollisionType.PASSABLE;
		}

        /// <summary>
        /// Gets first map collision found for a given grid position
        /// </summary>
        /// <param name="tile_x">Grid position X</param>
        /// <param name="tile_y">Grid position Y</param>
        /// <returns></returns>
        public eTileCollisionType GetCellAutotileCollision(int tile_x, int tile_y)
        {
            return eTileCollisionType.EMPTY;
        }

        /// <summary>
        /// Gets map collision over a tile and an offset position relative to the tile
        /// </summary>
        /// <param name="tile_x">X tile coordinate of the map</param>
        /// <param name="tile_y">Y tile coordinate of the map</param>
        /// <param name="layer">Layer of the map (see eLayerType)</param>
        /// <param name="vTileOffset"></param>
        /// <returns></returns>
		public eTileCollisionType GetAutotileCollision( int tile_x, int tile_y, int layer, Vector2 vTileOffset )
		{
            return eTileCollisionType.EMPTY;
		}

		// NOTE: depending of the collType and tilePartType, this method returns the collType or eTileCollisionType.PASSABLE
		// This is for special tiles like Fence and Wall where not all of tile part should return collisions
		eTileCollisionType _GetTilePartCollision( eTileCollisionType collType, eTilePartType tilePartType, int tilePartIdx, Vector2 vTilePartOffset )
		{
            int tilePartHalfW = Tileset.TilePartWidth / 2;
            int tilePartHalfH = Tileset.TilePartHeight / 2;
			if( collType == eTileCollisionType.FENCE )
			{
				if( tilePartType == eTilePartType.EXT_CORNER || tilePartType == eTilePartType.V_SIDE )
				{
					// now check inner collision ( half left for tile AC and half right for tiles BD )
					// AX|BX|A1|B1	A: 0
					// CX|DX|C1|D1	B: 1
					// A2|B4|A4|B2	C: 2
					// C5|D3|C3|D5	D: 3
					// A5|B3|A3|B5
					// C2|D4|C4|D2
					if( 
					   (tilePartIdx == 0 || tilePartIdx == 2) && (vTilePartOffset.x < tilePartHalfW ) ||
					   (tilePartIdx == 1 || tilePartIdx == 3) && (vTilePartOffset.x > tilePartHalfW )
					)
					{
						return eTileCollisionType.PASSABLE;
					}
                    /* test: removing top part of fence collider
                    else if (tilePartType == eTilePartType.EXT_CORNER && (tilePartIdx == 0 || tilePartIdx == 1) && (vTilePartOffset.y < Tileset.TilePartHeight))
                    {
                        return eTileCollisionType.PASSABLE;
                    }*/
				}
                /* test: removing top part of fence collider
                else if ((tilePartIdx == 0 || tilePartIdx == 1) && (vTilePartOffset.y < Tileset.TilePartHeight))
                {                   
                    return eTileCollisionType.PASSABLE;
                }*/
			}
			else if( collType == eTileCollisionType.WALL )
			{
				if( tilePartType == eTilePartType.INTERIOR )
				{
					return eTileCollisionType.PASSABLE;
				}
				else if( tilePartType == eTilePartType.H_SIDE )
				{
					if( 
					   (tilePartIdx == 0 || tilePartIdx == 1) && (vTilePartOffset.y >= tilePartHalfH ) ||
					   (tilePartIdx == 2 || tilePartIdx == 3) && (vTilePartOffset.y < tilePartHalfH )
					   )
					{
						return eTileCollisionType.PASSABLE;
					}
				}
				else if( tilePartType == eTilePartType.V_SIDE )
				{
					if( 
					   (tilePartIdx == 0 || tilePartIdx == 2) && (vTilePartOffset.x >= tilePartHalfW ) ||
					   (tilePartIdx == 1 || tilePartIdx == 3) && (vTilePartOffset.x < tilePartHalfW )
					   )
					{
						return eTileCollisionType.PASSABLE;
					}
				}
				else
				{
					Vector2 vRelToIdx0 = vTilePartOffset; // to check only the case (tilePartIdx == 0) vTilePartOffset coords are mirrowed to put position over tileA with idx 0
					vRelToIdx0.x = (int)vRelToIdx0.x; // avoid precission errors when mirrowing, as 0.2 is 0, but -0.2 is 0 as well and should be -1
					vRelToIdx0.y = (int)vRelToIdx0.y;
                    if (tilePartIdx == 1) vRelToIdx0.x = -vRelToIdx0.x + Tileset.TilePartWidth - 1;
                    else if (tilePartIdx == 2) vRelToIdx0.y = -vRelToIdx0.y + Tileset.TilePartHeight - 1;
                    else if (tilePartIdx == 3) vRelToIdx0 = -vRelToIdx0 + new Vector2(Tileset.TilePartWidth - 1, Tileset.TilePartHeight - 1);

					if( tilePartType == eTilePartType.INT_CORNER )
					{
						if( (int)vRelToIdx0.x / tilePartHalfW == 1 || (int)vRelToIdx0.y / tilePartHalfH == 1 )
						{
							return eTileCollisionType.PASSABLE;
						}
					}
					else if( tilePartType == eTilePartType.EXT_CORNER )
					{
						if( (int)vRelToIdx0.x / tilePartHalfW == 1 && (int)vRelToIdx0.y / tilePartHalfH == 1 )
						{
							return eTileCollisionType.PASSABLE;
						}
					}

				}
			}
			return collType;
		}

		// V vertical, H horizontal, D diagonal
		private eTilePartType _getTileByNeighbours( int tileId, int tileIdV, int tileIdH, int tileIdD )
		{
            if (tileIdV == k_outofboundsTileId) tileIdV = tileId;
            if (tileIdH == k_outofboundsTileId) tileIdH = tileId;
            if (tileIdD == k_outofboundsTileId) tileIdD = tileId;

			if (
				(tileIdV == tileId) &&
				(tileIdH == tileId) &&
				(tileIdD != tileId)
				) 
			{
				return eTilePartType.INT_CORNER;
			}
			else if (
				(tileIdV != tileId) &&
				(tileIdH != tileId)
				) 
			{
				return eTilePartType.EXT_CORNER;
			}
			else if (
				(tileIdV == tileId) &&
				(tileIdH == tileId) &&
				(tileIdD == tileId)
				) 
			{
				return eTilePartType.INTERIOR;
			}
			else if (
				(tileIdV != tileId) &&
				(tileIdH == tileId)
				) 
			{
				return eTilePartType.H_SIDE;
			}
			else /*if (
				(tile_typeV == tile_type) &&
				(tile_typeH != tile_type)
				)*/
			{
				return eTilePartType.V_SIDE;
			}
		}

        Color _support_GetAvgColorOfTexture( Color[] aColors)
		{
			float r, g, b, a;
			r = g = b = a = 0;
			for( int i = 0; i < aColors.Length; ++i )
			{
				r += aColors[i].r;
				g += aColors[i].g;
				b += aColors[i].b;
				a += aColors[i].a;
			}
			r /= aColors.Length;
			g /= aColors.Length;
			b /= aColors.Length;
			a /= aColors.Length;
			return new Color(r, g, b, a);
		}

		void _GenerateMinimapTilesTexture()
		{
            m_thumbnailTextures = new Texture2D[ Tileset.SlotAONs.Count ];
			int coll = 8;
            int row = Tileset.SlotAONs.Count /coll + 1;
            int w = coll * Tileset.TileWidth;
            int h = row * Tileset.TileHeight;
            int ceil = Mathf.CeilToInt( (float)Math.Sqrt( Tileset.SlotAONs.Count ) );
            Color[] aColors = Enumerable.Repeat<Color>( new Color(0f, 0f, 0f, 0f) , ceil * ceil).ToArray();
            Texture2D thumbTex = new Texture2D( w, h, TextureFormat.ARGB32, false );
            thumbTex.filterMode = FilterMode.Point;
            int idx = 0;
			foreach( SlotAON slot in Tileset.SlotAONs)
			{
                int x = Mathf.RoundToInt(idx % coll) * Tileset.TileWidth;
                int y = Mathf.RoundToInt(idx / coll) * Tileset.TileHeight;
				Color[] colors = slot.PixelsThumb;
                thumbTex.SetPixels( x, h - y - Tileset.TileHeight, Mathf.RoundToInt(slot.AtlasRecThumb.width), Mathf.RoundToInt(slot.AtlasRecThumb.height), colors);
                aColors[idx] = _support_GetAvgColorOfTexture( colors);
                idx++;
			}
            thumbTex.Apply();
            m_thumbnailTextures[0] = thumbTex;
            m_minimapTilesTexture = new Texture2D(ceil, ceil);
            m_minimapTilesTexture.anisoLevel = 0;
            m_minimapTilesTexture.filterMode = FilterMode.Point;
            m_minimapTilesTexture.name = "MiniMapTiles";
            m_minimapTilesTexture.hideFlags = HideFlags.DontSave;
			m_minimapTilesTexture.SetPixels( aColors );
			m_minimapTilesTexture.Apply();
		}

        /// <summary>
        /// Refresh full minimp texture
        /// </summary>
		public void RefreshMinimapTexture( )
		{
			RefreshMinimapTexture( 0, 0, MapTileWidth, MapTileHeight );
		}

        /// <summary>
        /// Refresh minimap texture partially
        /// </summary>
        /// <param name="tile_x">X tile coordinate of the map</param>
        /// <param name="tile_y">Y tile coordinate of the map</param>
        /// <param name="width">Width in tiles</param>
        /// <param name="height">Height in tiles</param>
		public void RefreshMinimapTexture( int tile_x, int tile_y, int width, int height )
        {
            Debug.Log("RefreshMinimapTexture");
			tile_x = Mathf.Clamp( tile_x, 0, MinimapTexture.width - 1 );
			tile_y = Mathf.Clamp( tile_y, 0, MinimapTexture.height - 1 );
			width = Mathf.Min( width, MinimapTexture.width - tile_x );
			height = Mathf.Min( height, MinimapTexture.height - tile_y );
            if(height <= 0 || width <= 0){
                Debug.Log("Error: RefreshMinimapTexture w-h:" + width + " - " + height);
                return;
            }
			Color[] aTilesColors = m_minimapTilesTexture.GetPixels();
            //Get origin
            var l = MinimapTexture.GetPixels(tile_x, MinimapTexture.height - tile_y - height, width, height).Length;
			Color[] aMinimapColors = Enumerable.Repeat<Color>( new Color(0f, 0f, 0f, 1f) , l).ToArray();
			foreach( MapLayer mapLayer in MapLayers )
			{
                AutoTile[] aAutoTiles = TileLayers[mapLayer.TileLayerIdx];
                if (mapLayer.LayerType == eSlotAonType.Ground || mapLayer.LayerType == eSlotAonType.Overlay){
                }else{
                    continue;
                }
                // read tile type in the same way that texture pixel order, from bottom to top, right to left
                for (int yf = 0; yf < height; ++yf)
                {
                    for (int xf = 0; xf < width; ++xf)
                    {
                        int tx = tile_x + xf;
                        int ty = tile_y + yf;
                        int tileIdx = tx + ty * MapTileWidth;

                        int idTile = aAutoTiles[tileIdx] != null ? aAutoTiles[tileIdx].Id : -1;
                        if (idTile >= 0 && idTile < aTilesColors.Length)
                        {
                            // if(idTile >= aTilesColors.Length){
                            //     Debug.Log("Error");
                            // }
                            int idx = (height - 1 - yf) * width + xf;
                            Color baseColor = aMinimapColors[idx];
                            Color tileColor = aTilesColors[idTile];
                            aMinimapColors[idx] = baseColor * (1 - tileColor.a) + tileColor * tileColor.a;
                            aMinimapColors[idx].a = 1f;
                        }
                        /*
                        else if (mapLayer.LayerType == eLayerType.FogOfWar)
                        {
                            Color cFogColor = new Color32(32, 32, 32, 255); ;
                            byte[] fogAlpha = System.BitConverter.GetBytes(type);
                            for( int i = 0; i < fogAlpha.Length; ++i )
                            {
                                cFogColor.a += (fogAlpha[i] / 0xff);
                            }
                            cFogColor.a /= fogAlpha.Length;
                            int idx = (height - 1 - yf) * width + xf;
                            aMinimapColors[idx] = aMinimapColors[idx] * (1 - cFogColor.a) + cFogColor * cFogColor.a;
                            aMinimapColors[idx].a = 1f;
                        }
                        */
                    }
                }
			}
			MinimapTexture.SetPixels( tile_x, MinimapTexture.height - tile_y - height, width, height, aMinimapColors );
			MinimapTexture.Apply();
		}


        // private Dictionary<int, KeyValuePair<byte[], byte[]>> m_fogOfWarTilesToUpdate = new Dictionary<int, KeyValuePair<byte[], byte[]>>();
        // private List<KeyValuePair<int, byte[]>> m_fogOfWarSetRequests = new List<KeyValuePair<int, byte[]>>();
        // private MapLayer m_fogOfWarMapLayer = null;
        // private System.Threading.Mutex m_fogOfWarMutex = new System.Threading.Mutex();
        // private bool m_fogOfWarInitCoroutineOnNextUpdate = false;

        /// <summary>
        /// Set tile fog values. Each tile is divided in 4 tile parts and each tile part have a byte value for alpha.
        /// The fog tile will change its alpha values smoothly using a coroutine
        /// </summary>
        /// <param name="tileIdx">Tile index in the map (tileX + tileY*MapWidth)</param>
        /// <param name="values">Alpha value for each part of the tile. Should be an array of 4 elements</param>
        // public void AddFogOfWarSetToQueue(int tileIdx, byte[] values)
        // {
        //     m_fogOfWarMutex.WaitOne();
        //     m_fogOfWarSetRequests.Add( new KeyValuePair<int, byte[]>(tileIdx, values));
        //     m_fogOfWarMutex.ReleaseMutex();
        // }

        // private void _SetFogOfWarAsync( int tileIdx, byte[] values )
        // {
        //     KeyValuePair<byte[], byte[]> fromToPair;
        //     if (m_fogOfWarTilesToUpdate.TryGetValue(tileIdx, out fromToPair))
        //     {
        //         for (int i = 0; i < fromToPair.Value.Length; ++i)
        //         {
        //             fromToPair.Value[i] = (byte)Mathf.Min(fromToPair.Value[i], values[i]);
        //         }
        //     }
        //     else
        //     {
        //         byte[] fromData = System.BitConverter.GetBytes(TileLayers[m_fogOfWarMapLayer.TileLayerIdx][tileIdx].Id);
        //         byte[] toData = new byte[4];
        //         System.Array.Copy(values, toData, toData.Length);
        //         fromToPair = new KeyValuePair<byte[], byte[]>(fromData, toData );
        //         m_fogOfWarTilesToUpdate[tileIdx] = fromToPair;
        //     }
        // }


        // private IEnumerator FogOfWarCoroutine()
        // {
        //     List<int> tileIdxToRemove = new List<int>();
        //     float offsetDt = 0f;
        //     float prevTime = Time.time;
        //     while (m_fogOfWarMapLayer != null)
        //     {
        //         offsetDt += (Time.time - prevTime) * 0xff; // * (value) = offset changed by second ( used with option A. see below )
        //         prevTime = Time.time;

        //         int iOffsetDt = (int)offsetDt;
        //         offsetDt -= iOffsetDt;
        //         m_fogOfWarMutex.WaitOne();
        //         foreach( KeyValuePair<int, byte[]> request in m_fogOfWarSetRequests )
        //         {
        //             _SetFogOfWarAsync(request.Key, request.Value);
        //         }
        //         m_fogOfWarSetRequests.Clear();
        //         m_fogOfWarMutex.ReleaseMutex();
        //         foreach (KeyValuePair<int, KeyValuePair<byte[], byte[]>> entry in m_fogOfWarTilesToUpdate)
        //         {
        //             int tileIdx = entry.Key;
        //             byte[] fromData = entry.Value.Key;
        //             byte[] toData = entry.Value.Value;

        //             bool isDone = true;
        //             for( int i = 0; i < fromData.Length; ++i )
        //             {
        //                 if (fromData[i] > toData[i])
        //                 {
        //                     //NOTE: Option A: smooth fade based on time, but it has an effect of removing fog from inside to outside
        //                     //fromData[i] = ( iOffsetDt > (int)(fromData[i] - toData[i]) )? toData[i] : (byte)(fromData[i] - iOffsetDt);
                            
        //                     //Note: Option B: smooth fade based on frame but it has an effect of removing fog from inside to outside
        //                     iOffsetDt = (fromData[i] - toData[i]) / 20;
        //                     if (iOffsetDt == 0) iOffsetDt = 1;
        //                     fromData[i] -= (byte)iOffsetDt;
        //                     isDone = false;
        //                 }
        //             }                    

        //             if( isDone )
        //             {
        //                 tileIdxToRemove.Add( tileIdx );
        //             }
        //             else
        //             {
        //                 AutoTile autoTile = TileLayers[m_fogOfWarMapLayer.TileLayerIdx][tileIdx];
        //                 autoTile.Id = System.BitConverter.ToInt32(fromData, 0);
        //                 AutoTileMap_Editor.Instance.RefreshTile(autoTile);
        //             }
        //         }

        //         foreach( int idx in tileIdxToRemove )
        //         {
        //             m_fogOfWarTilesToUpdate.Remove(idx);
        //         }
        //         tileIdxToRemove.Clear();

        //         yield return null;
        //     }
        // }
	}
	#endif
}
