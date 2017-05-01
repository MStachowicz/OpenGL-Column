using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
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
        EntityManager level1Manager;
        EntityManager level2Manager;
        EntityManager level3Manager;

        public static Cube cube1;
        public static Sphere doomSphere;

        /// <summary>
        /// The number of spheres the sphere array will be instantiated to. 
        /// </summary>
        const int SPHERE_COUNT = 0;
        /// <summary>
        /// The number of unique object types the scene will contain, also used to reduce VAO + VBO usage.
        /// </summary>
        const int UNIQUE_OBJECTS = 3;
        //SphereOnCylinderResponse(normal);
        public static int[] mVAO_IDs = new int[UNIQUE_OBJECTS];
        public static int[] mVBO_IDs = new int[UNIQUE_OBJECTS * 2]; // each object has 2 vbo index'

        public static int vPositionLocation;
        public static int vNormal;

        // Physics todo: move to physics manager class
        public static Vector3 accelerationDueToGravity = new Vector3(0.0f, -9.81f, 0.0f);
        //public Vector3 accelerationDueToGravity = new Vector3(0.0f, 0.0f, 0.0f);
        public static float restitution = 0.7f;
        /// <summary>
        /// The most recent timestep returned by the timer class used in the acw update method.
        /// </summary>
        public static float timestep = 0.0f;

        /// <summary>
        /// The current material set in the shader. Used to prevent setting the 
        /// material again in entity manager render.
        /// </summary>
        public static Material materialSet;

        // Special features
        /// <summary>
        /// When set to true spheres will spin in their y axis.
        /// </summary>
        private bool spinningCylinders = false;
        /// <summary>
        /// When set to true, stops updating the scene from updating until toggled back on.
        /// Static so can be used to debug other classes by pausing at time of event of interest.
        /// </summary>
        private static bool pauseTime = false;
        /// <summary>
        /// Count of the number of collisions occuring in the scene.
        /// </summary>
        public static int CollisionCount = 0;


        /// <summary>
        /// Toggles all the cylinders spinning in their y axis.
        /// </summary>
        public void toggleSpinningCylinders()
        {
            spinningCylinders ^= true;
        }
        /// <summary>
        /// Toggles pausing the simulation.
        /// </summary>
        public static void pauseSimulation()
        {
            pauseTime ^= true;
        }


        public Timer mTimer;
        public static Random rand;

        #region Camera

        public enum CameraType
        { cFixed, cControlled, cFollow, cPath }
        public CameraType cameraType = CameraType.cFixed;


        public void ViewTranslate(Vector3 pTranslation)
        {
            mView = mView * Matrix4.CreateTranslation(pTranslation);

            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);

            SetLightPositions();
        }
        public void ViewRotateX(float pRotation)
        {
            Vector3 t = mView.ExtractTranslation();

            Matrix4 translation = Matrix4.CreateTranslation(t);
            Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);

            mView = mView * inverseTranslation * Matrix4.CreateRotationX(pRotation) * translation;

            // Set mview in the shader
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);

            SetLightPositions();
        }
        public void ViewRotateY(float pRotation)
        {
            string test = this.ToString();
            Vector3 t = mView.ExtractTranslation();

            Matrix4 translation = Matrix4.CreateTranslation(t);
            Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);

            mView = mView * inverseTranslation * Matrix4.CreateRotationY(pRotation) * translation;
            
            // Set mview in the shader
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);

            SetLightPositions();
        }
        public void ViewRotateZ(float pRotation)
        {
            string test = this.ToString();
            Vector3 t = mView.ExtractTranslation();

            Matrix4 translation = Matrix4.CreateTranslation(t);
            Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);

            mView = mView * inverseTranslation * Matrix4.CreateRotationZ(pRotation) * translation;
            
            // Set mview in the shader
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);

            SetLightPositions();
        }

        #endregion

        public void SetLightPositions()
        {
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
            mView = Matrix4.CreateTranslation(0.0f, 0.0f, -5.0f);
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

            level1Manager = new EntityManager();
            rand = new Random();

            // CUBE
            cube1 = new Cube();        
            level1Manager.ManageEntity(cube1); // cube added last for cull fix in the entity manager render method.
            
            // CYLINDERS
            // LEVEL 1
            Cylinder cylinder0 = new Cylinder(new Vector3(cube1.centerlevel1.X, cube1.centerlevel1.Y + 0.25f, cube1.centerlevel1.Z), 0.075f);
            level1Manager.ManageEntity(cylinder0);
            cylinder0.RotateX((float)Math.PI / 2);

            Cylinder cylinder1 = new Cylinder(new Vector3(cube1.centerlevel1.X, cube1.centerlevel1.Y + 0.25f, cube1.centerlevel1.Z), 0.075f);
            level1Manager.ManageEntity(cylinder1);
            cylinder1.RotateZ((float)Math.PI / 2);

            Cylinder cylinder2 = new Cylinder(new Vector3(cube1.centerlevel1.X, cube1.centerlevel1.Y - 0.25f, cube1.centerlevel1.Z), 0.15f);
            level1Manager.ManageEntity(cylinder2);
            cylinder2.RotateX((float)Math.PI / 2);

            Cylinder cylinder3 = new Cylinder(new Vector3(cube1.centerlevel1.X, cube1.centerlevel1.Y - 0.25f, cube1.centerlevel1.Z), 0.15f);
            level1Manager.ManageEntity(cylinder3);
            cylinder3.RotateZ((float)Math.PI / 2);

            // LEVEL 2
            Cylinder cylinder4 = new Cylinder(new Vector3(cube1.centerlevel2.X, cube1.centerlevel2.Y, cube1.centerlevel2.Z), 0.10f);
            level1Manager.ManageEntity(cylinder4);
            cylinder4.scale(new Vector3(1.0f, 1.3f, 1.0f));
            cylinder4.RotateX(DegreeToRadian(90));
            cylinder4.RotateY(DegreeToRadian(135));

            Cylinder cylinder5 = new Cylinder(new Vector3(cube1.centerlevel2.X, cube1.centerlevel2.Y, cube1.centerlevel2.Z), 0.15f);
            cylinder5.scale(new Vector3(1.0f, 1.6f, 1.0f));
            cylinder5.RotateX(DegreeToRadian(300));
            cylinder5.RotateY(DegreeToRadian(45));
            level1Manager.ManageEntity(cylinder5);

            // SPHERES
            for (int i = 0; i < SPHERE_COUNT; i++)
            {
                spawnSphere();
            }

            // sphere of doom
            level1Manager.ManageEntity(new Sphere(cube1.centerlevel3, 0.25f, true, Sphere.SphereType.doom));


            level1Manager.loadObjects();

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

        /// <summary>
        /// Creates an instance of the sphere and adds it to the entity manager and sphere list.
        /// </summary>
        private void spawnSphere()
        {
            level1Manager.ManageEntity(new Sphere(cube1));
        }

        private float DegreeToRadian(double angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButton.Left:
                    spawnSphere();
                    break;
                case MouseButton.Middle:
                    break;
                case MouseButton.Right:
                    break;
                default:
                    break;
            }

            base.OnMouseDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            float cameraSpeed = 0.1f;

            switch (e.KeyChar)
            {
                case '1':
                    toggleSpinningCylinders();
                    break;
                case '2':
                    spawnSphere();
                    break;
                case '3': // update the simulation by 0.1 seconds 
                    pauseSimulation();
                    OnUpdateFrame(new FrameEventArgs(0.1));
                    pauseSimulation();
                    break;

                case '4':
                    ViewRotateX(cameraSpeed);
                    break;
                case '5':
                    ViewRotateY(cameraSpeed);
                    break;
                case '6':
                    ViewRotateZ(cameraSpeed);
                    break;
                case 'p':
                    pauseSimulation();
                    break;
                case 'r':
                    level1Manager.resetSpheres();
                    break;


                    // Camera 
                case 'w':
                    ViewTranslate(new Vector3(0.0f, -cameraSpeed, 0.0f));
                    break;
                case 'a':
                    ViewTranslate(new Vector3(cameraSpeed, 0.0f, 0.0f));
                    break;
                case 's':
                    ViewTranslate(new Vector3(0.0f, cameraSpeed, 0.0f));
                    break;
                case 'd':
                    ViewTranslate(new Vector3(-cameraSpeed, 0.0f, 0.0f));
                    break;
                case 'q':
                    ViewTranslate(new Vector3(0.0f, 0.0f, -cameraSpeed));
                    break;
                case 'e':
                    ViewTranslate(new Vector3(0.0f, 0.0f, cameraSpeed));
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
            timestep = mTimer.GetElapsedSeconds();

            // Dont perform any updating if time is paused.
            if (!pauseTime)
            {
                level1Manager.updateObjects();
                level1Manager.CheckCollisions();

                //if (spinningCylinders)
                //{
                //}
                //    foreach (Cylinder c in CylinderList)
                //        c.RotateY(DegreeToRadian(0.2));
            }
            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            level1Manager.renderObjects();

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