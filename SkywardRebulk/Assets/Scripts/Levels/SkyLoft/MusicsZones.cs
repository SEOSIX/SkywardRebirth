using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class MusicsZones : MonoBehaviour
{
    [SerializeField] private Vector3 BoxSize;
    [SerializeField] private Color _colorBox;
    [SerializeField] private float FadeDuration = 2f;
    [SerializeField] private bool StartOnEnteredFirstTime = false;
    [SerializeField] private EventReference MusicEvent;
    [SerializeField] [Range(0f, 1f)] private float Volume = 1f;

    private EventInstance _musicInstance;
    private float _currentVolume = 0f;
    private float _targetVolume = 0f;
    
    private bool _hasStarted = false; 

    public static MusicsZones ActiveZone;

    void Start()
    {
        _musicInstance = RuntimeManager.CreateInstance(MusicEvent);
        
        if (!StartOnEnteredFirstTime)
        {
            _musicInstance.start();
            _hasStarted = true;
        }
        
        _musicInstance.setVolume(0f);
    }

    void Update()
    {
        Vector3 localPos = transform.InverseTransformPoint(Player.instance.transform.position);
        bool isInside = Mathf.Abs(localPos.x) <= BoxSize.x / 2f &&
                        Mathf.Abs(localPos.y) <= BoxSize.y / 2f &&
                        Mathf.Abs(localPos.z) <= BoxSize.z / 2f;
                        
        if (isInside)
        {
            if (!_hasStarted)
            {
                _musicInstance.start();
                _hasStarted = true;
            }

            if (ActiveZone != this)
            {
                bool sameParent = ActiveZone != null && ActiveZone.transform.parent == transform.parent;

                if (sameParent)
                {
                    ActiveZone._currentVolume = 0f;
                    ActiveZone._targetVolume  = 0f;
                    ActiveZone._musicInstance.setVolume(0f);

                    _currentVolume = 1f;
                }
                else
                {
                    if (ActiveZone != null)
                        ActiveZone._targetVolume = 0f;
                }

                ActiveZone = this;
            }
            _targetVolume = 1f;
        }
        else if (!isInside && ActiveZone == this)
        {
            _targetVolume = 0f;
        }

        float finalTargetVolume = _targetVolume * Volume;

        if (_currentVolume != finalTargetVolume)
        {
            _currentVolume = Mathf.MoveTowards(_currentVolume, finalTargetVolume, Time.deltaTime / FadeDuration);
            _musicInstance.setVolume(_currentVolume);
        }
    }

    void OnDestroy()
    {
        _musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _musicInstance.release();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color  = _colorBox;
        Gizmos.DrawWireCube(Vector3.zero, BoxSize);
        _colorBox.a   = 0.5f;
        Gizmos.color  = _colorBox;
        Gizmos.DrawCube(Vector3.zero, BoxSize);
        Gizmos.matrix = Matrix4x4.identity;
    }
}