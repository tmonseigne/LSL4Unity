using UnityEngine;

namespace LSL4Unity.Examples
{
	/// <summary> Just an example implementation for a Inlet recieving float values. </summary>
	public class ExampleFloatInlet : AFloatInlet
	{
		public float[] lastSample;

		protected override void Process(float[] sample, double time)
		{
			lastSample = sample;
			Debug.Log($"Got {sample.Length} samples at {time}");
		}
	}
}
