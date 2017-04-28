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
        {
            //vertexArrayObject = pVertexArrayObject;
            //vertexBufferObject = pVertexBufferObject;

            //shader = pShader;
        }

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
                //GL.FrontFace(FrontFaceDirection.Cw);

                if (i == 3) // first 3 objects are cubes to be reversed
                {
                    // reset back after the cubes are rendered
                    //GL.FrontFace(FrontFaceDirection.Ccw);
                }


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
                //GL.FrontFace(FrontFaceDirection.Cw);

                if (i == 1) // first 3 objects are cubes to be reversed
                {
                    // reset back after the cubes are rendered
                    //GL.FrontFace(FrontFaceDirection.Ccw);
                    //GL.Enable(EnableCap.CullFace);
                    GL.Disable(EnableCap.CullFace);
                }
                else
                {
                    GL.Enable(EnableCap.CullFace);
                }

                mObjects[i].Render();
            }
        }

        /// <summary>
        /// All the objects this entity manager is responsible for.
        /// </summary>
        public List<entity> mObjects = new List<entity>();

        // Setting up VAO and VBO for use with ACW window
        //public static int[] vertexArrayObject;
        //public static int[] vertexBufferObject;
        //public static ShaderUtility shader;

        //public static int vPositionLocation;
        //public static int vNormal;

        public static int VAOCount = 0;
        public static int VBOCount = 0;
    }

}
