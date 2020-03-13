using System;

namespace LSL4Unity.Scripts.OV.Template
{
	/// <summary> Implementation for a Inlet receiving Matrix (double) from OpenViBE. </summary>
	/// <seealso cref="OVDoubleInlet" />
	/// @todo Je dois d'abord vérifier si je ne peux pas envoyer le tableau d'un coup par LSL et le double ou float Inlet sera suffisant.
	public class MatrixInlet : OVDoubleInlet
	{
		public int       NChannel = -1;
		public int       NSample  = -1;
		public double[,] Matrix;
		public bool      ReadyToSend = false;

		private int _curChannel = -1;
		private int _curSample  = -1;

		private void ResetMatrix()
		{
			for (int i = 0; i < NChannel; i++)
			{
				for (int j = 0; j < NSample; j++) { Matrix[i, j] = 0; }
			}
			_curChannel = 0;
			_curSample  = 0;
		}

		protected override void Process(double[] sample, double time)
		{
			if (NChannel == -1) { NChannel = (int) (sample[0]); }
			else if (NSample == -1)
			{
				NSample = (int) (sample[0]);
				Matrix  = new double[NChannel, NSample];
				ResetMatrix();
			}
			else
			{
				// If We have complete the matrix
				if (_curChannel == NChannel && _curSample == NSample) { ResetMatrix(); }
				// Update Row and column
				if (_curSample == NSample)
				{
					_curChannel++;
					_curSample = 0;
				}
				else { _curSample++; }


				// If Now the matrix is completed
				if (_curChannel == NChannel && _curSample == NSample) { ReadyToSend = true; }
			}
		}
	}
}
