using UnityEngine;
using System.Collections;

public class MusicZoneManager : MonoBehaviour
{
    public static MusicZoneManager Instance { get; private set; }

    [Header("Track A")]
    public AudioSource SourceA_Intro;
    public AudioSource SourceA_Loop;

    [Header("Track B")]
    public AudioSource SourceB_Intro;
    public AudioSource SourceB_Loop;

    private MusicTrack _currentTrack;
    private MusicTrack _previousTrack;
    private Coroutine _fadeCoroutine;
    private Coroutine _introCoroutineA;
    private Coroutine _introCoroutineB;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SourceA_Intro.volume = 0f; SourceA_Intro.playOnAwake = false;
        SourceA_Loop.volume  = 0f; SourceA_Loop.playOnAwake  = false;
        SourceB_Intro.volume = 0f; SourceB_Intro.playOnAwake = false;
        SourceB_Loop.volume  = 0f; SourceB_Loop.playOnAwake  = false;
    }

    // Trouve la source Intro qui a le même clip que le track
    private AudioSource GetIntroSource(MusicTrack track)
    {
        if (track == null) return null;
        if (SourceA_Intro.clip == track.Intro) return SourceA_Intro;
        if (SourceB_Intro.clip == track.Intro) return SourceB_Intro;
        Debug.LogWarning($"[MusicZoneManager] Aucune source Intro trouvée pour {track.Intro?.name}");
        return null;
    }

    // Trouve la source Loop qui a le même clip que le track
    private AudioSource GetLoopSource(MusicTrack track)
    {
        if (track == null) return null;
        if (SourceA_Loop.clip == track.Loop) return SourceA_Loop;
        if (SourceB_Loop.clip == track.Loop) return SourceB_Loop;
        Debug.LogWarning($"[MusicZoneManager] Aucune source Loop trouvée pour {track.Loop?.name}");
        return null;
    }

    public void TransitionTo(MusicTrack newTrack, float fadeDuration)
    {
        if (newTrack == null) { Debug.LogWarning("[MusicZoneManager] Track null !"); return; }

        if (_currentTrack != null &&
            newTrack.Intro == _currentTrack.Intro &&
            newTrack.Loop  == _currentTrack.Loop)
        {
            Debug.Log("[MusicZoneManager] Même musique, rien à faire.");
            return;
        }

        Debug.Log($"[MusicZoneManager] Transition → Intro:{newTrack.Intro?.name} Loop:{newTrack.Loop?.name}");

        _previousTrack = _currentTrack;
        _currentTrack  = newTrack;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(CrossFade(newTrack, fadeDuration));
    }

    private IEnumerator CrossFade(MusicTrack newTrack, float duration)
{
    AudioSource incomingIntro = GetIntroSource(newTrack);
    AudioSource incomingLoop  = GetLoopSource(newTrack);
    AudioSource outgoingIntro = GetIntroSource(_previousTrack);
    AudioSource outgoingLoop  = GetLoopSource(_previousTrack);

    if (_introCoroutineA != null) StopCoroutine(_introCoroutineA);
    if (_introCoroutineB != null) StopCoroutine(_introCoroutineB);

    float outgoingIntroVol = outgoingIntro != null ? outgoingIntro.volume : 0f;
    float outgoingLoopVol  = outgoingLoop  != null ? outgoingLoop.volume  : 0f;

    bool hasIntro = incomingIntro != null && incomingIntro.clip != null;

    if (hasIntro)
    {
        incomingIntro.volume = duration == 0f ? 1f : 0f;
        incomingIntro.Play();

        double dspTimeAtStart = AudioSettings.dspTime;

        Coroutine introRoutine = StartCoroutine(
            PlayLoopAfterIntro(incomingIntro, incomingLoop, newTrack.LoopOffset, newTrack.LoopDelay, dspTimeAtStart)
        );

        if (incomingIntro == SourceA_Intro) _introCoroutineA = introRoutine;
        else                                _introCoroutineB = introRoutine;
    }
    else if (incomingLoop != null)
    {
        incomingLoop.volume = 0f;
        incomingLoop.time   = newTrack.LoopOffset;
        incomingLoop.loop   = true;
        incomingLoop.Play();
    }

    // Spawn immédiat — pas de fade
    if (duration == 0f)
    {
        if (incomingIntro != null) incomingIntro.volume = 1f;
        else if (incomingLoop != null) incomingLoop.volume = 1f;
        if (outgoingIntro != null) { outgoingIntro.Stop(); outgoingIntro.volume = 0f; }
        if (outgoingLoop  != null) { outgoingLoop.Stop();  outgoingLoop.volume  = 0f; }
        _fadeCoroutine = null;
        yield break;
    }

    // Crossfade
    float elapsed = 0f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;

        // Fade out l'ancien track
        if (outgoingIntro != null) outgoingIntro.volume = Mathf.Lerp(outgoingIntroVol, 0f, t);
        if (outgoingLoop  != null) outgoingLoop.volume  = Mathf.Lerp(outgoingLoopVol,  0f, t);

        // Fade in le nouveau track
        if (hasIntro)
            incomingIntro.volume = Mathf.Lerp(0f, 1f, t);
        else if (incomingLoop != null)
            incomingLoop.volume  = Mathf.Lerp(0f, 1f, t);

        yield return null;
    }

    if (outgoingIntro != null) { outgoingIntro.volume = 0f; outgoingIntro.Stop(); }
    if (outgoingLoop  != null) { outgoingLoop.volume  = 0f; outgoingLoop.Stop();  }

    _fadeCoroutine = null;
}

private IEnumerator PlayLoopAfterIntro(
    AudioSource introSource,
    AudioSource loopSource,
    float loopOffset,
    float loopDelay,
    double dspTimeAtStart)
{
    double introDuration = (double)introSource.clip.samples / introSource.clip.frequency;
    double loopStartDSP  = dspTimeAtStart + introDuration + loopDelay;

    // Loop schedulé à volume 0 — silencieux jusqu'à la fin de l'intro
    loopSource.volume = 0f;
    loopSource.time   = loopOffset;
    loopSource.loop   = true;
    loopSource.PlayScheduled(loopStartDSP);

    yield return new WaitForSeconds((float)(introDuration + loopDelay));

    // L'intro est finie — on coupe et on monte le loop
    introSource.Stop();
    introSource.volume = 0f;
    loopSource.volume  = 1f;

    Debug.Log($"[MusicZoneManager] Loop démarré : {loopSource.clip.name}");
}
    public void StopMusic(float fadeDuration)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeOut(fadeDuration));
    }

    private IEnumerator FadeOut(float duration)
    {
        AudioSource activeIntro = GetIntroSource(_currentTrack);
        AudioSource activeLoop  = GetLoopSource(_currentTrack);

        float introVol = activeIntro != null ? activeIntro.volume : 0f;
        float loopVol  = activeLoop  != null ? activeLoop.volume  : 0f;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (activeIntro != null) activeIntro.volume = Mathf.Lerp(introVol, 0f, t);
            if (activeLoop  != null) activeLoop.volume  = Mathf.Lerp(loopVol,  0f, t);
            yield return null;
        }

        if (activeIntro != null) { activeIntro.Stop(); activeIntro.volume = 0f; }
        if (activeLoop  != null) { activeLoop.Stop();  activeLoop.volume  = 0f; }
        _currentTrack = null;
        _fadeCoroutine = null;
    }
}