using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

public class WindownNoti : Window {

    GTextField _title;
    GTextField _text;

	public WindownNoti(){}

    protected override void OnInit()
	{
		// Debug.Log("WindowTest OnInit");
		this.SetSize( 300, 200);
		this.Center();
		this.contentPane = UIPackage.CreateObject("BlueSkin", "FrameNoti").asCom;
        _title = frame.GetChild("title").asTextField;
        _text  = this.contentPane.GetChild("text").asTextField;
	}

	override protected void OnShown()
	{
		// Debug.Log("WindowTest OnShown");
	}

    public void ShowNoti( string title, string text){
        _title.text = title;
		_text.text = text;
        Show();
	}
}
