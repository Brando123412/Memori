using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public enum GameState { Ready, Playing, Busy, Win, Lose }

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    [Header("Referencias")]
    public DeckConfig deck;
    public Transform gridParent;
    public CardView cardPrefab;

    [Header("UI del temporizador")]
    public TextMeshProUGUI timerText;
    public float timeLimit = 60f;

    [Header("Eventos")]
    public UnityEvent onWin;
    public UnityEvent onLose;

    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip correctSFX;
    public AudioClip wrongSFX;

    // Estado interno
    private CardView first, second;
    private int pairsFound;
    private int totalPairs;
    private float timeRemaining;
    private bool timerRunning;

    private GridLayoutGroup gridLayout;
    public GameState State { get; private set; } = GameState.Ready;

    void Awake()
    {
        I = this;
        if (gridParent != null)
            gridLayout = gridParent.GetComponent<GridLayoutGroup>();
    }

    // -------------------------------------------------------------------
    // 🔵 INICIAR / REINICIAR JUEGO
    // -------------------------------------------------------------------
    public void StartGame()
    {
        State = GameState.Ready;
        timerRunning = false;

        first = null;
        second = null;
        pairsFound = 0;

        ClearGrid();

        if (deck == null)
        {
            Debug.LogWarning("GameManager: no hay DeckConfig.");
            return;
        }

        var cards = deck.GetShuffledPairs();
        totalPairs = cards.Count / 2;

        if (gridLayout != null)
            gridLayout.enabled = true;

        foreach (var data in cards)
        {
            var view = Instantiate(cardPrefab, gridParent);
            view.Setup(data);
        }

        var rect = gridParent as RectTransform;
        if (gridLayout != null && rect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            gridLayout.enabled = false;
        }

        timeRemaining = timeLimit;
        timerRunning = timeLimit > 0;
        UpdateTimerUI();

        State = GameState.Playing;
    }

    void Update()
    {
        if (!timerRunning || State != GameState.Playing)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            timerRunning = false;
            Lose();
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText == null)
            return;

        int total = Mathf.CeilToInt(Mathf.Max(0, timeRemaining));
        timerText.text = $"{total / 60:00}:{total % 60:00}";
    }

    void ClearGrid()
    {
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);
    }

    // -------------------------------------------------------------------
    // 🔵 SELECCIÓN DE CARTAS
    // -------------------------------------------------------------------
    public void SelectCard(CardView card)
    {
        if (State != GameState.Playing) return;
        if (first != null && second != null) return;
        if (first == card) return;
        if (card.IsMatched) return;

        if (first == null)
        {
            first = card;
            return;
        }

        second = card;
        StartCoroutine(CheckPair());
    }

    IEnumerator CheckPair()
    {
        State = GameState.Busy;
        yield return new WaitForSecondsRealtime(0.4f);

        bool isMatch = first != null && second != null &&
                       first.Data.cardId == second.Data.cardId;

        if (isMatch)
        {
            first.Match();
            second.Match();
            pairsFound++;

            PlaySound(correctSFX);

            if (pairsFound >= totalPairs)
                Win();
        }
        else
        {
            PlaySound(wrongSFX);
            yield return new WaitForSecondsRealtime(0.25f);

            first?.Hide();
            second?.Hide();
        }

        first = null;
        second = null;

        if (State == GameState.Busy)
            State = GameState.Playing;
    }

    // -------------------------------------------------------------------
    // 🔵 RESULTADOS
    // -------------------------------------------------------------------
    void Win()
    {
        State = GameState.Win;
        timerRunning = false;
        onWin?.Invoke();
    }

    void Lose()
    {
        State = GameState.Lose;
        timerRunning = false;
        onLose?.Invoke();
    }

    // -------------------------------------------------------------------
    // 🔵 UTILIDADES EXTRAS
    // -------------------------------------------------------------------
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    public void ReiniciarEscena()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartGame()
    {
        StartGame();
    }

    // -----------------------------------------------------------
    // 🔵 MÉTODO NUEVO — CANCELAR EL TIEMPO (lo que pediste)
    // -----------------------------------------------------------
    public void CancelarTiempo()
    {
        timerRunning = false;
    }
}
