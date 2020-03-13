using UnityEngine;

namespace LSL4Unity.Scripts.OV.Template
{
	/// <summary> Implementation for a Inlet receiving Stimulations (int) from OpenViBE. </summary>
	/// <seealso cref="OVFloatInlet" />
	public class StimulationInlet : OVIntInlet
	{
		public int[] LastSample { get; private set; }

		/// <inheritdoc cref="OVIntInlet.Process"/>
		protected override void Process(int[] input, double time)
		{
			LastSample = input;
			Debug.Log($"Got {input.Length} ints at {time}");
		}
	}
}
