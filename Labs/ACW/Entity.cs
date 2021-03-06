﻿using System;
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

        public float mVolume;
        public float mDensity;
        public float mMass;

        public Matrix4 mMatrix;
        public Vector3 mPosition;
        // todo: make this private and create a set velocity method that caps the velocity at terminal velocity in positive and negative directions
        public Vector3 mVelocity;
        public Material mMaterial;
        /// <summary>
        /// The amount of time in seconds before this entity is removed from the simulation. Used in particles.
        /// </summary>
        public double mLifetime;

        /// <summary>
        /// The manager this entity is managed by at this time.
        /// </summary>
        public EntityManager mManager;
        /// <summary>
        /// The key used by entity managers to identify this entity instance.
        /// </summary>
        public int mEntityKey;

        /// <summary>
        /// If the object is static then it is not updated.
        /// </summary>
        public bool staticObject;

        abstract public void Load();
        abstract public void Render();
        abstract public void Update();
    }
}
