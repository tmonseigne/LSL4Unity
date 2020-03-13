using UnityEngine;

namespace LSL4Unity.Scripts.OV.Template
{
	/// <summary> Implementation for a Inlet receiving Stimulations (int) from OpenViBE. </summary>
	/// <seealso cref="OVFloatInlet" />
	public class StimulationInlet : OVIntInlet
	{
		public int[] LastSample;

		/// <inheritdoc cref="OVIntInlet.Process"/>
		protected override void Process(int[] sample, double time)
		{
			LastSample = sample;
			Debug.Log($"Got {sample.Length} ints at {time}");
		}
	}
}
