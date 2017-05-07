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
                1600, // Width
                900, // Height
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

        // STATIC MANAGERS 
        public static EntityManager level1Manager;
        public static EntityManager level2Manager;
        public static EntityManager level3Manager;

        public static ParticleManager particleManager;

        public static Cube cube;
        public static Sphere doomSphere;

        /// <summary>
        /// The number of unique object types the scene will contain, also used to reduce VAO + VBO usage.
        /// </summary>
        const int UNIQUE_OBJECTS = 3;
        /// <summary>
        /// The time in seconds between to the next sphere spawn.
        /// </summary>
        public double sphereCountdown = 1.5;
        /// <summary>
        /// The maximum number of spheres that will spawn.
        /// </summary>
        const int MAXSPHERES = 7;



        //SphereOnCylinderResponse(normal);
        public static int[] mVAO_IDs = new int[UNIQUE_OBJECTS];
        public static int[] mVBO_IDs = new int[UNIQUE_OBJECTS * 2]; // each object has 2 vbo index'

        public static int vPositionLocation;
        public static int vNormal;

        // Physics todo: move to physics manager class
        public static Vector3 accelerationDueToGravity = new Vector3(0.0f, -9.81f, 0.0f);
        //public static Vector3 accelerationDueToGravity = new Vector3(0.0f, 0.0f, 0.0f);
        /// <summary>
        /// energy converted from kinetic (movement) energy to other types of energy. A coefficient of restitution 
        /// of 1 will result in a perfectly elastic collision where all energy involved in the collision is retained.
        /// A coefficient of 0 will result in all energy in the collision being lost.
        /// </summary>
        public static float restitution = 0.7f;
        /// <summary>
        /// The most recent timestep returned by the timer class used in the acw update method.
        /// </summary>
        public static float timestep = 0.0f;

        public enum IntegrationMethod
        {
            /// <summary>
            /// Assumes that the timestep is so small that the change in velocity during the timestep is constant.
            /// Has a tendency to increase the amount of energy in the system.
            /// Calculates the new velocity After calculating new position. 
            /// Position -> velocity.
            /// </summary>
            euler,

            /// <summary>
            /// Calculates the new velocity before calculating the new position.
            /// Velocity -> position
            /// </summary>
            symplecticEuler

            // todo search: 
            // Verlet integration - constant accelaration and stable timesteps
            // fourth order Runge Kutta (RK4) - costly

        };
        /// <summary>
        /// The method of integration used to update the position and velocity of the spheres in the sphere
        /// update method.
        /// </summary>
        public static IntegrationMethod integrationMethod = IntegrationMethod.euler;

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
        float cameraSpeed = 0.1f;

        public enum CameraType
        { cFixed, cControlled, cFollow, cPath }
        public CameraType cameraType = CameraType.cControlled;

        /// <summary>
        /// Translate the mView by a translation and sets the uView in the shader, 
        /// reset the light positions to original positions.
        /// </summary>
        /// <param name="pTranslation"></param>
        public void ViewTranslate(Vector3 pTranslation)
        {
            mView = mView * Matrix4.CreateTranslation(pTranslation);

            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);

            //int uEyePosition = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            //Vector4 eyePosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
            //GL.Uniform4(uEyePosition, eyePosition);

            SetLightPositions();
        }
        /// <summary>
        /// Sets the mView to a translation and sets the uView in the shader.
        /// </summary>
        /// <param name="pTranslation"></param>
        public void setViewPosition(Vector3 pTranslation)
        {
            mView = Matrix4.CreateTranslation(pTranslation);

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
            Vector4 lightPosition = new Vector4(cube.mPosition.X, cube.mPosition.Y + (cube.cubeDimensions.Y / 2), cube.mPosition.Z, 1.0f);
            Vector4 lightPosition1 = new Vector4(cube.mPosition, 1.0f);
            Vector4 lightPosition2 = new Vector4(cube.mPosition.X, cube.mPosition.Y - (cube.cubeDimensions.Y) / 2, cube.mPosition.Z, 1.0f);

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


            particleManager = new ParticleManager();

            level1Manager = new EntityManager();
            level2Manager = new EntityManager();
            level3Manager = new EntityManager();

            rand = new Random();

            // CUBE
            cube = new Cube();

            // Added to one manager for rendering once, collisions are checked using static cube1 instance
            level1Manager.ManageEntity(cube);


            // CYLINDERS
            // LEVEL 1
            Cylinder cylinder0 = new Cylinder(new Vector3(cube.centerlevel1.X, cube.centerlevel1.Y + 0.25f, cube.centerlevel1.Z), 0.075f);
            cylinder0.RotateX((float)Math.PI / 2);

            Cylinder cylinder1 = new Cylinder(new Vector3(cube.centerlevel1.X, cube.centerlevel1.Y + 0.25f, cube.centerlevel1.Z), 0.075f);
            cylinder1.RotateZ((float)Math.PI / 2);

            Cylinder cylinder2 = new Cylinder(new Vector3(cube.centerlevel1.X, cube.centerlevel1.Y - 0.25f, cube.centerlevel1.Z), 0.15f);
            cylinder2.RotateX((float)Math.PI / 2);

            Cylinder cylinder3 = new Cylinder(new Vector3(cube.centerlevel1.X, cube.centerlevel1.Y - 0.25f, cube.centerlevel1.Z), 0.15f);
            cylinder3.RotateZ((float)Math.PI / 2);

            level1Manager.ManageEntity(cylinder0);
            level1Manager.ManageEntity(cylinder1);
            level1Manager.ManageEntity(cylinder2);
            level1Manager.ManageEntity(cylinder3);

            // LEVEL 2
            Cylinder cylinder4 = new Cylinder(new Vector3(cube.centerlevel2.X, cube.centerlevel2.Y, cube.centerlevel2.Z), 0.10f);
            cylinder4.scale(new Vector3(1.0f, 1.3f, 1.0f));
            cylinder4.RotateX(DegreeToRadian(90));
            cylinder4.RotateY(DegreeToRadian(135));

            Cylinder cylinder5 = new Cylinder(new Vector3(cube.centerlevel2.X, cube.centerlevel2.Y, cube.centerlevel2.Z), 0.15f);
            cylinder5.scale(new Vector3(1.0f, 1.6f, 1.0f));
            cylinder5.RotateX(DegreeToRadian(300));
            cylinder5.RotateY(DegreeToRadian(45));

            level2Manager.ManageEntity(cylinder4);
            level2Manager.ManageEntity(cylinder5);

            // LEVEL 3 sphere of doom
            level3Manager.ManageEntity(new Sphere(cube.centerlevel3, 0.25f, true, Sphere.SphereType.doom));

            level1Manager.loadObjects();
            level2Manager.loadObjects();
            level3Manager.loadObjects();


            #region Loading in the lights and binding shader light variables

            float AmbientIntensity = 0.8f;
            float DiffuseIntensity = 0.8f;
            float SpecularIntensity = 0.1f;

            Vector4 lightPosition = new Vector4(cube.mPosition.X, cube.mPosition.Y + (cube.cubeDimensions.Y / 2), cube.mPosition.Z, 1.0f);
            Vector4 lightPosition1 = new Vector4(cube.mPosition, 1.0f);
            Vector4 lightPosition2 = new Vector4(cube.mPosition.X, cube.mPosition.Y - (cube.cubeDimensions.Y / 2), cube.mPosition.Z, 1.0f);

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
            int noOfSpheres = 0;

            for (int i = 0; i < Sphere.AllObjects.Count; i++)
            {
                if (Sphere.AllObjects[i].sphereType == Sphere.SphereType.red || Sphere.AllObjects[i].sphereType == Sphere.SphereType.yellow)
                {
                    noOfSpheres++; // only count the red and yellow spheres.
                }
            }


            if (noOfSpheres < MAXSPHERES)
            {
                //Console.WriteLine("Sphere spawned. Sphere count: " + Sphere.AllObjects.Count);
                level1Manager.ManageEntity(new Sphere(cube));
            }
        }

        private float DegreeToRadian(double angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }


        /// <summary>
        /// Changes the camera type to the next one.
        /// </summary>
        private void cycleCameraType()
        {
            switch (cameraType)
            {
                case CameraType.cControlled: // default set 
                    cameraType = CameraType.cPath;
                    break;
                case CameraType.cPath:
                    cameraSpeed = 0.1f; // reset the camera speed back
                    cameraType = CameraType.cFixed;
                    break;
                case CameraType.cFixed:
                    if (FindBallFollowIndex())
                    {
                        cameraType = CameraType.cFollow;
                    }
                    else
                    {
                        cameraType = CameraType.cControlled;
                    }
                    break;
                case CameraType.cFollow:
                    cameraType = CameraType.cControlled;
                    break;

                default:
                    throw new Exception(string.Format("The camera type \"{0}\" has not been implemented.", cameraType));
            }

            Console.WriteLine("Camera type set to: " + cameraType);
        }

        private void cycleIntegrationMethod()
        {
            switch (integrationMethod)
            {
                case IntegrationMethod.euler:
                    integrationMethod = IntegrationMethod.symplecticEuler;
                    break;
                case IntegrationMethod.symplecticEuler:
                    integrationMethod = IntegrationMethod.euler;
                    break;



                default:
                    throw new Exception("This integration method has not been implemented yet.");
            }

            Console.WriteLine("Integration method changed to: " + integrationMethod);
        }

        /// <summary>
        /// Reset all the spheres to the emitter box and reset the camera position and type to controlled camera
        /// </summary>
        public void resetSimulation()
        {
            // Reset sphere positions
            level1Manager.resetSpheres();
            level2Manager.resetSpheres();
            level3Manager.resetSpheres();

            setViewPosition(new Vector3(0.0f, 0.0f, -5.0f));
            cameraType = CameraType.cControlled;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            switch (e.KeyChar)
            {
                // SPECIAL FEATURES
                case '1':
                    toggleSpinningCylinders();
                    break;
                case '2':
                    break;
                case '3':
                    break;
                case '4':
                    break;
                case '5':
                    break;
                case '6':
                    break;


                // SIMULATION
                case 'o':
                    pauseSimulation(); // update the simulation by 0.1 seconds 
                    OnUpdateFrame(new FrameEventArgs(0.1));
                    pauseSimulation();
                    break;
                case 'p':
                    pauseSimulation();
                    break;
                case 'r':
                    resetSimulation();
                    break;
                case 'i':
                    cycleIntegrationMethod();
                    break;


                // CAMERA
                case '0': // reverse camera direction of movemenent
                    cameraSpeed = -cameraSpeed;
                    break;
                case '-':
                    cameraSpeed -= 0.001f;
                    break;
                case '=': // reverse camera direction of movemenent
                    cameraSpeed += 0.001f;
                    break;


                case ' ':
                    cycleCameraType();
                    break;


                // CONTROLLED CAMERA 
                case 'w':
                    if (cameraType == CameraType.cControlled)
                        ViewTranslate(new Vector3(0.0f, -cameraSpeed, 0.0f));
                    break;
                case 'a':
                    if (cameraType == CameraType.cControlled)
                        ViewTranslate(new Vector3(cameraSpeed, 0.0f, 0.0f));
                    break;
                case 's':
                    if (cameraType == CameraType.cControlled)
                        ViewTranslate(new Vector3(0.0f, cameraSpeed, 0.0f));
                    break;
                case 'd':
                    if (cameraType == CameraType.cControlled)
                        ViewTranslate(new Vector3(-cameraSpeed, 0.0f, 0.0f));
                    break;
                case 'q':
                    if (cameraType == CameraType.cControlled)
                        ViewTranslate(new Vector3(0.0f, 0.0f, -cameraSpeed));
                    break;
                case 'e':
                    if (cameraType == CameraType.cControlled)
                        ViewTranslate(new Vector3(0.0f, 0.0f, cameraSpeed));
                    break;
                case 'z':
                    if (cameraType == CameraType.cControlled || cameraType == CameraType.cFollow)
                        ViewRotateX(cameraSpeed);
                    break;
                case 'x':
                    if (cameraType == CameraType.cControlled || cameraType == CameraType.cFollow)
                        ViewRotateY(cameraSpeed);
                    break;
                case 'c':
                    if (cameraType == CameraType.cControlled || cameraType == CameraType.cFollow)
                        ViewRotateZ(cameraSpeed);
                    break;


                default:
                    break;
            }
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



        int ballFollowIndex = 0;

        public bool FindBallFollowIndex()
        {
            for (int i = ballFollowIndex; i < Sphere.AllObjects.Count; i++)
            {
                if (Sphere.AllObjects[i].sphereType == Sphere.SphereType.yellow || Sphere.AllObjects[i].sphereType == Sphere.SphereType.red)
                {
                    ballFollowIndex = i;
                    return true;
                }
            }
            return false;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            #region Camera

            // PATH CAMERA
            if (cameraType == CameraType.cPath)
            {
                Vector3 viewPosition = mView.ExtractTranslation();

                // Reverse the camera whenever it is above or below the cube 
                if (-viewPosition.Y > cube.cubeDimensions.Y)
                    cameraSpeed = -cameraSpeed;
                else if (-viewPosition.Y < -cube.cubeDimensions.Y) // bottom
                    cameraSpeed = -cameraSpeed;

                ViewTranslate(new Vector3(0.0f, cameraSpeed / 10, 0.0f));
                ViewRotateY(Math.Abs(cameraSpeed / 10));

                Vector3 viewPosition2 = mView.ExtractTranslation();
            }

            // FOLLOW CAMERA
            if (cameraType == CameraType.cFollow)
            {
                if (ballFollowIndex < Sphere.AllObjects.Count - 1)
                {
                    setViewPosition(new Vector3(
                        -Sphere.AllObjects[ballFollowIndex].mPosition.X,
                        -Sphere.AllObjects[ballFollowIndex].mPosition.Y,
                        -Sphere.AllObjects[ballFollowIndex].mPosition.Z - 1));
                }
                else
                {
                    // Reset camera type if the ball to follow index is greater than count of spheres.
                    Console.WriteLine("No sphere to follow exists, cycling camera type.");
                    cycleCameraType();
                }
            }

            #endregion

            // SIMULATION
            timestep = mTimer.GetElapsedSeconds();



            // Dont perform any updating if time is paused.
            if (!pauseTime)
            {
                sphereCountdown -= timestep;

                //Console.WriteLine("countdown to next sphere: " + sphereCountdown);

                if (sphereCountdown < 0)
                {
                    spawnSphere();
                    sphereCountdown = 1.5f;
                }

                // Updating all sphere positions
                level1Manager.updateObjects();
                level2Manager.updateObjects();
                level3Manager.updateObjects();

                // Checking for 
                level1Manager.CheckSpherePositions();
                level2Manager.CheckSpherePositions();
                level3Manager.CheckSpherePositions();

                level1Manager.CheckCollisions();
                level2Manager.CheckCollisions();
                level3Manager.CheckCollisions();

                particleManager.UpdateParticles();

                if (spinningCylinders)
                {
                    foreach (Cylinder c in Cylinder.AllObjects)
                        c.RotateY(DegreeToRadian(0.2));
                }
            }
            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            level1Manager.renderObjects();
            level2Manager.renderObjects();
            level3Manager.renderObjects();
            particleManager.RenderParticles();

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