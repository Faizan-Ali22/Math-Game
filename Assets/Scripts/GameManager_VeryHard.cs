using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager_VeryHard : MonoBehaviour
{
    [Header("Start Instructions Popup")]
    [SerializeField] private GameObject startInstructionsPopup;
    [SerializeField] private Button startCloseButton;

    [Header("Popups")]
    [SerializeField] private GameObject timesUpPopup;
    [SerializeField] private Button timesUpYesButton;
    [SerializeField] private Button timesUpNoButton;

    [Header("UI Elements")]
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

    [Header("Buttons")]
    [SerializeField] private Button option1Button;
    [SerializeField] private Button option2Button;
    [SerializeField] private Button option3Button;
    [SerializeField] private Button option4Button;

    [Header("Audio")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private AudioClip wrongSound;

    [Header("Transition")]
    [SerializeField] private CanvasGroup quizCanvasGroup;
    [SerializeField] private float fadeDuration = 0.1f;
    [SerializeField] private float answerFeedbackDelay = 0.35f;

    [Header("Timer Settings")]
    [SerializeField] private float totalTime = 120f;

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

    [Header("Score Card")]
    [SerializeField] private GameObject scoreCardPopup;
    [SerializeField] private TextMeshProUGUI scoreCardScoreText;
    [SerializeField] private TextMeshProUGUI scoreCardLevelText;
    [SerializeField] private Image scoreCardGradeImage;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button replayButton;

    [Header("Grades")]
    [SerializeField] private GradeTier[] gradeTiers;

    private int correctAnswer;
    private float remainingTime;
    private bool isGameRunning = false;
    private int score = 0;
    private Color defaultColor;
    private int streak = 0;
    private bool isTimeFrozen = false;
    private Coroutine powerUpSlideCoroutine;
    private Coroutine fiftyFiftySlideCoroutine;

    private int questionsAnswered = 0;
    private int maxStreakThisSession = 0;

    void Start()
    {
        defaultColor = questionText.color;

        // Hide gameplay UI initially
        questionText.gameObject.SetActive(false);
        option1Button.gameObject.SetActive(false);
        option2Button.gameObject.SetActive(false);
        option3Button.gameObject.SetActive(false);
        option4Button.gameObject.SetActive(false);
        timerSlider.transform.parent.gameObject.SetActive(false);
        if (levelText != null) levelText.gameObject.SetActive(false);
        comboObject.SetActive(false);
        if (powerUpRect != null) powerUpRect.gameObject.SetActive(false);
        if (fiftyFiftyRect != null) fiftyFiftyRect.gameObject.SetActive(false);
        if (starsUI != null) starsUI.SetActive(false);
        if (scoreCardPopup != null) scoreCardPopup.SetActive(false);
        if (timesUpPopup != null) timesUpPopup.SetActive(false);

        // Bind answer buttons
        option1Button.onClick.AddListener(() => CheckAnswer(1));
        option2Button.onClick.AddListener(() => CheckAnswer(2));
        option3Button.onClick.AddListener(() => CheckAnswer(3));
        option4Button.onClick.AddListener(() => CheckAnswer(4));

        // Bind powerups
        if (powerUpButton != null) powerUpButton.onClick.AddListener(ActivateTimeFreeze);
        if (fiftyFiftyButton != null) fiftyFiftyButton.onClick.AddListener(ActivateFiftyFifty);

        // Bind times up buttons
        if (timesUpNoButton != null) timesUpNoButton.onClick.AddListener(OnTimesUpNoClicked);
        if (timesUpYesButton != null) timesUpYesButton.onClick.AddListener(OnTimesUpYesClicked);

        // Bind scorecard buttons
        if (homeButton != null) homeButton.onClick.AddListener(GoToMainMenu);
        if (replayButton != null) replayButton.onClick.AddListener(ReplayGame);

        SetupAudio();

        // Show Start Popup
        if (startInstructionsPopup != null)
        {
            startInstructionsPopup.SetActive(true);
            if (startCloseButton != null)
            {
                startCloseButton.onClick.AddListener(OnStartPopupClosed);
            }
        }
        else
        {
            // If no popup assigned, start directly
            StartVeryHardGame();
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

    void PlayWrongSound()
    {
        bool isSoundOn = PlayerPrefs.GetInt("_Mute", 0) == 0;
        if (isSoundOn && sfxAudioSource != null && wrongSound != null)
        {
            sfxAudioSource.PlayOneShot(wrongSound);
        }
    }

    public void OnStartPopupClosed()
    {
        if (startInstructionsPopup != null)
        {
            startInstructionsPopup.SetActive(false);
        }
        StartVeryHardGame();
    }

    void StartVeryHardGame()
    {
        score = 0;
        streak = 0;
        questionsAnswered = 0;
        maxStreakThisSession = 0;
        remainingTime = totalTime;
        isTimeFrozen = false;
        isGameRunning = true;

        if (quizCanvasGroup != null) quizCanvasGroup.alpha = 1f;

        questionText.gameObject.SetActive(true);
        option1Button.gameObject.SetActive(true);
        option2Button.gameObject.SetActive(true);
        option3Button.gameObject.SetActive(true);
        option4Button.gameObject.SetActive(true);

        timerSlider.transform.parent.gameObject.SetActive(true);
        timerSlider.value = 1f;

        if (levelText != null)
        {
            levelText.gameObject.SetActive(true);
            levelText.text = "Very Hard";
        }

        ShowQuestion();
    }

    void Update()
    {
        // Skip question cheat for testing
        if (Input.GetKeyDown(KeyCode.N) && isGameRunning)
        {
            ShowQuestion();
        }

        if (isGameRunning)
        {
            // Keyboard Controls
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
            else if (option4Button.gameObject.activeSelf && option4Button.interactable && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.F)))
            {
                CheckAnswer(4);
            }

            // 120-second continuous timer
            if (!isTimeFrozen)
            {
                remainingTime -= Time.deltaTime;
                timerSlider.value = Mathf.Clamp01(remainingTime / totalTime);

                if (remainingTime <= 0f)
                {
                    remainingTime = 0f;
                    timerSlider.value = 0f;
                    TriggerTimesUpPopup();
                }
            }
        }
    }

    void ShowQuestion()
    {
        // Reset option button visibility & interactability (re-enables any hidden by 50/50)
        option1Button.gameObject.SetActive(true);
        option2Button.gameObject.SetActive(true);
        option3Button.gameObject.SetActive(true);
        option4Button.gameObject.SetActive(true);

        option1Button.interactable = true;
        option2Button.interactable = true;
        option3Button.interactable = true;
        option4Button.interactable = true;

        string qText;
        GenerateVeryHardEquation(out qText, out correctAnswer);
        questionText.text = qText;

        // Generate tricky distractors (off by ±1 or ±2)
        List<int> options = GenerateTrickyDistractors(correctAnswer);

        option1Text.text = options[0].ToString();
        option2Text.text = options[1].ToString();
        option3Text.text = options[2].ToString();
        option4Text.text = options[3].ToString();
    }

    void GenerateVeryHardEquation(out string qText, out int correctAns)
    {
        // Variety of Mixed Operations (PEMDAS) and Two-Digit Math
        int type = Random.Range(0, 8);

        switch (type)
        {
            case 0: // PEMDAS: A + (B x C)
            {
                int b = Random.Range(2, 10);
                int c = Random.Range(2, 10);
                int a = Random.Range(10, 50);
                correctAns = a + (b * c);
                qText = a + " + " + b + " <sprite=\"X\" index=0> " + c + " = ?";
                break;
            }

            case 1: // PEMDAS: (A x B) - C
            {
                int a = Random.Range(3, 11);
                int b = Random.Range(3, 11);
                int prod = a * b;
                int c = Random.Range(5, Mathf.Max(6, prod - 5));
                correctAns = prod - c;
                qText = a + " <sprite=\"X\" index=0> " + b + " - " + c + " = ?";
                break;
            }

            case 2: // PEMDAS: (A x B) + C
            {
                int a = Random.Range(3, 10);
                int b = Random.Range(3, 10);
                int c = Random.Range(15, 60);
                correctAns = (a * b) + c;
                qText = a + " <sprite=\"X\" index=0> " + b + " + " + c + " = ?";
                break;
            }

            case 3: // PEMDAS: A + (B ÷ C) -> Clean Integer Division
            {
                int c = Random.Range(2, 9);
                int quotient = Random.Range(3, 12);
                int b = c * quotient;
                int a = Random.Range(15, 60);
                correctAns = a + quotient;
                qText = a + " + " + b + " <sprite=\"%\" index=0> " + c + " = ?";
                break;
            }

            case 4: // PEMDAS: A - (B ÷ C) -> Clean Integer Division & Positive Result
            {
                int c = Random.Range(2, 8);
                int quotient = Random.Range(2, 10);
                int b = c * quotient;
                int a = Random.Range(quotient + 15, quotient + 60);
                correctAns = a - quotient;
                qText = a + " - " + b + " <sprite=\"%\" index=0> " + c + " = ?";
                break;
            }

            case 5: // Two-Digit Addition: A + B
            {
                int a = Random.Range(18, 85);
                int b = Random.Range(18, 85);
                correctAns = a + b;
                qText = a + " + " + b + " = ?";
                break;
            }

            case 6: // Two-Digit Subtraction: A - B
            {
                correctAns = Random.Range(15, 65);
                int b = Random.Range(14, 48);
                int a = correctAns + b;
                qText = a + " - " + b + " = ?";
                break;
            }

            default: // Two-Digit x Single Multiplication: A x B
            {
                int a = Random.Range(12, 28);
                int b = Random.Range(3, 9);
                correctAns = a * b;
                qText = a + " <sprite=\"X\" index=0> " + b + " = ?";
                break;
            }
        }
    }

    List<int> GenerateTrickyDistractors(int correct)
    {
        List<int> options = new List<int> { correct };

        // Candidate offsets around the correct answer (±1, ±2, ±3)
        List<int> candidateOffsets = new List<int> { -1, 1, -2, 2, -3, 3 };

        // Shuffle offsets
        for (int i = 0; i < candidateOffsets.Count; i++)
        {
            int temp = candidateOffsets[i];
            int r = Random.Range(i, candidateOffsets.Count);
            candidateOffsets[i] = candidateOffsets[r];
            candidateOffsets[r] = temp;
        }

        foreach (int offset in candidateOffsets)
        {
            int distractor = correct + offset;
            if (distractor >= 0 && !options.Contains(distractor))
            {
                options.Add(distractor);
                if (options.Count == 4) break;
            }
        }

        // Safety fallback if numbers were near 0
        int fallback = 4;
        while (options.Count < 4)
        {
            int val = correct + fallback;
            if (!options.Contains(val)) options.Add(val);
            fallback++;
        }

        // Shuffle the 4 options so the correct answer position is random
        for (int i = 0; i < options.Count; i++)
        {
            int temp = options[i];
            int r = Random.Range(i, options.Count);
            options[i] = options[r];
            options[r] = temp;
        }

        return options;
    }

    void CheckAnswer(int buttonNumber)
    {
        if (!isGameRunning) return;

        option1Button.interactable = false;
        option2Button.interactable = false;
        option3Button.interactable = false;
        option4Button.interactable = false;

        questionsAnswered++;

        int playerAnswer;
        if (buttonNumber == 1) playerAnswer = int.Parse(option1Text.text);
        else if (buttonNumber == 2) playerAnswer = int.Parse(option2Text.text);
        else if (buttonNumber == 3) playerAnswer = int.Parse(option3Text.text);
        else playerAnswer = int.Parse(option4Text.text);

        if (playerAnswer == correctAnswer)
        {
            score++;
            streak++;
            if (streak > maxStreakThisSession) maxStreakThisSession = streak;

            questionText.text = "Correct!";
            questionText.color = Color.green;

            if (streak >= 3)
            {
                comboObject.SetActive(true);
                comboText.text = "x" + streak;

                // Time Freeze at 3 streak
                if (streak == 3 && powerUpRect != null)
                {
                    if (powerUpSlideCoroutine != null) StopCoroutine(powerUpSlideCoroutine);
                    powerUpSlideCoroutine = StartCoroutine(SlidePowerUpRoutine(powerUpRect));
                }
                // 50/50 at 5 streak (throughout entire mode!)
                else if (streak == 5 && fiftyFiftyRect != null)
                {
                    if (fiftyFiftySlideCoroutine != null) StopCoroutine(fiftyFiftySlideCoroutine);
                    fiftyFiftySlideCoroutine = StartCoroutine(SlidePowerUpRoutine(fiftyFiftyRect));
                }
            }
        }
        else
        {
            PlayWrongSound();

            questionText.text = "Wrong! Answer: " + correctAnswer;
            questionText.color = Color.red;
            streak = 0;
            comboObject.SetActive(false);
        }

        StartCoroutine(NextQuestionDelay());
    }

    IEnumerator NextQuestionDelay()
    {
        yield return new WaitForSeconds(answerFeedbackDelay);

        if (quizCanvasGroup != null)
        {
            yield return StartCoroutine(FadeTo(0f));
        }

        questionText.color = defaultColor;

        if (isGameRunning)
        {
            ShowQuestion();
            if (quizCanvasGroup != null)
            {
                yield return StartCoroutine(FadeTo(1f));
            }
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
        if (powerUpSlideCoroutine != null) StopCoroutine(powerUpSlideCoroutine);
        if (powerUpRect != null) powerUpRect.gameObject.SetActive(false);
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

        Button[] buttons = { option1Button, option2Button, option3Button, option4Button };
        TextMeshProUGUI[] texts = { option1Text, option2Text, option3Text, option4Text };

        List<int> wrongIndices = new List<int>();

        for (int i = 0; i < 4; i++)
        {
            if (buttons[i].gameObject.activeSelf)
            {
                if (int.Parse(texts[i].text) != correctAnswer)
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

    void TriggerTimesUpPopup()
    {
        isGameRunning = false; // Pause immediately
        
        option1Button.interactable = false;
        option2Button.interactable = false;
        option3Button.interactable = false;
        option4Button.interactable = false;

        if (timesUpPopup != null) timesUpPopup.SetActive(true);
    }

    public void OnTimesUpYesClicked()
    {
        if (timesUpPopup != null) timesUpPopup.SetActive(false);
        remainingTime = 30f; // Give 30s revive bonus
        isGameRunning = true;
        
        option1Button.interactable = true;
        option2Button.interactable = true;
        option3Button.interactable = true;
        option4Button.interactable = true;
    }

    public void OnTimesUpNoClicked()
    {
        if (timesUpPopup != null) timesUpPopup.SetActive(false);
        EndVeryHardGame();
    }

    void EndVeryHardGame()
    {
        isGameRunning = false;

        // Hide gameplay UI
        option1Button.gameObject.SetActive(false);
        option2Button.gameObject.SetActive(false);
        option3Button.gameObject.SetActive(false);
        option4Button.gameObject.SetActive(false);
        timerSlider.transform.parent.gameObject.SetActive(false);
        questionText.gameObject.SetActive(false);
        comboObject.SetActive(false);
        if (powerUpRect != null) powerUpRect.gameObject.SetActive(false);
        if (fiftyFiftyRect != null) fiftyFiftyRect.gameObject.SetActive(false);

        // Show Score Card
        if (scoreCardPopup != null) scoreCardPopup.SetActive(true);
        if (scoreCardScoreText != null) scoreCardScoreText.text = score.ToString();
        if (scoreCardLevelText != null) scoreCardLevelText.text = "VH";

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

        int vhHigh = PlayerPrefs.GetInt("VeryHard_HighScore", 0);
        if (score > vhHigh) PlayerPrefs.SetInt("VeryHard_HighScore", score);

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
        if (!string.IsNullOrEmpty(historicalBestGrade.gradeName))
        {
            PlayerPrefs.SetString("BestGradeName", historicalBestGrade.gradeName);
        }
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

    void GoToMainMenu()
    {
        SceneManager.LoadScene("Main-Menu");
    }

    void ReplayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
