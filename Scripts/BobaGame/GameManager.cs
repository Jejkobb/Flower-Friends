using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject pearlPrefab;   // The tapioca pearl prefab
    public GameObject playerPearlPrefab;  // The player-controlled pearl prefab
    public GameObject strawPrefab;   // The straw prefab
    public GameObject roof;   // The straw prefab
    public float pearlSpacing = 1.5f;  // Spacing between pearls (for random positioning)

    public int numPearls = 10;       // Number of pearls to spawn
    public Vector2 gridSize = new Vector2(10, 5);  // The size of the "table" or play area

    public float suctionSpeed = 3f;  // Speed at which pearls get sucked into the straw
    public float suctionRadius = 1f;  // Radius around the straw to check for pearls to be sucked up

    private List<GameObject> pearls = new List<GameObject>();  // Store references to the spawned pearls
    private GameObject playerPearl;  // Reference to the player-controlled pearl
    private GameObject straw;  // Reference to the straw

    private GameObject selectedPearl;  // The pearl currently being selected/moved
    private Vector3 mouseStartPos;  // Where the mouse started dragging
    private Vector3 pearlStartPos;  // Original position of the selected pearl

    public LevelController levelController;

    void Start()
    {
        PlaceStraw();
        SpawnPearls();
        SpawnPlayerPearl();  // Spawn the player-controlled pearl
    }

    // Place the straw at a fixed position
    void PlaceStraw()
    {
        Vector3 strawPos = new Vector3(0, gridSize.y / 2, 0);  // Place the straw near the top
        straw = Instantiate(strawPrefab, strawPos, Quaternion.identity);
    }

    // Spawn the pearls randomly within the grid, but not near the straw
    void SpawnPearls()
    {
        for (int i = 0; i < numPearls; i++)
        {
            Vector3 randomPos;
            do
            {
                randomPos = new Vector3(Random.Range(-gridSize.x / 2, gridSize.x / 2), Random.Range(-gridSize.y / 2, gridSize.y / 2), 0);
            } while (Vector3.Distance(randomPos, straw.transform.position) < suctionRadius);  // Avoid placing pearls near the straw

            GameObject pearl = Instantiate(pearlPrefab, randomPos, Quaternion.identity);
            pearls.Add(pearl);
        }
    }

    // Spawn the player-controlled pearl
    void SpawnPlayerPearl()
    {
        Vector3 playerPearlPos;
        do
        {
            playerPearlPos = new Vector3(Random.Range(-gridSize.x / 2, gridSize.x / 2), Random.Range(-gridSize.y / 2, gridSize.y / 2), 0);
        } while (Vector3.Distance(playerPearlPos, straw.transform.position) < suctionRadius);  // Avoid placing the player pearl near the straw

        playerPearl = Instantiate(playerPearlPrefab, playerPearlPos, Quaternion.identity);
        pearls.Add(playerPearl);
        playerPearl.tag = "Player";  // Ensure the Player pearl has the "Player" tag
    }

    // Update is called once per frame
    void Update()
    {
        CheckSuckUpPearls();

        HandleMouseInput();

        roof.transform.position = new Vector3(roof.transform.position.x, -levelController.GetWorldYPositionFromThreshold(), roof.transform.position.z);
        straw.transform.position = new Vector3(straw.transform.position.x, Mathf.Max(-levelController.GetWorldYPositionFromThreshold()-0.5f, -4.16f), straw.transform.position.z);
    }

    // Check for pearls near the straw and suck them up if they're within the radius
    void CheckSuckUpPearls()
    {
        foreach (GameObject pearl in pearls)
        {
            if (pearl != null && Vector3.Distance(pearl.transform.position, straw.transform.position) < suctionRadius)
            {
                StartCoroutine(SuckPearlIntoStraw(pearl));
            }
            if (roof.transform.position.y < -3f)
            {
                if (pearl != null)
                {
                    pearl.GetComponent<Rigidbody2D>().isKinematic = true;
                    pearl.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    pearl.transform.position = Vector3.Lerp(pearl.transform.position, new Vector3(pearl.transform.position.x, -3.68f, pearl.transform.position.z), Time.deltaTime * suctionSpeed);
                }
            }
        }
    }

    // Coroutine to suck the pearl into the straw
    IEnumerator SuckPearlIntoStraw(GameObject pearl)
    {
        pearls.Remove(pearl);  // Remove from the list
        Vector3 endPos = straw.transform.position + new Vector3(0, 1f, 0);  // Move pearl upwards into the straw
        pearl.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        pearl.GetComponent<Rigidbody2D>().isKinematic = true;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * suctionSpeed;
            pearl.transform.position = Vector3.Lerp(pearl.transform.position, endPos, t);
            pearl.transform.position = Vector3.Lerp(pearl.transform.position, new Vector3(endPos.x, pearl.transform.position.y, pearl.transform.position.z), t * 2);
            yield return null;
        }

        Destroy(pearl);  // Destroy the pearl after being sucked up
    }

    // Handle mouse input to click, drag, and launch the player pearl
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;  // Zero out z-axis

            // Check if we are clicking on the Player pearl
            if (playerPearl != null && Vector3.Distance(mousePos, playerPearl.transform.position) < 0.5f)
            {
                selectedPearl = playerPearl;  // Select the Player pearl
                mouseStartPos = mousePos;
                pearlStartPos = playerPearl.transform.position;
            }
        }

        if (Input.GetMouseButton(0) && selectedPearl != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector3 direction = (mousePos - pearlStartPos).normalized;
            float distance = Vector3.Distance(mouseStartPos, mousePos);

            // Print the direction and distance while dragging
            Debug.Log("Direction: " + direction + ", Distance: " + distance);
        }

        if (Input.GetMouseButtonUp(0) && selectedPearl != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            // Calculate distance and direction from start position to the current mouse position
            float distance = Vector3.Distance(mouseStartPos, mousePos);
            Vector3 direction = (mousePos - pearlStartPos).normalized;

            // Apply force to the selected pearl based on distance and direction
            Rigidbody2D rb = selectedPearl.GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
            rb.AddForce(direction * distance * 200);  // Launch with a multiplier for power

            // Apply angular velocity (rotation) based on direction and distance in 2D
            float torqueAmount = distance * 10f;  // Adjust the multiplier to control the spin speed

            // Calculate cross product for determining the spin direction in 2D
            float spinDirection = Mathf.Sign(Vector2.SignedAngle(Vector2.right, direction));  // Get direction (-1 or 1) based on angle

            // Apply torque (angular velocity) to the Rigidbody2D
            rb.angularVelocity = 0;  // Reset the current angular velocity
            rb.AddTorque(spinDirection * torqueAmount);

            // Level progression
            levelController.SetLevel(levelController.currentLevelIndex + 1);

            // Deselect the pearl
            selectedPearl = null;
        }

    }
}
