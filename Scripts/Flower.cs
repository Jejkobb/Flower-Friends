using UnityEngine;
using System.Collections.Generic;

public class Flower : MonoBehaviour
{
    public List<Sprite> stemSprites = new List<Sprite>();
    public List<Sprite> flowerSprites = new List<Sprite>();
    public List<SpriteRenderer> leafSprites = new List<SpriteRenderer>();

    public SpriteRenderer stemSpriteRenderer;
    public SpriteRenderer flowerSpriteRenderer;

    public Vector3 offset;
    private Vector3 originalPosition;
    private bool isDragging = false;
    private Camera mainCamera;

    public float proximityThreshold = 3.5f;

    public bool taken = false;

    void Start()
    {
        mainCamera = Camera.main;
        originalPosition = transform.position;

        SetRandomStemAndFlowerAndLeaf();
    }

    void Update()
    {
        if (isDragging)
        {
            DragFlower();
        }
    }

    void OnMouseDown()
    {
        if (taken) return;

        isDragging = true;

        Broadcaster.Broadcast("PlaySFX", "pull");

        offset = transform.position - GetMouseWorldPosition();
    }

    void OnMouseUp()
    {
        if (taken) return;

        isDragging = false;
        CheckDropLocation();
    }

    void DragFlower()
    {
        if (taken) return;

        Vector3 newPosition = GetMouseWorldPosition() + offset;
        newPosition.z = 0;
        transform.position = newPosition;
    }

    void CheckDropLocation()
    {
        GameObject nearestNPC = FindNearestObjectWithTag("NPC");
        GameObject nearestThoughtBubble = FindNearestObjectWithTag("ThoughtBubble");

        GameObject firstNPCInQueue = FindFirstNPCInQueue();

        if (nearestNPC != null && firstNPCInQueue != null && nearestNPC == firstNPCInQueue)
        {
            if (Vector3.Distance(transform.position, nearestNPC.transform.position) <= proximityThreshold)
            {
                OnFlowerReleasedNearNPC(nearestNPC, nearestThoughtBubble);
                return;
            }
        }

        ResetPosition();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Broadcaster.Broadcast("PlaySFX", "step");
    }

    public void AddPhysicsToFlower()
    {
        Destroy(GetComponent<BoxCollider2D>());

        gameObject.transform.parent = null;
        gameObject.AddComponent<PolygonCollider2D>();

        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();

        rb.AddForce(Vector2.up * 10f, ForceMode2D.Impulse);
        rb.AddTorque(50f);
    }

    GameObject FindNearestObjectWithTag(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        GameObject nearestObject = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject obj in objects)
        {
            float distance = Vector3.Distance(transform.position, obj.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestObject = obj;
            }
        }

        return nearestObject;
    }

    GameObject FindFirstNPCInQueue()
    {
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
        GameObject closestNPC = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject npc in npcs)
        {
            float distanceToOrigin = Mathf.Abs(npc.transform.position.x);
            if (distanceToOrigin < minDistance)
            {
                minDistance = distanceToOrigin;
                closestNPC = npc;
            }
        }

        return closestNPC;
    }

    void ResetPosition()
    {
        transform.position = originalPosition;
    }

    void OnFlowerReleasedNearNPC(GameObject npc, GameObject thoughtBubble)
    {
        if (thoughtBubble != null)
        {
            ThoughtBubble bubble = thoughtBubble.GetComponent<ThoughtBubble>();
            Sprite bubbleFlowerSprite = bubble.GetCurrentFlowerSprite();
            int bubbleLeafCount = int.Parse(bubble.textMeshPro2.text);

            if (flowerSpriteRenderer.sprite == bubbleFlowerSprite && GetEnabledLeafCount() == bubbleLeafCount)
            {
                transform.SetParent(npc.transform);

                transform.localPosition = new Vector3(-0.911f, 1.273f, 0f);
                transform.localRotation = Quaternion.Euler(0, 0, -12.952f);
                transform.localScale = new Vector3(1f, 1f, 1f);

                GameController gameController = FindObjectOfType<GameController>();
                gameController.SendNPCForward(npc);

                bubble.DestroyThoughtBubble();

                gameObject.tag = "Untagged";
                taken = true;

                npc.tag = "Untagged";
                npc.GetComponentInChildren<SpriteRenderer>().sortingOrder = 260;

                Broadcaster.Broadcast("PlaySFX", "win");

                return;
            }
        }

        ResetPosition();
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(mainCamera.transform.position.z);
        return mainCamera.ScreenToWorldPoint(mousePosition);
    }

    void SetRandomStemAndFlowerAndLeaf()
    {
        if (stemSprites.Count > 0)
        {
            Sprite randomStem = stemSprites[Random.Range(0, stemSprites.Count)];
            stemSpriteRenderer.sprite = randomStem;
        }

        int leafsToEnable = Random.Range(0, leafSprites.Count + 1);
        for (int i = 0; i < leafSprites.Count; i++)
        {
            leafSprites[i].enabled = i < leafsToEnable;
        }

        if (flowerSprites.Count > 0)
        {
            Sprite randomFlower = flowerSprites[Random.Range(0, flowerSprites.Count)];
            flowerSpriteRenderer.sprite = randomFlower;
        }
    }

    public Sprite GetCurrentFlowerSprite()
    {
        return flowerSpriteRenderer.sprite;
    }

    public int GetEnabledLeafCount()
    {
        int count = 0;
        foreach (SpriteRenderer leaf in leafSprites)
        {
            if (leaf.enabled)
            {
                count++;
            }
        }
        return count;
    }
}
