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

        public Material mMaterial;

        /// <summary>
        /// If the object is static then it is not updated.
        /// </summary>
        public bool staticObject;

        abstract public void Load();
        abstract public void Render();
        abstract public void Update(float pTimestep, Vector3 pGravity);
    }
}
