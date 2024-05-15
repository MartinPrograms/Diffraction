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
			if (ImGui.CollapsingHeader(s.Path))
			{
				if (s == null)
				{
					ImGui.Text("Script is null");
					return;
				}
				
				if (s.Parent.GetObject() == null)
				{
					ImGui.Text("Parent is null");
					return;
				}
				
				ImGui.Text("Parent: " + s.Parent.GetObject().Name);
				ImGui.Text("Path: " + s.Path);
				ImGui.Text("Name: " + s.Name);
				
				ImGui.Columns(2);
				if (ImGui.Button("Reload"))
				{
					string text = System.IO.File.ReadAllText(s.Path);
					s.Reload(text);
				}
				
				ImGui.NextColumn();
				
				if (ImGui.Button("Open in VSC"))
				{
					Utilities.LaunchVSCode(s.Path);
				}
				
				ImGui.NextColumn();
				ImGui.Columns(1);
				
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