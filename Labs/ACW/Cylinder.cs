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
    public class Cylinder : entity
    {
        /// <summary>
        /// Places a cylinder in cube.
        /// </summary>
        /// <param name="pCube">The cube whos matrix forms the sceneGraph node </param>
        /// <param name="pPosition">The translation to apply to the cylinder matrix.</param>
        /// <param name="pRadius">The radius of the cylinder. 1 = 1 meter.</param>
        public Cylinder(Vector3 pPosition, float pRadius)
        {
            VAOIndex = entityManager.VAOCount++;
            VBOIndex = entityManager.VBOCount++;
            entityManager.VBOCount++;

            mRadius = pRadius;

            mScaleX = pRadius;
            mScaleY = 0.53f; // stretches the cylinder to the length of the cube
            mScaleZ = pRadius;

            mPosition = pPosition;

            mMatrix = Matrix4.CreateScale(new Vector3(mScaleX, mScaleY, mScaleZ)) *
                Matrix4.CreateTranslation(mPosition);


            mVelocity = new Vector3(0.0f, 0.0f, 0.0f);

            CylinderBottom = new Vector3(mPosition.X, mPosition.Y - (0.5f * mScaleY), mPosition.Z);
            CylinderTop = new Vector3(mPosition.X, mPosition.Y + (0.5f * mScaleY), mPosition.Z);
        }

        public float mRadius;
        public Vector3 CylinderBottom;
        public Vector3 CylinderTop;



        public void RotateX(float pRotation)
        {
            // https://open.gl/transformations
            // https://www.amazon.co.uk/Mathematics-Game-Programming-Computer-Graphics/dp/1435458869/ref=dp_ob_title_bk

            Vector3 t = mMatrix.ExtractTranslation();

            Matrix4 translation = Matrix4.CreateTranslation(t);
            Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);

            mMatrix = mMatrix * inverseTranslation * Matrix4.CreateRotationX(pRotation) * translation;

            Vector4 test2 = new Vector4(CylinderBottom, 1);
            Vector4 test3 = test2 * inverseTranslation * Matrix4.CreateRotationX(pRotation) * translation;
            CylinderBottom = new Vector3(test3.X, test3.Y, test3.Z);

            Vector4 test4 = new Vector4(CylinderTop, 1);
            Vector4 test5 = test4 * inverseTranslation * Matrix4.CreateRotationX(pRotation) * translation;
            CylinderTop = new Vector3(test5.X, test5.Y, test5.Z);
        }

        public void RotateY(float pRotation)
        {
            string test = this.ToString();
            Vector3 t = mMatrix.ExtractTranslation();

            Matrix4 translation = Matrix4.CreateTranslation(t);
            Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);

            mMatrix = mMatrix * inverseTranslation * Matrix4.CreateRotationY(pRotation) * translation;



            Vector4 test2 = new Vector4(CylinderBottom, 1);
            Vector4 test3 = test2 * inverseTranslation * Matrix4.CreateRotationY(pRotation) * translation;
            CylinderBottom = new Vector3(test3.X, test3.Y, test3.Z);

            Vector4 test4 = new Vector4(CylinderTop, 1);
            Vector4 test5 = test4 * inverseTranslation * Matrix4.CreateRotationY(pRotation) * translation;
            CylinderTop = new Vector3(test5.X, test5.Y, test5.Z);
        }
        public void RotateZ(float pRotation)
        {
            string test = this.ToString();
            Vector3 t = mMatrix.ExtractTranslation();

            Matrix4 translation = Matrix4.CreateTranslation(t);
            Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);

            mMatrix = mMatrix * inverseTranslation * Matrix4.CreateRotationZ(pRotation) * translation;

            Vector4 test2 = new Vector4(CylinderBottom, 1);
            Vector4 test3 = test2 * inverseTranslation * Matrix4.CreateRotationZ(pRotation) * translation;
            CylinderBottom = new Vector3(test3.X, test3.Y, test3.Z);

            Vector4 test4 = new Vector4(CylinderTop, 1);
            Vector4 test5 = test4 * inverseTranslation * Matrix4.CreateRotationZ(pRotation) * translation;
            CylinderTop = new Vector3(test5.X, test5.Y, test5.Z);
        }

        public void scale(Vector3 pScale)
        {
            Vector3 t = mMatrix.ExtractTranslation();
            Matrix4 translation = Matrix4.CreateTranslation(t);
            Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);

            mMatrix = mMatrix * inverseTranslation * Matrix4.CreateScale(pScale) * translation;

            Vector4 test2 = new Vector4(CylinderBottom, 1);
            Vector4 test3 = test2 * inverseTranslation * Matrix4.CreateScale(pScale) * translation;
            CylinderBottom = new Vector3(test3.X, test3.Y, test3.Z);

            Vector4 test4 = new Vector4(CylinderTop, 1);
            Vector4 test5 = test4 * inverseTranslation * Matrix4.CreateScale(pScale) * translation;
            CylinderTop = new Vector3(test5.X, test5.Y, test5.Z);
        }

        public override void Load()
        {
            mModelUtility = ModelUtility.LoadModel(@"Utility/Models/cylinder.bin");
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

            // next 2 lines cause cylinder to dissapear
            GL.EnableVertexAttribArray(ACWWindow.vNormal);
            GL.VertexAttribPointer(ACWWindow.vNormal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));
        }

        public override void Render()
        {
            int uModel = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref mMatrix);

            GL.BindVertexArray(ACWWindow.mVAO_IDs[VAOIndex]);
            GL.DrawElements(PrimitiveType.Triangles, mModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        public override void Update(float pTimestep, Vector3 pGravity)
        {
            throw new NotImplementedException();
        }
    }
}
