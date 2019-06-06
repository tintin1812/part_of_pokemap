using System;
using FairyGUI;
using UnityEngine;

public class ControlShop {

    protected GComponent _contentPane;
    public GComponent contentPane
    {
        get { return _contentPane; }
    }
    
    protected GTextField _title;

    protected GList _listItem;

    protected GTextField _item_name;

    protected GTextField _item_des;

    protected GList _listAtion;

    public GButton _btClose;

    public GGraph _model;

    public GLoader _item_image;

    private RenderImage _renderImage;
    public RenderImage RenderImage{
        get{
            if(_renderImage == null){
                _renderImage = new RenderImage(_model);
                _renderImage.SetBackground(contentPane.GetChild("background"));
            }
            return _renderImage;
        }
    }

	public ControlShop()
    {
        string urlPopup = "ui://BlueSkin/Shop_popup";
        _contentPane = UIPackage.CreateObjectFromURL(urlPopup).asCom;

        _title = _contentPane.GetChild("title").asTextField;

        _listItem = _contentPane.GetChild("list").asList;
        
        _item_name = _contentPane.GetChild("item_name").asTextField;

        _item_des = _contentPane.GetChild("item_des").asTextField;

        _listAtion = _contentPane.GetChild("list_action").asList;

        _btClose = _contentPane.GetChild("bt_close").asButton;

        _model = _contentPane.GetChild("model").asGraph;
        
        var r = RenderImage;

        _item_image = _contentPane.GetChild("item_image").asLoader;
        
        GTextField _coin = _contentPane.GetChild("coin").asTextField;
        // _coin.text = TriggerGame.WorldFlag["Coin"].ToString() + " " + DefineAON.CoinName;
        TriggerGame.Instance.WorldFlag.AddEventListener(_coin, "Coin", () => {
            Debug.Log("Value change");
            _coin.text = TriggerGame.Instance.WorldFlag["Coin"].ToString() + " " + DefineAON.CoinName;
        });
        //----//

        _listItem.RemoveChildrenToPool();
        _listItem.onClickItem.Add(__clickItem);
        
        _listAtion.RemoveChildrenToPool();

        if(_listItem.scrollPane != null){
            _listItem.scrollPane.mouseWheelEnabled = false;
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
		contentPane.Dispose();
        if(_renderImage != null){
            _renderImage.Dispose();
            _renderImage = null;
        }
	}
    
    private void __clickItem(EventContext context)
    {
        GButton item = ((GObject)context.data).asButton;
        if (item == null)
            return;

        if (item.grayed)
        {
            _listItem.selectedIndex = -1;
            return;
        }

        Controller c = item.GetController("checked");
        if (c != null && c.selectedIndex != 0)
        {
            if (c.selectedIndex == 1)
                c.selectedIndex = 2;
            else
                c.selectedIndex = 1;
        }
        
        if (item.data is EventCallback0)
            ((EventCallback0)item.data)();
        else if (item.data is EventCallback1)
            ((EventCallback1)item.data)(context);
    }

    public void ShowOn(GComponent r)
    {
        // GComponent r = target != null ? target : GRoot.inst;
        this.contentPane.x =  r.width - this.contentPane.width - 10;
        this.contentPane.y =  (r.height - this.contentPane.height)  * 0.3f;
        this.contentPane.visible = true;
        r.AddChild(this.contentPane);
    }

    public GButton AddItem(string caption, string caption_right, EventCallback0 callback)
    {
        GButton item = _listItem.AddItemFromPool().asButton;
        item.title = caption;
        item.data = callback;
        item.grayed = false;
        item.GetChild("t_right").asTextField.text = caption_right;
        Controller c = item.GetController("checked");
        if (c != null)
            c.selectedIndex = 0;
        return item;
    }

    public void ResetListItem(){
        _listItem.RemoveChildrenToPool();
        _listItem.selectedIndex = -1;
    }
    
    public void ResetSelectItem(){
        _item_des.text = "";
        _listAtion.RemoveChildrenToPool();
        _model.visible = false;
        _item_image.visible = false;
    }

    public void SetTitle( string title){
        _title.text = title;
    }
    
    public void SetItemName( string des){
        _item_name.text = des;
    }

    public void SetItemDes( string des){
        _item_des.text = des;
    }

    public GButton AddAction(string caption, EventCallback0 callback)
    {
        GButton item = _listAtion.AddItemFromPool().asButton;
        item.title = caption;
        item.onClick.Clear();
        if(callback != null){
            item.onClick.Add(callback);
        }else
        {
           item.grayed = true; 
        }
        // item.data = callback;
        // item.grayed = false;
        return item;
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

    public bool IsLoadModel()
    {
        return _model.visible;   
    }

    public void LoadImageItem( string path)
    {
        if(string.IsNullOrEmpty(path)){
            _item_image.visible = false;
        }else
        {
            _item_image.visible = true;
			_item_image.texture = new NTexture( Resources.Load<Texture2D>("Icons/" + path));
        }
    }
}
