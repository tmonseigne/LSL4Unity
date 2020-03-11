using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace LSL4Unity.Editor
{
	public class BuildHooks
	{
		private const string LIB_LSL_NAME = "liblsl";
		private const string PLUGIN_DIR   = "Plugins";

		[PostProcessBuild(1)]
		public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
		{
			var buildName = Path.GetFileNameWithoutExtension(pathToBuiltProject);

			var buildHostDirectory = pathToBuiltProject.Replace(Path.GetFileName(pathToBuiltProject), "");

			var dataDirectoryName = buildName + "_Data";

			var pathToDataDirectory = Path.Combine(buildHostDirectory, dataDirectoryName);

			var pluginDirectory = Path.Combine(pathToDataDirectory, PLUGIN_DIR);

			switch (target)
			{
				case BuildTarget.StandaloneWindows:
					RenameLibFile(pluginDirectory, LSLEditorIntegration.LIB32_NAME, LSLEditorIntegration.LIB64_NAME, LSLEditorIntegration.DLL_ENDING);
					break;
				case BuildTarget.StandaloneWindows64:
					RenameLibFile(pluginDirectory, LSLEditorIntegration.LIB64_NAME, LSLEditorIntegration.LIB32_NAME, LSLEditorIntegration.DLL_ENDING);
					break;
				case BuildTarget.StandaloneLinux64:
					RenameLibFile(pluginDirectory, LSLEditorIntegration.LIB64_NAME, LSLEditorIntegration.LIB32_NAME, LSLEditorIntegration.SO_ENDING);
					break;
				case BuildTarget.StandaloneOSX:
					RenameLibFile(pluginDirectory, LSLEditorIntegration.LIB64_NAME, LSLEditorIntegration.LIB32_NAME, LSLEditorIntegration.BUNDLE_ENDING);
					break;
			}
		}

		private static void RenameLibFile(string pluginDirectory, string sourceName, string nameOfObsoleteFile, string fileEnding)
		{
			var obsoleteFile = Path.Combine(pluginDirectory, nameOfObsoleteFile + fileEnding);

			Debug.Log("[LSL BUILD Hook] Delete obsolete file: " + obsoleteFile);

			File.Delete(obsoleteFile);

			var sourceFile = Path.Combine(pluginDirectory, sourceName + fileEnding);

			var targetFile = Path.Combine(pluginDirectory, LIB_LSL_NAME + fileEnding);

			Debug.Log($"[LSL BUILD Hook] Renaming: {sourceFile} to {targetFile}");

			File.Move(sourceFile, targetFile);
		}
	}
}
