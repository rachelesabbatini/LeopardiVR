using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Opening of the Gameplay Environment: a 360° video plays as the skybox while the world is hidden.
/// When the video ends, the original skybox and the world come back
/// and the player is teleported to the starting point of the experience.
/// </summary>
public class IntroManagerVR : MonoBehaviour
{
    [Header("Video & Skybox Settings")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Material skyBoxVideo;     // Panoramic (360°) skybox material that shows the video

    [Header("World Settings")]
    [SerializeField] private GameObject[] objectsToHide; // world objects hidden during the video
    [SerializeField] private TeleportingPlayerVR teleporter;

    [Header("Player Settings")]
    [SerializeField] private GameObject XR_Origin;       // XR rig
    [SerializeField] private Vector3 playerStartPosition = new Vector3(128.8f, 14.08f, 819.6f);
    [SerializeField] private Quaternion playerStartRotation = Quaternion.identity;

    private Material originalSkyBox; // saved automatically at startup

    void Start()
    {
        // Place the player where the video should be watched from.
        if (XR_Origin != null)
        {
            XR_Origin.transform.position = playerStartPosition;
            XR_Origin.transform.rotation = playerStartRotation;
        }

        // Remember the scene's own skybox, to restore it later.
        originalSkyBox = RenderSettings.skybox;

        // Hide the world during the video.
        foreach (var obj in objectsToHide)
            if (obj != null)
            {
                obj.SetActive(false);
            }


        // Show the video as the skybox.
        if (skyBoxVideo != null)
            RenderSettings.skybox = skyBoxVideo;

        // Start the video and get notified when it ends.
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += FineCutscene;
            videoPlayer.Play();
        }
        else
        {
            Debug.LogError("IntroManagerVR: VideoPlayer not assigned in the Inspector!");
        }
    }

    // Called when the video reaches its end.
    private void FineCutscene(VideoPlayer vp)
    {
        // 1. Restore the original skybox and update the ambient lighting to match it
        RenderSettings.skybox = originalSkyBox;
        DynamicGI.UpdateEnvironment();

        // 2. Stop and turn off the video
        videoPlayer.Stop();
        videoPlayer.loopPointReached -= FineCutscene;
        videoPlayer.gameObject.SetActive(false);

        // 3. Bring the world back
        foreach (var obj in objectsToHide)
            if (obj != null)
            {
                obj.SetActive(true);
            }

        // 4. Move the player to the start of the experience
        teleporter.TeleportPlayer();

    }
}
