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


        /// <summary>
        /// Incremented by all entity constructors to keep count of objects and assign new keys.
        /// </summary>
        public static int entityKeyCount = 0;

        /// <summary>
        /// Creates an empty entity manager to manage a section of the cube.
        /// </summary>
        public EntityManager()
        { }


        public void ManageEntity(entity pEntity)
        {
            // Add the entity to the list of objects this manager controls.
            mObjects.Add(pEntity);

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
            pEntity.mEntityKey = entityKeyCount++;
        }

        /// <summary>
        /// Stops the manager managing this object, finds the object by its unique key in the the lists its contained in.
        /// </summary>
        /// <param name="pKey"></param>
        public void StopManaging(int pKey)
        {
            for (int i = 0; i < mObjects.Count; i++)
            {
                if (mObjects[i].mEntityKey == pKey)
                {
                    // Remove the entity from the correct sub type list.
                    if (mObjects[i] is Sphere)
                    {
                        for (int j = 0; j < Spheres.Count; j++)
                        {
                            if (Spheres[j].mEntityKey == pKey)
                            {
                                Spheres.RemoveAt(j);
                            }
                        }
                    }
                    else if (mObjects[i] is Cylinder)
                    {
                        for (int j = 0; j < Cylinders.Count; j++)
                        {
                            if (Cylinders[j].mEntityKey == pKey)
                            {
                                Cylinders.RemoveAt(j);
                            }
                        }
                    }
                    else if (mObjects[i] is Cube)
                    {
                        for (int j = 0; j < Cubes.Count; j++)
                        {
                            if (Cubes[j].mEntityKey == pKey)
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

        /// <summary>
        /// Passes the entity to the parameter entity manager to manage and removes it from the object list
        /// and its mObjects list.
        /// </summary>
        /// <param name="pEntityManager">The new entity manager the entity belongs to</param>
        /// <param name="pEntity">The entity being removed from the </param>
        public void PassEntity(EntityManager pEntityManager, entity pEntity)
        {
            pEntityManager.ManageEntity(pEntity);
            StopManaging(pEntity.mEntityKey);
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
                if (Spheres[i].sphereType == Sphere.SphereType.yellow || Spheres[i].sphereType == Sphere.SphereType.red)
                {
                    // Sphere collision with cube
                    Spheres[i].hasCollidedWithCube(ACWWindow.cube);

                    // Sphere on sphere detection and response.
                    for (int s = 0; s < Spheres.Count; s++)
                    {
                        if (i != s) // If this is not the same sphere
                            if (Spheres[i].hasCollidedWithSphere(Spheres[s]))  // check collision
                            {
                                // if collision is with sphere of doom
                                if (Spheres[s].sphereType == Sphere.SphereType.doom)
                                {
                                    Spheres[i].SphereOnDoomSphereResponse(Spheres[s]); // perform the response to sphere of doom collision
                                }
                                else
                                {
                                    Spheres[i].sphereOnSphereResponse(Spheres[s]); // perform standard sphere on sphere response
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
        }

        public void resetSpheres()
        {
            for (int i = 0; i < Spheres.Count; i++)
            {
                if (Spheres[i].sphereType == Sphere.SphereType.red)
                {
                    Spheres[i].SetRadius(0.08f);
                    Spheres[i].MoveToEmitterBox(ACWWindow.cube, false);
                }
                if (Spheres[i].sphereType == Sphere.SphereType.yellow)
                {
                    Spheres[i].SetRadius(0.04f);
                    Spheres[i].MoveToEmitterBox(ACWWindow.cube, false);
                }
            }
        }

        /// <summary>
        /// Checks every sphere for its Y position in the column and migrates it to the correct entity manager 
        /// when appropriate.
        /// </summary>
        public void CheckSpherePositions()
        {
            for (int i = 0; i < Spheres.Count; i++)
            {
                // For all the red and yellow spheres in this entity managers sphere list
                if (Spheres[i].sphereType == Sphere.SphereType.red || Spheres[i].sphereType == Sphere.SphereType.yellow)
                {
                    if (Spheres[i].mPosition.Y < 0 && Spheres[i].mPosition.Y > -1) // manager 2 position
                    {
                        if (!Spheres[i].mManager.Equals(ACWWindow.level2Manager))
                        {
                            PassEntity(ACWWindow.level2Manager, Spheres[i]);
                        }
                    }
                    else if (Spheres[i].mPosition.Y < -1 && Spheres[i].mPosition.Y > -2) // manager 3 position
                    {
                        if (!Spheres[i].mManager.Equals(ACWWindow.level3Manager))
                        {
                            PassEntity(ACWWindow.level3Manager, Spheres[i]);
                        }
                    }
                    else
                    {
                        if (!Spheres[i].mManager.Equals(ACWWindow.level1Manager)) // manager 1 position
                        {
                            PassEntity(ACWWindow.level1Manager, Spheres[i]);
                        }
                    }
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
