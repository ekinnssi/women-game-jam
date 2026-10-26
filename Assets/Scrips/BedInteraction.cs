using UnityEngine;

public class BedInteraction : MonoBehaviour
{
    // Fare ile yataðýn üzerindeki Box Collider alanýna týklandýðýnda çalýþýr
    private void OnMouseDown()
    {
        if (GameFlowManager.Instance != null)
        {
            // Eski uyku sistemi yerine yeni hikaye akýþýný tetikle
            GameFlowManager.Instance.AdvanceStory();
        }
    }
}