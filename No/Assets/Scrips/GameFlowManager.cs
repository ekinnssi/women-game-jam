using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;

    public enum StoryState
    {
        Day1_WakeUp_CheckMirror,
        Day1_DoorKnocking,
        Day1_TalkCin,
        Day1_GoToBed
    }

    public StoryState currentState;

    [Header("Tetikleyiciler")]
    [SerializeField] private SimpleTriggerInteract mirrorTrigger;
    [SerializeField] private SimpleTriggerInteract doorTrigger; // Giriş kapısı (Cin)
    [SerializeField] private SimpleTriggerInteract bedTrigger;  // Yatak

    private void Awake() => Instance = this;

    private void Start()
    {
        // 1. Gün Başlangıç Durumu
        currentState = StoryState.Day1_WakeUp_CheckMirror;
        
        // Sadece aynaya basılabilir, diğerleri kilitli
        mirrorTrigger.isLocked = false;
        doorTrigger.isLocked = true;
        bedTrigger.isLocked = true;
    }

    public void AdvanceStory()
    {
        switch (currentState)
        {
            case StoryState.Day1_WakeUp_CheckMirror:
                // Aynadan çıktık -> Kapı çalar
                currentState = StoryState.Day1_DoorKnocking;
                Debug.Log("KAPI ÇALIYOR! (Tık tık tık)");
                
                // Kapının E'sini aç
                doorTrigger.isLocked = false;
                break;

            case StoryState.Day1_DoorKnocking:
                // Cinle konuşuldu -> Yatağın E'si açılır
                currentState = StoryState.Day1_GoToBed;
                bedTrigger.isLocked = false;
                break;
        }
    }
}