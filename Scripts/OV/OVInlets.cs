using System;
using System.Collections;
using UnityEngine;

namespace LSL4Unity.Scripts.OV
{
	/// <summary> Base Inlet for OpenViBE Link </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public abstract class OVInlet : MonoBehaviour
	{
		public enum UpdateMoment { FixedUpdate, Update }

		public UpdateMoment Moment;

		public string StreamName = "ovSignal";

		protected liblsl.StreamInlet        Inlet;
		private   liblsl.StreamInfo[]       _results;
		private   liblsl.ContinuousResolver _resolver;

		protected int ExpectedChannels = 0;

		/// <summary> Start is called before the first frame update. </summary>
		private void Start()
		{
			var hasAName = StreamName.Length != 0;

			if (!hasAName)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				enabled = false;
				return;
			}

			Debug.Log("Creating LSL resolver for stream " + StreamName);
			_resolver = new liblsl.ContinuousResolver("name", StreamName);

			StartCoroutine(ResolveExpectedStream());
		}

		/// <summary> Fixupdate is called once per physics framerate. </summary>
		private void FixedUpdate()
		{
			if (Moment == UpdateMoment.FixedUpdate && Inlet != null) { PullSamples(); }
		}

		/// <summary> Update is called once per frame. </summary>
		private void Update()
		{
			if (Moment == UpdateMoment.Update && Inlet != null) { PullSamples(); }
		}

		/// <summary> Resolves the stream. </summary>
		/// <returns></returns>
		private IEnumerator ResolveExpectedStream()
		{
			_results = _resolver.Results();
			yield return new WaitUntil(() => _results.Length > 0);

			Debug.Log($"Resolving Stream : {StreamName}");

			Inlet            = new liblsl.StreamInlet(_results[0]);
			ExpectedChannels = Inlet.Info().ChannelCount();

			yield return null;
		}

		/// <summary> Pull the samples. </summary>
		protected abstract void PullSamples();
	}

	public abstract class OVFloatInlet : OVInlet
	{
		private float[] _sample;

		/// <inheritdoc cref="OVInlet.PullSamples"/>
		protected override void PullSamples()
		{
			_sample = new float[ExpectedChannels];

			try
			{
				double lastTimeStamp = Inlet.PullSample(_sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = Inlet.PullSample(_sample, 0.0f)) > Constants.TOLERANCE) { Process(_sample, lastTimeStamp); }
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
	}

	public abstract class OVDoubleInlet : OVInlet
	{
		private double[] _sample;

		/// <inheritdoc cref="OVInlet.PullSamples"/>
		protected override void PullSamples()
		{
			_sample = new double[ExpectedChannels];

			try
			{
				double lastTimeStamp = Inlet.PullSample(_sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = Inlet.PullSample(_sample, 0.0f)) > Constants.TOLERANCE) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <inheritdoc cref="OVFloatInlet.Process"/>
		protected abstract void Process(double[] sample, double time);
	}

	public abstract class OVIntInlet : OVInlet
	{
		private int[] _sample;

		/// <inheritdoc cref="OVInlet.PullSamples"/>
		protected override void PullSamples()
		{
			_sample = new int[ExpectedChannels];

			try
			{
				double lastTimeStamp = Inlet.PullSample(_sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = Inlet.PullSample(_sample, 0.0f)) > Constants.TOLERANCE) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <inheritdoc cref="OVFloatInlet.Process"/>
		protected abstract void Process(int[] sample, double time);
	}
}
