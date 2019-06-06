using System.Collections;
using System.Collections.Generic;
using AON.RpgMapEditor;
using UnityEngine;

public class TriggerDetail {
	public int refMap;
	public eSlotAonTypeObj typeObj;
	public int refId;
	public bool isCreateFromMap;
	public int x;
	public int y;
	private string hash = "";
	public string Hash{
		get{
			if(hash == ""){
				// hash = refMap + "_" + typeObj.ToString() + "_" + refId + "_" + (isCreateFromMap ? "T_" : "F_") + x + "_" + y;
				hash = typeObj.ToString() + "_" + refId + "_" + refMap + "_" + (isCreateFromMap ? "T_" : "F_") + x + "_" + y;
			}
			return hash;
		}
	}
}

public class TriggerDetailBehaviour : MonoBehaviour {
	public TriggerDetail Data;
}
