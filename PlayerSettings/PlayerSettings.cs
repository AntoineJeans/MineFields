using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSettings
{
    private PlayerSettingsData data;

    // colorpacks
    Dictionary<string, string> colorpack; 
    public enum ColorPackCodes{
        def, dark, light, soft
    }

    private Dictionary<ColorPackCodes, Dictionary<string, string>> ColorPacks = new Dictionary<ColorPackCodes, Dictionary<string, string>>()
    {
        {
            ColorPackCodes.def, new Dictionary<string, string>
            {
                { "background", "#969696" },
                { "menuBackground", "#220002" },
                { "0", "#000000" },
                { "1", "#0015FF" },
                { "2", "#0F5A00" },
                { "3", "#B30B00" },
                { "4", "#010041" },
                { "5", "#3D0000" },
                { "6", "#00524D" },
                { "7", "#000000" },
                { "8", "#3A3A3A" },
                { "mine", "#FFFFFF" },
                { "unturnedTile", "#4D4D4D" },
                { "flaggedTile", "#FF0000" },
                { "SpecialClickStatus", "#FFB500" },
                { "AIMine", "#FF8300" },
                { "AIEmpty", "#00CA0C" },
                { "WinTzxcxext", "#FF8300" },
                { "DisabledTile", "#404040" },
                { "xzcx", "#00CA0C" },
                { "guy", "#005DFF" }

            }
        },
        {
            ColorPackCodes.dark, new Dictionary<string, string>
            {
                // Add the hex color values for the "dark" color pack here
            }
        },
        {
            ColorPackCodes.light, new Dictionary<string, string>
            {
                // Add the hex color values for the "light" color pack here
            }
        }
    };


    // clickActions
    public enum ClickActions{
        def, doubleClickToBreak, holdToFlag
    }
    private bool settings_changed_before_last_save = false;

    public PlayerSettings(){
        data = new PlayerSettingsData();
        colorpack = ColorPacks[ColorPackCodes.def];
        data.currentClickAction = ClickActions.def;
        data.currentColorPackCode = ColorPackCodes.def;
        data.volume = 10;
        data.aiStatus = GridData.AIStatus.onHint;
        data.lastGameFinished = true;
    }


    public PlayerSettings(PlayerSettingsData saved_data){
        data = saved_data;
        colorpack = ColorPacks[data.currentColorPackCode];
    }

    public Color getColorFromPack(string key){
        Color color;
        if (ColorUtility.TryParseHtmlString(colorpack[key], out color)){return color;}
        return Color.magenta; // in case of problem with the hex code
    }

    public void changeColorPack(ColorPackCodes pack){

        if (data.currentColorPackCode != pack){
            colorpack = ColorPacks[pack];
            data.currentColorPackCode = pack;
            settings_changed_before_last_save = true;
        }
    }


    public void changeClickAction(ClickActions action){
        if (data.currentClickAction != action){
            data.currentClickAction = action;
            settings_changed_before_last_save = true;
        }
    }

    public ClickActions getClickAction(){
        return data.currentClickAction;
    }

    public void changeVolume(int newVolume){
        if(newVolume != data.volume && newVolume >= 0){
            data.volume = newVolume;
            settings_changed_before_last_save = true;
        }
    }

    public int getVolume(){
        return data.volume;
    }

    public bool lastGameFinished(){
        return data.lastGameFinished;
    }

    public void setLastGameFinished(bool has_finished){
        data.lastGameFinished = has_finished;
    }

    public GridData.AIStatus getAIPref(){
        return data.aiStatus;
    }

    public void save(){
        if (settings_changed_before_last_save){
            DataSaver.saveSettings(data);
            settings_changed_before_last_save = false;
        }
    }

    public void setLastGameTime(float time){
        data.lastGameTime = time;
    }

    public float getLastGameTime(){
        return data.lastGameTime;
    }

    public void setShowDecimal(bool show){
        data.show_decimal_timer = show;
    }

    public bool getShowDecimal(){
        return data.show_decimal_timer;
    }


}