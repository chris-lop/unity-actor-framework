using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    public VisualElement ui;
    public Button playButton;
    public Button continueButton;
    public Button exitButton;

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    void OnEnable()
    {
        playButton = ui.Q<Button>("PlayButton");
        playButton.clicked += OnPlayButtonClicked;

        continueButton = ui.Q<Button>("ContinueButton");
        continueButton.clicked += OnContinueButtonClicked;

        exitButton = ui.Q<Button>("ExitButton");
        exitButton.clicked += OnExitButtonClicked;
    }

    private void OnPlayButtonClicked()
    {
        gameObject.SetActive(false);
    }
    private void OnContinueButtonClicked()
    {
        Debug.Log("Continue!");
    }
    private void OnExitButtonClicked()
    {
        Application.Quit();
        EditorApplication.isPlaying = false;
    }

}
