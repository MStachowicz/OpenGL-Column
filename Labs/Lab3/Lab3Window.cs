using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;

namespace Labs.Lab3
{
    public class Lab3Window : GameWindow
    {
        public Lab3Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 3 Lighting and Material Properties",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        private int[] mVBO_IDs = new int[8];
        private int[] mVAO_IDs = new int[5];
        private ShaderUtility mShader;
        private ModelUtility mSphereModelUtility, mStatuelUtility, mCylinderUtility;
        private Matrix4 mView, mSphereModel, mGroundModel, mStatueModel, mStatueScale, mCylinderModel;

        protected override void OnLoad(EventArgs e)
        {
            // Set some GL state
            GL.ClearColor(Color4.White);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            // mShader = new ShaderUtility(@"Lab3/Shaders/vLighting.vert", @"Lab3/Shaders/fPassThrough.frag");
            mShader = new ShaderUtility(@"Lab3/Shaders/vPassThrough.vert", @"Lab3/Shaders/fLighting.frag");
            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vNormal = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal"); //find the index for the location of vNormal in the shader


            // Generating Vertex Array Objects and Vertex Buffer Objects
            GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);

            #region floor

            float[] vertices = new float[] {-10, 0, -10,0,1,0,
                                             -10, 0, 10,0,1,0,
                                             10, 0, 10,0,1,0,
                                             10, 0, -10,0,1,0,};

            GL.BindVertexArray(mVAO_IDs[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(vNormal); // enable it
            GL.VertexAttribPointer(vNormal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float)); //added
            #endregion

            #region Sphere model
            mSphereModelUtility = ModelUtility.LoadModel(@"Utility/Models/sphere.bin");

            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[2]);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mSphereModelUtility.Indices.Length * sizeof(float)), mSphereModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(mVAO_IDs[1]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[1]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mSphereModelUtility.Vertices.Length * sizeof(float)), mSphereModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[2]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mSphereModelUtility.Indices.Length * sizeof(float)), mSphereModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mSphereModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mSphereModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(vNormal); // ADDITIONAL
            GL.VertexAttribPointer(vNormal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));
            #endregion

            #region statue
            mStatuelUtility = ModelUtility.LoadModel(@"Utility/Models/model.bin");

            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[3]);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mStatuelUtility.Indices.Length * sizeof(float)), mStatuelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(mVAO_IDs[2]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[3]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mStatuelUtility.Vertices.Length * sizeof(float)), mStatuelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[4]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mStatuelUtility.Indices.Length * sizeof(float)), mStatuelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mStatuelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mStatuelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(vNormal); // ADDITIONAL
            GL.VertexAttribPointer(vNormal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            #endregion

            #region cylinder model
            mCylinderUtility = ModelUtility.LoadModel(@"Utility/Models/cylinder.bin");

            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[2]);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mSphereModelUtility.Indices.Length * sizeof(float)), mSphereModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(mVAO_IDs[3]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[6]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mCylinderUtility.Vertices.Length * sizeof(float)), mCylinderUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[7]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mCylinderUtility.Indices.Length * sizeof(float)), mCylinderUtility.Indices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCylinderUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCylinderUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(vNormal); // ADDITIONAL
            GL.VertexAttribPointer(vNormal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));
            #endregion

            GL.BindVertexArray(0);

            int uLightDirectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLightPosition");
            Vector4 lightDirection = new Vector4(2.0f, 1.0f, -8.5f, 1);
            GL.Uniform4(uLightDirectionLocation, lightDirection);

            mView = Matrix4.CreateTranslation(0, -1.5f, 0);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);

            Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
            GL.Uniform4(uLightDirectionLocation, lightPosition);


            int uEyePosition = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            Vector4 eyePosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
            GL.Uniform4(uEyePosition, eyePosition);


            mGroundModel = Matrix4.CreateTranslation(0, 0, -5f);
            mSphereModel = Matrix4.CreateTranslation(0, 1, -5f);
            mStatueScale = Matrix4.CreateScale(0.7f);
            mStatueModel = Matrix4.CreateTranslation(0, 0.9f, 0);
            mCylinderModel = Matrix4.CreateTranslation(-5, 0, -5f);

            base.OnLoad(e);

        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle);
            if (mShader != null)
            {
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 25);
                GL.UniformMatrix4(uProjectionLocation, true, ref projection);
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar == 'w')
            {
                // Camera movement
                mView = mView * Matrix4.CreateTranslation(0.0f, 0.0f, 0.05f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);

                // Light direction
                int uLightDirectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLightPosition");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                GL.Uniform4(uLightDirectionLocation, lightPosition);

                // Specular light
                int uEyePosition = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                GL.Uniform4(uEyePosition, eyePosition);

            }
            if (e.KeyChar == 'a')
            {
                mView = mView * Matrix4.CreateRotationY(-0.05f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);

                int uLightDirectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLightPosition");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                GL.Uniform4(uLightDirectionLocation, lightPosition);

                // Specular light
                int uEyePosition = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                GL.Uniform4(uEyePosition, eyePosition);
            }
            if (e.KeyChar == 's')
            {
                mView = mView * Matrix4.CreateTranslation(0.0f, 0.0f, -0.05f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);

                int uLightDirectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLightPosition");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                GL.Uniform4(uLightDirectionLocation, lightPosition);

                // Specular light
                int uEyePosition = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                GL.Uniform4(uEyePosition, eyePosition);
            }
            if (e.KeyChar == 'd')
            {
                mView = mView * Matrix4.CreateRotationY(+0.05f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);

                int uLightDirectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLightPosition");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                GL.Uniform4(uLightDirectionLocation, lightPosition);

                // Specular light
                int uEyePosition = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                GL.Uniform4(uEyePosition, eyePosition);
            }
            if (e.KeyChar == 'z')
            {
                Vector3 t = mGroundModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mGroundModel = mGroundModel * inverseTranslation * Matrix4.CreateRotationY(-0.025f) * translation;

                Vector3 j = mSphereModel.ExtractTranslation();
                Matrix4 stranslation = Matrix4.CreateTranslation(j);
                Matrix4 sinverseTranslation = Matrix4.CreateTranslation(-j);
                mSphereModel = mSphereModel * sinverseTranslation * Matrix4.CreateRotationY(-0.025f) * stranslation;
            }
            if (e.KeyChar == 'x')
            {
                Vector3 t = mGroundModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mGroundModel = mGroundModel * inverseTranslation * Matrix4.CreateRotationY(0.025f) * translation;

                Vector3 j = mSphereModel.ExtractTranslation();
                Matrix4 stranslation = Matrix4.CreateTranslation(j);
                Matrix4 sinverseTranslation = Matrix4.CreateTranslation(-j);
                mSphereModel = mSphereModel * sinverseTranslation * Matrix4.CreateRotationY(0.025f) * stranslation;
            }
            if (e.KeyChar == 'c') // rotating sphere in sphere space
            {
                Vector3 t = mSphereModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mSphereModel = mSphereModel * inverseTranslation * Matrix4.CreateRotationY(-0.025f) * translation;
            }
            if (e.KeyChar == 'v') // rotating sphere in sphere space
            {
                Vector3 t = mSphereModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mSphereModel = mSphereModel * inverseTranslation * Matrix4.CreateRotationY(0.025f) * translation;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            int uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref mGroundModel);

            GL.BindVertexArray(mVAO_IDs[0]);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            Matrix4 m = mSphereModel * mGroundModel;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m);

            GL.BindVertexArray(mVAO_IDs[1]);
            GL.DrawElements(PrimitiveType.Triangles, mSphereModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            #region Cylinder
            Matrix4 m2 = mStatueScale * mCylinderModel * mGroundModel;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m2);

            GL.BindVertexArray(mVAO_IDs[3]);
            GL.DrawElements(PrimitiveType.Triangles, mCylinderUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
            #endregion

            #region statue
            Matrix4 m3 = mStatueModel * m2;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m3); // uses the cylinder matrix.

            GL.BindVertexArray(mVAO_IDs[2]);
            GL.DrawElements(PrimitiveType.Triangles, mStatuelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.DrawElements(PrimitiveType.Triangles, mStatuelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
            #endregion

            GL.BindVertexArray(0);
            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffers(mVBO_IDs.Length, mVBO_IDs);
            GL.DeleteVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            mShader.Delete();
            base.OnUnload(e);
        }
    }
}
