using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class clickListener : MonoBehaviour
{
    public Camera maincamera;
    public GameObject gridObject; 
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

    private bool cancelNextButtonUp = false;
    
    public DropdownMenu dropdownMenu;

    private PlayerSettings.ClickActions clickaction;
    private GridDisplayManager griddy;



    void Start(){

        
        griddy = gridObject.GetComponent<GridDisplayManager>();
        maxX = griddy.grid.data.width;
        maxY = griddy.grid.data.height;
        zoomOutMax = griddy.maxCameraZoomout;
    }

    void Update(){

        // clicks and pans (1 click)
        // if(Input.GetMouseButton(0) && Input.touchCount == 1){
        if(Input.GetMouseButton(0)){
            if (Input.GetMouseButtonDown(0))
            {
                is_pan = false;
                startClickTime = Time.time;
                mouseStartPosition = Input.mousePosition;            
                worldStartPosition = Camera.main.ScreenToWorldPoint(mouseStartPosition);

                if (dropdownMenu.isExpanded && mouseStartPosition.x < 100 && mouseStartPosition.y > 750){
                    cancelNextButtonUp = true;
                }

                if(mouseStartPosition.y > 986){
                    cancelNextButtonUp = true;
                }

                if(griddy.is_finished){
                    cancelNextButtonUp = true;
                }
                
                

                

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

                    if(Time.time - startClickTime > griddy.minimumTimeToHoldFlag && 
                        griddy.clickaction == PlayerSettings.ClickActions.holdToFlag && !griddy.is_finished)
                    {
                        griddy.showHoldToFlagCompleted(worldStartPosition.x, worldStartPosition.y);
                    }

                    if ((mouseStartPosition - currentMousePosition).magnitude > minimumDistanceToPan){
                        is_pan = true;
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (cancelNextButtonUp){
                cancelNextButtonUp = false;
            }
            else if (!is_pan){
                if (griddy.clickaction == PlayerSettings.ClickActions.holdToFlag){
                    float time_delay = Time.time - startClickTime;
                    Debug.Log("timeDelay" + time_delay.ToString());
                    griddy.processClick(worldStartPosition.x, worldStartPosition.y, timediff:time_delay);
                }
                else {
                    griddy.processClick(worldStartPosition.x, worldStartPosition.y);
                }
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
