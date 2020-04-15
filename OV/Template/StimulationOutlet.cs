using UnityEngine;

namespace LSL4Unity.OV.Template
{
	/// <summary> Implementation for a Inlet receiving Stimulations (int) from OpenViBE. </summary>
	/// <seealso cref="OVIntOutlet" />
	public class StimulationOutlet : OVIntOutlet
	{
		/// <inheritdoc cref="OVIntOutlet.Process"/>
		protected override void Process(int[] input) { samples = input; }
	}
}
