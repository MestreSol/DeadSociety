using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
  [Header("Day/Night Cycle Settings")]
  [Tooltip("Duração de um dia em segundos.")]
  [SerializeField] private float dayDuration = 300f;
  [Range(0, 1)] [SerializeField] private float initialDayProgress = 0f;

  [Header("Lighting")]
  [SerializeField] private Light directionalLight;

  private float currentDayTime;

  public delegate void DayNightEvent();
  public static event DayNightEvent OnNewDayStarted;

  private void Start()
  {
    currentDayTime = initialDayProgress * dayDuration;
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
    currentDayTime += Time.deltaTime;

    if (currentDayTime >= dayDuration)
    {
      currentDayTime = 0f;
      OnNewDayStarted?.Invoke(); // Dispara evento para outros sistemas
    }

    UpdateLightRotation();
  }

  private void UpdateLightRotation()
  {
    if (directionalLight == null) return;

    float dayProgress = currentDayTime / dayDuration;
    float sunAngle = dayProgress * 360f - 90f;
    directionalLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
  }

  public string GetCurrentTime()
  {
    int hours = Mathf.FloorToInt(currentDayTime / 3600);
    int minutes = Mathf.FloorToInt((currentDayTime % 3600) / 60);
    int seconds = Mathf.FloorToInt(currentDayTime % 60);
    return $"{hours:00}:{minutes:00}:{seconds:00}";
  }
}
