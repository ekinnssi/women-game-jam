using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PencereKriziManager : MonoBehaviour
{
    public static PencereKriziManager Instance;

    [Header("Kriz Ayarları")]
    public SpriteRenderer[] butunPencereler;
    public Sprite normalCam;
    public Sprite sapikCamInsanlar;
    public Sprite sapikCamTelefonlar;

    public float krizSuresi = 15f;
    public float yeniCamAcilmaHizi = 0.3f; // Yeni hız ayarı

    [Header("Ambiyans Ses / Pitch Ayarları")]
    [SerializeField] private AudioSource ambiyansAudioSource;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;
    [SerializeField] private float pitchDegisimHizi = 2f;

    private bool krizAktif = false;
    private float varsayilanPitch = 1f;
    private List<SpriteRenderer> ekstraPencereler = new List<SpriteRenderer>();

    void Awake()
    {
        Instance = this;

        foreach (var p in butunPencereler)
        {
            if (!p.gameObject.activeSelf)
            {
                ekstraPencereler.Add(p);
            }
        }
    }

    public void KriziBaslat()
    {
        krizAktif = true;
        foreach (var p in butunPencereler)
        {
            p.sprite = normalCam;
            p.gameObject.SetActive(true);
        }

        if (ambiyansAudioSource != null)
        {
            varsayilanPitch = ambiyansAudioSource.pitch;
            ambiyansAudioSource.Play();
            StartCoroutine(PitchDalgalanmaDongusu());
        }

        StartCoroutine(KrizZamanlayici());
        StartCoroutine(CamAcmaDongusu());
    }

    IEnumerator PitchDalgalanmaDongusu()
    {
        while (krizAktif && ambiyansAudioSource != null)
        {
            float t = Mathf.PingPong(Time.time * pitchDegisimHizi, 1f);
            ambiyansAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, t);
            yield return null;
        }

        if (ambiyansAudioSource != null)
        {
            ambiyansAudioSource.pitch = varsayilanPitch;
            ambiyansAudioSource.Stop();
        }
    }

    IEnumerator KrizZamanlayici()
    {
        yield return new WaitForSeconds(krizSuresi);
        krizAktif = false; // Süre bitti, yeni saldırılar durdu

        // Süre bittiğinde ekranda hala açılmış (sapık/telefon belirsizliği olan) 
        // pencereler varsa, onları aniden kesmek yerine hızlıca ve doğallıkla normal cama çeviriyoruz
        foreach (var p in butunPencereler)
        {
            if (p.sprite != normalCam)
            {
                p.sprite = normalCam;
            }
        }

        // Ekstra (aktif olmayan) pencereleri sırayla gizleme efekti
        List<SpriteRenderer> silinecekPencereler = new List<SpriteRenderer>(ekstraPencereler);

        while (silinecekPencereler.Count > 0)
        {
            int rastgeleIndex = Random.Range(0, silinecekPencereler.Count);
            silinecekPencereler[rastgeleIndex].gameObject.SetActive(false);
            silinecekPencereler.RemoveAt(rastgeleIndex);

            yield return new WaitForSeconds(Random.Range(0.08f, 0.2f));
        }

        // Tüm süreç tamamlanınca diyalog devreye girer
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.OpenInspect(PanelManager.PanelType.None, new string[]
            {
                "[tremble]W-what was that?! Are they gone?[/tremble]",
                "I can't breathe... [shake]WHAT WAS THAT![/shake]"
            });
        }
    }

    IEnumerator CamAcmaDongusu()
    {
        while (krizAktif)
        {
            SpriteRenderer secilenCam = butunPencereler[Random.Range(0, butunPencereler.Length)];

            if (secilenCam.sprite == normalCam)
            {
                CamSaldirisi(secilenCam);
            }
            yield return new WaitForSeconds(yeniCamAcilmaHizi);
        }
    }

    void CamSaldirisi(SpriteRenderer cam)
    {
        Sprite secilenSaldiri = (Random.value > 0.5f) ? sapikCamInsanlar : sapikCamTelefonlar;
        cam.sprite = secilenSaldiri;

        if (ScreenFlash.Instance != null)
        {
            ScreenFlash.Instance.TriggerFlash(0.9f, 0.3f);
        }
    }

    public void PencereyeTiklandi(SpriteRenderer tiklanan)
    {
        // Oyuncu kriz anında veya son kapanış evresinde tıkladıkça pencereler teker teker kapanır
        if (tiklanan.sprite != normalCam)
        {
            tiklanan.sprite = normalCam;
        }
    }
}