using System.Collections;
using UnityEngine;
using TMPro;

public class AnswerScript : MonoBehaviour
{
    public bool isCorrect = false;
    public QuizManager quizManager;
    private static int value = 2;

    [Header("Effects")]
    public float delayBeforeNextQuestion = 2f;
    public GameObject celebrationParticles; // Assign particle system prefab
    
    public void answer()
    {
        if (isCorrect)
        {
            GetComponentInChildren<TextMeshProUGUI>().color = Color.green;
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
        yield return new WaitForSeconds(seconds);
        quizManager.correct(value);
    }
}
