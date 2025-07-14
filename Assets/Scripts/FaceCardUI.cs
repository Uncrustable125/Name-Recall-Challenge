using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FaceCardUI : MonoBehaviour
{
    public Image faceImage, faceAnswerImage;
    public TextMeshProUGUI nameText, feedback1Text, feedback2Text, reactionText, confidenceText, attemptsText;


    public void SetData(Sprite face, Sprite faceAnswer, string name, string feedback1, string feedback2, int attempts, float reaction, float confidence)
    {
        if (face != null)
        {
            faceImage.sprite = face;
        }
        else
        {
            Color color = faceImage.color;
            color.a = 0f;
            faceImage.color = color;
        }
        if (faceAnswer != null)
        {
            faceAnswerImage.GetComponent<LayoutElement>().preferredHeight = 600f;
            faceAnswerImage.sprite = faceAnswer;
        }
        else
        {
            faceAnswerImage.GetComponent<LayoutElement>().preferredHeight = 0f;
        }
        if(reaction >= 0)
        {
            attemptsText.text = "Attemps - " + attempts.ToString();
            reactionText.text = "Reaction Time - " + reaction.ToString("F3");
            confidenceText.text = "Confidence Rating - " + confidence.ToString("F2");
        }
        else
        {
            attemptsText.text = "";
            reactionText.text = "";
            confidenceText.text = "";
        }
        nameText.text = name;
        feedback1Text.text = feedback1;
        feedback2Text.text = feedback2;
    }


}
