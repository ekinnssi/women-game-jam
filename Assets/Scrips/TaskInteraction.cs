using UnityEngine;
using UnityEngine.EventSystems;

public class TaskInteraction : MonoBehaviour
{
    private void OnMouseDown()
    {
        // Fare bir UI butonunun (YES vb.) üzerindeyse veya diyalog paneli açýksa týklamayý engelle
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (PanelManager.Instance != null && PanelManager.Instance.IsPanelOpen) return;

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.InteractDoor();
        }
    }
}