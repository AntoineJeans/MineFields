using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{

    private AudioSource src;


    // Update is called once per frame
    void Start()
    {
        GameObject MusicMNG = GameObject.FindWithTag("AudioManager");
        src = MusicMNG.GetComponent<AudioSource>();
    }


    public void LoadMainMenu(){
        SceneManager.LoadScene(0);
    }
    public void LoadClassicMenu(){
        SceneManager.LoadScene(1);
    }

    public void LoadPuzzlesMenu(){
        SceneManager.LoadScene(2);
    }

    public void LoadSettingsMenu(){
        SceneManager.LoadScene(3);
    }

    public void LoadMainGame(){
        SceneManager.LoadScene(4);
    }

    public void LoadPuzzlesPlayer(){
        SceneManager.LoadScene(5);
    }



    public void quitGame(){
        Application.Quit();
    }
}
