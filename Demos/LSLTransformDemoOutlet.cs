using LSL4Unity.Scripts;
using UnityEngine;

namespace LSL4Unity.Demos
{
	/// <summary>
	/// An reusable example of an outlet which provides the orientation of an entity to LSL
	/// </summary>
	public class LSLTransformDemoOutlet : MonoBehaviour
	{
		private const string UNIQUE_SOURCE_ID = "D256CFBDBA3145978CFA641403219531";

		private liblsl.StreamOutlet _outlet;
		private liblsl.StreamInfo   _streamInfo;
		public  liblsl.StreamInfo   GetStreamInfo() { return _streamInfo; }

		/// <summary> Use a array to reduce allocation costs. </summary>
		private float[] _currentSample;

		private double _dataRate;

		public double GetDataRate() { return _dataRate; }
		public bool   HasConsumer() { return _outlet != null && _outlet.HaveConsumers(); }

		public string StreamName   = "BeMoBI.Unity.Orientation.<Add_a_entity_id_here>";
		public string StreamType   = "Unity.Quaternion";
		public int    ChannelCount = 4;

		public MomentForSampling Sampling;

		public Transform SampleSource;

		private void Start()
		{
			// initialize the array once
			_currentSample = new float[ChannelCount];
			_dataRate      = LSLUtils.GetSamplingRateFor(Sampling);
			_streamInfo    = new liblsl.StreamInfo(StreamName, StreamType, ChannelCount, _dataRate, liblsl.channel_format_t.cf_float32, UNIQUE_SOURCE_ID);
			_outlet        = new liblsl.StreamOutlet(_streamInfo);
		}

		private void PushSample()
		{
			if (_outlet == null) { return; }
			var rotation = SampleSource.rotation;

			// reuse the array for each sample to reduce allocation costs
			_currentSample[0] = rotation.x;
			_currentSample[1] = rotation.y;
			_currentSample[2] = rotation.z;
			_currentSample[3] = rotation.w;

			_outlet.PushSample(_currentSample, liblsl.LocalClock());
		}

		private void FixedUpdate()
		{
			if (Sampling == MomentForSampling.FixedUpdate) { PushSample(); }
		}

		private void Update()
		{
			if (Sampling == MomentForSampling.Update) { PushSample(); }
		}

		private void LateUpdate()
		{
			if (Sampling == MomentForSampling.LateUpdate) { PushSample(); }
		}
	}
}
