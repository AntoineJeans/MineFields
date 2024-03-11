public interface ObjectiveSolver
{
    public bool turnIsOK(int x, int y);
    public bool flagIsOK(int x, int y, bool is_flagged);
    public bool checkWin();
}

public class NTurns : ObjectiveSolver
{
    private int turns = 0;
    private int required_turns;
    public NTurns(int turns){
        required_turns = turns;
    }
    public bool turnIsOK(int x, int y){
        turns++;
        return true;
    }
    public bool flagIsOK(int x, int y, bool is_flagged){
        return true;
    }
    public bool checkWin(){
        return turns >= required_turns;
    }
}

public class Clear : ObjectiveSolver
{
    private int unturnedTiles;
    public Clear(int unturnedTilesAtTheStart){
        unturnedTiles = unturnedTilesAtTheStart;
    }
    public bool turnIsOK(int x, int y){
        unturnedTiles--;
        return true;
    }
    public bool flagIsOK(int x, int y, bool is_flagged){
        return true;
    }
    public bool checkWin(){
        return unturnedTiles == 0;
    }
}
