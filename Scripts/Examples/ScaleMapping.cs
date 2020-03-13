using UnityEngine;

namespace LSL4Unity.Scripts.Examples
{
	public class ScaleMapping : AFloatInlet
	{
		public Transform TargetTransform;

		public bool UseX, UseY, UseZ;

		protected override void Process(float[] sample, double time)
		{
			//Assuming that a sample contains at least 3 values for x,y,z
			float x = UseX ? sample[0] : 1;
			float y = UseY ? sample[1] : 1;
			float z = UseZ ? sample[2] : 1;

			// we map the data to the scale factors
			var targetScale = new Vector3(x, y, z);

			// apply the rotation to the target transform
			TargetTransform.localScale = targetScale;
		}
	}
}
