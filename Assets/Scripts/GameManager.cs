using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public int levelID;

    [Header("Game State")]
    public UnityEvent onGameStart = new UnityEvent();
    public UnityEvent onGameOver = new UnityEvent();
    public UnityEvent onGameWin = new UnityEvent();

    public bool gameIsOver { get; private set; }
    public bool gameHasStarted { get; private set; }
    public bool GameIsPlaying => !gameIsOver && gameHasStarted;
    public bool autoStart = true;

    [Header("Level Time")]
    float levelStartTime = -1;
    float levelEndTime = -1;
    public Vector2 starThresholds;
    public float LevelTime
    {
        get
        {
            if (levelStartTime < 0)
                return 0;

            if (levelEndTime < 0)
                return Time.time - levelStartTime;

            return levelEndTime - levelStartTime;
        }
    }
    int fishCount;
    public const string PLAYER_PREF_ID = "{ID}";
    public const string PLAYER_PREF_KEY = "LevelStars" + PLAYER_PREF_ID;

    public static GameManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        fishCount = FindObjectsOfType<FishController>().Length;

        if (autoStart)
            StartGame();
        else
        {
            MusicManager.Instance.SetMode(MusicManager.Mode.MainMenu);
        }
    }

    public void StartGame()
    {
        if (gameHasStarted)
            return;
        gameHasStarted = true;
        MusicManager.Instance.SetMode(MusicManager.Mode.Default);
        onGameStart.Invoke();
        levelStartTime = Time.time;
    }

    public void GameOver()
    {
        if (gameIsOver)
            return;
        gameIsOver = true;
        onGameOver.Invoke();
    }

    [ContextMenu("Clear Player Pref")]
    public void ClearPlayerPref()
    {
        PlayerPrefs.DeleteAll();
    }

    public void ClearLevel(bool cancelStars = false)
    {
        if (gameIsOver)
            return;

        if (cancelStars)
            levelEndTime = Time.time + 100;
        else
            levelEndTime = Time.time;
        Debug.Log($"Level Cleared in {LevelTime}!");
        gameIsOver = true;
        onGameWin.Invoke();
        MusicManager.Instance.SetMode(MusicManager.Mode.MainMenu);
        SFXManager.PlaySound(GlobalSFX.Win);

        UpdateHighscore();
    }

    void UpdateHighscore()
    {
        string key = PLAYER_PREF_KEY.Replace(PLAYER_PREF_ID, levelID.ToString());
        int savedScore = PlayerPrefs.GetInt(key, 0);
        int newScore = GetStarsObtained();
        if (newScore > savedScore)
            PlayerPrefs.SetInt(key, newScore);
    }

    public void OnFishDie(FishController controller)
    {
        fishCount--;

        if (fishCount <= 0)
        {
            ClearLevel();
        }
    }

    public int GetStarsObtained()
    {
        if (LevelTime < starThresholds.y)
            return 3;
        if (LevelTime < starThresholds.x)
            return 2;
        return 1;
    }
    public int GetStarsObtained(out float nextStarTime)
    {
        if (LevelTime < starThresholds.y)
        {
            nextStarTime = 0;
            return 3;
        }
        if (LevelTime < starThresholds.x)
        {
            nextStarTime = starThresholds.y;
            return 2;
        }

        nextStarTime = starThresholds.x;
        return 1;
    }
}
