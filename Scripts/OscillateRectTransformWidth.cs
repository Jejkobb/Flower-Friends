using UnityEngine;

public class OscillateRectTransformWidth : MonoBehaviour
{
    public RectTransform rectTransform;
    public float minWidth = 670f;
    public float maxWidth = 700f;
    public float oscillationSpeed = 1f;

    private float currentTime = 0f;

    void Update()
    {
        currentTime += Time.deltaTime * oscillationSpeed;

        float sineValue = Mathf.Sin(currentTime);

        float width = Mathf.Lerp(minWidth, maxWidth, (sineValue + 1) / 2);

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }
}
