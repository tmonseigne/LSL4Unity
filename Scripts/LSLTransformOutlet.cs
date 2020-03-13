using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LSL4Unity
{
	/// <summary> An reusable example of an outlet which provides the orientation and world position of an entity of an Unity Scene to LSL. </summary>
	public class LSLTransformOutlet : MonoBehaviour
	{
		private const string UNIQUE_SOURCE_ID_SUFFIX = "63CE5B03731944F6AC30DBB04B451A94";
		private       string uniqueSourceId;
		private       int    channelCount = 0;

		private liblsl.StreamOutlet outlet;
		private liblsl.StreamInfo   streamInfo;

		/// <summary> Use a array to reduce allocation costs and reuse it for each sampling call. </summary>
		private float[] sample;


		public string    streamName           = "BeMoBI.Unity.Orientation.<Add_a_entity_id_here>";
		public string    streamType           = "Unity.Quaternion";
		public bool      rotationAsQuaternion = true;
		public bool      rotationAsEuler      = true;
		public bool      position             = true;
		public Transform sampleSource;

		/// <summary> Due to an instable framerate we assume a irregular data rate. </summary>
		private const double DATA_RATE = liblsl.IRREGULAR_RATE;

		private void Awake()
		{
			// assigning a unique source id as a combination of a the instance ID for the case that
			// multiple LSLTransformOutlet are used and a guid identifing the script itself.
			uniqueSourceId = $"{GetInstanceID()}_{UNIQUE_SOURCE_ID_SUFFIX}";
		}

		private void Start()
		{
			var channelDefinitions = SetupChannels();
			// initialize the array once
			channelCount = channelDefinitions.Count;
			sample       = new float[channelCount];
			streamInfo   = new liblsl.StreamInfo(streamName, streamType, channelCount, DATA_RATE, liblsl.channel_format_t.cf_float32, uniqueSourceId);

			// it's not possible to create a XMLElement before and append it.
			var chns = streamInfo.Desc().AppendChild("channels");
			// so this workaround has been introduced.
			foreach (var def in channelDefinitions)
			{
				chns.AppendChild("channel").AppendChildValue("label", def.label).AppendChildValue("unit", def.unit).AppendChildValue("type", def.type);
			}

			outlet = new liblsl.StreamOutlet(streamInfo);
		}

		/// <summary>
		/// Sampling on Late Update to make sure the transform recieved all updates
		/// </summary>
		private void LateUpdate()
		{
			if (outlet == null) { return; }
			Sample();
		}

		private void Sample()
		{
			int offset = -1;

			if (rotationAsQuaternion)
			{
				var rotation = sampleSource.rotation;

				sample[++offset] = rotation.x;
				sample[++offset] = rotation.y;
				sample[++offset] = rotation.z;
				sample[++offset] = rotation.w;
			}
			if (rotationAsEuler)
			{
				var rotation = sampleSource.rotation.eulerAngles;

				sample[++offset] = rotation.x;
				sample[++offset] = rotation.y;
				sample[++offset] = rotation.z;
			}
			if (position)
			{
				var pos = sampleSource.position;

				sample[++offset] = pos.x;
				sample[++offset] = pos.y;
				sample[++offset] = pos.z;
			}

			outlet.PushSample(sample, liblsl.LocalClock());
		}


		#region workaround for channel creation

		private ICollection<ChannelDefinition> SetupChannels()
		{
			var list = new List<ChannelDefinition>();

			if (rotationAsQuaternion)
			{
				string[] quatlabels = { "x", "y", "z", "w" };

				list.AddRange(quatlabels.Select(item => new ChannelDefinition { label = item, unit = "unit quaternion", type = "quaternion component" }));
			}

			if (rotationAsEuler)
			{
				string[] eulerLabels = { "x", "y", "z" };

				list.AddRange(eulerLabels.Select(item => new ChannelDefinition { label = item, unit = "degree", type = "axis angle" }));
			}


			if (position)
			{
				string[] eulerLabels = { "x", "y", "z" };

				list.AddRange(eulerLabels.Select(item => new ChannelDefinition { label = item, unit = "meter", type = "position in world space" }));
			}

			return list;
		}

		#endregion
	}
}
