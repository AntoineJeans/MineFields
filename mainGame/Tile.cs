using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{

    public enum DisplayState
    {
        Turned,
        Unturned,
        Disabled
    }

    private DisplayState tileState;
    private int n_adjacent_mines;
    private bool is_mine = false;
    private bool is_flagged = false;
    public int position_x;
    public int position_y;

    private List<Tile> adjacent_tiles;

    private Grid grid;
    
    public Tile(int x, int y, bool is_disabled, Grid grid){
        
        this.position_x = x;
        this.position_y = y;

        if (!is_disabled){this.tileState = DisplayState.Unturned;}
        else{this.tileState = DisplayState.Disabled;}

        this.grid = grid;
    }

    public void CreateMine(){
        if (is_enabled()){
            is_mine = true;
            foreach (Tile tile in adjacent_tiles){
                tile.n_adjacent_mines++;
            }    
        }
    }

    public void RemoveMine(){
        if (is_enabled()){
            is_mine = false;
            foreach (Tile tile in adjacent_tiles){
                tile.n_adjacent_mines--;
            }
        }
    }

    public void assignAdjacentTiles(List<Tile> adjacent_tiles_list){
        if (is_enabled()){
            adjacent_tiles = adjacent_tiles_list;
        }
    }

    public void countAdjacentMines(){
        if (is_enabled()){
            int mineCount = 0;
            foreach (Tile tile in adjacent_tiles){
                if(tile.is_mine){mineCount++;}
            }
            n_adjacent_mines = mineCount;
        }
    }
    

    public bool is_enabled(){
        return tileState != DisplayState.Disabled;
    }

    public bool is_unturned(){
        return tileState == DisplayState.Unturned;
    }

    public bool contains_mine(){
        return is_mine;
    }

    public bool contains_flag(){
        return is_flagged;
    }

    public int getMineCount(){
        return n_adjacent_mines;
    }

    public List<Tile> getAdjacentTiles(){
        return adjacent_tiles;
    }

    public int getColorKey(){
        if(is_flagged){return 11;}
        else if (is_unturned()){return 10;}
        else if(is_mine){return 9;}
        else {return n_adjacent_mines;}
    }

    private int getCountAdjacentFlags(){
        if (is_enabled()){
            int flagCount = 0;
            foreach (Tile tile in adjacent_tiles){
                if(tile.contains_flag()){flagCount++;}
            }
            return flagCount;
        }
        return 0;
    }

    public void turn(){
        if (!is_flagged && is_unturned() && is_enabled()){
            tileState = DisplayState.Turned;
            if(is_mine){
                grid.triggerTurnedTile(position_x, position_y, -1);
                grid.loseGame();
            }
            else{
                grid.triggerTurnedTile(position_x, position_y, n_adjacent_mines);
                if (n_adjacent_mines == 0){
                    foreach (Tile tile in adjacent_tiles){tile.turn();}
                    tileState = DisplayState.Disabled; // disables some tiles when we know they can't be interacted with anymore. (not all)
                }
            } 
        }
    }

    public void chord(){
        if ((tileState == DisplayState.Turned) && (getCountAdjacentFlags() >= n_adjacent_mines)){
            foreach (Tile tile in adjacent_tiles){
                if (!tile.contains_flag()) {tile.turn();}
            }
            tileState = DisplayState.Disabled;
        }
    }

    public void flag(){
        if(tileState == DisplayState.Unturned) {
            is_flagged = !is_flagged;
            grid.triggerFlaggedTile(position_x, position_y, is_flagged);
        }
    }
}
