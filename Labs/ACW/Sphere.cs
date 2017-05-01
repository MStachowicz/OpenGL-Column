using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Labs.Utility;
using System.Collections.Generic;

namespace Labs.ACW
{
    public class Sphere : entity
    {
        /// <summary>
        /// Constructor used to create sphere of doom and any particle spheres.
        /// </summary>
        /// <param name="pPosition">Position of the sphere</param>
        /// <param name="pRadius">The radius of the sphere in m. (1 = 1m)</param>
        /// <param name="pStaticSphere">Is this sphere static.</param>
        /// <param name="pAddToList">Should this sphere be added to the static sphere list..</param>
        public Sphere(Vector3 pPosition, float pRadius, bool pStaticSphere, SphereType pType)
        {
            SetRadius(pRadius);
            mPosition = pPosition;

            switch (pType)
            {
                case SphereType.doom:
                    sphereType = SphereType.doom;
                    staticObject = true;
                    mMaterial = new Material(new Vector3(0.576f, 0.439f, 0.859f), 0.5f, 0.5f);
                    break;
                case SphereType.particle:
                    staticObject = false;
                    sphereType = SphereType.particle;
                    mMaterial = new Material(new Vector3(1.0f, 0.0f, 0.0f), 0.8f, 0.5f);

                    mVelocity = new Vector3(
                        NextFloat(1, 2),
                        NextFloat(1, 2),
                        NextFloat(1, 2));
                    break;
                case SphereType.test:
                    // todo add code here or create seperate constructor for the two test methods creating spheres.
                    break;

                default:
                    throw new Exception("Sphere being created using an incorrect constructor");
            }

            // Set the matrix for rendering.
            mMatrix = Matrix4.CreateScale(mScaleX, mScaleY, mScaleZ) * Matrix4.CreateTranslation(mPosition);
        }
        /// <summary>
        /// Creates a ball in the top cube of the scene in a random position. Alternates two ball types.
        /// </summary>
        /// <param name="pCube">The cube whos top box all the spheres will be translated to.</param>
        public Sphere(Cube pCube)
        {
            // SPHERE PROPERTIES

            #region Scale + mass for 2 different balls

            if (!changeBallType)
            { // 4cm radius ball
                mDensity = 1400f; // density =  0.0014 kg per centimetre cubed = 1400 kg per metre cubed
                SetRadius(0.04f); // radius = 4 cm = 0.04m
                mMaterial = new Material(new Vector3(1.0f, 1.0f, 0.0f), 0.5f, 1.0f); // shiny yellow
                sphereType = SphereType.yellow;
            }
            else
            {// 8cm radius ball
                mDensity = 0.001f; // density = 0.001 kg per metre cubed
                SetRadius(0.08f); // radius = 8 cm = 0.08m
                mMaterial = new Material(new Vector3(0.6f, 0.0f, 0.0f), 0.5f, 0.1f); // matt crimson
                sphereType = SphereType.red;
            }

            #endregion

            staticObject = false;
            MoveToEmitterBox(pCube, false);
            mMatrix = Matrix4.CreateScale(mScaleX, mScaleY, mScaleZ) * Matrix4.CreateTranslation(mPosition);

            // Next sphere instantiated will be of the other sphere type.
            changeBallType ^= true;
        }

        /// <summary>
        /// The type of sphere.
        /// </summary>
        public enum SphereType
        {
            /// <summary>
            /// 4cm radius shiny yellow sphere with high density.
            /// </summary>
            yellow,
            /// <summary>
            /// 8cm radius matt red sphere with low density.
            /// </summary>
            red,
            /// <summary>
            /// 25 cm radius medium purple sphere. Reduces radius of spheres which collide with it.
            /// </summary>
            doom,
            /// <summary>
            /// Used in particle effects, no collision response or detection is performed on these spheres.
            /// </summary>
            particle,
            /// <summary>
            /// Sphere type used for testing
            /// </summary>
            test
        };
        /// <summary>
        /// Stores which type of sphere this was originally instantiated to be.
        /// </summary>
        public SphereType sphereType;

        /// <summary>
        /// Updates all the members related to the radius of the sphere. (mScale, mVolume, mMass). 
        /// If the radius being set is negative the sphere is reset and moved to the emitter box.
        /// </summary>
        /// <param name="pRadius">The new radius being assigned to the sphere.</param>
        private void SetRadius(float pRadius)
        {
            // if radius is negative sphere is reset and moved back to the emitter box.
            if (pRadius < 0)
            {
                switch (sphereType)
                {
                    case SphereType.yellow:
                        mDensity = 1400f; // density =  0.0014 kg per centimetre cubed = 1400 kg per metre cubed
                        SetRadius(0.04f); // radius = 4 cm = 0.04m
                        mMaterial = new Material(new Vector3(1.0f, 1.0f, 0.0f), 0.5f, 1.0f); // shiny yellow
                        break;
                    case SphereType.red:
                        mDensity = 0.001f; // density = 0.001 kg per metre cubed
                        SetRadius(0.08f); // radius = 8 cm = 0.08m
                        mMaterial = new Material(new Vector3(0.6f, 0.0f, 0.0f), 0.5f, 0.1f); // matt crimson
                        break;
                    default:
                        break;
                }

                // move sphere back up
                MoveToEmitterBox(ACWWindow.cube1, false);
            }
            else
            {
                mRadius = pRadius;

                mScaleX = pRadius;
                mScaleY = pRadius;
                mScaleZ = pRadius;

                mVolume = (float)((4 / 3) * Math.PI * Math.Pow(pRadius, 3));
                mMass = mDensity * mVolume;
            }
        }

        /// <summary>
        /// Radius of the sphere in meters.
        /// </summary>
        public float mRadius;
        /// <summary>
        /// The position of the sphere in the previous frame.
        /// </summary>
        Vector3 lastPosition;

        /// <summary>
        /// Toggled to change the type of ball instantiated by the sphere constructor.
        /// </summary>
        private static bool changeBallType = true;
        /// <summary>
        /// Has a sphere object been loaded previously, if so following instances will use the same VAO and VBO index for their render.
        /// </summary>
        private static bool isLoaded = false;
        /// <summary>
        /// The index in the VAO that all spheres are loaded from. Set in the load method.
        /// </summary>
        public static int VAOIndex;
        /// <summary>
        /// The index in the VBO that all spheres are rendered from. Set in the load method.
        /// </summary>
        public static int VBOIndex;
        /// <summary>
        /// The model utility all the spheres will use.
        /// </summary>
        private static ModelUtility mModelUtility;

        public override void Load()
        {
            if (!isLoaded)
            {
                VAOIndex = EntityManager.VAOCount++; // set the VAO index all spheres will use.
                VBOIndex = EntityManager.VBOCount++; // set the VBO index all spheres will use.
                EntityManager.VBOCount++;
                isLoaded = true;

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
        }
        public override void Render()
        {
            int uModel = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uModel");

            GL.UniformMatrix4(uModel, true, ref mMatrix);

            GL.BindVertexArray(ACWWindow.mVAO_IDs[VAOIndex]);
            GL.DrawElements(PrimitiveType.Triangles, mModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            mMatrix = Matrix4.CreateScale(mScaleX, mScaleY, mScaleZ) * Matrix4.CreateTranslation(mPosition);

            //if (sphereType == SphereType.particle)
            //{
            //    Console.WriteLine("hit");
            //}
        }

        /// <summary>
        /// Set the current position of the sphere as the last position and move the sphere by the velocity calculated.
        /// </summary>
        public override void Update()
        {
            if (sphereType == SphereType.particle) // update for particles does not apply gravity
            {
                mVelocity = mVelocity + ACWWindow.accelerationDueToGravity * ACWWindow.timestep;
                mPosition = mPosition + mVelocity * ACWWindow.timestep;
            }
            else if (!staticObject)
                {
                    lastPosition = mPosition;

                    mVelocity = mVelocity + ACWWindow.accelerationDueToGravity * ACWWindow.timestep;
                    mPosition = mPosition + mVelocity * ACWWindow.timestep;

                    //Console.WriteLine("sphere cylinder after collision")
                }
            
        }

        // UTILITY METHODS
        /// <summary>
        /// Generate a floating point number between the minimum and maximum.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private float NextFloat(float min, float max)
        {
            return (float)(min + (ACWWindow.rand.NextDouble() * (max - min)));
        }
        /// <summary>
        /// Returns the double distance betweem the faces of the spheres adjusted for their radii.
        /// </summary>
        /// <param name="pSphere">The sphere to whose distance the return value is.</param>
        /// <returns></returns>
        private double DistanceBetweenSpheres(Sphere pSphere)
        {
            double x = mPosition.X - pSphere.mPosition.X;
            double y = mPosition.Y - pSphere.mPosition.Y;
            double z = mPosition.Z - pSphere.mPosition.Z;
            double distance = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));

            // adjusts the distance to ignore the radii of the sphere.
            double distanceToSphere = distance - (mRadius + pSphere.mRadius);
            return distanceToSphere;
        }
        /// <summary>
        /// Distance between a sphere and a cylinder's closest line segment. adjusted for radii of both.
        /// </summary>
        /// <param name="pCylinder"></param>
        /// <returns></returns>
        public double distanceToCylinder(Cylinder pCylinder)
        {
            // Line segment method
            Vector3 Hyp = (mPosition - pCylinder.mCylinderBottom); // The vector from the cylinder bottom to the sphere position. Forms the hypotenuse in right triangle.
            Vector3 AdjNormalized = Vector3.Normalize(pCylinder.mCylinderTop - pCylinder.mCylinderBottom); // The vector direction from the cylinder bottom to point of collision. Forms the adjacent in right triangle.

            double theta = Math.Acos(Vector3.Dot(AdjNormalized, Hyp) / Hyp.Length);
            double oppositeDistance = Math.Sin(theta) * Hyp.Length;

            return oppositeDistance - (mRadius + pCylinder.mRadius);
        }
        /// <summary>
        /// Moves the sphere to the top box (emitter box) of the parameter cube. 
        /// Adjusts the random location by the size of the cube and radius of sphere.
        /// </summary>
        /// <param name="pCube">The cube whos top box is the emitter box.</param>
        /// <param name="keepVelocity">Whether the sphere will keep its velocity after being translated .</param>
        public void MoveToEmitterBox(Cube pCube, bool keepVelocity)
        {
            // The positions in coordinate space the sphere will be translated to.
            float x;
            float y;
            float z;

            while (true)
            {
                // Random x y and z positions created adjusted for cube and sphere size
                x = NextFloat(pCube.mPosition.X - pCube.cubeDimensions.X + mRadius, // min / left
                    pCube.mPosition.X + (pCube.cubeDimensions.X) - mRadius); // max / right

                y = NextFloat((2.0f - 0.96f), // min
                    2.0f - 0.04f); // max

                z = NextFloat(pCube.mPosition.Z - pCube.cubeDimensions.Z + mRadius, // min / outside
                    pCube.mPosition.Z + pCube.cubeDimensions.Z - mRadius); // max / inside

                // If the position doesnt cause a collision on spawn with any other sphere in the sphere list.
                if (!checkPositionForCollisionSphere(new Vector3(x, y, z)))
                    break;
            }

            mPosition = new Vector3(x, y, z);

            if (!keepVelocity)
                mVelocity = new Vector3(0.0f, 0.0f, 0.0f); // reset velocity
        }


        // todo fix next two methods.
        /// <summary>
        /// Creates a temporary sphere to test for a collision with a sphere at the parameter position.
        /// </summary>
        /// <param name="pPosition"></param>
        /// <returns></returns>
        private bool checkPositionForCollisionSphere(Vector3 pPosition)
        {
            // sphere created to test for a collision with the same radius as this sphere and the parameter position.
            Sphere testSphere = new Sphere(pPosition, mRadius, true, SphereType.test);

            // Test every sphere for a collision with the test sphere. 
            foreach (entity e in EntityManager.AllObjects)
            {
                if (e is Sphere) // check all spheres
                    if (testSphere.hasCollidedWithSphere((Sphere)e))
                    {
                        return true;
                    }
            }
            return false;
        }
        /// <summary>
        /// Creates a temporary sphere to test for a collision with a cylinder at the parameter position.
        /// </summary>
        /// <param name="pPosition"></param>
        /// <returns></returns>
        private bool checkPositionForCollisionCyl(Vector3 pPosition)
        {
            // sphere created to test for a collision with the same radius as this sphere and the parameter position.
            Sphere testSphere = new Sphere(pPosition, mRadius, true, SphereType.test);

            // Test every sphere for a collision with the test sphere. 
            foreach (entity e in EntityManager.AllObjects)
            {
                if (testSphere.hasCollidedWithCylinder((Cylinder)e))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Scales the sphere by a uniform paramater in the x, y and z plane.
        /// </summary>
        /// <param name="pScale"></param>

        #region collision detection and response

        // COLLISION DETECTION
        /// <summary>
        /// Checks if the sphere has collided with the parameter cube on the inside. 
        /// Performs detection and response to save creating 6 seperate side methods.
        /// </summary>
        /// <param name="pCube"></param>
        public void hasCollidedWithCube(Cube pCube)
        {
            // X PLANE
            if ((mPosition.X + mRadius > (pCube.mPosition.X + (pCube.cubeDimensions.X))))
            {// right collision
                // only perform collision response if the direction of velocity is same sign as normal of cube
                if (mVelocity.X > 0)
                {
                    Vector3 normal = new Vector3(1, 0, 0);
                    mVelocity = mVelocity - (1 + ACWWindow.restitution) * Vector3.Dot(normal, mVelocity) * normal;
                }
            }
            if ((mPosition.X - mRadius < (pCube.mPosition.X - (pCube.cubeDimensions.X)))) // Left inside of pCube
            { // left collision
                if (mVelocity.X < 0)
                {
                    Vector3 normal = new Vector3(-1, 0, 0);
                    mVelocity = mVelocity - (1 + ACWWindow.restitution) * Vector3.Dot(normal, mVelocity) * normal;
                }
            }
            // Y PLANE
            if ((mPosition.Y + mRadius > (pCube.mPosition.Y + (pCube.cubeDimensions.Y))))
            { // top collision
                if (mVelocity.Y > 0)
                {
                    Vector3 normal = new Vector3(0, 1, 0);
                    mVelocity = mVelocity - (1 + ACWWindow.restitution) * Vector3.Dot(normal, mVelocity) * normal;
                }
            }
            if ((mPosition.Y - mRadius) < (pCube.mPosition.Y - (pCube.cubeDimensions.Y)))
            { // bottom collision
                if (mVelocity.Y < 0)
                {
                    MoveToEmitterBox(pCube, false);


                    // Original response to collision with inside bottom of cube
                    // improv. fix spheres sinking through - thread.sleep
                    //Vector3 normal = new Vector3(0, -1, 0);
                    //mVelocity = mVelocity - (1 + ACWWindow.restitution) * Vector3.Dot(normal, mVelocity) * normal;
                }
            }
            // Z PLANE
            if ((mPosition.Z + mRadius > (pCube.mPosition.Z + (pCube.cubeDimensions.Z))))
            { // inside collision
                if (mVelocity.Z > 0)
                {
                    Vector3 normal = new Vector3(0, 0, 1);
                    mVelocity = mVelocity - (1 + ACWWindow.restitution) * Vector3.Dot(normal, mVelocity) * normal;
                }
            }
            if ((mPosition.Z - mRadius) < (pCube.mPosition.Z - (pCube.cubeDimensions.Z)))
            { // outside collision
                if (mVelocity.Z < 0)
                {
                    Vector3 normal = new Vector3(0, 0, -1);
                    mVelocity = mVelocity - (1 + ACWWindow.restitution) * Vector3.Dot(normal, mVelocity) * normal;
                }
            }
        }
        /// <summary>
        /// Collision detection for sphere on sphere.
        /// </summary>
        /// <param name="pSphere">The sphere to check against.</param>
        /// <returns></returns>
        public bool hasCollidedWithSphere(Sphere pSphere)
        {
            // If the spheres intersect
            if (DistanceBetweenSpheres(pSphere) < 0)
            {
                Console.WriteLine("sphere on sphere collision");
                return true;
            }
            return false;
        }
        /// <summary>
        /// Uses the line segment of the cylinder to test if the sphere collided with the 
        /// cylinder. Performs the collision response.
        /// </summary>
        /// <param name="pCylinder">Cylinder whose line segment is used to test for a collision.</param>
        public bool hasCollidedWithCylinder(Cylinder pCylinder)
        {
            // Line segment method
            Vector3 Hyp = (mPosition - pCylinder.mCylinderBottom); // The vector from the cylinder bottom to the sphere position. Forms the hypotenuse in right triangle.
            Vector3 AdjNormalized = Vector3.Normalize(pCylinder.mCylinderTop - pCylinder.mCylinderBottom); // The vector direction from the cylinder bottom to point of collision. Forms the adjacent in right triangle.



            double theta = Math.Acos(Vector3.Dot(AdjNormalized, Hyp) / Hyp.Length);
            double oppositeDistance = Hyp.Length * Math.Sin(theta);

            //Console.WriteLine("Distance between sphere and cylinder: " + distanceToCylinder(pCylinder));
            //Console.WriteLine(distanceToCylinder(pCylinder) < 0);

            if (distanceToCylinder(pCylinder) < 0)
            {
                float AdjLength = (Hyp.Length * (float)(Math.Cos(theta)));
                // Vector3 Opp = Adj - Hyp;
                Vector3 Opp = Hyp - AdjLength * (pCylinder.mCylinderTop - pCylinder.mCylinderBottom).Normalized();

                Vector3 normal = Opp.Normalized(); // check if this is away from collision point

                // Console.WriteLine("Distance between sphere and cylinder: " + distanceToCylinder(pCylinder));
                if (Vector3.Dot(normal, mVelocity) < 0) // check if the new velocity is in the direction of point of collision.
                {
                    //Console.WriteLine("Distance between sphere and cylinder before response " + distanceToCylinder(pCylinder));
                    Console.WriteLine("sphere on cylinder collision");
                    SphereOnCylinderResponse(normal);

                    //Console.WriteLine("Distance between sphere and cylinder after response " + distanceToCylinder(pCylinder));
                    return true;
                }


            }
            return false;
        }

        // COLLISION RESPONSE.
        /// <summary>
        /// Uses the positions of the cylinder and sphere to calculate a collision response 
        /// of the sphere with a static cylinder. 
        /// </summary>
        /// <param name="pAdjacentNormalized">The normalized vector of the adjacent. 
        /// Normal of vector from the cylinder bottom to collision point.</param>
        /// <param name="pHypotenuse">Vector from the cylinder bottom to the sphere center.</param>
        /// <param name="ptheta">The angle between the two previous vectors.</param>
        private void SphereOnCylinderResponse(Vector3 normal)
        {
            // Set new velocity
            mVelocity = mVelocity - (1 + ACWWindow.restitution) * Vector3.Dot(normal, mVelocity) * normal;
            // Move sphere back to previous position.
            mPosition = lastPosition;
        }
        /// <summary>
        /// The collision response for two moving spheres.
        /// </summary>
        /// <param name="pSphere">Sphere to test position against.</param>
        public void sphereOnSphereResponse(Sphere pSphere)
        {
            //Vector3 circle1Momentumbefore = mCircleMass * mCircleVelocity;
            //Vector3 circle2Momentumbefore = mCircleMass2 * mCircleVelocity2;
            //Vector3 totalmomentumbefore = circle1Momentumbefore + circle2Momentumbefore;

            Vector3 OriginalVelocity = mVelocity;

            mVelocity = ((mMass * mVelocity) + (pSphere.mMass * pSphere.mVelocity) + (ACWWindow.restitution * pSphere.mMass * (pSphere.mVelocity - mVelocity))) / (mMass + pSphere.mMass);
            pSphere.mVelocity = ((pSphere.mMass * pSphere.mVelocity) + (mMass * OriginalVelocity) + (ACWWindow.restitution * mMass * (OriginalVelocity - pSphere.mVelocity))) / (pSphere.mMass + mMass);

            Vector3 positionBackup = mPosition;

            mPosition = lastPosition;
            pSphere.mPosition = pSphere.lastPosition;

            //Vector3 circle1Momentumafter = mCircleMass * mCircleVelocity;
            //Vector3 circle2Momentumafter = mCircleMass2 * mCircleVelocity2;
            //Vector3 totalmomentumafter = circle1Momentumafter + circle2Momentumafter;
        }

        /// <summary>
        /// Does not affect the trajectory of the sphere, the intersection of the sphere with the doom sphere is the amount
        /// by which the sphere is scaled down. If falls through doom sphere it is destroyed.
        /// </summary>
        public void SphereOnDoomSphereResponse(Sphere pDoomSphere)
        {
            double overlap = Math.Abs(DistanceBetweenSpheres(pDoomSphere));

            SetRadius(mRadius - (float)overlap);

            //mPosition = lastPosition;

            // translate away by the overlap distance in the direction of the normal from the 
            // point of collision vector (from sphere center to doomsphere).

            // add the collision detection from sphere on sphere to find the distance between the two centers and the 
            //proportion of overlap dictates how much to scale the sphere down by.
        }
        // improv. could caluclate the normal of the cube if check which plane is closest in vector (mposition - pCube.mposition) - closest value out of X Y Z plane
        // note. better to leave seperated otherwise create 6 different face collision response methods. consider a physics class to handle collisions.
        /// <summary>
        /// The response of a sphere to a collision with a static cube.
        /// </summary>
        public void SphereOnCubeResponse()
        {
            // example collision response for the top inside of the cube.

            // could check here if the object is static or moving to base response off of.
            Vector3 normal = new Vector3(1, 0, 0);
            mVelocity = mVelocity - (1 + ACWWindow.restitution) * Vector3.Dot(normal, mVelocity) * normal;
        }
        /// <summary>
        /// Check if a sphere in the parameter position collided with any spheres other than itself.
        /// </summary>
        /// <param name="pPosition"></param>




        #endregion
    }
}
