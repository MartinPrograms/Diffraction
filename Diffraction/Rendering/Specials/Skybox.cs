using System.Numerics;
using Diffraction.Rendering.Meshes;
using Newtonsoft.Json;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Diffraction.Rendering.Specials;

public class Skybox : Rendering.Objects.Object
{
    public string[] CubemapName;
    public string ShaderName;
    
    [JsonIgnore]
    private Diffraction.Rendering.Shaders.Shader _shader = null;
    
    [JsonIgnore]
    public Cubemap Cubemap = null;
    
    [JsonIgnore]
    private uint _vao;
    [JsonIgnore]
    private uint _vbo;
    
    [JsonIgnore]
    private GL _gl;
    
    public Skybox(string[] cubemapName, string shaderName)
    {
        CubemapName = cubemapName;
        ShaderName = shaderName;
    }

    public override void Render(Camera camera)
    {
        if (_shader == null)
        {
            _shader = Diffraction.Rendering.Shaders.ShaderUtils.GetShader(ShaderName);
            Cubemap = Diffraction.Rendering.Shaders.TextureUtils.GetCubemap(CubemapName);

            Name = "Skybox";

            float[] skyboxVertices =
            {
                // positions          
                -1.0f, 1.0f, -1.0f,
                -1.0f, -1.0f, -1.0f,
                1.0f, -1.0f, -1.0f,
                1.0f, -1.0f, -1.0f,
                1.0f, 1.0f, -1.0f,
                -1.0f, 1.0f, -1.0f,

                -1.0f, -1.0f, 1.0f,
                -1.0f, -1.0f, -1.0f,
                -1.0f, 1.0f, -1.0f,
                -1.0f, 1.0f, -1.0f,
                -1.0f, 1.0f, 1.0f,
                -1.0f, -1.0f, 1.0f,

                1.0f, -1.0f, -1.0f,
                1.0f, -1.0f, 1.0f,
                1.0f, 1.0f, 1.0f,
                1.0f, 1.0f, 1.0f,
                1.0f, 1.0f, -1.0f,
                1.0f, -1.0f, -1.0f,

                -1.0f, -1.0f, 1.0f,
                -1.0f, 1.0f, 1.0f,
                1.0f, 1.0f, 1.0f,
                1.0f, 1.0f, 1.0f,
                1.0f, -1.0f, 1.0f,
                -1.0f, -1.0f, 1.0f,

                -1.0f, 1.0f, -1.0f,
                1.0f, 1.0f, -1.0f,
                1.0f, 1.0f, 1.0f,
                1.0f, 1.0f, 1.0f,
                -1.0f, 1.0f, 1.0f,
                -1.0f, 1.0f, -1.0f,

                -1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f, 1.0f,
                1.0f, -1.0f, -1.0f,
                1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f, 1.0f,
                1.0f, -1.0f, 1.0f
            };

            _gl = Windowing.Window.Instance.GL;

            _vao = _gl.GenVertexArray();
            _vbo = _gl.GenBuffer();

            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            unsafe
            {
                fixed (float* ptr = skyboxVertices)
                {
                    _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(skyboxVertices.Length * sizeof(float)), ptr,
                        BufferUsageARB.StaticDraw);
                }
            }

            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            _gl.EnableVertexAttribArray(0);

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            _gl.BindVertexArray(0);

            Transform.Position = new Vector3(0, 0, 0);
        }
        
        if (!IsVisible)
        {
            return;
        }
        _gl.DepthMask(false);
        _shader.Use();
        //glm::mat4 view = glm::mat4(glm::mat3(camera.GetViewMatrix()));  
        MathUtils.Mat4x4ToMat3x3(camera.GetViewMatrix(), out Matrix3X3<float> tempView);
        Matrix4x4 view = new Matrix4x4(tempView.M11, tempView.M12, tempView.M13, 0,
            tempView.M21, tempView.M22, tempView.M23, 0,
            tempView.M31, tempView.M32, tempView.M33, 0,
            0, 0, 0, 1);
        _shader.SetMat4("view", view);
        _shader.SetMat4("projection", camera.GetProjectionMatrix());
        _shader.SetMat4("model", Transform.GetModelMatrix());
            
        _gl.BindVertexArray(_vao);
        Cubemap.Bind();
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
        _gl.BindVertexArray(0);
        _gl.DepthMask(true);

        base.Render(camera);
    }
    

}