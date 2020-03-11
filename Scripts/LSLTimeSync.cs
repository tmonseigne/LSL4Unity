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
		private static LSLTimeSync _instance;
		public static  LSLTimeSync Instance { get { return _instance; } }

		private double _fixedUpdateTimeStamp;
		public  double FixedUpdateTimeStamp { get { return _fixedUpdateTimeStamp; } }

		private double _updateTimeStamp;
		public  double UpdateTimeStamp { get { return _updateTimeStamp; } }

		private double _lateUpdateTimeStamp;
		public  double LateUpdateTimeStamp { get { return _lateUpdateTimeStamp; } }

		void Awake() { LSLTimeSync._instance = this; }

		void FixedUpdate() { _fixedUpdateTimeStamp = liblsl.local_clock(); }

		void Update() { _updateTimeStamp = liblsl.local_clock(); }

		void LateUpdate() { _lateUpdateTimeStamp = liblsl.local_clock(); }
	}
}
