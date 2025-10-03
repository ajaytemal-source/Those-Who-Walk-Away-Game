using UnityEngine;

[System.Serializable]
public class GlitchEffectData
{
    public MonoBehaviour glitchScript;
    public float minInterval = 1f;
    public float maxInterval = 5f;
    public float glitchDuration = 0.5f;

    [HideInInspector] public float nextGlitchTime = 0f;
    [HideInInspector] public float glitchEndTime = 0f;
    [HideInInspector] public bool isGlitching = false;
    [HideInInspector] public bool isManual = false;
    [HideInInspector] public bool forceOn = false;
    [HideInInspector] public bool useRandom = false; // <-- New flag
}

public class GlitchController : MonoBehaviour
{
    public GlitchEffectData[] glitchEffects;

    void Start()
    {
        // Make sure everything starts off
        foreach (var effect in glitchEffects)
        {
            if (effect.glitchScript != null)
                effect.glitchScript.enabled = false;
        }
    }

    void Update()
    {
        float t = Time.time;

        foreach (var effect in glitchEffects)
        {
            if (effect.forceOn)
            {
                if (effect.glitchScript != null)
                    effect.glitchScript.enabled = true;
                continue;
            }

            if (!effect.useRandom) continue; // <-- Skip unless random mode enabled

            if (!effect.isGlitching && t >= effect.nextGlitchTime)
            {
                StartGlitch(effect, effect.glitchDuration, false);
            }
            else if (effect.isGlitching && t >= effect.glitchEndTime)
            {
                EndGlitch(effect);
                if (!effect.isManual)
                    ScheduleNextGlitch(effect);
            }
        }
    }

    void ScheduleNextGlitch(GlitchEffectData effect)
    {
        effect.nextGlitchTime = Time.time + Random.Range(effect.minInterval, effect.maxInterval);
    }

    void StartGlitch(GlitchEffectData effect, float duration, bool manual)
    {
        if (effect.glitchScript == null) return;

        effect.glitchScript.enabled = true;
        effect.glitchEndTime = Time.time + duration;
        effect.isGlitching = true;
        effect.isManual = manual;
    }

    void EndGlitch(GlitchEffectData effect)
    {
        if (effect.glitchScript == null) return;

        effect.glitchScript.enabled = false;
        effect.isGlitching = false;
        effect.isManual = false;
    }

    public void TriggerGlitch(int index, float duration)
    {
        if (index < 0 || index >= glitchEffects.Length) return;
        StartGlitch(glitchEffects[index], duration, true);
    }

    public void ForceGlitchOn(int index)
    {
        if (index < 0 || index >= glitchEffects.Length) return;

        var effect = glitchEffects[index];
        effect.forceOn = true;
        if (effect.glitchScript != null)
            effect.glitchScript.enabled = true;
    }

    public void ForceGlitchOff(int index)
    {
        if (index < 0 || index >= glitchEffects.Length) return;

        var effect = glitchEffects[index];
        effect.forceOn = false;
        EndGlitch(effect);
    }

    public void EnableRandomGlitches(int index)
    {
        if (index < 0 || index >= glitchEffects.Length) return;
        var effect = glitchEffects[index];
        effect.useRandom = true;
        ScheduleNextGlitch(effect);
    }

    public void DisableRandomGlitches(int index)
    {
        if (index < 0 || index >= glitchEffects.Length) return;
        glitchEffects[index].useRandom = false;
        EndGlitch(glitchEffects[index]);
    }
}