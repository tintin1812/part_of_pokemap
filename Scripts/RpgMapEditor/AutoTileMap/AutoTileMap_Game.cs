using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AON.RpgMapEditor
{
	public class AutoTileMap_Game : AutoTileMap {

		public static AutoTileMap_Game Instance { get; private set; }

		void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
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
                if(m_mapIndex < 0 || m_mapIndex >= m_mapData.Maps.Count){
                    m_mapIndex = 0;
                }
                if (CanBeInitialized())
                {
                    // if( Application.isPlaying && ViewCamera && ViewCamera.name == "SceneCamera" )
                    // {
                    // 	ViewCamera = null;
                    // }
                    ViewCamera.enabled = true;
                    PlayCamera.enabled = false;
                    ViewCamera.gameObject.SetActive(true);
                    PlayCamera.gameObject.SetActive(false);
                    Agent.SetActive(false);
                    DefauseInfoMainCam = InfoMainCam;
                    ForceReloadMapNow();
                    BrushGizmo.Clear();
                }
                else
                {
                    Debug.LogWarning(" Autotilemap cannot be initialized because Tileset and/or Map Data is missing. Press create button in the inspector to create the missing part or select one.");
                }
            }
            else if (Instance != this)
            {
                if (CanBeInitialized())
                {
                    if (Application.isPlaying && ViewCamera && ViewCamera.name == "SceneCamera")
                    {
                        ViewCamera = null;
                    }
                    ForceReloadMapNow();
                    if (BrushGizmo != null)
                    {
                        BrushGizmo.Clear();
                    }
                }
                else
                {
                    Debug.LogWarning(" Autotilemap cannot be initialized because Tileset and/or Map Data is missing. Press create button in the inspector to create the missing part or select one.");
                }
            }
        }
	}
}