using System;
using UnityEngine;

public class GameStatistics : MonoBehaviour
{

  public static GameStatistics Instance {get; private set;}

  private int daysSurvived = 0;
  private int enemiesDefeated = 0;
  private int resourcesCollected = 0;

  private void Awake()
  {
    if(Instance != null && Instance != this)
    {
      Destroy(gameObject);
    } else {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }
  }
  public void IncrementDaysSurvived()
  {
    daysSurvived++;
    Debug.Log($"Dias Sobrevividos: {daysSurvived}");
  }

  public void IncrementEnemiesDefeated()
  {
    enemiesDefeated++;
    Debug.Log($"Inimigos Derrotados: {enemiesDefeated}");
  }

  public void IncrementResourcesCollected()
  {
    resourcesCollected++;
    Debug.Log($"Recursos Coletados: {resourcesCollected}");
  }

  public int GetDaysSurvived() => daysSurvived;
  public int GetEnemiesDefeated() => enemiesDefeated;
  public int GetResourcesCollected() => resourcesCollected;

  public void ResetStatistics()
  {
    daysSurvived = 0;
    enemiesDefeated = 0;
    resourcesCollected = 0;
    Debug.Log("Estatísticas Resetadas");
  }
}
