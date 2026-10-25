using UnityEngine;

public class InspectPanel : MonoBehaviour
{
    // yoneticiye iletim
    public void OpenPanel(string[] lines, System.Action onComplete = null)
    {
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.OpenInspect(PanelManager.PanelType.Mirror, lines, onComplete);
        }
    }

    public void ClosePanel()
    {
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.CloseCurrentPanel();
        }
    }
}
