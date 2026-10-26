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
    public float camEkrandaKalmaSuresi = 1f;
    public float yeniCamAcilmaHizi = 0.5f;

    private bool krizAktif = false;
    // Sadece sonradan eklenen pencereleri tutacak otomatik liste
    private List<SpriteRenderer> ekstraPencereler = new List<SpriteRenderer>();

    void Awake()
    {
        Instance = this;

        // Oyun başlarken tiki kapalı olan pencereleri bul ve ekstra listesine kaydet
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
            p.gameObject.SetActive(true); // Kriz anında hepsini aç
        }

        StartCoroutine(KrizZamanlayici());
        StartCoroutine(CamAcmaDongusu());
    }

    IEnumerator KrizZamanlayici()
    {
        yield return new WaitForSeconds(krizSuresi);
        krizAktif = false; // Süre doldu, yeni saldırıları durdur

        // Önce ekrandaki tüm pencereleri anında normal cama çevir
        foreach (var p in butunPencereler)
        {
            p.sprite = normalCam;
        }

        // Ekstra pencereleri kopyala ve rastgele sırayla teker teker gizle
        List<SpriteRenderer> silinecekPencereler = new List<SpriteRenderer>(ekstraPencereler);

        while (silinecekPencereler.Count > 0)
        {
            int rastgeleIndex = Random.Range(0, silinecekPencereler.Count);
            silinecekPencereler[rastgeleIndex].gameObject.SetActive(false);
            silinecekPencereler.RemoveAt(rastgeleIndex);

            // Diğer pencerenin kaybolması için 0.1 ile 0.3 saniye arası rastgele bekle
            yield return new WaitForSeconds(Random.Range(0.1f, 0.4f));
        }

        // Tüm pencereler tamamen temizlendikten sonra diyalogu ekrana getir
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
                StartCoroutine(CamSaldirisi(secilenCam));
            }
            yield return new WaitForSeconds(yeniCamAcilmaHizi);
        }
    }

    IEnumerator CamSaldirisi(SpriteRenderer cam)
    {
        Sprite secilenSaldiri = (Random.value > 0.5f) ? sapikCamInsanlar : sapikCamTelefonlar;
        cam.sprite = secilenSaldiri;

        float zaman = 0;

        while (zaman < camEkrandaKalmaSuresi && cam.sprite != normalCam && krizAktif)
        {
            zaman += Time.deltaTime;
            yield return null;
        }

        if (krizAktif)
        {
            cam.sprite = normalCam;
        }
    }

    public void PencereyeTiklandi(SpriteRenderer tiklanan)
    {
        if (krizAktif && tiklanan.sprite != normalCam)
        {
            tiklanan.sprite = normalCam;
        }
    }
}