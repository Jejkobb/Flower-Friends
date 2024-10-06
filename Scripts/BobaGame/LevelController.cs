using UnityEngine;

public class LevelController : MonoBehaviour
{
    // Reference to the Renderer component that uses the shader
    public Renderer targetRenderer;

    // Array of YThreshold levels
    public float[] levels = { 0f, 0.083f, 0.21f, 0.344f, 0.477f, 0.61f, 0.743f, 0.876f };

    // Current level index
    [Range(0, 7)]
    public int currentLevelIndex = 0;

    // Speed of the lerp
    public float lerpSpeed = 1.0f;

    // Internal variables
    private float targetYThreshold;
    private float currentYThreshold;

    // Reference to the main orthographic camera
    public Camera mainCamera;

    void Start()
    {
        if (targetRenderer == null)
        {
            Debug.LogError("LevelController: Target Renderer is not assigned.");
            enabled = false;
            return;
        }

        if (mainCamera == null || !mainCamera.orthographic)
        {
            Debug.LogError("LevelController: Main camera is either not assigned or not orthographic.");
            enabled = false;
            return;
        }

        // Initialize YThreshold to the starting level
        targetYThreshold = levels[currentLevelIndex];
        currentYThreshold = targetYThreshold;

        // Set the initial _YThreshold value in the shader
        targetRenderer.material.SetFloat("_YThreshold", currentYThreshold);
    }

    void Update()
    {
        // Smoothly interpolate the currentYThreshold towards the targetYThreshold
        currentYThreshold = Mathf.Lerp(currentYThreshold, targetYThreshold, Time.deltaTime * lerpSpeed);

        // Update the shader property
        targetRenderer.material.SetFloat("_YThreshold", currentYThreshold);
    }

    // Public method to set the level index
    public void SetLevel(int levelIndex)
    {
        if (levelIndex < 0)
        {
            levelIndex = 0;
        }

        if (levelIndex >= levels.Length)
        {
            levelIndex = levels.Length - 1;
        }

        currentLevelIndex = levelIndex;
        targetYThreshold = levels[currentLevelIndex];

        print(7 - currentLevelIndex);
    }

    // Function to get the world Y position from the current YThreshold
    public float GetWorldYPositionFromThreshold()
    {
        if (mainCamera == null || !mainCamera.orthographic)
        {
            Debug.LogError("Main camera is either not assigned or not orthographic.");
            return 0;
        }

        // Get the top and bottom world positions in Y from the orthographic camera
        float cameraHeight = mainCamera.orthographicSize * 2f;  // Total height of the camera view
        float worldBottomY = mainCamera.transform.position.y - mainCamera.orthographicSize;
        float worldTopY = mainCamera.transform.position.y + mainCamera.orthographicSize;

        // Calculate the world Y position based on the YThreshold (normalized value 0 to 1)
        float worldY = Mathf.Lerp(worldBottomY, worldTopY, currentYThreshold);

        return worldY;
    }
}
