using UnityEngine;
using UnityEngine.InputSystem; // 1. Yeni kütüphaneyi ekle

public class SimpleTriggerInteract : MonoBehaviour
{
    [SerializeField] private GameObject ePromptUI;
    [SerializeField] private InspectPanel targetInspectPanel;
    
    [Header("Bu etkileşimdeki diyalog")]
    [TextArea(2, 4)]
    [SerializeField] private string[] dialogue;

    private bool isPlayerNearby = false;
    public bool isLocked = false;

    private void Start()
    {
        if (ePromptUI != null) ePromptUI.SetActive(false);
    }

    private void Update()
    {
        // 2. Yeni Input System kontrolü: Keyboard.current.eKey.wasPressedThisFrame
        if (isPlayerNearby && !isLocked && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (ePromptUI != null) ePromptUI.SetActive(false);

            if (targetInspectPanel != null)
            {
                targetInspectPanel.OpenPanel(dialogue, OnDialogueFinished);
            }
        }
    }

    private void OnDialogueFinished()
    {
        GameFlowManager.Instance.AdvanceStory();
        isLocked = true; 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isLocked)
        {
            isPlayerNearby = true;
            if (ePromptUI != null) ePromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (ePromptUI != null) ePromptUI.SetActive(false);
        }
    }
}
