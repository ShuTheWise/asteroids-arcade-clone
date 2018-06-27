using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms2D;
using UnityEngine;

namespace AsteroidsArcadeClone
{
    [UpdateAfter(typeof(CollisionSystem))]
    public class PlayerResetSystem : ComponentSystem
    {
        struct EnemyData
        {
            public int Length;
            [ReadOnly] public EntityArray Entity;
            [ReadOnly] public ComponentDataArray<Enemy> Enemy;
        }

        struct PlayerData
        {
            public int Length;
            public ComponentDataArray<Position2D> Position;
            public ComponentDataArray<Health> Health;
            public ComponentDataArray<PlayerInput> Input;
            public ComponentDataArray<MoveForce> MoveForce;
            public ComponentDataArray<PlayerInvulnerableReset> Invulnerable;
        }

        [Inject] EnemyData m_Enemies;
        [Inject] PlayerData m_Data;

        protected override void OnUpdate()
        {
            float dt = Time.deltaTime;

            for (int i = 0; i < m_Data.Length; ++i)
            {
                var mv = m_Data.MoveForce[i];
                var pi = m_Data.Invulnerable[i];
                var pos = m_Data.Position[i];
                var input = m_Data.Input[i];

                pi.Cooldown -= dt;
                if (pi.Cooldown < 0f)
                {
                    pi.Cooldown = 0f;
                }

                if (pi.reset == 1)
                {
                    pi.reset = 0;
                    pos.Value = default(float2);
                    input.Move = default(float2);
                    mv.Value = default(float2);

                    //destroy enemies
                    for (int j = 0; j < m_Enemies.Length; j++)
                    {
                        PostUpdateCommands.DestroyEntity(m_Enemies.Entity[j]);
                    }

                    //create a spawner
                    PostUpdateCommands.CreateEntity(AsteroidsArcadeBootstrap.EnemySpawnerArchetype);
                    PostUpdateCommands.SetComponent(new EnemySpawnSystemState
                    {
                        counter = 0f,
                        delay = AsteroidsArcadeBootstrap.Settings.enemySpawnDelay,
                    });
                }

                m_Data.MoveForce[i] = mv;
                m_Data.Position[i] = pos;
                m_Data.Invulnerable[i] = pi;
                m_Data.Input[i] = input;
            }
        }
    }
}