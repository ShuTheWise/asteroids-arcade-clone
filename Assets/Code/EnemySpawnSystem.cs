using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Transforms2D;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AsteroidsArcadeClone
{
    internal class EnemySpawnSystem : ComponentSystem
    {
        private static readonly System.Random Rnd = new System.Random();

        struct State
        {
            public int Length;
            public EntityArray Enity;
            public ComponentDataArray<EnemySpawnSystemState> EnemySpawnState;
        }

        [Inject] State m_State;

        public static void SetupComponentData(EntityManager entityManager)
        {
            var arch = entityManager.CreateArchetype(
                typeof(EnemySpawnSystemState)
            );
            var stateEntity = entityManager.CreateEntity(arch);
            entityManager.SetComponentData(stateEntity, new EnemySpawnSystemState
            {
                counter = 0f,
                delay = AsteroidsArcadeBootstrap.Settings.enemySpawnDelay,
            });
        }

        protected override void OnUpdate()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < m_State.Length; i++)
            {
                var s = m_State.EnemySpawnState[i];
                if (s.counter < s.delay)
                {
                    s.counter += dt;
                }
                else
                {
                    SpawnEnemies(AsteroidsArcadeBootstrap.Settings.enemyCount);
                    PostUpdateCommands.DestroyEntity(m_State.Enity[i]);
                }

                m_State.EnemySpawnState[i] = s;
            }
        }

        void SpawnEnemies(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnEnemy();
            }
        }

        void SpawnEnemy()
        {
            var state = m_State.EnemySpawnState[0];
            var settings = AsteroidsArcadeBootstrap.Settings;

            var spawnData = ComputeSpawnLocationAndHeading(settings);

            var speed = GetRandom(AsteroidsArcadeBootstrap.Settings.enemyMinSpeed,
                AsteroidsArcadeBootstrap.Settings.enemyMaxSpeed);

            PostUpdateCommands.CreateEntity(AsteroidsArcadeBootstrap.EnemyAsteroidArchetype);
            PostUpdateCommands.SetComponent(new EnemySubdivide {Value = 2});
            PostUpdateCommands.SetComponent(new Position2D {Value = spawnData.pos});
            PostUpdateCommands.SetComponent(new Health {Value = settings.enemyHealth});
            PostUpdateCommands.SetComponent(new Heading2D {Value = spawnData.heading});
            PostUpdateCommands.SetComponent(default(Enemy));
            PostUpdateCommands.SetComponent(new MoveSpeed {speed = speed});
            PostUpdateCommands.AddSharedComponent(AsteroidsArcadeBootstrap.EnemyAsteroidBigLook);

            m_State.EnemySpawnState[0] = state;
        }

        (float2 pos, float2 heading) ComputeSpawnLocationAndHeading(AsteroidsArcadeSettings settings)
        {
            float2 heading =
                new float2(
                    GetRandom(-1f, 1f),
                    GetRandom(-1f, 1f)
                );

            float2 min = new float2(settings.playfield.xMin, settings.playfield.yMin);
            float2 max = new float2(settings.playfield.xMax, settings.playfield.yMax);
            float2 pos = GetRandomOutside(min, max, AsteroidsArcadeBootstrap.Settings.enemySpawnPositionOffset);
            return (pos, heading);
        }

        private float GetRandom(float min, float max)
        {
            return (float) Rnd.NextDouble() * (max - min) + min;
        }

        float2 GetRandomOutside(float2 min, float2 max, float offset)
        {
            var qudrant = Rnd.Next(0, 4);
            float x;
            float y;

            switch (qudrant)
            {
                case 0:
                    x = GetRandom(min.x * 2f, min.x - offset);
                    y = GetRandom(min.y * 2f, max.y * 2f);
                    break;
                case 1:
                    x = GetRandom(max.x + offset, max.x * 2f);
                    y = GetRandom(min.y * 2, max.y * 2f);
                    break;
                case 2:
                    y = GetRandom(min.y * 2, min.y - offset);
                    x = GetRandom(min.x * 2f, max.x * 2f);
                    break;
                case 3:
                    y = GetRandom(max.y + offset, max.y * 2f);
                    x = GetRandom(min.x * 2f, max.x * 2f);
                    break;
                default:
                    //somthing went wrong this should never happen
                    x = 0f;
                    y = 0f;
                    break;
            }

            return new float2(x, y);
        }
    }
}