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
        }
    }
}
