using System.Collections;
using System.Collections.Generic;
using AON.RpgMapEditor;
using UnityEngine;

public class PickMapAON : AONGUIBehaviour {
	
	const int k_visualTileWidth = 32; // doesn't matter the tileset tile size, this size will be used to paint it in the inspector
	const int k_visualTileHeight = 32;

	private AutoTileMap m_autoTileMap;
	private MiniMapAON m_miniMapAON;
	private Camera2DController m_camera2D;

	private Rect m_rEditorRect;

	private bool m_isInitialized = false;
	
	private enum eEditorWindow
	{
		NONE,
		TOOLS,
		MAPVIEW
	}
	private eEditorWindow m_focusWindow;

	public delegate void OnPickMapDelegate( PickMapAON p, int x, int y);
	public OnPickMapDelegate OnHadPickMap;

	public void Init() 
	{
		m_autoTileMap = GetComponent<AutoTileMap>();
		m_miniMapAON = GetComponent<MiniMapAON>();

		if( m_autoTileMap != null )
		{
			m_isInitialized = true;

			if( m_autoTileMap.ViewCamera == null )
			{
				Debug.LogWarning( "AutoTileMap has no ViewCamera set. Camera.main will be set as ViewCamera" );
				m_autoTileMap.ViewCamera = Camera.main;
			}
			m_camera2D = m_autoTileMap.ViewCamera.GetComponent<Camera2DController>();

			if( m_camera2D == null )
			{
				m_camera2D = m_autoTileMap.ViewCamera.gameObject.AddComponent<Camera2DController>();
			}
		}
	}
	private int m_startDragTileX = 0;
	private int m_startDragTileY = 0;
	private int m_dragTileX = 0;
	private int m_dragTileY = 0;
	// private Vector3 m_mousePrevPos;
	public void SetDragTile( int x, int y){
		m_startDragTileX = m_dragTileX = x;
		m_startDragTileY = m_dragTileY = y;
	}

	public override void Update() 
	{
		
		base.Update();

		if( !m_isInitialized )
		{
			Init();
			return;
		}
		
		bool isMouseLeft = Input.GetMouseButton(0);
		bool isMouseRight = Input.GetMouseButton(1);
		
		Vector3 vGuiMouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

		bool isFocusMiniMap = ( m_miniMapAON != null && m_miniMapAON.IsInitialized && (m_miniMapAON.IsFocus|| m_miniMapAON.MinimapRect.Contains( vGuiMouse )));
		//+++ Set window with focus
		if( !isMouseLeft )
		{
			if( m_rEditorRect.Contains( vGuiMouse ) )
			{
				m_focusWindow = eEditorWindow.TOOLS;
			}
			else if( isFocusMiniMap)
			{
				m_focusWindow = eEditorWindow.NONE;
			}
			else
			{
				m_focusWindow = eEditorWindow.MAPVIEW;
			}
		}

		if( isFocusMiniMap){
			//Do NotThing
		}
		else if( m_rEditorRect.Contains( vGuiMouse ) )
		{
		}
		else if( m_focusWindow == eEditorWindow.MAPVIEW )
		{
			Vector3 vWorldMousePos = m_autoTileMap.ViewCamera.ScreenToWorldPoint( new Vector3(Input.mousePosition.x, Input.mousePosition.y) );
			// m_autoTileMap.BrushGizmo.UpdateBrushGizmo( vWorldMousePos );

			if( isMouseRight || isMouseLeft )
			{
				var mousePosition = Input.mousePosition;
				mousePosition = m_autoTileMap.ViewCamera.ScreenToWorldPoint(mousePosition);
				int _x = (int)(mousePosition.x / m_autoTileMap.CellSize.x);
				int _y = (int)(-mousePosition.y / m_autoTileMap.CellSize.y);
				if ( m_autoTileMap.IsValidAutoTilePos(_x, _y))
				{
					int tile_x = _x;
					int tile_y = _y;
					m_startDragTileX = m_dragTileX = tile_x;
					m_startDragTileY = m_dragTileY = tile_y;
					AONGUI_ReDrawAll();
				}
			}
		}
	}

	string[] m_tileGroupNames;
	private int m_subTilesetIdx = 0;

	private Vector2 m_scrollPos = Vector2.zero;

	protected override void OnGUIAON()
	{
		if( !m_isInitialized )
		{
			return;
		}
		
		float fPad = 4f;
		float fScrollBarWidth = 16f;
		int tilesWidth = k_visualTileWidth * TilesetAON.AutoTilesPerRow;
		float w = tilesWidth + 2 * fPad + fScrollBarWidth;
		m_rEditorRect = new Rect(Screen.width - w, 0f, w, ScreenHeight() - w);
		AONGUI.Box( m_rEditorRect, "Pick position" );
		OnGUITileSet();
	}

	void OnGUITileSet(){
		if( m_startDragTileX != -1 && m_startDragTileY != -1)
		{
			Rect selRect = new Rect( );
			selRect.width = (Mathf.Abs(m_dragTileX - m_startDragTileX) + 1) * m_camera2D.Zoom * m_autoTileMap.CellSize.x * AutoTileset.PixelToUnits;
			selRect.height = (Mathf.Abs(m_dragTileY - m_startDragTileY) + 1) * m_camera2D.Zoom * m_autoTileMap.CellSize.y * AutoTileset.PixelToUnits;
			float worldX = Mathf.Min(m_startDragTileX, m_dragTileX) * m_autoTileMap.CellSize.x;
			float worldY = -Mathf.Min(m_startDragTileY, m_dragTileY) * m_autoTileMap.CellSize.y;                
			Vector3 vScreen = m_camera2D.Camera.WorldToScreenPoint(new Vector3(worldX, worldY) + m_autoTileMap.transform.position);

			//NOTE: vScreen will vibrate if the camera has KeepInsideMapBounds enabled and because of the zoom out, the camera area is bigger than camera limit bounds
			selRect.position = new Vector2( vScreen.x, vScreen.y );

			selRect.y = ScreenHeight() - selRect.y;
			UtilsGuiDrawing.DrawRectWithOutline( selRect, new Color(0f, 1f, 0f, 0.2f), new Color(0f, 1f, 0f, 1f));

			//
			float yGui = m_rEditorRect.y + 32f;
			AONGUI.Label(new Rect(m_rEditorRect.x+ 4f, yGui + 4f, m_rEditorRect.width - 8f, 32 - 8f), "Map: " + m_autoTileMap.MapIdxSelect);
			yGui += 16f;
			AONGUI.Label(new Rect(m_rEditorRect.x+ 4f, yGui + 4f, m_rEditorRect.width - 8f, 32 - 8f), "x: " + m_startDragTileX);
			yGui += 16f;
			AONGUI.Label(new Rect(m_rEditorRect.x+ 4f, yGui + 4f, m_rEditorRect.width - 8f, 32 - 8f), "y: " + m_startDragTileY);
			yGui += 32f;
			//
			AONGUI.Button( new Rect(m_rEditorRect.x, yGui, m_rEditorRect.width, 32), "Accept", () => {
				if(OnHadPickMap != null){
					OnHadPickMap( this, m_startDragTileX, m_startDragTileY);
				}
			});
		}
	}

	private int ScreenWidth(){
		return Screen.width;
	}

	private int ScreenHeight(){
		return Screen.height;
	}
}