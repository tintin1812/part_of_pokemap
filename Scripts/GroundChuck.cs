using System.Collections;
using System.Collections.Generic;
using AON.RpgMapEditor;
using UnityEngine;

public class GroundChuck : MonoBehaviour
{
    private Vector3[] m_vertices;
    private Vector2[] m_uv;
    private int[] m_triangles;
    private Color32[] m_colors;

    private AutoTileMap m_autoTileMap;
    private MeshFilter m_meshFilter;
    public int TileWidth = 8;
    public int TileHeight = 4;
    public int MapLayerIdx = 0;
    public int StartTileX = 0;
    public int StartTileY = 0;
    public int StartTileX2 = 0;
    public int StartTileY2 = 0;
    public int High = 0;
    void OnDestroy()
    {
        //avoid memory leak
        if (m_meshFilter != null && m_meshFilter.sharedMesh != null)
        {
            DestroyImmediate(m_meshFilter.sharedMesh);
        }
    }

    public void Configure(AutoTileMap autoTileMap, int layer, int startTileX, int startTileY, int tileChunkWidth, int tileChunkHeight, int high, ref bool[,] interior, ref bool[,] interiorNext)
    {
        m_autoTileMap = autoTileMap;
        TileWidth = tileChunkWidth;
        TileHeight = tileChunkHeight;
        MapLayerIdx = layer;
        StartTileX = startTileX;
        StartTileY = startTileY;
        StartTileX2 = StartTileX << 1;
        StartTileY2 = StartTileY << 1;
        High = high;
        // if(High > 0)
        int TileWidthX2 = TileWidth * 2;
        int TileHeightX2 = TileHeight * 2;
        {
            if (interior == null)
            {
                interior = new bool[TileWidthX2, TileHeightX2];
                // for( int x = 0 ; x < TileWidth; x++){
                // 	for( int y = 0 ; y < TileHeight; y++){
                // 		interior[x, y] = false;
                // 	}
                // }
            }
            if (interiorNext == null)
            {
                interiorNext = new bool[TileWidthX2, TileHeightX2];
                // for( int x = 0 ; x < TileWidth; x++){
                // 	for( int y = 0 ; y < TileHeight; y++){
                // 		interiorNext[x, y] = false;
                // 	}
                // }
            }
            bool isHaveTerrain = false;
            BuildTerrainPlayMode(ref interior, ref interiorNext);
            if (interior != null)
            {
                for (int x = 0; x < TileWidthX2; x++)
                {
                    for (int y = 0; y < TileHeightX2; y++)
                    {
                        if (interior[x, y])
                        {
                            isHaveTerrain = true;
                            break;
                        }
                    }
                }
            }
            if (isHaveTerrain)
            {
                RefreshTile(ref interior);
            }
        }
        // else{
        // 	RefreshTile(ref interior);
        // }
    }

    private Material AtlasMaterial()
    {
        // var AtlasMaterial = new Material(Shader.Find("Unlit/Texture"));
        // var AtlasMaterial = new Material( Shader.Find("Unlit/Transparent") );
        // var AtlasMaterial = new Material(Shader.Find("Legacy Shaders/Transparent/Cutout/Diffuse"));
        // AtlasMaterial.mainTexture = MyAutoTileMap.Tileset.DrawTileAonSetSelected.TextureThumb;
        // return AtlasMaterial;
        return m_autoTileMap.Tileset.Material_GroundChuck;
    }

    private void CheckMeshRenderer()
    {
        if (m_meshFilter == null)
        {
            var obj_Floor = new GameObject();
            obj_Floor.name = "Floor";
            obj_Floor.transform.parent = transform;
            obj_Floor.transform.localPosition = Vector3.zero;

            MeshRenderer meshRenderer = obj_Floor.AddComponent<MeshRenderer>();

            meshRenderer.sharedMaterial = this.AtlasMaterial();

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
#else
			meshRenderer.castShadows = false;
#endif
            meshRenderer.receiveShadows = false;

            m_meshFilter = obj_Floor.AddComponent<MeshFilter>();
        }
    }

    public void RefreshTile(ref bool[,] interior)
    {
        CheckMeshRenderer();
        // Debug.Log("TileChunk:RefreshTileData:" + this.transform.parent.name);
        if (m_meshFilter.sharedMesh == null)
        {
            m_meshFilter.sharedMesh = new Mesh();
            m_meshFilter.sharedMesh.hideFlags = HideFlags.DontSave;
        }
        Mesh mesh = m_meshFilter.sharedMesh;
        mesh.Clear();

        FillData(interior);

        mesh.vertices = m_vertices;
        mesh.colors32 = m_colors;
        mesh.uv = m_uv;
        mesh.triangles = m_triangles;

        mesh.RecalculateNormals(); // allow using lights
    }

    void FillData(bool[,] interior)
    {
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
                // var h = MyAutoTileMap.MapSelect.GetHighRef(tx, ty);
                // if( h != High) continue;
                // int tileIdx = ty * MyAutoTileMap.MapTileWidth + tx;
                AutoTile autoTile = m_autoTileMap.GetAutoTile(tx, ty, MapLayerIdx);
                if (autoTile.Id >= 0 && m_autoTileMap.Tileset.IsExitSlot(autoTile.Id))
                {
                    var slot = m_autoTileMap.Tileset.GetSlot(autoTile.Id);
                    // int subTileXBase = tx * 2; // <<1 == *2
                    // int subTileYBase = ty * 2; // <<1 == *2
                    int tx2 = tx << 1;
                    int ty2 = ty << 1;
                    int subTileXBase = _tileX << 1; // <<1 == *2
                    int subTileYBase = _tileY << 1; // <<1 == *2
                    for (int xf = 0; xf < 2; ++xf)
                    {
                        // if(xf == 0){
                        // 	if( MyAutoTileMap.MapSelect.GetHighRef(tx - 1, ty) < High) continue;
                        // }else{
                        // 	if( MyAutoTileMap.MapSelect.GetHighRef(tx + 1, ty) < High) continue;
                        // }
                        for (int yf = 0; yf < 2; ++yf)
                        {
                            if (interior != null)
                            {
                                bool isInterior = interior[subTileXBase + xf, subTileYBase + yf];
                                if (isInterior == false) continue;
                            }
                            float u0 = 0, u1 = 0, v0 = 0, v1 = 0;
                            #region Caculator
                            CaculatorTilePartType(ref u0, ref u1, ref v0, ref v1, xf, yf, subTileXBase, subTileYBase, tx, ty, slot, autoTile, m_autoTileMap, MapLayerIdx, High);
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

    private void BuildTerrainPlayMode(ref bool[,] interior, ref bool[,] interiorNext)
    {
        // Debug.Log("Add Ground");
        int mapWidth = MapTileWidth();
        int mapHeight = MapTileHeight();
        //Ground
        var obj_GroundSide = new GameObject();
        obj_GroundSide.name = "Terrain";
        var obj_Stair = new GameObject();
        obj_Stair.name = "Stair";
        var obj_Water = new GameObject();
        obj_Water.name = "Water";
        // var dataArt = MyAutoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRefFloor];
        // if(High % 2 == 0){
        // 	dataArt = MyAutoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRefFloor + 1];
        // }
        int endX2 = StartTileX2 + TileWidth * 2;
        int endY2 = StartTileY2 + TileHeight * 2;
        for (int tX2 = StartTileX2; tX2 < endX2; ++tX2)
        {
            for (int tY2 = StartTileY2; tY2 < endY2; ++tY2)
            {
                // var h = GetHighX2(tX2, tY2);
                // if(h < High){
                // 	continue;
                // }
                // if( h != 1)
                // 	continue;
                // h = MyAutoTileMap.MapSelect.GetHighRef(tile_x / 2, tile_y / 2);
                // int idRefFloor = MyAutoTileMap.Tileset.DrawTile3DAonSetSelected.idRefFloor;
                // var dataArt = MyAutoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRefFloor];
                BuildTerrainObj(obj_GroundSide, obj_Stair, obj_Water, tX2, tY2, ref interior, ref interiorNext);
            }
        }
        if (obj_GroundSide.transform.childCount > 0)
        {
            TileChunk.CombineMesh(obj_GroundSide);
            obj_GroundSide.transform.parent = transform;
            obj_GroundSide.transform.localPosition = Vector3.zero;
        }
        else
        {
            DestroyImmediate(obj_GroundSide);
        }
        if (obj_Stair.transform.childCount > 0)
        {
            TileChunk.CombineMesh(obj_Stair);
            obj_Stair.transform.parent = transform;
            obj_Stair.transform.localPosition = Vector3.zero;
        }
        else
        {
            DestroyImmediate(obj_Stair);
        }
        if (obj_Water.transform.childCount > 0)
        {
            TileChunk.CombineMesh(obj_Water);
            obj_Water.transform.parent = transform;
            obj_Water.transform.localPosition = Vector3.zero;
            obj_Water.GetComponent<Renderer>().sharedMaterial = m_autoTileMap.TileChunkPoolNode.MaterialWater;
        }
        else
        {
            DestroyImmediate(obj_Water);
        }
    }

    private void BuildTerrainObj(GameObject parentGround, GameObject parentStair, GameObject parentWater, int tX2, int tY2, ref bool[,] interior, ref bool[,] interiorNext)
    {
        #region  Art
        int tX = tX2 / 2;
        int tY = tY2 / 2;
        int slotId = m_autoTileMap.GetAutoTile(tX, tY, 0).Id;
        // int currentHigh = GetHighX2(tX2, tY2);
        // if(High == 2 && tX2 == 75 && tY2 == 81){
        // 	Debug.Log("Pause");
        // }
        if (High == 0 && tX2 % 2 == 0 && tY2 % 2 == 0 && IsDrawWarter(tX, tY))
        {
            // GroundChuck.eTilePartType tilePartType = GroundChuck.eTilePartType.INTERIOR;
            // int offsetRotate = 0;
            // SetupWater ( ref tile3DAon, ref data, ref tilePartType, ref interior, ref offsetRotate);
            int idRefWater3D = 11;
            if (idRefWater3D >= 0 && idRefWater3D < m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons.Count)
            {
                DrawTile3DAon data = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRefWater3D];
                //Up
                BuildTerrainArtX1(data.corner_2, data, parentWater.transform, tX, tY, 0, "");
                //Down
                BuildTerrainArtX1(data.interior, data, parentWater.transform, tX, tY, 0, "");
                //Side
                // bool isTopEmpty = MyAutoTileMap.GetAutoTile( tX, tY + 1, 0).Id == -1;
                bool isTopEmpty = !IsDrawWarter(tX, tY + 1);
                if (isTopEmpty)
                {
                    BuildTerrainArtX1(data.side, data, parentWater.transform, tX, tY, 0, "");
                }
                // bool isBotEmpty = MyAutoTileMap.GetAutoTile( tX, tY - 1, 0).Id == -1;
                bool isBotEmpty = !IsDrawWarter(tX, tY - 1);
                if (isBotEmpty)
                {
                    BuildTerrainArtX1(data.side, data, parentWater.transform, tX, tY, 180, "");
                }

                // bool isLeftEmpty = MyAutoTileMap.GetAutoTile( tX - 1, tY, 0).Id == -1;
                bool isLeftEmpty = !IsDrawWarter(tX - 1, tY);
                if (isLeftEmpty)
                {
                    BuildTerrainArtX1(data.side, data, parentWater.transform, tX, tY, 90, "");
                }
                // bool isRightEmpty = MyAutoTileMap.GetAutoTile( tX + 1, tY, 0).Id == -1;
                bool isRightEmpty = !IsDrawWarter(tX + 1, tY);
                if (isRightEmpty)
                {
                    BuildTerrainArtX1(data.side, data, parentWater.transform, tX, tY, 270, "");
                }
            }
        }
        /*
		if(false && currentHigh == 0 &&  slotId == DefineAON.IdSlot_Sand) {
			DrawTile3D tile3DAon = null;
			DrawTile3DAon data = null;
			GroundChuck.eTilePartType tilePartType = GroundChuck.eTilePartType.INTERIOR;
			int offsetRotate = 0;
			CaculatorTilePartTerrain( ref tilePartType, ref offsetRotate, tX2, tY2);
			SetupSand( ref tile3DAon, ref data, tX2, tY2, ref tilePartType, ref interior);
			BuildTerrainArtX2( tile3DAon, data, tilePartType, parentGround.transform, tX2, tY2, offsetRotate, "");
		}
		*/
		
		if (slotId == DefineAON.IdSlot_Interior){
			SetupAndBuildArtInterior(tX2, tY2, ref interior, parentGround);
		}else if (slotId != DefineAON.IdSlot_Water)
        {
            SetupAndBuildArtNormal(tX2, tY2, ref interior, parentGround, parentStair);
        }
        /*
		else if(false && slotId == DefineAON.IdSlot_Grass_0 || slotId == DefineAON.IdSlot_Grass_2){
			SetupTerrain( ref tile3DAon, ref data, tX2, tY2, ref tilePartType, ref interior, ref offsetRotate);
			var art = BuildTerrainArt( tile3DAon, data, parentGround.transform, tX2, tY2, offsetRotate);
			if(art != null){
				// Set _Color and _Color_Bot
				var meshs = art.GetComponentsInChildren<MeshRenderer>();
				foreach (var mesh in meshs)
				{
					if(mesh != null && mesh.material != null && mesh.material.shader != null && mesh.material.shader.name == "Unlit/Terrain_Map_0"){
						var _color = mesh.material.GetColor("_Color");
						if(_color != null){
							int tx = tX2 / 2;
							int ty = tY2 / 2;
							int offsetX = tX2 % 2;
							int offsetY = tY2 % 2;
							if(offsetX == 0){
								offsetX = tx - 1;
							}else {
								offsetX = tx + 1;
							}
							if(offsetY == 0){
								offsetY = ty - 1;
							}else {
								offsetY = ty + 1;
							}
							int tileHigh = MyAutoTileMap.getHighX2(tX2, tY2);
							int idTileCenter = MyAutoTileMap.GetAutoTile( tx, ty, 0).Id;
							int idTileOffset = MyAutoTileMap.GetAutoTile( offsetX, offsetY, 0).Id;
							MyAutoTileMap.TileChunkPoolNode.SetMeshRendererColourWithCache( mesh, tile3DAon.prefab_name, tileHigh, idTileCenter, idTileOffset);
						}
					}
				}
			}
		} */
        #endregion
    }

    private GameObject BuildTerrainArtX1(DrawTile3D tile3DAon, DrawTile3DAon data, Transform parent, int x, int y, int offsetRotate, string prefab_sub)
    {
        if (data == null || tile3DAon == null)
        {
            return null;
        }
        if (tile3DAon.prefab_name == "")
        {
            return null;
        }

        GameObject prefab = null;
        if (prefab_sub != "")
        {
            prefab = Resources.Load<GameObject>(tile3DAon.prefab_name + prefab_sub);
        }
        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>(tile3DAon.prefab_name);
        }
        // var obj = new GameObject();
        if (prefab == null)
        {
            Debug.LogError(string.Format("Prefab not find {0} at ({1} , {2})", tile3DAon.prefab_name, x, y));
            return null;
        }

        var art = Instantiate(prefab);
        art.transform.parent = parent;
        if (data.scalePrefab != 1)
        {
            art.transform.localScale = new Vector3(data.scalePrefab, data.scalePrefab, data.scalePrefab);
        }
        var localPosition = new Vector3(0.5f, -0.5f, 0f) + tile3DAon.offsetPos;
        float px0 = (CellSize().x) * (x - StartTileX);
        float py0 = -(CellSize().y) * (y - StartTileY);
        art.transform.localPosition = localPosition + new Vector3(px0, py0, 0f);
        art.transform.Rotate(-90, 0, 0);
        art.transform.Rotate(0, 180, 0);
        art.transform.Rotate(tile3DAon.offsetRotate.x, tile3DAon.offsetRotate.y + offsetRotate, tile3DAon.offsetRotate.z);
        // art.name = tilePartType.ToString() + "_yx_" + tilePartY + "_" + tilePartX + "_r_" + offsetRotate.ToString();
        art.name = "1_xy_" + x + "_" + y;
        return art;
    }

    private GameObject BuildTerrainArtX2(DrawTile3D tile3DAon, DrawTile3DAon data, GroundChuck.eTilePartType tilePartType, Transform parent, int tX2, int tY2, int offsetRotate, string prefab_sub, float offsetH = 0)
    {
        if (data == null || tile3DAon == null)
        {
            return null;
        }
        if (tile3DAon.prefab_name == "")
        {
            return null;
        }
        GameObject prefab = null;
        if (prefab_sub != "")
        {
            prefab = Resources.Load<GameObject>(tile3DAon.prefab_name + prefab_sub);
        }
        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>(tile3DAon.prefab_name);
        }
        // var obj = new GameObject();
        if (prefab == null)
        {
            Debug.LogError(string.Format("Prefab not find {0} at ({1} , {2})", tile3DAon.prefab_name, tX2 / 2, tY2 / 2));
            return null;
        }
        var art = Instantiate(prefab);
        art.transform.parent = parent;
        if (data.scalePrefab != 1)
        {
            art.transform.localScale = new Vector3(data.scalePrefab, data.scalePrefab, data.scalePrefab);
        }
        var localPosition = new Vector3(0.25f, -0.25f, 0f) + tile3DAon.offsetPos;
        float px0 = 0.5f * (CellSize().x) * (tX2 - StartTileX * 2);
        float py0 = -0.5f * (CellSize().y) * (tY2 - StartTileY * 2);
        art.transform.localPosition = localPosition + new Vector3(px0, py0, offsetH);
        art.transform.Rotate(-90, 0, 0);
        art.transform.Rotate(0, 180, 0);
        art.transform.Rotate(tile3DAon.offsetRotate.x, tile3DAon.offsetRotate.y + offsetRotate, tile3DAon.offsetRotate.z);
        // art.name = tilePartType.ToString() + "_yx_" + tilePartY + "_" + tilePartX + "_r_" + offsetRotate.ToString();
        art.name = tX2 + "_" + tY2 + "_" + offsetRotate + "_" + tilePartType.ToString();
        return art;
    }

    private bool BuildStairArtX2(GroundChuck.eTilePartType tilePartType, Transform parent, int tX2, int tY2, int offsetRotate, int tX, int tY)
    {
        if (High <= 0)
        {
            return false;
        }
        if (tilePartType != GroundChuck.eTilePartType.V_SIDE && tilePartType != GroundChuck.eTilePartType.H_SIDE)
        {
            return false;
        }
        int currentOverlay = m_autoTileMap.GetAutoTile(tX, tY, (int)eSlotAonTypeLayer.Overlay).Id;
        if (currentOverlay != DefineAON.IdSlot_Stair)
        {
            return false;
        }
        int highMax = GetHighX2(tX2, tY2);

        if (tilePartType == GroundChuck.eTilePartType.V_SIDE)
        {
            int offsetY = (offsetRotate == 90) ? 1 : -1;
            if (highMax - 1 != GetHighX2(tX2, tY2 + offsetY))
            {
                return false;
            }
            // if( highMax - 1 != GetHighX2(tX2 + 1, tY2 + offsetY)){
            // 	return false;
            // }
            // if( highMax - 1 != GetHighX2(tX2 - 1, tY2 + offsetY)){
            // 	return false;
            // }

            int leftOverlay = m_autoTileMap.GetAutoTile((tX2 - 1) / 2, tY, (int)eSlotAonTypeLayer.Overlay).Id;
            int rightOverlay = m_autoTileMap.GetAutoTile((tX2 + 1) / 2, tY, (int)eSlotAonTypeLayer.Overlay).Id;
            bool isLeft = leftOverlay == DefineAON.IdSlot_Stair && highMax == GetHighX2(tX2 - 2, tY2);
            // if( isLeft && highMax - 1 != GetHighX2(tX2 - 2, tY2 + offsetY)){
            // 	isLeft = false;
            // }
            bool isRight = rightOverlay == DefineAON.IdSlot_Stair && highMax == GetHighX2(tX2 + 2, tY2);
            // if( isRight && highMax - 1 != GetHighX2(tX2 + 2, tY2 + offsetY)){
            // 	isRight = false;
            // }
            if (!isLeft && !isRight)
            {
                return false;
            }
            int idRef_Stair = 6;
            DrawTile3DAon tile3DAon = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRef_Stair];
            if (isLeft && isRight)
            {
                BuildTerrainArtX2(tile3DAon.side, tile3DAon, tilePartType, parent, tX2, tY2, offsetRotate, "");
            }
            else if (isLeft)
            {
                // Left
                BuildTerrainArtX2((offsetRotate == 90) ? tile3DAon.corner_3 : tile3DAon.corner_2, tile3DAon, tilePartType, parent, tX2, tY2, offsetRotate, "");
            }
            else
            {
                // Right
                BuildTerrainArtX2((offsetRotate == 90) ? tile3DAon.corner_2 : tile3DAon.corner_3, tile3DAon, tilePartType, parent, tX2, tY2, offsetRotate, "");
            }
        }
        if (tilePartType == GroundChuck.eTilePartType.H_SIDE)
        {
            int offsetX = (offsetRotate == 0) ? 1 : -1;
            if (highMax - 1 != GetHighX2(tX2 + offsetX, tY2))
            {
                return false;
            }
            // if( highMax - 1 != GetHighX2(tX2 + offsetX, tY2 + 1)){
            // 	return false;
            // }
            // if( highMax - 1 != GetHighX2(tX2 + offsetX, tY2 - 1)){
            // 	return false;
            // }

            int topOverlay = m_autoTileMap.GetAutoTile(tX, (tY2 + 1) / 2, (int)eSlotAonTypeLayer.Overlay).Id;
            int botOverlay = m_autoTileMap.GetAutoTile(tX, (tY2 - 1) / 2, (int)eSlotAonTypeLayer.Overlay).Id;
            bool isTop = topOverlay == DefineAON.IdSlot_Stair && highMax == GetHighX2(tX2, tY2 + 2);
            // if( isTop && highMax - 1 != GetHighX2(tX2 + offsetX, tY2 + 2)){
            // 	isTop = false;
            // }
            bool isBot = botOverlay == DefineAON.IdSlot_Stair && highMax == GetHighX2(tX2, tY2 - 2);
            // if( isBot && highMax - 1 != GetHighX2(tX2 + offsetX, tY2 - 2)){
            // 	isBot = false;
            // }
            if (!isTop && !isBot)
            {
                return false;
            }
            int idRef_Stair = 6;
            DrawTile3DAon tile3DAon = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRef_Stair];
            if (isTop && isBot)
            {
                BuildTerrainArtX2(tile3DAon.side, tile3DAon, tilePartType, parent, tX2, tY2, offsetRotate, "");
            }
            else if (isTop)
            {
                // Top
                BuildTerrainArtX2((offsetRotate == 0) ? tile3DAon.corner_3 : tile3DAon.corner_2, tile3DAon, tilePartType, parent, tX2, tY2, offsetRotate, "");
            }
            else
            {
                // Bot
                BuildTerrainArtX2((offsetRotate == 0) ? tile3DAon.corner_2 : tile3DAon.corner_3, tile3DAon, tilePartType, parent, tX2, tY2, offsetRotate, "");
            }
        }
        return true;
    }

    // private void SetupWater(ref DrawTile3D tile3DAon, ref DrawTile3DAon data, ref GroundChuck.eTilePartType tilePartType, ref bool[,] interior, ref int offsetRotate){
    // 	int idRefWater3D = 11;
    // 	if(idRefWater3D >= 0 && idRefWater3D < MyAutoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons.Count){
    // 		data = MyAutoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRefWater3D];
    // 	}
    // 	if(data != null){
    // 		if(tilePartType == GroundChuck.eTilePartType.EXT_CORNER || tilePartType == GroundChuck.eTilePartType.DOUBLE_EXT_CORNER){
    // 			tile3DAon = data.corner_2;
    // 		}else if(tilePartType == GroundChuck.eTilePartType.H_SIDE || tilePartType == GroundChuck.eTilePartType.V_SIDE){
    // 			tile3DAon = data.side;
    // 		}else if(tilePartType == GroundChuck.eTilePartType.INT_CORNER){
    // 			tile3DAon = data.corner_3;
    // 		}else{
    // 			tile3DAon = data.interior;
    // 		}
    // 	}
    // }

    private void SetupSand(ref DrawTile3D tile3DAon, ref DrawTile3DAon data, int tX2, int tY2, ref GroundChuck.eTilePartType tilePartType, ref bool[,] interior)
    {
        int idRefTile3D = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.idRefSand;
        data = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRefTile3D];
        // tile3DAon = data.interior;
        if (tilePartType == GroundChuck.eTilePartType.EXT_CORNER || tilePartType == GroundChuck.eTilePartType.DOUBLE_EXT_CORNER)
        {
            tile3DAon = data.corner_2;
        }
        else if (tilePartType == GroundChuck.eTilePartType.H_SIDE || tilePartType == GroundChuck.eTilePartType.V_SIDE)
        {
            tile3DAon = data.side;
        }
        else if (tilePartType == GroundChuck.eTilePartType.INT_CORNER)
        {
            tile3DAon = data.corner_3;
        }
        else
        {
            tile3DAon = data.interior;
        }
        if (tilePartType == GroundChuck.eTilePartType.INTERIOR && interior != null && GetHighX2(tX2, tY2) == High)
        {
            int interiorX = tX2 - StartTileX2;
            int interiorY = tY2 - StartTileY2;
            bool t = true;
            interior[interiorX, interiorY] = t;
        }
    }

    private bool SetupStair(ref DrawTile3D tile3DAon, ref DrawTile3DAon data, int tX2, int tY2, ref GroundChuck.eTilePartType tilePartType, ref bool[,] interior, ref int offsetRotate)
    {
        if (GetHighX2(tX2, tY2) != High)
        {
            return false;
        }
        if (tilePartType == GroundChuck.eTilePartType.INTERIOR)
        {
            return false;
        }
        //Check Stair
        int iDStair = -1;
        int gridX = tX2 / 2;
        int gridY = tY2 / 2;
        var tileStair = m_autoTileMap.GetAutoTile(gridX, gridY, (int)eSlotAonTypeLayer.Overlay);
        if (m_autoTileMap.Tileset.IsExitSlot(tileStair.Id))
        {
            var slot = m_autoTileMap.Tileset.GetSlot(tileStair.Id);
            if (slot.TypeObj == eSlotAonTypeObj.Stair)
            {
                iDStair = slot.idRef;
            }
        }
        if (iDStair == -1)
        {
            return false;
        }
        var drawTile3DAons = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons;
        if (iDStair < 0 || iDStair >= drawTile3DAons.Count)
        {
            return false;
        }
        {
            //Check high around can't > 2
            var right = GetHighX2(tX2 + 1, tY2);
            if (right >= High + 2 || right <= High - 2)
            {
                return false;
            }
            var left = GetHighX2(tX2 - 1, tY2);
            if (left >= High + 2 || left <= High - 2)
            {
                return false;
            }
            var top = GetHighX2(tX2, tY2 + 1);
            if (top >= High + 2 || top <= High - 2)
            {
                return false;
            }
            var bot = GetHighX2(tX2, tY2 - 1);
            if (bot >= High + 2 || bot <= High - 2)
            {
                return false;
            }
        }
        var artStair = drawTile3DAons[iDStair];
        if (tilePartType == GroundChuck.eTilePartType.H_SIDE)
        {

            bool isRight = tileStair.Id == m_autoTileMap.GetAutoTile((tX2 + 1) / 2, gridY, 1).Id;
            bool isLeft = tileStair.Id == m_autoTileMap.GetAutoTile((tX2 - 1) / 2, gridY, 1).Id;

            if (isRight)
            {
                bool H_Right2 = High == GetHighX2(tX2 + 2, tY2);
                if (!H_Right2)
                {
                    isRight = false;
                }
            }
            if (isLeft)
            {
                bool H_Left2 = High == GetHighX2(tX2 - 2, tY2);
                if (!H_Left2)
                {
                    isLeft = false;
                }
            }
            if (isRight && isLeft)
            {
                tile3DAon = artStair.interior;
            }
            else if (isRight)
            {
                tile3DAon = (offsetRotate == 0 ? artStair.corner_2 : artStair.corner_3);
            }
            else if (isLeft)
            {
                tile3DAon = (offsetRotate == 0 ? artStair.corner_3 : artStair.corner_2);
            }
            else
            {
                //Single stair
                // tile3DAon = artStair.side;
                return false;
            }
        }
        else if (tilePartType == GroundChuck.eTilePartType.V_SIDE)
        {
            bool isTop = tileStair.Id == m_autoTileMap.GetAutoTile(gridX, (tY2 + 1) / 2, 1).Id;
            bool isBot = tileStair.Id == m_autoTileMap.GetAutoTile(gridX, (tY2 - 1) / 2, 1).Id;
            if (isTop)
            {
                bool H_Top2 = High == GetHighX2(tX2, tY2 + 2);
                if (!H_Top2)
                {
                    isTop = false;
                }
            }
            if (isBot)
            {
                bool H_Bot2 = High == GetHighX2(tX2, tY2 - 2);
                if (!H_Bot2)
                {
                    isBot = false;
                }
            }
            if (isTop && isBot)
            {
                tile3DAon = artStair.interior;
            }
            else if (isTop)
            {
                tile3DAon = (offsetRotate == 90 ? artStair.corner_2 : artStair.corner_3);
            }
            else if (isBot)
            {
                tile3DAon = (offsetRotate == 90 ? artStair.corner_3 : artStair.corner_2);
            }
            else
            {
                //Single stair
                // tile3DAon = artStair.side;
                return false;
            }
        }
        data = drawTile3DAons[iDStair];
        return true;
    }

	private void SetupAndBuildArtInterior(int tX2, int tY2, ref bool[,] interior, GameObject parentGround)
    {
        int highMax = GetHighX2(tX2, tY2);
        if (High != highMax)
        {
            return;
        }
        if (interior != null && highMax == High)
		{
			int interiorX = tX2 - StartTileX2;
			int interiorY = tY2 - StartTileY2;
			bool t = true;
			interior[interiorX, interiorY] = t;
		}
    }

    private void SetupAndBuildArtNormal(int tX2, int tY2, ref bool[,] interior, GameObject parentGround, GameObject parentStair)
    {
        int highMax = GetHighX2(tX2, tY2);
        if (High != highMax)
        {
            return;
        }
        //Add bitmap into interior
        int tX = tX2 / 2;
        int tY = tY2 / 2;

        int currentSlot = m_autoTileMap.GetAutoTile(tX, tY, 0).Id;
        string prefab_sub = "";
        if (currentSlot == DefineAON.IdSlot_Grass_0)
        {
            // Grass
        }
        else if (currentSlot == DefineAON.IdSlot_Dirt_Road)
        {
            prefab_sub = "_Grass_2";
        }else
        {
            //City
            prefab_sub = "_City";
        }
        {
            int idRef_Y = 5;
            int idRef_H = 9;
            GroundChuck.eTilePartType tilePartType = GroundChuck.eTilePartType.INTERIOR;
            int offsetRotate = 0;
            CaculatorTilePartTerrainX2(ref tilePartType, ref offsetRotate, tX2, tY2);
            DrawTile3DAon data = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRef_Y];

            if (!BuildStairArtX2(tilePartType, parentStair.transform, tX2, tY2, offsetRotate, tX, tY))
            {
                DrawTile3D tile3DAon = null;
                SetupTerrainY(ref tile3DAon, ref data, ref tilePartType);
                BuildTerrainArtX2(tile3DAon, data, tilePartType, parentGround.transform, tX2, tY2, offsetRotate, prefab_sub);
            }
            if (tilePartType == GroundChuck.eTilePartType.V_SIDE)
            {
                // int offsetY = tY2 % 2 == 0 ? -1 : 1;
                int offsetY = (offsetRotate == 90) ? 1 : -1;
                int minHigh = HightBuildTerrain(tX2, tY2, 0, offsetY);
                if (minHigh > HightBuildTerrain(tX2 - 1, tY2, 0, offsetY)
                || minHigh > HightBuildTerrain(tX2 + 1, tY2, 0, offsetY))
                {
                    minHigh--;
                }
                if (High - 1 > minHigh)
                {
                    for (int h = High - 1; h > minHigh; h--)
                    {
                        DrawTile3DAon data_h = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRef_H];
                        BuildTerrainArtX2(data_h.side, data, tilePartType, parentGround.transform, tX2, tY2, offsetRotate, prefab_sub, (High - h) * 0.5f);
                    }
                }
            }
            else if (tilePartType == GroundChuck.eTilePartType.H_SIDE)
            {
                // int offsetX = tX2 % 2 == 0 ? -1 : 1;
                int offsetX = (offsetRotate == 0) ? 1 : -1;
                int minHigh = HightBuildTerrain(tX2, tY2, offsetX, 0);
                if (minHigh > HightBuildTerrain(tX2, tY2 + 1, offsetX, 0)
                || minHigh > HightBuildTerrain(tX2, tY2 - 1, offsetX, 0))
                {
                    minHigh--;
                }
                if (High - 1 > minHigh)
                {
                    for (int h = High - 1; h > minHigh; h--)
                    {
                        DrawTile3DAon data_h = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRef_H];
                        BuildTerrainArtX2(data_h.side, data, tilePartType, parentGround.transform, tX2, tY2, offsetRotate, prefab_sub, (High - h) * 0.5f);
                    }
                }
            }
            else if (tilePartType == GroundChuck.eTilePartType.EXT_CORNER)
            {
                // int offsetX = tX2 % 2 == 0 ? -1 : 1;
                // int offsetY = tY2 % 2 == 0 ? -1 : 1;
                int offsetX = (offsetRotate == 90 || offsetRotate == 180) ? 1 : -1;
                int offsetY = (offsetRotate == 180 || offsetRotate == 270) ? 1 : -1;
                int minHigh = Min(
                    HightBuildTerrain(tX2, tY2, offsetX, offsetY),
                    HightBuildTerrain(tX2, tY2, 0, offsetY),
                    HightBuildTerrain(tX2, tY2, offsetX, 0));
                if (High - 1 > minHigh)
                {
                    int hOffsetX = HightBuildTerrain(tX2, tY2, offsetX, 0);
                    int hOffsetY = HightBuildTerrain(tX2, tY2, 0, offsetY);
                    for (int h = High - 1; h > minHigh; h--)
                    {
                        if (h > hOffsetX && h > hOffsetY)
                        {
                            DrawTile3DAon data_h = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRef_H];
                            BuildTerrainArtX2(data_h.corner_2, data, tilePartType, parentGround.transform, tX2, tY2, offsetRotate, prefab_sub, (High - h) * 0.5f);
                        }
                        else
                        {
                            if (h < hOffsetX && h < hOffsetY)
                            {
                                // In
                            }
                            else if (h > hOffsetX && h < hOffsetY)
                            {
                                DrawTile3DAon data_h = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRef_H];
                                BuildTerrainArtX2(data_h.side, data, tilePartType, parentGround.transform, tX2, tY2, offsetX == -1 ? 180 : 0, prefab_sub, (High - h) * 0.5f);
                            }
                            else if (h > hOffsetY && h < hOffsetX)
                            {
                                DrawTile3DAon data_h = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRef_H];
                                BuildTerrainArtX2(data_h.side, data, tilePartType, parentGround.transform, tX2, tY2, offsetY == -1 ? 270 : 90, prefab_sub, (High - h) * 0.5f);
                            }
                            else if (h == hOffsetX && h == hOffsetY)
                            {
                                DrawTile3DAon data_y = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRef_Y];
                                BuildTerrainArtX2(data_y.side, data, tilePartType, parentGround.transform, tX2, tY2, offsetX == -1 ? 180 : 0, prefab_sub, (High - h) * 0.5f);
                                BuildTerrainArtX2(data_y.side, data, tilePartType, parentGround.transform, tX2, tY2, offsetY == -1 ? 270 : 90, prefab_sub, (High - h) * 0.5f);

                                DrawTile3DAon data_h = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRef_H];
                                BuildTerrainArtX2(data_h.corner_2, data, tilePartType, parentGround.transform, tX2, tY2, offsetRotate, prefab_sub, (High - h) * 0.5f);
                            }
                            else if (h == hOffsetX)
                            {
                                DrawTile3DAon data_y = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRef_Y];
                                BuildTerrainArtX2(data_y.side, data, tilePartType, parentGround.transform, tX2, tY2, offsetY == -1 ? 270 : 90, prefab_sub, (High - h) * 0.5f);

                                DrawTile3DAon data_h = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRef_H];
                                BuildTerrainArtX2(data_h.corner_2, data, tilePartType, parentGround.transform, tX2, tY2, offsetRotate, prefab_sub, (High - h) * 0.5f);
                            }
                            else if (h == hOffsetY)
                            {
                                DrawTile3DAon data_y = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRef_Y];
                                BuildTerrainArtX2(data_y.side, data, tilePartType, parentGround.transform, tX2, tY2, offsetX == -1 ? 180 : 0, prefab_sub, (High - h) * 0.5f);

                                DrawTile3DAon data_h = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRef_H];
                                BuildTerrainArtX2(data_h.corner_2, data, tilePartType, parentGround.transform, tX2, tY2, offsetRotate, prefab_sub, (High - h) * 0.5f);
                            }
                            else
                            {
                                DrawTile3DAon data_h = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRef_H];
                                var art = BuildTerrainArtX2(data_h.corner_2, data, tilePartType, parentGround.transform, tX2, tY2, offsetRotate, prefab_sub, (High - h) * 0.5f);
                                art.name = h + "_" + hOffsetX + "_" + hOffsetY;
                            }
                        }
                        // BuildTerrainArtX2( data_h.side, data, tilePartType,  parentGround.transform, tX2, tY2, offsetRotate, prefab_sub, (High - h) * 0.5f);
                    }
                }
                if (minHigh >= 0)
                {
                    DrawTile3DAon data_h = m_autoTileMap.Tileset.DrawTile3DAonSetSelected.DrawTile3DAons[idRef_H];
                    BuildTerrainArtX2(data_h.interior, data, tilePartType, parentGround.transform, tX2, tY2, offsetRotate, prefab_sub, (High - minHigh) * 0.5f);
                }
            }
            if (interior != null && highMax == High && tilePartType == GroundChuck.eTilePartType.INTERIOR)
            {
                int interiorX = tX2 - StartTileX2;
                int interiorY = tY2 - StartTileY2;
                bool t = true;
                interior[interiorX, interiorY] = t;
            }
        }
    }
    

    private void SetupTerrainY(ref DrawTile3D tile3DAon, ref DrawTile3DAon data, ref GroundChuck.eTilePartType tilePartType)
    {
        if (data == null)
        {
            return;
        }
        if (tilePartType == GroundChuck.eTilePartType.EXT_CORNER || tilePartType == GroundChuck.eTilePartType.DOUBLE_EXT_CORNER)
        {
            tile3DAon = data.corner_2;
        }
        else if (tilePartType == GroundChuck.eTilePartType.H_SIDE || tilePartType == GroundChuck.eTilePartType.V_SIDE)
        {
            tile3DAon = data.side;
        }
        else if (tilePartType == GroundChuck.eTilePartType.INT_CORNER)
        {
            tile3DAon = data.corner_3;
        }
        else
        {
            tile3DAon = data.interior;
        }
    }
    /*
	private void CaculatorTilePartTerrainX1(ref GroundChuck.eTilePartType tilePartType, ref int offsetRotate, int tX, int tY){
		int hightCurrent = GetHighX1( tX, tY);
		bool top = hightCurrent > GetHighX1( tX, tY + 1);
		bool bot = hightCurrent > GetHighX1( tX, tY - 1);
		bool left = hightCurrent > GetHighX1( tX - 1, tY);
		bool right = hightCurrent > GetHighX1( tX + 1, tY);

		int tile_count = _getTileCount( top, bot, left, right);
		int bitmask = _getTileBitmask( top, bot, left, right);
		//  1
		//8   2
		//  4
		if(tile_count == 0){
			tilePartType = GroundChuck.eTilePartType.INTERIOR;
		}else if(tile_count == 1){
			if(bitmask == 2){
				tilePartType = GroundChuck.eTilePartType.H_SIDE;
				offsetRotate = 0;
			}else if(bitmask == 1){
				tilePartType = GroundChuck.eTilePartType.V_SIDE;
				offsetRotate = 90;
			}else if(bitmask == 8){
				tilePartType = GroundChuck.eTilePartType.H_SIDE;
				offsetRotate = 180;
			}else if(bitmask == 4){
				tilePartType = GroundChuck.eTilePartType.V_SIDE;
				offsetRotate = 270;
			}
		}else if(tile_count == 2){
			//Side W or H: 
			if(bitmask == 5){//Side_H
				tilePartType = GroundChuck.eTilePartType.V_SIDE;
				offsetRotate = 90;
			}else if(bitmask == 10){//Side_W
				tilePartType = GroundChuck.eTilePartType.H_SIDE;
				offsetRotate = 0;
			}else{
				tilePartType = GroundChuck.eTilePartType.EXT_CORNER;
				if(bitmask == 3){ // Top, Right
					offsetRotate = 180;
				}else if(bitmask == 9){ // Top, Left
					offsetRotate = 270;
				}else if(bitmask == 12){ // Bot, Left
					offsetRotate = 0;
				}else if(bitmask == 6){ //Bot, Right
					offsetRotate = 90;
				}
			}
		}else if(tile_count == 3){
			if(bitmask == 7){ // Right
				tilePartType = GroundChuck.eTilePartType.V_SIDE;
				offsetRotate = 90;
				// offsetP.x = data.offset;
			}else if(bitmask == 11){ // Top
				tilePartType = GroundChuck.eTilePartType.H_SIDE;
				offsetRotate = 180;
				// offsetP.y = -data.offset;
			}else if(bitmask == 13){ // Left
				tilePartType = GroundChuck.eTilePartType.V_SIDE;
				offsetRotate = 270;
				// offsetP.x = -data.offset;
			}else if(bitmask == 14){ // Bot
				tilePartType = GroundChuck.eTilePartType.H_SIDE;
				offsetRotate = 0;
				// offsetP.y = data.offset;
			}
		}else{ // tile_count == 4
			bool TR = hightCurrent > GetHighX1( tX + 1, tY - 1);
			bool TL = hightCurrent > GetHighX1( tX - 1, tY - 1);
			bool BR = hightCurrent > GetHighX1( tX + 1, tY + 1);
			bool BL = hightCurrent > GetHighX1( tX - 1, tY + 1);
			int tile_count_offset = _getTileCount( TR, TL, BR, BL);
			if(tile_count_offset == 3){
				tilePartType = GroundChuck.eTilePartType.INT_CORNER;
				if(!TR){
					offsetRotate = 0;
				}else if(!BR){
					offsetRotate = 90;
				}else if(!BL){
					offsetRotate = 180;
				}else{ //TL
					offsetRotate = 270;
				}
			}else if(tile_count_offset == 2){
				tilePartType = GroundChuck.eTilePartType.DOUBLE_EXT_CORNER;
				if(!BR && !TL){
					offsetRotate = 90;
				}else if(!TR && !BL){
					offsetRotate = 180;
				}else if(!BR && !TL){
					offsetRotate = 270;
				}else{ //!BL && !TR
					offsetRotate = 0;
				}
			}else{//tile_count_offset == 4 || tile_count_offset == 1
				tilePartType = GroundChuck.eTilePartType.INTERIOR;
			}
		}
	}
	 */

    private void CaculatorTilePartTerrain(ref GroundChuck.eTilePartType tilePartType, ref int offsetRotate, int tX2, int tY2)
    {
        int hightCurrent = HightBuildTerrain(tX2, tY2, 0, 0);
        Vector3 offsetP = Vector3.zero;
        bool top = hightCurrent == HightBuildTerrain(tX2, tY2, 0, 1);
        bool bot = hightCurrent == HightBuildTerrain(tX2, tY2, 0, -1);
        bool left = hightCurrent == HightBuildTerrain(tX2, tY2, -1, 0);
        bool right = hightCurrent == HightBuildTerrain(tX2, tY2, 1, 0);

        int tile_count = _getTileCount(top, bot, left, right);
        int bitmask = _getTileBitmask(top, bot, left, right);
        //  1
        //8   2
        //  4
        if (tile_count == 0)
        {
            tilePartType = GroundChuck.eTilePartType.INTERIOR;
        }
        else if (tile_count == 1)
        {
            if (bitmask == 2)
            {
                tilePartType = GroundChuck.eTilePartType.H_SIDE;
                offsetRotate = 0;
            }
            else if (bitmask == 1)
            {
                tilePartType = GroundChuck.eTilePartType.V_SIDE;
                offsetRotate = 90;
            }
            else if (bitmask == 8)
            {
                tilePartType = GroundChuck.eTilePartType.H_SIDE;
                offsetRotate = 180;
            }
            else if (bitmask == 4)
            {
                tilePartType = GroundChuck.eTilePartType.V_SIDE;
                offsetRotate = 270;
            }
        }
        else if (tile_count == 2)
        {
            //Side W or H: 
            if (bitmask == 5)
            {//Side_H
                tilePartType = GroundChuck.eTilePartType.V_SIDE;
                offsetRotate = 90;
            }
            else if (bitmask == 10)
            {//Side_W
                tilePartType = GroundChuck.eTilePartType.H_SIDE;
                offsetRotate = 0;
            }
            else
            {
                tilePartType = GroundChuck.eTilePartType.EXT_CORNER;
                if (bitmask == 3)
                { // Top, Right
                    offsetRotate = 180;
                }
                else if (bitmask == 9)
                { // Top, Left
                    offsetRotate = 270;
                }
                else if (bitmask == 12)
                { // Bot, Left
                    offsetRotate = 0;
                }
                else if (bitmask == 6)
                { //Bot, Right
                    offsetRotate = 90;
                }
            }
        }
        else if (tile_count == 3)
        {
            if (bitmask == 7)
            { // Right
                tilePartType = GroundChuck.eTilePartType.V_SIDE;
                offsetRotate = 90;
                // offsetP.x = data.offset;
            }
            else if (bitmask == 11)
            { // Top
                tilePartType = GroundChuck.eTilePartType.H_SIDE;
                offsetRotate = 180;
                // offsetP.y = -data.offset;
            }
            else if (bitmask == 13)
            { // Left
                tilePartType = GroundChuck.eTilePartType.V_SIDE;
                offsetRotate = 270;
                // offsetP.x = -data.offset;
            }
            else if (bitmask == 14)
            { // Bot
                tilePartType = GroundChuck.eTilePartType.H_SIDE;
                offsetRotate = 0;
                // offsetP.y = data.offset;
            }
        }
        else
        { // tile_count == 4
            bool TR = hightCurrent == HightBuildTerrain(tX2, tY2, 1, -1);
            bool TL = hightCurrent == HightBuildTerrain(tX2, tY2, -1, -1);
            bool BR = hightCurrent == HightBuildTerrain(tX2, tY2, 1, 1);
            bool BL = hightCurrent == HightBuildTerrain(tX2, tY2, -1, 1);
            int tile_count_offset = _getTileCount(TR, TL, BR, BL);
            if (tile_count_offset == 3)
            {
                tilePartType = GroundChuck.eTilePartType.INT_CORNER;
                if (!TR)
                {
                    offsetRotate = 0;
                }
                else if (!BR)
                {
                    offsetRotate = 90;
                }
                else if (!BL)
                {
                    offsetRotate = 180;
                }
                else
                { //TL
                    offsetRotate = 270;
                }
            }
            else if (tile_count_offset == 2)
            {
                tilePartType = GroundChuck.eTilePartType.DOUBLE_EXT_CORNER;
                if (!BR && !TL)
                {
                    offsetRotate = 90;
                }
                else if (!TR && !BL)
                {
                    offsetRotate = 180;
                }
                else if (!BR && !TL)
                {
                    offsetRotate = 270;
                }
                else
                { //!BL && !TR
                    offsetRotate = 0;
                }
            }
            else
            {//tile_count_offset == 4 || tile_count_offset == 1
                tilePartType = GroundChuck.eTilePartType.INTERIOR;
            }
        }
    }

    private bool CompareHightBuildTerrainByH(int hightCurrent, int tX2, int tY2, int xd, int yd)
    {
        return hightCurrent > HightBuildTerrain(tX2, tY2, xd, yd);
    }

    private void CaculatorTilePartTerrainByH(ref GroundChuck.eTilePartType tilePartType, ref int offsetRotate, int tX2, int tY2)
    {
        // int hightCurrent = HightBuildTerrain( tX2, tY2, 0 , 0);
        int hightCurrent = High;
        Vector3 offsetP = Vector3.zero;
        bool top = CompareHightBuildTerrainByH(hightCurrent, tX2, tY2, 0, 1);
        bool bot = CompareHightBuildTerrainByH(hightCurrent, tX2, tY2, 0, -1);
        bool left = CompareHightBuildTerrainByH(hightCurrent, tX2, tY2, -1, 0);
        bool right = CompareHightBuildTerrainByH(hightCurrent, tX2, tY2, 1, 0);

        int tile_count = _getTileCount(top, bot, left, right);
        int bitmask = _getTileBitmask(top, bot, left, right);
        //  1
        //8   2
        //  4
        if (tile_count == 0)
        {
            tilePartType = GroundChuck.eTilePartType.INTERIOR;
            /*
			bool TR = CompareHightBuildTerrainByH( hightCurrent, tX2, tY2, 1, -1);
			bool TL = CompareHightBuildTerrainByH( hightCurrent, tX2, tY2, -1, -1);
			bool BR = CompareHightBuildTerrainByH( hightCurrent, tX2, tY2, 1, 1);
			bool BL = CompareHightBuildTerrainByH( hightCurrent, tX2, tY2, -1, 1);
			int tile_count_offset = _getTileCount( TR, TL, BR, BL);
			if(tile_count_offset == 1){
				tilePartType = GroundChuck.eTilePartType.INT_CORNER;
				if(TR){
					offsetRotate = 0;
				}else if(BR){
					offsetRotate = 90;
				}else if(BL){
					offsetRotate = 180;
				}else{ //TL
					offsetRotate = 270;
				}
			}
			*/
        }
        else if (tile_count == 1)
        {
            if (bitmask == 2)
            {
                tilePartType = GroundChuck.eTilePartType.H_SIDE;
                offsetRotate = 0;
            }
            else if (bitmask == 1)
            {
                tilePartType = GroundChuck.eTilePartType.V_SIDE;
                offsetRotate = 90;
            }
            else if (bitmask == 8)
            {
                tilePartType = GroundChuck.eTilePartType.H_SIDE;
                offsetRotate = 180;
            }
            else if (bitmask == 4)
            {
                tilePartType = GroundChuck.eTilePartType.V_SIDE;
                offsetRotate = 270;
            }
        }
        else if (tile_count == 2)
        {
            //Side W or H: 
            if (bitmask == 5)
            {//Side_H
                tilePartType = GroundChuck.eTilePartType.V_SIDE;
                offsetRotate = 90;
            }
            else if (bitmask == 10)
            {//Side_W
                tilePartType = GroundChuck.eTilePartType.H_SIDE;
                offsetRotate = 0;
            }
            else
            {
                tilePartType = GroundChuck.eTilePartType.EXT_CORNER;
                if (bitmask == 3)
                { // Top, Right
                    offsetRotate = 180;
                }
                else if (bitmask == 9)
                { // Top, Left
                    offsetRotate = 270;
                }
                else if (bitmask == 12)
                { // Bot, Left
                    offsetRotate = 0;
                }
                else if (bitmask == 6)
                { //Bot, Right
                    offsetRotate = 90;
                }
            }
        }
        else if (tile_count == 3)
        {
            if (bitmask == 7)
            { // Right
                tilePartType = GroundChuck.eTilePartType.V_SIDE;
                offsetRotate = 90;
                // offsetP.x = data.offset;
            }
            else if (bitmask == 11)
            { // Top
                tilePartType = GroundChuck.eTilePartType.H_SIDE;
                offsetRotate = 180;
                // offsetP.y = -data.offset;
            }
            else if (bitmask == 13)
            { // Left
                tilePartType = GroundChuck.eTilePartType.V_SIDE;
                offsetRotate = 270;
                // offsetP.x = -data.offset;
            }
            else if (bitmask == 14)
            { // Bot
                tilePartType = GroundChuck.eTilePartType.H_SIDE;
                offsetRotate = 0;
                // offsetP.y = data.offset;
            }
        }
        else
        { // tile_count == 4
            bool TR = CompareHightBuildTerrainByH(hightCurrent, tX2, tY2, 1, -1);
            bool TL = CompareHightBuildTerrainByH(hightCurrent, tX2, tY2, -1, -1);
            bool BR = CompareHightBuildTerrainByH(hightCurrent, tX2, tY2, 1, 1);
            bool BL = CompareHightBuildTerrainByH(hightCurrent, tX2, tY2, -1, 1);
            int tile_count_offset = _getTileCount(TR, TL, BR, BL);
            if (tile_count_offset == 3)
            {
                tilePartType = GroundChuck.eTilePartType.INT_CORNER;
                if (!TR)
                {
                    offsetRotate = 0;
                }
                else if (!BR)
                {
                    offsetRotate = 90;
                }
                else if (!BL)
                {
                    offsetRotate = 180;
                }
                else
                { //TL
                    offsetRotate = 270;
                }
            }
            else if (tile_count_offset == 2)
            {
                tilePartType = GroundChuck.eTilePartType.EXT_CORNER;
                if (!BR && !TL)
                {
                    offsetRotate = 90;
                }
                else if (!TR && !BL)
                {
                    offsetRotate = 180;
                }
                else if (!BR && !TL)
                {
                    offsetRotate = 270;
                }
                else
                { //!BL && !TR
                    offsetRotate = 0;
                }
            }
            else
            {//tile_count_offset == 4 || tile_count_offset == 1
                tilePartType = GroundChuck.eTilePartType.INTERIOR;
            }
        }
    }


    private bool CompareHightBuildTerrainX2(int hightCurrent, int tX2, int tY2, int xd, int yd)
    {
        return hightCurrent > HightBuildTerrain(tX2, tY2, xd, yd);
    }
    private void CaculatorTilePartTerrainX2(ref GroundChuck.eTilePartType tilePartType, ref int offsetRotate, int tX2, int tY2)
    {
        // int hightCurrent = HightBuildTerrain( tX2, tY2, 0 , 0);
        int hightCurrent = High;
        Vector3 offsetP = Vector3.zero;
        bool top = CompareHightBuildTerrainX2(hightCurrent, tX2, tY2, 0, 1);
        bool bot = CompareHightBuildTerrainX2(hightCurrent, tX2, tY2, 0, -1);
        bool left = CompareHightBuildTerrainX2(hightCurrent, tX2, tY2, -1, 0);
        bool right = CompareHightBuildTerrainX2(hightCurrent, tX2, tY2, 1, 0);

        int tile_count = _getTileCount(top, bot, left, right);
        int bitmask = _getTileBitmask(top, bot, left, right);
        //  1
        //8   2
        //  4
        if (tile_count == 0)
        {
            tilePartType = GroundChuck.eTilePartType.INTERIOR;
            bool TR = CompareHightBuildTerrainX2(hightCurrent, tX2, tY2, 1, -1);
            bool TL = CompareHightBuildTerrainX2(hightCurrent, tX2, tY2, -1, -1);
            bool BR = CompareHightBuildTerrainX2(hightCurrent, tX2, tY2, 1, 1);
            bool BL = CompareHightBuildTerrainX2(hightCurrent, tX2, tY2, -1, 1);
            int tile_count_offset = _getTileCount(TR, TL, BR, BL);
            if (tile_count_offset == 1)
            {
                tilePartType = GroundChuck.eTilePartType.INT_CORNER;
                if (TR)
                {
                    offsetRotate = 0;
                }
                else if (BR)
                {
                    offsetRotate = 90;
                }
                else if (BL)
                {
                    offsetRotate = 180;
                }
                else
                { //TL
                    offsetRotate = 270;
                }
            }
        }
        else if (tile_count == 1)
        {
            if (bitmask == 2)
            {
                tilePartType = GroundChuck.eTilePartType.H_SIDE;
                offsetRotate = 0;
            }
            else if (bitmask == 1)
            {
                tilePartType = GroundChuck.eTilePartType.V_SIDE;
                offsetRotate = 90;
            }
            else if (bitmask == 8)
            {
                tilePartType = GroundChuck.eTilePartType.H_SIDE;
                offsetRotate = 180;
            }
            else if (bitmask == 4)
            {
                tilePartType = GroundChuck.eTilePartType.V_SIDE;
                offsetRotate = 270;
            }
        }
        else if (tile_count == 2)
        {
            //Side W or H: 
            if (bitmask == 5)
            {//Side_H
                tilePartType = GroundChuck.eTilePartType.V_SIDE;
                offsetRotate = 90;
            }
            else if (bitmask == 10)
            {//Side_W
                tilePartType = GroundChuck.eTilePartType.H_SIDE;
                offsetRotate = 0;
            }
            else
            {
                tilePartType = GroundChuck.eTilePartType.EXT_CORNER;
                if (bitmask == 3)
                { // Top, Right
                    offsetRotate = 180;
                }
                else if (bitmask == 9)
                { // Top, Left
                    offsetRotate = 270;
                }
                else if (bitmask == 12)
                { // Bot, Left
                    offsetRotate = 0;
                }
                else if (bitmask == 6)
                { //Bot, Right
                    offsetRotate = 90;
                }
            }
        }
        else if (tile_count == 3)
        {
            if (bitmask == 7)
            { // Right
                tilePartType = GroundChuck.eTilePartType.V_SIDE;
                offsetRotate = 90;
                // offsetP.x = data.offset;
            }
            else if (bitmask == 11)
            { // Top
                tilePartType = GroundChuck.eTilePartType.H_SIDE;
                offsetRotate = 180;
                // offsetP.y = -data.offset;
            }
            else if (bitmask == 13)
            { // Left
                tilePartType = GroundChuck.eTilePartType.V_SIDE;
                offsetRotate = 270;
                // offsetP.x = -data.offset;
            }
            else if (bitmask == 14)
            { // Bot
                tilePartType = GroundChuck.eTilePartType.H_SIDE;
                offsetRotate = 0;
                // offsetP.y = data.offset;
            }
        }
        else
        { // tile_count == 4
            bool TR = CompareHightBuildTerrainX2(hightCurrent, tX2, tY2, 1, -1);
            bool TL = CompareHightBuildTerrainX2(hightCurrent, tX2, tY2, -1, -1);
            bool BR = CompareHightBuildTerrainX2(hightCurrent, tX2, tY2, 1, 1);
            bool BL = CompareHightBuildTerrainX2(hightCurrent, tX2, tY2, -1, 1);
            int tile_count_offset = _getTileCount(TR, TL, BR, BL);
            if (tile_count_offset == 3)
            {
                tilePartType = GroundChuck.eTilePartType.INT_CORNER;
                if (!TR)
                {
                    offsetRotate = 0;
                }
                else if (!BR)
                {
                    offsetRotate = 90;
                }
                else if (!BL)
                {
                    offsetRotate = 180;
                }
                else
                { //TL
                    offsetRotate = 270;
                }
            }
            else if (tile_count_offset == 2)
            {
                tilePartType = GroundChuck.eTilePartType.EXT_CORNER;
                if (!BR && !TL)
                {
                    offsetRotate = 90;
                }
                else if (!TR && !BL)
                {
                    offsetRotate = 180;
                }
                else if (!BR && !TL)
                {
                    offsetRotate = 270;
                }
                else
                { //!BL && !TR
                    offsetRotate = 0;
                }
            }
            else
            {//tile_count_offset == 4 || tile_count_offset == 1
                tilePartType = GroundChuck.eTilePartType.INTERIOR;
            }
        }
    }

    public int GetHighX1(int x, int y)
    {
        return m_autoTileMap.MapSelect.GetHighRef(x, y);
    }

    public int GetHighX2(int x, int y)
    {
        return m_autoTileMap.getHighX2(x, y);
    }

    public bool IsDrawWarter(int x, int y)
    {
        // if (MyAutoTileMap.GetAutoTile( x, y, 0).Id == DefineAON.IdSlot_Water
        // || MyAutoTileMap.GetAutoTile( x - 1, y, 0).Id == DefineAON.IdSlot_Water
        // || MyAutoTileMap.GetAutoTile( x + 1, y, 0).Id == DefineAON.IdSlot_Water
        // || MyAutoTileMap.GetAutoTile( x, y - 1, 0).Id == DefineAON.IdSlot_Water
        // || MyAutoTileMap.GetAutoTile( x, y + 1, 0).Id == DefineAON.IdSlot_Water
        // ){
        // 	return true;
        // }
        return m_autoTileMap.IsWaterInGame(x, y);
    }

    private int HightBuildTerrain(int tX2, int tY2, int xd, int yd)
    {
        int x2 = tX2 + xd;
        int y2 = tY2 + yd;
        int idSlot = m_autoTileMap.GetAutoTile(x2 / 2, y2 / 2, 0).Id;
        if (idSlot == -1 || idSlot == DefineAON.IdSlot_Water)
        {
            return -1;
        }
        int h = m_autoTileMap.getHighX2(x2, y2);
        return h;
    }


    // Math
    public int _getTileCount(bool top, bool bot, bool left, bool right)
    {
        int count = 0;
        count += (top ? 1 : 0);
        count += (left ? 1 : 0);
        count += (right ? 1 : 0);
        count += (bot ? 1 : 0);
        return count;
    }

    public int _getTileBitmask(bool top, bool bot, bool left, bool right)
    {
        int count = 0;
        count += (top ? 1 : 0);
        count += (left ? 8 : 0);
        count += (right ? 2 : 0);
        count += (bot ? 4 : 0);
        return count;
    }

    //Utility

    public int GetAutoTileIsGround(int gridX, int gridY, int iLayer)
    {
        var h = m_autoTileMap.MapSelect.GetHighRef(gridX, gridY);
        // return h;
        if (h >= High)
        {
            return 1;
        }
        return -1;
    }

    private int GetIDTile(int gridX, int gridY, int iLayer)
    {
        var autoTile = m_autoTileMap.GetAutoTile(gridX, gridY, MapLayerIdx);
        return autoTile.Id;
    }

    private int MapTileWidth()
    {
        return m_autoTileMap.MapTileWidth;
    }

    private int MapTileHeight()
    {
        return m_autoTileMap.MapTileHeight;
    }

    private Vector2 CellSize()
    {
        return m_autoTileMap.CellSize;
    }
    /// <summary>
    /// An auto-tile has 4 parts that change according to neighbors. These are the different types for each part.
    /// </summary>
    public enum eTilePartType
    {
        INT_CORNER,
        EXT_CORNER,
        INTERIOR,
        H_SIDE, // horizontal sides
        V_SIDE, // vertical sides
        DOUBLE_EXT_CORNER,
        // EXT_CORNER_FLOOR
    }
    // V vertical, H horizontal, D diagonal
    public static eTilePartType _getTileByNeighbours(int tileId, int tileIdV, int tileIdH, int tileIdD)
    {
        // if (tileIdV == AutoTileMap.k_outofboundsTileId) tileIdV = tileId;
        // if (tileIdH == AutoTileMap.k_outofboundsTileId) tileIdH = tileId;
        // if (tileIdD == AutoTileMap.k_outofboundsTileId) tileIdD = tileId;

        if (
            (tileIdV == tileId) &&
            (tileIdH == tileId) &&
            (tileIdD != tileId)
            )
        {
            return eTilePartType.INT_CORNER;
        }
        else if (
            (tileIdV != tileId) &&
            (tileIdH != tileId)
            )
        {
            return eTilePartType.EXT_CORNER;
        }
        else if (
            (tileIdV == tileId) &&
            (tileIdH == tileId) &&
            (tileIdD == tileId)
            )
        {
            return eTilePartType.INTERIOR;
        }
        else if (
            (tileIdV != tileId) &&
            (tileIdH == tileId)
            )
        {
            return eTilePartType.H_SIDE;
        }
        else /*if (
			(tile_typeV == tile_type) &&
			(tile_typeH != tile_type)
			)*/
        {
            return eTilePartType.V_SIDE;
        }
    }

    public static int[,] aTileAff = new int[,]
    {
        {2, 0},
        {0, 2},
        {2, 4},
        {2, 2},
        {0, 4},
    };

    public static int[,] aTileBff = new int[,]
    {
        {3, 0},
        {3, 2},
        {1, 4},
        {1, 2},
        {3, 4},
    };

    public static int[,] aTileCff = new int[,]
    {
        {2, 1},
        {0, 5},
        {2, 3},
        {2, 5},
        {0, 3},
    };

    public static int[,] aTileDff = new int[,]
    {
        {3, 1},
        {3, 5},
        {1, 3},
        {1, 5},
        {3, 3},
    };

    public static Rect GetRectTile_6_3(DrawTileAon drawTile, int tilePartX, int tilePartY, AutoTile autoTile, AutoTileMap MyAutoTileMap, int MapLayerIdx, int xf, int yf, int tx, int ty)
    {
        float size = 16;
        Rect sprTileRect = new Rect(drawTile.xBegin, drawTile.yBegin, size, size);
        float xT = drawTile.xBegin;
        float yT = drawTile.yBegin;
        //
        if (tilePartX >= 0 && tilePartX <= 3 && tilePartY >= 2 && tilePartY <= 5)
        {
            //xy: 0-3 2-5
            tilePartY = tilePartY - 2;
            //Conver 4 tile into 6
            {
                if (tilePartX == 1 || tilePartX == 2)
                {
                    if (xf == 0)
                    {
                        int right = MyAutoTileMap.GetAutoTile(tx + 1, ty, MapLayerIdx).Id;
                        if (autoTile.Id != right)
                        { //out
                            tilePartX = 4;
                        }
                        else
                        { //in
                            tilePartX = 2 + xf;
                        }
                    }
                    else
                    { //xf == 1
                        int left = MyAutoTileMap.GetAutoTile(tx - 1, ty, MapLayerIdx).Id;
                        if (autoTile.Id != left)
                        { //out
                            tilePartX = 1;
                        }
                        else
                        { //in
                            tilePartX = 2 + xf;
                        }
                    }
                }
                else if (tilePartX == 3)
                {
                    tilePartX = 5;
                }
                if (tilePartY == 1 || tilePartY == 2)
                {
                    if (yf == 0)
                    {
                        int top = MyAutoTileMap.GetAutoTile(tx, ty + 1, MapLayerIdx).Id;
                        if (autoTile.Id != top)
                        { //out
                            tilePartY = 4;
                        }
                        else
                        { //in
                            tilePartY = 2 + yf;
                        }
                    }
                    else
                    { //yf == 1
                        int bot = MyAutoTileMap.GetAutoTile(tx, ty - 1, MapLayerIdx).Id;
                        if (autoTile.Id != bot)
                        { //out
                            tilePartY = 1;
                        }
                        else
                        { //in
                            tilePartY = 2 + yf;
                        }
                    }
                }
                else if (tilePartY == 3)
                {
                    tilePartY = 5;
                }
            }
            float unit = 1.0f / 6.0f;
            float xx = unit * (tilePartX);
            float yy = unit * (tilePartY);
            float sizeA = 96;
            unit = unit * sizeA;
            xx = xx * sizeA;
            yy = yy * sizeA;
            sprTileRect = new Rect(xT + xx, yT - unit - yy, unit, unit);
        }
        else if (tilePartX >= 2 && tilePartX <= 3 && tilePartY >= 0 && tilePartY <= 1)
        {
            //Out
            tilePartX = tilePartX - 2;
            sprTileRect = new Rect(xT + 96 + size * tilePartX, yT - size - 64 - size * tilePartY, size, size);
        }
        else
        {
            sprTileRect.x = sprTileRect.x + sprTileRect.width * tilePartX;
            sprTileRect.y = sprTileRect.y - sprTileRect.height * (tilePartY + 1);
        }
        return sprTileRect;
    }


    public static int GetAutoTile(AutoTileMap autoTileMap, int layerIdx, int x, int y, int originId, int hight)
    {
        // if(originId == 3){ // Ground
        // 	int result = autoTileMap.GetAutoTile( x, y, layerIdx ).Id;
        // 	if( result == 0){
        // 		return 0;
        // 	}
        // 	return originId;
        // }
        // int current = autoTileMap.MapSelect.GetHighRef(x, y);
        return autoTileMap.GetAutoTile(x, y, layerIdx).Id;
    }

    public static void CaculatorTilePartType(ref float u0, ref float u1, ref float v0, ref float v1, int xf, int yf, int subTileXBase, int subTileYBase, int tx, int ty, SlotAon slot, AutoTile autoTile, AutoTileMap myAutoTileMap, int mapLayerIdx, int high = 1)
    {
        eTilePartType tilePartType;
        int tilePartX = 0;
        int tilePartY = 0;
        int tile_2_x = subTileXBase + xf;
        int tile_2_y = subTileYBase + yf;
        int currentId = GetAutoTile(myAutoTileMap, mapLayerIdx, tx, ty, autoTile.Id, high);
        if (tile_2_x % 2 == 0 && tile_2_y % 2 == 0) //A
        {
            tilePartType = _getTileByNeighbours(currentId,
                                GetAutoTile(myAutoTileMap, mapLayerIdx, tx, ty - 1, autoTile.Id, high), //V 
                                GetAutoTile(myAutoTileMap, mapLayerIdx, tx - 1, ty, autoTile.Id, high), //H 
                                GetAutoTile(myAutoTileMap, mapLayerIdx, tx - 1, ty - 1, autoTile.Id, high) //D
                                );
            tilePartX = aTileAff[(int)tilePartType, 0];
            tilePartY = aTileAff[(int)tilePartType, 1];
        }
        else if (tile_2_x % 2 != 0 && tile_2_y % 2 == 0) //B
        {
            tilePartType = _getTileByNeighbours(currentId,
                                GetAutoTile(myAutoTileMap, mapLayerIdx, tx, ty - 1, autoTile.Id, high), //V 
                                GetAutoTile(myAutoTileMap, mapLayerIdx, tx + 1, ty, autoTile.Id, high), //H 
                                GetAutoTile(myAutoTileMap, mapLayerIdx, tx + 1, ty - 1, autoTile.Id, high)  //D
                                );
            tilePartX = aTileBff[(int)tilePartType, 0];
            tilePartY = aTileBff[(int)tilePartType, 1];
        }
        else if (tile_2_x % 2 == 0 && tile_2_y % 2 != 0) //C
        {
            tilePartType = _getTileByNeighbours(currentId,
                                GetAutoTile(myAutoTileMap, mapLayerIdx, tx, ty + 1, autoTile.Id, high), //V 
                                GetAutoTile(myAutoTileMap, mapLayerIdx, tx - 1, ty, autoTile.Id, high), //H 
                                GetAutoTile(myAutoTileMap, mapLayerIdx, tx - 1, ty + 1, autoTile.Id, high)  //D
                                );
            tilePartX = aTileCff[(int)tilePartType, 0];
            tilePartY = aTileCff[(int)tilePartType, 1];
        }
        else //if (tile_x % 2 != 0 && tile_y % 2 != 0) //D
        {
            tilePartType = _getTileByNeighbours(currentId,
                                GetAutoTile(myAutoTileMap, mapLayerIdx, tx, ty + 1, autoTile.Id, high), //V 
                                GetAutoTile(myAutoTileMap, mapLayerIdx, tx + 1, ty, autoTile.Id, high), //H 
                                GetAutoTile(myAutoTileMap, mapLayerIdx, tx + 1, ty + 1, autoTile.Id, high)  //D
                                );
            tilePartX = aTileDff[(int)tilePartType, 0];
            tilePartY = aTileDff[(int)tilePartType, 1];
        }
        {
            DrawTileAon drawTile = null;
            if (slot.idRef >= 0 && slot.idRef < myAutoTileMap.Tileset.DrawTileAonSetSelected.DrawTileAons.Count)
            {
                drawTile = myAutoTileMap.Tileset.DrawTileAonSetSelected.DrawTileAons[slot.idRef];
            }
            if (drawTile == null)
            {
                Texture2D tex = myAutoTileMap.Tileset.DrawTileAonSetSelected.TextureThumb;
                float size = 16;
                Rect sprTileRect = new Rect(tex.width - size, tex.height - size, size, size);
                u0 = sprTileRect.x / tex.width;
                u1 = (sprTileRect.x + sprTileRect.width) / tex.width;
                v0 = (sprTileRect.y + sprTileRect.height) / tex.height;
                v1 = sprTileRect.y / tex.height;
            }
            else if (drawTile.type == DrawTileAon.eDrawTileAonType.Tile_1_1)
            {
                Texture2D tex = myAutoTileMap.Tileset.DrawTileAonSetSelected.TextureThumb;
                float size = drawTile.size / 2;
                Rect sprTileRect = new Rect(drawTile.xBegin, drawTile.yBegin - size, size, size);
                sprTileRect.x = sprTileRect.x + sprTileRect.width * xf;
                sprTileRect.y = sprTileRect.y - sprTileRect.height * yf;
                u0 = sprTileRect.x / tex.width;
                u1 = (sprTileRect.x + sprTileRect.width) / tex.width;
                v0 = (sprTileRect.y + sprTileRect.height) / tex.height;
                v1 = sprTileRect.y / tex.height;
            }
            else if (drawTile.type == DrawTileAon.eDrawTileAonType.Tile_2_3)
            {
                Texture2D tex = myAutoTileMap.Tileset.DrawTileAonSetSelected.TextureThumb;
                float size = 16;
                Rect sprTileRect = new Rect(drawTile.xBegin, drawTile.yBegin, size, size);
                sprTileRect.x = sprTileRect.x + sprTileRect.width * tilePartX;
                sprTileRect.y = sprTileRect.y - sprTileRect.height * (tilePartY + 1);
                u0 = sprTileRect.x / tex.width;
                u1 = (sprTileRect.x + sprTileRect.width) / tex.width;
                v0 = (sprTileRect.y + sprTileRect.height) / tex.height;
                v1 = sprTileRect.y / tex.height;
            }
            else if (drawTile.type == DrawTileAon.eDrawTileAonType.Tile_6_3)
            {
                Texture2D tex = myAutoTileMap.Tileset.DrawTileAonSetSelected.TextureThumb;
                Rect sprTileRect = GetRectTile_6_3(drawTile, tilePartX, tilePartY, autoTile, myAutoTileMap, mapLayerIdx, xf, yf, tx, ty);
                u0 = sprTileRect.x / tex.width;
                u1 = (sprTileRect.x + sprTileRect.width) / tex.width;
                v0 = (sprTileRect.y + sprTileRect.height) / tex.height;
                v1 = sprTileRect.y / tex.height;
            }
        }
    }
    public int Min(int a, int b, int c)
    {
        if (a <= b && a <= c)
            return a;
        if (b <= c)
            return b;
        return c;
    }
}
