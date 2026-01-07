using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ConfettiUI : MonoBehaviour
{
    [Header("Dónde se dibuja (RectTransform dentro del Canvas)")]
    public RectTransform container;

    [Header("Apariencia")]
    public Sprite[] sprites;
    public Color[] palette = new Color[]
    {
        new Color(1f,0.35f,0.4f),
        new Color(1f,0.7f,0.2f),
        new Color(1f,0.95f,0.3f),
        new Color(0.4f,0.9f,0.5f),
        new Color(0.35f,0.7f,1f),
        new Color(0.7f,0.5f,1f),
        Color.white
    };

    [Header("Emisión")]
    [Tooltip("Cantidad por ráfaga")]
    public int count = 60;
    [Tooltip("Ráfagas seguidas (repeticiones)")]
    public int repeticiones = 3;
    [Tooltip("Tiempo entre ráfagas (s)")]
    public float intervalo = 0.4f;

    [Tooltip("Tamaño mínimo y máximo (en px)")]
    public Vector2 sizeRange = new Vector2(8f, 20f);
    [Tooltip("Velocidad inicial (px/s)")]
    public Vector2 speedRange = new Vector2(350f, 700f);
    [Tooltip("Ángulo de disparo (grados) y apertura")]
    public Vector2 angleAndSpread = new Vector2(90f, 120f);
    [Tooltip("Gravedad (px/s²)")]
    public float gravity = -1200f;
    [Tooltip("Rotación inicial (deg/s)")]
    public Vector2 spinRange = new Vector2(-360f, 360f);
    [Tooltip("Vida útil (s)")]
    public Vector2 lifeRange = new Vector2(1.0f, 1.8f);
    [Tooltip("Fade-out final (fracción de vida)")]
    [Range(0f,1f)] public float fadeTail = 0.35f;
    [Tooltip("¿Colisión con bordes horizontales (rebote simple)?")]
    public bool bounceSides = true;
    [Range(0f,1f)] public float bounceDamping = 0.6f;

    // --- pool interno ---
    class Piece
    {
        public RectTransform rt;
        public Image img;
        public Vector2 vel;
        public float spin;
        public float life;
        public float t;
        public float startAlpha;
        public Color baseColor;
        public bool active;
    }

    readonly List<Piece> pool = new List<Piece>(256);
    readonly List<Piece> active = new List<Piece>(256);
    static readonly Vector3 V3Z = new Vector3(0, 0, 1);

    void Awake()
    {
        if (!container)
            container = transform as RectTransform;
    }

    void Update()
    {
        if (active.Count == 0) return;

        float dt = Time.unscaledDeltaTime;
        float left = container.rect.xMin;
        float right = container.rect.xMax;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            var p = active[i];
            if (!p.active) continue;

            // física
            p.vel.y += gravity * dt;

            // mover
            Vector2 pos = p.rt.anchoredPosition;
            pos += p.vel * dt;

            // rebote lateral
            if (bounceSides)
            {
                if (pos.x < left)
                {
                    pos.x = left;
                    p.vel.x = -p.vel.x * bounceDamping;
                }
                else if (pos.x > right)
                {
                    pos.x = right;
                    p.vel.x = -p.vel.x * bounceDamping;
                }
            }

            p.rt.anchoredPosition = pos;
            p.rt.localEulerAngles += V3Z * (p.spin * dt);

            // vida & fade
            p.t += dt;
            float u = Mathf.Clamp01(p.t / p.life);
            if (u >= 1f)
            {
                Desactivar(p, i);
                continue;
            }

            float alpha = p.startAlpha;
            if (u > 1f - fadeTail && fadeTail > 0f)
            {
                float k = (u - (1f - fadeTail)) / fadeTail;
                alpha = Mathf.Lerp(p.startAlpha, 0f, k);
            }

            var c = p.baseColor; c.a = alpha;
            p.img.color = c;
        }
    }

    void Desactivar(Piece p, int activeIndex)
    {
        p.active = false;
        p.rt.gameObject.SetActive(false);
        active.RemoveAt(activeIndex);
        pool.Add(p);
    }

    Piece GetPiece()
    {
        Piece p;
        if (pool.Count > 0)
        {
            p = pool[pool.Count - 1];
            pool.RemoveAt(pool.Count - 1);
            p.rt.gameObject.SetActive(true);
        }
        else
        {
            var go = new GameObject("confetti", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(container, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            p = new Piece { rt = rt, img = go.GetComponent<Image>() };
        }
        return p;
    }

    Sprite PickSprite()
    {
        if (sprites != null && sprites.Length > 0)
            return sprites[Random.Range(0, sprites.Length)];
        return null;
    }

    Color PickColor()
    {
        if (palette != null && palette.Length > 0)
            return palette[Random.Range(0, palette.Length)];
        return Color.white;
    }

    float Rng(Vector2 r) => Random.Range(r.x, r.y);

    // ============================
    // API PÚBLICA
    // ============================

    public void PlayCentered()
    {
        StartCoroutine(EmitRafagas(Vector2.zero, true));
    }

    public void PlayAtScreenPosition(Vector2 screenPos, Camera uiCamera = null)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(container, screenPos, uiCamera, out var localPos);
        StartCoroutine(EmitRafagas(localPos, false));
    }

    IEnumerator EmitRafagas(Vector2 localPos, bool spreadFullCircle)
    {
        for (int i = 0; i < repeticiones; i++)
        {
            Emit(count, localPos, spreadFullCircle);
            yield return new WaitForSeconds(intervalo);
        }
    }

    public void Emit(int n, Vector2 localPos, bool spreadFullCircle)
    {
        if (!container) return;

        float baseAngle = angleAndSpread.x;
        float spread = angleAndSpread.y;

        for (int i = 0; i < n; i++)
        {
            var p = GetPiece();

            p.img.sprite = PickSprite();
            p.baseColor = PickColor();
            p.startAlpha = 1f;
            p.img.raycastTarget = false;

            float s = Rng(sizeRange);
            p.rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, s);
            p.rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, s * Random.Range(0.7f, 1.3f));

            p.rt.anchoredPosition = localPos;
            p.rt.localEulerAngles = V3Z * Random.Range(0f, 360f);

            p.life = Rng(lifeRange);
            p.t = 0f;

            float ang = spreadFullCircle ? Random.Range(0f, 360f) : (baseAngle + Random.Range(-spread * 0.5f, spread * 0.5f));
            float spd = Rng(speedRange);
            float rad = ang * Mathf.Deg2Rad;
            p.vel = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * spd;

            p.spin = Rng(spinRange);

            var c = p.baseColor; c.a = p.startAlpha;
            p.img.color = c;

            p.active = true;
            active.Add(p);
        }
    }
}
