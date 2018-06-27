using System;
using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms2D;

namespace AsteroidsArcadeClone
{
    public class PlayerMoveSystem : ComponentSystem
    {
        private const float DegToRad = Mathf.PI / 180f;

        private struct Data
        {
            public int Length;
            public ComponentDataArray<MoveForce> MoveForce;
            public ComponentDataArray<Heading2D> Heading;
            public ComponentDataArray<PlayerInput> Input;
            public ComponentDataArray<Position2D> Position;
        }

        [Inject] private Data m_Data;

        protected override void OnUpdate()
        {
            var settings = AsteroidsArcadeBootstrap.Settings;

            float dt = Time.deltaTime;
            for (int index = 0; index < m_Data.Length; ++index)
            {
                var moveForce = m_Data.MoveForce[index].Value;
                var heading = m_Data.Heading[index].Value;
                var position = m_Data.Position[index].Value;
                var playerInput = m_Data.Input[index];

                if (playerInput.Move.y > 0)
                {
                    moveForce += playerInput.Move.y * heading * settings.playerMoveSpeed * dt;
                }

                moveForce -= moveForce * dt * settings.playerInteriaFade;

                heading += Rotate(heading, -playerInput.Move.x * dt * settings.playerLookSpeed);
                heading = math.normalize(heading);

                //Player fires weapon
                if (playerInput.Fire)
                {
                    playerInput.FireCooldown = settings.playerFireCoolDown;

                    PostUpdateCommands.CreateEntity(AsteroidsArcadeBootstrap.ShotSpawnArchetype);
                    PostUpdateCommands.SetComponent(new ShotSpawnData
                    {
                        Shot = new Shot
                        {
                            TimeToLive = settings.bulletTimeToLive
                        },
                        Position = new Position2D {Value = position},
                        Heading = new Heading2D {Value = heading}
                    });
                }

                position += moveForce * dt;
                m_Data.Position[index] = new Position2D {Value = position};
                m_Data.MoveForce[index] = new MoveForce {Value = moveForce};
                m_Data.Heading[index] = new Heading2D {Value = heading};
                m_Data.Input[index] = playerInput;
            }
        }

        private static float2 Rotate(float2 v, float degrees)
        {
            return RotateRadians(v, degrees * DegToRad);
        }

        private static float2 RotateRadians(float2 v, float radians)
        {
            var ca = math.cos(radians);
            var sa = math.sin(radians);
            return new float2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
        }
    }
}