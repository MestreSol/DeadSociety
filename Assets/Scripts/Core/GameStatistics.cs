using System;
using UnityEngine;

public class GameStatistics : MonoBehaviour
{
  public static GameStatistics Instance { get; private set; }

  private int daysSurvived = 0;
  private int enemiesDefeated = 0;
  private int resourcesCollected = 0;

  public event Action<int> OnDaysSurvivedChanged;
  public event Action<int> OnEnemiesDefeatedChanged;
  public event Action<int> OnResourcesCollectedChanged;

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

  public void IncrementDaysSurvived()
  {
    daysSurvived++;
    Debug.Log($"Dias Sobrevividos: {daysSurvived}");
    OnDaysSurvivedChanged?.Invoke(daysSurvived);
  }

  public void IncrementEnemiesDefeated()
  {
    enemiesDefeated++;
    Debug.Log($"Inimigos Derrotados: {enemiesDefeated}");
    OnEnemiesDefeatedChanged?.Invoke(enemiesDefeated);
  }

  public void IncrementResourcesCollected()
  {
    resourcesCollected++;
    Debug.Log($"Recursos Coletados: {resourcesCollected}");
    OnResourcesCollectedChanged?.Invoke(resourcesCollected);
  }

  public int GetDaysSurvived() => daysSurvived;
  public int GetEnemiesDefeated() => enemiesDefeated;
  public int GetResourcesCollected() => resourcesCollected;

  public void ResetStatistics()
  {
    daysSurvived = 0;
    enemiesDefeated = 0;
    resourcesCollected = 0;

    OnDaysSurvivedChanged?.Invoke(daysSurvived);
    OnEnemiesDefeatedChanged?.Invoke(enemiesDefeated);
    OnResourcesCollectedChanged?.Invoke(resourcesCollected);

    Debug.Log("Estatísticas Resetadas");
  }
}
