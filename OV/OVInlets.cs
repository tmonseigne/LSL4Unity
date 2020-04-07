using System;
using System.Collections;
using UnityEngine;

namespace LSL4Unity.OV
{
	/// <summary> Base Inlet for OpenViBE Link. </summary>
	/// <seealso cref="MonoBehaviour" />
	public abstract class OVInlet<T> : MonoBehaviour
	{
		private enum UpdateMoment { FixedUpdate, Update, OnDemand }

		[SerializeField] private UpdateMoment moment     = UpdateMoment.Update;
		[SerializeField] private string       streamName = "ovSignal";

		public string StreamName => streamName;

		protected liblsl.StreamInlet        inlet;
		private   liblsl.ContinuousResolver resolver;

		private   bool readyToResolve   = true;
		protected int  expectedChannels = 0;
		protected T[]  samples;


		/// <summary> Start is called before the first frame update. </summary>
		private void Start()
		{
			bool hasAName = streamName.Length != 0;

			if (!hasAName)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			Debug.Log("Creating LSL resolver for stream " + streamName);
			resolver = new liblsl.ContinuousResolver("name", streamName);

			StartCoroutine(ResolveExpectedStream());
		}

		/// <summary> Fixupdate is called once per physics framerate. </summary>
		private void FixedUpdate()
		{
			if (moment == UpdateMoment.FixedUpdate && inlet != null) { PullSamples(); }
		}

		/// <summary> Update is called once per frame. </summary>
		private void Update()
		{
			if (moment == UpdateMoment.Update && inlet != null) { PullSamples(); }
		}

		/// <summary> ForceUpdate is called when it's needed. </summary>
		public void ForceUpdate() { PullSamples(); }

		/// <summary> Resolves the stream. </summary>
		/// <returns></returns>
		private IEnumerator ResolveExpectedStream()
		{
			yield return new WaitUntil(() => readyToResolve); // False mutex to wait Found Stream before search an other
			readyToResolve = false;                           // Avoïd double resolver

			liblsl.StreamInfo[] results = resolver.Results();
			yield return new WaitUntil(() => results.Length > 0);

			Debug.Log($"Resolving Stream : Name = {streamName}, Steam Info Name = {results[0].Name()}, Stream Info Type = ({results[0].Type()}");

			inlet            = new liblsl.StreamInlet(results[0]);
			expectedChannels = inlet.Info().ChannelCount();

			readyToResolve = true;
			yield return null;
		}

		/// <summary> Pull the samples. </summary>
		protected abstract void PullSamples();

		/// <summary> Override this method in the subclass to specify what should happen when samples are available. </summary>
		/// <param name="input"> The Incomming Sample. </param>
		/// <param name="time"> The current Time. </param>
		protected abstract void Process(T[] input, double time);
	}

	/// <summary> Float Inlet for OpenViBE Link. </summary>
	/// <seealso cref="OVInlet{T}" />
	public abstract class OVFloatInlet : OVInlet<float>
	{
		/// <inheritdoc cref="OVInlet{T}.PullSamples"/>
		protected override void PullSamples()
		{
			samples = new float[expectedChannels];

			try
			{
				double lastTimeStamp = inlet.PullSample(samples, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(samples, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = inlet.PullSample(samples, 0.0f)) > Constants.TOLERANCE) { Process(samples, lastTimeStamp); }
				}
			}
			catch (ArgumentException e)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(e, this);
			}
		}
	}

	/// <summary> Double Inlet for OpenViBE Link. </summary>
	/// <seealso cref="OVInlet{T}" />
	public abstract class OVDoubleInlet : OVInlet<double>
	{
		/// <inheritdoc cref="OVInlet{T}.PullSamples"/>
		protected override void PullSamples()
		{
			samples = new double[expectedChannels];

			try
			{
				double lastTimeStamp = inlet.PullSample(samples, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(samples, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = inlet.PullSample(samples, 0.0f)) > Constants.TOLERANCE) { Process(samples, lastTimeStamp); }
				}
			}
			catch (ArgumentException e)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(e, this);
			}
		}
	}

	/// <summary> Int Inlet for OpenViBE Link. </summary>
	/// <seealso cref="OVInlet{T}" />
	public abstract class OVIntInlet : OVInlet<int>
	{
		/// <inheritdoc cref="OVInlet{T}.PullSamples"/>
		protected override void PullSamples()
		{
			samples = new int[expectedChannels];

			try
			{
				double lastTimeStamp = inlet.PullSample(samples, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(samples, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = inlet.PullSample(samples, 0.0f)) > Constants.TOLERANCE) { Process(samples, lastTimeStamp); }
				}
			}
			catch (ArgumentException e)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(e, this);
			}
		}
	}
}
