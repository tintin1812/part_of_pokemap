using UnityEngine;
using System.Collections;
 
public class FPSDisplay
{
	private static FPSDisplay _instance = null;
		
	public static FPSDisplay Instance{ 
		get{
			if(_instance == null){
				_instance = new FPSDisplay();
			}
			return _instance;
		}
	}
	private FPSDisplay(){}

	float deltaTime = 0.0f;
	float fps = 60;
	float m_savedFrames = 0f;

	GUIStyle style = new GUIStyle();
	
	public void Update()
	{
		// deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
		// deltaTime += (Time.unscaledDeltaTime) * 0.1f;
		deltaTime += (Time.unscaledDeltaTime);
		if( deltaTime > 1.0f){
            fps = Time.frameCount - m_savedFrames;
            m_savedFrames = Time.frameCount;
			deltaTime -= 1f;
		}
	}
 
	public void OnGUI()
	{
		// int w = Screen.width, h = Screen.height;
		
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = 14;
		style.normal.textColor = new Color (0.0f, 0.0f, 0.5f, 1.0f);
		// string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		string text = string.Format("{0:0.} fps", fps);
		Rect rect = new Rect(4, 0, Screen.width, 25);
		AONGUI.Label(rect, text, style);
		// GUI.Label(rect, text, style);
	}
}
