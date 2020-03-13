using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LSL4Unity.Scripts
{
	/// <summary>
	/// Encapsulates the lookup logic for LSL streams with an event based appraoch
	/// your custom stream inlet implementations could be subscribed to the On
	/// </summary>
	public class Resolver : MonoBehaviour, IEventSystemHandler
	{
		public StreamEvent OnStreamFound     = new StreamEvent();
		public StreamEvent OnStreamLost      = new StreamEvent();
		public float       ForgetStreamAfter = 1.0f;

		public  List<LSLStreamInfoWrapper> KnownStreams;
		private liblsl.ContinuousResolver  _resolver;

		// Use this for initialization
		private void Start()
		{
			_resolver = new liblsl.ContinuousResolver(ForgetStreamAfter);

			StartCoroutine(ResolveContinuously());
		}

		public bool IsStreamAvailable(out LSLStreamInfoWrapper info, string streamName = "", string streamType = "", string hostName = "")
		{
			var result = KnownStreams.Where(i => (streamName == "" || i.Name.Equals(streamName)) && (streamType == "" || i.Type.Equals(streamType))
																								 && (hostName == "" || i.Type.Equals(hostName))).ToList();

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
				var results = _resolver.Results();

				foreach (var item in KnownStreams)
				{
					if (!results.Any(r => r.Name().Equals(item.Name)))
					{
						if (OnStreamLost.GetPersistentEventCount() > 0) { OnStreamLost.Invoke(item); }
					}
				}

				// remove lost streams from cache
				KnownStreams.RemoveAll(s => !results.Any(r => r.Name().Equals(s.Name)));

				// add new found streams to the cache
				foreach (var item in results)
				{
					if (!KnownStreams.Any(s => s.Name == item.Name() && s.Type == item.Type()))
					{
						Debug.Log($"Found new Stream {item.Name()}");

						var newStreamInfo = new LSLStreamInfoWrapper(item);
						KnownStreams.Add(newStreamInfo);

						if (OnStreamFound.GetPersistentEventCount() > 0) { OnStreamFound.Invoke(newStreamInfo); }
					}
				}
				yield return new WaitForSecondsRealtime(0.1f);
			}
		}
	}

	[Serializable]
	public class LSLStreamInfoWrapper
	{
		public string Name;
		public string Type;

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
			Name          = item.Name();
			Type          = item.Type();
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
