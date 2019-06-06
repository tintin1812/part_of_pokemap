using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AON.RpgMapEditor
{
    /// <summary>
    /// This class manages the tile brush used to paint tiles in the editor or in game:
    /// - Drawing tiles
    /// - Copying tiles
    /// - Redo/Undo drawing actions
    /// </summary>
	public class AutoTileBrush : MonoBehaviour
    {
        /// <summary>
        /// The AutoTileMap owner of this brush
        /// </summary>
		public AutoTileMap MyAutoTileMap;

        /// <summary>
        /// Tile action with copied tiles to be pasted over the map
        /// </summary>
		public TileAction BrushAction;

        /// <summary>
        /// Position of the brush over the auto tile map in tile coordinates
        /// </summary>
		public Vector2 BrushTilePos;
        public bool HasChangedTilePos;

        /// <summary>
        /// Selected layer where the brush will draw the tiles and will take as reference for special actions when holding action key
        /// </summary>
        // public int SelectedLayer = 0;

        /// <summary>
        /// When this is true, the brush will have some special functionalities to make the map edition easier
        /// </summary>
        // public bool SmartBrushEnabled = true;

        #region Historic Ctrl-Z Ctrl-Y
        [System.Serializable]
        public class TileAction
        {
            public class TileData
            {
                public int Tile_x;
                public int Tile_y;
                public int Tile_id;
                public int Tile_layer;
                public int Tile_high = 0;
                // public int Tile_type_prev;
            }

            public List<TileData> aTileData = new List<TileData>();

            public void Push(AutoTileMap _autoTileMap, int tile_x, int tile_y, int tileId, int tile_layer, int high_brush = -2)
            {
                // int high_brush = -2;
                // if(tile_layer == 0){//Ground
                // 	high_brush = _autoTileMap.MapSelect.GetHighRef(tile_x, tile_y);
                // }
                TileData tileData = new TileData()
                {
                    Tile_x = tile_x,
                    Tile_y = tile_y,
                    Tile_id = tileId,
                    Tile_layer = tile_layer,
                    Tile_high = high_brush
                };
                aTileData.Add(tileData);
            }

            //Return only 1 TileData has change
            public TileData DoAction(AutoTileMap _autoTileMap)
            {
                TileData onlyChange = null;
                //Check is ok with Size Large
                {
                    var brush = aTileData[0];
                    int tile_id = brush.Tile_id;
                    if (_autoTileMap.Tileset.IsExitSlot(tile_id))
                    {
                        var slot = _autoTileMap.Tileset.GetSlot(tile_id);
                        if (slot.Size.x > 1 || slot.Size.y > 1)
                        {
                            if (brush.Tile_x < 0)
                            {
                                Debug.Log("Out Left Map");
                                return onlyChange;
                            }
                            if (brush.Tile_y < 0)
                            {
                                Debug.Log("Out Top Map");
                                return onlyChange;
                            }
                            if (brush.Tile_x + slot.Size.x > (_autoTileMap.MapTileWidth))
                            {
                                Debug.Log("Out Right Map");
                                return onlyChange;
                            }
                            if (brush.Tile_y + slot.Size.x > (_autoTileMap.MapTileWidth))
                            {
                                Debug.Log("Out Bot Map");
                                return onlyChange;
                            }
                            //Check all tile will be brush don't have other Size Large
                            for (int x = brush.Tile_x; x < brush.Tile_x + slot.Size.x; x++)
                            {
                                for (int y = brush.Tile_y; y < brush.Tile_y + slot.Size.y; y++)
                                {
                                    int idCheck = _autoTileMap.GetAutoTile(x, y, brush.Tile_layer).Id;
                                    if (_autoTileMap.Tileset.IsExitSlot(idCheck))
                                    {
                                        var slotCheck = _autoTileMap.Tileset.GetSlot(idCheck);
                                        if (slotCheck.Size.x > 1 || slotCheck.Size.y > 1)
                                        {
                                            Debug.Log("Have other Size Large");
                                            return onlyChange;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //
                int tileMinX = _autoTileMap.MapTileWidth - 1;
                int tileMinY = _autoTileMap.MapTileHeight - 1;
                int tileMaxX = 0;
                int tileMaxY = 0;
                bool HasChangedTile = false;
                for (int i = 0; i < aTileData.Count; ++i)
                {
                    TileData tileData = aTileData[i];
                    // save prev tile type for undo action
                    // tileData.Tile_type_prev = _autoTileMap.GetAutoTile( tileData.Tile_x, tileData.Tile_y, tileData.Tile_layer ).Id;
                    // if(!_autoTileMap.Tileset.IsExitSlot(tileData.Tile_id)){
                    // 	continue;
                    // }
                    int idCheck = _autoTileMap.GetAutoTile(tileData.Tile_x, tileData.Tile_y, tileData.Tile_layer).Id;
                    if (_autoTileMap.Tileset.IsExitSlot(idCheck))
                    {
                        var slotCheck = _autoTileMap.Tileset.GetSlot(idCheck);
                        if (!slotCheck.IsCanCopyWhenDraw)
                        {
                            continue;
                        }
                    }
                    if (tileData.Tile_id >= -1)
                    {
                        bool isUpdate = _autoTileMap.SetAutoTile(tileData.Tile_x, tileData.Tile_y, tileData.Tile_id, tileData.Tile_layer, true);
                        if (isUpdate)
                        {
                            HasChangedTile = true;
                        }
                    }
                    if (tileData.Tile_high >= 0)
                    {
                        bool isUpdate = _autoTileMap.MapSelect.SetHighRef(tileData.Tile_x, tileData.Tile_y, tileData.Tile_high);
                        if (isUpdate)
                        {
                            HasChangedTile = true;
                            int tile_layer_high = (int)eSlotAonTypeLayer.Trigger;
                            _autoTileMap.RefreshTile(tileData.Tile_x, tileData.Tile_y, tile_layer_high);
                        }
                    }
                    if (aTileData.Count == 1)
                    {
                        onlyChange = tileData;
                    }

                    // if(slot.Size.x == 1 && slot.Size.y == 1){

                    // }else{
                    // 	// _autoTileMap.SetAutoTile( tileData.Tile_x, tileData.Tile_y, tileData.Tile_id, tileData.Tile_layer );
                    // 	for(int x = tileData.Tile_x; x < tileData.Tile_x + slot.Size.x; x++){
                    // 		for(int y = tileData.Tile_y; y < tileData.Tile_y + slot.Size.y; y++){
                    // 			// _autoTileMap.SetAutoTile( x, y, -2, tileData.Tile_layer );
                    // 			_autoTileMap.SetAutoTile( x, y, tileData.Tile_id, tileData.Tile_layer );
                    // 		}
                    // 	}
                    // }
                    if (HasChangedTile)
                    {
                        tileMinX = Mathf.Min(tileMinX, tileData.Tile_x);
                        tileMinY = Mathf.Min(tileMinY, tileData.Tile_y);
                        tileMaxX = Mathf.Max(tileMaxX, tileData.Tile_x);
                        tileMaxY = Mathf.Max(tileMaxY, tileData.Tile_y);
                    }
                }
                if (HasChangedTile)
                {
                    // if (_autoTileMap.BrushGizmo.IsRefreshMinimapEnabled)
                    {
                        _autoTileMap.RefreshMinimapTexture(tileMinX, tileMinY, (tileMaxX - tileMinX) + 1, (tileMaxY - tileMinY) + 1);
                    }
                    _autoTileMap.UpdateChunks();
                }
                return onlyChange;
            }

            public void CopyRelative(AutoTileMap _autoTileMap, TileAction _action, int tile_x, int tile_y)
            {
                foreach (TileData tileData in _action.aTileData)
                {
                    Push(_autoTileMap, tileData.Tile_x + tile_x, tileData.Tile_y + tile_y, tileData.Tile_id, tileData.Tile_layer, tileData.Tile_high);
                }
            }

            /// <summary>
            /// Tile layer will be moved a layer forward ( top direction ) but only when layer has ground type and there is a ground layer over it.
            /// This is not checking max layer count, so be careful with layer value
            /// </summary>
            /// <param name="layer"></param> 
            public void BecomeOverlay(int layer)
            {
                for (int idx = 0; idx < aTileData.Count; ++idx)
                {
                    TileData tileData = aTileData[idx];
                    if (tileData.Tile_layer == layer)
                    {
                        tileData.Tile_layer = layer + 1;
                    }
                    if (tileData.Tile_layer == (layer + 1) && tileData.Tile_id == -1)
                    {
                        aTileData.RemoveAt(idx);
                        --idx;
                    }
                }
            }

        }

        // private int m_actionIdx = -1;
        // private List<TileAction>  m_actionsHistoric = new List<TileAction>();

        //Return only 1 TileData has change
        public TileAction.TileData PerformAction(TileAction _action)
        {
            return _action.DoAction(MyAutoTileMap);
        }
        #endregion

        /// <summary>
        /// Update Brush by calling this method with mouse position
        /// </summary>
        /// <param name="mousePos"></param>
        public void UpdateBrushGizmo(Vector3 mousePos)
        {
            Vector3 vTemp = mousePos;
            vTemp.x -= mousePos.x % MyAutoTileMap.CellSize.x;
            vTemp.y -= mousePos.y % MyAutoTileMap.CellSize.y;
            vTemp.z += 1f;
            transform.position = vTemp;

            int tile_x = (int)(0.5 + transform.position.x / MyAutoTileMap.CellSize.x);
            int tile_y = (int)(0.5 + -transform.position.y / MyAutoTileMap.CellSize.y);

            Vector2 vPrevTilePos = BrushTilePos;
            BrushTilePos = new Vector2(tile_x, tile_y);
            HasChangedTilePos = (vPrevTilePos != BrushTilePos);
        }

        /// <summary>
        /// Clear Brush tile selection
        /// </summary>
		public void Clear()
        {
            // RefreshBrushGizmo( -1, -1, -1, -1, -1, -1, false );
            BrushAction = null;
            RefreshSpriteRenderers();
        }

        /// <summary>
        /// Copy a section of the map and use it as drawing template
        /// </summary>
        /// <param name="tile_start_x"></param>
        /// <param name="tile_start_y"></param>
        /// <param name="tile_end_x"></param>
        /// <param name="tile_end_y"></param>
        /// <param name="_dragEndTileX"></param>
        /// <param name="_dragEndTileY"></param>
        /// <param name="isCtrlKeyHold"></param>
		public void RefreshBrushGizmo(int tile_start_x, int tile_start_y, int tile_end_x, int tile_end_y, int _dragEndTileX, int _dragEndTileY, bool isCtrlKeyHold)
        {
            Vector2 pivot = new Vector2(0f, 1f);
            SpriteRenderer[] aSprites = GetComponentsInChildren<SpriteRenderer>();

            int sprIdx = 0;
            for (int tile_x = tile_start_x; tile_x <= tile_end_x; ++tile_x)
            {
                for (int tile_y = tile_start_y; tile_y <= tile_end_y; ++tile_y)
                {
                    for (int tile_layer = 0; tile_layer < MyAutoTileMap.GetLayerCount(); ++tile_layer)
                    {
                        // if(
                        //     (isCtrlKeyHold && tile_layer == SelectedLayer) || //copy all layers over the SelectedLayer
                        //     !MyAutoTileMap.MapLayers[tile_layer].Visible // skip invisible layers
                        // )
                        // {
                        // 	continue;
                        // }

                        AutoTile autoTile = MyAutoTileMap.GetAutoTile(tile_x, tile_y, tile_layer);
                        if (autoTile != null && autoTile.Id >= 0)
                        {
                            // for( int partIdx = 0; partIdx < autoTile.TilePartsLength; ++partIdx, ++sprIdx )
                            // {
                            int idSlot = autoTile.Id;
                            if (!MyAutoTileMap.Tileset.IsExitSlot(idSlot))
                                continue;
                            var slot = MyAutoTileMap.Tileset.GetSlot(idSlot);
                            if (!slot.IsCanCopyWhenDraw)
                                continue;

                            SpriteRenderer spriteRender = GetSpriteRenderer(aSprites, ref sprIdx);

                            SetSpriteRendererSlot(spriteRender, slot, pivot);
                            spriteRender.sortingOrder = 50; //TODO: +50 temporal? see for a const number later

                            // get last tile as relative position
                            int tilePart_x = (tile_x - _dragEndTileX) * 2;
                            int tilePart_y = (tile_y - _dragEndTileY) * 2;

                            float xFactor = MyAutoTileMap.CellSize.x / 2f;
                            float yFactor = MyAutoTileMap.CellSize.y / 2f;
                            spriteRender.transform.localPosition = new Vector3(tilePart_x * xFactor, -tilePart_y * yFactor, spriteRender.transform.position.z);
                            spriteRender.transform.localScale = new Vector3(MyAutoTileMap.CellSize.x * AutoTileset.PixelToUnits / MyAutoTileMap.Tileset.TileWidth, MyAutoTileMap.CellSize.y * AutoTileset.PixelToUnits / MyAutoTileMap.Tileset.TileHeight, 1f);
                            // }
                        }
                    }
                }
            }
            // clean unused sprite objects
            while (sprIdx < aSprites.Length)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(aSprites[sprIdx].transform.gameObject);
                }
                else
                {
                    Destroy(aSprites[sprIdx].transform.gameObject);
                }
                ++sprIdx;
            }
        }

        private SpriteRenderer GetSpriteRenderer(SpriteRenderer[] aSprites, ref int sprIdx)
        {
            if (sprIdx < aSprites.Length)
            {
                var s = aSprites[sprIdx];
                s.gameObject.name = "BrushGizmoPart" + sprIdx;
                sprIdx++;
                return s;
            }
            GameObject spriteObj = new GameObject();
            spriteObj.transform.parent = transform;
            var spriteRender = spriteObj.AddComponent<SpriteRenderer>();
            spriteRender.transform.gameObject.name = "BrushGizmoPart" + sprIdx;
            spriteRender.sortingOrder = 50;
            sprIdx++;
            return spriteRender;

        }

        private void SetSpriteRendererSlot(SpriteRenderer spriteRender, SlotAon slot, Vector2 pivot)
        {
            if (slot != null)
            {
                // slot.AtlasRecThumb;
                spriteRender.sprite = Sprite.Create(MyAutoTileMap.Tileset.TextureSlot, slot.AtlasRecThumb, pivot, AutoTileset.PixelToUnits);
                // spriteRender.color = new Color32( 255, 255, 255, 160);
                spriteRender.color = new Color32(255, 255, 255, 192);
            }
        }
        private void SetSpriteRendererHigh(SpriteRenderer spriteRender, int high, Vector2 pivot)
        {
            Texture2D tex = MyAutoTileMap.Tileset.TextureSlot;
            Rect sprTileRect = new Rect(high * 32, tex.height - 32, 32, 32);
            spriteRender.sprite = Sprite.Create(MyAutoTileMap.Tileset.TextureSlot, sprTileRect, pivot, AutoTileset.PixelToUnits);
            spriteRender.color = new Color32(255, 255, 255, 255);
        }

        public void RefreshSpriteRenderers()
        {
            int sprIdx = 0;
            SpriteRenderer[] aSprites = GetComponentsInChildren<SpriteRenderer>();
            if (BrushAction != null)
            {
                var localScale = new Vector3(MyAutoTileMap.CellSize.x * AutoTileset.PixelToUnits / MyAutoTileMap.Tileset.TileWidth, MyAutoTileMap.CellSize.y * AutoTileset.PixelToUnits / MyAutoTileMap.Tileset.TileHeight, 1f);
                for (int i = 0; i < BrushAction.aTileData.Count; i++)
                {
                    var brushData = BrushAction.aTileData[i];
                    if (brushData == null)
                    {
                        continue;
                    }
                    SpriteRenderer spriteRender = null;
                    Vector2 pivot = new Vector2(0f, 1f);
                    if (brushData.Tile_high >= 0)
                    {
                        spriteRender = GetSpriteRenderer(aSprites, ref sprIdx);
                        SetSpriteRendererHigh(spriteRender, brushData.Tile_high, pivot);
                    }
                    else if (brushData.Tile_id >= -1)
                    {
                        if (MyAutoTileMap.Tileset.IsExitSlot(brushData.Tile_id))
                        {
                            var slot = MyAutoTileMap.Tileset.GetSlot(brushData.Tile_id);
                            spriteRender = GetSpriteRenderer(aSprites, ref sprIdx);
                            SetSpriteRendererSlot(spriteRender, slot, pivot);
                        }
                        else
                        {
                            if (brushData.Tile_layer == (int)eSlotAonTypeLayer.Ground)
                            {
                                spriteRender = GetSpriteRenderer(aSprites, ref sprIdx);
                                SetSpriteRendererSlot(spriteRender, null, pivot);
                            }
                        }
                    }
                    if (spriteRender != null)
                    {
                        int tilePart_x = (brushData.Tile_x) * 2;
                        int tilePart_y = (brushData.Tile_y) * 2;
                        float xFactor = MyAutoTileMap.CellSize.x / 2f;
                        float yFactor = MyAutoTileMap.CellSize.y / 2f;
                        spriteRender.transform.localPosition = new Vector3(tilePart_x * xFactor, -tilePart_y * yFactor, 0);
                        spriteRender.transform.localScale = localScale;
                    }

                }
            }
            // clean unused sprite objects
            while (sprIdx < aSprites.Length)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(aSprites[sprIdx].transform.gameObject);
                }
                else
                {
                    Destroy(aSprites[sprIdx].transform.gameObject);
                }
                ++sprIdx;
            }
        }

        public void RefreshBrushGizmoFromTileset(int tileset, int high = -2)
        {
            BrushAction = new TileAction();
            if (tileset >= -1)
            {
                if (MyAutoTileMap.Tileset.IsExitSlot(tileset))
                {
                    SlotAon slot = MyAutoTileMap.Tileset.GetSlot(tileset);
                    int selTileW = (int)slot.Size.x;
                    int selTileH = (int)slot.Size.y;
                    for (int j = 0; j < selTileH; ++j)
                    {
                        for (int i = 0; i < selTileW; ++i)
                        {
                            BrushAction.Push(MyAutoTileMap, i, j, tileset, slot.LayerDraw, -2);
                        }
                    }
                }
                else
                {
                    BrushAction.Push(MyAutoTileMap, 0, 0, tileset, 0, -2);
                }
            }
            else
            {
                BrushAction.Push(MyAutoTileMap, 0, 0, -2, 0, high);
            }
            RefreshSpriteRenderers();
        }

    }
}
