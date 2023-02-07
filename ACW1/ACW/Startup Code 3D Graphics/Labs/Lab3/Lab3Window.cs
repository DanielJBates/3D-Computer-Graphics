using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;

namespace Labs.Lab3
{
    public class Lab3Window : GameWindow
    {
        public Lab3Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 3 Lighting and Material Properties",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        private int[] mVBO_IDs = new int[5];
        private int[] mVAO_IDs = new int[3];
        private ShaderUtility mShader;
        private ModelUtility mMonsterModelUtility, mCylinderModelUtility;
        private Matrix4 mView, mMonsterModel, mCylinderModel, mGroundModel;

        protected override void OnLoad(EventArgs e)
        {
            // Set some GL state
            GL.ClearColor(Color4.CadetBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            mShader = new ShaderUtility(@"Lab3/Shaders/vPassThrough.vert", @"Lab3/Shaders/fLighting.frag");
            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vNormalLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");
            int uEyeLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            Vector4 eyePosition = new Vector4(2, 1, -8.5f, 1);
            GL.Uniform4(uEyeLocation, ref eyePosition);

            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].Position"); 
            Vector4 lightPosition = new Vector4(2, 4, -8.5f, 1); 
            lightPosition = Vector4.Transform(lightPosition, mView); 
            GL.Uniform4(uLightPositionLocation, lightPosition);
            
            int uAmbientLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].AmbientLight"); 
            Vector3 colour = new Vector3(1.0f, 0f, 0f); 
            GL.Uniform3(uAmbientLightLocation, colour);

            int uDiffuseLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].DiffuseLight");
            GL.Uniform3(uDiffuseLightLocation, colour);

            int uSpecularLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].SpecularLight");
            GL.Uniform3(uSpecularLightLocation, colour);

            uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].Position");
            lightPosition = Vector4.Transform(lightPosition, mView);
            GL.Uniform4(uLightPositionLocation, lightPosition);

            uAmbientLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].AmbientLight");
            colour = new Vector3(0f, 1.0f, 0f);
            GL.Uniform3(uAmbientLightLocation, colour);

            uDiffuseLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].DiffuseLight");
            GL.Uniform3(uDiffuseLightLocation, colour);

            uSpecularLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].SpecularLight");
            GL.Uniform3(uSpecularLightLocation, colour);

            uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].Position");
            lightPosition = Vector4.Transform(lightPosition, mView);
            GL.Uniform4(uLightPositionLocation, lightPosition);

            uAmbientLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].AmbientLight");
            colour = new Vector3(0f, 0f, 1.0f);
            GL.Uniform3(uAmbientLightLocation, colour);

            uDiffuseLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].DiffuseLight");
            GL.Uniform3(uDiffuseLightLocation, colour);

            uSpecularLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].SpecularLight");
            GL.Uniform3(uSpecularLightLocation, colour);

            GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);

            float[] vertices = new float[] {-10, 0, -10,0,1,0,
                                             -10, 0, 10,0,1,0,
                                             10, 0, 10,0,1,0,
                                             10, 0, -10,0,1,0};

            GL.BindVertexArray(mVAO_IDs[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            mCylinderModelUtility = ModelUtility.LoadModel(@"Utility/Models/cylinder.bin");

            GL.BindVertexArray(mVAO_IDs[1]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[1]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mCylinderModelUtility.Vertices.Length * sizeof(float)), mCylinderModelUtility.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[2]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mCylinderModelUtility.Indices.Length * sizeof(float)), mCylinderModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCylinderModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCylinderModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            mMonsterModelUtility = ModelUtility.LoadModel(@"Utility/Models/model.bin");

            GL.BindVertexArray(mVAO_IDs[2]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[3]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mMonsterModelUtility.Vertices.Length * sizeof(float)), mMonsterModelUtility.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[4]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mMonsterModelUtility.Indices.Length * sizeof(float)), mMonsterModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mMonsterModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mMonsterModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.BindVertexArray(0);

            mView = Matrix4.CreateTranslation(0, -1.5f, 0);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);

            mGroundModel = Matrix4.CreateTranslation(0, 0, -5f);
            mCylinderModel = Matrix4.CreateTranslation(0, 1, -5f);
            mMonsterModel = Matrix4.CreateRotationY(-1.5f) * Matrix4.CreateTranslation(0, 3, -5f);

            base.OnLoad(e);
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

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar == 'w')
            {
                mView = mView * Matrix4.CreateTranslation(0.0f, 0.0f, 0.05f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);

                int uEyeLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = new Vector4(2, 1, -8.5f, 1);
                GL.Uniform4(uEyeLocation, ref eyePosition);

                for (int i = 0; i < 3; i++)
                {
                    int uLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[i].Position");
                    Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                    GL.Uniform4(uLightLocation, lightPosition);
                }
            }
            if (e.KeyChar == 's')
            {
                mView = mView * Matrix4.CreateTranslation(0.0f, 0.0f, -0.05f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);

                int uEyeLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = new Vector4(2, 1, -8.5f, 1);
                GL.Uniform4(uEyeLocation, ref eyePosition);

                for (int i = 0; i < 3; i++)
                {
                    int uLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[i].Position");
                    Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                    GL.Uniform4(uLightLocation, lightPosition);
                }
            }
            if (e.KeyChar == 'a')
            {
                mView = mView * Matrix4.CreateRotationY(-0.025f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);

                int uEyeLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = new Vector4(2, 1, -8.5f, 1);
                GL.Uniform4(uEyeLocation, ref eyePosition);

                for (int i = 0; i < 3; i++)
                {
                    int uLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[i].Position");
                    Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                    GL.Uniform4(uLightLocation, lightPosition);
                }
            }
            if (e.KeyChar == 'd')
            {
                mView = mView * Matrix4.CreateRotationY(0.025f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);

                int uEyeLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = new Vector4(2, 1, -8.5f, 1);
                GL.Uniform4(uEyeLocation, ref eyePosition);

                for (int i = 0; i < 3; i++)
                {
                    int uLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[i].Position");
                    Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -8.5f, 1), mView);
                    GL.Uniform4(uLightLocation, lightPosition);
                }

            }
            if (e.KeyChar == 'z')
            {
                Vector3 t = mGroundModel.ExtractTranslation(); 
                Matrix4 translation = Matrix4.CreateTranslation(t); 
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t); 
                mGroundModel = mGroundModel * inverseTranslation * Matrix4.CreateRotationY(-0.025f) * translation;
            }
            if (e.KeyChar == 'x')
            {
                Vector3 t = mGroundModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mGroundModel = mGroundModel * inverseTranslation * Matrix4.CreateRotationY(0.025f) * translation;
            }
            if (e.KeyChar == 'c')
            {
                Vector3 t = mMonsterModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mMonsterModel = mMonsterModel * inverseTranslation * Matrix4.CreateRotationY(-0.025f) * translation;
            }
            if (e.KeyChar == 'v')
            {
                Vector3 t = mMonsterModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mMonsterModel = mMonsterModel * inverseTranslation * Matrix4.CreateRotationY(0.025f) * translation;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            int uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref mGroundModel);

            updateMaterial(new Vector3(0.05f, 0.05f, 0.05f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.7f, 0.7f, 0.7f), 0.078125f);

            GL.BindVertexArray(mVAO_IDs[0]);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            Matrix4 m = mCylinderModel * mGroundModel;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m);

            updateMaterial(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.5f, 0.0f, 0.0f), new Vector3(0.7f, 0.6f, 0.6f), 0.25f);

            GL.BindVertexArray(mVAO_IDs[1]);
            GL.DrawElements(PrimitiveType.Triangles, mCylinderModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            m = mMonsterModel * mGroundModel;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m);

            updateMaterial(new Vector3(0.24725f, 0.1995f, 0.0745f), new Vector3(0.75164f, 0.60648f, 0.22648f), new Vector3(0.628281f, 0.555802f, 0.366065f), 0.4f);

            GL.BindVertexArray(mVAO_IDs[2]);
            GL.DrawElements(PrimitiveType.Triangles, mMonsterModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

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

        private void updateMaterial(Vector3 pColour1, Vector3 pColour2, Vector3 pColour3, float pShininess)
        { 
            int uAmbientReflectivity = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.AmbientReflectivity");
            GL.Uniform3(uAmbientReflectivity, pColour1);
            int uDiffuseReflectivity = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.DiffuseReflectivity");
            GL.Uniform3(uDiffuseReflectivity, pColour2);
            int uSpecularReflectivity = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.SpecularReflectivity");
            GL.Uniform3(uSpecularReflectivity, pColour3);
            int uShininess = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.Shininess");
            GL.Uniform1(uShininess, pShininess);
        }
        private void updateLightColour(int pIndex, Vector3 pColour1, Vector3 pColour2, Vector3 pColour3)
        {
            int uAmbientLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[" + pIndex + "].AmbientLight");
            GL.Uniform3(uAmbientLightLocation, pColour1);

            int uDiffuseLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[" + pIndex + "].DiffuseLight");
            GL.Uniform3(uDiffuseLightLocation, pColour2);

            int uSpecularLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[" + pIndex + "].SpecularLight");
            GL.Uniform3(uSpecularLightLocation, pColour3);
        }
        private void updateLightLocation(int pIndex, Vector4 pNewLocation)
        {
            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[" + pIndex + "].Position");
            Vector4 lightPosition = pNewLocation;
            lightPosition = Vector4.Transform(lightPosition, mView);
            GL.Uniform4(uLightPositionLocation, lightPosition);
        }

    }
}