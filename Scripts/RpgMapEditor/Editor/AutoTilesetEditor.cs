using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Linq;

namespace AON.RpgMapEditor
{
	[CustomEditor(typeof(AutoTileset))]
	public class AutoTilesetEditor : Editor
	{
		AutoTileset MyAutoTileset{ get{ return (AutoTileset)target; } }

        // int m_vSlots = 2;
        // int m_hSlots = 2;
        
		public override void OnInspectorGUI() 
		{
			// Update the serializedProperty - always do this in the beginning of OnInspectorGUI.

			serializedObject.Update();
            #if UNITY_EDITOR
            EditorGUILayout.Space();
            if (GUILayout.Button("Export")){
                string filePath = EditorUtility.SaveFilePanel( "Save tileset",	"",	"tileset" + ".json", "json");
                if( filePath.Length > 0 )
                {
                    // string json = JsonUtility.ToJson(MyAutoTileset, true);
                    string json = UtilsAON.SerializeObject(this);
                    File.WriteAllText(filePath, json);
                }
            }
            if (GUILayout.Button("Import")){
                string filePath = EditorUtility.OpenFilePanel( "Load tileset",	"", "json");
                if( filePath.Length > 0 )
                {
                    var s = File.ReadAllText(filePath);
                    JsonUtility.FromJsonOverwrite(s, target);
                    // AutoTileset obj = JsonUtility.FromJson<AutoTileset>(s);
                    // target = obj;
                    // UtilsAutoTileMap.ImportTexture(MyAutoTileset.AtlasTexture);
                }
            }
	        #endif
            serializedObject.ApplyModifiedProperties();
            DrawDefaultInspector ();
		}
	}
}