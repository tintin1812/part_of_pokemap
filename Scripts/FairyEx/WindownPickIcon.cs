using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using AON.RpgMapEditor;

public class WindownPickIcon : Window {

    GTextField _title;
    GTextField _text;
	GComboBox _topic;
	GList _list_icon;
	GButton _bt_pick;
	GLoader _image;

	public WindownPickIcon(){}

    protected override void OnInit()
	{
		// Debug.Log("WindowTest OnInit");
		this.SetSize( 300, 200);
		this.Center();
		this.contentPane = UIPackage.CreateObject("BlueSkin", "FramePickIcon").asCom;
        _title = frame.GetChild("title").asTextField;
        _text  = this.contentPane.GetChild("text").asTextField;
		_topic = this.contentPane.GetChild("topic").asComboBox;
		_list_icon = this.contentPane.GetChild("list_icon").asList;
		_bt_pick = this.contentPane.GetChild("bt_pick").asButton;
		_image = this.contentPane.GetChild("image").asLoader;
	}

	override protected void OnShown()
	{
		// Debug.Log("WindowTest OnShown");
	}

	public delegate void OnPick(string topic, string icon);
    public void ShowNoti(IconsDatabase data, OnPick callback){
        if( data == null || data.IconList == null || data.IconList.Count == 0){
			return;
		}
		_topic.items = data.Topic;
		_topic.onChanged.Clear();
		_topic.onChanged.Add(()=>{
			_refreshIconList(data, callback);
		});
		_refreshIconList(data, callback);
        Show();
	}

	private void _refreshIconList( IconsDatabase data, OnPick callback){
		_list_icon.RemoveChildrenToPool();
		_list_icon.onClickItem.Clear();
		_bt_pick.visible = false;
		_image.visible = false;
		
		if(_topic.selectedIndex < 0 || _topic.selectedIndex >= data.IconList.Count){
			return;
		}
		Icons icons =  data.IconList[_topic.selectedIndex];
		_onChangedTopic(icons, callback);
	}

	private void _onChangedTopic(Icons icons, OnPick callback){
		_bt_pick.visible = false;
		_image.visible = false;
		var data = icons.data;
		for (int i = 0; i < data.Length; i++)
		{
			GButton item = _list_icon.AddItemFromPool().asButton;
			item.title = data[i];
		}
		_list_icon.onClickItem.Add((EventContext context) =>{
			string icon =  data[_list_icon.selectedIndex];
			_bt_pick.visible = true;
			_bt_pick.onClick.Clear();
			_bt_pick.onClick.Add(()=>{
				callback(icons.topic, icon);
			});
			_image.visible = true;
			_image.texture = new NTexture( Resources.Load<Texture2D>("Icons/" + icons.topic + "/" + icon));
		});
	}
}
