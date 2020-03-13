using UnityEngine;
using UnityEngine.UI;

namespace LSL4Unity.Demos
{
	public class StreamInfo : MonoBehaviour
	{
		public LSLTransformDemoOutlet outlet;

		public Text streamNameLabel;
		public Text streamTypeLabel;
		public Text dataRate;
		public Text hasConsumerLabel;

		// Use this for initialization
		private void Start()
		{
			streamNameLabel.text  = outlet.streamName;
			streamTypeLabel.text  = outlet.streamType;
			dataRate.text         = $"Data Rate: {outlet.GetDataRate()}";
			hasConsumerLabel.text = "Has no consumers";
		}

		// Update is called once per frame
		private void Update()
		{
			if (outlet.HasConsumer())
			{
				hasConsumerLabel.text  = "Has consumers";
				hasConsumerLabel.color = Color.green;
			}
			else
			{
				hasConsumerLabel.text  = "No Consumers";
				hasConsumerLabel.color = Color.black;
			}
		}
	}
}
