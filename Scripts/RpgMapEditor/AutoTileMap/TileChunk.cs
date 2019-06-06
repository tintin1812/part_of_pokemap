using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using ch.sycoforge.Combine;

namespace AON.RpgMapEditor
{
    /// <summary>
    /// Manage a chunk of tiles in the 3D world
    /// </summary>
	public class TileChunk : MonoBehaviour 
	{
        public static bool IS_ALWAY_SHOW_OVERLAY = true;

        private NavMeshBuildSource CreateNavMeshBuildSource( MeshFilter _filter){
            return new NavMeshBuildSource
            {
                transform = _filter.transform.localToWorldMatrix,
                shape = NavMeshBuildSourceShape.Mesh,
                sourceObject = _filter.mesh,
                area = 0
            };
        }
        public void BuildNavByMesh(ref List<NavMeshBuildSource> sources) {
            var tileLayer = m_autoTileMap.MapSelect.TileData[MapLayerIdx];
            if(tileLayer.LayerType == eSlotAonTypeLayer.Trigger){
                return;
            }
            if(tileLayer.LayerType == eSlotAonTypeLayer.Ground){
                Transform _ground = transform.Find("Ground");
                if(_ground != null && _ground.transform.childCount > 0)
                {
                    for( int h = 0; h < _ground.childCount; h++)
                    {
                        var _high= _ground.GetChild(h);
                        Transform floor = _high.Find("Floor");
                        if(floor != null){
                            MeshFilter m = floor.GetComponent<MeshFilter>();
                            if(m != null){
                                sources.Add(CreateNavMeshBuildSource(m));
                            }
                        }
                        Transform stair = _high.Find("Stair");
                        if(stair != null){
                            MeshFilter[] _filters = stair.GetComponentsInChildren<MeshFilter>();
                            foreach (MeshFilter _filter in _filters)
                            {
                                if( _filter.mesh == null || _filter.mesh.vertexCount <= 0){
                                    continue;
                                }
                                // if( _filter.gameObject.name == "Terrain" || _filter.gameObject.name == "Water"){
                                //     continue;
                                // }
                                sources.Add(CreateNavMeshBuildSource(_filter));
                                // _filter.gameObject.AddComponent<MeshCollider>().sharedMesh = _filter.mesh;
                            }
                        }   
                    }
                }
            }else if(tileLayer.LayerType == eSlotAonTypeLayer.Overlay){
                var root = transform;
                BoxColliderGen[] boxColliderGens = root.GetComponentsInChildren<BoxColliderGen>();
                foreach (var boxColliderGen in boxColliderGens)
                {
                    var s = boxColliderGen.BuildSource();
                    sources.AddRange(s);
                    boxColliderGen.gameObject.SetActive(false);
                }
                //
                var sources_more = new List<NavMeshBuildSource>();
                var markups = new List<NavMeshBuildMarkup>();
                NavMeshBuilder.CollectSources(root, ~0, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources_more);
                sources.AddRange(sources_more);
                //
                foreach (var boxColliderGen in boxColliderGens)
                {
                    boxColliderGen.gameObject.SetActive(true);
                }
            }
            #if false // Check House, Filler3D

            Transform house3D = transform.Find("House");
            if(house3D != null && house3D.childCount > 0){
                var root = house3D;
                var sources_more = new List<NavMeshBuildSource>();
                var markups = new List<NavMeshBuildMarkup>();
                NavMeshBuilder.CollectSources(root, ~0, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources_more);
                sources.AddRange(sources_more);
            }
            Transform filler3D = transform.Find("Filler3D");
            if(filler3D != null && filler3D.childCount > 0){
                #if true //using box colider
                // BoxCollider[] _cols = filler3D.GetComponentsInChildren<BoxCollider>();
                // foreach (BoxCollider _col in _cols)
                // {
                //     NavMeshBuildSource s = new NavMeshBuildSource
                //     {
                //         transform = _col.transform.localToWorldMatrix,
                //         shape = NavMeshBuildSourceShape.Box,
                //         size = _col.bounds.size,
                //         area = 0
                //     };
                //     sources.Add(s);
                // }
                {
                    var root = filler3D;
                    var sources_more = new List<NavMeshBuildSource>();
                    var markups = new List<NavMeshBuildMarkup>();
                    NavMeshBuilder.CollectSources(root, ~0, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources_more);
                    sources.AddRange(sources_more);
                }
                #else //using mesh colider
                MeshFilter[] _filters = filler3D.GetComponents<MeshFilter>();
                foreach (MeshFilter _filter in _filters)
                {
                    if( _filter.mesh == null || _filter.mesh.vertexCount <= 0)
                        continue;
                    float h = _filter.mesh.bounds.max.y - _filter.mesh.bounds.min.y;
                    // Debug.Log("h: " + h);
                    if( h <= 0.5f){
                        continue;
                    }
                    NavMeshBuildSource s = new NavMeshBuildSource
                    {
                        transform = _filter.transform.localToWorldMatrix,
                        shape = NavMeshBuildSourceShape.Mesh,
                        sourceObject = _filter.mesh,
                        area = 0
                    };
                    sources.Add(s);
                    _filter.gameObject.AddComponent<MeshCollider>().sharedMesh = _filter.mesh;
                }
                #endif
            }
            #endif
        }
        
		private Vector3[] m_vertices;
		private Vector2[] m_uv;
		private int[] m_triangles;
        private Color32[] m_colors;

		public AutoTileMap m_autoTileMap;

        public int TileWidth = 8;
        public int TileHeight = 8;
		public int MapLayerIdx = 0;
		public int StartTileX = 0;
		public int StartTileY = 0;

        public int SortingOrder
        {
            get{ 
                CheckMeshRenderer();
                return m_meshFilter.GetComponent<Renderer>().sortingOrder; 
            }
            set{ 
                CheckMeshRenderer();
                m_meshFilter.GetComponent<Renderer>().sortingOrder = value; 
            }
        }

        public string SortingLayer
        {
            get {
                CheckMeshRenderer();
                return m_meshFilter.GetComponent<Renderer>().sortingLayerName; 
            }
            set { 
                CheckMeshRenderer();
                m_meshFilter.GetComponent<Renderer>().sortingLayerName = value; 
            }
        }

		private MeshFilter m_meshFilter;

        private HighChunk highChunk = null;

        void OnDestroy() {
            //avoid memory leak
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                DestroyImmediate(meshFilter.sharedMesh);
            }
        }

        /// <summary>
        /// Configure this chunk
        /// </summary>
        /// <param name="autoTileMap"></param>
        /// <param name="layer"></param>
        /// <param name="startTileX"></param>
        /// <param name="startTileY"></param>
        /// <param name="tileChunkWidth"></param>
        /// <param name="tileChunkHeight"></param>
		public void Configure (AutoTileMap autoTileMap, int layer, int startTileX, int startTileY, int tileChunkWidth, int tileChunkHeight) {
			m_autoTileMap = autoTileMap;
			TileWidth = tileChunkWidth;
			TileHeight = tileChunkHeight;
			MapLayerIdx = layer;
			StartTileX = startTileX;
			StartTileY = startTileY;

			// transform.gameObject.name = "TileChunk_"+startTileX+"_"+startTileY;
            
			Vector3 vPosition = new Vector3();
            vPosition.x = startTileX * this.CellSize().x;
            vPosition.y = -startTileY * this.CellSize().y;
			transform.localPosition = vPosition;

            var tileLayer = m_autoTileMap.MapSelect.TileData[MapLayerIdx];
            if(tileLayer.LayerType == eSlotAonTypeLayer.Trigger && m_autoTileMap.IsPlayMode == false && highChunk == null){
                if(highChunk == null){
                    var obj = new GameObject();
                    obj.name = "High";
                    obj.transform.parent = transform;
                    obj.transform.localPosition = new Vector3(0, 0, 0.5f);
                    obj.transform.localRotation = Quaternion.identity;
                    highChunk = obj.AddComponent<HighChunk>();
                    highChunk.Configure( autoTileMap, layer, startTileX, startTileY, tileChunkWidth, tileChunkHeight);
                }
            }
		}

        /// <summary>
        /// Regenerate the mesh for this tile chunk
        /// </summary>
		public void RefreshTileData(ref Material material_water) {
            if(m_autoTileMap.IsPlayMode){
                // AutoTileMap.MapLayer mapLayer = AutoTileMap.MapLayers[MapLayerIdx];
                var tileLayer = m_autoTileMap.MapSelect.TileData[MapLayerIdx];
                if(tileLayer.LayerType == eSlotAonTypeLayer.Ground){
                    // RefreshTileGroundPlayMode();
                    CreateGroundChuck();
                    // AddSideTileGroundPlayMode();
                    #if false // Add water
                    var tileWater = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.WaterTile;
                    AddSideTileWarterPlayMode( "Water", tileWater, ref material_water);
                    var tileWaterDown = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.WaterTileDown;
                    Material m = null;
                    AddSideTileWarterPlayMode( "WaterDown", tileWaterDown, ref m);
                    #endif
                }else if(tileLayer.LayerType == eSlotAonTypeLayer.Trigger){
                    // RefreshTileTrigger();
                }else if(tileLayer.LayerType == eSlotAonTypeLayer.Overlay){
                    RefreshTileOverlay(false);
                }
            }else{
                // RefreshTileGround();
                var tileLayer = m_autoTileMap.MapSelect.TileData[MapLayerIdx];
                if(tileLayer.LayerType == eSlotAonTypeLayer.Ground){
                    RefreshEditorGround();
                }else if(tileLayer.LayerType == eSlotAonTypeLayer.Trigger){
                    RefreshEditorTriggerAndOverlay();
                    if(highChunk != null){
                        highChunk.RefreshHigh();
                    }
                }else if(tileLayer.LayerType == eSlotAonTypeLayer.Overlay){
                    RefreshEditorTriggerAndOverlay();
                    if(IS_ALWAY_SHOW_OVERLAY){
                        RefreshTileOverlay(true);
                    }
                }
            }
		}

        private void RefreshEditorTriggerAndOverlay() {
            CheckMeshRenderer();
            // Debug.Log("TileChunk:RefreshTileData:" + this.transform.parent.name);
            if (m_meshFilter.sharedMesh == null)
            {
                m_meshFilter.sharedMesh = new Mesh();
                m_meshFilter.sharedMesh.hideFlags = HideFlags.DontSave;
            }
            Mesh mesh = m_meshFilter.sharedMesh;
            mesh.Clear();

            FillEditorTriggerAndOverlay();

            mesh.vertices = m_vertices;
            mesh.colors32 = m_colors;
            mesh.uv = m_uv;
            mesh.triangles = m_triangles;

            mesh.RecalculateNormals(); // allow using lights

        }

        private void FillEditorTriggerAndOverlay() {
            m_vertices = new Vector3[TileWidth * TileHeight * 4]; // 4 subtiles x 4 vertex per tile
            m_colors = new Color32[TileWidth * TileHeight * 4];
            m_uv = new Vector2[m_vertices.Length];
            m_triangles = new int[TileWidth * TileHeight * 2 * 3]; // 4 subtiles x 2 triangles per tile x 3 vertex per triangle

            int vertexIdx = 0;
            int triangleIdx = 0;
            
            int mapWidth = MapTileWidth();
            int mapHeight = MapTileHeight();
            for (int tileX = 0; tileX < TileWidth; ++tileX)
            {
                for (int tileY = 0; tileY < TileHeight; ++tileY)
                {
                    int tx = StartTileX + tileX;
                    int ty = StartTileY + tileY;
                    if (tx >= mapWidth || ty >= mapHeight) continue;
                    var idTile = GetIDTile(StartTileX + tileX, StartTileY + tileY, MapLayerIdx);
                    if ( idTile >= 0 && idTile < m_autoTileMap.Tileset.SlotAons.Count)
                    {
                        float px0 = tileX * (CellSize().x);
                        float py0 = -tileY * (CellSize().y);
                        float px1 = (tileX + 1) * (CellSize().x);
                        float py1 = -(tileY + 1) * (CellSize().y);

                        m_vertices[vertexIdx + 0] = new Vector3(px0, py0, 0);
                        m_vertices[vertexIdx + 1] = new Vector3(px0, py1, 0);
                        m_vertices[vertexIdx + 2] = new Vector3(px1, py1, 0);
                        m_vertices[vertexIdx + 3] = new Vector3(px1, py0, 0);

                        m_colors[vertexIdx + 0] = new Color32(255, 255, 255, 255);
                        m_colors[vertexIdx + 1] = new Color32(255, 255, 255, 255);
                        m_colors[vertexIdx + 2] = new Color32(255, 255, 255, 255);
                        m_colors[vertexIdx + 3] = new Color32(255, 255, 255, 255);

                        m_triangles[triangleIdx + 0] = vertexIdx + 2;
                        m_triangles[triangleIdx + 1] = vertexIdx + 1;
                        m_triangles[triangleIdx + 2] = vertexIdx + 0;
                        m_triangles[triangleIdx + 3] = vertexIdx + 0;
                        m_triangles[triangleIdx + 4] = vertexIdx + 3;
                        m_triangles[triangleIdx + 5] = vertexIdx + 2;

                        float u0, u1, v0, v1;
                        
                        var slot = m_autoTileMap.Tileset.SlotAons[idTile];
                        Rect sprTileRect = slot.AtlasRecThumb;
                        Texture2D tex = m_autoTileMap.Tileset.TextureSlot;
                        u0 = sprTileRect.x / tex.width;
                        u1 = (sprTileRect.x + sprTileRect.width) / tex.width;
                        v0 = (sprTileRect.y + sprTileRect.height) / tex.height;
                        v1 = sprTileRect.y / tex.height;

                        m_uv[vertexIdx + 0] = new Vector3(u0, v0, 0);
                        m_uv[vertexIdx + 1] = new Vector3(u0, v1, 0);
                        m_uv[vertexIdx + 2] = new Vector3(u1, v1, 0);
                        m_uv[vertexIdx + 3] = new Vector3(u1, v0, 0);

                        // increment vectex and triangle idx
                        vertexIdx += 4;
                        triangleIdx += 6;
                    }
                }
            }

            // resize arrays
            System.Array.Resize(ref m_vertices, vertexIdx);
            System.Array.Resize(ref m_colors, vertexIdx);
            System.Array.Resize(ref m_uv, vertexIdx);
            System.Array.Resize(ref m_triangles, triangleIdx);
        }

        private void CheckMeshRenderer() {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
			if( meshRenderer == null )
			{
				meshRenderer = transform.gameObject.AddComponent<MeshRenderer>();
			}
            meshRenderer.sharedMaterial = this.AtlasMaterial();

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
#else
            meshRenderer.castShadows = false;
#endif
			meshRenderer.receiveShadows = false;

            
            if (m_meshFilter == null)
            {
                m_meshFilter = GetComponent<MeshFilter>();
                if (m_meshFilter == null)
                {
                    m_meshFilter = transform.gameObject.AddComponent<MeshFilter>();
                }
            }
        }

        private void CreateGroundChuck(){
            var obj = new GameObject();
            obj.name = "Ground";
            obj.transform.parent = transform;
            obj.transform.localPosition = new Vector3(0, 0, 0);
            obj.transform.localRotation = Quaternion.identity;
                        
            int totalHigh = 10;
            // GroundChuck[] gc = new GroundChuck[totalHigh];
            int mapWidth = m_autoTileMap.MapTileWidth;
            int mapHeight = m_autoTileMap.MapTileHeight;
            //Create by Tile && High
            bool[,] interior = null;
            bool[,] interiorNext = null;
            for( int hCurrent = totalHigh; hCurrent >= 0; hCurrent --){
                var objG = new GameObject();
                objG.name = hCurrent.ToString();
                objG.transform.parent = obj.transform;
                objG.transform.localPosition = new Vector3(0, 0, 0.5f * -hCurrent);
                objG.transform.localRotation = Quaternion.identity;
                GroundChuck groundChuck = objG.AddComponent<GroundChuck>();
                groundChuck.Configure( m_autoTileMap, MapLayerIdx, StartTileX, StartTileY, TileWidth, TileHeight, hCurrent, ref interior, ref interiorNext);
                interior = interiorNext;
                interiorNext = null;
                if(objG.transform.childCount == 0){
                    objG.transform.parent = null;
                    DestroyImmediate(objG);
                }
            }
            if(obj.transform.childCount == 0){
                obj.transform.parent = null;
                DestroyImmediate(obj);
            }
            /*
            //Create by Tile
            {
                bool[,] interior = null;
                bool[,] interiorNext = null;
                for (int _tileX = 0; _tileX < TileWidth; ++_tileX)
                {
                    for (int _tileY = 0; _tileY < TileHeight; ++_tileY)
                    {
                        int tx = StartTileX + _tileX;
                        int ty = StartTileY + _tileY;
                        if (tx >= mapWidth || ty >= mapHeight) continue;
                        var h = MyAutoTileMap.MapSelect.GetHighRef(tx, ty);
                        if( h >= 0 && h < totalHigh && gc[h] == null){
                            var objG = new GameObject();
                            objG.name = h.ToString();
                            objG.transform.parent = obj.transform;
                            objG.transform.localPosition = new Vector3(0, 0, 0.5f * -h);
                            // objG.transform.localPosition = new Vector3(0, 0, -h);
                            objG.transform.localRotation = Quaternion.identity;
                            GroundChuck groundChuck = objG.AddComponent<GroundChuck>();
                            groundChuck.Configure( MyAutoTileMap, MapLayerIdx, StartTileX, StartTileY, TileWidth, TileHeight, h, ref interior, ref interiorNext);
                            gc[h]= groundChuck;
                            interior = interiorNext;
                            interiorNext = null;
                        }
                    }
                }
            }
            { //Create by High
                bool[,] interior = null;
                bool[,] interiorNext = null;
                int minX = StartTileX * 2;
                int maxX = minX + TileWidth * 2;
                if(maxX > MyAutoTileMap.w2){
                    maxX = MyAutoTileMap.w2;
                }
                int minY = StartTileY * 2;
                int maxY = minY + TileHeight * 2;
                if(maxY > MyAutoTileMap.h2){
                    maxY = MyAutoTileMap.h2;
                }
                for (int x = minX; x < maxX; ++x)
                {
                    for (int y = minY; y < maxY; ++y)
                    {
                        var h = MyAutoTileMap.getHighX2(x,y);
                        if( h > 0 && h < totalHigh && gc[h] == null){
                            var objG = new GameObject();
                            objG.name = h.ToString();
                            objG.transform.parent = obj.transform;
                            objG.transform.localPosition = new Vector3(0, 0, 0.5f * -h);
                            // objG.transform.localPosition = new Vector3(0, 0, -h);
                            objG.transform.localRotation = Quaternion.identity;
                            GroundChuck groundChuck = objG.AddComponent<GroundChuck>();
                            groundChuck.Configure( MyAutoTileMap, MapLayerIdx, StartTileX, StartTileY, TileWidth, TileHeight, h, ref interior, ref interiorNext);
                            gc[h]= groundChuck;
                            interior = interiorNext;
                            interiorNext = null;
                        }
                    }
                }
            }
             */
        }

        private void RefreshEditorGround() {
            
            CheckMeshRenderer();
            
            if (m_meshFilter.sharedMesh == null)
            {
                m_meshFilter.sharedMesh = new Mesh();
                m_meshFilter.sharedMesh.hideFlags = HideFlags.DontSave;
            }
            Mesh mesh = m_meshFilter.sharedMesh;
            mesh.Clear();

            FillEditorGround();

            mesh.vertices = m_vertices;
            mesh.colors32 = m_colors;
            mesh.uv = m_uv;
            mesh.triangles = m_triangles;

            mesh.RecalculateNormals(); // allow using lights

            //Add MeshCollider for touch to move
            if(GetComponent<MeshCollider>() != null){
                GetComponent<MeshCollider>().sharedMesh = m_meshFilter.mesh;
            }
        }

        void FillEditorGround() {
            m_vertices = new Vector3[TileWidth * TileHeight * 4 * 4]; // 4 subtiles x 4 vertex per tile
            m_colors = new Color32[TileWidth * TileHeight * 4 * 4];
            m_uv = new Vector2[m_vertices.Length];
            m_triangles = new int[TileWidth * TileHeight * 4 * 2 * 3]; // 4 subtiles x 2 triangles per tile x 3 vertex per triangle

            int vertexIdx = 0;
            int triangleIdx = 0;
            //TODO: optimize updating only updated tiles inside the chunk
            // Dictionary<int, AutoTile> tileCache = new Dictionary<int, AutoTile>();
            int mapWidth = m_autoTileMap.MapTileWidth;
            int mapHeight = m_autoTileMap.MapTileHeight;
            for (int _tileX = 0; _tileX < TileWidth; ++_tileX)
            {
                for (int _tileY = 0; _tileY < TileHeight; ++_tileY)
                {
                    int tx = StartTileX + _tileX;
                    int ty = StartTileY + _tileY;
                    if (tx >= mapWidth || ty >= mapHeight) continue;
                    // int tileIdx = ty * MyAutoTileMap.MapTileWidth + tx;
                    AutoTile autoTile = m_autoTileMap.GetAutoTile(tx, ty, MapLayerIdx);
                    if (autoTile.Id >= 0 && m_autoTileMap.Tileset.IsExitSlot(autoTile.Id))
                    {
                        var slot = m_autoTileMap.Tileset.GetSlot(autoTile.Id);
                        int subTileXBase = _tileX << 1; // <<1 == *2
                        int subTileYBase = _tileY << 1; // <<1 == *2
                        for (int xf = 0; xf < 2; ++xf)
                        {
                            for (int yf = 0; yf < 2; ++yf)
                            {
                                #region Caculator
                                float u0 = 0, u1 = 0, v0 = 0, v1 = 0;
                                GroundChuck.CaculatorTilePartType(ref u0, ref u1, ref v0, ref v1, xf, yf, subTileXBase, subTileYBase, tx, ty, slot, autoTile, m_autoTileMap, MapLayerIdx);
                                #endregion //Caculator
                                #region Paint
                                {
                                    int subTileX = subTileXBase + xf;
                                    int subTileY = subTileYBase + yf;

                                    float px0 = subTileX * (m_autoTileMap.CellSize.x / 2f);
                                    float py0 = -subTileY * (m_autoTileMap.CellSize.y / 2f);
                                    float px1 = (subTileX + 1) * (m_autoTileMap.CellSize.x / 2f);
                                    float py1 = -(subTileY + 1) * (m_autoTileMap.CellSize.y / 2f);
                                    
                                    m_vertices[vertexIdx + 0] = new Vector3(px0, py0, 0);
                                    m_vertices[vertexIdx + 1] = new Vector3(px0, py1, 0);
                                    m_vertices[vertexIdx + 2] = new Vector3(px1, py1, 0);
                                    m_vertices[vertexIdx + 3] = new Vector3(px1, py0, 0);
                                    m_colors[vertexIdx + 0] = new Color32(255, 255, 255, 255);
                                    m_colors[vertexIdx + 1] = new Color32(255, 255, 255, 255);
                                    m_colors[vertexIdx + 2] = new Color32(255, 255, 255, 255);
                                    m_colors[vertexIdx + 3] = new Color32(255, 255, 255, 255);

                                    m_triangles[triangleIdx + 0] = vertexIdx + 2;
                                    m_triangles[triangleIdx + 1] = vertexIdx + 1;
                                    m_triangles[triangleIdx + 2] = vertexIdx + 0;
                                    m_triangles[triangleIdx + 3] = vertexIdx + 0;
                                    m_triangles[triangleIdx + 4] = vertexIdx + 3;
                                    m_triangles[triangleIdx + 5] = vertexIdx + 2;
                                    
                                    m_uv[vertexIdx + 0] = new Vector3(u0, v0, 0);
                                    m_uv[vertexIdx + 1] = new Vector3(u0, v1, 0);
                                    m_uv[vertexIdx + 2] = new Vector3(u1, v1, 0);
                                    m_uv[vertexIdx + 3] = new Vector3(u1, v0, 0);

                                    // increment vectex and triangle idx
                                    vertexIdx += 4;
                                    triangleIdx += 6;
                                }
                                #endregion
                            }
                        }
                    }
                }
            }

            // resize arrays
            System.Array.Resize(ref m_vertices, vertexIdx);
            System.Array.Resize(ref m_colors, vertexIdx);
            System.Array.Resize(ref m_uv, vertexIdx);
            System.Array.Resize(ref m_triangles, triangleIdx);
        }

        private int GetIDTile(int gridX, int gridY, int iLayer) {
            var autoTile = m_autoTileMap.GetAutoTile( gridX, gridY, iLayer);
            return autoTile.Id;
        }

        // private void GetSpriteAuto(int gridX, int gridY, int iLayer, int subTileX, int subTileY, out Rect sprTileRect, out Texture2D tex){
        //     var autoTile = AutoTileMap.GetAutoTile( gridX, gridY, MapLayerIdx);
        //     int tilePartIdx = (subTileY % 2) * 2 + (subTileX % 2);
        //     int spriteIdx = autoTile.TilePartsIdx[tilePartIdx];
        //     if(spriteIdx >= 0 && spriteIdx < AutoTileMap.Tileset.AutoTileRects.Count){
        //         sprTileRect = AutoTileMap.Tileset.AutoTileRects[spriteIdx];
        //     }else{
        //         Debug.Log("Error get sprTileRect: " + gridX + " : " + gridY);
        //         sprTileRect = AutoTileMap.Tileset.AutoTileRects[0];
        //     }
        //     tex = AutoTileMap.Tileset.AtlasTexture;
        // }

        private int MapTileWidth(){
            return m_autoTileMap.MapTileWidth;
        }

        private int MapTileHeight(){
            return m_autoTileMap.MapTileHeight;
        }

        private Vector2 CellSize(){
            return m_autoTileMap.CellSize;
        }

        private Material AtlasMaterial() {
            // var AtlasMaterial = new Material( Shader.Find("Sprites/Default") );
            var AtlasMaterial = new Material( Shader.Find("Unlit/Transparent") );
            // var AtlasMaterial = new Material( Shader.Find("Unlit/Texture") );
            var tileLayer = m_autoTileMap.MapSelect.TileData[MapLayerIdx];
            if(tileLayer.LayerType == eSlotAonTypeLayer.Ground){
                AtlasMaterial.mainTexture = m_autoTileMap.Tileset.DrawTileAonSetSelected.TextureThumb;
            }else{
                AtlasMaterial.mainTexture = m_autoTileMap.Tileset.TextureSlot;
            }
            return AtlasMaterial;
            // return AutoTileMap.Tileset.AtlasMaterial;
        }

        public void RefreshTileTrigger() {
            int mapWidth = MapTileWidth();
            int mapHeight = MapTileHeight();
            for (int tileX = 0; tileX < TileWidth; ++tileX)
            {
                for (int tileY = 0; tileY < TileHeight; ++tileY)
                {
                    int tx = StartTileX + tileX;
                    int ty = StartTileY + tileY;
                    if (tx >= mapWidth || ty >= mapHeight) continue;
                    var idTile = GetIDTile(tx, ty, MapLayerIdx);
                    if ( idTile >= 0 && idTile < m_autoTileMap.Tileset.SlotAons.Count)
                    {
                        var high = m_autoTileMap.MapSelect.GetHighRef(tx, ty) * 0.5f;
                        var triggerRef = m_autoTileMap.MapSelect.GetTriggerRef(tx, ty);
                        if(triggerRef != -1){
                            // float px0 = tx * (CellSize().x);
                            // float py0 = -ty * (CellSize().y);
                            float px0 = tileX * (CellSize().x);
                            float py0 = -tileY * (CellSize().y);
                            // float px1 = (tileX + 1) * (CellSize().x);
                            // float py1 = -(tileY + 1) * (CellSize().y);
                            var slot = m_autoTileMap.Tileset.SlotAons[idTile];
                            if(slot.TypeObj == eSlotAonTypeObj.Script || slot.TypeObj == eSlotAonTypeObj.Warps){
                                // GameObject obj = m_autoTileMap.CreateItem( "OBJ/DoorInteraction2", tx, ty);
                                GameObject obj = m_autoTileMap.CreateItem( "OBJ/CheckPoint2", tx, ty);
                                // obj.name = slot.TypeObj.ToString() + "_" + triggerRef + "_" + tx + ty;
                                var col = obj.AddComponent<BoxCollider>();
                                col.center = new Vector3(0, -0.5f, 0);
                                col.isTrigger =true;
                                // Add TriggerDetal
                                TriggerDetail detail = new TriggerDetail(){
                                    refMap = m_autoTileMap.MapIdxSelect,
                                    typeObj = slot.TypeObj,
                                    refId = triggerRef,
                                    isCreateFromMap = true,
                                    x = tx,
                                    y = ty
                                };
                                var triggerDetal = obj.AddComponent<TriggerDetailBehaviour>();
                                triggerDetal.Data = detail;
                                obj.name = detail.Hash;
                            }else if(slot.TypeObj == eSlotAonTypeObj.Person){
                                TriggerDetail detail = new TriggerDetail(){
                                    refMap = m_autoTileMap.MapIdxSelect,
                                    typeObj = slot.TypeObj,
                                    refId = triggerRef,
                                    isCreateFromMap = true,
                                    x = tx,
                                    y = ty
                                };
                                m_autoTileMap.ShowNPC(detail);
                            }
                        }
                    }
                }
            }
        }

        private void RefreshTileOverlay(bool isEditorMode) {
            int mapWidth = MapTileWidth();
            int mapHeight = MapTileHeight();
            for (int tileX = 0; tileX < TileWidth; ++tileX)
            {
                for (int tileY = 0; tileY < TileHeight; ++tileY)
                {
                    int tx = StartTileX + tileX;
                    int ty = StartTileY + tileY;
                    if (tx >= mapWidth || ty >= mapHeight) continue;
                    RefreshTileOverlay( tx, ty, isEditorMode);
                }
            }
        }

        public void ClearTileOverlay( int tx, int ty){
            string tile_hash = string.Format("{0}_{1}", tx, ty);
            Transform p = transform.Find(tile_hash);
            if( p != null){
                DestroyImmediate(p.gameObject);
            }
        }

        public void RefreshTileOverlay( int tx, int ty, bool isEditorMode){
            var idTile = GetIDTile(tx, ty, MapLayerIdx);
            if ( idTile >= 0 && idTile < m_autoTileMap.Tileset.SlotAons.Count)
            {
                var slot = m_autoTileMap.Tileset.SlotAons[idTile];
                var triggerRef = m_autoTileMap.MapSelect.GetTriggerRef(tx, ty);
                var overlayRef = m_autoTileMap.MapSelect.GetOverlayRef(tx, ty);
                var rotateRef = m_autoTileMap.MapSelect.GetRotateRef(tx, ty);
                string tile_hash = string.Format("{0}_{1}", tx, ty);
                string child_name_hash = string.Format("{0}_{1}_{2}_{3}", idTile, triggerRef, overlayRef, rotateRef);
                Transform p = transform.Find(tile_hash);
                if( p == null){
                    var obj = new GameObject();
                    obj.name = tile_hash;
                    obj.transform.parent = transform;
                    obj.transform.localPosition = Vector3.zero;
                    p = obj.transform;
                }else{
                    if(isEditorMode){
                        //Remove all orther child
                        // for( int i = p.childCount -1; i >= 0; i--){
                        //     var c = p.GetChild(i).gameObject;
                        //     if(c.name != child_name_hash){
                        //         DestroyImmediate(c);    
                        //     }
                        // }
                        for( int i = p.childCount -1; i >= 0; i--){
                            var c = p.GetChild(i).gameObject;
                            DestroyImmediate(c);    
                        }
                    }
                    Transform cHas = p.Find(child_name_hash);
                    if(cHas != null){
                        if(cHas.name == child_name_hash){
                            return;
                        }
                    }
                }
                float px0 = (tx + slot.Size.x - StartTileX) * (CellSize().x);
                float py0 = -(ty - StartTileY) * (CellSize().y);
                float w = slot.Size.x * (CellSize().x);
                float h = slot.Size.y * (CellSize().y);
                float high = 0;
                if(isEditorMode == false){ //IsEditor mode
                    high = m_autoTileMap.MapSelect.GetHighRef(tx, ty) * 0.5f;
                }
                if(slot.TypeObj == eSlotAonTypeObj.Default){
                    #region Default
                    if(slot.idRef < 0  || slot.idRef >= m_autoTileMap.Tileset.OverlayAonSetSelected.OverlayAons.Count)
                        return;
                    var obj = new GameObject();
                    obj.transform.parent = p;
                    obj.transform.localPosition = new Vector3( px0 - w, py0, -high);
                    obj.transform.localRotation = Quaternion.identity;
                    obj.name = child_name_hash;
                    var data = m_autoTileMap.Tileset.OverlayAonSetSelected.OverlayAons[slot.idRef];
                    #region Art
                    {
                        var path = "OBJ/";
                        path += data.art.ToString();
                        var prefab = Resources.Load<GameObject>(path);
                        // var obj = new GameObject();
                        if(prefab != null){
                            var art = Instantiate(prefab);
                            art.transform.parent = obj.transform;
                            // float scaleArt = slot.Size.x / 10.0f; 
                            // art.transform.localScale = new Vector3( scaleArt, scaleArt, scaleArt);
                            art.transform.Rotate( -90, 0 , 0);
                            // art.transform.Rotate( 0, 180 , 0);
                            art.transform.localPosition = Vector3.zero + new Vector3( w * 0.5f, -h * 0.5f, 0);
                            art.name = data.art.ToString();
                        }
                    }
                    #endregion
                    #region NavMeshObstacle
                    if(data.PhysicSize != Vector2.zero){
                        // data.PhysicSize
                        var col = obj.AddComponent<NavMeshObstacle>();
                        col.center = new Vector3(w * 0.5f, -h * 0.5f, -0.5f);
                        col.size = new Vector3(data.PhysicSize.x, data.PhysicSize.y, 1);
                        col.carving = true;
                    }
                    #endregion
                    #endregion
                }else if(slot.TypeObj == eSlotAonTypeObj.Tileset3D){
                    #region Tileset3D
                    if(slot.idRef < 0  || slot.idRef >= m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons.Count)
                        return;
                    var obj = new GameObject();
                    obj.transform.parent = p;
                    obj.transform.localPosition = new Vector3( px0 - w, py0, -high);
                    obj.transform.localRotation = Quaternion.identity;
                    obj.name = child_name_hash;
                    #region Art
                    bool addPhysic = true;
                    {
                        var data = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[slot.idRef];
                        if(data.type == DrawTile3DAon.eDrawTile3DAonType.Tile_4){
                            AddOverlay3DRow4( obj.transform, data, idTile, tx, ty);
                        }else if(data.type == DrawTile3DAon.eDrawTile3DAonType.Tile_6){
                            AddOverlay3DRow6( obj.transform, data, idTile, tx, ty);
                        }else if(data.type == DrawTile3DAon.eDrawTile3DAonType.Tile_SkyBox){
                            AddOverlay3DRowSkyBox( obj.transform, data, idTile, tx, ty);
                            addPhysic = false;
                        }else if(data.type == DrawTile3DAon.eDrawTile3DAonType.Tile_Fence){
                            AddOverlayFence( obj.transform, data, idTile, tx, ty);
                        }
                    }
                    #endregion
                    #region NavMeshObstacle
                    if(addPhysic) {
                        // data.PhysicSize
                        var col = obj.AddComponent<NavMeshObstacle>();
                        col.center = new Vector3(w * 0.5f, -h * 0.5f, -0.5f);
                        col.size = new Vector3(1, 1, 1);
                        col.carving = true;
                    }
                    #endregion
                    #endregion
                }else if(slot.TypeObj == eSlotAonTypeObj.House){
                    #region House
                    if(overlayRef < 0  || overlayRef >= m_autoTileMap.MapSelect.HouseData.Count)
                        return;
                    var obj = new GameObject();
                    obj.transform.parent = p;
                    obj.transform.localPosition = new Vector3( px0 - w, py0, -high);
                    obj.transform.localRotation = Quaternion.identity;
                    obj.name = child_name_hash;
                    var houseData = m_autoTileMap.MapSelect.HouseData[overlayRef];
                    #region Art
                    {
                        var houseList = m_autoTileMap.Tileset.HouseList;
                        if( houseData.IdxArt < 0 || houseData.IdxArt >= houseList.Length)
                            return;
                        var path = "MODEL/BUILDING/Exterior/Prefab/";
                        path += houseList[houseData.IdxArt];
                        var prefab = Resources.Load<GameObject>(path);
                        if(prefab != null){
                            var art = Instantiate(prefab);
                            art.transform.parent = obj.transform;
                            art.name = houseList[houseData.IdxArt];
                            art.transform.localPosition = new Vector3( w * 0.5f, -h * 0.5f, 0f);
                            art.transform.localRotation = Quaternion.identity;
                            art.transform.Rotate( -90, 0 , 0);
                            art.transform.Rotate( 0, 180 - rotateRef, 0);
                            // art.transform.Rotate( 0, 180 + rotateRef, 0);
                            // art.transform.Rotate( 0, rotateRef , 0);
                            if(isEditorMode == true){ //IsEditor mode move to posZ = 0
                                var o = art.transform.position;
                                o.z = 0;
                                art.transform.position = o;
                            }
                        }else
                        {
                            Debug.LogError("Not found asset");
                        }
                    }
                    #endregion
                    #region Door
                    if(isEditorMode == false)
                    {
                        GameObject door = m_autoTileMap.CreateDoorForHouse( overlayRef, slot, houseData, tx, ty);
                        if( door != null){
                            // Add TriggerDetal
                            TriggerDetail detail = new TriggerDetail(){
                                refMap = m_autoTileMap.MapIdxSelect,
                                typeObj = slot.TypeObj,
                                refId = overlayRef,
                                isCreateFromMap = true,
                                x = tx,
                                y = ty
                            };
                            var triggerDetal = door.AddComponent<TriggerDetailBehaviour>();
                            triggerDetal.Data = detail;
                            door.name = detail.Hash;
                        }
                    }
                    #endregion
                    /*
                    #region NavMeshObstacle
                    {
                        var col = obj.AddComponent<NavMeshObstacle>();
                        col.center = new Vector3(w * 0.5f, -h * 0.5f, -0.5f);
                        col.size = new Vector3(w, h, 1);
                        col.carving = true;
                    }
                    #endregion
                    */
                    #endregion
                }else if(slot.TypeObj == eSlotAonTypeObj.Filler3D) {
                    #region Filler3D
                    var path = m_autoTileMap.Tileset.GetPathFiler3D(slot.idRef, overlayRef);
                    if(path != null){
                        var prefab = Resources.Load<GameObject>(path);
                        if(prefab != null){
                            var obj = new GameObject();
                            obj.transform.parent = p;
                            obj.transform.localPosition = new Vector3( px0 - w, py0, -high);
                            // obj.transform.localRotation = Quaternion.identity;
                            obj.name = child_name_hash;
                            var art = Instantiate(prefab);
                            art.transform.SetParent(obj.transform, false);
                            // art.transform.parent = obj.transform;
                            art.name = path;
                            art.transform.Rotate( 0, 0 , rotateRef);
                            art.transform.Rotate( -90, 0 , 0);
                            art.transform.Rotate( 0, 180 , 0);
                            art.transform.localPosition = Vector3.zero + new Vector3( w * 0.5f, -h * 0.5f, 0f);
                            if(isEditorMode == true){ //IsEditor mode move to posZ = 0
                                var o = art.transform.position;
                                o.z = 0;
                                art.transform.position = o;
                            }
                        }
                    }
                    #endregion
                }
            }else
            {
                if(IS_ALWAY_SHOW_OVERLAY){
                    ClearTileOverlay(tx, ty);
                }
            }
        }
        
        public int _getTileCount( bool top, bool bot, bool left, bool right ) {
            int count = 0;
            count += (top ? 1 : 0);
            count += (left ? 1 : 0);
            count += (right ? 1 : 0);
            count += (bot ? 1 : 0);
            return count;
        }

        public int _getTileBitmask( bool top, bool bot, bool left, bool right ) {
            int count = 0;
            count += (top ? 1 : 0);
            count += (left ? 8 : 0);
            count += (right ? 2 : 0);
            count += (bot ? 4 : 0);
            return count;
		}
        
        /*
        private void AddSideTileGroundPlayMode() {
            // Debug.Log("Add Ground");
            int idRefFloor = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.idRefFloor;
            if(idRefFloor == -1)
                return;
            int mapWidth = MapTileWidth();
            int mapHeight = MapTileHeight();
            { //Ground
                var obj_GroundSide = new GameObject();
                obj_GroundSide.name = "GroundSide";
                var dataArt = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRefFloor];
                int offset = 1;
                int minX = StartTileX <= 0 ? -offset : StartTileX;
                int maxX = StartTileX + TileWidth >= mapWidth ? mapWidth + offset : StartTileX + TileWidth;
                int minY = StartTileY <= 0 ? -offset : StartTileY;
                int maxY = StartTileY + TileHeight >= mapHeight ? mapHeight + offset : StartTileY + TileHeight;
                for (int tx = minX; tx < maxX; ++tx)
                {
                    if (tx >= mapWidth + offset) continue;
                    for (int ty = minY; ty < maxY; ++ty)
                    {
                        if (ty >= mapHeight + offset) continue;
                        var idTile = GetIDTile(tx, ty, MapLayerIdx);
                        if ( idTile < 0)
                        {
                            GameObject obj = null;
                            AddOverlay3DRow6( ref obj, dataArt, m_autoTileMap.GetAutoTileIsGround( tx, ty, MapLayerIdx ), tx, ty, "", true);
                            if(obj != null){
                                obj.transform.parent = obj_GroundSide.transform;
                                float px0 = (tx - StartTileX) * (CellSize().x);
                                float py0 = -(ty - StartTileY) * (CellSize().y);
                                obj.transform.localPosition = new Vector3( px0, py0, 0f);
                                obj.transform.localRotation = Quaternion.identity;
                                obj.name = string.Format("Floor: {0}, {1}", ty, tx);
                            }
                        }
                    }
                }
                
                if(obj_GroundSide.transform.childCount > 0){
                    #if false// Combine terrain
                    CombineMesh( obj_GroundSide);
                    #endif
                    obj_GroundSide.transform.parent = transform;
                    obj_GroundSide.transform.localPosition = Vector3.zero;
                }else{
                    DestroyImmediate(obj_GroundSide);
                }
            }
        }

        private void AddSideTileWarterPlayMode( string name, DrawTile3D tileWater, ref Material material_water) {
            // Debug.Log("Add Warter");
            // var tileWater = MyAutoTileMap.Tileset.DrawTile3DAonSetSelected.WaterTile;
            var prefabWarter = Resources.Load<GameObject>(tileWater.prefab_name);
            if(prefabWarter == null){
                return;
            }
            GameObject obj_warter = null;
            int sizeWater = 2;
            int mapWidth = MapTileWidth();
            int mapHeight = MapTileHeight();
            // int offset = sizeWater;
            int offset = 0;
            int minX = StartTileX <= 0 ? -offset : StartTileX;
            int maxX = StartTileX + TileWidth >= mapWidth ? mapWidth + offset : StartTileX + TileWidth;
            int minY = StartTileY <= 0 ? -offset : StartTileY;
            int maxY = StartTileY + TileHeight >= mapHeight ? mapHeight + offset : StartTileY + TileHeight;
            
            for (int tx = minX; tx < maxX; ++tx)
            {
                if (tx >= mapWidth + offset) continue;
                if(tx % sizeWater != 0)
                    continue;
                for (int ty = minY; ty < maxY; ++ty)
                {
                    if (ty >= mapHeight + offset) continue;
                    if(ty % sizeWater != 0) continue;
                    bool isFillAllGround = true;
                    bool asLeastOneWarter = false;
                    for (int tX2 = tx - 1; tX2 < tx + sizeWater + 1 && isFillAllGround; ++tX2)
                    {
                        for (int tY2 = ty - 1; tY2 < ty + sizeWater + 1 && isFillAllGround; ++tY2)
                        {
                            var idTile = GetIDTile(tX2, tY2, MapLayerIdx);
                            if ( idTile < 0){
                                isFillAllGround = false;
                            }
                            var idTileOverlay = GetIDTile(tX2, tY2, 1);
                            if(idTileOverlay == 30){ // 30 is water
                                asLeastOneWarter = true;
                            }
                        }
                    }
                    if(isFillAllGround == false && asLeastOneWarter == true){
                        if(obj_warter == null){
                            obj_warter = new GameObject();
                            obj_warter.name = name;
                        }
                        AddSideTileWater( tx, ty, 0, new Vector3(sizeWater, 0, 0), tileWater.prefab_name, prefabWarter, sizeWater, obj_warter.transform);
                    }
                }
            }
            if( obj_warter != null){
                obj_warter.transform.parent = transform;
                obj_warter.transform.localPosition = new Vector3( tileWater.offsetPos.x, tileWater.offsetPos.y, tileWater.offsetPos.z);
            }
            if( obj_warter != null){
                #if false //Combine warter
                CombineMesh( obj_warter);
                if( material_water != null){
                    var render = obj_warter.GetComponent<Renderer>();
                    render.sharedMaterial = material_water; 
                }
                #else
                if( material_water != null){
                    var render = obj_warter.GetComponentsInChildren<Renderer>();
                    foreach (var item in render)
                    {
                        item.sharedMaterial = material_water;    
                    }
                }
                #endif
            }
            
        }
        */

        /*
        private void AddSideTileGroundPlayModeClassic() {
            int mapWidth = MyAutoTileMap.MapTileWidth;
            int mapHeight = MyAutoTileMap.MapTileHeight;
            {//warter
                int sizeWater = 16;
                for (int tileX = 0; tileX < mapWidth + sizeWater; ++tileX)
                {
                    if(tileX % sizeWater != 0)
                        continue;
                    for (int tileY = 0; tileY < mapHeight + sizeWater; ++tileY)
                    {
                        if(tileY % sizeWater == 0){
                            AddSideTileWater( tileX, tileY, 0, new Vector3(0, 0, 0.3f), "Water", "Ocean/");
                        }
                    }
                }
            }
            for (int tileY = 0; tileY < mapHeight; ++tileY)
            {
                //Top
                AddSideTileGroundPlayModeClassic( 0, tileY, 90, Vector3.zero, "tile_mgrass_side");
                //Bot
                AddSideTileGroundPlayModeClassic( mapWidth + 1, tileY, -90, Vector3.zero, "tile_mgrass_side");
            }
            for (int tileX = 0; tileX < mapWidth; ++tileX)
            {
                //Left
                AddSideTileGroundPlayModeClassic( tileX + 1, -1, 180, Vector3.zero, "tile_mgrass_side");
                //Right
                AddSideTileGroundPlayModeClassic( tileX + 1, mapHeight, 0, Vector3.zero, "tile_mgrass_side");
            }
            //
            AddSideTileGroundPlayModeClassic( 0, -1, 180, Vector3.zero, "tile_mgrass_top");
            AddSideTileGroundPlayModeClassic( mapWidth + 1, mapHeight, 0, Vector3.zero, "tile_mgrass_top");
            //
            AddSideTileGroundPlayModeClassic( mapWidth + 1, -1, -90, Vector3.zero, "tile_mgrass_top");
            AddSideTileGroundPlayModeClassic( 0, mapHeight, 90, Vector3.zero, "tile_mgrass_top");
        }

        private void AddSideTileGroundPlayModeClassic(int tileX, int tileY, int rotate, Vector3 offset, string prefab_name, string p = "Tileset/_prefab/"){
            float px0 = tileX * (CellSize().x);
            float py0 = -tileY * (CellSize().y);
            float w = 1;
            float h = 1;
            var obj = new GameObject();
            obj.transform.parent = transform;
            obj.transform.localPosition = Vector3.zero + new Vector3( px0 - w, py0, 0f);
            obj.transform.localRotation = Quaternion.identity;
            obj.name = prefab_name;
            #region Art
            {
                var path = p + prefab_name;
                var prefab = Resources.Load<GameObject>(path);
                // var obj = new GameObject();
                if(prefab != null){
                    var art = Instantiate(prefab);
                    art.transform.parent = obj.transform;
                    // float scaleArt = slot.Size.x / 10.0f; 
                    // art.transform.localScale = new Vector3( scaleArt, scaleArt, scaleArt);
                    art.transform.Rotate( -90, 0 , 0);
                    art.transform.Rotate( 0, rotate , 0);
                    art.transform.localPosition = Vector3.zero + new Vector3( w * 0.5f, -h * 0.5f, 0.5f) + offset;
                    art.name = prefab_name;
                }
            }
            #endregion
        }
        */

        private void AddSideTileWater(int tileX, int tileY, int rotate, Vector3 offset, string prefab_name, GameObject prefab, int sizeWater, Transform parent){
            float px0 = (tileX - StartTileX) * (CellSize().x);
            float py0 = -(tileY - StartTileY) * (CellSize().y);
            float w = sizeWater;
            float h = sizeWater;
            var obj = new GameObject();
            obj.transform.parent = parent;
            obj.transform.localPosition = Vector3.zero + new Vector3( px0 - w, py0, 0f);
            obj.transform.localRotation = Quaternion.identity;
            obj.name = prefab_name;
            #region Art
            {
                // if(prefab != null)
                {
                    var art = Instantiate(prefab);
                    art.transform.parent = obj.transform;
                    // art.transform.localScale = new Vector3( 10, 10, 10);
                    art.transform.Rotate( -90, 0 , 0);
                    art.transform.Rotate( 0, rotate , 0);
                    art.transform.localPosition = Vector3.zero + new Vector3( w * 0.5f, -h * 0.5f, 0.5f) + offset;
                    art.name = prefab_name;
                }
            }
            #endregion
        }

        private void AddOverlay3DRow6( Transform parent, DrawTile3DAon data, int idTile, int tileX, int tileY){
            int subTileXBase = tileX << 1; // <<1 == *2
            int subTileYBase = tileY << 1; // <<1 == *2
            for (int xf = 0; xf < 2; ++xf)
            {
                for (int yf = 0; yf < 2; ++yf)
                {
                    GroundChuck.eTilePartType tilePartType;
                    int tilePartX = 0;
                    int tilePartY = 0;
                    #region Caculator
                    {
                        int tile_x = subTileXBase + xf;
                        int tile_y = subTileYBase + yf;
                        
                        if (tile_x % 2 == 0 && tile_y % 2 == 0) //A
                        {
                            tilePartType = GroundChuck._getTileByNeighbours( idTile, 
                                            m_autoTileMap.GetAutoTile( tileX, tileY-1, MapLayerIdx ).Id, //V 
                                            m_autoTileMap.GetAutoTile( tileX-1, tileY, MapLayerIdx ).Id, //H 
                                            m_autoTileMap.GetAutoTile( tileX-1, tileY-1, MapLayerIdx ).Id  //D
                                            );
                            tilePartX = GroundChuck.aTileAff[ (int)tilePartType, 0 ];
                            tilePartY = GroundChuck.aTileAff[ (int)tilePartType, 1 ];
                        } 
                        else if (tile_x % 2 != 0 && tile_y % 2 == 0) //B
                        {
                            tilePartType = GroundChuck._getTileByNeighbours( idTile, 
                                            m_autoTileMap.GetAutoTile( tileX, tileY-1, MapLayerIdx ).Id, //V 
                                            m_autoTileMap.GetAutoTile( tileX+1, tileY, MapLayerIdx ).Id, //H 
                                            m_autoTileMap.GetAutoTile( tileX+1, tileY-1, MapLayerIdx ).Id  //D
                                            );
                            tilePartX = GroundChuck.aTileBff[ (int)tilePartType, 0 ];
                            tilePartY = GroundChuck.aTileBff[ (int)tilePartType, 1 ];
                        }
                        else if (tile_x % 2 == 0 && tile_y % 2 != 0) //C
                        {
                            
                            tilePartType = GroundChuck._getTileByNeighbours( idTile, 
                                            m_autoTileMap.GetAutoTile( tileX, tileY+1, MapLayerIdx ).Id, //V 
                                            m_autoTileMap.GetAutoTile( tileX-1, tileY, MapLayerIdx ).Id, //H 
                                            m_autoTileMap.GetAutoTile( tileX-1, tileY+1, MapLayerIdx ).Id  //D
                                            );
                            tilePartX = GroundChuck.aTileCff[ (int)tilePartType, 0 ];
                            tilePartY = GroundChuck.aTileCff[ (int)tilePartType, 1 ];
                        }
                        else //if (tile_x % 2 != 0 && tile_y % 2 != 0) //D
                        {
                            tilePartType = GroundChuck._getTileByNeighbours( idTile,
                                            m_autoTileMap.GetAutoTile( tileX, tileY+1, MapLayerIdx ).Id, //V 
                                            m_autoTileMap.GetAutoTile( tileX+1, tileY, MapLayerIdx ).Id, //H 
                                            m_autoTileMap.GetAutoTile( tileX+1, tileY+1, MapLayerIdx ).Id  //D
                                            );
                            tilePartX = GroundChuck.aTileDff[ (int)tilePartType, 0 ];
                            tilePartY = GroundChuck.aTileDff[ (int)tilePartType, 1 ];
                        }
                    }
                    #endregion
                    #region  Art
                    DrawTile3D tile3DAon = null;
                    int offsetRotate = 0;
                    // tile3DAon = data.interior;
                    if(tilePartType == GroundChuck.eTilePartType.EXT_CORNER){
                        tile3DAon = data.corner_2;
                    }else if(tilePartType == GroundChuck.eTilePartType.H_SIDE || tilePartType == GroundChuck.eTilePartType.V_SIDE){
                        tile3DAon = data.side;
                    }else if(tilePartType == GroundChuck.eTilePartType.INT_CORNER){
                        tile3DAon = data.corner_3;
                    }else{
                        tile3DAon = data.interior;
                    }
                    int [,] r = {
                        {  0,  0,  270, 0}, // 0
                        {  0,  0,  180, 90}, // 1
                        {  90,  180,  180, 180}, // 2
                        {  90,  0,  0, 270}, // 3
                        {  90,  0,  0, 270}, // 4
                        {  0,  0,  0, 270} // 5
                    };
                    if(tilePartX >= 0 && tilePartX <= 3 && tilePartY >= 0 && tilePartY <= 5){
                        offsetRotate = r[ tilePartY, tilePartX];
                    }
                    if(tile3DAon != null){
                        var prefab = Resources.Load<GameObject>(tile3DAon.prefab_name);
                        // var obj = new GameObject();
                        if(prefab != null){
                            var art = Instantiate(prefab);
                            art.transform.parent = parent;
                            if(data.scalePrefab != 1){
                                art.transform.localScale = new Vector3( data.scalePrefab, data.scalePrefab, data.scalePrefab);
                            }
                            art.transform.localPosition =  new Vector3( 0.5f + (xf == 0 ? -data.offset : data.offset), -0.5f + (yf == 0 ? data.offset : -data.offset), 0f) + tile3DAon.offsetPos;
                            // art.transform.localPosition =  new Vector3(  xf == 0 ? 0 : 1, yf == 0 ? 0 : -1, 0f) + tile3DAon.offsetPos;
                            art.transform.Rotate( -90, 0 , 0);
                            art.transform.Rotate( 0, 180 , 0);
                            art.transform.Rotate( tile3DAon.offsetRotate.x, tile3DAon.offsetRotate.y + offsetRotate, tile3DAon.offsetRotate.z);
                            art.name = tilePartType.ToString() + "_yx_" + tilePartY + "_" + tilePartX + "_r_" + offsetRotate.ToString();
                        }
                    }
                    #endregion
                }
            }
        }

        private void AddOverlay3DRow4( Transform parent, DrawTile3DAon data, int idTile, int tileX, int tileY){
            Vector3 offsetP = Vector3.zero;
            int offsetRotate = 0;
            bool top = idTile == m_autoTileMap.GetAutoTile( tileX, tileY+1, MapLayerIdx ).Id;
            bool bot = idTile == m_autoTileMap.GetAutoTile( tileX, tileY-1, MapLayerIdx ).Id;
            bool left = idTile == m_autoTileMap.GetAutoTile( tileX-1, tileY, MapLayerIdx ).Id;
            bool right = idTile == m_autoTileMap.GetAutoTile( tileX+1, tileY, MapLayerIdx ).Id;
            int tile_count = _getTileCount( top, bot, left, right);
            int bitmask = _getTileBitmask( top, bot, left, right);
            //  1
            //8   2
            //  4
            DrawTile3D tile3DAon = null;
            if(tile_count == 0){
                tile3DAon = data.interior;
                offsetP.y = data.offset;
            }else if(tile_count == 1){
                tile3DAon = data.side;
                if(bitmask == 2){
                    offsetRotate = 0;
                    offsetP.y = data.offset;
                }else if(bitmask == 1){
                    offsetRotate = 90;
                    offsetP.x = data.offset;
                }else if(bitmask == 8){
                    offsetRotate = 180;
                    offsetP.y = -data.offset;
                }else if(bitmask == 4){
                    offsetRotate = 270;
                    offsetP.x = -data.offset;
                }
            }else if(tile_count == 2){
                //Side W or H: 
                if(bitmask == 5){//Side_H
                    tile3DAon = data.side;
                    offsetRotate = 90;
                    offsetP.x = data.offset;
                }else if(bitmask == 10){//Side_W
                    tile3DAon = data.side;
                    offsetRotate = 0;
                    offsetP.y = data.offset;
                }else{
                    tile3DAon = data.corner_2;
                    if(bitmask == 3){ // Top, Left
                        offsetRotate = 180;
                        offsetP.x = data.offset;
                        offsetP.y = -data.offset;
                    }else if(bitmask == 9){ // Top, Right
                        offsetRotate = 270;
                        offsetP.x = -data.offset;
                        offsetP.y = -data.offset;
                    }else if(bitmask == 12){ // Bot, Right
                        offsetRotate = 0;
                        offsetP.x = -data.offset;
                        offsetP.y = data.offset;
                    }else if(bitmask == 6){ //Bot, Left
                        offsetRotate = 90;
                        offsetP.x = data.offset;
                        offsetP.y = data.offset;
                    }
                }
            }else if(tile_count == 3){
                tile3DAon = data.corner_3;
                if(bitmask == 7){ // Right
                    offsetRotate = 90;
                    offsetP.x = data.offset;
                }else if(bitmask == 11){ // Top
                    offsetRotate = 180;
                    offsetP.y = -data.offset;
                }else if(bitmask == 13){ // Left
                    offsetRotate = 270;
                    offsetP.x = -data.offset;
                }else if(bitmask == 14){ // Bot
                    offsetRotate = 0;
                    offsetP.y = data.offset;
                }
            }else{
                tile3DAon = data.interior;
                offsetP.y = data.offset;
            }

            //  1
            //8   2
            //  4
            if(tile3DAon != null){
                string path = tile3DAon.prefab_name;
                var prefab = Resources.Load<GameObject>(path);
                // var obj = new GameObject();
                if(prefab != null){
                    var art = Instantiate(prefab);
                    art.transform.parent = parent;
                    if(data.scalePrefab != 1){
                        art.transform.localScale = new Vector3( data.scalePrefab, data.scalePrefab, data.scalePrefab);
                    }
                    art.transform.Rotate( -90, 0 , 0);
                    art.transform.Rotate( 0, 180 , 0);
                    art.transform.Rotate( tile3DAon.offsetRotate.x, tile3DAon.offsetRotate.y + offsetRotate, tile3DAon.offsetRotate.z);
                    art.transform.localPosition =  new Vector3( 0.5f, -0.5f, 0f) + tile3DAon.offsetPos + offsetP;
                    art.name = tile_count.ToString() + "_" + bitmask.ToString() + "_r_" + offsetRotate;
                }
            }
        }

        private void AddOverlay3DRowSkyBox( Transform parent, DrawTile3DAon data, int idTile, int tileX, int tileY){
            Vector3 offsetP = Vector3.zero;
            int offsetRotate = 0;
            // int idTop = m_autoTileMap.GetAutoTile( tileX, tileY+1, 0 ).Id;
            // int idBot = m_autoTileMap.GetAutoTile( tileX, tileY-1, 0 ).Id;
            // int idLeft = m_autoTileMap.GetAutoTile( tileX-1, tileY, 0 ).Id;
            // int idRight = m_autoTileMap.GetAutoTile( tileX+1, tileY, 0 ).Id;
            // bool top = idTop >= 0;
            // bool bot = idBot >= 0;
            // bool left = idLeft >= 0;
            // bool right = idRight >= 0;

            // int idTopOverlay = m_autoTileMap.GetAutoTile( tileX, tileY+1, 1 ).Id;
            // int idBotOverlay = m_autoTileMap.GetAutoTile( tileX, tileY-1, 1 ).Id;
            // int idLeftOverlay = m_autoTileMap.GetAutoTile( tileX-1, tileY, 1 ).Id;
            // int idRightOverlay = m_autoTileMap.GetAutoTile( tileX+1, tileY, 1 ).Id;

            bool top = idTile == m_autoTileMap.GetAutoTile( tileX, tileY+1, MapLayerIdx ).Id;
            bool bot = idTile == m_autoTileMap.GetAutoTile( tileX, tileY-1, MapLayerIdx ).Id;
            bool left = idTile == m_autoTileMap.GetAutoTile( tileX-1, tileY, MapLayerIdx ).Id;
            bool right = idTile == m_autoTileMap.GetAutoTile( tileX+1, tileY, MapLayerIdx ).Id;
            // bool top = (idTile == idTopOverlay) || idTop >= 0;
            // bool bot = (idTile == idBotOverlay) || idBot >= 0;
            // bool left = (idTile == idLeftOverlay) || idLeft >= 0;
            // bool right = (idTile == idRightOverlay) || idRight >= 0;

            int tile_count = _getTileCount( top, bot, left, right);
            int bitmask = _getTileBitmask( top, bot, left, right);
            //  1
            //8   2
            //  4
            DrawTile3D tile3DAon = null;
            if(tile_count == 0){
                tile3DAon = data.interior;
                offsetP.y = data.offset;
            }else if(tile_count == 1){
                tile3DAon = data.side;
                if(bitmask == 2){
                    offsetRotate = 0;
                    offsetP.y = data.offset;
                }else if(bitmask == 1){
                    offsetRotate = 90;
                    offsetP.x = data.offset;
                }else if(bitmask == 8){
                    offsetRotate = 180;
                    offsetP.y = -data.offset;
                }else if(bitmask == 4){
                    offsetRotate = 270;
                    offsetP.x = -data.offset;
                }
            }else if(tile_count == 2){
                //Side W or H: 
                if(bitmask == 5){//Side_H
                    tile3DAon = data.side;
                    offsetRotate = 90;
                    offsetP.x = data.offset;
                }else if(bitmask == 10){//Side_W
                    tile3DAon = data.side;
                    offsetRotate = 0;
                    offsetP.y = data.offset;
                }else{
                    tile3DAon = data.corner_2;
                    if(bitmask == 3){ // Top, Left
                        offsetRotate = 180;
                        offsetP.x = data.offset;
                        offsetP.y = -data.offset;
                    }else if(bitmask == 9){ // Top, Right
                        offsetRotate = 270;
                        offsetP.x = -data.offset;
                        offsetP.y = -data.offset;
                    }else if(bitmask == 12){ // Bot, Right
                        offsetRotate = 0;
                        offsetP.x = -data.offset;
                        offsetP.y = data.offset;
                    }else if(bitmask == 6){ //Bot, Left
                        offsetRotate = 90;
                        offsetP.x = data.offset;
                        offsetP.y = data.offset;
                    }
                }
            }else if(tile_count == 3){
                tile3DAon = data.corner_3;
                if(bitmask == 7){ // Right
                    offsetRotate = 90;
                    offsetP.x = data.offset;
                }else if(bitmask == 11){ // Top
                    offsetRotate = 180;
                    offsetP.y = -data.offset;
                }else if(bitmask == 13){ // Left
                    offsetRotate = 270;
                    offsetP.x = -data.offset;
                }else if(bitmask == 14){ // Bot
                    offsetRotate = 0;
                    offsetP.y = data.offset;
                }
            }else{
                tile3DAon = data.interior;
                offsetP.y = data.offset;
            }

            //  1
            //8   2
            //  4
            if(tile3DAon != null){
                string path = tile3DAon.prefab_name;
                var prefab = Resources.Load<GameObject>(path);
                // var obj = new GameObject();
                if(prefab != null){
                    var art = Instantiate(prefab);
                    art.transform.parent = parent;
                    if(data.scalePrefab != 1){
                        art.transform.localScale = new Vector3( data.scalePrefab, data.scalePrefab, data.scalePrefab);
                    }
                    art.transform.Rotate( -90, 0 , 0);
                    art.transform.Rotate( 0, 180 , 0);
                    art.transform.Rotate( tile3DAon.offsetRotate.x, tile3DAon.offsetRotate.y + offsetRotate, tile3DAon.offsetRotate.z);
                    art.transform.localPosition =  new Vector3( 0.5f, -0.5f, 0f) + tile3DAon.offsetPos + offsetP;
                    art.name = tile_count.ToString() + "_" + bitmask.ToString() + "_r_" + offsetRotate;
                }
            }
        }

        private void AddOverlayFence( Transform parent, DrawTile3DAon data, int idTile, int tileX, int tileY){
            Vector3 offsetP = Vector3.zero;
            int offsetRotate = 0;
            bool top = idTile == m_autoTileMap.GetAutoTile( tileX, tileY+1, MapLayerIdx ).Id;
            bool bot = idTile == m_autoTileMap.GetAutoTile( tileX, tileY-1, MapLayerIdx ).Id;
            bool left = idTile == m_autoTileMap.GetAutoTile( tileX-1, tileY, MapLayerIdx ).Id;
            bool right = idTile == m_autoTileMap.GetAutoTile( tileX+1, tileY, MapLayerIdx ).Id;
            int tile_count = _getTileCount( top, bot, left, right);
            int bitmask = _getTileBitmask( top, bot, left, right);
            //  1
            //8   2
            //  4
            DrawTile3D tile3DAon = null;
            if(tile_count == 0){
                tile3DAon = data.side;
                offsetP.y = data.offset;
            }else if(tile_count == 1){
                tile3DAon = data.corner_1;
                if(bitmask == 2){
                    offsetRotate = 0;
                    offsetP.y = data.offset;
                }else if(bitmask == 1){
                    offsetRotate = 90;
                    offsetP.x = data.offset;
                }else if(bitmask == 8){
                    offsetRotate = 180;
                    offsetP.y = -data.offset;
                }else if(bitmask == 4){
                    offsetRotate = 270;
                    offsetP.x = -data.offset;
                }
            }else if(tile_count == 2){
                //Side W or H: 
                if(bitmask == 5){//Side_H
                    tile3DAon = data.side;
                    offsetRotate = 90;
                    offsetP.x = data.offset;
                }else if(bitmask == 10){//Side_W
                    tile3DAon = data.side;
                    offsetRotate = 0;
                    offsetP.y = data.offset;
                }else{
                    tile3DAon = data.corner_2;
                    if(bitmask == 3){ // Top, Left
                        offsetRotate = 180;
                        offsetP.x = data.offset;
                        offsetP.y = -data.offset;
                    }else if(bitmask == 9){ // Top, Right
                        offsetRotate = 270;
                        offsetP.x = -data.offset;
                        offsetP.y = -data.offset;
                    }else if(bitmask == 12){ // Bot, Right
                        offsetRotate = 0;
                        offsetP.x = -data.offset;
                        offsetP.y = data.offset;
                    }else if(bitmask == 6){ //Bot, Left
                        offsetRotate = 90;
                        offsetP.x = data.offset;
                        offsetP.y = data.offset;
                    }
                }
            }else if(tile_count == 3){
                tile3DAon = data.corner_3;
                if(bitmask == 7){ // Right
                    offsetRotate = 90;
                    offsetP.x = data.offset;
                }else if(bitmask == 11){ // Top
                    offsetRotate = 180;
                    offsetP.y = -data.offset;
                }else if(bitmask == 13){ // Left
                    offsetRotate = 270;
                    offsetP.x = -data.offset;
                }else if(bitmask == 14){ // Bot
                    offsetRotate = 0;
                    offsetP.y = data.offset;
                }
            }else{
                tile3DAon = data.interior;
                offsetP.y = data.offset;
            }

            //  1
            //8   2
            //  4
            if(tile3DAon != null){
                string path = tile3DAon.prefab_name;
                var prefab = Resources.Load<GameObject>(path);
                // var obj = new GameObject();
                if(prefab != null){
                    var art = Instantiate(prefab);
                    art.transform.parent = parent;
                    if(data.scalePrefab != 1){
                        art.transform.localScale = new Vector3( data.scalePrefab, data.scalePrefab, data.scalePrefab);
                    }
                    art.transform.Rotate( -90, 0 , 0);
                    art.transform.Rotate( 0, 180 , 0);
                    art.transform.Rotate( tile3DAon.offsetRotate.x, tile3DAon.offsetRotate.y + offsetRotate, tile3DAon.offsetRotate.z);
                    art.transform.localPosition =  new Vector3( 0.5f, -0.5f, 0f) + tile3DAon.offsetPos + offsetP;
                    // art.transform.localPosition =  tile3DAon.offsetPos + offsetP;
                    art.name = tile_count.ToString() + "_" + bitmask.ToString() + "_r_" + offsetRotate;
                }
            }
        }

        static public MeshFilter CombineMesh( GameObject art){
            var p = art.transform.parent;
            var lo = art.transform.localPosition;
            var ls = art.transform.localScale;
            var lr = art.transform.localRotation;
            art.transform.parent = null;
            art.transform.localPosition = Vector3.zero;
            art.transform.localScale = Vector3.one;
            art.transform.localRotation = Quaternion.identity;
            List<GameObject> combineInstanceArrays = new List<GameObject>();
            MeshFilter[] meshFilters = art.transform.GetComponentsInChildren<MeshFilter>();
            foreach( MeshFilter meshFilter in meshFilters )
            {
                combineInstanceArrays.Add(meshFilter.gameObject);
            }
            // GUI.enabled = false;
            CombinedMeshData data = EasyCombine.Combine(combineInstanceArrays, UV2Mode.Keep, false, false);
            // GUI.enabled = true;
            // GameObject obj_result = new GameObject();
            // obj_result.transform.parent = art.transform;
            // obj_result.name = "CombineMesh";
            // obj_result.AddComponent(data.CombinedMesh);
            //Finish
            MeshFilter meshFilterCombine = art.GetComponent<MeshFilter>();
            if(!meshFilterCombine){
                meshFilterCombine = art.AddComponent<MeshFilter>();
            }
            MeshRenderer meshRendererCombine = art.GetComponent<MeshRenderer>();
            if(!meshRendererCombine)
                meshRendererCombine = art.AddComponent<MeshRenderer>();

            meshFilterCombine.sharedMesh = data.CombinedMesh;
            meshRendererCombine.materials = data.Materials;
            //Remove All child
            while(art.transform.childCount != 0){
                DestroyImmediate(art.transform.GetChild(0).gameObject);
            }
            art.transform.parent = p;
            art.transform.localPosition = lo;
            art.transform.localScale = ls;
            art.transform.localRotation = lr;
            return meshFilterCombine;
        }

        static public MeshFilter CombineMeshClassic( GameObject art){
            MeshFilter meshFilterResult = null;
            MeshFilter[] meshFilters = art.transform.GetComponents<MeshFilter>();
            ArrayList materials = new ArrayList();
            ArrayList combineInstanceArrays = new ArrayList();
            foreach( MeshFilter meshFilter in meshFilters )
            {
                MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
                
                // Handle bad input
                if(!meshRenderer) {
                    Debug.LogError("MeshFilter does not have a coresponding MeshRenderer."); 
                    continue; 
                }
                if(meshRenderer.materials.Length != meshFilter.sharedMesh.subMeshCount) { 
                    Debug.LogError("Mismatch between material count and submesh count. Is this the correct MeshRenderer?"); 
                    continue; 
                }
                
                for(int s = 0; s < meshFilter.sharedMesh.subMeshCount; s++)
                {
                    int materialArrayIndex = 0;
                    for(materialArrayIndex = 0; materialArrayIndex < materials.Count; materialArrayIndex++)
                    {
                        if(materials[materialArrayIndex] == meshRenderer.sharedMaterials[s])
                            break;
                    }
                    
                    if(materialArrayIndex == materials.Count)
                    {
                        materials.Add(meshRenderer.sharedMaterials[s]);
                        combineInstanceArrays.Add(new ArrayList());
                    }
                    
                    CombineInstance combineInstance = new CombineInstance();
                    combineInstance.transform = meshRenderer.transform.localToWorldMatrix;
                    combineInstance.subMeshIndex = s;
                    combineInstance.mesh = meshFilter.sharedMesh;
                    (combineInstanceArrays[materialArrayIndex] as ArrayList).Add( combineInstance );
                }
                meshFilter.gameObject.SetActive(false);
            }
            
            // For MeshFilter
            {
                // Get / Create mesh filter
                MeshFilter meshFilterCombine = art.GetComponent<MeshFilter>();
                if(!meshFilterCombine)
                    meshFilterCombine = art.AddComponent<MeshFilter>();
                meshFilterResult = meshFilterCombine;
                // Combine by material index into per-material meshes
                // also, Create CombineInstance array for next step
                Mesh[] meshes = new Mesh[materials.Count];
                CombineInstance[] combineInstances = new CombineInstance[materials.Count];
                
                for( int m = 0; m < materials.Count; m++ )
                {
                    CombineInstance[] combineInstanceArray = (combineInstanceArrays[m] as ArrayList).ToArray(typeof(CombineInstance)) as CombineInstance[];
                    meshes[m] = new Mesh();
                    meshes[m].CombineMeshes( combineInstanceArray, true, true );
                    
                    combineInstances[m] = new CombineInstance();
                    combineInstances[m].mesh = meshes[m];
                    combineInstances[m].subMeshIndex = 0;
                }
                
                // Combine into one
                meshFilterCombine.sharedMesh = new Mesh();
                meshFilterCombine.sharedMesh.CombineMeshes( combineInstances, false, false );
                
                // Destroy other meshes
                foreach( Mesh mesh in meshes )
                {
                    mesh.Clear();
                    DestroyImmediate(mesh);
                }
            }
            
            // For MeshRenderer
            {
                // Get / Create mesh renderer
                MeshRenderer meshRendererCombine = art.GetComponent<MeshRenderer>();
                if(!meshRendererCombine)
                    meshRendererCombine = art.AddComponent<MeshRenderer>();
                
                // Assign materials
                Material[] materialsArray = materials.ToArray(typeof(Material)) as Material[];
                meshRendererCombine.materials = materialsArray;    
            }
            return meshFilterResult;
        }

    }
}