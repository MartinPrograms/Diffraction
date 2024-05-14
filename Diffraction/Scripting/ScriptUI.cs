using Diffraction.Rendering;
using Diffraction.Scripting.Globals;
using ImGuiNET;

namespace Diffraction.Scripting;
 
public class ScriptUI : EventObject
{
	public override void Render(Camera camera)
	{
		ImGui.Begin("Script Editor");
		if (ObjectScene.Instance.SelectedObject == null)
		{
			ImGui.Text("No object selected");
		}
		else
		{
			RenderScripts();
		}
		
		ImGui.End();
	}

	private void RenderScripts()
	{
		var scripts = ScriptUtils.Find(ObjectScene.Instance.SelectedObject.Id);

		foreach (Script s in scripts)
		{
			// Show the path, and some properties (like the parent, scripts global variables, etc.)
			if (ImGui.CollapsingHeader("Script Data"))
			{
				// Reload button, open in vscode button
				ImGui.Columns(2);
				
				if (ImGui.Button("Reload"))
				{
					s.Reload();
				}
				
				ImGui.NextColumn();
				
				if (ImGui.Button("Open in VSCode"))
				{
					Utilities.LaunchVSCode(s.Path);
				}
				
				// end the header
				
				ImGui.Columns(1);
				ImGui.Separator();
				ImGui.NewLine();
				
			}
			
			if (ImGui.CollapsingHeader(s.Path))
			{
				ImGui.Text("Parent: " + s.Parent.GetObject().Name);
				ImGui.Text("Path: " + s.Path);
				ImGui.Text("Name: " + s.Name);
				
				ImGui.NewLine();
				ImGui.Text("Global Variables");
				ImGui.Columns(3); // Name, Type, Value
				
				ImGui.Text("Name");
				ImGui.NextColumn();
				
				ImGui.Text("Type");
				ImGui.NextColumn();
				
				ImGui.Text("Value");
				ImGui.NextColumn();
				
				ImGui.Separator();

				foreach (var variable in s.GetVariables())
				{
					ImGui.Text(variable.Key);
					ImGui.NextColumn();
					
					ImGui.Text(variable.Value.GetType().Name);
					ImGui.NextColumn();
					
					ImGui.Text(variable.Value.ToString());
					ImGui.NextColumn();
					
					ImGui.Separator();
				}
				
				
				ImGui.Columns(1);
				ImGui.Separator();
				ImGui.NewLine();
				
			}
		}
	}
}