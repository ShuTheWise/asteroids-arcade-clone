using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms2D;

namespace AsteroidsArcadeClone
{
    /// <summary>
    /// Damage player moving into asteroids and asteroids gettings shot by player missiles
    /// </summary>
    //TODO put both systems in seperate classes
    class CollisionSystem : JobComponentSystem
    {
        struct Players
        {
            public int Length;
            public ComponentDataArray<Health> Health;
            public ComponentDataArray<PlayerInvulnerableReset> Invulnerable;
            public ComponentDataArray<PlayerScore> Score;
            [ReadOnly] public ComponentDataArray<Position2D> Position;
            [ReadOnly] public ComponentDataArray<PlayerInput> PlayerMarker;
        }

        [Inject] Players m_Players;

        struct Enemies
        {
            public int Length;
            public ComponentDataArray<Health> Health;
            [ReadOnly] public ComponentDataArray<EnemySubdivide> Subdiv;
            [ReadOnly] public ComponentDataArray<Position2D> Position;
            [ReadOnly] public ComponentDataArray<Enemy> EnemyMarker;
        }

        [Inject] Enemies m_Enemies;

        /// <summary>
        /// All player shots.
        /// </summary>
        struct PlayerShotData
        {
            public int Length;
            public ComponentDataArray<Shot> Shot;
            [ReadOnly] public ComponentDataArray<Position2D> Position;
            [ReadOnly] public ComponentDataArray<PlayerShot> PlayerShotMarker;
        }

        [Inject] PlayerShotData m_PlayerShots;

        [BurstCompile]
        struct ShotCollisionJob : IJobParallelFor
        {
            public float CollisionRadiusSquared;

            public int bigScore;
            public int mediumScore;
            public int smallScore;
            [NativeDisableParallelForRestriction] public ComponentDataArray<PlayerScore> Score;
            public ComponentDataArray<Health> Health;

            [ReadOnly] public ComponentDataArray<EnemySubdivide> Subdiv;

            [ReadOnly] public ComponentDataArray<Position2D> Positions;

            [NativeDisableParallelForRestriction] public ComponentDataArray<Shot> Shots;

            [NativeDisableParallelForRestriction] [ReadOnly]
            public ComponentDataArray<Position2D> ShotPositions;

            public void Execute(int index)
            {
                float2 receiverPos = Positions[index].Value;
                var h = Health[index];
                var subdiv = Subdiv[index];

                for (int si = 0; si < Shots.Length; ++si)
                {
                    float2 shotPos = ShotPositions[si].Value;
                    float2 delta = shotPos - receiverPos;
                    float distSquared = math.dot(delta, delta);
                    if (distSquared <= CollisionRadiusSquared)
                    {
                        var shot = Shots[si];
                        h.Value--;
                        // Set the shot's time to live to zero, so it will be collected by the shot destroy system
                        shot.TimeToLive = 0.0f;
                        Shots[si] = shot;

                        //increment score
                        for (int i = 0; i < Score.Length; i++)
                        {
                            var score = Score[i];

                            switch (subdiv.Value)
                            {
                                case 2:
                                    score.Value += bigScore;
                                    break;
                                case 1:
                                    score.Value += mediumScore;
                                    break;
                                default:
                                    score.Value += smallScore;
                                    break;
                            }

                            Score[i] = score;
                        }
                    }
                }

                Health[index] = h;
            }
        }

        [BurstCompile]
        struct PlayerCollisionJob : IJobParallelFor
        {
            public float CollisionRadiusSquared;
            public float Cooldown;

            public ComponentDataArray<Health> Health;
            public ComponentDataArray<PlayerInvulnerableReset> Invulnerable;

            [ReadOnly] public ComponentDataArray<Position2D> Positions;

            [NativeDisableParallelForRestriction] [ReadOnly]
            public ComponentDataArray<Position2D> Enemies;

            public void Execute(int index)
            {
                var invulnerable = Invulnerable[index];

                float2 receiverPos = Positions[index].Value;
                var h = Health[index];

                for (int si = 0; si < Enemies.Length; ++si)
                {
                    float2 enemyPos = Enemies[si].Value;
                    float2 delta = enemyPos - receiverPos;
                    float distSquared = math.dot(delta, delta);
                    if (distSquared <= CollisionRadiusSquared && invulnerable.Cooldown <= 0.0)
                    {
                        h.Value--;
                        if (h.Value > 0)
                        {
                            invulnerable.Cooldown = Cooldown;
                            invulnerable.reset = 1;
                        }
                    }
                }

                Invulnerable[index] = invulnerable;
                Health[index] = h;
            }
        }

        [BurstCompile]
        struct EnemiesSelfCollisions : IJobParallelFor
        {
            public float CollisionRadiusSquared;
            [ReadOnly] public ComponentDataArray<Position2D> Positions;
            [NativeDisableParallelForRestriction] public ComponentDataArray<Health> Health;

            public void Execute(int index)
            {
                var refPos = Positions[index].Value;
                var health = Health[index];

                //asteroids self collisions
                for (int i = 0; i < Positions.Length; i++)
                {
                    if (i != index)
                    {
                        float2 delta = refPos - Positions[i].Value;
                        float distSquared = math.dot(delta, delta);
                        if (distSquared <= CollisionRadiusSquared)
                        {
                            health.Value--;
                        }
                    }
                }

                Health[index] = health;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var settings = AsteroidsArcadeBootstrap.Settings;

            if (settings == null)
                return inputDeps;

            EnemiesSelfCollisions enemiesVsEnemies = new EnemiesSelfCollisions
            {
                Positions = m_Enemies.Position,
                Health = m_Enemies.Health,
                CollisionRadiusSquared = settings.enemyCollisionRadius * settings.enemyCollisionRadius
            };
            JobHandle enemiesVsEnemiesHandle = enemiesVsEnemies.Schedule(m_Enemies.Length, 2, inputDeps);

            ShotCollisionJob playersVsEnemies = new ShotCollisionJob
            {
                ShotPositions = m_PlayerShots.Position,
                Shots = m_PlayerShots.Shot,
                CollisionRadiusSquared = settings.enemyCollisionRadius * settings.enemyCollisionRadius,
                Subdiv = m_Enemies.Subdiv,
                Health = m_Enemies.Health,
                Positions = m_Enemies.Position,
                Score = m_Players.Score,
                bigScore = settings.enemyBigAsteroidPoints,
                mediumScore = settings.enemyMediumAsteroidPoints,
                smallScore = settings.enemySmallAsteroidPoints
            };
            JobHandle playersVsEnemiesHandle = playersVsEnemies.Schedule(m_Enemies.Length, 1, enemiesVsEnemiesHandle);
            playersVsEnemiesHandle.Complete();

            JobHandle enemiesVsPlayer = new PlayerCollisionJob
            {
                CollisionRadiusSquared = settings.playerCollisionRadius * settings.playerCollisionRadius,
                Positions = m_Players.Position,
                Health = m_Players.Health,
                Enemies = m_Enemies.Position,
                Invulnerable = m_Players.Invulnerable,
                Cooldown = settings.playerInvulnerableCooldown
            }.Schedule(m_Players.Length, 1, playersVsEnemiesHandle);

            return enemiesVsPlayer;
        }
    }
}