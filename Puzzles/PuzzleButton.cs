using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    
public class PuzzleButton : MonoBehaviour
{
    public static event System.Action<int> OnPuzzleIdClicked;
    public int buttonID;
    public void ButtonClickHandler(){
        OnPuzzleIdClicked?.Invoke(buttonID);
    }
}
