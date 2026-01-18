using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ResultQuestion
{
    public string questionText;
    public List<string> selectedAnswer;

    public ResultQuestion()
    {
        this.selectedAnswer = new List<string>();
    }
    public override string ToString()
    {
        string result = questionText + "\n";
        result += "\nSelected Answer Index: ";
        for (int i = 0; i < selectedAnswer.Count; i++)
        {
            result += selectedAnswer[i] + ", ";
        }
        result += "\n";
        return result;
    }
}
