using UnityEngine;

public class SessionPair
{
    public Sprite face, faceAnswer;
    public string name, userAnswer;
    public AudioClip nameAudio;
    public int attempts;
    public bool correctlyAnswered, answeredByFace;
    public float responseTime, confidenceRating;

    public SessionPair(Sprite face, string name, AudioClip audio)
    {
        this.face = face;
        this.name = name;
        this.nameAudio = audio;
    }
    public void EvaluateAnswers(bool correct, float answerStartTime, string option, Sprite answeredSprite)
    {
        if (correct)
        {
            correctlyAnswered = true;
            userAnswer = null;
            faceAnswer = null; 
        }
        else
        {
            correctlyAnswered = false;
            userAnswer = option;
            faceAnswer = answeredSprite;
        }

        attempts++;
        responseTime = responseTime + (Time.time - answerStartTime);
        Debug.Log("User took " + responseTime + " seconds to answer.");

        // IMPLEMENT ATTEMPTS INTO CONFIDENCE
        float confidence = 50f; // Base confidence

        // Correctness impact
        if (correctlyAnswered)
            confidence += 30f;
        else
            confidence -= 20f;

        // Response time impact
        if (responseTime < 2f)
            confidence += 20f;
        else if (responseTime > 5f)
            confidence -= 10f;

        // Attempts impact (more attempts = lower confidence)
        if (attempts == 1)
            confidence += 10f; // perfect
        else if (attempts == 2)
            confidence += 5f;
        else if (attempts >= 3)
            confidence -= 10f;

        confidenceRating = Mathf.Clamp(confidence, 0f, 100f);

    }

}
