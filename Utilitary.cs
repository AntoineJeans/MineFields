using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class Utilitary{
    public static bool[,] getBigFalseArray(int width, int height){
        bool[,] arr = new bool[width,height];
        for(int i = 0; i < width; i++){
            for(int j=0; j < height; j++){
                arr[i,j] = false;
            }
        }
        return arr;        
    }

    public static int[,] getBigZeroArray(int width, int height){
        int[,] arr = new int[width,height];
        for(int i = 0; i < width; i++){
            for(int j=0; j < height; j++){
                arr[i,j] = 0;
            }
        }
        return arr;        
    }
}