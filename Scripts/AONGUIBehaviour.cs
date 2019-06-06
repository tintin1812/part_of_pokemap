using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AONGUIBehaviour : MonoBehaviour {

	protected abstract void OnGUIAON();

	public List<AComponent> AComponents = new List<AComponent>();

	public delegate void Action();
	public event Action Actions;

	public bool IsClearNext = false;

	public static List<AONGUIBehaviour> ALL = new List<AONGUIBehaviour>();

	void OnDisable()
    {
        ALL.Remove(this);
    }

    void OnEnable()
    {
        ALL.Add(this);
    }
 
	void OnGUI()
	{
		if(AONGUI.Target != null){
			Debug.LogError("Just 1 Target in 1 call");
			return;
		}
		if(Actions != null){
			// Wait active action
			return;
		}
		// Begin
		AONGUI.Target = this;

		if(IsClearNext){
			AComponents.Clear();
			IsClearNext = false;
		}
		if(AComponents.Count == 0){
			Debug.Log("OnGUIAON");
			OnGUIAON();
		}
		for (int i = 0; i < AComponents.Count; i++)
		{
			var a = AComponents[i];
			a.draw(a);
			if(Actions != null){
				break;
			}
			// if(AONGUI.changed){
			// 	break;
			// }
		}
		if(AONGUI.changed){
			AComponents.Clear();
		}
		//End
		AONGUI.Target = null;
	}

	public void AONGUI_ReDraw(){
		IsClearNext = true;
	}

	public static void AONGUI_ReDrawAll(){
		for (int i = 0; i < ALL.Count; i++)
		{
			ALL[i].IsClearNext = true;
		}
	}
	
	public virtual void Update()
	{
		if(Actions != null){
			Actions();
			Actions = null;
			IsClearNext = true;
		}
	}
}
