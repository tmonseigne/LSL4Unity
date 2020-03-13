using System;
using UnityEditor;

namespace LSL4Unity.Editor
{
	[InitializeOnLoad]
	internal class ScriptOrderManagement
	{
		static ScriptOrderManagement()
		{
			foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
			{
				if (monoScript.GetClass() != null)
				{
					foreach (Attribute a in Attribute.GetCustomAttributes(monoScript.GetClass(), typeof(ScriptOrder)))
					{
						int currentOrder = MonoImporter.GetExecutionOrder(monoScript);
						int newOrder     = ((ScriptOrder)a).order;
						if (currentOrder != newOrder) { MonoImporter.SetExecutionOrder(monoScript, newOrder); }
					}
				}
			}
		}
	}
}
