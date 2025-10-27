using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;

public class AnswerScriptTests
{
    private GameObject testGameObject;
    private AnswerScript answerScript;
    private GameObject quizManagerObject;
    private QuizManager mockQuizManager;

    [SetUp]
    public void SetUp()
    {
        // Create test GameObject with AnswerScript
        testGameObject = new GameObject("TestAnswer");
        answerScript = testGameObject.AddComponent<AnswerScript>();
        
        // Create child GameObject with TextMeshProUGUI
        GameObject textChild = new GameObject("Text");
        textChild.transform.SetParent(testGameObject.transform);
        textChild.AddComponent<TextMeshProUGUI>();

        // Create mock QuizManager
        GameData.daily = true;
        quizManagerObject = new GameObject("QuizManager");
        mockQuizManager = quizManagerObject.AddComponent<QuizManager>();
        mockQuizManager.questionText = new GameObject().AddComponent<TMPro.TextMeshProUGUI>();
        mockQuizManager.scoreText = new GameObject().AddComponent<TMPro.TextMeshProUGUI>();
        mockQuizManager.options = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            mockQuizManager.options[i] = new GameObject();
            mockQuizManager.options[i].AddComponent<AnswerScript>();
            var textObj = new GameObject();
            textObj.transform.parent = mockQuizManager.options[i].transform;
            textObj.AddComponent<TMPro.TextMeshProUGUI>();
        }
        answerScript.quizManager = mockQuizManager;
    }

    [TearDown]
    public void TearDown()
    {
        if (testGameObject != null)
        {
            Object.DestroyImmediate(testGameObject);
        }
        if (quizManagerObject != null)
        {
            Object.DestroyImmediate(quizManagerObject);
        }
    }

    [UnityTest]
    public IEnumerator Answer_WithCorrectAnswer_ChangesTextColor()
    {
        // Arrange
        answerScript.isCorrect = true;
        var textComponent = answerScript.GetComponentInChildren<TextMeshProUGUI>();
        
        // Act
        answerScript.answer();
        yield return null; // Wait one frame
        
        // Assert
        Color c;
        if (ColorUtility.TryParseHtmlString("#009900", out c)){}
        else c = Color.green; // fallback
        Assert.AreEqual(c, textComponent.color);
    }

    [UnityTest]
    public IEnumerator Answer_WithIncorrectAnswer_ChangesTextColor()
    {
        // Arrange
        answerScript.isCorrect = false;
        var textComponent = answerScript.GetComponentInChildren<TextMeshProUGUI>();
        
        // Act
        answerScript.answer();
        yield return null; // Wait one frame
        
        // Assert
        Assert.AreEqual(Color.red, textComponent.color);
    }
}