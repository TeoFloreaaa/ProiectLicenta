using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings
{
    public static List<Setting> settingsList = new List<Setting>();

    public static void AddSetting(Setting setting)
    {
        settingsList.Add(setting);
    }
}

public class Setting
{
    public string playerName;
    public int selectedType;
    public int selectedColor;

    public Setting(string _name, int type, int color)
    {
        playerName = _name;
        selectedType = type;
        selectedColor = color;
    }
}

