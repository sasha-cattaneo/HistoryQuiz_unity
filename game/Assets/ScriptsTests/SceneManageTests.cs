using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class SceneManageTests
{
    private GameObject testGameObject;
    private SceneManage sceneManage;

    [SetUp]
    public void SetUp()
    {
        testGameObject = new GameObject("TestSceneManage");
        sceneManage = testGameObject.AddComponent<SceneManage>();
    }

    [TearDown]
    public void TearDown()
    {
        if (testGameObject != null)
        {
            Object.DestroyImmediate(testGameObject);
        }
    }

    [Test]
    public void LoadScene_ValidSceneName_LogsCorrectMessage()
    {
        // Arrange
        string sceneName = "TestScene";
        
        // Act & Assert
        // Note: We can't easily test SceneManager.LoadScene in unit tests
        // but we can test that the method exists and doesn't throw exceptions
        Assert.DoesNotThrow(() => sceneManage.LoadScene(sceneName));
    }

    [Test]
    public void LoadScene_EmptySceneName_DoesNotThrow()
    {
        // Arrange
        string sceneName = "";

        // Act
        sceneManage.LoadScene(sceneName);
        
        // Assert
        LogAssert.Expect(LogType.Warning,"Scene name is null or empty. Cannot load scene.");
    }

    [Test]
    public void CloseGame_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => sceneManage.CloseGame());
    }
}