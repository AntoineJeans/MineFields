using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PuzzleCreator : MonoBehaviour
{
    public PuzzleData griddata;
    private SpriteRenderer[,] imageGrid;
    private SpriteRenderer[,] objectiveIndicatorGrid;

    private enum EditingMode{
        disable, mine, objective, preturn, flag
    }
    private EditingMode mode = EditingMode.disable;

    public Sprite unturnedTile, mine, adj1, adj2, adj3, adj4, adj5, adj6, adj7, adj8, disabledTile, objectiveIndicator, nextTurnIndicator;

    private int cellsize = 1;

    public PlayerSettings settings;

    public GameObject tilePrefab;

    public float maxCameraZoomout;
    public GameObject main_camera;

    private int[,] adjacent_mines_array;
    private bool[,] nextTurns;

    void Start(){
        setUpPlayerSettings();

        getGridData();

        objectiveIndicatorGrid = new SpriteRenderer[griddata.width, griddata.height];
        fillWithNonObjective();
        
        InitiateImageGrid();

        placeCamera();   
    }

    private void getGridData(){
        GameObject parentObject = GameObject.FindGameObjectWithTag("StaticSceneElements");
        GridDataHolder container = parentObject.GetComponent<GridDataHolder>();
        adjacent_mines_array = getBigZeroArray();
        if(container != null){
            GridData tempoData = container.griddata;
            griddata = tempoData as PuzzleData;
            Destroy(container);
        }
        else{
            InitiateGridData();
        }
    }
    private void InitiateGridData(){
        griddata = new PuzzleData();
        griddata.width = 10;
        griddata.height = 10;
        griddata.aiStatus = GridData.AIStatus.disabled;

        griddata.objective = GridData.Objective.clear;       
        griddata.disableTiles = true;
        griddata.disableMap = getBigFalseArray();
        griddata.useMineMap = true;
        griddata.mineMap = getBigFalseArray();
        griddata.preTurnTiles = true;
        griddata.preTurnMap = getBigFalseArray();
        griddata.flagMap = getBigFalseArray();
        griddata.objectiveMap = getBigFalseArray();
        adjacent_mines_array = getBigZeroArray();
    }

    private void InitiateImageGrid(){
        imageGrid = new SpriteRenderer[griddata.width, griddata.height];
        determineGridImages();
    }


    private void determineGridImages(){
        for (int i = 0; i < griddata.width; i++){
            for (int j = 0; j < griddata.height; j++){
                Vector3 pos = new Vector3((i + 0.5f) * cellsize, (j + 0.5f) * cellsize, 0) ;
                GameObject newTile = Instantiate(tilePrefab, pos, Quaternion.identity, this.transform);
                SpriteRenderer newTileImage = newTile.GetComponent<SpriteRenderer>();
                imageGrid[i, j] = newTileImage;
            }
        }

        for (int i = 0; i < griddata.width; i++){
            for (int j = 0; j < griddata.height; j++){

                if(griddata.disableMap[i,j]){
                    makeDisabled(i,j);
                }
                else if(griddata.preTurnMap[i,j]){
                    makePreTurn(i,j);
                }
                else{
                    if(griddata.mineMap[i,j]){
                        makeMine(i,j);
                    }
                    else{
                        imageGrid[i, j].sprite = unturnedTile;
                        imageGrid[i, j].color = settings.getColorFromPack("unturnedTile");
                    }
                    if(griddata.flagMap[i,j]){
                        makeFlag(i,j);
                    }
                    if(griddata.objectiveMap[i,j]){
                        makeObjective(i,j);
                    }
                    
                }
            }   
        }
    }

    private void fillWithNonObjective(){
        for (int i = 0; i < griddata.width; i++){
            for (int j = 0; j < griddata.height; j++){
                Vector3 pos = new Vector3((i + 0.7f) * cellsize, (j + 0.7f) * cellsize, 1) ;
                GameObject newTile = Instantiate(tilePrefab, pos, Quaternion.identity, this.transform);

                SpriteRenderer newTileImage = newTile.GetComponent<SpriteRenderer>();
                newTileImage.sortingOrder = 1;
                newTileImage.sprite = null;
                objectiveIndicatorGrid[i, j] = newTileImage;
            }   
        }
    }

    private bool[,] getBigFalseArray(){
        bool[,] arr = new bool[griddata.width,griddata.height];
        for(int i = 0; i < griddata.width; i++){
            for(int j=0; j < griddata.height; j++){
                arr[i,j] = false;
            }
        }
        return arr;        
    }

    private int[,] getBigZeroArray(){
        int[,] arr = new int[griddata.width,griddata.height];
        for(int i = 0; i < griddata.width; i++){
            for(int j=0; j < griddata.height; j++){
                arr[i,j] = 0;
            }
        }
        return arr;        
    }

    private int[,] generate_adjacent_mines_array(bool[,] mineMap){
        int[,] arr = new int[griddata.width,griddata.height];
        for(int i = 0; i < griddata.width; i++){
            for(int j=0; j < griddata.height; j++){
                arr[i,j] = count_adjacent_mines(i, j, mineMap);
            }
        }
        return arr;    
    }

    private int count_adjacent_mines(int position_x, int position_y, bool[,] mineMap){
        int mine_counter = 0;
        int[] adjacent_operation = {-1,0,1};
        for (int x_index = 0; x_index < adjacent_operation.Length; x_index++){
            for (int y_index = 0; y_index < adjacent_operation.Length; y_index++){
                int x = position_x + adjacent_operation[x_index];
                int y = position_y + adjacent_operation[y_index];
                if (x >= 0 && y >= 0 && x < griddata.width && y < griddata.height){
                    if (mineMap[x,y]){
                        mine_counter++;
                    }
                }
            }
        }
        return mine_counter;
    }


    private void placeCamera(){
        main_camera.transform.position = new Vector3(griddata.width * cellsize / 2, griddata.height * cellsize / 2, -10);
        Camera camComponent = main_camera.GetComponent<Camera>();
        camComponent.backgroundColor = settings.getColorFromPack("background");
        float height_to_camera_size_ratio = 0.6f;
        float width_to_camera_size_ratio = 0.8f;
        camComponent.orthographicSize = Math.Max(height_to_camera_size_ratio * griddata.height, width_to_camera_size_ratio * griddata.width);
        maxCameraZoomout = camComponent.orthographicSize;
    }



    public void SetEditingModeMine(){mode = EditingMode.mine;}
    public void SetEditingModeDisable(){mode = EditingMode.disable;}
    public void SetEditingModeFlag(){mode = EditingMode.flag;}
    public void SetEditingModePreturn(){mode = EditingMode.preturn;}
    public void SetEditingModeObjective(){mode = EditingMode.objective;}

    public void undo(){

    }

        
    private void setUpPlayerSettings(){
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("PlayerSettings");
        GameObject settings_object = gameObjects[0];
        settings = settings_object.GetComponent<PlayerSettingsInstance>().settings;
    }




    private void changeImage(int x, int y, Sprite sprite){
        imageGrid[x,y].sprite = sprite;
    }

    // disable is king, only way to change disable state is to be in that state
    private void makeDisabled(int x, int y){
        // always works
        // necessary changes to make a disabled on x,y
        // disables every state and enables disabled

        if(griddata.mineMap[x,y]){removeMine(x,y);}
        if(griddata.flagMap[x,y]){removeFlag(x,y);}
        if(griddata.objectiveMap[x,y]){removeObjective(x,y);}
        if(griddata.preTurnMap[x,y]){removePreTurn(x,y);}
        
        griddata.disableMap[x,y] = true;
        changeImage(x,y,disabledTile);
        Color disabled_color;
        if (ColorUtility.TryParseHtmlString("#FFFFFF", out disabled_color)){}
        else {disabled_color = Color.magenta;}
        imageGrid[x,y].color = disabled_color;
    }

    private void removeDisabled(int x, int y){
        // only apply if disableMap[x,y] is true
        // returns to non-flagged, non-disabled, non-preturned, non-objective, non-mine
        changeImage(x,y,unturnedTile);
        griddata.disableMap[x,y] = false;
        imageGrid[x,y].color = settings.getColorFromPack("unturnedTile");
    }

    // preturn is queen, can only change preturn with preturn mode AND disable Mode
    private void makePreTurn(int x, int y){
        if (!griddata.disableMap[x,y]){
            
            // necessary changes to make a PreTurn on x,y
            // disables every state and enables PreTurn
            if(griddata.mineMap[x,y]){removeMine(x,y);}
            if(griddata.flagMap[x,y]){removeFlag(x,y);}
            if(griddata.objectiveMap[x,y]){removeObjective(x,y);}

            griddata.preTurnMap[x,y] = true;
            assignAdjacentImage(x,y);
        }
    }

    private void removePreTurn(int x, int y){
        // only apply if preTurnMap[x,y] is true
        // returns to non-flagged, non-disabled, non-preturned, non-objective, non-mine
        changeImage(x,y,unturnedTile);
        griddata.preTurnMap[x,y] = false;
        imageGrid[x,y].color = settings.getColorFromPack("unturnedTile");
    }


    private void makeMine(int x, int y){
        if (!griddata.disableMap[x,y] && !griddata.preTurnMap[x,y]){
            imageGrid[x,y].color = settings.getColorFromPack("mine");
            changeImage(x,y,mine);
            griddata.mineMap[x,y] = true;
            incrementAdjacentMines(x,y);

        }
    }

    private void removeMine(int x, int y){
        // only apply if mineMap[x,y] is true
        // returns to non-disabled, non-preturned, non-mine, maybe obective
        imageGrid[x,y].color = settings.getColorFromPack("unturnedTile");
        changeImage(x,y, unturnedTile);
        griddata.mineMap[x,y] = false;
        decrementAdjacentMines(x,y);
    }


    private void makeFlag(int x, int y){
        if (!griddata.disableMap[x,y] && !griddata.preTurnMap[x,y]){
            griddata.flagMap[x,y] = true;
            imageGrid[x,y].color = settings.getColorFromPack("flaggedTile");
        }      
    }

    private void removeFlag(int x, int y){
        // only apply if flagMap[x,y] is true
        // returns to non-flagged, non-disabled, non-preturned, non-objective, maybe mine
        griddata.flagMap[x,y] = false;
        if (griddata.mineMap[x,y]){
            imageGrid[x,y].color = settings.getColorFromPack("mine");
        }
        else{
            imageGrid[x,y].color = settings.getColorFromPack("unturnedTile");
        }
        
    }

    private void makeObjective(int x, int y){
        if (!griddata.disableMap[x,y] && !griddata.preTurnMap[x,y]){
            griddata.objectiveMap[x,y] = true; 
            createObjectiveIndication(x,y);
        }      
    }

    private void removeObjective(int x, int y){
        // only apply if preTurnMap[x,y] is true
        // returns to non-flagged, non-disabled, non-preturned, non-objective, maybe mine
        griddata.objectiveMap[x,y] = false;
        removeObjectiveIndication(x,y);
    }

    private void createObjectiveIndication(int x, int y){
        objectiveIndicatorGrid[x,y].sprite = objectiveIndicator;
    }

    private void removeObjectiveIndication(int x, int y){
        objectiveIndicatorGrid[x, y].sprite = null;
    }

    private void incrementAdjacentMines(int position_x, int position_y){
        int[] adjacent_operation = {-1,0,1};
        for (int x_index = 0; x_index < adjacent_operation.Length; x_index++){
            for (int y_index = 0; y_index < adjacent_operation.Length; y_index++){
                int x = position_x + adjacent_operation[x_index];
                int y = position_y + adjacent_operation[y_index];
                if (x >= 0 && y >= 0 && x < griddata.width && y < griddata.height){
                    adjacent_mines_array[x,y]++;
                    if (griddata.preTurnMap[x,y]){ Debug.Log("changing tile image while turned");

                        assignAdjacentImage(x,y);}
                }
            }
        }
    }

    private void decrementAdjacentMines(int position_x, int position_y){
        int[] adjacent_operation = {-1,0,1};
        for (int x_index = 0; x_index < adjacent_operation.Length; x_index++){
            for (int y_index = 0; y_index < adjacent_operation.Length; y_index++){
                int x = position_x + adjacent_operation[x_index];
                int y = position_y + adjacent_operation[y_index];
                if (x >= 0 && y >= 0 && x < griddata.width && y < griddata.height){
                    adjacent_mines_array[x,y]--;
                    if (griddata.preTurnMap[x,y]){assignAdjacentImage(x,y);}
                }


            }
        }
    }

    private void assignAdjacentImage(int x, int y){
        SpriteRenderer sr = imageGrid[x,y];
        int numb = adjacent_mines_array[x,y];
        switch(numb){
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
        if (numb > 0){sr.color = settings.getColorFromPack(numb.ToString());}
        
    }



    public void processClick(float x_click, float y_click){
        int x = (int) Math.Floor((double) (x_click/cellsize));
        int y = (int) Math.Floor((double) (y_click/cellsize));
        bool position_valid = x >= 0 && y >= 0 && x < griddata.width && y < griddata.height;
        
        if (position_valid){
            switch(mode){
                case EditingMode.disable:
                    if(griddata.disableMap[x,y]){removeDisabled(x,y);}
                    else{makeDisabled(x,y);}
                    break;
                case EditingMode.preturn:
                    if(griddata.preTurnMap[x,y]){removePreTurn(x,y);}
                    else{makePreTurn(x,y);}
                    break;
                case EditingMode.objective:
                    if(griddata.objectiveMap[x,y]){removeObjective(x,y);}
                    else{makeObjective(x,y);}
                    break;
                case EditingMode.mine:
                    if(griddata.mineMap[x,y]){removeMine(x,y);}
                    else{makeMine(x,y);}
                    break;
                case EditingMode.flag:
                    if(griddata.flagMap[x,y]){removeFlag(x,y);}
                    else{makeFlag(x,y);}
                    break;
            }

            nextTurns = getBigFalseArray();
            recompileNextTurns();
            displayNextTurns();
        }
        
    }

    private void recompileNextTurns(){
        for(int i = 0; i < griddata.width; i++){
            for(int j=0; j < griddata.height; j++){
                if (griddata.preTurnMap[i,j] && adjacent_mines_array[i,j] == 0){
                    nextTurnAllAdjacents(i,j);
                }
            }
        }     
    }

    private void nextTurnAllAdjacents(int position_x, int position_y){
        int[] adjacent_operation = {-1,0,1};
        for (int x_index = 0; x_index < adjacent_operation.Length; x_index++){
            for (int y_index = 0; y_index < adjacent_operation.Length; y_index++){
                int x = position_x + adjacent_operation[x_index];
                int y = position_y + adjacent_operation[y_index];
                if (x >= 0 && y >= 0 && x < griddata.width && y < griddata.height){
                    bool is_regular_unturned_tile = (!griddata.preTurnMap[x,y] && !griddata.disableMap[x,y] && !griddata.flagMap[x,y]);
                    if (is_regular_unturned_tile && !nextTurns[x,y]){
                        nextTurns[x,y] = true;
                        if(adjacent_mines_array[x,y] == 0){
                            nextTurnAllAdjacents(x,y);
                        }
                    }
                }
            }
        }
    }

    private void displayNextTurns(){
        for(int i = 0; i < griddata.width; i++){
            for(int j=0; j < griddata.height; j++){
                if (nextTurns[i,j]){
                    imageGrid[i,j].sprite = nextTurnIndicator;
                }
                else if (!griddata.preTurnMap[i,j] && !griddata.disableMap[i,j] && !griddata.flagMap[i,j] && !griddata.mineMap[i,j]){
                    imageGrid[i,j].sprite = unturnedTile;
                }
            }
        }
    }

    public void plus_x(){
        griddata.width++;
    }
    public void plus_y(){
        griddata.width--;
    }
    public void minus_x(){
        griddata.height++;
    }
    public void minus_y(){
        griddata.height--;
    }

    public void cacheCurrentPuzzle(){
        this.cache(griddata);
    }

    private void cache(GridData game){
        GameObject parentObject = GameObject.FindGameObjectWithTag("StaticSceneElements");
        GridDataHolder container = parentObject.AddComponent<GridDataHolder>();
        container.griddata = game;
    }

    public void savePuzzle(){
        DataSaver.savePuzzle(griddata);
    }
    
}
 

   

