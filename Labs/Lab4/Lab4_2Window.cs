﻿using System;
using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labs.Lab4
{
    public class Lab4_2Window : GameWindow
    {
        private int[] mVertexArrayObjectIDArray = new int[2];
        private int[] mVertexBufferObjectIDArray = new int[2];
        private ShaderUtility mShader;
        private Matrix4 mSquareMatrix;
        private Vector3 mCirclePosition;
        private Vector3 mCircleVelocity;
        private float mCircleRadius;
        private Timer mTimer;

        public Lab4_2Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 4_2 Physically Based Simulation",
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
            GL.ClearColor(Color4.AliceBlue);

            mShader = new ShaderUtility(@"Lab4/Shaders/vLab4.vert", @"Lab4/Shaders/fLab4.frag");
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            GL.UseProgram(mShader.ShaderProgramID);


            #region square

            float[] vertices = new float[] { 
                   -1f, -1f,
                   1f, -1f,
                   1f, 1f,
                   -1f, 1f
            };

            GL.GenVertexArrays(mVertexArrayObjectIDArray.Length, mVertexArrayObjectIDArray);
            GL.GenBuffers(mVertexBufferObjectIDArray.Length, mVertexBufferObjectIDArray);

            GL.BindVertexArray(mVertexArrayObjectIDArray[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            mSquareMatrix = Matrix4.CreateScale(4f) * Matrix4.CreateRotationZ(0.0f) * Matrix4.CreateTranslation(0, 0, 0);
            #endregion

            #region circle

            vertices = new float[200];

            for (int i = 0; i < 100; ++i)
            {
                vertices[2 * i] = (float)Math.Cos(MathHelper.DegreesToRadians(i * 360.0 / 100));
                vertices[2 * i + 1] = (float)Math.Cos(MathHelper.DegreesToRadians(90.0 + i * 360.0 / 100));
            }


            GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[1]);

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            mCircleRadius = 0.2f;
            mCirclePosition = new Vector3(0, 2, 0);
            mCircleVelocity = new Vector3(2f, 2.0f, 0);
            
            #endregion

            int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            Matrix4 m = Matrix4.CreateTranslation(0, 0, 0);
            GL.UniformMatrix4(uViewLocation, true, ref m);

            base.OnLoad(e);

            mTimer = new Timer();
            mTimer.Start();
        }

        private void SetCamera()
        {
            float height = ClientRectangle.Height;
            float width = ClientRectangle.Width;
            if (mShader != null)
            {
                Matrix4 proj;
                if (height > width)
                {
                    if (width == 0)
                    {
                        width = 1;
                    }
                    proj = Matrix4.CreateOrthographic(10, 10 * height / width, 0, 10);
                }
                else
                {
                    if (height == 0)
                    {
                        height = 1;
                    }
                    proj = Matrix4.CreateOrthographic(10 * width / height, 10, 0, 10);
                }
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                GL.UniformMatrix4(uProjectionLocation, true, ref proj);
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            float timestep = mTimer.GetElapsedSeconds();
            Vector3 oldPosition = mCirclePosition;



            // move the circle into square space by transforming it by inverse of square 1 matrix
            Vector4 circleInSquareSpace = Vector4.Transform(new Vector4(mCirclePosition, 1), mSquareMatrix.Inverted());

            if (circleInSquareSpace.X + (mCircleRadius / mSquareMatrix.ExtractScale().X) > 1) // right
            {
                Vector3 normal = Vector3.Transform(new Vector3(1, 0, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
            }
            if (circleInSquareSpace.X - (mCircleRadius / mSquareMatrix.ExtractScale().X) < -1) // left
            {
                Vector3 normal = Vector3.Transform(new Vector3(-1, 0, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
            }
            if (circleInSquareSpace.Y + (mCircleRadius / mSquareMatrix.ExtractScale().X) > 1) // top
            {
                Vector3 normal = Vector3.Transform(new Vector3(0, 1, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
            }
            if (circleInSquareSpace.Y - (mCircleRadius / mSquareMatrix.ExtractScale().X) < -1) // bottom
            {
                Vector3 normal = Vector3.Transform(new Vector3(0, -1, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
            }








            mCirclePosition = mCirclePosition + mCircleVelocity * timestep;
            
            base.OnUpdateFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle);
            SetCamera();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int uModelMatrixLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            int uColourLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uColour");

            GL.Uniform4(uColourLocation, Color4.DodgerBlue);

            GL.UniformMatrix4(uModelMatrixLocation, true, ref mSquareMatrix);
            GL.BindVertexArray(mVertexArrayObjectIDArray[0]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 4);

            Matrix4 circleMatrix = Matrix4.CreateScale(mCircleRadius) * Matrix4.CreateTranslation(mCirclePosition);

            GL.UniformMatrix4(uModelMatrixLocation, true, ref circleMatrix);
            GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 100);

            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            GL.DeleteBuffers(mVertexBufferObjectIDArray.Length, mVertexBufferObjectIDArray);
            GL.DeleteVertexArrays(mVertexArrayObjectIDArray.Length, mVertexArrayObjectIDArray);
            GL.UseProgram(0);
            mShader.Delete();
        }
    }
}