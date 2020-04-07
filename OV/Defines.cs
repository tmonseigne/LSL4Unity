namespace LSL4Unity.OV
{
	public static class Constants
	{
		public const double TOLERANCE = 1e-6;
	}

	/// <summary> List of GDF Stimulations. </summary>
	/// <remarks> You can use [nameof operator (C# reference)](https://docs.microsoft.com/dotnet/csharp/language-reference/operators/nameof) to see the name instead of the value of the staimulation in log or other. </remarks>
	public static class GDFStimulations
	{
		public const int GDF_ARTIFACT_EOG_LARGE             = 00257;
		public const int GDF_ARTIFACT_ECG                   = 00258;
		public const int GDF_ARTIFACT_EMG                   = 00259;
		public const int GDF_ARTIFACT_MOVEMENT              = 00260;
		public const int GDF_ARTIFACT_FAILING_ELECTRODE     = 00261;
		public const int GDF_ARTIFACT_SWEAT                 = 00262;
		public const int GDF_ARTIFACT_50_60_HZ_INTERFERENCE = 00263;
		public const int GDF_ARTIFACT_BREATHING             = 00264;
		public const int GDF_ARTIFACT_PULSE                 = 00265;
		public const int GDF_ARTIFACT_EOG_SMALL             = 00266;

		public const int GDF_CALIBRATION = 00271;

		public const int GDF_EEG_SLEEP_SPLINDLES        = 00273;
		public const int GDF_EEG_K_COMPLEXES            = 00274;
		public const int GDF_EEG_SAW_TOOTH_WAVES        = 00275;
		public const int GDF_EEG_IDLING_EEG_EYES_OPEN   = 00276;
		public const int GDF_EEG_IDLING_EEG_EYES_CLOSED = 00277;
		public const int GDF_EEG_SPIKE                  = 00278;
		public const int GDF_EEG_SEIZURE                = 00279;

		public const int GDF_VEP   = 00289;
		public const int GDF_AEP   = 00290;
		public const int GDF_SEP   = 00291;
		public const int GDF_TMS   = 00303;
		public const int GDF_SSVEP = 00305;
		public const int GDF_SSAEP = 00306;
		public const int GDF_SSSEP = 00307;

		public const int GDF_START_OF_TRIAL        = 00768;
		public const int GDF_LEFT                  = 00769;
		public const int GDF_RIGHT                 = 00770;
		public const int GDF_FOOT                  = 00771;
		public const int GDF_TONGUE                = 00772;
		public const int GDF_CLASS5                = 00773;
		public const int GDF_DOWN                  = 00774;
		public const int GDF_CLASS7                = 00775;
		public const int GDF_CLASS8                = 00776;
		public const int GDF_CLASS9                = 00777;
		public const int GDF_CLASS10               = 00778;
		public const int GDF_CLASS11               = 00779;
		public const int GDF_UP                    = 00780;
		public const int GDF_FEEDBACK_CONTINUOUS   = 00781;
		public const int GDF_FEEDBACK_DISCRETE     = 00782;
		public const int GDF_CUE_UNKNOWN_UNDEFINED = 00783;
		public const int GDF_BEEP                  = 00785;
		public const int GDF_CROSS_ON_SCREEN       = 00786;
		public const int GDF_FLASHING_LIGHT        = 00787;
		public const int GDF_END_OF_TRIAL          = 00800;

		public const int GDF_CORRECT   = 00897;
		public const int GDF_INCORRECT = 00898;

		public const int GDF_END_OF_SESSION                 = 01010;
		public const int GDF_REJECTION                      = 01023;
		public const int GDF_OAHE                           = 01025;
		public const int GDF_RERA                           = 01026;
		public const int GDF_CAHE                           = 01027;
		public const int GDF_CSB                            = 01028;
		public const int GDF_SLEEP_HYPOVENTILATION          = 01029;
		public const int GDF_MAXIMUM_INSPIRATION            = 01038;
		public const int GDF_START_OF_INSPIRATION           = 01039;
		public const int GDF_WAKE                           = 01040;
		public const int GDF_STAGE_1                        = 01041;
		public const int GDF_STAGE_2                        = 01042;
		public const int GDF_STAGE_3                        = 01043;
		public const int GDF_STAGE_4                        = 01044;
		public const int GDF_REM                            = 01045;
		public const int GDF_LIGHTS_ON                      = 01056;
		public const int GDF_EYES_LEFT                      = 01073;
		public const int GDF_EYES_RIGHT                     = 01074;
		public const int GDF_EYES_UP                        = 01075;
		public const int GDF_EYES_DOWN                      = 01076;
		public const int GDF_HORIZONTAL_EYE_MOVEMENT        = 01077;
		public const int GDF_VERTICAL_EYE_MOVEMENT          = 01078;
		public const int GDF_ROTATION_CLOCKWISE             = 01079;
		public const int GDF_ROTATION_COUNTERCLOCKWISE      = 01080;
		public const int GDF_EYE_BLINK                      = 01081;
		public const int GDF_LEFT_HAND_MOVEMENT             = 01089;
		public const int GDF_RIGHT_HAND_MOVEMENT            = 01090;
		public const int GDF_HEAD_MOVEMENT                  = 01091;
		public const int GDF_TONGUE_MOVEMENT                = 01092;
		public const int GDF_SWALLOWING                     = 01093;
		public const int GDF_BITING                         = 01094;
		public const int GDF_FOOT_MOVEMENT                  = 01095;
		public const int GDF_FOOT_RIGHT_MOVEMENT            = 01096;
		public const int GDF_ARM_MOVEMENT                   = 01097;
		public const int GDF_ARM_RIGHT_MOVEMENT             = 01098;
		public const int GDF_ECG_FIDUCIAL_POINT_QRS_COMPLEX = 01281;
		public const int GDF_ECG_P_WAVE                     = 01282;
		public const int GDF_ECG_QRS_COMPLEX                = 01283;
		public const int GDF_ECG_R_POINT                    = 01284;
		public const int GDF_ECG_T_WAVE                     = 01286;
		public const int GDF_ECG_U_WAVE                     = 01287;
		public const int GDF_START                          = 01408;

		public const int GDF_25_WATT  = 01409;
		public const int GDF_50_WATT  = 01410;
		public const int GDF_75_WATT  = 01411;
		public const int GDF_100_WATT = 01412;
		public const int GDF_125_WATT = 01413;
		public const int GDF_150_WATT = 01414;
		public const int GDF_175_WATT = 01415;
		public const int GDF_200_WATT = 01416;
		public const int GDF_225_WATT = 01417;
		public const int GDF_250_WATT = 01418;
		public const int GDF_275_WATT = 01419;
		public const int GDF_300_WATT = 01420;
		public const int GDF_325_WATT = 01421;
		public const int GDF_350_WATT = 01422;

		public const int GDF_START_OF_NEW_SEGMENT           = 32766;
		public const int GDF_NON_EQUIDISTANT_SAMPLING_VALUE = 32767;
	}

	/// <summary> List of OpenViBE Stimulations. </summary>
	/// <remarks> You can use [nameof operator (C# reference)](https://docs.microsoft.com/dotnet/csharp/language-reference/operators/nameof) to see the name instead of the value of the staimulation in log or other. </remarks>
	public static class Stimulations
	{
		public const int NUMBER_00 = 00000;
		public const int NUMBER_01 = 00001;
		public const int NUMBER_02 = 00002;
		public const int NUMBER_03 = 00003;
		public const int NUMBER_04 = 00004;
		public const int NUMBER_05 = 00005;
		public const int NUMBER_06 = 00006;
		public const int NUMBER_07 = 00007;
		public const int NUMBER_08 = 00008;
		public const int NUMBER_09 = 00009;
		public const int NUMBER_10 = 00016;
		public const int NUMBER_11 = 00017;
		public const int NUMBER_12 = 00018;
		public const int NUMBER_13 = 00019;
		public const int NUMBER_14 = 00020;
		public const int NUMBER_15 = 00021;
		public const int NUMBER_16 = 00022;
		public const int NUMBER_17 = 00023;
		public const int NUMBER_18 = 00024;
		public const int NUMBER_19 = 00025;

		public const int EXPERIMENT_START                      = 32769;
		public const int EXPERIMENT_STOP                       = 32770;
		public const int SEGMENT_START                         = 32771;
		public const int SEGMENT_STOP                          = 32772;
		public const int TRIAL_START                           = 32773;
		public const int TRIAL_STOP                            = 32774;
		public const int BASELINE_START                        = 32775;
		public const int BASELINE_STOP                         = 32776;
		public const int REST_START                            = 32777;
		public const int REST_STOP                             = 32778;
		public const int VISUAL_STIMULATION_START              = 32779;
		public const int VISUAL_STIMULATION_STOP               = 32780;
		public const int VISUAL_STEADY_STATE_STIMULATION_START = 32784;
		public const int VISUAL_STEADY_STATE_STIMULATION_STOP  = 32785;

		public const int BUTTON1_PRESSED  = 32786;
		public const int BUTTON1_RELEASED = 32787;
		public const int BUTTON2_PRESSED  = 32788;
		public const int BUTTON2_RELEASED = 32789;
		public const int BUTTON3_PRESSED  = 32790;
		public const int BUTTON3_RELEASED = 32791;
		public const int BUTTON4_PRESSED  = 32792;
		public const int BUTTON4_RELEASED = 32793;

		public const int LABEL_00 = 33024;
		public const int LABEL_01 = 33025;
		public const int LABEL_02 = 33026;
		public const int LABEL_03 = 33027;
		public const int LABEL_04 = 33028;
		public const int LABEL_05 = 33029;
		public const int LABEL_06 = 33030;
		public const int LABEL_07 = 33031;
		public const int LABEL_08 = 33032;
		public const int LABEL_09 = 33033;
		public const int LABEL_10 = 33040;
		public const int LABEL_11 = 33041;
		public const int LABEL_12 = 33042;
		public const int LABEL_13 = 33043;
		public const int LABEL_14 = 33044;
		public const int LABEL_15 = 33045;
		public const int LABEL_16 = 33046;
		public const int LABEL_17 = 33047;
		public const int LABEL_18 = 33048;
		public const int LABEL_19 = 33049;
		public const int LABEL_20 = 33056;
		public const int LABEL_21 = 33057;
		public const int LABEL_22 = 33058;
		public const int LABEL_23 = 33059;
		public const int LABEL_24 = 33060;
		public const int LABEL_25 = 33061;
		public const int LABEL_26 = 33062;
		public const int LABEL_27 = 33063;
		public const int LABEL_28 = 33064;
		public const int LABEL_29 = 33065;
		public const int LABEL_30 = 33072;
		public const int LABEL_31 = 33073;
		public const int LABEL_32 = 33074;
		public const int LABEL_33 = 33075;
		public const int LABEL_34 = 33076;
		public const int LABEL_35 = 33077;
		public const int LABEL_36 = 33078;
		public const int LABEL_37 = 33079;
		public const int LABEL_38 = 33080;
		public const int LABEL_39 = 33081;
		public const int LABEL_40 = 33088;
		public const int LABEL_41 = 33089;
		public const int LABEL_42 = 33090;
		public const int LABEL_43 = 33091;
		public const int LABEL_44 = 33092;
		public const int LABEL_45 = 33093;
		public const int LABEL_46 = 33094;
		public const int LABEL_47 = 33095;
		public const int LABEL_48 = 33096;
		public const int LABEL_49 = 33097;
		public const int LABEL_50 = 33104;
		public const int LABEL_51 = 33105;
		public const int LABEL_52 = 33106;
		public const int LABEL_53 = 33107;
		public const int LABEL_54 = 33108;
		public const int LABEL_55 = 33109;
		public const int LABEL_56 = 33110;
		public const int LABEL_57 = 33111;
		public const int LABEL_58 = 33112;
		public const int LABEL_59 = 33113;
		public const int LABEL_60 = 33120;
		public const int LABEL_61 = 33121;
		public const int LABEL_62 = 33122;
		public const int LABEL_63 = 33123;
		public const int LABEL_64 = 33124;
		public const int LABEL_65 = 33125;
		public const int LABEL_66 = 33126;
		public const int LABEL_67 = 33127;
		public const int LABEL_68 = 33128;
		public const int LABEL_69 = 33129;
		public const int LABEL_70 = 33136;
		public const int LABEL_71 = 33137;
		public const int LABEL_72 = 33138;
		public const int LABEL_73 = 33139;
		public const int LABEL_74 = 33140;
		public const int LABEL_75 = 33141;
		public const int LABEL_76 = 33142;
		public const int LABEL_77 = 33143;
		public const int LABEL_78 = 33144;
		public const int LABEL_79 = 33145;
		public const int LABEL_80 = 33152;
		public const int LABEL_81 = 33153;
		public const int LABEL_82 = 33154;
		public const int LABEL_83 = 33155;
		public const int LABEL_84 = 33156;
		public const int LABEL_85 = 33157;
		public const int LABEL_86 = 33158;
		public const int LABEL_87 = 33159;
		public const int LABEL_88 = 33160;
		public const int LABEL_89 = 33161;
		public const int LABEL_90 = 33168;
		public const int LABEL_91 = 33169;
		public const int LABEL_92 = 33170;
		public const int LABEL_93 = 33171;
		public const int LABEL_94 = 33172;
		public const int LABEL_95 = 33173;
		public const int LABEL_96 = 33174;
		public const int LABEL_97 = 33175;
		public const int LABEL_98 = 33176;
		public const int LABEL_99 = 33177;

		public const int TRAIN                     = 33281;
		public const int BEEP                      = 33282;
		public const int DOUBLE_BEEP               = 33283;
		public const int END_OF_FILE               = 33284;
		public const int TARGET                    = 33285;
		public const int NON_TARGET                = 33286;
		public const int TRAIN_COMPLETED           = 33287;
		public const int RESET                     = 33288;
		public const int THRESHOLD_PASSED_POSITIVE = 33289;
		public const int THRESHOLD_PASSED_NEGATIVE = 33296;
		public const int NO_ARTIFACT               = 33537;
		public const int ARTIFACT                  = 33538;
		public const int REMOVED_SAMPLES           = 33552;
		public const int ADDED_SAMPLES_BEGIN       = 33553;
		public const int ADDED_SAMPLES_END         = 33554;
		public const int GDF_LIGHTS_OFF            = 33824;
	}
}
