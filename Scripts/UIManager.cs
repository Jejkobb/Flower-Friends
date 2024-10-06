using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public CanvasGroup buttonCanvasGroup;
    public CanvasGroup imageCanvasGroup;

    public GameObject mainGame;

    public float fadeDuration = 1f;

    public void Play()
    {
        StartCoroutine(FadeOutAndHide(buttonCanvasGroup, imageCanvasGroup));
        mainGame.SetActive(true);

        Broadcaster.Broadcast("PlaySFX", "pop");
    }

    private IEnumerator FadeOutAndHide(CanvasGroup buttonGroup, CanvasGroup imageGroup)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alphaValue = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            buttonGroup.alpha = alphaValue;
            imageGroup.alpha = alphaValue;

            yield return null;
        }

        buttonGroup.alpha = 0f;
        imageGroup.alpha = 0f;

        buttonGroup.gameObject.SetActive(false);
        imageGroup.gameObject.SetActive(false);
    }
}
