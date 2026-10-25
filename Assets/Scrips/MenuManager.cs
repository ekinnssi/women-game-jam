using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void SahneYukle(string sahneAdi)
    {
        SceneManager.LoadScene(sahneAdi);
    }
}