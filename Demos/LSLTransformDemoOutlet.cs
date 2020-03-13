using UnityEngine;

namespace LSL4Unity.Demos
{
	/// <summary> An reusable example of an outlet which provides the orientation of an entity to LSL. </summary>
	public class LSLTransformDemoOutlet : MonoBehaviour
	{
		private const string UNIQUE_SOURCE_ID = "D256CFBDBA3145978CFA641403219531";

		private liblsl.StreamOutlet outlet;
		private liblsl.StreamInfo   info;
		//public  liblsl.StreamInfo   GetStreamInfo() { return info; }

		public string streamName   = "BeMoBI.Unity.Orientation.<Add_a_entity_id_here>";
		public string streamType   = "Unity.Quaternion";
		public int    channelCount = 4;

		public MomentForSampling sampling;
		public Transform         sampleSource;

		/// <summary> Use a array to reduce allocation costs. </summary>
		private float[] sample;

		private double dataRate;

		public double GetDataRate() { return dataRate; }
		public bool   HasConsumer() { return outlet != null && outlet.HaveConsumers(); }

		private void Start()
		{
			// initialize the array once
			sample = new float[channelCount];
			dataRate      = LSLUtils.GetSamplingRateFor(sampling);
			info          = new liblsl.StreamInfo(streamName, streamType, channelCount, dataRate, liblsl.channel_format_t.cf_float32, UNIQUE_SOURCE_ID);
			outlet        = new liblsl.StreamOutlet(info);
		}

		private void PushSample()
		{
			if (outlet == null) { return; }
			var rotation = sampleSource.rotation;

			// reuse the array for each sample to reduce allocation costs
			sample[0] = rotation.x;
			sample[1] = rotation.y;
			sample[2] = rotation.z;
			sample[3] = rotation.w;

			outlet.PushSample(sample, liblsl.LocalClock());
		}

		private void FixedUpdate()
		{
			if (sampling == MomentForSampling.FixedUpdate) { PushSample(); }
		}

		private void Update()
		{
			if (sampling == MomentForSampling.Update) { PushSample(); }
		}

		private void LateUpdate()
		{
			if (sampling == MomentForSampling.LateUpdate) { PushSample(); }
		}
	}
}
