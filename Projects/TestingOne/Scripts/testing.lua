import ('System.Numerics')
import ('Diffraction', 'Diffraction.Input') -- Importing the Input module, this is in C#, not Lua
import ('Diffraction', 'Diffraction.Scripting.Globals') -- Global variables, includes classes like Time, Network, etc.
import ('Diffraction', 'Diffraction.Rendering.Meshes') -- This includes the Transform class, used for moving objects
import ('Diffraction', 'Diffraction.Rendering') -- This includes the Camera class, used for moving the camera
import ('Diffraction', 'Diffraction.Rendering.GUI.Text')
import ('Silk.NET.Input.Common', 'Silk.NET.Input')

--[[

Global Variables: (outside this script)
    - Time (class)
        - .DeltaTime (float)
        - .Time (float)

    - Transform (class)
        - .Position (Vector3)
        - .Rotation (Quaternion)
        - .Scale (Vector3)

    - Input (class)
        - .GetKey (function)
        - .GetKeyDown (function)
        - .GetKeyUp (function)
        - .GetMouseButton (function)
        - .GetMouseButtonDown (function)
        - .GetMouseButtonUp (function)
        - .GetMousePosition (function)
        - .GetMouseScrollDelta (function)
        - .GetMouseDelta (function)
    ]]--

function Start()
    print("Example movement script started")
end

function Update()
    local forward = Transform.Forward
    local up = Vector3.UnitY

    local speed = 5

    if Input.GetKey(Key.ShiftLeft) then
        speed = 10
    else
        speed = 5
    end

    if Input.GetKey(Key.W) then
        Transform.Position = Vector3.Add(Transform.Position, Vector3.Multiply(Transform.Forward, Time.DeltaTime * speed))
    end

    if Input.GetKey(Key.S) then
        Transform.Position = Vector3.Subtract(Transform.Position, Vector3.Multiply(Transform.Forward, Time.DeltaTime * speed))
    end

    if Input.GetKey(Key.A) then
        Transform.Rotation = Quaternion.Multiply(Transform.Rotation, Quaternion.CreateFromAxisAngle(up, Time.DeltaTime))
    end

    if Input.GetKey(Key.D) then
        Transform.Rotation = Quaternion.Multiply(Transform.Rotation, Quaternion.CreateFromAxisAngle(up, -Time.DeltaTime))
    end

    if Input.GetKey(Key.Space) then
        Transform.Position = Vector3.Add(Transform.Position, up * Time.DeltaTime * speed)
    end

    if Input.GetKey(Key.ControlLeft) then
        Transform.Position = Vector3.Subtract(Transform.Position, up * Time.DeltaTime * speed)
    end

    MainCamera.Position = Transform.Position + Vector3.UnitY * 2 - Vector3.Multiply(Transform.Forward, 5)
    MainCamera:SetForward(Transform.Forward)

    StaticText.RenderTextB("testing")
    StaticText.RenderTextA("testing " .. Time.DeltaTime, Vector2(-50, -50), 2, Vector4(1, 0.5, 0.25, 0.5));
    local mousePos = Input.GetMousePosition():ToString()
    local mouseDelta = Input.GetMouseDelta():ToString()
    StaticText.RenderTextA("mouse pos" .. mousePos .. " mouse delta " .. mouseDelta, Vector2(-50, -100), 2, Vector4(1, 0.5, 0.75, 0.75));
end