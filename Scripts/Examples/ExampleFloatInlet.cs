using System;
using System.Linq;
using UnityEngine;

namespace LSL4Unity.Scripts.Examples
{
	/// <summary>
	/// Just an example implementation for a Inlet recieving float values
	/// </summary>
	public class ExampleFloatInlet : AFloatInlet
	{
		public string LastSample = String.Empty;

		protected override void Process(float[] newSample, double timeStamp)
		{
			// just as an example, make a string out of all channel values of this sample
			LastSample = string.Join(" ", newSample.Select(c => c.ToString()).ToArray());

			Debug.Log($"Got {newSample.Length} samples at {timeStamp}");
		}
	}
}
