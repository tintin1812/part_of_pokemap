using UnityEngine;
using System.Collections;
using System.Linq;
using System.IO;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AON.RpgMapEditor
{
	public class UtilsAutoTileMap 
	{

		private static void _FillSpritesFromRect( List<Rect> _outList, AutoTileset autoTileset, int x, int y, int width, int height )
		{

            Rect srcRect = new Rect(0, 0, autoTileset.TileWidth, autoTileset.TileHeight);
            for (srcRect.y = height - autoTileset.TileHeight; srcRect.y >= 0; srcRect.y -= autoTileset.TileHeight)
			{
                for (srcRect.x = 0; srcRect.x < width; srcRect.x += autoTileset.TileWidth)
				{
					Rect sprRect = srcRect;
					sprRect.x += x;
					sprRect.y += y;
                    _outList.Add(sprRect);
				}
			}
		}

        /// <summary>
        /// Generate a tileset atlas
        /// </summary>
        /// <param name="autoTileset"></param>
        /// <param name="hSlots"></param>
        /// <param name="vSlots"></param>
        /// <returns></returns>
		// public static Texture2D GenerateAtlas( AutoTileset autoTileset, int hSlots, int vSlots )
		// {
        //     Debug.Log("GenerateAtlas");
        //     int w = hSlots * autoTileset.TilesetSlotSize;
        //     int h = vSlots * autoTileset.TilesetSlotSize;
		// 	Texture2D atlasTexture = new Texture2D(w, h);
		// 	Color32[] atlasColors = Enumerable.Repeat<Color32>( new Color32(0, 0, 0, 0) , w*h).ToArray();
		// 	atlasTexture.SetPixels32(atlasColors);
		// 	atlasTexture.Apply();
		// 	return atlasTexture;
		// }

        /// <summary>
        /// Clear an area of the atlas texture
        /// </summary>
        /// <param name="atlasTexture"></param>
        /// <param name="dstX"></param>
        /// <param name="dstY"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void ClearAtlasArea(Texture2D atlasTexture, int dstX, int dstY, int width, int height)
        {
            Color[] atlasColors = Enumerable.Repeat<Color>(new Color(0f, 0f, 0f, 0f), width * height).ToArray();
            atlasTexture.SetPixels(dstX, dstY, width, height, atlasColors);
            atlasTexture.Apply();
        }

        /// <summary>
        /// Import the texture making sure the texture import settings are properly set
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
		public static bool ImportTexture( Texture2D texture )
		{
	#if UNITY_EDITOR
			if( texture != null )
			{
				return ImportTexture( AssetDatabase.GetAssetPath(texture) );
			}
	#endif
			return false;
		}        

        /// <summary>
        /// Import the texture making sure the texture import settings are properly set
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
		public static bool ImportTexture( string path )
		{
	#if UNITY_EDITOR
			if( path.Length > 0 )
			{
				TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter; 
				if( textureImporter )
				{
                    textureImporter.alphaIsTransparency = true; // default
                    textureImporter.anisoLevel = 1; // default
                    textureImporter.borderMipmap = false; // default
                    textureImporter.mipmapEnabled = false; // default
                    textureImporter.compressionQuality = 100;
					textureImporter.isReadable = true;
					textureImporter.spritePixelsPerUnit = AutoTileset.PixelToUnits;                    
					//textureImporter.spriteImportMode = SpriteImportMode.None;
					textureImporter.wrapMode = TextureWrapMode.Clamp;
					textureImporter.filterMode = FilterMode.Point;
                    textureImporter.npotScale = TextureImporterNPOTScale.None;
#if UNITY_5_5_OR_NEWER
                    textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                    //textureImporter.textureType = TextureImporterType.Default;
#else
                    textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                    //textureImporter.textureType = TextureImporterType.Advanced;
#endif
                    textureImporter.maxTextureSize = AutoTileset.k_MaxTextureSize;                    
					AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate); 
				}
				return true;
			}
	#endif
			return false;
		}
		
		// private static void _CopyBuildingThumbnails( AutoTileset autoTileset, Texture2D tilesetTex, int dstX, int dstY )
		// {
		// 	if( tilesetTex != null )
		// 	{
        //         Rect srcRect = new Rect(0, 0, autoTileset.TilePartWidth, autoTileset.TilePartWidth);
        //         Rect dstRect = new Rect(0, 0, autoTileset.TileWidth, autoTileset.TileHeight);
        //         for (dstRect.y = dstY, srcRect.y = 0; dstRect.y < (dstY + 4 * autoTileset.TileHeight); dstRect.y += autoTileset.TileHeight, srcRect.y += 2 * autoTileset.TileHeight)
		// 		{
        //             for (dstRect.x = dstX, srcRect.x = 0; dstRect.x < dstX + autoTileset.AutoTilesPerRow * autoTileset.TileWidth; dstRect.x += autoTileset.TileWidth, srcRect.x += 2 * autoTileset.TileWidth)
		// 			{
		// 				Color[] thumbnailPartColors;
		// 				thumbnailPartColors = tilesetTex.GetPixels( Mathf.RoundToInt(srcRect.x), Mathf.RoundToInt(srcRect.y), Mathf.RoundToInt(srcRect.width), Mathf.RoundToInt(srcRect.height));
        //                 autoTileset.AtlasTexture.SetPixels(Mathf.RoundToInt(dstRect.x), Mathf.RoundToInt(dstRect.y), Mathf.RoundToInt(srcRect.width), Mathf.RoundToInt(srcRect.height), thumbnailPartColors);

        //                 thumbnailPartColors = tilesetTex.GetPixels(Mathf.RoundToInt(srcRect.x) + 3 * autoTileset.TilePartWidth, Mathf.RoundToInt(srcRect.y), Mathf.RoundToInt(srcRect.width), Mathf.RoundToInt(srcRect.height));
        //                 autoTileset.AtlasTexture.SetPixels(Mathf.RoundToInt(dstRect.x) + autoTileset.TilePartWidth, Mathf.RoundToInt(dstRect.y), Mathf.RoundToInt(srcRect.width), Mathf.RoundToInt(srcRect.height), thumbnailPartColors);

        //                 thumbnailPartColors = tilesetTex.GetPixels(Mathf.RoundToInt(srcRect.x), Mathf.RoundToInt(srcRect.y) + 3 * autoTileset.TilePartHeight, Mathf.RoundToInt(srcRect.width), Mathf.RoundToInt(srcRect.height));
        //                 autoTileset.AtlasTexture.SetPixels(Mathf.RoundToInt(dstRect.x), Mathf.RoundToInt(dstRect.y) + autoTileset.TilePartHeight, Mathf.RoundToInt(srcRect.width), Mathf.RoundToInt(srcRect.height), thumbnailPartColors);

        //                 thumbnailPartColors = tilesetTex.GetPixels(Mathf.RoundToInt(srcRect.x) + 3 * autoTileset.TilePartWidth, Mathf.RoundToInt(srcRect.y) + 3 * autoTileset.TilePartHeight, Mathf.RoundToInt(srcRect.width), Mathf.RoundToInt(srcRect.height));
        //                 autoTileset.AtlasTexture.SetPixels(Mathf.RoundToInt(dstRect.x) + autoTileset.TilePartWidth, Mathf.RoundToInt(dstRect.y) + autoTileset.TilePartHeight, Mathf.RoundToInt(srcRect.width), Mathf.RoundToInt(srcRect.height), thumbnailPartColors);
						
		// 			}
		// 		}
		// 	}
		// }

        // private static void _CopyWallThumbnails(AutoTileset autoTileset, Texture2D tilesetTex, int dstX, int dstY)
		// {
		// 	if( tilesetTex != null )
		// 	{
        //         Rect srcRect = new Rect(0, 3 * autoTileset.TileHeight, autoTileset.TilePartWidth, autoTileset.TilePartWidth);
        //         Rect dstRect = new Rect(0, 0, autoTileset.TileWidth, autoTileset.TileHeight);
        //         for (dstRect.y = dstY, srcRect.y = 0; dstRect.y < (dstY + 3 * autoTileset.TileHeight); dstRect.y += autoTileset.TileHeight, srcRect.y += 5 * autoTileset.TileHeight)
		// 		{
        //             for (dstRect.x = dstX, srcRect.x = 0; dstRect.x < dstX + autoTileset.AutoTilesPerRow * autoTileset.TileWidth; dstRect.x += autoTileset.TileWidth, srcRect.x += 2 * autoTileset.TileWidth)
		// 			{
		// 				Color[] thumbnailPartColors;
		// 				thumbnailPartColors = tilesetTex.GetPixels( Mathf.RoundToInt(srcRect.x), Mathf.RoundToInt(srcRect.y), Mathf.RoundToInt(srcRect.width), Mathf.RoundToInt(srcRect.height));
        //                 autoTileset.AtlasTexture.SetPixels(Mathf.RoundToInt(dstRect.x), Mathf.RoundToInt(dstRect.y), Mathf.RoundToInt(srcRect.width), Mathf.RoundToInt(srcRect.height), thumbnailPartColors);

        //                 thumbnailPartColors = tilesetTex.GetPixels(Mathf.RoundToInt(srcRect.x) + 3 * autoTileset.TilePartWidth, Mathf.RoundToInt(srcRect.y), Mathf.RoundToInt(srcRect.width), Mathf.RoundToInt(srcRect.height));
        //                 autoTileset.AtlasTexture.SetPixels(Mathf.RoundToInt(dstRect.x) + autoTileset.TilePartWidth, Mathf.RoundToInt(dstRect.y), Mathf.RoundToInt(srcRect.width), Mathf.RoundToInt(srcRect.height), thumbnailPartColors);

        //                 thumbnailPartColors = tilesetTex.GetPixels(Mathf.RoundToInt(srcRect.x), Mathf.RoundToInt(srcRect.y) + 3 * autoTileset.TilePartHeight, Mathf.RoundToInt(srcRect.width), Mathf.RoundToInt(srcRect.height));
        //                 autoTileset.AtlasTexture.SetPixels(Mathf.RoundToInt(dstRect.x), Mathf.RoundToInt(dstRect.y) + autoTileset.TilePartHeight, Mathf.RoundToInt(srcRect.width), Mathf.RoundToInt(srcRect.height), thumbnailPartColors);

        //                 thumbnailPartColors = tilesetTex.GetPixels(Mathf.RoundToInt(srcRect.x) + 3 * autoTileset.TilePartWidth, Mathf.RoundToInt(srcRect.y) + 3 * autoTileset.TilePartHeight, Mathf.RoundToInt(srcRect.width), Mathf.RoundToInt(srcRect.height));
        //                 autoTileset.AtlasTexture.SetPixels(Mathf.RoundToInt(dstRect.x) + autoTileset.TilePartWidth, Mathf.RoundToInt(dstRect.y) + autoTileset.TilePartHeight, Mathf.RoundToInt(srcRect.width), Mathf.RoundToInt(srcRect.height), thumbnailPartColors);
						
		// 			}
		// 		}
		// 	}
		// }
		
        private static void _CopyTilesetInAtlas( Texture2D atlasTexture, Texture2D tilesetTex, Rect rect )
        {
            _CopyTilesetInAtlas(atlasTexture, tilesetTex, (int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        }

		private static void _CopyTilesetInAtlas( Texture2D atlasTexture, Texture2D tilesetTex, int dstX, int dstY, int width, int height )
		{
			Color[] atlasColors;
			if( tilesetTex == null )
			{
				atlasColors = Enumerable.Repeat<Color>( new Color(0f, 0f, 0f, 0f) , width*height).ToArray();
			}
			else
			{
				atlasColors = tilesetTex.GetPixels();
			}
			
			atlasTexture.SetPixels( dstX, dstY, width, height, atlasColors);
		}

        public static Texture2D GenerateThumb( AutoTileset autoTileset, SlotAon slotAON )
		{
            int x = Mathf.RoundToInt(slotAON.AtlasRecThumb.x);
            int y = Mathf.RoundToInt(slotAON.AtlasRecThumb.y);
            int w = Mathf.RoundToInt(slotAON.AtlasRecThumb.width);
            int h = Mathf.RoundToInt(slotAON.AtlasRecThumb.height);
            Texture2D tilesetTexture = new Texture2D( w, h, TextureFormat.ARGB32, false );
            tilesetTexture.filterMode = FilterMode.Point;
			// Color[] autotileColors = autoTileset.PixelsThumb( slotAON).GetPixels(x, y, w, h);
			Color[] autotileColors = autoTileset.PixelsThumb( slotAON);
            tilesetTexture.SetPixels(autotileColors);
			tilesetTexture.Apply();
			return tilesetTexture;
		}
	}
}
