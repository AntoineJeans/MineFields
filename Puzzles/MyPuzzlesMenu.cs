using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;


public class MyPuzzlesMenu : MonoBehaviour
{
    public GameObject deleteButtonObject;
    private Button deleteButtonComponent;
    public Button cancelNameButton;
    public Button createNameButton;
    public GameObject NameMenu;
    public GameObject puzzlePreviewPrefab;
    public GameObject puzzlePreviewGrid;
    public TextMeshProUGUI puzzleNameField;
    int selected_puzzle_id = 0;
    private List<PuzzleData> existingPuzzles;
    // Start is called before the first frame update
    void Start()
    {
        disableNameMenu();
        PuzzleButton.OnPuzzleIdClicked += select_puzzle;
        deleteButtonComponent = deleteButtonObject.GetComponent<Button>();
        refresh();

    }

    public void refresh(){
        existingPuzzles = new List<PuzzleData>();

        foreach (Transform child in puzzlePreviewGrid.transform){
            Destroy(child.gameObject);
        }

        findAllSavedPuzzles();
        createAllPuzzlePreviews();
        deleteButtonComponent.interactable = false;
        selected_puzzle_id = 0;
        if (existingPuzzles.Count == 0){
            Debug.Log("You have no puzzles");
        }
    }

    private void findAllSavedPuzzles(){
        string folderPath = Path.Combine(Application.persistentDataPath, "myPuzzles"); 
        for (int i = 1; i < 1000; i++){
            string filePath = Path.Combine(folderPath, i.ToString() + ".guy");
            if (File.Exists(filePath)){
                existingPuzzles.Add(DataSaver.loadPuzzle(i));               
            }
        }
    }

    private void createAllPuzzlePreviews(){
        foreach (PuzzleData data in existingPuzzles){
            createPuzzlePreview(data);
        }
    }

    private void createPuzzlePreview(PuzzleData data){
        GameObject newPuzzlePreview = Instantiate(puzzlePreviewPrefab, Vector3.zero, Quaternion.identity);
        newPuzzlePreview.transform.SetParent(puzzlePreviewGrid.transform, false);
        newPuzzlePreview.GetComponent<PuzzleButton>().buttonID = data.id;

        Transform childTransform = newPuzzlePreview.transform.Find("id");
        TextMeshProUGUI textComponent = childTransform.GetComponent<TextMeshProUGUI>();
        textComponent.text = data.id.ToString();

        childTransform = newPuzzlePreview.transform.Find("name");
        textComponent = childTransform.GetComponent<TextMeshProUGUI>();
        textComponent.text = data.puzzle_name;

        childTransform = newPuzzlePreview.transform.Find("date");
        textComponent = childTransform.GetComponent<TextMeshProUGUI>();
        textComponent.text = data.modification_date;
    }

    private void select_puzzle(int id){
        selected_puzzle_id = id;
        Debug.Log($"Method called from ButtonTrigger with the puzzle id: {id}");
        deleteButtonComponent.interactable = true;
    }

    public void delete_puzzle(){
        if (selected_puzzle_id != 0){
            DataSaver.deletePuzzle(selected_puzzle_id);
            Debug.Log($"Deleting the puzzle with the puzzle id: {selected_puzzle_id}");
        }
    }

    public void cacheSelectedPuzzle(){
        if (selected_puzzle_id != 0){
            GridData griddy = DataSaver.loadPuzzle(selected_puzzle_id);
            GameParams.cache(griddy);
        }
        
    }

    public void activate_button(){
        deleteButtonComponent.interactable = true; 
    }

    public void createNewPuzzle(){
        string puzzleName = puzzleNameField.text;
        Debug.Log("Puzzle Text Name: " + puzzleName);
        // this is an invisible character for some reason
        if (puzzleName == "​"){
            puzzleName = "My New Puzzle";
        }

        PuzzleData griddata = new PuzzleData();
        griddata.puzzle_name  = puzzleName;
        griddata.width = 10;
        griddata.height = 10;
        griddata.aiStatus = GridData.AIStatus.disabled;

        griddata.objective = GridData.Objective.clear;       
        griddata.disableTiles = true;
        griddata.disableMap = Utilitary.getBigFalseArray(griddata.width, griddata.height);
        griddata.useMineMap = true;
        griddata.mineMap = Utilitary.getBigFalseArray(griddata.width, griddata.height);
        griddata.preTurnTiles = true;
        griddata.preTurnMap = Utilitary.getBigFalseArray(griddata.width, griddata.height);
        griddata.flagMap = Utilitary.getBigFalseArray(griddata.width, griddata.height);
        griddata.objectiveMap = Utilitary.getBigFalseArray(griddata.width, griddata.height);
        GameParams.cache(griddata);


    }

    public void enableNameMenu(){
        NameMenu.SetActive(true);
    }
    
    public void disableNameMenu(){
        NameMenu.SetActive(false);
    }
}
