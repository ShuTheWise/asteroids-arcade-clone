using Unity.Entities;
using Unity.Transforms2D;
using UnityEngine;

namespace AsteroidsArcadeClone
{
    public class CrossEdgeInnerPlayefieldSystem : ComponentSystem
    {
        private struct Data
        {
            public int Length;
            public ComponentDataArray<Position2D> Position;
            public ComponentDataArray<InPlayfield> CrossEdge;
        }

        [Inject] private Data m_Data;

        protected override void OnUpdate()
        {
            var settings = AsteroidsArcadeBootstrap.Settings;
            var playfield = settings.playfield;

            for (int index = 0; index < m_Data.Length; ++index)
            {
                var position = m_Data.Position[index].Value;

                //Check if entity in inside playfield and if not set it's position accordingly
                if (position.x > playfield.xMax)
                {
                    position.x = playfield.xMin;
                }

                if (position.x < playfield.xMin)
                {
                    position.x = playfield.xMax;
                }

                if (position.y > playfield.yMax)
                {
                    position.y = playfield.yMin;
                }

                if (position.y < playfield.yMin)
                {
                    position.y = playfield.yMax;
                }

                m_Data.Position[index] = new Position2D {Value = position};
            }
        }
    }
}