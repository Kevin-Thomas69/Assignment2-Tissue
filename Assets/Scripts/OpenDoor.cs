using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class OpenDoor : MonoBehaviour
{
    public bool isOpen = false;  
    public bool playerInRange = false;  
    public bool lockLeft = true;
    public Vector3 openTarget = new Vector3(0,110,0);
    public Vector3 closeTarget = new Vector3(0,0,0);
    [Header("Nav Obstacle")]
    public NavMeshObstacle navObstacle;
    public bool useCarving = true;
    [Header("Interaction")]
    public bool allowPlayerToggle = true;
    public bool disableAnimatorWhenForcedOpen = true;

    private void Start()
    {
        if (!lockLeft)
        {
            openTarget = new Vector3(0,-110,0);
        }

        if (navObstacle != null)
        {
            navObstacle.carving = useCarving;
        }
        if (isOpen)
        {
            transform.localRotation = Quaternion.Euler(openTarget);
            ApplyNavBlock(true);
            if (!allowPlayerToggle && disableAnimatorWhenForcedOpen)
            {
                var anim = GetComponent<Animator>();
                if (anim != null) anim.enabled = false;
            }
        }
        else
        {
            ApplyNavBlock(false);
        }
    }
    void Update()
    {
        if (!allowPlayerToggle)
        {
            if (!isOpen)
            {
                isOpen = true;
                ApplyNavBlock(true);
            }
            transform.localRotation = Quaternion.Euler(openTarget);
            return;
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.F) && !isOpen)
        {
            transform.DOLocalRotate(openTarget, 0.6f).SetEase(Ease.Linear).OnComplete(() =>
            {
                isOpen = true;
                ApplyNavBlock(true);
            });
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.F) && isOpen)
        {
            transform.DOLocalRotate(closeTarget, 0.6f).SetEase(Ease.Linear).OnComplete(() =>
            {
                isOpen = false;
                ApplyNavBlock(false);
            });
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  
        {
            playerInRange = true;  
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))  
        {
            playerInRange = false;  
        }
    }

    private void ApplyNavBlock(bool nowOpen)
    {
        if (navObstacle == null) return;
        if (nowOpen)
        {
            navObstacle.enabled = false;
        }
        else
        {
            navObstacle.enabled = true;
        }
    }

    public void SetOpenFromTimeline()
    {
        isOpen = true;
        ApplyNavBlock(true);
        allowPlayerToggle = false;
    }

    public void AllowPlayerToggle(bool allow)
    {
        allowPlayerToggle = allow;
    }
}