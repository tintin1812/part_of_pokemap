using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AON.RpgMapEditor;
using UnityEngine;

public class OccHelper : MonoBehaviour{
	[SerializeField]
	private AutoTileMap m_autoTileMap;
	[SerializeField]
	private bool IsShowArt = true;
	[SerializeField]
	private Renderer[] rs0 = null;

	public void Initialize (AutoTileMap autoTileMap)
	{
		/*
		m_autoTileMap = autoTileMap;
		IsShowArt = true;
		if(rs0 == null){
			var art = transform.Find("art");
			if(art != null){
				rs0 = art.GetComponentsInChildren<Renderer>(false).ToArray ();
			}
		}
		*/
		// rs0 = transform.GetComponentsInChildren<Renderer>(false).Where(
		// 	x => x.gameObject.GetComponent<Light>() == null
		// ).ToArray ();
	}

	static public int offSetShowRight = 8;
	static public int offSetShowLeft = 30;
	static public int offSetShowTop = 30;
	static public int offSetShowBot = 8;
	public const int k_TileChunkWidth = 8;
	public const int k_TileChunkHeight = 8;

	/*
	/// <summary>
	/// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
	/// </summary>
	void FixedUpdate()
	{
		if(m_autoTileMap && m_autoTileMap.IsPlayMode && rs0 != null){
			var posAgent = m_autoTileMap.Agent.transform.position;
			var xAgent = posAgent.x;
			var yAgent = -posAgent.z;
			float minX = transform.position.x - offSetShowLeft;
			float maxX = transform.position.x + k_TileChunkWidth + offSetShowRight;
			float minY = -transform.position.z - offSetShowBot;
			float maxY = -transform.position.z + k_TileChunkHeight + offSetShowTop;
			if(xAgent > minX
			&& xAgent < maxX
			&& yAgent > minY
			&& yAgent < maxY){
				if(!IsShowArt){
					// for( int i = 0; i < transform.childCount; i ++){
					// 	transform.GetChild(i).gameObject.SetActive(true);
					// }
					for(int i=0;i<rs0.Length;i++)
					{
						rs0[i].enabled = true;
					}
					IsShowArt = true;
				}
			}else{
				if(IsShowArt){
					// for( int i = 0; i < transform.childCount; i ++){
					// 	transform.GetChild(i).gameObject.SetActive(false);
					// }
					for(int i=0;i<rs0.Length;i++)
					{
						rs0[i].enabled = false;
					}
					IsShowArt = false;
				}
			}
		}
	}
	*/
}
