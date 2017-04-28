using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Labs.Utility;

namespace Labs.ACW
{
    public class Sphere : entity
    {
        public Sphere(Vector3 pPosition)
        {
            VAOIndex = entityManager.VAOCount++;
            VBOIndex = entityManager.VBOCount++;
            entityManager.VBOCount++;

            mScaleX = 0.1f;
            mScaleY = 0.1f;
            mScaleZ = 0.1f;

            mPosition = pPosition;
            mVelocity = new Vector3(0, 0, 0);
        }

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

            mVelocity = new Vector3(0, 0, 0);

            #region Translation

            float x = NextFloat(pCube.mPosition.X + (pCube.cubeDimensions.X), pCube.mPosition.X - (pCube.cubeDimensions.X));
            float y = NextFloat((2.0f - 0.96f), 2.0f - 0.04f);
            float z = NextFloat(pCube.mPosition.Z + (pCube.cubeDimensions.Z), pCube.mPosition.Z - (pCube.cubeDimensions.Z));

            // Set all the spheres to random locations inside emitter box.
            mPosition = new Vector3(x, y, z);


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
        /// Moves the sphere to the top box (emitter box) of the scene.
        /// </summary>
        /// <param name="pCube">The cube whos top box is the emitter box.</param>
        public void MoveToEmitterBox(Cube pCube)
        {
            //float x = NextFloat(pCube.mPosition.X + (pCube.cubeDimensions.X), pCube.mPosition.X - (pCube.cubeDimensions.X));
            //float y = NextFloat((2.0f - 0.96f), 2.0f - 0.04f);
            //float z = NextFloat(pCube.mPosition.Z + (pCube.cubeDimensions.Z), pCube.mPosition.Z - (pCube.cubeDimensions.Z));

            // Set all the spheres to random locations inside cube
            mPosition = new Vector3(mPosition.X, pCube.cubeDimensions.Y, mPosition.Z);
            mVelocity = new Vector3(NextFloat(-2, 2), NextFloat(-2, 2), NextFloat(-2, 2));
            //mVelocity = new Vector3(0.0f, 0.0f, 0.0f);
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
            mMatrix = Matrix4.CreateScale(mScaleX, mScaleY, mScaleZ) * Matrix4.CreateTranslation(mPosition);

            int uModel = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uModel");

            Matrix4 moveToWorldSpace = mMatrix * ACWWindow.cubeSpace;
            GL.UniformMatrix4(uModel, true, ref moveToWorldSpace);

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
            //this.mVelocity
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


            if (distance < mRadius + pSphere.mRadius)
            {
                //Vector3 circle1Momentumbefore = mCircleMass * mCircleVelocity;
                //Vector3 circle2Momentumbefore = mCircleMass2 * mCircleVelocity2;
                //Vector3 totalmomentumbefore = circle1Momentumbefore + circle2Momentumbefore;

                Vector3 pointOfCollision = new Vector3((float)x / 2, (float)y / 2, (float)z / 2);

                Vector3 sphereToCollision = mPosition - pointOfCollision;
                Vector3 sphereToCollision2 = pSphere.mPosition - pointOfCollision;

                Vector3 OriginalVelocity = mVelocity;
                Vector3 OriginalVelocity2 = pSphere.mVelocity;

                ACWWindow.CollisionCount++;
                mVelocity = ((mMass * mVelocity) + (pSphere.mMass * pSphere.mVelocity) + (restitution * pSphere.mMass * (pSphere.mVelocity - mVelocity))) / (mMass + pSphere.mMass);
                pSphere.mVelocity = ((pSphere.mMass * pSphere.mVelocity) + (mMass * OriginalVelocity) + (restitution * mMass * (OriginalVelocity - pSphere.mVelocity))) / (pSphere.mMass + mMass);

                mPosition = lastPosition;
                pSphere.mPosition = pSphere.lastPosition;


                //Vector3 circle1Momentumafter = mCircleMass * mCircleVelocity;
                //Vector3 circle2Momentumafter = mCircleMass2 * mCircleVelocity2;
                //Vector3 totalmomentumafter = circle1Momentumafter + circle2Momentumafter;

                return true;
            }
            return false;
        }
        public void hasCollisedWithCylinder(Cylinder pCylinder)
        {
            float restitution = 1f;

            // Line segment method
            Vector3 endPoint1 = new Vector3(pCylinder.mPosition.X, pCylinder.mPosition.Y + 0.5f, pCylinder.mPosition.Z);
            Vector3 endPoint2 = new Vector3(pCylinder.mPosition.X, pCylinder.mPosition.Y - 0.5f, pCylinder.mPosition.Z);

            Vector3 test = pCylinder.CylinderTop;
            Vector3 test2 = pCylinder.CylinderBottom;

            endPoint1 = test;
            endPoint2 = test2;

            Vector3 Hyp = (mPosition - endPoint1); // green line
            Vector3 AdjNormalized = Vector3.Normalize(endPoint2 - endPoint1); // blue line normalized

            float adotb = Vector3.Dot(AdjNormalized, Hyp);
            double theta = Math.Acos(adotb / Hyp.Length);
            double oppositeDistance = Math.Sin(theta) * Hyp.Length;


            if (oppositeDistance < mRadius + pCylinder.mRadius)
            {
                Vector3 Adj = AdjNormalized * (Hyp * (float)(Math.Cos(theta)));
                Vector3 Opp = Adj - Hyp;

                Vector3 normal = -Opp.Normalized();
                Vector3 velocityBefore = mVelocity;



                if (Vector3.Dot(normal, mVelocity) < 0)
                {
                    mVelocity = mVelocity - (1 + restitution) * Vector3.Dot(normal, mVelocity) * normal;
                    Console.WriteLine("colllision" + ACWWindow.CollisionCount++);
                    mPosition = lastPosition;
                }

            }
        }
        #endregion
    }

}
