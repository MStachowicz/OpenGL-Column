using System;
using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labs.Lab4
{
    public class Lab4_1Window : GameWindow
    {
        private ShaderUtility mShader;

        private int[] mVertexArrayObjectIDArray = new int[4];
        private int[] mVertexBufferObjectIDArray = new int[4];

        // 2 Squares
        private Matrix4 mSquareMatrix, mSquareMatrix2;

        // 2 Circles
        private Vector3 mCirclePosition, mCircleVelocity, mCirclePosition2, mCircle2Velocity;
        private float mCircleRadius, mCircle2Radius;

        private Timer mTimer;

        public Lab4_1Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 4_1 Simple Animation and Collision Detection",
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


            GL.GenVertexArrays(mVertexArrayObjectIDArray.Length, mVertexArrayObjectIDArray);
            GL.GenBuffers(mVertexBufferObjectIDArray.Length, mVertexBufferObjectIDArray);

            #region square

            float[] vertices = new float[] {
                   -1f, -1f,
                   1f, -1f,
                   1f, 1f,
                   -1f, 1f
            };

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

            mSquareMatrix = Matrix4.CreateScale(3f, 2f, 1f) * Matrix4.CreateRotationZ(0.5f) * Matrix4.CreateTranslation(0.5f, 0.5f, 0);

            #endregion

            #region Square 2

            GL.BindVertexArray(mVertexArrayObjectIDArray[3]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[3]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            //mSquareMatrix2 = Matrix4.CreateScale(0.3f) * Matrix4.CreateRotationZ(0.0f) * Matrix4.CreateTranslation(-0.4f, 0.0f, 0);
            mSquareMatrix2 = Matrix4.CreateScale(1f) * Matrix4.CreateRotationZ(0.5f) * Matrix4.CreateTranslation(0.0f, 0.0f, 0);

            #endregion

            #region Circle 1

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

            int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            Matrix4 m = Matrix4.CreateTranslation(0, 0, 0);
            GL.UniformMatrix4(uViewLocation, true, ref m);

            // CIRCLE PROPERTIES
            mCirclePosition = new Vector3(1.5f, 1.5f, 0.0f);
            mCircleVelocity = new Vector3(-1.0f, -1.0f, 0.0f);
            mCircleRadius = 0.1f;

            #endregion

            #region Circle 2
            GL.BindVertexArray(mVertexArrayObjectIDArray[2]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[2]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            m = Matrix4.CreateTranslation(0, 0, 0);
            GL.UniformMatrix4(uViewLocation, true, ref m);

            mCirclePosition2 = new Vector3(-6.8f, 0.0f, 0.0f);
            mCircle2Velocity = new Vector3(0.0f, 0.0f, 0.0f);
            mCircle2Radius = 0.4f;

            #endregion


            mTimer = new Timer();
            mTimer.Start();

            base.OnLoad(e);
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




            GL.Uniform4(uColourLocation, Color4.DodgerBlue); // set to blue

            // Square
            GL.UniformMatrix4(uModelMatrixLocation, true, ref mSquareMatrix);
            GL.BindVertexArray(mVertexArrayObjectIDArray[0]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 4);

            // Square 2
            GL.UniformMatrix4(uModelMatrixLocation, true, ref mSquareMatrix2);
            GL.BindVertexArray(mVertexArrayObjectIDArray[3]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 4);

            // Circle 1
            Matrix4 circleMatrix = Matrix4.CreateScale(mCircleRadius) * Matrix4.CreateTranslation(mCirclePosition);
            GL.UniformMatrix4(uModelMatrixLocation, true, ref circleMatrix);
            GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 100);

            // Circle 2
            Matrix4 circle2Matrix = Matrix4.CreateScale(mCircle2Radius) * Matrix4.CreateTranslation(mCirclePosition2);
            GL.UniformMatrix4(uModelMatrixLocation, true, ref circle2Matrix);
            GL.BindVertexArray(mVertexArrayObjectIDArray[2]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 100);



            // Following code redraws the scene in square space in red
            GL.Uniform4(uColourLocation, Color4.Red); // set to red

            // Square
            Matrix4 m = mSquareMatrix * mSquareMatrix.Inverted();
            GL.UniformMatrix4(uModelMatrixLocation, true, ref m);
            GL.BindVertexArray(mVertexArrayObjectIDArray[0]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 4);

            // Square 2
            m = mSquareMatrix2 * mSquareMatrix.Inverted();
            GL.UniformMatrix4(uModelMatrixLocation, true, ref m);
            GL.BindVertexArray(mVertexArrayObjectIDArray[3]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 4);

            // Circle 1
            m = (Matrix4.CreateScale(mCircleRadius) * Matrix4.CreateTranslation(mCirclePosition)) * mSquareMatrix.Inverted();
            GL.UniformMatrix4(uModelMatrixLocation, true, ref m);
            GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 100);

            // Circle 2
            m = (Matrix4.CreateScale(mCircle2Radius) * Matrix4.CreateTranslation(mCirclePosition2)) * mSquareMatrix.Inverted();
            GL.UniformMatrix4(uModelMatrixLocation, true, ref m);
            GL.BindVertexArray(mVertexArrayObjectIDArray[2]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 100);


            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            float timestep = mTimer.GetElapsedSeconds();
            mCirclePosition = mCirclePosition + mCircleVelocity * timestep; // move the circle on this update

            #region Circle 1 collision with square 1

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

            #endregion

            #region Circle 1 collision with circle 2 

            double x = mCirclePosition.X - mCirclePosition2.X;
            double y = mCirclePosition.Y - mCirclePosition2.Y;
            double distance = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

            if (distance < mCircleRadius + mCircle2Radius)
            {
                Vector3 normal = (mCirclePosition - mCirclePosition2).Normalized();
                mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
            }

            #endregion      

            #region circle 1 collision with square 2 outside

            // move the circle into square 2 space to perform collision detection
            circleInSquareSpace = Vector4.Transform(new Vector4(mCirclePosition, 1), mSquareMatrix2.Inverted());

            // Square Vertices
            Vector4 topLeft = new Vector4(-1.0f, 1.0f, 0.0f, 1f);
            //topLeft = Vector4.Transform(topLeft, mSquareMatrix2.Inverted());
            Vector4 bottomLeft = new Vector4(-1.0f, -1.0f, 0.0f, 1f);
            //bottomLeft = Vector4.Transform(topLeft, mSquareMatrix2.Inverted());
            Vector4 topRight = new Vector4(1.0f, 1.0f, 0.0f, 1f);
            //topRight = Vector4.Transform(topLeft, mSquareMatrix2.Inverted());
            Vector4 bottomRight = new Vector4(1.0f, -1.0f, 0.0f, 1f);
            //bottomRight = Vector4.Transform(topLeft, mSquareMatrix2.Inverted());

            Vector4 LineSegment;
            Vector4 LineSegmentToCircle;
            float LineSegmentf;

            // LEFT
            LineSegment = (Vector4.Dot((circleInSquareSpace - bottomLeft), (topLeft - bottomLeft).Normalized()) * (topLeft - bottomLeft).Normalized());
            LineSegmentf = (Vector4.Dot((circleInSquareSpace - bottomLeft), (topLeft - bottomLeft).Normalized()));
            LineSegmentToCircle = bottomLeft + LineSegment - circleInSquareSpace;
            if (LineSegmentToCircle.Length < mCircleRadius)
            {
                if (LineSegmentf > 0 && LineSegment.Length < (topLeft - bottomLeft).Length) // nested if checks if circle passing above the square
                {
                    Vector3 normal = Vector3.Transform(new Vector3(-1, 0, 0), mSquareMatrix2.ExtractRotation());
                   // mCircleVelocity = Vector3.Transform((mCircleVelocity), mSquareMatrix2.ExtractRotation().Inverted());
                    mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
                    //mCircleVelocity = Vector3.Transform(mCircleVelocity, mSquareMatrix2.ExtractRotation());
                }
            }
            // TOP        
            LineSegment = (Vector4.Dot((circleInSquareSpace - topLeft), (topRight - topLeft).Normalized()) * (topRight - topLeft).Normalized());
            LineSegmentf = (Vector4.Dot((circleInSquareSpace - topLeft), (topRight - topLeft).Normalized()));
            LineSegmentToCircle = topLeft + LineSegment - circleInSquareSpace;
            if (LineSegmentToCircle.Length < mCircleRadius)
            {
                if (LineSegmentf > 0 && LineSegment.Length < (topRight - topLeft).Length)
                {
                    Vector3 normal = Vector3.Transform(new Vector3(0, 1, 0), mSquareMatrix2.ExtractRotation());
                   // mCircleVelocity = Vector3.Transform((mCircleVelocity), mSquareMatrix2.ExtractRotation().Inverted());
                    mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
                   // mCircleVelocity = Vector3.Transform(mCircleVelocity, mSquareMatrix2.ExtractRotation());
                }
            }
            // RIGHT
            LineSegment = (Vector4.Dot((circleInSquareSpace - bottomRight), (topRight - bottomRight).Normalized()) * (topRight - bottomRight).Normalized());
            LineSegmentf = (Vector4.Dot((circleInSquareSpace - bottomRight), (topRight - bottomRight).Normalized()));
            LineSegmentToCircle = bottomRight + LineSegment - circleInSquareSpace;
            if (LineSegmentToCircle.Length < mCircleRadius)
            {
                if (LineSegmentf > 0 && LineSegment.Length < (topRight - bottomRight).Length)
                {
                    Vector3 normal = Vector3.Transform(new Vector3(1, 0, 0), mSquareMatrix2.ExtractRotation());
                    //mCircleVelocity = Vector3.Transform((mCircleVelocity), mSquareMatrix2.ExtractRotation().Inverted());
                    mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
                   // mCircleVelocity = Vector3.Transform(mCircleVelocity, mSquareMatrix2.ExtractRotation());
                }
            }

            // BOTTOM
            LineSegment = (Vector4.Dot((circleInSquareSpace - bottomLeft), (bottomRight - bottomLeft).Normalized()) * (bottomRight - bottomLeft).Normalized());
            LineSegmentf = (Vector4.Dot((circleInSquareSpace - bottomLeft), (bottomRight - bottomLeft).Normalized()));
            LineSegmentToCircle = bottomLeft + LineSegment - circleInSquareSpace;
            if (LineSegmentToCircle.Length < mCircleRadius)
            {
                if (LineSegmentf > 0 && LineSegment.Length < (bottomRight - bottomLeft).Length)
                {
                    Vector3 normal = Vector3.Transform(new Vector3(0, -1, 0), mSquareMatrix2.ExtractRotation());
                    //mCircleVelocity = Vector3.Transform((mCircleVelocity), mSquareMatrix2.ExtractRotation().Inverted());
                    mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
                   // mCircleVelocity = Vector3.Transform(mCircleVelocity, mSquareMatrix2.ExtractRotation());
                }
            }

            //corner collisions for circle 1 and square 2  (circle collision)

            double cornerX;
            double cornerY;
            double cornerDistance;

            // TOP LEFT COLLISION
            cornerX = circleInSquareSpace.X - topLeft.X;
            cornerY = circleInSquareSpace.Y - topLeft.Y;
            cornerDistance = Math.Sqrt(Math.Pow(cornerX, 2) + Math.Pow(cornerY, 2));

            if (cornerDistance < mCircleRadius)
            {
                Vector3 normal = (mCirclePosition - new Vector3((float)cornerX, (float)cornerY, 0)).Normalized();
               // mCircleVelocity = Vector3.Transform((mCircleVelocity), mSquareMatrix2.ExtractRotation().Inverted());
                mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
               // mCircleVelocity = Vector3.Transform(mCircleVelocity, mSquareMatrix2.ExtractRotation());
            }

            // TOP RIGHT COLLISION
            cornerX = circleInSquareSpace.X - topRight.X;
            cornerY = circleInSquareSpace.Y - topRight.Y;
            cornerDistance = Math.Sqrt(Math.Pow(cornerX, 2) + Math.Pow(cornerY, 2));

            if (cornerDistance < mCircleRadius)
            {
                Vector3 normal = (mCirclePosition - new Vector3((float)cornerX, (float)cornerY, 0)).Normalized();
                //mCircleVelocity = Vector3.Transform((mCircleVelocity), mSquareMatrix2.ExtractRotation().Inverted());
                mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
                //mCircleVelocity = Vector3.Transform(mCircleVelocity, mSquareMatrix2.ExtractRotation());
            }

            // BOTTOM RIGHT
            cornerX = circleInSquareSpace.X - bottomRight.X;
            cornerY = circleInSquareSpace.Y - bottomRight.Y;
            cornerDistance = Math.Sqrt(Math.Pow(cornerX, 2) + Math.Pow(cornerY, 2));

            if (cornerDistance < mCircleRadius)
            {
                Vector3 normal = (mCirclePosition - new Vector3((float)cornerX, (float)cornerY, 0)).Normalized();
                //mCircleVelocity = Vector3.Transform((mCircleVelocity), mSquareMatrix2.ExtractRotation().Inverted());
                mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
                //mCircleVelocity = Vector3.Transform(mCircleVelocity, mSquareMatrix2.ExtractRotation());
            }
            // BOTTOM LEFT
            cornerX = circleInSquareSpace.X - bottomLeft.X;
            cornerY = circleInSquareSpace.Y - bottomLeft.Y;
            cornerDistance = Math.Sqrt(Math.Pow(cornerX, 2) + Math.Pow(cornerY, 2));

            if (cornerDistance < mCircleRadius)
            {
                Vector3 normal = (mCirclePosition - new Vector3((float)cornerX, (float)cornerY, 0)).Normalized();
                //mCircleVelocity = Vector3.Transform((mCircleVelocity), mSquareMatrix2.ExtractRotation().Inverted());
                mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;
                //mCircleVelocity = Vector3.Transform(mCircleVelocity, mSquareMatrix2.ExtractRotation());
            }

            #endregion

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