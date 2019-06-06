using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableFlag : Dictionary<string, int>{
	/*
	public List<string> k = new List<string>();
	public List<int> v = new List<int>();

	public Flags FlagsYaml{
		get{
			var d = new Flags();
			if(k.Count == v.Count){
				for( int i = 0; i < k.Count; i++){
					d[k[i]] = v[i];
				}
			}
			return d;
		}
		set{
			k.Clear();
			v.Clear();
			foreach(KeyValuePair<string, int> pair in value)
			{
				k.Add(pair.Key);
				v.Add(pair.Value);
			}
		}
	}
	*/

	public Flags Data{
		get{
			var d = new Flags();
			foreach (var item in this)
			{
				d[item.Key] = item.Value;	
			}
			return d;
		}
		set{
			Clear();
			foreach(KeyValuePair<string, int> pair in value)
			{
				Add(pair.Key, pair.Value);
			}
		}
	}
}