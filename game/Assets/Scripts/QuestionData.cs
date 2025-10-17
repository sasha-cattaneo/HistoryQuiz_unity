
[System.Serializable]
public class QuestionData 
{
    public string questionText;
    public string[] answers;
    public int[] correctAnswers;

    public QuestionData()
    {
        this.answers = new string[4];
        this.correctAnswers = new int[4];
    }
    public override string ToString()
    {
        string result = questionText + "\n";
        result += "Answers: ";
        for (int i = 0; i < answers.Length; i++)
        {
            result += answers[i] + ", ";
        }
        result += "\nCorrect Answer Index: " + correctAnswers;

        return result;
    }
}
