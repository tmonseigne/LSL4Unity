using System;
using UnityEngine;

namespace LSL4Unity.Scripts.OV.Template
{
	/// <summary> Just an example implementation for a Inlet receiving double values for OpenViBE Link. </summary>
	/// <seealso cref="OVFloatInlet" />
	public class DoubleInlet : OVDoubleInlet
	{
		public double[] LastSample;

		/// <inheritdoc cref="OVDoubleInlet.Process"/>
		protected override void Process(double[] sample, double time) { LastSample = sample; }
	}
}
