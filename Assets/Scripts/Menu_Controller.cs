using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public struct AchievementTier
{
    public int requiredScore;
    public Image medalImage;
    public Sprite unlockedSprite;
}

public class Menu_Controller : MonoBehaviour {

    [Tooltip("_sceneToLoadOnPlay is the name of the scene that will be loaded when users click play")]
    public string _sceneToLoadOnPlay = "Level";
    
    [Header("Audio Components")]
    public AudioClip _audioClip;
    public AudioSource _audioSource;

    [Header("Sound Settings")]
    public Image soundButtonImage;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    [Header("Music Settings")]
    public AudioSource backgroundMusicAudioSource;
    public Image musicButtonImage;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject scoreBoardPanel;
    public GameObject achievementsPanel;
    public GameObject instructionsPanel;

    [Header("ScoreBoard Texts")]
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI gamesPlayedText;
    public TextMeshProUGUI maxStreakText;
    public TextMeshProUGUI bestAccuracyText;
    public TextMeshProUGUI bestGradeText;

    [Header("Achievements")]
    public AchievementTier[] achievementTiers;

    [Header("Very Hard Mode Lock Settings")]
    public Button veryHardButton;
    public GameObject veryHardLockIcon;
    [Tooltip("Minimum score required to unlock Very Hard mode (set this to your Gold or Diamond score threshold)")]
    public int veryHardUnlockScore = 50;
    public GameObject veryHardLockedPopup;

    private bool isSoundOn = true;
    private bool isMusicOn = true;

    Scene scene;

    void Awake()
    {
        if (!PlayerPrefs.HasKey("_Mute")) PlayerPrefs.SetInt("_Mute", 0);
        if (!PlayerPrefs.HasKey("MusicOn")) PlayerPrefs.SetInt("MusicOn", 1);

        isSoundOn = PlayerPrefs.GetInt("_Mute") == 0;
        isMusicOn = PlayerPrefs.GetInt("MusicOn") == 1;

        UpdateSoundUI();
        UpdateMusicUI();
        UpdateBackgroundMusic();

        if (scoreBoardPanel != null) scoreBoardPanel.SetActive(false);
        if (achievementsPanel != null) achievementsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (veryHardLockedPopup != null) veryHardLockedPopup.SetActive(false);

        UpdateVeryHardLockUI();

        scene = SceneManager.GetActiveScene();
        PlayerPrefs.SetString("_LastScene", scene.name); 
    }

    public void OpenScoreBoard()
    {
        PlayClickSound();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (scoreBoardPanel != null) scoreBoardPanel.SetActive(true);

        if (highScoreText != null) highScoreText.text = PlayerPrefs.GetInt("HighScore", 0).ToString();
        if (gamesPlayedText != null) gamesPlayedText.text = PlayerPrefs.GetInt("GamesPlayed", 0).ToString();
        if (maxStreakText != null) maxStreakText.text = PlayerPrefs.GetInt("MaxStreak", 0).ToString();
        if (bestAccuracyText != null) bestAccuracyText.text = Mathf.RoundToInt(PlayerPrefs.GetFloat("BestAccuracy", 0f)).ToString() + "%";
        
        string bestG = PlayerPrefs.GetString("BestGradeName", "None");
        if (string.IsNullOrEmpty(bestG)) bestG = "None";
        if (bestGradeText != null) bestGradeText.text = bestG;
         PlayClickSound();
    }

    public void OpenAchievements()
    {
        PlayClickSound();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (achievementsPanel != null) achievementsPanel.SetActive(true);

        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (achievementTiers != null)
        {
            foreach (var tier in achievementTiers)
            {
                if (highScore >= tier.requiredScore && tier.medalImage != null && tier.unlockedSprite != null)
                {
                    tier.medalImage.sprite = tier.unlockedSprite;
                }
            }
        }
         PlayClickSound();
    }

    public void BackToMainMenu()
    {
        PlayClickSound();
        if (scoreBoardPanel != null) scoreBoardPanel.SetActive(false);
        if (achievementsPanel != null) achievementsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        PlayClickSound();
    }
    public void OpenInstructions()
    {
        PlayClickSound();
        if (instructionsPanel != null) instructionsPanel.SetActive(true);
        PlayClickSound();
    }
    public void CloseInstructions()
    {
        PlayClickSound();
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        PlayClickSound();
    }
    public void PlayGame()
    {
        PlayClickSound();
        PlayerPrefs.SetString("_LastScene", scene.name);
        SceneManager.LoadScene(_sceneToLoadOnPlay);
    }

    public bool IsVeryHardUnlocked()
    {
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
        return currentHighScore >= veryHardUnlockScore;
    }

    private void UpdateVeryHardLockUI()
    {
        bool isUnlocked = IsVeryHardUnlocked();

        // Always ensure the Very Hard button itself stays visible
        if (veryHardButton != null)
        {
            veryHardButton.gameObject.SetActive(true);
            if (veryHardLockedPopup == null)
            {
                veryHardButton.interactable = isUnlocked;
            }
        }

        // Only toggle the lock icon if it's a separate icon object (not the button itself)
        if (veryHardLockIcon != null)
        {
            if (veryHardButton == null || veryHardLockIcon != veryHardButton.gameObject)
            {
                veryHardLockIcon.SetActive(!isUnlocked);
            }
        }
    }

    public void PlayVeryHard()
    {
        PlayClickSound();

        if (!IsVeryHardUnlocked())
        {
            if (veryHardLockedPopup != null)
            {
                veryHardLockedPopup.SetActive(true);
            }
            return;
        }

        PlayerPrefs.SetString("_LastScene", scene.name);
        SceneManager.LoadScene("VeryHard");
    }

    public void CloseVeryHardLockedPopup()
    {
        PlayClickSound();
        if (veryHardLockedPopup != null)
        {
            veryHardLockedPopup.SetActive(false);
        }
    }

    // --- TEST CHEAT: Unlocks required score/medal and removes lock so you can click Very Hard button to play ---
    public void DevUnlockVeryHard()
    {
        PlayClickSound();

        int currentHigh = PlayerPrefs.GetInt("HighScore", 0);
        if (currentHigh < veryHardUnlockScore)
        {
            PlayerPrefs.SetInt("HighScore", veryHardUnlockScore);
            PlayerPrefs.Save();
        }

        UpdateVeryHardLockUI();
    }

    public void DevBypassAndPlayVeryHard()
    {
        DevUnlockVeryHard();
    }
    
    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt("_Mute", isSoundOn ? 0 : 1);
        UpdateSoundUI();
        PlayClickSound();
    }

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 1 : 0);
        UpdateMusicUI();
        UpdateBackgroundMusic();
        PlayClickSound();
    }

    private void UpdateBackgroundMusic()
    {
        if (backgroundMusicAudioSource == null)
        {
            GameObject mpObj = GameObject.Find("_MusicPlayer");
            if (mpObj != null)
            {
                backgroundMusicAudioSource = mpObj.GetComponent<AudioSource>();
            }
        }

        if (backgroundMusicAudioSource != null)
        {
            backgroundMusicAudioSource.mute = !isMusicOn;
        }
    }

    private void UpdateSoundUI()
    {
        if (soundButtonImage != null)
        {
            soundButtonImage.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
        }
    }

    private void UpdateMusicUI()
    {
        if (musicButtonImage != null)
        {
            musicButtonImage.sprite = isMusicOn ? musicOnSprite : musicOffSprite;
        }
    }

    private void PlayClickSound()
    {
        if (isSoundOn && _audioSource != null && _audioClip != null)
        {
            // Do not play click sound if the assigned clip is a long music track (> 3 sec)
            if (_audioClip.length > 3f) return;
            _audioSource.PlayOneShot(_audioClip);
        }
    }

    public void QuitGame()
    {
        PlayClickSound();
        #if !UNITY_EDITOR
            Application.Quit();
        #endif
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
