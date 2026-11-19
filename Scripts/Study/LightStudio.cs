using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// The desk lamp in Leopardi's study, one of the tutorial objects:
/// selecting it switches the light on and off with a click sound.
/// ToggleLight and SetHighlight are connected to the interactable's events in the Inspector.
/// </summary>
public class LightStudio : XRSimpleInteractable
{
    [Header("Visuals")]
    [SerializeField] private Outline outline;

    [Header("References")]
    [SerializeField] private GameObject luce; // the light object to switch on and off

    [Header("Audio")]
    [SerializeField] private AudioSource switchon;

    // Current state of the light
    private bool isLightOn = false;

    protected override void Awake()
    {
        base.Awake();

        if (outline != null) outline.enabled = false;

        // Apply the initial state to the light.
        if (luce != null) luce.SetActive(isLightOn);
    }

    // Switches the light on or off (called on Select).
    public void ToggleLight()
    {
        if (luce == null) return;

        isLightOn = !isLightOn;
        luce.SetActive(isLightOn);

        if(switchon != null)
        {
            switchon.Play();
        }
    }

    // Visual feedback while the controller points at the lamp (called on Hover).
    public void SetHighlight(bool value)
    {
        if (outline != null)
            outline.enabled = value;
    }
}
