using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PuzzleData : GridData
{
    public int id = 0;
    public string creation_date;
    public string modification_date;
    public string puzzle_name;

    public PuzzleData(){}
}
