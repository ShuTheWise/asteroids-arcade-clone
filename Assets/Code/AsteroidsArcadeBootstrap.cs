using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Transforms2D;

namespace AsteroidsArcadeClone
{
    public sealed class AsteroidsArcadeBootstrap
    {
        public static EntityArchetype PlayerArchetype;
        public static EntityArchetype EnemyAsteroidArchetype;
        public static EntityArchetype ShotSpawnArchetype;
        public static EntityArchetype EnemySpawnerArchetype;

        public static MeshInstanceRenderer PlayerLook;
        public static MeshInstanceRenderer PlayerShotLook;
        public static MeshInstanceRenderer EnemyAsteroidBigLook;
        public static MeshInstanceRenderer EnemyAsteroidMediumLook;
        public static MeshInstanceRenderer EnemyAsteroidSmallLook;

        public static AsteroidsArcadeSettings Settings;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            UpdatePlayerHUD.state = UpdatePlayerHUD.State.NewGame;

            // This method creates archetypes for entities we will spawn frequently in this game.
            // Archetypes are optional but can speed up entity spawning substantially.

            var entityManager = World.Active.GetOrCreateManager<EntityManager>();

            // Create player archetype
            PlayerArchetype = entityManager.CreateArchetype(
                typeof(Position2D),
                typeof(Heading2D),
                typeof(PlayerInput),
                typeof(Health),
                typeof(PlayerInvulnerableReset),
                typeof(PlayerScore),
                typeof(TransformMatrix),
                typeof(InPlayfield),
                typeof(MoveForce));

            // Create an archetype for "shot spawn request" entities
            ShotSpawnArchetype = entityManager.CreateArchetype(typeof(ShotSpawnData));

            // Create an archetype for enemies (asteroids).
            EnemyAsteroidArchetype = entityManager.CreateArchetype(
                typeof(Enemy),
                typeof(Health),
                typeof(Position2D),
                typeof(Heading2D),
                typeof(TransformMatrix),
                typeof(MoveSpeed),
                typeof(OutsidePlayfield),
                typeof(EnemySubdivide),
                typeof(Living),
                typeof(MoveForward));

            EnemySpawnerArchetype = entityManager.CreateArchetype(typeof(EnemySpawnSystemState));
        }

        // Restart game.
        public static void RestartGame()
        {
            NewGame();
        }

        // Begin a new game.
        public static void NewGame()
        {
            // Access the ECS entity manager
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();

            AddPlayer();

            EnemySpawnSystem.SetupComponentData(entityManager);

            //Set game state
            UpdatePlayerHUD.state = UpdatePlayerHUD.State.Playing;
        }

        public static void AddPlayer()
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();

            // Create an entity based on the player archetype
            Entity player = entityManager.CreateEntity(PlayerArchetype);

            entityManager.SetComponentData(player, new Position2D {Value = new float2(0.0f, 0.0f)});
            entityManager.SetComponentData(player, new Heading2D {Value = new float2(0.0f, 1.0f)});
            entityManager.SetComponentData(player, new Health {Value = Settings.playerLives});
            entityManager.SetComponentData(player,
                new PlayerInvulnerableReset {Cooldown = Settings.playerFireCoolDown});

            //shared component dictates the rendered look
            entityManager.AddSharedComponentData(player, PlayerLook);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeWithScene()
        {
            var settingsGO = GameObject.Find("Settings");
            Settings = settingsGO?.GetComponent<AsteroidsArcadeSettings>();
            if (!Settings)
                return;

            PlayerLook = GetLookFromPrototype("PlayerRenderPrototype");
            PlayerShotLook = GetLookFromPrototype("PlayerShotRenderPrototype");
            EnemyAsteroidBigLook = GetLookFromPrototype("EnemyAsteroidBigRenderPrototype");
            EnemyAsteroidMediumLook = GetLookFromPrototype("EnemyAsteroidMediumRenderPrototype");
            EnemyAsteroidSmallLook = GetLookFromPrototype("EnemyAsteroidSmallRenderPrototype");
            
            World.Active.GetOrCreateManager<UpdatePlayerHUD>().SetupGameObjects();
        }

        private static MeshInstanceRenderer GetLookFromPrototype(string protoName)
        {
            var proto = GameObject.Find(protoName);
            var result = proto.GetComponent<MeshInstanceRendererComponent>().Value;
            Object.Destroy(proto);
            return result;
        }
    }
}