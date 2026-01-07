using UnityEngine;
using UnityEngine.UI;

public class SpriteSwitcher : MonoBehaviour
{
    [Header("Imágenes a alternar (GameObjects con componente Image)")]
    public GameObject imageA;
    public GameObject imageB;

    [Header("Tiempo entre cambios (segundos)")]
    public float switchInterval = 0.5f;

    private float timer;
    private bool showingA = true;

    void Start()
    {
        if (imageA == null || imageB == null)
        {
            Debug.LogError(" Asigna ambas imágenes en el inspector.");
            enabled = false;
            return;
        }

        // Solo una imagen activa al inicio
        imageA.SetActive(true);
        imageB.SetActive(false);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= switchInterval)
        {
            timer = 0f;
            showingA = !showingA;

            imageA.SetActive(showingA);
            imageB.SetActive(!showingA);
        }
    }
}
