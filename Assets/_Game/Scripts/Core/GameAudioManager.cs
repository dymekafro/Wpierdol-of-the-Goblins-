using UnityEngine;

namespace WPG.Core
{
    // Centralny manager SFX/Music. Respektuje SettingsManager (master/sfx/music volume).
    public class GameAudioManager : MonoBehaviour
    {
        public static GameAudioManager Instance { get; private set; }

        [Header("Volume multipliers")]
        public float hitVolume = 0.85f;
        public float deathVolume = 1f;
        public float uiClickVolume = 0.6f;
        public float fireballVolume = 0.9f;
        public float footstepVolume = 0.35f;

        private AudioSource _sfxSource;
        private AudioSource _musicSource;

        private AudioClip _hitClip;
        private AudioClip _deathClip;
        private AudioClip _uiClickClip;
        private AudioClip _fireballClip;
        private AudioClip _footstepClip;

        public static GameAudioManager EnsureExists()
        {
            if (Instance != null) return Instance;

            var existing = FindAnyObjectByType<GameAudioManager>();
            if (existing != null)
            {
                Instance = existing;
                return Instance;
            }

            var go = new GameObject("[GameAudioManager]");
            Instance = go.AddComponent<GameAudioManager>();
            DontDestroyOnLoad(go);
            return Instance;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.spatialBlend = 0f;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.spatialBlend = 0f;

            LoadClips();
            GameAssetLoader.LogAssetScanOnce();
            SettingsManager.EnsureExists();
        }

        private void LoadClips()
        {
            _hitClip = GameAssetLoader.LoadAudio(GameAssetPaths.SfxHit, GameAssetPaths.ResSfxHit);
            _deathClip = GameAssetLoader.LoadAudio(GameAssetPaths.SfxDeath, GameAssetPaths.ResSfxDeath);
            _uiClickClip = GameAssetLoader.LoadAudio(GameAssetPaths.SfxUIClick, GameAssetPaths.ResSfxClick);
            _fireballClip = GameAssetLoader.LoadAudio(GameAssetPaths.SfxFireball, GameAssetPaths.ResSfxFireball);
            _footstepClip = GameAssetLoader.LoadAudio(GameAssetPaths.SfxFootstep, GameAssetPaths.ResSfxFootstep);
        }

        private float SfxScale => SettingsManager.Instance != null
            ? SettingsManager.Instance.Settings.masterVolume * SettingsManager.Instance.Settings.sfxVolume
            : 1f;

        private float MusicScale => SettingsManager.Instance != null
            ? SettingsManager.Instance.Settings.masterVolume * SettingsManager.Instance.Settings.musicVolume
            : 1f;

        public void PlayHit(Vector3? worldPos = null) => PlayClip(_hitClip, hitVolume, worldPos);
        public void PlayDeath(Vector3? worldPos = null) => PlayClip(_deathClip, deathVolume, worldPos);
        public void PlayUIClick() => PlayClip(_uiClickClip, uiClickVolume, null);
        public void PlayFireballCast(Vector3? worldPos = null) => PlayClip(_fireballClip, fireballVolume, worldPos);
        public void PlayFootstep(Vector3? worldPos = null) => PlayClip(_footstepClip, footstepVolume, worldPos);

        public void PlayMusic(AudioClip clip, float volume = 0.5f)
        {
            if (clip == null || _musicSource == null) return;
            if (_musicSource.clip == clip && _musicSource.isPlaying) return;
            _musicSource.clip = clip;
            _musicSource.volume = volume * MusicScale;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            if (_musicSource != null) _musicSource.Stop();
        }

        private void PlayClip(AudioClip clip, float volume, Vector3? worldPos)
        {
            if (clip == null || _sfxSource == null) return;

            float vol = volume * SfxScale;
            if (vol <= 0.001f) return;

            if (worldPos.HasValue)
            {
                AudioSource.PlayClipAtPoint(clip, worldPos.Value, vol);
            }
            else
            {
                _sfxSource.PlayOneShot(clip, vol);
            }
        }
    }
}
