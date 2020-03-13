using System;
using System.Linq;
using LSL4Unity.OV;
using UnityEngine;

namespace LSL4Unity
{
	public abstract class ABaseInlet : MonoBehaviour
	{
		public string streamName;
		public string streamType;

		protected liblsl.StreamInlet inlet;

		protected int expectedChannels;

		protected Resolver resolver;

		/// <summary> Call this method when your inlet implementation got created at runtime. </summary>
		protected virtual void RegisterAndLookUpStream()
		{
			resolver = FindObjectOfType<Resolver>();

			//Resolver.OnStreamFound.AddListener(new UnityAction<LSLStreamInfoWrapper>(AStreamIsFound));	// Redundant to explicit delegate creation
			//Resolver.OnStreamLost.AddListener(new UnityAction<LSLStreamInfoWrapper>(AStreamGotLost));		// Redundant to explicit delegate creation
			resolver.onStreamFound.AddListener(AStreamIsFound);
			resolver.onStreamLost.AddListener(AStreamGotLost);

			if (resolver.streams.Any(IsTheExpected))
			{
				var stream = resolver.streams.First(IsTheExpected);
				AStreamIsFound(stream);
			}
		}

		/// <summary> Callback method for the Resolver gets called each time the resolver found a stream. </summary>
		/// <param name="stream"> The stream. </param>
		public virtual void AStreamIsFound(LSLStreamInfoWrapper stream)
		{
			if (!IsTheExpected(stream)) { return; }

			Debug.Log($"LSL Stream {stream.name} found for {name}");

			inlet            = new liblsl.StreamInlet(stream.Item);
			expectedChannels = stream.ChannelCount;

			OnStreamAvailable();
		}

		/// <summary> Callback method for the Resolver gets called each time the resolver misses a stream within its cache. </summary>
		/// <param name="stream"> The stream. </param>
		public virtual void AStreamGotLost(LSLStreamInfoWrapper stream)
		{
			if (!IsTheExpected(stream)) { return; }

			Debug.Log($"LSL Stream {stream.name} Lost for {name}");

			OnStreamLost();
		}

		/// <summary> Determines if the specified stream is the expected stream. </summary>
		/// <param name="stream"> The stream. </param>
		/// <returns> <c>true</c> if if the specified stream is the expected stream; otherwise, <c>false</c>. </returns>
		protected virtual bool IsTheExpected(LSLStreamInfoWrapper stream)
		{
			bool predicate = streamName.Equals(stream.name);
			predicate &= streamType.Equals(stream.type);

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

		protected float[] sample;

		protected override void PullSamples()
		{
			sample = new float[expectedChannels];

			try
			{
				double time = inlet.PullSample(sample, 0.0f);

				if (Math.Abs(time) > Constants.TOLERANCE)
				{
					// do not miss the first one found
					Process(sample, time);
					// pull as long samples are available
					while (Math.Abs(time = inlet.PullSample(sample, 0.0f)) > Constants.TOLERANCE) { Process(sample, time); }
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

	/// <inheritdoc/>
	public abstract class InletDoubleSamples : ABaseInlet
	{
		/// <inheritdoc cref="InletFloatSamples.Process"/>
		protected abstract void Process(double[] sample, double time);

		protected double[] sample;

		protected override void PullSamples()
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
	}

	/// <inheritdoc/>
	public abstract class InletIntSamples : ABaseInlet
	{
		/// <inheritdoc cref="InletFloatSamples.Process"/>
		protected abstract void Process(int[] sample, double time);

		protected int[] sample;

		protected override void PullSamples()
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
	}

	/// <inheritdoc/>
	public abstract class InletCharSamples : ABaseInlet
	{
		/// <inheritdoc cref="InletFloatSamples.Process"/>
		protected abstract void Process(char[] sample, double time);

		protected char[] sample;

		protected override void PullSamples()
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
	}

	/// <inheritdoc/>
	public abstract class InletStringSamples : ABaseInlet
	{
		/// <inheritdoc cref="InletFloatSamples.Process"/>
		protected abstract void Process(string[] sample, double time);

		protected string[] sample;

		protected override void PullSamples()
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
	}

	/// <inheritdoc/>
	public abstract class InletShortSamples : ABaseInlet
	{
		/// <inheritdoc cref="InletFloatSamples.Process"/>
		protected abstract void Process(short[] sample, double time);

		protected short[] sample;

		protected override void PullSamples()
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
	}
}
