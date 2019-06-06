using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable] public class MyDictionary1 : SerializableDictionary<string, int> { }
[Serializable] public class MyDictionary2 : SerializableDictionary<KeyCode, GameObject> { }

public class TriggerGameBehaviour : MonoBehaviour {

	public static TriggerGameBehaviour Instance { get; private set; }
	void Awake()
	{
		Instance = this;
	}

	// [SerializeField]
    // public TriggerGameData Data;

	public MyDictionary1 dictionary1;
	public MyDictionary2 dictionary2;

	public Flags flag;
}
