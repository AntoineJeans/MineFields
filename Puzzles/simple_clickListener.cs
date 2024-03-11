using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class simple_clickListener : MonoBehaviour
{
    public PuzzleCreator puz;

    public Camera maincamera;
    int minX = 0;
    int minY = 0;
    int maxX;
    int maxY;


    private Vector3 worldStartPosition;
    private Vector3 mouseStartPosition;
    private bool is_pan = false;

    private float zoomOutMin = 2;
    private float zoomOutMax;
    private float minimumDistanceToPan = 10;

    private float startClickTime;

    private PlayerSettings.ClickActions clickaction;


    void Start(){
        Debug.Log("2");
        maxX = puz.griddata.width;
        maxY = puz.griddata.height;
        zoomOutMax = puz.maxCameraZoomout;
    }

    public void updateCameraMovement(){
        maxX = puz.griddata.width;
        maxY = puz.griddata.height;
        zoomOutMax = puz.maxCameraZoomout;
    }

    void Update(){

        // clicks and pans (1 click)
        // if(Input.GetMouseButton(0) && Input.touchCount == 1){
        if(Input.GetMouseButton(0)){
            if (Input.GetMouseButtonDown(0))
            {
                mouseStartPosition = Input.mousePosition;
                worldStartPosition = Camera.main.ScreenToWorldPoint(mouseStartPosition);
                is_pan = false;
            }
            else{
                Vector3 currentMousePosition = Input.mousePosition;
                Vector3 currentPostion = maincamera.ScreenToWorldPoint(currentMousePosition);
                Vector3 direction = worldStartPosition - currentPostion;
                Vector3 newPosition = maincamera.transform.position + direction;

                newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
                newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
                maincamera.transform.position = newPosition;

                if (!is_pan){
                    if ((mouseStartPosition - currentMousePosition).magnitude > minimumDistanceToPan)
                    {
                        is_pan = true;
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!is_pan){
                puz.processClick(worldStartPosition.x, worldStartPosition.y);
            }
        }

        if(Input.touchCount >= 2){
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;
            float difference = currentMagnitude - prevMagnitude;
            zoom(difference * 0.01f);
        }

         if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                zoom(1);
            }
        if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                zoom(-1);
            }
	}

    void zoom(float increment){
        maincamera.orthographicSize = Mathf.Clamp(maincamera.orthographicSize - increment, zoomOutMin, zoomOutMax);
    }

}
