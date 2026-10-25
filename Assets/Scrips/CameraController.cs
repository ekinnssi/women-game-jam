using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    public float[] odaXPozisyonlari = { 0f, 25f, 50f };
    [SerializeField] private float gecisSuresi = 0.5f;
    [SerializeField] private Ease gecisEgrisi = Ease.OutCubic;
    private int mevcutOda = 1; // 1 = Yatak Odası
    private Tween cameraTween;

    public int MevcutOda => mevcutOda;
    public bool IsMoving => cameraTween != null && cameraTween.IsActive() && cameraTween.IsPlaying();
    public bool IsAtTargetRoom => !IsMoving && Mathf.Abs(transform.position.x - odaXPozisyonlari[mevcutOda]) < 0.15f;

    private void Awake()
    {
        Instance = this;
        // Kamerayı başlangıçta doğrudan Yatak Odasına (X: 25) oturt
        if (odaXPozisyonlari != null && odaXPozisyonlari.Length > 1)
        {
            mevcutOda = 1;
            Vector3 pos = transform.position;
            pos.x = odaXPozisyonlari[mevcutOda];
            transform.position = pos;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (PanelManager.Instance != null && PanelManager.Instance.IsPanelOpen) return;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.wasPressedThisFrame)
                NextRoom();
            else if (Keyboard.current.aKey.wasPressedThisFrame)
                PrevRoom();
        }
    }

    public void NextRoom()
    {
        if (mevcutOda < odaXPozisyonlari.Length - 1)
        {
            GoToRoom(mevcutOda + 1);
        }
    }

    public void PrevRoom()
    {
        if (mevcutOda > 0)
        {
            GoToRoom(mevcutOda - 1);
        }
    }

    public void GoToRoom(int targetRoom)
    {
        if (targetRoom < 0 || targetRoom >= odaXPozisyonlari.Length) return;
        mevcutOda = targetRoom;

        if (cameraTween != null) cameraTween.Kill();
        cameraTween = transform.DOMoveX(odaXPozisyonlari[mevcutOda], gecisSuresi)
            .SetEase(gecisEgrisi)
            .SetUpdate(true);
    }

    private void OnDestroy()
    {
        if (cameraTween != null) cameraTween.Kill();
    }
}