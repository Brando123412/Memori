using UnityEngine;
using UnityEngine.UI;
//using DG.Tweening;
public class FadeController : MonoBehaviour
{
    public static FadeController instance;
    [SerializeField] CanvasGroup cg;
    [SerializeField] float fadeDuration = 1f;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void FadeIn()
    {
        cg.gameObject.SetActive(true);
        cg.interactable = true;
        cg.blocksRaycasts = true;
        //cg.DOFade(1, fadeDuration).OnComplete(() => FadeOut());
        //Invoke("FadeOut",1);
    }
    public void FadeOut()
    {
        /*cg.DOFade(0, fadeDuration).OnComplete(() =>
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
            cg.gameObject.SetActive(false);
        });*/
    }
}
