using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Shows subtitles in sync with a voice line and keeps the panel floating in front of the user in VR.
/// Subtitle timings are loaded from JSON files in a Resources folder.
/// Objects in the scene start it through the static Instance.
/// </summary>
public class SubtitleManager : MonoBehaviour
{
    // Static access, so triggers and interactables can reach the manager without a direct reference
    public static SubtitleManager Instance;

    [System.Serializable]
    public class SubtitleLine
    {
        public string text;
        public float start; // seconds from the start of the audio
        public float end;
    }

    // JsonUtility cannot read a top-level JSON array, so the array is wrapped in an object
    [System.Serializable]
    private class SubtitleArrayWrapper
    {
        public SubtitleLine[] lines;
    }

    [Header("Riferimenti UI")]
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private GameObject subtitlePanel;

    [Header("Impostazioni Inseguimento VR")]
    [SerializeField] private float distanceFromPlayer = 2.0f;
    [SerializeField] private float smoothSpeed = 3.0f;
    [SerializeField] private float verticalOffset = -0.3f; // slightly below eye level, easier to read

    private List<SubtitleLine> subtitles;
    private int currentIndex = -1;
    private Transform mainCameraTransform;
    private AudioSource currentAudioSource;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }

        // The subtitles start hidden
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        if (subtitleText != null) subtitleText.text = "";
    }

    // LateUpdate runs after the headset pose is updated, so the panel follows without lag
    private void LateUpdate()
    {
        // 1. Text in sync with the audio (only while it is playing)
        if (currentAudioSource != null && currentAudioSource.isPlaying)
        {
            UpdateSubtitleLogic(currentAudioSource.time);
        }
        else if (currentAudioSource != null && !currentAudioSource.isPlaying && subtitlePanel.activeSelf)
        {
            // The audio has ended: hide the panel
            CloseSubtitles();
        }

        // 2. Smooth follow (only while the panel is visible)
        if (subtitlePanel != null && subtitlePanel.activeSelf)
        {
            FollowPlayerLogic();
        }
    }

    // Loads the subtitle file, plays the voice line from the start and shows the panel
    public void StartSubtitles(string jsonFileName, AudioSource audioToPlay)
    {
        if (string.IsNullOrEmpty(jsonFileName) || audioToPlay == null) return;

        currentAudioSource = audioToPlay;

        // Load the timings from this line's JSON file
        LoadSubtitles(jsonFileName);

        // Play the audio from the beginning
        currentAudioSource.Stop();
        currentAudioSource.time = 0f;
        currentAudioSource.Play();

        currentIndex = -1;
        if (subtitlePanel != null) subtitlePanel.SetActive(true);
    }

    public void CloseSubtitles()
    {
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        if (subtitleText != null) subtitleText.text = "";
        currentIndex = -1;
    }

    private void LoadSubtitles(string fileName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);

        if (jsonFile == null)
        {
            Debug.LogError($"SubtitleManager: JSON file '{fileName}' not found in Assets/Resources!");
            return;
        }

        string wrappedJson = "{\"lines\":" + jsonFile.text + "}";
        SubtitleArrayWrapper wrapper = JsonUtility.FromJson<SubtitleArrayWrapper>(wrappedJson);

        if (wrapper != null)
        {
            subtitles = new List<SubtitleLine>(wrapper.lines);
        }
    }

    private void UpdateSubtitleLogic(float currentTime)
    {
        if (subtitles == null || subtitles.Count == 0) return;

        for (int i = 0; i < subtitles.Count; i++)
        {
            var line = subtitles[i];

            if (currentTime >= line.start && currentTime <= line.end)
            {
                // Update the text only when the line changes, not every frame
                if (currentIndex != i)
                {
                    subtitleText.text = line.text;
                    currentIndex = i;
                    if (subtitlePanel != null) subtitlePanel.SetActive(true);
                }
                return;
            }
        }

        // Between two lines: clear the text
        subtitleText.text = "";
    }

    // Moves the panel smoothly to a point in front of the user and turns it to face the same way
    private void FollowPlayerLogic()
    {
        if (mainCameraTransform == null) return;

        // Target point in front of the user, slightly lower than the eyes
        Vector3 targetPosition = mainCameraTransform.position + (mainCameraTransform.forward * distanceFromPlayer);
        targetPosition.y += verticalOffset;

        // Smoothing avoids the panel snapping with every small head movement
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);

        // Face the same direction as the camera
        transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward,
                         mainCameraTransform.rotation * Vector3.up);
    }
}
