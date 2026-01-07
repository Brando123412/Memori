using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardView : MonoBehaviour
{
    [Header("Componentes")]
    public Image frontImage;
    public Image backImage;
    public Button button;

    [HideInInspector] public CardData Data;
    public bool IsMatched { get; private set; }
    public bool IsFlipped { get; private set; }

    private float flipTime = 0.25f;

    void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
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
        if (IsMatched || GameManager.I.State != GameState.Playing) return;
        if (!IsFlipped)
        {
            StartCoroutine(Flip(true));
            GameManager.I.SelectCard(this);
        }
    }

    public IEnumerator Flip(bool showFront)
    {
        float t = 0;
        var rt = (RectTransform)transform;
        Vector3 start = rt.localScale;
        Vector3 mid = new Vector3(0, 1, 1);

        while (t < flipTime / 2)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.Lerp(start, mid, t / (flipTime / 2));
            yield return null;
        }

        frontImage.enabled = showFront;
        backImage.enabled = !showFront;

        t = 0;
        while (t < flipTime / 2)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.Lerp(mid, Vector3.one, t / (flipTime / 2));
            yield return null;
        }

        IsFlipped = showFront;
    }

    public void Hide() => StartCoroutine(Flip(false));
    public void Match() { IsMatched = true; button.interactable = false; }
}
