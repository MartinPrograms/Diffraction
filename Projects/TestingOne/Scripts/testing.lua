import ('System.Numerics')
import ('MagicPhysX', 'MagicPhysX.Toolkit')
import ('Diffraction', 'Diffraction.Input') -- Importing the Input module, this is in C#, not Lua
import ('Diffraction', 'Diffraction.Scripting.Globals') -- Global variables, includes classes like Time, Network, etc.
import ('Diffraction', 'Diffraction.Rendering.Meshes') -- This includes the Transform class, used for moving objects
import ('Diffraction', 'Diffraction.Rendering') -- This includes the Camera class, used for moving the camera
import ('Diffraction', 'Diffraction.Rendering.GUI.Text') -- This includes the StaticText class, used for rendering text
import ('Diffraction', 'Diffraction.Physics') -- Includes Raycast.RaycastScene, used for raycasting
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

    local rigidbody = this:GetComponent("PhysicsObject")
    this.IsVisible = false

    local speed = 5
    local jumpForce = 5

    if (Input.GetKey(Key.ShiftLeft)) then
        speed = 10
    end


    if (Input.GetKeyDown(Key.Space) and rigidbody.IsGrounded) then
        rigidbody:AddForce(Vector3.UnitY * jumpForce, ForceMode.VelocityChange)
    end

    if (Input.GetKey(Key.W)) then
        Transform.Position = Transform.Position + forward * Time.DeltaTime * speed
    end

    if (Input.GetKey(Key.S)) then
        Transform.Position = Transform.Position - forward * Time.DeltaTime * speed
    end

    if (Input.GetKey(Key.A)) then
        Transform.Position = Transform.Position - Transform.Right * Time.DeltaTime * speed
    end

    if (Input.GetKey(Key.D)) then
        Transform.Position = Transform.Position + Transform.Right * Time.DeltaTime * speed
    end
    
    -- Grounded check
    local bottom = Transform.Position - Vector3.UnitY * 1.01
    local ray = Raycast.RaycastScene(bottom, -Vector3.UnitY, 0.1)
    if (ray.Hit) then
        rigidbody.IsGrounded = true
    else
        rigidbody.IsGrounded = false
    end

    StaticText.RenderTextA("Grounded: " .. tostring(rigidbody.IsGrounded), Vector2(10, 10), 2, Vector4(1, 1, 1, 1))

    -- Now for mouse movement
    local mouseDelta = Input.GetMouseDelta()

    if (Input.IsMouseLocked()) then
        Transform.Rotation = Transform.Rotation * Quaternion.CreateFromAxisAngle(Vector3.UnitY, -mouseDelta.X * Time.DeltaTime * 0.5)
        
        --Rotate the camera forward along the x & y axis
        MainCamera.Pitch = MainCamera.Pitch - mouseDelta.Y * Time.DeltaTime * 0.5
        MainCamera.Yaw = MainCamera.Yaw - mouseDelta.X * Time.DeltaTime * 0.5
    end

    if (Input.GetKeyUp(Key.Escape)) then
        Input.ToggleMouseLock()
    end

    MainCamera.Position = Transform.Position --+ Vector3.UnitY * 2 - Vector3.Multiply(Transform.Forward, 5)
    MainCamera.TargetSpeed = 45
   
    rigidbody:SetPhysicsTransform(Transform);
end