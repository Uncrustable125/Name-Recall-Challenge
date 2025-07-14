using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Grids / Tranforms")]
    public GameObject responseGrid;
    public GameObject faceResponseGrid;
    public GameObject faceGrid;
    public Transform faceParent;
    public Transform faceParentAnswering;


    [Header("Buttons")]
    public GameObject answerTypeButton;
    public GameObject audioSelButton;
    public GameObject nameOrFaceButton;
    public GameObject StartButton;
    public GameObject feedbackScrollButton;
    public GameObject reStartButton;
  
    [Header("Prefabs")]
    public GameObject facePrefab;
    public GameObject buttonPrefab;
    public GameObject typedAnswerPrefab;


    [Header("UI Elements")]
    public GameObject Slider;
    public GameObject mathUI;
    public Slider exposureTimeSlider;
    public Slider faceCountSlider;
    public Slider chancesSlider;
    public TMPro.TextMeshProUGUI mathQuestionText;
    public TMPro.TextMeshProUGUI feedbackText;
    public TMPro.TextMeshProUGUI faceCountLabel;
    public TMPro.TextMeshProUGUI exposureTimeLabel;
    public TMPro.TextMeshProUGUI chancesTimeLabel;




    int selectedFaceCount, exposureTime, nameOrFace, chances, chancesLeft;
    float answerStartTime;
    bool NextFace, gameStart, audioEnabled, multChoiceEnabled;

    public enum Phase { ENCODE_START, DISTRACTOR_ON, RECALL_START, FEEDBACK }
    private Phase currentPhase;
    private GameObject faceCard;
    private List<SessionPair> currentPairs;
    private List<Button> buttonsList = new List<Button>();
    private Button toggleAudioButton, toggleMultChoiceButton, toggleNameOrFaceButton, startButton;
    private TMPro.TextMeshProUGUI audioButtonText, multButtonText, NameOrFaceText, startButtonText;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        initializeMenu();
        StartCoroutine(DelayThenStart());
    }

    void ToggleMultChoice()
    {
        multChoiceEnabled = !multChoiceEnabled;
        multButtonText.text = multChoiceEnabled ? "Multiple Choice: ON" : "Text Response: On";
    }
    void ToggleAudio()
    {
        audioEnabled = !audioEnabled;
        audioButtonText.text = audioEnabled ? "Audio: ON" : "Audio: OFF";
    }

    void ToggleNameFace()
    {
        if (nameOrFace == 0)//guess name
        {
            nameOrFace = 1;
            NameOrFaceText.text = "Guess the Name";
        }
        else if (nameOrFace == 1)//guess face
        {
            nameOrFace = 2;
            NameOrFaceText.text = "Guess the Face";
        }
        else//both
        {
            nameOrFace = 0;
            NameOrFaceText.text = "Both";
        }
    }
    void initializeMenu()
    {
        // Sldiers
        faceCountSlider.onValueChanged.AddListener(OnFaceCountChanged);
        OnFaceCountChanged(faceCountSlider.value); 
        exposureTimeSlider.onValueChanged.AddListener(OnExposureChanged);
        OnExposureChanged(exposureTimeSlider.value); 
        chancesSlider.onValueChanged.AddListener(OnChancesChanged);
        OnChancesChanged(chancesSlider.value); 

        //Buttons
        SetupAudioButton();
        SetupChoiceButton();
        SetupStartButton();
        SetupNameFaceButtonButton();
    }
    IEnumerator DelayThenStart()
    {
        yield return StartCoroutine(Menu());
        currentPairs = GenerateSession(selectedFaceCount); 
        NextFace = false;
        yield return StartCoroutine(GameLoop());
    }
    IEnumerator Menu()
    {
        gameStart = false;
        yield return new WaitUntil(() => gameStart);
        SetMenuStatus(false);
    }
    void OnFaceCountChanged(float value)
    {
        selectedFaceCount = Mathf.RoundToInt(value);
        faceCountLabel.text = $"{selectedFaceCount} Faces";
    }
    void OnExposureChanged(float value)
    {
        exposureTime = Mathf.RoundToInt(value);
        exposureTimeLabel.text = $"{exposureTime} Seconds";
    }

    void OnChancesChanged(float value)
    {
        chances = Mathf.RoundToInt(value);
        chancesTimeLabel.text = $"{chances} Chances";
    }
    

    void SetMenuStatus(bool activity)
    {
        answerTypeButton.gameObject.SetActive(activity);
        nameOrFaceButton.gameObject.SetActive(activity);
        audioSelButton.gameObject.SetActive(activity);
        startButton.gameObject.SetActive(activity);
        Slider.gameObject.SetActive(activity);
        exposureTimeSlider.gameObject.SetActive(activity);
        chancesSlider.gameObject.SetActive(activity);

    }

    void SetupAudioButton()
    {
        audioEnabled = true;
        toggleAudioButton = audioSelButton.GetComponent<Button>();
        audioButtonText = audioSelButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        toggleAudioButton.onClick.AddListener(ToggleAudio);
        ToggleAudio();
    }
    void SetupChoiceButton()
    {
        multChoiceEnabled = false;
        toggleMultChoiceButton = answerTypeButton.GetComponent<Button>();
        multButtonText = answerTypeButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        toggleMultChoiceButton.onClick.AddListener(ToggleMultChoice);
        ToggleMultChoice();

        
    }

    void SetupNameFaceButtonButton()
    {
        nameOrFace = 0; //1 guess name, 2 faces, 0 both
        toggleNameOrFaceButton = nameOrFaceButton.GetComponent<Button>();
        NameOrFaceText = nameOrFaceButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        toggleNameOrFaceButton.onClick.AddListener(ToggleNameFace);
        ToggleNameFace();

        
    }

    void SetupStartButton()
    {
        startButton = StartButton.GetComponent<Button>();
        startButtonText = StartButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        startButton.onClick.AddListener(() =>
        {
            gameStart = !gameStart;
            Debug.Log("Toggled: " + gameStart);
        });
    }
    IEnumerator GameLoop()
    {
        yield return EncodingPhase();
        yield return DistractorPhase();
        yield return RecallPhase();
        yield return FeedbackPhase();
    }

    IEnumerator EncodingPhase()
    {
        currentPhase = Phase.ENCODE_START;
        Debug.Log("Encoding Phase");

        GameObject currentCard = SpawnFacePair(0, 0);
        // Play audio if present
        if (audioEnabled)
            audioSource.PlayOneShot(currentPairs[0].nameAudio);
        yield return new WaitForSeconds(exposureTime);

        for (int i = 1; i < currentPairs.Count; i++)
        {
            Destroy(currentCard);
            currentCard = SpawnFacePair(i, 0);
            if (audioEnabled)
                audioSource.PlayOneShot(currentPairs[i].nameAudio);
            yield return new WaitForSeconds(exposureTime);
        }

        Destroy(currentCard);
    }

    GameObject SpawnFacePair(int i, int operation)
    {
        if (operation == 0)//Encoding
        {
            GameObject card = Instantiate(facePrefab, faceParent);
            FaceCardUI ui = card.GetComponent<FaceCardUI>();
            ui.SetData(currentPairs[i].face,null, currentPairs[i].name, "", "", -1, -1, -1);

            RectTransform rt = card.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;

            if (audioEnabled)
            {
                Button btn = card.GetComponentInChildren<Button>();
                AudioSource source = card.GetComponentInChildren<AudioSource>();
                AudioClip clip = currentPairs[i].nameAudio;

                btn.onClick.AddListener(() => source.PlayOneShot(clip));

            }
            else
            {
                Image audioIcon = card.transform.Find("Audio")?.GetComponent<Image>();
                audioIcon.gameObject.SetActive(false);
            }

            return card;
        }
        else if (operation == 1)//Recall Name
        {
            GameObject card = Instantiate(facePrefab, faceParentAnswering);
            FaceCardUI ui = card.GetComponent<FaceCardUI>();
            ui.SetData(currentPairs[i].face,null, "", "", "", -1, -1, -1 );

            RectTransform rt = card.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            Image audioIcon = card.transform.Find("Audio")?.GetComponent<Image>();
            audioIcon.gameObject.SetActive(false);
            return card;
        }
        else if (operation == 2)//Recall Face
        {
            GameObject card = Instantiate(facePrefab, faceParentAnswering);
            FaceCardUI ui = card.GetComponent<FaceCardUI>();
            ui.SetData(null, null, currentPairs[i].name, "", "", -1, -1, -1);

            RectTransform rt = card.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            if (audioEnabled)
            {
                Button btn = card.GetComponentInChildren<Button>();
                AudioSource source = card.GetComponentInChildren<AudioSource>();
                AudioClip clip = currentPairs[i].nameAudio;

                btn.onClick.AddListener(() => source.PlayOneShot(clip));

            }
            else
            {
                Image audioIcon = card.transform.Find("Audio")?.GetComponent<Image>();
                audioIcon.gameObject.SetActive(false);
            }
            return card;
        }
        else if (operation == 3)//Feedback
        {
            GameObject card = Instantiate(facePrefab, faceGrid.transform);
            FaceCardUI ui = card.GetComponent<FaceCardUI>();

            if (currentPairs[i].correctlyAnswered)
            {
                ui.SetData(currentPairs[i].face, currentPairs[i].faceAnswer, currentPairs[i].name, "Correct!", ""
                    + currentPairs[i].userAnswer, currentPairs[i].attempts, currentPairs[i].responseTime, currentPairs[i].confidenceRating);
            }
            else
            {
                ui.SetData(currentPairs[i].face, currentPairs[i].faceAnswer, currentPairs[i].name, "Incorrect", "You answered "
                    + currentPairs[i].userAnswer, currentPairs[i].attempts, currentPairs[i].responseTime, currentPairs[i].confidenceRating);
            }



            RectTransform rt = card.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            card.transform.localScale = new Vector3(0.55f, 0.5f, 1f);
            Image audioIcon = card.transform.Find("Audio")?.GetComponent<Image>();
            audioIcon.gameObject.SetActive(false);
            return card;
        }
        return null;
    }


    IEnumerator DistractorPhase()
    {
        currentPhase = Phase.DISTRACTOR_ON;
        mathUI.SetActive(true);
        DeleteButtons(responseGrid); // Clear previous buttons

        int a = UnityEngine.Random.Range(1, 10);
        int b = UnityEngine.Random.Range(1, 10);
        int c = UnityEngine.Random.Range(1, 10);
        int correct = a + b * c;
        mathQuestionText.text = $"{a} + {b} * {c} = ?";

        bool answered = false;

        if (!multChoiceEnabled) // TEXT INPUT VERSION
        {
            GameObject typedUI = Instantiate(typedAnswerPrefab, responseGrid.transform);
            TypedAnswerUI typedComponent = typedUI.GetComponent<TypedAnswerUI>();

            typedComponent.Initialize((typedAnswer) =>
            {
                if (int.TryParse(typedAnswer.Trim(), out int typedResult) && typedResult == correct)
                {
                    answered = true;

                }
                else
                {
                    feedbackText.text = "Incorrect, try again";
                }
            });
        }
        else // MULTIPLE-CHOICE BUTTONS VERSION
        {
            List<int> options = new List<int> { correct };
            while (options.Count < 3)
            {
                int wrong = correct + UnityEngine.Random.Range(-3, 4);
                if (wrong != correct && !options.Contains(wrong) && wrong > 0)
                    options.Add(wrong);
            }

            // Shuffle
            for (int i = 0; i < options.Count; i++)
            {
                int rand = UnityEngine.Random.Range(i, options.Count);
                (options[i], options[rand]) = (options[rand], options[i]);
            }

            foreach (int option in options)
            {
                GameObject btnObj = Instantiate(buttonPrefab, responseGrid.transform);
                TMPro.TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                btnText.text = option.ToString();

                var btn = btnObj.GetComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    if (option == correct)
                        answered = true;
                    else
                        feedbackText.text = "Incorrect";
                });
            }
        }

        yield return new WaitUntil(() => answered);
        feedbackText.text = "";
        mathUI.SetActive(false);
        DeleteButtons(responseGrid);
    }

    IEnumerator RecallPhase()
    {
        currentPhase = Phase.RECALL_START;
        Debug.Log("Recall Phase");
        currentPairs = ShuffleList(currentPairs);
        //Goes through each pair and allows user to answer them
        //Split logic for faces vs names
        for (int i = 0; i < currentPairs.Count; i++)
        {
            chancesLeft = chances;
            var correctPair = currentPairs[i];
            bool recallFace;
            if (nameOrFace == 1)//guess the name
            {
                recallFace = false;
            }
            else if (nameOrFace == 2)//faces
            {
                recallFace = true;
            }
            else //both
            {
                recallFace = UnityEngine.Random.value > 0.5f;
            }
            if (!recallFace)//answered by text/choice
            {
                GameObject currentCard = SpawnFacePair(i, 1);
                currentPairs[i].answeredByFace = false;
                //Guess the Name
                if (multChoiceEnabled)
                {
                    bool isFemale = currentPairs[i].face.name.Contains("Female");
                    List<string> options = GenerateRandomNameOptions(correctPair.name, isFemale);
                    foreach (string option in options)
                    {
                        SpawnAnswerButtons(responseGrid, option, null, correctPair);
                    }

                }
                else//Text
                {
                    GameObject typedUI = Instantiate(typedAnswerPrefab, responseGrid.transform);
                    TypedAnswerUI typedComponent = typedUI.GetComponent<TypedAnswerUI>();

                    typedComponent.Initialize((typedAnswer) =>
                    {
                        chancesLeft--;
                        bool isCorrect = typedAnswer.Trim().Equals(correctPair.name, StringComparison.OrdinalIgnoreCase);
                        if (!isCorrect)
                        {
                            feedbackText.text = "Incorrect, try again";
                            if (chancesLeft <= 0)
                            {
                                feedbackText.text = "";
                                NextFace = true;
                            }

                        }
                        else
                        {
                            feedbackText.text = "";
                            NextFace = true;
                        }
                        correctPair.EvaluateAnswers(isCorrect, answerStartTime, typedAnswer, null);
                        
                    });
  
                }
                answerStartTime = Time.time;
                yield return new WaitUntil(() => NextFace);
                NextFace = false;
                DeleteButtons(responseGrid);
                Destroy(currentCard);
            }
            else//answered by face
            {
                GameObject currentCard = SpawnFacePair(i, 2);
                currentPairs[i].answeredByFace = true;
                List<Sprite> options = GenerateRandomFaceOptions(correctPair.face);
                    foreach (Sprite option in options)
                    {
                        SpawnAnswerButtons(faceResponseGrid, null, option, correctPair);
                    }

                    answerStartTime = Time.time;
                    yield return new WaitUntil(() => NextFace);
                    NextFace = false;
                    DeleteButtons(faceResponseGrid);
                    Destroy(currentCard);
                
            }
        }
    }
    List<T> ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randIndex = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[randIndex]) = (list[randIndex], list[i]);
        }
        return list;
    }

    void SpawnAnswerButtons(GameObject grid, string buttonText, Sprite faceSprite, SessionPair currentPair)
    {

        GameObject btnObj = Instantiate(buttonPrefab, grid.transform);
        if(buttonText != null){
            TMPro.TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            btnText.text = buttonText;
            Button btn = btnObj.GetComponent<Button>();
            buttonsList.Add(btn);
            btn.onClick.AddListener(() =>
            {
                HighlightOnlyThis(btn);
                chancesLeft--;
                bool isCorrect = (buttonText == currentPair.name);
                if (!isCorrect)
                {
                    feedbackText.text = "Incorrect, try again";
                    if (chancesLeft <= 0)
                    {
                        feedbackText.text = "";
                        NextFace = true;
                    }
                    
                }
                else
                {
                    feedbackText.text = "";
                    NextFace = true;
                }
                Debug.Log("Chances Left: " + chancesLeft);
                currentPair.EvaluateAnswers(isCorrect, answerStartTime, buttonText, null);
            });
        }
        else
        {
            TMPro.TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            btnText.text = "";
            Image btnImage = btnObj.GetComponentInChildren<Image>();
            btnImage.sprite = faceSprite;
            btnImage.color = Color.white;
            Button btn = btnObj.GetComponent<Button>();
            buttonsList.Add(btn);
            btn.onClick.AddListener(() =>
            {
                chancesLeft--;
                bool isCorrect = (btnImage.sprite == currentPair.face);
                if (!isCorrect)
                {
                    feedbackText.text = "Incorrect, try again";
                    if (chancesLeft <= 0)
                    {
                        feedbackText.text = "";
                        NextFace = true;
                    }

                }
                else
                {
                    feedbackText.text = "";
                    NextFace = true;
                }
                Debug.Log("Chances Left: " + chancesLeft);

                currentPair.EvaluateAnswers(isCorrect, answerStartTime, "", btnImage.sprite);
            });
        }



    }
    void DeleteButtons(GameObject grid)
    {
        buttonsList.Clear();
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);
    }

    void HighlightOnlyThis(Button selectedBtn)
    {
        foreach (var btn in buttonsList)
        {
            var colors = btn.colors;
            colors.normalColor = (btn == selectedBtn) ? Color.yellow : Color.white;
            btn.colors = colors;
        }
    }

    IEnumerator FeedbackPhase()
    {
        bool reStartGame = false;
        int pageNum = 0;
        int totalPageNum = 0;

        currentPhase = Phase.FEEDBACK;
        feedbackText.text = "";

        if(currentPairs.Count < 5)
        {
            totalPageNum = 0;
        }
        else if (currentPairs.Count < 9)
        {
            totalPageNum = 1;
        }
        else
        {
            totalPageNum = 2;
        }

        //Page Scroll and Restart buttons
        if (totalPageNum != 0)
        {
            feedbackScrollButton.SetActive(true);
            Button btn = feedbackScrollButton.GetComponent<Button>();
            buttonsList.Add(btn);
            btn.onClick.AddListener(() =>
            {
                if (pageNum < totalPageNum)
                {
                    pageNum++;
                }
                else
                {
                    pageNum = 0;
                }
                ShowFeedbackPage(pageNum);
            });
        }

        reStartButton.SetActive(true);
        Button btn2 = reStartButton.GetComponent<Button>();
        buttonsList.Add(btn2);
        btn2.onClick.AddListener(() =>
        {
            reStartGame = true;
        });

        ShowFeedbackPage(0);

        yield return new WaitUntil(() => reStartGame);
        feedbackScrollButton.SetActive(false);
        reStartButton.SetActive(false);
        foreach (Transform child in faceGrid.transform)
        {
            Destroy(child.gameObject);
        }
        SetMenuStatus(true);
        StartCoroutine(DelayThenStart());
    }

    void ShowFeedbackPage(int page)
    {
        // Clear existing faces
        foreach (Transform child in faceGrid.transform)
        {
            Destroy(child.gameObject);
        }

        int itemsPerPage = 4;
        int startIndex = page * itemsPerPage;
        int remaining = currentPairs.Count - startIndex;
        int countThisPage = Mathf.Min(itemsPerPage, remaining);

        // Spawn only the pairs for this page
        for (int i = 0; i < countThisPage; i++)
        {
            SpawnFacePair(startIndex + i, 3);
        }
    }
    List<SessionPair> GenerateSession(int count)
    {
        List<SessionPair> session = new List<SessionPair>();
        Sprite[] allSprites = Resources.LoadAll<Sprite>("Sprites/Faces");
        List<Sprite> availableSprites = new List<Sprite>(allSprites);

        // Track available names
        List<string> maleNames = new List<string> { "James", "Liam", "Noah", "Elijah", "Logan", "Lucas", "Mason", "Ethan" };
        List<string> femaleNames = new List<string> { "Emma", "Olivia", "Ava", "Sophia", "Mia", "Isabella", "Charlotte", "Amelia" };

        for (int i = 0; i < count; i++)
        {
            if (availableSprites.Count == 0) break;

            int spriteIndex = UnityEngine.Random.Range(0, availableSprites.Count);
            Sprite faceSprite = availableSprites[spriteIndex];
            availableSprites.RemoveAt(spriteIndex);

            string spriteName = faceSprite.name;
            bool isFemale = spriteName.Contains("Female");

            string name;
            if (isFemale && femaleNames.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, femaleNames.Count);
                name = femaleNames[index];
                femaleNames.RemoveAt(index);
            }
            else if (!isFemale && maleNames.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, maleNames.Count);
                name = maleNames[index];
                maleNames.RemoveAt(index);
            }
            else
            {
                name = "Unknown"; // fallback in case of exhaustion
            }

            AudioClip nameClip = Resources.Load<AudioClip>($"Audio/{name}");
            session.Add(new SessionPair(faceSprite, name, nameClip));
        }

        return session;
    }


    string GenerateRandomMaleName()//If want more names, I will need to generate the audio to acompany them
    {
        string[] maleNames = { "James", "Liam", "Noah", "Elijah", "Logan", "Lucas", "Mason", "Ethan" };
        return maleNames[UnityEngine.Random.Range(0, maleNames.Length)];
    }

    string GenerateRandomFemaleName()//If want more names, I will need to generate the audio to acompany them
    {
        string[] femaleNames = { "Emma", "Olivia", "Ava", "Sophia", "Mia", "Isabella", "Charlotte", "Amelia" }; 
        return femaleNames[UnityEngine.Random.Range(0, femaleNames.Length)];
    }

    List<string> GenerateRandomNameOptions(string correctName, bool isFemale, int totalOptions = 4)
    {
        List<string> options = new List<string> { correctName };

        while (options.Count < totalOptions)
        {
            string rand;
            
            if (isFemale)
            {
                rand = GenerateRandomFemaleName();
            }
            else
            {
                rand = GenerateRandomMaleName();
            }
                
            if (!options.Contains(rand))
                options.Add(rand);
        }

        for (int i = 0; i < options.Count; i++)
        {
            int randIndex = UnityEngine.Random.Range(i, options.Count);
            (options[i], options[randIndex]) = (options[randIndex], options[i]);
        }

        return options;
    }
    List<Sprite> GenerateRandomFaceOptions(Sprite correctFace, int totalOptions = 4)
    {
        bool isFemale = correctFace.name.Contains("Female");

        List<Sprite> options = new List<Sprite> { correctFace };
        Sprite[] allSprites = Resources.LoadAll<Sprite>("Sprites/Faces");
        List<Sprite> matchingSprites = new List<Sprite>(allSprites);

        matchingSprites = matchingSprites.FindAll(sprite =>
          isFemale ? sprite.name.Contains("Female") : sprite.name.Contains("Male"));
        while (options.Count < totalOptions)//Gender the names!
        {
            Sprite selectedSprite = null;
            int spriteIndex = UnityEngine.Random.Range(0, matchingSprites.Count);
            selectedSprite = matchingSprites[spriteIndex];
            matchingSprites.Remove(selectedSprite);
            if (!options.Contains(selectedSprite))
                options.Add(selectedSprite);
        }

        for (int i = 0; i < options.Count; i++)//shuffle options
        {
            int randIndex = UnityEngine.Random.Range(i, options.Count);
            (options[i], options[randIndex]) = (options[randIndex], options[i]);
        }

        return options;
    }

    public string GenerateRandomName()
    {
        if (UnityEngine.Random.value > 0.5f)
        {
            return GenerateRandomMaleName();
        }
        else
        {
            return GenerateRandomFemaleName();
        }
    }

}
