using System;
using UnityEngine;

namespace Core
{
    public class GameStatistics : MonoBehaviour
    {
        public static GameStatistics Instance { get; private set; }

        private int daysSurvived = 2;
        
        public event Action<int> OnDaysSurvivedChanged;

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public void IncrementDaysSurvived()
        {
            daysSurvived++;
            Debug.Log($"Dias Sobrevividos: {daysSurvived}");
            OnDaysSurvivedChanged?.Invoke(daysSurvived);
        }
        
        public int GetDaysSurvived() => daysSurvived;
        
        public void ResetStatistics()
        {
            daysSurvived = 0;
            OnDaysSurvivedChanged?.Invoke(daysSurvived);
            
            Debug.Log("Estatísticas resetadas");
        }
    }
}