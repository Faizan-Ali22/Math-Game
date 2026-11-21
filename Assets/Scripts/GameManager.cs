using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct GradeTier
{
    public string gradeName;
    public int minScore;
    public Sprite gradeSprite;
}

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI option1Text;
    [SerializeField] private TextMeshProUGUI option2Text;
    [SerializeField] private TextMeshProUGUI option3Text;
    [SerializeField] private TextMeshProUGUI option4Text;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject comboObject;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private GameObject starsUI;
    [SerializeField] private GameObject[] modeStars;

    [Header("Popups")]
    [SerializeField] private GameObject timesUpPopup;
    [SerializeField] private Button timesUpYesButton;
    [SerializeField] private Button timesUpNoButton;
    
    [Header("Score Card")]
    [SerializeField] private GameObject scoreCardPopup;
    [SerializeField] private TextMeshProUGUI scoreCardScoreText;
    [SerializeField] private TextMeshProUGUI scoreCardLevelText;
    [SerializeField] private Image scoreCardGradeImage;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button replayButton;

    [Header("Grades")]
    [SerializeField] private GradeTier[] gradeTiers;

    [Header("PowerUp - Time Freeze")]
    [SerializeField] private RectTransform powerUpRect;
    [SerializeField] private Button powerUpButton;
    [SerializeField] private float powerUpSlideDuration = 3f;
    [SerializeField] private float freezeDuration = 5f;
    [SerializeField] private float powerUpStartY = 104.5f;
    [SerializeField] private float powerUpEndY = -926f;

    [Header("PowerUp - 50/50")]
    [SerializeField] private RectTransform fiftyFiftyRect;
    [SerializeField] private Button fiftyFiftyButton;

    [Header("Buttons")]
    [SerializeField] private Button option1Button;
    [SerializeField] private Button option2Button;
    [SerializeField] private Button option3Button;
    [SerializeField] private Button option4Button;

    [Header("Audio")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;

    [Header("Transition")]
    [SerializeField] private CanvasGroup quizCanvasGroup;
    [SerializeField] private float fadeDuration = 0.1f;

    [Header("Settings")]
    [SerializeField] private float timePerQuestion = 5f;
    [SerializeField] private float timeBonus = 0.2f;
    [SerializeField] private float timePenalty = 0.2f;
    [SerializeField] private int questionsPerLevel = 15;
    [SerializeField] private int maxLevels = 12;
    private int totalQuestions => questionsPerLevel * maxLevels;

    private int currentQuestion = 0;
    private int correctAnswer;
    private float currentTime;
    private bool isGameRunning = false;
    private int score = 0;
    private Color defaultColor;
    private List<string> askedQuestions = new List<string>();
    private int streak = 0;
    private bool isTimeFrozen = false;
    private Coroutine powerUpSlideCoroutine;
    private Coroutine fiftyFiftySlideCoroutine;
    
    private int questionsAnswered = 0;
    private int maxStreakThisSession = 0;

    void Start()
    {
        defaultColor = questionText.color;
        questionText.gameObject.SetActive(false);
        option1Button.gameObject.SetActive(false);
        option2Button.gameObject.SetActive(false);
        option3Button.gameObject.SetActive(false);
        if (option4Button != null) option4Button.gameObject.SetActive(false);
        timerSlider.transform.parent.gameObject.SetActive(false);
        levelText.gameObject.SetActive(false);
        comboObject.SetActive(false);
        powerUpRect.gameObject.SetActive(false);
        if (fiftyFiftyRect != null) fiftyFiftyRect.gameObject.SetActive(false);
        if (starsUI != null) starsUI.SetActive(false);

        if (timesUpPopup != null) timesUpPopup.SetActive(false);
        if (scoreCardPopup != null) scoreCardPopup.SetActive(false);

        option1Button.onClick.AddListener(() => CheckAnswer(1));
        option2Button.onClick.AddListener(() => CheckAnswer(2));
        option3Button.onClick.AddListener(() => CheckAnswer(3));
        if (option4Button != null) option4Button.onClick.AddListener(() => CheckAnswer(4));
        powerUpButton.onClick.AddListener(ActivateTimeFreeze);
        if (fiftyFiftyButton != null) fiftyFiftyButton.onClick.AddListener(ActivateFiftyFifty);

        if (timesUpNoButton != null) timesUpNoButton.onClick.AddListener(OnTimesUpNoClicked);
        if (timesUpYesButton != null) timesUpYesButton.onClick.AddListener(OnTimesUpYesClicked);

        if (homeButton != null) homeButton.onClick.AddListener(GoToMainMenu);
        if (replayButton != null) replayButton.onClick.AddListener(ReplayGame);

        SetupAudio();

        if (countdownText != null)
        {
            StartCoroutine(CountdownRoutine());
        }
    }

    void SetupAudio()
    {
        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
        }

        if (musicAudioSource == null)
        {
            musicAudioSource = gameObject.AddComponent<AudioSource>();
        }

        if (bgmClip != null)
        {
            musicAudioSource.clip = bgmClip;
            musicAudioSource.loop = true;
            bool isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
            musicAudioSource.mute = !isMusicOn;
            musicAudioSource.Play();
        }
    }

    void PlayCorrectSound()
    {
        bool isSoundOn = PlayerPrefs.GetInt("_Mute", 0) == 0;
        if (isSoundOn && sfxAudioSource != null && correctSound != null)
        {
            sfxAudioSource.PlayOneShot(correctSound);
        }
    }

    void PlayWrongSound()
    {
        bool isSoundOn = PlayerPrefs.GetInt("_Mute", 0) == 0;
        if (isSoundOn && sfxAudioSource != null && wrongSound != null)
        {
            sfxAudioSource.PlayOneShot(wrongSound);
        }
    }

    void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main-Menu"); // Adjust to your actual main menu scene name
    }

    void ReplayGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void ResetStars()
    {
        if (modeStars == null) return;
        foreach (GameObject star in modeStars)
        {
            if (star != null) star.SetActive(false);
        }
    }

    void Update()
    {
        // --- TEST CHEAT: Press 'N' to skip 5 questions ---
        if (Input.GetKeyDown(KeyCode.N))
        {
            SkipQuestions(5);
        }

        if (isGameRunning)
        {
            // Keyboard Input (Left/A, Right/D, Down/S, Up/W/F)
            if (option1Button.interactable && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)))
            {
                CheckAnswer(1);
                
            }
            else if (option2Button.interactable && (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)))
            {
                CheckAnswer(2);
            }
            else if (option3Button.gameObject.activeSelf && option3Button.interactable && (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)))
            {
                CheckAnswer(3);
            }
            else if (option4Button != null && option4Button.gameObject.activeSelf && option4Button.interactable && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.F)))
            {
                CheckAnswer(4);
            }

            // Timer update
            if (!isTimeFrozen)
            {
                currentTime -= Time.deltaTime;
                timerSlider.value = currentTime / timePerQuestion;

                if (currentTime <= 0)
                {
                    currentTime = 0;
                    timerSlider.value = 0;
                    TriggerTimesUpPopup();
                }
            }
        }
    }

    public void SkipQuestions(int amount)
    {
        if (!isGameRunning) return;

        currentQuestion += amount;

        
        if (currentQuestion >= totalQuestions)
        {
            ShowScoreCard();
        }
        else
        {
            
            streak = 0;
            comboObject.SetActive(false);
            ShowQuestion();
        }
    }

    private IEnumerator CountdownRoutine()
    {
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);

        StartGame();
    }

    void StartGame()
    {
        score = 0;
        currentQuestion = 0;
        isGameRunning = true;
        askedQuestions.Clear();
        streak = 0;
        questionsAnswered = 0;
        maxStreakThisSession = 0;

        questionText.gameObject.SetActive(true);
        option1Button.gameObject.SetActive(true);
        option2Button.gameObject.SetActive(true);
        
        timerSlider.transform.parent.gameObject.SetActive(true);
        levelText.gameObject.SetActive(true);

        if (starsUI != null) starsUI.SetActive(true);
        ResetStars();

        ShowQuestion();
    }

    void ShowQuestion()
    {
        int currentLevel = (currentQuestion / questionsPerLevel) + 1;
        int localQuestion = currentQuestion % questionsPerLevel;
        levelText.text = "Level " + currentLevel;

        // Reset stars if starting a new difficulty mode (every 5 questions)
        if (localQuestion % 5 == 0)
        {
            ResetStars();
        }

        string questionKey;
        string qText;

        do
        {
            GenerateEquation(currentLevel, localQuestion, out qText, out correctAnswer);
            questionKey = qText;
        } while (askedQuestions.Contains(questionKey));

        askedQuestions.Add(questionKey);

        questionText.text = qText + " = ?";

        int numOptions = (currentLevel < 5) ? 3 : 4;
        
        option1Button.gameObject.SetActive(true);
        option2Button.gameObject.SetActive(true);
        option3Button.gameObject.SetActive(true);
        if (option4Button != null) option4Button.gameObject.SetActive(numOptions == 4);

        List<int> wrongAnswers = new List<int>();
        while (wrongAnswers.Count < numOptions - 1)
        {
            int w = GenerateWrongAnswer(correctAnswer);
            if (!wrongAnswers.Contains(w)) wrongAnswers.Add(w);
        }

        int correctPosition = Random.Range(1, numOptions + 1);
        TextMeshProUGUI[] optionTexts = { option1Text, option2Text, option3Text, option4Text };

        int wrongIndex = 0;
        for (int i = 0; i < numOptions; i++)
        {
            if (i + 1 == correctPosition)
            {
                optionTexts[i].text = correctAnswer.ToString();
            }
            else
            {
                optionTexts[i].text = wrongAnswers[wrongIndex].ToString();
                wrongIndex++;
            }
        }

        currentTime = timePerQuestion;
        timerSlider.value = 1f;

        option1Button.interactable = true;
        option2Button.interactable = true;
        option3Button.interactable = true;
        if (option4Button != null) option4Button.interactable = true;
    }

    void GenerateEquation(int level, int questionIndex, out string qText, out int correctAns)
    {
        int diffMode = questionIndex / 5; // 0=easy, 1=med, 2=hard
        int operatorType = (level - 1) % 4; // 0: +, 1: -, 2: *, 3: /
        int operandCount = ((level - 1) / 4) + 2; // Level 1-4: 2, Level 5-8: 3, Level 9-12: 4

        int minVal = 1;
        int maxVal = 10;
        
        if (operatorType == 0 || operatorType == 1) // + or -
        {
            if (diffMode == 0) { minVal = 1; maxVal = 10; }
            else if (diffMode == 1) { minVal = 5; maxVal = 20; }
            else { minVal = 10; maxVal = 50; }
        }
        else // * or /
        {
            if (diffMode == 0) { minVal = 1; maxVal = 6; }
            else if (diffMode == 1) { minVal = 2; maxVal = 10; }
            else { minVal = 3; maxVal = 15; } 
        }

        List<int> vals = new List<int>();
        for (int i = 0; i < operandCount; i++)
        {
            vals.Add(Random.Range(minVal, maxVal));
        }

        if (operatorType == 0)                              // Addition
        {
            correctAns = 0;
            qText = "";
            for (int i = 0; i < vals.Count; i++)
            {
                correctAns += vals[i];
                qText += vals[i].ToString();
                if (i < vals.Count - 1) qText += " + ";
            }
        }
        else if (operatorType == 1)                      // Subtraction
        {
            int startNum = 0;
            foreach (int v in vals) startNum += v;
            
            correctAns = vals[vals.Count - 1];
            qText = startNum.ToString();
            for (int i = 0; i < vals.Count - 1; i++)
            {
                qText += " - " + vals[i];
            }
        }
        else if (operatorType == 2)                     // Multiplication
        {
            correctAns = 1;
            qText = "";
            for (int i = 0; i < vals.Count; i++)
            {
                correctAns *= vals[i];
                qText += vals[i].ToString();
                if (i < vals.Count - 1) qText += " <sprite=\"X\" index=0> ";
            }
        }
        else                                                 // Division
        {
            int startNum = 1;
            foreach (int v in vals) startNum *= v;
            
            correctAns = vals[vals.Count - 1];
            qText = startNum.ToString();
            for (int i = 0; i < vals.Count - 1; i++)
            {
                qText += " <sprite=\"%\" index=0> ";
                qText += vals[i];
            }
        }
    }

    int GenerateWrongAnswer(int correct)
    {
        int wrong;

        do
        {
            int offset = Random.Range(1, 6);

            if (Random.Range(0, 2) == 0)
                wrong = correct + offset;
            else
                wrong = correct - offset;

        } while (wrong == correct || wrong < 0);

        return wrong;
    }

    void CheckAnswer(int buttonNumber)
    {
        if (!isGameRunning) return;

        option1Button.interactable = false;
        option2Button.interactable = false;
        option3Button.interactable = false;
        if (option4Button != null) option4Button.interactable = false;

        questionsAnswered++;

        int playerAnswer;
        if (buttonNumber == 1) playerAnswer = int.Parse(option1Text.text);
        else if (buttonNumber == 2) playerAnswer = int.Parse(option2Text.text);
        else if (buttonNumber == 3) playerAnswer = int.Parse(option3Text.text); 
        else playerAnswer = int.Parse(option4Text.text);

        int localQuestion = currentQuestion % questionsPerLevel;
        int starIndex = localQuestion % 5;

        if (playerAnswer == correctAnswer)
        {
            score++;
            streak++;
            if (streak > maxStreakThisSession) maxStreakThisSession = streak;

            PlayCorrectSound();

            questionText.text = "Correct!";
            questionText.color = Color.green;
            timePerQuestion += timeBonus;

            if (modeStars != null && starIndex < modeStars.Length && modeStars[starIndex] != null)
            {
                modeStars[starIndex].SetActive(true);
            }

            if (streak >= 3)
            {
                comboObject.SetActive(true);
                comboText.text = "x" + streak;

                if (streak == 3) // Time Freeze at 3 streak
                {
                    if (powerUpSlideCoroutine != null) StopCoroutine(powerUpSlideCoroutine);
                    powerUpSlideCoroutine = StartCoroutine(SlidePowerUpRoutine(powerUpRect));
                }

                int currentLevel = (currentQuestion / questionsPerLevel) + 1;
                if (streak == 5 && currentLevel >= 5) // 50/50 at 5 streak (level 5+ has 4 options)
                {
                    if (fiftyFiftySlideCoroutine != null) StopCoroutine(fiftyFiftySlideCoroutine);
                    fiftyFiftySlideCoroutine = StartCoroutine(SlidePowerUpRoutine(fiftyFiftyRect));
                }
            }
        }
        else
        {
            PlayWrongSound();

            questionText.text = "Wrong! Answer was " + correctAnswer;
            questionText.color = Color.red;
            timePerQuestion -= timePenalty;
            streak = 0;
            comboObject.SetActive(false);

            if (timePerQuestion < 1f)
                timePerQuestion = 1f;
        }

        StartCoroutine(NextQuestionDelay());
    }

    void TriggerTimesUpPopup()
    {
        isGameRunning = false; // Pause immediately
        
        option1Button.interactable = false;
        option2Button.interactable = false;
        option3Button.interactable = false;
        if (option4Button != null) option4Button.interactable = false;

        if (timesUpPopup != null) timesUpPopup.SetActive(true);
    }

    public void OnTimesUpYesClicked()
    {
        // Ad logic later. For now, revive the player and continue!
        if (timesUpPopup != null) timesUpPopup.SetActive(false);
        currentTime = timePerQuestion;
        isGameRunning = true;
        
        option1Button.interactable = true;
        option2Button.interactable = true;
        option3Button.interactable = true;
        if (option4Button != null && option4Button.gameObject.activeSelf) option4Button.interactable = true;
    }

    public void OnTimesUpNoClicked()
    {
        if (timesUpPopup != null) timesUpPopup.SetActive(false);
        questionsAnswered++; // Count the timeout as a failed attempt
        ShowScoreCard();
    }

    void ShowScoreCard()
    {
        isGameRunning = false;
        
        // Hide game UI (replaces old GameOver logic)
        option1Button.gameObject.SetActive(false);
        option2Button.gameObject.SetActive(false);
        option3Button.gameObject.SetActive(false);
        if (option4Button != null) option4Button.gameObject.SetActive(false);
        timerSlider.transform.parent.gameObject.SetActive(false);
        if (starsUI != null) starsUI.SetActive(false);
        questionText.gameObject.SetActive(false);
        comboObject.SetActive(false);

        if (scoreCardPopup != null) scoreCardPopup.SetActive(true);
        if (scoreCardScoreText != null) scoreCardScoreText.text = score.ToString();
        
        int currentLevel = (currentQuestion / questionsPerLevel) + 1;
        if (scoreCardLevelText != null) scoreCardLevelText.text = currentLevel.ToString("D2");

        GradeTier earnedGrade = GetBestGradeTier(score);
        if (scoreCardGradeImage != null && earnedGrade.gradeSprite != null)
        {
            scoreCardGradeImage.sprite = earnedGrade.gradeSprite;
            scoreCardGradeImage.gameObject.SetActive(true);
        }
        else if (scoreCardGradeImage != null)
        {
            scoreCardGradeImage.gameObject.SetActive(false);
        }

        // SAVE STATS
        int totalGames = PlayerPrefs.GetInt("GamesPlayed", 0) + 1;
        PlayerPrefs.SetInt("GamesPlayed", totalGames);

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (score > highScore) PlayerPrefs.SetInt("HighScore", score);

        int maxS = PlayerPrefs.GetInt("MaxStreak", 0);
        if (maxStreakThisSession > maxS) PlayerPrefs.SetInt("MaxStreak", maxStreakThisSession);

        float accuracy = 0f;
        if (questionsAnswered > 0)
        {
            accuracy = ((float)score / questionsAnswered) * 100f;
        }
        float bestAcc = PlayerPrefs.GetFloat("BestAccuracy", 0f);
        if (accuracy > bestAcc) PlayerPrefs.SetFloat("BestAccuracy", accuracy);

        int historicalHighScore = PlayerPrefs.GetInt("HighScore", 0); 
        GradeTier historicalBestGrade = GetBestGradeTier(historicalHighScore);
        PlayerPrefs.SetString("BestGradeName", historicalBestGrade.gradeName);
        PlayerPrefs.Save();
    }

    private GradeTier GetBestGradeTier(int currentScore)
    {
        GradeTier bestTier = new GradeTier();
        int highestMinScore = -1;

        if (gradeTiers != null)
        {
            foreach (var tier in gradeTiers)
            {
                if (currentScore >= tier.minScore && tier.minScore > highestMinScore)
                {
                    highestMinScore = tier.minScore;
                    bestTier = tier;
                }
            }
        }

        return bestTier;
    }

    IEnumerator NextQuestionDelay()
    {
        isGameRunning = false;

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(FadeTo(0f));

        questionText.color = defaultColor;

        currentQuestion++;

        if (currentQuestion >= totalQuestions)
        {
            ShowScoreCard();
        }
        else
        {
            ShowQuestion();
            yield return StartCoroutine(FadeTo(1f));
            isGameRunning = true;
        }
    }

    IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = quizCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            quizCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        quizCanvasGroup.alpha = targetAlpha;
    }

    private IEnumerator SlidePowerUpRoutine(RectTransform targetRect)
    {
        if (targetRect == null) yield break;

        targetRect.gameObject.SetActive(true);
        float elapsed = 0f;
        Vector2 pos = targetRect.anchoredPosition;
        
        while (elapsed < powerUpSlideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / powerUpSlideDuration;
            pos.y = Mathf.Lerp(powerUpStartY, powerUpEndY, t);
            targetRect.anchoredPosition = pos;
            yield return null;
        }

        targetRect.gameObject.SetActive(false);
    }

    private void ActivateTimeFreeze()
    {
        if (powerUpSlideCoroutine != null)
        {
            StopCoroutine(powerUpSlideCoroutine);
        }
        powerUpRect.gameObject.SetActive(false);
        StartCoroutine(TimeFreezeRoutine());
    }

    private IEnumerator TimeFreezeRoutine()
    {
        isTimeFrozen = true;
        yield return new WaitForSeconds(freezeDuration);
        isTimeFrozen = false;
    }

    private void ActivateFiftyFifty()
    {
        if (fiftyFiftySlideCoroutine != null) StopCoroutine(fiftyFiftySlideCoroutine);
        if (fiftyFiftyRect != null) fiftyFiftyRect.gameObject.SetActive(false);
        
        int currentLevel = (currentQuestion / questionsPerLevel) + 1;
        if (currentLevel < 5) return; // Only for 4 options

        Button[] buttons = { option1Button, option2Button, option3Button, option4Button };
        TextMeshProUGUI[] texts = { option1Text, option2Text, option3Text, option4Text };

        List<int> wrongIndices = new List<int>();
        int correctIndex = -1;

        for (int i = 0; i < 4; i++)
        {
            if (buttons[i].gameObject.activeSelf)
            {
                if (int.Parse(texts[i].text) == correctAnswer)
                {
                    correctIndex = i;
                }
                else
                {
                    wrongIndices.Add(i);
                }
            }
        }

        // Disable 2 wrong options, keep the correct one and the farthest wrong one
        if (wrongIndices.Count == 3)
        {
            int farthestIndex = wrongIndices[0];
            int maxDiff = Mathf.Abs(int.Parse(texts[wrongIndices[0]].text) - correctAnswer);

            for (int i = 1; i < 3; i++)
            {
                int diff = Mathf.Abs(int.Parse(texts[wrongIndices[i]].text) - correctAnswer);
                if (diff > maxDiff)
                {
                    maxDiff = diff;
                    farthestIndex = wrongIndices[i];
                }
            }

            foreach (int wIndex in wrongIndices)
            {
                if (wIndex != farthestIndex)
                {
                    buttons[wIndex].gameObject.SetActive(false);
                }
            }
        }
    }
}
