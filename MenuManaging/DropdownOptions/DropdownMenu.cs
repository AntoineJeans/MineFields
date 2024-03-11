using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownMenu : MonoBehaviour
{
    public Vector2 spacing;
    public Button mainButton;
    private Vector2 buttPosition;
    private DropdownElement[] elements;
    public bool isExpanded = false;

    int elementsCount;
    

    void Start(){
        elementsCount = transform.childCount - 1;
        elements = new DropdownElement[elementsCount];
        for(int i = 0; i < elementsCount; i++){
            elements[i] = transform.GetChild(i + 1).GetComponent<DropdownElement>();
        }
        mainButton = transform.GetChild(0).GetComponent<Button>();
        mainButton.transform.SetAsLastSibling();
        mainButton.onClick.AddListener(OnEnabledMenu);
        buttPosition = mainButton.transform.position;
        resetPosition();
    }

    void resetPosition(){
        for(int i = 0; i < elementsCount; i++){
            elements[i].trans.position = buttPosition;
        }
    }

    void OnEnabledMenu(){
        isExpanded = !isExpanded;

        if(isExpanded){
            for(int i = 0; i < elementsCount; i++){
                elements[i].trans.position = buttPosition + spacing*(i+1) ;
            }
        }
        else{
            resetPosition();
        }
    }

    void OnDestroy(){
        mainButton.onClick.RemoveListener(OnEnabledMenu);
    }
}
