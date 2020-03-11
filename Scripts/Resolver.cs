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
		public List<LSLStreamInfoWrapper> KnownStreams;

		public float ForgetStreamAfter = 1.0f;

		private liblsl.ContinuousResolver _resolver;

		private bool _resolve = true;

		public StreamEvent OnStreamFound = new StreamEvent();
		public StreamEvent OnStreamLost = new StreamEvent();

		// Use this for initialization
		void Start()
		{
			_resolver = new liblsl.ContinuousResolver(ForgetStreamAfter);

			StartCoroutine(ResolveContinuously());
		}

		public bool IsStreamAvailable(out LSLStreamInfoWrapper info, string streamName = "", string streamType = "", string hostName = "")
		{
			var result = KnownStreams.Where(i => (streamName == "" || i.Name.Equals(streamName)) && (streamType == "" || i.Type.Equals(streamType))
																								 && (hostName == "" || i.Type.Equals(hostName)));

			if (result.Any())
			{
				info = result.First();
				return true;
			}
			else
			{
				info = null;
				return false;
			}
		}

		private IEnumerator ResolveContinuously()
		{
			while (_resolve)
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
			yield return null;
		}
	}

	[Serializable]
	public class LSLStreamInfoWrapper
	{
		public string Name;

		public string Type;

		private          liblsl.StreamInfo _item;
		private readonly string            _streamUid;

		private readonly int    _channelCount;
		private readonly string _sessionId;
		private readonly string _sourceId;
		private readonly double _dataRate;
		private readonly string _hostName;
		private readonly int    _streamVersion;

		public LSLStreamInfoWrapper(liblsl.StreamInfo item)
		{
			_item     = item;
			Name          = item.Name();
			Type          = item.Type();
			_channelCount  = item.channel_count();
			_streamUid     = item.Uid();
			_sessionId     = item.session_id();
			_sourceId      = item.source_id();
			_dataRate      = item.nominal_srate();
			_hostName      = item.Hostname();
			_streamVersion = item.Version();
		}

		public liblsl.StreamInfo Item { get { return _item; } }

		public string StreamUid { get { return _streamUid; } }

		public int ChannelCount { get { return _channelCount; } }

		public string SessionId { get { return _sessionId; } }

		public string SourceId { get { return _sourceId; } }

		public string HostName { get { return _hostName; } }

		public double DataRate { get { return _dataRate; } }

		public int StreamVersion { get { return _streamVersion; } }
	}

	[Serializable]
	public class StreamEvent : UnityEvent<LSLStreamInfoWrapper> { }
}
