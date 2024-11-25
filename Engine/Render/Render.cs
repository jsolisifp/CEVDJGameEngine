using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;

namespace GameEngine
{
    internal class Render
    {
        static Vector3 clearColor;

        static Vector4 ambientLight = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
        static Vector4 directionalLight;
        static Vector3 directionalLightColor;

        static Matrix4x4 viewMatrix;
        static Matrix4x4 projectionMatrix;

        static float opacity;

        static bool transparentBlending;

        static IWindow window;

        public delegate void OnRenderOverlay(float deltaTime);
        public static OnRenderOverlay onRenderOverlay;

        public delegate void OnOverrideView(ref Matrix4x4 viewMatrix, ref Matrix4x4 projectionMatrix);
        public static OnOverrideView onOverrideView;

        static GL context;



        public static void Init(IWindow _window)
        {
            context = GL.GetApi(_window);

            context.Enable(GLEnum.CullFace);

            Assets.RegisterAssetLoader("png", new TextureLoader(context));
            Assets.RegisterAssetLoader("obj", new ModelLoader(context));
            Assets.RegisterAssetLoader("shader", new ShaderLoader(context));

            viewMatrix = Matrix4x4.Identity;
            projectionMatrix = Matrix4x4.Identity;

            transparentBlending = false;
            opacity = 1;

            window = _window;

            window.FramebufferResize += OnFramebufferResize;
        }


        public static GL GetContext()
        {
            return context;
        }


        public static void Finish()
        {
            window.FramebufferResize -= OnFramebufferResize;

            context.Dispose();
        }

        public static void DrawModel(Vector3 position, Vector3 rotation, Vector3 scale, Model model, Shader shader, Texture texture)
        {
            Shader.BlendType blendType = shader.GetBlendType();
            if (blendType == Shader.BlendType.transparent || blendType == Shader.BlendType.additive)
            {   context.Enable(GLEnum.Blend);

                if(blendType == Shader.BlendType.transparent)
                {
                    context.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
                    context.DepthMask(false);
                }
                else // Shader.BlendType.additive
                {
                    context.BlendFunc(GLEnum.SrcAlpha, GLEnum.One);
                    context.DepthMask(false);
                }
                context.Enable(GLEnum.PolygonOffsetFill);
                context.PolygonOffset(1f, -2f);
            }
            else
            {
                context.Disable(GLEnum.Blend);
                context.Disable(GLEnum.PolygonOffsetFill);
                context.DepthMask(true);
            }

            texture.Bind();
            shader.Use();
            shader.SetUniform("uTexture0", 0);
            shader.SetUniform("uView", viewMatrix);
            shader.SetUniform("uProjection", projectionMatrix);
            shader.SetUniform("uDirectionalLight", directionalLight);
            shader.SetUniform("uDirectionalLightColor", directionalLightColor);
            shader.SetUniform("uAmbientLight", ambientLight);
            if(shader.GetBlendType() != Shader.BlendType.opaque)
            {
                shader.SetUniform("uOpacity", opacity);
            }

            float rotX = MathUtils.DegreesToRadians(rotation.X);
            float rotY = MathUtils.DegreesToRadians(rotation.Y);
            float rotZ = MathUtils.DegreesToRadians(rotation.Z);

            var modelMatrix =   Matrix4x4.CreateScale(scale) *
                                Matrix4x4.CreateRotationX(rotX) *
                                Matrix4x4.CreateRotationY(rotY) *
                                Matrix4x4.CreateRotationZ(rotZ) *
                                Matrix4x4.CreateTranslation(position);

            int c = model.meshes.Count;
            for (int i = 0; i < c; i ++)
            {
                Mesh m = model.meshes[i];
                m.Bind();
                shader.SetUniform("uModel", modelMatrix);

                context.DrawArrays(PrimitiveType.Triangles, 0, (uint)m.vertices.Length);
            }

        }

        public static unsafe void OnRender(float deltaTime)
        {
            context.Enable(EnableCap.DepthTest);
            context.ClearColor(clearColor.X, clearColor.Y, clearColor.Z, 1.0f);
            context.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Override view

            onOverrideView.Invoke(ref viewMatrix, ref projectionMatrix);

            // Renderizar los objetos

            SceneManager.Render(deltaTime);

            // Renderizar los overlays

            onRenderOverlay.Invoke(deltaTime);

        }

        public static void SetView(Vector3 position, Vector3 rotation, float fov, float zNear, float zFar)
        {
            float zRads = MathUtils.DegreesToRadians(rotation.Z);
            float yRads = MathUtils.DegreesToRadians(rotation.Y);
            float xRads = MathUtils.DegreesToRadians(rotation.X);

            float fovRads = MathUtils.DegreesToRadians(fov);

            Vector2D<int> size = window.FramebufferSize;

            Matrix4x4 temp = Matrix4x4.CreateRotationX(xRads) * 
                            Matrix4x4.CreateRotationY(yRads) *
                            Matrix4x4.CreateRotationZ(zRads) *
                            Matrix4x4.CreateTranslation(position);
            Matrix4x4.Invert(temp, out viewMatrix);
            projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fovRads, (float)size.X / size.Y, zNear, zFar);
        }

        public static void ClearDepth()
        {
            context.Clear(ClearBufferMask.DepthBufferBit);
        }

        public static void SetOpacity(float o)
        {
            opacity = o;
        }

        public static void SetDirectionalLight(Vector3 direction, float intensity, Vector3 color)
        {
            directionalLight = new Vector4(direction, intensity);
            directionalLightColor = color;
        }

        public static void SetClearColor(Vector3 color)
        {
            clearColor = color;
        }

        public static void SetAmbientLight(Vector3 color, float intensity)
        {
            ambientLight = new Vector4(color, intensity);
        }


        static void OnFramebufferResize(Vector2D<int> newSize)
        {
            context.Viewport(newSize);
        }


    }
}
