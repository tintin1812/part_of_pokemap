using System;
using System.Collections;
using System.Collections.Generic;
using AON.RpgMapEditor;
using FairyGUI;
using UnityEngine;

public class Flags : Dictionary<string, int>
{
    // [YamlMember(Alias = "input", ApplyNamingConventions = false)]
    // public Input Input { get; set; }

    // public void Dispose(){
    //     if(_eventDispatcher != null) {
    //         _eventDispatcher = null;
    //     }
    //     Debug.Log("Flags Dispose");
    // }

    ~Flags()
    {
        // Debug.Log("Flags Dispose");
        if(_eventDispatcher != null){
            _eventDispatcher.RemoveEventListeners();
        }
    }

    public Flags Clone()
    {
        Flags f = new Flags();
        foreach (var item in this)
        {
            f.Add(item.Key, item.Value);
        }
        return f;
    }

    public bool CheckLockKey(string[] lockKey = null)
    {
        if (lockKey != null)
        {
            var keys = Keys;
            bool needReset = false;
            do
            {
                if (keys.Count < lockKey.Length)
                {
                    needReset = true;
                    break;
                }
                int i = 0;
                foreach (string key in keys)
                {
                    if (lockKey[i] != key)
                    {
                        needReset = true;
                        break;
                    }
                    i++;
                    if (i >= lockKey.Length)
                    {
                        break;
                    }
                }
            } while (false);
            if (needReset)
            {
                Flags n = new Flags();
                foreach (string key in lockKey)
                {
                    if (ContainsKey(key))
                    {
                        n[key] = this[key];
                    }
                    else
                    {
                        n[key] = 0;
                    }
                }
                foreach (string key in keys)
                {
                    if (n.ContainsKey(key) == false)
                    {
                        n[key] = this[key];
                    }
                }
                Clear();
                foreach (var d in n)
                {
                    Add(d.Key, d.Value);
                }
                return true;
            }
        }
        return false;
    }

    public void ActionOperation(FlagAction action)
    {
        if (string.IsNullOrEmpty(action.Key))
        {
            return;
        }
        string key = action.Key;
        if (this.ContainsKey(key) == false)
            return;

        // int operation = action.Operation;
        // int value = action.Value;

        ScriptGui.EOperation o = (ScriptGui.EOperation)action.Operation;
        // if( o == ScriptGui.EOperation.Divi && value == 0){
        // 	InputFieldHelper.Instance.ShowNoti( "Error: Can't division with zero");
        // 	return;
        // }
        switch (o)
        {
            case ScriptGui.EOperation.Set:
            {
                this[key] = action.Value;
                break;
            }
            case ScriptGui.EOperation.Add:
            {
                this[key] = this[key] + action.Value;
                break;
            }
            case ScriptGui.EOperation.Sub:
            {
                this[key] = this[key] - action.Value;
                break;
            }
            case ScriptGui.EOperation.Multi:
            {
                this[key] = this[key] * action.Value;
                break;
            }
            case ScriptGui.EOperation.Divi:
            {
                if (action.Value == 0)
                {
                    this[key] = 0;
                }
                else
                {
                    this[key] = this[key] / action.Value;
                }
                break;
            }
            case ScriptGui.EOperation.Add_A_B:
            {
                if (action.KeyA != null && action.KeyB != null && this.ContainsKey(action.KeyA) && this.ContainsKey(action.KeyB))
                {
                    this[key] = this[action.KeyA] + this[action.KeyB];
                }
                break;
            }
            case ScriptGui.EOperation.Sub_A_B:
            {
                if (action.KeyA != null && action.KeyB != null && this.ContainsKey(action.KeyA) && this.ContainsKey(action.KeyB))
                {
                    this[key] = this[action.KeyA] - this[action.KeyB];
                }
                break;
            }
            case ScriptGui.EOperation.Multi_A_B:
            {
                if (action.KeyA != null && action.KeyB != null && this.ContainsKey(action.KeyA) && this.ContainsKey(action.KeyB))
                {
                    this[key] = this[action.KeyA] * this[action.KeyB];
                }
                break;
            }
            case ScriptGui.EOperation.Divi_A_B:
            {
                if (action.KeyA != null && action.KeyB != null && this.ContainsKey(action.KeyA) && this.ContainsKey(action.KeyB))
                {
                    var vB = this[action.KeyB];
                    if (vB == 0)
                    {
                        this[key] = 0;
                    }
                    else
                    {
                        this[key] = this[action.KeyA] / vB;
                    }
                }
                break;
            }
            case ScriptGui.EOperation.Divi_A_B_1:
            {
                if (action.KeyA != null && action.KeyB != null && this.ContainsKey(action.KeyA) && this.ContainsKey(action.KeyB))
                {
                    var vB = this[action.KeyB];
                    if (vB == 0)
                    {
                        this[key] = 0;
                    }
                    else
                    {
                        this[key] = this[action.KeyA] * 100 / vB;
                    }
                }
                break;
            }
            case ScriptGui.EOperation.Divi_A_B_2:
            {
                if (action.KeyA != null && action.KeyB != null && this.ContainsKey(action.KeyA) && this.ContainsKey(action.KeyB))
                {
                    var vA = this[action.KeyA];
                    var vB = this[action.KeyB];
                    if (vA + vB == 0)
                    {
                        this[key] = 0;
                    }
                    else
                    {
                        this[key] = vA * 100 / (vA + vB);
                    }
                }
                break;
            }
            default:
            {
                break;
            }
        }
        _onValueCharge(key);
    }

    public void DoAdd(string key, int value)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }
        if (this.ContainsKey(key) == false)
            return;
        this[key] += value;
        _onValueCharge(key);
    }

    // public delegate void ValueCharge(string key, int value);
    
    private EventDispatcher _eventDispatcher;
    // EventListener
    
    public void AddEventListener(GObject gObj, string strType, EventCallback0 updateValue)
    {
        if(_eventDispatcher == null){
            _eventDispatcher = new EventDispatcher();
        }
        if (gObj.onStage){
            _eventDispatcher.AddEventListener( strType, updateValue);
            updateValue();   
        }
        gObj.onAddedToStage.Add(()=>{
            _eventDispatcher.AddEventListener( strType, updateValue);
            updateValue();
        });
        gObj.onRemovedFromStage.Add(()=>{
            _eventDispatcher.RemoveEventListener(strType, updateValue);
        });
    }

    public void AddEventListener_RemoveByFlag(string strType, EventCallback0 updateValue)
    {
        if(_eventDispatcher == null){
            _eventDispatcher = new EventDispatcher();
        }
        _eventDispatcher.AddEventListener( strType, updateValue);
        updateValue();
    }

    private void _onValueCharge(string key)
    {
        if(_eventDispatcher == null){
            return;
        }
        _eventDispatcher.DispatchEvent(key);
    }
}


public class Flags2 : Dictionary<string, int>
{
    // [YamlMember(Alias = "input", ApplyNamingConventions = false)]
    // public Input Input { get; set; }

    // public void Dispose(){
    //     if(_eventDispatcher != null) {
    //         _eventDispatcher = null;
    //     }
    //     Debug.Log("Flags Dispose");
    // }

    ~Flags2()
    {
        // Debug.Log("Flags Dispose");
        if(_eventDispatcher != null){
            _eventDispatcher.RemoveEventListeners();
        }
    }

    public Flags Clone()
    {
        Flags f = new Flags();
        foreach (var item in this)
        {
            f.Add(item.Key, item.Value);
        }
        return f;
    }

    public bool CheckLockKey(string[] lockKey = null)
    {
        if (lockKey != null)
        {
            var keys = Keys;
            bool needReset = false;
            do
            {
                if (keys.Count < lockKey.Length)
                {
                    needReset = true;
                    break;
                }
                int i = 0;
                foreach (string key in keys)
                {
                    if (lockKey[i] != key)
                    {
                        needReset = true;
                        break;
                    }
                    i++;
                    if (i >= lockKey.Length)
                    {
                        break;
                    }
                }
            } while (false);
            if (needReset)
            {
                Flags n = new Flags();
                foreach (string key in lockKey)
                {
                    if (ContainsKey(key))
                    {
                        n[key] = this[key];
                    }
                    else
                    {
                        n[key] = 0;
                    }
                }
                foreach (string key in keys)
                {
                    if (n.ContainsKey(key) == false)
                    {
                        n[key] = this[key];
                    }
                }
                Clear();
                foreach (var d in n)
                {
                    Add(d.Key, d.Value);
                }
                return true;
            }
        }
        return false;
    }

    public void ActionOperation(FlagAction action)
    {
        if (string.IsNullOrEmpty(action.Key))
        {
            return;
        }
        string key = action.Key;
        if (this.ContainsKey(key) == false)
            return;

        // int operation = action.Operation;
        // int value = action.Value;

        ScriptGui.EOperation o = (ScriptGui.EOperation)action.Operation;
        // if( o == ScriptGui.EOperation.Divi && value == 0){
        // 	InputFieldHelper.Instance.ShowNoti( "Error: Can't division with zero");
        // 	return;
        // }
        switch (o)
        {
            case ScriptGui.EOperation.Set:
            {
                this[key] = action.Value;
                break;
            }
            case ScriptGui.EOperation.Add:
            {
                this[key] = this[key] + action.Value;
                break;
            }
            case ScriptGui.EOperation.Sub:
            {
                this[key] = this[key] - action.Value;
                break;
            }
            case ScriptGui.EOperation.Multi:
            {
                this[key] = this[key] * action.Value;
                break;
            }
            case ScriptGui.EOperation.Divi:
            {
                if (action.Value == 0)
                {
                    this[key] = 0;
                }
                else
                {
                    this[key] = this[key] / action.Value;
                }
                break;
            }
            case ScriptGui.EOperation.Add_A_B:
            {
                if (action.KeyA != null && action.KeyB != null && this.ContainsKey(action.KeyA) && this.ContainsKey(action.KeyB))
                {
                    this[key] = this[action.KeyA] + this[action.KeyB];
                }
                break;
            }
            case ScriptGui.EOperation.Sub_A_B:
            {
                if (action.KeyA != null && action.KeyB != null && this.ContainsKey(action.KeyA) && this.ContainsKey(action.KeyB))
                {
                    this[key] = this[action.KeyA] - this[action.KeyB];
                }
                break;
            }
            case ScriptGui.EOperation.Multi_A_B:
            {
                if (action.KeyA != null && action.KeyB != null && this.ContainsKey(action.KeyA) && this.ContainsKey(action.KeyB))
                {
                    this[key] = this[action.KeyA] * this[action.KeyB];
                }
                break;
            }
            case ScriptGui.EOperation.Divi_A_B:
            {
                if (action.KeyA != null && action.KeyB != null && this.ContainsKey(action.KeyA) && this.ContainsKey(action.KeyB))
                {
                    var vB = this[action.KeyB];
                    if (vB == 0)
                    {
                        this[key] = 0;
                    }
                    else
                    {
                        this[key] = this[action.KeyA] / vB;
                    }
                }
                break;
            }
            case ScriptGui.EOperation.Divi_A_B_1:
            {
                if (action.KeyA != null && action.KeyB != null && this.ContainsKey(action.KeyA) && this.ContainsKey(action.KeyB))
                {
                    var vB = this[action.KeyB];
                    if (vB == 0)
                    {
                        this[key] = 0;
                    }
                    else
                    {
                        this[key] = this[action.KeyA] * 100 / vB;
                    }
                }
                break;
            }
            case ScriptGui.EOperation.Divi_A_B_2:
            {
                if (action.KeyA != null && action.KeyB != null && this.ContainsKey(action.KeyA) && this.ContainsKey(action.KeyB))
                {
                    var vA = this[action.KeyA];
                    var vB = this[action.KeyB];
                    if (vA + vB == 0)
                    {
                        this[key] = 0;
                    }
                    else
                    {
                        this[key] = vA * 100 / (vA + vB);
                    }
                }
                break;
            }
            default:
            {
                break;
            }
        }
        _onValueCharge(key);
    }

    public void DoAdd(string key, int value)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }
        if (this.ContainsKey(key) == false)
            return;
        this[key] += value;
        _onValueCharge(key);
    }

    // public delegate void ValueCharge(string key, int value);
    
    private EventDispatcher _eventDispatcher;
    // EventListener
    
    public void AddEventListener(GObject gObj, string strType, EventCallback0 updateValue)
    {
        if(_eventDispatcher == null){
            _eventDispatcher = new EventDispatcher();
        }
        if (gObj.onStage){
            _eventDispatcher.AddEventListener( strType, updateValue);
            updateValue();   
        }
        gObj.onAddedToStage.Add(()=>{
            _eventDispatcher.AddEventListener( strType, updateValue);
            updateValue();
        });
        gObj.onRemovedFromStage.Add(()=>{
            _eventDispatcher.RemoveEventListener(strType, updateValue);
        });
    }

    public void AddEventListener_RemoveByFlag(string strType, EventCallback0 updateValue)
    {
        if(_eventDispatcher == null){
            _eventDispatcher = new EventDispatcher();
        }
        _eventDispatcher.AddEventListener( strType, updateValue);
        updateValue();
    }

    private void _onValueCharge(string key)
    {
        if(_eventDispatcher == null){
            return;
        }
        _eventDispatcher.DispatchEvent(key);
    }
}