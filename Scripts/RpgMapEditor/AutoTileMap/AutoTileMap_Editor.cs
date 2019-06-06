using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AON.RpgMapEditor
{
	public class AutoTileMap_Editor : AutoTileMap {

		public static AutoTileMap_Editor Instance { get; private set; }

		void Awake()
        {
            if (Instance == null)
            {
                // Application.targetFrameRate = 60;
                //DontDestroyOnLoad(gameObject); //check how to deal this after make demo with transitions. Should be only one AutoTileMap instance but not persistent
                Instance = this;
                // m_mapData = new AutoTileMapData();
                // if (m_mapData == null)
                // {
                //     m_mapData = new AutoTileMapData();
                // }
                //Force Init
                // m_mapData = new AutoTileMapData();
                #if UNITY_EDITOR
                // Load file AllMap on Mac
                var path = Application.persistentDataPath + "/AllMap.json";
                if (File.Exists(path))
                {
                    var data = File.ReadAllText(path);
                    m_mapData = new AutoTileMapData();
                    if (m_mapData.LoadDataWorld(data, true) == true)
                    {
                        Debug.Log("Load all map success");
                    }else{
                        m_mapData.CheckAndInit();
                    }
                }
                #else
                TextAsset targetFile = (TextAsset)Resources.Load("Maps/AllMap", typeof(TextAsset));
                if (targetFile != null && !string.IsNullOrEmpty(targetFile.text))
                {
                    m_mapData = new AutoTileMapData();
                    if (m_mapData.LoadDataWorld(targetFile.text, true) == true)
                    {
                        Debug.Log("Load all map success");
                    }else{
                        m_mapData.CheckAndInit();
                    }
                }
                #endif
                if(m_mapData == null){
                    m_mapData = new AutoTileMapData();
                    m_mapData.CheckAndInit();   
                }
                m_mapIndex = m_mapData.MapIndex;
                // #if UNITY_EDITOR
                // if(m_mapData == null){
                //     m_mapData = Resources.Load<AutoTileMapData>("Maps/AutoTileMapData");
                //     if (m_mapData != null)
                //     {
                //         m_mapData.CheckAndInit();
                //         m_mapIndex = m_mapData.m_mapIndex;
                //     }else
                //     {
                //         m_mapData = new AutoTileMapData();
                //         m_mapData.CheckAndInit();
                //         m_mapIndex = 0;   
                //     }
                // }else
                // {
                //     m_mapData.CheckAndInit();
                //     m_mapIndex = m_mapData.m_mapIndex;
                // }
                // #else
                //     m_mapData = Resources.Load<AutoTileMapData>("Maps/AutoTileMapData_Build");
                //     if (m_mapData != null)
                //     {
                //         m_mapData.CheckAndInit();
                //         m_mapIndex = m_mapData.m_mapIndex;
                //     }else
                //     {
                //         m_mapData = new AutoTileMapData();
                //         m_mapData.CheckAndInit();
                //         m_mapIndex = 0;   
                //     }
                // #endif
                if(m_mapIndex < 0 || m_mapIndex >= m_mapData.Maps.Count){
                    m_mapIndex = 0;
                }
                if (CanBeInitialized())
                {
                    ViewCamera.enabled = true;
                    PlayCamera.enabled = false;
                    ViewCamera.gameObject.SetActive(true);
                    PlayCamera.gameObject.SetActive(false);
                    Agent.SetActive(false);
                    DefauseInfoMainCam = InfoMainCam;
                    // ForceReloadMapNow();
                    // BrushGizmo.Clear();
                    // #if UNITY_EDITOR
                    // #else
                    StartCoroutine(PlayAsync(MapIdxSelect, MapSelect.StartX, MapSelect.StartY));
                    // #endif
                }
                else
                {
                    Debug.LogWarning(" Autotilemap cannot be initialized because Tileset and/or Map Data is missing. Press create button in the inspector to create the missing part or select one.");
                }
            }
            else if (Instance != this)
            {
				Debug.LogWarning("Init twine");
            }
        }

        void OnDisable()
        {
            if (IsInitialized)
            {
                SaveMap();
            }
        }
	}
}
