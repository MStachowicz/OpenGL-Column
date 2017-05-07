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
        /// When enabled collision checking is performed in the entity manager update function.
        /// </summary>
        public static bool CheckCollisions = true;

        /// <summary>
        /// List containing all the particles this entity manager is controlling.
        /// </summary>
        public List<Sphere> mParticles = new List<Sphere>();
        /// <summary>
        /// Number of particles created every time a particle effect is used
        /// </summary>
        public const int NoOfParticles = 5;
        /// <summary>
        /// Maximum number of particles allowed to exist in the simulation.
        /// </summary>
        public static int MaxParticles = 100;

        public static float sphereParticleRadius = 0.005f;

        /// <summary>
        /// Creates particles at the parameter position and gives them random velocities.
        /// </summary>
        /// <param name="pCollisionPoint">The point from which the spheres will be emitted.</param>
        /// <param name="pNumberOfParticles">The number of particles to be released from the point.</param>
        public void ParticleEffectSpheres(Vector3 pCollisionPoint, int pNumberOfParticles, Vector3 pVelocity)
        {
            for (int i = 0; i < NoOfParticles; i++)
            {
                if (mParticles.Count < MaxParticles)
                {
                    mParticles.Add(new Sphere(pCollisionPoint, sphereParticleRadius, false, Sphere.SphereType.particle, pVelocity));
                }
            }
        }

        /// <summary>
        /// Renders all the particles and sets the material properties if they are not set in the shader currently.
        /// </summary>
        public void RenderParticles()
        {
            for (int i = 0; i < mParticles.Count; i++)
            {
                if (ACWWindow.materialSet != mParticles[i].mMaterial)
                    mParticles[i].mMaterial.SetMaterial();

                mParticles[i].Render();
            }
        }
        /// <summary>
        /// Updates all the particle positions.
        /// </summary>
        public void UpdateParticles()
        {
            //Console.WriteLine("number of particles: " + mParticles.Count);
            for (int i = 0; i < mParticles.Count; i++)
            {
                // update the particle life time
                mParticles[i].mLifetime -= ACWWindow.timestep;

                //scale spheres slowly - dissolve

                if (mParticles[i].mLifetime < 0)
                {
                    mParticles.RemoveAt(i);
                }
                else
                {
                    mParticles[i].Update();
                }
            }

            if (CheckCollisions)
                for (int i = 0; i < mParticles.Count; i++)
                {
                    mParticles[i].hasCollidedWithCube(ACWWindow.cube);
                }
        }
    }
}

