using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    public float[] odaXPozisyonlari = { 0f, 25f, 50f };
    [SerializeField] private float gecisSuresi = 0.5f;
    [SerializeField] private Ease gecisEgrisi = Ease.OutCubic;
    private int mevcutOda = 0;
    private Tween cameraTween;

    public int MevcutOda => mevcutOda;
    public bool IsMoving => cameraTween != null && cameraTween.IsActive() && cameraTween.IsPlaying();
    public bool IsAtTargetRoom => !IsMoving && Mathf.Abs(transform.position.x - odaXPozisyonlari[mevcutOda]) < 0.15f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // zaman kontrolu
        if (Time.timeScale == 0f) return;

        // diyalog kontrolu
        if (PanelManager.Instance != null && PanelManager.Instance.IsPanelOpen) return;

        // tus kontrolu
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.wasPressedThisFrame)
                NextRoom();
            else if (Keyboard.current.aKey.wasPressedThisFrame)
                PrevRoom();
        }
    }

    // sonraki oda
    public void NextRoom()
    {
        if (mevcutOda < odaXPozisyonlari.Length - 1)
        {
            GoToRoom(mevcutOda + 1);
        }
    }

    // onceki oda
    public void PrevRoom()
    {
        if (mevcutOda > 0)
        {
            GoToRoom(mevcutOda - 1);
        }
    }

    // odaya git
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
