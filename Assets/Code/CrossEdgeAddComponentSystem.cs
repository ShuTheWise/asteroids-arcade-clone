using Unity.Entities;
using Unity.Transforms2D;

namespace AsteroidsArcadeClone
{
    public class CrossEdgeAddComponentSystem : ComponentSystem
    {
        private struct Data
        {
            public int Length;
            public EntityArray Entity;
            public ComponentDataArray<Position2D> Position;
            public ComponentDataArray<OutsidePlayfield> OutsidePlayfield;
        }

        [Inject] private Data m_Data;

        protected override void OnUpdate()
        {
            var settings = AsteroidsArcadeBootstrap.Settings;
            var playfield = settings.playfield;

            for (int index = 0; index < m_Data.Length; ++index)
            {
                var position = m_Data.Position[index].Value;
                var entity = m_Data.Entity[index];

                //Check if entity in inside playfield and if it is try add cross edge component
                if (position.x < playfield.xMax &&
                    position.x > playfield.xMin &&
                    position.y < playfield.xMax &&
                    position.y > playfield.yMin)
                {
                    PostUpdateCommands.RemoveComponent<OutsidePlayfield>(entity);
                    PostUpdateCommands.AddComponent(entity, default(InPlayfield));
                }
            }
        }
    }
}