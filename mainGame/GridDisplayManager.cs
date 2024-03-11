using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public abstract class GridDisplayManager : MonoBehaviour
{
    public Grid grid;
    public PlayerSettings settings;
    private int cellsize = 1;
    private SpriteRenderer[,] imageGrid;
    public Sprite unturnedTile, disabledTile, mine, adj1, adj2, adj3, adj4, adj5, adj6, adj7, adj8;
    public GameObject tilePrefab;
    public GameObject main_camera;
    public float maxCameraZoomout;

    // information for handling different click actions
    public PlayerSettings.ClickActions clickaction;
    private bool mine_mode = true;
    private Vector2 lastTileClicked = new Vector2(-1, -1); // impossible tile, simplifies things
    private float timeWhenLastClicked;   
    public float minimumTimeToHoldFlag = 0.1f;
    public float maximumDelayBetweenDoubleClicks = 0.3f; 

    public bool is_finished;
    public Image FlagMode;
    public Image MineMode;
    
    public List<int[]> memory_AI_flags = new List<int[]>{};
    public List<int[]> memory_AI_turns = new List<int[]>{};

     // All this stuff is for grid setup
    protected virtual void Start(){
        is_finished = false;
        setUpPlayerSettings();
        GridData griddata = getGridData();
        makeGrid(griddata);
        placeCamera();

        FlagMode.gameObject.SetActive(false);
        MineMode.gameObject.SetActive(true);

    }

    protected abstract GridData getGridData();
    protected abstract void makeGrid(GridData griddata);
    public abstract void winGame();
    public abstract void loseGame();

    protected void destroyAllImages(){
        foreach (Transform child in transform){
            Destroy(child.gameObject);
        }
    }

    protected virtual void makeGrid(GridData griddata, bool requires_first_move_clear){
        grid = new Grid(this, griddata, requires_first_move_clear);
        imageGrid = new SpriteRenderer[grid.data.width, grid.data.height];
        fillWithUnturned();
    }

    private void fillWithUnturned(){
        for (int i = 0; i < grid.data.width; i++){
            for (int j = 0; j < grid.data.height; j++){
                Vector3 pos = new Vector3((i + 0.5f) * cellsize, (j + 0.5f) * cellsize, 0) ;
                GameObject newTile = Instantiate(tilePrefab, pos, Quaternion.identity, this.transform);
                SpriteRenderer newTileImage = newTile.GetComponent<SpriteRenderer>();
                if (grid.TileIsEnabled(i, j)){
                    newTileImage.sprite = unturnedTile;
                    newTileImage.color = settings.getColorFromPack("unturnedTile");
                }
                else{
                    newTileImage.color = settings.getColorFromPack("DisabledTile");
                    newTileImage.sprite = disabledTile;
                }
                imageGrid[i, j] = newTileImage;
            }   
        }
    }

    private void setUpPlayerSettings(){
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("PlayerSettings");
        GameObject settings_object = gameObjects[0];
        settings = settings_object.GetComponent<PlayerSettingsInstance>().settings;
        clickaction = settings.getClickAction();
    }

    private void placeCamera(){
        main_camera.transform.position = new Vector3(grid.data.width * cellsize / 2, grid.data.height * cellsize / 2, -10);
        Camera camComponent = main_camera.GetComponent<Camera>();
        camComponent.backgroundColor = settings.getColorFromPack("background");
        float height_to_camera_size_ratio = 0.6f;
        float width_to_camera_size_ratio = 0.8f;
        camComponent.orthographicSize = Math.Max(height_to_camera_size_ratio * grid.data.height, width_to_camera_size_ratio * grid.data.width);
        maxCameraZoomout = camComponent.orthographicSize;
    }


    // 2 triggers to be used in game
    public void triggerTurnedTile(int x, int y, int n_adjacent_mines){
        SpriteRenderer sr = imageGrid[x,y];
        switch(n_adjacent_mines){
            case -1: sr.sprite = mine; break;
            case 0: sr.sprite = null; break;
            case 1: sr.sprite = adj1; break;
            case 2: sr.sprite = adj2; break;
            case 3: sr.sprite = adj3; break;
            case 4: sr.sprite = adj4; break;
            case 5: sr.sprite = adj5; break;
            case 6: sr.sprite = adj6; break;
            case 7: sr.sprite = adj7; break;
            case 8: sr.sprite = adj8; break;
        }
        if (n_adjacent_mines == -1){sr.color = settings.getColorFromPack("mine");}
        else {sr.color = settings.getColorFromPack(n_adjacent_mines.ToString());}
    }

    public virtual void triggerFlaggedTile(int x, int y, bool is_flagged){
        if (is_flagged){
            imageGrid[x,y].color = settings.getColorFromPack("flaggedTile");
        }
        else{
            int[] tile = new int[]{x,y};
            if (memory_AI_flags.Contains(tile)){imageGrid[x,y].color = settings.getColorFromPack("AIMine");}
            else if (memory_AI_turns.Contains(tile)){imageGrid[x,y].color = settings.getColorFromPack("AIEmpty");}
            else{imageGrid[x,y].color = settings.getColorFromPack("unturnedTile");}
        }
    }

    // functions to declare a game won or lost



    // decision for what to do on the board when a click is detected by click listener
    public void processClick(float x_click, float y_click, float timediff = 0){  
        int x = (int) Math.Floor((double) (x_click/cellsize));
        int y = (int) Math.Floor((double) (y_click/cellsize));
        bool position_valid = x >= 0 && y >= 0 && x < grid.data.width && y < grid.data.height;
        
        if (position_valid && !is_finished){
            switch (clickaction){
                case PlayerSettings.ClickActions.def:
                    if (mine_mode){grid.mineClick(x,y);}
                    else {grid.flagClick(x,y);}
                    break;
                    // simple stuff

                case PlayerSettings.ClickActions.holdToFlag:
                    if (grid.tileIsFlagged(x,y)) {grid.flagClick(x,y);}
                    else if (timediff > minimumTimeToHoldFlag){grid.flagClick(x,y);} 
                    // when this condition is met, clickListener will change the color of the tile by itself
                    else {grid.mineClick(x,y);} // only mineclick if tile is shortly clicked and unflagged
                    break;

                case PlayerSettings.ClickActions.doubleClickToBreak:
                    Vector2 tileClicked = new Vector2(x,y);
                    if (tileClicked == lastTileClicked && (Time.time - timeWhenLastClicked) < maximumDelayBetweenDoubleClicks){
                        // regardless if the tile is flagged at the moment of the double clicked, it is destroyed
                        grid.forcedMineClick(x,y);
                    }
                    else {
                        grid.flagClick(x,y);
                        if (grid.tileIsUnturned(x,y)){
                            lastTileClicked = tileClicked;
                            timeWhenLastClicked = Time.time;
                            changeTileToColor(x,y, settings.getColorFromPack("SpecialClickStatus"), maximumDelayBetweenDoubleClicks);
                        }
                    }
                    break;
            }
        }       
    }


    // called by click listener when the tile is held long enough to be flagged
    public void showHoldToFlagCompleted(float x_click, float y_click){
        int x = (int) Math.Floor((double) (x_click/cellsize));
        int y = (int) Math.Floor((double) (y_click/cellsize));
        bool position_valid = x >= 0 && y >= 0 && x < grid.data.width && y < grid.data.height;
        
        if(position_valid && grid.tileIsUnturned(x,y)){
            changeTileToColor(x,y,settings.getColorFromPack("SpecialClickStatus"));
        }
    }


    // Tile color management
    public void changeTileToColor(int x, int y, Color color, float amountOfTime = -1){
        SpriteRenderer sr = imageGrid[x,y];
        if (amountOfTime < 0) {sr.color = color;}
        else{StartCoroutine(ChangeColorCoroutine(x, y, color, amountOfTime));}
    }
    
    private IEnumerator ChangeColorCoroutine(int x, int y, Color newColor, float duration){
        SpriteRenderer sr = imageGrid[x,y];
        sr.color = newColor;
        yield return new WaitForSeconds(duration);
        int currentColorKey = grid.getColorKey(x,y);
        sr.color = settings.getColorFromPack("unturnedTile");
    }

    protected Color HexToColor(string hex){
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color)){
            return color;
        }
        else{
            Debug.LogError("Invalid hexadecimal color code: " + hex);
            return Color.black;
        }
    }


    // in game buttons and controls for managing the grid
    public void switchMode(){
        mine_mode = !mine_mode;
        if (mine_mode){
            FlagMode.gameObject.SetActive(false);
            MineMode.gameObject.SetActive(true);
        }
        else{
            FlagMode.gameObject.SetActive(true);
            MineMode.gameObject.SetActive(false);
        }
        
    }

    public void switchClickAction(){
        clickaction = GetNextEnumValue(clickaction);
        Debug.Log("current ClickAction = " + clickaction.ToString());
    }

    private PlayerSettings.ClickActions GetNextEnumValue(PlayerSettings.ClickActions currentClickAction)
    {
        int totalEnumValues = Enum.GetNames(typeof(PlayerSettings.ClickActions)).Length;
        int nextValue = (int)currentClickAction + 1;
        if (nextValue >= totalEnumValues){nextValue = 0;}
        return (PlayerSettings.ClickActions)nextValue;
    }

    public void getAllHints(){
        grid.getAIHint();
    }

    public void getOneHint(){
        grid.getSingleAIHint();
    }

}





