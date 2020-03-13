using System.Diagnostics;
using UnityEngine;

namespace LSL4Unity.Scripts
{
	public enum MomentForSampling { Update, FixedUpdate, LateUpdate }


	public class LSLOutlet : MonoBehaviour
	{
		private liblsl.StreamOutlet _outlet;
		private liblsl.StreamInfo   _streamInfo;
		private float[]             _currentSample;

		public string StreamName   = "Unity.ExampleStream";
		public string StreamType   = "Unity.FixedUpdateTime";
		public int    ChannelCount = 1;

		private Stopwatch _watch;

		// Use this for initialization
		private void Start()
		{
			_watch = new Stopwatch();
			_watch.Start();

			_currentSample = new float[ChannelCount];
			_streamInfo    = new liblsl.StreamInfo(StreamName, StreamType, ChannelCount, Time.fixedDeltaTime * 1000);
			_outlet        = new liblsl.StreamOutlet(_streamInfo);
		}

		public void FixedUpdate()
		{
			if (_watch == null) { return; }

			_watch.Stop();

			_currentSample[0] = _watch.ElapsedMilliseconds;

			_watch.Reset();
			_watch.Start();

			_outlet.PushSample(_currentSample);
		}
	}
}
