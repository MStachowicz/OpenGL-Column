using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Labs.Utility;
using System;
using System.Collections.Generic;

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

        protected ShaderUtility mShader;
        private Matrix4 mView;

        protected static int VAOCount = 0;
        protected static int VBOCount = 0;
        protected static int[] mVAO_IDs = new int[6];
        protected static int[] mVBO_IDs = new int[12];

        protected static int vPositionLocation;
        protected static int vNormal;

        public Vector3 accelerationDueToGravity = new Vector3(0.0f, -9.81f, 0.0f);
        float restitution = 1f;

        private Timer mTimer;

        // OBJECTS
        entityManager Manager;
        Sphere sphereTest;
        Cube cube1;
        Cube cube2;
        Cube cube3;
        Cylinder cylinder1;

        Vector4 lightPosition;
        protected override void OnLoad(EventArgs e)
        {
            // Set some GL state
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);

            #region Loading in shaders and shader variables

            mShader = new ShaderUtility(@"ACW/Shaders/vertexShader.vert", @"ACW/Shaders/fragmentShader.frag");
            GL.UseProgram(mShader.ShaderProgramID);

            vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            vNormal = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal"); //find the index for the location of vNormal in the shader

            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            mView = Matrix4.CreateTranslation(0.0f, 0.0f,0.0f);
            GL.UniformMatrix4(uView, true, ref mView);

            int uEyePosition = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            Vector4 eyePosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
            GL.Uniform4(uEyePosition, eyePosition);

            #endregion

            #region Loading in the lights and binding shader light variables

            float AmbientIntensity = 0.9f;
            float DiffuseIntensity = 0.7f;
            float SpecularIntensity = 0.001f;

            #region Red Light 1
            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].Position");
            lightPosition = new Vector4(0.5f, 2.0f, -5.0f, 1.0f);
            //lightPosition = Vector4.Transform(lightPosition, mView);
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
            Vector4 lightPosition1 = new Vector4(0.5f, 1.0f, -5.0f, 1.0f);
            //lightPosition1 = Vector4.Transform(lightPosition1, mView);
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
            Vector4 lightPosition2 = new Vector4(0.5f, 0.0f, -5.0f, 1.0f);
            //lightPosition2 = Vector4.Transform(lightPosition2, mView);
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

            // Generating Vertex Array Objects and Vertex Buffer Objects
            GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);

            #region Loading in models

            Manager = new entityManager(mShader, mVAO_IDs, mVBO_IDs);

            // CUBES
            cube1 = new Cube();
            cube2 = new Cube();
            cube3 = new Cube();

            Manager.ManageEntity(cube1);
            Manager.ManageEntity(cube2);
            Manager.ManageEntity(cube3);

            // 100 cm = 1.0f
            cube1.mPosition = new Vector3(0.0f, 2.0f, -5.0f);
            cube2.mPosition = new Vector3(0.0f, 1.0f, -5.0f);
            cube3.mPosition = new Vector3(0.0f, 0.0f, -5.0f);

            // SPHERES
            sphereTest = new Sphere();

            Manager.ManageEntity(sphereTest);

            // sphereTest.mPosition = new Vector3(2.0f, 1.0f, -5.0f);
            sphereTest.mPosition = cube1.mPosition;
            //sphereTest.mVelocity = new Vector3(0.0f, -9.81f, 0.0f);
            sphereTest.mScale = 1f;
            // CYLINDERS
            cylinder1 = new Cylinder();

            Manager.ManageEntity(cylinder1);

            cylinder1.mPosition = new Vector3(-2.0f, 1.0f, -5.0f);
            cylinder1.mScale = 1f;

            #endregion

            Manager.loadObjects();

            GL.BindVertexArray(0);

            mTimer = new Timer();
            mTimer.Start();

            base.OnLoad(e);
        }

        public void moveCamera(Vector3 Translation)
        {
            // Camera movement
            mView = mView * Matrix4.CreateTranslation(Translation);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);


            // LIGHT 1
            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].Position");
            lightPosition = new Vector4(0.5f, 2.0f, -5.0f, 1.0f);
            lightPosition = Vector4.Transform(lightPosition, mView);
            GL.Uniform4(uLightPositionLocation, lightPosition);

            // LIGHT 2
            int uLightPositionLocation1 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].Position");
            Vector4 lightPosition1 = new Vector4(0.5f, 1.0f, -5.0f, 1.0f);
            lightPosition1 = Vector4.Transform(lightPosition1, mView);
            GL.Uniform4(uLightPositionLocation1, lightPosition1);

            // LIGHT 3
            int uLightPositionLocation2 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].Position");
            Vector4 lightPosition2 = new Vector4(0.5f, 0.0f, -5.0f, 1.0f);
            lightPosition2 = Vector4.Transform(lightPosition2, mView);
            GL.Uniform4(uLightPositionLocation2, lightPosition2);

            //int uEyePosition = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            //Vector4 eyePosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
            //GL.Uniform4(uEyePosition, eyePosition);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            float cameraSpeed = 0.2f;

            switch (e.KeyChar)
            {
                case '1':
                    cylinder1.mPosition.X += 1;
                    break;
                case '2':
                    cylinder1.mPosition.Y += 1;
                    break;
                case '3':
                    cylinder1.mPosition.Z += 1;
                    break;
                case 'w':
                    moveCamera(new Vector3(0.0f, -cameraSpeed, 0.0f));
                    break;
                case 'a':
                    moveCamera(new Vector3(cameraSpeed, 0.0f, 0.0f));
                    break;
                case 's':
                    moveCamera(new Vector3(0.0f, cameraSpeed, 0.0f));
                    break;
                case 'd':
                    moveCamera(new Vector3(-cameraSpeed, 0.0f, 0.0f));
                    break;
                case 'q':
                    moveCamera(new Vector3(0.0f, 0.0f, -cameraSpeed));
                    break;
                case 'e':
                    moveCamera(new Vector3(0.0f, 0.0f, cameraSpeed));
                    break;
                case 'g':
                    lightPosition.Y += 1;
                    break;

                default:
                    break;
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

            //sphereTest.Update(timestep, accelerationDueToGravity);
            sphereTest.hasCollidedWithCube(cube1);

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


            #region Rendering models

            // White rubber
            setMaterialProperties(0.05f, 0.05f, 0.05f, 0.5f, 0.5f, 0.5f, 0.7f, 0.7f, 0.7f, 0.078125f);

            Manager.renderObjects();

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


    public class entityManager
    {
        public entityManager(ShaderUtility pShader, int[] pVertexArrayObject, int[] pVertexBufferObject)
        {
            vertexArrayObject = pVertexArrayObject;
            vertexBufferObject = pVertexBufferObject;

            shader = pShader;
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
            //foreach (entity i in mObjects)
            //{
            //    i.Load();
            //}



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
            //foreach (entity i in mObjects)
            //{
            //    i.Render();
            //}

            for (int i = 0; i < mObjects.Count; i++)
            {
                //GL.FrontFace(FrontFaceDirection.Cw);

                if (i == 3) // first 3 objects are cubes to be reversed
                {
                    // reset back after the cubes are rendered
                   // GL.FrontFace(FrontFaceDirection.Ccw);
                }


                mObjects[i].Render();
            }

        }

        /// <summary>
        /// All the objects this entity manager is responsible for.
        /// </summary>
        public List<entity> mObjects = new List<entity>();

        // Setting up VAO and VBO for use with ACW window
        public static int[] vertexArrayObject;
        public static int[] vertexBufferObject;
        public static ShaderUtility shader;

        public static int vPositionLocation;
        public static int vNormal;

        public static int VAOCount = 0;
        public static int VBOCount = 0;
    }

    public abstract class entity
    {
        /// <summary>
        /// The index in the VAO that this entity belongs to.
        /// </summary>
        protected int VAOIndex;
        /// <summary>
        /// The index in the VBO that this entity belongs to.
        /// </summary>
        protected int VBOIndex;

        protected ModelUtility mModelUtility;

        // OBJECT PROPERTIES      
        public float mScale;
        public float mVolume;
        public float mDensity;
        public float mMass;

        public Matrix4 mMatrix;
        public Vector3 mPosition;
        public Vector3 mVelocity;

        abstract public void Load();
        abstract public void Render();
        abstract public void Update(float pTimestep, Vector3 pGravity);
    }

    public class Sphere : entity
    {
        public Sphere()
        {
            VAOIndex = entityManager.VAOCount++;
            VBOIndex = entityManager.VBOCount++;
            entityManager.VBOCount++;

            // SPHERE PROPERTIES
            mRadius = 4f;
            mScale = 1.0f;
            mVolume = (4 / 3) * (float)Math.PI * (float)Math.Pow(mRadius, 3);
            mDensity = 1f;
            mMass = mDensity * mVolume;

            mPosition = new Vector3(0.0f, 0.0f, 0.0f);
            mVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        }

        // Sphere unique members
        public float mRadius;


        public override void Load()
        {
            int size;
            mModelUtility = ModelUtility.LoadModel(@"Utility/Models/sphere.bin");

            GL.BindVertexArray(entityManager.vertexArrayObject[VAOIndex]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, entityManager.vertexBufferObject[VBOIndex]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mModelUtility.Vertices.Length * sizeof(float)), mModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, entityManager.vertexBufferObject[VBOIndex + 1]);

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

            GL.EnableVertexAttribArray(entityManager.vPositionLocation);
            GL.VertexAttribPointer(entityManager.vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(entityManager.vNormal);
            GL.VertexAttribPointer(entityManager.vNormal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 2 * sizeof(float));
        }

        public override void Render()
        {
            mMatrix = Matrix4.CreateScale(mScale) * Matrix4.CreateTranslation(mPosition);
            int uModel = GL.GetUniformLocation(entityManager.shader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref mMatrix);

            GL.BindVertexArray(entityManager.vertexArrayObject[VAOIndex]);
            GL.DrawElements(PrimitiveType.Triangles, mModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        public override void Update(float dt, Vector3 gravity)
        {
            mVelocity = mVelocity + gravity * dt;
            mPosition = mPosition + mVelocity * dt;
        }

        public void hasCollidedWithCube(Cube pCube)
        {
            // RIGHT
            if (mPosition.X + mRadius > 0.5) 
            {
                // COLLISION RESPONSE
            }
            // LEFT
            //if (mPosition.X - (mRadius / pCube.mPosition.X) < -0.5) 
            //{
            //    // COLLISION RESPONSE
            //}
            //// TOP
            //if (mPosition.Y + (mRadius / pCube.mPosition.X) > 0.5)
            //{
            //    // COLLISION RESPONSE
            //}
            //// BOTTOM
            //if (mPosition.Y - (mRadius / pCube.mPosition.X) < -0.5) 
            //{
            //    // COLLISION RESPONSE
            //}
        }
    }

    public class Cube : entity
    {
        public Cube()
        {
            VAOIndex = entityManager.VAOCount++;
            VBOIndex = entityManager.VBOCount++;
            entityManager.VBOCount++; // cube uses two VBOS

            // CUBE PROPERTIES
            mScale = 1.0f;
            mVolume = 0.0f; // TODO ADD VOLUME FOR CUBE
            mDensity = 1f;
            mMass = mDensity * mVolume;

            mPosition = new Vector3(0.0f, 0.0f, 0.0f);
            mVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        }

        public override void Load()
        {
            mModelUtility = ModelUtility.LoadModel(@"Utility/Models/cube.sjg");
            int size;

            GL.BindVertexArray(entityManager.vertexArrayObject[VAOIndex]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, entityManager.vertexBufferObject[VBOIndex]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mModelUtility.Vertices.Length * sizeof(float)), mModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, entityManager.vertexBufferObject[VBOIndex + 1]);
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

            GL.EnableVertexAttribArray(entityManager.vPositionLocation);
            GL.VertexAttribPointer(entityManager.vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        }

        public override void Render()
        {
            mMatrix = Matrix4.CreateScale(mScale) * Matrix4.CreateTranslation(mPosition);
            int uModel = GL.GetUniformLocation(entityManager.shader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref mMatrix);

            GL.BindVertexArray(entityManager.vertexArrayObject[VAOIndex]);
            GL.DrawElements(PrimitiveType.Triangles, mModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        public override void Update(float pTimestep, Vector3 pGravity)
        {
            throw new NotImplementedException();
        }
    }

    public class Cylinder : entity
    {
        public Cylinder()
        {
            VAOIndex = entityManager.VAOCount++;
            VBOIndex = entityManager.VBOCount++;
            entityManager.VBOCount++;

            // CYLINDER PROPERTIES TODO: correct these values (currently same as sphere)
            mRadius = 4f;
            mScale = 1.0f;
            mVolume = (4 / 3) * (float)Math.PI * (float)Math.Pow(mRadius, 3);
            mDensity = 1f;
            mMass = mDensity * mVolume;

            mPosition = new Vector3(0.0f, 0.0f, 0.0f);
            mVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        }

        float mRadius;

        public override void Load()
        {
            mModelUtility = ModelUtility.LoadModel(@"Utility/Models/cylinder.bin");
            int size;

            GL.BindVertexArray(entityManager.vertexArrayObject[VAOIndex]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, entityManager.vertexBufferObject[VBOIndex]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mModelUtility.Vertices.Length * sizeof(float)), mModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, entityManager.vertexBufferObject[VBOIndex + 1]);
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

            GL.EnableVertexAttribArray(entityManager.vPositionLocation);
            GL.VertexAttribPointer(entityManager.vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            // next 2 lines cause cylinder to dissapear
            //GL.EnableVertexAttribArray(entityManager.vNormal);
            //GL.VertexAttribPointer(entityManager.vNormal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));
        }

        public override void Render()
        {
            mMatrix = Matrix4.CreateScale(mScale) * Matrix4.CreateTranslation(mPosition);
            int uModel = GL.GetUniformLocation(entityManager.shader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref mMatrix);

            GL.BindVertexArray(entityManager.vertexArrayObject[VAOIndex]);
            GL.DrawElements(PrimitiveType.Triangles, mModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        public override void Update(float pTimestep, Vector3 pGravity)
        {
            throw new NotImplementedException();
        }
    }
}