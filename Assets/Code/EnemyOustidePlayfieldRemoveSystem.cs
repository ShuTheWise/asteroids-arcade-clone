using System.Collections;
using System.Collections.Generic;
using AsteroidsArcadeClone;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class EnemyOustidePlayfieldRemoveSystem : JobComponentSystem
{
    struct Data
    {
        public int Length;
        [ReadOnly] public EntityArray Entity;
        [ReadOnly] public ComponentDataArray<Living> Living;
        [ReadOnly] public ComponentDataArray<OutsidePlayfield> oustide;
    }
    [Inject] private EnemyLifetimeBarrier EnemyLifetimeBarrier;
    [Inject] private Data m_Data;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return new RemoveOutlived
        {
            maxTime = AsteroidsArcadeBootstrap.Settings.enemyMaxLifetime,
            Commands =  EnemyLifetimeBarrier.CreateCommandBuffer(),
            Living = m_Data.Living,
            Entity = m_Data.Entity
        }.Schedule(inputDeps);
    }

    [BurstCompile]
    struct RemoveOutlived : IJob
    {
        public float maxTime;
        [ReadOnly] public EntityArray Entity;
        [ReadOnly] public ComponentDataArray<Living> Living;
        public EntityCommandBuffer Commands;

        public void Execute()
        {
            for (int i = 0; i < Entity.Length; ++i)
            {
                if (Living[i].Value > (maxTime))
                {
                    Commands.DestroyEntity(Entity[i]);
                }
            }
        }
    }
}

public class EnemyLifetimeBarrier : BarrierSystem
{
}