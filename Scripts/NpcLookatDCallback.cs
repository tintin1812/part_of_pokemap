using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcLookatDCallback : MonoBehaviour {

	public static NpcLookatDCallback InstanceOf(GameObject npcOBJ){
		var c = npcOBJ.GetComponent<NpcLookatDCallback>();
		if(c == null){
			c = npcOBJ.AddComponent<NpcLookatDCallback>();
		}
		return c;
	}

	public delegate void OnFinishLook( NpcLookatDCallback n );
    private OnFinishLook mOnFinish = null;
	private Quaternion mLookRotation;
	private bool mRequest = false;

	public void StopLock(){
		// if(mOnFinish != null){
		// 	Debug.LogWarning("Can't stop Lock when Waiting");
		// 	return;
		// }
		mOnFinish = null;
		mRequest = false;
	}

	public void LookTo( Transform to, OnFinishLook callback){
		Vector3 d = to.position - transform.position;
		LookTo(d, callback);
	}

	public void LookTo( Vector3 d, OnFinishLook callback){
		mLookRotation = Quaternion.LookRotation(d.normalized);
        mOnFinish = callback;
		mRequest = true;
    }

	public void Update(){
		if(mRequest){
			transform.rotation = Quaternion.Slerp(transform.rotation, mLookRotation, Time.deltaTime * 5);
			var angleNPC = Quaternion.Angle( transform.rotation, mLookRotation );
			// Debug.Log(angleNPC.ToString());
			if( angleNPC <= 0.5){
				mRequest = false;
				if(mOnFinish != null){
					var callBack = mOnFinish;
					mOnFinish = null;
					callBack( this);
				}
			}
		}	
	}
}
