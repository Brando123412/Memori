using UnityEngine;

public class SetResolution : MonoBehaviour
{
    void Start()
    {
        #if UNITY_ANDROID
        // Fuerza la orientación a portrait para dispositivos Android, ya que 1080x1920 sugiere una pantalla vertical
        Screen.orientation = ScreenOrientation.Portrait;
        #endif
        
        // Configura la resolución a 1080x1920 y en modo pantalla completa
        Screen.SetResolution(1080, 1920, true);
    }
}
