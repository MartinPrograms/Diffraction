using System.Numerics;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace SilkyGizmos;

public static class Gizmos // Used in immediate mode like imgui, but for gizmos!
{
    private static List<Action> _drawCalls = new();
    public static void Init(GL gl, IntPtr context, IWindow window, IInputContext input)
    {
        _gl = gl;
        Rendering.Init(gl);
        _context = context;
        _window = window;
        _input = input;
        
        _drawCalls.Clear();
        
        Input.Start(input);
        Input.SetInput(input);
    }
    
    private static GL _gl; // OpenGL context (used for drawing gizmos)
    private static IntPtr _context; // ImGui context (used for drawing window based gizmos)
    private static IWindow _window; // Window (used for drawing gizmos correctly)
    private static IInputContext _input; // Input context (used for interacting with gizmos)
    private static Vector4 _renderPositionAndSize = new Vector4(0, 0, 0, 0); // x, y, width, height
    
    
    /* EXAMPLE RENDERING CODE
    _drawCalls.Add(() =>
    {
        Rendering.DrawLine(Vector3.Zero, Vector3.UnitY, Vector3.One, view, projection, _renderPositionAndSize);
    });
    */
    
    private static bool _scaleXDraggedLastFrame = false;
    private static bool _scaleYDraggedLastFrame = false;
    private static bool _scaleZDraggedLastFrame = false;
    
    private static bool _translateXDraggedLastFrame = false;
    private static bool _translateYDraggedLastFrame = false;
    private static bool _translateZDraggedLastFrame = false;
    
    public static bool _rotateXDraggedLastFrame = false;
    public static bool _rotateYDraggedLastFrame = false;
    public static bool _rotateZDraggedLastFrame = false;
    
    private static Vector3 _lastMouseWorldPos = Vector3.Zero;
    
    public static bool Manipulate(Matrix4x4 view, Matrix4x4 projection, OPERATION operation, MODE mode, ref sgTransform model)
    {
        bool result = false;
        if (_renderPositionAndSize == Vector4.Zero)
        {
            _renderPositionAndSize = new Vector4(0, 0, _window.Size.X, _window.Size.Y);
        }
        // takes in 2 matrices, then applies modifications to the model matrix
        if (operation == OPERATION.SHOW)
        {
            var position = model.Position;
            var forward = model.Forward;
            var up = model.Up;
            var right = model.Right;
            
            _drawCalls.Add(() =>
            {
                // The forward vector is blue, the up vector is green, and the right vector is red
                Rendering.DrawLine(position, position + forward, Vector3.UnitZ, view, projection, _renderPositionAndSize);
                Rendering.DrawLine(position, position + up, Vector3.UnitY, view, projection, _renderPositionAndSize);
                Rendering.DrawLine(position, position + right, Vector3.UnitX, view, projection, _renderPositionAndSize);
            });
        }

        if (operation == OPERATION.SCALE)
        {
            Vector3 position = model.Position;
            Vector3 scalefudge = model.Scale;
            float size = 0.3f; // drag point sizes
            
            // 3 balls, x, y, z
            Vector3 x = (model.Right * model.Scale.X) + position;
            Vector3 y = (model.Up * model.Scale.Y) + position;
            Vector3 z = (model.Forward * model.Scale.Z) + position;
            
            Vector3 xColor = new Vector3(0.6f, 0.2f, 0.1f);
            Vector3 yColor = new Vector3(0.1f, 0.6f, 0.2f);
            Vector3 zColor = new Vector3(0.2f, 0.1f, 0.6f);
            
            // Now for interaction
            Vector2 mousePos = Input.MousePosition;
            Vector3 xScreenPos = WorldToScreen(x, view, projection, new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            Vector3 yScreenPos = WorldToScreen(y, view, projection, new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            Vector3 zScreenPos = WorldToScreen(z, view, projection, new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            
            // Get the first ball that is being hovered
            
            float xDistanceFromCamera = (x - view.ExtractTranslation()).Length();
            float yDistanceFromCamera = (y - view.ExtractTranslation()).Length();
            float zDistanceFromCamera = (z - view.ExtractTranslation()).Length();
            
            bool xHovered = IsMouseHoveringBall(mousePos, xScreenPos, size / xDistanceFromCamera, new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            bool yHovered = IsMouseHoveringBall(mousePos, yScreenPos, size / yDistanceFromCamera, new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            bool zHovered = IsMouseHoveringBall(mousePos, zScreenPos, size / zDistanceFromCamera, new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));

            bool xDrag = false;
            bool yDrag = false;
            bool zDrag = false;
            
            if (xHovered)
            {
                xColor = xColor * 1.5f;

                if (Input.GetMouseButtonDown(MouseButton.Left))
                {
                    xDrag = true;
                }
            }
            else if (yHovered)
            {
                yColor = yColor * 1.5f;
                
                if (Input.GetMouseButtonDown(MouseButton.Left))
                {
                    yDrag = true;
                }
            }
            else if (zHovered)
            {
                zColor = zColor * 1.5f;
                
                if (Input.GetMouseButtonDown(MouseButton.Left))
                {
                    zDrag = true;
                }
            }
            
            Vector3 currentWorldPos = ScreenToWorld(mousePos, view, projection, new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            
            Vector3 deltaWorld = currentWorldPos - _lastMouseWorldPos;
            
            _lastMouseWorldPos = currentWorldPos;
            
            deltaWorld *= 4.0f; // Speed
            
            if (xDrag || _scaleXDraggedLastFrame)
            {
                model.Scale += new Vector3(-deltaWorld.X, 0, 0);
                
                result = true;
            }
            
            if (yDrag || _scaleYDraggedLastFrame)
            {
                model.Scale += new Vector3(0, deltaWorld.Y, 0);
                
                result = true;
            }
            
            if (zDrag || _scaleZDraggedLastFrame)
            {
                model.Scale += new Vector3(0, 0, deltaWorld.Z);
                
                result = true;
            }

            if (Input.GetMouseButtonUp(MouseButton.Left))
            {
                _scaleXDraggedLastFrame = false;
                _scaleYDraggedLastFrame = false;
                _scaleZDraggedLastFrame = false;
            }
            
            if (xDrag)
                _scaleXDraggedLastFrame = true;
            if (yDrag)
                _scaleYDraggedLastFrame = true;
            if (zDrag)
                _scaleZDraggedLastFrame = true;
            
            _drawCalls.Add(() =>
            {
                // Some helper lines
                Rendering.DrawLine(position, x, xColor, view, projection, _renderPositionAndSize);
                Rendering.DrawLine(position, y, yColor, view, projection, _renderPositionAndSize);
                Rendering.DrawLine(position, z, zColor, view, projection, _renderPositionAndSize);
                
                Rendering.DrawWireCube(position, scalefudge, Vector3.One, view, projection, _renderPositionAndSize);
                
                // The drag points
                if (!xHovered)
                    Rendering.DrawWireCube(x, size/3f, xColor, view, projection, _renderPositionAndSize);
                else
                    Rendering.DrawCube(x, size/3f, xColor, view, projection, _renderPositionAndSize);
                
                if (!yHovered)
                    Rendering.DrawWireCube(y, size/3f, yColor, view, projection, _renderPositionAndSize);
                else
                    Rendering.DrawCube(y, size/3f, yColor, view, projection, _renderPositionAndSize);
                
                if (!zHovered)
                    Rendering.DrawWireCube(z, size/3f, zColor, view, projection, _renderPositionAndSize);
                else
                    Rendering.DrawCube(z, size/3f, zColor, view, projection, _renderPositionAndSize);
            });
        }

        if (operation == OPERATION.ROTATE)
        {
            Vector3 position = model.Position;
            Vector3 forward = model.Forward;
            Vector3 up = model.Up;
            Vector3 right = model.Right;
            
            Vector3 x = right + position;
            Vector3 y = up + position;
            Vector3 z = forward + position;
            
            Vector3 xColor = new Vector3(0.6f, 0.2f, 0.1f);
            Vector3 yColor = new Vector3(0.1f, 0.6f, 0.2f);
            Vector3 zColor = new Vector3(0.2f, 0.1f, 0.6f);
            
            Vector3 xScreenPos = WorldToScreen(x, view, projection, new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            Vector3 yScreenPos = WorldToScreen(y, view, projection, new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            Vector3 zScreenPos = WorldToScreen(z, view, projection, new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            
            Vector3 currentWorldPos = ScreenToWorld(Input.MousePosition, view, projection, new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            
            Vector3 deltaWorld = currentWorldPos - _lastMouseWorldPos;
            
            _lastMouseWorldPos = currentWorldPos;
            
            deltaWorld *= 4.0f; // Speed
            
            bool xHovered = IsMouseHoveringBall(Input.MousePosition, xScreenPos,
                0.3f / (x - view.ExtractTranslation()).Length(),
                new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            
            bool yHovered = IsMouseHoveringBall(Input.MousePosition, yScreenPos, 
                0.3f / (y - view.ExtractTranslation()).Length(),
                new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            
            bool zHovered = IsMouseHoveringBall(Input.MousePosition, zScreenPos, 
                0.3f / (z - view.ExtractTranslation()).Length(),
                new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));

            bool xDrag = false;
            bool yDrag = false;
            bool zDrag = false;
            
            if (xHovered)
            {
                xColor = xColor * 1.5f;

                if (Input.GetMouseButtonDown(MouseButton.Left))
                {
                    xDrag = true;
                }
            }
            else if (yHovered)
            {
                yColor = yColor * 1.5f;
                
                if (Input.GetMouseButtonDown(MouseButton.Left))
                {
                    yDrag = true;
                }
            }
            else if (zHovered)
            {
                zColor = zColor * 1.5f;

                if (Input.GetMouseButtonDown(MouseButton.Left))
                {
                    zDrag = true;
                }
            }
            
            if (xDrag || _rotateXDraggedLastFrame)
            {
                model.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, -deltaWorld.X);
                
                result = true;
            }
            
            if (yDrag || _rotateYDraggedLastFrame)
            {
                model.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, deltaWorld.Y);
                
                result = true;
            }
            
            if (zDrag || _rotateZDraggedLastFrame)
            {
                model.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, deltaWorld.Z);
                
                result = true;
            }
            
            if (Input.GetMouseButtonUp(MouseButton.Left))
            {
                _rotateXDraggedLastFrame = false;
                _rotateYDraggedLastFrame = false;
                _rotateZDraggedLastFrame = false;
            }
            
            if (xDrag)
                _rotateXDraggedLastFrame = true;
            if (yDrag)
                _rotateYDraggedLastFrame = true;
            if (zDrag)
                _rotateZDraggedLastFrame = true;

            sgTransform nm = model;
            
            _drawCalls.Add(() =>
            {
                Rendering.DrawLine(x, x + nm.Up, xColor, view, projection, _renderPositionAndSize);
                Rendering.DrawLine(y, y + nm.Forward, yColor, view, projection, _renderPositionAndSize);
                Rendering.DrawLine(z, z + nm.Right, zColor, view, projection, _renderPositionAndSize);
                
                Rendering.DrawLine(position, position + nm.Up, xColor, view, projection, _renderPositionAndSize);
                Rendering.DrawLine(position, position + nm.Forward, yColor, view, projection, _renderPositionAndSize);
                Rendering.DrawLine(position, position + nm.Right, zColor, view, projection, _renderPositionAndSize);
                
                Rendering.DrawBall(x, 0.3f, xColor, view, projection, _renderPositionAndSize);
                
                Rendering.DrawBall(y, 0.3f, yColor, view, projection, _renderPositionAndSize);
                
                Rendering.DrawBall(z, 0.3f, zColor, view, projection, _renderPositionAndSize);
                
                Rendering.DrawWireCube(position + nm.Forward * 1.3f, 0.1f, new Vector3(0.6f), view, projection, _renderPositionAndSize);
            });
        }

        if (operation == OPERATION.TRANSLATE)
        {
            // 3 arrows, x, y, z
            // + 3 squares, that allow for 2D movement in the x, y, z plane
            
            Vector3 position = model.Position;
            Vector3 forward = model.Forward;
            Vector3 up = model.Up;
            Vector3 right = model.Right;
            
            Vector3 x = right + position;
            Vector3 y = up + position;
            Vector3 z = forward + position;
            
            if (mode == MODE.LOCAL)
            {
                x = right + position;
                y = up + position;
                z = forward + position;
            }
            else
            {
                x = new Vector3(1, 0, 0) + position;
                y = new Vector3(0, 1, 0) + position;
                z = new Vector3(0, 0, 1) + position;
            }
            
            Vector3 xColor = new Vector3(0.6f, 0.2f, 0.1f);
            Vector3 yColor = new Vector3(0.1f, 0.6f, 0.2f);
            Vector3 zColor = new Vector3(0.2f, 0.1f, 0.6f);
            
            Vector3 xScreenPos = WorldToScreen(x, view, projection, new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            Vector3 yScreenPos = WorldToScreen(y, view, projection, new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            Vector3 zScreenPos = WorldToScreen(z, view, projection, new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            
            Vector3 currentWorldPos = ScreenToWorld(Input.MousePosition, view, projection, new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            
            Vector3 deltaWorld = currentWorldPos - _lastMouseWorldPos;
            
            _lastMouseWorldPos = currentWorldPos;
            

            bool xHovered = IsMouseHoveringBall(Input.MousePosition, xScreenPos,
                0.3f / (x - view.ExtractTranslation()).Length(),
                new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            
            bool yHovered = IsMouseHoveringBall(Input.MousePosition, yScreenPos,
                0.3f / (y - view.ExtractTranslation()).Length(),
                new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));
            
            bool zHovered = IsMouseHoveringBall(Input.MousePosition, zScreenPos,
                0.3f / (z - view.ExtractTranslation()).Length(),
                new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W));

            if (!xHovered)
                xHovered = IsMouseHoveringLine(Input.MousePosition, position, x, 4,
                    new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W), view, projection);
            
            if (!yHovered)
                yHovered = IsMouseHoveringLine(Input.MousePosition, position, y, 4,
                    new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W), view, projection);
            
            if (!zHovered)
                zHovered = IsMouseHoveringLine(Input.MousePosition, position, z, 4,
                    new Vector2(_renderPositionAndSize.Z, _renderPositionAndSize.W), view, projection);

            deltaWorld *= 4.0f; // Speed
            
            bool xDrag = false;
            bool yDrag = false;
            bool zDrag = false;
            
            if (xHovered)
            {
                xColor = xColor * 1.5f;

                if (Input.GetMouseButtonDown(MouseButton.Left))
                {
                    xDrag = true;
                }
            }
            else if (yHovered)
            {
                yColor = yColor * 1.5f;
                
                if (Input.GetMouseButtonDown(MouseButton.Left))
                {
                    yDrag = true;
                }
            }
            else if (zHovered)
            {
                zColor = zColor * 1.5f;

                if (Input.GetMouseButtonDown(MouseButton.Left))
                {
                    zDrag = true;
                }

            }

            if (zDrag || _translateZDraggedLastFrame)
            {
                if (mode == MODE.LOCAL)
                {
                    model.Position += model.Forward * deltaWorld.Z;
                }
                else
                {
                    model.Position += new Vector3(0, 0, deltaWorld.Z);
                }

                result = true;
            }
            
            if (xDrag || _translateXDraggedLastFrame)
            {
                if (mode == MODE.LOCAL)
                {
                    model.Position -= model.Right * deltaWorld.X;
                }
                else
                {
                    model.Position += new Vector3(deltaWorld.X, 0, 0);
                }
                result = true;
            }
            
            if (yDrag || _translateYDraggedLastFrame)
            {
                if (mode == MODE.LOCAL)
                {
                    model.Position += model.Up * deltaWorld.Y;
                }
                else
                {
                    model.Position += new Vector3(0, deltaWorld.Y, 0);
                }
                result = true;
            }
            
            if (Input.GetMouseButtonUp(MouseButton.Left))
            {
                _translateXDraggedLastFrame = false;
                _translateYDraggedLastFrame = false;
                _translateZDraggedLastFrame = false;
            }
            
            if (xDrag)
                _translateXDraggedLastFrame = true;
            if (yDrag)
                _translateYDraggedLastFrame = true;
            if (zDrag)
                _translateZDraggedLastFrame = true;

            _drawCalls.Add(() =>
            {
                Rendering.DrawLine(position, x, xColor, view, projection, _renderPositionAndSize);
                Rendering.DrawLine(position, y, yColor, view, projection, _renderPositionAndSize);
                Rendering.DrawLine(position, z, zColor, view, projection, _renderPositionAndSize);
                
                Rendering.DrawBall(x, 0.3f, xColor, view, projection, _renderPositionAndSize);
                Rendering.DrawBall(y, 0.3f, yColor, view, projection, _renderPositionAndSize);
                Rendering.DrawBall(z, 0.3f, zColor, view, projection, _renderPositionAndSize);
            });
        }
        
        return result;
    }

    private static Vector3 ScreenToWorld(Vector2 vector2, Matrix4x4 view, Matrix4x4 projection, Vector2 vector3)
    {
        Vector4 clipSpace = new Vector4(vector2.X / vector3.X * 2 - 1, 1 - vector2.Y / vector3.Y * 2, 0, 1);
        Matrix4x4.Invert(projection, out Matrix4x4 invertedProjection);
        Vector4 viewSpace = Vector4.Transform(clipSpace, invertedProjection);
        viewSpace.Z = 1;
        viewSpace.W = 0;
        Matrix4x4.Invert(view, out Matrix4x4 invertedView);
        Vector4 worldSpace = Vector4.Transform(viewSpace, invertedView);
        return new Vector3(worldSpace.X, worldSpace.Y, worldSpace.Z);
    }

    static Vector3 WorldToScreen(Vector3 worldPos, Matrix4x4 view, Matrix4x4 projection, Vector2 windowSize)
    {
        Vector4 clipSpace = Vector4.Transform(new Vector4(worldPos, 1.0f), view * projection);
        Vector3 ndc = new Vector3(clipSpace.X, clipSpace.Y, clipSpace.Z) / clipSpace.W; // Normalize
        Vector3 windowSpace = new Vector3((ndc.X + 1) / 2.0f * windowSize.X, (1 - ndc.Y) / 2.0f * windowSize.Y, ndc.Z);
        return windowSpace;
    }
    
    static bool IsMouseHoveringBall(Vector2 mousePos, Vector3 ballScreenPos, float ballSize, Vector2 windowSize)
    {
        float dx = mousePos.X - ballScreenPos.X;
        float dy = mousePos.Y - ballScreenPos.Y;
        float distanceSquared = dx * dx + dy * dy;
        float ballRadiusScreenSpace = (ballSize / ballScreenPos.Z) * (windowSize.X / 2); // Approximation

        return distanceSquared <= ballRadiusScreenSpace * ballRadiusScreenSpace;
    }

    static bool IsMouseHoveringLine(Vector2 mousePos, Vector3 lineStart, Vector3 lineEnd, int amount,
        Vector2 windowSize, Matrix4x4 view, Matrix4x4 projection)
    {
        // Divide the line into segments
        Vector3 delta = (lineEnd - lineStart) / amount;
        for (int i = 0; i < amount; i++)
        {
            Vector3 segmentStart = lineStart + delta * i;
            Vector3 segmentEnd = lineStart + delta * (i + 1);
            
            Vector3 segmentMid = (segmentStart + segmentEnd) / 2;
            float segmentSize = (segmentEnd - segmentStart).Length();
            
            var screenPos = WorldToScreen(segmentMid, view, projection, windowSize);
            
            if (IsMouseHoveringBall(mousePos, screenPos, (segmentSize*2f) / (segmentMid - view.ExtractTranslation()).Length() , windowSize))
            {
                return true;
            }
        }
        
        return false;
    }

// Helper function to project 3D point to screen space (assuming perspective projection)
    static Vector2 ProjectToScreen(Vector3 worldPos, Vector2 windowSize)
    {
        float screenW = windowSize.X;
        float screenH = windowSize.Y;

        // Assuming perspective projection with near plane = 0.1 and far plane = 1000
        float nearPlane = 0.1f;
        float farPlane = 1000f;

        float w = worldPos.X / (worldPos.Z * (1f / nearPlane - 1f / farPlane) + 1f);
        float h = worldPos.Y / (worldPos.Z * (1f / nearPlane - 1f / farPlane) + 1f);

        return new Vector2(w * screenW / 2f + screenW / 2f, h * screenH / 2f + screenH / 2f);    }

    public static void Render()
    {
        foreach (Action drawCall in _drawCalls)
        {
            drawCall();
        }
        
        _drawCalls.Clear();
    }

    public static void Update()
    {
        Input.SetInput(_input);
    }

    public static void SetResolution(int sizeX, int sizeY)
    {
        _renderPositionAndSize = new Vector4(0, 0, sizeX, sizeY);
    }

    public static void SetOffset(float f, float f1)
    {
        _renderPositionAndSize.X = f;
        _renderPositionAndSize.Y = f1;
    }

    public static void SetLineThickness(float i)
    {
        Rendering.SetLineThickness(i);
    }
}

public enum OPERATION
{
    TRANSLATE,
    ROTATE,
    SCALE,
    SHOW,
}

public enum MODE
{
    LOCAL,
    WORLD
}