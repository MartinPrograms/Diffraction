using System.Numerics;
using System.Text.Json.Serialization;
using Diffraction.Rendering.GUI;
using Diffraction.Rendering.Shaders;
using Diffraction.Rendering.Shaders.Materials;
using Diffraction.Rendering.Specials.Lighting;
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

        var lightManager = ObjectScene.Instance.FindObject("Light Manager") as LightManager;
        if (lightManager.LightEnabled)
        {
            _material.Shader.SetVec3("ambientColor", lightManager.AmbientLight);
            var lights = lightManager.Lights;
            
            for (int i = 0; i < lightManager.LightCount; i++)
            {
                var type = lights[i].GetLightType();
                _material.Shader.SetVec3("lightColor[" + i + "]", lights[i].Color);
                _material.Shader.SetVec3("lightPos[" + i + "]", type == (int)LightType.Directional ? lights[i].Transform.Forward : lights[i].Transform.Position);
                _material.Shader.SetFloat("lightIntensity[" + i + "]", lights[i].Intensity);
                _material.Shader.SetFloat("lightRange[" + i + "]", lights[i].Range);
                _material.Shader.SetInt("lightType[" + i + "]", type);
                
                // Shadow stuff
                if (type == (int)LightType.Directional)
                {
                    // Set shadowMaps[i] to the shadow map of the light
                    lights[i].BindShadowMap(TextureUnit.Texture8 + i);
                    _material.Shader.SetInt("shadowMaps[" + i + "]", 8 + i);
                    var light = lights[i] as DirectionalLight;
                    _material.Shader.SetMat4("shadowMatrices[" + i + "]", light.GetLightSpaceMatrix());
                }
                
                if (type == (int)LightType.Point)
                {
                    // Set shadowMaps[i] to the shadow map of the light
                    lights[i].BindShadowMap(TextureUnit.Texture1 + i);
                    _material.Shader.SetInt("shadowCubeMap[" + i + "]", (int)lights[i].ShadowMap);
                    var light = lights[i] as PointLight;
                    _material.Shader.SetFloat("far_plane[" + i+ "]", light.ShadowFar);
                    _material.Shader.SetVec3("lightPos[" + i + "]", light.Transform.Position);
                }
                
                _material.Shader.SetFloat("shadowBias[" + i + "]", lights[i].ShadowBias);
            }
            
            _material.Shader.SetInt("lightCount", lightManager.LightCount);
        }
        else
        {
            _material.Shader.SetVec3("ambientColor", new Vector3(1.0f, 1.0f, 1f));
        }
        
        _material.Shader.SetVec3("cameraPos", camera.Position);

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

    public void RawRender(Shaders.Shader shader)
    {
        _data.Bind();
        
        shader.SetMat4("model", _transform.GetModelMatrix() * _parentTransform.GetModelMatrix());
        
        _data.Draw();
    }
}