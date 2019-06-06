using System.Collections;
using System.Collections.Generic;
using AON.RpgMapEditor;
using UnityEngine;

public class HighChunk : MonoBehaviour {
	private Vector3[] m_vertices;
	private Vector2[] m_uv;
	private int[] m_triangles;
	private Color32[] m_colors;

	public AutoTileMap MyAutoTileMap;
	private MeshFilter m_meshFilter;
	public int TileWidth = 8;
	public int TileHeight = 4;
	public int MapLayerIdx = 0;
	public int StartTileX = 0;
	public int StartTileY = 0;

	void OnDestroy() {
		//avoid memory leak
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		if (meshFilter != null && meshFilter.sharedMesh != null)
		{
			DestroyImmediate(meshFilter.sharedMesh);
		}
	}

	public void Configure (AutoTileMap autoTileMap, int layer, int startTileX, int startTileY, int tileChunkWidth, int tileChunkHeight) {
		MyAutoTileMap = autoTileMap;
		TileWidth = tileChunkWidth;
		TileHeight = tileChunkHeight;
		MapLayerIdx = layer;
		StartTileX = startTileX;
		StartTileY = startTileY;

		// transform.gameObject.name = "TileChunk_"+startTileX+"_"+startTileY;
		
		// Vector3 vPosition = new Vector3();
		// vPosition.x = startTileX * MyAutoTileMap.CellSize.x;
		// vPosition.y = -startTileY * MyAutoTileMap.CellSize.y;
		// transform.localPosition = vPosition;
	}

	private Material AtlasMaterial() {
		// var AtlasMaterial = new Material( Shader.Find("Sprites/Default") );
		var AtlasMaterial = new Material( Shader.Find("Unlit/Transparent") );
		// var AtlasMaterial = new Material( Shader.Find("Unlit/Texture") );
		AtlasMaterial.mainTexture = MyAutoTileMap.Tileset.TextureSlot;
		return AtlasMaterial;
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

	public void RefreshHigh() {
		CheckMeshRenderer();
		// Debug.Log("TileChunk:RefreshTileData:" + this.transform.parent.name);
		if (m_meshFilter.sharedMesh == null)
		{
			m_meshFilter.sharedMesh = new Mesh();
			m_meshFilter.sharedMesh.hideFlags = HideFlags.DontSave;
		}
		Mesh mesh = m_meshFilter.sharedMesh;
		mesh.Clear();

		FillData();

		mesh.vertices = m_vertices;
		mesh.colors32 = m_colors;
		mesh.uv = m_uv;
		mesh.triangles = m_triangles;

		mesh.RecalculateNormals(); // allow using lights
	}

	void FillData() {
		m_vertices = new Vector3[TileWidth * TileHeight * 4]; // 4 subtiles x 4 vertex per tile
		m_colors = new Color32[TileWidth * TileHeight * 4];
		m_uv = new Vector2[m_vertices.Length];
		m_triangles = new int[TileWidth * TileHeight * 2 * 3]; // 4 subtiles x 2 triangles per tile x 3 vertex per triangle

		int vertexIdx = 0;
		int triangleIdx = 0;
		
		int mapWidth = MyAutoTileMap.MapTileWidth;
		int mapHeight = MyAutoTileMap.MapTileHeight;
		for (int tileX = 0; tileX < TileWidth; ++tileX)
		{
			for (int tileY = 0; tileY < TileHeight; ++tileY)
			{
				int tx = StartTileX + tileX;
				int ty = StartTileY + tileY;
				if (tx >= mapWidth || ty >= mapHeight) continue;
				var high = GetHigh(StartTileX + tileX, StartTileY + tileY, MapLayerIdx);
				if ( high >= 0)
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
					
					Texture2D tex = MyAutoTileMap.Tileset.TextureSlot;
					Rect sprTileRect = new Rect( high * 32, tex.height - 32, 32, 32);
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

	private Vector2 CellSize(){
		return MyAutoTileMap.CellSize;
	}

	private int GetHigh(int gridX, int gridY, int iLayer) {
		// var autoTile = MyAutoTileMap.GetAutoTile( gridX, gridY, iLayer);
		// if(autoTile.Id < 0){
		// 	return -1;
		// }
		var high = MyAutoTileMap.MapSelect.GetHighRef(gridX, gridY);
		// if(high == -1){
		// 	high = 0;
		// }
		return high;
	}
}
