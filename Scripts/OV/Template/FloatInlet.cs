
namespace LSL4Unity.Scripts.OV.Template
{
	public class FloatInlet : OVFloatInlet
	{
		public float[] LastSample;

		protected override void Process(float[] newSample, double timeStamp) { LastSample = newSample; }
	}
}