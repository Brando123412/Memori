using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MenuController : MonoBehaviour
{
    public string sceneName;
    public float timeToChangeScene;
    public float timeToReloadScene;
    [SerializeField] Animator animatorMidle;
    [SerializeField] Image imageMidle;

    private void Start()
    {
        Time.timeScale = 1.0f;
    }
    public void ChangeScene()
    {
        StartCoroutine(TimeToDelayChangeScene(sceneName));
    }
    public void ChangeSceneByName(string name)
    {
        StartCoroutine(TimeToDelayChangeScene(name));
    }
    public void ReloadScene()
    {
        StartCoroutine(TimeToDelayReloadScene());
    }
    public void ExitGame()
    {
        Application.Quit();
    }

    public void SetAnimator(Animator aimPlayer)
    {
        animatorMidle.runtimeAnimatorController = aimPlayer.runtimeAnimatorController;
    }
    public void SetImage(Image imagePlayer)
    {
        imageMidle.sprite = imagePlayer.sprite;
        imageMidle.SetNativeSize();
    }
    #region  Coroutines
    IEnumerator TimeToDelayChangeScene(string name)
    {
        FadeController.instance.FadeIn();
        yield return new WaitForSeconds(timeToChangeScene);
        SceneManager.LoadScene(name);
    }
    IEnumerator TimeToDelayReloadScene()
    {
        yield return new WaitForSeconds(timeToReloadScene);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    #endregion
}
