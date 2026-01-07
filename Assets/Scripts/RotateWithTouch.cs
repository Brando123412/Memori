using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class RotateWithTouch : MonoBehaviour, IDragHandler
{
    [Header("Rotación")]
    public float rotationSpeed = 2f;
    private float totalRotation = 0f;
    private RectTransform rectTransform;

    [Header("Tarea completada")]
    public GameObject completedObject;
    public GameObject completedObject2;
    private bool isCompleted = false;

    [SerializeField] GameObject tareaFinish;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (completedObject != null) completedObject.SetActive(false);
        if (completedObject2 != null) completedObject2.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isCompleted) return;

        float deltaX = eventData.delta.x;

        // Solo gira hacia la izquierda (abrir válvula)
        if (deltaX < 0)
        {
            float rotationAmount = Mathf.Abs(deltaX) * rotationSpeed;
            totalRotation += rotationAmount;

            rectTransform.Rotate(0f, 0f, rotationAmount);

            // 🔹 Al completar la rotación total (360°)
            if (totalRotation >= 360f)
            {
                isCompleted = true;
                totalRotation = 360f;
                Time.timeScale = 0f; // Pausa el juego al completar la tarea

                if (completedObject != null) completedObject.SetActive(true);
                if (completedObject2 != null) completedObject2.SetActive(true);

                Debug.Log("¡Válvula completamente abierta!");
                GameController.Instance.AddScore(40);

                rectTransform.GetComponent<MonoBehaviour>().StartCoroutine(ResumeAfterDelay());
            }
        }
    }

    private IEnumerator ResumeAfterDelay()
    {
        yield return new WaitForSecondsRealtime(1.2f);
        Time.timeScale = 1f;
        tareaFinish.SetActive(false);
        GameController.Instance.AddScore(30);
    }
}
