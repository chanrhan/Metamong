using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    [SerializeField]
    private GameObject menu;
    [SerializeField]
    private GameObject contents;

    private Button[] menuButtons;
    private GameObject[] contentPanels;

    private GameObject activePanel;

    void Awake()
    {
        menuButtons = menu.GetComponentsInChildren<Button>();
        contentPanels = contents.GetComponentsInChildren<GameObject>();

        for(int i=0;i<menuButtons.Count();++i){
            Button button = menuButtons[i];

            button.onClick.AddListener(()=>{
                OnMenuChanged(i);
            });
        }
    }

    private void OnMenuChanged(int index){
        if(activePanel != null){
            activePanel.SetActive(false);
        }
        activePanel = contentPanels[index];
        if(activePanel != null){
            activePanel.SetActive(true);
        }
    }


}
