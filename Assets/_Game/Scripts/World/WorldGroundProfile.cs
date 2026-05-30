using UnityEngine;

namespace WPG.World
{
    /// <summary>
    /// Trzyma konfigurację pofalowanego terenu na obiekcie świata (np. "Ground" w WorldRoot.prefab)
    /// i odtwarza ją w runtime, zanim cokolwiek zacznie próbkować wysokość.
    ///
    /// Dzięki temu zapieczony (baked) prefab i kod runtime (gobliny, itd.) używają DOKŁADNIE
    /// tej samej powierzchni co siatka gruntu wypalona do prefabu.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1000)]
    public class WorldGroundProfile : MonoBehaviour
    {
        public float offsetX;
        public float offsetZ;
        public float amplitude = 2f;
        public float noiseScale = 55f;

        [System.Serializable]
        public struct SerializableZone
        {
            public float x;
            public float z;
            public float radius;
            public float falloff;
            public float pin;
        }

        public SerializableZone[] flattenZones = System.Array.Empty<SerializableZone>();

        private void Awake() => Apply();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                Apply();
        }
#endif

        public void Apply()
        {
            WorldGround.ConfigureRaw(offsetX, offsetZ, amplitude, noiseScale);
            if (flattenZones == null) return;
            foreach (var z in flattenZones)
                WorldGround.AddFlatZoneRaw(z.x, z.z, z.radius, z.falloff, z.pin);
        }
    }
}
