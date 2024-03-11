using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class DataSaver : MonoBehaviour
{

    public static void saveSettings(PlayerSettingsData data){
        string filePath = Path.Combine(Application.persistentDataPath, "PlayerSettings.guy");
        try{
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(filePath, FileMode.Create))                {
                formatter.Serialize(stream, data);
            }
            Debug.Log("GridData saved to: " + filePath);
        }
        catch (Exception e){
            Debug.LogError("Error saving puzzle: " + e.Message);
        }
    }

    public static PlayerSettings loadSettings(){
        string path = Path.Combine(Application.persistentDataPath, "PlayerSettings.guy");
        if (File.Exists(path))        {
            Debug.Log("File found!");
            using (FileStream stream = new FileStream(path, FileMode.Open))            {
                BinaryFormatter formatter = new BinaryFormatter();
                PlayerSettingsData data = formatter.Deserialize(stream) as PlayerSettingsData;
                return new PlayerSettings(data);
            }
        }
        else {return new PlayerSettings();}
    }
    


    public static void saveLastGame(GridData data){
        string filePath = Path.Combine(Application.persistentDataPath, "lastGame.guy");
        SaveGridData(filePath, data);
    }

    public static GridData loadLastGame(){
        string filePath = Path.Combine(Application.persistentDataPath, "lastGame.guy");
        return loadGridData(filePath);
    }


    public static void savePuzzle(PuzzleData data){
        Debug.Log("saving puzzle");
        string folderPath = Path.Combine(Application.persistentDataPath, "myPuzzles"); 
        Directory.CreateDirectory(folderPath);
        
        string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
        string filePath = "";
        
        if(data.id != 0){
            filePath = Path.Combine(folderPath, data.id.ToString() + ".guy");
        }
        else{
            // find an id for the new puzzle
            for (int i = 1; i < 1000; i++){
                filePath = Path.Combine(folderPath, i.ToString() + ".guy");
                if (!File.Exists(filePath)){
                    data.id = i;
                    // creation date only assigned if the grid doesn't have an ID (it's a new one)
                    data.creation_date = currentDate;
                    break;
                }
            }
        }

        if(!string.IsNullOrEmpty(filePath)){
            // assign modification date right before saving
            data.modification_date = currentDate;
            SaveGridData(filePath, data);
        }
        Debug.Log("puzzle saved at : " +  data.id.ToString());
    }

    public static void deletePuzzle(int id){
        string folderPath = Path.Combine(Application.persistentDataPath, "myPuzzles"); 
        string filePath = Path.Combine(folderPath, id.ToString() + ".guy");
        File.Delete(filePath);
    }

    public static PuzzleData loadPuzzle(int id){
        string folderPath = Path.Combine(Application.persistentDataPath, "myPuzzles"); 
        string filePath = Path.Combine(folderPath, id.ToString() + ".guy");
        GridData griddata = loadGridData(filePath);
        return griddata as PuzzleData;
    }




    private static void SaveGridData(string path, GridData data){
        try{
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(path, FileMode.Create)){
                formatter.Serialize(stream, data);
            }
            Debug.Log("GridData saved to: " + path);
        }
        catch (Exception e){
            Debug.LogError("Error saving puzzle: " + e.Message);
        }
    }
    
    private static GridData loadGridData(string path){
        try{
            if (File.Exists(path)){
                Debug.Log("loading GridData - File found!");
                using (FileStream stream = new FileStream(path, FileMode.Open)){
                    BinaryFormatter formatter = new BinaryFormatter();
                    GridData data = formatter.Deserialize(stream) as GridData;
                    return data;
                }
            }
            else {return new GridData();}
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading data: " + e.Message);
            return new GridData();
        }
    }

}

