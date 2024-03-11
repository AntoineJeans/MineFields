using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
[Serializable]
public class GridData
{

    // standard data
    public int width;
    public int height;


    // use mine ratio instead of mine count
    public bool useMineRatio = false;
    public int n_mines;
    public float mineRatio;


    // objective to measure win (clear in standard mode)
    public Objective objective = Objective.clear;
    public enum Objective{
        clear, findEveryCertainty, Nflags, nturns, findNMinesAndNEmpty
    };
    public int nMinesToFind;
    public int nEmptyToFind;
    public bool[,] objectiveMap;

    // Status of the AI in the game
    public enum AIStatus{
        disabled, onHint, always, player
    };
    public AIStatus aiStatus = AIStatus.disabled;

    // Map of the tiles to disable (false) - or to enable (true)
    public bool disableTiles = false;
    public bool[,] disableMap;

    // Map of mine tiles (true) - or empty tiles (false) - always use mineMap with puzzle mode
    public bool useMineMap = false;
    public bool[,] mineMap;

    // Map of tiles to preturn before the game starts (true) - vs tiles to keep unturned (false) - always use in puzzles, or "continue" option in classic
    public bool preTurnTiles = false;
    public bool[,] preTurnMap;
    public bool[,] flagMap;


    public GridData(){}

    public GridData Clone()
    {
        GridData clone = new GridData();
        System.Reflection.FieldInfo[] fields = typeof(GridData).GetFields();

        foreach (var field in fields)
        {
            var value = field.GetValue(this);
            if (value is Array)
            {
                Array array = (Array)value;
                Array newArray = (Array)array.Clone();
                field.SetValue(clone, newArray);
            }
            else
            {
                field.SetValue(clone, value);
            }
        }
        return clone;
    }
}
