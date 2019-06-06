using FairyGUI;
using UnityEngine;

public class GButton_TypingEffect : GButton {
    
    Transition _trans;

    public override void ConstructFromXML(FairyGUI.Utils.XML cxml)
	{
		base.ConstructFromXML(cxml);
		_trans = this.GetTransition("t0");
	}

    public override void Dispose()
    {
        base.Dispose();
        if(_te != null && Timers.inst.Exists(_print)){
            Timers.inst.Remove(_print);
        }
    }

    private TypingEffect _te;
    public void PlayTypingEffect(){
        if(_te == null){
            _te = new TypingEffect(GetTextField());
        }
        _te.Start();
        if(Timers.inst.Exists(_print)){
            Timers.inst.Remove(_print);
        }
        Timers.inst.Add(0.050f, 0, _print, this);
    }

    public void _print(object param){
        if(_te == null || !_te.Print()){
            Timers.inst.Remove(_print);
        }
    }

    public void PlayEffect(float delay)
	{
		this.visible = false;
        this.touchable = false;
        // this.x = this.parent.width;
		// _trans.Play(1, delay, onComplete);
        float xTo = this.x;
        float xFrom = this.x + this.width + 200;
        this.x = xFrom;
        GTween.Kill(this);
        this.TweenMoveX(xTo, 0.4f).SetDelay(delay).SetEase(EaseType.QuadOut)
        .OnStart(()=>{
            this.visible = true;
        })
        .OnComplete(()=>{
            this.touchable = true;
        });
	}
}
