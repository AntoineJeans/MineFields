using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dont_destroy_me : MonoBehaviour
{
    void Awake(){
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("StaticSceneElements");
        if (gameObjects.Length > 1){
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }
}