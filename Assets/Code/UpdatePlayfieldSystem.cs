using System.Collections;
using System.Collections.Generic;
using AsteroidsArcadeClone;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class UpdatePlayfieldSystem : ComponentSystem
{
    private float _lastRatio;

    protected override void OnUpdate()
    {
        float aspectRatio = Screen.width / (float) Screen.height;
        if (math.abs(_lastRatio - aspectRatio) > math.epsilon_normal)
        {
            var settings = AsteroidsArcadeBootstrap.Settings;
            var playfield = settings.playfield;
            playfield.width = 60f * aspectRatio;
            playfield.x = -playfield.width * 0.5f;
            AsteroidsArcadeBootstrap.Settings.playfield = playfield;
            _lastRatio = aspectRatio;
        }
    }
}