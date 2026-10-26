using UnityEngine;

public class TaskInteraction : MonoBehaviour
{
    private bool isTaskDone = false; // Görevin tekrar yapýlmasýný engeller

    private void OnMouseDown()
    {
        if (!isTaskDone)
        {
            if (GameFlowManager.Instance != null)
            {
                // Göreve týklandýðýnda hikayeyi bir sonraki aþamaya geçir
                GameFlowManager.Instance.AdvanceStory();
            }

            isTaskDone = true;
            Debug.Log(gameObject.name + " görevi tamamlandý!");
        }
    }
}