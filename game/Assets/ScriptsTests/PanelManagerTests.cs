using NUnit.Framework;
using UnityEngine;

public class PanelManagerTests
{
    private GameObject testGameObject;
    private PanelManager panelManager;
    private GameObject[] testPanels;

    [SetUp]
    public void SetUp()
    {
        testGameObject = new GameObject("TestPanelManager");
        panelManager = testGameObject.AddComponent<PanelManager>();
        
        // Create test panels
        testPanels = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            testPanels[i] = new GameObject($"Panel{i}");
            testPanels[i].SetActive(false);
        }
        
        panelManager.panels = testPanels;
    }

    [TearDown]
    public void TearDown()
    {
        if (testGameObject != null)
        {
            Object.DestroyImmediate(testGameObject);
        }
        
        foreach (var panel in testPanels)
        {
            if (panel != null)
            {
                Object.DestroyImmediate(panel);
            }
        }
    }

    [Test]
    public void TogglePanel_ValidPanelName_ActivatesCorrectPanel()
    {
        // Arrange
        string panelName = "Panel0";
        
        // Act
        panelManager.TogglePanel(panelName);
        
        // Assert
        Assert.IsTrue(testPanels[0].activeSelf);
        Assert.IsFalse(testPanels[1].activeSelf);
        Assert.IsFalse(testPanels[2].activeSelf);
    }

    [Test]
    public void TogglePanel_ActivePanel_DeactivatesPanel()
    {
        // Arrange
        string panelName = "Panel0";
        testPanels[0].SetActive(true);
        
        // Act
        panelManager.TogglePanel(panelName);
        
        // Assert
        Assert.IsFalse(testPanels[0].activeSelf);
    }

    [Test]
    public void TogglePanel_InvalidPanelName_NoChanges()
    {
        // Arrange
        string panelName = "NonExistentPanel";
        bool[] originalStates = new bool[testPanels.Length];
        for (int i = 0; i < testPanels.Length; i++)
        {
            originalStates[i] = testPanels[i].activeSelf;
        }
        
        // Act
        panelManager.TogglePanel(panelName);
        
        // Assert
        for (int i = 0; i < testPanels.Length; i++)
        {
            Assert.AreEqual(originalStates[i], testPanels[i].activeSelf);
        }
    }
}