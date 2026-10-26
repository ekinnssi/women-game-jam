using UnityEngine;

public class PanikPenceresi : MonoBehaviour
{
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        if (PencereKriziManager.Instance != null)
        {
            PencereKriziManager.Instance.PencereyeTiklandi(sr);
        }
    }
}