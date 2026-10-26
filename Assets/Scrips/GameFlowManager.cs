using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;

    public enum StoryState
    {
        Day1_Mirror, Day1_Door, Day1_Bed,
        Day2_Mirror, Day2_Door, Day2_Bed,
        Day3_MirrorMorning, Day3_Phone, Day3_MirrorPanic, Day3_Door, Day3_Bed,
        Day4_Phone, Day4_MirrorHappy, GameEnded
    }

    public StoryState currentState = StoryState.Day1_Mirror;

    [Header("Tetikleyiciler")]
    [SerializeField] private SimpleTriggerInteract mirrorTrigger;
    [SerializeField] private SimpleTriggerInteract doorTrigger;
    [SerializeField] private SimpleTriggerInteract bedTrigger;
    [SerializeField] private SimpleTriggerInteract phoneTrigger;

    [Header("Gorseller")]
    [SerializeField] private Image cornerPortraitImage;
    [SerializeField] private Sprite normalPortraitSprite;
    [SerializeField] private Sprite prettyPortraitSprite;

    [SerializeField] private Image mirrorCharacterImage;
    [SerializeField] private Sprite mirrorUglySprite;
    [SerializeField] private Sprite mirrorPrettySprite;
    [SerializeField] private Sprite mirrorNormalHappySprite;

    [SerializeField] private SpriteRenderer windowRenderer;
    [SerializeField] private Sprite normalWindowSprite;
    [SerializeField] private Sprite stalkerWindowSprite;
    [SerializeField] private GameObject stalkerWindowObject;

    [Header("Sesler")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip doorKnockClip;
    [SerializeField] private AudioClip phoneNotifyClip;
    [SerializeField] private AudioClip cameraFlashNoiseClip;

    [Header("Bitis Sahnesi Adi")]
    [SerializeField] private string endSceneName = "EndScene";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentState = StoryState.Day1_Mirror;
        UnlockAllTriggers();
        ApplyVisuals(isPretty: false, mirrorState: 0, stalkerWindow: false);
    }

    private void UnlockAllTriggers()
    {
        if (mirrorTrigger != null) mirrorTrigger.isLocked = false;
        if (doorTrigger != null) doorTrigger.isLocked = false;
        if (bedTrigger != null) bedTrigger.isLocked = false;
        if (phoneTrigger != null) phoneTrigger.isLocked = false;
    }

    private void ApplyVisuals(bool isPretty, int mirrorState, bool stalkerWindow)
    {
        if (cornerPortraitImage != null)
            cornerPortraitImage.sprite = isPretty ? prettyPortraitSprite : normalPortraitSprite;

        if (mirrorCharacterImage != null)
        {
            if (mirrorState == 0 && mirrorUglySprite != null) mirrorCharacterImage.sprite = mirrorUglySprite;
            else if (mirrorState == 1 && mirrorPrettySprite != null) mirrorCharacterImage.sprite = mirrorPrettySprite;
            else if (mirrorState == 2 && mirrorNormalHappySprite != null) mirrorCharacterImage.sprite = mirrorNormalHappySprite;
        }

        if (windowRenderer != null && normalWindowSprite != null)
            windowRenderer.sprite = stalkerWindow ? stalkerWindowSprite : normalWindowSprite;

        if (stalkerWindowObject != null)
            stalkerWindowObject.SetActive(stalkerWindow);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    public void InteractDoor() { StartCurrentStateInteraction(PanelManager.PanelType.Door); }
    public void InteractBed() { StartCurrentStateInteraction(PanelManager.PanelType.Bed); }
    public void InteractMirror() { StartCurrentStateInteraction(PanelManager.PanelType.Mirror); }
    public void InteractPhone() { StartCurrentStateInteraction(PanelManager.PanelType.Phone); }

    public bool CanInteract(PanelManager.PanelType type)
    {
        switch (currentState)
        {
            case StoryState.Day1_Mirror:
            case StoryState.Day2_Mirror:
            case StoryState.Day3_MirrorMorning:
            case StoryState.Day3_MirrorPanic:
            case StoryState.Day4_MirrorHappy:
                return type == PanelManager.PanelType.Mirror;

            case StoryState.Day1_Door:
            case StoryState.Day2_Door:
            case StoryState.Day3_Door:
                return type == PanelManager.PanelType.Door;

            case StoryState.Day3_Phone:
            case StoryState.Day4_Phone:
                return type == PanelManager.PanelType.Phone;

            case StoryState.Day1_Bed:
            case StoryState.Day2_Bed:
            case StoryState.Day3_Bed:
                return type == PanelManager.PanelType.Bed;

            default:
                return false;
        }
    }

    public void StartCurrentStateInteraction(PanelManager.PanelType type)
    {
        if (PanelManager.Instance == null) return;
        if (!CanInteract(type)) return;

        switch (currentState)
        {
            case StoryState.Day1_Mirror:
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.Mirror, new string[]
                {
                    "Ugh... Look at me. I'm so [tremble]ugly[/tremble].",
                    "No matter what I do, I can't bear looking at this face..."
                }, AdvanceStory);
                break;

            case StoryState.Day1_Door:
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.Door, new string[]
                {
                    "Hello little one... I heard your wish.",
                    "I can give you the beauty you desire. [wave]Do you trust me?[/wave]"
                }, AdvanceStory, PanelManager.ChoiceMode.YesOnly_QuestionLocked, "Hehehe... Just wait and see tomorrow.");
                break;

            case StoryState.Day1_Bed:
                AdvanceStory();
                break;

            case StoryState.Day2_Mirror:
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.Mirror, new string[]
                {
                    "Wait... [bounce]What happened to me?![/bounce]",
                    "Was that thing at the door real?! I look... gorgeous!"
                }, AdvanceStory);
                break;

            case StoryState.Day2_Door:
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.Door, new string[]
                {
                    "You like your new face, don't you?",
                    "Why stop here? I can give you fame, adoration, millions of fans... [wave]Do you want it?[/wave]"
                }, AdvanceStory, PanelManager.ChoiceMode.YesOnly_QuestionLocked, "Wonderful... Everyone will know your name.");
                break;

            case StoryState.Day2_Bed:
                AdvanceStory();
                break;

            case StoryState.Day3_MirrorMorning:
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.Mirror, new string[]
                {
                    "Look at this perfection. Everyone loves me now!",
                    "I am the center of the world."
                }, AdvanceStory);
                break;

            case StoryState.Day3_Phone:
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.Phone, new string[]
                {
                    "Hundreds of messages... 'I'm watching you'...",
                    "[tremble]What is this?! They are all creeps! I'm scared...[/tremble]"
                }, AdvanceStory);
                break;

            case StoryState.Day3_MirrorPanic:
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.Mirror, new string[]
                {
                    "[tremble]I don't want this life anymore! Please make it stop![/tremble]",
                    "There are people outside my window... I hate this face!"
                }, AdvanceStory);
                break;

            case StoryState.Day3_Door:
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.Door, new string[]
                {
                    "Look how popular you are! Don't you want [shake]MORE[/shake]?",
                    "I can give you even more attention! Say yes!"
                }, AdvanceStory, PanelManager.ChoiceMode.NoOnly_YesLocked, "...Fine. Suit yourself.");
                break;

            case StoryState.Day3_Bed:
                AdvanceStory();
                break;

            case StoryState.Day4_Phone:
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                {
                    "Let me check my phone...",
                    "Wait... my followers are all gone! The stalkers... they aren't here today!"
                }, AdvanceStory);
                break;

            case StoryState.Day4_MirrorHappy:
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.Mirror, new string[]
                {
                    "[bounce]I'm back to my old self![/bounce]",
                    "I missed my real face so much... I'm finally happy."
                }, AdvanceStory);
                break;
        }
    }

    public void AdvanceStory()
    {
        UnlockAllTriggers();

        switch (currentState)
        {
            case StoryState.Day1_Mirror:
                currentState = StoryState.Day1_Door;
                PlaySound(doorKnockClip);
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                {
                    "[shake]*KNOCK KNOCK KNOCK*[/shake]",
                    "Who could that be? Nobody ever visits me..."
                });
                break;

            case StoryState.Day1_Door:
                currentState = StoryState.Day1_Bed;
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                {
                    "What was that...? Anyway, I'm exhausted. I should go to bed."
                });
                break;

            case StoryState.Day1_Bed:
                PanelManager.Instance.ShowSleepScreen(
                    onMidSleep: () =>
                    {
                        currentState = StoryState.Day2_Mirror;
                        if (CameraController.Instance != null) { CameraController.Instance.GoToRoom(1); }
                    },
                    onWakeUp: () =>
                    {
                        ApplyVisuals(isPretty: true, mirrorState: 1, stalkerWindow: false);
                        PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                        {
                            "Ugh, what a strange night... Let me wash my face."
                        });
                    });
                break;

            case StoryState.Day2_Mirror:
                currentState = StoryState.Day2_Door;
                PlaySound(doorKnockClip);
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                {
                    "[shake]*KNOCK KNOCK KNOCK*[/shake]\nThe door again!"
                });
                break;

            case StoryState.Day2_Door:
                currentState = StoryState.Day2_Bed;
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                {
                    "Fame and fans... Time to sleep."
                });
                break;

            case StoryState.Day2_Bed:
                PanelManager.Instance.ShowSleepScreen(
                    onMidSleep: () =>
                    {
                        currentState = StoryState.Day3_MirrorMorning;
                        if (CameraController.Instance != null) { CameraController.Instance.GoToRoom(1); }
                    },
                    onWakeUp: () =>
                    {
                        ApplyVisuals(isPretty: true, mirrorState: 1, stalkerWindow: false);
                        PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                        {
                            "Ah, another morning. Let's look at the mirror."
                        });
                    });
                break;

            case StoryState.Day3_MirrorMorning:
                currentState = StoryState.Day3_Phone;
                PlaySound(phoneNotifyClip);
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                {
                    "*BZZT! BZZT!* My phone is blowing up!"
                });
                break;

            case StoryState.Day3_Phone:
                currentState = StoryState.Day3_MirrorPanic;
                ApplyVisuals(isPretty: true, mirrorState: 1, stalkerWindow: false);
                PlaySound(cameraFlashNoiseClip);

                // Diyalog tamamen kapandıktan SONRA pencere krizi başlar
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                {
                    "[shake]*CLICK! FLASH!*[/shake]\nWhat's that noise outside?!",
                    "[tremble]Cameras at my window! I need to run to the bathroom![/tremble]"
                }, () =>
                {
                    if (PencereKriziManager.Instance != null)
                    {
                        PencereKriziManager.Instance.KriziBaslat();
                    }
                });
                break;

            case StoryState.Day3_MirrorPanic:
                currentState = StoryState.Day3_Door;
                ApplyVisuals(isPretty: true, mirrorState: 1, stalkerWindow: false);
                PlaySound(doorKnockClip);
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                {
                    "[shake]*KNOCK KNOCK KNOCK*[/shake]\nHe is at the door..."
                });
                break;

            case StoryState.Day3_Door:
                currentState = StoryState.Day3_Bed;
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                {
                    "I finally said NO... I just want to sleep."
                });
                break;

            case StoryState.Day3_Bed:
                PanelManager.Instance.ShowSleepScreen(
                    onMidSleep: () =>
                    {
                        currentState = StoryState.Day4_Phone;
                        if (CameraController.Instance != null) { CameraController.Instance.GoToRoom(1); }
                    },
                    onWakeUp: () =>
                    {
                        ApplyVisuals(isPretty: false, mirrorState: 2, stalkerWindow: false);
                        PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                        {
                            "It's so quiet today..."
                        });
                    });
                break;

            case StoryState.Day4_Phone:
                currentState = StoryState.Day4_MirrorHappy;
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                {
                    "Let me check the bathroom mirror quickly!"
                });
                break;

            case StoryState.Day4_MirrorHappy:
                currentState = StoryState.GameEnded;
                PanelManager.Instance.ShowSleepScreen(
                    onMidSleep: () =>
                    {
                        if (!string.IsNullOrEmpty(endSceneName))
                        {
                            SceneManager.LoadScene(endSceneName);
                        }
                    });
                break;
        }
    }
}