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
		public static void OnPostprocessBuild(BuildTarget target, string buildPath)
		{
			if (buildPath != null)
			{
				var buildDir     = Path.GetFileNameWithoutExtension(buildPath);
				var buildHostDir = buildPath.Replace(Path.GetFileName(buildPath), "");
				var dataDir      = buildDir + "_Data";
				var dataPath     = Path.Combine(buildHostDir, dataDir);
				var pluginDir    = Path.Combine(dataPath,     PLUGIN_DIR);

				switch (target)
				{
					case BuildTarget.StandaloneWindows:
						RenameLibFile(pluginDir, LSLEditorIntegration.LIB32_NAME, LSLEditorIntegration.LIB64_NAME, LSLEditorIntegration.DLL_ENDING);
						break;
					case BuildTarget.StandaloneWindows64:
						RenameLibFile(pluginDir, LSLEditorIntegration.LIB64_NAME, LSLEditorIntegration.LIB32_NAME, LSLEditorIntegration.DLL_ENDING);
						break;
					case BuildTarget.StandaloneLinux64:
						RenameLibFile(pluginDir, LSLEditorIntegration.LIB64_NAME, LSLEditorIntegration.LIB32_NAME, LSLEditorIntegration.SO_ENDING);
						break;
					case BuildTarget.StandaloneOSX:
						RenameLibFile(pluginDir, LSLEditorIntegration.LIB64_NAME, LSLEditorIntegration.LIB32_NAME, LSLEditorIntegration.BUNDLE_ENDING);
						break;
				}
			}
		}

		private static void RenameLibFile(string pluginDir, string srcName, string oldName, string extension)
		{
			var oldFile = Path.Combine(pluginDir, oldName + extension);
			Debug.Log("[LSL BUILD Hook] Delete obsolete file: " + oldFile);
			File.Delete(oldFile);

			var srcFile = Path.Combine(pluginDir, srcName + extension);
			var dstFile = Path.Combine(pluginDir, LIB_LSL_NAME + extension);
			Debug.Log($"[LSL BUILD Hook] Renaming: {srcFile} to {dstFile}");
			File.Move(srcFile, dstFile);
		}
	}
}
