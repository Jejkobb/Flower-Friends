using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // For TextMeshPro support

public class ThoughtBubble : MonoBehaviour
{
    public List<Sprite> flowerSprites = new List<Sprite>();
    public SpriteRenderer flowerSpriteRenderer;
    public SpriteRenderer extraSpriteRenderer;

    public TextMeshProUGUI textMeshPro1;
    public TextMeshProUGUI textMeshPro2;

    public AnimationCurve scaleCurve;
    public float animationDuration = 1f;
    public float initialDelay = 0f;
    public float delayBetweenObjects = 0.2f;

    private Vector3 initialScale;
    private Vector3 initialFlowerScale;
    private Vector3 initialExtraSpriteScale;
    private Vector3 initialTextScale1;
    private Vector3 initialTextScale2;

    private float animationTime;

    void Start()
    {
        initialScale = transform.localScale;
        initialFlowerScale = flowerSpriteRenderer.transform.localScale;
        initialExtraSpriteScale = extraSpriteRenderer.transform.localScale;
        initialTextScale1 = textMeshPro1.transform.localScale;
        initialTextScale2 = textMeshPro2.transform.localScale;

        transform.localScale = Vector3.zero;
        flowerSpriteRenderer.transform.localScale = Vector3.zero;
        extraSpriteRenderer.transform.localScale = Vector3.zero;
        textMeshPro1.transform.localScale = Vector3.zero;
        textMeshPro2.transform.localScale = Vector3.zero;

        AddRandomFlowerSpriteAndNumber();
        StartCoroutine(AnimateThoughtBubble(true));
    }

    void AddRandomFlowerSpriteAndNumber()
    {
        GameObject[] flowersInScene = GameObject.FindGameObjectsWithTag("Flower");

        if (flowersInScene.Length > 0)
        {
            Flower randomFlower = flowersInScene[Random.Range(0, flowersInScene.Length)].GetComponent<Flower>();

            flowerSpriteRenderer.sprite = randomFlower.GetCurrentFlowerSprite();

            textMeshPro2.text = "" + randomFlower.GetEnabledLeafCount();
        }
        else
        {
            //Debug.Log("All flowers are collected! You've won!");
        }
    }

    public Sprite GetCurrentFlowerSprite()
    {
        return flowerSpriteRenderer.sprite;
    }

    IEnumerator AnimateThoughtBubble(bool isForward)
    {
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            animationTime = isForward ? elapsedTime / animationDuration : 1 - (elapsedTime / animationDuration);

            float scaleValue = scaleCurve.Evaluate(animationTime);
            transform.localScale = initialScale * scaleValue;

            yield return null;
        }

        if (isForward)
        {
            yield return new WaitForSeconds(initialDelay);
            StartCoroutine(AnimateFlowerSprite(true));
            Broadcaster.Broadcast("PlaySFX", "pop");
            yield return new WaitForSeconds(delayBetweenObjects);
            StartCoroutine(AnimateExtraSprite(true));
            Broadcaster.Broadcast("PlaySFX", "pop");
            yield return new WaitForSeconds(delayBetweenObjects);
            StartCoroutine(AnimateTextMeshPro(textMeshPro1, initialTextScale1, true));
            yield return new WaitForSeconds(delayBetweenObjects);
            StartCoroutine(AnimateTextMeshPro(textMeshPro2, initialTextScale2, true));
            Broadcaster.Broadcast("PlaySFX", "pop");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator AnimateFlowerSprite(bool isForward)
    {
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            animationTime = isForward ? elapsedTime / animationDuration : 1 - (elapsedTime / animationDuration);

            float scaleValue = scaleCurve.Evaluate(animationTime);
            flowerSpriteRenderer.transform.localScale = initialFlowerScale * scaleValue;

            yield return null;
        }
    }

    IEnumerator AnimateExtraSprite(bool isForward)
    {
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            animationTime = isForward ? elapsedTime / animationDuration : 1 - (elapsedTime / animationDuration);

            float scaleValue = scaleCurve.Evaluate(animationTime);
            extraSpriteRenderer.transform.localScale = initialExtraSpriteScale * scaleValue;

            yield return null;
        }
    }

    IEnumerator AnimateTextMeshPro(TextMeshProUGUI textMeshPro, Vector3 initialScale, bool isForward)
    {
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            animationTime = isForward ? elapsedTime / animationDuration : 1 - (elapsedTime / animationDuration);

            float scaleValue = scaleCurve.Evaluate(animationTime);
            textMeshPro.transform.localScale = initialScale * scaleValue;

            yield return null;
        }
    }

    public void DestroyThoughtBubble()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateFlowerSprite(false));
        StartCoroutine(AnimateExtraSprite(false));
        StartCoroutine(AnimateTextMeshPro(textMeshPro1, initialTextScale1, false));
        StartCoroutine(AnimateTextMeshPro(textMeshPro2, initialTextScale2, false));
        StartCoroutine(AnimateThoughtBubble(false));
    }
}
