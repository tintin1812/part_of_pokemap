using System;
using FairyGUI;
using UnityEngine;

public class QuickControlList : ControlList
{
    public QuickControlList() : base("ui://BlueSkin/ComboBox_popup")
    {
        _contentPane.onTouchBegin.Add((EventContext context) =>
        {
            context.StopPropagation();
        });
        _contentPane.onClick.Add((EventContext context) =>
        {
            context.StopPropagation();
        });
    }

    public void SetParent(GComponent target)
    {
        base.ShowOn(target);
        this.ResizeWidthMinFromItems(false);
        this.ResizeHeightToFix();
        //Disable scroll
        if (this.list.scrollPane != null)
        {
            this.list.scrollPane.touchEffect = false;
        }
        var r = target;
        this.contentPane.x = r.width - this.contentPane.width - 10;
        this.contentPane.y = r.height - this.contentPane.height - 10;
    }

    public void AddBt(string caption, EventCallback1 callback)
    {
        base.AddItemWithUrl("ui://BlueSkin/Button_choise_w", caption, callback);
    }

    public void SetOnDispose(EventCallback0 callback)
    {
        this.contentPane.onRemovedFromStage.Add(callback);
    }
}
