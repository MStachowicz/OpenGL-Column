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
    public class Material
    {
        public Material(Vector3 pAmbientReflectivity, Vector3 pDiffuseReflectivity, Vector3 pSpecularReflectivity, float pShininess)
        {
            AmbientReflectivity = pAmbientReflectivity;
            DiffuseReflectivity = pDiffuseReflectivity;
            SpecularReflectivity = pSpecularReflectivity;
            Shininess = pShininess;
        }

        /// <summary>
        /// Set an RGB value for the material.
        /// </summary>
        /// <param name="RGB">The components of red green and blue.</param>
        /// <param name="pIntensity">The intensity all the components will be multiplied by.</param>
        /// <param name="pShininess">The shininess of the material.</param>
        public Material(Vector3 RGB, float pIntensity ,float pShininess)
        {
            AmbientReflectivity = new Vector3(RGB.X * pIntensity, RGB.Y * pIntensity, RGB.Z * pIntensity);
            DiffuseReflectivity = new Vector3(RGB.X * pIntensity, RGB.Y * pIntensity, RGB.Z * pIntensity);
            SpecularReflectivity = new Vector3(RGB.X * pIntensity, RGB.Y * pIntensity, RGB.Z * pIntensity);
            Shininess = pShininess;
        }

        private Vector3 AmbientReflectivity;
        private Vector3 DiffuseReflectivity;
        private Vector3 SpecularReflectivity;
        private float Shininess;

        /// <summary>
        /// Sets the material values in the shader.
        /// </summary>
        public void SetMaterial()
        {
            int uAmbientReflectivityLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uMaterial.AmbientReflectivity");
            GL.Uniform3(uAmbientReflectivityLocation, AmbientReflectivity);

            int uDiffuseReflectivityLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uMaterial.DiffuseReflectivity");
            GL.Uniform3(uDiffuseReflectivityLocation, DiffuseReflectivity);

            int uSpecularReflectivityLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uMaterial.SpecularReflectivity");
            GL.Uniform3(uSpecularReflectivityLocation, SpecularReflectivity);

            int uShininessLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uMaterial.Shininess");
            GL.Uniform1(uShininessLocation, Shininess * 128);

            ACWWindow.materialSet = this;
        }


        public static Material gold = new Material(new Vector3(0.24725f, 0.1995f, 0.0745f), new Vector3(0.75164f, 0.60648f, 0.22648f), new Vector3(0.628281f, 0.555802f, 0.366065f), 0.4f);
        public static Material emerald = new Material(new Vector3(0.0215f, 0.1745f, 0.0215f), new Vector3(0.07568f, 0.61424f, 0.07568f), new Vector3(0.633f, 0.727811f, 0.633f), 0.6f);
        public static Material chrome = new Material(new Vector3(0.25f, 0.25f, 0.25f), new Vector3(0.4f, 0.4f, 0.4f), new Vector3(0.774597f, 0.774597f, 0.774597f), 0.6f);
        public static Material silver = new Material(new Vector3(0.19225f, 0.19225f, 0.19225f), new Vector3(0.50754f, 0.50754f, 0.50754f), new Vector3(0.508273f, 0.508273f, 0.508273f), 0.4f);
        public static Material pearl = new Material(new Vector3(0.25f, 0.20725f, 0.20725f), new Vector3(1f, 0.829f, 0.829f), new Vector3(0.296648f, 0.296648f, 0.296648f), 0.088f);
    }
}
