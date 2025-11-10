using UnityEngine;

public class CameraPickup : MonoBehaviour
{
    public Camera playerCamera;
    public float pickupRange = 3f;
    public LayerMask pickupMask;

    private OutlineHighlight lastHighlight;

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * pickupRange, Color.green);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupMask))
        {
            if (hit.collider.CompareTag("Pickable"))
            {
                OutlineHighlight oh = hit.collider.GetComponent<OutlineHighlight>();
                if (oh != null && lastHighlight != oh)
                {
                    if (lastHighlight != null) lastHighlight.Highlight(false);
                    oh.Highlight(true);
                    lastHighlight = oh;
                }

                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (hit.collider.gameObject.GetComponent<PickUpItems>() != null)
                    {
                        hit.collider.gameObject.GetComponent<PickUpItems>().PickUp();
                    }
                    else if (hit.collider.gameObject.GetComponent<Tissue>() != null)
                    {
                        Tissue.Instance.PickedUpTissue();
                    }
                }
            }
        }
        else
        {
            if (lastHighlight != null)
            {
                lastHighlight.Highlight(false);
                lastHighlight = null;
            }
        }
    }
}
