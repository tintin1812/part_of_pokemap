using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGui {
	// public static bool IgnoreMouse = false;
	private static Rect Rect = Rect.zero;
	public static bool IsCheck = false;

	public static bool IsIgnoreMouse(Vector2 mouse){
		mouse.y = Screen.height - mouse.y;
		if(IsCheck && Rect.Contains(mouse)){
			return true;
		}
		return false;
	}

	public static void SetRectIgnore(Rect rect){
		Rect = rect;
		IsCheck = true;
	}
}
