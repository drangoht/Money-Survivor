using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { MainMenu, Playing, Paused, LevelUp, GameOver }

/// <summary>
/// Singleton GameManager. Drives the game-state machine and tracks
/// global stats (time survived, enemies killed).
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Runtime Stats")]
    public float TimeSurvived   { get; private set; }
    public int   EnemiesKilled  { get; private set; }
    public int   CurrentLevel   { get; private set; } = 1;

    public GameState State { get; private set; } = GameState.MainMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        EventBus.OnEnemyKilled += HandleEnemyKilled;
        EventBus.OnPlayerDeath += HandlePlayerDeath;
        EventBus.OnPlayerLevelUp += HandleLevelUp;
    }

    private void OnDisable()
    {
        EventBus.OnEnemyKilled -= HandleEnemyKilled;
        EventBus.OnPlayerDeath -= HandlePlayerDeath;
        EventBus.OnPlayerLevelUp -= HandleLevelUp;
    }

    private void Update()
    {
        if (State == GameState.Playing)
        {
            TimeSurvived += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Escape)) PauseGame();
        }
        else if (State == GameState.Paused)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) ResumeGame();
        }
    }

    // ── State transitions ────────────────────────────────────────────────────

    public void StartGame()
    {
        TimeSurvived  = 0f;
        EnemiesKilled = 0;
        CurrentLevel  = 1;
        SetState(GameState.Playing);
        EventBus.RaiseGameStart();
        SceneManager.LoadScene("Game");
    }

    public void PauseGame()
    {
        if (State != GameState.Playing) return;
        SetState(GameState.Paused);
        Time.timeScale = 0f;
        EventBus.RaiseGamePaused();
    }

    public void ResumeGame()
    {
        if (State != GameState.Paused && State != GameState.LevelUp) return;
        SetState(GameState.Playing);
        Time.timeScale = 1f;
        EventBus.RaiseGameResumed();
    }

    public void EnterLevelUp()
    {
        SetState(GameState.LevelUp);
        Time.timeScale = 0f;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        EventBus.ClearAll();
        SetState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }

    // ── Event handlers ───────────────────────────────────────────────────────

    private void HandleEnemyKilled(Vector3 _, int __) => EnemiesKilled++;
    private void HandlePlayerDeath()                  => SetState(GameState.GameOver);
    private void HandleLevelUp(int lvl)              => CurrentLevel = lvl;

    private void SetState(GameState newState)
    {
        State = newState;
        Debug.Log($"[GameManager] State → {newState}");
    }
}
