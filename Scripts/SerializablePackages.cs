using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class SerializablePackages {
	

	[Serializable]
	public class PayBy {
		public int coin;
	}

	[Serializable]
	public class Item {
		public string property;
		public PayBy payBy;
	}
	
	[Serializable]
    public class Items : AONHash<Item>
    {
        protected override string GetKey(Item item)
        {
            return item.property;
        }

        protected override void SetKey(Item item, string key)
        {
            item.property = key;
        }
    }

	[Serializable]
	public class Package {

		public Package(string k)
        {
            Key = k;
        }

		public string Key = "";

		public string Name = "";
		
		public string Des = "";

		// public List<string> propertys = new List<string>();
		// public List<PayBy> payBys = new List<PayBy>();

		public Items data = new Items();
		
		public void AddProperty( string slugName){
			// propertys.Add(slugName);
			// payBys.Add( new PayBy());
			data.Add( new Item(){
				property = slugName,
				payBy = new PayBy()
			});
		}

		public void Remove( int index){
			if( index < 0 || index >= data.Count){
				return;
			}
			data.RemoveAt(index);
		}

		public void RemoveAll(){
			data.Clear();
		}
	}

	[Serializable]
    public class Packages : AONHash<Package>
    {
        protected override string GetKey(Package item)
        {
            return item.Key;
        }

        protected override void SetKey(Package item, string key)
        {
            item.Key = key;
        }
    }

	[SerializeField]
    private Packages all = new Packages();

	public void Add( string slugName){
		Package p = new Package(slugName);
		all.Add(p);
	}

	public void Remove( int index){
		all.RemoveAt( index);
	}

	public int Copy( int slugIndex){
		return all.Copy(slugIndex);
	}

	//
	public int Count {
		get{
			return all.Count;		
		}
	}

	public int IndexOf( string slugName){
		return all.IndexOf( slugName);
	}

	public Package PackageBySlug(string slugName){
		var index = all.IndexOf(slugName);
		if(index < 0)
			return null;
		return all[index];
	}

    public List<string> AllKey{
        get{
            return all.Keys;
        }
    }

	public string SlugByIndex(int index){
        if(index < 0 || index >= all.Count){
            return "";
        }
        return all[index].Key;
    }

	public Package PackageByIndex(int index)
    {
        if(index < 0 || index >= all.Count){
            return null;
        }
        return all[index];
    }
}
