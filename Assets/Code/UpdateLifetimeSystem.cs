using AsteroidsArcadeClone;
using Unity.Entities;
using UnityEngine;

public class UpdateLifetimeSystem : ComponentSystem
{
    struct Data
    {
        public int Length;
        public ComponentDataArray<Living> living;
    }

    [Inject] private Data data;

    protected override void OnUpdate()
    {
        var dt = Time.deltaTime;
        for (int i = 0; i < data.Length; i++)
        {
            var living = data.living[i];
            living.Value += dt;
            data.living[i] = living;
        }
    }
}