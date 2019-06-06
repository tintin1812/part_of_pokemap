using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCallback : MonoBehaviour {
	public delegate void OnMainCharEnter( Collider col );
    public OnMainCharEnter mOnMainCharEnter = null;
}
