using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class AIPlayer 
{
    public List<(Tuple<int, int>, bool)> simple_reccomendations = new List<(Tuple<int, int>, bool)>();

    private Grid grid;
    private enum InternalTileRepresentation{
        hidden, toTurn, updated, turned, forgotten, disabled, mine, empty
    };
    private InternalTileRepresentation[,] tileRepresentations;
    private int[,] adjacentMinesArray;

    private bool[,] gridShape;
    private bool useGridShape = false;

    private List<int[]> tilesToTurn = new List<int[]>();
    private List<int[]> infoTiles = new List<int[]>();
    private List<int[]> filteredInfoTiles = new List<int[]>();



    private int topBorder;
    private int bottomBorder;
    private int leftBorder;
    private int rightBorder;

    private int n_mines;

    private bool grid_changed = false;
    private bool changes_occured = false;
    private bool change_in_poss_occured = false;

    private bool infoTilesAredefined = false;

    private List<PotentialMineCombination>[,] possibilitiesGrid;



    // only function beside constructor where information goes from grid to AI
    public void turnTile(int x, int y, int n_adjacent_mines){
        if(x >= leftBorder && x < rightBorder && y >= bottomBorder && y < topBorder){
            if (tileIsUnturned(x,y) || tileRepresentations[x,y] == InternalTileRepresentation.empty){
                tileRepresentations[x,y] = InternalTileRepresentation.updated;
                ActOnEveryAdjacentTile(makeUpdate, x, y);
                adjacentMinesArray[x,y] = n_adjacent_mines;
                grid_changed = true;
            }
        }
    }   


    public AIPlayer(Grid grid, bool[,] gridShape = null){
        this.grid = grid;
        if(gridShape != null){
            useGridShape = true;
            this.gridShape = gridShape;
        }
        leftBorder = 0;
        bottomBorder = 0;
        rightBorder = grid.data.width;
        topBorder = grid.data.height;

        n_mines = grid.data.n_mines;

        tileRepresentations = new InternalTileRepresentation[rightBorder, topBorder];
        adjacentMinesArray = new int[rightBorder, topBorder];
        possibilitiesGrid = new List<PotentialMineCombination>[rightBorder, topBorder];

        ActOnEveryTile(InitiateTileRepresentation);
        ActOnEveryTile(InitiateAdjacentMines);
    }

    private void ActOnEveryTile(Action<int, int> do_x){
        for (int i = leftBorder; i < rightBorder; i++){
            for (int j = bottomBorder; j < topBorder; j++){
                do_x(i, j);
            }
        }  
    }

    private void ActOnEveryAdjacentTile(Action<int, int> do_x, int position_x, int position_y){
        int[] adjacent_operation = {-1,0,1};
        for (int x_index = 0; x_index < adjacent_operation.Length; x_index++){
            for (int y_index = 0; y_index < adjacent_operation.Length; y_index++){
                int x = position_x + adjacent_operation[x_index];
                int y = position_y + adjacent_operation[y_index];
                if (x >= leftBorder && y >= bottomBorder && x < rightBorder && y < topBorder && tileRepresentations[x,y] != InternalTileRepresentation.disabled){
                    if (x != position_x || y != position_y){
                        do_x(x, y);
                    }
                }
            }
        }
    }

    private int CountOnEveryAdjacentTile(Func<int, int, bool> do_x, int position_x, int position_y){
        int i = 0;
        int[] adjacent_operation = {-1,0,1};
        for (int x_index = 0; x_index < adjacent_operation.Length; x_index++){
            for (int y_index = 0; y_index < adjacent_operation.Length; y_index++){
                int x = position_x + adjacent_operation[x_index];
                int y = position_y + adjacent_operation[y_index];
                if (x >= leftBorder && y >= bottomBorder && x < rightBorder && y < topBorder && tileRepresentations[x,y] != InternalTileRepresentation.disabled){
                    bool accepted = do_x(x, y);
                    if(accepted){
                        i++;
                    }
                }
            }
        }
        return i;
    }

    private void InitiateTileRepresentation(int x, int y){
        if (!useGridShape) {tileRepresentations[x, y] = InternalTileRepresentation.hidden;}
        else if (gridShape[x,y]) {tileRepresentations[x, y] = InternalTileRepresentation.hidden;}
        else {tileRepresentations[x, y] = InternalTileRepresentation.disabled;}
    }

    private void InitiateAdjacentMines(int x, int y){
        if(tileRepresentations[x, y] != InternalTileRepresentation.disabled){
            adjacentMinesArray[x,y] = -1;
        }
    }

    private bool tileIsTurned(int x, int y){
        InternalTileRepresentation tileRep = tileRepresentations[x, y];
        return tileRep == InternalTileRepresentation.forgotten || tileRep == InternalTileRepresentation.updated || tileRep == InternalTileRepresentation.turned;
        
    }

    private bool tileIsUnturned(int x, int y){
        InternalTileRepresentation tileRep = tileRepresentations[x, y];
        return tileRep == InternalTileRepresentation.hidden || tileRep == InternalTileRepresentation.toTurn;
    }

    private bool tileCouldBeMine(int x, int y){
        InternalTileRepresentation tileRep = tileRepresentations[x, y];
        return tileIsUnturned(x,y) || tileRep == InternalTileRepresentation.mine;
    }

    private bool tileCantBeMine(int x, int y){
        InternalTileRepresentation tileRep = tileRepresentations[x, y];
        return tileIsTurned(x,y) || tileRep == InternalTileRepresentation.empty;
    }

    private bool TileIsFlagged(int x, int y){
        return tileRepresentations[x,y] == InternalTileRepresentation.mine;
    }

    private void makeUpdate(int x, int y){
        if (tileRepresentations[x,y] == InternalTileRepresentation.turned){
            tileRepresentations[x,y] = InternalTileRepresentation.updated;
        }
    }

    private void defineUpdatedTile(int x, int y){
        if (tileRepresentations[x,y] == InternalTileRepresentation.updated){
            if (CountOnEveryAdjacentTile(tileIsUnturned, x, y) == 0){
                tileRepresentations[x,y] = InternalTileRepresentation.forgotten;
            }
            else{
                tileRepresentations[x,y] = InternalTileRepresentation.turned;
            }
        }
    }

    private void clearRecs(){
        simple_reccomendations = new List<(Tuple<int, int>, bool)>();
    }


    private void checkForSimpleFlags(int x, int y){
        InternalTileRepresentation tileRep = tileRepresentations[x, y];
        if (tileRep == InternalTileRepresentation.turned){
            int n_adjacent_mines = adjacentMinesArray[x,y];
            if(CountOnEveryAdjacentTile(tileCouldBeMine, x, y) == n_adjacent_mines){
                ActOnEveryAdjacentTile(defineTileAsMine, x, y);
            }
        }
    }

    private void checkForSimpleTurns(int x, int y){
        InternalTileRepresentation tileRep = tileRepresentations[x, y];
        if (tileRep == InternalTileRepresentation.turned){
            int n_adjacent_mines = adjacentMinesArray[x,y];
            if (CountOnEveryAdjacentTile(TileIsFlagged,x,y) == n_adjacent_mines){
                ActOnEveryAdjacentTile(defineTileAsEmpty,x,y);
            }
        }
    }

    private void defineTileAsMine(int x, int y){
        if (tileIsUnturned(x, y) && !TileIsFlagged(x,y)){
            (Tuple<int, int>, bool) rec = (new Tuple<int, int>(x, y), true);
            if (!simple_reccomendations.Contains(rec)) {
                simple_reccomendations.Add(rec);
                tileRepresentations[x,y] = InternalTileRepresentation.mine;
                ActOnEveryAdjacentTile(makeUpdate, x, y);
                ActOnEveryAdjacentTile(defineUpdatedTile, x, y);
                changes_occured = true;
                Debug.Log("FOUND MINE: " + x.ToString() + ", " + y.ToString());
            }
        }
    }

    private void defineTileAsEmpty(int x, int y){
        if (tileIsUnturned(x, y)){
            (Tuple<int, int>, bool) rec = (new Tuple<int, int>(x, y), false);
            if (!simple_reccomendations.Contains(rec)) {
                simple_reccomendations.Add(rec);
                tileRepresentations[x,y] = InternalTileRepresentation.empty;
                ActOnEveryAdjacentTile(makeUpdate, x, y);
                ActOnEveryAdjacentTile(defineUpdatedTile, x, y);
                changes_occured = true;
                Debug.Log("FOUND EMPTY: " + x.ToString() + ", " + y.ToString());
            }
        }
    }

    public void checkForAllMoves(){
        clearRecs();
        ActOnEveryTile(defineUpdatedTile);
        changes_occured = true;
        simpleMovesLoop(); 
        findAllInfoTiles();
        definePossibilitiesGrid();

        Debug.Log("------------------------ yooo ----------------------------");
        defineFilteredInfoTiles();
        Debug.Log("filteredInfoTiles : " + filteredInfoTiles.Count.ToString());
        findMoreComplicatedStuff(); 
    }


    public void checkForOneMove(){
        string justification;
        grid_changed = false;
        clearRecs();
        ActOnEveryTile(defineUpdatedTile);
        
        changes_occured = false;
        
        for (int i = leftBorder; i < rightBorder; i++){
            for (int j = bottomBorder; j < topBorder; j++){
                checkForSimpleFlags(i, j);
                if(changes_occured){
                    justification = "Found new mine! Position: all tiles around " + i.ToString() + ", " + j.ToString() 
                    + " \\ Justification: Tile " + i.ToString() + ", " + j.ToString() + "touches as many unturned tiles as it does mines.";
                    Debug.Log(justification);
                    break;
                }
            }
            if(changes_occured){break;}
        }  

        if(!changes_occured){
            for (int i = leftBorder; i < rightBorder; i++){
                for (int j = bottomBorder; j < topBorder; j++){
                    checkForSimpleTurns(i, j);
                    if(changes_occured){
                        justification = "Found new empty tile! Position: all tiles around " + i.ToString() + ", " + j.ToString()
                        + " \\ Justification: Tile " + i.ToString() + ", " + j.ToString() + "touches as many mines as there are flags adjacent to it.";
                        Debug.Log(justification);
                        break;
                    }
                }
                if(changes_occured){break;}
            }  
        }


        if(!changes_occured){
            if(grid_changed || !infoTilesAredefined){
                findAllInfoTiles();
                definePossibilitiesGrid();
                defineFilteredInfoTiles();
                infoTilesAredefined = true;
            }

            Debug.Log("------------------------ yooo ----------------------------");
            Debug.Log("filteredInfoTiles : " + filteredInfoTiles.Count.ToString());
            findMoreComplicatedStuff(); 
        }

    }



    private void simpleMovesLoop(){
        while(changes_occured){
            Debug.Log("start new simple loop");
            changes_occured = false;
            ActOnEveryTile(checkForSimpleFlags);
            ActOnEveryTile(checkForSimpleTurns);
            if(changes_occured){Debug.Log("AI found simple moves");}
            else{Debug.Log("AI did not find simple moves");}
        }
    }




    private void AddAdjacentToTilesToTurn(int x, int y){
    if (tileRepresentations[x, y] == InternalTileRepresentation.turned){
            ActOnEveryAdjacentTile(AddToTilesToTurn, x, y);
            infoTiles.Add(new int[] {x,y});
        }
    }

    private void AddToTilesToTurn(int x, int y){
        if(tileIsUnturned(x,y)){
            tilesToTurn.Add(new int[] {x,y});
        }
    }

    private void findAllInfoTiles(){
        infoTiles = new List<int[]>();
        ActOnEveryTile(AddAdjacentToTilesToTurn);

        string infoTilesString = "info Tiles:";
        foreach(int[] tile in infoTiles){
            infoTilesString += "(" + tile[0] + ", " + tile[1] + ");";
        }
        Debug.Log(infoTilesString);
    }


    private List<int[]> getUnturnedAdjacentTiles(int position_x, int position_y){
        List<int[]> unturnedTiles = new List<int[]>();
        int[] adjacent_operation = {-1,0,1};
        for (int x_index = 0; x_index < adjacent_operation.Length; x_index++){
            for (int y_index = 0; y_index < adjacent_operation.Length; y_index++){
                int x = position_x + adjacent_operation[x_index];
                int y = position_y + adjacent_operation[y_index];
                if (x >= leftBorder && y >= bottomBorder && x < rightBorder && y < topBorder && tileRepresentations[x,y] != InternalTileRepresentation.disabled){
                    if ((x != position_x || y != position_y) && tileCouldBeMine(x,y)){
                        unturnedTiles.Add(new int[]{x,y});
                    }
                }
            }
        }

        return unturnedTiles;
    }

    private List<int[]> getTurnedAdjacentTiles(int position_x, int position_y){

        List<int[]> turnedTiles = new List<int[]>();
        int[] adjacent_operation = {-1,0,1};
        for (int x_index = 0; x_index < adjacent_operation.Length; x_index++){
            for (int y_index = 0; y_index < adjacent_operation.Length; y_index++){
                int x = position_x + adjacent_operation[x_index];
                int y = position_y + adjacent_operation[y_index];
                if (x >= leftBorder && y >= bottomBorder && x < rightBorder && y < topBorder && tileRepresentations[x,y] != InternalTileRepresentation.disabled){
                    int[] temp = new int[] {x,y};
                    if (ListContainsArray(infoTiles, temp)){
                        turnedTiles.Add(temp);
                    }
                }
            }
        }
        return turnedTiles;
    }


    private bool ListContainsArray(List<int[]> list, int[] array){
        foreach(int[] el in list){
            if(el[0] == array[0] && el[1] == array[1]){
                return true;
            }
        }
        return false;
    }


    private bool ArrayContainsArray(int[][] list, int[] array){
        foreach(int[] el in list){
            if(el[0] == array[0] && el[1] == array[1]){
                return true;
            }
        }
        return false;
    }

    public struct PotentialMineCombination
    {
        public int[][] Vectors;
        public bool[] Booleans;

    }


    private List<PotentialMineCombination> generatePossibleMineCombinations(int x, int y){
        List<int[]> unturnedTiles = getUnturnedAdjacentTiles(x,y);
        List<int[]> relevant_unturnedTiles = new List<int[]>();

        int n_unfound_mines = adjacentMinesArray[x,y];      

        foreach(int[] tile in unturnedTiles){
            if(tileRepresentations[tile[0], tile[1]] == InternalTileRepresentation.mine){
                n_unfound_mines--;
            }
            else{
                relevant_unturnedTiles.Add(tile);
            }
        }

        int n_possible_slots = relevant_unturnedTiles.Count;

        List<PotentialMineCombination> possibilities = new List<PotentialMineCombination>();

        Debug.Log(x.ToString() + ", " + y.ToString() + ": " + n_unfound_mines.ToString() + " in " + n_possible_slots.ToString());
        List<bool[]> result = GenerateArrays(n_unfound_mines, n_possible_slots);
        Debug.Log(result.Count.ToString() + " possible combinations");

        foreach (bool[] arr in result) {
            PotentialMineCombination pmc = new PotentialMineCombination();
            pmc.Vectors = relevant_unturnedTiles.ToArray();
            pmc.Booleans = arr;
            possibilities.Add(pmc);
        }

        return possibilities;        
    }

    private void realizePMC(PotentialMineCombination pmc){
        for(int i = 0; i < pmc.Booleans.Length; i++){
            if(pmc.Booleans[i]){
                defineTileAsMine(pmc.Vectors[i][0], pmc.Vectors[i][1]);
            }
            else if (pmc.Booleans[i]){
                defineTileAsEmpty(pmc.Vectors[i][0], pmc.Vectors[i][1]);
            }
        }        
    }
    
    private List<bool[]> GenerateArrays(int m, int n)
    {
        List<bool[]> result = new List<bool[]>();
        bool[] array = new bool[n];
        int n_unfound_mines = m;
        int n_possible_slots = n;
        GenerateArraysHelper(array, n_unfound_mines, n_possible_slots, 0, result);
        return result;
    }
    
    private static void GenerateArraysHelper(bool[] array, int m, int n, int index, List<bool[]> result)
    {
        if (m == 0 && index == n)
        {
            result.Add(array.Clone() as bool[]);
            return;
        }

        if (index >= n)
        {
            return;
        }
        
        if(m > 0){
            array[index] = true;
            GenerateArraysHelper(array, m - 1, n, index + 1, result);
        }

        array[index] = false;
        GenerateArraysHelper(array, m, n, index + 1, result);
    }

    private void definePossibilitiesGrid(){
        foreach(int[] tile in infoTiles){
            int x = tile[0];
            int y = tile[1];
            possibilitiesGrid[x,y] = generatePossibleMineCombinations(x, y);
        }
    }
    private void findMoreComplicatedStuff(){

        change_in_poss_occured = true;
        while(change_in_poss_occured){
            change_in_poss_occured = false;
            Debug.Log("loop smart conclusion");
        
            foreach(int[] ATile in filteredInfoTiles){
                int x = ATile[0];
                int y = ATile[1];
                List<PotentialMineCombination> ATilePossibilities = possibilitiesGrid[x,y];

                Debug.Log("checking: " + x.ToString() + ", " + y.ToString());
                Debug.Log("poss length = " + ATilePossibilities.Count.ToString());

                int[][] adjacentUnturnedTiles = ATilePossibilities[0].Vectors;
                int[][] relatedTiles = getAllRelatedTiles(adjacentUnturnedTiles, x, y); 

                foreach(int[] BTile in relatedTiles){
                    List<PotentialMineCombination> BTilePossibilities = possibilitiesGrid[BTile[0],BTile[1]];
                    List<PotentialMineCombination> newList = new List<PotentialMineCombination>();

                    foreach(PotentialMineCombination Bposs in BTilePossibilities){
                        find_new_Atile_poss(x, y, Bposs, newList);
                    }
                    
                    concludeFromCombinations(newList);
                    simpleMovesLoop();

                    if (possibilitiesGrid[x,y].Count > newList.Count){
                        possibilitiesGrid[x,y] = newList;
                        change_in_poss_occured = true;
                        Debug.Log("removed some possibilities on tile: " + x.ToString() + ", " + y.ToString());
                    }   
                }
            }

            if (change_in_poss_occured){
                Debug.Log("AI found some complicated stuff");
            }
            else{
                Debug.Log("AI didn't find any complicated stuff");
            }
        }
    }

    private void defineFilteredInfoTiles(){
        filteredInfoTiles = infoTiles.ToList();
        foreach(int[] tile in infoTiles){
            int x = tile[0];
            int y = tile[1];
            int[][] adjacentUnturnedTiles = possibilitiesGrid[x,y][0].Vectors;
            int[][] relatedTiles = getAllRelatedTiles(adjacentUnturnedTiles, x, y);            

            foreach (int[] compTile in relatedTiles){
                int comp_x = compTile[0];
                int comp_y = compTile[1];
                int[][] compAdjacentUnturnedTiles = possibilitiesGrid[comp_x,comp_y][0].Vectors;
                if(rangeIsInRange(adjacentUnturnedTiles, compAdjacentUnturnedTiles)){
                    
                    if (rangeIsInRange(compAdjacentUnturnedTiles, adjacentUnturnedTiles)){
                        // the 2 tiles array are equal - only keep one - the same one in both loops
                        if(x > comp_x || (x == comp_x && y > comp_y)){ // take right-most, if == take upmost
                            filteredInfoTiles.Remove(tile);
                            break;
                        }

                        else{// keep looking brother, there might be 3 equal ranges
                        }
                    }
                    else{
                        filteredInfoTiles.Remove(tile);
                        break;
                    }

                }
            }
        }
    }

    private bool compatible_possibilities(int[][] v1, int[][] v2, bool[] b1, bool[] b2){
        for (int i = 0; i < v1.Length; i++){
            for (int j = 0; j < v2.Length; j++){
                if (v1[i][0] == v2[j][0] && v1[i][1] == v2[j][1]){
                    if (b1[i] != b2[j]){return false;}
                }
            }
        }
        return true;
    }

    private void find_new_Atile_poss(int x, int y, PotentialMineCombination Btile, List<PotentialMineCombination> newList){
        foreach (PotentialMineCombination comb in possibilitiesGrid[x,y]){
            if(compatible_possibilities(comb.Vectors, Btile.Vectors, comb.Booleans, Btile.Booleans)){
                if(!newList.Contains(comb)){
                    newList.Add(comb);
                }
            }
        }
    }

    private PotentialMineCombination unite_combinations_if_possible(PotentialMineCombination p1, PotentialMineCombination p2){
        PotentialMineCombination newCombination = new PotentialMineCombination();
        int[][] v1 = p1.Vectors;
        int[][] v2 = p2.Vectors; 
        bool[] b1 = p1.Booleans;
        bool[] b2 = p2.Booleans;
        if(compatible_possibilities(v1, v2, b1, b2)){
            
            List<int[]> tile_list = v1.ToList();
            List<bool> bool_list = b1.ToList();
            for (int i = 0; i < v2.Length; i++){
                if (ArrayContainsArray(v1, v2[i])){
                    tile_list.Add(v2[i]);
                    bool_list.Add(b2[i]);
                }
            }
            newCombination.Vectors = tile_list.ToArray();
            newCombination.Booleans = bool_list.ToArray();
        }
        return newCombination;
    }

    private void concludeFromCombinations(List<PotentialMineCombination> possibilities){
        bool[] start_array = possibilities[0].Booleans;
        int length = start_array.Length;
        bool [] changed_array = new bool[length];
        for (int i = 0; i < length; i++){
            changed_array[i] = false;
        }
        foreach (PotentialMineCombination comb in possibilities){
            for (int j = 0; j < length; j++){
                if (!changed_array[j]){
                    if(start_array[j] != comb.Booleans[j]){
                        changed_array[j] = true;
                    }
                }
            }
        }

        int[][] tile_array = possibilities[0].Vectors;

        for (int i = 0; i < length; i++){
            if(!changed_array[i]){
                if(start_array[i]){
                    defineTileAsMine(tile_array[i][0], tile_array[i][1]);
                }
                else{
                    defineTileAsEmpty(tile_array[i][0], tile_array[i][1]);
                }
            }
        }
    }

    private int[][] getAllRelatedTiles(int[][] unturnedAdjacents, int x, int y){

        List<int[]> unfilteredRelatedTiles = new List<int[]>();

        foreach(int[] tile in unturnedAdjacents){
            List<int[]> turnedAdjacentTiles = getTurnedAdjacentTiles(tile[0], tile[1]);
            foreach(int[] newtile in turnedAdjacentTiles){
                if (!(newtile[0] == x && newtile[1] == y)){
                    if(!ListContainsArray(unfilteredRelatedTiles, newtile)){
                        unfilteredRelatedTiles.Add(newtile);
                    }
                }
            }
                
        }
        return unfilteredRelatedTiles.ToArray();
    }


    private bool rangeIsInRange(int[][] r1, int[][] r2){
        bool found_different_tile = true;
        foreach(int[] tile in r1){
            if(!ArrayContainsArray(r2, tile)) {found_different_tile = false;}
        }
        return found_different_tile;
    }   
}