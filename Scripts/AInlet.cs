using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LSL4Unity.Scripts.OV;
using UnityEngine;

namespace LSL4Unity.Scripts
{
	/// <summary> Float Inlet. </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public abstract class AFloatInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment Moment;

		public string StreamName;
		public string StreamType;

		private liblsl.StreamInfo[]       _results;
		private liblsl.StreamInlet        _inlet;
		private liblsl.ContinuousResolver _resolver;

		private int _expectedChannels = 0;

		private float[] _sample;

		private void Start()
		{
			var hasAName = StreamName.Length != 0;
			var hasAType = StreamType.Length != 0;

			if (!hasAName && !hasAType)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			if (hasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + StreamName);
				_resolver = new liblsl.ContinuousResolver("name", StreamName);
			}
			else // if (expectedStreamHasAType) // Useless with the first if
			{
				Debug.Log("Creating LSL resolver for stream with type " + StreamType);
				_resolver = new liblsl.ContinuousResolver("type ", StreamType);
			}

			StartCoroutine(ResolveExpectedStream());

			AdditionalStart();
		}

		/// <summary> Override this method in the subclass to specify what should happen during Start(). </summary>
		protected void AdditionalStart() { } //By default, do nothing.

		private IEnumerator ResolveExpectedStream()
		{
			var results = _resolver.Results();

			yield return new WaitUntil(() => results.Length > 0);

			Debug.Log($"Resolving Stream: {StreamName}");

			_inlet = new liblsl.StreamInlet(results[0]);

			_expectedChannels = _inlet.Info().ChannelCount();

			yield return null;
		}

		protected void PullSamples()
		{
			_sample = new float[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.PullSample(_sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = _inlet.PullSample(_sample, 0.0f)) > Constants.TOLERANCE) { Process(_sample, lastTimeStamp); }
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
		/// <param name="sample"> The Incomming Sample. </param>
		/// <param name="time"> The current Time. </param>
		protected abstract void Process(float[] sample, double time);

		private void FixedUpdate()
		{
			if (Moment == UpdateMoment.FixedUpdate && _inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (Moment == UpdateMoment.Update && _inlet != null) { PullSamples(); }
		}
	}

	/// <summary> Double Inlet. </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public abstract class ADoubleInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment Moment;

		public string StreamName;
		public string StreamType;

		private liblsl.StreamInfo[]       _results;
		private liblsl.StreamInlet        _inlet;
		private liblsl.ContinuousResolver _resolver;

		private int _expectedChannels = 0;

		private double[] _sample;

		private void Start()
		{
			var expectedStreamHasAName = !StreamName.Equals("");
			var expectedStreamHasAType = !StreamType.Equals("");

			if (!expectedStreamHasAName && !expectedStreamHasAType)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			if (expectedStreamHasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + StreamName);
				_resolver = new liblsl.ContinuousResolver("name", StreamName);
			}
			else // if (expectedStreamHasAType) // Useless with the first if
			{
				Debug.Log("Creating LSL resolver for stream with type " + StreamType);
				_resolver = new liblsl.ContinuousResolver("type", StreamType);
			}

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

				_expectedChannels = _inlet.Info().ChannelCount();
			}

			yield return null;
		}

		private liblsl.StreamInfo GetStreamInfoFrom(IEnumerable<liblsl.StreamInfo> results)
		{
			var targetInfo = results.First(r => r.Name().Equals(StreamName));
			return targetInfo;
		}

		protected void PullSamples()
		{
			_sample = new double[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.PullSample(_sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = _inlet.PullSample(_sample, 0.0f)) > Constants.TOLERANCE) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <inheritdoc cref="AFloatInlet.Process"/>
		protected abstract void Process(double[] sample, double time);

		private void FixedUpdate()
		{
			if (Moment == UpdateMoment.FixedUpdate && _inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (Moment == UpdateMoment.Update && _inlet != null) { PullSamples(); }
		}
	}

	/// <summary> Char Inlet. </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public abstract class ACharInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment Moment;

		public string StreamName;
		public string StreamType;

		private liblsl.StreamInfo[]       _results;
		private liblsl.StreamInlet        _inlet;
		private liblsl.ContinuousResolver _resolver;

		private int _expectedChannels = 0;

		private char[] _sample;

		private void Start()
		{
			var expectedStreamHasAName = !StreamName.Equals("");
			var expectedStreamHasAType = !StreamType.Equals("");

			if (!expectedStreamHasAName && !expectedStreamHasAType)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			if (expectedStreamHasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + StreamName);
				_resolver = new liblsl.ContinuousResolver("name", StreamName);
			}
			else // if (expectedStreamHasAType) // Useless with the first if
			{
				Debug.Log("Creating LSL resolver for stream with type " + StreamType);
				_resolver = new liblsl.ContinuousResolver("type", StreamType);
			}

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

			_expectedChannels = _inlet.Info().ChannelCount();

			yield return null;
		}

		protected void PullSamples()
		{
			_sample = new char[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.PullSample(_sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = _inlet.PullSample(_sample, 0.0f)) > Constants.TOLERANCE) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <inheritdoc cref="AFloatInlet.Process"/>
		protected abstract void Process(char[] sample, double time);

		private void FixedUpdate()
		{
			if (Moment == UpdateMoment.FixedUpdate && _inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (Moment == UpdateMoment.Update && _inlet != null) { PullSamples(); }
		}
	}

	/// <summary> Float Inlet. </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public abstract class AShortInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment Moment;

		public string StreamName;
		public string StreamType;

		private liblsl.StreamInfo[]       _results;
		private liblsl.StreamInlet        _inlet;
		private liblsl.ContinuousResolver _resolver;

		private int _expectedChannels = 0;

		private short[] _sample;

		private void Start()
		{
			var expectedStreamHasAName = !StreamName.Equals("");
			var expectedStreamHasAType = !StreamType.Equals("");

			if (!expectedStreamHasAName && !expectedStreamHasAType)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			if (expectedStreamHasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + StreamName);
				_resolver = new liblsl.ContinuousResolver("name", StreamName);
			}
			else // if (expectedStreamHasAType) // Useless with the first if
			{
				Debug.Log("Creating LSL resolver for stream with type " + StreamType);
				_resolver = new liblsl.ContinuousResolver("type", StreamType);
			}

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

			_expectedChannels = _inlet.Info().ChannelCount();

			yield return null;
		}

		protected void PullSamples()
		{
			_sample = new short[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.PullSample(_sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = _inlet.PullSample(_sample, 0.0f)) > Constants.TOLERANCE) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <inheritdoc cref="AFloatInlet.Process"/>
		protected abstract void Process(short[] sample, double time);

		private void FixedUpdate()
		{
			if (Moment == UpdateMoment.FixedUpdate && _inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (Moment == UpdateMoment.Update && _inlet != null) { PullSamples(); }
		}
	}

	/// <summary> Int Inlet. </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public abstract class AIntInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment Moment;

		public string StreamName;
		public string StreamType;

		private liblsl.StreamInfo[]       _results;
		private liblsl.StreamInlet        _inlet;
		private liblsl.ContinuousResolver _resolver;

		private int _expectedChannels = 0;

		private int[] _sample;

		private void Start()
		{
			var expectedStreamHasAName = !StreamName.Equals("");
			var expectedStreamHasAType = !StreamType.Equals("");

			if (!expectedStreamHasAName && !expectedStreamHasAType)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			if (expectedStreamHasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + StreamName);
				_resolver = new liblsl.ContinuousResolver("name", StreamName);
			}
			else // if (expectedStreamHasAType) // Useless with the first if
			{
				Debug.Log("Creating LSL resolver for stream with type " + StreamType);
				_resolver = new liblsl.ContinuousResolver("type", StreamType);
			}

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

			_expectedChannels = _inlet.Info().ChannelCount();

			yield return null;
		}

		protected void PullSamples()
		{
			_sample = new int[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.PullSample(_sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = _inlet.PullSample(_sample, 0.0f)) > Constants.TOLERANCE) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <inheritdoc cref="AFloatInlet.Process"/>
		protected abstract void Process(int[] sample, double time);

		private void FixedUpdate()
		{
			if (Moment == UpdateMoment.FixedUpdate && _inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (Moment == UpdateMoment.Update && _inlet != null) { PullSamples(); }
		}
	}

	/// <summary> String Inlet. </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public abstract class AStringInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment Moment;

		public string StreamName;
		public string StreamType;

		private liblsl.StreamInfo[]       _results;
		private liblsl.StreamInlet        _inlet;
		private liblsl.ContinuousResolver _resolver;

		private int _expectedChannels = 0;

		private string[] _sample;

		private void Start()
		{
			var expectedStreamHasAName = !StreamName.Equals("");
			var expectedStreamHasAType = !StreamType.Equals("");

			if (!expectedStreamHasAName && !expectedStreamHasAType)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			if (expectedStreamHasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + StreamName);
				_resolver = new liblsl.ContinuousResolver("name", StreamName);
			}
			else // if (expectedStreamHasAType) // Useless with the first if
			{
				Debug.Log("Creating LSL resolver for stream with type " + StreamType);
				_resolver = new liblsl.ContinuousResolver("type", StreamType);
			}

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

			_expectedChannels = _inlet.Info().ChannelCount();

			yield return null;
		}

		protected void PullSamples()
		{
			_sample = new string[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.PullSample(_sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = _inlet.PullSample(_sample, 0.0f)) > Constants.TOLERANCE) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <inheritdoc cref="AFloatInlet.Process"/>
		protected abstract void Process(string[] sample, double time);

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
