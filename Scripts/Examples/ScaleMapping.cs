using UnityEngine;

namespace LSL4Unity.Scripts.Examples
{
	public class ScaleMapping : AFloatInlet
	{
		public Transform TargetTransform;

		public bool UseX, UseY, UseZ;

		protected override void Process(float[] newSample, double timeStamp)
		{
			//Assuming that a sample contains at least 3 values for x,y,z
			float x = UseX ? newSample[0] : 1;
			float y = UseY ? newSample[1] : 1;
			float z = UseZ ? newSample[2] : 1;

			// we map the data to the scale factors
			var targetScale = new Vector3(x, y, z);

			// apply the rotation to the target transform
			TargetTransform.localScale = targetScale;
		}
	}
}
