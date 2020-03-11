using UnityEngine;
using UnityEngine.UI;

namespace LSL4Unity.Demos
{
	public class StreamInfo : MonoBehaviour
	{
		public LSLTransformDemoOutlet Outlet;

		public Text StreamNameLabel;
		public Text StreamTypeLabel;
		public Text DataRate;
		public Text HasConsumerLabel;

		// Use this for initialization
		void Start()
		{
			StreamNameLabel.text  = Outlet.StreamName;
			StreamTypeLabel.text  = Outlet.StreamType;
			DataRate.text         = $"Data Rate: {Outlet.GetDataRate()}";
			HasConsumerLabel.text = "Has no consumers";
		}

		// Update is called once per frame
		void Update()
		{
			if (Outlet.HasConsumer())
			{
				HasConsumerLabel.text  = "Has consumers";
				HasConsumerLabel.color = Color.green;
			}
			else
			{
				HasConsumerLabel.text  = "No Consumers";
				HasConsumerLabel.color = Color.black;
			}
		}
	}
}
