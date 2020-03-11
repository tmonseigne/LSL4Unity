using UnityEngine;

namespace LSL4Unity.Scripts.Examples
{
	/// <summary> Just an example implementation for a Inlet recieving float values. </summary>
	public class ExampleFloatInlet : AFloatInlet
	{
		public float[] LastSample;

		protected override void Process(float[] newSample, double timeStamp)
		{
			LastSample = newSample;
			Debug.Log($"Got {newSample.Length} samples at {timeStamp}");
		}
	}
}
