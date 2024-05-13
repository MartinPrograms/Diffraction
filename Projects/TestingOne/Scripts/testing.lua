import ('System.Numerics')
import ('Diffraction', 'Diffraction.Input') -- Importing the Input module, this is in C#, not Lua
import ('Diffraction', 'Diffraction.Scripting.Globals') -- Global variables, includes classes like Time, Network, etc.
import ('Diffraction', 'Diffraction.Rendering.Meshes') -- This includes the Transform class, used for moving objects
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
    print("This is a test movement script.")
end

function Update()
    forward = Transform.Forward

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
        Transform.Rotation = Quaternion.Multiply(Transform.Rotation, Quaternion.CreateFromAxisAngle(Vector3.UnitY, Time.DeltaTime))
    end

    if Input.GetKey(Key.D) then
        Transform.Rotation = Quaternion.Multiply(Transform.Rotation, Quaternion.CreateFromAxisAngle(Vector3.UnitY, -Time.DeltaTime))
    end

end