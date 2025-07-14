using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TypedAnswerUI : MonoBehaviour
{
    public TMP_InputField inputField;

    private Action<string> onSubmit;

    public void Initialize(Action<string> callback)
    {
        onSubmit = callback;
        inputField.onSubmit.RemoveAllListeners();
        inputField.onSubmit.AddListener(Submit);
        inputField.ActivateInputField(); // Focus immediately
    }

    private void Submit(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            onSubmit?.Invoke(value);
        }
    }
}
