using System;
using System.Linq;
using LSL4Unity.Scripts.OV;
using UnityEngine;

namespace LSL4Unity.Scripts
{
	public abstract class ABaseInlet : MonoBehaviour
	{
		public string StreamName;
		public string StreamType;

		protected liblsl.StreamInlet Inlet;

		protected int ExpectedChannels;

		protected Resolver Resolver;

		/// <summary> Call this method when your inlet implementation got created at runtime. </summary>
		protected virtual void RegisterAndLookUpStream()
		{
			Resolver = FindObjectOfType<Resolver>();

			//Resolver.OnStreamFound.AddListener(new UnityAction<LSLStreamInfoWrapper>(AStreamIsFound));	// Redundant to explicit delegate creation
			//Resolver.OnStreamLost.AddListener(new UnityAction<LSLStreamInfoWrapper>(AStreamGotLost));		// Redundant to explicit delegate creation
			Resolver.OnStreamFound.AddListener(AStreamIsFound);
			Resolver.OnStreamLost.AddListener(AStreamGotLost);

			if (Resolver.KnownStreams.Any(IsTheExpected))
			{
				var stream = Resolver.KnownStreams.First(IsTheExpected);
				AStreamIsFound(stream);
			}
		}

		/// <summary> Callback method for the Resolver gets called each time the resolver found a stream. </summary>
		/// <param name="stream"> The stream. </param>
		public virtual void AStreamIsFound(LSLStreamInfoWrapper stream)
		{
			if (!IsTheExpected(stream)) { return; }

			Debug.Log($"LSL Stream {stream.Name} found for {name}");

			Inlet            = new liblsl.StreamInlet(stream.Item);
			ExpectedChannels = stream.ChannelCount;

			OnStreamAvailable();
		}

		/// <summary> Callback method for the Resolver gets called each time the resolver misses a stream within its cache. </summary>
		/// <param name="stream"> The stream. </param>
		public virtual void AStreamGotLost(LSLStreamInfoWrapper stream)
		{
			if (!IsTheExpected(stream)) { return; }

			Debug.Log($"LSL Stream {stream.Name} Lost for {name}");

			OnStreamLost();
		}

		/// <summary> Determines if the specified stream is the expected stream. </summary>
		/// <param name="stream"> The stream. </param>
		/// <returns> <c>true</c> if if the specified stream is the expected stream; otherwise, <c>false</c>. </returns>
		protected virtual bool IsTheExpected(LSLStreamInfoWrapper stream)
		{
			bool predicate = StreamName.Equals(stream.Name);
			predicate &= StreamType.Equals(stream.Type);

			return predicate;
		}

		/// <summary> Pull the samples. </summary>
		protected abstract void PullSamples();

		/// <summary> Called when a stream is available. </summary>
		/// <exception cref="NotImplementedException">Please override this method in a derived class!</exception>
		protected virtual void OnStreamAvailable()
		{
			// base implementation may not decide what happens when the stream gets available
			throw new NotImplementedException("Please override this method in a derived class!");
		}

		/// <summary> Called when a stream is lost. </summary>
		/// <exception cref="NotImplementedException">Please override this method in a derived class!</exception>
		protected virtual void OnStreamLost()
		{
			// base implementation may not decide what happens when the stream gets lost
			throw new NotImplementedException("Please override this method in a derived class!");
		}
	}

	/// <inheritdoc/>
	public abstract class InletFloatSamples : ABaseInlet
	{
		/// <summary> Override this method in the subclass to specify what should happen when samples are available. </summary>
		/// <param name="sample"> The Incomming Sample. </param>
		/// <param name="time"> The current Time. </param>
		protected abstract void Process(float[] sample, double time);

		protected float[] Sample;

		protected override void PullSamples()
		{
			Sample = new float[ExpectedChannels];

			try
			{
				double time = Inlet.PullSample(Sample, 0.0f);

				if (Math.Abs(time) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(Sample, time);
					// pull as long samples are available
					while (Math.Abs(time = Inlet.PullSample(Sample, 0.0f)) > Constants.TOLERANCE) { Process(Sample, time); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}
	}

	/// <inheritdoc/>
	public abstract class InletDoubleSamples : ABaseInlet
	{
		/// <inheritdoc cref="InletFloatSamples.Process"/>
		protected abstract void Process(double[] sample, double time);

		protected double[] Sample;

		protected override void PullSamples()
		{
			Sample = new double[ExpectedChannels];

			try
			{
				double lastTimeStamp = Inlet.PullSample(Sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(Sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = Inlet.PullSample(Sample, 0.0f)) > Constants.TOLERANCE) { Process(Sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}
	}

	/// <inheritdoc/>
	public abstract class InletIntSamples : ABaseInlet
	{
		/// <inheritdoc cref="InletFloatSamples.Process"/>
		protected abstract void Process(int[] sample, double time);

		protected int[] Sample;

		protected override void PullSamples()
		{
			Sample = new int[ExpectedChannels];

			try
			{
				double lastTimeStamp = Inlet.PullSample(Sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(Sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = Inlet.PullSample(Sample, 0.0f)) > Constants.TOLERANCE) { Process(Sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}
	}

	/// <inheritdoc/>
	public abstract class InletCharSamples : ABaseInlet
	{
		/// <inheritdoc cref="InletFloatSamples.Process"/>
		protected abstract void Process(char[] sample, double time);

		protected char[] Sample;

		protected override void PullSamples()
		{
			Sample = new char[ExpectedChannels];

			try
			{
				double lastTimeStamp = Inlet.PullSample(Sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(Sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = Inlet.PullSample(Sample, 0.0f)) > Constants.TOLERANCE) { Process(Sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}
	}

	/// <inheritdoc/>
	public abstract class InletStringSamples : ABaseInlet
	{
		/// <inheritdoc cref="InletFloatSamples.Process"/>
		protected abstract void Process(string[] sample, double time);

		protected string[] Sample;

		protected override void PullSamples()
		{
			Sample = new string[ExpectedChannels];

			try
			{
				double lastTimeStamp = Inlet.PullSample(Sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(Sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = Inlet.PullSample(Sample, 0.0f)) > Constants.TOLERANCE) { Process(Sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}
	}

	/// <inheritdoc/>
	public abstract class InletShortSamples : ABaseInlet
	{
		/// <inheritdoc cref="InletFloatSamples.Process"/>
		protected abstract void Process(short[] sample, double time);

		protected short[] Sample;

		protected override void PullSamples()
		{
			Sample = new short[ExpectedChannels];

			try
			{
				double lastTimeStamp = Inlet.PullSample(Sample, 0.0f);

				if (Math.Abs(lastTimeStamp) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(Sample, lastTimeStamp);
					// pull as long samples are available
					while (Math.Abs(lastTimeStamp = Inlet.PullSample(Sample, 0.0f)) > Constants.TOLERANCE) { Process(Sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				enabled = false;
				Debug.LogException(aex, this);
			}
		}
	}
}
