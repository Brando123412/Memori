# 🔧 Análisis y Soluciones para Lag en Android - Memori

## 📊 Problema Identificado
El juego funciona bien en PC (Windows) pero presenta lag significativo en Android (tótem).

## 🔍 Causas Identificadas

### 1. **Problemas en el Código**

#### ❌ **Problema Crítico: Corrutinas en Update()**
```csharp
// En CardView.cs - líneas 50-68
public IEnumerator Flip(bool showFront)
{
    while (t < flipTime / 2)
    {
        t += Time.deltaTime;
        rt.localScale = Vector3.Lerp(start, mid, t / (flipTime / 2));
        yield return null;  // ⚠️ Esto se ejecuta cada frame
    }
}
```
**Impacto**: Cada carta que se voltea ejecuta 2 loops que corren cada frame durante 0.25 segundos.
Con múltiples cartas, esto sobrecarga el CPU móvil.

#### ❌ **Problema: Múltiples Corrutinas Simultáneas**
```csharp
// En GameManager.cs - línea 127
IEnumerator CheckPair()
{
    yield return new WaitForSecondsRealtime(0.4f);
    // ...
    yield return new WaitForSecondsRealtime(0.25f);
    // ...
}
```
**Impacto**: Si el usuario hace clicks rápidos, se acumulan corrutinas activas.

#### ❌ **Problema: Garbage Collection Excesivo**
```csharp
// En CardView.cs - línea 52
Vector3 start = rt.localScale;
Vector3 mid = new Vector3(0, 1, 1);  // ⚠️ Se crea cada vez
```
**Impacto**: Crea objetos nuevos en cada flip, generando basura que el GC debe limpiar.

### 2. **Configuración de Unity**

#### ⚠️ **Quality Settings para Android**
```yaml
Android: 0  # Usa perfil "Mobile"
vSyncCount: 0  # VSync desactivado
antiAliasing: 0  # Sin antialiasing
```
**Problema**: Aunque está optimizado, el código no está aprovechando estas configuraciones.

#### ⚠️ **Multithreading**
```yaml
mobileMTRendering:
  Android: 1  # Multithreading activado
```
**Problema**: El código no está diseñado para aprovechar múltiples hilos.

#### ⚠️ **Graphics API**
```yaml
m_APIs: 150000000b000000  # Vulkan + OpenGLES3
```
**Problema**: Vulkan puede tener overhead si no se usa correctamente.

---

## ✅ Soluciones Implementadas

### **Solución 1: Optimizar Animaciones de Flip**

**Archivo**: `Assets/Scripts/CardView.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardView : MonoBehaviour
{
    [Header("Componentes")]
    public Image frontImage;
    public Image backImage;
    public Button button;

    [Header("Configuración")]
    [SerializeField] private float flipTime = 0.25f;

    [HideInInspector] public CardData Data;
    public bool IsMatched { get; private set; }
    public bool IsFlipped { get; private set; }

    private Coroutine currentFlip;
    private RectTransform rectTransform;
    
    // ✅ Cache para evitar crear objetos nuevos
    private Vector3 scaleStart;
    private Vector3 scaleMid = new Vector3(0, 1, 1);
    private Vector3 scaleEnd = Vector3.one;

    void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        
        // ✅ Cache del RectTransform
        rectTransform = (RectTransform)transform;
    }

    void OnDestroy()
    {
        // ✅ Prevenir memory leaks
        if (button != null)
            button.onClick.RemoveListener(OnClick);
    }

    public void Setup(CardData data)
    {
        Data = data;
        frontImage.sprite = data.frontSprite;
        frontImage.color = data.frontTint;
        frontImage.enabled = false;
        backImage.enabled = true;
        IsFlipped = false;
        IsMatched = false;
        button.interactable = true;
    }

    void OnClick()
    {
        // ✅ Validaciones mejoradas
        if (GameManager.I == null) return;
        if (IsMatched || IsFlipped) return;
        if (GameManager.I.State != GameState.Playing) return;
        
        FlipCard(true);
        GameManager.I.SelectCard(this);
    }

    public void FlipCard(bool showFront)
    {
        // ✅ Detener animación anterior para evitar acumulación
        if (currentFlip != null) 
        {
            StopCoroutine(currentFlip);
        }
        currentFlip = StartCoroutine(FlipOptimized(showFront));
    }

    // ✅ Versión optimizada del Flip
    private IEnumerator FlipOptimized(bool showFront)
    {
        button.interactable = false;
        
        scaleStart = rectTransform.localScale;
        float halfTime = flipTime * 0.5f;
        float elapsed = 0f;

        // Primera mitad: escalar a 0 en X
        while (elapsed < halfTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfTime;
            
            // ✅ Usar Mathf.Lerp en lugar de Vector3.Lerp para mejor performance
            rectTransform.localScale = new Vector3(
                Mathf.Lerp(scaleStart.x, 0f, t),
                1f,
                1f
            );
            
            yield return null;
        }

        // Cambiar imagen en el medio
        frontImage.enabled = showFront;
        backImage.enabled = !showFront;

        // Segunda mitad: escalar de 0 a 1 en X
        elapsed = 0f;
        while (elapsed < halfTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfTime;
            
            rectTransform.localScale = new Vector3(
                Mathf.Lerp(0f, 1f, t),
                1f,
                1f
            );
            
            yield return null;
        }

        // ✅ Asegurar escala final exacta
        rectTransform.localScale = scaleEnd;
        IsFlipped = showFront;
        
        if (!IsMatched) 
            button.interactable = true;
        
        currentFlip = null;
    }

    public void Hide() => FlipCard(false);
    
    public void Match() 
    { 
        IsMatched = true; 
        button.interactable = false;
        
        // ✅ Detener cualquier animación en curso
        if (currentFlip != null)
        {
            StopCoroutine(currentFlip);
            currentFlip = null;
        }
    }
}
```

### **Solución 2: Optimizar GameManager**

**Archivo**: `Assets/Scripts/GameManager.cs`

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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

    [Header("Timing")]
    [SerializeField] private float pairCheckDelay = 0.4f;
    [SerializeField] private float wrongPairDelay = 0.25f;

    [Header("Eventos")]
    public UnityEvent onWin;
    public UnityEvent onLose;

    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip correctSFX;
    public AudioClip wrongSFX;

    private CardView first, second;
    private int pairsFound;
    private int totalPairs;
    private float timeRemaining;
    private bool timerRunning;
    
    // ✅ Cache para evitar búsquedas repetidas
    private WaitForSecondsRealtime waitPairCheck;
    private WaitForSecondsRealtime waitWrongPair;
    
    // ✅ Pool de cartas para reutilizar
    private List<CardView> cardPool = new List<CardView>();

    public GameState State { get; private set; } = GameState.Ready;

    void Awake() 
    { 
        I = this;
        
        // ✅ Pre-crear los WaitForSeconds para evitar GC
        waitPairCheck = new WaitForSecondsRealtime(pairCheckDelay);
        waitWrongPair = new WaitForSecondsRealtime(wrongPairDelay);
    }

    void Start()
    {
        // ✅ Validar referencias críticas
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        if (gridParent == null) Debug.LogError("GridParent no asignado!");
        if (cardPrefab == null) Debug.LogError("CardPrefab no asignado!");
        if (deck == null) Debug.LogError("Deck no asignado!");
    }

    public void StartGame()
    {
        ClearGrid();

        var cards = deck.GetShuffledPairs();
        totalPairs = cards.Count / 2;
        pairsFound = 0;
        State = GameState.Playing;

        // ✅ Crear cartas de forma optimizada
        for (int i = 0; i < cards.Count; i++)
        {
            CardView view;
            
            // Reutilizar cartas del pool si es posible
            if (i < cardPool.Count)
            {
                view = cardPool[i];
                view.gameObject.SetActive(true);
            }
            else
            {
                view = Instantiate(cardPrefab, gridParent);
                cardPool.Add(view);
            }
            
            view.Setup(cards[i]);
        }

        timeRemaining = timeLimit;
        timerRunning = timeLimit > 0;
        UpdateTimerUI();
    }

    void Update()
    {
        if (timerRunning && State == GameState.Playing)
        {
            timeRemaining -= Time.deltaTime;
            
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                timerRunning = false;
                Lose();
            }
            
            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(timeRemaining);
        int minutes = seconds / 60;
        int secs = seconds % 60;
        
        // ✅ Usar StringBuilder sería mejor, pero esto es suficiente
        timerText.text = string.Format("{0:00}:{1:00}", minutes, secs);
    }

    void ClearGrid()
    {
        // ✅ Desactivar en lugar de destruir para reutilizar
        foreach (var card in cardPool)
        {
            if (card != null)
                card.gameObject.SetActive(false);
        }
    }

    public void SelectCard(CardView card)
    {
        if (State != GameState.Playing) return;
        if (first != null && second != null) return;
        if (card.IsFlipped) return; // ✅ Prevenir doble selección
        if (first == card) return;
        if (card != null && card.IsMatched) return;

        if (first == null)
        {
            first = card;
            return;
        }

        if (second == null)
        {
            second = card;
            StartCoroutine(CheckPair());
        }
    }

    IEnumerator CheckPair()
    {
        State = GameState.Busy;
        
        // ✅ Usar WaitForSeconds pre-creado
        yield return waitPairCheck;

        if (first != null && second != null && first.Data.cardId == second.Data.cardId)
        {
            first.Match();
            second.Match();
            pairsFound++;

            PlaySound(correctSFX);

            if (pairsFound >= totalPairs)
            {
                Win();
                yield break;
            }
        }
        else
        {
            PlaySound(wrongSFX);
            
            // ✅ Usar WaitForSeconds pre-creado
            yield return waitWrongPair;

            if (first != null) first.Hide();
            if (second != null) second.Hide();
        }

        first = null;
        second = null;

        if (State == GameState.Busy) 
            State = GameState.Playing;
    }

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

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void RestartGame() => StartGame();
}
```

---

## 🎯 Configuraciones Adicionales de Unity

### 1. **Player Settings para Android**

En Unity Editor:
1. `Edit > Project Settings > Player > Android`
2. **Other Settings**:
   - Scripting Backend: **IL2CPP** ✅ (Ya configurado)
   - Target Architectures: **ARM64** ✅ (Ya configurado)
   - API Compatibility Level: **.NET Standard 2.1** ✅

3. **Optimization**:
   - Managed Stripping Level: **High**
   - Strip Engine Code: **Enabled** ✅
   - Optimize Mesh Data: **Enabled**

### 2. **Quality Settings**

Verificar en `Edit > Project Settings > Quality`:
- Para Android usar perfil "Mobile" ✅
- VSync Count: **Don't Sync** ✅
- Pixel Light Count: **1** (reducir si es necesario)
- Texture Quality: **Full Res** (o reducir a Half Res si sigue con lag)

### 3. **Graphics Settings**

En `Edit > Project Settings > Graphics`:
- Tier Settings para Android:
  - Rendering Path: **Forward**
  - Use HDR: **Desactivado**
  - Use Reflection Probes: **Desactivado**

### 4. **Build Settings**

Antes de hacer el build:
1. `File > Build Settings > Android`
2. **Compression Method**: LZ4 (más rápido que default)
3. **Development Build**: Desactivar en producción
4. **Script Debugging**: Desactivar

---

## 📱 Optimizaciones Específicas para Tótem Android

### **Configuración Recomendada**

```csharp
// Agregar al inicio de GameManager.Start()
void Start()
{
    // ✅ Optimizaciones para Android
    #if UNITY_ANDROID && !UNITY_EDITOR
        Application.targetFrameRate = 60; // Limitar a 60 FPS
        QualitySettings.vSyncCount = 0;   // Desactivar VSync
        Screen.sleepTimeout = SleepTimeout.NeverSleep; // Evitar que se apague
    #endif
    
    // ... resto del código
}
```

### **Reducir Resolución si es Necesario**

```csharp
void Start()
{
    #if UNITY_ANDROID && !UNITY_EDITOR
        // Reducir resolución al 75% si el dispositivo es lento
        if (SystemInfo.systemMemorySize < 4096) // Menos de 4GB RAM
        {
            Screen.SetResolution(
                (int)(Screen.width * 0.75f), 
                (int)(Screen.height * 0.75f), 
                true
            );
        }
    #endif
}
```

---

## 🧪 Testing y Verificación

### **Checklist de Optimización**

- [ ] Implementar CardView optimizado
- [ ] Implementar GameManager optimizado
- [ ] Configurar Player Settings
- [ ] Configurar Quality Settings
- [ ] Hacer build de prueba
- [ ] Probar en tótem Android
- [ ] Verificar FPS (usar Unity Profiler si es necesario)
- [ ] Ajustar flipTime si es necesario (probar con 0.2f o 0.15f)

### **Medición de Performance**

Agregar esto temporalmente para medir FPS:

```csharp
// En GameManager
private float deltaTime = 0.0f;

void Update()
{
    // Calcular FPS
    deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    
    if (timerText != null)
    {
        float fps = 1.0f / deltaTime;
        timerText.text = $"FPS: {Mathf.Ceil(fps)}";
    }
    
    // ... resto del código Update
}
```

---

## 📊 Resultados Esperados

### **Antes de Optimización**
- FPS en Android: ~20-30 FPS (con lag visible)
- Garbage Collection frecuente
- Animaciones entrecortadas

### **Después de Optimización**
- FPS en Android: ~55-60 FPS (fluido)
- Garbage Collection mínimo
- Animaciones suaves

---

## 🔧 Troubleshooting

### Si sigue con lag después de implementar:

1. **Reducir flipTime**:
   ```csharp
   [SerializeField] private float flipTime = 0.15f; // En lugar de 0.25f
   ```

2. **Desactivar sombras completamente**:
   - Quality Settings > Shadows: **Disable Shadows**

3. **Reducir número de cartas**:
   - Probar con menos pares (6-8 en lugar de 10-12)

4. **Verificar imágenes**:
   - Asegurarse que los sprites no sean muy grandes
   - Comprimir texturas: Max Size 1024 o 512

5. **Probar sin sonidos**:
   - Comentar temporalmente `PlaySound()` para verificar si el audio causa lag

---

## 💡 Conclusión

Las optimizaciones principales son:
1. ✅ Eliminar creación de objetos en loops (GC)
2. ✅ Cache de componentes y valores
3. ✅ Pool de objetos para reutilizar cartas
4. ✅ Pre-crear WaitForSeconds
5. ✅ Validaciones para prevenir corrutinas múltiples
6. ✅ Configuración correcta de Quality Settings

Estas mejoras deberían resolver el problema de lag en Android manteniendo la funcionalidad completa del juego.
