using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using System.Collections.Generic;
using DG.Tweening;

public class SimpleTriggerInteract : MonoBehaviour
{
    // etkilesim tipleri
    public enum InteractType
    {
        InspectDialogue,
        RoomTransition
    }

    [Header("Etkilesim Tipi")]
    [SerializeField] private InteractType interactType = InteractType.InspectDialogue;

    [Header("Oda Ayarlari")]
    [SerializeField] private int currentRoomIndex = -1;
    [SerializeField] private int targetRoomToGo = -1;

    [Header("Diyalog Ayarlari")]
    [SerializeField] private GameObject ePromptUI;
    [SerializeField] private PanelManager.PanelType panelType = PanelManager.PanelType.None;
    [FormerlySerializedAs("dialogueSteps")]
    [SerializeField] private List<DialogueStep> dialogue = new List<DialogueStep>();

    [Header("DOTween Efektleri")]
    [SerializeField] private float hoverScaleFactor = 1.07f;
    [SerializeField] private float animDuration = 0.22f;
    [SerializeField] private Color glowColor = new Color(1f, 0.95f, 0.65f, 0.65f);

    public bool isLocked = false;

    private Transform targetTransform;
    private SpriteRenderer targetRenderer;
    private Collider2D hoverCollider;
    private SpriteRenderer glowRenderer;
    private Vector3 baseScale;
    private Color baseColor;
    private bool isHovered = false;

    private void Awake()
    {
        // hedef tespiti
        if (GetComponent<SpriteRenderer>() != null)
        {
            targetTransform = transform;
        }
        else if (transform.parent != null && transform.parent.GetComponent<SpriteRenderer>() != null)
        {
            targetTransform = transform.parent;
        }
        else
        {
            targetTransform = transform;
        }

        targetRenderer = targetTransform.GetComponent<SpriteRenderer>();
        hoverCollider = GetComponent<Collider2D>();
        if (hoverCollider == null && targetTransform != null)
            hoverCollider = targetTransform.GetComponent<Collider2D>();

        // collider kontrolu
        if (hoverCollider == null && targetRenderer != null)
        {
            var box = targetTransform.gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = targetRenderer.size != Vector2.zero ? targetRenderer.size : Vector2.one;
            hoverCollider = box;
        }

        baseScale = targetTransform.localScale;
        if (targetRenderer != null)
            baseColor = targetRenderer.color;

        // oda tespiti
        DetectRoomIndex();

        // glow olustur
        InitGlow();
    }

    // oda indeksi
    private void DetectRoomIndex()
    {
        // kapi tipi
        bool hasDialogue = dialogue != null && dialogue.Count > 0;
        if (interactType == InteractType.InspectDialogue && panelType == PanelManager.PanelType.None && !hasDialogue)
        {
            interactType = InteractType.RoomTransition;
        }

        if (currentRoomIndex < 0)
        {
            // panel tipine gore
            if (interactType == InteractType.InspectDialogue)
            {
                if (panelType == PanelManager.PanelType.Door) currentRoomIndex = 0;
                else if (panelType == PanelManager.PanelType.Bed) currentRoomIndex = 1;
                else if (panelType == PanelManager.PanelType.Mirror) currentRoomIndex = 2;
            }

            // pozisyona gore
            if (currentRoomIndex < 0)
            {
                float posX = transform.position.x;
                if (posX < 12.5f) currentRoomIndex = 0;
                else if (posX < 37.5f) currentRoomIndex = 1;
                else currentRoomIndex = 2;
            }
        }

        // hedef oda
        if (interactType == InteractType.RoomTransition && targetRoomToGo < 0)
        {
            if (currentRoomIndex == 0) targetRoomToGo = 1;
            else if (currentRoomIndex == 2) targetRoomToGo = 1;
            else if (currentRoomIndex == 1)
            {
                float localX = transform.localPosition.x;
                targetRoomToGo = (localX < 0) ? 0 : 2;
            }
        }
    }

    // glow hazirla
    private void InitGlow()
    {
        if (targetRenderer == null) return;

        var existing = targetTransform.Find("_GlowOutline");
        GameObject glowObj;
        if (existing != null)
        {
            glowObj = existing.gameObject;
        }
        else
        {
            glowObj = new GameObject("_GlowOutline");
            glowObj.transform.SetParent(targetTransform, false);
            glowObj.transform.localPosition = Vector3.zero;
            glowObj.transform.localRotation = Quaternion.identity;
            glowObj.transform.localScale = Vector3.one * 1.08f;
        }

        glowRenderer = glowObj.GetComponent<SpriteRenderer>();
        if (glowRenderer == null)
            glowRenderer = glowObj.AddComponent<SpriteRenderer>();

        glowRenderer.sprite = targetRenderer.sprite;
        glowRenderer.sortingOrder = targetRenderer.sortingOrder - 1;
        glowRenderer.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0f);
    }

    private void Start()
    {
        // baslangic sifirlama
        isLocked = false;
        if (ePromptUI != null) ePromptUI.SetActive(false);
    }

    // etkilesim izni
    public bool IsInteractable()
    {
        if (interactType == InteractType.RoomTransition)
        {
            return targetRoomToGo >= 0;
        }

        if (isLocked) return false;
        if (GameFlowManager.Instance != null)
            return GameFlowManager.Instance.CanInteract(panelType);
        return true;
    }

    private void Update()
    {
        // oda kontrolu
        bool inRoom = CameraController.Instance != null &&
                      CameraController.Instance.MevcutOda == currentRoomIndex &&
                      CameraController.Instance.IsAtTargetRoom;

        // panel kontrolu
        if (PanelManager.Instance != null && PanelManager.Instance.IsPanelOpen)
        {
            inRoom = false;
        }

        bool canInteract = inRoom && IsInteractable();

        // kamera bul
        Camera cam = Camera.main;
        if (cam == null && CameraController.Instance != null)
            cam = CameraController.Instance.GetComponent<Camera>();
        if (cam == null)
            cam = FindFirstObjectByType<Camera>();

       // mouse kontrolu
        bool mouseOver = false;
        if (canInteract && cam != null && Mouse.current != null)
        {
            Vector2 mouseScreen = Mouse.current.position.ReadValue();

            // NaN veya sonsuzluk kontrolü: koordinatlar geçerli ve ekran sınırları içindeyse çalıştır
            if (!float.IsNaN(mouseScreen.x) && !float.IsNaN(mouseScreen.y) &&
                !float.IsInfinity(mouseScreen.x) && !float.IsInfinity(mouseScreen.y))
            {
                float targetZ = targetTransform != null ? targetTransform.position.z : 0f;
                float dist = Mathf.Abs(cam.transform.position.z - targetZ);

                // dist 0 veya geçersizse kameranın varsayılan nearClipPlane değerini baz al
                if (dist <= 0f || float.IsNaN(dist)) dist = 10f;

                Vector3 world3D = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, dist));
                Vector2 mouseWorld = new Vector2(world3D.x, world3D.y);

                if (hoverCollider != null && hoverCollider.OverlapPoint(mouseWorld))
                {
                    mouseOver = true;
                }
                else if (targetRenderer != null && targetRenderer.bounds.Contains(world3D))
                {
                    mouseOver = true;
                }
            }
        }

        // hover uygula
        SetHover(mouseOver);

        // ipucu guncelleme
        if (ePromptUI != null)
        {
            if (ePromptUI.activeSelf != canInteract)
                ePromptUI.SetActive(canInteract);
        }

        // etkilesim kontrolu
        bool interactTriggered = false;
        if (canInteract)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                interactTriggered = true;
            else if (mouseOver && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                interactTriggered = true;
        }

        if (interactTriggered)
        {
            TriggerInteraction();
        }
    }

    // hover durumu
    private void SetHover(bool hover)
    {
        if (isHovered == hover) return;
        isHovered = hover;

        if (targetTransform != null)
        {
            targetTransform.DOKill();
            Vector3 targetScale = hover ? baseScale * hoverScaleFactor : baseScale;
            targetTransform.DOScale(targetScale, animDuration).SetEase(Ease.OutQuad);
        }

        if (glowRenderer != null)
        {
            glowRenderer.DOKill();
            float targetAlpha = hover ? glowColor.a : 0f;
            glowRenderer.DOFade(targetAlpha, animDuration);
        }

        if (targetRenderer != null)
        {
            targetRenderer.DOKill();
            Color col = hover ? (baseColor * 1.15f) : baseColor;
            col.a = baseColor.a;
            targetRenderer.DOColor(col, animDuration);
        }
    }

    // etkilesimi baslat
    private void TriggerInteraction()
    {
        SetHover(false);

        if (targetTransform != null)
        {
            targetTransform.DOPunchScale(baseScale * 0.05f, 0.2f, 8, 0.5f);
        }

        if (ePromptUI != null) ePromptUI.SetActive(false);

        // oda gecisi
        if (interactType == InteractType.RoomTransition)
        {
            if (CameraController.Instance != null && targetRoomToGo >= 0)
            {
                CameraController.Instance.GoToRoom(targetRoomToGo);
            }
            return;
        }

      // diyalog paneli
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.StartCurrentStateInteraction(panelType);
        }
    }

    // diyalog sonu
    private void OnDialogueFinished()
    {
        isLocked = true;
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.AdvanceStory();
        }
    }

    private void OnDisable()
    {
        // tween temizligi
        if (targetTransform != null) targetTransform.DOKill();
        if (glowRenderer != null) glowRenderer.DOKill();
        if (targetRenderer != null) targetRenderer.DOKill();
    }
}
