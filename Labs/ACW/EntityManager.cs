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
    public class entityManager
    {
        public entityManager()
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

                if (i == mObjects.Count - 1) // cube render 
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
        /// All the objects this entity manager is responsible for.
        /// </summary>
        public List<entity> mObjects = new List<entity>();

        public static int VAOCount = 0;
        public static int VBOCount = 0;
    }

}
