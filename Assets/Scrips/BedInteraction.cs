using UnityEngine;
using UnityEngine.EventSystems;

public class BedInteraction : MonoBehaviour
{
    private void OnMouseDown()
    {
        // UI týklamalarýný, açýk panelleri ve diyalog kapandýktan sonraki ilk 0.3 saniyelik seri týklamayý engelle
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (PanelManager.Instance != null && PanelManager.Instance.IsPanelOpen) return;
        if (Time.unscaledTime - PanelManager.lastPanelCloseTime < 0.3f) return;

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.InteractBed();
        }
    }
}