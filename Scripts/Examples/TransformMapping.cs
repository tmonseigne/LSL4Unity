using UnityEngine;

namespace LSL4Unity.Scripts.Examples
{
	public class TransformMapping : AFloatInlet
	{
		public Transform TargetTransform;

		protected override void Process(float[] sample, double time)
		{
			//Assuming that a sample contains at least 3 values for x,y,z
			float x = sample[0];
			float y = sample[1];
			float z = sample[2];

			// we map the coordinates to a rotation
			var targetRotation = Quaternion.Euler(x, y, z);

			// apply the rotation to the target transform
			TargetTransform.rotation = targetRotation;
		}
	}
}
