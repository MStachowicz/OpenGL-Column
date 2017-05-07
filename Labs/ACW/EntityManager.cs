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
        /// <summary>
        /// All the objects this entity manager is responsible for.
        /// </summary>
        public List<entity> mObjects = new List<entity>();

        private List<Sphere> Spheres = new List<Sphere>();
        private List<Cube> Cubes = new List<Cube>();
        private List<Cylinder> Cylinders = new List<Cylinder>();


        public static int VAOCount = 0;
        public static int VBOCount = 0;

        public int entityKey = 0;

        public EntityManager()
        { }


        public void ManageEntity(entity pEntity)
        {
            // Add the entity to the list of objects this manager controls.
            mObjects.Add(pEntity);
            // Add to the static list of all objects in the scene.
            //AllObjects.Add(pEntity);

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

            // Set the entity member manager to this instance of the entity manager
            pEntity.mManager = this;
            pEntity.EntityKey = entityKey++;
        }

        /// <summary>
        /// Stops the manager managing this object, finds the object by its unique key in the the lists its contained in.
        /// Does not remove the entity from the static all objects list
        /// </summary>
        /// <param name="pKey"></param>
        public void StopManaging(int pKey)
        {
            for (int i = 0; i < mObjects.Count; i++)
            {
                if (mObjects[i].EntityKey == pKey)
                {
                    // Add the entity to the correct sub list containing specific types.
                    if (mObjects[i] is Sphere)
                    {
                        for (int j = 0; j < Spheres.Count; j++)
                        {
                            if (mObjects[i].EntityKey == pKey)
                            {
                                Spheres.RemoveAt(j);
                            }
                        }
                    }
                    else if (mObjects[i] is Cylinder)
                    {
                        for (int j = 0; j < Cylinders.Count; j++)
                        {
                            if (mObjects[i].EntityKey == pKey)
                            {
                                Cylinders.RemoveAt(j);
                            }
                        }
                    }
                    else if (mObjects[i] is Cube)
                    {
                        for (int j = 0; j < Cubes.Count; j++)
                        {
                            if (mObjects[i].EntityKey == pKey)
                            {
                                Cubes.RemoveAt(j);
                            }
                        }
                    }

                    mObjects.RemoveAt(i);
                    break;
                }
            }




        }

        public void RemoveEntity()
        {

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
                        if (Spheres[i].hasCollidedWithCylinder(Cylinders[c]))
                        {
                            //ACWWindow.particleManager.ParticleEffectSpheres(Spheres[i].mPosition);
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
