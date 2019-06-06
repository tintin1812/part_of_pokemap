using System.Collections;
using System.Collections.Generic;
using AON.RpgMapEditor;
using UnityEngine;

public class HitCount : MonoBehaviour {
    public int Hit = 0;
}

public class AgentCollision : MonoBehaviour {
    
    public void MakeShouldBeExitTrigger(){
        shouldBeExitTrigger = true;
        waitForFixBugUnity = true;
        IEnumerator coroutine = FixBug();
        StartCoroutine(coroutine);
    }


    private bool waitForFixBugUnity = false;
    private IEnumerator FixBug(){
        yield return null;
        yield return null;
        yield return null;
        //wait for 3 frame
        waitForFixBugUnity = false;
        //
        var boxGoOut = GameObject.Find("ShouldBeExitTrigger");
        if(boxGoOut == null)
            boxGoOut = new GameObject();
        boxGoOut.transform.position = transform.position;
        boxGoOut.transform.localRotation = Quaternion.identity;
        boxGoOut.name = "ShouldBeExitTrigger";
        var col = boxGoOut.AddComponent<BoxCollider>();
        col.size = new Vector3(1.0f, 1.0f, 1.0f);
        col.center = new Vector3(0, 0.5f, 0);
        col.isTrigger =true;
        
        yield break;
    }

    private bool shouldBeExitTrigger = false;

	// void OnCollisionEnter(Collision col)
    // {
	// 	Debug.Log("OnCollisionEnter: " + col.ToString());
		// foreach(ContactPoint contact in col.contacts) {
        //     //checking the individual collisions
        //     if(contact.Equals(this.target))
        //     {
        //         if(!attacking) {
        //             Debug.Log("hitting target");
        //         } else {
        //             Debug.Log("dying");
        //             //engage death sequence
        //         }
        //     }
        // }
    // }

    void OnTriggerEnter(Collider col) {
        /*
        {//Fix Bug muti call OnTriggerEnter
            HitCount h = col.GetComponent<HitCount>();
            if(h == null){
                h = col.gameObject.AddComponent<HitCount>();
            }
            h.Hit = h.Hit + 1;
            if(h.Hit != 1){
                return;
            }
        }
        */
        if(waitForFixBugUnity){
            return;
        }
        Debug.Log("OnTriggerEnter: " + col.name);
        if(shouldBeExitTrigger){
            return;
        }
        if(!InputFieldHelper.Instance.IsMainCharCanMove()){
            return;
        }
        if(col.name == "GoOutInterior"){
            Debug.Log("GoOutInterior");
            AutoTileMap_Editor.Instance.GoOutInterior();
            return;
        }
        var s = col.name.Split('_');
        if(s.Length >= 2){
            if(s[0] == "Warps"){
                var triggerRef = int.Parse(s[1]);
                if( triggerRef >= 0 && triggerRef < AutoTileMap_Editor.Instance.MapSelect.WarpsData.Count){
                    var w = AutoTileMap_Editor.Instance.MapSelect.WarpsData[triggerRef];
                    Debug.Log("GoTo: " + w.map + "_" + w.x + "_" + w.y);
                    AutoTileMap_Editor.Instance.WarpsTo( col, w.map, w.x, w.y);
                    // AutoTileMap_Editor.Instance.SetModePlay(true, w.map);
                }
            }else if(s[0] == "House"){
                var houseRef = int.Parse(s[1]);
                if( houseRef >= 0 && houseRef < AutoTileMap_Editor.Instance.MapSelect.HouseData.Count){
                    var ho = AutoTileMap_Editor.Instance.MapSelect.HouseData[houseRef];
                    if(ho.IdxInterior >= 0){
                        Debug.Log("GoIn Interior " + ho.IdxInterior);
                        AutoTileMap_Editor.Instance.GoToInterior(col, houseRef, ho.IdxInterior, ho.OffsetOut);
                        // int y = Mathf.RoundToInt(col.transform.localEulerAngles.y + 180) % 360;
                        // Debug.Log(y.ToString());
                        // if(y == 270){
                        //     y = 90;
                        // }else if(y == 90){
                        //     y = 270;
                        // }
                        // AutoTileMap_Editor.Instance.ResetMainCam();
                    }
                    // AutoTileMap_Editor.Instance.WarpsTo( w.map, w.x, w.y);
                    // AutoTileMap_Editor.Instance.SetModePlay(true, w.map);
                }
            }else if(s[0] == "Script"){
                var triggerRef = int.Parse(s[1]);
                if( triggerRef >= 0 && triggerRef < AutoTileMap_Editor.Instance.MapSelect.ScriptData.Count){
                    TriggerGame.Instance.OnScriptEnter( col.gameObject, triggerRef);
                }
            }else if(s[0] == "Person"){
                var PersonRef = int.Parse(s[1]);
                if( PersonRef >= 0 && PersonRef < AutoTileMap_Editor.Instance.MapSelect.NPCData.Count){
                    col.transform.SetSiblingIndex(0);
                    TriggerGame.Instance.OnNPCEnter( col.gameObject, PersonRef);
                }
            }else if(s[0] == "PersonInHouse"){
                var PersonRef = int.Parse(s[1]);
                if( PersonRef >= 0 && PersonRef < AutoTileMap_Editor.Instance.MapSelect.NPCData.Count){
                    col.transform.SetSiblingIndex(0);
                    // TriggerGame.Instance.OnNPCInHouseEnter( PersonRef);
                    TriggerGame.Instance.OnNPCEnter( col.gameObject, PersonRef);
                }
            }
        }
        var c = col.gameObject.GetComponent<CollisionCallback>();
        if(c != null && c.mOnMainCharEnter != null){
            c.mOnMainCharEnter( col);
        }
    }

    void OnTriggerStay(Collider col) {
    }
    
    void OnTriggerExit(Collider col) {
        if(waitForFixBugUnity){
            return;
        }
        Debug.Log("OnTriggerExit: " + col.name);
        if(shouldBeExitTrigger){
            if(col.name == "ShouldBeExitTrigger"){
                shouldBeExitTrigger = false;
                Debug.Log("Has exit done");
                Destroy(col.gameObject);
            }
            return;
        }
        if(col.name == "ShouldBeExitTrigger"){
            //Bug unity
            return;
        };
        var s = col.name.Split('_');
        if( s.Length >= 2){
            if(s[0] == "Person"){
                TriggerGame.Instance.OnNPCExit( col.gameObject);
            }
        }
    }
}
