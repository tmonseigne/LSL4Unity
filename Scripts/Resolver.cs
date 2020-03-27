using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LSL4Unity
{
	/// <summary> Encapsulates the lookup logic for LSL streams with an event based appraoch your custom stream inlet implementations could be subscribed to the On. </summary>
	public class Resolver : MonoBehaviour, IEventSystemHandler
	{
		public StreamEvent onStreamFound     = new StreamEvent();
		public StreamEvent onStreamLost      = new StreamEvent();
		public float       forgetStreamAfter = 1.0f;

		public  List<LSLStreamInfoWrapper> streams;
		private liblsl.ContinuousResolver  resolver;

		/// <summary> Use this for initialization. </summary>
		private void Start()
		{
			resolver = new liblsl.ContinuousResolver(forgetStreamAfter);
			StartCoroutine(ResolveContinuously());
		}

		public bool IsStreamAvailable(out LSLStreamInfoWrapper info, string streamName = "", string streamType = "", string hostName = "")
		{
			var result = streams.Where(i => (streamName.Length == 0 || i.name.Equals(streamName)) && (streamType.Length == 0 || i.type.Equals(streamType))
																								  && (hostName.Length == 0 || i.type.Equals(hostName)))
				.ToList();

			if (result.Any())
			{
				info = result.First();
				return true;
			}
			info = null;
			return false;
		}

		private IEnumerator ResolveContinuously()
		{
			while (true)
			{
				var results = resolver.Results();

				//foreach (var item in streams) {
				//if (!results.Any(r => r.Name().Equals(item.name))) { 
				//if (onStreamLost.GetPersistentEventCount() > 0) { onStreamLost.Invoke(item); } } }
				//Replace by LINQ Expression

				foreach (LSLStreamInfoWrapper item in streams.Where(item => !results.Any(r => r.Name().Equals(item.name)))
					.Where(item => onStreamLost.GetPersistentEventCount() > 0)) { onStreamLost.Invoke(item); }

				// remove lost streams from cache
				streams.RemoveAll(s => !results.Any(r => r.Name().Equals(s.name)));

				// add new found streams to the cache
				foreach (var item in results)
				{
					if (!streams.Any(s => s.name == item.Name() && s.type == item.Type()))
					{
						Debug.Log($"Found new Stream {item.Name()}");

						var newStreamInfo = new LSLStreamInfoWrapper(item);
						streams.Add(newStreamInfo);

						if (onStreamFound.GetPersistentEventCount() > 0) { onStreamFound.Invoke(newStreamInfo); }
					}
				}
				yield return new WaitForSecondsRealtime(0.1f);
			}
		}
	}

	[Serializable]
	public class LSLStreamInfoWrapper
	{
		public string name;
		public string type;

		public liblsl.StreamInfo Item { get; }

		public string StreamUid     { get; }
		public int    ChannelCount  { get; }
		public string SessionId     { get; }
		public string SourceId      { get; }
		public string HostName      { get; }
		public double DataRate      { get; }
		public int    StreamVersion { get; }

		public LSLStreamInfoWrapper(liblsl.StreamInfo item)
		{
			Item          = item;
			name          = item.Name();
			type          = item.Type();
			ChannelCount  = item.ChannelCount();
			StreamUid     = item.Uid();
			SessionId     = item.SessionId();
			SourceId      = item.SourceId();
			DataRate      = item.Sampling();
			HostName      = item.Hostname();
			StreamVersion = item.Version();
		}
	}

	[Serializable]
	public class StreamEvent : UnityEvent<LSLStreamInfoWrapper> { }
}
