﻿using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Labs.Utility;
using System;

namespace Labs.ACW
{
    public class ACWWindow : GameWindow
    {
        public ACWWindow()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Assessed Coursework",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        private ShaderUtility mShader;
        private Matrix4 mView;

        private int[] mVAO_IDs = new int[3];
        private int[] mVBO_IDs = new int[5];

        private Timer mTimer;

        public Matrix4 accelerationDueToGravity = Matrix4.CreateTranslation(new Vector3(0.0f, -9.81f, 0.0f));
        float restitution = 1f;

        // Cube properties
        private ModelUtility mCubeModelUtility;

        // Objects
        CubeFace floor;
        Sphere sphere1;

        protected override void OnLoad(EventArgs e)
        {
            // Set some GL state
            GL.ClearColor(Color4.CadetBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            #region Loading in shaders and shader variables

            mShader = new ShaderUtility(@"ACW/Shaders/vertexShader.vert", @"ACW/Shaders/fragmentShader.frag");
            GL.UseProgram(mShader.ShaderProgramID);

            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vNormal = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal"); //find the index for the location of vNormal in the shader

            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            mView = Matrix4.CreateTranslation(0, -1.5f, 0);
            GL.UniformMatrix4(uView, true, ref mView);

            int uEyePosition = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            Vector4 eyePosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
            GL.Uniform4(uEyePosition, eyePosition);


            #region Loading in the lights and binding shader light variables

            float AmbientIntensity = 0.8f;
            float DiffuseIntensity = 0.7f;
            float SpecularIntensity = 0.001f;

            #region Red Light 1
            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].Position");
            Vector4 lightPosition = new Vector4(1, 4, -8.5f, 1);
            lightPosition = Vector4.Transform(lightPosition, mView);
            GL.Uniform4(uLightPositionLocation, lightPosition);

            int uAmbientLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].AmbientLight");
            Vector3 AmbientColour = new Vector3(AmbientIntensity, 0.0f, 0.0f);
            GL.Uniform3(uAmbientLightLocation, AmbientColour);

            int uDiffuseLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].DiffuseLight");
            Vector3 DiffuseColour = new Vector3(DiffuseIntensity, 0.0f, 0.0f);
            GL.Uniform3(uDiffuseLightLocation, DiffuseColour);

            int uSpecularLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].SpecularLight");
            Vector3 SpecularColour = new Vector3(SpecularIntensity, 0.0f, 0.0f);
            GL.Uniform3(uSpecularLightLocation, SpecularColour);
            #endregion

            #region Green Light 2
            int uLightPositionLocation1 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].Position");
            Vector4 lightPosition1 = new Vector4(5, 4, -8.5f, 1);
            lightPosition1 = Vector4.Transform(lightPosition1, mView);
            GL.Uniform4(uLightPositionLocation1, lightPosition1);

            int uAmbientLightLocation1 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].AmbientLight");
            Vector3 AmbientColour1 = new Vector3(0.0f, AmbientIntensity, 0.0f);
            GL.Uniform3(uAmbientLightLocation1, AmbientColour1);

            int uDiffuseLightLocation1 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].DiffuseLight");
            Vector3 DiffuseColour1 = new Vector3(0.0f, DiffuseIntensity, 0.0f);
            GL.Uniform3(uDiffuseLightLocation1, DiffuseColour1);

            int uSpecularLightLocation1 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].SpecularLight");
            Vector3 SpecularColour1 = new Vector3(0.0f, SpecularIntensity, 0.0f);
            GL.Uniform3(uSpecularLightLocation1, SpecularColour1);
            #endregion

            #region blue Light 3
            int uLightPositionLocation2 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].Position");
            Vector4 lightPosition2 = new Vector4(10, 4, -8.5f, 1);
            lightPosition2 = Vector4.Transform(lightPosition2, mView);
            GL.Uniform4(uLightPositionLocation2, lightPosition2);

            int uAmbientLightLocation2 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].AmbientLight");
            Vector3 AmbientColour2 = new Vector3(0.0f, 0.0f, AmbientIntensity);
            GL.Uniform3(uAmbientLightLocation2, AmbientColour2);

            int uDiffuseLightLocation2 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].DiffuseLight");
            Vector3 DiffuseColour2 = new Vector3(0.0f, 0.0f, DiffuseIntensity);
            GL.Uniform3(uDiffuseLightLocation2, DiffuseColour2);

            int uSpecularLightLocation2 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].SpecularLight");
            Vector3 SpecularColour2 = new Vector3(0.0f, 0.0f, SpecularIntensity);
            GL.Uniform3(uSpecularLightLocation2, SpecularColour2);
            #endregion

            #endregion


            #endregion


            // Generating Vertex Array Objects and Vertex Buffer Objects
            GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);
            int size;

            #region Loading in models 

            #region ground model

            floor = new CubeFace(mShader, mVAO_IDs, mVBO_IDs);
            floor.load(vPositionLocation, vNormal);
            floor.setPosition(Matrix4.CreateTranslation(0.0f, 0.0f, -5.0f));

            #endregion

            #region Sphere model

            sphere1 = new Sphere(mShader, mVAO_IDs, mVBO_IDs);
            sphere1.load(vPositionLocation, vNormal);
            sphere1.setPosition(Matrix4.CreateTranslation(0.0f, 5.0f, -15.0f));
            sphere1.setVelocity(Matrix4.CreateTranslation(0.0f, 8.0f, 0.0f));

            #endregion

            #region cube model

            mCubeModelUtility = ModelUtility.LoadModel(@"Utility/Models/cube.sjg");

            GL.BindVertexArray(mVAO_IDs[2]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[3]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mCubeModelUtility.Vertices.Length * sizeof(float)), mCubeModelUtility.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[4]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mCubeModelUtility.Indices.Length * sizeof(float)), mCubeModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCubeModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCubeModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            #endregion

            #endregion


            GL.BindVertexArray(0);

            mTimer = new Timer();
            mTimer.Start();

            base.OnLoad(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            float cameraSpeed = 1.0f;

            if (e.KeyChar == 'w')
            {
                // Camera movement
                mView = mView * Matrix4.CreateTranslation(0.0f, -cameraSpeed, 0.0f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);
            }
            if (e.KeyChar == 'a')
            {
                // Camera movement
                mView = mView * Matrix4.CreateTranslation(cameraSpeed, 0.0f, 0.0f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);
            }
            if (e.KeyChar == 's')
            {
                // Camera movement
                mView = mView * Matrix4.CreateTranslation(0.0f, cameraSpeed, 0.0f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);
            }
            if (e.KeyChar == 'd')
            {
                // Camera movement
                mView = mView * Matrix4.CreateTranslation(-cameraSpeed, 0.0f, 0.0f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);
            }
            if (e.KeyChar == 'q')
            {
            }
            if (e.KeyChar == 'e')
            {
            }
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

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            float timestep = mTimer.GetElapsedSeconds();

            sphere1.update(timestep, accelerationDueToGravity);

            #region Sphere collision with ground

            //// RIGHT
            //if (mSpherePosition.ExtractTranslation().X + (mSphereRadius / mGroundPosition.ExtractScale().X) > 1) // 
            //{
            //    // COLLISION RESPONSE
            //}
            //// LEFT
            //if (mSpherePosition.ExtractTranslation().X - (mSphereRadius / mGroundPosition.ExtractScale().X) < -1) // 
            //{
            //    // COLLISION RESPONSE
            //}
            //// TOP
            //if (mSpherePosition.ExtractTranslation().Y + (mSphereRadius / mGroundPosition.ExtractScale().X) > 1) // 
            //{
            //    // COLLISION RESPONSE
            //}
            //// BOTTOM
            //if (mSpherePosition.ExtractTranslation().Y - (mSphereRadius / mGroundPosition.ExtractScale().X) < -1) // 
            //{
            //    // COLLISION RESPONSE
            //}

            #endregion




            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //GL.ClearColor(Color4.HotPink);

            int uModel;

            #region Rendering models

            floor.Render();

            sphere1.render();

            #region Cube

            Matrix4 mCubePosition = Matrix4.CreateScale(5f) * Matrix4.CreateTranslation(0, 5, -15f);
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref mCubePosition);

            GL.BindVertexArray(mVAO_IDs[2]);
            GL.DrawElements(PrimitiveType.Triangles, mCubeModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            #endregion

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

    public class Timer
    {
        DateTime mLastTime;

        public Timer()
        { }

        public void Start()
        {
            mLastTime = DateTime.Now;
        }

        public float GetElapsedSeconds()
        {
            DateTime now = DateTime.Now;
            TimeSpan elasped = now - mLastTime;
            mLastTime = now;
            return (float)elasped.Ticks / TimeSpan.TicksPerSecond;
        }
    }

    public class CubeFace
    {
        Matrix4 Position = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);
        float[] vertices = new float[] {-10, 0, -10,0,1,0,
                                             -10, 0, 10,0,1,0,
                                             10, 0, 10,0,1,0,
                                             10, 0, -10,0,1,0,};
        ShaderUtility mShader;
        int[] vertexArrayObject;
        int[] vertexBufferObject;

        public CubeFace(ShaderUtility pShader, int[] pVertexArrayObject, int[] pVertexBufferObject)
        {
            mShader = pShader;
            vertexArrayObject = pVertexArrayObject;
            vertexBufferObject = pVertexBufferObject;
        }

        public void setPosition(Matrix4 pPosition)
        {
            Position = pPosition;
        }

        public void load(int pPositionLocation, int pNormal)
        {
            int size;

            GL.BindVertexArray(vertexArrayObject[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(pPositionLocation);
            GL.VertexAttribPointer(pPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(pNormal); // enable it
            GL.VertexAttribPointer(pNormal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float)); //added
        }

        public void Render()
        {
            // Link ground matrix to the shader
            int uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref Position);

            // White rubber
            setMaterialProperties(0.05f, 0.05f, 0.05f, 0.5f, 0.5f, 0.5f, 0.7f, 0.7f, 0.7f, 0.078125f);

            // Bind the ground array and draw it
            GL.BindVertexArray(vertexArrayObject[0]);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
        }


        private void setMaterialProperties(float AmbientR, float AmbientG, float AmbientB,
    float DiffuseR, float DiffuseG, float DiffuseB,
    float SpecularR, float SpecularG, float SpecularB,
    float Shininess)
        {
            int uAmbientReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.AmbientReflectivity");
            Vector3 AmbientReflectivity = new Vector3(AmbientR, AmbientG, AmbientB);
            GL.Uniform3(uAmbientReflectivityLocation, AmbientReflectivity);

            int uDiffuseReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.DiffuseReflectivity");
            Vector3 DiffuseReflectivity = new Vector3(DiffuseR, DiffuseG, DiffuseB);
            GL.Uniform3(uDiffuseReflectivityLocation, DiffuseReflectivity);

            int uSpecularReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.SpecularReflectivity");
            Vector3 SpecularReflectivity = new Vector3(SpecularR, SpecularG, SpecularB);
            GL.Uniform3(uSpecularReflectivityLocation, SpecularReflectivity);

            int uShininessLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.Shininess");
            GL.Uniform1(uShininessLocation, Shininess);
        }

    }

    public class Sphere
    {
        private ModelUtility mSphereModelUtility;
        private ShaderUtility mShader;
        int[] vertexArrayObject;
        int[] vertexBufferObject;

        public Matrix4 mPosition = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);
        public Matrix4 mVelocity = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);
        public Matrix4 mScale = Matrix4.CreateScale(1.0f);

        public float mSphereRadius, mSphereVolume, mSphereMass, mSphereDensity;   
    
        public void setPosition(Matrix4 pPosition)
        {
            mPosition = pPosition;
        }

        public void setVelocity(Matrix4 pVelocity)
        {
            mVelocity = pVelocity;
        }

        public Sphere(ShaderUtility pShader, int[] pVertexArrayObject, int[] pVertexBufferObject)
        {
            mShader = pShader;
            vertexArrayObject = pVertexArrayObject;
            vertexBufferObject = pVertexBufferObject;

            // SPHERE PROPERTIES
            mSphereRadius = 4f; // TODO check if this value
            mSphereVolume = (4 / 3) * (float)Math.PI * (float)Math.Pow(mSphereRadius, 3);
            mSphereDensity = 1f;
            mSphereMass = mSphereDensity * mSphereVolume;
        }
        public void load(int pPositionLocation, int pNormal)
        {
            int size;
            mSphereModelUtility = ModelUtility.LoadModel(@"Utility/Models/sphere.bin");

            GL.BindVertexArray(vertexArrayObject[1]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject[1]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mSphereModelUtility.Vertices.Length * sizeof(float)), mSphereModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vertexBufferObject[2]);
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

            GL.EnableVertexAttribArray(pPositionLocation);
            GL.VertexAttribPointer(pPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(pNormal);
            GL.VertexAttribPointer(pNormal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));
        }
        public void render()
        {
            int uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref mPosition);

            // cyan
            //setMaterialProperties(0.0f, 0.05f, 0.05f, 0.4f, 0.5f, 0.5f, 0.04f, 0.7f, 0.7f, 0.078125f);

            GL.BindVertexArray(vertexArrayObject[1]);
            GL.DrawElements(PrimitiveType.Triangles, mSphereModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        public void update(float timestep, Matrix4 gravity)
        {
            mVelocity = mVelocity + gravity * timestep;
            mPosition = mPosition + mVelocity * timestep;
        }
    }
}
