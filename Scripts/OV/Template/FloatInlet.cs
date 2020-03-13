using System;
using UnityEngine;

namespace LSL4Unity.Scripts.OV.Template
{
	/// <summary> Just an example implementation for a Inlet receiving Float values for OpenViBE Link. </summary>
	/// <seealso cref="OVFloatInlet" />
	public class FloatInlet : OVFloatInlet
	{
		public float[] LastSample;

		/// <inheritdoc cref="OVFloatInlet.Process"/>
		protected override void Process(float[] sample, double time) { LastSample = sample; }
	}
}
