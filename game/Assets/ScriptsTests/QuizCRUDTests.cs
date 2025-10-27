using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizCRUDTests
{
    private string testQuizFolderPath;
    private string testQuizPath;
    private QuizCRUD quizCRUD;
    private string mockFileDataOneQuestion = "{\"questions\":[{\"questionText\":\"Test question\",\"answers\":[\"A\",\"B\",\"C\",\"D\"],\"correctAnswers\":[1,0,0,0]}]}";
    private string mockFileDataMultipleQuestions = "{\"questions\":[{\"questionText\":\"Test question 1\",\"answers\":[\"A\",\"B\",\"C\",\"D\"],\"correctAnswers\":[1,0,0,0]},{\"questionText\":\"Test question 2\",\"answers\":[\"A\",\"B\",\"C\",\"D\"],\"correctAnswers\":[0,1,0,0]}]}";
    // private string mockFileDataInvalid = "{\"questions\":[{\"questionText\":\"Test question\",\"answers\":[\"A\",\"B\",\"C\",\"D\"],\"correctAnswer\":[1,0,0]}]}";
    private string mockFileDataInvalid = "{\"questdddddddddddddddons\":[]}";
    [UnitySetUp]
    public IEnumerator Setup()
    {
        SceneManager.LoadScene("QuizCRUDScene");
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "QuizCRUDScene");

        quizCRUD = GameObject.FindFirstObjectByType<QuizCRUD>();
        Debug.Log("Setup completed.");
    }

    [TearDown]
    public void TearDown()
    {

        string filePath = (quizCRUD != null ? quizCRUD.folderPath : Application.streamingAssetsPath + "/Quiz/") + "TestQuiz.quiz";

        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                File.Delete(filePath + ".meta");
                Debug.Log($"Deleted test file: TestQuiz.quiz");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to delete TestQuiz.quiz: {ex.Message}");
            }
        }
        
        
        if (quizCRUD != null)
        {
            quizCRUD.questionCount = 0;
            quizCRUD.createPanel.SetActive(false);
        }
    }
    
    [UnityTest]
    public IEnumerator QuizCRUD_InitializationTest()
    {
        Assert.IsNotNull(quizCRUD.createPanel, "Create Panel is not assigned in QuizCRUD");
        Assert.IsNotNull(quizCRUD.quizPanel, "Quiz Panel is not assigned in QuizCRUD");
        Assert.IsNotNull(quizCRUD.quizPanelPrefab, "Quiz Panel Prefab is not assigned in QuizCRUD");
        Assert.IsNotNull(quizCRUD.createQuizPanel, "Create Quiz Panel is not assigned in QuizCRUD");
        Assert.IsNotNull(quizCRUD.questionPanelPrefab, "Question Panel Prefab is not assigned in QuizCRUD");

        yield return null;
    }

    [UnityTest]
    public IEnumerator QuizCRUD_CreateQuiz_with_ValidData()
    {
        // Arrange
        GameObject newQuizButton = GameObject.Find("CreateButton");
        newQuizButton.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        yield return null;
        //yield return new WaitForSeconds(10);

        // Act
        quizCRUD.createPanel.transform.Find("QuizInput").GetComponent<TMP_InputField>().text = "TestCreate";
        quizCRUD.addQuestionPanel();
        yield return null;
        GameObject questionPanel = quizCRUD.createQuizPanel.transform.Find("QuestionPanel(Clone)").gameObject;
        questionPanel.transform.Find("QuestionInput").GetComponent<TMP_InputField>().text = "TestQuestion";
        questionPanel.transform.Find("Answer1Input").GetComponent<TMP_InputField>().text = "TestAnswer";
        questionPanel.transform.Find("Answer2Input").GetComponent<TMP_InputField>().text = "TestAnswer";
        questionPanel.transform.Find("Answer3Input").GetComponent<TMP_InputField>().text = "TestAnswer";
        questionPanel.transform.Find("Answer4Input").GetComponent<TMP_InputField>().text = "TestAnswer";

        yield return null;

        quizCRUD.createEditQuiz();

        // Assert
        Assert.IsTrue(File.Exists(quizCRUD.folderPath + "TestCreate.quiz"), "Quiz file was not created successfully.");

        yield return null;
    }
    [UnityTest]
    public IEnumerator QuizCRUD_CreateQuiz_with_InvalidName()
    {
        // Arrange
        GameObject newQuizButton = GameObject.Find("CreateButton");
        newQuizButton.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        yield return null;
        //yield return new WaitForSeconds(2);

        // Act
        GameObject textField = quizCRUD.createPanel.transform.Find("QuizInput").gameObject;
        textField.GetComponent<TMP_InputField>().text = "quiz1";

        // Assert
        bool saveButtonEnabled = quizCRUD.createPanel.transform.Find("SaveText").GetComponent<Button>().interactable;

        LogAssert.Expect(LogType.Error, "A quiz with the name quiz1 already exists");
        Assert.AreEqual(saveButtonEnabled, false, "Save button should be disabled for invalid quiz name.");
        yield return null;
        //yield return new WaitForSeconds(2);
    }
    [UnityTest]
    public IEnumerator QuizCRUD_CreateQuiz_with_NoName()
    {
        // Arrange
        GameObject newQuizButton = GameObject.Find("CreateButton");
        newQuizButton.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        yield return null;
        //yield return new WaitForSeconds(2);

        // Act
        GameObject a = quizCRUD.createPanel;
        GameObject textField = a.transform.Find("QuizInput").gameObject;
        textField.GetComponent<TMP_InputField>().text = "";
        GameObject saveButton = quizCRUD.createPanel.transform.Find("SaveText").gameObject;
        saveButton.GetComponent<Button>().onClick.Invoke();
    
        // Assert
        LogAssert.Expect(LogType.Error, "Quiz name cannot be empty");
        yield return null;
        //yield return new WaitForSeconds(2);
    }

    [UnityTest]
    public IEnumerator QuizCRUD_EditQuiz_with_ValidData()
    {
        // Arrange
        File.WriteAllLines(quizCRUD.folderPath + "TestCreate.quiz", mockFileDataOneQuestion.Split('\n'));
        quizCRUD.addQuizPanel("TestCreate");
        yield return null;
        int childCount = quizCRUD.quizPanel.transform.childCount;
        GameObject testQuizPanel = quizCRUD.quizPanel.transform.GetChild(childCount - 1).gameObject;
        GameObject editButton = testQuizPanel.transform.Find("EditButton").gameObject;
        editButton.GetComponent<Button>().onClick.Invoke();
        yield return null;
        //yield return new WaitForSeconds(2);
        // Act
        quizCRUD.addQuestionPanel();
        yield return null;
        GameObject questionPanel = quizCRUD.createQuizPanel.transform.GetChild(1).gameObject;
        questionPanel.transform.Find("QuestionInput").GetComponent<TMP_InputField>().text = "TestQuestion";
        questionPanel.transform.Find("Answer1Input").GetComponent<TMP_InputField>().text = "TestAnswer";
        questionPanel.transform.Find("Answer2Input").GetComponent<TMP_InputField>().text = "TestAnswer";
        questionPanel.transform.Find("Answer3Input").GetComponent<TMP_InputField>().text = "TestAnswer";
        questionPanel.transform.Find("Answer4Input").GetComponent<TMP_InputField>().text = "TestAnswer";
        //yield return new WaitForSeconds(2);
        yield return null;
        quizCRUD.createEditQuiz();

        // Assert
        editButton.GetComponent<Button>().onClick.Invoke();
        Assert.AreEqual(2, quizCRUD.questionCount, "Quiz edit did not save correctly.");

        yield return null;
        //yield return new WaitForSeconds(2);
    }
    [UnityTest]
    public IEnumerator QuizCRUD_DeleteQuiz()
    {
        // Arrange
        File.WriteAllLines(quizCRUD.folderPath + "TestQuiz.quiz", mockFileDataOneQuestion.Split('\n'));
        quizCRUD.addQuizPanel("TestQuiz");
        yield return null;
        int originalChildCount = quizCRUD.quizPanel.transform.childCount;
        yield return null;
        //yield return new WaitForSeconds(2);
        // Act
        GameObject testQuizPanel = quizCRUD.quizPanel.transform.GetChild(originalChildCount - 1).gameObject;
        GameObject removeButton = testQuizPanel.transform.Find("RemoveButton").gameObject;
        removeButton.GetComponent<Button>().onClick.Invoke();
        yield return null;
        //yield return new WaitForSeconds(2);
        // Assert
        int newChildCount = quizCRUD.quizPanel.transform.childCount;
        Assert.AreEqual(originalChildCount - 1, newChildCount, "Quiz panel was not removed from UI.");
        Assert.IsFalse(File.Exists(quizCRUD.folderPath + "TestQuiz.quiz"), "Quiz file was not deleted.");

        yield return null;
    }
    [UnityTest]
    public IEnumerator QuizCRUD_ValidateQuizTest()
    {
        // Arrange
        // Act
        File.WriteAllText(quizCRUD.folderPath + "TestQuiz.quiz", mockFileDataOneQuestion);
        bool isValidOneQuestion = quizCRUD.validateQuiz(quizCRUD.folderPath + "TestQuiz.quiz");

        File.WriteAllText(quizCRUD.folderPath + "TestQuiz.quiz", mockFileDataMultipleQuestions);
        bool isValidMultipleQuestions = quizCRUD.validateQuiz(quizCRUD.folderPath + "TestQuiz.quiz");

        File.WriteAllText(quizCRUD.folderPath + "TestQuiz.quiz", mockFileDataInvalid);
        bool isNotValid = quizCRUD.validateQuiz(quizCRUD.folderPath + "TestQuiz.quiz");
        LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("^JSON Validation Error:.*"));
        LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("^JSON Validation Error:.*"));

        // Assert
        Assert.IsTrue(isValidOneQuestion, "The quiz validation failed for a valid quiz with one question.");
        Assert.IsTrue(isValidMultipleQuestions, "The quiz validation failed for a valid quiz with multiple questions.");
        Assert.IsFalse(isNotValid, "The quiz validation should fail for invalid quiz data.");
        yield return null;
    }
        
    /*
    [UnityTest]
    public IEnumerator QuizCRUD_ImportQuiz()
    {
        // Arrange
        File.WriteAllLines(quizCRUD.folderPath + "TestCreate.quiz", mockFileData.Split('\n'));
        quizCRUD.addQuizPanel("TestCreate");
        yield return null;
        int childCount = quizCRUD.quizPanel.transform.childCount;
        GameObject testQuizPanel = quizCRUD.quizPanel.transform.GetChild(childCount - 1).gameObject;
        GameObject editButton = testQuizPanel.transform.Find("EditButton").gameObject;
        editButton.GetComponent<Button>().onClick.Invoke();
        yield return null;
        // Act
        quizCRUD.addQuestionPanel();
        yield return null;
        GameObject questionPanel = quizCRUD.createQuizPanel.transform.GetChild(1).gameObject;
        questionPanel.transform.Find("QuestionInput").GetComponent<TMP_InputField>().text = "TestQuestion";
        questionPanel.transform.Find("Answer1Input").GetComponent<TMP_InputField>().text = "TestAnswer";
        questionPanel.transform.Find("Answer2Input").GetComponent<TMP_InputField>().text = "TestAnswer";
        questionPanel.transform.Find("Answer3Input").GetComponent<TMP_InputField>().text = "TestAnswer";
        questionPanel.transform.Find("Answer4Input").GetComponent<TMP_InputField>().text = "TestAnswer";

        yield return null;
        quizCRUD.createEditQuiz();

        // Assert
        editButton.GetComponent<Button>().onClick.Invoke();
        Assert.AreEqual(2, quizCRUD.questionCount, "Quiz edit did not save correctly.");

        yield return null;

        TearDown();
    }
    [UnityTest]
    public IEnumerator QuizCRUD_EditQuiz_with_ValidData()
    {
        // Arrange
        File.WriteAllLines(quizCRUD.folderPath + "TestCreate.quiz", mockFileData.Split('\n'));
        quizCRUD.addQuizPanel("TestCreate");
        yield return null;
        int childCount = quizCRUD.quizPanel.transform.childCount;
        GameObject testQuizPanel = quizCRUD.quizPanel.transform.GetChild(childCount - 1).gameObject;
        GameObject editButton = testQuizPanel.transform.Find("EditButton").gameObject;
        editButton.GetComponent<Button>().onClick.Invoke();
        yield return null;
        // Act
        quizCRUD.addQuestionPanel();
        yield return null;
        GameObject questionPanel = quizCRUD.createQuizPanel.transform.GetChild(1).gameObject;
        questionPanel.transform.Find("QuestionInput").GetComponent<TMP_InputField>().text = "TestQuestion";
        questionPanel.transform.Find("Answer1Input").GetComponent<TMP_InputField>().text = "TestAnswer";
        questionPanel.transform.Find("Answer2Input").GetComponent<TMP_InputField>().text = "TestAnswer";
        questionPanel.transform.Find("Answer3Input").GetComponent<TMP_InputField>().text = "TestAnswer";
        questionPanel.transform.Find("Answer4Input").GetComponent<TMP_InputField>().text = "TestAnswer";

        yield return null;
        quizCRUD.createEditQuiz();

        // Assert
        editButton.GetComponent<Button>().onClick.Invoke();
        Assert.AreEqual(2, quizCRUD.questionCount, "Quiz edit did not save correctly.");

        yield return null;

        TearDown();
    }*/
}