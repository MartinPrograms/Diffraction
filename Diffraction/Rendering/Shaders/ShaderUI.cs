using System.Numerics;
using System.Text;
using Diffraction.Rendering.Meshes;
using Diffraction.Scripting;
using ImGuiNET;

namespace Diffraction.Rendering.Shaders;

public class ShaderUI : EventObject
{
    public override void Render(Camera camera)
    {
        ImGui.Begin("Shader Editor");
        string errors = CheckErrors();
        
        ImGui.Separator();
        
        if (ImGui.Button("Recompile All Shaders"))
        {
            ShaderUtils.RecompileAll();
        }
        
        ImGui.Text("Errors: " + (errors ?? "None"));
        
        ListShaders();
        
        ImGui.End();
    }

    private string CheckErrors()
    {
        StringBuilder sb = new();
        foreach (Shader shader in ShaderUtils.Shaders.Values)
        {
            if (shader.GetErrors() != null)
            {
                sb.AppendLine("\n" + shader.Name + ": " + shader.GetErrors() + "\n");
            }
        }
        
        return sb.ToString();
    }

    private void ListShaders()
    {
        if (ShaderUtils.Shaders.Count > 0)
        {
            foreach (Shader shader in ShaderUtils.Shaders.Values)
            {
                if (ImGui.CollapsingHeader(shader.Name))
                {
                    var errorsString = shader.GetErrors();

                    ImGui.Text("Shader Type: " + shader.Type);
                    ImGui.Text("Vertex Location: " + shader.GetVertexLocation());
                    ImGui.Text("Fragment Location: " + shader.GetFragmentLocation());
                    ImGui.Text("Compute Location: " + shader.GetComputeLocation());
                    ImGui.Text("Errors: " + (errorsString ?? "None"));
                    
                    MakeTable(shader);
                    
                    ImGui.NewLine();

                    if (ImGui.Button("Recompile"))
                    {
                        shader.Recompile();
                    }

                    ImGui.SameLine();
                    if ((shader.Type & ShaderType.Vertex) != 0)
                    {
                        
                        if (ImGui.Button("Edit Vertex"))
                        {
                            Utilities.LaunchVSCode(shader.GetVertexLocation());
                        }
                        ImGui.SameLine();

                    }
                    if ((shader.Type & ShaderType.Fragment) != 0)
                    {
                        if (ImGui.Button("Edit Fragment"))
                        {
                            Utilities.LaunchVSCode(shader.GetFragmentLocation());
                        }
                        ImGui.SameLine();

                    }
                    if ((shader.Type & ShaderType.Compute) != 0)
                    {
                        if (ImGui.Button("Edit Compute"))
                        {
                            Utilities.LaunchVSCode(shader.GetComputeLocation());
                        }
                        ImGui.SameLine();

                    }
                    ImGui.NewLine();
                }
            }
        }
    }

    private void MakeTable(Shader shader)
    {
        // Make a table on the shader.
        // The table should have inputs, outputs, and uniforms.
        ImGui.NewLine();
        ImGui.Text("Inputs");
        ImGui.Columns(4);
        ImGui.Separator();
        ImGui.Text("Name");
        ImGui.NextColumn();
        ImGui.Text("Type");
        ImGui.NextColumn();
        ImGui.Text("Location");
        ImGui.NextColumn();
        ImGui.Text("Size");
        ImGui.NextColumn();
        ImGui.Separator();
        foreach (ShaderAttribute input in shader.Inputs)
        {
            ImGui.Text(input.Name);
            ImGui.NextColumn();
            ImGui.Text(input.Type.ToString());
            ImGui.NextColumn();
            ImGui.Text(input.Location.ToString());
            ImGui.NextColumn();
            ImGui.Text(input.Size.ToString());
            ImGui.NextColumn();
        }
        ImGui.Columns(1);
        ImGui.Separator();
        
        ImGui.NewLine();
        ImGui.Text("Uniforms");
        ImGui.Columns(4);
        ImGui.Separator();
        ImGui.Text("Name");
        ImGui.NextColumn();
        ImGui.Text("Type");
        ImGui.NextColumn();
        ImGui.Text("Location");
        ImGui.NextColumn();
        ImGui.Text("Size");
        ImGui.NextColumn();
        ImGui.Separator();
        foreach (ShaderAttribute output in shader.Uniforms)
        {
            ImGui.Text(output.Name);
            ImGui.NextColumn();
            ImGui.Text(output.Type.ToString());
            ImGui.NextColumn();
            ImGui.Text(output.Location.ToString());
            ImGui.NextColumn();
            ImGui.Text(output.Size.ToString());
            ImGui.NextColumn();
        }
        ImGui.Columns(1);
        ImGui.Separator();
        
    }
}