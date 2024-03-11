using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class ClassicGDM : GridDisplayManager{
    public float timeSpent = 0;
    public GameObject timerTextObject;
    private TextMeshProUGUI timerText;
    public GameObject minesLeftObject;
    private TextMeshProUGUI minesLeftText;

    public GameObject endOfGameMenu;
    public TextMeshProUGUI endOfGameTime;
    public TextMeshProUGUI endOfGameDifficulty;
    public TextMeshProUGUI endOfGameText;
    public Image headerColor;
    public Image mainBodyColor;
 
    

    protected override void Start(){
        timerText = timerTextObject.GetComponent<TextMeshProUGUI>();
        minesLeftText = minesLeftObject.GetComponent<TextMeshProUGUI>();
        base.Start();
        minesLeftText.text = grid.n_mines_left.ToString();
        endOfGameMenu.SetActive(false);
    }
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
        base.makeGrid(griddata, true);
        if(!settings.lastGameFinished()){
            grid.preTurnAndFlag();
            timeSpent = settings.getLastGameTime();
            timerText.text = (Mathf.Round(10 * timeSpent)/10).ToString();
        }
    }

    public override void triggerFlaggedTile(int x, int y, bool is_flagged){
        base.triggerFlaggedTile(x, y, is_flagged);
        minesLeftText.text = grid.n_mines_left.ToString();
    }


    public void restartGame(){
        is_finished = false;
        settings.setLastGameFinished(true);
        destroyAllImages();
        GridData newGame = grid.data;
        newGame.preTurnTiles = false;
        newGame.useMineMap = false;
        makeGrid(newGame, true);
        timeSpent = 0;
        minesLeftText.text = grid.n_mines_left.ToString();

        endOfGameMenu.SetActive(false);
    }



    public override void winGame(){
        endOfGameText.text = "Congratulations!";
        headerColor.color = HexToColor("#00b300");
        mainBodyColor.color = HexToColor("#79d279");
        endGame();
    }
    public override void loseGame(){
        endOfGameText.text = "Failed";
        headerColor.color = HexToColor("#b30000");
        mainBodyColor.color = HexToColor("#ff8c66");
        endGame();
    }

    private bool isEasy(int width, int height, int n_mines){
        return width==8 && height==8 && n_mines==10;
    }
    private bool isMedium(int width, int height, int n_mines){
        return width==16 && height==16 && n_mines==40;
    }
    private bool isExpert(int width, int height, int n_mines){
        return width==16 && height==30 && n_mines==99;
    }
    
    private void endGame(){
        // add a display menu
        is_finished = true;
        settings.setLastGameFinished(true);
        endOfGameMenu.SetActive(true);

        endOfGameTime.text = timeSpent.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " seconds";

        if (isEasy(grid.data.width, grid.data.height, grid.data.n_mines)){
            endOfGameDifficulty.text = "Easy";
        } else if (isMedium(grid.data.width, grid.data.height, grid.data.n_mines)){
            endOfGameDifficulty.text = "Medium";
        } else if (isExpert(grid.data.width, grid.data.height, grid.data.n_mines)){
            endOfGameDifficulty.text = "Expert";
        } else {
            endOfGameDifficulty.text = "Undefined";
        }
         
    }

    public void exitGame(){
        if (!is_finished) {
            settings.setLastGameFinished(false);
            settings.setLastGameTime(timeSpent);
            GridData lastGame = grid.saveCurrentGameState();
            lastGame.preTurnTiles = true;
            lastGame.useMineMap = true;
            DataSaver.saveLastGame(lastGame);
        }
        else{
            settings.setLastGameTime(0);
        }
    }
    void Update(){
        if (!is_finished){
            timeSpent += Time.deltaTime;
            if (settings.getShowDecimal()){
                timerText.text = timeSpent.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
            }
            else{
                timerText.text = timeSpent.ToString("0");
            }   
        }
             
    }
}