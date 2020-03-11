namespace LSL4Unity.Scripts.OV.Template
{
	public class SignalInlet : OVDoubleInlet
	{
		public double[] LastSample;

		protected override void Process(double[] sample, double time) { LastSample = sample; }
	}
}
