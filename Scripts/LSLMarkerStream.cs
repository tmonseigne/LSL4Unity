using System.Collections;
using UnityEngine;

namespace LSL4Unity.Scripts
{
	[HelpURL("https://github.com/xfleckx/LSL4Unity/wiki#using-a-marker-stream")]
	public class LSLMarkerStream : MonoBehaviour
	{
		private const string UNIQUE_SOURCE_ID = "D3F83BB699EB49AB94A9FA44B88882AB";

		public string LSLStreamName = "Unity_<Paradigma_Name_here>";
		public string LSLStreamType = "LSL_Marker_Strings";

		private liblsl.StreamInfo   _lslStreamInfo;
		private liblsl.StreamOutlet _lslOutlet;
		private int                 _lslChannelCount = 1;

		//Assuming that markers are never send in regular intervalls
		private double _nominalSrate = liblsl.IRREGULAR_RATE;

		private const liblsl.channel_format_t LSL_CHANNEL_FORMAT = liblsl.channel_format_t.cf_string;

		private string[] _sample;

		private void Awake()
		{
			_sample        = new string[_lslChannelCount];
			_lslStreamInfo = new liblsl.StreamInfo(LSLStreamName, LSLStreamType, _lslChannelCount, _nominalSrate, LSL_CHANNEL_FORMAT, UNIQUE_SOURCE_ID);
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
