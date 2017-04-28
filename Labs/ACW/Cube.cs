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
            VAOIndex = entityManager.VAOCount++;
            VBOIndex = entityManager.VBOCount++;
            entityManager.VBOCount++; // cube uses two VBOS

            // CUBE PROPERTIES
            mDimension = 0.5f;

            mScaleX = 1.0f;
            mScaleY = 4.0f;
            mScaleZ = 1.0f;


            mRotationX = 1.0f;
            mRotationY = 1.0f;
            mRotationZ = 1.0f;

            mVolume = 0.0f; // TODO ADD VOLUME FOR CUBE
            mDensity = 1f;
            mMass = mDensity * mVolume;

            cubeDimensions = new Vector3(mDimension * mScaleX, mDimension * mScaleY, mDimension * mScaleZ);
            mPosition = new Vector3(0.0f, 0.0f, 0.0f);
            mVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        }

        public Vector3 cubeDimensions;
        private float mDimension;

        public override void Load()
        {
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

            GL.EnableVertexAttribArray(ACWWindow.vPositionLocation);
            GL.VertexAttribPointer(ACWWindow.vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(ACWWindow.vNormal);
            GL.VertexAttribPointer(ACWWindow.vNormal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));
        }
        public override void Render()
        {
            mMatrix = Matrix4.CreateScale(new Vector3(mScaleX, mScaleY, mScaleZ)) *
            //Matrix4.CreateRotationX(mRotationX) *
            //Matrix4.CreateRotationX(mRotationY) *
            //Matrix4.CreateRotationX(mRotationZ) *
            Matrix4.CreateTranslation(mPosition);

            int uModel = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uModel");
            Matrix4 moveToWorldSpace = mMatrix * ACWWindow.cubeSpace;
            GL.UniformMatrix4(uModel, true, ref moveToWorldSpace);

            GL.BindVertexArray(ACWWindow.mVAO_IDs[VAOIndex]);
            GL.DrawElements(PrimitiveType.Triangles, mModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        public override void Update(float pTimestep, Vector3 pGravity)
        {
            throw new NotImplementedException();
        }
    }
}
