using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class QuizManagerTests
{
    private GameObject quizManagerObject;
    private QuizManager quizManager;

    [SetUp]
    public void Setup()
    {
        quizManagerObject = new GameObject();
        quizManager = quizManagerObject.AddComponent<QuizManager>();
        quizManager.options = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            quizManager.options[i] = new GameObject();
            quizManager.options[i].AddComponent<AnswerScript>();
            var textObj = new GameObject();
            textObj.transform.parent = quizManager.options[i].transform;
            textObj.AddComponent<TMPro.TextMeshProUGUI>();
        }
        quizManager.questionText = new GameObject().AddComponent<TMPro.TextMeshProUGUI>();
        quizManager.scoreText = new GameObject().AddComponent<TMPro.TextMeshProUGUI>();
        quizManager.sceneManager = new GameObject().AddComponent<SceneManage>();
        quizManager.currentQuestion = 0;
        QuestionData sampleQuestion = new QuestionData
        {
            questionText = "Qual'è la capitale d'Italia?",
            answers = new string[] { "Roma", "Milano", "Napoli", "Bergamo" },
            correctAnswers = new int[] { 1, 0, 0, 0 }
        };
        quizManager.questions = new List<QuestionData>{sampleQuestion};
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(quizManagerObject);
    }

    [Test]
    public void QuizManager_Correct_IncreasesScore()
    {
        // Arrange
        int initialScore = 0;
        quizManager.scoreText.text = initialScore.ToString();
        int valueToAdd = 10;

        // Act
        quizManager.correct(valueToAdd);

        // Assert
        int score = initialScore + valueToAdd;
        string result = (score >= quizManager.passingScore) ? "<color=green>" : "<color=red>";
        string expectedScoreText = "Quiz completato! Punteggio: " + result + score + "</color>/" + quizManager.questionCount * 2;
        string text = quizManager.questionText.text;
        Assert.AreEqual(expectedScoreText, text);
    }
}