using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace LSL4Unity.Editor
{
	public class LSLEditorIntegration
	{
		public const string LIB64_NAME    = "liblsl64";
		public const string LIB32_NAME    = "liblsl32";
		public const string DLL_ENDING    = ".dll";
		public const string SO_ENDING     = ".so";
		public const string BUNDLE_ENDING = ".bundle";

		private const string WIKI_URL         = "https://github.com/xfleckx/LSL4Unity/wiki";
		private const string WRAPPER_FILENAME = "LSL.cs";
		private const string ASSET_SUB_FOLDER = "LSL4Unity";
		private const string LIB_FOLDER       = "Plugins";

		[MenuItem("LSL/Show Streams")]
		private static void OpenLSLWindow()
		{
			var window = EditorWindow.GetWindow<LSLShowStreamsWindow>(true);
			window.Init();
			window.ShowUtility();
		}

		[MenuItem("LSL/Show Streams", true)]
		private static bool ValidateOpenLSLWindow()
		{
			string assetDirectory = Application.dataPath;

			var results = Directory.GetDirectories(assetDirectory, ASSET_SUB_FOLDER, SearchOption.AllDirectories);

			Assert.IsTrue(results.Any(),
						  "Expecting a directory named: '" + ASSET_SUB_FOLDER + "' containing the content inlcuding this script! Did you renamed it?");

			var root = results.Single();

			bool lib32Available = File.Exists(Path.Combine(root, Path.Combine(LIB_FOLDER, LIB32_NAME + DLL_ENDING)));
			bool lib64Available = File.Exists(Path.Combine(root, Path.Combine(LIB_FOLDER, LIB64_NAME + DLL_ENDING)));

			lib32Available &= File.Exists(Path.Combine(root, Path.Combine(LIB_FOLDER, LIB32_NAME + SO_ENDING)));
			lib64Available &= File.Exists(Path.Combine(root, Path.Combine(LIB_FOLDER, LIB64_NAME + SO_ENDING)));
			lib32Available &= File.Exists(Path.Combine(root, Path.Combine(LIB_FOLDER, LIB32_NAME + BUNDLE_ENDING)));
			lib64Available &= File.Exists(Path.Combine(root, Path.Combine(LIB_FOLDER, LIB64_NAME + BUNDLE_ENDING)));

			bool apiAvailable = File.Exists(Path.Combine(root, WRAPPER_FILENAME));

			if ((lib64Available || lib32Available) && apiAvailable) { return true; }

			Debug.LogError("LabStreamingLayer libraries not available! See " + WIKI_URL + " for installation instructions");
			return false;
		}
	}
}
