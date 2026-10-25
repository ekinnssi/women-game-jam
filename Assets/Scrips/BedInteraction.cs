using UnityEngine;

public class BedInteraction : MonoBehaviour
{
    public DayManager dayManager;

    // Fare ile yataðýn üzerindeki Box Collider alanýna týklandýðýnda çalýþýr
    private void OnMouseDown()
    {
        if (dayManager != null)
        {
            dayManager.SleepAndProgressDay();
        }
    }
}