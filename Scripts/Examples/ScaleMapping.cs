using UnityEngine;

namespace LSL4Unity.Examples
{
	public class ScaleMapping : AFloatInlet
	{
		public Transform targetTransform;
		public bool      useX, useY, useZ;

		protected override void Process(float[] sample, double time)
		{
			//Assuming that a sample contains at least 3 values for x,y,z
			float x = useX ? sample[0] : 1;
			float y = useY ? sample[1] : 1;
			float z = useZ ? sample[2] : 1;

			// we map the data to the scale factors
			var targetScale = new Vector3(x, y, z);

			// apply the rotation to the target transform
			targetTransform.localScale = targetScale;
		}
	}
}
