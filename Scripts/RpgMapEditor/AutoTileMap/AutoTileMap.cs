using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System;
using UnityEngine.AI;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using FairyGUI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AON.RpgMapEditor
{

    /// <summary>
    /// Create and manage the auto tile map
    /// </summary>
    // [RequireComponent(typeof(AutoTileMapGui))]
    // [RequireComponent(typeof(AutoTileMapEditorBehaviour))]
    public class AutoTileMap : AutoTileMap_Chunk
    {
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
                    ForceReloadMapNow();
                }
            }
        }

        public Camera ViewCamera;
        
        public Camera PlayCamera;

        public Texture2D MinimapTexture { get; private set; }

        public GameObject Agent;

        private Texture2D[] m_thumbnailTextures; // thumbnail texture for each sub tileset
        public Texture2D[] ThumbnailTextures { get { return m_thumbnailTextures; } }
        protected Vector3 DefauseInfoMainCam { get; set;}

        // [SerializeField]
        public override void LoadMapsData( AutoTileMapData mapData, int mapIdx){
            base.LoadMapsData(mapData, mapIdx);
            ForceReloadMapNow();
        }
        public void ForceReloadMapNow()
        {
            // StartCoroutine(LoadMapAsync(onMapLoaded));
            IEnumerator coroutine = LoadMapAsync(MapSelect);
            while (coroutine.MoveNext());
        }

		public bool IsLoading { get; private set; }
		void OnValidate()
        {
            IsLoading = false;
        }
		public delegate void OnMapLoadedDelegate(AutoTileMap_Chunk autoTileMap);
        /// <summary>
        /// Load Map asynchronously according to MapData.
        /// </summary>
		public IEnumerator LoadMapAsync(AutoTileMapSerializeData map, OnMapLoadedDelegate onMapLoaded = null)
        {
            if (IsLoading)
            {
                Debug.LogWarning("Cannot load map while loading");
            }
            else
            {
                IsLoading = true;
                {
                    // preparing
                    if(m_mapIndex < 0 || m_mapIndex >= m_mapData.Maps.Count){
                        m_mapIndex = 0;
                    }
                    if (Tileset != null && CellSize == Vector2.zero)
                    {
                        CellSize = new Vector2(Tileset.TileWidth / AutoTileset.PixelToUnits, Tileset.TileHeight / AutoTileset.PixelToUnits);
                    }
                    if (_triggerNode != null)
                    {
                        _triggerNode.name = "";
                        DestroyImmediate(_triggerNode.gameObject);
                        _triggerNode = null;
                    }
                    DestroyChunk();
                    InitChunk().Initialize(this);
                }
                if (Tileset != null)
                {
                    if (map != null)
                    {
                        //Load data
                        // IEnumerator coroutine = MapSelect.LoadToMap(this);
                        // while (coroutine.MoveNext()) yield return null;
                        LoadToMap(map);
                        map.LoadFromCompression();
                        if (IsPlayMode)
                        {
                            LoadHigh();
                            LoadWater();
                        }
                        IEnumerator coroutine = TileChunkPoolNode.UpdateChunksAsync();
                        while (coroutine.MoveNext()) yield return null;
                        TileChunkPoolNode.UpdateLayersData( IsPlayMode);
                        if(IsPlayMode){
                            TileChunkPoolNode.transform.eulerAngles = new Vector3(90, 0, 0);
                            List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
                            TileChunkPoolNode.BuildNavByMesh(ref sources);
                            LocalNavMeshBuilderAON.Instance.UpdateNavMesh(sources);
                            coroutine = RefreshTileTrigger();
                            while (coroutine.MoveNext()) yield return null;
                        }
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
                if (onMapLoaded != null)
                {
                    onMapLoaded(this);
                }
            }
            if (IsPlayMode == false)
            {
                ComboBoxHelper.ResetInstance();
            }
            AONGUIBehaviour.AONGUI_ReDrawAll();
            yield return null;
        }

        public void LoadToMap(AutoTileMapSerializeData MapSelect)
		{

			this.Initialize();

            int TileMapWidth = MapSelect.TileMapWidth;
            int TileMapHeight = MapSelect.TileMapHeight;
            int totalMapTiles = TileMapWidth * TileMapHeight;
            var TileData = MapSelect.TileData;
			for( int iLayer = 0; iLayer < TileData.Count; ++iLayer )
			{
                TileLayer tileData = TileData[iLayer];
                this.MapLayers.Add(
                    new AutoTileMap.MapLayer() {
                        Name = tileData.Name,
                        Visible = tileData.Visible, 
                        LayerType = tileData.LayerType, 
                        SortingOrder = tileData.SortingOrder,
                        SortingLayer = tileData.SortingLayer,
                        Depth = tileData.Depth,
                        TileLayerIdx = iLayer,
                    });
                // _autoTileMap.TileLayers.Add( new AutoTile[totalMapTiles] );
                this.TileLayers.Add( new AutoTile[TileMapWidth, TileMapHeight] );
				int iTileRepetition = 1;
				int iTileIdx = 0;
                for (int i = 0; i < tileData.Tiles.Count; ++i )
                {
                    int iType = tileData.Tiles[i];
                    //see compression notes in CreateFromTilemap
                    if (iType < -1)
                    {
                        iTileRepetition = -iType;
                    }
                    else
                    {
                        //---
                        if (iTileRepetition > totalMapTiles)
                        {
                            Debug.LogError("Error uncompressing layer " + tileData.Name + ". The repetition of a tile was higher than map tiles " + iTileRepetition + " > " + totalMapTiles);
                            iTileRepetition = 0;
                        }
                        for (; iTileRepetition > 0; --iTileRepetition, ++iTileIdx)
                        {
                            // if (iTileIdx % 10000 == 0)
                            // {
                                //float loadingPercent = ((float)(iTileIdx + iLayer * TileMapWidth * TileMapHeight)) / (TileMapWidth * TileMapHeight * TileData.Count);
                                //Debug.Log(" Loading " + (int)(loadingPercent * 100) + "%");
                                // yield return null;
                            // }
                            int tile_x = iTileIdx % TileMapWidth;
                            int tile_y = iTileIdx / TileMapWidth;
                            if (iType >= 0)
                            {
                                this.SetAutoTile(tile_x, tile_y, iType, iLayer, false);
                            }
                        }
                        iTileRepetition = 1;
                    }
                }
			}
            if(this.IsEmptyTerrain()){
                this.ResetTerrain();
                // this.SaveData( _autoTileMap);
            }
            this.CreateChunkLayersData();
            this.RefreshAllTiles();
			this.RefreshMinimapTexture();
		}

        //

        public AutoTileMapSerializeData MapAt( int mapIdxSelect)
        {
            return m_mapData.Maps[mapIdxSelect];
        }

        [SerializeField]
        ItemCharDatabase m_itemCharDatabase;
        public ItemCharDatabase ItemCharData
        {
            get
            {
                return m_itemCharDatabase;
            }
            set
            {
                m_itemCharDatabase = value;
            }
        }

        public void SetDataMapSelect( int idxMap){
            if(idxMap >= 0 && idxMap < m_mapData.Maps.Count){
                m_mapIndex = idxMap;
            }
        }

        // [SerializeField]
        AutoTileBrush m_brushGizmo;
        public AutoTileBrush BrushGizmo
        {
            get
            {
                if (m_brushGizmo == null)
                {
                    GameObject objBrush = new GameObject();
                    objBrush.name = "BrushGizmo";
                    objBrush.transform.parent = transform;
                    m_brushGizmo = objBrush.AddComponent<AutoTileBrush>();
                    m_brushGizmo.MyAutoTileMap = this;
                }
                return m_brushGizmo;
            }
        }

        public void AgentSetStop(bool isStop)
        {
            IEnumerator coroutine = IAgentStop(isStop);
            StartCoroutine(coroutine);
        }

        public IEnumerator IAgentStop(bool isStop)
        {
            yield return null;
            if (Agent.activeSelf && Agent.GetComponent<NavMeshAgent>().isStopped != isStop)
            {
                Agent.GetComponent<NavMeshAgent>().isStopped = isStop;
                Agent.GetComponent<NavMeshAgent>().ResetPath();
            }
            yield break;
        }
        
        private Texture2D m_minimapTilesTexture;
        // private float m_tileAnim3Frame = 0f;
        // private float m_tileAnim4Frame = 0f;
        private Color[] aTilesColors;
        // [SerializeField]

        //m_triggerNode have NPC
        [NonSerialized]
        private Transform _triggerNode = null;
        public Transform TriggerNode
        {
            get
            {
                if (_triggerNode != null)
                    return _triggerNode;
                var obj_triggerNode = GameObject.Find("TriggerNode");
                if (obj_triggerNode == null)
                {
                    var obj = new GameObject();
                    // obj.transform.parent = m_tileChunkPoolNode.transform;
                    obj.transform.localRotation = Quaternion.identity;
                    obj.name = "TriggerNode";
                    _triggerNode = obj.transform;
                }
                else
                {
                    _triggerNode = obj_triggerNode.transform;
                }
                return _triggerNode;
            }
        }

        private void AddObjToTriggerNode(Transform c, bool isAddOccHelper)
        {
            c.parent = TriggerNode;
            if (isAddOccHelper)
            {
                c.gameObject.AddComponent<OccHelper>().Initialize(this);
            }
        }
        
        // public Vector3 DefauseInfoMainCam
        // {
        //     get
        //     {
        //         return m_defauseInfoMainCam;
        //     }
        // }

        public void ResetMainCam(){
            InfoMainCam = DefauseInfoMainCam;
            CamControler.target = Agent.transform;
        }

        public void ResetMainCamWithAni( GTweenCallback onComplete){
            if(CamControler.target == Agent.transform){
                if(onComplete != null){
                    onComplete();
                }
                return;
            }
            float _camXAngle_to = DefauseInfoMainCam.x;
            float _y_to = DefauseInfoMainCam.y;
            float _distance_to = DefauseInfoMainCam.z;
            Vector3 _target_to = Agent.transform.position;
            CamControler.MoveCamTo(_camXAngle_to, _y_to, _distance_to, _target_to, () =>{
                CamControler.target = Agent.transform;
                if(onComplete != null){
                    onComplete();
                }
            });
        }

        public void RequestAgentFaceToNpc(GameObject npcTarget, GTweenCallback onComplete){
            if(npcTarget.name == TriggerGame.Instance.NpcCurrentFaceTo){
                onComplete();
                return;
            }
            // if(npcTarget != null && npcTarget.activeSelf){
            Debug.Log("RequestAgentFaceToNpc " + npcTarget.name);
            TriggerGame.Instance.NpcCurrentFaceTo = npcTarget.name;
            CamControler.MoveCamToLock(Agent.transform, npcTarget.transform, null);
            bool isRide = Agent.GetComponentInChildren<BasicMecanimControl>().IsRide;
            var moveTo = npcTarget.transform.position + npcTarget.transform.forward * (isRide ? 2.25f : 1.75f);
            Agent.GetComponent<NavMeshAgentCallback>().WalkTo( 0, moveTo, (NavMeshAgent nav) => {
                // Lock to
                bool requestLockBack = true;
                NpcLookatMainCallback.InstanceOf(npcTarget).LookTo( Agent, requestLockBack, (NpcLookatMainCallback n) => {
                    onComplete();
                });
            });
            // }
        }

        [SerializeField]
        private DayNightCycle m_dayNightCycle = null;
        public DayNightCycle DayNight{
            get{
                return m_dayNightCycle;
            }
        }

        private void ResetCacheForPlay(int idMap){
            if (idMap == -1)
            {
                // Repare for Pick postion when Editor
                ViewCamera.enabled = false;
                PlayCamera.enabled = true;
                ViewCamera.gameObject.SetActive(false);
                PlayCamera.gameObject.SetActive(true);
                BrushGizmo.Clear();
                GetComponent<MiniMapAON>().enabled = false;
            }else
            {
                // Repare for Game
                TriggerGame.ResetCache();
                PropertysGame.ResetCache();
                ShopGame.ResetCache();
                ViewCamera.enabled = false;
                PlayCamera.enabled = true;
                ViewCamera.gameObject.SetActive(false);
                PlayCamera.gameObject.SetActive(true);
                BrushGizmo.Clear();
                GetComponent<MiniMapAON>().enabled = false;
                ResetMainCam();
                InputFieldHelper.Instance.BeginMenuGame();    
            }
        }

        private void ResetCacheForEditor(){
            ScriptGui.Instance.ResetCache();
            InputFieldHelper.Instance.HideAll();
            _updateTimeSun();
        }


        protected IEnumerator PlayAsync(int idMap, int xLast, int yLast){
            // yield return new WaitForSeconds(0.5f);
            yield return null;
		    StartPlayMap( idMap, xLast, yLast);
            yield break;
        }

        public void StartPlayMap(int idMap, int tileX, int tileY){
            if(IsPlayMode && idMap >= 0 && idMap != m_mapIndex){
                return;
            }
            // Save map editor
            SaveMap();
            GetComponent<MiniMapAON>().enabled = false;
            // Load map
            ResetCacheForPlay(idMap);
            IsPlayMode = true;
            SetDataMapSelect(idMap);
            ForceReloadMapNow();
            //Update time
            _updateRealTime();
            //Camera
            ViewCamera.enabled = false;
            ViewCamera.gameObject.SetActive(false);
            PlayCamera.enabled = true;
            PlayCamera.gameObject.SetActive(true);
            //Warp
            float px0 = (0.5f + tileX) * (CellSize.x);
            float py0 = -(tileY + 0.5f) * (CellSize.y);
            Agent.SetActive(true);
            Agent.GetComponentInChildren<CharGame>().GoNake();
            UtilsAON.WarpTo(Agent.transform, new Vector3(px0, 0, py0));
            Agent.transform.localRotation = Quaternion.identity;
            Agent.GetComponent<AgentCollision>().MakeShouldBeExitTrigger();
            if (idMap != -1)
            {
                InfoMainCam = DefauseInfoMainCam;
                ControlCameraMainChar = false;
            }
            Agent.GetComponent<NavMeshAgentCallback>().ResumeMove();
        }

        public void SetModePlay(bool isPlayMode, int idMap = -1, int tileX = 0, int tileY = 0)
        {
            StopAllCoroutines();
            IsPlayMode = isPlayMode;
            if (IsPlayMode)
            {
                ResetCacheForPlay(idMap);
                if (idMap != -1)
                {
                    SaveMap();
                    SetDataMapSelect( idMap);
                    ForceReloadMapNow();
                }
                float px0 = (0.5f + tileX) * (CellSize.x);
                float py0 = -(tileY + 0.5f) * (CellSize.y);
                Agent.SetActive(true);
                Agent.GetComponent<NavMeshAgent>().Warp(new Vector3(px0, 0, py0));
                Agent.GetComponent<AgentCollision>().MakeShouldBeExitTrigger();
                if (idMap != -1)
                {
                    InfoMainCam = DefauseInfoMainCam;
                    ControlCameraMainChar = false;
                }
                Agent.GetComponent<NavMeshAgentCallback>().ResumeMove();
            }
            else
            {
                ResetCacheForEditor();
                Agent.SetActive(false);
                ViewCamera.enabled = true;
                PlayCamera.enabled = false;
                ViewCamera.gameObject.SetActive(true);
                PlayCamera.gameObject.SetActive(false);
                GetComponent<MiniMapAON>().enabled = true;
                if (idMap != -1)
                {
                    SetDataMapSelect( idMap);
                }
                ForceReloadMapNow();
            }
        }

        public void WarpsTo(Collider col, int idMap, int tileX, int tileY)
        {
            Agent.GetComponent<NavMeshAgentCallback>().WalkTo( 0, col.transform.position, (NavMeshAgent nav) => {
                WarpsTo(idMap, tileX, tileY);
			});
        }

        public void WarpsTo(int idMap, int tileX, int tileY)
        {
            InputFieldHelper.Instance.ChargeSceneToDark(()=>{
                IEnumerator coroutine = WarpsToASync(idMap, tileX, tileY, () => {
                    PlayCamera.enabled = true;
                    PlayCamera.gameObject.SetActive(true);
                    TriggerGame.Instance.CallBack_AffterWarps();
                    InputFieldHelper.Instance.ChargeSceneToLight(()=>{
                        Debug.Log("Warps Done");
                    });
                });
                StartCoroutine(coroutine);
            });
            PlayCamera.enabled = false;
            PlayCamera.gameObject.SetActive(false);
        }

        public IEnumerator WarpsToASync(int idMap, int tileX, int tileY, GTweenCallback onComplete = null)
        {
            Agent.SetActive(false);
            // yield return new WaitForSeconds(0.5f);
            if (idMap != -1 && MapIdxSelect != idMap)
            {
                SetDataMapSelect( idMap);
                ForceReloadMapNow();
            }
            float px0 = (0.5f + tileX) * (CellSize.x);
            float py0 = -(tileY + 0.5f) * (CellSize.y);
            Agent.SetActive(true);
            Agent.GetComponent<NavMeshAgent>().Warp(new Vector3(px0, 0, py0));
            FaceYMainChar = 0;
            Agent.GetComponent<AgentCollision>().MakeShouldBeExitTrigger();
            if(onComplete != null){
                onComplete();
            }
            yield break;
        }

        public void GoToInterior(Collider col, int houseRef, int idxInterior, Vector3 p, bool isCreateGoOut = true)
        {
            InputFieldHelper.Instance.IsChargeScene = true;
            Agent.GetComponent<NavMeshAgentCallback>().WalkTo( 0, col.transform.position, (NavMeshAgent nav) => {
                InputFieldHelper.Instance.ChargeSceneToDark(()=>{
                    IEnumerator coroutine = GoToInteriorASync(houseRef, idxInterior, p, isCreateGoOut, ()=>{
                        PlayCamera.enabled = true;
                        PlayCamera.gameObject.SetActive(true);
                        InputFieldHelper.Instance.ChargeSceneToLight(()=>{
                            Debug.Log("Load Interior Done");
                            InputFieldHelper.Instance.IsChargeScene = false;
                        });
                    });
                    StartCoroutine(coroutine);
                });
                PlayCamera.enabled = false;
                PlayCamera.gameObject.SetActive(false);
			});
        }

        private GameObject _objInterior;
        private Vector3 _posBeforeGoInterior;
        public IEnumerator GoToInteriorASync(int houseRef, int idxInterior, Vector3 warp, bool isCreateGoOut, GTweenCallback onComplete = null)
        {
            Agent.SetActive(false);
            yield return new WaitForSeconds(1);
            GameObject obj = new GameObject();
            obj.name = name + " Interior";
            var interiorList = Tileset.InteriorList;
            // var path = "MODEL/BUILDING/Interior/Prefab/";
            var path = "interiors/";
            path += interiorList[idxInterior];
            var prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                yield break;
            }
            var art = Instantiate(prefab);
            art.transform.parent = obj.transform;
            // art.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            // art.transform.Rotate( 0, 90 , 0);
            // art.transform.Rotate( 0, 180 , 0);
            // art.transform.localPosition = Vector3.zero + new Vector3( w * 0.5f, -h * 0.5f, 0f);
            art.name = interiorList[idxInterior];
            /*
            var meshs = art.GetComponentsInChildren<MeshRenderer>();
            foreach( var m in meshs){
                m.gameObject.AddComponent<MeshCollider>();
            }
            */

            //Clear Map
            {
                TileChunkPoolNode.gameObject.SetActive(false);
                if (_triggerNode != null)
                {
                    _triggerNode.gameObject.SetActive(false);
                }
            }
            #region NavMeshSurface
            {
                // var nav = obj.AddComponent<NavMeshSurface>();
                // nav.collectObjects = CollectObjects.Children;
                // nav.size = new Vector3(256, 256, 256);
                // nav.overrideTileSize = true;
                // nav.tileSize = 16;
                // nav.BuildNavMesh();
                /*
                { //Build floor
                    
                    MeshFilter[] _filters = art.GetComponentsInChildren<MeshFilter>();
                    foreach (MeshFilter _filter in _filters)
                    {
                        if( _filter.mesh == null || _filter.mesh.vertexCount <= 0)
                            continue;
                        // float h = _filter.mesh.bounds.max.y - _filter.mesh.bounds.min.y;
                        // // Debug.Log("h: " + h);
                        if( _filter.mesh.bounds.min.y > 0.5f){
                            continue;
                        }
                        if( _filter.GetComponent<BoxCollider>() != null){
                            continue;
                        }
                        BoxCollider boxCollider = _filter.gameObject.AddComponent<BoxCollider>();
                        boxCollider.size = new Vector3(_filter.mesh.bounds.size.x, 0.5f, _filter.mesh.bounds.size.z);
                        boxCollider.center = new Vector3(_filter.mesh.bounds.center.x,- 0.5f, _filter.mesh.bounds.center.z);
                    }
                }
                */
                //Add sources_meshs
                var sources_meshs = new List<NavMeshBuildSource>();
                #if false
                {
                    MeshFilter[] _filters = art.GetComponentsInChildren<MeshFilter>();
                    foreach (MeshFilter _filter in _filters)
                    {
                        if (_filter.mesh == null || _filter.mesh.vertexCount <= 0)
                            continue;
                        // Debug.Log(_filter.name + ": " + _filter.mesh.bounds.size.y);
                        bool forceUseBox = true;
                        if (forceUseBox || _filter.mesh.bounds.size.y >= 5.0f)
                        {
                            //add floor
                            BoxCollider boxCollider = _filter.gameObject.AddComponent<BoxCollider>();
                            boxCollider.size = new Vector3(_filter.mesh.bounds.size.x, 0.5f, _filter.mesh.bounds.size.z);
                            // boxCollider.center = new Vector3(_filter.mesh.bounds.center.x, -(_filter.mesh.bounds.size.y * 0.5f) - 0.25f, _filter.mesh.bounds.center.z);
                            boxCollider.center = new Vector3(_filter.mesh.bounds.center.x, _filter.mesh.bounds.min.y - 0.25f, _filter.mesh.bounds.center.z);
                        }
                        else
                        {
                            //add mesh
                            NavMeshBuildSource s = new NavMeshBuildSource
                            {
                                transform = _filter.transform.localToWorldMatrix,
                                shape = NavMeshBuildSourceShape.Mesh,
                                sourceObject = _filter.mesh,
                                area = 0
                            };
                            sources_meshs.Add(s);
                            _filter.gameObject.AddComponent<MeshCollider>().sharedMesh = _filter.mesh;
                        }
                    }
                }
                #endif
                var root = art.transform;
                var sources = new List<NavMeshBuildSource>();
                var markups = new List<NavMeshBuildMarkup>();
                NavMeshBuilder.CollectSources(root, ~0, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources);
                sources.AddRange(sources_meshs);
                LocalNavMeshBuilderAON.Instance.UpdateNavMesh(sources);
                // var defaultBuildSettings = NavMesh.GetSettingsByID(0);
                // var bounds = new Bounds(Vector3.zero, 1000.0f * Vector3.one);
                // var navmesh = NavMeshBuilder.BuildNavMeshData(defaultBuildSettings, sources, bounds, root.position, root.rotation);
                // navmesh.name = "Navmesh";
                // NavMeshBuilder.UpdateNavMeshData(navmesh, defaultBuildSettings, sources, bounds);
            }
            #endregion

            BoxColliderGen boxColliderGen = art.GetComponent<BoxColliderGen>();
            if(boxColliderGen != null && boxColliderGen.navMeshData != null){
                // var defaultBuildSettings = NavMesh.GetSettingsByID(0);
                // var bounds = new Bounds(Vector3.zero, 1000.0f * Vector3.one);
                // NavMeshBuilder.UpdateNavMeshData(boxColliderGen.navMeshData, defaultBuildSettings, sources, bounds);
                // LocalNavMeshBuilderAON.Instance.UpdateNavMesh(sources);
            }
            /*
            #region Remove all BoxCollider
            {
                var cols = art.transform.GetComponents<BoxCollider>();
                foreach(var comp in cols){
                    comp.enabled = false;
                }
                cols = art.transform.GetComponentsInChildren<BoxCollider>();
                foreach(var comp in cols){
                    comp.enabled = false;
                }
            }
            #endregion
            * */
            // TileChunk.CombineMesh(art);
            _objInterior = obj;
            // yield return new WaitForSeconds(1);
            // float px0 = (0.5f + tileX) * (CellSize.x);
            // float py0 = -(tileY + 0.5f) * (CellSize.y);
            // Agent.GetComponent<NavMeshAgent>().Warp( new Vector3(px0 , 0, py0));
            _posBeforeGoInterior = Agent.transform.position;
            Agent.SetActive(true);
            Agent.GetComponent<NavMeshAgent>().Warp(warp);
            Agent.GetComponent<AgentCollision>().MakeShouldBeExitTrigger();
            if (isCreateGoOut)
            {
                //Create Box Go Out
                var boxGoOut = new GameObject();
                boxGoOut.transform.parent = obj.transform;
                boxGoOut.transform.localPosition = Agent.transform.position;
                boxGoOut.transform.localRotation = Quaternion.identity;
                boxGoOut.name = "GoOutInterior";
                var col = boxGoOut.AddComponent<BoxCollider>();
                col.center = new Vector3(0, -0.5f, 0);
                col.isTrigger = true;
                {
                    //Art
                    GameObject prefab_art = Resources.Load<GameObject>("OBJ/DoorInteraction2");
                    if (prefab != null)
                    {
                        var a = GameObject.Instantiate(prefab_art);
                        a.transform.parent = boxGoOut.transform;
                        a.transform.localRotation = Quaternion.identity;
                        a.transform.localPosition = new Vector3(0, 0, 0);
                    }
                    //Npc
                    var houseData = MapSelect.HouseData[houseRef];
                    if (houseData.NpcInHouses != null && houseData.NpcInHouses.Count > 0)
                    {
                        foreach (var npcRef in houseData.NpcInHouses)
                        {
                            if (npcRef.IdxNPC >= 0 && npcRef.IdxNPC < MapSelect.NPCData.Count)
                            {
                                ShowNPCInHouse(houseData, npcRef, obj);
                            }
                        }
                    }
                }
                ControlCameraMainChar = false;
            }
            else
            {
                //Mode edit
                ControlCameraMainChar = true;
            }
            yield return null;
            if (IsPlayMode && houseRef >= 0)
            {
                TriggerGame.Instance.OnHouseEnter(houseRef);
                var ho = MapSelect.HouseData[houseRef];
                if (ho.CamOut != Vector3.zero)
                {
                    InfoMainCam = ho.CamOut;
                    FaceYMainChar = ho.CamOut.y;
                }
                else
                {
                    InfoMainCam = DefauseInfoMainCam;
                }
                _updateTimeSun();
            }
            if(onComplete != null){
                onComplete();
            }
            yield break;
        }

        public bool CloseInteriorPick()
        {
            if (_objInterior != null)
            {
                TileChunkPoolNode.gameObject.SetActive(true);
                if (_triggerNode != null)
                {
                    _triggerNode.gameObject.SetActive(true);
                }
                List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
                TileChunkPoolNode.BuildNavByMesh(ref sources);
                LocalNavMeshBuilderAON.Instance.UpdateNavMesh(sources);
                DestroyImmediate(_objInterior);
                _objInterior = null;
                Agent.GetComponent<NavMeshAgent>().Warp(_posBeforeGoInterior);
                Agent.GetComponent<AgentCollision>().MakeShouldBeExitTrigger();
                if (IsPlayMode)
                {
                    InfoMainCam = DefauseInfoMainCam;
                }
                return true;
            }
            return false;
        }

        public void GoOutInterior()
        {
            if (_objInterior != null)
            {
                IEnumerator coroutine = GoOutInteriorASync();
                StartCoroutine(coroutine);
            }
        }

        public IEnumerator GoOutInteriorASync()
        {
            if (_objInterior != null)
            {
                Agent.SetActive(false);
                yield return new WaitForSeconds(1);
                TileChunkPoolNode.gameObject.SetActive(true);
                if (_triggerNode != null)
                {
                    _triggerNode.gameObject.SetActive(true);
                }
                List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
                TileChunkPoolNode.BuildNavByMesh(ref sources);
                LocalNavMeshBuilderAON.Instance.UpdateNavMesh(sources);
                DestroyImmediate(_objInterior);
                _objInterior = null;
                Agent.SetActive(true);
                Agent.GetComponent<NavMeshAgent>().Warp(_posBeforeGoInterior);
                Agent.GetComponent<AgentCollision>().MakeShouldBeExitTrigger();
                if (IsPlayMode)
                {
                    ControlCameraMainChar = false;
                    InfoMainCam = DefauseInfoMainCam;
                    _updateRealTime();
                }
            }
            yield break;
        }
        

        void OnDestroy()
        {
            if (m_brushGizmo != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(m_brushGizmo.gameObject);
#else
				Destroy(m_brushGizmo.gameObject);
#endif
            }

            DestroyImmediate(m_minimapTilesTexture);
            DestroyImmediate(MinimapTexture);
        }
        
        /// <summary>
        /// Save current map to MapData
        /// </summary>
        /// <returns></returns>
		public bool SaveMap()
        {
            bool isOk = false;
            if (IsLoading)
            {
                //Debug.LogWarning("Cannot save map while loading");
            }
            else
            {
                MapsData.UpdateRaw();
                isOk = MapSelect.SaveData(this);
#if UNITY_EDITOR
                // EditorUtility.SetDirty(this.m_mapData);
                // AssetDatabase.SaveAssets();
                // Debug.Log("AutoTileMap SaveAssets");
                // Save file AllMap on Mac
                var path = Application.persistentDataPath + "/AllMap.json";
                if (File.Exists(path))
                {
                    File.WriteAllText(path, MapsData.GetDataWorld(true));
                }
#endif
            }
            return isOk;
        }

        /*
        public string GetMapDataForSave()
        {
            SaveMap();
            // string json = JsonUtility.ToJson(MapSelect, true);
            string json = UtilsAON.SerializeObject(MapSelect);
            return json;
        }
        */

        public string GetMapDataForLoad()
        {
            string json = PlayerPrefs.GetString("JsonMapData_" + MapIdxSelect.ToString(), "");
            if (!string.IsNullOrEmpty(json))
            {
                return json;
            }
            return "";
        }

        public void SaveMapWithData(string data)
        {
            string name = "JsonMapData_" + MapIdxSelect.ToString();
            PlayerPrefs.SetString(name, data);
        }

        public void LoadMapWithData(string data)
        {
            // AutoTileMapSerializeData mapData = JsonUtility.FromJson<AutoTileMapSerializeData>(data);
            AutoTileMapSerializeData mapData = UtilsAON.DeserializeObject<AutoTileMapSerializeData>(data);
            MapSelect.CopyData(mapData);
            ForceReloadMapNow();
        }

        /*
        /// <summary>
        /// Display a save dialog to save the current map in xml format
        /// </summary>
		public void ShowSaveDialog()
        {
            string name = "JsonMapData_" + MapIdxSelect.ToString();
#if false && UNITY_EDITOR
			string filePath = EditorUtility.SaveFilePanel( "Save tilemap",	"",	name + ".json", "json");
			if( filePath.Length > 0 )
			{
				SaveMap();
                string json = JsonUtility.ToJson(MapSelect);
                PlayerPrefs.SetString(name, json);
				File.WriteAllText(filePath, json);
                return json;
			}
#else
            // SaveMap();
            // string json = JsonUtility.ToJson(MapSelect);
            // string destination = Application.persistentDataPath + "/" + name + ".json";
            // Debug.Log(destination);
            // FileStream file;
            // if(File.Exists(destination)) file = File.OpenWrite(destination);
            // else file = File.Create(destination);
            // BinaryFormatter bf = new BinaryFormatter();
            // bf.Serialize(file, json);
            // file.Close();
            // PlayerPrefs.SetString(name, json);
#endif
        }
        */

        // public string GetDestination(){
        //     string destination = Application.persistentDataPath + "/Map_" + MapIdxSelect;
        //     return destination;
        // }

        public void SaveCurrentMapJson(string path)
        {
            SaveMap();
            // string json = JsonUtility.ToJson(MapSelect, true);
            string json = UtilsAON.SerializeObject(MapSelect);
            // string destination = GetDestination() + ".json";
            File.WriteAllText(path, json);
        }

        // public string SaveAndGetDestinationByYaml(){
        //     SaveMap();
        //     // string json = JsonUtility.ToJson(MapSelect);
        //     var serializer = new SerializerBuilder().Build();
        //     var yaml = serializer.Serialize(MapSelect);
        //     string destination = GetDestination() + ".yaml";
        //     File.WriteAllText(destination, yaml);
        //     return destination;
        // }

        public string SaveAllMapAsJson(string path)
        {
            SaveMap();
            File.WriteAllText(path, MapsData.GetDataWorld(true));
            return path;
        }

        public bool LoadAt(string destination)
        {
            if (File.Exists(destination))
            {
                AutoTileMapSerializeData mapData = AutoTileMapSerializeData.LoadFromFile(destination);
                if (mapData != null && mapData.TileData.Count > 0)
                {
                    MapSelect.CopyData(mapData);
                    ForceReloadMapNow();
                    return true;
                }
            }
            return false;
        }

        public bool LoadAllMap(string destination)
        {
            if (File.Exists(destination))
            {
                var data = File.ReadAllText(destination);
                if (MapsData.LoadDataWorld(data, true) == true)
                {
                    ForceReloadMapNow();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If map can be initialized
        /// </summary>
        /// <returns></returns>
		public bool CanBeInitialized()
        {
            return Tileset != null && MapSelect != null;
        }

		public void Initialize()
        {
            //Debug.Log("AutoTileMap:Initialize");

            if (MapSelect == null)
            {
                Debug.LogError(" AutoTileMap.Initialize called when MapData was null");
            }
            else if (Tileset == null)
            {
                Debug.LogError(" AutoTileMap.Initialize called when Tileset or Tileset.TilesetsAtlasTexture was null");
            }
            else
            {
                //Set the instance if executed in editor where Awake is not called
                
                // Tileset.GenerateAutoTileData();

                //TODO: Allow changing minimap offset to allow minimaps smaller than map size
                int minimapWidth = Mathf.Min(MapTileWidth, 2048); //NOTE: 2048 is a maximum safe size for a texture
                int minimapHeigh = Mathf.Min(MapTileHeight, 2048);
                if (MinimapTexture != null)
                {
                    DestroyImmediate(MinimapTexture);
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

                _generateMinimapTilesTexture();

                InitMapLayers();
                // TileLayers = new AutoTile[];
            }
        }

        Color _support_GetAvgColorOfTexture(Color[] aColors)
        {
            float r, g, b, a;
            r = g = b = a = 0;
            for (int i = 0; i < aColors.Length; ++i)
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

        private void _generateMinimapTilesTexture()
        {
            m_thumbnailTextures = new Texture2D[Tileset.SlotAons.Count];
            int coll = 8;
            int row = Tileset.SlotAons.Count / coll + 1;
            int w = coll * Tileset.TileWidth;
            int h = row * Tileset.TileHeight;
            int ceil = Mathf.CeilToInt((float)Math.Sqrt(Tileset.SlotAons.Count));
            Color[] aColors = Enumerable.Repeat<Color>(new Color(0f, 0f, 0f, 0f), ceil * ceil).ToArray();
            Texture2D thumbTex = new Texture2D(w, h, TextureFormat.ARGB32, false);
            thumbTex.filterMode = FilterMode.Point;
            int idx = 0;
            foreach (SlotAon slot in Tileset.SlotAons)
            {
                int x = Mathf.RoundToInt(idx % coll) * Tileset.TileWidth;
                int y = Mathf.RoundToInt(idx / coll) * Tileset.TileHeight;
                Color[] colors = Tileset.PixelsThumb(slot);
                thumbTex.SetPixels(x, h - y - Tileset.TileHeight, Mathf.RoundToInt(slot.AtlasRecThumb.width), Mathf.RoundToInt(slot.AtlasRecThumb.height), colors);
                aColors[idx] = _support_GetAvgColorOfTexture(colors);
                idx++;
            }
            thumbTex.Apply();
            m_thumbnailTextures[0] = thumbTex;
            m_minimapTilesTexture = new Texture2D(ceil, ceil);
            m_minimapTilesTexture.anisoLevel = 0;
            m_minimapTilesTexture.filterMode = FilterMode.Point;
            m_minimapTilesTexture.name = "MiniMapTiles";
            m_minimapTilesTexture.hideFlags = HideFlags.DontSave;
            m_minimapTilesTexture.SetPixels(aColors);
            m_minimapTilesTexture.Apply();
        }

        /// <summary>
        /// Refresh full minimp texture
        /// </summary>
		public void RefreshMinimapTexture()
        {
            RefreshMinimapTexture(0, 0, MapTileWidth, MapTileHeight);
        }

        /// <summary>
        /// Refresh minimap texture partially
        /// </summary>
        /// <param name="tile_x">X tile coordinate of the map</param>
        /// <param name="tile_y">Y tile coordinate of the map</param>
        /// <param name="width">Width in tiles</param>
        /// <param name="height">Height in tiles</param>
		public void RefreshMinimapTexture(int tile_x, int tile_y, int width, int height)
        {
            Debug.Log("RefreshMinimapTexture");
            tile_x = Mathf.Clamp(tile_x, 0, MinimapTexture.width - 1);
            tile_y = Mathf.Clamp(tile_y, 0, MinimapTexture.height - 1);
            width = Mathf.Min(width, MinimapTexture.width - tile_x);
            height = Mathf.Min(height, MinimapTexture.height - tile_y);
            if (height <= 0 || width <= 0)
            {
                Debug.Log("Error: RefreshMinimapTexture w-h:" + width + " - " + height);
                return;
            }
            Color[] aTilesColors = m_minimapTilesTexture.GetPixels();
            //Get origin
            var l = MinimapTexture.GetPixels(tile_x, MinimapTexture.height - tile_y - height, width, height).Length;
            // Color[] aMinimapColors = Enumerable.Repeat<Color>( new Color(51f / 255f, 204f / 255f, 255f / 255f, 1f) , l).ToArray();
            Color[] aMinimapColors = Enumerable.Repeat<Color>(new Color(0f, 0f, 0f, 1f), l).ToArray();
            foreach (MapLayer mapLayer in MapLayers)
            {
                if (mapLayer.LayerType == eSlotAonTypeLayer.Ground || mapLayer.LayerType == eSlotAonTypeLayer.Overlay)
                {
                }
                else
                {
                    continue;
                }
                AutoTile[,] aAutoTiles = TileLayers[mapLayer.TileLayerIdx];
                // read tile type in the same way that texture pixel order, from bottom to top, right to left
                for (int yf = 0; yf < height; ++yf)
                {
                    for (int xf = 0; xf < width; ++xf)
                    {
                        int tx = tile_x + xf;
                        int ty = tile_y + yf;
                        // int tileIdx = tx + ty * MapTileWidth;

                        // int idTile = aAutoTiles[tileIdx] != null ? aAutoTiles[tileIdx].Id : -1;
                        int idTile = aAutoTiles[tx, ty] != null ? aAutoTiles[tx, ty].Id : -1;
                        if (idTile >= 0 && idTile < aTilesColors.Length)
                        {
                            int idx = (height - 1 - yf) * width + xf;
                            Color baseColor = aMinimapColors[idx];
                            Color tileColor = aTilesColors[idTile];
                            aMinimapColors[idx] = baseColor * (1 - tileColor.a) + tileColor * tileColor.a;
                            aMinimapColors[idx].a = 1f;
                        }
                    }
                }
            }
            MinimapTexture.SetPixels(tile_x, MinimapTexture.height - tile_y - height, width, height, aMinimapColors);
            MinimapTexture.Apply();
        }
        
        public void WarpTo(Transform transform, int tileX, int tileY)
        {
            var high = MapSelect.GetHighRef(tileX, tileY) * 0.5f;
            UtilsAON.WarpTo(transform, new Vector3((0.5f + tileX) * CellSize.x, high + 1, -(0.5f + tileY) * CellSize.y));
        }

        private GameObject CreateNewNPCOnMap(NPC npcData)
        {
            var modelList = Tileset.NPCModelList;
            if (npcData.IdxArt < 0 || npcData.IdxArt >= modelList.Length)
                return null;
            var art_data = modelList[npcData.IdxArt];
            GameObject prefab_art = null;
            GameObject prefab_base = null;
            var localScaleArt = 1.5f;
            if (art_data.type == AutoTileset.NPCModelType.Humanoid)
            {
                prefab_base = Resources.Load<GameObject>("NPC/npc_base");
                var path = "NPC/" + art_data.patch;
                prefab_art = Resources.Load<GameObject>(path);
            }
            else if (art_data.type == AutoTileset.NPCModelType.Legacy)
            {
                prefab_base = Resources.Load<GameObject>("NPC/npc_base");
                var path = "ExtraHuman/FBX/" + art_data.patch;
                prefab_art = Resources.Load<GameObject>(path);
                if (prefab_art != null)
                {
                    Animator a = prefab_art.GetComponent<Animator>();
                    if (a == null)
                    {
                        a = prefab_art.AddComponent<Animator>();
                    }
                    a.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("NPC/Animation/KidAnimator");
                    a.applyRootMotion = false;
                    a.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                }
                localScaleArt = 1;
            }
            if (prefab_base != null && prefab_art != null)
            {
                // var art = new GameObject();
                var obj = Instantiate(prefab_base);
                obj.transform.rotation = Quaternion.identity;
                var art = Instantiate(prefab_art);
                art.name = "art";
                art.transform.parent = obj.transform;
                art.transform.localPosition = new Vector3(0.0f, -1f, 0.0f);
                art.transform.localScale = new Vector3(localScaleArt, localScaleArt, localScaleArt);
                return obj;
            }
            return null;
        }

        public Transform GetNPC(TriggerDetail detail)
        {
            if(detail.typeObj != eSlotAonTypeObj.Person){
                return null;
            }
            return TriggerNode.Find(detail.Hash);
        }

        public GameObject ShowNPC(TriggerDetail detail)
        {
            if(detail.refMap != MapIdxSelect){
                return null;
            }
            int tileX = detail.x;
            int tileY = detail.y;
            Transform t = TriggerNode.Find(detail.Hash);
            if ( t != null)
            {
                WarpTo(t, detail.x, detail.y);
                return null;
            }
            var npcRef = detail.refId;
            if (npcRef < 0 || npcRef >= MapSelect.NPCData.Count)
                return null;
            var npcData = MapSelect.NPCData[npcRef];
            GameObject obj = CreateNewNPCOnMap(npcData);
            if (obj != null)
            {
                obj.name = detail.Hash;
                obj.AddComponent<TriggerDetailBehaviour>().Data = detail;
                var high = MapSelect.GetHighRef(tileX, tileY) * 0.5f;
                obj.transform.position = new Vector3((0.5f + tileX) * CellSize.x, high + 1, -(0.5f + tileY) * CellSize.y);
                var rotateRef = MapSelect.GetRotateRef(tileX, tileY);
                obj.transform.rotation = Quaternion.Euler(0, 180 - rotateRef, 0);
                //Fix NavMeshAgent not auto add
                obj.GetComponent<NavMeshAgent>().enabled = false;
                obj.GetComponent<NavMeshAgent>().enabled = true;

                AddObjToTriggerNode(obj.transform, true);

                TriggerGame.Instance.CallBack_CreateNPC(detail, npcRef, obj, npcData, tileX, tileY);
            }
            return obj;
        }

        public void ShowNPCInHouse(House houseData, House.NpcInHouse npcRef, GameObject interior)
        {
            var npcData = MapSelect.NPCData[npcRef.IdxNPC];
            var obj = CreateNewNPCOnMap(npcData);
            if (obj != null)
            {
                obj.name = "PersonInHouse_" + npcRef.IdxNPC;
                obj.transform.parent = interior.transform;
                obj.transform.position = npcRef.NPC_OffsetOut;
                Quaternion rotation = Quaternion.Euler(0, npcRef.NPC_CamOut.y, 0);
                obj.transform.rotation = rotation;
                //Fix NavMeshAgent not auto add
                obj.GetComponent<NavMeshAgent>().enabled = false;
                obj.GetComponent<NavMeshAgent>().enabled = true;
            }
        }

        public void RemoveNPC(int npcRef)
        {
            var t = GetNPC(npcRef);
            if (t != null)
            {
                t.name = "";
                DestroyImmediate(t.gameObject);
            }
        }
        public void RemoveNPC(TriggerDetail detail)
        {
            var t = GetNPC(detail);
            if (t != null)
            {
                t.name = "";
                DestroyImmediate(t.gameObject);
            }
        }

        public GameObject GetNPC(int npcRef)
        {
            if (npcRef < 0 || npcRef >= MapSelect.NPCData.Count)
                return null;
            string name = "Person_" + npcRef;
            for (int i = 0; i < TriggerNode.childCount; i++)
            {
                var c = TriggerNode.GetChild(i);
                if(c.name.IndexOf(name) == 0){
                    return c.gameObject;
                }
            }
            return null;
        }

        public GameObject CreateItem(string patch, int tileX, int tileY)
        {
            GameObject prefab = Resources.Load<GameObject>(patch);
            // var obj = new GameObject();
            if (prefab != null)
            {
                // var art = new GameObject();
                var obj = Instantiate(prefab);
                // obj.name = patch;
                obj.transform.rotation = Quaternion.identity;
                // art.transform.localPosition = Vector3.zero + new Vector3( 0.5f, 0.5f, 0.5f);
                var high = MapSelect.GetHighRef(tileX, tileY) * 0.5f;
                obj.transform.position = new Vector3((0.5f + tileX) * CellSize.x, high + 1, -(0.5f + tileY) * CellSize.y);
                AddObjToTriggerNode(obj.transform, false);
                // var rotateRef = MapSelect.GetRotateRef(tileX, tileY);
                // obj.transform.rotation =  Quaternion.Euler( 0, 180 + rotateRef , 0);
                return obj;
            }
            return null;
        }

        public GameObject CreateDoorForHouse(int overlayRef, SlotAon slot, House houseData, int tileX, int tileY)
        {
            GameObject prefab = Resources.Load<GameObject>("OBJ/DoorInteraction2");
            if (prefab == null)
            {
                return null;
            }
            GameObject door = Instantiate(prefab);

            var high = MapSelect.GetHighRef(tileX, tileY) * 0.5f;

            int xOffsetIn = 0, yOffsetIn = 0;
            var rotateRef = MapSelect.GetRotateRef(tileX, tileY);
            houseData.GetOffsetFromRotate(ref xOffsetIn, ref yOffsetIn, rotateRef);

            door.transform.position = new Vector3((0.5f + tileX + xOffsetIn) * CellSize.x, high + 1, -(0.5f + tileY + yOffsetIn) * CellSize.y);

            door.transform.rotation = Quaternion.Euler(0, 180 + rotateRef, 0);
            var col = door.AddComponent<BoxCollider>();
            col.center = new Vector3(0, -0.5f, 0);
            col.isTrigger = true;

            AddObjToTriggerNode(door.transform, false);

            return door;
        }

        private ARPGCameraController mCamControler;
        public ARPGCameraController CamControler{
            get{
                if(mCamControler == null){
                    mCamControler = PlayCamera.GetComponent<ARPGCameraController>();
                }
                return mCamControler;
            }
        }

        public Vector3 InfoMainCam
        {
            get
            {
                var cam = CamControler;
                Vector3 v_cam = new Vector3(cam.camXAngle, cam.yCam, cam.distance);
                return v_cam;
            }
            set
            {
                var cam = CamControler;
                cam.camXAngle = value.x;
                cam.yCam = value.y;
                cam.distance = value.z;
            }
        }

        public float FaceYMainChar
        {
            set
            {
                Quaternion rotation = Quaternion.Euler(0, value, 0);
                Agent.transform.rotation = rotation;
            }
        }

        public bool ControlCameraMainChar
        {
            set
            {
                var cam = CamControler;
                cam.canControl = value;
            }
        }

        private void _updateRealTime(){
            if(m_dayNightCycle != null){
                m_dayNightCycle.GetRealTime();
                m_dayNightCycle.UpdateTime();
                _updateLightByTime();
            }
        }

        public void _updateTimeSun(){
            if(m_dayNightCycle != null){
                m_dayNightCycle.SetCustomTime(12);
                m_dayNightCycle.UpdateTime();
                _updateLightByTime();
            }
        }

        public void UpdateTime1Hour(){
            if(m_dayNightCycle != null){
                m_dayNightCycle.SetCustomTime(m_dayNightCycle.Hour + 1);
                m_dayNightCycle.UpdateTime();
                _updateLightByTime();
            }
        }

        private void _updateLightByTime(){
            float activateTime = 20f;
            float deactivateTime = 4f;
            if (m_dayNightCycle.Hour >= deactivateTime && m_dayNightCycle.Hour < activateTime)
            {
                SetLightActive(false);
            }
            else
            {
                SetLightActive(true);
            }
        }

        public void NextBase(){
            PropertysGame.Instance.UnEquipAll();
            Agent.GetComponentInChildren<CharGame>().NextBase();
        }
        
        public void NextSkin(){
            Agent.GetComponentInChildren<CharGame>().NextSkin();
        }

        public void NextCostume(){
            PropertysGame.Instance.UnEquipAll();
            Agent.GetComponentInChildren<CharGame>().NextCostume();
            // string error = "";
            // Agent.GetComponentInChildren<CharGame>().AddEquipment("top_girl_02_black", ref error);
        }
    }
}
