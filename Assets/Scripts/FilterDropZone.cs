using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class FilterDropZone : MonoBehaviour, IDropHandler
{
    [Header("Referencia al filtro viejo en la escena")]
    public GameObject oldFilter;

    [Header("Referencia al filtro nuevo (UI o prefab)")]
    public GameObject newFilter;

    [Header("Texto o imagen de confirmación")]
    public GameObject completedText; // Por ejemplo: "Tarea Completada"

    private bool taskCompleted = false;
    [SerializeField] GameObject tareaFinish;

    public void OnDrop(PointerEventData eventData)
    {
        if (taskCompleted) return;

        if (eventData.pointerDrag != null && eventData.pointerDrag.CompareTag("NewFilter"))
        {
            // 🔹 Pausar el juego mientras se realiza la tarea
            Time.timeScale = 0f;

            if (oldFilter != null) oldFilter.SetActive(false);
            if (newFilter != null) newFilter.SetActive(true);

            if (completedText != null)
                completedText.SetActive(true);

            Debug.Log("Filtro reemplazado correctamente.");

            taskCompleted = true;
            GameController.Instance.AddScore(40);

            newFilter.GetComponent<MonoBehaviour>().StartCoroutine(ResumeAfterDelay());
        }
    }

    private IEnumerator ResumeAfterDelay()
    {
        yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 1f;
        tareaFinish.SetActive(false);
        GameController.Instance.AddScore(30);
    }
}
