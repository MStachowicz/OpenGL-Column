using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Labs.Utility;
using System.Timers;

namespace Labs.ACW
{
    public class EntityManager
    {
        private List<Sphere> Spheres = new List<Sphere>();
        private List<Cube> Cubes = new List<Cube>();
        private List<Cylinder> Cylinders = new List<Cylinder>();
        /// <summary>
        /// All the objects this entity manager is responsible for.
        /// </summary>
        public List<entity> mObjects = new List<entity>();
        
        /// <summary>
        /// All the objects contained by all instances of the entity manager class.
        /// </summary>
        public static List<entity> AllObjects = new List<entity>();

        public static int VAOCount = 0;
        public static int VBOCount = 0;

        public EntityManager()
        { }


        public void ManageEntity(entity pEntity)
        {
            // Add the entity to the list of objects this manager controls.
            mObjects.Add(pEntity);
            // Add to the static list of all objects in the scene.
            AllObjects.Add(pEntity);

            // Add the entity to the correct sub list containing specific types.
            if (pEntity is Sphere)
            {
                Spheres.Add((Sphere)pEntity);
            }
            else if (pEntity is Cylinder)
            {
                Cylinders.Add((Cylinder)pEntity);
            }
            else if (pEntity is Cube)
            {
                Cubes.Add((Cube)pEntity);
            }
        }



        /// <summary>
        /// Generate a floating point number between the minimum and maximum.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private float NextFloat(float min, float max)
        {
            return (float)(min + (ACWWindow.rand.NextDouble() * (max - min)));
        }


        /// <summary>
        /// Calls the load method of all the objects contained by this entity manager.
        /// </summary>
        public void loadObjects()
        {
            for (int i = 0; i < mObjects.Count; i++)
            {
                mObjects[i].Load();
            }
        }

        /// <summary>
        /// Calls the render method of all the objects contained by this entity manager.
        /// </summary>
        public void renderObjects()
        {
            for (int i = 0; i < mObjects.Count; i++)
            {
                // set the material of the object in the shader if different from current set
                if (ACWWindow.materialSet != mObjects[i].mMaterial)
                    mObjects[i].mMaterial.SetMaterial();

                if (mObjects[i] is Cube)
                {
                    GL.Enable(EnableCap.CullFace);
                    mObjects[i].Render();
                    GL.Disable(EnableCap.CullFace);
                }
                else // every other object render
                    mObjects[i].Render();
            }
        }

        /// <summary>
        /// Performs collision detection and response for all the spheres managed by this entity manager.
        /// </summary>
        public void CheckCollisions()
        {
            // i is the sphere being checked for collisions with all other objects.
            for (int i = 0; i < Spheres.Count; i++)
            {
                // Sphere on cube collision detection and response. (inside of static cube)
                Spheres[i].hasCollidedWithCube(Cubes[0]);

                // Sphere on sphere detection and response.
                for (int s = 0; s < Spheres.Count; s++)
                {
                    if (i != s) // If this is not the same sphere
                        if (Spheres[i].hasCollidedWithSphere(Spheres[s])) // check collision
                        {
                            // if neither sphere is a sphere of doom
                            if (Spheres[i].sphereType != Sphere.SphereType.doom && Spheres[s].sphereType != Sphere.SphereType.doom)
                            {
                                Spheres[i].sphereOnSphereResponse(Spheres[s]); // perform standard sphere on sphere response
                            }
                            else if (Spheres[s].sphereType == Sphere.SphereType.doom) // if the other sphere is a sphere of doom
                            {
                                Spheres[i].SphereOnDoomSphereResponse(Spheres[s]); // perform the response to sphere of doom collision
                                //ACWWindow.pauseSimulation();
                            }
                        }
                }

                // sphere on cylinder detection and response.
                for (int c = 0; c < Cylinders.Count; c++)
                {
                    if (Spheres[i].sphereType != Sphere.SphereType.doom)
                    {
                        // Sphere on cylinder collision detection and response. (static cylinder)
                        if(Spheres[i].hasCollidedWithCylinder(Cylinders[c]))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            ACWWindow.particleManager.ParticleEffectSpheres(Spheres[i].mPosition);
                        }
                    }
                }
            }
        }

        public void resetSpheres()
        {
            for (int i = 0; i < Spheres.Count; i++)
            {
                if (Spheres[i].sphereType != Sphere.SphereType.doom)
                {
                    Spheres[i].MoveToEmitterBox(ACWWindow.cube1, false);
                }
            }
        }

        /// <summary>
        /// Updates every object in its list is it's not a static object.
        /// </summary>
        /// <param name="pTimestep"></param>
        /// <param name="pGravity"></param>
        public void updateObjects()
        {
            for (int i = 0; i < mObjects.Count; i++)
            {
                if (!mObjects[i].staticObject)
                {
                    mObjects[i].Update();
                }
            }
        }

    }

}
