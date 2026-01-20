using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizHistoryManager : MonoBehaviour
{
    public GameObject quizHistoryPanel;
    public GameObject quizDetailsPanel;
    public GameObject quizDetailsContainerPanel;
    public GameObject quizPanelPrefab;
    public GameObject questionPanelPrefab;
    public GameObject questionAttemptPrefab;
    public string folderPath = Application.streamingAssetsPath + "/statistics/";
    public ResultData resultData;
    public int questionCount = 0;
    public Sprite checkMarkSprite;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure the quizPanel has a VerticalLayoutGroup for proper layout
        VerticalLayoutGroup layoutGroup = quizHistoryPanel.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = quizHistoryPanel.AddComponent<VerticalLayoutGroup>();
        }
        layoutGroup.spacing = 2f;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childControlHeight = false;

        PopulateQuizList();
    }

    // Add every file existing in the statistics folder to the quizHistoryPanel
    void PopulateQuizList()
    {
        if (Directory.Exists(folderPath))
        {
            //for each json file in the directory, create a new panel in the quizPanel
            string[] files = Directory.GetFiles(folderPath, "*.json");
            foreach (var file in files)
            {
                if (validateQuiz(file))
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    //Debug.Log("Found quiz file: " + fileName);

                    addQuizPanel(fileName);
                }
                else
                {
                    Debug.LogError("Invalid quiz file: " + file);
                }
            }
        }
        else
        {
            Debug.LogError("Directory not found: " + folderPath);
        }
    }
    // Create a new panel for the quiz and resize the parent panel
    public void addQuizPanel(string fileName)
    {
        string name = fileName.Split('_')[0];
        string temp_date = fileName.Split('_')[1];
        string year = temp_date.Split('-')[0];
        string month = temp_date.Split('-')[1];
        string day = temp_date.Split('-')[2];

        string date = day + "/" + month + "/" + year + " " + fileName.Split('_')[2].Replace("-", ":");
        // Create a new panel for the quiz and resize the parent panel
        GameObject panel = Instantiate(quizPanelPrefab, quizHistoryPanel.transform);
        panel.transform.Find("QuizName").gameObject.GetComponent<TextMeshProUGUI>().text = name;
        panel.transform.Find("QuizDate").gameObject.GetComponent<TextMeshProUGUI>().text = date;
        GameObject resultPanel = panel.transform.Find("ResultPanel").gameObject;

        // Change color based on result
        resultData = loadData(fileName);
        string result = resultData.score + " / " + resultData.questions.Length * 2;
        resultPanel.GetComponentInChildren<TextMeshProUGUI>().text = result;
        
        if (resultData.score >= resultData.passingScore)
        {
            resultPanel.GetComponent<Image>().color = new Color32(0, 255, 0, 228); // light green
        }
        else
        {
            resultPanel.GetComponent<Image>().color = new Color32(255, 0, 0, 200); // light red
        }

        Transform detailsTransform = panel.transform.Find("DetailsButton");
        if (detailsTransform != null)
        {
            Button detailsButton = detailsTransform.GetComponent<Button>();
            if (detailsButton != null)
            {
                //Debug.Log("Adding listener to edit button for quiz: " + fileName);
                detailsButton.onClick.AddListener(() => detailsButtonClick(fileName));
            }
        }

        quizHistoryPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(quizHistoryPanel.GetComponent<RectTransform>().sizeDelta.x, quizHistoryPanel.GetComponent<RectTransform>().sizeDelta.y + 86f);
    }
    
    public void detailsButtonClick(string fileName)
    {
        string name = fileName.Split('_')[0];
        string temp_date = fileName.Split('_')[1];
        string year = temp_date.Split('-')[0];
        string month = temp_date.Split('-')[1];
        string day = temp_date.Split('-')[2];

        string date = day + "/" + month + "/" + year + " " + fileName.Split('_')[2].Replace("-", ":");

        resetQuizDetailsPanel();
        Debug.Log("Showing details of: " + fileName);
        quizDetailsPanel.SetActive(true);
        Transform quizNameTransform = quizDetailsPanel.transform.Find("QuizNameInput");
        // Set the quiz name in the input field and make it non-interactable
        if (quizNameTransform != null)
        {
            TMP_InputField quizNameInput = quizNameTransform.GetComponent<TMP_InputField>();
            if (quizNameInput != null)
            {
                quizNameInput.text = name;
                quizNameInput.interactable = false;
            }
        }
        Transform quizDateTransform = quizDetailsPanel.transform.Find("QuizDateInput");
        // Set the quiz name in the input field and make it non-interactable
        if (quizDateTransform != null)
        {
            TMP_InputField quizDateInput = quizDateTransform.GetComponent<TMP_InputField>();
            if (quizDateInput != null)
            {
                quizDateInput.text = date;
                quizDateInput.interactable = false;
            }
        }
        

        // Populate the createQuizPanel with the quiz data
        populateQuizDetailsData(fileName);
    }
    public void resetQuizDetailsPanel()
    {
        // Clear the input field and reset the createQuizPanel
        TMP_InputField quizNameInput = quizDetailsPanel.transform.Find("QuizNameInput").GetComponent<TMP_InputField>();
        quizNameInput.text = "";
        quizNameInput.interactable = false;
        TMP_InputField quizDateInput = quizDetailsPanel.transform.Find("QuizDateInput").GetComponent<TMP_InputField>();
        quizDateInput.text = "";
        quizDateInput.interactable = false;
        foreach (Transform child in quizDetailsContainerPanel.transform)
        {
            Destroy(child.gameObject);
        }
        // Reset the size of the quizDetailsPanel
        quizDetailsContainerPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(quizDetailsContainerPanel.GetComponent<RectTransform>().sizeDelta.x, 0f);
        questionCount = 0;
        Debug.Log("Reset quiz details panel height: " + quizDetailsContainerPanel.GetComponent<RectTransform>().sizeDelta.y);
    }

    public bool validateQuiz(string path)
    {
        // string json = File.ReadAllText(path);

        // return JsonHelper.ValidateJson(json);
        return true;
    }
    public void populateQuizDetailsData(string fileName)
    {
        ResultData data = loadData(fileName);

        foreach (var question in data.questions)
        {
            addQuestionPanel(question);
        }

    }
    // Create a new question panel with existing data and resize the parent panel
    public void addQuestionPanel(ResultQuestion question = null)
    {
        questionCount++;
        // Create a new panel for the question and resize the parent panel
        GameObject panel = Instantiate(questionPanelPrefab, quizDetailsContainerPanel.transform);
        panel.GetComponentInChildren<TextMeshProUGUI>().text = "Domanda " + questionCount;
        quizDetailsContainerPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(quizDetailsContainerPanel.GetComponent<RectTransform>().sizeDelta.x, quizDetailsContainerPanel.GetComponent<RectTransform>().sizeDelta.y + 105f);

        Debug.Log("Added question panel height: " + quizDetailsContainerPanel.GetComponent<RectTransform>().sizeDelta.y);

        // Populate the fields with existing data
        // Set question text
        panel.transform.Find("QuestionNameText").gameObject.GetComponent<TextMeshProUGUI>().text = question.questionText;

        int counter = 0;
        int tot_attempts = question.selectedAnswer.Count;
        foreach (var attempt in question.selectedAnswer)
        {
            GameObject attemptPanel = Instantiate(questionAttemptPrefab, panel.transform.Find("AttemptsContainerPanel").transform);
            attemptPanel.GetComponentInChildren<TextMeshProUGUI>().text = "Scelta n." + ++counter;
            attemptPanel.transform.Find("AnswerNameText").gameObject.GetComponent<TextMeshProUGUI>().text = attempt;

            // Resize the AttemptsContainer panel
            panel.GetComponent<RectTransform>().sizeDelta = new Vector2(panel.GetComponent<RectTransform>().sizeDelta.x, panel.GetComponent<RectTransform>().sizeDelta.y + 75f);
            quizDetailsContainerPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(quizDetailsContainerPanel.GetComponent<RectTransform>().sizeDelta.x, quizDetailsContainerPanel.GetComponent<RectTransform>().sizeDelta.y + 85f);
            //panel.transform.Find("AttemptsContainerPanel").GetComponent<RectTransform>().sizeDelta = new Vector2(panel.transform.Find("AttemptsContainerPanel").GetComponent<RectTransform>().sizeDelta.x, panel.transform.Find("AttemptsContainerPanel").GetComponent<RectTransform>().sizeDelta.y + 75f);
            if(counter == tot_attempts)
            {
                attemptPanel.transform.Find("AnswerCorrect").GetComponent<Image>().sprite = checkMarkSprite;
            }
        }
    }
    // Load quiz data from file
    ResultData loadData(string fileName)
    {
        string filePath = folderPath + fileName + ".json";
        ResultData data = new ResultData();

        if (File.Exists(filePath))
        {
            if(validateQuiz(filePath) == false)
            {
                Debug.LogError("Invalid quiz file: " + fileName);
                return data;
            }
            string dataAsJson = File.ReadAllText(filePath);
            data = JsonConvert.DeserializeObject<ResultData>(dataAsJson);
            Debug.Log("File imported: " + dataAsJson);
        }
        else
        {
            Debug.LogError("Cannot find file!");
        }
        return data;
    }
}
