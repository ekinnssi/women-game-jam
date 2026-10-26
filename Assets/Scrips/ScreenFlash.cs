using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScreenFlash : MonoBehaviour
{
    public static ScreenFlash Instance;

    [SerializeField] private Image flashImage;

    [Header("Flaş Sesi")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip flashSound;

    private void Awake()
    {
        Instance = this;
        if (flashImage != null)
        {
            Color c = flashImage.color;
            c.a = 0f;
            flashImage.color = c;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Ekranda anlık beyaz flaş patlaması oluşturur ve sesi çalar.
    /// </summary>
    /// <param name="alpha">Flaşın tepe parlaklığı (0 ile 1 arası, varsayılan 0.85)</param>
    /// <param name="fadeDuration">Flaşın kaybolma süresi (saniye)</param>
    public void TriggerFlash(float alpha = 0.85f, float fadeDuration = 0.25f)
    {
        // Görsel Flaş Efekti
        if (flashImage != null)
        {
            flashImage.DOKill();

            Color c = flashImage.color;
            c.a = alpha;
            flashImage.color = c;

            flashImage.DOFade(0f, fadeDuration).SetUpdate(true);
        }

        // Flaş Ses Efekti
        if (audioSource != null && flashSound != null)
        {
            audioSource.PlayOneShot(flashSound);
        }
    }
}