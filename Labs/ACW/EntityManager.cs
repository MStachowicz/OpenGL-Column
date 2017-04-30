using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Labs.Utility;

namespace Labs.ACW
{
    public class EntityManager
    {
        public EntityManager()
        { }

        public void ManageEntity(entity pEntity)
        {
            mObjects.Add(pEntity);
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

        public void CheckCollisions()
        {
            //for (int i = 0; i < mObjects.Count; i++)
            //{
            //    // if the object is not static
            //    if (!mObjects[i].staticObject)
            //    {
            //        // Sphere collision tests
            //        if (mObjects[i] is Sphere)
            //        {

            //        }
            //    }
            //}

            for (int i = 0; i < ACWWindow.SphereList.Count; i++)
            {
                // CUBE COLLISION CHECK            
                ACWWindow.SphereList[i].hasCollidedWithCube(ACWWindow.CubeList[0]); // hard coded single cube

                // SPHERE ON SPHERE COLLISION CHECK
                foreach (Sphere j in ACWWindow.SphereList)
                {
                    // check for collisions with all other spheres
                    if (!ACWWindow.SphereList[i].Equals(j)) // If this is not the same sphere
                        if (ACWWindow.SphereList[i].hasCollidedWithSphere(j)) // check collision
                            ACWWindow.SphereList[i].sphereOnSphereResponse(j); // perform response

                    j.hasCollidedWithSphere(ACWWindow.doomSphere); // check every sphere for collision with doom sphere.
                }
                // SPHERE ON CYLINDER CHECK performs response too
                foreach (Cylinder j in ACWWindow.CylinderList)
                    ACWWindow.SphereList[i].hasCollidedWithCylinder(j);
            }
        }

        /// <summary>
        /// Updates every object in its list is it's not a static object.
        /// </summary>
        /// <param name="pTimestep"></param>
        /// <param name="pGravity"></param>
        public void updateObjects()
        {
            foreach (entity i in mObjects)
            {
                if (!i.staticObject) // if the object is not static.
                {
                    i.Update();
                }
            }
        }

        /// <summary>
        /// All the objects this entity manager is responsible for.
        /// </summary>
        public List<entity> mObjects = new List<entity>();

        public static int VAOCount = 0;
        public static int VBOCount = 0;
    }

}
