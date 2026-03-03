using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomepageManager : MonoBehaviour
{
    public TMP_Dropdown categoryDropdown;
    [SerializeField] Slider volumeSlider;
    public TMP_Text dailyScoreText;
    public Button dailyButton;
    private bool isDailyDone = false;
    public string log_folder_path;
    public string quiz_folder_path;
    void Start()
    {
        log_folder_path = Path.Combine(Application.persistentDataPath, "statistics");
        quiz_folder_path = Path.Combine(Application.persistentDataPath, "Quiz");
        Debug.Log("Persistent data path: " + Application.persistentDataPath);
        Debug.Log("Log folder path: " + log_folder_path);
        Debug.Log("Quiz folder path: " + quiz_folder_path);
        if (!Directory.Exists(log_folder_path))
        {
            Directory.CreateDirectory(log_folder_path);
            Debug.Log("Created log folder at: " + log_folder_path);
        }
        if (!Directory.Exists(quiz_folder_path))
        {
            Directory.CreateDirectory(quiz_folder_path);
            Debug.Log("Created quiz folder at: " + quiz_folder_path);
        }
        quiz_folder_path = quiz_folder_path + "/";

        string lastDaily = PlayerPrefs.GetString("lastDaily", "");Debug.Log("Last daily date: " + lastDaily);
        Debug.Log("is daily set: "+ GameData.daily);
        isDailyDone = lastDaily == System.DateTime.Today.ToString("yyyy-MM-dd");
        populateDropdown();
        setQuiz(0);
        loadVolume();
        if (isDailyDone)
        {
            dailyButton.interactable = false;
        }
        else if (lastDaily != System.DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd") && lastDaily != "")
        {
            ResetDailyStreak();
        }
        loadDailyScore();
    }
    void populateDropdown()
    {
        categoryDropdown.ClearOptions();

        string filePath = quiz_folder_path;

        List<string> quizOptions = new List<string>();

        if (Directory.Exists(filePath))
        {
            string[] files = Directory.GetFiles(filePath, "*.json");
            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (JsonHelper.ValidateJson(File.ReadAllText(file)))
                {
                    quizOptions.Add(fileName);
                }
            }
            if (quizOptions.Count == 0)
            {
                Debug.LogWarning("No quiz files found in the specified directory.");
            }
            else
            {
                Debug.Log("Quiz files found: " + string.Join(", ", quizOptions));
            }
            categoryDropdown.AddOptions(quizOptions);
        }
        else
        {
            Debug.LogError("Directory not found: " + filePath);
        }

    }
    public void setQuiz(int quiz)
    {
        Debug.Log("Selected quiz index: " + quiz);
        Debug.Log("Selected quiz name: " + GetSelectedQuizName());
        GameData.selectedQuiz = GetSelectedQuizName();
    }
    public string GetSelectedQuizName()
    {
        if (categoryDropdown.options.Count > 0)
        {
            return categoryDropdown.options[categoryDropdown.value].text;
        }
        return "";
    }
    public void setDaily()
    {
        if (isDailyDone)
        {
            Debug.Log("Daily quiz already done today.");
            return;
        }
        isDailyDone = true;
        GameData.daily = true;
        Debug.Log("Daily quiz set to: " + GameData.daily);
    }
    public void changeVolume()
    {
        Debug.Log("Volume set to: " + volumeSlider.value);
        AudioListener.volume = volumeSlider.value;
        saveVolume();
    }
    void saveVolume()
    {
        PlayerPrefs.SetFloat("volume", volumeSlider.value);
        PlayerPrefs.Save();
    }
    void loadVolume()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("volume");
    }
    void loadDailyScore()
    {
        dailyScoreText.text = "Daily winstreak: " + PlayerPrefs.GetInt("dailyStreak", 0).ToString();
    }
    void ResetDailyStreak()
    {
        PlayerPrefs.SetInt("dailyStreak", 0);
        PlayerPrefs.Save();
    }
}
