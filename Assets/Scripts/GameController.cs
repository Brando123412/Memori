using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Settings")]
    public int score = 0;
    public int lives = 3;
    public float gameDuration = 60f; // 1 minuto límite base

    [Header("UI")]
    public TMP_Text nameText;
    public TMP_Text scoreText;
    public TMP_Text timerText;
    [SerializeField] Image[] corazones;

    [Header("PlayerData")]
    [SerializeField] SaveDataPlayer playerInformation;
    [SerializeField] GameObject playerHombre;
    [SerializeField] GameObject playerMujer;

    private float timeElapsed = 0f;
    private bool gameActive = true;

    [SerializeField] InsertionSort savepuntaje;
    [SerializeField] GameObject panelScore;

    [SerializeField] TMP_Text[] nombrePlayer;
    [SerializeField] TMP_Text[] scorePlayer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        
    }
    private void OnEnable()
    {
        if (playerInformation.GetGenero() == "Hombre")
            playerHombre.SetActive(true);
        else if (playerInformation.GetGenero() == "Mujer")
            playerMujer.SetActive(true);
    }

    private void Start()
    {
        timeElapsed = 0f; //
        UpdateUI();
    }

    private void Update()
    {
        if (gameActive)
        {
            timeElapsed += Time.deltaTime;

            if (timerText != null)
                timerText.text = "Tiempo: " + Mathf.FloorToInt(timeElapsed) + "s";

            // Si quieres que el juego termine después de cierto tiempo, puedes mantener una condición
            // Ejemplo: si pasa de 120s, terminar automáticamente
            if (timeElapsed >= 120f) // 2 minutos de límite total (opcional)
            {
                GameOver();
            }
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Puntaje: " + score;

        if (nameText != null)
            nameText.text = playerInformation.GetName();

        if (timerText != null)
            timerText.text = "Tiempo: " + Mathf.FloorToInt(timeElapsed) + "s";
    }

    public void AddScore(int amount)
    {
        if (!gameActive) return;

        score += amount;
        Debug.Log("Nuevo score: " + score);
        scoreText.text = "Puntaje: " + score;
    }

    public void LoseLife()
    {
        if (!gameActive) return;

        lives--;
        Debug.Log("Vidas restantes: " + lives);

        if (lives >= 0 && lives < corazones.Length)
            corazones[lives].enabled = false;

        if (lives <= 0)
            GameOver();
    }

    public void GameOver()
    {
        if (!gameActive) return;

        gameActive = false;
        Time.timeScale = 0f;

        int extraTime = Mathf.Max(0, Mathf.FloorToInt(timeElapsed - gameDuration)); // tiempo después del minuto
        int penalty = (extraTime / 10) * 10; // cada 10s resta 10 puntos
        score -= penalty;

        if (score < 0) score = 0; 

        Debug.Log($" Tiempo total: {timeElapsed:F1}s | Penalización: -{penalty} puntos");
        Debug.Log($" Puntaje final: {score}");

        savepuntaje.SavePuntaje(playerInformation.GetName(), score);
        ImplementacionScore();
    }

    public void ImplementacionScore()
    {
        panelScore.SetActive(true);
        var lista = savepuntaje.ReturnList();

        for (int i = 0; i < nombrePlayer.Length; i++)
        {
            nombrePlayer[i].text = "-";
            scorePlayer[i].text = "-";
        }

        int index = 0;
        for (int i = lista.Count - 1; i >= 0 && index < nombrePlayer.Length; i--)
        {
            nombrePlayer[index].text = lista[i].nombre;
            scorePlayer[index].text = lista[i].puntaje.ToString();
            index++;
        }
    }

    public void GoMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
