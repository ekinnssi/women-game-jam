using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;

    // hikaye durumlari
    public enum StoryState
    {
        Day1_WakeUp_CheckMirror,
        Day1_DoorKnocking,
        Day1_TalkCin,
        Day1_GoToBed,
        Day1_Completed
    }

    public StoryState currentState;

    [Header("Tetikleyiciler")]
    [SerializeField] private SimpleTriggerInteract mirrorTrigger;
    [SerializeField] private SimpleTriggerInteract doorTrigger;
    [SerializeField] private SimpleTriggerInteract bedTrigger;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // baslangic akisi
        currentState = StoryState.Day1_WakeUp_CheckMirror;

        if (mirrorTrigger != null) mirrorTrigger.isLocked = false;
        if (doorTrigger != null) doorTrigger.isLocked = false;
        if (bedTrigger != null) bedTrigger.isLocked = false;
    }

    // etkilesim izni
    public bool CanInteract(PanelManager.PanelType type)
    {
        switch (type)
        {
            case PanelManager.PanelType.Mirror:
                return currentState == StoryState.Day1_WakeUp_CheckMirror;
            case PanelManager.PanelType.Door:
                return currentState == StoryState.Day1_DoorKnocking || currentState == StoryState.Day1_TalkCin;
            case PanelManager.PanelType.Bed:
                return currentState == StoryState.Day1_GoToBed;
            default:
                return false;
        }
    }

    // hikaye ilerlemesi
    public void AdvanceStory()
    {
        switch (currentState)
        {
            case StoryState.Day1_WakeUp_CheckMirror:
                // kapi calmasi
                currentState = StoryState.Day1_DoorKnocking;
                if (doorTrigger != null) doorTrigger.isLocked = false;

                // kapi bildirimi
                if (PanelManager.Instance != null)
                {
                    PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                    {
                        "[shake]*GÜM GÜM GÜM!*[/shake]\nBirisi kapıyı çalıyor...",
                        "Bu saatte kim olabilir ki? Giriş kapısına [tremble]bakmalıyım[/tremble]."
                    });
                }
                break;

            case StoryState.Day1_DoorKnocking:
            case StoryState.Day1_TalkCin:
                // cinle konusuldu
                currentState = StoryState.Day1_GoToBed;
                if (bedTrigger != null) bedTrigger.isLocked = false;

                // yatak bildirimi
                if (PanelManager.Instance != null)
                {
                    PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                    {
                        "Kapının altından [wave]soğuk bir esinti[/wave] geçti...",
                        "Çok yorgunum. Artık [tremble]yatağa gidip uyumalıyım[/tremble]."
                    });
                }
                break;

            case StoryState.Day1_GoToBed:
                // gun tamamlama
                currentState = StoryState.Day1_Completed;

                if (PanelManager.Instance != null)
                {
                    PanelManager.Instance.ShowSleepScreen();
                }
                break;
        }
    }
}
