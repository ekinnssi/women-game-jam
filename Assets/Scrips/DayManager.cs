using UnityEngine;
using UnityEngine.UI;

public class DayManager : MonoBehaviour
{
    public Image characterPortrait;
    public Sprite[] dailyPortraits;

    public int[] tasksRequiredPerDay; // Her gün için gereken görev sayısı

    private int currentDay = 0;
    private int tasksCompletedToday = 0;

    void Start()
    {
        UpdatePortrait();
    }

    // Herhangi bir görev yapıldığında bu fonksiyonu çağıracaksın
    public void CompleteTask()
    {
        tasksCompletedToday++;
        Debug.Log("Görev yapıldı! Bugün yapılan: " + tasksCompletedToday);
    }

    public void SleepAndProgressDay()
    {
        // O günkü gereken görev sayısına ulaşıldı mı kontrolü
        if (tasksCompletedToday >= tasksRequiredPerDay[currentDay])
        {
            if (currentDay < dailyPortraits.Length - 1)
            {
                currentDay++;
                tasksCompletedToday = 0; // Yeni gün için görevleri sıfırla
                UpdatePortrait();
            }
        }
        else
        {
            Debug.Log("Henüz uyuyamazsın, tüm görevleri bitirmedin!");
        }
    }

    private void UpdatePortrait()
    {
        if (dailyPortraits[currentDay] != null)
        {
            characterPortrait.sprite = dailyPortraits[currentDay];
        }
    }
}