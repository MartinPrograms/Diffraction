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

    local direction = Vector3.Zero

    if (Input.GetKey(Key.ShiftLeft)) then
        speed = 10
    end


    if (Input.GetKeyDown(Key.Space) and rigidbody.IsGrounded) then
        rigidbody:AddForce(Vector3.UnitY * jumpForce, ForceMode.VelocityChange)
    end

    if (Input.GetKey(Key.W)) then
        direction = direction + forward * Time.DeltaTime;
    end

    if (Input.GetKey(Key.S)) then
        direction = direction - forward * Time.DeltaTime;
    end

    if (Input.GetKey(Key.A)) then
        direction = direction - Transform.Right * Time.DeltaTime;
    end

    if (Input.GetKey(Key.D)) then
        direction = direction + Transform.Right * Time.DeltaTime;
    end

    direction = Vector3.Normalize(direction) * speed
    if (direction:Length() > 0) then
        local velocity = rigidbody.Velocity
        velocity.X = direction.X
        velocity.Z = direction.Z

        rigidbody.Velocity = velocity
    end

    -- Grounded check
    local bottom = Transform.Position - Vector3.UnitY * 1.01
    local ray = Raycast.RaycastScene(bottom, -Vector3.UnitY, 0.1)
    if (ray.Hit) then
        rigidbody.IsGrounded = true
    else
        rigidbody.IsGrounded = false
    end

    StaticText.RenderText("Grounded: " .. tostring(rigidbody.IsGrounded), Vector2(10, 10), 2, Vector4(1, 1, 1, 1))

    -- Now for mouse movement
    local mouseDelta = Input.GetMouseDelta()

    if (Input.IsMouseLocked()) then        
        --Rotate the camera forward along the x & y axis
        MainCamera:SetPitch(MainCamera.TargetPitch - mouseDelta.Y * Time.DeltaTime * 0.5)
        MainCamera:SetYaw(MainCamera.TargetYaw + mouseDelta.X * Time.DeltaTime * 0.5)

        StaticText.RenderText("Yaw: " .. MainCamera.Yaw, Vector2(10, 30), 2, Vector4(1, 1, 1, 1))
        StaticText.RenderText("Pitch: " .. MainCamera.Pitch, Vector2(10, 50), 2, Vector4(1, 1, 1, 1))
        StaticText.RenderText("Mouse X: " .. mouseDelta.X, Vector2(10, 70), 2, Vector4(1, 1, 1, 1)) 
        StaticText.RenderText("Mouse Y: " .. mouseDelta.Y, Vector2(10, 90), 2, Vector4(1, 1, 1, 1))

        Transform.Rotation = Quaternion.CreateFromYawPitchRoll(-MainCamera.Yaw + math.pi / 2, 0, 0)
    end

    if (Input.GetKeyUp(Key.Escape)) then
        Input.ToggleMouseLock()
    end

    --MainCamera.Position = Transform.Position --+ Vector3.UnitY * 2 - Vector3.Multiply(Transform.Forward, 5)
   
    rigidbody:SetPhysicsTransform(Transform);
end