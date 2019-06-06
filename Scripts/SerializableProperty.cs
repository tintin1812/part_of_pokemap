using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


[Serializable]
public class SerializablePropertys
{   
    public enum EType : int
    {
        Outfit = 0,
        Item_Consume = 1,
        Item_NotConsume = 2,
        Certificates = 3,
        Pet = 4,
        House = 5,
    }

    public static string[] StrEType = Enum.GetNames(typeof(EType));

    [Serializable]
    public class Property
    {
        public string Key = "";

        public Property()
        {
        }

        public Property(string k)
        {
            Key = k;
        }

        private int Type = (int)EType.Outfit;

        [JsonIgnore]
        public EType _Type
        {
            get
            {
                return (EType)Type;
            }
            set
            {
                int to = (int)value;
                if (Type != to)
                {
                    Type = to;
                    RefSlug = null;
                    ActionUsing = null;
                    RefIcon = null;
                }
            }
        }

        public string Name;

        public string Des;

        // public bool StackInBag = true;

        // public bool Consume = false;
        public bool StackInBag
        {
            get
            {
                return _Type == EType.Item_Consume;
            }
        }

        public bool IsOutfit
        {
            get
            {
                return _Type == EType.Outfit;
            }
        }

        public bool IsItem
        {
            get
            {
                return _Type == EType.Item_Consume || _Type == EType.Item_NotConsume;
            }
        }

        public bool IsCertificates
        {
            get
            {
                return _Type == EType.Certificates;
            }
        }

        public bool IsPet
        {
            get
            {
                return _Type == EType.Pet;
            }
        }

        public string RefSlug;

        // public bool CanUsing = false;

        public string ActionUsing;

        public string RefIcon;
    }

    [Serializable]
    public class Propertys : AONHash<Property>
    {
        protected override string GetKey(Property item)
        {
            return item.Key;
        }

        protected override void SetKey(Property item, string key)
        {
            item.Key = key;
        }
    }

    [SerializeField]
    private Propertys all = new Propertys();
    // [SerializeField]
    // public Propertys Items = new Propertys();
    // [SerializeField]
    // public Propertys Certificates = new Propertys();
    // [SerializeField]
    // public Propertys Pet = new Propertys();
    // [SerializeField]
    // public Propertys House = new Propertys();

    // public Propertys GetByType( EType t){
    //     return Outfit;
    // }
    

    // public Property PropertyBySlug(EType type, string slugName)
    // {
    //     var index = Outfit.key.IndexOf(slugName);
    //     if (index < 0)
    //         return null;
    //     return Outfit[index];
    // }

    public SerializablePropertys.Property Add(string slugName)
    {
        SerializablePropertys.Property p = new SerializablePropertys.Property(slugName);
        all.Add(p);
        return p;
    }

    public void Remove(int index)
    {
        all.RemoveAt(index);
    }

    public int Copy(int slugIndex)
    {
        return all.Copy(slugIndex);
    }

    //

    public int Count
    {
        get
        {
            return all.Count;
        }
    }

    public int IndexOf(string slugName)
    {
        return all.Keys.IndexOf(slugName);
    }

    public List<string> AllKey{
        get{
            return all.Keys;
        }
    }

    public Property PropertyBySlug(string key)
    {
        var index = all.IndexOf(key);
        if (index < 0)
            return null;
        return all[index];
    }

    public string SlugByIndex(int index){
        if(index < 0 || index >= all.Count){
            return "";
        }
        return all[index].Key;
    }

    public Property PropertyByIndex(int index)
    {
        if(index < 0 || index >= all.Count){
            return null;
        }
        return all[index];
    }
}
