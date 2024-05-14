using Diffraction.Serializables;

namespace Diffraction.Scripting;

public static class ScriptUtils
{
	public static Dictionary<string, Script> Scripts = new();
	public static List<sScript> sScripts = new();
	
	public static Script Load(string path, sObject parent)
	{
		Scripts[parent.Id.ToString() + path] = new Script(System.IO.File.ReadAllText(path)){Path = path, Parent = parent};
		sScripts.Add(new sScript(path, parent));
		return new Script(System.IO.File.ReadAllText(path)){Path = path, Parent = parent};
	}
	
	public static IEnumerable<Script> Find(sObject parent)
	{
		// Get all scripts with the parent's ID
		foreach (var script in Scripts)
		{
			if (script.Value.Parent.Id == parent.Id)
			{
				yield return script.Value;
			}
		}
	}

	public static IEnumerable<Script> Find(Guid id)
	{
		return Find(new sObject(id));
	}

	public static IEnumerable<sScript> FindSScript(sObject parent)
	{
		foreach (var script in sScripts)
		{
			if (script.Parent.Id == parent.Id)
			{
				yield return script;
			}
		}
	}
	
	public static IEnumerable<sScript> FindSScript(Guid id)
	{
		return FindSScript(new sObject(id));
	}
}