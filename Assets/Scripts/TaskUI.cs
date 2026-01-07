using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TaskUI : MonoBehaviour
{
    public static TaskUI Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject panelTarea;
    [SerializeField] private Slider barraProgreso;
    [SerializeField] private TMP_Text textoInstruccion;
    [SerializeField] private Button botonToque;

    [Header("Configuración")]
    [SerializeField] private int cantidadToques = 10;
    [SerializeField] private float velocidadDescenso = 1f; // velocidad con la que baja el progreso si no tocas

    private int toquesActuales = 0;
    bool tareaActiva = false;
    float tmoValue;
    private System.Action onTaskComplete;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        panelTarea.SetActive(false);
        botonToque.onClick.AddListener(RegistrarToque);
    }

    void Update()
    {
        if (tareaActiva)
        {
            // Si no se presiona, el progreso baja gradualmente
            if (toquesActuales > 0)
            {
                toquesActuales -= Mathf.CeilToInt(velocidadDescenso * Time.deltaTime * cantidadToques);
                if (toquesActuales < 0) toquesActuales = 0;
                barraProgreso.value = (float)toquesActuales / cantidadToques;
            }
        }
    }

    public void MostrarPanel(System.Action onComplete)
    {
        onTaskComplete = onComplete;
        panelTarea.SetActive(true);
        barraProgreso.value = 0;
        toquesActuales = 0;
        tareaActiva = true;
        tmoValue= InfiniteBackgroundAuto.Instance.scrollSpeed;
        InfiniteBackgroundAuto.Instance.scrollSpeed = 0;
        textoInstruccion.text = "Toca repetidamente para completar la tarea";
    }

    private void RegistrarToque()
    {
        if (!tareaActiva) return;

        toquesActuales++;
        barraProgreso.value = (float)toquesActuales / cantidadToques;

        if (toquesActuales >= cantidadToques)
        {
            CompletarTarea();
        }
    }

    private void CompletarTarea()
    {
        InfiniteBackgroundAuto.Instance.scrollSpeed = tmoValue;
        tareaActiva = false;
        panelTarea.SetActive(false);
        onTaskComplete?.Invoke();
        Debug.Log(" Tarea completada!");
    }
}
