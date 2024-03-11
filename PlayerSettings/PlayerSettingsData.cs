using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PlayerSettingsData
{
    public PlayerSettings.ColorPackCodes currentColorPackCode;
    public PlayerSettings.ClickActions currentClickAction;
    public int volume;
    public GridData.AIStatus aiStatus;
    public bool lastGameFinished;
    public float lastGameTime;
    public bool show_decimal_timer;

    public PlayerSettingsData(){}

}
