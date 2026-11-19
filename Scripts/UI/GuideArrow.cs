using UnityEngine;
using DG.Tweening;

/// <summary>
/// An arrow that points the user toward the next interaction.
/// It appears after a delay, plays its animation and hides itself after a few seconds.
/// The timers use DOTween, so they keep working even while the arrow object is inactive.
/// </summary>
public class GuideArrow : MonoBehaviour
{
    [Header("Impostazioni Base")]
    public GameObject frecciaObj;        // the arrow object
    public float durataVisibilita = 5f;  // how long it stays visible (seconds)

    [Header("Animazione")]
    [Tooltip("Object with the arrow's Animator component")]
    public Animator arrowAnimator;

    [Tooltip("Optional: name of the Trigger parameter, if the animation does not start by itself")]
    public string animationTriggerName = "";

    // DOTween timers, kept so they can be stopped if needed
    private Tween timerAttesa;
    private Tween timerSpegnimento;

    private void Awake()
    {
        // The arrow starts hidden
        if (frecciaObj != null)
        {
            frecciaObj.SetActive(false);
        }
    }

    // Shows the arrow after "delay" seconds
    public void MostraConRitardo(float delay)
    {
        // 1. Cancel any previous timers (e.g. if called more than once)
        timerAttesa?.Kill();
        timerSpegnimento?.Kill();

        // 2. Wait, then show the arrow
        timerAttesa = DOVirtual.DelayedCall(delay, () =>
        {
            if (frecciaObj != null)
            {
                frecciaObj.SetActive(true);

                // Start the animation, if a trigger name is set
                if (arrowAnimator != null && !string.IsNullOrEmpty(animationTriggerName))
                {
                    arrowAnimator.SetTrigger(animationTriggerName);
                }

                // Hide it again after a few seconds
                timerSpegnimento = DOVirtual.DelayedCall(durataVisibilita, NascondiFreccia);
            }
        });
    }

    private void NascondiFreccia()
    {
        if (frecciaObj != null)
        {
            frecciaObj.SetActive(false);
        }
    }

    // If the scene is unloaded, stop the timers so they don't run on destroyed objects
    private void OnDestroy()
    {
        timerAttesa?.Kill();
        timerSpegnimento?.Kill();
    }
}
