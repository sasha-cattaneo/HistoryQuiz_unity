using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ToggleCorrectAnswerHandler : MonoBehaviour
{
    public Sprite checkMarkSprite;
    public Sprite closeSprite;

    public void toggleCorrectAnswer(Button clickedButton)
    {
        Debug.Log("Clicked:" + clickedButton.name);
        if (clickedButton.GetComponentInChildren<TextMeshProUGUI>().text == "1")
        {
            Debug.Log("Deselecting correct answer");
            clickedButton.GetComponentInChildren<TextMeshProUGUI>().text = "0";
            clickedButton.GetComponentInChildren<Image>().sprite = closeSprite;
            return;
        }
        else
        {
            Debug.Log("Selecting correct answer");
            clickedButton.GetComponentInChildren<TextMeshProUGUI>().text = "1";
            clickedButton.GetComponentInChildren<Image>().sprite = checkMarkSprite;
            return;
        }
    }
}
