/* ref: http://wiki.unity3d.com/wiki/index.php?title=PopupList
*/

using System.Collections.Generic;
using UnityEngine;

public class ComboBox
{
    private static ComboBox lastComboBoxShow = null;
    // private static int delayDontshow = 0;
    // private static int useControlID = -1;

    private static float UnitHeight = 28f;
    
    public static void ResetInstance(){
        lastComboBoxShow = null;
        // delayDontshow = 2;
        // useControlID = -1;
    }

    // public static void UpdateOnGUI(){
    //     if(delayDontshow > 0){
    //         delayDontshow--;
    //     }
    // }

    public static bool IsOnShow(){
        return (lastComboBoxShow != null);
    }
    //
    public static ComboBox CreateComboBox(int idxRef, string[] content, string empty = ""){
        int count = content.Length;
        GUIContent[] guiContent = new GUIContent[count];
        for (int i = 0; i < count; ++i)
        {
            guiContent[i] = new GUIContent( content[i]);
        }
        ComboBox comboBox = new ComboBox(new Rect(0, 0, 150, 20), guiContent, new GUIContent( empty));
        comboBox.SelectedItemIndex = idxRef;
        return comboBox;
    }

    private static GUIStyle listStyleContent = null;
    public static GUIStyle ListStyleContent{
        get{
            if(listStyleContent == null){
                listStyleContent = new GUIStyle();
                listStyleContent.alignment = TextAnchor.LowerLeft;
			    listStyleContent.normal.textColor = Color.white;
                listStyleContent.padding.left =
                listStyleContent.padding.right =
                listStyleContent.padding.top =
                listStyleContent.padding.bottom = 4;
            }
            return listStyleContent;
        }
    }

    private static GUIStyle listStyleGrid = null;
    public static GUIStyle ListStyleGrid{
        get{
            if(listStyleGrid == null){
                listStyleGrid = new GUIStyle();
            }
            return listStyleGrid;
        }
    }

    private static GUIStyle listStyleHightlight = null;
    public static GUIStyle ListStyleHightlight{
        get{
            if(listStyleHightlight == null){
                listStyleHightlight = new GUIStyle();
            }
            return listStyleHightlight;
        }
    }
    //
    public Rect Rect;
    public bool mIsDropDownListVisible = false;
    public bool IsDropDownListVisible {
        get {
            return mIsDropDownListVisible;
        }
    }

    public bool IsDropDownWithHash( string h){
        if(currentHash == h){
            return true;
        }
        return false;
    }

    private int selectedItemIndex = -1;

    public GUIContent buttonContent;
    private GUIContent empty = new GUIContent("NULL");
    public string Empty{
        set{
            if(empty == null){
                empty = new GUIContent(value);
            }else{
                empty.text = value;
            }
        }
    }
    private GUIContent[] listContent;
    public GUIContent[] ListContent{
        get {
            return listContent;
        }
        set{
            listContent = value;
        }
    }
    
    public void UpdateContentLength( int Length){
        if(listContent == null || Length != listContent.Length){
            listContent = new GUIContent[Length];
            for (int i = 0; i < Length; ++i)
            {
                listContent[i] = new GUIContent();
            }   
        }
    }

    public void UpdateListContent( List<string> keys){
        if(keys == null || keys.Count == 0){
            if(listContent == null || listContent.Length > 0){
                listContent = new GUIContent[0];
            }
            return;
        }
        if(listContent == null || keys.Count != listContent.Length){
            listContent = new GUIContent[keys.Count];
            for (int i = 0; i < keys.Count; ++i)
            {
                var k = keys[i];
                listContent[i] = new GUIContent(k);
            }
        }else {
            for (int i = 0; i < keys.Count; ++i)
            {
                var k = keys[i];
                listContent[i].text = k;
            }
        }
    }

    public void UpdateListContentAndIndex( List<string> keys, string index){
        if(keys == null || keys.Count == 0){
            if(listContent == null || listContent.Length > 0){
                listContent = new GUIContent[0];
            }
            return;
        }
        if(listContent == null || keys.Count != listContent.Length){
            listContent = new GUIContent[keys.Count];
            for (int i = 0; i < keys.Count; ++i)
            {
                var k = keys[i];
                listContent[i] = new GUIContent(k);
                if(k == index){
                    SelectedItemIndex = i;
                }
            }
        }else {
            for (int i = 0; i < keys.Count; ++i)
            {
                var k = keys[i];
                listContent[i].text = k;
                if(k == index){
                    SelectedItemIndex = i;
                }
            }
        }
    }
    private string currentHash = "";
    private string currentSearch = "";
    private int itemHightLight = -1;
    
    private Vector2 m_scrollPos = Vector2.zero;

    public ComboBox(Rect rect, int selectedIdx, string[] options)
    {
        GUIContent[] listContent = new GUIContent[options.Length];
        for (int i = 0; i < options.Length; ++i)
        {
            listContent[i] = new GUIContent(options[i]);
        }

        this.Rect = rect;
        selectedItemIndex = selectedIdx;
        if(selectedIdx >= 0 && selectedIdx < listContent.Length){
            this.buttonContent = listContent[selectedIdx];
        }else
        {
            this.buttonContent = empty;
        }
        
        this.listContent = listContent;
    }

    public ComboBox(Rect rect, GUIContent[] listContent)
    {
        this.Rect = rect;
        this.listContent = listContent;
        buttonContent = empty;
    }

    public ComboBox(Rect rect, GUIContent[] listContent, GUIContent buttonEmpty = null)
    {
        this.Rect = rect;
        this.listContent = listContent;
        if(buttonEmpty != null){
            this.empty = buttonEmpty;
        }
        this.buttonContent = empty;
    }

    /*
    public int ForceShow(float limitHeight, int indext)
    {
        GUIStyle listStyle = ComboBoxHelper.Instance.GUIStyle();
        {
            float contentHeight = listStyle.CalcHeight(listContent[0], 1.0f) * listContent.Length;
            limitHeight -= Rect.height;
            limitHeight -= 4; //Offset
            if( limitHeight <= 0 || limitHeight > contentHeight ){
                Rect listRect = new Rect(Rect.x, Rect.y + Rect.height,
                        Rect.width, contentHeight);
                AONGUI.Box(listRect, "", boxStyle);
                int newSelectedItemIndex = AONGUI.SelectionGrid(listRect, indext, listContent, 1, listStyle);
                if (newSelectedItemIndex != indext)
                {
                    indext = newSelectedItemIndex;
                }
            }else{
                Rect view = new Rect(Rect.x, Rect.y + Rect.height,
                        Rect.width, limitHeight);
                Rect listRect = new Rect(Rect.x, Rect.y + Rect.height,
                        Rect.width - 16f, contentHeight);
                m_scrollPos = AONGUI.BeginScrollView(view, m_scrollPos, listRect, false, false);
                //
                AONGUI.Box(listRect, "", boxStyle);
                int newSelectedItemIndex = AONGUI.SelectionGrid(listRect, indext, listContent, 1, listStyle);
                if (newSelectedItemIndex != indext)
                {
                    indext = newSelectedItemIndex;
                }
                //
                AONGUI.EndScrollView();
            }
        }
        return indext;
    }
    */

    // public void ForceShow(string hash = "defause"){
    //     currentHash = hash;
    // }

    public float HeightForShowAll(){
        return ListContent.Length * UnitHeight + 32f;
    }

    public delegate void OnSelect(int next);
    // public OnSelect onSelect = null;

    //float viewHeight, string hash = "defause", bool showSearch = true, bool forceShow = false, float xBoxBegin = -1, float xBoxEnd = -1
    public void Show(float viewHeight, OnSelect onSelect){
        Show(viewHeight, "defause", true, false, -1, -1, onSelect);
    }

    public void Show(float viewHeight, string hash, OnSelect onSelect){
        Show(viewHeight, hash, true, false, -1, -1, onSelect);
    }

    public void Show(float viewHeight, string hash, bool showSearch, OnSelect onSelect){
        Show(viewHeight, hash, showSearch, false, -1, -1, onSelect);
    }

    public void Show(float viewHeight, string hash, bool showSearch, bool forceShow, OnSelect onSelect){
        Show(viewHeight, hash, showSearch, forceShow, -1, -1, onSelect);
    }

    public void Show(float viewHeight, string hash, bool showSearch, bool forceShow, float xBoxBegin, float xBoxEnd, OnSelect onSelect)
    {
        // onSelect = null;
        if(forceShow){
            Rect.y = Rect.y - Rect.height;
            // viewHeight += Rect.height;
        }else
        {
            viewHeight -= Rect.height;
        }
        GUIStyle StyleContent = ListStyleContent;
        mIsDropDownListVisible = (currentHash == hash);
        bool isDropDown = mIsDropDownListVisible || forceShow; 
        // if(isDropDown && listContent.Length > 0){
        //     float contentHeightMax = UnitHeight * listContent.Length;
        //     if(showSearch){
        //         contentHeightMax += 32f;
        //     }
        //     if(viewHeight > contentHeightMax){
        //         viewHeight = contentHeightMax;
        //     }
        // }

        // bool isWillClose = false;
        if(forceShow == false) {
            AONGUI.AddOnGui((AComponent a) => {
                int controlID = GUIUtility.GetControlID(FocusType.Passive);
                switch (Event.current.GetTypeForControl(controlID))
                {
                    case EventType.MouseUp:
                        {
                            if (isDropDown)
                            {
                                float xBegin = xBoxBegin > 0 ? xBoxBegin : Rect.x;
                                float xEnd = xBoxEnd > 0 ? xBoxEnd : Rect.x + Rect.width;
                                float yEndContent = Rect.y + Rect.height + viewHeight;
                                Vector2 posMouse = Event.current.mousePosition;
                                if(posMouse.x < xBegin 
                                || posMouse.x > xEnd 
                                || posMouse.y < Rect.y
                                || posMouse.y > yEndContent){
                                    Close();
                                }
                                /*
                                if( showSearch
                                    && posMouse.y >= yEndContent - 32f
                                    && posMouse.y <= yEndContent
                                    && posMouse.x >= xBegin
                                    && posMouse.x <= xEnd) {
                                    //Click on Box Search And do not thing
                                }else if( posMouse.x >= xEnd - 16
                                    && posMouse.x <= xEnd
                                    && posMouse.y >= Rect.y
                                    && posMouse.y <= yEndContent) {
                                    // Click on Scroll and do nothing
                                }else
                                {
                                    isWillClose = true;
                                }
                                */
                            }
                        }
                        break;
                }
            });
            AONGUI.Button(new Rect(Rect.x, Rect.y, Rect.width, DefineAON.GUI_Height_Button), buttonContent, () => {
                if (currentHash == hash)
                {
                    Close();
                }else{
                    if(lastComboBoxShow == this){
                        if(currentHash != hash){
                            currentHash = hash;
                        }
                    }else
                    {
                        if(lastComboBoxShow != null){
                            lastComboBoxShow.currentHash = "";
                            lastComboBoxShow.mIsDropDownListVisible = false;
                        }
                        if(mIsDropDownListVisible == false){
                            // Open
                            mIsDropDownListVisible = true;
                            currentHash = hash;
                            lastComboBoxShow = this;
                        }else
                        {
                            Close();
                        }
                    }
                }
            });
        }

        if (isDropDown)
        {
            if(xBoxBegin > 0){
                Rect.x = xBoxBegin;
            }
            if(xBoxEnd > 0){
                Rect.xMax = xBoxEnd;
            }
            if(listContent != null && listContent.Length > 0){
                // int max_count = (int) ((viewHeight - 32) / UnitHeight);
                // if(listContent.Length > max_count){
                //     GUIContent[] listContent2 = new GUIContent[max_count];
                //     for (int i = 0; i < max_count; i++)
                //     {
                //         listContent2[i] = listContent[i];
                //     }
                //     listContent = listContent2;
                // }

                // float unitHeight = StyleContent.CalcHeight(listContent[0], 1.0f);
                float contentHeight = UnitHeight * listContent.Length;
                // viewHeight -= Rect.height;
                {
                    //Input Fillter
                    AONGUI.Box(new Rect(Rect.x, Rect.y + Rect.height, Rect.width, viewHeight), "", ListStyleGrid);
                    float heightSearch = 0;
                    if(showSearch){ //Search
                        if(!forceShow){
                            AONGUI.SetNextControlName("TextField");
                        }
                        float yGui = Rect.y + Rect.height + viewHeight - DefineAON.GUI_Height_Button;
                        AONGUI.TextField( new Rect(Rect.x, yGui, Rect.width - 50, 25), currentSearch, (string text) => {
                            currentSearch = text.ToLower();
                            itemHightLight = -1;
                            if(currentSearch != ""){
                                for(int i = 0; i < listContent.Length; i++){
                                    string t = listContent[i].text;
                                    if( !string.IsNullOrEmpty(t) && t.ToLower().IndexOf(currentSearch) >= 0){
                                        itemHightLight = i;
                                        m_scrollPos.y = UnitHeight * itemHightLight;;
                                        break;
                                    }
                                }
                            }
                        });
                        if(!forceShow){
                            AONGUI.FocusControl("TextField");
                        }
                        AONGUI.Button( new Rect(Rect.x + Rect.width - 50, yGui, 25, 25), "<", KeyCode.LeftArrow, () => {
                            if(currentSearch != ""){
                                int lastHightlight = itemHightLight - 1;
                                if(itemHightLight < 0 || itemHightLight >= listContent.Length){
                                    lastHightlight = listContent.Length -1;
                                }
                                itemHightLight = -1;
                                for(int i = lastHightlight; i >= 0; i--){
                                    string t = listContent[i].text;
                                    if(!string.IsNullOrEmpty(t) && t.ToLower().IndexOf(currentSearch) >= 0){
                                        itemHightLight = i;
                                        m_scrollPos.y = UnitHeight * itemHightLight;;
                                        break;
                                    }
                                }
                            }
                        });
                        AONGUI.Button( new Rect(Rect.x + Rect.width - 25, yGui, 25, 25), ">", KeyCode.RightArrow, () => {
                            if(currentSearch != ""){
                                int lastHightlight = itemHightLight + 1;
                                if(lastHightlight < 0 || lastHightlight >= listContent.Length){
                                    lastHightlight = 0;
                                }
                                itemHightLight = -1;
                                for(int i = lastHightlight; i < listContent.Length; i++){
                                    string t = listContent[i].text;
                                    if(!string.IsNullOrEmpty(t) && t.ToLower().IndexOf(currentSearch) >= 0){
                                        itemHightLight = i;
                                        m_scrollPos.y = UnitHeight * itemHightLight;;
                                        break;
                                    }
                                }
                            }
                        });
                        heightSearch = 32f;
                    }
                    if(!forceShow){
                        AONGUI.AddOnGui((AComponent a) => {
                            if(itemHightLight != -1){
                                if( Event.current.isKey && Event.current.keyCode == KeyCode.DownArrow){
                                    itemHightLight++;
                                    if(itemHightLight >= listContent.Length){
                                        itemHightLight = 0;
                                    }
                                    m_scrollPos.y = UnitHeight * itemHightLight;;
                                }
                                if( Event.current.isKey && Event.current.keyCode == KeyCode.UpArrow){
                                    itemHightLight--;
                                    if(itemHightLight < 0){
                                        itemHightLight = listContent.Length - 1;
                                    }
                                    m_scrollPos.y = UnitHeight * itemHightLight;;
                                }
                            }else
                            {
                                if( Event.current.isKey && (Event.current.keyCode == KeyCode.DownArrow || Event.current.keyCode == KeyCode.UpArrow)){
                                    itemHightLight = SelectedItemIndex;
                                    m_scrollPos.y = UnitHeight * itemHightLight;;
                                }
                            }
                        });
                    }
                    Rect view = new Rect(Rect.x, Rect.y + Rect.height, Rect.width, viewHeight - heightSearch);
                    Rect listRect = new Rect(Rect.x, Rect.y + Rect.height, Rect.width, contentHeight);
                    bool showScroll = listRect.height > view.height;
                    if(showScroll){
                        listRect.width = Rect.width - 16f;
                        // Optimal Grid
                        AONGUI.AddOnGui((AComponent a) => {
                            
                            m_scrollPos = GUI.BeginScrollView(view, m_scrollPos, listRect, false, showSearch ? true : false);

                            int pos_begin = (int) (m_scrollPos.y / UnitHeight);
                            int max_count = (int) ( view.height/ UnitHeight) + 1;
                            if(max_count + pos_begin > listContent.Length){
                                max_count = listContent.Length - pos_begin - 1;
                            }
                            GUIContent[] listContent2 = new GUIContent[max_count];
                            for (int i = 0; i < max_count; i++)
                            {
                                listContent2[i] = listContent[i + pos_begin];
                            }
                            Rect listRect2 = listRect;
                            listRect2.y = listRect.y + pos_begin * UnitHeight;
                            listRect2.height = max_count * UnitHeight;
                            var nextselect = GUI.SelectionGrid(listRect2, selectedItemIndex, listContent2, 1, StyleContent);
                            if(nextselect != selectedItemIndex){
                                AONGUI.Target.Actions += ()=>{
                                    SelectedItemIndex = pos_begin + nextselect;
                                    Close();
                                    onSelect(SelectedItemIndex);
                                };
                            }
                            // AONGUI.SelectionGrid(listRect2, selectedItemIndex, listContent2, 1, StyleContent, (int next) => {
                                // SelectedItemIndex = next;
                                // Close();
                                // onSelect(SelectedItemIndex);
                            // });
                            // GUI.SelectionGrid(listRect, selectedItemIndex, listContent, 1, StyleContent);

                            if(itemHightLight != -1){
                                float y = listRect.y + UnitHeight * itemHightLight;
                                GUI.Box( new Rect(listRect.x, y, listRect.width, UnitHeight), "", ListStyleHightlight);
                                if(Event.current.isKey && Event.current.keyCode == KeyCode.Return){
                                    SelectedItemIndex = itemHightLight;
                                    AONGUI.Target.Actions += ()=>{
                                        SelectedItemIndex = itemHightLight;
                                        Close();
                                        onSelect(SelectedItemIndex);
                                    };
                                }
                            }
                            GUI.EndScrollView();
                        });
                        
                    }else
                    {
                        AONGUI.AddOnGui((AComponent a) => {
                            var nextselect = GUI.SelectionGrid(listRect, selectedItemIndex, listContent, 1, StyleContent);
                            if(nextselect != selectedItemIndex){
                                AONGUI.Target.Actions += ()=>{
                                    SelectedItemIndex = nextselect;
                                    Close();
                                    onSelect(SelectedItemIndex);
                                };
                            }
                            if(itemHightLight != -1){
                                float y = listRect.y + UnitHeight * itemHightLight;
                                GUI.Box( new Rect(listRect.x, y, listRect.width, UnitHeight), "", ListStyleHightlight);
                                if(Event.current.isKey && Event.current.keyCode == KeyCode.Return){
                                    SelectedItemIndex = itemHightLight;
                                    AONGUI.Target.Actions += ()=>{
                                        SelectedItemIndex = itemHightLight;
                                        Close();
                                        onSelect(SelectedItemIndex);
                                    };
                                }
                            }
                        });
                    }
                }
            }else
            {
                // listContent is null
                // Rect view = new Rect(Rect.x, Rect.y + Rect.height, Rect.width, limitHeight - Rect.height);
                Rect view = new Rect(Rect.x, Rect.y + Rect.height, Rect.width, 32f);
                AONGUI.Box( view, "", ListStyleGrid);
                AONGUI.Label( view, "- Empty -", StyleContent);
            }
        }

        return;
    }

    private void Close(){
        mIsDropDownListVisible = false;
        currentHash = "";
        lastComboBoxShow = null;
        AONGUIBehaviour.AONGUI_ReDrawAll();
        // delayDontshow = 2;
    }

    public int SelectedItemIndex
    {
        get
        {
            return selectedItemIndex;
        }
        set
        {
            if(selectedItemIndex != value){
                selectedItemIndex = value;
                if(selectedItemIndex == -1 || selectedItemIndex < 0 || selectedItemIndex >= listContent.Length){
                    buttonContent = empty;
                }else{
                    buttonContent = listContent[selectedItemIndex];
                }
            }
        }
    }
}