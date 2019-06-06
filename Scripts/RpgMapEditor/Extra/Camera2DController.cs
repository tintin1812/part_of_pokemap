using UnityEngine;
using System.Collections;

namespace AON.RpgMapEditor
{
	[RequireComponent(typeof(Camera))]
	public class Camera2DController : MonoBehaviour {


		public Camera Camera{ get; private set; }

		public float Zoom = 50f;
		public float PixelToUnits = 1f;
        public bool KeepInsideMapBounds = true;

        // private Rect m_boundingBox;

		void Start () 
		{
			Camera = GetComponent<Camera>();
            // m_boundingBox = new Rect();
            // AutoTileMap autoTileMap = CreativeSpore.RpgMapEditor.AutoTileMap_Editor.Instance;
            // m_boundingBox.width = autoTileMap.MapTileWidth * autoTileMap.CellSize.x;
            // m_boundingBox.height = autoTileMap.MapTileHeight * autoTileMap.CellSize.y;
            // m_boundingBox.x = autoTileMap.transform.position.x;
            // m_boundingBox.y = autoTileMap.transform.position.y;
            m_vCamRealPos = Camera.transform.position;
		}
		
		Vector3 m_vCamRealPos = Vector3.zero;
        void LateUpdate()
        {
            //Note: ViewCamera.orthographicSize is not a real zoom based on pixels. This is the formula to calculate the real zoom.
            Camera.orthographicSize = (Camera.pixelRect.height) / (2f * Zoom * PixelToUnits);
            Vector3 vOri = Camera.ScreenPointToRay(Vector3.zero).origin;

            m_vCamRealPos = Camera.transform.position;
            Vector3 vPos = Camera.transform.position;
            float mod = (1f / (Zoom * PixelToUnits));
            vPos.x -= vOri.x % mod;
            vPos.y -= vOri.y % mod;
            vPos.z = -10;
            Camera.transform.position = vPos;

            if (KeepInsideMapBounds && RpgMapEditor.AutoTileMap_Editor.Instance.isActiveAndEnabled)
            {
                // DoKeepInsideMapBounds();
                DoKeepInsideMapBoundsAdd1Cell();
            }
        }

        /*
        void DoKeepInsideMapBounds()
        {
            float fPad = 4f;
            float fScrollBarWidth = 16f;
            float tilesWidth = 32 * 8;
            float minimapRectW = tilesWidth + 2 * fPad + fScrollBarWidth;

            Rect rCamera = new Rect();
            rCamera.width = (Screen.width - minimapRectW) / (PixelToUnits * Zoom);
            rCamera.height = Screen.height / (PixelToUnits * Zoom);
            var p = Camera.transform.position;
            float offsetX = (minimapRectW / (PixelToUnits * Zoom));
            p.x = p.x - offsetX *0.5f;
            rCamera.center = p;

            Rect rMap = new Rect();
            AutoTileMap autoTileMap = CreativeSpore.RpgMapEditor.AutoTileMap_Editor.Instance;
            rMap.width = autoTileMap.MapTileWidth * autoTileMap.CellSize.x;
            rMap.height = autoTileMap.MapTileHeight * autoTileMap.CellSize.y;
            rMap.x = autoTileMap.transform.position.x;
            rMap.y = autoTileMap.transform.position.y;

            rMap.y -= rMap.height;

            Vector3 vOffset = Vector3.zero;

            // CreativeSpore.RpgMapEditor.RpgMapHelper.DebugDrawRect(Vector3.zero, rCamera, Color.cyan);
            // CreativeSpore.RpgMapEditor.RpgMapHelper.DebugDrawRect(Vector3.zero, rMap, Color.green);
            // if(rMap.width <= rCamera.width && rMap.height <= rCamera.height)
            //     return;
            float right = (rCamera.x < rMap.x) ? rMap.x - rCamera.x : 0;
            float left = (rCamera.xMax > rMap.xMax) ? rMap.xMax - rCamera.xMax : 0;
            float down = (rCamera.y < rMap.y) ? rMap.y - rCamera.y : 0f;
            float up = (rCamera.yMax > rMap.yMax) ? rMap.yMax - rCamera.yMax : 0f;
            if(right == 0 || left == 0)
                vOffset.x = (right != 0f && left != 0f) ? rMap.center.x - Camera.transform.position.x : right + left;
            if(down == 0 || up == 0)
                vOffset.y = (down != 0f && up != 0f) ? rMap.center.y - Camera.transform.position.y : up + down;

            Camera.transform.position += vOffset;
            m_vCamRealPos += vOffset;
        }
        */
        
        void DoKeepInsideMapBoundsAdd1Cell()
        {
            float fPad = 4f;
            float fScrollBarWidth = 16f;
            float tilesWidth = 32 * 8;
            float minimapRectW = tilesWidth + 2 * fPad + fScrollBarWidth;

            Rect rCamera = new Rect();
            rCamera.width = (Screen.width - minimapRectW) / (PixelToUnits * Zoom);
            rCamera.height = Screen.height / (PixelToUnits * Zoom);
            var p = Camera.transform.position;
            float offsetX = (minimapRectW / (PixelToUnits * Zoom));
            p.x = p.x - offsetX * 0.5f;
            rCamera.center = p;

            Rect rMap = new Rect();
            AutoTileMap autoTileMap = RpgMapEditor.AutoTileMap_Editor.Instance;
            rMap.width = autoTileMap.MapTileWidth * autoTileMap.CellSize.x + 2;
            rMap.height = autoTileMap.MapTileHeight * autoTileMap.CellSize.y + 2;
            rMap.x = autoTileMap.transform.position.x - 1;
            rMap.y = autoTileMap.transform.position.y + 1;

            rMap.y -= rMap.height;

            Vector3 vOffset = Vector3.zero;

            // CreativeSpore.RpgMapEditor.RpgMapHelper.DebugDrawRect(Vector3.zero, rCamera, Color.cyan);
            // CreativeSpore.RpgMapEditor.RpgMapHelper.DebugDrawRect(Vector3.zero, rMap, Color.green);
            // if(rMap.width <= rCamera.width && rMap.height <= rCamera.height)
            //     return;
            float right = (rCamera.x < rMap.x) ? rMap.x - rCamera.x : 0;
            float left = (rCamera.xMax > rMap.xMax) ? rMap.xMax - rCamera.xMax : 0;
            float down = (rCamera.y < rMap.y) ? rMap.y - rCamera.y : 0f;
            float up = (rCamera.yMax > rMap.yMax) ? rMap.yMax - rCamera.yMax : 0f;
            if(right == 0 || left == 0)
                vOffset.x = (right != 0f && left != 0f) ? rMap.center.x - Camera.transform.position.x : right + left;
            if(down == 0 || up == 0)
                vOffset.y = (down != 0f && up != 0f) ? rMap.center.y - Camera.transform.position.y : up + down;

            Camera.transform.position += vOffset;
            m_vCamRealPos += vOffset;
        }

        // void DoKeepInsideBounds()
        // {
        //     Rect rCamera = new Rect();
        //     rCamera.width = Screen.width / (PixelToUnits * Zoom);
        //     rCamera.height = Screen.height / (PixelToUnits * Zoom);
        //     rCamera.center = Camera.transform.position;

        //     Vector3 vOffset = Vector3.zero;
        //     Rect rBoundingBox = m_boundingBox;
        //     rBoundingBox.y -= rBoundingBox.height;

        //     float right = (rCamera.x < rBoundingBox.x) ? rBoundingBox.x - rCamera.x : 0f;
        //     float left = (rCamera.xMax > rBoundingBox.xMax) ? rBoundingBox.xMax - rCamera.xMax : 0f;
        //     float down = (rCamera.y < rBoundingBox.y) ? rBoundingBox.y - rCamera.y : 0f;
        //     float up = (rCamera.yMax > rBoundingBox.yMax) ? rBoundingBox.yMax - rCamera.yMax : 0f;

        //     vOffset.x = (right != 0f && left != 0f) ? rBoundingBox.center.x - Camera.transform.position.x : right + left;
        //     vOffset.y = (down != 0f && up != 0f) ? rBoundingBox.center.y - Camera.transform.position.y : up + down;

        //     Camera.transform.position += vOffset;
        // }

		void OnPostRender()
		{
            if(Camera != null){
                Camera.transform.position = m_vCamRealPos;
            }
		}
	}
}
