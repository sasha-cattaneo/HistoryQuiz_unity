using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
public static class JsonHelper
{
    public static List<T> FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.questions;
    }
    public static string ToJson<T>(List<T> list)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.questions = list;
        return JsonUtility.ToJson(wrapper, true);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> questions;
    }
}

public class QuizManager : MonoBehaviour
{
    public GameObject[] options;
    public int currentQuestion;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI scoreText;
    private List<QuestionData> questions;
    private int questionNumber = 0;
    private int score = 0;
    private int passingScore = 0;
    private List<QuestionData> allAvailableQuestions;
    private bool quizFinished = false;
    public SceneManage sceneManager;

    private void Start()
    {
        loadQuiz();
        setPassingScore();
        generateQuestion();
        scoreText.text = score.ToString();
    }
    public void correct(int value)
    {
        questions.RemoveAt(currentQuestion);
        score += value;
        scoreText.text = score.ToString();
        foreach (var option in options)
        {
            option.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        }
        options[0].GetComponent<AnswerScript>().resetValue();

        generateQuestion();
    }
    private void setPassingScore()
    {
        questionNumber = questions.Count;
        if (GameData.daily)
        {
            // daily quiz 80% passing score
            passingScore = questionNumber * 2 * 80 / 100;
        }
        else
        {
            // regular quiz passing score is 60%
            passingScore = questionNumber * 2 * 60 / 100;
        }
    }
    private void setDailyQuiz()
    {
        Debug.Log("Creating daily random quiz...");

        // Use today's date as seed for consistent daily quiz
        DateTime today = System.DateTime.Today;
        int seed = today.Year * 10000 + today.Month * 100 + today.Day;
        UnityEngine.Random.InitState(seed);

        // Load all questions
        allAvailableQuestions = new List<QuestionData>();
        string quizFile = Application.streamingAssetsPath + "/questions.json";
        if (File.Exists(quizFile))
        {
            string dataAsJson = File.ReadAllText(quizFile);
            allAvailableQuestions = JsonHelper.FromJson<QuestionData>(dataAsJson);
            Debug.Log("Total questions available: " + allAvailableQuestions.Count);
        }
        else
        {
            Debug.LogError("Cannot find file: " + quizFile);
            return;
        }
        // Select a random subset of questions for today's quiz
        int numberOfQuestions = Math.Min(10, allAvailableQuestions.Count);  // Limit to
        questions = new List<QuestionData>();
        List<int> selectedIndices = new List<int>();
        while (questions.Count < numberOfQuestions)
        {
            int randomIndex = UnityEngine.Random.Range(0, allAvailableQuestions.Count);
            if (!selectedIndices.Contains(randomIndex))
            {
                selectedIndices.Add(randomIndex);
                questions.Add(allAvailableQuestions[randomIndex]);
                Debug.Log("Selected " + questions.Count + "/" + numberOfQuestions);
            }
        }

    }
    private void setRegularQuiz()
    {
        string filePath = Application.streamingAssetsPath + "/Quiz/" + GameData.selectedQuiz + ".json";

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            questions = JsonHelper.FromJson<QuestionData>(dataAsJson);
            foreach (var q in questions)
            {
                Debug.Log(q.ToString());
            }
            Debug.Log("Questions loaded: " + questions.Count);
        }
        else
        {
            Debug.LogError("Cannot find file!");
        }
    }
    private void loadQuiz()
    {
        Debug.Log("is daily quiz: " + GameData.daily);
        if (GameData.daily)
        {
            setDailyQuiz();
        }
        else
        {
            setRegularQuiz();
        }

    }
    void setAnswers()
    {
        for (int i = 0; i < options.Length; i++)
        {
            //options[i].GetComponent<Image>().color = Color.white;
            options[i].GetComponentInChildren<TextMeshProUGUI>().text = questions[currentQuestion].answers[i];
            if (questions[currentQuestion].correctAnswers[i] == 1)
            {
                options[i].GetComponent<AnswerScript>().isCorrect = true;
            }
            else
            {
                options[i].GetComponent<AnswerScript>().isCorrect = false;
            }
        }
    }
    void generateQuestion()
    {
        if (questions.Count == 0)
        {
            quizEnd();
            return;
        }
        currentQuestion = UnityEngine.Random.Range(0, questions.Count);
        questionText.text = questions[currentQuestion].questionText;

        setAnswers();
    }
    private void quizEnd()
    {
        questionText.text = "Quiz finished!";
        foreach (var option in options)
        {
            option.SetActive(false);
        }
        string result = (score >= passingScore) ? "<color=green>" : "<color=red>";

        // Show final score
        questionText.text = "Quiz completato! Punteggio: " + result + score + "</color>/" + questionNumber * 2;
        //questionText.color = (score >= passingScore) ? Color.green : Color.red;
        questionText.fontSize = 50;
        scoreText.text = "Premere un tasto per tornare al menu principale";

        Debug.Log("Quiz finished! Final score: " + score + "/" + questionNumber * 2);
        if (GameData.daily)
        {
            if (score >= passingScore)
            {
                int dailyStreak = PlayerPrefs.GetInt("dailyStreak", 0);
                dailyStreak += 1;
                PlayerPrefs.SetInt("dailyStreak", dailyStreak);
                Debug.Log("Daily quiz passed! Current streak: " + dailyStreak);
            }
            else
            {
                PlayerPrefs.SetInt("dailyStreak", 0);
                Debug.Log("Daily quiz failed. Streak reset to 0.");
            }
            PlayerPrefs.SetString("lastDaily", DateTime.Now.ToString("yyyy-MM-dd"));
        }
        
        PlayerPrefs.Save();

        // Reset GameData for next quiz 
        GameData.daily = false;
        GameData.selectedQuiz = "";

        quizFinished = true;
    }

    void Update()
    {
        // Only listen for input if quiz is finished
        if (quizFinished && Input.anyKeyDown)
        {
            sceneManager.LoadScene("HomepageScene");
        }
    }
}
