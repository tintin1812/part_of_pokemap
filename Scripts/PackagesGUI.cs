using System.Collections;
using System.Collections.Generic;
using AON.RpgMapEditor;
using UnityEngine;

public class PackagesGUI
{

    private static PackagesGUI _instance = null;

    public static PackagesGUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PackagesGUI();
            }
            return _instance;
        }
    }

    // private string SlugCurrentTarget = "";
    private ComboBox comboBoxSlug = new ComboBox(new Rect(200, 0, 200, 32), null, new GUIContent("Not selected"));
    private int slugIndex = -1;

    private ComboBox comboBoxItem = new ComboBox(new Rect(200, 0, 200, 32), null, new GUIContent("Not selected"));
    private int itemIndex = -1;

    public ComboBox ComboBoxSlug(SerializablePackages data)
    {
        comboBoxSlug.UpdateListContent(data == null ? null : data.AllKey);
        return comboBoxSlug;
    }

    public delegate void OnPickSlugItem( string slug);
    public void PickSlugItem(string hash, SerializablePackages data, string slugCurrent, float x, float y, float w, ref float yGui, ref bool isWaitUI, OnPickSlugItem onPick)
    {
        int idProperty = data.IndexOf(slugCurrent);
        var comboBox = PackagesGUI.Instance.ComboBoxSlug(data);
        if (slugCurrent == null || slugCurrent == "")
        {
            comboBox.Empty = "NULL";
        }
        else
        {
            comboBox.Empty = slugCurrent + " (Not found)";
        }
        comboBox.SelectedItemIndex = idProperty;
        comboBox.Rect.x = x;
        comboBox.Rect.y = y;
        comboBox.Rect.width = w;
        comboBox.Rect.height = 32f;
        float limitHeight = 32f * 6;
        comboBox.Show(limitHeight, hash, (int idNext) => {
            onPick(data.SlugByIndex(idNext));
        });
        if (comboBox.IsDropDownWithHash(hash))
        {
            yGui += limitHeight;
            isWaitUI = true;
        }
    }

    private string slugNameAdd = "";

    public void OnGUI(SerializablePackages packages, SerializablePropertys propertys, List<FlagAction> listFlagAction, TilesetAON tilesetAON, Rect rect)
    {
        float height_top = 68;
        AONGUI.Box(new Rect(rect.x, rect.y, rect.width, height_top), "", tilesetAON.ListStyleBlack2);
        if (OnTopMenu(packages, tilesetAON, new Rect(rect.x, rect.y, rect.width, rect.height)))
        {
            return;
        }
        if (OnGuiBot(packages, propertys, listFlagAction, tilesetAON, new Rect(rect.x, height_top, rect.width, rect.height)))
        {
            return;
        }
    }

    private bool OnTopMenu(SerializablePackages data, TilesetAON tilesetAON, Rect rect)
    {
        float widthLeft = 200;
        //menu
        float yGui = rect.y;
        AONGUI.Label(new Rect(rect.x + 4, yGui + DefineAON.GUI_Y_Label, widthLeft, DefineAON.GUI_Height_Label), "Property edit :");
        yGui += 32f;
        float xGui = rect.x + 4;
        AONGUI.Label(new Rect(xGui, yGui + DefineAON.GUI_Y_Label, 90, DefineAON.GUI_Height_Label), "Slug property");
        xGui += 94;
        AONGUI.TextField(new Rect(xGui, yGui + DefineAON.GUI_Y_TextField, 200, DefineAON.GUI_Height_TextField), slugNameAdd, (string text) => {
            slugNameAdd = text;
        });
        xGui += 204;
        if (slugNameAdd.Length == 0)
        {
            // GUI.Label(new Rect( rect.x, yGui + DefineAON.GUI_Y_Label, 200, DefineAON.GUI_Height_Label ), "Input slug property");
        }
        else
        {
            bool isUnique = true;
            var keys = data.AllKey;
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i] == slugNameAdd)
                {
                    isUnique = false;
                    break;
                }
            }
            if (isUnique)
            {
                AONGUI.Button(new Rect(xGui, yGui + DefineAON.GUI_Y_Button, 80, DefineAON.GUI_Height_Button), "Add (Enter)", KeyCode.Return, () => {
                    data.Add(slugNameAdd);
                    slugNameAdd = "";
                });
            }
            else
            {
                AONGUI.Label(new Rect(xGui, yGui + DefineAON.GUI_Y_Label, widthLeft, DefineAON.GUI_Height_Label), "Slug should be unique");
            }
        }
        return false;
    }

    private bool OnGuiBot(SerializablePackages packages, SerializablePropertys propertys, List<FlagAction> listFlagAction, TilesetAON tilesetAON, Rect rect)
    {
        if (packages == null)
        {
            return false;
        }
        float yGui = rect.y + 4;
        float widthLeft = 250;
        AONGUI.Label(new Rect(rect.x + 4, yGui + DefineAON.GUI_Y_Label, widthLeft, DefineAON.GUI_Height_Label), "Edit package:");
        yGui += 32f;
        {
            comboBoxSlug.UpdateListContent(packages.AllKey);
            comboBoxSlug.Empty = "Not selected";
            comboBoxSlug.SelectedItemIndex = slugIndex;
            comboBoxSlug.Rect.x = rect.x;
            comboBoxSlug.Rect.y = yGui;
            comboBoxSlug.Rect.width = widthLeft;
            comboBoxSlug.Rect.height = 32f;
            comboBoxSlug.Show(rect.height - yGui, "defause", true, false, (int next) => {
                slugIndex = next;
            });
        }
        // if(comboBoxSlug.IsDropDownListVisible){
        // 	return true;
        // }
        yGui = rect.y + 4;
        rect.x = rect.x + widthLeft + 4;
        if (slugIndex < 0 || slugIndex >= packages.Count)
        {
            return true;
        }

        AONGUI.Button(new Rect(rect.x, yGui + DefineAON.GUI_Y_Button, 120, DefineAON.GUI_Height_Button), "Remove by slug", () => {
            packages.Remove(slugIndex);
            slugIndex = -1;
        });

        SerializablePackages.Package package = packages.PackageByIndex(slugIndex);
        AONGUI.Button(new Rect(rect.x + 130, yGui + DefineAON.GUI_Y_Button, 120, DefineAON.GUI_Height_Button), "Duplicate by slug", () => {
            var n = packages.Copy(slugIndex);
            if (n >= 0)
            {
                slugIndex = n;
            }
        });

        AONGUI.Button(new Rect(rect.x + 260, yGui + DefineAON.GUI_Y_Button, 100, DefineAON.GUI_Height_Label), "Add all Pets", () => {
            _addAllPets(package, propertys);
        });
        yGui += 32f;
        AONGUI.Label(new Rect(rect.x, yGui + DefineAON.GUI_Y_Label, 40, DefineAON.GUI_Height_Label), "Name");
        AONGUI.TextField(new Rect(rect.x + 40, yGui + DefineAON.GUI_Y_TextField, widthLeft - 40, DefineAON.GUI_Height_TextField), package.Name, (string text) => {
            package.Name =  text;
        });
        yGui += 32f;

        AONGUI.Label(new Rect(rect.x, yGui + DefineAON.GUI_Y_Label, 40, DefineAON.GUI_Height_Label), "Des");
        AONGUI.TextField(new Rect(rect.x + 40, yGui + DefineAON.GUI_Y_TextField, widthLeft - 40, DefineAON.GUI_Height_TextField), package.Des, (string text) => {
            package.Des = text;
        });
        yGui += 32f;
        if (propertys == null || propertys.Count <= 0)
        {
            AONGUI.Label(new Rect(rect.x, yGui + DefineAON.GUI_Y_Label, widthLeft, DefineAON.GUI_Height_Label), "Propertys is empty");
            return false;
        }
        AONGUI.Label(new Rect(rect.x, yGui + DefineAON.GUI_Y_Label, widthLeft, DefineAON.GUI_Height_Label), "Propertys list:");
        yGui += 32f;
        {
            comboBoxItem.UpdateListContent(package.data.Keys);
            comboBoxItem.Empty = "Not selected";
            comboBoxItem.SelectedItemIndex = itemIndex;
            comboBoxItem.Rect.x = rect.x;
            comboBoxItem.Rect.y = yGui;
            comboBoxItem.Rect.width = widthLeft;
            comboBoxItem.Rect.height = 32f;
            comboBoxItem.Show(rect.height - yGui, "defause", true, true, ( int next) => {
                itemIndex = next;
            });
        }
        AONGUI.Button(new Rect(rect.x + widthLeft + 10, yGui + DefineAON.GUI_Y_Button, 30, DefineAON.GUI_Height_Label), " + ", () => {
            if (itemIndex >= 0 && itemIndex < propertys.Count)
            {
                package.AddProperty(propertys.SlugByIndex(itemIndex));
            }
            else
            {
                package.AddProperty(propertys.SlugByIndex(0));
            }
        });
        if (itemIndex < 0)
        {
            return false;
        }
        AONGUI.Button(new Rect(rect.x + widthLeft + 50, yGui + DefineAON.GUI_Y_Button, 30, DefineAON.GUI_Height_Label), " - ", () => {
            package.Remove(itemIndex);
        });
        if (package.data.Count == 0)
        {
            return false;
        }
        if (itemIndex >= package.data.Count)
        {
            itemIndex = package.data.Count - 1;
            return false;
        }
        rect.x = rect.x + widthLeft + 10;
        yGui += 32f;
        string slugProperty = package.data[itemIndex].property;
        {
            // Pick Property
            bool isWaitUI = false;
            PropertysGUI.Instance.PickSlugItem(slugIndex.ToString(), propertys, slugProperty, rect.x, yGui, widthLeft, ref yGui, ref isWaitUI, (string slugPropertyPick) => {
                package.data[itemIndex].property = slugPropertyPick;
                package.data.ResetKeys();
            });
            yGui += 32f;
        }
        AONGUI.Label(new Rect(rect.x, yGui + DefineAON.GUI_Y_Label, widthLeft, DefineAON.GUI_Height_Label), "Pay by:");
        yGui += 32f;

        SerializablePackages.PayBy payBy = package.data[itemIndex].payBy;
        AONGUI.Label(new Rect(rect.x, yGui + DefineAON.GUI_Y_Label, 100, DefineAON.GUI_Height_Label), "Coin");
        AONGUI.TextField(new Rect(rect.x + 100, yGui + DefineAON.GUI_Y_Label, widthLeft - 100, DefineAON.GUI_Height_Label), payBy.coin.ToString(), (string text) => {
            payBy.coin = UtilsAON.StrToIntDef(text);
        });
        yGui += 32f;
        return false;
    }

    private void _addAllPets(SerializablePackages.Package package, SerializablePropertys propertys){
        package.RemoveAll();
        for (int i = 0; i < propertys.Count; i++)
        {
            SerializablePropertys.Property p = propertys.PropertyByIndex(i);
            if(p._Type == SerializablePropertys.EType.Pet){
                package.AddProperty(propertys.SlugByIndex(i));
            }
        }
    }
}
