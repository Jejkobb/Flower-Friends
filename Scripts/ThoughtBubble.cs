using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // For TextMeshPro support

public class ThoughtBubble : MonoBehaviour
{
    public List<Sprite> flowerSprites = new List<Sprite>();  // List of possible flower sprites
    public SpriteRenderer flowerSpriteRenderer;              // Reference to the flower SpriteRenderer
    public SpriteRenderer extraSpriteRenderer;               // Reference to the second sprite object

    public TextMeshProUGUI textMeshPro1;                         // Reference to the first TextMeshPro object
    public TextMeshProUGUI textMeshPro2;                         // Reference to the second TextMeshPro object

    public AnimationCurve scaleCurve;    // Curve for scaling animation (used for all elements)
    public float animationDuration = 1f; // Time for the animation (both forward and reverse)
    public float initialDelay = 0f;      // Delay before showing the flower sprite
    public float delayBetweenObjects = 0.2f; // Delay between each object (flower, extra sprite, and text objects)

    private Vector3 initialScale;        // Initial scale of the thought bubble
    private Vector3 initialFlowerScale;  // Initial scale of the flower sprite
    private Vector3 initialExtraSpriteScale; // Initial scale of the extra sprite
    private Vector3 initialTextScale1;   // Initial scale of TextMeshPro1
    private Vector3 initialTextScale2;   // Initial scale of TextMeshPro2

    private float animationTime;   // Used to track animation progress

    void Start()
    {
        // Store the initial scale of the thought bubble and other elements
        initialScale = transform.localScale;
        initialFlowerScale = flowerSpriteRenderer.transform.localScale;
        initialExtraSpriteScale = extraSpriteRenderer.transform.localScale;
        initialTextScale1 = textMeshPro1.transform.localScale;
        initialTextScale2 = textMeshPro2.transform.localScale;

        // Start with a scale of 0 for all objects (invisible)
        transform.localScale = Vector3.zero;
        flowerSpriteRenderer.transform.localScale = Vector3.zero;
        extraSpriteRenderer.transform.localScale = Vector3.zero;
        textMeshPro1.transform.localScale = Vector3.zero;
        textMeshPro2.transform.localScale = Vector3.zero;

        AddRandomFlowerSpriteAndNumber(); // Add a random flower sprite
        StartCoroutine(AnimateThoughtBubble(true)); // Start the forward animation
    }

    // Adds a random flower sprite from the list
    void AddRandomFlowerSpriteAndNumber()
    {
        GameObject[] flowersInScene = GameObject.FindGameObjectsWithTag("Flower");

        // Check if there are any Flowers left in the scene
        if (flowersInScene.Length > 0)
        {
            // Pick a random flower from the scene
            Flower randomFlower = flowersInScene[Random.Range(0, flowersInScene.Length)].GetComponent<Flower>();

            // Set the flower sprite from the randomly selected Flower object
            flowerSpriteRenderer.sprite = randomFlower.GetCurrentFlowerSprite();

            // Set the number of leaves from the randomly selected Flower object
            textMeshPro2.text = "" + randomFlower.GetEnabledLeafCount();
        }
        else
        {
            // All flowers are gone, which means the player has won
            // TODO: Add win condition logic here
            Debug.Log("All flowers are collected! You've won!");
        }
    }

    // Function to get the current flower sprite
    public Sprite GetCurrentFlowerSprite()
    {
        return flowerSpriteRenderer.sprite;
    }

    // Coroutine to animate the thought bubble (forward or backward)
    IEnumerator AnimateThoughtBubble(bool isForward)
    {
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            animationTime = isForward ? elapsedTime / animationDuration : 1 - (elapsedTime / animationDuration);

            // Apply the scale curve to the thought bubble
            float scaleValue = scaleCurve.Evaluate(animationTime);
            transform.localScale = initialScale * scaleValue;

            yield return null;
        }

        // If it's the forward animation, start animating the elements in sequence
        if (isForward)
        {
            yield return new WaitForSeconds(initialDelay);  // Wait for the initial delay
            StartCoroutine(AnimateFlowerSprite(true));
            Broadcaster.Broadcast("PlaySFX", "pop");
            yield return new WaitForSeconds(delayBetweenObjects);  // Delay between flower and extra sprite
            StartCoroutine(AnimateExtraSprite(true));
            Broadcaster.Broadcast("PlaySFX", "pop");
            yield return new WaitForSeconds(delayBetweenObjects);  // Delay between extra sprite and first text
            StartCoroutine(AnimateTextMeshPro(textMeshPro1, initialTextScale1, true));
            yield return new WaitForSeconds(delayBetweenObjects);  // Delay between first text and second text
            StartCoroutine(AnimateTextMeshPro(textMeshPro2, initialTextScale2, true));
            Broadcaster.Broadcast("PlaySFX", "pop");
        }
        else
        {
            Destroy(gameObject);  // If reverse, destroy the thought bubble
        }
    }

    // Coroutine to animate the flower sprite (forward or backward)
    IEnumerator AnimateFlowerSprite(bool isForward)
    {
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            animationTime = isForward ? elapsedTime / animationDuration : 1 - (elapsedTime / animationDuration);

            // Apply the scale curve to the flower sprite
            float scaleValue = scaleCurve.Evaluate(animationTime);
            flowerSpriteRenderer.transform.localScale = initialFlowerScale * scaleValue;

            yield return null;
        }
    }

    // Coroutine to animate the extra sprite (forward or backward)
    IEnumerator AnimateExtraSprite(bool isForward)
    {
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            animationTime = isForward ? elapsedTime / animationDuration : 1 - (elapsedTime / animationDuration);

            // Apply the scale curve to the extra sprite
            float scaleValue = scaleCurve.Evaluate(animationTime);
            extraSpriteRenderer.transform.localScale = initialExtraSpriteScale * scaleValue;

            yield return null;
        }
    }

    // Coroutine to animate TextMeshPro objects (forward or backward)
    IEnumerator AnimateTextMeshPro(TextMeshProUGUI textMeshPro, Vector3 initialScale, bool isForward)
    {
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            animationTime = isForward ? elapsedTime / animationDuration : 1 - (elapsedTime / animationDuration);

            // Apply the scale curve to the TextMeshPro object
            float scaleValue = scaleCurve.Evaluate(animationTime);
            textMeshPro.transform.localScale = initialScale * scaleValue;

            yield return null;
        }
    }

    // Public function to start the reverse animation and destroy the object
    public void DestroyThoughtBubble()
    {
        StopAllCoroutines();                // Stop any running animation
        StartCoroutine(AnimateFlowerSprite(false)); // Animate the flower backwards
        StartCoroutine(AnimateExtraSprite(false));  // Animate the extra sprite backwards
        StartCoroutine(AnimateTextMeshPro(textMeshPro1, initialTextScale1, false)); // Animate TextMeshPro1 backwards
        StartCoroutine(AnimateTextMeshPro(textMeshPro2, initialTextScale2, false)); // Animate TextMeshPro2 backwards
        StartCoroutine(AnimateThoughtBubble(false));  // Start the reverse animation for the thought bubble
    }
}
