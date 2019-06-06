using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AON.RpgMapEditor;
using UnityEngine;

public class MiniMapAON : AONGUIBehaviour {

	// const int k_visualTileWidth = 32; // doesn't matter the tileset tile size, this size will be used to paint it in the inspector
	// const int k_visualTileHeight = 32;
	private AutoTileMap m_autoTileMap;
	private Camera2DController m_camera2D;
	private Rect m_rMinimapRect;
	public Rect MinimapRect { get { return m_rMinimapRect; } }
	private bool m_isInitialized = false;
	public bool IsInitialized { get { return m_isInitialized; } }
	private bool m_focus = false;
	public bool IsFocus { get { return m_focus; } }
	private float countMoveMap = 0.5f;
	public void Init()
	{
		m_autoTileMap = GetComponent<AutoTileMap>();

		if( m_autoTileMap != null && m_autoTileMap.IsInitialized )
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
			//MinimapEnabled
			// m_autoTileMap.BrushGizmo.gameObject.SetActive(true);
			m_autoTileMap.RefreshMinimapTexture();
		}
	}

	public override void Update() 
	{
		base.Update();
		
		if( !m_isInitialized )
		{
			Init();
			return;
		}
		if(InputFieldHelper.Instance.IsShowNoti()){
			return;
		}
		
		bool isMouseLeft = Input.GetMouseButton(0);
		
		Vector3 vGuiMouse = new Vector2(Input.mousePosition.x, ScreenHeight() - Input.mousePosition.y);
		
		if( Input.GetMouseButtonDown(0)){
			if( m_rMinimapRect.Contains( vGuiMouse ) )
			{
				m_focus = true;
				// Debug.Log("ButtonDown");
			}
		}
		if( Input.GetMouseButtonUp(0)){
			if( m_focus)
			{
				m_focus = false;
				// Debug.Log("ButtonUp");
			}
		}

		// if(m_focus && m_rMinimapRect.Contains( vGuiMouse ))
		if(m_focus)
		{
			if( isMouseLeft )
			{
				float minimapScale = m_rMinimapRect.width / m_autoTileMap.MinimapTexture.width;
				Vector3 vPos = vGuiMouse - new Vector3( m_rMinimapRect.position.x, m_rMinimapRect.position.y);
				vPos.y = -vPos.y;
				vPos.x *= m_autoTileMap.CellSize.x / minimapScale;
				vPos.y *= m_autoTileMap.CellSize.y / minimapScale;
				vPos.z = m_camera2D.transform.position.z;
				m_camera2D.transform.position = vPos;
				AONGUIBehaviour.AONGUI_ReDrawAll();
			}
		}
		countMoveMap -=  Time.deltaTime;
		if(countMoveMap > 0){
			return;
		}
		countMoveMap = 0.05f;
		if( Input.GetKey(KeyCode.W )){
			if(!Input.GetKey(KeyCode.S )){
				var p = m_camera2D.transform.position;
				p.y += (m_autoTileMap.CellSize.y);
				m_camera2D.transform.position = p;
				AONGUIBehaviour.AONGUI_ReDrawAll();
			}
		}else if(Input.GetKey(KeyCode.S )){
			var p = m_camera2D.transform.position;
			p.y -= (m_autoTileMap.CellSize.y);
			m_camera2D.transform.position = p;
			AONGUIBehaviour.AONGUI_ReDrawAll();
		}
		if( Input.GetKey(KeyCode.A )){
			if(!Input.GetKey(KeyCode.D )){
				var p = m_camera2D.transform.position;
				p.x -= (m_autoTileMap.CellSize.x);
				m_camera2D.transform.position = p;
				AONGUIBehaviour.AONGUI_ReDrawAll();
			}
		}else if(Input.GetKey(KeyCode.D )){
			var p = m_camera2D.transform.position;
			p.x += (m_autoTileMap.CellSize.x);
			m_camera2D.transform.position = p;
			AONGUIBehaviour.AONGUI_ReDrawAll();
		}
	}

	private Vector2 m_scrollPos = Vector2.zero;

	protected override void OnGUIAON()
	{
		if( !m_isInitialized )
		{
			return;
		}
		// int tilesWidth = k_visualTileWidth * m_autoTileMap.Tileset.AutoTilesPerRow;
		// int tilesHeight = k_visualTileHeight * (256 / m_autoTileMap.Tileset.AutoTilesPerRow);

		// var m_rEditorRect = new Rect(0f, 0f, tilesWidth+2*fPad + fScrollBarWidth, ScreenHeight());
		// var m_rMapViewRect = new Rect( m_rEditorRect.x + m_rEditorRect.width, 0f, ScreenWidth() - m_rEditorRect.width, ScreenHeight());
		// float minimapRectW = Mathf.Min(m_rMapViewRect.width * 0.25f, m_autoTileMap.MinimapTexture.width);  // fix to limit the size of minimap for big maps

		float fPad = 4f;
		float fScrollBarWidth = 16f;
		float tilesWidth = 32 * 8;
		float minimapRectW = tilesWidth + 2 * fPad + fScrollBarWidth;
		// float minimapRectW = ScreenWidth() * 0.2f;
		float minimapRectH = m_autoTileMap.MinimapTexture.height * minimapRectW / m_autoTileMap.MinimapTexture.width;
		float fOffset = 18f;
		var RectBox = new Rect(ScreenWidth() - minimapRectW, ScreenHeight() - minimapRectH, minimapRectW, minimapRectH);
		m_rMinimapRect = new Rect(RectBox.x + fOffset, RectBox.y + fOffset * 1.75f, RectBox.width - 2 * fOffset, RectBox.height - 2 * fOffset);

		float minimapScale = m_rMinimapRect.width / m_autoTileMap.MinimapTexture.width;
		//NOTE: the texture is drawn blurred in web player unless default quality is set to Fast in project settings
		// see here for solution http://forum.unity3d.com/threads/webplayer-gui-issue.100256/#post-868451
		// UtilsGuiDrawing.DrawRectWithOutline( m_rMinimapRect, new Color(0, 0, 0, 0), Color.black );
		AONGUI.Box( RectBox, "Mini Map (W,A,S,D to move)" );
		AONGUI.DrawTexture( m_rMinimapRect, m_autoTileMap.MinimapTexture );

		// Draw camera region on minimap
		Vector3 vCameraPos = m_autoTileMap.ViewCamera.ScreenPointToRay(new Vector3(-m_rMinimapRect.width, ScreenHeight()-1)).origin;
		float camTileX = (vCameraPos.x / m_autoTileMap.CellSize.x);
		float camTileY = (-vCameraPos.y / m_autoTileMap.CellSize.y);
		Rect rMinimapCam = new Rect(camTileX, camTileY, minimapScale * ScreenWidth() / (m_camera2D.Zoom * m_autoTileMap.CellSize.x * AutoTileset.PixelToUnits), minimapScale * ScreenHeight() / (m_camera2D.Zoom * m_autoTileMap.CellSize.y * AutoTileset.PixelToUnits));
		rMinimapCam.position *= minimapScale;
		rMinimapCam.position += m_rMinimapRect.position;
		//Clamp rMinimapCam
		if(rMinimapCam.xMin < m_rMinimapRect.x)
			rMinimapCam.xMin = m_rMinimapRect.x;
		if( rMinimapCam.xMax > m_rMinimapRect.xMax)
			rMinimapCam.xMax = m_rMinimapRect.xMax;
		if(rMinimapCam.yMin < m_rMinimapRect.y)
			rMinimapCam.yMin = m_rMinimapRect.y;
		if( rMinimapCam.yMax > m_rMinimapRect.yMax)
			rMinimapCam.yMax = m_rMinimapRect.yMax;
		UtilsGuiDrawing.DrawRectWithOutline( rMinimapCam, new Color(0, 0, 0, 0), Color.white );
	}

	/*
	private void DrawAlphaBackground(Vector2 offset)
	{
		// texture to draw behind any alpha tiles
		var thumbnailBackground = MakeTileTex(32, 32);
		var thumbIdx = 0;
		for (var y = 0; thumbIdx < 256; ++y) //256 number of tileset for each tileset group
		{
			for (var x = 0; x < m_autoTileMap.Tileset.AutoTilesPerRow; ++x, ++thumbIdx)
			{
				var rDst = new Rect(x * k_visualTileWidth, y * k_visualTileHeight, k_visualTileWidth,
					k_visualTileHeight);
				rDst.position += offset;
				//if (m_autoTileMap.IsAutoTileHasAlpha(thumbIdx))
				{
					GUI.DrawTexture(rDst, thumbnailBackground);
				}
			}
		}
	}

	private static Texture2D MakeTileTex(int width, int height)
	{
		// checkerboard pixel colors
		var darkTile = new Color32(220, 220, 220, 255);
		var lightTile = new Color(1f, 1f, 1f);

		// color array for entire texture
		Color[] pix = new Color[width * height];

		// create a checkboard pattern using our dark and light colors
		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var index = (y * height) + x;

				if (y < height / 2)
				{
					if (x < width / 2)
					{
						pix[index] = darkTile;
					}
					else
					{
						pix[index] = lightTile;
					}
				}
				else
				{
					if (x < width / 2)
					{
						pix[index] = lightTile;
					}
					else
					{
						pix[index] = darkTile;
					}
				}
			}
		}

		var result = new Texture2D(width, height);
		result.SetPixels(pix);
		result.Apply();

		return result;
	}
	*/

	private int ScreenWidth(){
		return Screen.width;
	}

	private int ScreenHeight(){
		return Screen.height;
	}
}
