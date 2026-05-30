using System.Collections.Generic;
using UnityEngine;

namespace WPG.World
{
    /// <summary>
    /// Wspólne źródło prawdy o kształcie terenu. Proceduralne, łagodne pofalowanie (Perlin, 3 oktawy)
    /// + "plateau" (płaskie place) pod bazą, obozami i miejscami mocy, żeby struktury siedziały równo.
    ///
    /// Używane przez:
    ///  - <see cref="WorldGenerator"/> (budowa siatki gruntu + osadzanie obiektów),
    ///  - <see cref="WorldAssetPlacer"/> (przyklejanie roślinności do gruntu),
    ///  - <see cref="WorldGroundProfile"/> (odtworzenie konfiguracji w runtime z prefabu),
    ///  - gobliny (poruszają się tylko w XZ, więc każdą klatkę dociskane są do terenu).
    ///
    /// Gdy świat nie został skonfigurowany (np. stary, płaski prefab bez profilu),
    /// <see cref="GetGroundHeight(float,float)"/> zwraca 0 — czyli zachowanie jak dla płaskiej mapy.
    /// </summary>
    public static class WorldGround
    {
        public struct FlattenZone
        {
            public float x;
            public float z;
            public float radius;   // płaski promień (pełne wypłaszczenie)
            public float falloff;  // strefa wtopienia w teren
            public float pin;      // docelowa wysokość placu
        }

        static bool _configured;
        static float _amplitude;
        static float _scale;
        static float _offX;
        static float _offZ;
        static readonly List<FlattenZone> _zones = new List<FlattenZone>();

        public static bool IsConfigured => _configured;
        public static float Amplitude => _amplitude;
        public static float NoiseScale => _scale;
        public static float OffsetX => _offX;
        public static float OffsetZ => _offZ;
        public static IReadOnlyList<FlattenZone> Zones => _zones;

        /// <summary>Konfiguracja z ziarna świata — offsety szumu wyprowadzane deterministycznie.</summary>
        public static void Configure(int seed, float amplitude, float noiseScale)
        {
            var rng = new System.Random(seed == 0 ? 1 : seed);
            float offX = (float)(rng.NextDouble() * 2000.0 - 1000.0);
            float offZ = (float)(rng.NextDouble() * 2000.0 - 1000.0);
            ConfigureRaw(offX, offZ, amplitude, noiseScale);
        }

        /// <summary>Konfiguracja wprost (offsety zapisane w profilu prefabu) — bez wyprowadzania z ziarna.</summary>
        public static void ConfigureRaw(float offsetX, float offsetZ, float amplitude, float noiseScale)
        {
            _offX = offsetX;
            _offZ = offsetZ;
            _amplitude = Mathf.Max(0f, amplitude);
            _scale = Mathf.Max(1f, noiseScale);
            _zones.Clear();
            _configured = true;
        }

        public static void ClearZones() => _zones.Clear();

        /// <summary>Dodaje płaski plac. Jeśli pin==null, przypina się do naturalnej wysokości szumu w środku.</summary>
        public static void AddFlatZone(float x, float z, float radius, float falloff, float? pin = null)
        {
            AddFlatZoneRaw(x, z, radius, falloff, pin ?? RawHeight(x, z));
        }

        public static void AddFlatZoneRaw(float x, float z, float radius, float falloff, float pin)
        {
            _zones.Add(new FlattenZone
            {
                x = x,
                z = z,
                radius = Mathf.Max(0f, radius),
                falloff = Mathf.Max(0.01f, falloff),
                pin = pin
            });
        }

        static float RawHeight(float x, float z)
        {
            if (_amplitude <= 0.0001f) return 0f;

            float baseFreq = 1f / _scale;
            float n = 0f;
            float amp = 1f;
            float total = 0f;
            float freq = baseFreq;
            for (int o = 0; o < 3; o++)
            {
                n += (Mathf.PerlinNoise((x + _offX) * freq, (z + _offZ) * freq) - 0.5f) * amp;
                total += amp;
                amp *= 0.5f;
                freq *= 2.1f;
            }
            // ~[-1,1] * amplituda → łagodne wzgórza i doliny (bez ostrych gór).
            return (n / total) * 2f * _amplitude;
        }

        /// <summary>Wysokość terenu (Y) w punkcie świata (x,z).</summary>
        public static float GetGroundHeight(float x, float z)
        {
            if (!_configured) return 0f;

            float h = RawHeight(x, z);
            for (int i = 0; i < _zones.Count; i++)
            {
                FlattenZone zn = _zones[i];
                float dx = x - zn.x;
                float dz = z - zn.z;
                float d = Mathf.Sqrt(dx * dx + dz * dz);
                if (d >= zn.radius + zn.falloff) continue;

                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((d - zn.radius) / zn.falloff));
                h = Mathf.Lerp(zn.pin, h, t);
            }
            return h;
        }

        public static float GetGroundHeight(Vector3 p) => GetGroundHeight(p.x, p.z);

        /// <summary>Zwraca punkt z Y osadzonym na terenie (+ opcjonalny offset).</summary>
        public static Vector3 Snap(Vector3 p, float yOffset = 0f)
        {
            p.y = GetGroundHeight(p.x, p.z) + yOffset;
            return p;
        }

        /// <summary>Buduje pofalowaną siatkę gruntu wyśrodkowaną w (0,0). XZ w metrach.</summary>
        public static Mesh BuildMesh(float size, int resolution)
        {
            resolution = Mathf.Clamp(resolution, 8, 250);
            int vpr = resolution + 1;
            var verts = new Vector3[vpr * vpr];
            var uvs = new Vector2[vpr * vpr];
            var tris = new int[resolution * resolution * 6];

            float half = size * 0.5f;
            float step = size / resolution;
            float uvScale = Mathf.Max(1f, size / 8f); // ~8 m na powtórzenie tekstury

            for (int z = 0; z < vpr; z++)
            {
                for (int x = 0; x < vpr; x++)
                {
                    float wx = -half + x * step;
                    float wz = -half + z * step;
                    int idx = z * vpr + x;
                    verts[idx] = new Vector3(wx, GetGroundHeight(wx, wz), wz);
                    uvs[idx] = new Vector2((float)x / resolution * uvScale, (float)z / resolution * uvScale);
                }
            }

            int ti = 0;
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int bl = z * vpr + x;
                    int br = bl + 1;
                    int tl = bl + vpr;
                    int tr = tl + 1;
                    tris[ti++] = bl;
                    tris[ti++] = tl;
                    tris[ti++] = tr;
                    tris[ti++] = bl;
                    tris[ti++] = tr;
                    tris[ti++] = br;
                }
            }

            var mesh = new Mesh
            {
                name = "WPG_GroundMesh",
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Zapisuje bieżącą konfigurację do profilu, który odtworzy ją w runtime.</summary>
        public static void PopulateProfile(WorldGroundProfile profile)
        {
            if (profile == null) return;
            profile.offsetX = _offX;
            profile.offsetZ = _offZ;
            profile.amplitude = _amplitude;
            profile.noiseScale = _scale;

            var zones = new WorldGroundProfile.SerializableZone[_zones.Count];
            for (int i = 0; i < _zones.Count; i++)
            {
                zones[i] = new WorldGroundProfile.SerializableZone
                {
                    x = _zones[i].x,
                    z = _zones[i].z,
                    radius = _zones[i].radius,
                    falloff = _zones[i].falloff,
                    pin = _zones[i].pin
                };
            }
            profile.flattenZones = zones;
        }
    }
}
