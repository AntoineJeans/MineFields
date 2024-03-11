using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettingsInstance : MonoBehaviour
{    
        public PlayerSettings settings;
    void Start(){
        settings = DataSaver.loadSettings(); 
    }
}
