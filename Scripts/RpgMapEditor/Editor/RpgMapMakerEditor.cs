using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace AON.RpgMapEditor
{
	public class RpgMapMakerEditor : EditorWindow 
	{
		[MenuItem ("Assets/Create/RpgMapEditor/AutoTileset")]
		public static AutoTileset CreateTileset() 
		{
            /* old way, by opening save file dialog
			string assetPath = GetUniqueAssetPath("AutoTileset");

			if( string.IsNullOrEmpty( assetPath ) )
			{
				return null;
			}
			else
			{
				AutoTileset autoTileSet = ScriptableObject.CreateInstance<AutoTileset>();
				AssetDatabase.CreateAsset( autoTileSet, assetPath );
				AssetDatabase.Refresh();
				return autoTileSet;
			}
            */

            return CreateAssetInSelectedDirectory<AutoTileset>();
		}

		public static AutoTileMapData CreateAutoTileMapData() 
		{
			string assetPath = GetUniqueAssetPath("AutoTileMapData");

			if( string.IsNullOrEmpty( assetPath ) )
			{
				return null;
			}
			else
			{
				AutoTileMapData autoTileMapData = ScriptableObject.CreateInstance<AutoTileMapData>();
				autoTileMapData.CreateAutoTileMapData();
				AssetDatabase.CreateAsset( autoTileMapData, assetPath );
				AssetDatabase.Refresh();
				return autoTileMapData;
			}
		}

		[MenuItem("GameObject/RpgMapEditor/AutoTileMap", false, 10)]
		public static void CreateAutoTileMap() 
		{
			GameObject objTilemap = new GameObject();
			objTilemap.name = "AutoTileMap";
			objTilemap.AddComponent<AutoTileMap>();
		}

        // [MenuItem("GameObject/RpgMapEditor/Directional Animation Character", false, 10)]
        // public static void CreateDirAnimCharacter()
        // {
        //     GameObject obj = new GameObject("Character");
        //     DirectionalAnimation CharAnimationController = obj.AddComponent<DirectionalAnimation>();
        //     SpriteRenderer sprRenderer = obj.AddComponent<SpriteRenderer>();
        //     CharAnimationController.TargetSpriteRenderer = sprRenderer;
        //     Selection.activeObject = obj;
        //     Camera sceneCam = (SceneView.currentDrawingSceneView ?? SceneView.lastActiveSceneView).camera;
        //     obj.transform.position = sceneCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
        //     obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, -0.5f);
        // }

        // [MenuItem("GameObject/RpgMapEditor/Legacy/Character", false, 10)]
        // public static void CreateCharacter()
        // {
        //     GameObject obj = new GameObject("Character");
        //     CharAnimationController CharAnimationController = obj.AddComponent<CharAnimationController>();
        //     GameObject spriteObj = new GameObject("Sprite");
        //     spriteObj.transform.parent = obj.transform;
        //     SpriteRenderer sprRenderer = spriteObj.AddComponent<SpriteRenderer>();
        //     CharAnimationController.TargetSpriteRenderer = sprRenderer;
        //     Selection.activeObject = obj;
        //     Camera sceneCam = (SceneView.currentDrawingSceneView ?? SceneView.lastActiveSceneView).camera;
        //     obj.transform.position = sceneCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
        //     obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, -0.5f);
        // }

		public static string GetUniqueAssetPath( string name )
		{
			Object obj = Selection.activeObject;
			string assetPath = AssetDatabase.GetAssetPath(obj);
			if (assetPath.Length == 0)		
			{
				assetPath = EditorUtility.SaveFilePanel( "Save asset prefab",	"",	name + ".prefab",	"prefab");
				string cwd = System.IO.Directory.GetCurrentDirectory().Replace("\\","/") + "/assets/";
				if (assetPath.ToLower().IndexOf(cwd.ToLower()) != 0)
				{
					EditorUtility.DisplayDialog("Error saving asset", "You must save the asset inside the Asset Folder", "Ok");
				}
				else 
				{
					assetPath = assetPath.Substring(cwd.Length - "/assets".Length);
				}
			}
			else
			{
				// is a directory
				string path = System.IO.Directory.Exists(assetPath) ? assetPath : System.IO.Path.GetDirectoryName(assetPath);
				assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + name + ".prefab");
			}
			return assetPath;
		}

        public static T CreateAssetInSelectedDirectory<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string objName = typeof(T).ToString();
            string objExt = Path.GetExtension(objName);
            if (!string.IsNullOrEmpty(objExt))
            {
                objName = objExt.Remove(0, 1);
            }
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + objName + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            return asset;
        }
	}
}