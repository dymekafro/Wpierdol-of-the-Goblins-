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
        public float jumpVolume = 0.55f;
        public float fallVolume = 0.6f;
        public float goblinHopVolume = 0.5f;
        public float punchVolume = 0.8f;

        [Header("Music layer volumes")]
        public float soundtrackVolume = 0.5f;   // gra zawsze (tło)
        public float menuMusicVolume = 0.55f;    // gra w menu (main menu + pauza)
        public float ambientVolume = 0.45f;      // gra w rozgrywce (World)

        private AudioSource _sfxSource;
        private AudioSource _musicSource;

        // Pula głosów 3D dla pozycyjnych SFX (kroki, hopy goblinów, walka) —
        // round-robin zamiast PlayClipAtPoint, które tworzy/niszczy GameObjecty (skoki GC).
        private const int PositionalVoiceCount = 10;
        private AudioSource[] _positionalVoices;
        private int _positionalVoiceIndex;

        // Warstwy muzyki (grane jednocześnie wg stanu gry).
        private AudioSource _soundtrackSource;
        private AudioSource _menuMusicSource;
        private AudioSource _ambientSource;

        private AudioClip _hitClip;
        private AudioClip _deathClip;
        private AudioClip _uiClickClip;
        private AudioClip _fireballClip;
        private AudioClip _footstepClip;
        private AudioClip _footstepClip2;
        private AudioClip _jumpClip;
        private AudioClip _fallWooshClip;
        private AudioClip _goblinHopClip;
        private AudioClip _punchClip;

        private AudioClip _soundtrackClip;
        private AudioClip _menuMusicClip;
        private AudioClip _ambientClip;

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

            _soundtrackSource = CreateMusicLayer();
            _menuMusicSource = CreateMusicLayer();
            _ambientSource = CreateMusicLayer();

            CreatePositionalVoicePool();

            LoadClips();
            GameAssetLoader.LogAssetScanOnce();
            SettingsManager.EnsureExists();

            // SOUNDTRACK gra cały czas, od startu gry.
            StartLayer(_soundtrackSource, _soundtrackClip);
            ApplyMusicVolumes();
        }

        private void OnEnable()
        {
            SettingsManager.OnSettingsChanged += ApplyMusicVolumes;
        }

        private void OnDisable()
        {
            SettingsManager.OnSettingsChanged -= ApplyMusicVolumes;
        }

        private AudioSource CreateMusicLayer()
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.spatialBlend = 0f;
            return src;
        }

        private void CreatePositionalVoicePool()
        {
            _positionalVoices = new AudioSource[PositionalVoiceCount];
            for (int i = 0; i < PositionalVoiceCount; i++)
            {
                var go = new GameObject($"SfxVoice_{i}");
                go.transform.SetParent(transform, false);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 1f;
                _positionalVoices[i] = src;
            }
        }

        private void LoadClips()
        {
            _hitClip = GameAssetLoader.LoadAudio(GameAssetPaths.SfxHit, GameAssetPaths.ResSfxHit);
            _deathClip = GameAssetLoader.LoadAudio(GameAssetPaths.SfxDeath, GameAssetPaths.ResSfxDeath);
            _uiClickClip = GameAssetLoader.LoadAudio(GameAssetPaths.SfxUIClick, GameAssetPaths.ResSfxClick);
            _fireballClip = GameAssetLoader.LoadAudio(GameAssetPaths.SfxFireball, GameAssetPaths.ResSfxFireball);
            _footstepClip = GameAssetLoader.LoadAudio(GameAssetPaths.SfxFootstep, GameAssetPaths.ResSfxFootstep);
            _footstepClip2 = GameAssetLoader.LoadAudio(GameAssetPaths.SfxFootstep2, GameAssetPaths.ResSfxFootstep2);
            _jumpClip = GameAssetLoader.LoadAudio(GameAssetPaths.SfxJump, GameAssetPaths.ResSfxJump);
            _fallWooshClip = GameAssetLoader.LoadAudio(GameAssetPaths.SfxFallWoosh, GameAssetPaths.ResSfxFallWoosh);
            _goblinHopClip = GameAssetLoader.LoadAudio(GameAssetPaths.SfxGoblinHop, GameAssetPaths.ResSfxGoblinHop);
            _punchClip = GameAssetLoader.LoadAudio(GameAssetPaths.SfxPunch, GameAssetPaths.ResSfxPunch);

            _soundtrackClip = GameAssetLoader.LoadAudio(GameAssetPaths.MusicSoundtrack, GameAssetPaths.ResMusicSoundtrack);
            _menuMusicClip = GameAssetLoader.LoadAudio(GameAssetPaths.MusicMenu, GameAssetPaths.ResMusicMenu);
            _ambientClip = GameAssetLoader.LoadAudio(GameAssetPaths.MusicAmbient, GameAssetPaths.ResMusicAmbient);
        }

        private float SfxScale => SettingsManager.Instance != null
            ? SettingsManager.Instance.Settings.masterVolume * SettingsManager.Instance.Settings.sfxVolume
            : 1f;

        private float MusicScale => SettingsManager.Instance != null
            ? SettingsManager.Instance.Settings.masterVolume * SettingsManager.Instance.Settings.musicVolume
            : 1f;

        // Warstwa otoczenia ma własny suwak, niezależny od suwaka muzyki.
        private float AmbientScale => SettingsManager.Instance != null
            ? SettingsManager.Instance.Settings.masterVolume * SettingsManager.Instance.Settings.ambientVolume
            : 1f;

        public void PlayHit(Vector3? worldPos = null) => PlayClip(_hitClip, hitVolume, worldPos);
        public void PlayDeath(Vector3? worldPos = null) => PlayClip(_deathClip, deathVolume, worldPos);
        public void PlayUIClick() => PlayClip(_uiClickClip, uiClickVolume, null);
        public void PlayFireballCast(Vector3? worldPos = null) => PlayClip(_fireballClip, fireballVolume, worldPos);
        public void PlayFootstep(Vector3? worldPos = null)
        {
            // Losujemy wariant kroku (gdy drugi dostępny) — mniej monotonnie.
            AudioClip clip = _footstepClip;
            if (_footstepClip2 != null && (_footstepClip == null || Random.value > 0.5f))
                clip = _footstepClip2;
            PlayClip(clip, footstepVolume, worldPos);
        }

        public void PlayJump(Vector3? worldPos = null) => PlayClip(_jumpClip, jumpVolume, worldPos);
        public void PlayFall(Vector3? worldPos = null) => PlayClip(_fallWooshClip, fallVolume, worldPos);
        public void PlayGoblinHop(Vector3? worldPos = null) => PlayClip(_goblinHopClip, goblinHopVolume, worldPos);
        public void PlayPunch(Vector3? worldPos = null) => PlayClip(_punchClip, punchVolume, worldPos);

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

        // === Warstwowa muzyka ===

        // Menu główne: SOUNDTRACK + MENU, bez AMBIENT.
        public void EnterMainMenuMusic()
        {
            StartLayer(_soundtrackSource, _soundtrackClip);
            StartLayer(_menuMusicSource, _menuMusicClip);
            StopLayer(_ambientSource);
            ApplyMusicVolumes();
        }

        // Rozgrywka (World): SOUNDTRACK + AMBIENT, bez MENU.
        public void EnterGameplayMusic()
        {
            StartLayer(_soundtrackSource, _soundtrackClip);
            StartLayer(_ambientSource, _ambientClip);
            StopLayer(_menuMusicSource);
            ApplyMusicVolumes();
        }

        // Warstwa MENU on/off — używane przy otwarciu/zamknięciu pauzy (ESC).
        // SOUNDTRACK i AMBIENT grają dalej bez zmian.
        public void SetMenuLayerActive(bool active)
        {
            if (active) StartLayer(_menuMusicSource, _menuMusicClip);
            else StopLayer(_menuMusicSource);
            ApplyMusicVolumes();
        }

        private static void StartLayer(AudioSource src, AudioClip clip)
        {
            if (src == null || clip == null) return;
            if (src.clip != clip) src.clip = clip;
            if (!src.isPlaying) src.Play();
        }

        private static void StopLayer(AudioSource src)
        {
            if (src != null && src.isPlaying) src.Stop();
        }

        private void ApplyMusicVolumes()
        {
            float scale = MusicScale;
            if (_soundtrackSource != null) _soundtrackSource.volume = soundtrackVolume * scale;
            if (_menuMusicSource != null) _menuMusicSource.volume = menuMusicVolume * scale;
            // AMBIENT korzysta z własnego suwaka (AmbientScale), nie z suwaka muzyki.
            if (_ambientSource != null) _ambientSource.volume = ambientVolume * AmbientScale;
        }

        private void PlayClip(AudioClip clip, float volume, Vector3? worldPos)
        {
            if (clip == null) return;

            float vol = volume * SfxScale;
            if (vol <= 0.001f) return;

            if (worldPos.HasValue)
            {
                if (_positionalVoices == null || _positionalVoices.Length == 0) return;

                var voice = _positionalVoices[_positionalVoiceIndex];
                _positionalVoiceIndex = (_positionalVoiceIndex + 1) % _positionalVoices.Length;
                if (voice == null) return;

                voice.transform.position = worldPos.Value;
                voice.PlayOneShot(clip, vol);
            }
            else
            {
                if (_sfxSource == null) return;
                _sfxSource.PlayOneShot(clip, vol);
            }
        }
    }
}
