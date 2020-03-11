using System.Collections;
using UnityEngine;

namespace LSL4Unity.Scripts
{
	[HelpURL("https://github.com/xfleckx/LSL4Unity/wiki#using-a-marker-stream")]
	public class LSLMarkerStream : MonoBehaviour
	{
		public string LSLStreamName = "Unity_<Paradigma_Name_here>";
		public string LSLStreamType = "LSL_Marker_Strings";

		private const string UNIQUE_SOURCE_ID  = "D3F83BB699EB49AB94A9FA44B88882AB";
		private const int    LSL_CHANNEL_COUNT = 1;
		private const double NOMINAL_SRATE     = liblsl.IRREGULAR_RATE;

		private const liblsl.channel_format_t LSL_CHANNEL_FORMAT = liblsl.channel_format_t.cf_string;

		private liblsl.StreamInfo   _lslStreamInfo;
		private liblsl.StreamOutlet _lslOutlet;

		//Assuming that markers are never send in regular intervalls
		private string[] _sample;

		private void Awake()
		{
			_sample        = new string[LSL_CHANNEL_COUNT];
			_lslStreamInfo = new liblsl.StreamInfo(LSLStreamName, LSLStreamType, LSL_CHANNEL_COUNT, NOMINAL_SRATE, LSL_CHANNEL_FORMAT, UNIQUE_SOURCE_ID);
			_lslOutlet     = new liblsl.StreamOutlet(_lslStreamInfo);
		}

		public void Write(string marker)
		{
			_sample[0] = marker;
			_lslOutlet.push_sample(_sample);
		}

		public void Write(string marker, double customTimeStamp)
		{
			_sample[0] = marker;
			_lslOutlet.push_sample(_sample, customTimeStamp);
		}

		public void Write(string marker, float customTimeStamp)
		{
			_sample[0] = marker;
			_lslOutlet.push_sample(_sample, customTimeStamp);
		}

		public void WriteBeforeFrameIsDisplayed(string marker) { StartCoroutine(WriteMarkerAfterImageIsRendered(marker)); }

		private IEnumerator WriteMarkerAfterImageIsRendered(string pendingMarker)
		{
			yield return new WaitForEndOfFrame();
			Write(pendingMarker);
			yield return null;
		}
	}
}
