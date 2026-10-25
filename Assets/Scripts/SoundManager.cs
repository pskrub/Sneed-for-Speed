using UnityEngine;

// NEW SCRIPT
// All the game sounds. Put this on an empty object in the game scene (e.g. "Sound")
// and drag in the clips. It makes its own AudioSources, so you don't need to add any.
//   Music     : loops while playing, stops when caught
//   Siren     : loops, louder and higher the closer the chaser is
//   Success   : on every hit
//   Fail      : on every miss / too slow
//   Game over : when the snail is caught
// Your other scripts don't need any changes: it watches PromptManager and SnailSpeedCalc.
public class SoundManager : MonoBehaviour
{
    [Header("Game")]
    public SnailSpeedCalc snail;
    public ChaserSpeedCalc chaser;
    public PromptManager prompts;

    [Header("Clips")]
    public AudioClip music;
    public AudioClip siren;
    public AudioClip success;
    public AudioClip fail;
    public AudioClip gameOver;

    [Header("Volume")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sirenMaxVolume = 0.8f;
    [Range(0f, 1f)] public float effectsVolume = 1f;

    [Header("Siren")]
    [Tooltip("Chaser Gap where the siren is at full volume (Gap is in track pieces, like Danger Gap on the HUD).")]
    public float sirenCloseGap = 0.1f;
    [Tooltip("Chaser Gap where the siren is silent.")]
    public float sirenFarGap = 0.5f;
    [Tooltip("How much higher the siren gets when the chaser is right behind you (0.3 = 30% higher).")]
    public float sirenExtraPitch = 0.3f;

    AudioSource musicSource;
    AudioSource sirenSource;
    AudioSource effectsSource;

    PromptState lastState;
    bool caught;

    void Start()
    {
        musicSource = MakeSource(music, true, musicVolume);
        sirenSource = MakeSource(siren, true, 0f);
        effectsSource = MakeSource(null, false, effectsVolume);

        if (music != null) musicSource.Play();
        if (siren != null) sirenSource.Play();

        if (snail != null) snail.Died += OnCaught;
        if (prompts != null) lastState = prompts.state;

        // NEW: tell which fields are missing, since the sounds that need them stay silent
        if (snail == null) Debug.LogWarning("SoundManager: Snail is empty, so there's no game over sound. Drag in Snail1.");
        if (chaser == null) Debug.LogWarning("SoundManager: Chaser is empty, so there's no siren. Drag in Chaser.");
        if (prompts == null) Debug.LogWarning("SoundManager: Prompts is empty, so there's no success/fail sound. Drag in MotionGame.");
        if (siren == null || success == null || fail == null || gameOver == null)
            Debug.LogWarning("SoundManager: one or more sound clips are empty.");
    }

    void OnDestroy()
    {
        if (snail != null) snail.Died -= OnCaught;
    }

    void Update()
    {
        if (caught) return;

        // Siren: louder and higher the closer the chaser is. Paused while the game is paused.
        if (chaser != null && siren != null)
        {
            if (Time.timeScale == 0f)
            {
                if (sirenSource.isPlaying) sirenSource.Pause();
            }
            else
            {
                if (!sirenSource.isPlaying) sirenSource.UnPause();

                float closeness = 1f - Mathf.InverseLerp(sirenCloseGap, sirenFarGap, chaser.Gap);
                sirenSource.volume = sirenMaxVolume * closeness;
                sirenSource.pitch = 1f + sirenExtraPitch * closeness;
            }
        }

        // Success / fail: PromptManager goes to Feedback right after every hit or miss.
        if (prompts != null)
        {
            if (prompts.state == PromptState.Feedback && lastState != PromptState.Feedback)
            {
                if (prompts.lastResult == "HIT!") PlayEffect(success);
                else PlayEffect(fail);
            }
            lastState = prompts.state;
        }
    }

    void OnCaught()
    {
        caught = true;
        musicSource.Stop();
        sirenSource.Stop();
        PlayEffect(gameOver);
    }

    void PlayEffect(AudioClip clip)
    {
        if (clip != null) effectsSource.PlayOneShot(clip, effectsVolume);
    }

    AudioSource MakeSource(AudioClip clip, bool loop, float volume)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
        source.volume = volume;
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D sound: same volume no matter where the camera is
        return source;
    }
}