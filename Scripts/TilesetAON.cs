using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AON.RpgMapEditor;
using UnityEngine;
using PygmyMonkey.FileBrowser;

public class TilesetAON : AONGUIBehaviour {
	static bool USING_LIST_TILESET = true;
	public static int AutoTilesPerRow = 8;
	const int k_visualTileWidth = 32; // doesn't matter the tileset tile size, this size will be used to paint it in the inspector
	const int k_visualTileHeight = 32;

	private AutoTileMap _autoTileMap;
	private MiniMapAON _miniMapAON;
	private Camera2DController _camera2D;
	// private FollowObjectBehaviour m_camera2DFollowBehaviour;

	private Rect _rEditorRect;
	// private Rect m_rTilesetRect;
	[SerializeField]
	private bool _isCHold = false;

	// private const float k_timeBefor eKeyRepeat = 1f;
	// private const float k_timeBetweenKeyRepeat = 0.01f;
	// private float m_keyPressTimer = 0f;
	private bool _showCollisions = false;
	private bool _isInitialized = false;

	private bool _isShowEditInfo = false;
	private bool IsShowEditInfo{
		get{
			return _isShowEditInfo;
		}
		set{
			_isShowEditInfo = value;
			RefreshEnableMinimap();
		}
	}

	private AutoTile _tileShowMoreInfo = null;
	public AutoTile TileShowMoreInfo{
		set{
			if(_tileShowMoreInfo == value){
				return;
			}
			if(_tileShowMoreInfo != null){
				//hide art
				if(!TileChunk.IS_ALWAY_SHOW_OVERLAY){
					_autoTileMap.TileChunkPoolNode.GetTileChunk( _tileShowMoreInfo.TileX, _tileShowMoreInfo.TileY, _tileShowMoreInfo.Layer).ClearTileOverlay( _tileShowMoreInfo.TileX, _tileShowMoreInfo.TileY);
				}
			}
			_tileShowMoreInfo = value;
			TriggerShowMoreInfo = null;
			IsShowEditInfo = false;
			if(_tileShowMoreInfo != null){
				//show art
				_autoTileMap.TileChunkPoolNode.GetTileChunk( _tileShowMoreInfo.TileX, _tileShowMoreInfo.TileY, _tileShowMoreInfo.Layer).RefreshTileOverlay( _tileShowMoreInfo.TileX, _tileShowMoreInfo.TileY, true);	
			}
			_autoTileMap.BrushGizmo.Clear();
		}
		get{
			return _tileShowMoreInfo;
		}
	}

	private Trigger _triggerShowMore = null;
	public Trigger TriggerShowMoreInfo{
		get{
			return _triggerShowMore;
		}
		set{
			_triggerShowMore = value;
			RefreshEnableMinimap();
		}
	}

	private void RefreshEnableMinimap(){
		if(_isShowDialogWorld
		|| _isShowGuiMap
		|| TriggerShowMoreInfo != null
		|| IsShowEditInfo){
			_miniMapAON.enabled = false;
			return;
		}
		_miniMapAON.enabled = true;
	}

	private bool IsModeEditInterior = false;
	public delegate void OnPickInteriorDelegate( TilesetAON t, Vector3 p, Vector3 cam);
	public OnPickInteriorDelegate OnPickInterior = null;

	private int m_selectedTileIdx = 0;

	private Texture2D t_dark = null;
	private Texture2D t_gray_2 = null;
	private Texture2D t_gray_3 = null;
	private Texture2D t_gray_35 = null;
	private Texture2D t_gray_4 = null;
	private Texture2D t_gray_7 = null;

	private GUIStyle listStyle = null;
	public GUIStyle ListStyle {
		get{
			return listStyle;
		}
	}

	private GUIStyle listStyleGrid = null;
	public GUIStyle ListStyleGrid {
		get{
			return listStyleGrid;
		}
	}

	private GUIStyle listStyleBlack = null;
	public GUIStyle ListStyleBlack {
		get{
			return listStyleBlack;
		}
	}

	private GUIStyle listStyleBlack2 = null;
	public GUIStyle ListStyleBlack2 {
		get{
			return listStyleBlack2;
		}
	}
	
	
	private GUIContent[] listContentTitleSet = null;
	private int selecteTileSetGui = 0;
	private List<int> refTileSetGui;
	
	public Texture2D t_hightLight;
	
	eSlotAonTypeBrush m_subTilesetIdx = eSlotAonTypeBrush.Terrain;
	string[] str_hight = {"0", "1", "2", "3", "4", "5", "6", "7", "8"};
	int idx_high = 0;
	/*
	GUIContent[] comboBoxTrigger2List;
	private ComboBox comboBoxTrigger2Control;

	public ComboBox ComboBoxTrigger2{
		get{
			return comboBoxTrigger2Control;
		}
	}

	public void ComboBoxTrigger2Init( int idxRef, int idMap, string slotName){
		if(comboBoxTrigger2Control == null){
			int countTrigger = m_autoTileMap.MapsData.Maps[idMap].TriggerCountAt(slotName);
			comboBoxTrigger2List = new GUIContent[countTrigger];
			for (int i = 0; i < countTrigger; ++i)
			{
				AutoTileMapSerializeData.Trigger trigger = m_autoTileMap.MapsData.Maps[idMap].GetTriggerByIdxRef(i, slotName);
				comboBoxTrigger2List[i] = new GUIContent( string.Format("{0} ({1}) {2}", slotName, i.ToString(), trigger.Name()));
			}
			comboBoxTrigger2Control = new ComboBox(new Rect(0, 0, 150, 20), null, comboBoxTrigger2List, "button", "box", listStyle, new GUIContent("Sellect a " + slotName + "..."));
			comboBoxTrigger2Control.SelectedItemIndex = idxRef;
		}
	}

	public void ComboBoxTrigger2Reset(){
		comboBoxTrigger2Control = null;
		comboBoxTrigger2List = null;
	}
	*/

	//For UI
	// GUIContent[] comboBoxMapList;
	// private ComboBox comboBoxControl;

	enum eEditorWindow
	{
		NONE,
		TOOLS,
		MAPVIEW
	}
	private eEditorWindow _focusWindow;

	private bool _isShowDialogWorld = false;
	private bool _isShowGuiMap = false;
	public bool _isShowDialogClearMap = false;

	private bool _isShowDialogSave = false;
	private bool _isShowDialogLoad = false;
	
	private string ShowSaveDialog = "";
	private string ShowLoadDialog = "";

	[SerializeField]
	private int _prevMouseTileX = -1;
	[SerializeField]
	private int _prevMouseTileY = -1;
	[SerializeField]
	private int _startDragTileX = -1;
	[SerializeField]
	private int _startDragTileY = -1;
	[SerializeField]
	private int _dragTileX = -1;
	[SerializeField]
	private int _dragTileY = -1;
	private bool _drawSelectionRect;
	private Vector3 _mousePrevPos;
	private Vector2 _prevScreenSize;
	// private float _countTimeSyncData = 1;

	public void Init() 
	{
		_autoTileMap = GetComponent<AutoTileMap>();
		_miniMapAON = GetComponent<MiniMapAON>();

		if( _autoTileMap != null && _autoTileMap.IsInitialized )
		{
			_isInitialized = true;

			if( _autoTileMap.ViewCamera == null )
			{
				Debug.LogWarning( "AutoTileMap has no ViewCamera set. Camera.main will be set as ViewCamera" );
				_autoTileMap.ViewCamera = Camera.main;
			}
			_camera2D = _autoTileMap.ViewCamera.GetComponent<Camera2DController>();

			if( _camera2D == null )
			{
				_camera2D = _autoTileMap.ViewCamera.gameObject.AddComponent<Camera2DController>();
			}
		}
	}

	public override void Update() 
	{
		
		base.Update();

		if( !_isInitialized )
		{
			Init();
			return;
		}
		
		FPSDisplay.Instance.Update();

		if(IsModePlay == false){
			// if( _countTimeSyncData > 0){
			// 	_countTimeSyncData -=  Time.deltaTime;
			// }
			// if(_countTimeSyncData <= 0){
			// 	_countTimeSyncData = 1;
			// 	ScriptGui.Instance.SaveCurrentYamlToStr( false);
			// }
		}else{
			TriggerGame.Instance.OnUpdate();
		}
		// Draw not work when show Tile Info
		if(IsModePlay){
			return;
		}
		// if(m_tileShowMoreInfo != null || IsModePlay){
		// 	return;
		// }

		// if( Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.R)) //TODO: only delete the tiles in ground layer, fix this
		// {
			// select delete tile
			// ClearSeletedTile();
		// }

		#region Undo / Redo
		/*
		if( m_isCtrlKeyHold )
		{
			if( Input.GetKeyDown(KeyCode.Z ) )
			{
				m_autoTile.BrushGizmo.UndoAction();
			}
			else if( Input.GetKeyDown(KeyCode.Y ) )
			{
				m_autoTileMap.BrushGizmo.RedoAction();
			}

			//+++ Key Repetition Implementation
			if( Input.GetKey(KeyCode.Z ) )
			{
				m_keyPressTimer += Time.deltaTime;
				if( m_keyPressTimer >= k_timeBeforeKeyRepeat )
				{
					m_keyPressTimer -= k_timeBetweenKeyRepeat;
					m_autoTileMap.BrushGizmo.UndoAction();
				}
			}
			else if( Input.GetKey(KeyCode.Y ) )
			{
				m_keyPressTimer += Time.deltaTime;
				if( m_keyPressTimer >= k_timeBeforeKeyRepeat )
				{
					m_keyPressTimer -= k_timeBetweenKeyRepeat;
					m_autoTileMap.BrushGizmo.RedoAction();
				}
			}
			else
			{
				m_keyPressTimer = 0f;
			}
			//---
		}
		*/
		#endregion

		// if( Input.GetKeyDown(KeyCode.C) ) m_showCollisions = !m_showCollisions;

		bool isMouseLeft = Input.GetMouseButton(0);
		bool isMouseRight = Input.GetMouseButton(1);
		bool isMouseMiddle = Input.GetMouseButton(2);
		bool isMouseLeftDown = Input.GetMouseButtonDown(0);
		bool isMouseRightDown = Input.GetMouseButtonDown(1);
		_isCHold = Input.GetKey(KeyCode.C);
		_drawSelectionRect = false;

		Vector3 vGuiMouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
		Vector3 vGuiMouseDelta = vGuiMouse - _mousePrevPos;
		_mousePrevPos = vGuiMouse;

		bool isFocusMiniMap = ( _miniMapAON != null && _miniMapAON.IsInitialized && (_miniMapAON.IsFocus|| _miniMapAON.MinimapRect.Contains( vGuiMouse )));
		//+++ Set window with focus
		if( !isMouseLeft )
		{
			// if( m_rEditorRect.Contains( vGuiMouse ) )
			if( vGuiMouse.x >= _rEditorRect.x)
			{
				_focusWindow = eEditorWindow.TOOLS;
			}
			else
			{
				_focusWindow = eEditorWindow.MAPVIEW;
			}
			// Added an extra padding to avoid drawing tiles when resizing window
			/*
			else if( new Rect(m_rEditorRect.x + m_rEditorRect.width + 10f, 10f, Screen.width-20f-(m_rEditorRect.x + m_rEditorRect.width), Screen.height-20f).Contains( vGuiMouse ) )
			{
				m_focusWindow = eEditorWindow.MAPVIEW;
			}
			else
			{
				m_focusWindow = eEditorWindow.NONE;
			}
			*/
		}
		//---

		// drag and move over the map
		if( isMouseMiddle )
		{
			// if( m_camera2DFollowBehaviour )
			// {
			// 	m_camera2DFollowBehaviour.Target = null;
			// }
			Vector3 vTemp = vGuiMouseDelta; vTemp.y = -vTemp.y;
			_camera2D.transform.position -= (vTemp/100)/_camera2D.Zoom;
		}
		
		if( isFocusMiniMap || IsModeEditInterior || IsModePlay || _isShowDialogClearMap || _isShowGuiMap || _isShowDialogWorld || TriggerShowMoreInfo != null || IsShowEditInfo){
			//Do NotThing
		}
		//
		// Inputs inside Editor Rect
		//
		else if( _rEditorRect.Contains( vGuiMouse ) )
		{
			/*
			if( !USING_LIST_TILESET && !m_isShowDialogClearMap && m_rTilesetRect.Contains( vGuiMouse ) )
			{
				vGuiMouse += new Vector3(m_scrollPos.x, m_scrollPos.y);
				Vector3 vOff = new Vector2(vGuiMouse.x, vGuiMouse.y) - m_rTilesetRect.position;
				int tileX = (int)(vOff.x / k_visualTileWidth);
				int tileY = (int)(vOff.y / k_visualTileHeight);
				int autotileIdx = tileY * m_autoTileMap.Tileset.AutoTilesPerRow + tileX;
				
				if(m_autoTileMap.Tileset.IsExitSlot(autotileIdx) && isMouseLeftDown)
				{
					ClearSeletedTile();
					// select pressed tile
					m_selectedTileIdx = autotileIdx;
					// m_tilesetSelStart = m_tilesetSelEnd = autotileIdx;
					m_autoTileMap.BrushGizmo.RefreshBrushGizmoFromTileset( m_selectedTileIdx);
				}
			}
			*/
		}
		//
		// Insputs inside map view
		//
		else if( _focusWindow == eEditorWindow.MAPVIEW )
		{
			Vector3 vWorldMousePos = _autoTileMap.ViewCamera.ScreenToWorldPoint( new Vector3(Input.mousePosition.x, Input.mousePosition.y) );
			_autoTileMap.BrushGizmo.UpdateBrushGizmo( vWorldMousePos );

			if( isMouseRight || isMouseLeft )
			{
				var mousePosition = Input.mousePosition;
				mousePosition = _autoTileMap.ViewCamera.ScreenToWorldPoint(mousePosition);
				// Debug.Log( string.Format("mousePosition: {0} | {1}", mousePosition.x, mousePosition.y));
				int _x = (int)(mousePosition.x / _autoTileMap.CellSize.x);
				int _y = (int)(-mousePosition.y / _autoTileMap.CellSize.y);
				
				if ( !_autoTileMap.IsValidAutoTilePos(_x, _y)){
					if(isMouseRightDown){
						ResetDrag();
					}
				}else
				{
					if(_drawSelectionRect != isMouseRight){
						_drawSelectionRect = isMouseRight;
						AONGUI_ReDrawAll();
					}
					// get the hit point:
					int tile_x = _x;
					int tile_y = _y;
					// for optimization, is true when mouse is over a diffent tile during the first update
					bool isMouseTileChanged = (tile_x != _prevMouseTileX) || (tile_y != _prevMouseTileY);
					if(isMouseTileChanged){
						AONGUI_ReDrawAll();
					}
					{
						// mouse right for tile selection
						if( isMouseRightDown || isMouseRight && isMouseTileChanged )
						{
							AutoTile tileShowMoreInfoNext = null;
							if( isMouseRightDown )
							{
								//Chech
								for (int layerIdx = 0; layerIdx < _autoTileMap.GetLayerCount(); layerIdx ++){
									var tile = _autoTileMap.GetAutoTile(tile_x, tile_y, layerIdx);
									if(tile == TileShowMoreInfo){
										continue;
									}
									var tileIdx = tile.Id;
									if(_autoTileMap.Tileset.IsExitSlot(tileIdx)){
										if(_autoTileMap.Tileset.GetSlot(tileIdx).IsCanCopyWhenDraw == false){
											tileShowMoreInfoNext = tile;
											Debug.Log("Has right click on Obj");
											break; 	
										}
									}
								}
								if(tileShowMoreInfoNext == null && TileShowMoreInfo != null && TileShowMoreInfo.TileX == tile_x && TileShowMoreInfo.TileY == tile_y){
									tileShowMoreInfoNext = TileShowMoreInfo;
								}
								//
								if(tileShowMoreInfoNext == null){
									// begin mouse right down
									_startDragTileX = tile_x;
									_startDragTileY = tile_y;
									m_selectedTileIdx = -1;
								}
							}
							if(tileShowMoreInfoNext != null){
								ResetDrag();
								TileShowMoreInfo = tileShowMoreInfoNext;
								RefreshTileInfo();
							}else if(_startDragTileX != -1 && _startDragTileY != -1){
								TileShowMoreInfo = null;
								_dragTileX = tile_x;
								_dragTileY = tile_y;
								// m_tilesetSelStart = m_tilesetSelEnd = -1;
							}
						}
						// isMouseLeft
						else if( isMouseLeftDown || isMouseTileChanged) // avoid Push the same action twice during mouse drag
						{
							if(TileShowMoreInfo == null){
								//Brush
								if(isMouseTileChanged){
									Debug.Log("isMouseTileChanged");
								}
								if( _autoTileMap.BrushGizmo.BrushAction != null )
								{
									//+++ case of multiple tiles painting
									AutoTileBrush.TileAction action = new AutoTileBrush.TileAction();
									action.CopyRelative( _autoTileMap, _autoTileMap.BrushGizmo.BrushAction, tile_x, tile_y );
									AutoTileBrush.TileAction.TileData tileOnyChange = _autoTileMap.BrushGizmo.PerformAction( action );
									if(tileOnyChange != null){
										if(tileOnyChange.Tile_id >= 0){
											var slotCheck = _autoTileMap.Tileset.GetSlot(tileOnyChange.Tile_id);
											if(slotCheck.IsCanCopyWhenDraw == false){
												ResetDrag();
												TileShowMoreInfo = _autoTileMap.GetAutoTile(tileOnyChange.Tile_x, tileOnyChange.Tile_y, tileOnyChange.Tile_layer);
												RefreshTileInfo();
											}
										}
									}
								}
								// else 
								// {
								// 	if(m_selectedTileIdx < 0){
								// 		//When m_selectedTileIdx =-1, delete tile at layer 0 and 1
								// 		action.Push(m_autoTileMap, tile_x, tile_y, m_selectedTileIdx, 0);
								// 		action.Push(m_autoTileMap, tile_x, tile_y, m_selectedTileIdx, 1);
								// 	}else if(m_autoTileMap.Tileset.IsExitSlot(m_selectedTileIdx)){
								// 		int layer = m_autoTileMap.Tileset.GetSlot(m_selectedTileIdx).LayerDraw;
								// 		action.Push(m_autoTileMap, tile_x, tile_y, m_selectedTileIdx, layer);
								// 	}
								// }
							}else{
								if(TileShowMoreInfo != null && (TileShowMoreInfo.TileX != tile_x || TileShowMoreInfo.TileY != tile_y)
								 && _autoTileMap.GetAutoTile(tile_x, tile_y, TileShowMoreInfo.Layer).Id < 0){
									int prevX = TileShowMoreInfo.TileX;
									int prevY = TileShowMoreInfo.TileY;
									if(_isCHold){
										if(isMouseLeftDown){
											Debug.Log("Copy obj to: " + tile_x + "_" + tile_y);
											int layer = TileShowMoreInfo.Layer;
											int idTileset = TileShowMoreInfo.Id;
											int idTrigger = _autoTileMap.MapSelect.GetTriggerRef(prevX, prevY);
											int idOverlay = _autoTileMap.MapSelect.GetOverlayRef(prevX, prevY);
											int idRotate = _autoTileMap.MapSelect.GetRotateRef(prevX, prevY);
											
											_autoTileMap.MapSelect.SetTriggerRef( tile_x, tile_y, idTrigger);
											_autoTileMap.MapSelect.SetOverlayRef( tile_x, tile_y, idOverlay);
											_autoTileMap.MapSelect.SetRotateRef( tile_x, tile_y, idRotate);
											_autoTileMap.SetAutoTile( tile_x, tile_y, idTileset, layer, true);
										}
									}else if(isMouseTileChanged){
										Debug.Log("Drag obj to: " + tile_x + "_" + tile_y);
										int layer = TileShowMoreInfo.Layer;
										int idTileset = TileShowMoreInfo.Id;
										int idTrigger = _autoTileMap.MapSelect.GetTriggerRef(prevX, prevY);
										int idOverlay = _autoTileMap.MapSelect.GetOverlayRef(prevX, prevY);
										int idRotate = _autoTileMap.MapSelect.GetRotateRef(prevX, prevY);

										_autoTileMap.MapSelect.ResetTriggerRef( prevX, prevY);
										_autoTileMap.MapSelect.ResetOverlayRef( prevX, prevY);
										_autoTileMap.MapSelect.SetRotateRef( prevX, prevY, 0);
										_autoTileMap.SetAutoTile( prevX, prevY, -1, layer, true);
										
										_autoTileMap.MapSelect.SetTriggerRef( tile_x, tile_y, idTrigger);
										_autoTileMap.MapSelect.SetOverlayRef( tile_x, tile_y, idOverlay);
										_autoTileMap.MapSelect.SetRotateRef( tile_x, tile_y, idRotate);
										_autoTileMap.SetAutoTile( tile_x, tile_y, idTileset, layer, true);

										if(TileShowMoreInfo != null){
											_autoTileMap.TileChunkPoolNode.GetTileChunk( TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, TileShowMoreInfo.Layer).ClearTileOverlay( TileShowMoreInfo.TileX, TileShowMoreInfo.TileY);
										}
										TileShowMoreInfo = _autoTileMap.GetAutoTile(tile_x, tile_y, layer);	
									}
								}
							}
						}
					}
					if(isMouseTileChanged){
						_prevMouseTileX = tile_x;
						_prevMouseTileY = tile_y;
					}
				}
			}
			else
			{
				// Copy selected tiles
				if( _dragTileX != -1 && _dragTileY != -1 )
				{
					//Apply Brush
					_autoTileMap.BrushGizmo.BrushAction = new AutoTileBrush.TileAction();
					int startTileX = Mathf.Min( _startDragTileX, _dragTileX );
					int startTileY = Mathf.Min( _startDragTileY, _dragTileY );
					int endTileX = Mathf.Max( _startDragTileX, _dragTileX );
					int endTileY = Mathf.Max( _startDragTileY, _dragTileY );

					for( int tile_x = startTileX; tile_x <= endTileX; ++tile_x  )
					{
						for( int tile_y = startTileY; tile_y <= endTileY; ++tile_y  )
						{
							// Tile position is relative to last released position ( m_dragTile )
							// if( m_isCtrlKeyHold )
							// {
							// }
							// else
							{
								for (int i = 0; i < _autoTileMap.GetLayerCount(); ++i )
								{
									int tileType = _autoTileMap.GetAutoTile(tile_x, tile_y, i).Id;
									// this allow paste overlay tiles without removing ground or ground overlay
									if(_autoTileMap.Tileset.IsExitSlot(tileType)){
										var slot = _autoTileMap.Tileset.GetSlot(tileType);
										if(!slot.IsCanCopyWhenDraw){
											continue;
										}
									}
									_autoTileMap.BrushGizmo.BrushAction.Push(_autoTileMap, tile_x - _dragTileX, tile_y - _dragTileY, tileType, i, -2);
								}
								int high_brush = _autoTileMap.MapSelect.GetHighRef(tile_x, tile_y);
								_autoTileMap.BrushGizmo.BrushAction.Push(_autoTileMap, tile_x - _dragTileX, tile_y - _dragTileY, -2, (int) eSlotAonTypeLayer.Trigger, high_brush);
							}
						}
					}
					// m_selectedTileIdx = -1;
					// m_autoTileMap.BrushGizmo.RefreshBrushGizmo( startTileX, startTileY, endTileX, endTileY, m_dragTileX, m_dragTileY, m_isCtrlKeyHold );
					_autoTileMap.BrushGizmo.RefreshSpriteRenderers();
					
					_dragTileX = _dragTileY = -1;
				}

				#if false //Zoom Out/In Map
				if (Input.GetAxis("Mouse ScrollWheel") < 0) // back
				{
					if( m_camera2D.Zoom > 1f )
						m_camera2D.Zoom = Mathf.Max(m_camera2D.Zoom-1, 1);
					else
						m_camera2D.Zoom = Mathf.Max(m_camera2D.Zoom/2f, 0.05f);
				}
				else if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
				{
					if( m_camera2D.Zoom >= 1f )
						m_camera2D.Zoom = Mathf.Min(m_camera2D.Zoom+1, 10);
					else
						m_camera2D.Zoom*=2f;
				}
				#endif
			}
		}
	}

	
	private void ResetDrag(){
		_startDragTileX = _prevMouseTileX = -1;
		_startDragTileY = _prevMouseTileY = -1;
	}
	// void _GenerateCollisionTexture()
	// {
	// 	SpriteRenderer sprRender = m_spriteCollLayer.GetComponent<SpriteRenderer>();
	// 	Texture2D texture = new Texture2D(Screen.width / (m_autoTileMap.Tileset.TilePartWidth / 2) + 50, Screen.height / (m_autoTileMap.Tileset.TilePartHeight / 2) + 50);
	// 	texture.filterMode = FilterMode.Point;
	// 	texture.wrapMode = TextureWrapMode.Clamp;
	// 	sprRender.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), AutoTileset.PixelToUnits);
	// }

	public static void OnGUIBox(Rect position, string text){
		// Color oldColor = GUI.color;
		// GUI.color = Color.black;
		AONGUI.Box( position, "" );
		// GUI.color = oldColor;
	}

	protected override void OnGUIAON()
	{
		if( !_isInitialized )
		{
			return;
		}
		if(InputFieldHelper.Instance.IsShowPickIcon || InputFieldHelper.Instance.IsShowPickModel){
			return;
		}
		GameGui.IsCheck = false;
		// ComboBox.UpdateOnGUI();
		FPSDisplay.Instance.OnGUI();
		if(IsModeEditInterior){
			OnGUIInterior();
			return;
		}
		if(IsModePlay){
			AONGUI.Button( new Rect(2, 2, 60, DefineAON.GUI_Height_Button), "Back", () => {
				StopPlay();
			});
			TriggerGame.Instance.OnGUI();
			TriggerGame.Instance.OnGUIDebug( this);
			// PropertysGame.Instance.OnGUI();
			// ShopGame.Instance.OnGUI();
			return;
		}
		if(InputFieldHelper.Instance.IsShowNoti()){
			return;
		}
		#region Init listStyle
		if(t_dark == null){
			t_dark = new Texture2D(2, 2);
			t_hightLight = UtilsAON.MakeTex( 2, 2, new Color( 1f, 1f, 0f, 0.2f ) );
			t_gray_2 = UtilsAON.MakeTex( 2, 2, new Color( 0.2f, 0.2f, 0.2f, 1.0f ) );
			t_gray_3 = UtilsAON.MakeTex( 2, 2, new Color( 0.3f, 0.3f, 0.3f, 1.0f ) );
			t_gray_35 = UtilsAON.MakeTex( 2, 2, new Color( 0.35f, 0.35f, 0.35f, 1.0f ) );
			t_gray_4 = UtilsAON.MakeTex( 2, 2, new Color( 0.4f, 0.4f, 0.4f, 1.0f ) );
			t_gray_7 = UtilsAON.MakeTex( 2, 2, new Color( 0.7f, 0.7f, 0.7f, 1.0f ) );
		}
		if(listStyle == null){
			listStyle = new GUIStyle();
			listStyle.normal.textColor = Color.white;
		}
		listStyle.padding.left =
		listStyle.padding.right =
		listStyle.padding.top =
		listStyle.padding.bottom = 4;
		AONGUI.skin.label.alignment = TextAnchor.LowerLeft;
		AONGUI.skin.textField.alignment = TextAnchor.MiddleLeft;
		AONGUI.skin.button.alignment = TextAnchor.MiddleLeft;
		AONGUI.skin.textArea.alignment = TextAnchor.MiddleLeft;
		listStyle.onNormal.background =
		listStyle.onHover.background =
		listStyle.hover.background = t_dark;
		AONGUI.skin.box.normal.background = t_gray_4;

		if(listStyleGrid == null){
			listStyleGrid = new GUIStyle();
		}
		listStyleGrid.padding.left =
		listStyleGrid.padding.right =
		listStyleGrid.padding.top =
		listStyleGrid.padding.bottom = 4;
		listStyleGrid.normal.textColor = new Color( 0.7f, 0.7f, 0.7f, 1.0f );
		listStyleGrid.onHover.textColor = Color.white;
		listStyleGrid.hover.textColor = Color.white;
		listStyleGrid.normal.background = t_gray_3; 
		listStyleGrid.onNormal.background = t_gray_7;
		listStyleGrid.onHover.background =
		listStyleGrid.hover.background = t_gray_7;


		if(listStyleBlack == null){
			listStyleBlack = new GUIStyle();
		}
		listStyleBlack.normal.background = t_gray_3;

		if(listStyleBlack2 == null){
			listStyleBlack2 = new GUIStyle();
		}
		listStyleBlack2.normal.background = t_gray_35;

		ComboBox.ListStyleContent.onNormal.background = t_dark;
		ComboBox.ListStyleContent.onHover.background = t_dark;
		ComboBox.ListStyleContent.hover.background = t_dark;
		ComboBox.ListStyleContent.normal.background = t_gray_2;

		ComboBox.ListStyleGrid.normal.background = t_gray_35;
		ComboBox.ListStyleHightlight.normal.background = t_hightLight;
		#endregion
		
		if(_isShowDialogWorld){
			AComponent_Button.OnClick closeDialog = () => {
				_isShowDialogWorld = false;
				RefreshEnableMinimap();
			};
			WorldGui.Instance.WorldOnGUI(this, _autoTileMap.MapsData, closeDialog);
			return;
		}
		// m_isCtrlKeyHold = Event.current.control || Event.current.command;

		OnGUIDrawOnMap();

		float fPad = 4f;
		float fScrollBarWidth = 16f;
		int tilesWidth = k_visualTileWidth * AutoTilesPerRow;
		float w = tilesWidth + 2 * fPad + fScrollBarWidth;
		
		if(_miniMapAON.enabled){
			OnGUIBox( new Rect(Screen.width - w, 0, w, Screen.height - w), "" );
			_rEditorRect = new Rect(Screen.width - w, 4f, w, Screen.height - w - 4);
		}else
		{
			OnGUIBox( new Rect(Screen.width - w, 0, w, Screen.height), "" );
			_rEditorRect = new Rect(Screen.width - w, 4f, w, Screen.height - 8);
		}
		
		if(_isShowDialogClearMap){
			OnGUIDialogClearMap();
			return;
		}
		
		if(TriggerShowMoreInfo != null){
			bool isShowMoreInfo = true;
			AComponent_Button.OnClick onCloseDialog = () => {
				TriggerShowMoreInfo = null;
			};
			TriggerShowGUI(TriggerShowMoreInfo, new Rect(_rEditorRect.x, _rEditorRect.y, _rEditorRect.width, _rEditorRect.height), _autoTileMap, this, ref isShowMoreInfo, onCloseDialog);
			if(isShowMoreInfo){
				AONGUI.Button( new Rect(_rEditorRect.x + _rEditorRect.width - 154, _rEditorRect.y, 150, DefineAON.GUI_Height_Button), "Close edit " + TriggerShowMoreInfo.GetType().Name + " (Esc)", KeyCode.Escape, onCloseDialog);
			}
			return;
		}
		if(_isShowGuiMap){
			OnGUIMap();
			return;
		}
		if(TileShowMoreInfo != null){
			OnGUIMoreInfo();
			return;
		}
		// m_autoTileMap.ClearMap();
		// m_autoTileMap.RefreshMinimapTexture();
		if(_isShowDialogSave){
			float yBeginGui = 4f;
			if(ShowSaveDialog == ""){
				yBeginGui += 16f;
				AONGUI.Button( 
					new Rect(_rEditorRect.x, yBeginGui, _rEditorRect.width, DefineAON.GUI_Height_Button), 
					"Save Map " + _autoTileMap.MapIdxSelect, 
					()=>{
						var title = "Save Map " + _autoTileMap.MapIdxSelect;
						FileBrowser.SaveFilePanel( title, title, Application.persistentDataPath, "Map_" + _autoTileMap.MapIdxSelect + ".json", new string[] { "json"}, null, (bool canceled, string filePath) => {
							if (canceled)
							{
								return;
							}
							_autoTileMap.SaveCurrentMapJson(filePath);
							ShowSaveDialog = filePath;
						});
					}
				);
				
				yBeginGui += 32f;
				yBeginGui += 16f;
				AONGUI.Button( new Rect(_rEditorRect.x, yBeginGui, _rEditorRect.width, DefineAON.GUI_Height_Button), "Save All Map", () =>{
					var title = "Save All Map";
					FileBrowser.SaveFilePanel( title, title, Application.persistentDataPath, "AllMap.json", new string[] { "json"}, null, (bool canceled, string filePath) => {
						if (canceled)
						{
							return;
						}
						_autoTileMap.SaveAllMapAsJson(filePath);
						ShowSaveDialog = filePath;
					});
				});
				yBeginGui += 32f;
				yBeginGui += 16f;
			}else{
				yBeginGui += 16f;
				AONGUI.Label(new Rect(_rEditorRect.x+ 4f, yBeginGui, _rEditorRect.width - 8f, DefineAON.GUI_Height_Label), "Data has saved at:");
				yBeginGui += 32f;
				AONGUI.TextField(new Rect(_rEditorRect.x+ 4f, yBeginGui, _rEditorRect.width - 8f, 22f), ShowSaveDialog, (string text) => {
					// string EditShowSaveDialog = text;
				});
				yBeginGui += 32f;
				// if( GUI.Button( new Rect(m_rEditorRect.x, yBeginGui, m_rEditorRect.width, 32), "Save") )
				// {
				// 	m_autoTileMap.SaveMapWithData(ShowSaveDialog);
				// 	ShowSaveDialog = "";
				// 	m_isShowDialogSave = false;
				// }
				// yBeginGui += 32f;
				AONGUI.Button( new Rect(_rEditorRect.x, yBeginGui, _rEditorRect.width, DefineAON.GUI_Height_Button), "Open folder", ()=>{
					OpenInFileBrowser.Open(ShowSaveDialog);
				});
				yBeginGui += 32f;
				yBeginGui += 16f;
			}
			AONGUI.Button( new Rect(_rEditorRect.x, yBeginGui, _rEditorRect.width, DefineAON.GUI_Height_Button), "Back", ()=>{
				ShowSaveDialog = "";
				_isShowDialogSave = false;
			});
			return;
		}
		if( _isShowDialogLoad){
			float yBeginGui = 4f;
			yBeginGui += 16f;
			if(ShowLoadDialog == ""){
				// GUI.Label(new Rect(m_rEditorRect.x+ 4f, yBeginGui, m_rEditorRect.width - 8f, 32 - 8f), "Can't load data");
				// yBeginGui += 32f;
				// yBeginGui += 16f;
				AONGUI.Button( new Rect(_rEditorRect.x, yBeginGui, _rEditorRect.width, DefineAON.GUI_Height_Button), "Load Map " + _autoTileMap.MapIdxSelect, ()=>{
					var title = "Load Map " + _autoTileMap.MapIdxSelect;
					var path = Application.persistentDataPath + "/Map_" + _autoTileMap.MapIdxSelect + ".json";
					FileBrowser.OpenFilePanel(title, path, new string[] { "json"}, null, (bool canceled, string filePath) => {
						if (canceled)
						{
							return;
						}
						bool result = _autoTileMap.LoadAt(filePath);
						if(result){
							ShowLoadDialog = "Load success";
						}else{
							ShowLoadDialog = "Load fail";
						}
					});
				});
				yBeginGui += 32f;
				//
				yBeginGui += 16f;
				AONGUI.Button( new Rect(_rEditorRect.x, yBeginGui, _rEditorRect.width, DefineAON.GUI_Height_Button), "Load All Map", ()=>{
					var title = "Load Map " + _autoTileMap.MapIdxSelect;
					var path = Application.persistentDataPath + "/AllMap.json";
					FileBrowser.OpenFilePanel(title, path, new string[] { "json"}, null, (bool canceled, string filePath) => {
						if (canceled)
						{
							return;
						}
						bool result = _autoTileMap.LoadAllMap(filePath);
						if(result){
							ShowLoadDialog = "Load success";
						}else{
							ShowLoadDialog = "Load fail";
						}
					});
				});
				yBeginGui += 32f;
			}else{
				AONGUI.Label(new Rect(_rEditorRect.x+ 4f, yBeginGui, _rEditorRect.width - 8f, DefineAON.GUI_Height_Label), ShowLoadDialog);
				yBeginGui += 32f;
			}
			yBeginGui += 16f;
			AONGUI.Button( new Rect(_rEditorRect.x, yBeginGui, _rEditorRect.width, DefineAON.GUI_Height_Label), "Back", () => {
				ShowLoadDialog = "";
				_isShowDialogLoad = false;
			});
			return;
		}

		float yGui = _rEditorRect.y;
		var listRect = new Rect( _rEditorRect.x, _rEditorRect.y, _rEditorRect.width - 18, Math.Max(LastContentEnd - yGui, _rEditorRect.height));
		AONGUI.BeginScrollView(_rEditorRect, m_scrollPos, listRect, false, true, (Vector2 v) => {
			m_scrollPos = v;
		});
		OnGUITileSet( ref yGui, listRect);
		LastContentEnd = yGui;
		AONGUI.EndScrollView();
	}
	private Vector2 m_scrollPos = Vector2.zero;
	private float LastContentEnd = 0;

	void OnGUIMap(){
		AComponent_Button.OnClick closeDialog = () => {
			_isShowGuiMap = false;
			RefreshEnableMinimap();
		};
		MapGui.Instance.MapOnGUI( this, _autoTileMap.MapSelect, _autoTileMap, closeDialog);
	}
	
	void OnGUIDialogClearMap(){
		float yGui = _rEditorRect.y + 4f;
		AONGUI.Label(new Rect(_rEditorRect.x+ 4f, yGui, _rEditorRect.width - 8f, DefineAON.GUI_Height_Label), "All data map( terrain, filer, housing, trigger)");
		yGui += 30f;
		AONGUI.Label(new Rect(_rEditorRect.x+ 4f, yGui, _rEditorRect.width - 8f, DefineAON.GUI_Height_Label), "From [map " + _autoTileMap.MapIdxSelect + "] will be clear!");
		yGui += 32f;
		AONGUI.Button( new Rect(_rEditorRect.x, yGui, _rEditorRect.width, DefineAON.GUI_Height_Button), "Cancel", () => {
			_isShowDialogClearMap = false;
		});
		
		yGui += 34f;
		AONGUI.Button( new Rect(_rEditorRect.x, yGui, _rEditorRect.width, DefineAON.GUI_Height_Button), "Ok", ()=>{
			_autoTileMap.BrushGizmo.Clear();
			_autoTileMap.MapSelect.ClearData();
			_autoTileMap.ResetTerrain();
			_autoTileMap.MapSelect.LoadFromCompression();
			_autoTileMap.MapSelect.SaveData( _autoTileMap);
			_autoTileMap.RefreshMinimapTexture();
			_autoTileMap.ResetTileChunkPool();
			_autoTileMap.UpdateChunks();
			_isShowDialogClearMap = false;
		});
	}

	void OnGUIDrawOnMap(){
		#region Draw Selection Rect
		// Map Version
		if( _drawSelectionRect )
		{
			Rect selRect = new Rect( );
			selRect.width = (Mathf.Abs(_dragTileX - _startDragTileX) + 1) * _camera2D.Zoom * _autoTileMap.CellSize.x * AutoTileset.PixelToUnits;
			selRect.height = (Mathf.Abs(_dragTileY - _startDragTileY) + 1) * _camera2D.Zoom * _autoTileMap.CellSize.y * AutoTileset.PixelToUnits;
			
			float worldX = Mathf.Min(_startDragTileX, _dragTileX) * _autoTileMap.CellSize.x;
			float worldY = -Mathf.Min(_startDragTileY, _dragTileY) * _autoTileMap.CellSize.y;
			
			Vector3 vScreen = _camera2D.Camera.WorldToScreenPoint(new Vector3(worldX, worldY) + _autoTileMap.transform.position);

			//NOTE: vScreen will vibrate if the camera has KeepInsideMapBounds enabled and because of the zoom out, the camera area is bigger than camera limit bounds
			selRect.position = new Vector2( vScreen.x, vScreen.y );

			selRect.y = Screen.height - selRect.y;
			UtilsGuiDrawing.DrawRectWithOutline( selRect, new Color(0f, 1f, 0f, 0.2f), new Color(0f, 1f, 0f, 1f));
		}
		#endregion
		
		if(TileShowMoreInfo != null && TileShowMoreInfo.Id >= 0){
			//m_tileShowMoreInfo
			var slot = _autoTileMap.Tileset.GetSlot(TileShowMoreInfo.Id);
			//Draw Select
			{
				Rect selRect = new Rect( );
				selRect.width = slot.Size.x * _camera2D.Zoom * _autoTileMap.CellSize.x * AutoTileset.PixelToUnits;
				selRect.height = slot.Size.y * _camera2D.Zoom * _autoTileMap.CellSize.y * AutoTileset.PixelToUnits;
				float worldX = TileShowMoreInfo.TileX * _autoTileMap.CellSize.x;
				float worldY = -TileShowMoreInfo.TileY * _autoTileMap.CellSize.y;                
				Vector3 vScreen = _camera2D.Camera.WorldToScreenPoint(new Vector3(worldX, worldY) + _autoTileMap.transform.position);

				//NOTE: vScreen will vibrate if the camera has KeepInsideMapBounds enabled and because of the zoom out, the camera area is bigger than camera limit bounds
				selRect.position = new Vector2( vScreen.x, vScreen.y );
				selRect.y = Screen.height - selRect.y;
				UtilsGuiDrawing.DrawRectWithOutline( selRect, new Color(0f, 1f, 0f, 0.2f), new Color(0f, 1f, 0f, 1f));
			}
		}
		
	}

	void OnGUIMoreInfo(){
		if(TileShowMoreInfo.Id == -1){
			Debug.Log("m_tileShowMoreInfo.Id should be not -1");
			return;
		}
		//m_tileShowMoreInfo
		var slot = _autoTileMap.Tileset.GetSlot(TileShowMoreInfo.Id);
		// Set rotate
		float yGui = 0;
		if(IsShowEditInfo == false){
			AONGUI.Label(new Rect(_rEditorRect.x + 4f, yGui + 4f, _rEditorRect.width - 8f, DefineAON.GUI_Height_Label), "(click) to move Obj, (C) + (click) to copy Obj");
			yGui += 32;
		}
		if(slot.TypeLayer == eSlotAonTypeLayer.Overlay && slot.TypeObj == eSlotAonTypeObj.Filler3D){
			OnGUIMoreInfoOverlayByModel3D( slot, yGui);
			return;
		}
		if(slot.TypeLayer == eSlotAonTypeLayer.Trigger || slot.TypeLayer == eSlotAonTypeLayer.Overlay){
			OnGUIMoreInfoOverlayOrTrigger( slot, yGui);
			return;
		}
	}

	string[] m_rotate_list = {"Bot", "Right" , "Top", "Left"};
	void OnGUIPickRotate(float yTopGui, SlotAon slot){
		float xLeft = 70;
		AONGUI.Label(new Rect(_rEditorRect.x + 4f, yTopGui - 2f, xLeft, DefineAON.GUI_Height_Label), "Face to:");
		int rotate = _autoTileMap.MapSelect.GetRotateRef( TileShowMoreInfo.TileX, TileShowMoreInfo.TileY);
		rotate = (rotate % 360)  / 90;
		if(rotate < 0 || rotate >= 4){
			rotate = 0;
		}
		var vRectSubTile = new Rect(_rEditorRect.x + 4f + xLeft, _rEditorRect.y + yTopGui, _rEditorRect.width - xLeft - 4f, 26);
		AONGUI.SelectionGrid(vRectSubTile, rotate, m_rotate_list, m_rotate_list.Length, listStyleGrid, (int rotateNext) => {
			rotateNext = rotateNext * 90;
			_autoTileMap.MapSelect.SetRotateRef( TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, rotateNext);
			_autoTileMap.TileChunkPoolNode.GetTileChunk( TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, (int)slot.TypeLayer).RefreshTileOverlay( TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, true);	
		});
	}

	void OnGUIMoreInfoOverlayByModel3D( SlotAon slot, float yGui){
		AONGUI.Label(new Rect(_rEditorRect.x + 4f, yGui + 4f, _rEditorRect.width - 8f, DefineAON.GUI_Height_Label), string.Format("{0}: [{1},{2}]", slot.Name, TileShowMoreInfo.TileX, TileShowMoreInfo.TileY));
		yGui += 32;
		OnGUIPickRotate(yGui, slot);
		yGui += 32;
		AONGUI.Button( new Rect(_rEditorRect.x, yGui, _rEditorRect.width, DefineAON.GUI_Height_Button), "Remove", () =>{
			int tx = TileShowMoreInfo.TileX;
			int ty = TileShowMoreInfo.TileY;
			_autoTileMap.TileChunkPoolNode.GetTileChunk( tx, ty, (int)slot.TypeLayer).ClearTileOverlay( tx, ty);
			_autoTileMap.SetAutoTile( tx, ty, -1, TileShowMoreInfo.Layer, true);
			_autoTileMap.MapSelect.SetOverlayRef(TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, -1);
			_autoTileMap.MapSelect.SetRotateRef(TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, 0);
			TileShowMoreInfo = null;
		});
		
		yGui += 32;
		int idxModelRef = _autoTileMap.MapSelect.GetOverlayRef(TileShowMoreInfo.TileX, TileShowMoreInfo.TileY);
		#region PickModel
		{
			float left = 80f;
			AONGUI.Label(new Rect(_rEditorRect.x + 4f, yGui, left, DefineAON.GUI_Height_Label), "Model: ");
			var comboBoxFiler3DList = ComboBoxHelper.Instance.Filer3DList(_autoTileMap, slot.idRef);
			comboBoxFiler3DList.SelectedItemIndex = idxModelRef;
			comboBoxFiler3DList.Rect.x = _rEditorRect.x + left;
			comboBoxFiler3DList.Rect.y = yGui;
			comboBoxFiler3DList.Rect.width = _rEditorRect.width - left;
			comboBoxFiler3DList.Rect.height = 32f;
			comboBoxFiler3DList.Show( _rEditorRect.height - yGui, (int selectedInterior) => {
				_autoTileMap.MapSelect.SetOverlayRef(TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, selectedInterior);
				int tx = TileShowMoreInfo.TileX;
				int ty = TileShowMoreInfo.TileY;
				_autoTileMap.TileChunkPoolNode.GetTileChunk( tx, ty, (int)slot.TypeLayer).RefreshTileOverlay( tx, ty, true);
			});
			yGui += 32f;
			if(comboBoxFiler3DList.IsDropDownListVisible){
				return;
			}
		}
		#endregion
		AONGUI.Button( new Rect(_rEditorRect.x, _rEditorRect.y + _rEditorRect.height - 32, _rEditorRect.width, DefineAON.GUI_Height_Button), "Close (Esc)", KeyCode.Escape, ()=>{
			TileShowMoreInfo = null;
		});
	}

	void OnGUIMoreInfoOverlayOrTrigger( SlotAon slot, float yBeginGui){
		#region Name & Ref
		if(IsShowEditInfo == false){
			AONGUI.Label(new Rect(_rEditorRect.x+ 4f, yBeginGui + 4f, _rEditorRect.width - 8f, DefineAON.GUI_Height_Label), string.Format("{0}: [{1},{2}]", slot.Name, TileShowMoreInfo.TileX, TileShowMoreInfo.TileY));
			yBeginGui += 32f;
			AONGUI.Button( new Rect(_rEditorRect.x, yBeginGui, _rEditorRect.width, DefineAON.GUI_Height_Button), "Remove", () => {
				if(!TileChunk.IS_ALWAY_SHOW_OVERLAY && slot.TypeLayer == eSlotAonTypeLayer.Overlay){
					_autoTileMap.TileChunkPoolNode.GetTileChunk( TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, (int)slot.TypeLayer).ClearTileOverlay( TileShowMoreInfo.TileX, TileShowMoreInfo.TileY);
				}
				for(int x = TileShowMoreInfo.TileX; x < TileShowMoreInfo.TileX + slot.Size.x && x < _autoTileMap.MapSelect.TileMapWidth; x++){
					for(int y = TileShowMoreInfo.TileY; y < TileShowMoreInfo.TileY + slot.Size.y && y < _autoTileMap.MapSelect.TileMapHeight; y++){
						_autoTileMap.SetAutoTile( x, y, -1, TileShowMoreInfo.Layer, true);
					}
				}
				_autoTileMap.MapSelect.SetRotateRef(TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, 0);
				if(slot.TypeLayer == eSlotAonTypeLayer.Trigger){
					_autoTileMap.MapSelect.ResetTriggerRef(TileShowMoreInfo.TileX, TileShowMoreInfo.TileY);
				}else if(slot.TypeLayer == eSlotAonTypeLayer.Overlay){
					_autoTileMap.MapSelect.ResetOverlayRef(TileShowMoreInfo.TileX, TileShowMoreInfo.TileY);
				}
				TileShowMoreInfo = null;
			});
			yBeginGui += 32f;
			AONGUI.Button( new Rect(_rEditorRect.x, _rEditorRect.y + _rEditorRect.height - 32, _rEditorRect.width, DefineAON.GUI_Height_Button), "Close (Esc)", KeyCode.Escape, ()=>{
				TileShowMoreInfo = null;
			});
			//Reference
			if(slot.TypeLayer == eSlotAonTypeLayer.Trigger){
				int idxRef = _autoTileMap.MapSelect.GetTriggerRef(TileShowMoreInfo.TileX, TileShowMoreInfo.TileY);
				int countTrigger = _autoTileMap.MapSelect.TriggerCountAt(slot.TypeObj);
				if(countTrigger == 0){
					AONGUI.Button( new Rect(_rEditorRect.x, yBeginGui, _rEditorRect.width, DefineAON.GUI_Height_Button), "Create new " + slot.Name, () => {
						_autoTileMap.MapSelect.CreateNewTrigger(TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, slot.TypeObj);
					});
					return;
				}else{
					if(slot.TypeObj == eSlotAonTypeObj.Person){
						OnGUIPickRotate(yBeginGui, slot);
						yBeginGui += 32f;
					}
					AONGUI.Label(new Rect(_rEditorRect.x, yBeginGui, _rEditorRect.width, DefineAON.GUI_Height_Label), string.Format("Ref {0}: {1}", slot.Name, idxRef));
					yBeginGui += 32f;
					string[] l = new string[countTrigger];
					for( int i = 0; i < countTrigger; i++){
						Trigger trigger = _autoTileMap.MapSelect.GetTriggerByIdxRef(i, slot.TypeObj);
						l[i] = string.Format("{0} ({1}) {2}", slot.Name, i.ToString(), trigger.Name());
					}
					var ComboBoxOverlayTrigger = ComboBoxHelper.Instance.TypeObj( _autoTileMap.MapSelect, slot.TypeObj);
					if(ComboBoxOverlayTrigger.IsDropDownListVisible){
						//Create new warps
						AONGUI.Button( new Rect(_rEditorRect.x, _rEditorRect.y + _rEditorRect.height - 64f, _rEditorRect.width, DefineAON.GUI_Height_Button), "Create new "  + slot.Name, ()=>{
							_autoTileMap.MapSelect.CreateNewTrigger(TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, slot.TypeObj);
						});
					}
					ComboBoxOverlayTrigger.SelectedItemIndex = idxRef;
					ComboBoxOverlayTrigger.Rect.x = _rEditorRect.x;
					ComboBoxOverlayTrigger.Rect.y = yBeginGui;
					ComboBoxOverlayTrigger.Rect.width = _rEditorRect.width;
					ComboBoxOverlayTrigger.Rect.height = 32;
					ComboBoxOverlayTrigger.Show( _rEditorRect.height - yBeginGui - 64f, (int selectedTrigger) => {
						_autoTileMap.MapSelect.SetTriggerRef(TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, selectedTrigger);
					});
					yBeginGui += 32f;
					if(ComboBoxOverlayTrigger.IsDropDownListVisible){
						return;
					}
				}
				yBeginGui += 4;
				if( idxRef != -1)
				{
					AONGUI.Button( new Rect(_rEditorRect.x + _rEditorRect.width - 200, yBeginGui, 200, DefineAON.GUI_Height_Button), "Edit " + slot.Name, () => {
						IsShowEditInfo = true;
					});
				}
			}
			else if(slot.TypeLayer == eSlotAonTypeLayer.Overlay){
				OnGUIPickRotate(yBeginGui, slot);
				yBeginGui += 32f;
				int idxRef = _autoTileMap.MapSelect.GetOverlayRef(TileShowMoreInfo.TileX, TileShowMoreInfo.TileY);
				int countOverlay = _autoTileMap.MapSelect.OverlayCountAt(slot.TypeObj);
				if(countOverlay == 0){
					AONGUI.Button( new Rect(_rEditorRect.x, yBeginGui, _rEditorRect.width, DefineAON.GUI_Height_Button), "Create new " + slot.Name, () => {
						_autoTileMap.MapSelect.CreateNewOverlay(TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, slot.TypeObj);
					});
					return;
				}else{
					AONGUI.Label(new Rect(_rEditorRect.x, yBeginGui, _rEditorRect.width, DefineAON.GUI_Height_Label), string.Format("Ref {0}: {1}", slot.Name, idxRef));
					yBeginGui += 32f;
					string[] l = new string[countOverlay];
					for( int i = 0; i < countOverlay; i++){
						Overlay Overlay = _autoTileMap.MapSelect.GetOverlayByIdxRef(i, slot.TypeObj);
						l[i] = string.Format("{0} ({1}) {2}", slot.Name, i.ToString(), Overlay.Name());
					}
					var ComboBoxOverlayTrigger = ComboBoxHelper.Instance.TypeObj( _autoTileMap.MapSelect, slot.TypeObj);
					if(ComboBoxOverlayTrigger.IsDropDownListVisible){
						//Create new warps
						AONGUI.Button( new Rect(_rEditorRect.x, _rEditorRect.y + _rEditorRect.height - 64f, _rEditorRect.width, DefineAON.GUI_Height_Button), "Create new "  + slot.Name, () => {
							_autoTileMap.MapSelect.CreateNewOverlay(TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, slot.TypeObj);
						});
					}
					ComboBoxOverlayTrigger.SelectedItemIndex = idxRef;
					ComboBoxOverlayTrigger.Rect.x = _rEditorRect.x;
					ComboBoxOverlayTrigger.Rect.y = yBeginGui;
					ComboBoxOverlayTrigger.Rect.width = _rEditorRect.width;
					ComboBoxOverlayTrigger.Rect.height = 32;
					ComboBoxOverlayTrigger.Show( _rEditorRect.height - yBeginGui - 64f, (int selectedOverlay) => {
						int tx = TileShowMoreInfo.TileX;
						int ty = TileShowMoreInfo.TileY;
						_autoTileMap.MapSelect.SetOverlayRef(tx, ty, selectedOverlay);
						_autoTileMap.TileChunkPoolNode.GetTileChunk( TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, (int)slot.TypeLayer).RefreshTileOverlay( TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, true);	
					});
					yBeginGui += 32f;
					if(ComboBoxOverlayTrigger.IsDropDownListVisible){
						return;
					}
				}
				
				if( idxRef != -1)
				{
					AONGUI.Button( new Rect(_rEditorRect.x + _rEditorRect.width - 200, yBeginGui, 200, DefineAON.GUI_Height_Button), "Edit " + slot.Name, () => {
						IsShowEditInfo = true;
					});
					return;
				}
			}
		}
		#endregion
		if(IsShowEditInfo){
			// Edit
			bool isShowMoreInfo = true;
			AComponent_Button.OnClick onCloseDialog = () => {
				IsShowEditInfo = false;
			};
			var rectRight = new Rect(_rEditorRect.x + 4, _rEditorRect.y, _rEditorRect.width - 8, _rEditorRect.height);
			if(slot.TypeLayer == eSlotAonTypeLayer.Trigger){
				Trigger trigger = _autoTileMap.MapSelect.GetTrigger(TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, slot.TypeObj);
				if(trigger != null){
					TriggerShowGUI( trigger, rectRight, _autoTileMap, this, ref isShowMoreInfo, onCloseDialog);
				}
			}else if(slot.TypeLayer == eSlotAonTypeLayer.Overlay){
				Overlay overlay = _autoTileMap.MapSelect.GetOverlay(TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, slot.TypeObj);
				if(overlay != null && IsShowEditInfo){
					bool hadChanged = overlay.ShowGUI(rectRight, _autoTileMap, this);
				}
			}
			if(isShowMoreInfo){
				AONGUI.Button( new Rect(rectRight.x + rectRight.width - 154, rectRight.y, 150, DefineAON.GUI_Height_Button), "Close edit " + slot.Name + " (Esc)", KeyCode.Escape, onCloseDialog);
			}
		}
	}

	void OnGUITileSet( ref float yGui, Rect rect){
		float fPad = 4f;
		float fScrollBarWidth = 16f;
		// int tilesHeight = k_visualTileHeight * (256 / _autoTileMap.Tileset.AutoTilesPerRow);
		float w_bt = rect.width / 3;
		//TOP TOOLBAR
		{
			var ComboBoxMap = ComboBoxHelper.Instance.MapList(_autoTileMap);
			ComboBoxMap.Rect.x = rect.x + 2;
			ComboBoxMap.Rect.y = yGui;
			ComboBoxMap.Rect.width = w_bt * 2 - 4;
			ComboBoxMap.Rect.height = 32;
			ComboBoxMap.SelectedItemIndex = _autoTileMap.MapIdxSelect;
			if(ComboBoxMap.IsDropDownListVisible){
				AONGUI.Button( new Rect(rect.x, rect.y + rect.height - 32, rect.width, DefineAON.GUI_Height_Button), "Add Map", () => {
					Debug.Log("On click Addmap");
					_autoTileMap.MapsData.AddMap();
					//Reset BoxMapUI
					ComboBoxHelper.Instance.UpdateDataMapList(_autoTileMap);
				});
			}
			ComboBoxMap.Show( rect.height - 32, (int selectedMap) => {
				_autoTileMap.BrushGizmo.Clear();
				_autoTileMap.MapsData.MapIndex = selectedMap;
				_autoTileMap.SaveMap();
				_autoTileMap.SetDataMapSelect(selectedMap);
				_autoTileMap.ForceReloadMapNow();
			});
			if(ComboBoxMap.IsDropDownListVisible){
				return;
			}
			AONGUI.Button( new Rect(rect.x + w_bt * 2 + 2, yGui, w_bt - 4, DefineAON.GUI_Height_Button), "World edit", () => {
				_isShowDialogWorld = true;
				RefreshEnableMinimap();
			});
			yGui += 32f;
			/*
			if( GUI.Button( new Rect(m_rEditorRect.x, m_rEditorRect.y, 130, 32), m_showCollisions? "Hide Collisions (C)" : "Show Collisions (C)") )
			{
				m_showCollisions = !m_showCollisions;
			}
			 */
			
			AONGUI.Button( new Rect(rect.x + 2, yGui, w_bt - 4, DefineAON.GUI_Height_Button), "Save", () => {
				ShowSaveDialog = "";
				_isShowDialogSave = true;
			});
			
			AONGUI.Button( new Rect(rect.x + w_bt + 2, yGui, w_bt - 4, DefineAON.GUI_Height_Button), "Load", ()=> {
				ShowLoadDialog = "";
				_isShowDialogLoad = true;
			});

			AONGUI.Button( new Rect(rect.x + w_bt * 2 + 2, yGui, w_bt - 4, DefineAON.GUI_Height_Button), "Map edit", () => {
				_isShowGuiMap = true;
				RefreshEnableMinimap();
			});
			yGui += 32;
			// if( GUI.Button( new Rect(m_rEditorRect.x + w_bt * 2, m_rEditorRect.y + 32, w_bt, 32), "Clear Map") )
			// {
			// 	m_isShowDialogClearMap = true;
			// }

			/*
			if (GUI.Button(new Rect(m_rEditorRect.x + 250, m_rEditorRect.y + 32, 30, 32), ">"))
			{
				m_isLayersMenuHidden = !m_isLayersMenuHidden;
			}
			if (!m_isLayersMenuHidden)
			{
				if (GUI.Button(new Rect(m_rTilesetRect.x, m_rEditorRect.y + 64, m_rTilesetRect.width + fScrollBarWidth, 32), m_autoTileMap.BrushGizmo.SmartBrushEnabled ? "Smart Brush Enabled" : "Smart Brush Disabled"))
				{
					m_autoTileMap.BrushGizmo.SmartBrushEnabled = !m_autoTileMap.BrushGizmo.SmartBrushEnabled;
				}
				comboBoxControl.Rect.x = m_rTilesetRect.x;
				comboBoxControl.Rect.y = m_rEditorRect.y + 96;
				comboBoxControl.Rect.width = m_rTilesetRect.width + fScrollBarWidth;
				comboBoxControl.Rect.height = 32;
				m_autoTileMap.BrushGizmo.SelectedLayer = comboBoxControl.Show();
			}
			*/
			
		}
		#region Draw Tileset Selection Buttons
		{
			yGui += 8;
			var vRectSubTile = new Rect(rect.x, yGui, rect.width, 26);
			string[] m_tileGroupNames = Enum.GetNames(typeof(eSlotAonTypeBrush));
			var subTilesetIdx_last = (int)m_subTilesetIdx;
			// var selecteTileSetGuiNext  = GUI.SelectionGrid(listRect, selecteTileSetGui, listContentTitleSet, 1, listStyle);
			AONGUI.SelectionGrid(vRectSubTile, subTilesetIdx_last, m_tileGroupNames, m_tileGroupNames.Length, listStyleGrid, (int subTilesetIdx) => {
				m_subTilesetIdx = (eSlotAonTypeBrush)subTilesetIdx;
				listContentTitleSet = null;
			});
			yGui += (vRectSubTile.height + 4);
		}
		#endregion
		float bottom_height = 2 * fPad + 32f;
		#region Tileset
		if(m_subTilesetIdx == eSlotAonTypeBrush.High){
			//Pick High
			var vRectSubTile = new Rect(rect.x, yGui + fPad, rect.width, 26);
			AONGUI.SelectionGrid(vRectSubTile, idx_high, str_hight, str_hight.Length, listStyleGrid, (int hight_next) => {
				idx_high = hight_next;
				// m_autoTileMap.BrushGizmo.Clear();
				_autoTileMap.BrushGizmo.RefreshBrushGizmoFromTileset( -2 , idx_high);
			});
			yGui += 32f;
		}else{
			var m_rTilesetRect = new Rect( rect.x + fPad, yGui + fPad, rect.width, rect.height - bottom_height);
			// if(USING_LIST_TILESET)
			{ // List with text
				// Check listContentTitleSet
				if(m_selectedTileIdx == -1){
					selecteTileSetGui = -1;
				}else{
					var slot = _autoTileMap.Tileset.GetSlot(m_selectedTileIdx);
					if(slot.TypeBrush != m_subTilesetIdx){
						// m_subTilesetIdx = (eSlotAonTypeBrush)slot.TypeBrush;
						// listContentTitleSet = null;
						selecteTileSetGui = -1;
					}
				}
				//
				if(listContentTitleSet == null){
					selecteTileSetGui = -1;
					refTileSetGui = new List<int>();
					for (int i = 0; i < _autoTileMap.Tileset.SlotAons.Count; ++i){
						var slot = _autoTileMap.Tileset.SlotAons[i];
						if(slot.TypeBrush == m_subTilesetIdx && slot.Hidden == false){
							refTileSetGui.Add(i);
							if(i == m_selectedTileIdx){
								selecteTileSetGui = refTileSetGui.Count() - 1;
							}
						}
					}
					listContentTitleSet = new GUIContent[refTileSetGui.Count];
					for (int i = 0; i < refTileSetGui.Count; ++i)
					{
						listContentTitleSet[i] = new GUIContent(_autoTileMap.Tileset.SlotAons[refTileSetGui[i]].Name);
					}
				}
				Rect view = m_rTilesetRect;
				view.width += fScrollBarWidth;
				// float fTileRowNb = 32;
				Rect listRect = new Rect(m_rTilesetRect.x, m_rTilesetRect.y, m_rTilesetRect.width, (listContentTitleSet.Length == 0 ? 0 : listStyle.CalcHeight(listContentTitleSet[0], 1.0f) * listContentTitleSet.Length));
				AONGUI.SelectionGrid(listRect, selecteTileSetGui, listContentTitleSet, 1, listStyleGrid, (int selecteTileSetGuiNext) => {
					selecteTileSetGui = selecteTileSetGuiNext;
					m_selectedTileIdx = refTileSetGui[selecteTileSetGui];
					// m_autoTileMap.BrushGizmo.Clear();
					_autoTileMap.BrushGizmo.RefreshBrushGizmoFromTileset( m_selectedTileIdx);
				});
				yGui += listRect.height;
				
			}
		}
		#endregion
		#region Bottom
		AONGUI.Button( new Rect(rect.x, rect.y + rect.height - 32, rect.width, DefineAON.GUI_Height_Button), "Play", () => {
			Play(_autoTileMap.MapIdxSelect, _autoTileMap.MapSelect.StartX, _autoTileMap.MapSelect.StartY );
		});
		yGui += bottom_height;
		#endregion
		// Tileset Version
		/*
		if( m_tilesetSelStart >= 0 && m_tilesetSelEnd >= 0 )
		{
			int tilesetIdxStart = m_tilesetSelStart; // make it relative to selected tileset
			int tilesetIdxEnd = m_tilesetSelEnd; // make it relative to selected tileset
			Rect selRect = new Rect( );
			int TileStartX = tilesetIdxStart % m_autoTileMap.Tileset.AutoTilesPerRow;
			int TileStartY = tilesetIdxStart / m_autoTileMap.Tileset.AutoTilesPerRow;
			int TileEndX = tilesetIdxEnd % m_autoTileMap.Tileset.AutoTilesPerRow;
			int TileEndY = tilesetIdxEnd / m_autoTileMap.Tileset.AutoTilesPerRow;
			selRect.width = (Mathf.Abs(TileEndX - TileStartX) + 1) * k_visualTileWidth;
			selRect.height = (Mathf.Abs(TileEndY - TileStartY) + 1) * k_visualTileHeight;
			float scrX = Mathf.Min(TileStartX, TileEndX) * k_visualTileWidth;
			float scrY = Mathf.Min(TileStartY, TileEndY) * k_visualTileHeight;
			selRect.position = new Vector2( scrX, scrY - m_scrollPos.y );
			selRect.position += m_rTilesetRect.position;
			//selRect.y = ScreenHeight() - selRect.y;
			UtilsGuiDrawing.DrawRectWithOutline( selRect, new Color(0f, 1f, 0f, 0.2f), new Color(0f, 1f, 0f, 1f));
		}
		*/
		
		//---
	}
	
	private void TriggerShowGUI( Trigger trigger, Rect rect, AutoTileMap autoTileMap, TilesetAON tilesetAON, ref bool isShowMoreInfo, AComponent_Button.OnClick onCloseDialog){
		trigger.ShowGUI(new Rect(_rEditorRect.x, _rEditorRect.y, _rEditorRect.width, _rEditorRect.height), _autoTileMap, this, ref isShowMoreInfo, onCloseDialog);
	}

	private void RefreshTileInfo(){
		{
			var slot = _autoTileMap.Tileset.GetSlot(TileShowMoreInfo.Id);
			if(slot.Size.x <= 1 || slot.Size.y <= 1 ){
				// if(slot.TypeLayer == eSlotAonTypeLayer.Overlay){
				// 	m_autoTileMap.m_tileChunkPoolNode.GetTileChunk( TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, (int)slot.TypeLayer).RefreshTileOverlay( TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, true);	
				// }
				return;
			}
			int minX = TileShowMoreInfo.TileX;
			int maxX = TileShowMoreInfo.TileX;
			int minY = TileShowMoreInfo.TileY;
			int maxY = TileShowMoreInfo.TileY;
			while(true){
				minX--;
				if(	minX < 0
					// || minX <= m_tileShowMoreInfo.TileX - (int)slot.Size.x
					|| TileShowMoreInfo.Id != _autoTileMap.GetAutoTile(minX, TileShowMoreInfo.TileY, TileShowMoreInfo.Layer).Id){
					minX++;
					break;
				}
			}
			while(true){
				minY--;
				if(	minY < 0
					// || minY <= m_tileShowMoreInfo.TileY - (int)slot.Size.y
					|| TileShowMoreInfo.Id != _autoTileMap.GetAutoTile(TileShowMoreInfo.TileX, minY, TileShowMoreInfo.Layer).Id){
					minY++;
					break;
				}
			}
			if(minX < TileShowMoreInfo.TileX){
				while(minX < TileShowMoreInfo.TileX){
					minX += (int)slot.Size.x;
				}
				if(minX > TileShowMoreInfo.TileX){
					minX -= (int)slot.Size.x;
				}
			}
			if(minY < TileShowMoreInfo.TileY){
				while(minY < TileShowMoreInfo.TileY){
					minY += (int)slot.Size.y;
				}
				if(minY > TileShowMoreInfo.TileY){
					minY -= (int)slot.Size.y;
				}
			}
			Debug.Log("minXY: " + minX + " | " + minY);
			TileShowMoreInfo = _autoTileMap.GetAutoTile( minX, minY, TileShowMoreInfo.Layer);
		}
		//Show 3d Model
		// {
		// 	var slot = m_autoTileMap.Tileset.GetSlot(TileShowMoreInfo.Id);
		// 	if(slot.TypeLayer == eSlotAonTypeLayer.Overlay){
		// 		m_autoTileMap.m_tileChunkPoolNode.GetTileChunk( TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, (int)slot.TypeLayer).RefreshTileOverlay( TileShowMoreInfo.TileX, TileShowMoreInfo.TileY, true);	
		// 	}
		// }
		
	}

	public void PickPosOnMap(int idMap, int xLast, int yLast, PickMapAON.OnPickMapDelegate OnHadPickMap){
		IEnumerator coroutine = PickPosOnMapASyn(idMap, xLast, yLast, OnHadPickMap);
		StartCoroutine( coroutine);
		// while (coroutine.MoveNext()) ;
	}

	public GameObject pickMapAON;
	public IEnumerator PickPosOnMapASyn(int idMap, int xLast, int yLast, PickMapAON.OnPickMapDelegate OnHadPickMap){
		// yield return null;
		if(idMap != _autoTileMap.MapIdxSelect){
			//Create new map for pick
			TilesetAON tilesetAON = this;
			_autoTileMap.TileChunkPoolNode.gameObject.SetActive(false);
			_autoTileMap.enabled = false;
			_miniMapAON.enabled = false;
			tilesetAON.enabled = false;
			AutoTileMap autoTileMapPickMap = pickMapAON.GetComponent<AutoTileMap>();
			autoTileMapPickMap.LoadMapsData(_autoTileMap.MapsData, idMap);
			pickMapAON.GetComponent<PickMapAON>().SetDragTile( xLast, yLast);
			MonoBehaviour[] objs = pickMapAON.GetComponents<MonoBehaviour>();
			foreach( MonoBehaviour o in objs){
				o.enabled = true;
			}
			pickMapAON.SetActive(true);
			autoTileMapPickMap.ForceReloadMapNow();
			pickMapAON.GetComponent<PickMapAON>().OnHadPickMap = (PickMapAON p, int x, int y) => {
				_autoTileMap.TileChunkPoolNode.gameObject.SetActive(true);
				_autoTileMap.enabled = true;
				_autoTileMap.GetComponent<MiniMapAON>().enabled = true;
				tilesetAON.enabled = true;
				autoTileMapPickMap.DestroyChunk();
				foreach( MonoBehaviour o in objs){
					o.enabled = false;
				}
				pickMapAON.SetActive(false);
				if(OnHadPickMap != null){
					OnHadPickMap( p, x, y);
				}
				RefreshEnableMinimap();
			};
			yield break;
		}else{
			//Reuse current map
			TilesetAON tilesetAON = this;
			PickMapAON pp = null;
			tilesetAON.enabled = false;
			_miniMapAON.enabled = true;
			pp = _autoTileMap.GetComponent<PickMapAON>();
			if(pp == null){
				pp = _autoTileMap.gameObject.AddComponent<PickMapAON>();
			}
			pp.SetDragTile( xLast, yLast);
			pp.OnHadPickMap = (PickMapAON p, int x, int y) => {
				tilesetAON.enabled = true;
				if(OnHadPickMap != null){
					OnHadPickMap( p, x, y);
				}
				Destroy(pp);
				RefreshEnableMinimap();
			};
			yield break;
		}
	}

	public void PickPosOnInterior(int idxInterior, Vector3 pLast, Vector3 cam, OnPickInteriorDelegate _OnPickInterior){
		IsModeEditInterior = true;
		OnPickInterior = _OnPickInterior;
		_autoTileMap.Agent.SetActive(false);
		if(cam != Vector3.zero){
			_autoTileMap.InfoMainCam = cam;
		}
		IEnumerator coroutine = PickPosOnInteriorAsync(idxInterior, pLast);
		StartCoroutine( coroutine);
		// while (coroutine.MoveNext()) ;
	}
	public IEnumerator PickPosOnInteriorAsync(int idxInterior, Vector3 pLast){
		yield return null;
		_autoTileMap.SetModePlay( true, -1);
		_autoTileMap.Agent.SetActive(false);
		_autoTileMap.TileChunkPoolNode.gameObject.SetActive(false);
		_autoTileMap.enabled = false;
		_autoTileMap.GetComponent<MiniMapAON>().enabled = false;
		IEnumerator loadInterior = _autoTileMap.GoToInteriorASync( -1, idxInterior, pLast, false);
		yield return loadInterior;
		// while (loadInterior.MoveNext());
		yield break;
	}

	private void OnGUIInterior(){
		float fPad = 4f;
		float fScrollBarWidth = 16f;
		int tilesWidth = k_visualTileWidth * AutoTilesPerRow;
		float w = tilesWidth + 2 * fPad + fScrollBarWidth;
		_rEditorRect = new Rect(Screen.width - w, 0f, w, 134);
		OnGUIBox( _rEditorRect, "" );
		float yGui = _rEditorRect.y + 4f;
		AONGUI.Label(new Rect(_rEditorRect.x+ 4f, yGui, _rEditorRect.width - 8f, DefineAON.GUI_Height_Label), "Input [Enter] to Apply / [Escape] to Back");
		yGui += 32f;
		AONGUI.Label(new Rect(_rEditorRect.x+ 4f, yGui, _rEditorRect.width - 8f, DefineAON.GUI_Height_Label), "Hold [W] to Warp");
		yGui += 32f;
		AONGUI.Label(new Rect(_rEditorRect.x+ 4f, yGui, _rEditorRect.width - 8f, DefineAON.GUI_Height_Label), "Pos: " + _autoTileMap.Agent.transform.position.ToString());
		yGui += 32f;
		Vector3 v_cam = _autoTileMap.InfoMainCam;
		AONGUI.Label(new Rect(_rEditorRect.x+ 4f, yGui, _rEditorRect.width - 8f, DefineAON.GUI_Height_Label), "Cam: " + v_cam);
		yGui += 32f;
		// if( GUI.Button( new Rect(m_rEditorRect.x, m_rEditorRect.y + m_rEditorRect.height - 64, m_rEditorRect.width, 32), "Apply") )
		if( Input.GetKeyDown(KeyCode.Return ) )
		{	
			var r = _autoTileMap.Agent.transform.position;
			_autoTileMap.CloseInteriorPick();
			_autoTileMap.SetModePlay( false, -1);
			_autoTileMap.enabled = true;
			IsModeEditInterior = false;
			if(OnPickInterior != null){
				OnPickInterior( this, r, v_cam);
				OnPickInterior = null;
			}
			return;
		}
		// if( GUI.Button( new Rect(m_rEditorRect.x, m_rEditorRect.y + m_rEditorRect.height - 32, m_rEditorRect.width, 32), "Back") )
		if( Input.GetKeyDown(KeyCode.Escape ) )
		{
			_autoTileMap.CloseInteriorPick();
			_autoTileMap.SetModePlay( false, -1);
			_autoTileMap.enabled = true;
			// m_autoTileMap.GetComponent<MiniMapAON>().enabled = true;
			IsModeEditInterior = false;
			OnPickInterior = null;
			return;
		}
	}

	public void LoadMap(int idMap, int xLast, int yLast){
		IEnumerator coroutine = LoadMapASyn(idMap, xLast, yLast);
		StartCoroutine( coroutine);
		// while (coroutine.MoveNext()) ;
	}

	public IEnumerator LoadMapASyn(int idMap, int xLast, int yLast){
		yield return null;
		_autoTileMap.BrushGizmo.Clear();
		_autoTileMap.SaveMap();
		_autoTileMap.SetDataMapSelect( idMap);
		_autoTileMap.ForceReloadMapNow();
		yield break;
	}

	private bool IsModePlay {
		get {
			return _autoTileMap.IsPlayMode;
		}
	}
	private int m_lastMapIdEdit = 0;

	public void Play(int idMap, int xLast, int yLast){
		// m_autoTileMap.BrushGizmo.Clear();
		m_lastMapIdEdit = _autoTileMap.MapIdxSelect;
		StartCoroutine(PlayAsync( idMap, xLast, yLast));
	}

	private IEnumerator PlayAsync(int idMap, int xLast, int yLast){
		_autoTileMap.StartPlayMap( idMap, xLast, yLast);
		yield break;
	}

	public void StopPlay(){
		IEnumerator coroutine = StopPlayAsync();
		StartCoroutine( coroutine);
		// while (coroutine.MoveNext()) ;
	}
	
	private IEnumerator StopPlayAsync(){
		yield return null;
		if(!_autoTileMap.CloseInteriorPick()){
			_autoTileMap.SetModePlay( false, m_lastMapIdEdit);
			_autoTileMap.enabled = true;
			yield return null;
		}
		yield break;
	}
}
