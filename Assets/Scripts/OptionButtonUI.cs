using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// CAN GET RID OF THIS
/// </summary>
public class OptionButtonUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Button button;

    private string optionValue;
    private System.Action<string> onSelectedCallback;

    public void SetData(string name, System.Action<string> callback)
    {
        optionValue = name;
        nameText.text = name;
        onSelectedCallback = callback;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onSelectedCallback?.Invoke(optionValue));
    }
}
