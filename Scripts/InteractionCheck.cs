using System.Collections;
using System.Collections.Generic;
using AON.RpgMapEditor;
using UnityEngine;

public class InteractionCheck : MonoBehaviour {
	private bool mIsShowInteractions = true;
	private GameObject mArtCheckPoint = null;

	public bool IsShowInteractions{
		set{
			mIsShowInteractions = value;
		}
	}

	public void Init(){
		GameObject prefab = Resources.Load<GameObject>("OBJ/CheckPoint3");
		if(prefab == null){
			return;
		}
		var art = GameObject.Instantiate(prefab);
		art.transform.parent = this.transform;
		art.transform.localRotation = Quaternion.identity;
		art.transform.localPosition = new Vector3( 0 , 0, 0);
		mArtCheckPoint = art;
	}

	public void Update(){
		if(mArtCheckPoint == null){
			return;
		}
		if(mArtCheckPoint.activeSelf){
			if(TriggerGame.Instance.IsScriptMainRunning){
				mArtCheckPoint.SetActive( false);
				return;
			}
			if(!mIsShowInteractions){
				mArtCheckPoint.SetActive( false);	
				return;
			}
		}else{
			if(mIsShowInteractions && !TriggerGame.Instance.IsScriptMainRunning){
				mArtCheckPoint.SetActive( true);
			}
		}
	}

}
