using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcLookatMainCallback : MonoBehaviour {
	
	public static NpcLookatMainCallback InstanceOf(GameObject npcOBJ){
		var c = npcOBJ.GetComponent<NpcLookatMainCallback>();
		if(c == null){
			c = npcOBJ.AddComponent<NpcLookatMainCallback>();
		}
		return c;
	}

	public delegate void OnFinishLook( NpcLookatMainCallback n );
    private OnFinishLook mOnFinish = null;
	private bool mRequestLockBack;
	private GameObject mAgent;

	public bool LookTo( GameObject agent, bool requestLockBack, OnFinishLook callback){
		if(mOnFinish != null){
			return false;
		}
		mAgent = agent;
		mRequestLockBack = requestLockBack;
        mOnFinish = callback;
		return true;
    }

	public void StopLock(){
		// if(mOnFinish != null){
		// 	Debug.LogWarning("Can't stop Lock when Waiting");
		// 	return;
		// }
		mAgent = null;
        mOnFinish = null;
	}


	public void Update(){
		if(mAgent != null){
			var npcOBJ = gameObject;
			var agent = mAgent;
			Vector3 direction = (agent.transform.position - npcOBJ.transform.position).normalized;
			Quaternion lookRotation = Quaternion.LookRotation(direction);
			//Char
			float angleChar = 0;
			if(mRequestLockBack){
				Quaternion i_lookRotation = Quaternion.LookRotation((npcOBJ.transform.position - agent.transform.position).normalized);
				agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, i_lookRotation, Time.deltaTime * 5);
				angleChar = Quaternion.Angle( agent.transform.rotation, i_lookRotation );
			}
			//NPC
			npcOBJ.transform.rotation = Quaternion.Slerp(npcOBJ.transform.rotation, lookRotation, Time.deltaTime * 5);
			var angleNPC = Quaternion.Angle( npcOBJ.transform.rotation, lookRotation );
			if(angleChar <= 1 && angleNPC <= 1){
				mAgent = null;
				if(mOnFinish != null){
					var callBack = mOnFinish;
					mOnFinish = null;
					callBack( this);
				}
			}
		}	
	}

}
