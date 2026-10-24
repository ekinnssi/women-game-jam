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

    // panel tipleri
    public enum PanelType
    {
        None,
        Mirror,
        Door,
        Phone,
        Bed
    }

    // yazi efektleri
    public enum TextEffect
    {
        None,
        Wave,
        Shake,
        Tremble,
        Bounce
    }

    [Header("Paneller")]
    [SerializeField] private GameObject mirrorPanel;
    [SerializeField] private GameObject doorPanel;
    [SerializeField] private GameObject phonePanel;
    [SerializeField] private GameObject sleepCanvas;

    [Header("Ortak Diyalog")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text continueHintText;
    [SerializeField] private string hintMessage = "Geçmek için Space tuşuna bas";

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
        HideAllPanels();
    }

    // ipucu hazirligi
    private void InitHintText()
    {
        if (continueHintText != null || dialogueBox == null) return;

        var existing = dialogueBox.transform.Find("Continue_Hint");
        if (existing != null)
        {
            continueHintText = existing.GetComponent<TMP_Text>();
        }

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
            if (dialogueText != null)
            {
                tmp.font = dialogueText.font;
            }
            tmp.fontSize = 16f;
            tmp.alignment = TextAlignmentOptions.BottomRight;
            tmp.color = new Color(1f, 1f, 1f, 0.65f);
            tmp.text = hintMessage;
            continueHintText = tmp;
        }
    }

    // panelleri gizle
    public void HideAllPanels()
    {
        if (mirrorPanel != null) mirrorPanel.SetActive(false);
        if (doorPanel != null) doorPanel.SetActive(false);
        if (phonePanel != null) phonePanel.SetActive(false);
        if (sleepCanvas != null) sleepCanvas.SetActive(false);
        if (dialogueBox != null) dialogueBox.SetActive(false);
        currentPanelType = PanelType.None;
        isPanelOpen = false;
    }

    // panel ac
    public void OpenInspect(PanelType panelType, List<DialogueStep> steps, Action onComplete = null)
    {
        HideAllPanels();
        currentPanelType = panelType;

        switch (panelType)
        {
            case PanelType.Mirror:
                if (mirrorPanel != null) mirrorPanel.SetActive(true);
                break;
            case PanelType.Door:
                if (doorPanel != null) doorPanel.SetActive(true);
                break;
            case PanelType.Phone:
                if (phonePanel != null) phonePanel.SetActive(true);
                break;
        }

        Time.timeScale = 0f;
        dialogueSteps = steps ?? new List<DialogueStep>();
        onCompleteCallback = onComplete;
        currentLineIndex = 0;
        isPanelOpen = true;
        openedTime = Time.unscaledTime;

        ShowCurrentLine();
    }

    // string overload
    public void OpenInspect(PanelType panelType, string[] lines, Action onComplete = null)
    {
        List<DialogueStep> steps = new List<DialogueStep>();
        if (lines != null)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                steps.Add(new DialogueStep { text = lines[i] });
            }
        }
        OpenInspect(panelType, steps, onComplete);
    }

    // satir goster
    private void ShowCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // adim dongusu
        while (dialogueSteps != null && currentLineIndex < dialogueSteps.Count)
        {
            DialogueStep step = dialogueSteps[currentLineIndex];

            // obje durumlari
            ApplyObjectToggles(step);

            string rawLine = (step != null && step.text != null) ? step.text : "";
            var parsed = ParseTextEffects(rawLine);

            // metin kontrolu
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

                    // sifirla
                    dialogueText.maxVisibleCharacters = 0;
                    dialogueText.ForceMeshUpdate();
                }

                typingCoroutine = StartCoroutine(TypewriterRoutine());
                return;
            }

            // bos satir
            if (dialogueBox != null) dialogueBox.SetActive(false);
            currentLineIndex++;
        }

        // panel kontrolu
        if (currentPanelType != PanelType.None)
        {
            if (dialogueBox != null) dialogueBox.SetActive(false);
            isTyping = false;
        }
        else
        {
            CloseCurrentPanel();
        }
    }

    // obje kontrolu
    private void ApplyObjectToggles(DialogueStep step)
    {
        if (step == null || step.objectToggles == null || step.objectToggles.Count == 0) return;

        for (int i = 0; i < step.objectToggles.Count; i++)
        {
            var toggle = step.objectToggles[i];
            if (toggle != null && toggle.target != null)
            {
                toggle.target.SetActive(toggle.setActive);
            }
        }
    }

    // daktilo rutini
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

    // tus ilerlemesi
    public void HandleAdvanceInput()
    {
        if (!isPanelOpen) return;

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

    // sonraki satir
    public void NextLine()
    {
        currentLineIndex++;
        if (dialogueSteps != null && currentLineIndex < dialogueSteps.Count)
        {
            ShowCurrentLine();
        }
        else
        {
            CloseCurrentPanel();
        }
    }

    // panel kapat
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

        var cb = onCompleteCallback;
        onCompleteCallback = null;
        cb?.Invoke();
    }

    // uyku ekrani
    public void ShowSleepScreen(Action onComplete = null)
    {
        HideAllPanels();
        if (sleepCanvas != null) sleepCanvas.SetActive(true);
        Time.timeScale = 1f;
        onComplete?.Invoke();
    }

    private void Update()
    {
        if (!isPanelOpen) return;

        // ipucu animasyonu
        if (continueHintText != null)
        {
            float alpha = 0.45f + 0.35f * Mathf.Sin(Time.unscaledTime * 4f);
            Color c = continueHintText.color;
            c.a = alpha;
            continueHintText.color = c;
        }

        // tus kontrolu
        if (Time.unscaledTime - openedTime > 0.2f)
        {
            bool advancePressed = false;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                advancePressed = true;
            else if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame ||
                                                 Keyboard.current.eKey.wasPressedThisFrame ||
                                                 Keyboard.current.enterKey.wasPressedThisFrame ||
                                                 Keyboard.current.numpadEnterKey.wasPressedThisFrame))
                advancePressed = true;

            if (advancePressed)
            {
                HandleAdvanceInput();
            }
        }
    }

    private void LateUpdate()
    {
        if (!isPanelOpen) return;

        // yazi animasyonu
        AnimateTextVertices();
    }

    // vertex animasyonlari
    private void AnimateTextVertices()
    {
        if (dialogueText == null) return;
        if (charEffects == null || charEffects.Count == 0) return;

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
                    float waveY = Mathf.Sin(Time.unscaledTime * waveSpeed + i * waveFrequency) * waveAmount;
                    offset = new Vector3(0f, waveY, 0f);
                    break;

                case TextEffect.Shake:
                    offset = new Vector3(UnityEngine.Random.Range(-shakeAmount, shakeAmount), UnityEngine.Random.Range(-shakeAmount, shakeAmount), 0f);
                    break;

                case TextEffect.Tremble:
                    float trembleX = (Mathf.PerlinNoise(i * 1.5f, Time.unscaledTime * trembleSpeed) - 0.5f) * trembleAmount * 2f;
                    float trembleY = (Mathf.PerlinNoise(i * 1.5f + 50f, Time.unscaledTime * trembleSpeed) - 0.5f) * trembleAmount * 2f;
                    offset = new Vector3(trembleX, trembleY, 0f);
                    break;

                case TextEffect.Bounce:
                    float bounceY = Mathf.Abs(Mathf.Sin(Time.unscaledTime * bounceSpeed + i * 0.4f)) * bounceAmount;
                    offset = new Vector3(0f, bounceY, 0f);
                    break;
            }

            destVerts[vertIndex + 0] = charInfo.bottomLeft + offset;
            destVerts[vertIndex + 1] = charInfo.topLeft + offset;
            destVerts[vertIndex + 2] = charInfo.topRight + offset;
            destVerts[vertIndex + 3] = charInfo.bottomRight + offset;
        }

        if (hasAnyEffect)
        {
            dialogueText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
    }

    // etiket ayristirma
    private (string cleanText, List<TextEffect> effects) ParseTextEffects(string rawText)
    {
        if (string.IsNullOrEmpty(rawText))
            return ("", new List<TextEffect>());

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
                    bool isCustomTag = tag == "wave" || tag == "/wave" ||
                                       tag == "shake" || tag == "/shake" ||
                                       tag == "tremble" || tag == "/tremble" ||
                                       tag == "bounce" || tag == "/bounce";

                    if (isCustomTag)
                    {
                        if (tag.StartsWith("/"))
                        {
                            if (stack.Count > 1) stack.Pop();
                        }
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
