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
                1920, // Width
                1080, // Height
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

        // OBJECTS
        entityManager Manager;
        Cube cube1;
        Cylinder[] cylinderArray;
        Sphere[] sphereArray;

        /// <summary>
        /// The number of spheres the sphere array will be instantiated to. 
        /// </summary>
        const int SPHERECOUNT = 5;
        /// <summary>
        /// The number of cylinders the cylinder array will contain.
        /// </summary>
        const int CYLINDER_COUNT = 6;
        /// <summary>
        /// The number of unique object types the scene will contain, also used to reduce VAO + VBO usage.
        /// </summary>
        const int NUMBER_UNIQUE_OBJECTS = 3;


        public static int[] mVAO_IDs = new int[NUMBER_UNIQUE_OBJECTS];
        public static int[] mVBO_IDs = new int[NUMBER_UNIQUE_OBJECTS * 2]; // each object has 2 vbo index'

        public static int vPositionLocation;
        public static int vNormal;

        //public Vector3 accelerationDueToGravity = new Vector3(0.0f, 0.0f, 0.0f);
        public Vector3 accelerationDueToGravity = new Vector3(0.0f, -9.81f, 0.0f);
        public static float restitution = 0.8f;


        // Special features
        /// <summary>
        /// When set to true spheres are no longer bounded by the cube.
        /// </summary>
        bool releaseSpheres = false;
        /// <summary>
        /// When set to true spheres will spin in their y axis.
        /// </summary>
        bool spinningCylinders = false;
        /// <summary>
        /// When set to true, stops updating the scene from updating until toggled back on.
        /// </summary>
        public bool pauseTime = false;
        /// <summary>
        /// Count of the number of collisions occuring in the scene.
        /// </summary>
        public static int CollisionCount = 0;


        private Timer mTimer;
        public static Random rand;

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
            // 100 cm = 1.0f

            Manager = new entityManager();
            rand = new Random();

            // CUBE
            cube1 = new Cube();
            cube1.mPosition = new Vector3(0.0f, 0.0f, -5.0f);

            Vector3 centerlevel1 = new Vector3(cube1.mPosition.X, cube1.mPosition.Y + 0.5f, cube1.mPosition.Z);
            Vector3 centerlevel2 = new Vector3(cube1.mPosition.X, cube1.mPosition.Y - 0.5f, cube1.mPosition.Z);
            Vector3 centerlevel3 = new Vector3(cube1.mPosition.X, cube1.mPosition.Y - 1.5f, cube1.mPosition.Z);

            // CYLINDERS
            cylinderArray = new Cylinder[CYLINDER_COUNT];

            // LEVEL 1
            cylinderArray[0] = new Cylinder(new Vector3(centerlevel1.X, centerlevel1.Y + 0.25f, centerlevel1.Z), 0.075f);
            cylinderArray[0].RotateX((float)Math.PI / 2);
            cylinderArray[1] = new Cylinder(new Vector3(centerlevel1.X, centerlevel1.Y + 0.25f, centerlevel1.Z), 0.075f);
            cylinderArray[1].RotateZ((float)Math.PI / 2);

            cylinderArray[2] = new Cylinder(new Vector3(centerlevel1.X, centerlevel1.Y - 0.25f, centerlevel1.Z), 0.15f);
            cylinderArray[2].RotateX((float)Math.PI / 2);
            cylinderArray[3] = new Cylinder(new Vector3(centerlevel1.X, centerlevel1.Y - 0.25f, centerlevel1.Z), 0.15f);
            cylinderArray[3].RotateZ((float)Math.PI / 2);

            // LEVEL 2
            cylinderArray[4] = new Cylinder(new Vector3(centerlevel2.X, centerlevel2.Y, centerlevel2.Z), 0.10f);
            cylinderArray[4].scale(new Vector3(1.0f, 1.3f, 1.0f));
            cylinderArray[4].RotateX(DegreeToRadian(90));
            cylinderArray[4].RotateY(DegreeToRadian(135));

            cylinderArray[5] = new Cylinder(new Vector3(centerlevel2.X, centerlevel2.Y, centerlevel2.Z), 0.15f);
            cylinderArray[5].scale(new Vector3(1.0f, 1.6f, 1.0f));
            cylinderArray[5].RotateX(-(float)Math.PI / 3);
            cylinderArray[5].RotateY((float)Math.PI / 4);

            foreach (Cylinder i in cylinderArray)
                Manager.ManageEntity(i);

            // SPHERES
            sphereArray = new Sphere[SPHERECOUNT]; // create spheres array

            for (int i = 0; i < sphereArray.Length; i++)
            {
                sphereArray[i] = new Sphere(cube1);
                Manager.ManageEntity(sphereArray[i]);
            }


            //sphere1 = new Sphere(cube1);
            //sphere1.mPosition = new Vector3(cylinder1.mPosition.X - 0.4f, cylinder1.mPosition.Y + 1.4f, cylinder1.mPosition.Z);
            //sphere1.mVelocity = new Vector3(0.4f, 0.4f, 0.001f);

            //Manager.ManageEntity(sphere1);


            //sphere2 = new Sphere(cube1);
            //sphere2.mPosition = new Vector3(cube1.mPosition.X - 0.4f, cube1.mPosition.Y, cube1.mPosition.Z);
            //sphere2.mVelocity = new Vector3(2.0f, 0.0f, 0.0f);

            //Manager.ManageEntity(sphere2);


            Manager.ManageEntity(cube1); // cube added last for cull fix in the entity manager render method.
            Manager.loadObjects();

            #endregion


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

        private float DegreeToRadian(double angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            float cameraSpeed = 0.1f;

            switch (e.KeyChar)
            {
                case '1':
                    break;
                case '2':
                    break;
                case '3':
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
                    break;
                case 't':
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


            if (spinningCylinders)
                foreach (Cylinder i in cylinderArray)
                    i.RotateY(DegreeToRadian(0.2));

            if (!pauseTime)
                for (int i = 0; i < sphereArray.Length; i++)
                {
                    // UPDATE SPHERE POSITION
                    sphereArray[i].Update(timestep, accelerationDueToGravity);

                    // CUBE COLLISION CHECK
                    if (!releaseSpheres) // allows switching off collisions with bounding cube             
                        sphereArray[i].hasCollidedWithCube(cube1);

                    // SPHERE ON SPHERE COLLISION CHECK
                    foreach (var j in sphereArray)
                        if (!sphereArray[i].Equals(j)) // If this is not the same sphere
                            sphereArray[i].hasCollidedWithSphere(j); // check for collisions with all other spheres

                    // SPHERE ON CYLINDER CHECK
                    foreach (Cylinder j in cylinderArray)
                        sphereArray[i].hasCollisedWithCylinder(j);
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
}