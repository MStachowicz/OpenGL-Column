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
        /// Constructor used to create sphere of doom and test sphere in collision response/detection.
        /// </summary>
        /// <param name="pPosition">Position of the sphere</param>
        /// <param name="pRadius">The radius of the sphere in m. (1 = 1m)</param>
        /// <param name="pStaticSphere">Is this sphere static.</param>
        /// <param name="pAddToList">Should this sphere be added to the static sphere list..</param>
        public Sphere(Vector3 pPosition, float pRadius, bool pStaticSphere, bool pAddToList)
        {
            mScaleX = pRadius;
            mScaleY = pRadius;
            mScaleZ = pRadius;

            mPosition = pPosition;

            staticObject = pStaticSphere;

            if (!pStaticSphere)
                mVelocity = new Vector3(1, 1, 1); // only give velocity if the sphere is not static
            if (pAddToList)
            sphereList.Add(this);
            
        }
        /// <summary>
        /// Creates a ball in the top cube of the scene in a random position with a random velocity.
        /// </summary>
        /// <param name="pCube">The cube whos top box all the spheres will be translated to.</param>
        public Sphere(Cube pCube)
        {
            // SPHERE PROPERTIES

            mVelocity = new Vector3(0, 0, 0);

            #region Translation

            // Set all the spheres to random locations inside emitter box.
            MoveToEmitterBox(pCube, false);

            #endregion

            #region Scale + mass for 2 different balls

            if (!changeBallType)
            { // 4cm radius ball
                mScaleX = 0.04f;
                mScaleY = 0.04f;
                mScaleZ = 0.04f;

                mRadius = 1.0f * mScaleX; // radius = 4 cm = 0.04m
                mVolume = (float)((4 / 3) * Math.PI * Math.Pow(mRadius, 3));
                mDensity = 1400f; // density =   0.001 kg/cm^3 = 1400 kg/m^3
                mMass = mDensity * mVolume;
            }
            else
            {// 8cm radius ball
                mScaleX = 0.08f;
                mScaleY = 0.08f;
                mScaleZ = 0.08f;

                mRadius = 1.0f * mScaleX; // radius = 8 cm = 0.08m
                mVolume = (float)((4 / 3) * Math.PI * Math.Pow(mRadius, 3));
                mDensity = 0.001f;
                mMass = mDensity * mVolume;
            }

            #endregion

            mRotationX = 1.0f;
            mRotationY = 1.0f;
            mRotationZ = 1.0f;

            staticObject = false;
            sphereList.Add(this);

            // Next sphere instantiated will be of the other sphere type.
            changeBallType ^= true;
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
        /// <summary>
        /// Stores all the instances of the sphere that currently exist.
        /// </summary>
        private static List<Sphere> sphereList = new List<Sphere>();

        public override void Load()
        {
            if (!isLoaded)
            {
                VAOIndex = entityManager.VAOCount++; // set the VAO index all spheres will use.
                VBOIndex = entityManager.VBOCount++; // set the VBO index all spheres will use.
                entityManager.VBOCount++;
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
            mMatrix = Matrix4.CreateScale(mScaleX, mScaleY, mScaleZ) * Matrix4.CreateTranslation(mPosition);

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
            double x = mPosition.X - pSphere.mPosition.X;
            double y = mPosition.Y - pSphere.mPosition.Y;
            double z = mPosition.Z - pSphere.mPosition.Z;
            double distance = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));

            if (distance < mRadius + pSphere.mRadius)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Uses the line segment of the cylinder to test if the sphere collided with the cylinder.
        /// </summary>
        /// <param name="pCylinder">Cylinder whose line segment is used to test for a collision.</param>
        public void hasCollidedWithCylinder(Cylinder pCylinder)
        {
            // Line segment method
            Vector3 Hyp = (mPosition - pCylinder.mCylinderTop); // The vector from the cylinder bottom to the sphere position. Forms the hypotenuse in right triangle.
            Vector3 AdjNormalized = Vector3.Normalize(pCylinder.mCylinderBottom - pCylinder.mCylinderTop); // The vector direction from the cylinder bottom to point of collision. Forms the adjacent in right triangle.

            float adotb = Vector3.Dot(AdjNormalized, Hyp);

            /// <summary>
            /// theta
            /// </summary>
            double theta = Math.Acos(adotb / Hyp.Length);
            double oppositeDistance = Math.Sin(theta) * Hyp.Length;


            if (oppositeDistance < mRadius + pCylinder.mRadius)
            {
                SphereOnCylinderResponse(AdjNormalized, Hyp, theta);
            }
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
        public void SphereOnCylinderResponse(Vector3 pAdjacentNormalized, Vector3 pHypotenuse, double ptheta)
        {
            Vector3 Adj = pAdjacentNormalized * (pHypotenuse * (float)(Math.Cos(ptheta)));
            Vector3 Opp = Adj - pHypotenuse;

            Vector3 normal = -Opp.Normalized();
            Vector3 velocityBefore = mVelocity;

            if (Vector3.Dot(normal, mVelocity) < 0)
            {
                mVelocity = mVelocity - (1 + ACWWindow.restitution) * Vector3.Dot(normal, mVelocity) * normal;
                mPosition = lastPosition;
                Console.WriteLine("Sphere on cylinder collision detected " + ACWWindow.CollisionCount++);
            }
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

            Console.WriteLine("Sphere on sphere collision detected " + ACWWindow.CollisionCount++);

            Vector3 OriginalVelocity = mVelocity;

            mVelocity = ((mMass * mVelocity) + (pSphere.mMass * pSphere.mVelocity) + (ACWWindow.restitution * pSphere.mMass * (pSphere.mVelocity - mVelocity))) / (mMass + pSphere.mMass);
            pSphere.mVelocity = ((pSphere.mMass * pSphere.mVelocity) + (mMass * OriginalVelocity) + (ACWWindow.restitution * mMass * (OriginalVelocity - pSphere.mVelocity))) / (pSphere.mMass + mMass);

            mPosition = lastPosition;
            pSphere.mPosition = pSphere.lastPosition;

            //Vector3 circle1Momentumafter = mCircleMass * mCircleVelocity;
            //Vector3 circle2Momentumafter = mCircleMass2 * mCircleVelocity2;
            //Vector3 totalmomentumafter = circle1Momentumafter + circle2Momentumafter;
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


        // todo: Should check if there is a sphere already in position
        /// <summary>
        /// Moves the sphere to the top box (emitter box) of the parameter cube. 
        /// Adjusts the random location by the size of the cube and radius of sphere.
        /// </summary>
        /// <param name="pCube">The cube whos top box is the emitter box.</param>
        /// <param name="keepVelocity">Whether the sphere will keep its velocity after being translated .</param>
        public void MoveToEmitterBox(Cube pCube, bool keepVelocity)
        {
            // Random x y and z positions created adjusted for cube and sphere size
            float x = NextFloat(pCube.mPosition.X + (pCube.cubeDimensions.X), pCube.mPosition.X - (pCube.cubeDimensions.X));
            float y = NextFloat((2.0f - 0.96f), 2.0f - 0.04f);
            float z = NextFloat(pCube.mPosition.Z + (pCube.cubeDimensions.Z), pCube.mPosition.Z - (pCube.cubeDimensions.Z));

            mPosition = new Vector3(x, y, z);

            if (!keepVelocity)
                mVelocity = new Vector3(0.0f, 0.0f, 0.0f); // reset velocity
        }

        /// <summary>
        /// Check if a sphere in the parameter position collided with any spheres other than itself.
        /// </summary>
        /// <param name="pPosition"></param>
        private bool checkPositionForCollision(Vector3 pPosition)
        {
            // sphere created to test for a collision with the same radius as this sphere and the parameter position.
            Sphere testSphere = new Sphere(pPosition, mRadius, true, false);

            // Test every sphere for a collision with the test sphere. 
            foreach (Sphere s in sphereList)
            {
                if (testSphere.hasCollidedWithSphere(s))
                    return true;
            }

            return false;
        }
        #endregion
    }

}
