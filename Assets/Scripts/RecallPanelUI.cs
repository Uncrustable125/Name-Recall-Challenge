using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecallPanelUI : MonoBehaviour
{
    [Header("Containers")]
    public GameObject optionButtonsContainer;
    public GameObject typeInContainer;

    [Header("Multiple Choice")]
    public GameObject optionButtonPrefab;
    public Transform optionButtonsParent;

    [Header("Type-In")]
    public TMP_InputField inputField;
    public Button submitButton;

    public Action<string> onAnswerSubmitted;

    public void ShowMultipleChoice(List<string> options)
    {
        optionButtonsContainer.SetActive(true);
        typeInContainer.SetActive(false);

        // Clear old
        foreach (Transform child in optionButtonsParent)
            Destroy(child.gameObject);

        foreach (string option in options)
        {
            GameObject btnObj = Instantiate(optionButtonPrefab, optionButtonsParent);
            btnObj.GetComponentInChildren<TMP_Text>().text = option;

            btnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                onAnswerSubmitted?.Invoke(option);
            });
        }
    }

    public void ShowTypeIn()
    {
        optionButtonsContainer.SetActive(false);
        typeInContainer.SetActive(true);

        inputField.text = "";
        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(() =>
        {
            string answer = inputField.text.Trim();
            if (!string.IsNullOrEmpty(answer))
            {
                onAnswerSubmitted?.Invoke(answer);
            }
        });
    }

    public void Hide()
    {
        optionButtonsContainer.SetActive(false);
        typeInContainer.SetActive(false);
    }
}
