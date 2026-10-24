using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Etkilesim : MonoBehaviour
{
    public UnityEvent eTusunaBasildi;
    private bool alandaMi = false;

    void Update()
    {
        if (alandaMi && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("E tuşuna basıldı, görev çalışıyor!");
            eTusunaBasildi.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            alandaMi = true;
            Debug.Log("Kamera hedefin alanına başarıyla girdi!");
        }
    }
}