using System.Collections.Generic;
using UnityEngine;
using WPG.Core;
using WPG.Enemies;

namespace WPG.World
{
    // Buduje proceduralnie cały las wokół (0,0,0). Wywoływany przez WorldBootstrap.
    // Wynik: list odebranych referencji + spawn point.
    public class WorldGenerator
    {
        public int seed = 12345;
        public float mapRadius = 125f;       // ~250 m średnicy
        public int treeCount = 480;
        public int rockCount = 90;
        public int bushCount = 200;
        public int mushroomLightCount = 70;
        public Transform parent;

        public Vector3 SpawnPoint { get; private set; }
        public DruidBase DruidBase { get; private set; }
        public readonly List<GoblinCamp> Camps = new List<GoblinCamp>();
        public readonly List<PowerSite> PowerSites = new List<PowerSite>();
        public readonly List<Vector3> OccupiedZones = new List<Vector3>();
        public readonly List<Vector3> ClearingCenters = new List<Vector3>();

        private System.Random _rng;
        private WorldAssetPlacer _assetPlacer;
        private List<Vector3> _pathPoints = new List<Vector3>();

        public void Generate()
        {
            _rng = new System.Random(seed);
            AssetCatalog.Initialize();
            GameAssetRegistry.Initialize();

            SpawnPoint = new Vector3(0f, 0f, 0f);
            _pathPoints = BuildPathNetwork();
            _assetPlacer = new WorldAssetPlacer(_rng, parent, _pathPoints, pathHalfWidth: WorldAssetPlacer.DefaultPathHalfWidth);

            // Niebo, mgła i ambient — GoldenHourLighting (nie zależy od skyMaterial.mat w paczce).
            GoldenHourLighting.Apply(parent);
            BuildGround();
            BuildDruidBase();
            BuildCamps();
            BuildPowerSites();
            BuildVegetation();
            BuildFutureMarkers();

            MaterialUpgrader.UpgradeHierarchy(parent.gameObject);

            ForestAtmosphereSettings.Instance?.ApplyAtmosphere();

            AssetCatalog.LogReport();
            LogGroundMaterialSources();
            Debug.Log($"[WorldGenerator] {WorldAssetPlacer.GetPlacementSummary(_assetPlacer?.PlaceholderCount ?? 0)}");
        }

        private List<Vector3> BuildPathNetwork()
        {
            var points = new List<Vector3> { Vector3.zero };

            var hubs = new[]
            {
                new Vector3(Mathf.Cos(200f * Mathf.Deg2Rad) * 50f, 0f, Mathf.Sin(200f * Mathf.Deg2Rad) * 50f),
                new Vector3(Mathf.Cos(40f * Mathf.Deg2Rad) * 55f, 0f, Mathf.Sin(40f * Mathf.Deg2Rad) * 55f),
                new Vector3(Mathf.Cos(130f * Mathf.Deg2Rad) * 85f, 0f, Mathf.Sin(130f * Mathf.Deg2Rad) * 85f),
                new Vector3(Mathf.Cos(280f * Mathf.Deg2Rad) * 90f, 0f, Mathf.Sin(280f * Mathf.Deg2Rad) * 90f),
                new Vector3(Mathf.Cos(350f * Mathf.Deg2Rad) * 110f, 0f, Mathf.Sin(350f * Mathf.Deg2Rad) * 110f),
                new Vector3(70f, 0f, 0f),
                new Vector3(-60f, 0f, 70f),
            };

            for (int i = 0; i < hubs.Length; i++)
                AppendWindingPath(points, Vector3.zero, hubs[i], 14 + i * 2, i);

            AppendWindingPath(points, hubs[0], hubs[1], 10, 20);
            AppendWindingPath(points, hubs[2], hubs[3], 12, 21);
            AppendWindingPath(points, hubs[0], hubs[5], 9, 22);

            return points;
        }

        private void AppendWindingPath(List<Vector3> points, Vector3 from, Vector3 to, int segments, int pathId)
        {
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector3 p = Vector3.Lerp(from, to, t);
                float n1 = Mathf.PerlinNoise(t * 4f + pathId * 0.31f + seed * 0.001f, pathId * 0.17f) - 0.5f;
                float n2 = Mathf.PerlinNoise(pathId * 0.22f, t * 5f + seed * 0.002f) - 0.5f;
                p.x += n1 * 10f * Mathf.Sin(t * Mathf.PI);
                p.z += n2 * 8f * Mathf.Cos(t * Mathf.PI * 1.2f);
                points.Add(p);
            }
        }

        private static void LogGroundMaterialSources()
        {
            GameAssetRegistry.TryGetPath(GameAssetRegistry.Slot.TexturePath, out var dirtPath);
            GameAssetRegistry.TryGetPath(GameAssetRegistry.Slot.TextureGrass, out var grassPath);
            GameAssetRegistry.TryGetPath(GameAssetRegistry.Slot.SkyboxMaterial, out var skyPath);
            bool hasDirt = GameAssetRegistry.PathDirtTexture != null;
            bool hasGrass = GameAssetRegistry.GrassGroundTexture != null;
            Debug.Log($"[WorldGenerator] Teren: dirt={(hasDirt ? dirtPath : "fallback kolor")}, grass={(hasGrass ? grassPath : "fallback kolor")}, skybox={skyPath ?? "procedural"}");
        }

        private void BuildGround()
        {
            var groundRoot = new GameObject("Ground");
            groundRoot.transform.SetParent(parent, false);

            // Główna płaszczyzna - gigantyczny Plane
            float planeSize = mapRadius * 2.4f;
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "GroundPlane";
            ground.transform.SetParent(groundRoot.transform, false);
            ground.transform.localScale = new Vector3(planeSize / 10f, 1f, planeSize / 10f);
            var groundMR = ground.GetComponent<MeshRenderer>();
            groundMR.sharedMaterial = MaterialFactory.GetForestFloor();

            // Polany z lekko jaśniejszym kolorem (kilka clearings)
            for (int i = 0; i < 10; i++)
            {
                float angle = (float)(_rng.NextDouble() * Mathf.PI * 2.0);
                float dist = (float)(_rng.NextDouble() * mapRadius * 0.9f);
                Vector3 pos = new Vector3(Mathf.Cos(angle) * dist, 0.01f, Mathf.Sin(angle) * dist);
                if (i == 0) pos = Vector3.zero + Vector3.up * 0.01f; // przy bazie
                float r = 6f + (float)_rng.NextDouble() * 8f;
                var clearing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                clearing.name = $"Clearing_{i}";
                clearing.transform.SetParent(groundRoot.transform, false);
                clearing.transform.position = pos;
                clearing.transform.localScale = new Vector3(r * 2f, 0.02f, r * 2f);
                var cc = clearing.GetComponent<Collider>(); if (cc != null) Object.Destroy(cc);
                clearing.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.GetGrassMeadow(i % 2 == 0);
                OccupiedZones.Add(new Vector3(pos.x, r * 0.7f, pos.z));
                ClearingCenters.Add(new Vector3(pos.x, 0f, pos.z));
            }

            BuildPathMeshes(groundRoot.transform);

            // Niewidzialne ściany / kill plane - prosty pierścień colliderów
            BuildBoundaryWalls(mapRadius * 1.05f);
        }

        private void BuildPathMeshes(Transform groundRoot)
        {
            if (_pathPoints == null || _pathPoints.Count < 2) return;

            var pathsRoot = new GameObject("Paths");
            pathsRoot.transform.SetParent(groundRoot, false);

            for (int i = 0; i < _pathPoints.Count - 1; i++)
            {
                Vector3 a = _pathPoints[i];
                Vector3 b = _pathPoints[i + 1];
                float len = Vector3.Distance(a, b);
                if (len < 0.5f) continue;

                var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.name = $"PathSeg_{i}";
                seg.transform.SetParent(pathsRoot.transform, false);
                seg.transform.position = (a + b) * 0.5f + Vector3.up * 0.02f;
                seg.transform.rotation = Quaternion.LookRotation((b - a).normalized, Vector3.up);
                seg.transform.localScale = new Vector3(5.5f, 0.06f, len + 0.3f);
                var cc = seg.GetComponent<Collider>(); if (cc != null) Object.Destroy(cc);
                seg.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.GetPathDirt();
            }
        }

        private void BuildBoundaryWalls(float radius)
        {
            var bounds = new GameObject("WorldBounds");
            bounds.transform.SetParent(parent, false);

            int segments = 24;
            for (int i = 0; i < segments; i++)
            {
                float a = i * (Mathf.PI * 2f / segments);
                float a2 = (i + 1) * (Mathf.PI * 2f / segments);
                Vector3 p1 = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
                Vector3 p2 = new Vector3(Mathf.Cos(a2) * radius, 0f, Mathf.Sin(a2) * radius);
                Vector3 mid = (p1 + p2) * 0.5f;
                float len = Vector3.Distance(p1, p2);

                var wall = new GameObject($"Wall_{i}");
                wall.transform.SetParent(bounds.transform, false);
                wall.transform.position = mid + Vector3.up * 15f;
                wall.transform.rotation = Quaternion.LookRotation((p2 - p1).normalized, Vector3.up);
                var box = wall.AddComponent<BoxCollider>();
                box.size = new Vector3(2f, 30f, len);
            }
        }

        private void BuildDruidBase()
        {
            var baseRoot = new GameObject("DruidBase");
            baseRoot.transform.SetParent(parent, false);
            baseRoot.transform.position = Vector3.zero;

            // Pierścień kamieni
            int stones = 9;
            for (int i = 0; i < stones; i++)
            {
                float a = i * (Mathf.PI * 2f / stones);
                Vector3 pos = new Vector3(Mathf.Cos(a) * 5f, 0f, Mathf.Sin(a) * 5f);
                var stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stone.transform.SetParent(baseRoot.transform, false);
                stone.transform.position = pos;
                stone.transform.localScale = new Vector3(0.8f, 1.8f, 0.7f);
                stone.transform.rotation = Quaternion.Euler((float)_rng.NextDouble() * 8f - 4f, (float)(_rng.NextDouble() * 360.0), (float)_rng.NextDouble() * 8f - 4f);
                stone.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(new Color(0.35f, 0.4f, 0.4f), 0.4f, new Color(0.2f, 0.6f, 0.5f), 0.25f);
                OccupiedZones.Add(new Vector3(pos.x, 1.2f, pos.z));
            }

            // Centralne drzewo życia - duży cylinder + sphere korona z lekkim emission
            var tree = new GameObject("LifeTree");
            tree.transform.SetParent(baseRoot.transform, false);
            tree.transform.position = Vector3.zero;
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localScale = new Vector3(1.6f, 5f, 1.6f);
            trunk.transform.localPosition = new Vector3(0f, 5f, 0f);
            trunk.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(new Color(0.25f, 0.16f, 0.10f));
            var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.transform.SetParent(tree.transform, false);
            canopy.transform.localScale = new Vector3(7f, 5f, 7f);
            canopy.transform.localPosition = new Vector3(0f, 11f, 0f);
            canopy.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(new Color(0.18f, 0.35f, 0.18f), 0.3f, new Color(0.3f, 0.7f, 0.4f), 0.3f);

            // Ognisko / źródło światła w centrum bazy
            var fire = new GameObject("BaseFire");
            fire.transform.SetParent(baseRoot.transform, false);
            fire.transform.localPosition = new Vector3(0f, 1f, 0f);

            var fireGlow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fireGlow.transform.SetParent(fire.transform, false);
            fireGlow.transform.localScale = Vector3.one * 0.8f;
            var fc = fireGlow.GetComponent<Collider>(); if (fc != null) Object.Destroy(fc);
            fireGlow.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(new Color(0.6f, 0.95f, 0.55f), 0.4f, new Color(0.5f, 1f, 0.7f), 5f);

            var lightGo = new GameObject("Light").AddComponent<Light>();
            lightGo.transform.SetParent(fire.transform, false);
            lightGo.color = new Color(0.6f, 1f, 0.7f);
            lightGo.range = 30f;
            lightGo.intensity = 6f;

            // Wokół - świecące grzyby
            for (int i = 0; i < 14; i++)
            {
                float a = (float)(_rng.NextDouble() * Mathf.PI * 2.0);
                float d = 6f + (float)_rng.NextDouble() * 9f;
                Vector3 pos = new Vector3(Mathf.Cos(a) * d, 0f, Mathf.Sin(a) * d);
                _assetPlacer?.PlaceMushroom(baseRoot.transform, baseRoot.transform.position + pos);
            }

            _assetPlacer?.DecorateDruidBase(baseRoot.transform);

            // Trigger zone bazy
            var trigGO = new GameObject("DruidBase_Trigger");
            trigGO.tag = "Untagged";
            trigGO.transform.SetParent(baseRoot.transform, false);
            trigGO.transform.localPosition = Vector3.zero;
            var sph = trigGO.AddComponent<SphereCollider>();
            sph.isTrigger = true;
            sph.radius = 12f;
            DruidBase = trigGO.AddComponent<DruidBase>();
            DruidBase.spawnPoint = new Vector3(0f, 1f, 3f);
            DruidBase.zoneName = "Sady Ostatniego Strażnika";

            SpawnPoint = DruidBase.spawnPoint;
            OccupiedZones.Add(new Vector3(0f, 14f, 0f)); // No-spawn dla drzew/skał w bazie
        }

        private void BuildCamps()
        {
            // 4 obozy w różnych pierścieniach
            CampDef[] defs = new CampDef[]
            {
                new CampDef
                {
                    campId = "goblin_camp_first_clearing",
                    displayName = "Pierwsza Zagroda",
                    ring = 1,
                    radius = 9f,
                    angleDeg = 200f,
                    distance = 50f,
                    stormtroopers = 2, archers = 1,
                    captureBonusType = 1,
                    seedOffset = 1
                },
                new CampDef
                {
                    campId = "goblin_camp_forest_den",
                    displayName = "Leśna Nora",
                    ring = 1,
                    radius = 8f,
                    angleDeg = 40f,
                    distance = 55f,
                    stormtroopers = 3, archers = 0,
                    captureBonusType = 0,
                    seedOffset = 2
                },
                new CampDef
                {
                    campId = "goblin_camp_ember_moss",
                    displayName = "Mchy Żaru",
                    ring = 2,
                    radius = 10f,
                    angleDeg = 130f,
                    distance = 85f,
                    stormtroopers = 3, archers = 2,
                    captureBonusType = 0,
                    seedOffset = 3
                },
                new CampDef
                {
                    campId = "goblin_camp_lost_roots",
                    displayName = "Korzenie Zagubione",
                    ring = 2,
                    radius = 10f,
                    angleDeg = 280f,
                    distance = 90f,
                    stormtroopers = 2, archers = 3,
                    captureBonusType = 2,
                    seedOffset = 4
                },
                new CampDef
                {
                    campId = "goblin_camp_shade_glade",
                    displayName = "Cienista Polana",
                    ring = 3,
                    radius = 12f,
                    angleDeg = 350f,
                    distance = 110f,
                    stormtroopers = 4, archers = 3, elites = 2,
                    captureBonusType = 2,
                    seedOffset = 5
                },
            };

            foreach (var def in defs)
            {
                Vector3 pos = new Vector3(
                    Mathf.Cos(def.angleDeg * Mathf.Deg2Rad) * def.distance,
                    0f,
                    Mathf.Sin(def.angleDeg * Mathf.Deg2Rad) * def.distance);
                BuildCamp(def, pos);
                OccupiedZones.Add(new Vector3(pos.x, def.radius + 4f, pos.z));
            }
        }

        private struct CampDef
        {
            public string campId;
            public string displayName;
            public int ring;
            public float radius;
            public float angleDeg;
            public float distance;
            public int stormtroopers;
            public int archers;
            public int elites;
            public int captureBonusType;
            public int seedOffset;
        }

        private void BuildCamp(CampDef def, Vector3 center)
        {
            var root = new GameObject($"Camp_{def.campId}");
            root.transform.SetParent(parent, false);
            root.transform.position = center;

            // Polana - jaśniejsza ziemia
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ground.transform.SetParent(root.transform, false);
            ground.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            ground.transform.localScale = new Vector3(def.radius * 2f, 0.03f, def.radius * 2f);
            var gc = ground.GetComponent<Collider>(); if (gc != null) Object.Destroy(gc);
            ground.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.GetGrassMeadow();

            // Palisada - cubes po obwodzie
            var palisades = new List<Renderer>();
            int sticks = Mathf.RoundToInt(def.radius * 4f);
            for (int i = 0; i < sticks; i++)
            {
                if (_rng.NextDouble() < 0.15) continue; // luki w palisadzie
                float a = i * (Mathf.PI * 2f / sticks);
                Vector3 p = new Vector3(Mathf.Cos(a) * def.radius, 0f, Mathf.Sin(a) * def.radius);
                var stick = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stick.transform.SetParent(root.transform, false);
                stick.transform.localPosition = new Vector3(p.x, 1.1f, p.z);
                stick.transform.localScale = new Vector3(0.25f, 2.2f + (float)_rng.NextDouble() * 0.6f, 0.25f);
                stick.transform.rotation = Quaternion.Euler((float)_rng.NextDouble() * 6f - 3f, 0f, (float)_rng.NextDouble() * 6f - 3f);
                var mat = MaterialFactory.Get(new Color(0.22f, 0.15f, 0.08f));
                stick.GetComponent<MeshRenderer>().sharedMaterial = mat;
                palisades.Add(stick.GetComponent<MeshRenderer>());
            }

            // Ognisko w centrum
            var fire = new GameObject("Campfire");
            fire.transform.SetParent(root.transform, false);
            fire.transform.localPosition = Vector3.zero;

            // Drewno
            for (int j = 0; j < 5; j++)
            {
                var log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                log.transform.SetParent(fire.transform, false);
                log.transform.localScale = new Vector3(0.18f, 0.5f, 0.18f);
                float a = j * (Mathf.PI / 2.5f);
                log.transform.localPosition = new Vector3(Mathf.Cos(a) * 0.3f, 0.25f, Mathf.Sin(a) * 0.3f);
                log.transform.localRotation = Quaternion.Euler(70f, j * 30f, 0f);
                var col = log.GetComponent<Collider>(); if (col != null) Object.Destroy(col);
                log.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(new Color(0.16f, 0.1f, 0.05f));
            }

            // Płomień (sphere emissive)
            var flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flame.transform.SetParent(fire.transform, false);
            flame.transform.localScale = Vector3.one * 0.7f;
            flame.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var flameCol = flame.GetComponent<Collider>(); if (flameCol != null) Object.Destroy(flameCol);
            flame.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(new Color(1f, 0.55f, 0.2f), 0.3f, new Color(1f, 0.5f, 0.15f), 4f);

            var firelight = new GameObject("FireLight").AddComponent<Light>();
            firelight.transform.SetParent(fire.transform, false);
            firelight.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            firelight.color = new Color(1f, 0.45f, 0.15f);
            firelight.range = 20f;
            firelight.intensity = 4f;

            // System dymu z paliuszkami
            var smokeGO = new GameObject("Smoke");
            smokeGO.transform.SetParent(fire.transform, false);
            smokeGO.transform.localPosition = new Vector3(0f, 1f, 0f);
            var ps = smokeGO.AddComponent<ParticleSystem>();
            var psr = smokeGO.GetComponent<ParticleSystemRenderer>();
            psr.material = MaterialFactory.GetParticle(new Color(0.3f, 0.25f, 0.2f, 0.55f));
            var main = ps.main;
            main.startLifetime = 4f;
            main.startSpeed = 1.5f;
            main.startSize = 1.5f;
            main.startColor = new Color(0.3f, 0.25f, 0.2f, 0.55f);
            main.maxParticles = 60;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emis = ps.emission;
            emis.rateOverTime = 8f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 18f;
            shape.radius = 0.2f;

            // Totem
            var totemGO = new GameObject("Totem");
            totemGO.transform.SetParent(root.transform, false);
            totemGO.transform.localPosition = new Vector3(0f, 0f, def.radius * 0.55f);
            var totem = totemGO.AddComponent<Totem>();
            totem.maxHealth = 60 + def.ring * 20;
            totem.buffRadius = def.radius + 4f;

            // Camp component
            var camp = root.AddComponent<GoblinCamp>();
            camp.campId = def.campId;
            camp.displayName = def.displayName;
            camp.ring = def.ring;
            camp.campFireLight = firelight;
            camp.smokeSystem = ps;
            camp.campFireRoot = fire.transform;
            camp.palisadeRenderers = palisades.ToArray();
            camp.captureBonusType = def.captureBonusType;
            camp.RegisterTotem(totem);
            totem.camp = camp;

            // Interaction collider przy ognisku
            var camptrig = new GameObject("Campfire_Interact");
            camptrig.transform.SetParent(fire.transform, false);
            camptrig.transform.localPosition = Vector3.zero;
            var inter = camptrig.AddComponent<CampInteractable>();
            inter.camp = camp;
            var box = camptrig.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(2f, 2f, 2f);

            // Strefa - WorldZone trigger
            var zoneGO = new GameObject("CampZone");
            zoneGO.transform.SetParent(root.transform, false);
            zoneGO.transform.localPosition = Vector3.zero;
            var zSph = zoneGO.AddComponent<SphereCollider>();
            zSph.isTrigger = true;
            zSph.radius = def.radius;
            var z = zoneGO.AddComponent<WorldZone>();
            z.zoneName = $"Obóz: {def.displayName}";

            // Spawn goblinów - rozproszone w obozie
            var sub = new System.Random(seed + def.seedOffset);
            for (int i = 0; i < def.stormtroopers; i++)
            {
                Vector3 p = RandomPointInDisc(sub, def.radius * 0.7f);
                var go = new GameObject($"Goblin_Storm_{i}");
                go.transform.SetParent(root.transform, false);
                go.transform.localPosition = p;
                var gob = go.AddComponent<GoblinStormtrooper>();
                camp.RegisterGoblin(gob);
            }
            for (int i = 0; i < def.archers; i++)
            {
                Vector3 p = RandomPointInDisc(sub, def.radius * 0.85f);
                var go = new GameObject($"Goblin_Archer_{i}");
                go.transform.SetParent(root.transform, false);
                go.transform.localPosition = p;
                var gob = go.AddComponent<GoblinArcher>();
                camp.RegisterGoblin(gob);
            }
            for (int i = 0; i < def.elites; i++)
            {
                Vector3 p = RandomPointInDisc(sub, def.radius * 0.45f);
                var go = new GameObject($"Goblin_Elite_{i}");
                go.transform.SetParent(root.transform, false);
                go.transform.localPosition = p;
                var gob = go.AddComponent<GoblinShamanElite>();
                camp.RegisterGoblin(gob);
            }

            // Inicjalizacja stanu z save
            CampState initState = GameManager.Instance != null
                ? GameManager.Instance.GetCampState(def.campId, CampState.Active)
                : CampState.Active;
            if (initState == CampState.Cleared || initState == CampState.Captured)
            {
                // Wszystko wczytane jako pokonane: usuwamy goblinów i totem
                foreach (var g in camp.Goblins) if (g != null) Object.Destroy(g.gameObject);
                camp.Goblins.Clear();
                if (totem != null) Object.Destroy(totem.gameObject);
                camp.RegisterTotem(null);
            }
            camp.Initialize(initState);

            _assetPlacer?.DecorateCamp(root.transform, def.radius, 3 + def.ring);

            Camps.Add(camp);
        }

        private static Vector3 RandomPointInDisc(System.Random rng, float radius)
        {
            float r = (float)System.Math.Sqrt(rng.NextDouble()) * radius;
            float a = (float)(rng.NextDouble() * Mathf.PI * 2.0);
            return new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
        }

        private void BuildPowerSites()
        {
            // Kamienny Krąg w pierścieniu 2
            var ring = new[]
            {
                new { id = "power_site_stone_circle", name = "Kamienny Krąg", desc = "+1 INT (jednorazowo)", x = 70f, z = 0f, glow = new Color(0.7f, 0.5f, 1f), bonus = 0 },
                new { id = "power_site_glow_shrine", name = "Kapliczka Światłości", desc = "+1 END (jednorazowo)", x = -60f, z = 70f, glow = new Color(1f, 0.85f, 0.5f), bonus = 1 }
            };
            foreach (var def in ring)
            {
                var root = new GameObject($"PowerSite_{def.id}");
                root.transform.SetParent(parent, false);
                root.transform.position = new Vector3(def.x, 0f, def.z);

                // Krąg kamieni
                int stones = 6;
                for (int i = 0; i < stones; i++)
                {
                    float a = i * (Mathf.PI * 2f / stones);
                    var st = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    st.transform.SetParent(root.transform, false);
                    st.transform.localPosition = new Vector3(Mathf.Cos(a) * 2.5f, 1.0f, Mathf.Sin(a) * 2.5f);
                    st.transform.localScale = new Vector3(0.5f, 2f, 0.4f);
                    st.GetComponent<MeshRenderer>().sharedMaterial =
                        MaterialFactory.Get(new Color(0.34f, 0.36f, 0.33f), 0.4f);
                }

                // Świecący centralny kryształ
                var crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                crystal.transform.SetParent(root.transform, false);
                crystal.transform.localPosition = new Vector3(0f, 1.2f, 0f);
                crystal.transform.localScale = Vector3.one * 0.8f;
                crystal.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.Get(def.glow, 0.5f, def.glow, 3f);

                var light = new GameObject("Light").AddComponent<Light>();
                light.transform.SetParent(root.transform, false);
                light.transform.localPosition = new Vector3(0f, 1.5f, 0f);
                light.color = def.glow;
                light.range = 14f;
                light.intensity = 4f;

                var ps = root.AddComponent<PowerSite>();
                ps.siteId = def.id;
                ps.displayName = def.name;
                ps.bonusDescription = def.desc;

                var trig = new GameObject("Trigger");
                trig.transform.SetParent(root.transform, false);
                trig.transform.localPosition = Vector3.zero;
                var sc = trig.AddComponent<SphereCollider>();
                sc.isTrigger = true;
                sc.radius = 2.5f;
                // Podpinamy IInteractable wyzej (PowerSite na root) — trig potrzebuje też referencji
                var fwd = trig.AddComponent<InteractableForwarder>();
                fwd.target = ps;

                if (GameManager.Instance != null && GameManager.Instance.visitedPowerSites.Contains(def.id))
                {
                    ps.MarkUsedSilent();
                }

                _assetPlacer?.DecoratePowerSite(root.transform, def.id == "power_site_stone_circle");
                MaterialUpgrader.UpgradeHierarchy(root);

                PowerSites.Add(ps);
                OccupiedZones.Add(new Vector3(def.x, 4f, def.z));
            }
        }

        private void BuildVegetation()
        {
            _assetPlacer?.FillForest(
                treeCount, rockCount, bushCount, mushroomLightCount,
                mapRadius, (pos, radius) => IsBlocked(pos, radius));

            BuildGrassDecoration();
        }

        private void BuildGrassDecoration()
        {
            if (_assetPlacer == null) return;

            int grassPoolCount = WPG.Core.GameAssetRegistry.WorldGrassPoolCount;
            if (grassPoolCount == 0)
            {
                Debug.Log("[WorldGenerator] Brak prefabu trawy — pomijam scatter (importuj Fantasy Forest grass01 lub Nature Starter Kit 2).");
                return;
            }

            var grassRoot = new GameObject("Grass");
            grassRoot.transform.SetParent(parent, false);
            Transform grassT = grassRoot.transform;

            int meadowBesidePaths = _assetPlacer.ScatterGrassNearPaths(
                grassT,
                bandWidth: 2.6f,
                density: 1.4f,
                mapRadius: mapRadius,
                isBlocked: (pos, padding) => IsBlocked(pos, padding, includeBase: false));

            int inClearings = _assetPlacer.ScatterGrassInClearings(
                grassT,
                ClearingCenters,
                clearingRadius: 9f,
                perClearing: 22,
                mapRadius: mapRadius,
                isBlocked: (pos, padding) => IsBlocked(pos, padding, includeBase: false));

            int sparseForest = ScatterGrassSparseForest(grassT, count: 220);

            Debug.Log($"[WorldGenerator] Trawa rozłożona: łąki przy ścieżkach={meadowBesidePaths}, polany={inClearings}, las rzadko={sparseForest} (wykluczenie ścieżki={_assetPlacer.PathSurfaceExclusionRadius:F1}m)");
        }

        private int ScatterGrassSparseForest(Transform parentT, int count)
        {
            if (_assetPlacer == null) return 0;
            int placed = 0;
            int attempts = 0;
            int maxAttempts = count * 8;
            while (placed < count && attempts < maxAttempts)
            {
                attempts++;
                Vector3 p = RandomMapPointForGrass();
                if (IsBlocked(p, 0.2f, includeBase: false)) continue;
                if (_assetPlacer.IsOnPath(p)) continue;
                if (_assetPlacer.PlaceGrass(parentT, p)) placed++;
            }
            return placed;
        }

        private Vector3 RandomMapPointForGrass()
        {
            float a = (float)(_rng.NextDouble() * Mathf.PI * 2.0);
            float r = Mathf.Sqrt((float)_rng.NextDouble()) * mapRadius;
            return new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
        }

        private void BuildFutureMarkers()
        {
            var futureRoot = new GameObject("FutureContent");
            futureRoot.transform.SetParent(parent, false);

            PlaceFutureMarker(futureRoot.transform, "future_gate_faded", "Przeszła Brama",
                new Vector3(0f, 0f, mapRadius * 0.92f));
            PlaceFutureMarker(futureRoot.transform, "future_region_swamp", "Wschód: Bagna",
                new Vector3(mapRadius * 0.85f, 0f, 0f));
            PlaceFutureMarker(futureRoot.transform, "future_quest_hermit", "Chata Pustelnika",
                new Vector3(-mapRadius * 0.7f, 0f, -mapRadius * 0.5f));
        }

        private void PlaceFutureMarker(Transform parentT, string id, string label, Vector3 pos)
        {
            var go = new GameObject(id);
            go.tag = "Untagged";
            go.transform.SetParent(parentT, false);
            go.transform.position = pos;
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "Marker";
            marker.transform.SetParent(go.transform, false);
            marker.transform.localScale = new Vector3(0.6f, 2.5f, 0.6f);
            marker.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            var col = marker.GetComponent<Collider>(); if (col != null) Object.Destroy(col);
            marker.GetComponent<MeshRenderer>().sharedMaterial =
                MaterialFactory.Get(new Color(0.25f, 0.28f, 0.32f), 0.3f, new Color(0.4f, 0.5f, 0.6f), 0.5f);
        }

        private bool IsBlocked(Vector3 p, float padding = 1.2f, bool includeBase = true)
        {
            // Mapa boundary
            if (p.sqrMagnitude > mapRadius * mapRadius) return true;
            foreach (var z in OccupiedZones)
            {
                if (!includeBase && z.x == 0f && z.z == 0f && z.y < 16f) continue;
                Vector3 z2 = new Vector3(z.x, 0f, z.z);
                if (Vector3.Distance(p, z2) < z.y + padding) return true;
            }
            return false;
        }
    }

    // Mały pośrednik - SphereCollider trigger ma swój IInteractable rzutowany na cel.
    public class InteractableForwarder : MonoBehaviour, IInteractable
    {
        public MonoBehaviour target; // np. PowerSite

        public string GetPrompt(GameObject player)
        {
            return target is IInteractable i ? i.GetPrompt(player) : "";
        }

        public bool CanInteract(GameObject player)
        {
            return target is IInteractable i && i.CanInteract(player);
        }

        public void Interact(GameObject player)
        {
            (target as IInteractable)?.Interact(player);
        }
    }
}
