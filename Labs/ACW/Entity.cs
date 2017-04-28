using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Labs.Utility;


namespace Labs.ACW
{
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

        public Vector3 mPosition;

        public float mVolume;
        public float mDensity;
        public float mMass;

        public Matrix4 mMatrix;
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
}
