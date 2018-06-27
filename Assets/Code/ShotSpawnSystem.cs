using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace AsteroidsArcadeClone
{
    public class ShotSpawnBarrier : BarrierSystem
    {
    }

    public class ShotSpawnSystem : ComponentSystem
    {
        public struct Data
        {
            public int Length;
            public EntityArray SpawnedEntities;
            [ReadOnly] public ComponentDataArray<ShotSpawnData> SpawnData;
        }

        [Inject] private Data m_Data;

        protected override void OnUpdate()
        {
            var em = PostUpdateCommands;

            for (int i = 0; i < m_Data.Length; ++i)
            {
                var sd = m_Data.SpawnData[i];
                var shotEntity = m_Data.SpawnedEntities[i];

                em.RemoveComponent<ShotSpawnData>(shotEntity);
                em.AddSharedComponent(shotEntity, default(MoveForward));
                em.AddComponent(shotEntity, default(InPlayfield));
                em.AddComponent(shotEntity, sd.Shot);
                em.AddComponent(shotEntity, sd.Position);
                em.AddComponent(shotEntity, sd.Heading);
                em.AddComponent(shotEntity, default(TransformMatrix));
                em.AddComponent(shotEntity, new PlayerShot());
                em.AddComponent(shotEntity, new MoveSpeed {speed = AsteroidsArcadeBootstrap.Settings.bulletMoveSpeed});
                em.AddSharedComponent(shotEntity, AsteroidsArcadeBootstrap.PlayerShotLook);
            }
        }
    }
}