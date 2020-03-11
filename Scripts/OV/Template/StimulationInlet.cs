﻿using UnityEngine;

namespace LSL4Unity.Scripts.OV.Template
{
	/// <summary> Just an example implementation for a Inlet recieving float values. </summary>
	public class OVStimulation : OVIntInlet
	{
		public int[] LastSample;

		protected override void Process(int[] newSample, double timeStamp)
		{
			LastSample = newSample;
			Debug.Log($"Got {newSample.Length} ints at {timeStamp}");
		}
	}
}
