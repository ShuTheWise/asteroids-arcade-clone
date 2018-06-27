using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms2D;

namespace AsteroidsArcadeClone
{
    // Removes enemies that are off screen
    class CrossEdgeOuterPlayfieldSystem : JobComponentSystem
    {
        struct ChangePositionJob : IJobProcessComponentData<Position2D, Heading2D, Enemy>
        {
            public float2 min;
            public float2 max;

            private float GetRandom(float min, float max)
            {
                Random random= new System.Random();
                return (float) random.NextDouble() * (max - min) + min;
            }

            public void Execute(ref Position2D pos, ref Heading2D heading2D, [ReadOnly] ref Enemy enemyTag)
            {
                var position = pos.Value;
                var heading = heading2D.Value;
                bool ch = false;
                if (position.x > max.x)
                {
                    position.x = min.x;
                    ch = true;
                }

                if (position.x < min.x)
                {
                    position.x = max.x;
                    ch = true;
                }

                if (position.y > max.y)
                {
                    position.y = min.y;
                    ch = true;
                }

                if (position.y < min.y)
                {
                    position.y = max.y;
                    ch = true;
                }

                if (ch)
                {
                    if (heading.x < math.epsilon_normal || heading.y < math.epsilon_normal ||
                        heading.x - 1f < math.epsilon_normal || heading.y - 1f < math.epsilon_normal)
                    {
                        heading2D = new Heading2D
                        {
                            Value = new float2(GetRandom(-1f, 1f), GetRandom(-1f, 1f))
                        };
                    }
                }

                pos.Value = position;
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (AsteroidsArcadeBootstrap.Settings == null)
                return inputDeps;

            var boundaryKillJob = new ChangePositionJob
            {
                min = new float2(AsteroidsArcadeBootstrap.Settings.playfield.xMin,
                          AsteroidsArcadeBootstrap.Settings.playfield.yMin) * 2f,
                max = new float2(AsteroidsArcadeBootstrap.Settings.playfield.xMax,
                          AsteroidsArcadeBootstrap.Settings.playfield.yMax) * 2f
            };

            return boundaryKillJob.Schedule(this, 64, inputDeps);
        }
    }
}