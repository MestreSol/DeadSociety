using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  public static GameManager Instance { get; private set; }

  public enum GameState { Menu, Playing, Paused, GameOver }
  public GameState CurrentGameState { get; private set; }

  public event Action<GameState> OnGameStateChanged;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
  }

  private void Start()
  {
    ChangeState(GameState.Playing);
  }

  private void Update()
  {
    HandleInput();
  }

  private void HandleInput()
  {
    if (Input.GetKeyDown(KeyCode.Escape) && CurrentGameState != GameState.Menu)
    {
      if (CurrentGameState == GameState.Playing)
        ChangeState(GameState.Paused);
      else if (CurrentGameState == GameState.Paused)
        ChangeState(GameState.Playing);
    }
  }

  public void ChangeState(GameState newState)
  {
    if (CurrentGameState == newState) return;

    CurrentGameState = newState;
    OnGameStateChanged?.Invoke(newState);

    Time.timeScale = newState switch
    {
      GameState.Menu => 0f,
      GameState.Paused => 0f,
      GameState.GameOver => 0f,
      GameState.Playing => 1f,
      _ => Time.timeScale
    };

    Debug.Log($"Estado do jogo alterado para: {newState}");
  }

  public void GameOver()
  {
    ChangeState(GameState.GameOver);
    Debug.Log("Game Over");
  }
}
