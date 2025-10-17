using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class HomeSceneLoader : MonoBehaviour
{

    public SceneManage sceneManager;
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame 
        || Mouse.current.leftButton.wasReleasedThisFrame 
        || Mouse.current.rightButton.wasReleasedThisFrame)
        {
            sceneManager.LoadScene("HomepageScene");
        }
    }
    void Start()
    {
        if (!PlayerPrefs.HasKey("volume"))
        {
            Debug.Log("Volume key not found. Setting default volume to 1.");
            PlayerPrefs.SetFloat("volume", 1);
            PlayerPrefs.Save();
            AudioListener.volume = 1;
        }
        else
        {
            Debug.Log("Volume key found. Setting volume to: " + PlayerPrefs.GetFloat("volume"));
            AudioListener.volume = PlayerPrefs.GetFloat("volume");
        }
    }
}
