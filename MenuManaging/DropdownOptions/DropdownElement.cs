using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownElement : MonoBehaviour
{
    public Image icon;
    public Transform trans;
    void Awake(){
        icon = GetComponent<Image>();
        trans = transform;
    }
}
