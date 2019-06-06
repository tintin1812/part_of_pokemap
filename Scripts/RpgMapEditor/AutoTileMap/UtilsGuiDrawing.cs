using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AON.RpgMapEditor
{
	public class UtilsGuiDrawing
	{
		static Texture2D Texture;
		public static void DrawRectWithOutline( Rect rect, Color color, Color colorOutline )
		{
			if(Texture == null){
				Texture = new Texture2D(1, 1);
				// Texture.SetPixel(0,0,colorOutline);
				Texture.SetPixel(0,0, Color.green);
				Texture.Apply();
			}

			Rect rLine = new Rect( rect.x, rect.y, rect.width, 2 );
			AONGUI.DrawTexture(rLine, Texture);
			rLine.y = rect.y + rect.height - 1;
			AONGUI.DrawTexture(rLine, Texture);
			rLine = new Rect( rect.x, rect.y + 1, 2, rect.height - 2 );
			AONGUI.DrawTexture(rLine, Texture);
			rLine.x = rect.x + rect.width - 1;
			AONGUI.DrawTexture(rLine, Texture);

			// Rect texCoord = new Rect( rect.x, rect.y, rect.width, rect.height );
			// AONGUI.DrawTextureWithTexCoords(rect, Texture, texCoord);
			// rect.x += 1;
			// rect.y += 1;
			// rect.width -= 2;
			// rect.height -= 2;
			// Texture.SetPixel(0,0,color);
			// Texture.Apply();
			// AONGUI.DrawTexture(rect, Texture);
		}
		/*
		public static void DrawRectWithOutline( Rect rect, Color color, Color colorOutline )
		{
	#if true//UNITY_STANDALONE_OSX || UNITY_WEBGL || UNITY_IOS
			if(Texture == null){
				Texture = new Texture2D(1, 1);
				Texture.SetPixel(0,0,new Color(0f, 1f, 0 , 1.0f));
				Texture.Apply();
			}
			// GUI.DrawTexture(rect, Texture);
			AONGUI.DrawTexture(rect, Texture, ScaleMode.StretchToFill, false, 1, Color.white, 4, 4);
			// Rect rLine = new Rect( rect.x, rect.y, rect.width, 1 );
			// GUI.DrawTexture(rLine, Texture);
			// rLine.y = rect.y + rect.height - 1;
			// GUI.DrawTexture(rLine, Texture);
			// rLine = new Rect( rect.x, rect.y+1, 1, rect.height-2 );
			// GUI.DrawTexture(rLine, Texture);
			// rLine.x = rect.x + rect.width - 1;
			// GUI.DrawTexture(rLine, Texture);
			
	#elif false//UNITY_EDITOR
		#if false
			// EditorGUI.DrawRect(rect, color);
			float offset = 2;
			float offset2x = 2*offset;
			EditorGUI.DrawRect(new Rect(rect.x + offset,rect.y,rect.width - offset2x,offset), colorOutline);//Top
			EditorGUI.DrawRect(new Rect(rect.x + offset,rect.y + rect.height - offset,rect.width - offset2x,offset), colorOutline);//Bot
			EditorGUI.DrawRect(new Rect(rect.x,rect.y,offset,rect.height), colorOutline);//Left
			EditorGUI.DrawRect(new Rect(rect.x + rect.width - offset,rect.y,offset,rect.height), colorOutline);//Right
		#else
			Vector3[] rectVerts = { new Vector3(rect.x, rect.y, 0), 
			new Vector3(rect.x + rect.width, rect.y, 0), 
			new Vector3(rect.x + rect.width, rect.y + rect.height, 0), 
			new Vector3(rect.x, rect.y + rect.height, 0) };
			Handles.DrawSolidRectangleWithOutline(rectVerts, color, colorOutline);
		#endif
	#else
			Texture2D texture = new Texture2D(1, 1);
			texture.SetPixel(0,0,colorOutline);
			texture.Apply();

			Rect rLine = new Rect( rect.x, rect.y, rect.width, 1 );
			GUI.DrawTexture(rLine, texture);
			rLine.y = rect.y + rect.height - 1;
			GUI.DrawTexture(rLine, texture);
			rLine = new Rect( rect.x, rect.y+1, 1, rect.height-2 );
			GUI.DrawTexture(rLine, texture);
			rLine.x = rect.x + rect.width - 1;
			GUI.DrawTexture(rLine, texture);

			rect.x += 1;
			rect.y += 1;
			rect.width -= 2;
			rect.height -= 2;
			texture.SetPixel(0,0,color);
			texture.Apply();
			GUI.DrawTexture(rect, texture);
	#endif
		}
		*/
	}
}