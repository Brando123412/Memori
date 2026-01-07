using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectPortrait : MonoBehaviour
{
    public float targetWidth  = 1080f;
    public float targetHeight = 1920f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        ApplyAspect();
    }

#if UNITY_EDITOR
    void Update()
    {
        // Para verlo bien también en el editor si cambias el GameView
        ApplyAspect();
    }
#endif

    void ApplyAspect()
    {
        float targetAspect  = targetWidth / targetHeight;          // 1080 / 1920
        float windowAspect  = (float)Screen.width / Screen.height;

        // Si ya es casi igual, usar toda la pantalla
        if (Mathf.Approximately(targetAspect, windowAspect))
        {
            cam.rect = new Rect(0f, 0f, 1f, 1f);
            return;
        }

        if (windowAspect > targetAspect)
        {
            // Pantalla más ancha → franjas negras a los lados
            float scale = targetAspect / windowAspect;
            float x = (1f - scale) * 0.5f;
            cam.rect = new Rect(x, 0f, scale, 1f);
        }
        else
        {
            // Pantalla más alta → franjas negras arriba/abajo
            float scale = windowAspect / targetAspect;
            float y = (1f - scale) * 0.5f;
            cam.rect = new Rect(0f, y, 1f, scale);
        }
    }
}
