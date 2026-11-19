using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// One 3D word or line of the poem, with the moment it appears in the recited audio.
/// </summary>
[System.Serializable]
public class ParolaAudio
{
    public GameObject oggettoParola; // the 3D word object
    public float tempoInizio;        // seconds from the start of the audio
    public float durata;             // how long it stays visible (seconds)
    [HideInInspector] public Renderer rendererCache; // cached at startup
}

/// <summary>
/// Makes 3D words appear and disappear in sync with a recited audio track.
/// Each word fades in when the voice reaches it and fades out afterwards,
/// driven by two properties of a custom shader (_Alpha and _EmissionPower).
/// The timings are set by hand in the Inspector.
/// </summary>
public class WordSyncManager : MonoBehaviour
{
    public AudioSource audioSource;
    public List<ParolaAudio> sequenza;
    public float velocitaFade = 2.5f; // fade speed

    void Start()
    {
        // Cache the renderers and make every word invisible right away
        foreach (var p in sequenza)
        {
            if (p.oggettoParola != null)
            {
                p.rendererCache = p.oggettoParola.GetComponentInChildren<Renderer>();

                // Set the material to fully transparent immediately, to avoid a one-frame flash
                if (p.rendererCache != null)
                {
                    Material mat = p.rendererCache.material;
                    mat.SetFloat("_Alpha", 0f);
                    mat.SetFloat("_EmissionPower", 0f);
                }

                // Keep the word inactive until its moment in the audio
                p.oggettoParola.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (audioSource == null || !audioSource.isPlaying) return;

        float t = audioSource.time;

        foreach (var p in sequenza)
        {
            if (p.oggettoParola == null || p.rendererCache == null) continue;

            bool deveEssereVisibile = (t >= p.tempoInizio && t < (p.tempoInizio + p.durata));

            if (deveEssereVisibile)
            {
                if (!p.oggettoParola.activeSelf) p.oggettoParola.SetActive(true);
                CambiaVisibilita(p, 1f);
            }
            else if (p.oggettoParola.activeSelf)
            {
                CambiaVisibilita(p, 0f);

                // Deactivate the word once it is almost invisible, so hidden words cost nothing to render
                if (OttieniAlpha(p) < 0.05f)
                    p.oggettoParola.SetActive(false);
            }
        }
    }

    // Starts the recited audio; the words follow it automatically
    public void IniziaSequenza()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    // Moves the word's transparency toward the target at a steady speed, with a matching glow
    void CambiaVisibilita(ParolaAudio p, float target)
    {
        Material mat = p.rendererCache.material;

        float currentAlpha = mat.GetFloat("_Alpha");
        float nextAlpha = Mathf.MoveTowards(currentAlpha, target, Time.deltaTime * velocitaFade);

        mat.SetFloat("_Alpha", nextAlpha);
        mat.SetFloat("_EmissionPower", nextAlpha * 3f);
    }

    float OttieniAlpha(ParolaAudio p)
    {
        return p.rendererCache.material.GetFloat("_Alpha");
    }
}
