using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

public delegate void DownLineCompleteCallback(TypingEffectByLine ty);

public class TypingEffectByLine : TypingEffect
{

	protected DownLineCompleteCallback _onComplete;

	protected int _lineIndex = 0;

	protected Rect _rectRaw;
	
	protected Container _containerDownline;
	protected GObject _showNext;
	protected GObject _clickNext;
	protected IEnumerator _print;

	protected GObject _gContainer;
	public GObject GContainer{
		get{
			return _gContainer;
		}
	}

    public TypingEffectByLine(GObject obj) : base(obj.asTextField)
    {
        CreateContainer(obj);
    }

    public TypingEffectByLine(GTextField textField) : base(textField)
    {
		CreateContainer(textField);
        // if (textField is GRichTextField == false)
        // {
        //     CreateParent(textField);
        // }else
		// {
		// 	_containerDownline = _textField.parent;
		// 	_rectRaw = new Rect(_containerDownline.x, _containerDownline.y, _containerDownline.width, _containerDownline.height);
		// }
    }

    public TextField TextField
    {
        get { return _textField; }
    }

    private void CreateContainer(GObject obj)
    {
        GComponent graph = new GComponent();
        obj.parent.AddChild(graph);
        graph.xy = obj.xy;
        graph.size = obj.size;
        obj.xy = Vector2.zero;
        graph.AddChild(obj);
		_gContainer = graph;
		_containerDownline = graph.container;
		_rectRaw = new Rect(_containerDownline.x, _containerDownline.y, _containerDownline.width, _containerDownline.height);
    }

	public void SetUp( string text, GObject showNext, GObject clickNext, DownLineCompleteCallback onComplete, bool isShowBtNext){
		_showNext = showNext;
		if(isShowBtNext){
			// _clickNext = clickNext;
			_onComplete = (TypingEffectByLine ty) => {
				this.ShowNext(()=>{
					onComplete(ty);
				});
			};
		}else
		{
			_onComplete = onComplete;
		}
		_clickNext = clickNext;	
		_lineIndex = 0;
		if(_showNext != null)
			_showNext.visible = false;
		_containerDownline.x = _rectRaw.x;
		_containerDownline.y = _rectRaw.y;
		_containerDownline.width = _rectRaw.width;
		_textField.autoSize = AutoSizeType.Height;
		if(!string.IsNullOrEmpty(text))
			_textField.text = text;
		_textField.xy = Vector2.zero;
		_containerDownline.height = _textField.height;
		_containerDownline.clipRect = new Rect(0, 0, _rectRaw.width, _rectRaw.height);
		base.Start();
		if(Timers.inst.Exists(_printText)){
			Timers.inst.Remove(_printText);
		}
		Timers.inst.Add(0.050f, 0, _printText);
		_gContainer.onRemovedFromStage.Add(() => {
			if(Timers.inst.Exists(_printText)){
				Timers.inst.Remove(_printText);
			}
		});
	}

	void _printText(object param)
	{
		PrintByLine(2, this._downLineNextCallback, this._onCompleteCallback);
	}

	// public delegate void DownLineNextCallback();

	private void _downLineNextCallback(){
		Debug.Log("_downLineNextCallback");
		Timers.inst.Remove(_printText);
		if(_showNext != null)
			_showNext.visible = true;
		// _clickNext.touchable = true;
		_clickNext.onClick.Add(_onClickNext);
	}

	private void _onClickNext(){
		Debug.Log("_onClickNext");
		_clickNext.onClick.Remove(_onClickNext);
		if(_showNext != null)
			_showNext.visible = false;
		DownLine(2);
		if(Timers.inst.Exists(_printText)){
			Timers.inst.Remove(_printText);
		}
		Timers.inst.Add(0.050f, 0, _printText);
		_gContainer.onRemovedFromStage.Add(() => {
			if(Timers.inst.Exists(_printText)){
				Timers.inst.Remove(_printText);
			}
		});
	}

	private void _onCompleteCallback(){
		// Debug.Log("_onCompleteCallback");
		Timers.inst.Remove(_printText);
		if(this._onComplete != null){
			this._onComplete( this);
		}
	}


    /*
	public bool PrintByLineSubstring( int line = 2)
	{
		if(_textField.lines.Count > line){
			var i0 = _textField.lines[line - 1].charCount;
			var l1 = _textField.lines[line - 1];
			var i1 = l1.charIndex + l1.charCount;
			var iAffterShouldBe = i1 - i0;
			if(_printIndex >= i1 - 1){
				if(_textField.htmlElements.Count > 0){
					
					// _textField.htmlText = _textField.text.Substring(i0);
				}else
				{
					_textField.text = _textField.text.Substring(i0);
				}
				
				Start();
				if(line > 1){
					if(ForcePrintTo(iAffterShouldBe) == false){
						return false;
					}
				}
				Debug.Log("Down line " + _printIndex.ToString());
			}
		}
		return base.Print();
	}

	public bool ForcePrintTo( int indexTo)
	{
		while (_printIndex < indexTo)
		{
			if(base.Print() == false){
				return false;
			}
		}
		return true;
	}
	*/

    // public IEnumerator PrintByLine(float interval, int line = 2, DownLineNextCallback downLineCallBack = null)
    // {
    //     while (PrintByLine(line, downLineCallBack))
    //         yield return new WaitForSeconds(interval);
    // }
    
    public void PrintByLine(int limit, EventCallback0 downLineCallBack, EventCallback0 onComplete)
    {
        if (_lineIndex < _textField.lines.Count - limit)
        {
            var lineCheck = _textField.lines[_lineIndex + limit];
            if (_printIndex >= lineCheck.charIndex - 1)
            {
				// int pos = lineCheck.charIndex;
				// List<TextField.CharPosition> charPositions = _textField.charPositions;
				// var next = charPositions[pos];
				// var check = charPositions[pos - 1];
				downLineCallBack();
				return;
            }
        }
        bool result = base.Print();
		if( result == false){
			onComplete();
		}
    }

    public void DownLine(int limit)
    {
        if (_lineIndex + limit < _textField.lines.Count)
        {
            var line0 = _textField.lines[_lineIndex];
			var line1 = _textField.lines[_lineIndex + limit];
			var y0 = line0.y + line0.height;
            var y1 = line1.y + line1.height;
            Container c = _containerDownline;
			c.clipRect = new Rect(0, y0, c.width, y1 - y0);
			// c.y = _rectRaw.y - y0;
			GTween.Kill(this);
			GTween.To(c.y, _rectRaw.y - y0, 0.05f).SetTarget(this).SetEase(EaseType.Linear).OnUpdate(
				(GTweener tweener) => {
					c.y = tweener.value.x;
				}
			);
			_lineIndex++;
        }
    }

	private void ShowNext( EventCallback0 callBack){
		if(_showNext != null){
			_showNext.visible = true;
		}
		_clickNext.onClick.Add(() => {
			if(_showNext != null){
				_showNext.visible = false;
			}
			_clickNext.onClick.Clear();
			callBack();
		});
	}
}
