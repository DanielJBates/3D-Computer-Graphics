using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Labs.Utility;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Labs.ACW
{
    public class ACWWindow : GameWindow
    {
        public ACWWindow()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Assessed Coursework",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        private int[] mVBO_IDs = new int[7];
        private int[] mVAO_IDs = new int[4];
        private int[] mTexture_IDs = new int[2];
        private int[] mCubeIndices, mPyramidIndices;
        private float[] mCubeVertices, mPyramidVertices;
        private ShaderUtility mShader;
        private ModelUtility mModelMonster;
        private Matrix4 mView, mGroundMatrix, mWallMatrix, mCubeMatrix, mPyramidMatrix, mMonsterMatrix;
        private float angle = 0.005f;

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color4.CornflowerBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            string filepath = @"ACW/WoodFloor.jpg";
            Bitmap TextureBitmap;
            BitmapData TextureData;
            if (System.IO.File.Exists(filepath))
            {
                TextureBitmap = new Bitmap(filepath);
                TextureData = TextureBitmap.LockBits(new System.Drawing.Rectangle(0, 0, TextureBitmap.Width, TextureBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            }
            else
            {
                throw new Exception("Could not find file " + filepath);
            }

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.GenTextures(1, out mTexture_IDs[0]);
            GL.BindTexture(TextureTarget.Texture2D, mTexture_IDs[0]);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TextureData.Width, TextureData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, TextureData.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            TextureBitmap.UnlockBits(TextureData);

            filepath = @"ACW/BrickTexture.jpg";
            if (System.IO.File.Exists(filepath))
            {
                TextureBitmap = new Bitmap(filepath);
                TextureData = TextureBitmap.LockBits(new System.Drawing.Rectangle(0, 0, TextureBitmap.Width, TextureBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            }
            else
            {
                throw new Exception("Could not find file " + filepath);
            }

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.GenTextures(1, out mTexture_IDs[1]);
            GL.BindTexture(TextureTarget.Texture2D, mTexture_IDs[1]);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TextureData.Width, TextureData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, TextureData.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            TextureBitmap.UnlockBits(TextureData);

            mModelMonster = ModelUtility.LoadModel(@"Utility/Models/model.bin");

            mShader = new ShaderUtility(@"ACW/Shaders/vACW.vert", @"ACW/Shaders/fACW.frag");
            GL.UseProgram(mShader.ShaderProgramID);

            mView = Matrix4.CreateTranslation(0,-0.5f,3.5f);                       
            int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uViewLocation, true, ref mView);

            int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 5);
            GL.UniformMatrix4(uProjectionLocation, true, ref projection);

            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");

            int vNormalLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");

            int uEyeLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            Vector4 eyePosition = new Vector4(2, 1, -8.5f, 1);
            GL.Uniform4(uEyeLocation, ref eyePosition);

            int vTexCoords = GL.GetAttribLocation(mShader.ShaderProgramID, "vTexCoords");

            int uTextureSamplerLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uTextureSampler");
            GL.Uniform1(uTextureSamplerLocation, 0);
            int uTextureSampler2Location = GL.GetUniformLocation(mShader.ShaderProgramID, "uTextureSampler2");
            GL.Uniform1(uTextureSampler2Location, 1);

            #region Lighting
            int uLightPosition = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.Position");
            Vector4 lightPosition = new Vector4(2, 4, -6f, 1);
            lightPosition = Vector4.Transform(lightPosition, mView);
            GL.Uniform4(uLightPosition, lightPosition);

            int uAmbientLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.AmbientLight");
            Vector3 colour = new Vector3(1.0f, 1.0f, 1.0f);
            GL.Uniform3(uAmbientLightLocation, colour);

            int uDiffuseLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.DiffuseLight");
            GL.Uniform3(uDiffuseLightLocation, colour);

            int uSpecularLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.SpecularLight");
            GL.Uniform3(uSpecularLightLocation, colour);
            #endregion

            #region Plane
            GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);

            float[] planeVertices = new float[] {-10, 0, -10,0,1,0, 0f, 0f,
                                             -10, 0, 10,0,1,0, 0f, 1f,
                                             10, 0, 10,0,1,0, 1f, 1f,
                                             10, 0, -10,0,1,0, 1f, 0f};

            GL.BindVertexArray(mVAO_IDs[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(planeVertices.Length * sizeof(float)), planeVertices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(vTexCoords);
            GL.VertexAttribPointer(vTexCoords, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (planeVertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }
            #endregion

            #region Cube

            mCubeVertices = new float[]
            {
                -0.2f,-0.2f,-0.2f,0,1,0,
                0.2f,-0.2f,-0.2f,0,1,0,
                -0.2f,0.2f,-0.2f,0,1,0,
                0.2f,0.2f,-0.2f,0,1,0,
                -0.2f,-0.2f,0.2f,0,1,0,
                0.2f,-0.2f,0.2f,0,1,0,
                -0.2f,0.2f,0.2f,0,1,0,
                0.2f,0.2f,0.2f,0,1,0,
                0.2f,-0.2f,-0.2f,1,0,0,
                0.2f,-0.2f,0.2f,1,0,0,
                0.2f,0.2f,-0.2f,1,0,0,
                0.2f,0.2f,0.2f,1,0,0,
                -0.2f,-0.2f,-0.2f,1,0,0,
                -0.2f,-0.2f,0.2f,1,0,0,
                -0.2f,0.2f,-0.2f,1,0,0,
                -0.2f,0.2f,0.2f,1,0,0,
                -0.2f,-0.2f,-0.2f,0,0,1,
                -0.2f,-0.2f,0.2f,0,0,1,
                0.2f,-0.2f,-0.2f,0,0,1,
                0.2f,-0.2f,0.2f,0,0,1,
                -0.2f,0.2f,-0.2f,0,0,1,
                -0.2f,0.2f,0.2f,0,0,1,
                0.2f,0.2f,-0.2f,0,0,1,
                0.2f,0.2f,0.2f,0,0,1
            };
            mCubeIndices = new int[]
            {
                1,0,2,
                1,2,3,
                4,5,6,
                6,5,7,
                9,8,10,
                9,10,11,
                12,13,14,
                14,13,15,
                17,16,18,
                17,18,19,
                20,21,22,
                22,21,23
            };

            GL.BindVertexArray(mVAO_IDs[1]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[1]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mCubeVertices.Length * sizeof(float)), mCubeVertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[2]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mCubeIndices.Length * sizeof(float)), mCubeIndices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCubeVertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCubeIndices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            #endregion

            #region Pyramind

            mPyramidVertices = new float[]
            {
                0.0f,0.2f,0,1,0,0,
                -0.2f,-0.2f,-0.2f,1,0,0,
                0.2f,-0.2f,-0.2f,1,0,0,
                -0.2f,-0.2f,0.2f,1,0,0,
                0.2f,-0.2f,0.2f,1,0,0,
                0.0f,0.2f,0.0f,0,1,0,
                0.2f,-0.2f,-0.2f,0,1,0,
                0.2f,-0.2f,0.2f,0,1,0,
                -0.2f,-0.2f,-0.2f,0,1,0,
                -0.2f,-0.2f,0.2f,0,1,0,
                -0.2f,-0.2f,-0.2f,0,0,1,
                -0.2f,-0.2f,0.2f,0,0,1,
                0.2f,-0.2f,-0.2f,0,0,1,
                0.2f,-0.2f,0.2f,0,0,1
            };
            mPyramidIndices = new int[]
            {
                0,2,1,
                0,3,4,
                5,7,6,
                5,8,9,
                10,12,11,
                12,13,11
            };

            GL.BindVertexArray(mVAO_IDs[2]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[3]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mPyramidVertices.Length * sizeof(float)), mPyramidVertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[4]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mPyramidIndices.Length * sizeof(float)), mPyramidIndices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mPyramidVertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mPyramidIndices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            #endregion

            #region Model
            GL.BindVertexArray(mVAO_IDs[3]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[5]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mModelMonster.Vertices.Length * sizeof(float)), mModelMonster.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[6]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mModelMonster.Indices.Length * sizeof(float)), mModelMonster.Indices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mModelMonster.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mModelMonster.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            #endregion

            GL.BindVertexArray(0);

            base.OnLoad(e);
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            MoveCamera(e);
        }
        private void MoveCamera(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar == 'a')
            {                
                mView = mView * Matrix4.CreateTranslation(0.01f, 0, 0);
                int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uViewLocation, true, ref mView);

                int uEyeLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = new Vector4(2, 1, -8.5f, 1);
                GL.Uniform4(uEyeLocation, ref eyePosition);

                int uLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.Position");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -6f, 1), mView);
                GL.Uniform4(uLightLocation, lightPosition);
            }
            else if (e.KeyChar == 'd')
            {
                mView = mView * Matrix4.CreateTranslation(-0.01f, 0, 0);
                int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uViewLocation, true, ref mView);

                int uEyeLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = new Vector4(2, 1, -8.5f, 1);
                GL.Uniform4(uEyeLocation, ref eyePosition);

                int uLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.Position");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -6f, 1), mView);
                GL.Uniform4(uLightLocation, lightPosition);
            }
            else if (e.KeyChar == 'w')
            {
                mView = mView * Matrix4.CreateTranslation(0, 0, 0.01f);
                int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uViewLocation, true, ref mView);

                int uEyeLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = new Vector4(2, 1, -8.5f, 1);
                GL.Uniform4(uEyeLocation, ref eyePosition);

                int uLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.Position");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -6f, 1), mView);
                GL.Uniform4(uLightLocation, lightPosition);
            }
            else if (e.KeyChar == 's')
            {
                mView = mView * Matrix4.CreateTranslation(0, 0, -0.01f);
                int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uViewLocation, true, ref mView);

                int uEyeLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = new Vector4(2, 1, -8.5f, 1);
                GL.Uniform4(uEyeLocation, ref eyePosition);

                int uLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.Position");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -6f, 1), mView);
                GL.Uniform4(uLightLocation, lightPosition);
            }
            else if (e.KeyChar == 'f')
            {
                mView = mView * Matrix4.CreateTranslation(0, 0.01f, 0);
                int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uViewLocation, true, ref mView);

                int uEyeLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = new Vector4(2, 1, -8.5f, 1);
                GL.Uniform4(uEyeLocation, ref eyePosition);

                int uLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.Position");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -6f, 1), mView);
                GL.Uniform4(uLightLocation, lightPosition);
            }
            else if (e.KeyChar == 'r')
            {
                mView = mView * Matrix4.CreateTranslation(0, -0.01f, 0);
                int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uViewLocation, true, ref mView);

                int uEyeLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = new Vector4(2, 1, -8.5f, 1);
                GL.Uniform4(uEyeLocation, ref eyePosition);

                int uLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.Position");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -6f, 1), mView);
                GL.Uniform4(uLightLocation, lightPosition);
            }
            else if (e.KeyChar == 'l')
            {
                mView = mView * Matrix4.CreateRotationY(0.01f);
                int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uViewLocation, true, ref mView);

                int uEyeLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = new Vector4(2, 1, -8.5f, 1);
                GL.Uniform4(uEyeLocation, ref eyePosition);

                int uLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.Position");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -6f, 1), mView);
                GL.Uniform4(uLightLocation, lightPosition);
            }
            else if (e.KeyChar == 'j')
            {
                mView = mView * Matrix4.CreateRotationY(-0.01f);
                int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uViewLocation, true, ref mView);

                int uEyeLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = new Vector4(2, 1, -8.5f, 1);
                GL.Uniform4(uEyeLocation, ref eyePosition);

                int uLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.Position");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -6f, 1), mView);
                GL.Uniform4(uLightLocation, lightPosition);
            }
            else if (e.KeyChar == 'k')
            {
                mView = mView * Matrix4.CreateRotationX(0.01f);
                int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uViewLocation, true, ref mView);

                int uEyeLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = new Vector4(2, 1, -8.5f, 1);
                GL.Uniform4(uEyeLocation, ref eyePosition);

                int uLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.Position");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -6f, 1), mView);
                GL.Uniform4(uLightLocation, lightPosition);
            }
            else if (e.KeyChar == 'i')
            {
                mView = mView * Matrix4.CreateRotationX(-0.01f);
                int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uViewLocation, true, ref mView);

                int uEyeLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
                Vector4 eyePosition = new Vector4(2, 1, -8.5f, 1);
                GL.Uniform4(uEyeLocation, ref eyePosition);

                int uLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.Position");
                Vector4 lightPosition = Vector4.Transform(new Vector4(2, 1, -6f, 1), mView);
                GL.Uniform4(uLightLocation, lightPosition);
            }
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
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(mVAO_IDs[0]);
            mGroundMatrix = Matrix4.CreateTranslation(0, 0, 0);

            int uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref mGroundMatrix);

            updateTexture(1);

            GL.BindVertexArray(mVAO_IDs[0]);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);            

            mWallMatrix = Matrix4.CreateScale(0.5f) * Matrix4.CreateRotationX(1.7f) * Matrix4.CreateTranslation(0, 3, -8);
            GL.UniformMatrix4(uModel, true, ref mWallMatrix);

            updateTexture(2);

            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            GL.BindVertexArray(mVAO_IDs[1]);
                                 
            updateMaterial(new Vector3(0.329412f, 0.223529f, 0.027451f), new Vector3(0.780392f, 0.568627f, 0.113725f), new Vector3(0.992157f, 0.941176f, 0.807843f), 0.21794872f);
            updateTexture(0);

            int uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            mCubeMatrix = Matrix4.Identity;
            mCubeMatrix = mCubeMatrix * Matrix4.CreateTranslation(0.5f, 0.25f, -5f);
            GL.UniformMatrix4(uModelLocation, true, ref mCubeMatrix);
            GL.DrawElements(BeginMode.Triangles, mCubeIndices.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(mVAO_IDs[2]);

            updateMaterial(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.5f, 0.0f, 0.0f), new Vector3(0.7f, 0.6f, 0.6f), 0.25f);
            updateTexture(0);

            uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            mPyramidMatrix = Matrix4.Identity;
            mPyramidMatrix = mPyramidMatrix * Matrix4.CreateTranslation(-0.5f, 0.25f, -5f);
            GL.UniformMatrix4(uModelLocation, true, ref mPyramidMatrix);
            GL.DrawElements(BeginMode.Triangles, mPyramidIndices.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(mVAO_IDs[3]);
                                         
            updateMaterial(new Vector3(0.135f, 0.2225f, 0.1575f), new Vector3(0.54f, 0.89f, 0.63f), new Vector3(0.316228f, 0.316228f, 0.316228f), 0.1f);
            updateTexture(0);

            uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            mMonsterMatrix = Matrix4.Identity;
            mMonsterMatrix = mMonsterMatrix * Matrix4.CreateScale(0.15f) * Matrix4.CreateRotationY(-1.7f + angle) * Matrix4.CreateTranslation(0, 0.25f, -5f);
            GL.UniformMatrix4(uModelLocation, true, ref mMonsterMatrix);
            GL.DrawElements(BeginMode.Triangles, mModelMonster.Indices.Length, DrawElementsType.UnsignedInt, 0);

            angle += 0.005f;

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
            GL.DeleteTextures(mTexture_IDs.Length, mTexture_IDs);
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
        private void updateTexture(int pTexture)
        {
            int uTexture = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.Texture");
            GL.Uniform1(uTexture, pTexture);
        }
    }
}