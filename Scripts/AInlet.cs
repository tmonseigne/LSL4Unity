using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace LSL4Unity.Scripts
{
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
			var expectedStreamHasAName = !StreamName.Equals("");
			var expectedStreamHasAType = !StreamType.Equals("");

			if (!expectedStreamHasAName && !expectedStreamHasAType)
			{
				Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
				this.enabled = false;
				return;
			}

			if (expectedStreamHasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + StreamName);

				_resolver = new liblsl.ContinuousResolver("name", StreamName);
			}
			else if (expectedStreamHasAType)
			{
				Debug.Log("Creating LSL resolver for stream with type " + StreamType);
				_resolver = new liblsl.ContinuousResolver("type ", StreamType);
			}

			StartCoroutine(ResolveExpectedStream());

			AdditionalStart();
		}

		/// <summary>
		/// Override this method in the subclass to specify what should happen during Start().
		/// </summary>
		protected virtual void AdditionalStart()
		{
			//By default, do nothing.
		}

		private IEnumerator ResolveExpectedStream()
		{
			var results = _resolver.Results();

			yield return new WaitUntil(() => results.Length > 0);

			Debug.Log($"Resolving Stream: {StreamName}");

			_inlet = new liblsl.StreamInlet(results[0]);

			_expectedChannels = _inlet.Info().channel_count();

			yield return null;
		}

		protected void PullSamples()
		{
			_sample = new float[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.pull_sample(_sample, 0.0f);

				if (lastTimeStamp != 0.0)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while ((lastTimeStamp = _inlet.pull_sample(_sample, 0.0f)) != 0) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				this.enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <summary>
		/// Override this method in the subclass to specify what should happen when samples are available.
		/// </summary>
		/// <param name="newSample"></param>
		protected abstract void Process(float[] newSample, double timeStamp);

		private void FixedUpdate()
		{
			if (Moment == UpdateMoment.FixedUpdate && _inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (Moment == UpdateMoment.Update && _inlet != null) { PullSamples(); }
		}
	}

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
				this.enabled = false;
				return;
			}

			if (expectedStreamHasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + StreamName);

				_resolver = new liblsl.ContinuousResolver("name", StreamName);
			}
			else if (expectedStreamHasAType)
			{
				Debug.Log("Creating LSL resolver for stream with type " + StreamType);
				_resolver = new liblsl.ContinuousResolver("type", StreamType);
			}

			StartCoroutine(ResolveExpectedStream());

			AdditionalStart();
		}

		/// <summary>
		/// Override this method in the subclass to specify what should happen during Start().
		/// </summary>
		protected virtual void AdditionalStart()
		{
			//By default, do nothing.
		}

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

		private liblsl.StreamInfo GetStreamInfoFrom(liblsl.StreamInfo[] results)
		{
			var targetInfo = results.Where(r => r.Name().Equals(StreamName)).First();
			return targetInfo;
		}

		protected void PullSamples()
		{
			_sample = new double[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.pull_sample(_sample, 0.0f);

				if (lastTimeStamp != 0.0)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while ((lastTimeStamp = _inlet.pull_sample(_sample, 0.0f)) != 0) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				this.enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <summary>
		/// Override this method in the subclass to specify what should happen when samples are available.
		/// </summary>
		/// <param name="newSample"></param>
		protected abstract void Process(double[] newSample, double timeStamp);

		private void FixedUpdate()
		{
			if (Moment == UpdateMoment.FixedUpdate && _inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (Moment == UpdateMoment.Update && _inlet != null) { PullSamples(); }
		}
	}

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
				this.enabled = false;
				return;
			}

			if (expectedStreamHasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + StreamName);

				_resolver = new liblsl.ContinuousResolver("name", StreamName);
			}
			else if (expectedStreamHasAType)
			{
				Debug.Log("Creating LSL resolver for stream with type " + StreamType);
				_resolver = new liblsl.ContinuousResolver("type", StreamType);
			}

			StartCoroutine(ResolveExpectedStream());

			AdditionalStart();
		}

		/// <summary>
		/// Override this method in the subclass to specify what should happen during Start().
		/// </summary>
		protected virtual void AdditionalStart()
		{
			//By default, do nothing.
		}


		private IEnumerator ResolveExpectedStream()
		{
			var results = _resolver.Results();

			yield return new WaitUntil(() => results.Length > 0);

			_inlet = new liblsl.StreamInlet(results[0]);

			_expectedChannels = _inlet.Info().channel_count();

			yield return null;
		}

		protected void PullSamples()
		{
			_sample = new char[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.pull_sample(_sample, 0.0f);

				if (lastTimeStamp != 0.0)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while ((lastTimeStamp = _inlet.pull_sample(_sample, 0.0f)) != 0) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				this.enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <summary>
		/// Override this method in the subclass to specify what should happen when samples are available.
		/// </summary>
		/// <param name="newSample"></param>
		protected abstract void Process(char[] newSample, double timeStamp);

		private void FixedUpdate()
		{
			if (Moment == UpdateMoment.FixedUpdate && _inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (Moment == UpdateMoment.Update && _inlet != null) { PullSamples(); }
		}
	}

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
				this.enabled = false;
				return;
			}

			if (expectedStreamHasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + StreamName);

				_resolver = new liblsl.ContinuousResolver("name", StreamName);
			}
			else if (expectedStreamHasAType)
			{
				Debug.Log("Creating LSL resolver for stream with type " + StreamType);
				_resolver = new liblsl.ContinuousResolver("type", StreamType);
			}

			StartCoroutine(ResolveExpectedStream());

			AdditionalStart();
		}

		/// <summary>
		/// Override this method in the subclass to specify what should happen during Start().
		/// </summary>
		protected virtual void AdditionalStart()
		{
			//By default, do nothing.
		}

		private IEnumerator ResolveExpectedStream()
		{
			var results = _resolver.Results();

			yield return new WaitUntil(() => results.Length > 0);

			_inlet = new liblsl.StreamInlet(results[0]);

			_expectedChannels = _inlet.Info().channel_count();

			yield return null;
		}

		protected void PullSamples()
		{
			_sample = new short[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.pull_sample(_sample, 0.0f);

				if (lastTimeStamp != 0.0)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while ((lastTimeStamp = _inlet.pull_sample(_sample, 0.0f)) != 0) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				this.enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <summary>
		/// Override this method in the subclass to specify what should happen when samples are available.
		/// </summary>
		/// <param name="newSample"></param>
		protected abstract void Process(short[] newSample, double timeStamp);

		private void FixedUpdate()
		{
			if (Moment == UpdateMoment.FixedUpdate && _inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (Moment == UpdateMoment.Update && _inlet != null) { PullSamples(); }
		}
	}

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
				this.enabled = false;
				return;
			}

			if (expectedStreamHasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + StreamName);

				_resolver = new liblsl.ContinuousResolver("name", StreamName);
			}
			else if (expectedStreamHasAType)
			{
				Debug.Log("Creating LSL resolver for stream with type " + StreamType);
				_resolver = new liblsl.ContinuousResolver("type", StreamType);
			}

			StartCoroutine(ResolveExpectedStream());

			AdditionalStart();
		}

		/// <summary>
		/// Override this method in the subclass to specify what should happen during Start().
		/// </summary>
		protected virtual void AdditionalStart()
		{
			//By default, do nothing.
		}

		private IEnumerator ResolveExpectedStream()
		{
			var results = _resolver.Results();

			yield return new WaitUntil(() => results.Length > 0);

			_inlet = new liblsl.StreamInlet(results[0]);

			_expectedChannels = _inlet.Info().channel_count();

			yield return null;
		}

		protected void PullSamples()
		{
			_sample = new int[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.pull_sample(_sample, 0.0f);

				if (lastTimeStamp != 0.0)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while ((lastTimeStamp = _inlet.pull_sample(_sample, 0.0f)) != 0) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				this.enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <summary>
		/// Override this method in the subclass to specify what should happen when samples are available.
		/// </summary>
		/// <param name="newSample"></param>
		protected abstract void Process(int[] newSample, double timeStamp);

		private void FixedUpdate()
		{
			if (Moment == UpdateMoment.FixedUpdate && _inlet != null) { PullSamples(); }
		}

		private void Update()
		{
			if (Moment == UpdateMoment.Update && _inlet != null) { PullSamples(); }
		}
	}

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
				this.enabled = false;
				return;
			}

			if (expectedStreamHasAName)
			{
				Debug.Log("Creating LSL resolver for stream " + StreamName);

				_resolver = new liblsl.ContinuousResolver("name", StreamName);
			}
			else if (expectedStreamHasAType)
			{
				Debug.Log("Creating LSL resolver for stream with type " + StreamType);
				_resolver = new liblsl.ContinuousResolver("type", StreamType);
			}

			StartCoroutine(ResolveExpectedStream());

			AdditionalStart();
		}

		/// <summary>
		/// Override this method in the subclass to specify what should happen during Start().
		/// </summary>
		protected virtual void AdditionalStart()
		{
			//By default, do nothing.
		}

		private IEnumerator ResolveExpectedStream()
		{
			var results = _resolver.Results();

			yield return new WaitUntil(() => results.Length > 0);

			_inlet = new liblsl.StreamInlet(results[0]);

			_expectedChannels = _inlet.Info().channel_count();

			yield return null;
		}

		protected void PullSamples()
		{
			_sample = new string[_expectedChannels];

			try
			{
				double lastTimeStamp = _inlet.pull_sample(_sample, 0.0f);

				if (lastTimeStamp != 0.0)
				{
					// do not miss the first one found
					Process(_sample, lastTimeStamp);
					// pull as long samples are available
					while ((lastTimeStamp = _inlet.pull_sample(_sample, 0.0f)) != 0) { Process(_sample, lastTimeStamp); }
				}
			}
			catch (ArgumentException aex)
			{
				Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
				this.enabled = false;
				Debug.LogException(aex, this);
			}
		}

		/// <summary>
		/// Override this method in the subclass to specify what should happen when samples are available.
		/// </summary>
		/// <param name="newSample"></param>
		protected abstract void Process(string[] newSample, double timeStamp);

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
