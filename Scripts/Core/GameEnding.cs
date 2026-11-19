using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.SceneManagement;

/// <summary>
/// Closing sequence of a reading path: fades the view to black with a quad in front
/// of the camera, shows the credits, then loads the hub scene (Leopardi's study).
/// </summary>
public class GameEnding : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private MeshRenderer fadeQuadRenderer;  // black quad in front of the camera
    [SerializeField] private CanvasGroup creditsCanvasGroup;

    [Header("Tempi")]
    [SerializeField] private float fadeDuration = 2.5f;
    [SerializeField] private float creditsDelay = 1.0f;
    [SerializeField] private float displayDuration = 4.0f;

    private Material _fadeMaterial;

    void Awake()
    {
        // 1. Fade material: black but fully transparent, quad hidden
        if (fadeQuadRenderer != null)
        {
            _fadeMaterial = fadeQuadRenderer.material;
            _fadeMaterial.color = new Color(0, 0, 0, 0);
            fadeQuadRenderer.gameObject.SetActive(false);
        }

        // 2. Credits hidden
        if (creditsCanvasGroup != null)
        {
            creditsCanvasGroup.alpha = 0;
        }
    }

    public void LaunchFinalSequence()
    {
        StartCoroutine(FinalSequenceRoutine());
    }

    private IEnumerator FinalSequenceRoutine()
    {
        // --- STEP 1: FADE TO BLACK ---
        if (fadeQuadRenderer != null)
        {
            fadeQuadRenderer.gameObject.SetActive(true);
            // URP shaders use _BaseColor instead of _Color
            _fadeMaterial.DOColor(new Color(0, 0, 0, 1), "_BaseColor", fadeDuration);
        }

        yield return new WaitForSeconds(fadeDuration);

        // --- STEP 2: PAUSE ---
        yield return new WaitForSeconds(creditsDelay);

        // --- STEP 3: SHOW THE CREDITS ---
        if (creditsCanvasGroup != null)
        {
            creditsCanvasGroup.DOFade(1f, 1.5f);
        }
        // --- STEP 4: WAIT, THEN BACK TO THE STUDY ---
        yield return new WaitForSeconds(displayDuration);

        // Fade the credits out before changing scene
        if (creditsCanvasGroup != null)
        {
            creditsCanvasGroup.DOFade(0f, 1.5f);
            yield return new WaitForSeconds(1.5f);
        }

        SceneManager.LoadScene("StudioLeo");
    }
}
