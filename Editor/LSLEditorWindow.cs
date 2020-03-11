using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LSL4Unity.Editor
{
	public class LSLShowStreamsWindow : EditorWindow
	{
		//public double WaitOnResolveStreams = 2;

		private const string NO_STREAMS_FOUND = "No streams found!";
		private const string N_STREAMS_FOUND  = " Streams found";

		//private const string CLICK_LOOK_UP_FIRST = "Click lookup first";

		private readonly List<string> _listNamesOfStreams = new List<string>();

		private Vector2 _scrollVector;
		private string  _streamLookUpResult;

		private liblsl.ContinuousResolver _resolver;
		private string                    _lslVersionInfos;

		public void Init()
		{
			_resolver = new liblsl.ContinuousResolver();

			var libVersion      = liblsl.library_version();
			var protocolVersion = liblsl.protocol_version();

			var libMajor  = libVersion / 100;
			var libMinor  = libVersion % 100;
			var protMajor = protocolVersion / 100;
			var protMinor = protocolVersion % 100;

			_lslVersionInfos = $"You are using LSL library: {libMajor}.{libMinor} implementing protocol version: {protMajor}.{protMinor}";

			titleContent = new GUIContent("LSL Utility");
		}

		liblsl.StreamInfo[] _streamInfos = null;

		void OnGUI()
		{
			if (_resolver == null) { Init(); }

			UpdateStreams();

			EditorGUILayout.BeginVertical();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField(_lslVersionInfos, EditorStyles.miniLabel);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(_streamLookUpResult, EditorStyles.boldLabel);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.Separator();

			_scrollVector = EditorGUILayout.BeginScrollView(_scrollVector, GUILayout.Width(EditorGUIUtility.currentViewWidth));
			GUILayoutOption fieldWidth = GUILayout.Width(EditorGUIUtility.currentViewWidth / 4.3f);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Name",      EditorStyles.boldLabel, fieldWidth);
			EditorGUILayout.LabelField("Type",      EditorStyles.boldLabel, fieldWidth);
			EditorGUILayout.LabelField("HostName",  EditorStyles.boldLabel, fieldWidth);
			EditorGUILayout.LabelField("Data Rate", EditorStyles.boldLabel, fieldWidth);
			EditorGUILayout.EndHorizontal();

			foreach (var item in _listNamesOfStreams)
			{
				string[] s = item.Split(' ');

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(new GUIContent(s[0], s[0]), fieldWidth);
				EditorGUILayout.LabelField(new GUIContent(s[1], s[1]), fieldWidth);
				EditorGUILayout.LabelField(new GUIContent(s[2], s[2]), fieldWidth);
				EditorGUILayout.LabelField(new GUIContent(s[3], s[3]), fieldWidth);
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}

		private void UpdateStreams()
		{
			_listNamesOfStreams.Clear();
			_streamInfos = _resolver.Results();

			if (_streamInfos.Length == 0) { _streamLookUpResult = NO_STREAMS_FOUND; }
			else
			{
				foreach (var item in _streamInfos) { _listNamesOfStreams.Add($"{item.Name()} {item.Type()} {item.Hostname()} {item.nominal_srate()}"); }
				_streamLookUpResult = _listNamesOfStreams.Count + N_STREAMS_FOUND;
			}
		}
	}
}
