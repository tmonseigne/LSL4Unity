using System.Collections.Generic;
using UnityEngine;

namespace LSL4Unity.Scripts
{
	/// <summary>
	/// An reusable example of an outlet which provides the orientation and world position of an entity of an Unity Scene to LSL
	/// </summary>
	public class LSLTransformOutlet : MonoBehaviour
	{
		private const string UNIQUE_SOURCE_ID_SUFFIX = "63CE5B03731944F6AC30DBB04B451A94";

		private string _uniqueSourceId;

		private liblsl.StreamOutlet _outlet;
		private liblsl.StreamInfo   _streamInfo;

		private int _channelCount = 0;

		/// <summary>
		/// Use a array to reduce allocation costs
		/// and reuse it for each sampling call
		/// </summary>
		private float[] _currentSample;

		public Transform SampleSource;

		public string StreamName = "BeMoBI.Unity.Orientation.<Add_a_entity_id_here>";
		public string StreamType = "Unity.Quaternion";

		public bool StreamRotationAsQuaternion = true;
		public bool StreamRotationAsEuler      = true;
		public bool StreamPosition             = true;

		/// <summary>
		/// Due to an instable framerate we assume a irregular data rate.
		/// </summary>
		private const double DATA_RATE = liblsl.IRREGULAR_RATE;

		void Awake()
		{
			// assigning a unique source id as a combination of a the instance ID for the case that
			// multiple LSLTransformOutlet are used and a guid identifing the script itself.
			_uniqueSourceId = $"{GetInstanceID()}_{UNIQUE_SOURCE_ID_SUFFIX}";
		}

		void Start()
		{
			var channelDefinitions = SetupChannels();

			_channelCount = channelDefinitions.Count;

			// initialize the array once
			_currentSample = new float[_channelCount];

			_streamInfo = new liblsl.StreamInfo(StreamName, StreamType, _channelCount, DATA_RATE, liblsl.channel_format_t.cf_float32, _uniqueSourceId);

			// it's not possible to create a XMLElement before and append it.
			liblsl.XMLElement chns = _streamInfo.Desc().append_child("channels");
			// so this workaround has been introduced.
			foreach (var def in channelDefinitions)
			{
				chns.append_child("channel").append_child_value("label", def.Label).append_child_value("unit", def.Unit).append_child_value("type", def.Type);
			}

			_outlet = new liblsl.StreamOutlet(_streamInfo);
		}

		/// <summary>
		/// Sampling on Late Update to make sure the transform recieved all updates
		/// </summary>
		void LateUpdate()
		{
			if (_outlet == null) { return; }

			Sample();
		}

		private void Sample()
		{
			int offset = -1;

			if (StreamRotationAsQuaternion)
			{
				var rotation = SampleSource.rotation;

				_currentSample[++offset] = rotation.x;
				_currentSample[++offset] = rotation.y;
				_currentSample[++offset] = rotation.z;
				_currentSample[++offset] = rotation.w;
			}
			if (StreamRotationAsEuler)
			{
				var rotation = SampleSource.rotation.eulerAngles;

				_currentSample[++offset] = rotation.x;
				_currentSample[++offset] = rotation.y;
				_currentSample[++offset] = rotation.z;
			}
			if (StreamPosition)
			{
				var position = SampleSource.position;

				_currentSample[++offset] = position.x;
				_currentSample[++offset] = position.y;
				_currentSample[++offset] = position.z;
			}

			_outlet.push_sample(_currentSample, liblsl.local_clock());
		}


		#region workaround for channel creation

		private ICollection<ChannelDefinition> SetupChannels()
		{
			var list = new List<ChannelDefinition>();

			if (StreamRotationAsQuaternion)
			{
				string[] quatlabels = { "x", "y", "z", "w" };

				foreach (var item in quatlabels)
				{
					var definition = new ChannelDefinition();
					definition.Label = item;
					definition.Unit  = "unit quaternion";
					definition.Type  = "quaternion component";
					list.Add(definition);
				}
			}

			if (StreamRotationAsEuler)
			{
				string[] eulerLabels = { "x", "y", "z" };

				foreach (var item in eulerLabels)
				{
					var definition = new ChannelDefinition();
					definition.Label = item;
					definition.Unit  = "degree";
					definition.Type  = "axis angle";
					list.Add(definition);
				}
			}


			if (StreamPosition)
			{
				string[] eulerLabels = { "x", "y", "z" };

				foreach (var item in eulerLabels)
				{
					var definition = new ChannelDefinition();
					definition.Label = item;
					definition.Unit  = "meter";
					definition.Type  = "position in world space";
					list.Add(definition);
				}
			}

			return list;
		}

		#endregion
	}
}
