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
    public class Cube : entity
    {
        public Cube()
        {
            // CUBE PROPERTIES
            mDimension = 0.5f;

            mScaleX = 1.0f;
            mScaleY = 4.0f;
            mScaleZ = 1.0f;

            mVolume = 0.0f; // TODO ADD VOLUME FOR CUBE
            mDensity = 1f;
            mMass = mDensity * mVolume;

            cubeDimensions = new Vector3(mDimension * mScaleX, mDimension * mScaleY, mDimension * mScaleZ);
            mPosition = new Vector3(0.0f, 0.0f, 0.0f);
            mVelocity = new Vector3(0.0f, 0.0f, 0.0f);

            mMaterial = Material.pearl;
            staticObject = true;

            centerlevel1 = new Vector3(mPosition.X, mPosition.Y + 0.5f, mPosition.Z);
            centerlevel2 = new Vector3(mPosition.X, mPosition.Y - 0.5f, mPosition.Z);
            centerlevel3 = new Vector3(mPosition.X, mPosition.Y - 1.5f, mPosition.Z);
        }

        public Vector3 centerlevel1;
        public Vector3 centerlevel2;
        public Vector3 centerlevel3;
        public Vector3 cubeDimensions;
        private float mDimension;

        /// <summary>
        /// Has a cube object been loaded previously, if so following instances will use the same VAO and VBO index for their render.
        /// </summary>
        private static bool isLoaded = false;
        /// <summary>
        /// The index in the VAO that all cubes are loaded from. Set in the load method.
        /// </summary>
        public static int VAOIndex;
        /// <summary>
        /// The index in the VBO that all cubes are rendered from. Set in the load method.
        /// </summary>
        public static int VBOIndex;
        /// <summary>
        /// The model utility all the cubes will use.
        /// </summary>
        private static ModelUtility mModelUtility;

        public override void Load()
        {
            if (!isLoaded)
            {
                isLoaded = true;

                VAOIndex = EntityManager.VAOCount++; // set the VAO index all cubes will use.
                VBOIndex = EntityManager.VBOCount++; // set the VBO index all cubes will use.
                EntityManager.VBOCount++; // add extra vbo used for indices of cube faces

                //// 8 vertices forming a cube + normals for each point
                //float[] vertices2 = new float[]
                //{
                //     0.5f,  0.5f, -0.5f,
                //     0.5f,  0.5f,  0.5f,
                //     0.5f, -0.5f, -0.5f,
                //     0.5f, -0.5f,  0.5f,
                //    -0.5f,  0.5f, -0.5f,
                //    -0.5f,  0.5f,  0.5f,
                //    -0.5f, -0.5f, -0.5f,
                //    -0.5f, -0.5f,  0.5f,
                //};

                //float[] vertices = vertices2;

                //// 12 indices, 2 triangles per face.
                //int[] indices = new int[]
                //      {
                //    0,2,1,  /*-1,0,0, // right top*/
                //    2,1,3,  /*-1,0,0, // right bottom   */
                //    1,3,5,  /* 0,0,-1, //  front top*/
                //    3,5,7,  /* 0,0,-1, // front bottom*/
                //    5,7,4,  /* 1,0,0,  // left top*/
                //    7,4,6,  /* 1,0,0,  // left bottom */
                //    4,6,0,  /* 0,0,1,  // back top*/
                //    6,0,2,  /* 0,0,1,  // back bottom*/
                //    0,1,4,  /* 0,-1,0,  // top away*/
                //    1,4,5,  /* 0,-1,0,  // top close*/
                //    2,3,6,  /* 0,1,0,  // bottom away*/
                //    3,6,7  /* 0,1,0   // bottom close*/
                //       };


                //// Binding VAO and VBO for vertices
                //GL.BindVertexArray(ACWWindow.mVAO_IDs[VAOIndex]); // bind vertex array object

                //// VBO 0 - VERTICES
                //GL.BindBuffer(BufferTarget.ArrayBuffer, ACWWindow.mVBO_IDs[VBOIndex]); // bind buffer object
                //// buffer vertex array data
                //GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

                //int size;
                //GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
                //if (vertices.Length * sizeof(float) != size)
                //{
                //    throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
                //}

                //// Set the position location in the shader
                //GL.EnableVertexAttribArray(ACWWindow.vPositionLocation);
                //GL.VertexAttribPointer(ACWWindow.vPositionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

                //// VBO 1 - INDICES + NORMALS
                //GL.BindBuffer(BufferTarget.ElementArrayBuffer, ACWWindow.mVBO_IDs[VBOIndex + 1]);
                //GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), indices, BufferUsageHint.StaticDraw);

                //GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
                //if (indices.Length * sizeof(uint) != size)
                //{
                //    throw new ApplicationException("Index data not loaded onto graphics card correctly");
                //}

                //GL.EnableVertexAttribArray(ACWWindow.vNormal); // enable it
                //GL.VertexAttribPointer(ACWWindow.vNormal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(int), 3 * sizeof(int)); //added






                // MODEL CUBE DRAW

                mModelUtility = ModelUtility.LoadModel(@"Utility/Models/cube.sjg");
                int size;

                GL.BindVertexArray(ACWWindow.mVAO_IDs[VAOIndex]);

                GL.BindBuffer(BufferTarget.ArrayBuffer, ACWWindow.mVBO_IDs[VBOIndex]);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mModelUtility.Vertices.Length * sizeof(float)), mModelUtility.Vertices, BufferUsageHint.StaticDraw);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ACWWindow.mVBO_IDs[VBOIndex + 1]);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mModelUtility.Indices.Length * sizeof(float)), mModelUtility.Indices, BufferUsageHint.StaticDraw);

                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
                if (mModelUtility.Vertices.Length * sizeof(float) != size)
                {
                    throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
                }

                GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
                if (mModelUtility.Indices.Length * sizeof(float) != size)
                {
                    throw new ApplicationException("Index data not loaded onto graphics card correctly");
                }
                 //Vector4 test = new Vector4(1.0f,0.0f,0.0f);
                GL.EnableVertexAttribArray(ACWWindow.vPositionLocation);
                GL.VertexAttribPointer(ACWWindow.vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

                GL.EnableVertexAttribArray(ACWWindow.vNormal);
                GL.VertexAttribPointer(ACWWindow.vNormal, 3, VertexAttribPointerType.Int, true, 6 * sizeof(int), 3 * sizeof(int));
            }
        }
        public override void Render()
        {
            GL.Enable(EnableCap.CullFace);

            mMatrix = Matrix4.CreateScale(new Vector3(mScaleX, mScaleY, mScaleZ)) * Matrix4.CreateTranslation(mPosition);

            int uModel = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref mMatrix);



            GL.BindVertexArray(ACWWindow.mVAO_IDs[VAOIndex]);
            GL.DrawElements(PrimitiveType.Triangles, mModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.Disable(EnableCap.CullFace);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
