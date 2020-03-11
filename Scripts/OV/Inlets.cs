using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LSL4Unity.Scripts.OV
{
	public abstract class OVFloatInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment Moment;

		public string StreamName = "ovSignal";

		private liblsl.StreamInfo[]       _results;
		private liblsl.StreamInlet        _inlet;
		private liblsl.ContinuousResolver _resolver;

		private int     _expectedChannels = 0;
		private float[] _sample;

		private void Start()
		{
			var expectedStreamHasAName = !StreamName.Equals("");

			if (!expectedStreamHasAName)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			Debug.Log("Creating LSL resolver for stream " + StreamName);
			_resolver = new liblsl.ContinuousResolver("name", StreamName);

			StartCoroutine(ResolveExpectedStream());
			AdditionalStart();
		}

		/// <summary> Override this method in the subclass to specify what should happen during Start(). </summary>
		protected virtual void AdditionalStart() { } //By default, do nothing.

		private IEnumerator ResolveExpectedStream()
		{
			var results = _resolver.Results();

			yield return new WaitUntil(() => results.Length > 0);

			Debug.Log($"Resolving Stream: {StreamName}");

			_inlet = new liblsl.StreamInlet(results[0]);

			_expectedChannels = _inlet.Info().channel_count();

			yield return null;
		}

		private void PullSamples()
		{
			_sample = new float[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.pull_sample(_sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = _inlet.pull_sample(_sample, 0.0f)) > Constants.TOLERANCE) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <summary> Override this method in the subclass to specify what should happen when samples are available. </summary>
		/// <param name="sample"></param>
		/// <param name="time"></param>
		protected abstract void Process(float[]sample, double time);

		private void FixedUpdate()
		{
			if (Moment == UpdateMoment.FixedUpdate && _inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (Moment == UpdateMoment.Update && _inlet != null) { PullSamples(); }
		}
	}

	public abstract class OVDoubleInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment Moment;

		public string StreamName;

		private liblsl.StreamInfo[]       _results;
		private liblsl.StreamInlet        _inlet;
		private liblsl.ContinuousResolver _resolver;

		private int _expectedChannels = 0;

		private double[] _sample;

		private void Start()
		{
			var expectedStreamHasAName = !StreamName.Equals("");

			if (!expectedStreamHasAName)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			Debug.Log("Creating LSL resolver for stream " + StreamName);
			_resolver = new liblsl.ContinuousResolver("name", StreamName);

			StartCoroutine(ResolveExpectedStream());
			AdditionalStart();
		}

		/// <summary> Override this method in the subclass to specify what should happen during Start(). </summary>
		protected virtual void AdditionalStart() { } //By default, do nothing.

		private IEnumerator ResolveExpectedStream()
		{
			var results = _resolver.Results();

			while (_inlet == null)
			{
				yield return new WaitUntil(() => results.Length > 0);

				_inlet = new liblsl.StreamInlet(GetStreamInfoFrom(results));

				_expectedChannels = _inlet.Info().channel_count();
			}

			yield return null;
		}

		private liblsl.StreamInfo GetStreamInfoFrom(IEnumerable<liblsl.StreamInfo> results)
		{
			var targetInfo = results.First(r => r.Name().Equals(StreamName));
			return targetInfo;
		}

		private void PullSamples()
		{
			_sample = new double[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.pull_sample(_sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = _inlet.pull_sample(_sample, 0.0f)) > Constants.TOLERANCE) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <summary> Override this method in the subclass to specify what should happen when samples are available. </summary>
		/// <param name="sample"></param>
		/// <param name="time"></param>
		protected abstract void Process(double[]sample, double time);

		private void FixedUpdate()
		{
			if (Moment == UpdateMoment.FixedUpdate && _inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (Moment == UpdateMoment.Update && _inlet != null) { PullSamples(); }
		}
	}

	public abstract class OVCharInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment Moment;

		public string StreamName;

		private liblsl.StreamInfo[]       _results;
		private liblsl.StreamInlet        _inlet;
		private liblsl.ContinuousResolver _resolver;

		private int _expectedChannels = 0;

		private char[] _sample;

		private void Start()
		{
			var expectedStreamHasAName = !StreamName.Equals("");

			if (!expectedStreamHasAName)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}
			Debug.Log("Creating LSL resolver for stream " + StreamName);
			_resolver = new liblsl.ContinuousResolver("name", StreamName);

			StartCoroutine(ResolveExpectedStream());
			AdditionalStart();
		}

		/// <summary> Override this method in the subclass to specify what should happen during Start(). </summary>
		protected virtual void AdditionalStart() { } //By default, do nothing.

		private IEnumerator ResolveExpectedStream()
		{
			var results = _resolver.Results();

			yield return new WaitUntil(() => results.Length > 0);

			_inlet = new liblsl.StreamInlet(results[0]);

			_expectedChannels = _inlet.Info().channel_count();

			yield return null;
		}

		private void PullSamples()
		{
			_sample = new char[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.pull_sample(_sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = _inlet.pull_sample(_sample, 0.0f)) > Constants.TOLERANCE) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <summary> Override this method in the subclass to specify what should happen when samples are available. </summary>
		/// <param name="sample"></param>
		/// <param name="time"></param>
		protected abstract void Process(char[]sample, double time);

		private void FixedUpdate()
		{
			if (Moment == UpdateMoment.FixedUpdate && _inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (Moment == UpdateMoment.Update && _inlet != null) { PullSamples(); }
		}
	}

	public abstract class OVShortInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment Moment;

		public string StreamName;

		private liblsl.StreamInfo[]       _results;
		private liblsl.StreamInlet        _inlet;
		private liblsl.ContinuousResolver _resolver;

		private int _expectedChannels = 0;

		private short[] _sample;

		private void Start()
		{
			if (StreamName.Equals(""))
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			Debug.Log("Creating LSL resolver for stream " + StreamName);
			_resolver = new liblsl.ContinuousResolver("name", StreamName);

			StartCoroutine(ResolveExpectedStream());

			AdditionalStart();
		}

		/// <summary> Override this method in the subclass to specify what should happen during Start(). </summary>
		protected virtual void AdditionalStart() { } //By default, do nothing.

		private IEnumerator ResolveExpectedStream()
		{
			var results = _resolver.Results();

			yield return new WaitUntil(() => results.Length > 0);

			_inlet = new liblsl.StreamInlet(results[0]);

			_expectedChannels = _inlet.Info().channel_count();

			yield return null;
		}

		private void PullSamples()
		{
			_sample = new short[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.pull_sample(_sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = _inlet.pull_sample(_sample, 0.0f)) > Constants.TOLERANCE) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <summary> Override this method in the subclass to specify what should happen when samples are available. </summary>
		/// <param name="sample"></param>
		/// <param name="time"></param>
		protected abstract void Process(short[]sample, double time);

		private void FixedUpdate()
		{
			if (Moment == UpdateMoment.FixedUpdate && _inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (Moment == UpdateMoment.Update && _inlet != null) { PullSamples(); }
		}
	}

	public abstract class OVIntInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment Moment;

		public string StreamName;

		private liblsl.StreamInfo[]       _results;
		private liblsl.StreamInlet        _inlet;
		private liblsl.ContinuousResolver _resolver;

		private int _expectedChannels = 0;

		private int[] _sample;

		private void Start()
		{
			if (StreamName.Equals(""))
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			Debug.Log("Creating LSL resolver for stream " + StreamName);
			_resolver = new liblsl.ContinuousResolver("name", StreamName);

			StartCoroutine(ResolveExpectedStream());

			AdditionalStart();
		}

		/// <summary> Override this method in the subclass to specify what should happen during Start(). </summary>
		protected virtual void AdditionalStart() { } //By default, do nothing.

		private IEnumerator ResolveExpectedStream()
		{
			var results = _resolver.Results();

			yield return new WaitUntil(() => results.Length > 0);

			_inlet = new liblsl.StreamInlet(results[0]);

			_expectedChannels = _inlet.Info().channel_count();

			yield return null;
		}

		private void PullSamples()
		{
			_sample = new int[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.pull_sample(_sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = _inlet.pull_sample(_sample, 0.0f)) > Constants.TOLERANCE) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <summary> Override this method in the subclass to specify what should happen when samples are available. </summary>
		/// <param name="sample"></param>
		/// <param name="time"></param>
		protected abstract void Process(int[]sample, double time);

		private void FixedUpdate()
		{
			if (Moment == UpdateMoment.FixedUpdate && _inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (Moment == UpdateMoment.Update && _inlet != null) { PullSamples(); }
		}
	}

	public abstract class OVStringInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment Moment;

		public string StreamName;

		private liblsl.StreamInfo[]       _results;
		private liblsl.StreamInlet        _inlet;
		private liblsl.ContinuousResolver _resolver;

		private int _expectedChannels = 0;

		private string[] _sample;

		private void Start()
		{
			if (StreamName.Equals(""))
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			Debug.Log("Creating LSL resolver for stream " + StreamName);
			_resolver = new liblsl.ContinuousResolver("name", StreamName);

			StartCoroutine(ResolveExpectedStream());

			AdditionalStart();
		}

		/// <summary> Override this method in the subclass to specify what should happen during Start(). </summary>
		protected virtual void AdditionalStart() { } //By default, do nothing.

		private IEnumerator ResolveExpectedStream()
		{
			var results = _resolver.Results();

			yield return new WaitUntil(() => results.Length > 0);

			_inlet = new liblsl.StreamInlet(results[0]);

			_expectedChannels = _inlet.Info().channel_count();

			yield return null;
		}

		private void PullSamples()
		{
			_sample = new string[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.pull_sample(_sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = _inlet.pull_sample(_sample, 0.0f)) > Constants.TOLERANCE) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <summary> Override this method in the subclass to specify what should happen when samples are available. </summary>
		/// <param name="sample"></param>
		/// <param name="time"></param>
		protected abstract void Process(string[]sample, double time);

		private void FixedUpdate()
		{
			if (Moment == UpdateMoment.FixedUpdate && _inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (Moment == UpdateMoment.Update && _inlet != null) { PullSamples(); }
		}
	}
}
