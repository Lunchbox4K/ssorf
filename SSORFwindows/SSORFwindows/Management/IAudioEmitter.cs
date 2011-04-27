﻿using Microsoft.Xna.Framework;

namespace SSORF.Management
{
    /// <summary>
    /// Interface used by the AudioManager to look up the position
    /// and velocity of entities that can emit 3D sounds.
    /// </summary>
    public interface IAudioEmitter
    {
        Vector3 Position { get; }
        Vector3 Forward { get; }
        Vector3 Up { get; }
        Vector3 Velocity { get; }
    }
}
