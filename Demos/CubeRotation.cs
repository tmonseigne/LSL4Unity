using UnityEngine;

namespace LSL4Unity.Demos
{
	public class CubeRotation : MonoBehaviour
	{
		private float _yawSpeed   = 1.0f;
		private float _pitchSpeed = 1.0f;
		private float _rollSpeed  = 1.0f;

		void Update()
		{
			if (Input.GetKey("a")) { _yawSpeed                  += 1; }
			if (Input.GetKey("d") && _yawSpeed > 0) { _yawSpeed -= 1; }

			if (Input.GetKey("w")) { _pitchSpeed                    += 1; }
			if (Input.GetKey("s") && _pitchSpeed > 0) { _pitchSpeed -= 1; }

			if (Input.GetKey("e")) { _rollSpeed                   += 1; }
			if (Input.GetKey("q") && _rollSpeed > 0) { _rollSpeed -= 1; }

			transform.rotation *= Quaternion.Euler(_yawSpeed * Time.deltaTime, _pitchSpeed * Time.deltaTime, _rollSpeed * Time.deltaTime);
		}
	}
}
