using System.Collections;
using System.Collections.Generic;
using System.IO;
using AON.RpgMapEditor;
using FairyGUI;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldHelper : MonoBehaviour
{
    public static InputFieldHelper Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    UIPanel _panel;
    public UIPanel Panel
    {
        get
        {
            return _panel;
        }
    }

    GComponent _mainView;
    // public GComponent MainView{
    // 	get {
    // 		return _mainView;
    // 	}
    // }

    GComponent _popUp;
    public GComponent PopUp
    {
        get
        {
            return _popUp;
        }
    }

    public bool IsChargeScene{get;set;}

    void Start()
    {
        Application.targetFrameRate = 60;

        IsChargeScene = false;

        Stage.inst.onKeyDown.Add(OnKeyDown);

        // UIPackage.AddPackage("UI/BlueSkin");

        float w = 600;
        float h = 400;
        float scale = Mathf.Min(GRoot.inst.width / w, GRoot.inst.height / h);
        w = GRoot.inst.width / scale;
        h = GRoot.inst.height / scale;

        _panel = this.GetComponent<UIPanel>();
        _mainView = _panel.ui;
        _mainView.SetSize(w, h);
        _mainView.scaleX = scale;
        _mainView.scaleY = scale;
        _mainView.x = 0;
        _mainView.y = 0;

        _popUp = new GComponent();
        _popUp.x = _mainView.x;
        _popUp.y = _mainView.y;
        _popUp.width = _mainView.width;
        _popUp.height = _mainView.height;
        _mainView.AddChild(_popUp);

        //Option 1
        _mainView.onTouchBegin.Add(__onTouchBeginMainView);
        // _mainView.onTouchMove.Add(__onTouchMoveMainView);

        //Option 2
        // Stage.inst.onTouchBegin.Add(OnTouchBegin);
        // _mainView.touchable = false;
    }

    // void _onTouchBegin()
	// {
	// 	if (!Stage.isTouchOnUI)
	// 	{
    //         var m = new Vector2(Stage.inst.touchPosition.x, Screen.height - Stage.inst.touchPosition.y);
    //         AutoTileMap_Editor.Instance.Agent.GetComponent<ClickToMoveAON>().MoveTo(m);
    //     }
    // }
    

    private void __onTouchBeginMainView(EventContext context)
    {
        // Debug.Log("__onTouchBeginMainView");
        // Debug.Log("context.callChain.Count " + context.callChain.Count);
        if(context.callChain.Count != 2){
            return;
        }
        if(!IsMainCharCanMove())
            return;
        // context.CaptureTouch();
        var m = Input.mousePosition;
        AutoTileMap_Editor.Instance.Agent.GetComponent<ClickToMoveAON>().MoveTo(m);
    }

    /*
    private void __onTouchMoveMainView(EventContext context)
    {
        Debug.Log("__onTouchMoveMainView");
        Debug.Log("context.callChain.Count " + context.callChain.Count);
        if(!AutoTileMap_Editor.Instance.IsPlayMode){
            return;
        }
        if(context.callChain.Count != 2){
            return;
        }
        var m = Input.mousePosition;
        AutoTileMap_Editor.Instance.Agent.GetComponent<ClickToMoveAON>().MoveTo(m);
    }
    */

    public void HideAll()
    {
        DisposePopupAction();
        DisposeChatBottom();
        EndMenuGame();
        _popUp.RemoveChildren();
    }

    public bool IsMainCharCanMove()
    {
        if(IsChargeScene){
            return false;
        }
        if(AutoTileMap_Editor.Instance == null || !AutoTileMap_Editor.Instance.IsPlayMode){
            return false;
        }
        if(AutoTileMap_Editor.Instance.CamControler.target == null){
            return false;
        }
        bool isMainChar_Wait_To_Interaction = TriggerGame.Instance.IsMainChar_Wait_To_Interaction();
        if(TriggerGame.Instance.IsScriptMainRunning){
            if(!isMainChar_Wait_To_Interaction){
                return false;
            }
        }
        if (_windownNoti != null && _windownNoti.isShowing)
            return false;
        if(_chatBottom != null && _chatBottom.parent != null)
            return false;
        // if(_controlMenu != null && _controlMenu.contentPane.parent == null)
        //     return false;
        if(pm_choise_bag != null){
            return false;
        }
        if(_popUp != null && _popUp.numChildren > 0){
            for (int i = 0; i < _popUp.numChildren; i++)
            {
                if(_popUp.GetChildAt(i).visible){
                    return false;
                }
            }
        }
        return true;
    }

    //------------------------------

    private WindownNoti _windownNoti;

    private WindownNoti WindownNoti
    {
        get
        {
            if (_windownNoti == null)
            {
                _windownNoti = new WindownNoti();
                _windownNoti.Init();
            }

            return _windownNoti;
        }
    }

    public bool IsShowNoti()
    {
        if (_windownNoti == null)
            return false;
        return _windownNoti.isShowing;
    }

    // public void Show( string text, OnInputDelegate _onInput){
    // }

    public void ShowNoti(string text)
    {
        Debug.LogWarning(text);
        WindownNoti.ShowNoti("Error", text);
    }

    //------------------------------

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            // Application.Quit();
        }
        else if (context.inputEvent.keyCode == KeyCode.T)
        {
            // BasicMecanimControl basicMecanimControl = AutoTileMap_Editor.Instance.Agent.GetComponentInChildren<BasicMecanimControl>();
            // if(!basicMecanimControl.IsRide){
            //     basicMecanimControl.Pet = "pets/pets/Pet_Boar_green";
            // }else{
            //     basicMecanimControl.Pet = "";
            // }
            // TriggerGame.WorldFlag.DoAdd("Coin", 10);
        }
    }

    //------------------------------
    private ControlList _p_action;
    public ControlList ShowPopupAction_Conversation()
    {
        // string url = "ui://BlueSkin/ComboBox_popup"
        string urlPopup = "ui://BlueSkin/ComboBox_popup_w";
        if (_p_action == null)
        {
            _p_action = new ControlList(urlPopup);
        }
        else
        {
            _p_action.ClearItems();
        }
        _p_action.ShowOn(_popUp);
        return _p_action;
    }

    public void HidePopupAction()
    {
        if (_p_action != null)
        {
            _p_action.Hide();
        }
    }

    public void DisposePopupAction()
    {
        if (_p_action != null)
        {
            _p_action.Dispose();
            _p_action = null;
        }
    }

    //------------------------------
    private GComponent _chatBottom;
    private TypingEffectByLine _te;
    // public string LastTextChatBottom()
    // {
    //     return _te.TextField.text;
    // }

    public GComponent GComponent_ChatBottom
    {
        get
        {
            return _chatBottom;
        }
    }

    private void initChatBottom(){
        if (_te == null)
        {
            _chatBottom = UIPackage.CreateObjectFromURL("ui://BlueSkin/Chat1").asCom;
            _te = new TypingEffectByLine(_chatBottom.GetChild("title"));
        }
        if (_chatBottom.parent == null)
        {
            _popUp.AddChild(_chatBottom);
        }
        if (_chatBottom.visible == false)
        {
            _chatBottom.visible = true;
        }
    }

    public void ShowChatBottom(string text, bool isShowBtNext, DownLineCompleteCallback callback)
    {
        initChatBottom();
        _te.SetUp(text, _chatBottom.GetChild("next"), _chatBottom, callback, isShowBtNext);
        float posFrameLeft = (int)((_popUp.width - _chatBottom.width) / 2);
        float posFrameBot = _popUp.height - _chatBottom.height - 10;
        _chatBottom.SetXY(posFrameLeft, posFrameBot);
    }

    public void ShowChatBottomWithArrow(GameObject objListener, GameObject objTalker, string text, DownLineCompleteCallback callback)
    {
        initChatBottom();
        _te.SetUp(text, _chatBottom.GetChild("next"), _chatBottom, callback, true);
        float posFrameLeft = (int)((_popUp.width - _chatBottom.width) / 2);
        float posFrameRight = posFrameLeft;
        float posFrameTop = 10;
        float posFrameBot = _popUp.height - _chatBottom.height - 10;
        AddArrowTalker(objListener, objTalker, _chatBottom, posFrameTop, posFrameBot, posFrameLeft, posFrameRight);
    }

    public void HideChatBottom()
    {
        // if (_chatBottom != null )
        // {
        //     _chatBottom.RemoveFromParent();
        // }
        DisposeChatBottom();
    }
    public void DisposeChatBottom()
    {
        if (_chatBottom != null )
        {
            _chatBottom.Dispose();
            _chatBottom = null;
            _te = null;
        }
    }

    public static void AddArrowTalker(GameObject objListener, GameObject objTalker, GComponent frame, float posFrameTop, float posFrameBot, float posFrameLeft, float posFrameRight)
    {
        if (objTalker == null)
        {
            frame.SetXY(posFrameRight, posFrameBot);
            var tObj = frame.GetChild("talker");
            if (tObj != null)
            {
                frame.GetChild("talker").visible = false;
            }
        }
        else if (objListener == null)
        {
            frame.SetXY(posFrameRight, posFrameBot);
            var tObj = frame.GetChild("talker");
            if (tObj == null)
            {
                tObj = new GGraph();
                tObj.name = "talker";
                frame.AddChild(tObj);
            }
            else
            {
                tObj.visible = true;
            }
            GGraph graph = tObj.asGraph;
            // var pAgent = AutoTileMap_Editor.Instance.Agent.transform.position;
            var pTalk = objTalker.transform.position;
            Debug.Log("pAgent:" + pTalk.ToString());
            var p = AutoTileMap_Editor.Instance.PlayCamera.WorldToScreenPoint(pTalk);
            Debug.Log("ScreenPoint:" + p.ToString());
            var local = frame.GlobalToLocal(p);
            Debug.Log("local:" + local.ToString());
            // Vector2 a = new Vector2(_c.width / 2, _c.height / 2);
            Vector2 a = new Vector2(80, 5);
            Vector2 b = new Vector2(a.x, a.y);
            // local = (local + a) * 0.75f + a;
            local = Vector2.Lerp(a, local, 0.5f);
            a.x = a.x - 30;
            b.x = b.x + 30;
            var shape = graph.shape;
            shape.DrawPolygon(new Vector2[] {
                local,
                a,
                b,
            }, shape.color);
        }
        else
        {
            var cam = AutoTileMap_Editor.Instance.PlayCamera;
            var pL = cam.WorldToScreenPoint(objListener.transform.position);
            var pT = cam.WorldToScreenPoint(objTalker.transform.position);
            // Debug.Log("Listen:" + pL.ToString());
            // Debug.Log("Talk:" + pT.ToString());
            Vector2 a = new Vector2(80, 5);
            if (pT.z > pL.z)
            {
                // Top
                frame.SetXY(pT.x > pL.x ? posFrameRight : posFrameLeft, posFrameTop);
                a.y = frame.height - 5;
            }
            else
            {
                // Bot
                frame.SetXY(pT.x > pL.x ? posFrameRight : posFrameLeft, posFrameBot);
            }
            if (posFrameRight <= posFrameLeft)
            {
                // a.x = (int)(frame.width / 2);
                if (pT.x > pL.x)
                {
                    a.x = frame.width - 80;
                }
            }
            else if (pT.x < pL.x)
            {
                a.x = frame.width - 80;
            }
            var pTalk = objTalker.transform.position;
            var p = AutoTileMap_Editor.Instance.PlayCamera.WorldToScreenPoint(pTalk);
            var local = frame.GlobalToLocal(p);
            local = Vector2.Lerp(a, local, 0.5f);
            Vector2 b = new Vector2(a.x, a.y);
            a.x = a.x - 20;
            b.x = b.x + 20;

            var tObj = frame.GetChild("talker");
            if (tObj == null)
            {
                tObj = new GGraph();
                tObj.name = "talker";
                frame.AddChild(tObj);
            }
            else
            {
                tObj.visible = true;
            }
            GGraph graph = tObj.asGraph;
            var shape = graph.shape;
            shape.DrawPolygon(new Vector2[] {
                local,
                a,
                b,
            }, shape.color);
        }
    }

    //------------------------------//
    private ControlMenu _controlMenu;
    public void BeginMenuGame()
    {
        if(_controlMenu == null){
            _controlMenu = new ControlMenu();
            _controlMenu._btBag.onClick.Add((EventContext context) =>
            {
                Debug.Log("On click on Bag");
                // context.StopPropagation();
                ShowBag();
            });
            // _controlMenu._coin.text = TriggerGame.WorldFlag["Coin"].ToString() + " " + DefineAON.CoinName;
            TriggerGame.Instance.WorldFlag.AddEventListener(_controlMenu._coin, "Coin", () => {
                _controlMenu._coin.text = TriggerGame.Instance.WorldFlag["Coin"].ToString() + " " + DefineAON.CoinName;
            });

            TriggerGame.Instance.WorldFlag.AddEventListener(_controlMenu._stamina, "Stamina", () => {
                _controlMenu._stamina.text = TriggerGame.Instance.WorldFlag["Stamina"].ToString();
            });

            TriggerGame.Instance.WorldFlag.AddEventListener(_controlMenu._reputation, "Reputation", () => {
                _controlMenu._reputation.text = TriggerGame.Instance.WorldFlag["Reputation"].ToString();
            });
        }
        _controlMenu.ShowOn(_mainView);
    }

    public void EndMenuGame()
    {
        if (_controlMenu == null)
        {
            return;
        }
        _controlMenu.Dispose();
        _controlMenu = null;
    }

    public void ShowMenu()
    {
        if (_controlMenu == null)
        {
            return;
        }
        _controlMenu.ShowOn(_mainView);
    }

    public void HideMenu()
    {
        if (_controlMenu == null)
        {
            return;
        }
        _controlMenu.Hide();
    }

    public void Show_Menu_BtTalk( string text, EventCallback0 callback)
    {
        _controlMenu._btTalk.text = text;
        _controlMenu._btTalk.visible = true;
        _controlMenu._btTalk.onClick.Add(callback);
    }
    public void Hide_Menu_BtTalk()
    {
        _controlMenu._btTalk.visible = false;
        _controlMenu._btTalk.onClick.Clear();
    }

    //------------------------------//
    QuickControlList pm_choise_bag = null;
    public void ShowBag()
    {
        if(pm_choise_bag != null){
            ShowNoti("Bag was show");
            return;
        }
        if (_popUp != null)
        {
            for (int i = 0; i < _popUp.numChildren; i++)
            {
                if (_popUp.GetChildAt(i).visible)
                {
                    ShowNoti("Can't show Bag when Talking");
                    return;
                }
            }
        }
        HideMenu();
        // Show Bag
        pm_choise_bag = new QuickControlList();
        pm_choise_bag.AddBt("Outfits", (EventContext context) =>
        {
            // Debug.Log("Click Costume");
            CharGame c = AutoTileMap_Editor.Instance.Agent.GetComponentInChildren<CharGame>();
            PropertysGame.Instance.ShowOutfits(pm_choise_bag.contentPane, c, AutoTileMap_Editor.Instance.MapsData.Propertys);
        });
        pm_choise_bag.AddBt("Pets", (EventContext context) =>
        {
            PropertysGame.Instance.ShowPets(pm_choise_bag.contentPane, AutoTileMap_Editor.Instance.MapsData.Propertys);
        });
        pm_choise_bag.AddBt("Items", (EventContext context) =>
        {
            PropertysGame.Instance.ShowItems(pm_choise_bag.contentPane);
        });
        pm_choise_bag.AddBt("Certificates", (EventContext context) =>
        {
            PropertysGame.Instance.ShowCertificates(pm_choise_bag.contentPane, AutoTileMap_Editor.Instance.MapsData.Propertys);
        });
        pm_choise_bag.AddBt("Cancel", (EventContext context) =>
        {
            pm_choise_bag.Dispose();
            pm_choise_bag = null;
        });
        pm_choise_bag.SetParent(PopUp);
        pm_choise_bag.SetOnDispose(() =>
        {
            ShowMenu();
        });
    }

    //------------------WindownPickIcon------------//
    private WindownPickIcon _windownPickIcon;
    public void ShowPickIcon(WindownPickIcon.OnPick callback)
    {
        if(_windownPickIcon == null){
            _windownPickIcon = new WindownPickIcon();
            _windownPickIcon.Init();
        }else{
            _windownPickIcon.Show();
        }
        _windownPickIcon.ShowNoti(IconsDatabase.Instance, callback);
    }

    public void HidePickIcon(){
        if(_windownPickIcon == null){
            return;
        }
        _windownPickIcon.Hide();
    }

    public bool IsShowPickIcon{
        get{
            if(_windownPickIcon == null){
                return false;
            }
            return _windownPickIcon.isShowing;
        }
    }

    //------------------------------//
    private WindownPickModel _windownPickModel;
    public void ShowPickModel(WindownPickModel.OnPick callback)
    {
        if(_windownPickModel == null){
            _windownPickModel = new WindownPickModel();
            _windownPickModel.Init();
        }else{
            _windownPickModel.Show();
        }
        _windownPickModel.ShowNoti(PetsDatabase.Instance, callback);
    }

    public void HidePickModel(){
        if(_windownPickModel == null){
            return;
        }
        _windownPickModel.Hide();
    }

    public bool IsShowPickModel{
        get{
            if(_windownPickModel == null){
                return false;
            }
            return _windownPickModel.isShowing;
        }
    }

    public void ChargeSceneToDark(GTweenCallback onComplete){
        Debug.Log("Charge Scene");
        SimpleBlit simpleBlit = _panel.container.renderCamera.gameObject.GetComponent<SimpleBlit>();
        if(simpleBlit == null){
            simpleBlit = _panel.container.renderCamera.gameObject.AddComponent<SimpleBlit>();
            Object prefab = Resources.Load("BattleTransitions");
            // simpleBlit.TransitionMaterial = (Material)prefab;
            simpleBlit.TransitionMaterial = (Material)Object.Instantiate(prefab);
        }
        simpleBlit.enabled = true;
        GTween.To(0, 1, 1.0f).SetTarget(this).SetEase(EaseType.Linear).OnUpdate(
            (GTweener tweener) => {
                simpleBlit._cutoff = tweener.value.x;
            }
        ).OnComplete(()=>{
            onComplete();
        });
    }

    public void ChargeSceneToLight(GTweenCallback onComplete){
        SimpleBlit simpleBlit = _panel.container.renderCamera.gameObject.GetComponent<SimpleBlit>();
        GTween.To(0, 1, 1.0f).SetTarget(this).SetEase(EaseType.Linear).OnUpdate(
            (GTweener tweener) => {
                simpleBlit._cutoff = 1 - tweener.value.x;
            }
        ).OnComplete(()=>{
            // DestroyImmediate(simpleBlit);
            simpleBlit.enabled = false;
            onComplete();
        });
    }
}
