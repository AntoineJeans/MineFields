using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    public Tile[,] grid;
    public GridData data;
    private ObjectiveSolver objectiveSolver;
    private GridDisplayManager display;
    public int n_mines;
    public int n_mines_left;
    private int nMinesToFind;
    private int nEmptyToFind;
    private System.Random random = new System.Random();
    public bool requires_first_move_clear;

    private AIPlayer ai;


    

    public Grid(GridDisplayManager display, GridData data, bool requires_first_move_clear = false){
        // parameter bool : requires_first_move_clear
        // whether we want to make space for the first turn in the game, for there not to be mines. 
        // The grid takes care of the logic.
        this.requires_first_move_clear = requires_first_move_clear;
        this.display = display;
        this.data = data;

        grid = new Tile[data.width, data.height];
        
        if(data.disableTiles){
            initiateTiles(data.disableMap);
        }
        else{
            ActOnEveryTile(initiateTile);
        }
        
        if(data.useMineMap){
            set_up_mines_from_array(data.mineMap); // counts n_mine from the number of mines in the array
        }
        else{
            if(data.useMineRatio){this.n_mines = (int)(CountOnEveryTile(TileIsEnabled) * data.mineRatio);}
            else {this.n_mines = data.n_mines;}
            data.mineMap = getBigfalseArray();
            set_up_all_mines_and_tiles(); // sets up a count of n_mines randomly, and stores positions in mineTiles 
        }

        determineObjectiveSolver();

        if (data.aiStatus != GridData.AIStatus.disabled){ai = new AIPlayer(this, data.disableMap);}

        if(!data.preTurnTiles){
            data.preTurnMap = getBigfalseArray();
            data.flagMap = getBigfalseArray(); 
        }

        n_mines_left = n_mines;

        
        
    }

    private void ActOnEveryTile(Action<int, int> do_x){
        for (int i = 0; i < data.width; i++){
            for (int j = 0; j < data.height; j++){
                do_x(i, j);
            }
        }  
    }
    private int CountOnEveryTile(Func<int, int, bool> do_x){
        int count = 0;
        for (int i = 0; i < data.width; i++){
            for (int j = 0; j < data.height; j++){
                if(do_x(i,j)){count++;}
            }
        }  
        return count;
    }

    private bool[,] getBigfalseArray(){
        bool[,] boolArray = new bool[data.width, data.height];
        ActOnEveryTile((i, j) => boolArray[i, j] = false);
        return boolArray;
    }

    private void initiateTile(int i, int j){
        grid[i, j] = new Tile(i, j, false, this);
    }

    private void set_up_all_mines_and_tiles(){
        ActOnEveryTile(assignAdjacentTile);
        placeRandomMines();
    }

    private void placeRandomMines(){
        for (int i = 0; i < n_mines; i++){
            int [] pos = findValidMinePosition();
            grid[pos[0], pos[1]].CreateMine();
            data.mineMap[pos[0], pos[1]] = true;
        }
    }

    private int[] findValidMinePosition(int x = -1, int y = -1){
        bool requires_area_restriction = x != -1 && y != -1;
        bool impossible_because_area_restriction;
        Tile candidate;
        /// add error condition
        int random_x, random_y;
        while(true){
            random_x = random.Next(0, data.width);
            random_y = random.Next(0, data.height);
            candidate = grid[random_x, random_y];

            impossible_because_area_restriction = requires_area_restriction && (Math.Abs(random_x - x) <= 1) && (Math.Abs(random_y - y) <= 1);
            if (candidate.is_enabled() && !candidate.contains_mine() && !impossible_because_area_restriction){
                return new int[2] {random_x, random_y};
            }
        }
    }

    private void set_up_mines_from_array(bool [,] mine_array){
        ActOnEveryTile(assignAdjacentTile);
        placeMinesFromArray(mine_array);
    }

    private void placeMinesFromArray(bool[,] mine_array){
        this.n_mines = 0;
        ActOnEveryTile
        ((i, j) => {
        if (mine_array[i, j]){
            grid[i, j].CreateMine();
            n_mines++;
        }});
    }


    private void assignAdjacentTile(int i, int j){
        List<Tile> list = findAllAdjacentTiles(i,j);
        grid[i,j].assignAdjacentTiles(list);
    }

    private List<Tile> findAllAdjacentTiles(int position_x, int position_y){
        List<Tile> adjacent_tiles = new List<Tile>();
        int[] adjacent_operation = {-1,0,1};

        for (int x_index = 0; x_index < adjacent_operation.Length; x_index++){
            for (int y_index = 0; y_index < adjacent_operation.Length; y_index++){
                int x = position_x + adjacent_operation[x_index];
                int y = position_y + adjacent_operation[y_index];
                if (x >= 0 && y >= 0 && x < data.width && y < data.height){
                    Tile candidateTile = grid[x,y];
                    if (candidateTile.is_enabled() && (x != position_x || y != position_y)){
                        adjacent_tiles.Add(candidateTile);
                    }
                }
            }
        }
        return adjacent_tiles;
    }

    private void countAdjacentMines(int i, int j){
        grid[i,j].countAdjacentMines();
    }

    private void initiateTiles(bool[,] gridShape){
        for (int i = 0; i < data.width; i++){
            for (int j = 0; j < data.height; j++){
                grid[i, j] = new Tile(i, j, gridShape[i,j], this);
            }
        }
    }


    public void preTurnAndFlag(){
        if(data.preTurnTiles){
            for (int i = 0; i < data.width; i++){
                for (int j = 0; j < data.height; j++){
                    if(data.preTurnMap[i,j]){
                        requires_first_move_clear = false;
                        mineClick(i,j);
                    }
                }
            }
            // risque de chord si on les fait ensemble, étrange
            for (int i = 0; i < data.width; i++){
                for (int j = 0; j < data.height; j++){
                    if(data.flagMap[i,j]){flagClick(i,j);}
                }
            }
        }
    }


    private void determineObjectiveSolver(){
        switch(data.objective){
            case GridData.Objective.clear:
                int unturnedTilesAtTheStart = CountOnEveryTile(TileIsEnabled) - this.n_mines;
                objectiveSolver = new Clear(unturnedTilesAtTheStart);
                break;
            case GridData.Objective.nturns:
                int guy = 3;
                objectiveSolver = new NTurns(guy);
                break;
        }
    }

    private void reposition_mines_around(int x, int y){
        List<Tile> tilesToClear = findAllAdjacentTiles(x, y);
        tilesToClear.Add(grid[x,y]);

        for (int i = 0; i < tilesToClear.Count; i++){
            if(tilesToClear[i].contains_mine()){
                int [] pos = findValidMinePosition(x,y);
                grid[pos[0], pos[1]].CreateMine();
                data.mineMap[pos[0], pos[1]] = true;
                
                grid[tilesToClear[i].position_x, tilesToClear[i].position_y].RemoveMine();
                data.mineMap[tilesToClear[i].position_x, tilesToClear[i].position_y] = false;
            }
            
        }
        
        Debug.Log("Clearing mines around x and y!!!");
    }
    
    public void mineClick(int x, int y){
        Tile tileClicked = grid[x,y];
        if (tileClicked.is_enabled()){
            if (tileClicked.is_unturned()) {
                if (requires_first_move_clear){
                    reposition_mines_around(x,y);
                    requires_first_move_clear = false;
                }
                tileClicked.turn();
                }
            else {tileClicked.chord();}
        }
        if (checkWin()){winGame();}
        if (data.aiStatus == GridData.AIStatus.always){getAIHint();}      
    }

    public void flagClick(int x, int y){
        Tile tileClicked = grid[x,y];
        if (tileClicked.is_enabled()){
            if (tileClicked.is_unturned()) {tileClicked.flag();}
            else {
                tileClicked.chord();
                if (checkWin()){winGame();}
                if (data.aiStatus == GridData.AIStatus.always){getAIHint();}
            }
        }
    }

    public void forcedMineClick(int x, int y){
        Tile tileClicked = grid[x,y];
        if (tileClicked.is_enabled() && tileClicked.is_unturned()){
            if (tileClicked.contains_flag()){tileClicked.flag();}
            if (requires_first_move_clear){
                reposition_mines_around(x,y);
                requires_first_move_clear = false;
            }
            tileClicked.turn();
        }
        else {tileClicked.chord();}

        if (checkWin()){winGame();}
        if (data.aiStatus == GridData.AIStatus.always){getAIHint();}
        
    }

    public void triggerTurnedTile(int x, int y, int n_adjacent_mines){
        display.triggerTurnedTile(x, y, n_adjacent_mines);
        if(data.aiStatus != GridData.AIStatus.disabled){ai.turnTile(x,y,n_adjacent_mines);}
        data.preTurnMap[x,y] = true;

        if(!objectiveSolver.turnIsOK(x,y)){
            loseGame();
        }
    }

    public void triggerFlaggedTile(int x, int y, bool is_flagged){
        data.flagMap[x,y] = is_flagged;
        if (is_flagged){
            n_mines_left--;
        }
        else{
            n_mines_left++;
        }

        display.triggerFlaggedTile(x,y,is_flagged);       

        if(!objectiveSolver.flagIsOK(x,y,is_flagged)){
            loseGame();
        }
        
    }


    public bool checkWin(){
        return objectiveSolver.checkWin();
    }

    public void winGame(){
        display.winGame();
    }
    public void loseGame(){
        display.loseGame();
    }


    public int getColorKey(int x, int y){
        return grid[x,y].getColorKey();
    }

    public bool tileIsFlagged(int x, int y){
        return grid[x,y].contains_flag();
    }

    public bool tileIsUnturned(int x, int y){
        return grid[x,y].is_unturned();
    }

    public bool TileIsEnabled(int i, int j){
        return grid[i,j].is_enabled();
    }


    public void getAIHint(){
        if(data.aiStatus != GridData.AIStatus.disabled){
            ai.checkForAllMoves();
            displayReccomendations();
        }
    }

    public void getSingleAIHint(){
        if(data.aiStatus != GridData.AIStatus.disabled){
            ai.checkForOneMove();
            displayReccomendations();
        }
    }

    private void displayReccomendations(){
        for(int i = 0; i < ai.simple_reccomendations.Count; i++){
            Tuple<int, int> pos = ai.simple_reccomendations[i].Item1;
            int x = (int)pos.Item1;
            int y = (int)pos.Item2;
            bool tile_is_mine = ai.simple_reccomendations[i].Item2;

            if (tile_is_mine){
                display.changeTileToColor(x, y, Color.magenta);
                display.memory_AI_flags.Add(new int[]{x,y});
            }
            else {
                display.changeTileToColor(x, y, Color.green);
                display.memory_AI_turns.Add(new int[]{x,y});
            }
            
        }
    }

    public GridData saveCurrentGameState(){
        data.n_mines = n_mines;
        data.useMineRatio = false;
        return data;
    }

}
