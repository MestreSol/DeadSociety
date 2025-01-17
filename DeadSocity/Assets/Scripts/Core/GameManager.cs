using System;
using UnityEngine;

public enum GameState
{
    MainMenu,
    GamePlay,
    Pause,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public event Action<GameState> OnGameStateChanged;
    public GameState currentGameState;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }
    public void ChangeState(GameState state)
    {
        if (currentGameState == state) return;
        
        currentGameState = state;
        OnGameStateChanged?.Invoke(state);

        Time.timeScale = state switch
        {
            GameState.MainMenu => 0f,
            GameState.Pause => 0f,
            GameState.GameOver => 0f,
            GameState.GamePlay => 1f,
            _ => Time.timeScale
        };
        
        Debug.Log($"Estado do jogo alterado para: {state}");
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);
        Debug.Log("Game Over");
    }
}
