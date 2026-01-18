using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AnswerScript : MonoBehaviour
{
    public bool isCorrect = false;
    public QuizManager quizManager;
    private static int value = 2;
    public int answerIndex;

    [Header("Effects")]
    public float delayBeforeNextQuestion = 2f;
    public GameObject celebrationParticles; // Assign particle system prefab
    
    public void answer()
    {
        quizManager.logAnswer(answerIndex);
        Debug.Log("Answer selected: " + answerIndex );

        if (isCorrect)
        {
            Color c;
            if (ColorUtility.TryParseHtmlString("#009900", out c))
            {
                Debug.Log("Parsed color: " + c);
            }else
            {
                c = Color.green; // fallback color
            }
            GetComponentInChildren<TextMeshProUGUI>().color = c;
            Debug.Log("Correct Answer!");
            StartCoroutine(waiter(0.5f));
        }
        else
        {
            if (value > 0)
            {
                value -= 1;
                Debug.Log("Wrong Answer! Value decreased to: " + value);
            }

            GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
            // Shake effect
            StartCoroutine(ShakeButton());
            Debug.Log("Wrong Answer!");
        }
    }
    public void resetValue()
    {
        value = 2;
    }
    IEnumerator ShakeButton()
    {
        Vector3 originalPosition = transform.position;
        float shakeDuration = 0.5f;
        float shakeAmount = 10f;

        float timer = 0;
        while (timer < shakeDuration)
        {
            transform.position = originalPosition + Random.insideUnitSphere * shakeAmount;
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }
    IEnumerator waiter(float seconds)
    {
        SetButtonsInteractable(false);
        yield return new WaitForSeconds(seconds);
        quizManager.correct(value);
    }
    // Helper method to toggle all answer buttons
    private void SetButtonsInteractable(bool state)
    {

        // Option B: Disable controls on the QuizManager to prevent clicking ANY answer 
        // (Recommended for quiz games so user doesn't click a different answer while one is shaking)
        if (quizManager != null && quizManager.options != null)
        {
            foreach (GameObject option in quizManager.options)
            {
                Button btn = option.GetComponent<Button>();
                if (btn != null) btn.enabled = state;
            }
        }
    }
}
