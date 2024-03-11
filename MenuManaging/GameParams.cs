using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameParams: MonoBehaviour
{

    public enum GameMode{
        undefined, easy, medium, expert, custom
    }
    private GameMode gameMode = GameMode.undefined;

    private GridData newGame;


    private GridData lastGame;
    public Slider widthSlider, heightSlider, minesSlider;

    public Button continueButton;


    void Start(){

        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("PlayerSettings");
        GameObject settings_object = gameObjects[0];
        PlayerSettingsInstance settings = settings_object.GetComponent<PlayerSettingsInstance>();

        if(!settings.settings.lastGameFinished()){
            continueButton.interactable = true;
            lastGame = getLastGame();
        }
        else{
            continueButton.interactable = false;
        }

        newGame = new GridData();
        newGame.objective = GridData.Objective.clear;
        newGame.aiStatus = settings.settings.getAIPref();
        newGame.disableTiles = false;
        newGame.preTurnTiles = false;
        newGame.useMineMap = false; 
    }

    private GridData getLastGame(){
        return DataSaver.loadLastGame();        
    }

    public void setEasyMode() {gameMode = GameMode.easy;}
    public void setMediumMode() {gameMode = GameMode.medium;}
    public void setExpertMode() {gameMode = GameMode.expert;}
    public void setCustomMode() {gameMode = GameMode.custom;}

    public void cacheNewGame(){
        switch(gameMode){
            case GameMode.easy:
                newGame.width = 8;
                newGame.height = 8;
                newGame.n_mines = 10;
                break;
            case GameMode.medium:
                newGame.width = 16;
                newGame.height = 16;
                newGame.n_mines = 40;
                break;
            case GameMode.expert:
                newGame.width = 16;
                newGame.height = 30;
                newGame.n_mines = 99;
                break;
            case GameMode.custom:
                // the variables are set to custom choice by default
                break;

            case GameMode.undefined:
                newGame.width = 5;
                newGame.height = 5;
                newGame.n_mines = 2;
                break;
        }

        GameParams.cache(newGame);
    }

    public void cacheLastGame(){
        GameParams.cache(lastGame);
    }

    public static void cache(GridData game){
        GameObject parentObject = GameObject.FindGameObjectWithTag("StaticSceneElements");
        GridDataHolder container = parentObject.AddComponent<GridDataHolder>();
        container.griddata = game;
    }
}








