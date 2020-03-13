using UnityEngine;

namespace LSL4Unity.Scripts
{
	/// <summary>
	/// This singleton should provide an dedicated timestamp for each update call or fixed update LSL sample!
	/// So that each sample provided by an Unity3D app has the same timestamp 
	/// Important! Make sure that the script is called before the default execution order!
	/// </summary>
	[ScriptOrder(-1000)]
	public class LSLTimeSync : MonoBehaviour
	{
		public static LSLTimeSync Instance { get; private set; }

		public double FixedUpdateTimeStamp { get; private set; }
		public double UpdateTimeStamp      { get; private set; }
		public double LateUpdateTimeStamp  { get; private set; }

		private void Awake() { Instance = this; }

		private void FixedUpdate() { FixedUpdateTimeStamp = liblsl.LocalClock(); }

		private void Update() { UpdateTimeStamp = liblsl.LocalClock(); }

		private void LateUpdate() { LateUpdateTimeStamp = liblsl.LocalClock(); }
	}
}
