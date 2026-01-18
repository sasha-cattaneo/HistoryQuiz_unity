using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class QuizManager : MonoBehaviour
{
    public GameObject[] options;
    public int currentQuestion;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI scoreText;
    public List<QuestionData> questions;
    public int questionCount = 0;
    private int score = 0;
    public int passingScore = 0;
    private List<QuestionData> allAvailableQuestions;
    private bool quizFinished = false;
    public SceneManage sceneManager;
    public string logFilePath;
    public ResultData resultData;
    public int logQuestionCounter = -1;

    void Start()
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
        questionCount = questions.Count;
        if (GameData.daily)
        {
            // daily quiz 80% passing score
            passingScore = questionCount * 2 * 80 / 100;
        }
        else
        {
            // regular quiz passing score is 60%
            passingScore = questionCount * 2 * 60 / 100;
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
        // Initialize log file
        logFilePath = Application.streamingAssetsPath + "/statistics/daily_" + DateTime.Today.ToString("yyyy-MM-dd") + ".json";

        initializeLogFile();

    }
    private void setRegularQuiz()
    {
        string filePath = Application.streamingAssetsPath + "/Quiz/" + GameData.selectedQuiz + ".json";

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            questions = JsonHelper.FromJson<QuestionData>(dataAsJson);

            // Initialize log file
            logFilePath = Application.streamingAssetsPath + "/statistics/" + GameData.selectedQuiz + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".json";

            initializeLogFile();

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
            options[i].GetComponent<Button>().enabled = true;
            options[i].GetComponentInChildren<TextMeshProUGUI>().text = questions[currentQuestion].answers[i];
            options[i].GetComponent<AnswerScript>().answerIndex = i;
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
        
        logQuestionCounter++;
        logQuestion(questions[currentQuestion]);
        Debug.Log("Logged question: " + logQuestionCounter);

        setAnswers();
    }
    private void quizEnd()
    {
        questionText.text = "Quiz finished!";

        saveLogFile();

        foreach (var option in options)
        {
            option.SetActive(false);
        }
        string result = (score >= passingScore) ? "<color=#009900>" : "<color=red>";

        // Show final score
        questionText.text = "Quiz completato! Punteggio: " + result + score + "</color>/" + questionCount * 2;
        questionText.fontSize = 50;
        scoreText.text = "Premere un tasto per tornare al menu principale";

        Debug.Log("Quiz finished! Final score: " + score + "/" + questionCount * 2);
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
        if (quizFinished &&
            (Keyboard.current.anyKey.wasPressedThisFrame
            || Mouse.current.leftButton.wasReleasedThisFrame
            || Mouse.current.rightButton.wasReleasedThisFrame)
            )
        {
            sceneManager.LoadScene("HomepageScene");
        }

    }
    public void initializeLogFile()
    {
        resultData = new ResultData();
        resultData.date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        resultData.questions = new ResultQuestion[questions.Count];
        for (int i = 0; i < questions.Count; i++)
        {
            resultData.questions[i] = new ResultQuestion();
        }
        Debug.Log(resultData.questions.Length + " log question slots initialized.");
    }
    public void logQuestion(QuestionData question)
    {
        if (resultData != null)
        {
            resultData.questions[logQuestionCounter].questionText = question.questionText;
        }
    }
    public void logAnswer(int index)
    {
        if (resultData != null)
        {
            resultData.questions[logQuestionCounter].selectedAnswer.Add(questions[currentQuestion].answers[index]);
        }
        Debug.Log("Logged answer for question " + logQuestionCounter + "(Question: " + questions[currentQuestion].questionText + ")" + ": Answer index " + index + "(Answer: " + questions[currentQuestion].answers[index] + ")");
    }

    public void saveLogFile()
    {
        if (resultData != null)
        {
            resultData.score = score;
            resultData.passingScore = passingScore;
            string json = JsonConvert.SerializeObject(resultData, Formatting.Indented);
            File.WriteAllText(logFilePath, json);
            Debug.Log("Log file saved to: " + logFilePath);
        }
    }
}
