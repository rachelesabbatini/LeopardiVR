using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

/// <summary>
/// Moves the player to a fixed point through the XR Interaction Toolkit's teleportation system,
/// for the moments when the story (not the user) decides where to go:
/// after the intro video, after the labyrinth door, and so on.
/// </summary>
public class TeleportingPlayerVR : MonoBehaviour
{
    // Teleportation Provider on the XR rig
    [SerializeField] private TeleportationProvider teleportationProvider;

    // Where the player should land
    [SerializeField] private Transform destinationPoint;

    public void TeleportPlayer()
    {
        if (teleportationProvider == null || destinationPoint == null)
        {
            Debug.LogError("Teleport VR: Teleportation Provider or destination point not assigned!");
            return;
        }

        // Build the teleport request: position, rotation, and match the destination's facing
        TeleportRequest request = new TeleportRequest()
        {
            destinationPosition = destinationPoint.position,
            destinationRotation = destinationPoint.rotation,
            matchOrientation = MatchOrientation.TargetUpAndForward
        };

        // Queue it: the provider applies it safely in its own update
        teleportationProvider.QueueTeleportRequest(request);
    }
}
