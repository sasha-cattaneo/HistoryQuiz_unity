using UnityEngine;
public class PanelManager : MonoBehaviour
{
    public GameObject[] panels;

    void Start()
    {
        foreach (var panel in panels)
        {
            panel.SetActive(false);
        }
    }
    public void TogglePanel(string panelName)
    {
        foreach (var panel in panels)
        {
            if (panel.name == panelName)
            {
                panel.SetActive(!panel.activeSelf);
            }
            else
            {
                panel.SetActive(false);
            }
        }
    }

}
