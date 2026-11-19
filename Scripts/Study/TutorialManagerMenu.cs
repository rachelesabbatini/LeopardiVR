using UnityEngine;

/// <summary>
/// Shows the tutorial panels in the study only the first time the experience is opened.
/// The "tutorial seen" flag is saved in PlayerPrefs as soon as the panel appears.
/// </summary>
public class TutorialManagerMenu : MonoBehaviour
{
    [Header("Riferimenti UI")]
    [SerializeField] private GameObject pannelloTutorial; // the tutorial canvas or panel

    void Start()
    {
        // 0 = the tutorial has never been shown
        int statoTutorial = PlayerPrefs.GetInt("TutorialCompletato", 0);

        if (statoTutorial == 0)
        {
            // First time: show the tutorial.
            pannelloTutorial.SetActive(true);

            // Save right away that it has been seen,
            // so it won't appear again even if the user changes scene without closing it.
            PlayerPrefs.SetInt("TutorialCompletato", 1);
            PlayerPrefs.Save();
        }
        else
        {
            // Following times: keep it hidden.
            pannelloTutorial.SetActive(false);
        }
    }

    // Connected to the tutorial's "Close / Got it" button.
    // It only hides the panel: the flag has already been saved in Start.
    public void ChiudiTutorial()
    {
        if (pannelloTutorial != null)
        {
            pannelloTutorial.SetActive(false);
        }
    }
}
