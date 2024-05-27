﻿using System.Numerics;
using Microsoft.Extensions.Configuration;

using ImGuiNET;
using MagicPhysX.Toolkit;

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

using Diffraction.Audio;
using Diffraction.Editor.GUI;
using Diffraction.Input;
using Diffraction.Physics;
using Diffraction.Rendering;
using Diffraction.Rendering.Buffers;
using Diffraction.Rendering.GUI;
using Diffraction.Rendering.GUI.Text;
using Diffraction.Rendering.Meshes;
using Diffraction.Rendering.Objects;
using Diffraction.Rendering.Old;
using Diffraction.Rendering.Shaders;
using Diffraction.Rendering.Shaders.Gen;
using Diffraction.Rendering.Shaders.Materials;
using Diffraction.Rendering.Specials;
using Diffraction.Rendering.Specials.Lighting;
using Diffraction.Scripting;
using Diffraction.Scripting.Globals;
using Diffraction.Serializables;
using Silk.NET.OpenGL;
using SilkyGizmos;
using Button = Diffraction.Rendering.GUI.Interactables.Button;
using Mesh = Diffraction.Rendering.Meshes.Mesh;
using Object = Diffraction.Rendering.Objects.Object;
using Rectangle = Diffraction.Rendering.GUI.Rectangle;
using Simulation = Diffraction.Physics.Simulation;
using Transform = Diffraction.Rendering.Meshes.Transform;
using Window = Diffraction.Rendering.Windowing.Window;

IConfigurationRoot config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

ObjectScene scene = new ObjectScene(config["project_path"]);

// Name, Vertex?, Fragment?, Compute?, Geometry?
var shaders = new Tuple<string, string?, string?, string?, string?>[]
{
    // Name, Vertex, Fragment
    new ("MonkeyShader", "Shaders/normal/standard.vert", "Shaders/normal/monkey.frag", null, null),
    new ("SelectionShader", "Shaders/normal/standard.vert", "Shaders/normal/selected.frag", null, null),
    new ("TransparentShader", "Shaders/normal/standard.vert", "Shaders/normal/transparent.frag", null, null),
    new ("TextShader", "Shaders/normal/text.vert", "Shaders/normal/text.frag", null, null),
    new ("RectShader", "Shaders/normal/rect.vert", "Shaders/normal/rect.frag", null, null),
    new ("SkyboxShader", "Shaders/normal/skybox.vert", "Shaders/normal/skybox.frag", null, null),
    new ("MeshSkybox", "Shaders/normal/advanced_skybox.vert", "Shaders/normal/monkey.frag", null, null),
    new ("LitShader", "Shaders/normal/standard.vert", "Shaders/normal/lit.frag", null, null),
    
    new ("DirectionalLightShader", "Shaders/lights/directional.vert", "Shaders/lights/directional.frag", null, null),
    new ("PointLightShader", "Shaders/lights/point.vert", "Shaders/lights/point.frag", null, "Shaders/lights/point.gs"), // our first shader with geometry :)
};

var apiVersion = new APIVersion(3, 3);


WindowOptions options = WindowOptions.Default with
{
    Size = new Vector2D<int>(1280, 720),
    Title = "Diffraction",
    API = new GraphicsAPI(ContextAPI.OpenGL, apiVersion),
    VSync = false,
    WindowBorder = WindowBorder.Resizable,
    WindowState = WindowState.Maximized,
};

var window = Window.Create(options);

RenderTexture renderTexture = null;

ShaderUI shaderUI = new ShaderUI();
ObjectUI objectUI = new ObjectUI();

Camera camera = new Camera(new Vector3(0, 0, 0), 0, 0, 0, 69, new Vector2D<int>(1280, 720));

Simulation simulation = new Simulation();
PhysicsUI physicsUI = new PhysicsUI();
AudioUI audioUI = new AudioUI();
MainMenuBar mainMenuBar = new MainMenuBar();
Viewport viewport = null;
ScriptUI scriptUI = new ScriptUI();
LightUI lightUI = new LightUI();

Stats stats = new Stats();

SceneUI sceneUI = new SceneUI();

Rigidbody sphere = null;

window.Open += () =>
{
    foreach (var shader in shaders)
    {
        ShaderUtils.AddShader(shader);
    }
    
    TextRenderer.SetShader(ShaderUtils.GetShader("TextShader"));

    
    {
        for (int x = 0; x < 5; x++)
        {
            for (int y = 1; y < 6; y++)
            {
             /*   var mesh = new Mesh(new sMeshData("Models/cube.obj"), // Load model
                    new Material(new sShader("MonkeyShader"),
                        new sTexture("Textures/monkey.png"))); // Assign textures and shaders

                float deviatedX = x + y % 2 * 0.5f; // Deviate the x position
                var obj = new Object($"Monkey {x}:{y}") { Components = new List<EventObject>() { mesh } }; // Assign the model
                obj.Transform.Position = new Vector3(deviatedX * 4.0f, y * 2.0f, 5); // Set the position
                obj.Transform.Scale = new Vector3(2, 1, 1); // Set the scale
                /*obj.Updatables.Add(new PhysicsObject(obj,
                    simulation.AddDynamicBox(obj.Transform.Scale, obj.Transform.Position, obj.Transform.Rotation,
                        10))); // Set the physics information

                scene.AddObject(obj); // Add the object to the scene
            */
            }
        }
    }
/*
    {
        var mesh = new Mesh(new sMeshData("Models/cube.obj"),
            new Material(new sShader("LitShader"), new sTexture("Textures/Sand/color.jpg"), new sTexture("Textures/Sand/normal.jpg")), new Material(new sShader("MeshSkybox"), new sTexture("Textures/Sand/color.jpg"), new sTexture("Textures/Sand/normal.jpg")));
        
        var obj = new Object("Floor") { Components = new List<EventObject>() { mesh } }; 
        obj.Transform.Scale = new Vector3(30, 1, 30);
        obj.Transform.Position = new Vector3(0, -1, 0);
        obj.Components.Add(new StaticPhysicsObject(new sObject(obj.Id), new sRigidstatic(new sCollisionShape(CollisionShapeType.Box,obj.Transform.Scale.ToList(), obj.Transform.Position, Quaternion.Identity))));
        
        scene.AddObject(obj);
    }

    {
        var mesh = new Mesh(new sMeshData("Models/cube.obj"),
            new Material(new sShader("LitShader"), new sTexture("Textures/Organic/color.jpg"), new sTexture("Textures/Organic/normal.jpg")), new Material(new sShader("MeshSkybox"), new sTexture("Textures/Organic/color.jpg"), new sTexture("Textures/Organic/normal.jpg")));
        var obj = new Object("TestCube") { Components = new List<EventObject>() { mesh } };
        obj.Transform.Position = new Vector3(0, 3.177f, 0);
        obj.Transform.Scale = new Vector3(1, 1, 1);
        
        var script = Script.Load("Scripts/testing.lua", new sObject(obj.Id));
        
        obj.Components.Add(new PhysicsObject(new sObject(obj.Id), new sRigidbody(new sCollisionShape(CollisionShapeType.Box, obj.Transform.Scale.ToList(), obj.Transform.Position, obj.Transform.Rotation, 10))){LockRotation = LockRotationFlags.All});
        
        obj.Components.Add(script);
        
        scene.AddObject(obj);
    }
  */

    {
        var obj = new Object("Sponza");

        obj.Children = ObjectLoader.Load("Models/sponza/sponza.obj"); // defaults to the most advanced shader (lit shader)
        
        scene.AddObject(obj);
    }
    
    {
        var obj = new Object("PointLight");
                
        obj.Transform.Position = new Vector3(0f, 5, 0f); // Unlike the directional light, the position matters
        obj.Transform.Rotation = new Quaternion(0, 0, 0, 1); // Set the rotation, which does not matter, this is an omni light
        
        var light = new PointLight(new sObject(obj.Id), new sShader("PointLightShader"), true);
        light.Color = new Vector3(1, 0, 0);
        light.CastsShadows = true;
        
        obj.Components.Add(light);
        
        scene.AddObject(obj);
    }

    /*
    {
        
        var obj = new Object("DirectionalLight");
        obj.Transform.Position = new Vector3(0, 8, 0); // Does not matter, only rotation
        obj.Transform.Rotation = new Quaternion(0.442f, -0.206f, 0.774f, 0.405f); // Set the rotation
        
        var light = new DirectionalLight(new sObject(obj.Id), new sShader("DirectionalLightShader"), true);
        light.ShadowSize = 72;
        
        obj.Components.Add(light);
        
        scene.AddObject(obj);
        
    }*/

 
    var skybox = new Skybox(new string[]
    {
        "Textures/cubemap/sea/nx.png",
        "Textures/cubemap/sea/px.png",
        "Textures/cubemap/sea/py.png",
        "Textures/cubemap/sea/ny.png",
        "Textures/cubemap/sea/nz.png",
        "Textures/cubemap/sea/pz.png"
    }, "SkyboxShader");
    
    scene.AddObject(skybox);
    //ShaderUtils.AddWatcher("MonkeyShader");
    // broken due to some unix file system issue, should work on windows?

    renderTexture = new RenderTexture(window.GL, window.IWindow.Size.X, window.IWindow.Size.Y);
    viewport = new Viewport(renderTexture, camera);
    
    /*var skybox = new Skybox(new Cubemap(window.GL, new string[]
    {
        "Textures/cubemap/shanghai/nx.png",
        "Textures/cubemap/shanghai/px.png",
        "Textures/cubemap/shanghai/py.png",
        "Textures/cubemap/shanghai/ny.png",
        "Textures/cubemap/shanghai/nz.png",
        "Textures/cubemap/shanghai/pz.png"
    }), ShaderUtils.GetShader("SkyboxShader"));*/
        
    LightManager lightManager = new LightManager();
    scene.AddObject(lightManager);

    lightManager.LightEnabled = false;

    
    if (File.Exists("layout.ini"))
    {
        ImGui.LoadIniSettingsFromDisk("layout.ini");
    }
};

bool renderimgui = true;
window.Update += (time) =>
{
    camera.Update(time);
    camera.Resolution = new Vector2D<int>(window.IWindow.Size.X, window.IWindow.Size.Y);

    foreach (var shader in ShaderUtils.Shaders)
    {
        shader.Value.Update(time);
    }

    scene.Update(Time.DeltaTime);
    
    simulation.Update(Time.DeltaTime);
    
    if (Input.GetKeyUp(Key.F3))
        renderimgui = !renderimgui;

    if (Input.GetKeyUp(Key.F11))
    {
        window.ToggleFullscreen();
    }
    
    Gizmos.Update();
};

window.Render += (time) =>
{
    //quad.Render();


    if (renderimgui)
    {

        scene.RenderLighting(camera);

        renderTexture.Bind();
        scene.Render(camera);
        Gizmos.Render();
        renderTexture.Unbind();

        window.GL.Viewport(0, 0, (uint)window.IWindow.FramebufferSize.X, (uint)window.IWindow.FramebufferSize.Y);

        ImGui.DockSpaceOverViewport();

        mainMenuBar.Render(camera);

        shaderUI.Render(camera);

        objectUI.SetObjects(scene.Objects);
        objectUI.Render(camera);
        physicsUI.Render(camera);
        audioUI.Render(camera);
        sceneUI.Render(camera);
        scriptUI.Render(camera);
        lightUI.Render(camera);

        viewport.Render(camera);

    }
    else
    {
        scene.RenderLighting(camera);
        
        Vector2 size = new Vector2(window.IWindow.FramebufferSize.X, window.IWindow.FramebufferSize.Y);
        camera.Resolution = new Vector2D<int>((int)size.X, (int)size.Y);
        camera.AspectRatio = (float)size.X / size.Y;

        window.GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        
        window.GL.Viewport(0, 0, (uint)size.X, (uint)size.Y);
        scene.Render(camera);
        
        Gizmos.SetInteractionResolution((int)window.IWindow.Size.X, (int)window.IWindow.Size.Y);
        Gizmos.SetResolution((int)window.IWindow.FramebufferSize.X, (int)window.IWindow.FramebufferSize.Y);
        Gizmos.SetOffset(0, 0);
        Gizmos.Render();
        window.GL.Viewport(0, 0, (uint)window.IWindow.FramebufferSize.X, (uint)window.IWindow.FramebufferSize.Y);
    }

    var LM = ObjectScene.Instance.FindObject("Light Manager") as LightManager;
    LM.LightEnabled = true;
};
window.LateRender += d =>
{
    if (!ObjectScene.Instance.Paused)
        stats.Render(camera);
    
    //Gizmos.Render();
};

window.Run();