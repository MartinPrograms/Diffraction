using System.Numerics;
using System.Text.Json.Serialization;
using Diffraction.Rendering.Shaders;
using Diffraction.Rendering.Shaders.Materials;
using Diffraction.Rendering.Windowing;
using Diffraction.Scripting.Globals;
using Diffraction.Serializables;
using Newtonsoft.Json;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

using Texture = Diffraction.Rendering.Shaders.Texture;

namespace Diffraction.Rendering.Meshes;

[Serializable]
[JsonDerivedType(typeof(Mesh))]
public class Mesh : EventObject
{
    [JsonProperty]
    private Material _material;
    [JsonProperty]
    private Material _skyboxMaterial;
    
    [JsonProperty]
    private sMeshData _data;
    
    [JsonProperty]
    private Transform _transform;
    
    [JsonProperty]
    private Transform _parentTransform;
    
    [ExposeToLua("IsSkybox")]
    public bool IsSkybox = false;
    
    public Mesh( sMeshData data, Material material, Material skyboxMaterial)
    {
        _data = data;
        _material = material;
        _skyboxMaterial = skyboxMaterial;
        
        _transform = new Transform(Vector3.Zero, Quaternion.Identity, Vector3.One);
    }
    
    public Mesh()
    {
        
    }
    
    public override void Render(Camera camera)
    {
        _material.Use();
        _data.Bind();
        
        var view = camera.GetViewMatrix();
        
        if (IsSkybox)
        {
            _skyboxMaterial.Use();
            
            Window.Instance.GL.DepthMask(false);
            MathUtils.Mat4x4ToMat3x3(camera.GetViewMatrix(), out Matrix3X3<float> tempView);
            view = new Matrix4x4(tempView.M11, tempView.M12, tempView.M13, 0,
                tempView.M21, tempView.M22, tempView.M23, 0,
                tempView.M31, tempView.M32, tempView.M33, 0,
                0, 0, 0, 1);
            
            _skyboxMaterial.Shader.SetMat4("model", _transform.GetModelMatrix() * _parentTransform.GetModelMatrix());
            _skyboxMaterial.Shader.SetMat4("view", view);
            _skyboxMaterial.Shader.SetMat4("projection", camera.GetProjectionMatrix());
        }
        else
        {
            _material.Shader.SetMat4("model", _transform.GetModelMatrix() * _parentTransform.GetModelMatrix());
            _material.Shader.SetMat4("view", view);
            _material.Shader.SetMat4("projection", camera.GetProjectionMatrix());
        }

        _data.Draw();
        
        if (IsSkybox)
        {
            Window.Instance.GL.DepthMask(true);
        }
    }
    
    public void Render(Camera camera, Shaders.Shader shader, GLEnum mode)
    {
        shader.Use();
        _data.Bind();
        
        shader.SetMat4("model", _transform.GetModelMatrix() * _parentTransform.GetModelMatrix());
        shader.SetMat4("view", camera.GetViewMatrix());
        shader.SetMat4("projection", camera.GetProjectionMatrix());
        
        _data.Draw(mode);
    }

    public void SetParentTransform(Transform transform)
    {
        _parentTransform = transform;
    }
}