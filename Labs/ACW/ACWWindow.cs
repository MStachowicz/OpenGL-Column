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
        private ModelUtility mSphereModelUtility;

        private Matrix4 mView, mSphereModel, mSphereScale, mGroundModel;

        private int[] mVAO_IDs = new int[2];
        private int[] mVBO_IDs = new int[3];

        Vector3 accelerationDueToGravity = new Vector3(0, 0f, 0);
        float restitution = 1f;

        private float mSphereRadius, mSphereVolume, mSphereMass, mSphereDensity;
        private Vector3 mSpherePosition, mSphereVelocity;


        private Timer mTimer;


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

            mView = Matrix4.CreateTranslation(0, -1.5f, 0);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
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

            float[] vertices = new float[] {-10, 0, -10,0,1,0,
                                             -10, 0, 10,0,1,0,
                                             10, 0, 10,0,1,0,
                                             10, 0, -10,0,1,0,};

            GL.BindVertexArray(mVAO_IDs[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(vNormal); // enable it
            GL.VertexAttribPointer(vNormal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float)); //added

            mGroundModel = Matrix4.CreateTranslation(0, 0, -5f);           

            #endregion

            #region Sphere model
            mSphereModelUtility = ModelUtility.LoadModel(@"Utility/Models/sphere.bin");

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

            GL.EnableVertexAttribArray(vNormal);
            GL.VertexAttribPointer(vNormal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            // SPHERE PROPERTIES
            mSphereScale = Matrix4.CreateScale(2.0f);
            mSphereModel = Matrix4.CreateTranslation(0, 1, -5f);
            
            mSphereRadius = 1f; // TODO check if this value
            mSpherePosition = mSphereModel.ExtractTranslation(); //new Vector3(-2.0f, 2.0f, 0.0f);
            mSphereVelocity = new Vector3(2.0f, 0.0f, 0.0f);
            mSphereVolume = (4 / 3) * (float)Math.PI * (float)Math.Pow(mSphereRadius, 3);
            mSphereDensity = 1f;
            mSphereMass = mSphereDensity * mSphereVolume;


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






            base.OnUpdateFrame(e);
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

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //GL.ClearColor(Color4.HotPink);

            int uModel;

            #region Rendering models

            #region Ground
            // Link ground matrix to the shader
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref mGroundModel);

            // White rubber
            setMaterialProperties(0.05f, 0.05f, 0.05f, 0.5f, 0.5f, 0.5f, 0.7f, 0.7f, 0.7f, 0.078125f);

            // Bind the ground array and draw it
            GL.BindVertexArray(mVAO_IDs[0]);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
            #endregion

            #region Sphere
            // Link the sphere matrix to the shader
            Matrix4 m = mSphereModel * mGroundModel; 
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m);

            // cyan
            //setMaterialProperties(0.0f, 0.05f, 0.05f, 0.4f, 0.5f, 0.5f, 0.04f, 0.7f, 0.7f, 0.078125f);

            GL.BindVertexArray(mVAO_IDs[1]);
            GL.DrawElements(PrimitiveType.Triangles, mSphereModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
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
}
