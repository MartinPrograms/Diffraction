using System.Numerics;
using Silk.NET.GLFW;
using Diffraction.Input;
using Diffraction.Scripting.Globals;
using Silk.NET.Input;
using Silk.NET.Maths;
using MouseButton = Silk.NET.Input.MouseButton;

namespace Diffraction.Rendering;

using Input = Diffraction.Input.Input;
[Serializable]

public class Camera : EventObject
{
    [ExposeToLua("MainCamera", true)] public static Camera MainCamera;
    public bool IsMainCamera;

    public Vector3 Position;
    public Vector2D<int> Resolution;
    public float TargetSpeed = 12; // Lerp speed

    public float Yaw;
    public float Pitch;
    public float Roll;
    private float _targetYaw;
    private float _targetPitch;
    private float _targetRoll;

    public float FOV;
    public float AspectRatio;
    public Camera(Vector3 position, float yaw, float pitch, float roll, float fov, Vector2D<int> resolution)
    {
        Position = position;
        Yaw = yaw;
        Pitch = pitch;
        Roll = roll;
        FOV = fov;
        AspectRatio = resolution.X / (float)resolution.Y;
        Resolution = resolution;
        if (IsMainCamera || MainCamera == null)
        {
            MainCamera = this;
        }
    }
    
    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(Position, Position + Forward, Up);
    }
    
    public Matrix4x4 GetProjectionMatrix()
    {
        if (FOV > 180)
        {
            FOV = 180;
        }
        else if (FOV < 0)
        {
            FOV = 0;
        }
        return Matrix4x4.CreatePerspectiveFieldOfView(MathUtils.ToRadians(FOV), AspectRatio, 0.1f, 1000f);
    }
    
    public float Speed = 4f;
    public float Sensitivity  = 0.003f;
    private bool _mouseLocked = false;

    public Vector3 Forward
    {
        get
        {
            // calculate teh forward vector from the yaw, pitch, and roll
            var forward = new Vector3
            {
                X = MathF.Cos(Yaw) * MathF.Cos(Pitch),
                Y = MathF.Sin(Pitch),
                Z = MathF.Sin(Yaw) * MathF.Cos(Pitch)
            };
            return Vector3.Normalize(forward);
        }
    }
    
    public Vector3 Right
    {
        get
        {
            // calculate the right vector from the yaw, pitch, and roll
            var right = new Vector3
            {
                X = MathF.Cos(Yaw + MathF.PI / 2),
                Y = 0,
                Z = MathF.Sin(Yaw + MathF.PI / 2)
            };
            return Vector3.Normalize(right);
        }
    }
    
    public Vector3 Up
    {
        get
        {
            // calculate the up vector from the yaw, pitch, and roll
            return Vector3.Normalize(Vector3.Cross(Right, Forward));
        }
    }
    
    public void SetForward(Vector3 forward)
    {
        Yaw = MathF.Atan2(forward.Z, forward.X);
        Pitch = MathF.Asin(forward.Y);
    }

    public override void Update(double time)
    {
        if (Input.GetMouseButton(MouseButton.Right) && ObjectScene.Instance.Paused)
        {
            _mouseLocked = true;
            if (Input.GetKey(Key.W))
            {
                Position += Forward * Speed * (float)time;
            }

            if (Input.GetKey(Key.S))
            {
                Position -= Forward * Speed * (float)time;
            }

            if (Input.GetKey(Key.A))
            {
                Position -= Right * Speed * (float)time;
            }

            if (Input.GetKey(Key.D))
            {
                Position += Right * Speed * (float)time;
            }

            if (Input.GetKey(Key.Space))
            {
                Position += Up * Speed * (float)time;
            }

            if (Input.GetKey(Key.ShiftLeft))
            {
                Position -= Up * Speed * (float)time;
            }
        }
        else
        {
            _mouseLocked = false;
        }

        if (ObjectScene.Instance.Paused)
        {
            if (_mouseLocked)
            {
                var delta = Input.MouseDelta;
                _targetYaw += delta.X * Sensitivity;
                _targetPitch -= delta.Y * Sensitivity;
                if (Pitch > MathF.PI / 2)
                {
                    _targetPitch = MathF.PI / 2 - 0.001f;
                }
                else if (Pitch < -MathF.PI / 2)
                {
                    _targetPitch = -MathF.PI / 2 + 0.001f;
                }
                Input.SetCursorMode(CursorMode.Raw);
            }
            else
            {
                Input.SetCursorMode(CursorMode.Normal);
            }
        }

        if (Input.GetKeyUp(Key.Escape))
        {
            _mouseLocked = !_mouseLocked;
        }

        if (TargetSpeed == 0)
        {
            Yaw = _targetYaw;
            Pitch = _targetPitch;
            Roll = _targetRoll;
        }
        else
        {
            float speed = TargetSpeed * (float)time;
            Yaw = MathUtils.Lerp(Yaw, _targetYaw, speed);
            Pitch = MathUtils.Lerp(Pitch, _targetPitch, speed);
            Roll = MathUtils.Lerp(Roll, _targetRoll, speed);
        }
    }


    public void Dispose()
    {
        
    }
}

public static class MathUtils
{
    public static float ToRadians(float degrees)
    {
        return degrees * (MathF.PI / 180f);
    }

    public static Quaternion ToQuaternion(Vector3 camForward)
    {
        return Quaternion.CreateFromYawPitchRoll(MathF.Atan2(camForward.X, camForward.Z), MathF.Asin(camForward.Y), 0);
    }
    
    public static void Mat4x4ToMat3x3(Matrix4x4 mat4, out Matrix3X3<float> mat3)
    {
        mat3 = new Matrix3X3<float>(mat4.M11, mat4.M12, mat4.M13,
            mat4.M21, mat4.M22, mat4.M23,
            mat4.M31, mat4.M32, mat4.M33);
    }

    // Extend the Vector3 class to include a method that returns a list of floats
    public static List<float> ToList(this Vector3 vec)
    {
        return new List<float> {vec.X, vec.Y, vec.Z};
    }
    
    /*
       function easeInOutCirc(x: number): number {
       return x < 0.5
         ? (1 - Math.sqrt(1 - Math.pow(2 * x, 2))) / 2
         : (Math.sqrt(1 - Math.pow(-2 * x + 2, 2)) + 1) / 2;
       }
     */
    
    private static float EaseInOutCirc(float x)
    {
        return x < 0.5f
            ? (1 - MathF.Sqrt(1 - MathF.Pow(2 * x, 2))) / 2
            : (MathF.Sqrt(1 - MathF.Pow(-2 * x + 2, 2)) + 1) / 2;
    }
    
    public static float EaseInOutCirc(float a, float b, float t)
    {
        return Lerp(a, b, EaseInOutCirc(t));
    }
    
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}