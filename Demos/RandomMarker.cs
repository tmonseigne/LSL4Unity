using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace LSL4Unity.Demos
{
	public class RandomMarker : MonoBehaviour
	{
		public LSLMarkerStream stream;

		private void Start()
		{
			Assert.IsNotNull(stream, "You forgot to assign the reference to a marker stream implementation!");

			if (stream != null) { StartCoroutine(WriteContinouslyMarkerEachSecond()); }
		}

		private IEnumerator WriteContinouslyMarkerEachSecond()
		{
			while (true)
			{
				// an example for demonstrating the usage of marker stream
				string currentMarker = GetARandomMarker();
				stream.Write(currentMarker);
				yield return new WaitForSecondsRealtime(1f);
			}
		}

		private static string GetARandomMarker() { return Random.value > 0.5 ? "A" : "B"; }
	}
}
