using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    private void DestroyBird()
    {
        Destroy(gameObject);
    }

    private void SwoopSound()
    {
        Broadcaster.Broadcast("PlaySFX", "swoop");
    }

    private void BirdScreech()
    {
        Broadcaster.Broadcast("PlaySFX", "bird");
    }

    private void TriggerMiddlePart()
    {
        FindObjectOfType<Flower>().AddPhysicsToFlower();
        FindObjectOfType<NPCBehavior>().gameObject.SetActive(false);
        FindObjectOfType<SoundManager>().StopMusic();
    }
}
