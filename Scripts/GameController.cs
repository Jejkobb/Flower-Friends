using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GameController : MonoBehaviour
{
    public NPCBehavior npcPrefab;
    public ThoughtBubble thoughtBubblePrefab;
    private List<NPCBehavior> npcList = new List<NPCBehavior>();
    public float npcSpawnInterval = 3f;
    private int totalNPCsSpawned = 0;
    private int initialFlowerCount;
    private float nextSpawnTime;

    public bool ending = false;
    GameObject finalNPC;

    private Camera cam;

    public GameObject TheEndCard;
    public GameObject bird;

    void Start()
    {
        cam = Camera.main;

        initialFlowerCount = GameObject.FindGameObjectsWithTag("Flower").Length;
    }

    private float cameraSmoothTime = 1.5f; // Time it takes for the camera to smoothly follow the NPC
    private float cameraVelocity = 0.0f;

    void Update()
    {
        if (ending == true)
        {
            float targetX = finalNPC.transform.position.x;
            float smoothX = Mathf.SmoothDamp(cam.transform.position.x, targetX, ref cameraVelocity, cameraSmoothTime);

            if (cam.transform.position.x < -15f && TheEndCard.activeSelf == false)
            {
                TheEndCard.SetActive(true);
            }

            cam.transform.position = new Vector3(smoothX, cam.transform.position.y, cam.transform.position.z);

            return;
        }

        if (Time.time >= nextSpawnTime && totalNPCsSpawned < initialFlowerCount && npcList.Count < initialFlowerCount)
        {
            SpawnNPC();
            nextSpawnTime = Time.time + npcSpawnInterval;
        }

        ManageNPCMovement();

        CheckGameEndCondition();
    }

    void SpawnNPC()
    {
        NPCBehavior newNpc = Instantiate(npcPrefab, new Vector3(12, 0, 0), Quaternion.identity);
        newNpc.SetTargetPosition(new Vector3(0, 0, 0), OnNPCReachedTarget);
        npcList.Add(newNpc);
        totalNPCsSpawned++;
    }

    void ManageNPCMovement()
    {
        for (int i = 0; i < npcList.Count; i++)
        {
            NPCBehavior currentNPC = npcList[i];

            if (i > 0)
            {
                NPCBehavior npcInFront = npcList[i - 1];
                float distanceToNpcInFront = Vector3.Distance(currentNPC.transform.position, npcInFront.transform.position);

                if (distanceToNpcInFront < 2.5f)
                {
                    currentNPC.StopBehind();
                }
                else if (currentNPC.isStopped)
                {
                    currentNPC.ResumeWalking();
                }
            }
        }

        CleanUpNPCs();
    }

    void OnNPCReachedTarget(NPCBehavior npc)
    {
        if (ending == true)
        {
            //TheEndCard.SetActive(true);
            Broadcaster.Broadcast("PlaySFX", "win");

            StartCoroutine(ActivateBird());
            return;
        }

        NPCBehavior closestNPC = FindClosestNPCToZero();

        if (closestNPC == npc)
        {
            SpawnThoughtBubble();
        }
    }

    private IEnumerator ActivateBird()
    {
        yield return new WaitForSeconds(3f);
        bird.SetActive(true);
    }

    NPCBehavior FindClosestNPCToZero()
    {
        NPCBehavior closestNPC = null;
        float minDistance = Mathf.Infinity;

        foreach (NPCBehavior npc in npcList)
        {
            float distance = Mathf.Abs(npc.transform.position.x);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestNPC = npc;
            }
        }

        return closestNPC;
    }

    void CleanUpNPCs()
    {
        for (int i = npcList.Count - 1; i >= 0; i--)
        {
            NPCBehavior npc = npcList[i];
            if (npc.transform.position.x <= -12)
            {
                npcList.RemoveAt(i);
                if (npcList.Count == 0)
                {
                    // START ENDING CUTSCENE
                    finalNPC = npc.gameObject;

                    finalNPC.GetComponent<NPCBehavior>().SetTargetPosition(new Vector3(-24, 0, 0), OnNPCReachedTarget);
                    ending = true;
                }
                else
                {
                    Destroy(npc.gameObject);
                }
            }
        }
    }

    public void SendNPCForward(GameObject npc)
    {
        NPCBehavior npcBehavior = npc.GetComponent<NPCBehavior>();
        if (npcBehavior != null)
        {
            npcBehavior.SetTargetPosition(new Vector3(-13f, 0, 0), OnNPCReachedTarget);
            npcBehavior.ActivateHolding();
            npcBehavior.ResumeWalking();
        }
    }

    void SpawnThoughtBubble()
    {
        Vector3 bubblePosition = new Vector3(-1.25f, 3f, 14);

        Instantiate(thoughtBubblePrefab, bubblePosition, Quaternion.identity);
    }

    void CheckGameEndCondition()
    {
        if (totalNPCsSpawned >= initialFlowerCount && npcList.Count == 0)
        {
            // WINNNN
            Debug.Log("Game Over: All NPCs and Flowers have been processed!");
        }
    }
}
