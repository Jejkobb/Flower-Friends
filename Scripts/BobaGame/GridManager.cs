using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject pearlPrefab;  // The tapioca pearl prefab
    public GameObject strawPrefab;  // The straw prefab

    public int rows = 5;
    public int cols = 6;
    public float pearlSpacing = 1.5f;  // Spacing between pearls

    private GameObject[,] grid;  // The grid to hold pearls and temporary straw positions
    private Vector3[,] targetPositions; // Stores target positions for lerping
    private Quaternion[,] targetRotations; // Stores target rotations for lerping

    public float moveSpeed = 5f;  // Speed of position lerp movement
    public float rotationSpeed = 2f;  // Speed of rotation lerp movement
    public float suctionSpeed = 3f;  // Speed at which pearls get sucked into the straw
    public float suctionDelay = 0f;  // Delay before sucking pearls to avoid overlap
    public float suctionDelayIncrement = 0.2f;  // Amount to increment the delay after each repeat

    private int strawCol;  // The column where the straw is
    private int strawRow;  // The row position of the straw's bottom
    private Vector3 strawTargetPosition;  // Target position for the straw

    private Vector2 lastMoveDirection;  // To store the last move direction
    private bool pearlsSuckedUp;  // To track if pearls were sucked up
    private bool inputDisabled;  // To track if input is currently disabled
    private int suckingPearlsCount;  // To track the number of pearls being sucked up

    public LevelController levelController;

    void Start()
    {
        levelController.SetLevel(0);

        grid = new GameObject[rows, cols];
        targetPositions = new Vector3[rows, cols];
        targetRotations = new Quaternion[rows, cols];
        PlaceStraw();
        CreateGrid();
    }

    // Create the grid and instantiate pearls
    void CreateGrid()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                // Don't spawn pearls in the straw's column starting from the straw's row and up
                if (col == strawCol && row >= strawRow)
                    continue;

                if (Random.Range(0f, 1f) > 0.5f)
                {
                    Vector3 pos = new Vector3(col * pearlSpacing, row * pearlSpacing, 0);
                    Quaternion rot = GetRandomRotation(360f, 360f, 360f);
                    grid[row, col] = Instantiate(pearlPrefab, pos, Quaternion.identity);
                    grid[row, col].transform.rotation = rot;
                    targetPositions[row, col] = pos; // Set initial target positions
                    targetRotations[row, col] = rot; // Initial rotation
                }
            }
        }
    }

    // Place the straw in a random column and a row above the bottom
    void PlaceStraw()
    {
        strawCol = Random.Range(5, 5);  // Choose a random column for the straw
        strawRow = Random.Range(2, 2);  // Choose a random row for the straw, but not the bottom (so from row 1 and up)

        Vector3 strawPos = new Vector3(strawCol * pearlSpacing, strawRow * pearlSpacing, 0);
        GameObject strawInstance = Instantiate(strawPrefab, strawPos, Quaternion.identity);
        strawTargetPosition = strawPos;  // Set the initial target position for the straw
    }

    int LevelToGrid()
    {
        return 7 - levelController.currentLevelIndex;
    }

    void Update()
    {
        // Lerp the straw's position to the target
        UpdateStrawTargetPosition();
        LerpStrawToTargetPosition();

        // Disable input if pearls are currently being sucked up
        if (!inputDisabled)
        {
            // Listen for directional input
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                lastMoveDirection = Vector2.left;
                MovePearls(lastMoveDirection);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                lastMoveDirection = Vector2.right;
                MovePearls(lastMoveDirection);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                lastMoveDirection = Vector2.up;
                MovePearls(lastMoveDirection);  // Corrected: Up now corresponds to moving up in the game
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                lastMoveDirection = Vector2.down;
                MovePearls(lastMoveDirection);  // Corrected: Down now corresponds to moving down
            }
        }

        // Lerp all pearls to their target positions and rotations
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (grid[row, col] != null)
                {
                    // Lerp position
                    grid[row, col].transform.position = Vector3.Lerp(grid[row, col].transform.position, targetPositions[row, col], Time.deltaTime * moveSpeed);

                    // Lerp rotation (with a slower speed)
                    grid[row, col].transform.rotation = Quaternion.Lerp(grid[row, col].transform.rotation, targetRotations[row, col], Time.deltaTime * rotationSpeed);
                }
            }
        }
    }

    // Update the straw's target position based on its row
    void UpdateStrawTargetPosition()
    {
        strawTargetPosition = new Vector3(strawCol * pearlSpacing, strawRow * pearlSpacing, 0);
    }

    // Lerp the straw to its target position
    void LerpStrawToTargetPosition()
    {
        GameObject strawInstance = GameObject.FindWithTag("Straw");  // Assuming straw has the tag "Straw"
        if (strawInstance != null)
        {
            strawInstance.transform.position = Vector3.Lerp(strawInstance.transform.position, strawTargetPosition, Time.deltaTime * moveSpeed);
        }
    }

    // Function to temporarily add the straw to the grid for movement checks
    void AddStrawToGrid()
    {
        for (int row = strawRow; row < rows; row++)
        {
            grid[row, strawCol] = strawPrefab;  // Temporarily add the straw to the grid
        }
    }

    // Function to remove the straw from the grid after movement checks
    void RemoveStrawFromGrid()
    {
        for (int row = strawRow; row < rows; row++)
        {
            grid[row, strawCol] = null;  // Remove the straw from the grid
        }
    }

    Quaternion GetRandomRotation(float x, float y, float z)
    {
        return Quaternion.Euler(Random.Range(0, 0), Random.Range(0, 0), Random.Range(0, z));
    }

    // Function to move pearls based on direction and add random rotation
    void MovePearls(Vector2 direction)
    {
        AddStrawToGrid();  // Temporarily add the straw to the grid for movement checks
        pearlsSuckedUp = false;  // Reset the flag for each move

        if (direction == Vector2.left)
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 1; col < cols; col++) // Start from the second column to avoid edge
                {
                    if (grid[row, col] != null && grid[row, col] != strawPrefab)
                    {
                        int targetCol = col;
                        // Move left until the pearl hits the edge, another pearl, or the straw's column
                        while (targetCol - 1 >= 0 && grid[row, targetCol - 1] == null)
                        {
                            targetCol--;
                        }

                        // Update grid and target positions
                        if (targetCol != col)
                        {
                            grid[row, targetCol] = grid[row, col];
                            grid[row, col] = null;
                            targetPositions[row, targetCol] = new Vector3(targetCol * pearlSpacing, row * pearlSpacing, 0);

                            // Set random target rotation
                            targetRotations[row, targetCol] = GetRandomRotation(360f, 360f, 360f);
                        }
                    }
                }
            }
        }
        else if (direction == Vector2.right)
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = cols - 2; col >= 0; col--) // Start from the second last column
                {
                    if (grid[row, col] != null && grid[row, col] != strawPrefab)
                    {
                        int targetCol = col;
                        // Move right until the pearl hits the edge, another pearl, or the straw's column
                        while (targetCol + 1 < cols && grid[row, targetCol + 1] == null)
                        {
                            targetCol++;
                        }

                        // Update grid and target positions
                        if (targetCol != col)
                        {
                            grid[row, targetCol] = grid[row, col];
                            grid[row, col] = null;
                            targetPositions[row, targetCol] = new Vector3(targetCol * pearlSpacing, row * pearlSpacing, 0);

                            // Set random target rotation
                            targetRotations[row, targetCol] = GetRandomRotation(360f, 360f, 360f);
                        }
                    }
                }
            }
        }
        else if (direction == Vector2.up)
        {
            for (int col = 0; col < cols; col++)
            {
                for (int row = rows - 2; row >= 0; row--) // Start from the second last row
                {
                    if (grid[row, col] != null && grid[row, col] != strawPrefab)
                    {
                        int targetRow = row;
                        // Move up until the pearl hits the edge, another pearl, or the straw (it can pass under the straw)
                        while (targetRow + 1 < rows && grid[targetRow + 1, col] == null)
                        {
                            targetRow++;
                        }

                        // Update grid and target positions
                        if (targetRow != row)
                        {
                            grid[targetRow, col] = grid[row, col];
                            grid[row, col] = null;
                            targetPositions[targetRow, col] = new Vector3(col * pearlSpacing, targetRow * pearlSpacing, 0);

                            // Set random target rotation
                            targetRotations[targetRow, col] = GetRandomRotation(360f, 360f, 360f);
                        }
                    }
                }
            }
        }
        else if (direction == Vector2.down)
        {
            for (int col = 0; col < cols; col++)
            {
                for (int row = 1; row < rows; row++) // Start from the second row
                {
                    if (grid[row, col] != null && grid[row, col] != strawPrefab)
                    {
                        int targetRow = row;
                        // Move down until the pearl hits the edge, another pearl, or the straw (it can pass under the straw)
                        while (targetRow - 1 >= 0 && grid[targetRow - 1, col] == null)
                        {
                            targetRow--;
                        }

                        // Update grid and target positions
                        if (targetRow != row)
                        {
                            grid[targetRow, col] = grid[row, col];
                            grid[row, col] = null;
                            targetPositions[targetRow, col] = new Vector3(col * pearlSpacing, targetRow * pearlSpacing, 0);

                            // Set random target rotation
                            targetRotations[targetRow, col] = GetRandomRotation(360f, 360f, 360f);
                        }
                    }
                }
            }
        }

        RemoveStrawFromGrid();  // Remove the straw from the grid after movement checks

        // After each move, check for pearls under the straw and suck them up
        CheckPearlsUnderStraw();

        // If pearls were sucked up, repeat the last move
        if (pearlsSuckedUp)
        {
            suctionDelay += suctionDelayIncrement;  // Increment delay after each repeat
            MovePearls(lastMoveDirection);  // Repeat the last move
        }
        else
        {
            // Only update the strawRow and level once all pearls are sucked up
            if (suckingPearlsCount == 0)
            {
                if (LevelToGrid() <= strawRow)
                {
                    strawRow--;  // Decrease the straw row when the level increases
                    strawRow = Mathf.Max(0, strawRow);
                }
                suctionDelay = 0f;  // Reset delay when no more pearls are sucked up
                levelController.SetLevel(levelController.currentLevelIndex + 1);
                inputDisabled = false;  // Re-enable input after sucking finishes
            }
        }
    }

    // Check for pearls under the straw and suck them up
    void CheckPearlsUnderStraw()
    {
        inputDisabled = true;  // Disable input while sucking is happening
        for (int row = 0; row < strawRow; row++)  // Only check below the straw
        {
            if (grid[row, strawCol] != null)
            {
                GameObject pearl = grid[row, strawCol];
                grid[row, strawCol] = null;  // Remove the pearl from the grid

                pearlsSuckedUp = true;  // Mark that a pearl was sucked up
                suckingPearlsCount++;  // Increment the count of pearls being sucked

                // Start coroutine to animate the pearl being sucked into the straw
                StartCoroutine(SuckPearlIntoStraw(pearl, row));
            }
        }
    }

    // Coroutine to animate the pearl being sucked into the straw
    IEnumerator SuckPearlIntoStraw(GameObject pearl, int row)
    {
        Vector3 startPos = pearl.transform.position;

        // Apply delay before starting the sucking animation
        yield return new WaitForSeconds(suctionDelay);

        // Stage 1: Move pearl to the straw's X position while keeping the same Y position
        Vector3 intermediatePos = new Vector3(strawCol * pearlSpacing, targetPositions[row, strawCol].y, startPos.z);
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * suctionSpeed;
            pearl.transform.position = Vector3.Lerp(startPos, intermediatePos, t);
            yield return null;
        }

        suckingPearlsCount--;  // Decrement the number of pearls being sucked

        if (suckingPearlsCount == 0)
        {
            if (LevelToGrid() <= strawRow)
            {
                strawRow--;  // Decrease the straw row when the level increases
                strawRow = Mathf.Max(0, strawRow);
            }
            suctionDelay = 0f;  // Reset delay when no more pearls are sucked up
            levelController.SetLevel(levelController.currentLevelIndex + 1);
            inputDisabled = false;  // Re-enable input after sucking finishes
        }

        // Stage 2: Move pearl upwards by 10 units after aligning with the straw's X position
        Vector3 endPos = intermediatePos + new Vector3(0, 6f, 0);  // Move pearl upwards by 10 units
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * suctionSpeed;
            pearl.transform.position = Vector3.Lerp(intermediatePos, endPos, t);
            yield return null;
        }

        Destroy(pearl);  // Delete the pearl after the animation is complete
    }
}
