using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Transforms2D;
using UnityEngine;

namespace AsteroidsArcadeClone
{
    public class RemoveSubdivideBarrier : BarrierSystem
    {
    }

    /// <summary>
    /// This system deletes entities that have a Health component with a value less than or equal to zero.
    /// </summary>
    public class EnemySubdivideOrRemoveSystem : JobComponentSystem
    {
        struct Data
        {
            [ReadOnly] public EntityArray Entity;
            [ReadOnly] public ComponentDataArray<Health> Health;
        }

        struct EnemiesToSubdivide
        {
            [ReadOnly] public EntityArray Entity;
            [ReadOnly] public ComponentDataArray<Position2D> Position;
            [ReadOnly] public ComponentDataArray<Health> Health;
            [ReadOnly] public ComponentDataArray<EnemySubdivide> Subdivide;
        }

        struct PlayerCheck
        {
            [ReadOnly] public ComponentDataArray<PlayerInput> PlayerInput;
        }

        [Inject] private Data m_Data;
        [Inject] private EnemiesToSubdivide m_EnemiesToSubdivide;
        [Inject] private PlayerCheck m_PlayerCheck;
        [Inject] private RemoveSubdivideBarrier m_RemoveSubdivideBarrier;

        struct SubdivideJob : IJob
        {
            public float enemyCollisionRadius;
            public float minSpeed;
            public float maxSpeed;
            public bool playerDead;
            public EntityArchetype archetype;
            [ReadOnly] public EntityArray Entity;
            [ReadOnly] public ComponentDataArray<Position2D> Position;
            [ReadOnly] public ComponentDataArray<Health> Health;
            [ReadOnly] public ComponentDataArray<EnemySubdivide> Subdivide;

            public EntityCommandBuffer Commands;

            private float Next(System.Random random, float min, float max)
            {
                return (float) random.NextDouble() * (max - min) + min;
            }

            public void Execute()
            {
                var rnd = new System.Random();
                float r = enemyCollisionRadius;

                for (int i = 0; i < Entity.Length; ++i)
                {
                    if (Health[i].Value <= 0 || playerDead)
                    {
                        var sub = Subdivide[i].Value;
                        if (sub > 0)
                        {
                            var pos = Position[i].Value;

                            //static referennce doesn't allow to burst compile this
                            var look = sub == 2
                                ? AsteroidsArcadeBootstrap.EnemyAsteroidMediumLook
                                : AsteroidsArcadeBootstrap.EnemyAsteroidSmallLook;

                            //Create couple of child asteroids
                            float x;
                            float y;
                            do
                            {
                                x = Next(rnd, -r, r);
                                y = Next(rnd, -r, r);
                            } while (math.pow(x, 2) + math.pow(y, 2) > math.pow(r, 2));

                            var childpos = Position[i].Value;

                            float2 offset = new float2(x, y);

                            float randomSpeed = Next(rnd, minSpeed, maxSpeed);
                            childpos += offset;
                            var heading = childpos - pos;
                            CreateChild(childpos, look, sub, heading, randomSpeed);

                            childpos = Position[i].Value;
                            childpos -= offset;
                            heading = childpos - pos;
                            CreateChild(childpos, look, sub, heading, randomSpeed);
                        }
                    }
                }
            }

            private void CreateChild(float2 pos, MeshInstanceRenderer look, int sub, float2 heading, float speed)
            {
                Commands.CreateEntity(archetype);
                Commands.SetComponent(new EnemySubdivide {Value = sub - 1});
                Commands.SetComponent(new Position2D {Value = pos});
                Commands.SetComponent(new Health {Value = 1});
                Commands.SetComponent(new Heading2D {Value = heading});
                Commands.SetComponent(default(Enemy));
                Commands.SetComponent(new MoveSpeed {speed = speed});
                Commands.AddSharedComponent(look);
            }
        }

        [BurstCompile]
        struct RemoveDead : IJob
        {
            public bool playerDead;
            [ReadOnly] public EntityArray Entity;
            [ReadOnly] public ComponentDataArray<Health> Health;
            public EntityCommandBuffer Commands;

            public void Execute()
            {
                for (int i = 0; i < Entity.Length; ++i)
                {
                    if (Health[i].Value <= 0.0f || playerDead)
                    {
                        Commands.DestroyEntity(Entity[i]);
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var removeWithSubdivide =
                new SubdivideJob
                {
                    enemyCollisionRadius = AsteroidsArcadeBootstrap.Settings.enemyCollisionRadius,
                    archetype = AsteroidsArcadeBootstrap.EnemyAsteroidArchetype,
                    minSpeed = AsteroidsArcadeBootstrap.Settings.enemyMinSpeed,
                    maxSpeed = AsteroidsArcadeBootstrap.Settings.enemyMaxSpeed,
                    playerDead = m_PlayerCheck.PlayerInput.Length == 0,
                    Entity = m_EnemiesToSubdivide.Entity,
                    Health = m_EnemiesToSubdivide.Health,
                    Position = m_EnemiesToSubdivide.Position,
                    Commands = m_RemoveSubdivideBarrier.CreateCommandBuffer(),
                    Subdivide = m_EnemiesToSubdivide.Subdivide
                };
            var removeWithSubdivideHandle = removeWithSubdivide.Schedule(inputDeps);

            return new RemoveDead
            {
                playerDead = m_PlayerCheck.PlayerInput.Length == 0,
                Entity = m_Data.Entity,
                Health = m_Data.Health,
                Commands = m_RemoveSubdivideBarrier.CreateCommandBuffer(),
            }.Schedule(removeWithSubdivideHandle);
        }
    }
}