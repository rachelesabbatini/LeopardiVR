using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// The book on Leopardi's desk. Selecting it opens the main menu canvas
/// (Studia / Gioca) with a sound. It can be activated only once.
/// </summary>
public class BookStudioVRInteraction : XRSimpleInteractable
{
    [SerializeField] private Outline outline;
    [SerializeField] private GameObject canvas; // main menu canvas
    [SerializeField] public AudioSource noise;

    private bool hasBeenActivated = false;

    protected override void Awake()
    {
        base.Awake();
        if (outline != null) outline.enabled = false;
        if (canvas != null) canvas.SetActive(false);
    }

    // Highlight while the controller's ray points at the book.
    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        SetHighlight(true);
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        SetHighlight(false);
    }

    // Called when the user presses Select (trigger/grip) on the book.
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        ActivateStatue();
    }

    // Opens the menu (only the first time).
    private void ActivateStatue()
    {
        if (hasBeenActivated) return;

        hasBeenActivated = true;
        SetHighlight(false);

        if (canvas != null)
            canvas.SetActive(true);

        if (noise != null)
        {
            noise.Play();
        }
    }

    private void SetHighlight(bool value)
    {
        // Once the menu is open, the book is no longer highlighted.
        if (hasBeenActivated) value = false;

        if (outline != null)
            outline.enabled = value;
    }
}
