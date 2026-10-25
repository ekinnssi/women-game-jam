using UnityEngine;

public class TaskInteraction : MonoBehaviour
{
    public DayManager dayManager;
    private bool isTaskDone = false; // Görevin tekrar yapýlmasýný engeller

    private void OnMouseDown()
    {
        if (!isTaskDone && dayManager != null)
        {
            dayManager.CompleteTask();
            isTaskDone = true;
            Debug.Log(gameObject.name + " görevi tamamlandý!");
        }
    }
}