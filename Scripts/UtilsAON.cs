using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
abstract public class AONHash<T> : List<T>
{
    abstract protected string GetKey(T item);
    abstract protected void SetKey(T item, string key);

    // [SerializeField]
    // private List<T> data = new List<T>();

    private List<T> data{
        get{
            return this;
        }
    }

    [NonSerialized]
    private List<string> _k = null;
    public void ResetKeys(){
        _k = null;
    }

    public List<string> Keys
    {
        get
        {
            if (_k == null || _k.Count != data.Count)
            {
                _k = new List<string>(data.Count);
                for (int i = 0; i < data.Count; i++)
                {
                    _k.Add(GetKey(data[i]));
                }
            }
            return _k;
        }
    }

    public List<T> Data
    {
        get
        {
            return data;
        }
    }

    public new void Add(T item)
    {
        Keys.Add(GetKey(item));
        data.Add(item);
    }

    public new void RemoveAt(int index)
    {
        if (index < 0 || index >= data.Count)
        {
            return;
        }
        Keys.RemoveAt(index);
        data.RemoveAt(index);
    }

    public new void Clear()
    {
        Keys.Clear();
        data.Clear();
    }

    public new int Count
    {
        get
        {
            return data.Count;
        }
    }

    public new T this[int index]
    {
        get
        {   
            return data[index];
        }
    }

    public int IndexOf(string slugName)
    {
        return Keys.IndexOf(slugName);
    }

    public int Copy(int slugIndex)
    {
        if (slugIndex < 0 || slugIndex >= data.Count)
        {
            return -1;
        }
        var old_key = GetKey(data[slugIndex]);
        string new_key;
        int l = old_key.LastIndexOf('_');
        if (l < 0 || l == old_key.Length - 1)
        {
            new_key = old_key + "_1";
        }
        else
        {
            int next_l = old_key.Length - l - 1;
            string sub_b = old_key.Substring(l + 1, next_l);
            // int i = int.Parse(sub_b);
            int i = 0;
            Int32.TryParse(sub_b, out i);
            i++;
            new_key = old_key.Substring(0, l + 1) + i.ToString();
        }
        var o = Keys.IndexOf(new_key);
        if (o >= 0)
        {
            return o;
        }
        var old_value = data[slugIndex];
        var n = UtilsAON.DeepCopy(old_value);
        if (n == null)
        {
            return -1;
        }
        var new_value = (T)n;
        if (new_value == null)
        {
            return -1;
        }
        SetKey(new_value, new_key);
        Add(new_value);
        return data.Count - 1;
    }
}

public class ForceJSONSerializePrivatesResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
{
    JsonProperty _Type;
    JsonProperty Type;
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);
        if(!property.Ignored){
            if(!property.Writable){
                property.Ignored = true;
            }
            // if(property.PropertyName == "_Type"){
            //     _Type = property; 
            // }
            // if(property.PropertyName == "Type"){
            //     Type = property; 
            // }
            // if( property.PropertyName == "Propertys"){
            //     Debug.Log("");
            // }
            // if(property.PropertyType.IsAnsiClass){
            //     Debug.Log(property.PropertyName);
            // }
        }
        return property;
    }
}

public class UtilsAON
{
    static ForceJSONSerializePrivatesResolver ContractResolver = new ForceJSONSerializePrivatesResolver();
		 
    static JsonSerializerSettings Setting = new JsonSerializerSettings()
    {
        Formatting = Newtonsoft.Json.Formatting.Indented,
        ReferenceLoopHandling = ReferenceLoopHandling.Error,
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore,
        Error = HandleDeserializationError
        // ContractResolver = ContractResolver,
        // PreserveReferencesHandling = PreserveReferencesHandling.None
    };

    static void HandleDeserializationError(object sender, ErrorEventArgs errorArgs)
    {
        var currentError = errorArgs.ErrorContext.Error.Message;
        Debug.Log( currentError);
        errorArgs.ErrorContext.Handled = true;
    }

    static JsonSerializerSettings SettingShare{
        get{
            ContractResolver.DefaultMembersSearchFlags |= System.Reflection.BindingFlags.NonPublic;
            Setting.ContractResolver = ContractResolver;
            return Setting;
        }
    }

    static public bool IsDuringSerializeObject = false;
    
    public static string SerializeObject(object value){
        IsDuringSerializeObject = true;
        string json = JsonConvert.SerializeObject(value, SettingShare);
        IsDuringSerializeObject = false;
        return json;
    }

    public static T DeserializeObject<T>(string value){
        return JsonConvert.DeserializeObject<T>(value, SettingShare);
    }

    //---------------//
    static public Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    public static object DeepCopy(object obj)
    {
        if (obj == null)
            return null;
        Type type = obj.GetType();

        if (type.IsValueType || type == typeof(string))
        {
            return obj;
        }
        else if (type.IsArray)
        {
            Type elementType = Type.GetType(
                 type.FullName.Replace("[]", string.Empty));
            var array = obj as Array;
            Array copied = Array.CreateInstance(elementType, array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                copied.SetValue(DeepCopy(array.GetValue(i)), i);
            }
            return Convert.ChangeType(copied, obj.GetType());
        }
        else if (type.IsClass)
        {

            object toret = Activator.CreateInstance(obj.GetType());
            FieldInfo[] fields = type.GetFields(BindingFlags.Public |
                        BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                object fieldValue = field.GetValue(obj);
                if (fieldValue == null)
                    continue;
                field.SetValue(toret, DeepCopy(fieldValue));
            }
            return toret;
        }
        else
            throw new ArgumentException("Unknown type");
    }

    public static int StrToIntDef(string s, int def = 0)
    {
        if (s == "-" || s == "-0")
        {
            return -1;
        }
        int temp;
        if (!Int32.TryParse(s, out temp))
            return def;
        return temp;
    }

    public static Transform FindDescendentTransform(Transform searchTransform, string descendantName)
    {
        Transform result = null;

        int childCount = searchTransform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform childTransform = searchTransform.GetChild(i);

            // Not it, but has children? Search the children.
            if (childTransform.name.ToLower() != descendantName
            && childTransform.childCount > 0)
            {
                Transform grandchildTransform = FindDescendentTransform(childTransform, descendantName);
                if (grandchildTransform == null)
                    continue;

                result = grandchildTransform;
                break;
            }
            // Not it, but has no children?  Go on to the next sibling.
            else if (childTransform.name.ToLower() != descendantName
                    && childTransform.childCount == 0)
            {
                continue;
            }

            // Found it.
            result = childTransform;
            break;
        }

        return result;
    }

    public static string NameFormat(string value)
    {
        char[] array = value.ToCharArray();
        // Handle the first letter in the string.  
        if (array.Length >= 1)
        {
            if (char.IsLower(array[0]))
            {
                array[0] = char.ToUpper(array[0]);
            }
        }
        // Scan through the letters, checking for spaces.  
        // ... Uppercase the lowercase letters following spaces.
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i] == '_')
            {
                array[i] = ' ';
            }else if (array[i - 1] == ' ')
            {
                if (char.IsLower(array[i]))
                {
                    array[i] = char.ToUpper(array[i]);
                }
            }
        }
        return new string(array);
    }

    public static string DesFormat(string value)
    {
        char[] array = value.ToCharArray();
        if (array.Length >= 1)
        {
            if (char.IsLower(array[0]))
            {
                array[0] = char.ToUpper(array[0]);
            }
        }
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i] == '_')
            {
                array[i] = ' ';
            }
        }
        return new string(array);
    }

    public static void WarpTo(Transform transform, Vector3 pos)
    {
        transform.GetComponent<NavMeshAgent>().Warp(pos);
    }
}
