using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labs.ACW
{
    public class ParticleManager
    {
        /// <summary>
        /// Creates a timer and links it to the 
        /// </summary>
        public ParticleManager()
        {
        }

        /// <summary>
        /// List containing all the particles this entity manager is controlling.
        /// </summary>
        public List<entity> mParticles = new List<entity>();
        /// <summary>
        /// Number of particles created every time a particle effect is used
        /// </summary>
        public const int NoOfParticles = 3;
        /// <summary>
        /// Maximum number of particles allowed to exist in the simulation.
        /// </summary>
        public static int MaxParticles = 100;


        /// <summary>
        /// Creates particles at the parameter position and gives them random velocities.
        /// </summary>
        /// <param name="pCollisionPoint">The point from which the spheres will be emitted.</param>
        public void ParticleEffectSpheres(Vector3 pCollisionPoint)
        {
            for (int i = 0; i < NoOfParticles; i++)
            {
                if (mParticles.Count < MaxParticles)
                {
                    mParticles.Add(new Sphere(pCollisionPoint, 0.005f, false, Sphere.SphereType.particle));
                }
            }
        }

        /// <summary>
        /// Renders all the particles
        /// </summary>
        public void RenderParticles()
        {
            for (int i = 0; i < mParticles.Count; i++)
            {
                mParticles[i].Render();
            }
        }
        /// <summary>
        /// Updates all the particle positions.
        /// </summary>
        public void UpdateParticles()
        {
            Console.WriteLine("number of particles: " + mParticles.Count);
            for (int i = 0; i < mParticles.Count; i++)
            {
                // update the particle life time
                mParticles[i].mLifetime -= ACWWindow.timestep;

                if (mParticles[i].mLifetime < 0)
                {
                    mParticles.RemoveAt(i);
                }
                else
                {
                    mParticles[i].Update();
                }
            }
        }
    }
}
