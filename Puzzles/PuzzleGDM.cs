using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class PuzzleGDM : GridDisplayManager
{

    protected override GridData getGridData(){

        GameObject parentObject = GameObject.FindGameObjectWithTag("StaticSceneElements");
        GridDataHolder container = parentObject.GetComponent<GridDataHolder>();

        if(container != null){
            Debug.Log("trouvé une partie");
            GridData startGrid = container.griddata;
            Destroy(container);
            return startGrid;
        }

        return null;
    }

    protected override void makeGrid(GridData griddata){
        base.makeGrid(griddata, false);
        grid.preTurnAndFlag();
    }


    public void restartGame(){
        is_finished = false;
        destroyAllImages();
        GridData newGame = grid.data;
        makeGrid(newGame, false);
    }
 


    private GridData generate_bidon_game(){
        GridData newGame = new GridData();
        newGame.objective = GridData.Objective.clear;
        newGame.aiStatus = settings.getAIPref();
        newGame.disableTiles = false;
        newGame.preTurnTiles = false;
        newGame.useMineMap = false; 
        newGame.width = 10;
        newGame.height = 10;
        newGame.n_mines = 1;
        return newGame; 
    }


    public override void winGame(){
        endGame();
    }
    public override void loseGame(){
        endGame();
    }
    
    private void endGame(){
        // add a display menu
        is_finished = true;
    }

    public void exitGame(){
        if(is_finished){
            // action for finished puzzle
        }
    }

}
