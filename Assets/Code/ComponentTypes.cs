using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms2D;

namespace AsteroidsArcadeClone
{
    //General
    public struct Health : IComponentData
    {
        public int Value;
    }
    public struct MoveForce : IComponentData
    {
        public float2 Value;
    }

    //Player
    public struct PlayerInput : IComponentData
    {
        public float2 Move;
        public int Shoot;
        public float FireCooldown;
        public bool Fire => FireCooldown <= 0.0 && Shoot == 1;
    }

    public struct PlayerScore : IComponentData
    {
        public int Value;
    }

    public struct PlayerShot : IComponentData
    {
    }

    public struct PlayerInvulnerableReset : IComponentData
    {
        public int reset;
        public float Cooldown;
    }

    //Shot
    public struct Shot : IComponentData
    {
        public float TimeToLive;
    }

    public struct ShotSpawnData : IComponentData
    {
        public Shot Shot;
        public Position2D Position;
        public Heading2D Heading;
        public int Faction;
    }

    //Enemy
    public struct Enemy : IComponentData
    {
    }

    public struct EnemySpawnSystemState : IComponentData
    {
        public float counter;
        public float delay;
    }

    public struct EnemySubdivide : IComponentData
    {
        public int Value;
    }

    //Playfield markers
    public struct InPlayfield : IComponentData
    {
    }

    public struct OutsidePlayfield : IComponentData
    {
    }
    
    public struct Living : IComponentData
    {
        public float Value;
    }
}