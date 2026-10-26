using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

[System.Serializable]
public class ObjectToggle
{
    public GameObject target;
    public bool setActive = true;
}

[System.Serializable]
public class DialogueStep
{
    [TextArea(2, 4)]
    public string text;

    [Header("Acilacak / Kapanacak Objeler")]
    public List<ObjectToggle> objectToggles = new List<ObjectToggle>();
}

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance;

    // HIZLI TIKLAMA KORUMASI: Diyalog veya uyku bittigi an kaydedilir
    public static float lastPanelCloseTime = 0f;

    public enum PanelType { None, Mirror, Door, Phone, Bed }
    public enum TextEffect { None, Wave, Shake, Tremble, Bounce }
    public enum ChoiceMode { None, YesOnly_QuestionLocked, NoOnly_YesLocked }

    [Header("Paneller")]
    [SerializeField] private GameObject mirrorPanel;
    [SerializeField] private GameObject doorPanel;
    [SerializeField] private GameObject phonePanel;
    [SerializeField] private GameObject sleepCanvas;

    [Header("Ortak Diyalog")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text continueHintText;
    [SerializeField] private string hintMessage = "Press SPACE to continue";

    [Header("Daktilo Ayari")]
    [SerializeField] private float typingSpeed = 0.035f;

    [Header("Efekt Ayarlari")]
    [SerializeField] private float waveSpeed = 5f;
    [SerializeField] private float waveFrequency = 0.4f;
    [SerializeField] private float waveAmount = 3.5f;
    [SerializeField] private float shakeAmount = 2f;
    [SerializeField] private float trembleSpeed = 18f;
    [SerializeField] private float trembleAmount = 1.5f;
    [SerializeField] private float bounceSpeed = 7f;
    [SerializeField] private float bounceAmount = 4f;

    private List<DialogueStep> dialogueSteps = new List<DialogueStep>();
    private int currentLineIndex = 0;
    private Action onCompleteCallback;
    private PanelType currentPanelType = PanelType.None;
    private ChoiceMode currentChoiceMode = ChoiceMode.None;
    private string postChoiceReply = "";

    private GameObject choiceContainer;
    private Button leftBtn;
    private Button rightBtn;
    private TMP_Text leftBtnText;
    private TMP_Text rightBtnText;
    private bool waitingForChoice = false;

    private bool isPanelOpen = false;
    private bool isTyping = false;
    private float openedTime = -1f;
    private Coroutine typingCoroutine;
    private List<TextEffect> charEffects = new List<TextEffect>();

    public bool IsPanelOpen => isPanelOpen;

    private void Awake()
    {
        Instance = this;
        InitHintText();
        InitChoiceUI();
        HideAllPanels();
    }

    private void InitHintText()
    {
        if (continueHintText != null || dialogueBox == null) return;
        var existing = dialogueBox.transform.Find("Continue_Hint");
        if (existing != null) continueHintText = existing.GetComponent<TMP_Text>();

        if (continueHintText == null)
        {
            GameObject hintObj = new GameObject("Continue_Hint", typeof(RectTransform));
            hintObj.transform.SetParent(dialogueBox.transform, false);
            RectTransform rt = hintObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-25f, 15f);
            rt.sizeDelta = new Vector2(350f, 30f);

            var tmp = hintObj.AddComponent<TextMeshProUGUI>();
            if (dialogueText != null) tmp.font = dialogueText.font;
            tmp.fontSize = 16f;
            tmp.alignment = TextAlignmentOptions.BottomRight;
            tmp.color = new Color(1f, 1f, 1f, 0.65f);
            tmp.text = hintMessage;
            continueHintText = tmp;
        }
    }

    private void InitChoiceUI()
    {
        if (dialogueBox == null) return;
        choiceContainer = new GameObject("Choice_Container", typeof(RectTransform));
        choiceContainer.transform.SetParent(dialogueBox.transform.parent, false);
        RectTransform rt = choiceContainer.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(500f, 90f);
        rt.anchoredPosition = new Vector2(0f, -40f);

        leftBtn = CreateChoiceButton("Btn_Left", new Vector2(-130f, 0f), out leftBtnText);
        rightBtn = CreateChoiceButton("Btn_Right", new Vector2(130f, 0f), out rightBtnText);
        choiceContainer.SetActive(false);
    }

    private Button CreateChoiceButton(string name, Vector2 pos, out TMP_Text btnText)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(choiceContainer.transform, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200f, 65f);
        rt.anchoredPosition = pos;

        Image img = obj.GetComponent<Image>();
        img.color = new Color(0.12f, 0.12f, 0.15f, 0.95f);

        GameObject txtObj = new GameObject("Text", typeof(RectTransform));
        txtObj.transform.SetParent(obj.transform, false);
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;

        var tmp = txtObj.AddComponent<TextMeshProUGUI>();
        if (dialogueText != null) tmp.font = dialogueText.font;
        tmp.fontSize = 28f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        btnText = tmp;

        return obj.GetComponent<Button>();
    }

    public void HideAllPanels()
    {
        if (mirrorPanel != null) mirrorPanel.SetActive(false);
        if (doorPanel != null) doorPanel.SetActive(false);
        if (phonePanel != null) phonePanel.SetActive(false);
        if (sleepCanvas != null) sleepCanvas.SetActive(false);
        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (choiceContainer != null) choiceContainer.SetActive(false);
        waitingForChoice = false;
        currentPanelType = PanelType.None;
        isPanelOpen = false;
    }

    public void OpenInspect(PanelType panelType, List<DialogueStep> steps, Action onComplete = null, ChoiceMode choiceMode = ChoiceMode.None, string replyAfterChoice = "")
    {
        HideAllPanels();
        currentPanelType = panelType;
        currentChoiceMode = choiceMode;
        postChoiceReply = replyAfterChoice;

        switch (panelType)
        {
            case PanelType.Mirror: if (mirrorPanel != null) mirrorPanel.SetActive(true); break;
            case PanelType.Door: if (doorPanel != null) doorPanel.SetActive(true); break;
            case PanelType.Phone: if (phonePanel != null) phonePanel.SetActive(true); break;
        }

        Time.timeScale = 0f;
        dialogueSteps = steps != null ? new List<DialogueStep>(steps) : new List<DialogueStep>();
        onCompleteCallback = onComplete;
        currentLineIndex = 0;
        isPanelOpen = true;
        openedTime = Time.unscaledTime;

        ShowCurrentLine();
    }

    public void OpenInspect(PanelType panelType, string[] lines, Action onComplete = null, ChoiceMode choiceMode = ChoiceMode.None, string replyAfterChoice = "")
    {
        List<DialogueStep> steps = new List<DialogueStep>();
        if (lines != null)
        {
            for (int i = 0; i < lines.Length; i++)
                steps.Add(new DialogueStep { text = lines[i] });
        }
        OpenInspect(panelType, steps, onComplete, choiceMode, replyAfterChoice);
    }

    private void ShowCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        while (dialogueSteps != null && currentLineIndex < dialogueSteps.Count)
        {
            DialogueStep step = dialogueSteps[currentLineIndex];
            ApplyObjectToggles(step);

            string rawLine = (step != null && step.text != null) ? step.text : "";
            var parsed = ParseTextEffects(rawLine);

            if (!string.IsNullOrWhiteSpace(parsed.cleanText))
            {
                if (dialogueBox != null) dialogueBox.SetActive(true);
                if (continueHintText != null)
                {
                    continueHintText.text = hintMessage;
                    continueHintText.gameObject.SetActive(true);
                }

                charEffects = parsed.effects;

                if (dialogueText != null)
                {
                    dialogueText.alignment = TextAlignmentOptions.Center;
                    dialogueText.textWrappingMode = TextWrappingModes.Normal;
                    dialogueText.overflowMode = TextOverflowModes.Overflow;
                    dialogueText.text = parsed.cleanText;
                    dialogueText.maxVisibleCharacters = 0;
                    dialogueText.ForceMeshUpdate();
                }

                typingCoroutine = StartCoroutine(TypewriterRoutine());
                return;
            }

            if (dialogueBox != null) dialogueBox.SetActive(false);
            currentLineIndex++;
        }

        CloseCurrentPanel();
    }

    private void ApplyObjectToggles(DialogueStep step)
    {
        if (step == null || step.objectToggles == null) return;
        for (int i = 0; i < step.objectToggles.Count; i++)
        {
            var toggle = step.objectToggles[i];
            if (toggle != null && toggle.target != null)
                toggle.target.SetActive(toggle.setActive);
        }
    }

    private IEnumerator TypewriterRoutine()
    {
        isTyping = true;
        if (dialogueText != null)
        {
            int totalChars = (dialogueText.textInfo != null) ? dialogueText.textInfo.characterCount : dialogueText.text.Length;
            for (int i = 1; i <= totalChars; i++)
            {
                dialogueText.maxVisibleCharacters = i;
                yield return new WaitForSecondsRealtime(typingSpeed);
            }
            dialogueText.maxVisibleCharacters = totalChars;
        }
        isTyping = false;
        typingCoroutine = null;
    }

    public void HandleAdvanceInput()
    {
        if (!isPanelOpen || waitingForChoice) return;

        if (isTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
            if (dialogueText != null)
            {
                int totalChars = (dialogueText.textInfo != null) ? dialogueText.textInfo.characterCount : dialogueText.text.Length;
                dialogueText.maxVisibleCharacters = totalChars;
                dialogueText.ForceMeshUpdate();
            }
            isTyping = false;
        }
        else
        {
            NextLine();
        }
    }

    public void NextLine()
    {
        currentLineIndex++;
        if (dialogueSteps != null && currentLineIndex < dialogueSteps.Count)
        {
            ShowCurrentLine();
        }
        else if (currentChoiceMode != ChoiceMode.None)
        {
            ShowChoices();
        }
        else
        {
            CloseCurrentPanel();
        }
    }

    private void ShowChoices()
    {
        if (choiceContainer == null)
        {
            CloseCurrentPanel();
            return;
        }

        waitingForChoice = true;
        if (continueHintText != null) continueHintText.gameObject.SetActive(false);
        choiceContainer.SetActive(true);

        leftBtn.onClick.RemoveAllListeners();
        rightBtn.onClick.RemoveAllListeners();

        if (currentChoiceMode == ChoiceMode.YesOnly_QuestionLocked)
        {
            leftBtnText.text = "YES";
            leftBtn.interactable = true;
            leftBtnText.color = Color.white;
            leftBtn.onClick.AddListener(OnValidChoiceClicked);

            rightBtnText.text = "?";
            rightBtn.interactable = false;
            rightBtnText.color = new Color(1f, 1f, 1f, 0.25f);
        }
        else if (currentChoiceMode == ChoiceMode.NoOnly_YesLocked)
        {
            leftBtnText.text = "YES";
            leftBtn.interactable = false;
            leftBtnText.color = new Color(1f, 1f, 1f, 0.25f);

            rightBtnText.text = "NO!";
            rightBtn.interactable = true;
            rightBtnText.color = new Color(1f, 0.35f, 0.35f, 1f);
            rightBtn.onClick.AddListener(OnValidChoiceClicked);
        }
    }

    private void OnValidChoiceClicked()
    {
        waitingForChoice = false;
        currentChoiceMode = ChoiceMode.None;
        if (choiceContainer != null) choiceContainer.SetActive(false);

        openedTime = Time.unscaledTime;

        if (!string.IsNullOrEmpty(postChoiceReply))
        {
            dialogueSteps.Add(new DialogueStep { text = postChoiceReply });
            postChoiceReply = "";
            ShowCurrentLine();
        }
        else
        {
            CloseCurrentPanel();
        }
    }

    public void CloseCurrentPanel()
    {
        Time.timeScale = 1f;
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        charEffects.Clear();
        HideAllPanels();

        // Diyalog kapandigi an zamani kaydet (tıklama korumasi)
        lastPanelCloseTime = Time.unscaledTime;

        var cb = onCompleteCallback;
        onCompleteCallback = null;
        cb?.Invoke();
    }

    public void ShowSleepScreen(Action onMidSleep = null, Action onWakeUp = null)
    {
        StartCoroutine(SleepRoutine(onMidSleep, onWakeUp));
    }

    private IEnumerator SleepRoutine(Action onMidSleep, Action onWakeUp)
    {
        HideAllPanels();
        isPanelOpen = true; // Uykudayken tiklamalari kilitle
        if (sleepCanvas != null) sleepCanvas.SetActive(true);

        yield return new WaitForSecondsRealtime(1.2f);
        onMidSleep?.Invoke();
        yield return new WaitForSecondsRealtime(1.0f);

        if (sleepCanvas != null) sleepCanvas.SetActive(false);
        isPanelOpen = false;
        Time.timeScale = 1f;

        // Uyanma aninda zamani kaydet (tıklama korumasi)
        lastPanelCloseTime = Time.unscaledTime;

        onWakeUp?.Invoke();
    }

    private void Update()
    {
        if (!isPanelOpen) return;

        if (continueHintText != null && !waitingForChoice)
        {
            float alpha = 0.45f + 0.35f * Mathf.Sin(Time.unscaledTime * 4f);
            Color c = continueHintText.color;
            c.a = alpha;
            continueHintText.color = c;
        }

        if (!waitingForChoice && Time.unscaledTime - openedTime > 0.3f)
        {
            bool advancePressed = false;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                advancePressed = true;
            else if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame ||
                                                  Keyboard.current.eKey.wasPressedThisFrame ||
                                                  Keyboard.current.enterKey.wasPressedThisFrame ||
                                                  Keyboard.current.numpadEnterKey.wasPressedThisFrame))
                advancePressed = true;

            if (advancePressed) HandleAdvanceInput();
        }
    }

    private void LateUpdate()
    {
        if (!isPanelOpen) return;
        AnimateTextVertices();
    }

    private void AnimateTextVertices()
    {
        if (dialogueText == null || charEffects == null || charEffects.Count == 0) return;
        TMP_TextInfo textInfo = dialogueText.textInfo;
        if (textInfo == null || textInfo.characterCount == 0) return;

        int visibleCount = Mathf.Min(dialogueText.maxVisibleCharacters, textInfo.characterCount);
        if (visibleCount == 0) return;

        bool hasAnyEffect = false;
        for (int i = 0; i < visibleCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int charIndex = charInfo.index;
            TextEffect effect = (charIndex >= 0 && charIndex < charEffects.Count) ? charEffects[charIndex] : TextEffect.None;
            if (effect == TextEffect.None) continue;

            int matIndex = charInfo.materialReferenceIndex;
            int vertIndex = charInfo.vertexIndex;
            if (matIndex >= textInfo.meshInfo.Length) continue;

            Vector3[] destVerts = textInfo.meshInfo[matIndex].vertices;
            if (destVerts == null || vertIndex + 3 >= destVerts.Length) continue;

            hasAnyEffect = true;
            Vector3 offset = Vector3.zero;

            switch (effect)
            {
                case TextEffect.Wave:
                    offset = new Vector3(0f, Mathf.Sin(Time.unscaledTime * waveSpeed + i * waveFrequency) * waveAmount, 0f);
                    break;
                case TextEffect.Shake:
                    offset = new Vector3(UnityEngine.Random.Range(-shakeAmount, shakeAmount), UnityEngine.Random.Range(-shakeAmount, shakeAmount), 0f);
                    break;
                case TextEffect.Tremble:
                    float tx = (Mathf.PerlinNoise(i * 1.5f, Time.unscaledTime * trembleSpeed) - 0.5f) * trembleAmount * 2f;
                    float ty = (Mathf.PerlinNoise(i * 1.5f + 50f, Time.unscaledTime * trembleSpeed) - 0.5f) * trembleAmount * 2f;
                    offset = new Vector3(tx, ty, 0f);
                    break;
                case TextEffect.Bounce:
                    offset = new Vector3(0f, Mathf.Abs(Mathf.Sin(Time.unscaledTime * bounceSpeed + i * 0.4f)) * bounceAmount, 0f);
                    break;
            }

            destVerts[vertIndex + 0] = charInfo.bottomLeft + offset;
            destVerts[vertIndex + 1] = charInfo.topLeft + offset;
            destVerts[vertIndex + 2] = charInfo.topRight + offset;
            destVerts[vertIndex + 3] = charInfo.bottomRight + offset;
        }

        if (hasAnyEffect) dialogueText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }

    private (string cleanText, List<TextEffect> effects) ParseTextEffects(string rawText)
    {
        if (string.IsNullOrEmpty(rawText)) return ("", new List<TextEffect>());
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        List<TextEffect> effects = new List<TextEffect>();
        Stack<TextEffect> stack = new Stack<TextEffect>();
        stack.Push(TextEffect.None);

        int index = 0;
        while (index < rawText.Length)
        {
            if (rawText[index] == '[' || rawText[index] == '<')
            {
                int close = rawText.IndexOfAny(new char[] { ']', '>' }, index);
                if (close != -1)
                {
                    string tag = rawText.Substring(index + 1, close - index - 1).ToLower();
                    bool isCustomTag = tag == "wave" || tag == "/wave" || tag == "shake" || tag == "/shake" || tag == "tremble" || tag == "/tremble" || tag == "bounce" || tag == "/bounce";
                    if (isCustomTag)
                    {
                        if (tag.StartsWith("/")) { if (stack.Count > 1) stack.Pop(); }
                        else
                        {
                            if (tag == "wave") stack.Push(TextEffect.Wave);
                            else if (tag == "shake") stack.Push(TextEffect.Shake);
                            else if (tag == "tremble") stack.Push(TextEffect.Tremble);
                            else if (tag == "bounce") stack.Push(TextEffect.Bounce);
                        }
                        index = close + 1;
                        continue;
                    }
                }
            }
            sb.Append(rawText[index]);
            effects.Add(stack.Peek());
            index++;
        }
        return (sb.ToString(), effects);
    }
}