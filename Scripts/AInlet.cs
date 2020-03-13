using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LSL4Unity.OV;
using UnityEngine;

namespace LSL4Unity
{
	/// <summary> Float Inlet. </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public abstract class AFloatInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment moment;

		public string streamName;
		public string streamType;

		private liblsl.StreamInlet        inlet;
		private liblsl.ContinuousResolver resolver;

		private int expectedChannels = 0;

		private float[] sample;

		private void Start()
		{
			bool hasAName = streamName.Length != 0;
			bool hasAType = streamType.Length != 0;

			if (!hasAName && !hasAType)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			if (hasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + streamName);
				resolver = new liblsl.ContinuousResolver("name", streamName);
			}
			else // if (hasAType) // Useless with the first if
			{
				Debug.Log("Creating LSL resolver for stream with type " + streamType);
				resolver = new liblsl.ContinuousResolver("type ", streamType);
			}

			StartCoroutine(ResolveExpectedStream());

			AdditionalStart();
		}

		/// <summary> Override this method in the subclass to specify what should happen during Start(). </summary>
		protected void AdditionalStart() { } //By default, do nothing.

		private IEnumerator ResolveExpectedStream()
		{
			var results = resolver.Results();

			yield return new WaitUntil(() => results.Length > 0);

			Debug.Log($"Resolving Stream: {streamName}");

			inlet = new liblsl.StreamInlet(results[0]);

			expectedChannels = inlet.Info().ChannelCount();

			yield return null;
		}

		protected void PullSamples()
		{
			sample = new float[expectedChannels];

			try
			{
				double lastTimeStamp = inlet.PullSample(sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = inlet.PullSample(sample, 0.0f)) > Constants.TOLERANCE) { Process(sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException e)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(e, this);
			}
		}

		/// <summary> Override this method in the subclass to specify what should happen when samples are available. </summary>
		/// <param name="sample"> The Incomming Sample. </param>
		/// <param name="time"> The current Time. </param>
		protected abstract void Process(float[] sample, double time);

		private void FixedUpdate()
		{
			if (moment == UpdateMoment.FixedUpdate && inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (moment == UpdateMoment.Update && inlet != null) { PullSamples(); }
		}
	}

	/// <summary> Double Inlet. </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public abstract class ADoubleInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment moment;

		public string streamName;
		public string streamType;

		private liblsl.StreamInlet        inlet;
		private liblsl.ContinuousResolver resolver;

		private int expectedChannels = 0;

		private double[] sample;

		private void Start()
		{
			bool hasAName = streamName.Length != 0;
			bool hasAType = streamType.Length != 0;

			if (!hasAName && !hasAType)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			if (hasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + streamName);
				resolver = new liblsl.ContinuousResolver("name", streamName);
			}
			else // if (hasAType) // Useless with the first if
			{
				Debug.Log("Creating LSL resolver for stream with type " + streamType);
				resolver = new liblsl.ContinuousResolver("type", streamType);
			}

			StartCoroutine(ResolveExpectedStream());

			AdditionalStart();
		}

		/// <summary> Override this method in the subclass to specify what should happen during Start(). </summary>
		protected virtual void AdditionalStart() { } //By default, do nothing.

		private IEnumerator ResolveExpectedStream()
		{
			var results = resolver.Results();

			while (inlet == null)
			{
				yield return new WaitUntil(() => results.Length > 0);

				inlet = new liblsl.StreamInlet(GetStreamInfoFrom(results));

				expectedChannels = inlet.Info().ChannelCount();
			}

			yield return null;
		}

		private liblsl.StreamInfo GetStreamInfoFrom(IEnumerable<liblsl.StreamInfo> results)
		{
			var targetInfo = results.First(r => r.Name().Equals(streamName));
			return targetInfo;
		}

		protected void PullSamples()
		{
			sample = new double[expectedChannels];

			try
			{
				double lastTimeStamp = inlet.PullSample(sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = inlet.PullSample(sample, 0.0f)) > Constants.TOLERANCE) { Process(sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException e)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(e, this);
			}
		}

		/// <inheritdoc cref="AFloatInlet.Process"/>
		protected abstract void Process(double[] sample, double time);

		private void FixedUpdate()
		{
			if (moment == UpdateMoment.FixedUpdate && inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (moment == UpdateMoment.Update && inlet != null) { PullSamples(); }
		}
	}

	/// <summary> Char Inlet. </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public abstract class ACharInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment moment;

		public string streamName;
		public string streamType;

		private liblsl.StreamInlet        inlet;
		private liblsl.ContinuousResolver resolver;

		private int expectedChannels = 0;

		private char[] sample;

		private void Start()
		{
			bool hasAName = streamName.Length != 0;
			bool hasAType = streamType.Length != 0;

			if (!hasAName && !hasAType)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			if (hasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + streamName);
				resolver = new liblsl.ContinuousResolver("name", streamName);
			}
			else // if (hasAType) // Useless with the first if
			{
				Debug.Log("Creating LSL resolver for stream with type " + streamType);
				resolver = new liblsl.ContinuousResolver("type", streamType);
			}

			StartCoroutine(ResolveExpectedStream());

			AdditionalStart();
		}

		/// <summary> Override this method in the subclass to specify what should happen during Start(). </summary>
		protected virtual void AdditionalStart() { } //By default, do nothing.


		private IEnumerator ResolveExpectedStream()
		{
			var results = resolver.Results();

			yield return new WaitUntil(() => results.Length > 0);

			inlet = new liblsl.StreamInlet(results[0]);

			expectedChannels = inlet.Info().ChannelCount();

			yield return null;
		}

		protected void PullSamples()
		{
			sample = new char[expectedChannels];

			try
			{
				double lastTimeStamp = inlet.PullSample(sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = inlet.PullSample(sample, 0.0f)) > Constants.TOLERANCE) { Process(sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException e)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(e, this);
			}
		}

		/// <inheritdoc cref="AFloatInlet.Process"/>
		protected abstract void Process(char[] sample, double time);

		private void FixedUpdate()
		{
			if (moment == UpdateMoment.FixedUpdate && inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (moment == UpdateMoment.Update && inlet != null) { PullSamples(); }
		}
	}

	/// <summary> Float Inlet. </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public abstract class AShortInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment moment;

		public string streamName;
		public string streamType;

		private liblsl.StreamInlet        inlet;
		private liblsl.ContinuousResolver resolver;

		private int expectedChannels = 0;

		private short[] sample;

		private void Start()
		{
			bool hasAName = streamName.Length != 0;
			bool hasAType = streamType.Length != 0;

			if (!hasAName && !hasAType)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			if (hasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + streamName);
				resolver = new liblsl.ContinuousResolver("name", streamName);
			}
			else // if (hasAType) // Useless with the first if
			{
				Debug.Log("Creating LSL resolver for stream with type " + streamType);
				resolver = new liblsl.ContinuousResolver("type", streamType);
			}

			StartCoroutine(ResolveExpectedStream());

			AdditionalStart();
		}

		/// <summary> Override this method in the subclass to specify what should happen during Start(). </summary>
		protected virtual void AdditionalStart() { } //By default, do nothing.

		private IEnumerator ResolveExpectedStream()
		{
			var results = resolver.Results();

			yield return new WaitUntil(() => results.Length > 0);

			inlet = new liblsl.StreamInlet(results[0]);

			expectedChannels = inlet.Info().ChannelCount();

			yield return null;
		}

		protected void PullSamples()
		{
			sample = new short[expectedChannels];

			try
			{
				double lastTimeStamp = inlet.PullSample(sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = inlet.PullSample(sample, 0.0f)) > Constants.TOLERANCE) { Process(sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException e)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(e, this);
			}
		}

		/// <inheritdoc cref="AFloatInlet.Process"/>
		protected abstract void Process(short[] sample, double time);

		private void FixedUpdate()
		{
			if (moment == UpdateMoment.FixedUpdate && inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (moment == UpdateMoment.Update && inlet != null) { PullSamples(); }
		}
	}

	/// <summary> Int Inlet. </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public abstract class AIntInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment moment;

		public string streamName;
		public string streamType;

		private liblsl.StreamInlet        inlet;
		private liblsl.ContinuousResolver resolver;

		private int expectedChannels = 0;

		private int[] sample;

		private void Start()
		{
			bool hasAName = streamName.Length != 0;
			bool hasAType = streamType.Length != 0;

			if (!hasAName && !hasAType)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			if (hasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + streamName);
				resolver = new liblsl.ContinuousResolver("name", streamName);
			}
			else // if (hasAType) // Useless with the first if
			{
				Debug.Log("Creating LSL resolver for stream with type " + streamType);
				resolver = new liblsl.ContinuousResolver("type", streamType);
			}

			StartCoroutine(ResolveExpectedStream());

			AdditionalStart();
		}

		/// <summary> Override this method in the subclass to specify what should happen during Start(). </summary>
		protected virtual void AdditionalStart() { } //By default, do nothing.

		private IEnumerator ResolveExpectedStream()
		{
			var results = resolver.Results();

			yield return new WaitUntil(() => results.Length > 0);

			inlet = new liblsl.StreamInlet(results[0]);
			expectedChannels = inlet.Info().ChannelCount();

			yield return null;
		}

		protected void PullSamples()
		{
			sample = new int[expectedChannels];

			try
			{
				double lastTimeStamp = inlet.PullSample(sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = inlet.PullSample(sample, 0.0f)) > Constants.TOLERANCE) { Process(sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException e)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(e, this);
			}
		}

		/// <inheritdoc cref="AFloatInlet.Process"/>
		protected abstract void Process(int[] sample, double time);

		private void FixedUpdate()
		{
			if (moment == UpdateMoment.FixedUpdate && inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (moment == UpdateMoment.Update && inlet != null) { PullSamples(); }
		}
	}

	/// <summary> String Inlet. </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public abstract class AStringInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment moment;

		public string streamName;
		public string streamType;

		private liblsl.StreamInlet        inlet;
		private liblsl.ContinuousResolver resolver;

		private int expectedChannels = 0;

		private string[] sample;

		private void Start()
		{
			bool hasAName = streamName.Length != 0;
			bool hasAType = streamType.Length != 0;

			if (!hasAName && !hasAType)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			if (hasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + streamName);
				resolver = new liblsl.ContinuousResolver("name", streamName);
			}
			else // if (hasAType) // Useless with the first if
			{
				Debug.Log("Creating LSL resolver for stream with type " + streamType);
				resolver = new liblsl.ContinuousResolver("type", streamType);
			}

			StartCoroutine(ResolveExpectedStream());

			AdditionalStart();
		}

		/// <summary> Override this method in the subclass to specify what should happen during Start(). </summary>
		protected virtual void AdditionalStart() { } //By default, do nothing.

		private IEnumerator ResolveExpectedStream()
		{
			var results = resolver.Results();

			yield return new WaitUntil(() => results.Length > 0);

			inlet = new liblsl.StreamInlet(results[0]);

			expectedChannels = inlet.Info().ChannelCount();

			yield return null;
		}

		protected void PullSamples()
		{
			sample = new string[expectedChannels];

			try
			{
				double lastTimeStamp = inlet.PullSample(sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = inlet.PullSample(sample, 0.0f)) > Constants.TOLERANCE) { Process(sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException e)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(e, this);
			}
		}

		/// <inheritdoc cref="AFloatInlet.Process"/>
		protected abstract void Process(string[] sample, double time);

		private void FixedUpdate()
		{
			if (moment == UpdateMoment.FixedUpdate && inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (moment == UpdateMoment.Update && inlet != null) { PullSamples(); }
		}
	}
}
