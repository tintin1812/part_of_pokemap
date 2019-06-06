using System;
using FairyGUI;
using UnityEngine;

public class ControlMenu {

    protected GComponent _contentPane;
    
    public GButton _btBag;
    public GTextField _coin;
    public GTextField _stamina;
    public GTextField _reputation;
    
    public GButton _btTalk;

    public ControlMenu(){
        string urlPopup = "ui://BlueSkin/MenuInGame";
        _contentPane = UIPackage.CreateObjectFromURL(urlPopup).asCom;

        _btBag = _contentPane.GetChild("bt_bag").asButton;
        _coin = _contentPane.GetChild("coin").asTextField;
        _stamina = _contentPane.GetChild("stamina").asTextField;
        _reputation = _contentPane.GetChild("reputation").asTextField;
        _btTalk = _contentPane.GetChild("bt_talk").asButton;
        _btTalk.visible = false;
    }

    public GComponent contentPane
    {
        get { return _contentPane; }
    }

    public void ShowOn(GComponent r)
    {
        if (_contentPane.parent == null)
        {
            this.contentPane.x =  r.x;
            this.contentPane.y =  r.y;
            this.contentPane.width =  r.width;
            this.contentPane.height =  r.height;
            // r.AddChild(this.contentPane);
            r.AddChildAt(this.contentPane, 0);
        }
    }

	public void Hide()
    {
        if (_contentPane.parent != null)
        {
            GComponent r = (GComponent)_contentPane.parent;
            r.RemoveChild(this.contentPane);
        }
    }

    public void Dispose()
    {
        if (_contentPane != null)
        {
            _contentPane.Dispose();
        }
    }
}
