﻿using UnityEngine;

namespace LSL4Unity.Scripts.Examples
{
	/// <summary>
	/// Example that works with the Resolver component.
	/// This script waits for the resolver to resolve a Stream which matches the Name and Type.
	/// See the base class for more details. 
	/// 
	/// The specific implementation should only deal with the moment when the samples need to be pulled
	/// and how they should processed in your game logic
	///
	/// </summary>
	public class DemoInletForFloatSamples : InletFloatSamples
	{
		public Transform TargetTransform;

		public bool UseX;
		public bool UseY;
		public bool UseZ;

		private bool _pullSamplesContinuously = false;

		//void Start()
		//{
		//	[optional] call this only, if your gameobject hosting this component
		//	got instantiated during runtime
		//	registerAndLookUpStream();
		//}

		protected override bool IsTheExpected(LSLStreamInfoWrapper stream)
		{
			// the base implementation just checks for stream name and type
			var predicate = base.IsTheExpected(stream);
			// add a more specific description for your stream here specifying hostname etc.
			//predicate &= stream.HostName.Equals("Expected Hostname");
			return predicate;
		}

		/// <summary>
		/// Override this method to implement whatever should happen with the samples...
		/// IMPORTANT: Avoid heavy processing logic within this method, update a state and use
		/// coroutines for more complexe processing tasks to distribute processing time over
		/// several frames
		/// </summary>
		/// <param name="sample"></param>
		/// <param name="time"></param>
		protected override void Process(float[] sample, double time)
		{
			//Assuming that a sample contains at least 3 values for x,y,z
			float x = UseX ? sample[0] : 1;
			float y = UseY ? sample[1] : 1;
			float z = UseZ ? sample[2] : 1;

			// we map the data to the scale factors
			var targetScale = new Vector3(x, y, z);

			// apply the rotation to the target transform
			TargetTransform.localScale = targetScale;
		}

		protected override void OnStreamAvailable() { _pullSamplesContinuously = true; }

		protected override void OnStreamLost() { _pullSamplesContinuously = false; }

		private void Update()
		{
			if (_pullSamplesContinuously) { PullSamples(); }
		}
	}
}
