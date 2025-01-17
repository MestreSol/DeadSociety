using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    public enum GameState { Menu, Playing, Pause, GameOver }
    public GameState CurrentGameState {get; private set;}

    public delegate void GameStateChanged(GameState newGameState);
    public static event GameStateChanged OnGameStateChanged;

    private void Awake(){
      if(Instance != null && Instance != this){
        Destroy(gameObject);
      } else {
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
      if (Input.GetKeyDown(KeyCode.Escape) && CurrentGameState != GameState.Menu)
      {
        if (CurrentGameState == GameState.Playing)
        {
          ChangeState(GameState.Pause);
        }
        else if(CurrentGameState == GameState.Pause)
        {
          ChangeState(GameState.Playing);
        }
      }
    }

    public void ChangeState(GameState newState)
    {
      CurrentGameState = newState;
      OnGameStateChanged?.Invoke(newState);

      switch (newState)
      {
        case GameState.Menu:
        case GameState.Pause:
        case GameState.GameOver:
          Time.timeScale = 0f;
          break;
        case GameState.Playing:
          Time.timeScale = 1f;
          break;
      }
    }

    public void GameOver()
    {
      Debug.Log("Game Over");
      ChangeState(GameState.GameOver);
    }
}
