using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AON.RpgMapEditor
{
	public class AutoTileMap_Pick : AutoTileMap {
		
		void Awake()
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