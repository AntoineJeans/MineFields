using UnityEngine;
using UnityEngine.UI;

public class ToggleGroupHandler : MonoBehaviour
{
    public GameObject CAtoggleGroupObject;
    public GameObject decimalToggleGroupObject;
    private PlayerSettings settings;

    private void Start()
    {
        setUpPlayerSettings();
        ToggleGroup toggleGroup = CAtoggleGroupObject.GetComponent<ToggleGroup>();
        Toggle[] toggles = toggleGroup.GetComponentsInChildren<Toggle>();
        foreach (Toggle CAtoggle in toggles){
            CAtoggle.onValueChanged.AddListener(delegate { OnChangedClickAction(CAtoggle); });

            // put toggle as it was
            if(CAtoggle.name == "N" && settings.getClickAction() == PlayerSettings.ClickActions.def){CAtoggle.isOn = true;}
            if(CAtoggle.name == "HTF" && settings.getClickAction() == PlayerSettings.ClickActions.holdToFlag){CAtoggle.isOn = true;}
            if(CAtoggle.name == "2xCTT" && settings.getClickAction() == PlayerSettings.ClickActions.doubleClickToBreak){CAtoggle.isOn = true;}
        }

        toggleGroup = decimalToggleGroupObject.GetComponent<ToggleGroup>();
        toggles = toggleGroup.GetComponentsInChildren<Toggle>();
        toggles[0].onValueChanged.AddListener(delegate { OnChangedDecimal(toggles[0]); });
        toggles[0].isOn = settings.getShowDecimal();
    }

    private void setUpPlayerSettings(){
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("PlayerSettings");
        GameObject settings_object = gameObjects[0];
        settings = settings_object.GetComponent<PlayerSettingsInstance>().settings;
    }

    private void OnChangedClickAction(Toggle changedToggle)
    {
        // Check if the toggle is now selected
        if (changedToggle.isOn)
        {
            // Print something unique to the console based on the selected toggle
            if (changedToggle.name == "N"){
                Debug.Log("Toggle N selected");
                settings.changeClickAction(PlayerSettings.ClickActions.def);
            }
            else if (changedToggle.name == "HTF"){
                Debug.Log("Toggle HTF selected");
                settings.changeClickAction(PlayerSettings.ClickActions.holdToFlag);
            }
            else if (changedToggle.name == "2xCTT"){
                Debug.Log("Toggle 2xCTT selected");
                settings.changeClickAction(PlayerSettings.ClickActions.doubleClickToBreak);
            }
        }
    }

    private void OnChangedDecimal(Toggle changedToggle){
        settings.setShowDecimal(changedToggle.isOn);
    }
        
}

