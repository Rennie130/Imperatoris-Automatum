using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    private Camera cam;

    private IInteractable currentHover;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Vector2 mouse = cam.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mouse, Vector2.zero);

        IInteractable interactable = null;

        if (hit.collider != null)
        {
            interactable = hit.collider.GetComponent<IInteractable>();
        }

        if (interactable != currentHover)
        {
            currentHover?.HoverExit();

            currentHover = interactable;

            currentHover?.HoverEnter();
        }   

        if (Input.GetMouseButtonDown(0))
        {
            currentHover?.Interact();
        }
    }
}
