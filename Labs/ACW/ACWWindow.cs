﻿using OpenTK;
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
        { }

        public static ShaderUtility mShader;
        public Matrix4 mView;

        const int NumberOfSpheres = 5;
        const int NumberOfCylinders = 1;
        const int NumberOfCubes = 1;
        const int totalObjects = NumberOfSpheres + NumberOfCylinders + NumberOfCubes;

        public static int CollisionCount = 0;
        public static int SpheresWaitingCount = 0;

        public static int[] mVAO_IDs = new int[totalObjects + 2];
        public static int[] mVBO_IDs = new int[totalObjects * 2 + 4]; // each object has 2 vbo index'

        public static int vPositionLocation;
        public static int vNormal;

        public bool pauseTime = false;
        public Vector3 accelerationDueToGravity = new Vector3(0.0f, 0.0f, 0.0f);
        //public Vector3 accelerationDueToGravity = new Vector3(0.0f, -9.81f, 0.0f);
        float restitution = 1.0f;
        bool releaseSpheres = false;

        private bool ballCam = false;
        private int ballBeingFollowedIndex = 0;

        private Timer mTimer;
        public static Random rand;

        // OBJECTS
        entityManager Manager;

        Sphere[] sphereArray;
        Sphere sphere1;
        Sphere sphere2;


        Cube cube1;
        Cylinder cylinder1;

        public void pauseSimulation()
        {
            pauseTime ^= true;
        }
        public void moveCamera(Vector3 Translation)
        {
            // Camera movement
            mView = mView * Matrix4.CreateTranslation(Translation);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);


            Vector4 lightPosition = new Vector4(cube1.mPosition.X, cube1.mPosition.Y + (cube1.cubeDimensions.Y / 2), cube1.mPosition.Z, 1.0f);
            Vector4 lightPosition1 = new Vector4(cube1.mPosition, 1.0f);
            Vector4 lightPosition2 = new Vector4(cube1.mPosition.X, cube1.mPosition.Y - (cube1.cubeDimensions.Y) / 2, cube1.mPosition.Z, 1.0f);


            // LIGHT 1 - TOP
            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].Position");
            lightPosition = Vector4.Transform(lightPosition, mView);
            GL.Uniform4(uLightPositionLocation, lightPosition);

            // LIGHT 2 - CENTER
            int uLightPositionLocation1 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].Position");
            lightPosition1 = Vector4.Transform(lightPosition1, mView);
            GL.Uniform4(uLightPositionLocation1, lightPosition1);

            // LIGHT 3 - BOTTOM 
            int uLightPositionLocation2 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].Position");
            lightPosition2 = Vector4.Transform(lightPosition2, mView);
            GL.Uniform4(uLightPositionLocation2, lightPosition2);

            int uEyePosition = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            Vector4 eyePosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
            GL.Uniform4(uEyePosition, eyePosition);
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
            GL.Uniform1(uShininessLocation, Shininess * 128);
        }

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
            mView = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);
            GL.UniformMatrix4(uView, true, ref mView);

            int uEyePosition = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            Vector4 eyePosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
            GL.Uniform4(uEyePosition, eyePosition);


            #endregion



            // Generating Vertex Array Objects and Vertex Buffer Objects
            GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);

            #region Loading in models

            Manager = new entityManager();
            rand = new Random();

            // 100 cm = 1.0f



            // CUBES
            cube1 = new Cube();
            Manager.ManageEntity(cube1);

            cube1.mPosition = new Vector3(0.0f, 0.0f, -5.0f);


            // SPHERES
            sphereArray = new Sphere[NumberOfSpheres]; // create spheres array
            for (int i = 0; i < sphereArray.Length; i++)
            {
                sphereArray[i] = new Sphere(cube1);
            }


            // Manager will manage all these spheres
            foreach (var i in sphereArray)
                Manager.ManageEntity(i);


            sphere1 = new Sphere(cube1);
            Manager.ManageEntity(sphere1);

            //sphere1.mPosition = new Vector3(cube1.mPosition.X + 0.4f, cube1.mPosition.Y, cube1.mPosition.Z);
            sphere1.mVelocity = new Vector3(-1.0f, 1.0f, 0.0f);

            //sphere2 = new Sphere(cube1);
            //Manager.ManageEntity(sphere2);

            //sphere2.mPosition = new Vector3(cube1.mPosition.X - 0.4f, cube1.mPosition.Y, cube1.mPosition.Z);
            //sphere2.mVelocity = new Vector3(2.0f, 0.0f, 0.0f);

            // CYLINDERS
            cylinder1 = new Cylinder();

            Manager.ManageEntity(cylinder1);

            cylinder1.mPosition = new Vector3(0.0f, 1.0f, -5.0f);
            cylinder1.mScaleX = 0.1f;
            cylinder1.mScaleY = 0.5f;
            cylinder1.mScaleZ = 0.1f;
            cylinder1.mRotationX = 2.0f;
            //cylinder1.SetScale(0.1f);

            #endregion

            Manager.loadObjects();

            #region Loading in the lights and binding shader light variables

            float AmbientIntensity = 0.8f;
            float DiffuseIntensity = 0.8f;
            float SpecularIntensity = 0.1f;

            Vector4 lightPosition = new Vector4(cube1.mPosition.X, cube1.mPosition.Y + (cube1.cubeDimensions.Y / 2), cube1.mPosition.Z, 1.0f);
            Vector4 lightPosition1 = new Vector4(cube1.mPosition, 1.0f);
            Vector4 lightPosition2 = new Vector4(cube1.mPosition.X, cube1.mPosition.Y - (cube1.cubeDimensions.Y / 2), cube1.mPosition.Z, 1.0f);

            #region Red Light 1
            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].Position");
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

            //Vector4 lightPosition = new Vector4(0.0f, -1.0f, -5.0f, 1.0f);
            //Vector4 lightPosition2 = new Vector4(0.0f, 1, -5.0f, 1.0f);

            #endregion


            GL.BindVertexArray(0);

            mTimer = new Timer();
            mTimer.Start();

            base.OnLoad(e);
        }

        /// <summary>
        /// Generates a random floating point number between the minimum and maximum.
        /// </summary>
        /// <param name="rng"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private float NextFloat(Random rng, float min, float max)
        {
            return (float)(min + (rng.NextDouble() * (max - min)));
        }


        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            float cameraSpeed = 0.1f;

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
                case 'p':
                    pauseSimulation();
                    break;
                case 'n':
                    releaseSpheres = true;
                    break;
                case 'b':
                    // begins camera follow of a random ball
                    ballCam = true;
                    ballBeingFollowedIndex = rand.Next(0, sphereArray.Length);
                    Console.WriteLine("following ball index " + ballBeingFollowedIndex);
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

            if (ballCam)
            {
                mView = Matrix4.CreateTranslation(-sphereArray[ballBeingFollowedIndex].mPosition.X, -sphereArray[0].mPosition.Y, 0.0f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);
            }

            if (!pauseTime)
            {
                for (int i = 0; i < sphereArray.Length; i++)
                {
                    // Update sphere by its velocity and accelaration
                    sphereArray[i].Update(timestep, accelerationDueToGravity);

                    if (!releaseSpheres) // allows switching off collisions with bounding cube             
                        sphereArray[i].hasCollidedWithCube(cube1);

                    foreach (var j in sphereArray)
                    {
                        if (!sphereArray[i].Equals(j)) // If this is not the same cube
                            sphereArray[i].hasCollidedWithSphere(j); // check for collisions with all other spheres
                    }

                    sphereArray[i].hasCollidedWithSphere(sphere1);
                    //sphereArray[i].hasCollidedWithSphere(sphere2);

                }

                sphere1.Update(timestep, accelerationDueToGravity);
                sphere1.hasCollidedWithCube(cube1);
                //sphere1.hasCollidedWithSphere(sphere2);

                //sphere2.Update(timestep, accelerationDueToGravity);
                //sphere2.hasCollidedWithCube(cube1);
                //sphere2.hasCollidedWithSphere(sphere1);


                for (int j = 0; j < sphereArray.Length; j++)
                {
                    sphere1.hasCollidedWithSphere(sphereArray[j]);
                    //sphere2.hasCollidedWithSphere(sphereArray[j]);
                }
            }
            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            #region Rendering models

            // White rubber
            //setMaterialProperties(0.05f, 0.05f, 0.05f, 0.5f, 0.5f, 0.5f, 0.7f, 0.7f, 0.7f, 0.078125f);


            // chrome
            setMaterialProperties(0.25f, 0.25f, 0.25f, 0.4f, 0.4f, 0.4f, 0.774597f, 0.774597f, 0.774597f, 0.6f);

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

    public class lightManager
    {
        public List<light> mLights = new List<light>();

        public void LoadLights()
        {
            for (int i = 0; i < mLights.Count; i++)
            {
                //i.load();
            }
        }
    }

    public class light
    {
        public void load()
        {

        }
    }

    public class entityManager
    {
        public entityManager()
        {
            //vertexArrayObject = pVertexArrayObject;
            //vertexBufferObject = pVertexBufferObject;

            //shader = pShader;
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

                if (i == 1) // first 3 objects are cubes to be reversed
                {
                    // reset back after the cubes are rendered
                    //GL.FrontFace(FrontFaceDirection.Ccw);
                    //GL.Enable(EnableCap.CullFace);
                    GL.Disable(EnableCap.CullFace);
                }
                else
                {
                    GL.Enable(EnableCap.CullFace);
                }


                mObjects[i].Render();
            }

        }

        /// <summary>
        /// All the objects this entity manager is responsible for.
        /// </summary>
        public List<entity> mObjects = new List<entity>();

        // Setting up VAO and VBO for use with ACW window
        //public static int[] vertexArrayObject;
        //public static int[] vertexBufferObject;
        //public static ShaderUtility shader;

        //public static int vPositionLocation;
        //public static int vNormal;

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
        public float mScaleX;
        public float mScaleY;
        public float mScaleZ;

        public float mRotationX;
        public float mRotationY;
        public float mRotationZ;

        public float mVolume;
        public float mDensity;
        public float mMass;

        public Matrix4 mMatrix;
        public Vector3 mPosition;
        public Vector3 mVelocity;

        abstract public void Load();
        abstract public void Render();
        abstract public void Update(float pTimestep, Vector3 pGravity);

        /// <summary>
        /// Sets the scale of the entity in the x, y and z plane to the parameter value.
        /// </summary>
        /// <param name="pScale">The value the scale will be set to.</param>
        public void SetScale(float pScale)
        {
            mScaleX = pScale;
            mScaleY = pScale;
            mScaleZ = pScale;
        }
    }

    public class Sphere : entity
    {
        /// <summary>
        /// Creates a ball in the top cube of the scene in a random position with a random velocity.
        /// </summary>
        /// <param name="pCube">The cube all the spheres will be bounded by</param>
        public Sphere(Cube pCube)
        {
            VAOIndex = entityManager.VAOCount++;
            VBOIndex = entityManager.VBOCount++;
            entityManager.VBOCount++;

            // SPHERE PROPERTIES

            //float y = NextFloat(rand, cube1.mPosition.Y + (cube1.cubeDimensions.Y),
            //cube1.mPosition.Y - (cube1.cubeDimensions.Y));

            float x = NextFloat(pCube.mPosition.X + (pCube.cubeDimensions.X), pCube.mPosition.X - (pCube.cubeDimensions.X));
            float y = NextFloat((2.0f - 0.96f), 2.0f - 0.04f);
            float z = NextFloat(pCube.mPosition.Z + (pCube.cubeDimensions.Z), pCube.mPosition.Z - (pCube.cubeDimensions.Z));

            // Set all the spheres to random locations inside cube
            mPosition = new Vector3(x, y, z);
            mVelocity = new Vector3(ACWWindow.rand.Next(1, 3), ACWWindow.rand.Next(1, 3), 0);

            if (!changeBallType)
            {
            mScaleX = 0.04f;
            mScaleY = 0.04f;
            mScaleZ = 0.04f;

            mRadius = 1.0f * mScaleX;
            mVolume = (4 / 3) * (float)Math.PI * (float)Math.Pow(mRadius, 3);
            mDensity = 0.0014f;
            mMass = mDensity * mVolume;
            }
            else
            {
                mScaleX = 0.08f;
                mScaleY = 0.08f;
                mScaleZ = 0.08f;

                mRadius = 1.0f * mScaleX;
                mVolume = (4 / 3) * (float)Math.PI * (float)Math.Pow(mRadius, 3);
                mDensity = 0.001f;
                mMass = mDensity * mVolume;
            }


            mRotationX = 1.0f;
            mRotationY = 1.0f;
            mRotationZ = 1.0f;

            changeBallType ^= true;
        }

        // Sphere unique members
        public float mRadius;
        Vector3 lastPosition;
        private static bool changeBallType = true;

        public void MoveToEmitterBox(Cube pCube)
        {
            float x = NextFloat(pCube.mPosition.X + (pCube.cubeDimensions.X), pCube.mPosition.X - (pCube.cubeDimensions.X));
            float y = NextFloat((2.0f - 0.96f), 2.0f - 0.04f);
            float z = NextFloat(pCube.mPosition.Z + (pCube.cubeDimensions.Z), pCube.mPosition.Z - (pCube.cubeDimensions.Z));

            // Set all the spheres to random locations inside cube
            mPosition = new Vector3(x, y, z);
        }

        private float NextFloat(float min, float max)
        {
            return (float)(min + (ACWWindow.rand.NextDouble() * (max - min)));
        }

        public override void Load()
        {
            int size;
            mModelUtility = ModelUtility.LoadModel(@"Utility/Models/sphere.bin");

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
            GL.UniformMatrix4(uModel, true, ref mMatrix);

            GL.BindVertexArray(ACWWindow.mVAO_IDs[VAOIndex]);
            GL.DrawElements(PrimitiveType.Triangles, mModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
        }
        public override void Update(float dt, Vector3 gravity)
        {
            lastPosition = mPosition;
            mVelocity = mVelocity + gravity * dt;
            mPosition = mPosition + mVelocity * dt;
        }

        #region collision detection and response
        public void hasCollidedWithCube(Cube pCube)
        {
            float restitution = 1.0f;

            // X PLANE
            if ((mPosition.X + mRadius > (pCube.mPosition.X + (pCube.cubeDimensions.X))))
            {
                if (mVelocity.X > 0) // only perform collision response if the direction of velocity is same sign as normal of cube
                {
                    Vector3 normal = new Vector3(1, 0, 0);
                    mVelocity = mVelocity - (1 + restitution) * Vector3.Dot(normal, mVelocity) * normal;
                }
            }
            if ((mPosition.X - mRadius < (pCube.mPosition.X - (pCube.cubeDimensions.X)))) // Left inside of pCube
            {
                if (mVelocity.X < 0)
                {
                    Vector3 normal = new Vector3(-1, 0, 0);
                    mVelocity = mVelocity - (1 + restitution) * Vector3.Dot(normal, mVelocity) * normal;
                }
            }
            // Y PLANE
            if ((mPosition.Y + mRadius > (pCube.mPosition.Y + (pCube.cubeDimensions.Y))))
            {
                if (mVelocity.Y > 0)
                {
                    Vector3 normal = new Vector3(0, 1, 0);
                    mVelocity = mVelocity - (1 + restitution) * Vector3.Dot(normal, mVelocity) * normal;
                }

            }
            if ((mPosition.Y - mRadius) < (pCube.mPosition.Y - (pCube.cubeDimensions.Y)))
            {
                if (mVelocity.Y < 0)
                { // bottom collision

                    MoveToEmitterBox(pCube);

                    Vector3 normal = new Vector3(0, -1, 0);
                    mVelocity = mVelocity - (1 + restitution) * Vector3.Dot(normal, mVelocity) * normal;
                }
            }
            // Z PLANE
            if ((mPosition.Z + mRadius > (pCube.mPosition.Z + (pCube.cubeDimensions.Z))))
            {
                if (mVelocity.Z > 0)
                {
                    Vector3 normal = new Vector3(0, 0, 1);
                    mVelocity = mVelocity - (1 + restitution) * Vector3.Dot(normal, mVelocity) * normal;
                }
            }
            if ((mPosition.Z - mRadius) < (pCube.mPosition.Z - (pCube.cubeDimensions.Z)))
            {
                if (mVelocity.Z < 0)
                {
                    Vector3 normal = new Vector3(0, 0, -1);
                    mVelocity = mVelocity - (1 + restitution) * Vector3.Dot(normal, mVelocity) * normal;
                }
            }
        }
        public bool hasCollidedWithSphere(Sphere pSphere)
        {
            float restitution = 1f;

            double x = mPosition.X - pSphere.mPosition.X;
            double y = mPosition.Y - pSphere.mPosition.Y;
            double z = mPosition.Z - pSphere.mPosition.Z;
            double distance = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));


            if (distance < (mRadius * mScaleX) + (pSphere.mRadius * pSphere.mScaleX)) // multiplying by mScale X as scale matrix is uniform in spheres currently
            {
                //Vector3 circle1Momentumbefore = mCircleMass * mCircleVelocity;
                //Vector3 circle2Momentumbefore = mCircleMass2 * mCircleVelocity2;
                //Vector3 totalmomentumbefore = circle1Momentumbefore + circle2Momentumbefore;

                Vector3 pointOfCollision = new Vector3((float)x / 2, (float)y / 2, (float)z / 2);

                Vector3 sphereToCollision = mPosition - pointOfCollision;
                Vector3 sphereToCollision2 = pSphere.mPosition - pointOfCollision;

                Vector3 OriginalVelocity = mVelocity;
                Vector3 OriginalVelocity2 = pSphere.mVelocity;

                Vector3 normal = (pSphere.mPosition - mPosition).Normalized();
                Vector3 normal2 = (mPosition - pSphere.mPosition).Normalized();

                Vector3 velocityNorm = mVelocity.Normalized();
                Vector3 velocityNorm2 = pSphere.mVelocity.Normalized();


                ACWWindow.CollisionCount++;
                mVelocity = ((mMass * mVelocity) + (pSphere.mMass * pSphere.mVelocity) + (restitution * pSphere.mMass * (pSphere.mVelocity - mVelocity))) / (mMass + pSphere.mMass);
                pSphere.mVelocity = ((pSphere.mMass * pSphere.mVelocity) + (mMass * OriginalVelocity) + (restitution * mMass * (OriginalVelocity - pSphere.mVelocity))) / (pSphere.mMass + mMass);

                mPosition = lastPosition;
                pSphere.mPosition = pSphere.lastPosition;

                ////Vector3 test = Vector3.Cross(new Vector3(2, 1, 3), new Vector3(4, 2, 6));

                //if (Vector3.Cross(velocityNorm, velocityNorm2) != new Vector3(0, 0, 0)) // if the velocities
                //{
                //    mVelocity = OriginalVelocity;
                //}
                //if (Vector3.Cross(velocityNorm2, velocityNorm) != new Vector3(0, 0, 0))
                //{
                //    mVelocity = OriginalVelocity;
                //}

                //Vector3 circle1Momentumafter = mCircleMass * mCircleVelocity;
                //Vector3 circle2Momentumafter = mCircleMass2 * mCircleVelocity2;
                //Vector3 totalmomentumafter = circle1Momentumafter + circle2Momentumafter;

                return true;
            }
            return false;
        }
        #endregion
    }

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
            GL.UniformMatrix4(uModel, true, ref mMatrix);

            GL.BindVertexArray(ACWWindow.mVAO_IDs[VAOIndex]);
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
            mRadius = 1f;

            mScaleX = 1.0f;
            mScaleY = 1.0f;
            mScaleZ = 1.0f;

            mRotationX = 1.0f;
            mRotationY = 1.0f;
            mRotationZ = 1.0f;

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
            mMatrix = Matrix4.CreateScale(new Vector3(mScaleX, mScaleY, mScaleZ)) *
            //Matrix4.CreateRotationX(mRotationX) *
            //Matrix4.CreateRotationX(mRotationY) *
            //Matrix4.CreateRotationX(mRotationZ) *
            Matrix4.CreateTranslation(mPosition);

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