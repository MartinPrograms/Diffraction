using System.Numerics;
using System.Runtime.InteropServices;
using Diffraction.Rendering.Windowing;
using Silk.NET.OpenGL.Extensions.ImGui;
using SilkyGizmos;
using ImGui = ImGuiNET.ImGui;
using ImGuiCol = ImGuiNET.ImGuiCol;
using ImGuiConfigFlags = ImGuiNET.ImGuiConfigFlags;
using ImGuiIOPtr = ImGuiNET.ImGuiIOPtr;

namespace Diffraction.Rendering.GUI;
            
public class ImGuiInstance
{
    private ImGuiController _controller;
    
    public unsafe ImGuiInstance(Window window)
    {
        _controller = new ImGuiController(window.GL, window.IWindow, window.Input, OnConfigureIo);
        Gizmos.Init(window.GL, _controller.Context, window.IWindow, window.Input);
        
        window.Update += (time) =>
        {        
            _controller.Update((float)time);
        };
        
        window.PreRender += (time) =>
        {
            
        };
        
        window.Render += (time) =>
        {
            var view = Camera.MainCamera.GetViewMatrix();
            var projection = Camera.MainCamera.GetProjectionMatrix();
            
            _controller.Render();
        };
        
        window.Close += () =>
        {
            _controller.Dispose();
        };
    }

    private void OnConfigureIo()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.Fonts.AddFontFromFileTTF("Editor/Fonts/ruda-variable.ttf", 16.0f);
        LoadDefaultSkin();
    }

    public static void LoadDefaultSkin()
    {
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
        {
            //SetColors();
        }
    }

    private static void SetColors()
    {
        var style = ImGui.GetStyle();
        style.WindowPadding = new Vector2(15.0f, 15.0f);
        style.WindowRounding = 5.0f;
        style.FramePadding = new Vector2(5.0f, 5.0f);
        style.FrameRounding = 4.0f;
        style.ItemSpacing = new Vector2(12.0f, 8.0f);
        style.ItemInnerSpacing = new Vector2(8.0f, 6.0f);
        style.IndentSpacing = 25.0f;
        style.ScrollbarSize = 15.0f;
        style.ScrollbarRounding = 9.0f;
        style.GrabMinSize = 5.0f;
        style.GrabRounding = 3.0f;
        style.PopupRounding = 3.0f;
        style.AntiAliasedLines = true;
        style.ChildRounding = 4.0f;
        style.AntiAliasedFill = true;

        var colors = style.Colors;
        colors[(int)ImGuiCol.Text] = new Vector4(0.73f, 0.73f, 0.73f, 1.00f);
        colors[(int)ImGuiCol.Text] = new Vector4(0.80f, 0.80f, 0.83f, 1.00f);
        colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.24f, 0.23f, 0.29f, 1.00f);

        // Background colors
        colors[(int)ImGuiCol.WindowBg] = new Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        colors[(int)ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int)ImGuiCol.PopupBg] = new Vector4(0.07f, 0.07f, 0.09f, 1.00f);

        // Border colors
        colors[(int)ImGuiCol.Border] = new Vector4(0.80f, 0.80f, 0.83f, 0.88f);
        colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.92f, 0.91f, 0.88f, 0.00f);

        // Frame background colors
        colors[(int)ImGuiCol.FrameBg] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.24f, 0.23f, 0.29f, 1.00f);
        colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.56f, 0.56f, 0.58f, 1.00f);

        // Title bar colors
        colors[(int)ImGuiCol.TitleBg] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.07f, 0.07f, 0.09f, 1.00f);
        colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(1.00f, 0.98f, 0.95f, 0.75f);

        // Menu bar colors
        colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);

        // Scrollbar colors
        colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.80f, 0.80f, 0.83f, 0.31f);
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.06f, 0.05f, 0.07f, 1.00f);

        // Misc colors
        colors[(int)ImGuiCol.CheckMark] = new Vector4(0.80f, 0.80f, 0.83f, 0.31f);
        colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.80f, 0.80f, 0.83f, 0.31f);
        colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        colors[(int)ImGuiCol.Button] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.24f, 0.23f, 0.29f, 1.00f);
        colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        colors[(int)ImGuiCol.Header] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        colors[(int)ImGuiCol.Separator] = new Vector4(0.43f, 0.43f, 0.50f, 0.50f);
        colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.10f, 0.40f, 0.75f, 0.78f);
        colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.10f, 0.40f, 0.75f, 1.00f);
        colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        colors[(int)ImGuiCol.Tab] = new Vector4(0.10f, 0.10f, 0.17f, 0.86f);
        colors[(int)ImGuiCol.TabHovered] = new Vector4(0.24f, 0.23f, 0.29f, 1.00f);
        colors[(int)ImGuiCol.TabActive] = new Vector4(0.32f, 0.31f, 0.43f, 1.00f);
        colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.07f, 0.10f, 0.15f, 0.97f);
        colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.14f, 0.26f, 0.42f, 1.00f);
        colors[(int)ImGuiCol.PlotLines] = new Vector4(0.40f, 0.39f, 0.38f, 0.63f);
        colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.25f, 1.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.40f, 0.39f, 0.38f, 0.63f);
        colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(0.25f, 1.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.19f, 0.19f, 0.20f, 1.00f);
        colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.31f, 0.31f, 0.35f, 1.00f);
        colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.23f, 0.23f, 0.25f, 1.00f);
        colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
        colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.25f, 1.00f, 1.00f, 0.43f);
        colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
        colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
        colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
        colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
        colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);

        colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.20f, 0.20f, 0.20f, 0.40f);
    }
}