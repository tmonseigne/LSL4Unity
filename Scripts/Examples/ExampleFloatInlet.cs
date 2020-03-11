using UnityEngine;

namespace LSL4Unity.Scripts.Examples
{
	/// <summary> Just an example implementation for a Inlet recieving float values. </summary>
	public class ExampleFloatInlet : AFloatInlet
	{
		public float[] LastSample;

		protected override void Process(float[] sample, double time)
		{
			LastSample = sample;
			Debug.Log($"Got {sample.Length} samples at {time}");
		}
	}
}
