using System.Numerics;
using Silk.NET.GLFW;
using Diffraction.Input;
using Silk.NET.Input;
using Silk.NET.Maths;
using MouseButton = Silk.NET.Input.MouseButton;

namespace Diffraction.Rendering;

using Input = Diffraction.Input.Input;

public class Camera : EventObject
{
    public Vector3 Position { get; set; }
    public Vector2D<int> Resolution { get; set; }
    public Vector3 Forward { get; set; }
    private Vector3 _targetForward;
    public float TargetSpeed { get; set; } = 12; // Lerp speed
    public Vector3 Up { get; set; }
    public Vector3 Right { get; set; }
    
    public float FOV { get; set; }
    public float AspectRatio { get; set; }
    
    public Camera(Vector3 position, Vector3 forward, Vector3 up, float fov, float aspectRatio)
    {
        Position = position;
        Forward = forward;
        _targetForward = forward;
        Up = up;
        Right = Vector3.Cross(Forward, Up);
        FOV = fov;
        AspectRatio = aspectRatio;
    }
    
    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(Position, Position + Forward, Up);
    }
    
    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(MathUtils.ToRadians(FOV), AspectRatio, 0.1f, 1000f);
    }
    
    public float Speed { get; set; } = 4f;
    public float Sensitivity { get; set; } = 0.01f;
    private bool _mouseLocked = false;

    public override void Update(double time)
    {
        if (Input.GetMouseButton(MouseButton.Right))
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

        if (_mouseLocked)
        {
            var delta = Input.MouseDelta;
            _targetForward = Vector3.Transform(_targetForward,
                Matrix4x4.CreateFromAxisAngle(Up, -delta.X * 0.1f * Sensitivity));
            _targetForward = Vector3.Transform(_targetForward,
                Matrix4x4.CreateFromAxisAngle(Right, -delta.Y * 0.1f * Sensitivity));

            Input.SetCursorMode(CursorMode.Raw);
        }
        else
        {
            Input.SetCursorMode(CursorMode.Normal);
        }

        if (Input.GetKeyUp(Key.Escape))
        {
            _mouseLocked = !_mouseLocked;
        }

        Forward = Vector3.Lerp(Forward, _targetForward, TargetSpeed * (float)time);
        Right = Vector3.Cross(Forward, Up);
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
}