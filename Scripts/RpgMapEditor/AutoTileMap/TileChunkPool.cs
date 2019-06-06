using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

namespace AON.RpgMapEditor
{

    /// <summary>
    /// Manages the creation of all map tile chunks
    /// </summary>
	public class TileChunkPool : MonoBehaviour 
	{
        /// <summary>
        /// The width size of the generated tilechunks in tiles ( due max vertex limitation, this should be less than 62 )
        /// Increasing this value, map will load faster but drawing tiles will be slower
        /// </summary>
		public const int k_TileChunkWidth = 8;

        /// <summary>
        /// The height size of the generated tilechunks in tiles ( due max vertex limitation, this should be less than 62 )
        /// Increasing this value, map will load faster but drawing tiles will be slower
        /// </summary>
		public const int k_TileChunkHeight = 8;

		[System.Serializable]
		public class TileChunkLayer
		{
			public GameObject ObjNode;
			public TileChunk[] TileChunks;
            public int SortingOrder 
            { 
                get{ return _sortingOrder; }
                set
                {
                    _sortingOrder = value;
                    for (int i = 0; i < TileChunks.Length; ++i )
                    {
                        if (TileChunks[i] != null) TileChunks[i].SortingOrder = value;
                    }
                } 
            }

            public string SortingLayer
            {
                get { return _sortingLayer; }
                set
                {
                    _sortingLayer = value;
                    for (int i = 0; i < TileChunks.Length; ++i)
                    {
                        if (TileChunks[i] != null) TileChunks[i].SortingLayer = value;
                    }
                }
            }

            private int _sortingOrder = 0;
            private string _sortingLayer = "Default";
        }

		public List<TileChunkLayer> TileChunkLayers = new List<TileChunkLayer>();

		private List<TileChunk> m_tileChunkToBeUpdated = new List<TileChunk>();

		[SerializeField]
		private AutoTileMap m_autoTileMap;

        // [System.NonSerialized]
        [SerializeField]
        private Material material_water = null;
        public Material MaterialWater{
            get{
                return material_water;
            }
        }

		public void Initialize (AutoTileMap autoTileMap)
		{
			hideFlags = HideFlags.NotEditable;
			m_autoTileMap = autoTileMap;
			foreach( TileChunkLayer tileChunkLayer in TileChunkLayers )
			{
				if( tileChunkLayer.ObjNode != null )
				{
				#if UNITY_EDITOR
					DestroyImmediate(tileChunkLayer.ObjNode);
				#else
					Destroy(tileChunkLayer.ObjNode);
				#endif
				}
			}
			TileChunkLayers.Clear();
			m_tileChunkToBeUpdated.Clear();
		}

        /// <summary>
        /// Mark a tile to be updated during update
        /// </summary>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        /// <param name="layer"></param>
		public void MarkUpdatedTile( int tileX, int tileY, int layer )
		{
			TileChunk tileChunk = _GetTileChunk( tileX, tileY, layer );
			if( !m_tileChunkToBeUpdated.Contains(tileChunk) )
			{
				m_tileChunkToBeUpdated.Add( tileChunk );
			}
		}
        
        public void MarkUpdatedAllTile() {
            foreach(TileChunkLayer layer in TileChunkLayers){
                m_tileChunkToBeUpdated.AddRange(layer.TileChunks);
            }
        }

        /// <summary>
        /// Mark all tilechunks of a layer to be updated
        /// </summary>
        /// <param name="layer"></param>
        public void MarkLayerChunksForUpdate( int layer )
        {
            TileChunkLayer chunkLayer = _GetTileChunkLayer(layer);
            m_tileChunkToBeUpdated.AddRange(chunkLayer.TileChunks);
        }

        /// <summary>
        /// Update marked chunks
        /// </summary>
		public void UpdateChunks()
		{
            IEnumerator coroutine = UpdateChunksAsync();
            while (coroutine.MoveNext());
		}

        /// <summary>
        /// Update marked chunks asynchronously
        /// </summary>
        public IEnumerator UpdateChunksAsync ( )
        {
            while (m_tileChunkToBeUpdated.Count > 0)
            {
                if (m_tileChunkToBeUpdated[0]){
                    if(material_water == null){
                        var tileWater = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.WaterTile;
                        var prefabWarter = Resources.Load<GameObject>(tileWater.prefab_name);
                        var p = Instantiate(prefabWarter);
                        p.transform.parent = transform.transform;
                        material_water = p.GetComponent<Renderer>().material;
                        p.AddComponent<TextureTileToScaleWater>();
                        p.GetComponent<Renderer>().enabled = false;
                        p.name = "material_water";
                    }
                    m_tileChunkToBeUpdated[0].RefreshTileData( ref material_water);
                }   
                m_tileChunkToBeUpdated.RemoveAt(0);
                yield return null;
            }
        }

		public void UpdateLayersData (bool isPlayMode)
		{            
			for( int i = 0; i < TileChunkLayers.Count; ++i )
			{
                AutoTileMap.MapLayer mapLayer = m_autoTileMap.MapLayers[i];
                TileChunkLayer tileChunkLayer = TileChunkLayers[i];
                tileChunkLayer.ObjNode.SetActive(mapLayer.Visible);
                if(isPlayMode){
                    tileChunkLayer.ObjNode.transform.localPosition = Vector3.zero;
                }else{
                    if(mapLayer.TileLayerIdx == 0){
                        tileChunkLayer.ObjNode.transform.localPosition = Vector3.zero;
                    }else{
                        tileChunkLayer.ObjNode.transform.localPosition = Vector3.zero + new Vector3(0f, 0f, -5 - mapLayer.TileLayerIdx);
                    }
                    tileChunkLayer.SortingLayer = mapLayer.SortingLayer;
                    tileChunkLayer.SortingOrder = mapLayer.SortingOrder;
                }
			}
		}

        public void RefreshTileTrigger(){
            foreach(TileChunkLayer layer in TileChunkLayers){
                foreach(TileChunk chunk in layer.TileChunks){
                    chunk.RefreshTileTrigger();
                }
            }
        }

        // TODO: When a layer is created with no tiles ( all set to -1 ) layer is never created and this is causing problems
        // but I should find a better way of doing this. TileChunkPool should be redesigned
        public void InitLayers()
        {
            int totalLayers = m_autoTileMap.MapLayers.Count;
            _CreateTileChunkLayer(totalLayers - 1);
        }

        // Get with not create
        public TileChunk GetTileChunk( int tileX, int tileY, int layer ){
            // TileChunkLayer chunkLayer = _GetTileChunkLayer( layer );

			// int rowTotalChunks = 1 + ((m_autoTileMap.MapTileWidth - 1) / k_TileChunkWidth);
			// int chunkIdx = (tileY / k_TileChunkHeight) * rowTotalChunks + (tileX / k_TileChunkWidth);
			// TileChunk tileChunk = chunkLayer.TileChunks[chunkIdx];
            return _GetTileChunk( tileX, tileY, layer );
        }

		private TileChunk _GetTileChunk( int tileX, int tileY, int layer )
		{
			TileChunkLayer chunkLayer = _GetTileChunkLayer( layer );

			int rowTotalChunks = 1 + ((m_autoTileMap.MapTileWidth - 1) / k_TileChunkWidth);
			int chunkIdx = (tileY / k_TileChunkHeight) * rowTotalChunks + (tileX / k_TileChunkWidth);
			TileChunk tileChunk = chunkLayer.TileChunks[chunkIdx];
			return tileChunk;
		}

        private TileChunk _CreateTileChunk(int startTileX, int startTileY, int layer, TileChunkLayer chunkLayer){
            GameObject chunkObj = new GameObject();
            chunkObj.name = m_autoTileMap.MapLayers[layer].Name + "_" + startTileX + "_" + startTileY;
            chunkObj.transform.SetParent( chunkLayer.ObjNode.transform, false);
            chunkObj.layer = chunkLayer.ObjNode.layer;
            TileChunk tileChunk = chunkObj.AddComponent<TileChunk>();
            tileChunk.Configure( m_autoTileMap, layer, startTileX, startTileY, k_TileChunkWidth, k_TileChunkHeight );
            return tileChunk;
        }
        

		private TileChunkLayer _GetTileChunkLayer( int layer )
		{
			return TileChunkLayers.Count > layer? TileChunkLayers[layer] : _CreateTileChunkLayer( layer );
		}

		private TileChunkLayer _CreateTileChunkLayer( int layer )
		{
			int rowTotalChunks = 1 + ((m_autoTileMap.MapTileWidth - 1) / k_TileChunkWidth);
			int colTotalChunks = 1 + ((m_autoTileMap.MapTileHeight - 1) / k_TileChunkHeight);
			int totalChunks = rowTotalChunks * colTotalChunks;
			TileChunkLayer chunkLayer = null;
			while( TileChunkLayers.Count <= layer )
			{
				chunkLayer = new TileChunkLayer();
				chunkLayer.TileChunks = new TileChunk[ totalChunks ];
				chunkLayer.ObjNode = new GameObject();
				chunkLayer.ObjNode.transform.parent = transform;
                // chunkLayer.ObjNode.transform.localPosition = Vector3.zero + new Vector3(0f, 0f, m_autoTileMap.MapLayers[TileChunkLayers.Count].Depth);
                if(TileChunkLayers.Count == 0){
                    chunkLayer.ObjNode.transform.localPosition = Vector3.zero;
                }else{
                    chunkLayer.ObjNode.transform.localPosition = Vector3.zero + new Vector3(0f, 0f, -5 - TileChunkLayers.Count);
                }
                chunkLayer.ObjNode.transform.localRotation = Quaternion.identity;
                chunkLayer.SortingOrder = m_autoTileMap.MapLayers[TileChunkLayers.Count].SortingOrder;
                chunkLayer.SortingLayer = m_autoTileMap.MapLayers[TileChunkLayers.Count].SortingLayer;
                chunkLayer.ObjNode.name = m_autoTileMap.MapLayers[TileChunkLayers.Count].Name + "_" + TileChunkLayers.Count;
                for( int i = 0; i < totalChunks; i++){
                    int startTileX = (i % rowTotalChunks) * k_TileChunkWidth;
                    int startTileY = (i / rowTotalChunks) * k_TileChunkHeight;
                    chunkLayer.TileChunks[i] = _CreateTileChunk( startTileX, startTileY, TileChunkLayers.Count, chunkLayer);
                }
                TileChunkLayers.Add( chunkLayer );
			}
			return chunkLayer;
		}

        public void BuildNavByMesh(ref List<NavMeshBuildSource> sources)
        {
            ShowAllChuck();

            sources.Clear();
            
			for( int i = 0; i < TileChunkLayers.Count; ++i )
			{
                if( i >= m_autoTileMap.MapSelect.TileData.Count)
                    continue;
                var tileData = m_autoTileMap.MapSelect.TileData[i];
                if(tileData.LayerType == eSlotAonTypeLayer.Ground){
                    TileChunkLayer tileChunkLayer = TileChunkLayers[i];
                    if(tileChunkLayer == null) continue;
                    foreach( TileChunk tileChunk in tileChunkLayer.TileChunks)
                    {
                        if(tileChunk == null) continue;
                        tileChunk.BuildNavByMesh( ref sources);
                    }
                }else if(tileData.LayerType == eSlotAonTypeLayer.Overlay){
                    TileChunkLayer tileChunkLayer = TileChunkLayers[i];
                    if(tileChunkLayer == null) continue;
                    foreach( TileChunk tileChunk in tileChunkLayer.TileChunks)
                    {
                        if(tileChunk == null) continue;
                        tileChunk.BuildNavByMesh( ref sources);
                    }
                }else if(tileData.LayerType == eSlotAonTypeLayer.Trigger){
                    // TileChunkLayer tileChunkLayer = TileChunkLayers[i];
                    // if(tileChunkLayer == null) continue;
                    // foreach( TileChunk tileChunk in tileChunkLayer.TileChunks)
                    // {
                    //     if(tileChunk == null) continue;
                    //     tileChunk.BuildMeshCollider();
                    // }
                }
            }
        }

        static public int offSetShowRight = 8;
        static public int offSetShowLeft = 40;
        static public int offSetShowTop = 40;
        static public int offSetShowBot = 8;

        #if false // Occ
        /// <summary>
        /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
        /// </summary>
        void FixedUpdate()
        {
            if(m_autoTileMap.IsPlayMode){
                var posAgent = m_autoTileMap.Agent.transform.position;
                var xAgent = posAgent.x;
                var yAgent = -posAgent.z;
                
                for(int i = 0; i < TileChunkLayers.Count; i++){
                    if(i >= 2){ // Apply with layer 0 and 1
                        break;
                    }
                    var layer = TileChunkLayers[i];
                    if( layer.ObjNode != null ){
                        foreach( var chunk in layer.TileChunks){
                            int minX = chunk.StartTileX - offSetShowLeft;
                            int maxX = chunk.StartTileX + k_TileChunkWidth + offSetShowRight;
                            int minY = chunk.StartTileY - offSetShowBot;
                            int maxY = chunk.StartTileY + k_TileChunkHeight + offSetShowTop;
                            if(xAgent > minX
                            && xAgent < maxX
                            && yAgent > minY
                            && yAgent < maxY){
                                chunk.gameObject.SetActive( true);
                            }else{
                                chunk.gameObject.SetActive( false);
                            }
                        }
                    }
                }
            }
        }
        #endif

        void ShowAllChuck(){
            foreach(var layer in TileChunkLayers){
                if( layer.ObjNode != null ){
                    foreach( var chunk in layer.TileChunks){
                        chunk.gameObject.SetActive( true);
                    }
                }
            }
        }

        private Dictionary<string, Material> MaterialCache = new Dictionary<string, Material>();

        public void SetMeshRendererColourWithCache( MeshRenderer mesh, string prefab_name, int tileHigh, int idTileCenter, int idTileOffset){
            string hash = string.Format("{0}_{1}_{2}_{3}", prefab_name, tileHigh, idTileCenter, idTileOffset);
            // Check cache
            if(MaterialCache.ContainsKey(hash)){
                mesh.material = MaterialCache[hash];
                return;
            }
            var m = mesh.material;
            // _Color
            if( idTileCenter == DefineAON.IdSlot_Dirt_Road){
                m.SetColor("_Color", new Color(160f / 255f, 112f / 255f, 104f / 255f,1));
            }
            // _Color_Bot
            if( tileHigh <= 0 && idTileOffset == DefineAON.IdSlot_Water){ //Water
                m.SetColor("_Color_Bot", new Color(32f / 255f, 208f / 255f, 255f / 255f,1));
            }else if(idTileOffset == DefineAON.IdSlot_Grass_0){ // Grass_0
                m.SetColor("_Color_Bot", new Color(151f / 255f, 214f / 255f, 127f / 255f,1));
            }else if(idTileOffset == DefineAON.IdSlot_Dirt_Road){ // Grass_0
                m.SetColor("_Color_Bot", new Color(160f / 255f, 112f / 255f, 104f / 255f,1));
            }
            // End
            mesh.material = m;
            MaterialCache[hash] = m;
        }
    }
}