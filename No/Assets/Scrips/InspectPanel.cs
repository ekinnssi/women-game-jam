using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InspectPanel : MonoBehaviour
{
    [Header("UI Elemanları")]
    [SerializeField] private GameObject contentHolder; // Paneldeki tüm görselleri tutan iç obje
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;

    private string[] dialogueLines;
    private int currentLineIndex = 0;
    private System.Action onCompleteCallback;

    private void Start()
    {
        // Buton tıklamalarını oyun başında bir kez bağla
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextLine);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ClosePanel);
        }

        // Oyun başlarken görselleri gizle
        if (contentHolder != null)
        {
            contentHolder.SetActive(false);
        }
    }

    public void OpenPanel(string[] lines, System.Action onComplete = null)
    {
        Time.timeScale = 0f; // Hareketi dondur

        dialogueLines = lines;
        onCompleteCallback = onComplete;
        currentLineIndex = 0;

        // Görselleri aç
        if (contentHolder != null) contentHolder.SetActive(true);

        if (closeButton != null) closeButton.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(true);

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (dialogueLines != null && dialogueLines.Length > 0 && currentLineIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLineIndex];
        }
    }

    public void NextLine()
    {
        currentLineIndex++;

        // Sonraki satır var mı?
        if (dialogueLines != null && currentLineIndex < dialogueLines.Length)
        {
            ShowCurrentLine();

            // Son satıra geldiysek X'i aç, Next'i kapat
            if (currentLineIndex == dialogueLines.Length - 1)
            {
                if (nextButton != null) nextButton.gameObject.SetActive(false);
                if (closeButton != null) closeButton.gameObject.SetActive(true);
            }
        }
        else
        {
            // Bitti
            if (nextButton != null) nextButton.gameObject.SetActive(false);
            if (closeButton != null) closeButton.gameObject.SetActive(true);
        }
    }

    public void ClosePanel()
    {
        Time.timeScale = 1f; // Zamanı serbest bırak

        // Görselleri gizle
        if (contentHolder != null) contentHolder.SetActive(false);

        onCompleteCallback?.Invoke();
    }
}