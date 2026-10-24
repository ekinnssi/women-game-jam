using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float[] odaXPozisyonlari = { 0f, 25f, 50f };
    public float kaymaHizi = 5f;
    private int mevcutOda = 0;

    void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.wasPressedThisFrame && mevcutOda < odaXPozisyonlari.Length - 1)
                mevcutOda++;
            else if (Keyboard.current.aKey.wasPressedThisFrame && mevcutOda > 0)
                mevcutOda--;
        }

        Vector3 hedefPozisyon = new Vector3(odaXPozisyonlari[mevcutOda], transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, hedefPozisyon, Time.deltaTime * kaymaHizi);
    }
}