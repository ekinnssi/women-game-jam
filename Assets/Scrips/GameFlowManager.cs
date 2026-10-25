using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;

    public enum StoryState
    {
        // GUN 1
        Day1_Mirror,
        Day1_Door,
        Day1_Bed,

        // GUN 2
        Day2_Mirror,
        Day2_Door,
        Day2_Bed,

        // GUN 3
        Day3_MirrorMorning,
        Day3_Phone,
        Day3_MirrorPanic,
        Day3_Door,
        Day3_Bed,

        // GUN 4
        Day4_Phone,
        Day4_MirrorHappy,
        GameEnded
    }

    public StoryState currentState = StoryState.Day1_Mirror;

    [Header("Tetikleyiciler (Sahneden Surukle)")]
    [SerializeField] private SimpleTriggerInteract mirrorTrigger;
    [SerializeField] private SimpleTriggerInteract doorTrigger;
    [SerializeField] private SimpleTriggerInteract bedTrigger;
    [SerializeField] private SimpleTriggerInteract phoneTrigger;

    [Header("Kose Karakter Portresi (Normal / Guzel)")]
    [SerializeField] private Image cornerPortraitImage;
    [SerializeField] private Sprite normalPortraitSprite;
    [SerializeField] private Sprite prettyPortraitSprite;

    [Header("Ayna Paneli Icindeki Kiz Gorseli")]
    [SerializeField] private Image mirrorCharacterImage;
    [SerializeField] private Sprite mirrorUglySprite;
    [SerializeField] private Sprite mirrorPrettySprite;
    [SerializeField] private Sprite mirrorNormalHappySprite;

    [Header("Yatak Odasi Pencere / Cam Gorseli (3. Gun)")]
    [SerializeField] private SpriteRenderer windowRenderer;
    [SerializeField] private Sprite normalWindowSprite;
    [SerializeField] private Sprite stalkerWindowSprite;
    [SerializeField] private GameObject stalkerWindowObject; // Eger ayri bir obje ise

    [Header("Sesler")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip doorKnockClip;
    [SerializeField] private AudioClip phoneNotifyClip;
    [SerializeField] private AudioClip cameraFlashNoiseClip;

    [Header("Bitis Sahnesi Adi (Varsa)")]
    [SerializeField] private string endSceneName = "";

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
        {
            cornerPortraitImage.sprite = isPretty ? prettyPortraitSprite : normalPortraitSprite;
        }

        if (mirrorCharacterImage != null)
        {
            if (mirrorState == 0 && mirrorUglySprite != null) mirrorCharacterImage.sprite = mirrorUglySprite;
            else if (mirrorState == 1 && mirrorPrettySprite != null) mirrorCharacterImage.sprite = mirrorPrettySprite;
            else if (mirrorState == 2 && mirrorNormalHappySprite != null) mirrorCharacterImage.sprite = mirrorNormalHappySprite;
        }

        if (windowRenderer != null)
        {
            windowRenderer.sprite = stalkerWindow ? stalkerWindowSprite : normalWindowSprite;
        }
        if (stalkerWindowObject != null)
        {
            stalkerWindowObject.SetActive(stalkerWindow);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    // Hangi esya ile etkilesime girilebilir?
    public bool CanInteract(PanelManager.PanelType type)
    {
        switch (type)
        {
            case PanelManager.PanelType.Mirror:
                return currentState == StoryState.Day1_Mirror ||
                       currentState == StoryState.Day2_Mirror ||
                       currentState == StoryState.Day3_MirrorMorning ||
                       currentState == StoryState.Day3_MirrorPanic ||
                       currentState == StoryState.Day4_MirrorHappy;

            case PanelManager.PanelType.Door:
                return currentState == StoryState.Day1_Door ||
                       currentState == StoryState.Day2_Door ||
                       currentState == StoryState.Day3_Door;

            case PanelManager.PanelType.Phone:
                return currentState == StoryState.Day3_Phone ||
                       currentState == StoryState.Day4_Phone;

            case PanelManager.PanelType.Bed:
                return currentState == StoryState.Day1_Bed ||
                       currentState == StoryState.Day2_Bed ||
                       currentState == StoryState.Day3_Bed;

            default:
                return false;
        }
    }

    // Etkilesime basildiginda dogru gune ait diyalogu acar
    public void StartCurrentStateInteraction(PanelManager.PanelType type)
    {
        if (PanelManager.Instance == null) return;

        switch (currentState)
        {
            // ================= 1. GUN =================
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

            // ================= 2. GUN =================
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

            // ================= 3. GUN =================
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
                    "Hundreds of messages... 'I'm watching you', 'I know where you live'...",
                    "[tremble]What is this?! They are all creeps and stalkers! I'm scared...[/tremble]"
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

            // ================= 4. GUN =================
            case StoryState.Day4_Phone:
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.Phone, new string[]
                {
                    "All my followers... they are gone?!",
                    "The stalkers, the creepy messages... Everything disappeared!"
                }, AdvanceStory);
                break;

            case StoryState.Day4_MirrorHappy:
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.Mirror, new string[]
                {
                    "[bounce]I'm back to my old self![/bounce]",
                    "I missed my real face so much... I finally feel free and happy."
                }, AdvanceStory);
                break;
        }
    }

    public void AdvanceStory()
    {
        UnlockAllTriggers();

        switch (currentState)
        {
            // --- GUN 1 AKISI ---
            case StoryState.Day1_Mirror:
                currentState = StoryState.Day1_Door;
                PlaySound(doorKnockClip);
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                {
                    "[shake]*KNOCK KNOCK KNOCK*[/shake]",
                    "Who could that be? Nobody ever visits me... I should check the front door."
                });
                break;

            case StoryState.Day1_Door:
                currentState = StoryState.Day1_Bed;
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                {
                    "What was that thing...? Anyway, I'm exhausted. I should go to bed."
                });
                break;

            case StoryState.Day1_Bed:
                PanelManager.Instance.ShowSleepScreen(
                    onMidSleep: () =>
                    {
                        currentState = StoryState.Day2_Mirror;
                        ApplyVisuals(isPretty: true, mirrorState: 1, stalkerWindow: false);
                        if (CameraController.Instance != null) CameraController.Instance.GoToRoom(1);
                    },
                    onWakeUp: () =>
                    {
                        PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                        {
                            "Ahh... morning already. Let me go to the bathroom and wash my face."
                        });
                    });
                break;

            // --- GUN 2 AKISI ---
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
                    "Fame and fans... I can't wait for tomorrow. Time to sleep."
                });
                break;

            case StoryState.Day2_Bed:
                PanelManager.Instance.ShowSleepScreen(
                    onMidSleep: () =>
                    {
                        currentState = StoryState.Day3_MirrorMorning;
                        ApplyVisuals(isPretty: true, mirrorState: 1, stalkerWindow: false);
                        if (CameraController.Instance != null) CameraController.Instance.GoToRoom(1);
                    },
                    onWakeUp: () =>
                    {
                        PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                        {
                            "Another bright day! Let's admire myself in the mirror."
                        });
                    });
                break;

            // --- GUN 3 AKISI ---
            case StoryState.Day3_MirrorMorning:
                currentState = StoryState.Day3_Phone;
                PlaySound(phoneNotifyClip);
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                {
                    "*BZZT! BZZT!* My phone is blowing up with notifications in the bedroom!"
                });
                break;

            case StoryState.Day3_Phone:
                currentState = StoryState.Day3_MirrorPanic;
                ApplyVisuals(isPretty: true, mirrorState: 1, stalkerWindow: true);
                PlaySound(cameraFlashNoiseClip);
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                {
                    "[shake]*CLICK! FLASH! SHUTTER!*[/shake]\nWhat's that noise outside the window?!",
                    "[tremble]There are cameras at my window! I need to hide in the bathroom![/tremble]"
                });
                break;

            case StoryState.Day3_MirrorPanic:
                currentState = StoryState.Day3_Door;
                ApplyVisuals(isPretty: true, mirrorState: 1, stalkerWindow: false);
                PlaySound(doorKnockClip);
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                {
                    "[shake]*KNOCK KNOCK KNOCK*[/shake]\nHe is at the door again..."
                });
                break;

            case StoryState.Day3_Door:
                currentState = StoryState.Day3_Bed;
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                {
                    "I finally said NO... I just want to sleep and end this nightmare."
                });
                break;

            case StoryState.Day3_Bed:
                PanelManager.Instance.ShowSleepScreen(
                    onMidSleep: () =>
                    {
                        currentState = StoryState.Day4_Phone;
                        ApplyVisuals(isPretty: false, mirrorState: 2, stalkerWindow: false);
                        if (CameraController.Instance != null) CameraController.Instance.GoToRoom(1);
                    },
                    onWakeUp: () =>
                    {
                        PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                        {
                            "Huh... It's so quiet today. Let me check my phone."
                        });
                    });
                break;

            // --- GUN 4 AKISI ---
            case StoryState.Day4_Phone:
                currentState = StoryState.Day4_MirrorHappy;
                PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
                {
                    "Wait... let me look in the bathroom mirror!"
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