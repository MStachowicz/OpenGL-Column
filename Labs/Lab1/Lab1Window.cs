using System;
using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labs.Lab1
{
    public class Lab1Window : GameWindow
    {
        private int[] mVertexBufferObjectIDArray = new int[2];
        private ShaderUtility mShader;

        public Lab1Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 1 Hello, Triangle",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color4.Green);
            GL.Enable(EnableCap.CullFace);

            float[] vertices = new float[] { 0.0f, 0.8f, 0.8f, 0.4f, 0.6f, -0.6f, -0.6f, -0.6f, -0.8f, 0.4f };
            uint[] indices = new uint[] { 0, 2, 1, 0, 3, 2, 0, 4, 3 };

            //float[] vertices = new float[] { -0.2f, 0.8f,   //V0
            //                                 0.0f, 0.8f,    //V1
            //                                 -0.4f, 0.6f,   //V2
            //                                 -0.2f, 0.6f,   //V3
            //                                 0.0f, 0.6f,    //V4
            //                                 0.4f, 0.6f     //V5
            //                                 -0.8f, 0.2f,   //V6
            //                                 -0.6f, 0.2f,   //V7
            //                                 -0.4f, 0.2f,   //V8
            //                                 0.0f, 0.2f,    //V9
            //                                 0.4f, 0.2f,    //V10
            //                                 0.6f, 0.2f,    //V11
            //                                 0.8f, 0.2f,    //V12
            //                                 -0.4f, -0.2f,  //V13
            //                                 0.2f, -0.2f,   //V14
            //                                 -0.6f, -0.6f,  //V15
            //                                 -0.4f, -0.6f,  //V16
            //                                 0.2f, -0.6f,   //V17
            //                                 0.4f, -0.6f,   //V18
            //                                 0.6f, -0.6f,   //V19
            //                                 0.0f, -0.2f,   //V20
            //                                 0.4f, -0.2f }; //V21

            //uint[] indices = new uint[] {4,1,0,     //Triangle 1
            //                             3,4,0,    //Triangle 2
            //                             6,9,2,     //Triangle 3
            //                             2,9,5,     //Triangle 4
            //                             9,12,5,     //Triangle 5
            //                             17,8,7,     //Triangle 6
            //                             16,17,7,    //Triangle 7
            //                             9,21,10,     //Triangle 8
            //                             9,20,21,     //Triangle 9
            //                             10,19,11,    //Triangle 10
            //                             10,18,19,     //Triangle 11
            //                             14,13,17,     //Triangle 12
            //                             13,16,17};    //Triangle 13

            GL.GenBuffers(2, mVertexBufferObjectIDArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVertexBufferObjectIDArray[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (indices.Length * sizeof(uint) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            #region Shader Loading Code - Can be ignored for now

            mShader = new ShaderUtility( @"Lab1/Shaders/vSimple.vert", @"Lab1/Shaders/fSimple.frag");

            #endregion

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVertexBufferObjectIDArray[1]);

            // shader linking goes here
            #region Shader linking code - can be ignored for now

            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            #endregion

            GL.DrawElements(PrimitiveType.Triangles, 9, DrawElementsType.UnsignedInt, 0);

            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            GL.DeleteBuffers(2, mVertexBufferObjectIDArray);
            GL.UseProgram(0);
            mShader.Delete();
        }
    }
}
