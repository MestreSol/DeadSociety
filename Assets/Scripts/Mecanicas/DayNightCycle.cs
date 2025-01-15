using System;
using UnityEditor;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
  [Header("Day/Night Cycle Settings")]
  public float dayDuration = 300f; // Duração de um dia em segundos (5 minutos por padrão)
  private float currentDayTime = 0f;

  public GameObject directionalLight; // Luz principal para simular o Sol
  [Range(0, 1)] public float initialDayProgress = 0f; // Progresso inicial do dia (0 = amanhecer)


  private void Start()
  {
    currentDayTime = initialDayProgress * dayDuration;

    // Obtém referência ao GameManager (opcional, mas útil para integração)
  }

  private void Update()
  {
    if (GameManager.Instance != null && GameManager.Instance.CurrentGameState == GameManager.GameState.Playing)
    {
      UpdateDayNightCycle();
    }
  }

  private void UpdateDayNightCycle()
  {
    // Atualiza o tempo do ciclo
    currentDayTime += Time.deltaTime;

    if (currentDayTime >= dayDuration)
    {
      currentDayTime = 0f;

      // Notifica o GameManager que um novo dia começou
      GameStatistics.Instance.IncrementDaysSurvived();
    }

    // Atualiza a rotação da luz direcional (Sol)
    if (directionalLight != null)
    {
      float dayProgress = currentDayTime / dayDuration; // Progresso do dia (0 a 1)
      directionalLight.transform.rotation = Quaternion.Euler(new Vector3(dayProgress * 360f - 90f, 170f, 0f));
      Debug.Log($"Day Hour:{CurrentTime()}");
    }
  }

  public string CurrentTime()
  {
    int hours = Mathf.FloorToInt(currentDayTime / 3600);
    int minutes = Mathf.FloorToInt((currentDayTime % 3600) / 60);
    int seconds = Mathf.FloorToInt(currentDayTime % 60);
    return $"{hours:00}:{minutes:00}:{seconds:00}";
  }

 public void DrawnGizmos()
 {
   if(directionalLight != null)
   {
     Handles.Label(directionalLight.transform.position, $"Day Time: {CurrentTime()}");
   }
 }
}
