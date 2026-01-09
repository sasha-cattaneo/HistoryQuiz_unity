using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class QuizCRUD : MonoBehaviour
{
    public GameObject createPanel;
    public GameObject quizPanel;
    public GameObject quizPanelPrefab;
    public GameObject createQuizPanel;
    public GameObject questionPanelPrefab;
    public TextMeshProUGUI errorMessage;
    public GameObject saveButton;
    public float messageDuration = 4f;
    private bool isEdit = false;
    public string folderPath = Application.streamingAssetsPath + "/Quiz/";

    // Keep track of the number of questions added
    public int questionCount = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        errorMessage.gameObject.SetActive(false);

        // Ensure the quizPanel has a VerticalLayoutGroup for proper layout
        VerticalLayoutGroup layoutGroup = quizPanel.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = quizPanel.AddComponent<VerticalLayoutGroup>();
        }
        layoutGroup.spacing = 2f;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childControlHeight = false;

        PopulateQuizList();
    }
    // Add every quiz existing in the Quiz folder to the quizPanel
    void PopulateQuizList()
    {
        string filePath = Application.streamingAssetsPath + "/Quiz/";

        if (Directory.Exists(filePath))
        {
            //for each json file in the directory, create a new panel in the quizPanel
            string[] files = Directory.GetFiles(filePath, "*.quiz");
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
            Debug.LogError("Directory not found: " + filePath);
        }
    }
    // Create a new panel for the quiz and resize the parent panel
    public void addQuizPanel(string fileName)
    {
        // Create a new panel for the quiz and resize the parent panel
        GameObject panel = Instantiate(quizPanelPrefab, quizPanel.transform);
        panel.GetComponentInChildren<TextMeshProUGUI>().text = fileName;
        Transform editTransform = panel.transform.Find("EditButton");
        if (editTransform != null)
        {
            Button editButton = editTransform.GetComponent<Button>();
            if (editButton != null)
            {
                //Debug.Log("Adding listener to edit button for quiz: " + fileName);
                editButton.onClick.AddListener(() => editQuizButtonClick(fileName));
            }
        }
        Transform deleteTransform = panel.transform.Find("RemoveButton");
        if (deleteTransform != null)
        {
            Button deleteButton = deleteTransform.GetComponent<Button>();
            if (deleteButton != null)
            {
                //Debug.Log("Adding listener to delete button for quiz: " + fileName);
                deleteButton.onClick.AddListener(() => deleteQuiz(fileName));
            }
        }
        Transform exportTransform = panel.transform.Find("ExportButton");
        if (exportTransform != null)
        {
            Button exportButton = exportTransform.GetComponent<Button>();
            if (exportButton != null)
            {
                //Debug.Log("Adding listener to export button for quiz: " + fileName);
                exportButton.onClick.AddListener(() => exportQuiz(fileName));
            }
        }
        quizPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(quizPanel.GetComponent<RectTransform>().sizeDelta.x, quizPanel.GetComponent<RectTransform>().sizeDelta.y + 86f);
    }
    // Create a new question panel and resize the parent panel
    public void addQuestionPanel()
    {
        questionCount++;
        // Create a new panel for the question and resize the parent panel
        GameObject panel = Instantiate(questionPanelPrefab, createQuizPanel.transform);
        panel.GetComponentInChildren<TextMeshProUGUI>().text = "Question " + questionCount;
        createQuizPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(createQuizPanel.GetComponent<RectTransform>().sizeDelta.x, createQuizPanel.GetComponent<RectTransform>().sizeDelta.y + 244f);
    }
    // Create a new question panel with existing data and resize the parent panel
    public void addQuestionPanel(QuestionData question = null)
    {
        questionCount++;
        // Create a new panel for the question and resize the parent panel
        GameObject panel = Instantiate(questionPanelPrefab, createQuizPanel.transform);
        panel.GetComponentInChildren<TextMeshProUGUI>().text = "Question " + questionCount;
        createQuizPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(createQuizPanel.GetComponent<RectTransform>().sizeDelta.x, createQuizPanel.GetComponent<RectTransform>().sizeDelta.y + 244f);

        // If we are editing an existing quiz, populate the fields with existing data
        // Set question text
        Transform inputTransform = panel.transform.Find("QuestionInput");
        if (inputTransform != null)
        {
            TMP_InputField textInput = inputTransform.GetComponent<TMP_InputField>();
            if (textInput != null)
            {
                textInput.text = question.questionText;
            }
        }
        // Set answers text and correct buttons
        for (int i = 1; i <= 4; i++)
        {
            // Set answer text
            Transform answerInputTransform = panel.transform.Find("Answer" + i + "Input");
            if (answerInputTransform != null)
            {
                TMP_InputField textInput = answerInputTransform.GetComponent<TMP_InputField>();
                if (textInput != null)
                {
                    textInput.text = question.answers[i - 1];
                }
            }
            // Set correct button
            Transform buttonTransform = panel.transform.Find("Answer" + i + "Button");
            if (buttonTransform != null)
            {
                TextMeshProUGUI buttonText = buttonTransform.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    if (question.correctAnswers[i - 1] == 1)
                    {
                        //buttonTransform.GetComponentInChildren<Image>().sprite = checkMarkSprite;
                        buttonTransform.GetComponent<Button>().onClick.Invoke();
                    }
                }
            }
        }
    }
    // Remove the last question panel and resize the parent panel
    public void removeQuestionPanel()
    {
        // Remove the last question panel and resize the parent panel
        if (createQuizPanel.transform.childCount > 0)
        {
            Destroy(createQuizPanel.transform.GetChild(createQuizPanel.transform.childCount - 1).gameObject);
            createQuizPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(createQuizPanel.GetComponent<RectTransform>().sizeDelta.x, createQuizPanel.GetComponent<RectTransform>().sizeDelta.y - 244f);
            questionCount--;
        }
    }
    // Create or edit a quiz
    public void createEditQuiz()
    {
        // Get the quiz name from the input field in the createPanel
        TMP_InputField quizNameInput = createPanel.transform.Find("QuizInput").GetComponent<TMP_InputField>();
        string quizName = quizNameInput.text;

        string filePath = folderPath + quizName + ".quiz";
        // Validate quiz
        if (validateQuiz() == -1)
        {
            return;
        }
        // Create quiz data structure
        List<QuestionData> questions = createQuizDataStructure();
        // Serialize the quiz data to JSON
        string jsonContent = JsonHelper.ToJson(questions);
        Debug.Log("Serialized quiz data: " + jsonContent);
        // Save the JSON to a file
        File.WriteAllText(filePath, jsonContent);
        Debug.Log("Created new quiz: " + quizName);
        if (!isEdit)
        {
            addQuizPanel(quizName);
        }
        resetCreateQuizPanel();
        createPanel.SetActive(false);
        isEdit = false;
    }
    // Reset the create quiz panel to its initial state
    public void resetCreateQuizPanel()
    {
        // Clear the input field and reset the createQuizPanel
        TMP_InputField quizNameInput = createPanel.transform.Find("QuizInput").GetComponent<TMP_InputField>();
        quizNameInput.text = "";
        quizNameInput.interactable = true;
        foreach (Transform child in createQuizPanel.transform)
        {
            Destroy(child.gameObject);
        }
        // Reset the size of the createQuizPanel and question count
        createQuizPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(createQuizPanel.GetComponent<RectTransform>().sizeDelta.x, 0f);
        questionCount = 0;
    }
    // Event on click of the edit button of a quiz
    public void editQuizButtonClick(string quizName)
    {
        resetCreateQuizPanel();
        Debug.Log("Editing quiz: " + quizName);
        isEdit = true;
        createPanel.SetActive(true);
        Transform quizNameTransform = createPanel.transform.Find("QuizInput");
        // Set the quiz name in the input field and make it non-interactable
        if (quizNameTransform != null)
        {
            TMP_InputField quizNameInput = quizNameTransform.GetComponent<TMP_InputField>();
            if (quizNameInput != null)
            {
                quizNameInput.text = quizName;
                quizNameInput.interactable = false;
            }
        }
        

        // Populate the createQuizPanel with the quiz data
        populateQuizData(quizName);
    }
    // Populate the createQuizPanel with the quiz data
    void populateQuizData(string quizName)
    {
        List<QuestionData> questions = loadQuiz(quizName);
        foreach (var question in questions)
        {
            addQuestionPanel(question);
        }
    }
    // Load quiz data from file
    List<QuestionData> loadQuiz(string quizName)
    {
        string filePath = folderPath + quizName + ".quiz";
        List<QuestionData> questions = new List<QuestionData>();

        if (File.Exists(filePath))
        {
            if(validateQuiz(filePath) == false)
            {
                Debug.LogError("Invalid quiz file: " + quizName);
                return questions;
            }
            string dataAsJson = File.ReadAllText(filePath);
            questions = JsonHelper.FromJson<QuestionData>(dataAsJson);
            Debug.Log("Questions loaded: " + questions.Count);
        }
        else
        {
            Debug.LogError("Cannot find file!");
        }
        return questions;
    }
    // Create quiz data structure
    List<QuestionData> createQuizDataStructure()
    {
        // Create quiz data structure
        List<QuestionData> questions = new List<QuestionData>();

        foreach (Transform questionPanel in createQuizPanel.transform)
        {
            // Create QuestionData and add to list
            QuestionData questionData = new QuestionData();
            // Find the QuestionInput GameObject by name
            Transform inputTransform = questionPanel.Find("QuestionInput");
            if (inputTransform != null)
            {
                // Get the TMP_InputField component
                TMP_InputField textInput = inputTransform.GetComponent<TMP_InputField>();

                if (textInput != null)
                {
                    string questionText = textInput.text;
                    //Debug.Log("Found question: " + questionText);

                    questionData.questionText = questionText;
                }
            }
            else
            {
                Debug.LogError("QuestionInput GameObject not found in panel: " + questionPanel.name);
                break;
            }
            for (int i = 1; i <= 4; i++)
            {
                // Find the AnswerInput GameObject by name
                inputTransform = questionPanel.Find("Answer" + i + "Input");
                if (inputTransform != null)
                {
                    // Get the TMP_InputField component
                    TMP_InputField textInput = inputTransform.GetComponent<TMP_InputField>();

                    if (textInput != null)
                    {
                        string answerText = textInput.text;
                        //Debug.Log("Found answer " + i + ": " + answerText);

                        // Add answer to QuestionData
                        questionData.answers[i - 1] = answerText;
                    }
                }
                else
                {
                    Debug.LogError("Answer" + i + "Input GameObject not found in panel: " + questionPanel.name);
                    break;
                }

                // Find the AnswerButton GameObject by name
                inputTransform = questionPanel.Find("Answer" + i + "Button");
                if (inputTransform != null)
                {
                    // Get the TextMeshProUGUI component
                    TextMeshProUGUI textInput = inputTransform.GetComponentInChildren<TextMeshProUGUI>();

                    if (textInput != null)
                    {
                        string answerText = textInput.text;
                        //Debug.Log("Answer " + i + " is : " + (answerText == "1" ? "correct" : "incorrect"));

                        // Add answer to QuestionData
                        questionData.correctAnswers[i - 1] = int.Parse(answerText);
                    }
                    else
                    {
                        Debug.LogError("TMP_InputField component not found on: " + inputTransform.name);
                        break;
                    }
                }
                else
                {
                    Debug.LogError("Answer" + i + "Button GameObject not found in panel: " + questionPanel.name);
                    break;
                }
            }

            questions.Add(questionData);
            //Debug.Log("Added question: " + questionData.ToString());
        }

        return questions;
    }
    // Validate quiz data when creating a new quiz
    int validateQuiz()
    {
        if (!isEdit)
        {
            // Get the quiz name from the input field in the createPanel
            TMP_InputField quizNameInput = createPanel.transform.Find("QuizInput").GetComponent<TMP_InputField>();
            string quizName = quizNameInput.text;
            // Validate quiz name
            if (checkQuizName(quizName) == -1)
            {
                return -1;
            }
        }
    
        // Validate questions
        if (validateQuizQuestions() == -1)
        {
            return -1;
        }

        return 0;
    }
    // Validate quiz name at runtime when editing the input field
    public void checkQuizNameRuntime(string value)
    {
        // Skip validation when editing 
        if (isEdit) return;

        string quizName = value;
        string filePath = folderPath + quizName + ".quiz";
        if (File.Exists(filePath))
        {
            ShowMessage("Nome quiz già esistente", Color.red);
            Debug.LogError("A quiz with the name " + quizName + " already exists");
            saveButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            errorMessage.gameObject.SetActive(false);
            saveButton.GetComponent<Button>().interactable = true;
        }
    }
    // Validate quiz name when creating a new quiz
    int checkQuizName(string quizName)
    {
        if (string.IsNullOrEmpty(quizName))
        {
            ShowMessage("Nome quiz non può essere vuoto", Color.red);
            Debug.LogError("Quiz name cannot be empty");
            return -1;
        }
        // Check if a quiz with the same name already exists
        string filePath = folderPath + quizName + ".quiz";
        if (File.Exists(filePath))
        {
            ShowMessage("Nome quiz già esistente", Color.red);
            Debug.LogError("A quiz with the name " + quizName + " already exists");
            return -1;
        }
        return 0;
    }
    // Validate quiz questions when creating or editing a quiz
    int validateQuizQuestions()
    {
        // Validate that at least one question is added
        if (questionCount == 0)
        {
            ShowMessage("Devi aggiungere almeno una domanda", Color.red);
            Debug.LogError("You must add at least one question");
            return -1;
        }

        foreach (Transform questionPanel in createQuizPanel.transform)
        {
            bool hasEmptyQuestion = false;
            bool hasCorrectAnswer = false;
            int answersFilled = 0;
            int lastEmptyAnswerIndex = -1;
            bool emptyAnswerIsCorrect = false;

            // Find the QuestionInput GameObject by name
            Transform inputTransform = questionPanel.Find("QuestionInput");
            if (inputTransform != null)
            {
                // Get the TMP_InputField component
                TMP_InputField textInput = inputTransform.GetComponent<TMP_InputField>();

                if (textInput != null)
                {
                    string questionText = textInput.text;
                    if (string.IsNullOrEmpty(questionText))
                    {
                        hasEmptyQuestion = true;
                    }
                    Debug.Log("Found question: " + questionText);
                }
            }
            else
            {
                Debug.LogError("QuestionInput GameObject not found in panel: " + questionPanel.name);
                break;
            }
            for (int i = 1; i <= 4; i++)
            {
                // Find the AnswerInput GameObject by name
                inputTransform = questionPanel.Find("Answer" + i + "Input");
                if (inputTransform != null)
                {
                    // Get the TMP_InputField component
                    TMP_InputField textInput = inputTransform.GetComponent<TMP_InputField>();

                    if (textInput != null)
                    {
                        string answerText = textInput.text;
                        if (!string.IsNullOrEmpty(answerText))
                        {
                            answersFilled++;
                        }
                        else
                        {
                            lastEmptyAnswerIndex = i;
                        }
                        Debug.Log("Found answer " + i + ": " + answerText);
                    }
                }
                else
                {
                    Debug.LogError("Answer" + i + "Input GameObject not found in panel: " + questionPanel.name);
                    break;
                }

                // Find the AnswerButton GameObject by name
                inputTransform = questionPanel.Find("Answer" + i + "Button");
                if (inputTransform != null)
                {
                    // Get the TextMeshProUGUI component
                    TextMeshProUGUI textInput = inputTransform.GetComponentInChildren<TextMeshProUGUI>();

                    if (textInput != null)
                    {
                        string answerText = textInput.text;
                        if (answerText == "1")
                        {
                            if(lastEmptyAnswerIndex == i)
                            {
                                emptyAnswerIsCorrect = true;
                            }
                            else
                            {
                                hasCorrectAnswer = true;
                            }
                            
                        }
                        Debug.Log("Answer " + i + " is : " + (answerText == "1" ? "correct" : "incorrect"));
                    }
                    else
                    {
                        Debug.LogError("TMP_InputField component not found on: " + inputTransform.name);
                        break;
                    }
                }
                else
                {
                    Debug.LogError("Answer" + i + "Button GameObject not found in panel: " + questionPanel.name);
                    break;
                }
            }
            if (hasEmptyQuestion)
            {
                ShowMessage("Ogni domanda deve avere un testo", Color.red);
                Debug.LogError("Each question must have text");
                return -1;
            }
            if (answersFilled < 2)
            {
                ShowMessage("Ogni domanda deve avere almeno due risposte", Color.red);
                Debug.LogError("Each question must have at least two answers");
                return -1;
            }
            if(emptyAnswerIsCorrect)
            {
                ShowMessage("Le risposte vuote non possono essere corrette", Color.red);
                Debug.LogError("Empty answers cannot be correct");
                return -1;
            }
            if(!hasCorrectAnswer)
            {
                ShowMessage("Ogni domanda deve avere almeno una risposta corretta", Color.red);
                Debug.LogError("Each question must have at least one correct answer");
                return -1;
            }
            
        }

        return 0;
    }
    // Show an error message for a few seconds
    public void ShowMessage(string message, Color color)
    {
        //Debug.Log("Showing message: " + message);
        if (errorMessage != null)
        {
            errorMessage.color = color;
            errorMessage.text = message;
            errorMessage.gameObject.SetActive(true);

            // Hide message after duration
            StartCoroutine(HideMessageAfterDelay());
        }
    }
    // Coroutine to hide the error message after a delay
    IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        if (errorMessage != null)
        {
            errorMessage.gameObject.SetActive(false);
        }
    }
    // Delete a quiz file and remove its panel from the quizPanel
    public void deleteQuiz(string fileName)
    {
        //Debug.Log("Attempting to delete quiz: " + fileName);
        string filePath = folderPath + fileName + ".quiz";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            File.Delete(filePath + ".meta"); // Also delete the .meta file
            Debug.Log("Deleted quiz: " + fileName);
            // Find and remove the panel with matching TextMeshPro text
            RemoveQuizPanelByFileName(fileName);
        }
        else
        {
            Debug.LogError("Quiz not found: " + fileName);
        }
    }
    // Remove the quiz panel with the specified file name
    private void RemoveQuizPanelByFileName(string fileName)
    {
        // Loop through all children of quizPanel
        foreach (Transform child in quizPanel.transform)
        {
            // Get the TextMeshProUGUI component from the child
            TextMeshProUGUI textComponent = child.GetComponentInChildren<TextMeshProUGUI>();

            if (textComponent != null && textComponent.text == fileName)
            {
                // Found the matching panel, destroy it
                Destroy(child.gameObject);

                // Adjust the quizPanel height
                RectTransform quizPanelRect = quizPanel.GetComponent<RectTransform>();
                quizPanelRect.sizeDelta = new Vector2(quizPanelRect.sizeDelta.x, quizPanelRect.sizeDelta.y - 86f);

                //Debug.Log("Removed panel for quiz: " + fileName);
                return; // Exit after finding and removing the panel
            }
        }

        Debug.LogWarning("Could not find panel with text: " + fileName);
    }
    // Import a quiz from a specified file
    public void importQuiz()
    {
        // Open file with filter
        var extensions = new[] {
            new ExtensionFilter("Quiz File", "quiz"),
            new ExtensionFilter("Tutti i file", "*" ),
        };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Importa File", "", extensions, true);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string msg = "";
            bool first = true;
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                if (!first)
                {
                    Debug.Log(path.Substring(0, 1)[0] + " rimosso");
                    path = path.Remove(0, 1);
                }
                else
                {
                    first = false;
                }
                Debug.Log("Importing quiz from: " + path);
                string fileName = Path.GetFileName(path);
                string destFilePath = folderPath + fileName;
                Debug.Log("Destination file path: " + destFilePath);
                if (File.Exists(destFilePath))
                {
                    msg += "<color=red>" + fileName.Replace(".quiz", "") + " esiste già. </color>\n";
                    Debug.LogError("A quiz with the name " + fileName + " already exists");
                }
                else
                {
                    // Validate JSON
                    if (validateQuiz(path) == false)
                    {
                        msg += "<color=red>" + fileName.Replace(".quiz", "") + " non è un file quiz valido. </color>\n";
                        Debug.LogError("Invalid quiz file: " + fileName);
                    }
                    else
                    {
                        File.Copy(path, destFilePath);
                        //Debug.Log("Imported quiz: " + fileName);
                        msg += "<color=#009900>" + fileName.Replace(".quiz", "") + " importato con successo</color>\n";
                        addQuizPanel(fileName.Replace(".quiz", ""));
                    }
                }
            }
            ShowMessage(msg, Color.black);
        }
        else
        {
            Debug.Log("No file selected for import");
        }
    }
    // Export a quiz to a specified file
    public void exportQuiz(string quizName)
    {
        string sourceFilePath = folderPath + quizName + ".quiz";
        if (!File.Exists(sourceFilePath))
        {
            Debug.LogError("Quiz file not found: " + sourceFilePath);
            ShowMessage("File non trovato", Color.red);
            return;
        }

        var extensions = new[] {
            new ExtensionFilter("Quiz File", "quiz"),
            new ExtensionFilter("Tutti i file", "*" ),
        };
        var path = StandaloneFileBrowser.SaveFilePanel("Salva File", "", quizName + ".quiz", extensions);
        //Debug.Log("Selected save path: " + path);
        if (!string.IsNullOrEmpty(path))
        {
            File.Copy(sourceFilePath, path, true);
            //Debug.Log("Exported quiz to: " + path);
            ShowMessage("Esportato con successo", Color.green);
        }
        else
        {
            Debug.Log("No file selected for export");
        }
    }
    // Validate quiz JSON structure
    public bool validateQuiz(string path){
        string json = File.ReadAllText(path);

        return JsonHelper.ValidateJson(json);
    }
}
