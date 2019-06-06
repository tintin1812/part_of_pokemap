using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using AON.RpgMapEditor;

public class WindownPickModel : Window {

    GTextField _title;
    GTextField _text;
	GComboBox _topic;
	GList _list_icon;
	GButton _bt_pick;
	GGraph _model;
	private RenderImage _renderImage;
    public RenderImage RenderImage{
        get{
            if(_renderImage == null){
                Debug.Log("Create new RenderImage");
                _renderImage = new RenderImage(_model);
                _renderImage.SetBackground(this.frame.GetChild("n0"));
            }
            return _renderImage;
        }
    }

	public WindownPickModel(){}

	override public void Dispose()
	{	
		base.Dispose();
		if(_renderImage != null){
            _renderImage.Dispose();
            _renderImage = null;
        }
	}
    protected override void OnInit()
	{
		// Debug.Log("WindowTest OnInit");
		this.SetSize( 300, 200);
		this.Center();
		this.contentPane = UIPackage.CreateObject("BlueSkin", "FramePickModel").asCom;
        _title = frame.GetChild("title").asTextField;
        _text  = this.contentPane.GetChild("text").asTextField;
		_topic = this.contentPane.GetChild("topic").asComboBox;
		_list_icon = this.contentPane.GetChild("list_icon").asList;
		_bt_pick = this.contentPane.GetChild("bt_pick").asButton;
		_model = this.contentPane.GetChild("model").asGraph;
	}

	override protected void OnShown()
	{
		// Debug.Log("WindowTest OnShown");
	}

	override protected void OnHide()
	{
		if(_renderImage != null){
            _renderImage.Dispose();
            _renderImage = null;
        }
	}

	public delegate void OnPick(string topic, string icon);
    public void ShowNoti(PetsDatabase data, OnPick callback){
        if( data == null || data.PetList == null || data.PetList.Count == 0){
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

	private void _refreshIconList( PetsDatabase data, OnPick callback){
		_list_icon.RemoveChildrenToPool();
		_list_icon.onClickItem.Clear();
		_bt_pick.visible = false;
		_model.visible = false;
		
		if(_topic.selectedIndex < 0 || _topic.selectedIndex >= data.PetList.Count){
			return;
		}
		Pets pets =  data.PetList[_topic.selectedIndex];
		_onChangedTopic(pets, callback);
	}

	private void _onChangedTopic(Pets pets, OnPick callback){
		_bt_pick.visible = false;
		_model.visible = false;
		var data = pets.data;
		for (int i = 0; i < data.Length; i++)
		{
			GButton item = _list_icon.AddItemFromPool().asButton;
			item.title = data[i];
		}
		_list_icon.onClickItem.Add((EventContext context) =>{
			string petName =  data[_list_icon.selectedIndex];
			_bt_pick.visible = true;
			_bt_pick.onClick.Clear();
			_bt_pick.onClick.Add(()=>{
				callback(pets.topic, petName);
			});
			_model.visible = true;
			string path = "pets/" + pets.topic + "/" + petName;
			LoadModelItem(path);
			// _image.texture = new NTexture( Resources.Load<Texture2D>("Icons/" + pets.topic + "/" + icon));
		});
	}

	public void LoadModelItem(string modelUrl)
    {
        RenderImage _renderImage = RenderImage;
        _renderImage.LoadModel(modelUrl);
        _renderImage.SetCamLockModel();
        _renderImage.SetShader("Unlit/Texture");
        _renderImage.StartRotate(-1);
        _model.visible = true;
    }
}
