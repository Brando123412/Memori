using UnityEngine;

public class RotarModelo : MonoBehaviour
{
    public float velocidad = 30f;

    void Update()
    {
        transform.Rotate(new Vector3(5, 5, 5) * Time.deltaTime*velocidad);
        
    }
}
