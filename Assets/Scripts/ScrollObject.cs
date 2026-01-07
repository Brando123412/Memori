using UnityEngine;

public class ScrollObject : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 2f;

    void Update()
    {
        transform.position += Vector3.left * InfiniteBackgroundAuto.Instance.scrollSpeed * Time.deltaTime;
    }
}
