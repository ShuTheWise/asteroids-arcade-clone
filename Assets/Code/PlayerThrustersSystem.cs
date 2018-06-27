using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms2D;
using UnityEngine;

namespace AsteroidsArcadeClone
{
    [AlwaysUpdateSystem, UpdateAfter(typeof(PlayerMoveSystem))]
    public class PlayerThrustersSystem : ComponentSystem
    {
        private ParticleSystem _thrusters;

        private struct Data
        {
            public int Length;
            public ComponentDataArray<MoveForce> MoveForce;
            public ComponentDataArray<Heading2D> Heading;
            public ComponentDataArray<PlayerInput> Input;
            public ComponentDataArray<Position2D> Position;
        }

        [Inject] private Data m_Data;

        private void Destroy()
        {
            if (_thrusters != null)
            {
                GameObject.Destroy(_thrusters.transform.parent.gameObject);
            }
        }

        private void Instantiate(AsteroidsArcadeSettings settings)
        {
            if (_thrusters == null)
            {
                GameObject gameObject = GameObject.Instantiate(settings.playerThrustersPrefab);
                _thrusters = gameObject.GetComponentInChildren<ParticleSystem>();
            }
        }

        protected override void OnUpdate()
        {
            var settings = AsteroidsArcadeBootstrap.Settings;

            if (m_Data.Length > 0)
            {
                Instantiate(settings);
            }
            else
            {
                Destroy();
            }

            if (_thrusters == null) return;

            ParticleSystem.EmissionModule emissionModule = _thrusters.emission;

            for (int index = 0; index < m_Data.Length; ++index)
            {
                var heading = m_Data.Heading[index].Value;
                var position = m_Data.Position[index].Value;
                var playerInput = m_Data.Input[index];

                //update rotation
                _thrusters.transform.parent.position = new Vector3(position.x, 0, position.y);
                var rads = math.atan2(heading.y, -heading.x);
                var y = (rads / Mathf.PI) * 180.0f;
                _thrusters.transform.parent.eulerAngles = new Vector3(0, y - 90f, 0);
                
                //update emission
                emissionModule.enabled = playerInput.Move.y > 0;
            }
        }
    }
}