using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WPG.Core
{
    // Ładuje sprites/audio z importowanych paczek lub Resources/. Graceful fallback gdy brak assetów.
    public static class GameAssetLoader
    {
        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, AudioClip> AudioCache = new Dictionary<string, AudioClip>();
        private static readonly Dictionary<string, Texture2D> TextureCache = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Material> MaterialCache = new Dictionary<string, Material>();
        private static bool _scanLogged;

        public static bool HasFantasyGui => LoadSprite(GameAssetPaths.GuiPanel, GameAssetPaths.ResUiPanel) != null;
        public static bool HasRpgIcons => LoadSprite(GameAssetPaths.IconMelee, GameAssetPaths.ResIconMelee) != null;
        public static bool HasRpgSounds => LoadAudio(GameAssetPaths.SfxHit, GameAssetPaths.ResSfxHit) != null;

        public static void LogAssetScanOnce()
        {
            if (_scanLogged) return;
            _scanLogged = true;

            var sb = new StringBuilder();
            sb.AppendLine("[GameAssetLoader] Skan assetów UI/audio:");

            LogGroup(sb, "Fantasy Free GUI — panel", GameAssetPaths.GuiPanel, GameAssetPaths.ResUiPanel);
            LogGroup(sb, "Fantasy Free GUI — button", GameAssetPaths.GuiButton, GameAssetPaths.ResUiButton);
            LogGroup(sb, "Fantasy Free GUI — bar BG", GameAssetPaths.GuiBarBackground, GameAssetPaths.ResUiBarBg);
            LogGroup(sb, "Fantasy Free GUI — bar HP", GameAssetPaths.GuiBarFillHp, GameAssetPaths.ResUiBarHp);
            LogGroup(sb, "Fantasy Free GUI — bar mana", GameAssetPaths.GuiBarFillMana, GameAssetPaths.ResUiBarMana);
            LogGroup(sb, "Fantasy Free GUI — icon frame", GameAssetPaths.GuiIconFrame, GameAssetPaths.ResUiIconFrame);
            LogGroup(sb, "Fantasy Free GUI — menu BG", GameAssetPaths.GuiMenuBackground, GameAssetPaths.ResUiMenuBg);
            LogGroup(sb, "Modern RPG icons — melee", GameAssetPaths.IconMelee, GameAssetPaths.ResIconMelee);
            LogGroup(sb, "Modern RPG icons — fireball", GameAssetPaths.IconFireball, GameAssetPaths.ResIconFire);
            LogGroup(sb, "Modern RPG icons — heal", GameAssetPaths.IconHeal, GameAssetPaths.ResIconHeal);
            LogAudioGroup(sb, "Basic RPG Sounds — hit", GameAssetPaths.SfxHit, GameAssetPaths.ResSfxHit);
            LogAudioGroup(sb, "Basic RPG Sounds — death", GameAssetPaths.SfxDeath, GameAssetPaths.ResSfxDeath);
            LogAudioGroup(sb, "Basic RPG Sounds — UI click", GameAssetPaths.SfxUIClick, GameAssetPaths.ResSfxClick);
            LogAudioGroup(sb, "Basic RPG Sounds — fireball", GameAssetPaths.SfxFireball, GameAssetPaths.ResSfxFireball);
            LogAudioGroup(sb, "Movement SFX — footstep", GameAssetPaths.SfxFootstep, GameAssetPaths.ResSfxFootstep);
            LogAudioGroup(sb, "Movement SFX — jump", GameAssetPaths.SfxJump, GameAssetPaths.ResSfxJump);
            LogAudioGroup(sb, "Movement SFX — goblin hop", GameAssetPaths.SfxGoblinHop, GameAssetPaths.ResSfxGoblinHop);
            LogAudioGroup(sb, "Combat SFX — punch grunt", GameAssetPaths.SfxPunch, GameAssetPaths.ResSfxPunch);

            Debug.Log(sb.ToString());
        }

        public static Sprite LoadSprite(string[] assetPaths, string resourcesPath = null)
        {
            string key = "spr:" + (resourcesPath ?? "") + ":" + string.Join("|", assetPaths ?? System.Array.Empty<string>());
            if (SpriteCache.TryGetValue(key, out var cached)) return cached;

            Sprite found = null;
            if (assetPaths != null)
            {
                foreach (var path in assetPaths)
                {
                    found = LoadSpriteAtPath(path);
                    if (found != null) break;
                }
            }

#if UNITY_EDITOR
            if (found == null && assetPaths != null && assetPaths.Length > 0)
                found = TryFuzzySpriteInPackFolder(assetPaths[0]);
#endif

            if (found == null && !string.IsNullOrEmpty(resourcesPath))
                found = Resources.Load<Sprite>(resourcesPath);

            SpriteCache[key] = found;
            return found;
        }

        public static Texture2D LoadTexture(string[] assetPaths, string resourcesPath = null)
        {
            string key = "tex:" + (resourcesPath ?? "") + ":" + string.Join("|", assetPaths ?? System.Array.Empty<string>());
            if (TextureCache.TryGetValue(key, out var cached)) return cached;

            Texture2D found = null;
            if (assetPaths != null)
            {
                foreach (var path in assetPaths)
                {
                    found = LoadTextureAtPath(path);
                    if (found != null) break;
                }
            }

            if (found == null && !string.IsNullOrEmpty(resourcesPath))
                found = Resources.Load<Texture2D>(resourcesPath);

            TextureCache[key] = found;
            return found;
        }

        public static Material LoadMaterial(string[] assetPaths)
        {
            string key = "mat:" + string.Join("|", assetPaths ?? System.Array.Empty<string>());
            if (MaterialCache.TryGetValue(key, out var cached)) return cached;

            Material found = null;
            if (assetPaths != null)
            {
                foreach (var path in assetPaths)
                {
                    found = LoadMaterialAtPath(path);
                    if (found != null) break;
                }
            }

            MaterialCache[key] = found;
            return found;
        }

        public static AudioClip LoadAudio(string[] assetPaths, string resourcesPath = null)
        {
            string key = "aud:" + (resourcesPath ?? "") + ":" + string.Join("|", assetPaths ?? System.Array.Empty<string>());
            if (AudioCache.TryGetValue(key, out var cached)) return cached;

            AudioClip found = null;
            if (assetPaths != null)
            {
                foreach (var path in assetPaths)
                {
                    found = LoadAudioAtPath(path);
                    if (found != null) break;
                }
            }

#if UNITY_EDITOR
            if (found == null && assetPaths != null && assetPaths.Length > 0)
                found = TryFuzzyAudioInPackFolder(assetPaths[0]);
#endif

            if (found == null && !string.IsNullOrEmpty(resourcesPath))
                found = Resources.Load<AudioClip>(resourcesPath);

            AudioCache[key] = found;
            return found;
        }

        private static Sprite LoadSpriteAtPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

#if UNITY_EDITOR
            var direct = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (direct != null) return direct;

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            }
#endif
            return null;
        }

        private static AudioClip LoadAudioAtPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
#else
            return null;
#endif
        }

        private static Texture2D LoadTextureAtPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

#if UNITY_EDITOR
            if (path.EndsWith(".mat", System.StringComparison.OrdinalIgnoreCase))
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) return null;
                if (mat.HasProperty("_BaseMap")) return mat.GetTexture("_BaseMap") as Texture2D;
                if (mat.HasProperty("_MainTex")) return mat.GetTexture("_MainTex") as Texture2D;
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
#else
            return null;
#endif
        }

        private static Material LoadMaterialAtPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Material>(path);
#else
            return null;
#endif
        }

        private static void LogGroup(StringBuilder sb, string label, string[] paths, string resPath)
        {
            var sprite = LoadSprite(paths, resPath);
            sb.AppendLine(sprite != null ? $"  OK  {label}" : $"  --  {label} (brak — fallback kolory)");
        }

        private static void LogAudioGroup(StringBuilder sb, string label, string[] paths, string resPath)
        {
            var clip = LoadAudio(paths, resPath);
            sb.AppendLine(clip != null ? $"  OK  {label}" : $"  --  {label} (brak — silent)");
        }

#if UNITY_EDITOR
        static Sprite TryFuzzySpriteInPackFolder(string hintPath)
        {
            if (string.IsNullOrEmpty(hintPath)) return null;
            string fileHint = System.IO.Path.GetFileNameWithoutExtension(hintPath).ToLowerInvariant();
            string[] packTokens = hintPath.ToLowerInvariant().Contains("modern rpg")
                ? new[] { "modern rpg icons", "rpg icons" }
                : new[] { "fantasy free gui", "fantasy gui" };

            if (!GameAssetPacks.IsAnyFolderPresent(packTokens)) return null;

            int best = 0;
            Sprite bestSprite = null;
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)) continue;
                if (!GameAssetPacks.PathMatchesPackFolder(path, packTokens)) continue;

                string name = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                int score = name.Contains(fileHint) ? 30 : 0;
                if (name.Contains("panel") && fileHint.Contains("panel")) score += 25;
                if (name.Contains("button") && fileHint.Contains("button")) score += 25;
                if (name.Contains("bar") && fileHint.Contains("bar")) score += 20;
                if (name.Contains("sword") && fileHint.Contains("sword")) score += 25;
                if (name.Contains("fire") && fileHint.Contains("fire")) score += 25;
                if (name.Contains("heal") && fileHint.Contains("heal")) score += 25;
                if (score <= best) continue;

                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null) continue;
                best = score;
                bestSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            }
            return bestSprite;
        }

        static AudioClip TryFuzzyAudioInPackFolder(string hintPath)
        {
            if (string.IsNullOrEmpty(hintPath)) return null;
            string fileHint = System.IO.Path.GetFileNameWithoutExtension(hintPath).ToLowerInvariant();
            var packTokens = new[] { "basic rpg sounds", "rpg sounds" };
            if (!GameAssetPacks.IsAnyFolderPresent(packTokens)) return null;

            int best = 0;
            AudioClip bestClip = null;
            foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!GameAssetPacks.PathMatchesPackFolder(path, packTokens)) continue;
                string name = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                int score = name.Contains(fileHint) ? 30 : 0;
                if (fileHint.Contains("hit") && (name.Contains("hit") || name.Contains("sword"))) score += 25;
                if (fileHint.Contains("death") && name.Contains("death")) score += 25;
                if (fileHint.Contains("click") && name.Contains("click")) score += 25;
                if (fileHint.Contains("fire") && (name.Contains("fire") || name.Contains("spell"))) score += 25;
                if (fileHint.Contains("foot") && name.Contains("foot")) score += 25;
                if (score <= best) continue;
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null) continue;
                best = score;
                bestClip = clip;
            }
            return bestClip;
        }
#endif
    }
}
