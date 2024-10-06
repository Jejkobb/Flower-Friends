using System.Collections;
using UnityEngine;

public class FadeInUI : MonoBehaviour
{
    public CanvasGroup canvasGroup; // Reference to the CanvasGroup
    public float fadeDuration = 1f; // Duration for the fade-in effect

    private void Start()
    {
        // Ensure the CanvasGroup starts with an alpha of 0 (invisible)
        canvasGroup.alpha = 0f;
        // Start the fade-in coroutine
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;

        // Gradually increase the alpha of the CanvasGroup over time
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration); // Normalize the elapsed time
            yield return null; // Wait for the next frame
        }

        // Ensure the alpha is fully set to 1 at the end
        canvasGroup.alpha = 1f;
    }
}
