﻿using System;
using System.Runtime.InteropServices;

/**
* C# API for the lab streaming layer.
* 
* The lab streaming layer provides a set of functions to make instrument data accessible 
* in real time within a lab network. From there, streams can be picked up by recording programs, 
* viewing programs or custom experiment applications that access data streams in real time.
*
* The API covers two areas:
* - The "push API" allows to create stream outlets and to push data (regular or irregular measurement 
*   time series, event data, coded audio/video frames, etc.) into them.
* - The "pull API" allows to create stream inlets and read time-synched experiment data from them 
*   (for recording, viewing or experiment control).
*
*/
namespace LSL4Unity
{
	public class liblsl
	{
		/**
		* Constant to indicate that a stream has variable sampling rate.
		*/
		public const double IRREGULAR_RATE = 0.0;

		/**
		* Constant to indicate that a sample has the next successive time stamp.
		* This is an optional optimization to transmit less data per sample.
		* The stamp is then deduced from the preceding one according to the stream's sampling rate 
		* (in the case of an irregular rate, the same time stamp as before will is assumed).
		*/
		public const double DEDUCED_TIMESTAMP = -1.0;


		/**
		* A very large time duration (> 1 year) for timeout values.
		* Note that significantly larger numbers can cause the timeout to be invalid on some operating systems (e.g., 32-bit UNIX).
		*/
		public const double FOREVER = 32000000.0;

		/**
		* Data format of a channel (each transmitted sample holds an array of channels).
		*/
		public enum channel_format_t : byte
		{
			cf_float32 = 1, // For up to 24-bit precision measurements in the appropriate physical unit 

			// (e.g., microvolts). Integers from -16777216 to 16777216 are represented accurately.
			cf_double64 = 2, // For universal numeric data as long as permitted by network & disk budget. 

			// The largest representable integer is 53-bit.
			cf_string = 3, // For variable-length ASCII strings or data blobs, such as video frames,

			// complex event descriptions, etc.
			cf_int32 = 4, // For high-rate digitized formats that require 32-bit precision. Depends critically on 

			// meta-data to represent meaningful units. Useful for application event codes or other coded data.
			cf_int16 = 5, // For very high rate signals (40Khz+) or consumer-grade audio 

			// (for professional audio float is recommended).
			cf_int8 = 6, // For binary signals or other coded data. 

			// Not recommended for encoding string data.
			cf_int64 = 7, // For now only for future compatibility. Support for this type is not yet exposed in all languages. 

			// Also, some builds of liblsl will not be able to send or receive data of this type.
			cf_undefined = 0 // Can not be transmitted.
		}

		/**
		* Post-processing options for stream inlets. 
		*/
		public enum processing_options_t : byte
		{
			post_none = 0, // No automatic post-processing; return the ground-truth time stamps for manual post-processing

			// (this is the default behavior of the inlet).
			post_clocksync = 1, // Perform automatic clock synchronization; equivalent to manually adding the time_correction() value

			// to the received time stamps.
			post_dejitter = 2, // Remove jitter from time stamps. This will apply a smoothing algorithm to the received time stamps;

			// the smoothing needs to see a minimum number of samples (30-120 seconds worst-case) until the remaining  
			// jitter is consistently below 1ms.
			post_monotonize = 4,            // Force the time-stamps to be monotonically ascending (only makes sense if timestamps are dejittered).
			post_threadsafe = 8,            // Post-processing is thread-safe (same inlet can be read from by multiple threads); uses somewhat more CPU.
			post_ALL        = 1 | 2 | 4 | 8 // The combination of all possible post-processing options.
		}

		/**
		* Protocol version.
		* The major version is protocol_version() / 100;
		* The minor version is protocol_version() % 100;
		* Clients with different minor versions are protocol-compatible with each other 
		* while clients with different major versions will refuse to work together.
		*/
		public static int protocol_version() { return dll.lsl_protocol_version(); }

		/**
		* Version of the liblsl library.
		* The major version is library_version() / 100;
		* The minor version is library_version() % 100;
		*/
		public static int library_version() { return dll.lsl_library_version(); }

		/**
		* Obtain a local system time stamp in seconds. The resolution is better than a millisecond.
		* This reading can be used to assign time stamps to samples as they are being acquired. 
		* If the "age" of a sample is known at a particular time (e.g., from USB transmission 
		* delays), it can be used as an offset to local_clock() to obtain a better estimate of 
		* when a sample was actually captured. See stream_outlet::push_sample() for a use case.
		*/
		public static double local_clock() { return dll.lsl_local_clock(); }


		// ==========================
		// === Stream Declaration ===
		// ==========================

		/**
		* The stream_info object stores the declaration of a data stream.
		* Represents the following information:
		*  a) stream data format (#channels, channel format)
		*  b) core information (stream name, content type, sampling rate)
		*  c) optional meta-data about the stream content (channel labels, measurement units, etc.)
		*
		* Whenever a program wants to provide a new stream on the lab network it will typically first 
		* create a stream_info to describe its properties and then construct a stream_outlet with it to create
		* the stream on the network. Recipients who discover the outlet can query the stream_info; it is also
		* written to disk when recording the stream (playing a similar role as a file header).
		*/

		public class StreamInfo
		{
			/**
			* Construct a new StreamInfo object.
			* Core stream information is specified here. Any remaining meta-data can be added later.
			* @param name Name of the stream. Describes the device (or product series) that this stream makes available 
			*             (for use by programs, experimenters or data analysts). Cannot be empty.
			* @param type Content type of the stream. Please see https://github.com/sccn/xdf/wiki/Meta-Data (or web search for:
			*             XDF meta-data) for pre-defined content-type names, but you can also make up your own.
			*             The content type is the preferred way to find streams (as opposed to searching by name).
			* @param channel_count Number of channels per sample. This stays constant for the lifetime of the stream.
			* @param nominal_srate The sampling rate (in Hz) as advertised by the data source, if regular (otherwise set to IRREGULAR_RATE).
			* @param channel_format Format/type of each channel. If your channels have different formats, consider supplying 
			*                       multiple streams or use the largest type that can hold them all (such as cf_double64).
			* @param source_id Unique identifier of the device or source of the data, if available (such as the serial number). 
			*                  This is critical for system robustness since it allows recipients to recover from failure even after the 
			*                  serving app, device or computer crashes (just by finding a stream with the same source id on the network again).
			*                  Therefore, it is highly recommended to always try to provide whatever information can uniquely identify the data source itself.
			*/
			public StreamInfo(string           name, string type, int channelCount = 1,
							  double           sampling  = IRREGULAR_RATE,
							  channel_format_t channelFormat = channel_format_t.cf_float32, string sourceId = "")
			{
				_obj = dll.lsl_create_streaminfo(name, type, channelCount, sampling, channelFormat, sourceId);
			}

			public StreamInfo(IntPtr handle) { _obj = handle; }

			/// Destroy a previously created streaminfo object.
			~StreamInfo() { dll.lsl_destroy_streaminfo(_obj); }

			// ========================
			// === Core Information ===
			// ========================
			// (these fields are assigned at construction)

			/**
			* Name of the stream.
			* This is a human-readable name. For streams offered by device modules, it refers to the type of device or product series 
			* that is generating the data of the stream. If the source is an application, the name may be a more generic or specific identifier.
			* Multiple streams with the same name can coexist, though potentially at the cost of ambiguity (for the recording app or experimenter).
			*/
			public string Name() { return Marshal.PtrToStringAnsi(dll.lsl_get_name(_obj)); }


			/**
			* Content type of the stream.
			* The content type is a short string such as "EEG", "Gaze" which describes the content carried by the channel (if known). 
			* If a stream contains mixed content this value need not be assigned but may instead be stored in the description of channel types.
			* To be useful to applications and automated processing systems using the recommended content types is preferred. 
			* Content types usually follow those pre-defined in https://github.com/sccn/xdf/wiki/Meta-Data (or web search for: XDF meta-data).
			*/
			public string Type() { return Marshal.PtrToStringAnsi(dll.lsl_get_type(_obj)); }

			/**
			* Number of channels of the stream.
			* A stream has at least one channel; the channel count stays constant for all samples.
			*/
			public int channel_count() { return dll.lsl_get_channel_count(_obj); }

			/**
			* Sampling rate of the stream, according to the source (in Hz).
			* If a stream is irregularly sampled, this should be set to IRREGULAR_RATE.
			*
			* Note that no data will be lost even if this sampling rate is incorrect or if a device has temporary 
			* hiccups, since all samples will be recorded anyway (except for those dropped by the device itself). However, 
			* when the recording is imported into an application, a good importer may correct such errors more accurately 
			* if the advertised sampling rate was close to the specs of the device.
			*/
			public double nominal_srate() { return dll.lsl_get_nominal_srate(_obj); }

			/**
			* Channel format of the stream.
			* All channels in a stream have the same format. However, a device might offer multiple time-synched streams 
			* each with its own format.
			*/
			public channel_format_t channel_format() { return dll.lsl_get_channel_format(_obj); }

			/**
			* Unique identifier of the stream's source, if available.
			* The unique source (or device) identifier is an optional piece of information that, if available, allows that
			* endpoints (such as the recording program) can re-acquire a stream automatically once it is back online.
			*/
			public string source_id() { return Marshal.PtrToStringAnsi(dll.lsl_get_source_id(_obj)); }


			// ======================================
			// === Additional Hosting Information ===
			// ======================================
			// (these fields are implicitly assigned once bound to an outlet/inlet)

			/**
			* Protocol version used to deliver the stream.
			*/
			public int Version() { return dll.lsl_get_version(_obj); }

			/**
			* Creation time stamp of the stream.
			* This is the time stamp when the stream was first created
			* (as determined via local_clock() on the providing machine).
			*/
			public double created_at() { return dll.lsl_get_created_at(_obj); }

			/**
			* Unique ID of the stream outlet instance (once assigned).
			* This is a unique identifier of the stream outlet, and is guaranteed to be different
			* across multiple instantiations of the same outlet (e.g., after a re-start).
			*/
			public string Uid() { return Marshal.PtrToStringAnsi(dll.lsl_get_uid(_obj)); }

			/**
			* Session ID for the given stream.
			* The session id is an optional human-assigned identifier of the recording session.
			* While it is rarely used, it can be used to prevent concurrent recording activitites 
			* on the same sub-network (e.g., in multiple experiment areas) from seeing each other's streams 
			* (assigned via a configuration file by the experimenter, see Network Connectivity in the LSL wiki).
			*/
			public string session_id() { return Marshal.PtrToStringAnsi(dll.lsl_get_session_id(_obj)); }

			/**
			* Hostname of the providing machine.
			*/
			public string Hostname() { return Marshal.PtrToStringAnsi(dll.lsl_get_hostname(_obj)); }


			// ========================
			// === Data Description ===
			// ========================

			/**
			* Extended description of the stream.
			* It is highly recommended that at least the channel labels are described here. 
			* See code examples on the LSL wiki. Other information, such as amplifier settings, 
			* measurement units if deviating from defaults, setup information, subject information, etc., 
			* can be specified here, as well. Meta-data recommendations follow the XDF file format project
			* (github.com/sccn/xdf/wiki/Meta-Data or web search for: XDF meta-data).
			*
			* Important: if you use a stream content type for which meta-data recommendations exist, please 
			* try to lay out your meta-data in agreement with these recommendations for compatibility with other applications.
			*/
			public XMLElement Desc() { return new XMLElement(dll.lsl_get_desc(_obj)); }

			/**
			* Retrieve the entire stream_info in XML format.
			* This yields an XML document (in string form) whose top-level element is <info>. The info element contains
			* one element for each field of the stream_info class, including:
			*  a) the core elements <name>, <type>, <channel_count>, <nominal_srate>, <channel_format>, <source_id>
			*  b) the misc elements <version>, <created_at>, <uid>, <session_id>, <v4address>, <v4data_port>, <v4service_port>, <v6address>, <v6data_port>, <v6service_port>
			*  c) the extended description element <desc> with user-defined sub-elements.
			*/
			public string as_xml()
			{
				IntPtr pXml   = dll.lsl_get_xml(_obj);
				string strXml = Marshal.PtrToStringAnsi(pXml);
				dll.lsl_destroy_string(pXml);
				return strXml;
			}


			/**
			 * Get access to the underlying handle.
			 */
			public IntPtr Handle() { return _obj; }

			private readonly IntPtr _obj;
		}


		// =======================
		// ==== Stream Outlet ====
		// =======================

		/**
		* A stream outlet.
		* Outlets are used to make streaming data (and the meta-data) available on the lab network.
		*/
		public class StreamOutlet
		{
			/**
			* Establish a new stream outlet. This makes the stream discoverable.
			* @param info The stream information to use for creating this stream. Stays constant over the lifetime of the outlet.
			* @param chunk_size Optionally the desired chunk granularity (in samples) for transmission. If unspecified, 
			*                   each push operation yields one chunk. Inlets can override this setting.
			* @param max_buffered Optionally the maximum amount of data to buffer (in seconds if there is a nominal 
			*                     sampling rate, otherwise x100 in samples). The default is 6 minutes of data. 
			*/
			public StreamOutlet(StreamInfo info, int chunkSize = 0, int maxBuffered = 360)
			{
				_obj = dll.lsl_create_outlet(info.Handle(), chunkSize, maxBuffered);
			}

			/**
			* Destructor.
			* The stream will no longer be discoverable after destruction and all paired inlets will stop delivering data.
			*/
			~StreamOutlet() { dll.lsl_destroy_outlet(_obj); }


			// ========================================
			// === Pushing a sample into the outlet ===
			// ========================================

			/**
			* Push an array of values as a sample into the outlet. 
			* Each entry in the vector corresponds to one channel.
			* @param data An array of values to push (one for each channel).
			* @param time Optionally the capture time of the sample, in agreement with local_clock(); if omitted, the current time is used.
			* @param pushthrough Optionally whether to push the sample through to the receivers instead of buffering it with subsequent samples.
			*                    Note that the chunk_size, if specified at outlet construction, takes precedence over the pushthrough flag.
			*/
			public void push_sample(float[] data, double time = 0.0, bool pushthrough = true)
			{
				dll.lsl_push_sample_ftp(_obj, data, time, pushthrough ? 1 : 0);
			}

			public void push_sample(double[] data, double time = 0.0, bool pushthrough = true)
			{
				dll.lsl_push_sample_dtp(_obj, data, time, pushthrough ? 1 : 0);
			}

			public void push_sample(int[] data, double time = 0.0, bool pushthrough = true) { dll.lsl_push_sample_itp(_obj, data, time, pushthrough ? 1 : 0); }

			public void push_sample(short[] data, double time = 0.0, bool pushthrough = true)
			{
				dll.lsl_push_sample_stp(_obj, data, time, pushthrough ? 1 : 0);
			}

			public void push_sample(char[] data, double time = 0.0, bool pushthrough = true) { dll.lsl_push_sample_ctp(_obj, data, time, pushthrough ? 1 : 0); }

			public void push_sample(string[] data, double time = 0.0, bool pushthrough = true)
			{
				dll.lsl_push_sample_strtp(_obj, data, time, pushthrough ? 1 : 0);
			}


			// ===================================================
			// === Pushing an chunk of samples into the outlet ===
			// ===================================================

			/**
			* Push a chunk of samples into the outlet. Single time provided.
			* @param data A rectangular array of values for multiple samples.
			* @param time Optionally the capture time of the most recent sample, in agreement with local_clock(); if omitted, the current time is used.
			*                   The time stamps of other samples are automatically derived based on the sampling rate of the stream.
			* @param pushthrough Optionally whether to push the chunk through to the receivers instead of buffering it with subsequent samples.
			*                    Note that the chunk_size, if specified at outlet construction, takes precedence over the pushthrough flag.
			*/
			public void push_chunk(float[,] data, double time = 0.0, bool pushthrough = true)
			{
				dll.lsl_push_chunk_ftp(_obj, data, (uint) data.Length, time, pushthrough ? 1 : 0);
			}

			public void push_chunk(double[,] data, double time = 0.0, bool pushthrough = true)
			{
				dll.lsl_push_chunk_dtp(_obj, data, (uint) data.Length, time, pushthrough ? 1 : 0);
			}

			public void push_chunk(int[,] data, double time = 0.0, bool pushthrough = true)
			{
				dll.lsl_push_chunk_itp(_obj, data, (uint) data.Length, time, pushthrough ? 1 : 0);
			}

			public void push_chunk(short[,] data, double time = 0.0, bool pushthrough = true)
			{
				dll.lsl_push_chunk_stp(_obj, data, (uint) data.Length, time, pushthrough ? 1 : 0);
			}

			public void push_chunk(char[,] data, double time = 0.0, bool pushthrough = true)
			{
				dll.lsl_push_chunk_ctp(_obj, data, (uint) data.Length, time, pushthrough ? 1 : 0);
			}

			public void push_chunk(string[,] data, double time = 0.0, bool pushthrough = true)
			{
				dll.lsl_push_chunk_strtp(_obj, data, (uint) data.Length, time, pushthrough ? 1 : 0);
			}

			/**
			* Push a chunk of multiplexed samples into the outlet. One time per sample is provided.
			* @param data A rectangular array of values for multiple samples.
			* @param times An array of time values holding time stamps for each sample in the data buffer.
			* @param pushthrough Optionally whether to push the chunk through to the receivers instead of buffering it with subsequent samples.
			*                    Note that the chunk_size, if specified at outlet construction, takes precedence over the pushthrough flag.
			*/
			public void push_chunk(float[,] data, double[] times, bool pushthrough = true)
			{
				dll.lsl_push_chunk_ftnp(_obj, data, (uint) data.Length, times, pushthrough ? 1 : 0);
			}

			public void push_chunk(double[,] data, double[] times, bool pushthrough = true)
			{
				dll.lsl_push_chunk_dtnp(_obj, data, (uint) data.Length, times, pushthrough ? 1 : 0);
			}

			public void push_chunk(int[,] data, double[] times, bool pushthrough = true)
			{
				dll.lsl_push_chunk_itnp(_obj, data, (uint) data.Length, times, pushthrough ? 1 : 0);
			}

			public void push_chunk(short[,] data, double[] times, bool pushthrough = true)
			{
				dll.lsl_push_chunk_stnp(_obj, data, (uint) data.Length, times, pushthrough ? 1 : 0);
			}

			public void push_chunk(char[,] data, double[] times, bool pushthrough = true)
			{
				dll.lsl_push_chunk_ctnp(_obj, data, (uint) data.Length, times, pushthrough ? 1 : 0);
			}

			public void push_chunk(string[,] data, double[] times, bool pushthrough = true)
			{
				dll.lsl_push_chunk_strtnp(_obj, data, (uint) data.Length, times, pushthrough ? 1 : 0);
			}


			// ===============================
			// === Miscellaneous Functions ===
			// ===============================

			/**
			* Check whether consumers are currently registered.
			* While it does not hurt, there is technically no reason to push samples if there is no consumer.
			*/
			public bool have_consumers() { return dll.lsl_have_consumers(_obj) > 0; }

			/**
			* Wait until some consumer shows up (without wasting resources).
			* @return True if the wait was successful, false if the timeout expired.
			*/
			public bool wait_for_consumers(double timeout) { return dll.lsl_wait_for_consumers(_obj) > 0; }

			/**
			* Retrieve the stream info provided by this outlet.
			* This is what was used to create the stream (and also has the Additional Network Information fields assigned).
			*/
			public StreamInfo Info() { return new StreamInfo(dll.lsl_get_info(_obj)); }

			private readonly IntPtr _obj;
		}


		// ===========================
		// ==== Resolve Functions ====
		// ===========================

		/**
		* Resolve all streams on the network.
		* This function returns all currently available streams from any outlet on the network.
		* The network is usually the subnet specified at the local router, but may also include 
		* a multicast group of machines (given that the network supports it), or list of hostnames.
		* These details may optionally be customized by the experimenter in a configuration file 
		* (see Network Connectivity in the LSL wiki).
		* This is the default mechanism used by the browsing programs and the recording program.
		* @param wait_time The waiting time for the operation, in seconds, to search for streams.
		*                  Warning: If this is too short (less than 0.5s) only a subset (or none) of the 
		*                           outlets that are present on the network may be returned.
		* @return An array of stream info objects (excluding their desc field), any of which can 
		*         subsequently be used to open an inlet. The full info can be retrieve from the inlet.
		*/

		public static StreamInfo[] resolve_streams(double waitTime = 1.0)
		{
			IntPtr[]     buf = new IntPtr[1024];
			int          num = dll.lsl_resolve_all(buf, (uint) buf.Length, waitTime);
			StreamInfo[] res = new StreamInfo[num];
			for (int k = 0; k < num; k++) { res[k] = new StreamInfo(buf[k]); }
			return res;
		}

		/**
		* Resolve all streams with a specific value for a given property.
		* If the goal is to resolve a specific stream, this method is preferred over resolving all streams and then selecting the desired one.
		* @param prop The stream_info property that should have a specific value (e.g., "name", "type", "source_id", or "desc/manufaturer").
		* @param value The string value that the property should have (e.g., "EEG" as the type property).
		* @param minimum Optionally return at least this number of streams.
		* @param timeout Optionally a timeout of the operation, in seconds (default: no timeout).
		*                 If the timeout expires, less than the desired number of streams (possibly none) will be returned.
		* @return An array of matching stream info objects (excluding their meta-data), any of 
		*         which can subsequently be used to open an inlet.
		*/
		public static StreamInfo[] resolve_stream(string prop, string value, int minimum = 1, double timeout = FOREVER)
		{
			IntPtr[]     buf = new IntPtr[1024];
			int          num = dll.lsl_resolve_byprop(buf, (uint) buf.Length, prop, value, minimum, timeout);
			StreamInfo[] res = new StreamInfo[num];
			for (int k = 0; k < num; k++) { res[k] = new StreamInfo(buf[k]); }
			return res;
		}

		/**
		* Resolve all streams that match a given predicate.
		* Advanced query that allows to impose more conditions on the retrieved streams; the given string is an XPath 1.0 
		* predicate for the <info> node (omitting the surrounding []'s), see also http://en.wikipedia.org/w/index.php?title=XPath_1.0&oldid=474981951.
		* @param pred The predicate string, e.g. "name='BioSemi'" or "type='EEG' and starts-with(name,'BioSemi') and count(info/desc/channel)=32"
		* @param minimum Return at least this number of streams.
		* @param timeout Optionally a timeout of the operation, in seconds (default: no timeout).
		*                 If the timeout expires, less than the desired number of streams (possibly none) will be returned.
		* @return An array of matching stream info objects (excluding their meta-data), any of 
		*         which can subsequently be used to open an inlet.
		*/
		public static StreamInfo[] resolve_stream(string pred, int minimum = 1, double timeout = FOREVER)
		{
			IntPtr[]     buf = new IntPtr[1024];
			int          num = dll.lsl_resolve_bypred(buf, (uint) buf.Length, pred, minimum, timeout);
			StreamInfo[] res = new StreamInfo[num];
			for (int k = 0; k < num; k++) { res[k] = new StreamInfo(buf[k]); }
			return res;
		}


		// ======================
		// ==== Stream Inlet ====
		// ======================

		/**
		* A stream inlet.
		* Inlets are used to receive streaming data (and meta-data) from the lab network.
		*/
		public class StreamInlet
		{
			/**
					* Construct a new stream inlet from a resolved stream info.
					* @param info A resolved stream info object (as coming from one of the resolver functions).
					*             Note: the stream_inlet may also be constructed with a fully-specified stream_info, 
					*                   if the desired channel format and count is already known up-front, but this is 
					*                   strongly discouraged and should only ever be done if there is no time to resolve the 
					*                   stream up-front (e.g., due to limitations in the client program).
					* @param max_buflen Optionally the maximum amount of data to buffer (in seconds if there is a nominal 
					*                   sampling rate, otherwise x100 in samples). Recording applications want to use a fairly 
					*                   large buffer size here, while real-time applications would only buffer as much as 
					*                   they need to perform their next calculation.
					* @param max_chunklen Optionally the maximum size, in samples, at which chunks are transmitted 
					*                     (the default corresponds to the chunk sizes used by the sender).
					*                     Recording applications can use a generous size here (leaving it to the network how 
					*                     to pack things), while real-time applications may want a finer (perhaps 1-sample) granularity.
										  If left unspecified (=0), the sender determines the chunk granularity.
					* @param recover Try to silently recover lost streams that are recoverable (=those that that have a source_id set). 
					*                In all other cases (recover is false or the stream is not recoverable) functions may throw a 
					*                LostException if the stream's source is lost (e.g., due to an app or computer crash).
					*/
			public StreamInlet(StreamInfo info, int maxBuflen = 360, int maxChunklen = 0, bool recover = true)
			{
				_obj = dll.lsl_create_inlet(info.Handle(), maxBuflen, maxChunklen, recover ? 1 : 0);
			}

			/** 
			* Destructor.
			* The inlet will automatically disconnect if destroyed.
			*/
			~StreamInlet() { dll.lsl_destroy_inlet(_obj); }

			/**
			* Retrieve the complete information of the given stream, including the extended description.
			* Can be invoked at any time of the stream's lifetime.
			* @param timeout Timeout of the operation (default: no timeout).
			* @throws TimeoutException (if the timeout expires), or LostException (if the stream source has been lost).
			*/

			public StreamInfo Info(double timeout = FOREVER)
			{
				int    ec  = 0;
				IntPtr res = dll.lsl_get_fullinfo(_obj, timeout, ref ec);
				check_error(ec);
				return new StreamInfo(res);
			}

			/**
			* Subscribe to the data stream.
			* All samples pushed in at the other end from this moment onwards will be queued and 
			* eventually be delivered in response to pull_sample() or pull_chunk() calls. 
			* Pulling a sample without some preceding open_stream is permitted (the stream will then be opened implicitly).
			* @param timeout Optional timeout of the operation (default: no timeout).
			* @throws TimeoutException (if the timeout expires), or LostException (if the stream source has been lost).
			*/

			public void open_stream(double timeout = FOREVER)
			{
				int ec = 0;
				dll.lsl_open_stream(_obj, timeout, ref ec);
				check_error(ec);
			}

			/**
			* Set post-processing flags to use. By default, the inlet performs NO post-processing and returns the 
			* ground-truth time stamps, which can then be manually synchronized using time_correction(), and then 
			* smoothed/dejittered if desired. This function allows automating these two and possibly more operations.
			* Warning: when you enable this, you will no longer receive or be able to recover the original time stamps.
			* @param flags An integer that is the result of bitwise OR'ing one or more options from processing_options_t 
			*        together (e.g., post_clocksync|post_dejitter); the default is to enable all options.
			*/
			public void set_postprocessing(processing_options_t postFlags = processing_options_t.post_ALL) { dll.lsl_set_postprocessing(_obj, postFlags); }

			/**
			* Drop the current data stream.
			* All samples that are still buffered or in flight will be dropped and transmission 
			* and buffering of data for this inlet will be stopped. If an application stops being 
			* interested in data from a source (temporarily or not) but keeps the outlet alive, 
			* it should call close_stream() to not waste unnecessary system and network 
			* resources.
			*/
			public void close_stream() { dll.lsl_close_stream(_obj); }

			/**
			* Retrieve an estimated time correction offset for the given stream.
			* The first call to this function takes several miliseconds until a reliable first estimate is obtained.
			* Subsequent calls are instantaneous (and rely on periodic background updates).
			* The precision of these estimates should be below 1 ms (empirically within +/-0.2 ms).
			* @timeout Timeout to acquire the first time-correction estimate (default: no timeout).
			* @return The time correction estimate. This is the number that needs to be added to a time stamp 
			*         that was remotely generated via lsl_local_clock() to map it into the local clock domain of this machine.
			* @throws TimeoutException (if the timeout expires), or LostException (if the stream source has been lost).
			*/

			public double time_correction(double timeout = FOREVER)
			{
				int    ec  = 0;
				double res = dll.lsl_time_correction(_obj, timeout, ref ec);
				check_error(ec);
				return res;
			}

			// =======================================
			// === Pulling a sample from the inlet ===
			// =======================================

			/**
			* Pull a sample from the inlet and read it into an array of values.
			* Handles type checking & conversion.
			* @param sample An array to hold the resulting values.
			* @param timeout The timeout for this operation, if any. Use 0.0 to make the function non-blocking.
			* @return The capture time of the sample on the remote machine, or 0.0 if no new sample was available. 
			*          To remap this time stamp to the local clock, add the value returned by .time_correction() to it. 
			* @throws LostException (if the stream source has been lost).
			*/

			public double pull_sample(float[] sample, double timeout = FOREVER)
			{
				int    ec  = 0;
				double res = dll.lsl_pull_sample_f(_obj, sample, sample.Length, timeout, ref ec);
				check_error(ec);
				return res;
			}

			public double pull_sample(double[] sample, double timeout = FOREVER)
			{
				int    ec  = 0;
				double res = dll.lsl_pull_sample_d(_obj, sample, sample.Length, timeout, ref ec);
				check_error(ec);
				return res;
			}

			public double pull_sample(int[] sample, double timeout = FOREVER)
			{
				int    ec  = 0;
				double res = dll.lsl_pull_sample_i(_obj, sample, sample.Length, timeout, ref ec);
				check_error(ec);
				return res;
			}

			public double pull_sample(short[] sample, double timeout = FOREVER)
			{
				int    ec  = 0;
				double res = dll.lsl_pull_sample_s(_obj, sample, sample.Length, timeout, ref ec);
				check_error(ec);
				return res;
			}

			public double pull_sample(char[] sample, double timeout = FOREVER)
			{
				int    ec  = 0;
				double res = dll.lsl_pull_sample_c(_obj, sample, sample.Length, timeout, ref ec);
				check_error(ec);
				return res;
			}

			public double pull_sample(string[] sample, double timeout = FOREVER)
			{
				int      ec  = 0;
				IntPtr[] tmp = new IntPtr[sample.Length];
				double   res = dll.lsl_pull_sample_str(_obj, tmp, tmp.Length, timeout, ref ec);
				check_error(ec);
				try
				{
					for (int k = 0; k < tmp.Length; k++) { sample[k] = Marshal.PtrToStringAnsi(tmp[k]); }
				}
				finally
				{
					foreach (IntPtr t in tmp) { dll.lsl_destroy_string(t); }
				}
				return res;
			}


			// =================================================
			// === Pulling a chunk of samples from the inlet ===
			// =================================================

			/**
			* Pull a chunk of data from the inlet.
			* @param data_buffer A pre-allocated buffer where the channel data shall be stored.
			* @param timestamp_buffer A pre-allocated buffer where time stamps shall be stored. 
			* @param timeout Optionally the timeout for this operation, if any. When the timeout expires, the function 
			*                may return before the entire buffer is filled. The default value of 0.0 will retrieve only 
			*                data available for immediate pickup.
			* @return samples_written Number of samples written to the data and timestamp buffers.
			* @throws LostException (if the stream source has been lost).
			*/

			public int pull_chunk(float[,] buffer, double[] times, double timeout = 0.0)
			{
				int  ec  = 0;
				uint res = dll.lsl_pull_chunk_f(_obj, buffer, times, (uint) buffer.Length, (uint) times.Length, timeout, ref ec);
				check_error(ec);
				return (int) res / buffer.GetLength(1);
			}

			public int pull_chunk(double[,] buffer, double[] times, double timeout = 0.0)
			{
				int  ec  = 0;
				uint res = dll.lsl_pull_chunk_d(_obj, buffer, times, (uint) buffer.Length, (uint) times.Length, timeout, ref ec);
				check_error(ec);
				return (int) res / buffer.GetLength(1);
			}

			public int pull_chunk(int[,] buffer, double[] times, double timeout = 0.0)
			{
				int  ec  = 0;
				uint res = dll.lsl_pull_chunk_i(_obj, buffer, times, (uint) buffer.Length, (uint) times.Length, timeout, ref ec);
				check_error(ec);
				return (int) res / buffer.GetLength(1);
			}

			public int pull_chunk(short[,] buffer, double[] times, double timeout = 0.0)
			{
				int  ec  = 0;
				uint res = dll.lsl_pull_chunk_s(_obj, buffer, times, (uint) buffer.Length, (uint) times.Length, timeout, ref ec);
				check_error(ec);
				return (int) res / buffer.GetLength(1);
			}

			public int pull_chunk(char[,] buffer, double[] times, double timeout = 0.0)
			{
				int  ec  = 0;
				uint res = dll.lsl_pull_chunk_c(_obj, buffer, times, (uint) buffer.Length, (uint) times.Length, timeout, ref ec);
				check_error(ec);
				return (int) res / buffer.GetLength(1);
			}

			public int pull_chunk(string[,] buffer, double[] times, double timeout = 0.0)
			{
				int       ec  = 0;
				IntPtr[,] tmp = new IntPtr[buffer.GetLength(0), buffer.GetLength(1)];
				uint      res = dll.lsl_pull_chunk_str(_obj, tmp, times, (uint) tmp.Length, (uint) times.Length, timeout, ref ec);
				check_error(ec);
				try
				{
					for (int s = 0; s < tmp.GetLength(0); s++)
					{
						for (int c = 0; c < tmp.GetLength(1); c++) { buffer[s, c] = Marshal.PtrToStringAnsi(tmp[s, c]); }
					}
				}
				finally
				{
					for (int s = 0; s < tmp.GetLength(0); s++)
					{
						for (int c = 0; c < tmp.GetLength(1); c++) { dll.lsl_destroy_string(tmp[s, c]); }
					}
				}
				return (int) res / buffer.GetLength(1);
			}

			/**
			* Query whether samples are currently available for immediate pickup.
			* Note that it is not a good idea to use samples_available() to determine whether 
			* a pull_*() call would block: to be sure, set the pull timeout to 0.0 or an acceptably
			* low value. If the underlying implementation supports it, the value will be the number of 
			* samples available (otherwise it will be 1 or 0).
			*/
			public int samples_available() { return (int) dll.lsl_samples_available(_obj); }

			/**
			* Query whether the clock was potentially reset since the last call to was_clock_reset().
			* This is a rarely-used function that is only useful to applications that combine multiple time_correction 
			* values to estimate precise clock drift; it allows to tolerate cases where the source machine was 
			* hot-swapped or restarted in between two measurements.
			*/
			public bool was_clock_reset() { return (int) dll.lsl_was_clock_reset(_obj) != 0; }

			private readonly IntPtr _obj;
		}


		// =====================
		// ==== XML Element ====
		// =====================

		/**
		* A lightweight XML element tree; models the .desc() field of stream_info.
		* Has a name and can have multiple named children or have text content as value; attributes are omitted.
		* Insider note: The interface is modeled after a subset of pugixml's node type and is compatible with it.
		* See also http://pugixml.googlecode.com/svn/tags/latest/docs/manual/access.html for additional documentation.
		*/
		public struct XMLElement
		{
			public XMLElement(IntPtr handle) { _obj = handle; }

			// === Tree Navigation ===

			/// Get the first child of the element.
			public XMLElement first_child() { return new XMLElement(dll.lsl_first_child(_obj)); }

			/// Get the last child of the element.
			public XMLElement last_child() { return new XMLElement(dll.lsl_last_child(_obj)); }

			/// Get the next sibling in the children list of the parent node.
			public XMLElement next_sibling() { return new XMLElement(dll.lsl_next_sibling(_obj)); }

			/// Get the previous sibling in the children list of the parent node.
			public XMLElement previous_sibling() { return new XMLElement(dll.lsl_previous_sibling(_obj)); }

			/// Get the parent node.
			public XMLElement Parent() { return new XMLElement(dll.lsl_parent(_obj)); }


			// === Tree Navigation by Name ===

			/// Get a child with a specified name.
			public XMLElement Child(string name) { return new XMLElement(dll.lsl_child(_obj, name)); }

			/// Get the next sibling with the specified name.
			public XMLElement next_sibling(string name) { return new XMLElement(dll.lsl_next_sibling_n(_obj, name)); }

			/// Get the previous sibling with the specified name.
			public XMLElement previous_sibling(string name) { return new XMLElement(dll.lsl_previous_sibling_n(_obj, name)); }


			// === Content Queries ===

			/// Whether this node is empty.
			public bool Empty() { return dll.lsl_empty(_obj) != 0; }

			/// Whether this is a text body (instead of an XML element). True both for plain char data and CData.
			public bool is_text() { return dll.lsl_is_text(_obj) != 0; }

			/// Name of the element.
			public string Name() { return Marshal.PtrToStringAnsi(dll.lsl_name(_obj)); }

			/// Value of the element.
			public string Value() { return Marshal.PtrToStringAnsi(dll.lsl_value(_obj)); }

			/// Get child value (value of the first child that is text).
			public string child_value() { return Marshal.PtrToStringAnsi(dll.lsl_child_value(_obj)); }

			/// Get child value of a child with a specified name.
			public string child_value(string name) { return Marshal.PtrToStringAnsi(dll.lsl_child_value_n(_obj, name)); }


			// === Modification ===

			/**
			* Append a child node with a given name, which has a (nameless) plain-text child with the given text value.
			*/
			public XMLElement append_child_value(string name, string value) { return new XMLElement(dll.lsl_append_child_value(_obj, name, value)); }

			/**
			* Prepend a child node with a given name, which has a (nameless) plain-text child with the given text value.
			*/
			public XMLElement prepend_child_value(string name, string value) { return new XMLElement(dll.lsl_prepend_child_value(_obj, name, value)); }

			/**
			* Set the text value of the (nameless) plain-text child of a named child node.
			*/
			public bool set_child_value(string name, string value) { return dll.lsl_set_child_value(_obj, name, value) != 0; }

			/**
			* Set the element's name.
			* @return False if the node is empty.
			*/
			public bool set_name(string rhs) { return dll.lsl_set_name(_obj, rhs) != 0; }

			/**
			* Set the element's value.
			* @return False if the node is empty.
			*/
			public bool set_value(string rhs) { return dll.lsl_set_value(_obj, rhs) != 0; }

			/// Append a child element with the specified name.
			public XMLElement append_child(string name) { return new XMLElement(dll.lsl_append_child(_obj, name)); }

			/// Prepend a child element with the specified name.
			public XMLElement prepend_child(string name) { return new XMLElement(dll.lsl_prepend_child(_obj, name)); }

			/// Append a copy of the specified element as a child.
			public XMLElement append_copy(XMLElement e) { return new XMLElement(dll.lsl_append_copy(_obj, e._obj)); }

			/// Prepend a child element with the specified name.
			public XMLElement prepend_copy(XMLElement e) { return new XMLElement(dll.lsl_prepend_copy(_obj, e._obj)); }

			/// Remove a child element with the specified name.
			public void remove_child(string name) { dll.lsl_remove_child_n(_obj, name); }

			/// Remove a specified child element.
			public void remove_child(XMLElement e) { dll.lsl_remove_child(_obj, e._obj); }

			private readonly IntPtr _obj;
		}


		// ===========================
		// === Continuous Resolver ===
		// ===========================

		/** 
		* A convenience class that resolves streams continuously in the background throughout 
		* its lifetime and which can be queried at any time for the set of streams that are currently 
		* visible on the network.
		*/

		public class ContinuousResolver
		{
			/**
			* Construct a new continuous_resolver that resolves all streams on the network. 
			* This is analogous to the functionality offered by the free function resolve_streams().
			* @param forget_after When a stream is no longer visible on the network (e.g., because it was shut down),
			*                     this is the time in seconds after which it is no longer reported by the resolver.
			*/
			public ContinuousResolver(double forgetAfter = 5.0) { _obj = dll.lsl_create_continuous_resolver(forgetAfter); }

			/**
			* Construct a new continuous_resolver that resolves all streams with a specific value for a given property.
			* This is analogous to the functionality provided by the free function resolve_stream(prop,value).
			* @param prop The stream_info property that should have a specific value (e.g., "name", "type", "source_id", or "desc/manufaturer").
			* @param value The string value that the property should have (e.g., "EEG" as the type property).
			* @param forget_after When a stream is no longer visible on the network (e.g., because it was shut down),
			*                     this is the time in seconds after which it is no longer reported by the resolver.
			*/
			public ContinuousResolver(string prop, string value, double forgetAfter = 5.0)
			{
				_obj = dll.lsl_create_continuous_resolver_byprop(prop, value, forgetAfter);
			}

			/**
			* Construct a new continuous_resolver that resolves all streams that match a given XPath 1.0 predicate.
			* This is analogous to the functionality provided by the free function resolve_stream(pred).
			* @param pred The predicate string, e.g. "name='BioSemi'" or "type='EEG' and starts-with(name,'BioSemi') and count(info/desc/channel)=32"
			* @param forget_after When a stream is no longer visible on the network (e.g., because it was shut down),
			*                     this is the time in seconds after which it is no longer reported by the resolver.
			*/
			public ContinuousResolver(string pred, double forgetAfter = 5.0) { _obj = dll.lsl_create_continuous_resolver_bypred(pred, forgetAfter); }

			/** 
			* Destructor.
			*/
			~ContinuousResolver() { dll.lsl_destroy_continuous_resolver(_obj); }

			/**
			* Obtain the set of currently present streams on the network (i.e. resolve result).
			* @return An array of matching stream info objects (excluding their meta-data), any of 
			*         which can subsequently be used to open an inlet.
			*/
			public StreamInfo[] Results()
			{
				IntPtr[]     buf = new IntPtr[1024];
				int          num = dll.lsl_resolver_results(_obj, buf, (uint) buf.Length);
				StreamInfo[] res = new StreamInfo[num];
				for (int k = 0; k < num; k++) { res[k] = new StreamInfo(buf[k]); }
				return res;
			}

			private readonly IntPtr _obj;
		}

		// =======================
		// === Exception Types ===
		// =======================

		/**
		 * Exception class that indicates that a stream inlet's source has been irrecoverably lost.
		 */
		public class LostException : Exception
		{
			public LostException() { }
			public LostException(string                                            message) { }
			public LostException(string                                            message, Exception                                     inner) { }
			protected LostException(System.Runtime.Serialization.SerializationInfo info,    System.Runtime.Serialization.StreamingContext context) { }
		}

		/**
		 * Exception class that indicates that an internal error has occurred inside liblsl.
		 */
		public class InternalException : Exception
		{
			public InternalException() { }
			public InternalException(string                                            message) { }
			public InternalException(string                                            message, Exception                                     inner) { }
			protected InternalException(System.Runtime.Serialization.SerializationInfo info,    System.Runtime.Serialization.StreamingContext context) { }
		}

		/**
		 * Check an error condition and throw an exception if appropriate.
		 */
		public static void check_error(int ec)
		{
			if (ec < 0)
			{
				switch (ec)
				{
					case -1: throw new TimeoutException("The operation failed due to a timeout.");
					case -2: throw new LostException("The stream has been lost.");
					case -3: throw new ArgumentException("An argument was incorrectly specified (e.g., wrong format or wrong length).");
					case -4: throw new InternalException("An internal internal error has occurred.");
					default: throw new Exception("An unknown error has occurred.");
				}
			}
		}


		// === Internal: C library function definitions. ===

		private class dll
		{
#if (UNITY_EDITOR_WIN && UNITY_EDITOR_64)
			private const string LIBNAME = "liblsl64";
#elif UNITY_EDITOR_WIN
			const string LIBNAME = "liblsl32";
#elif UNITY_STANDALONE_WIN
			// a build hook will took care that the correct dll will be renamed after a successfull build 
			const string LIBNAME = "liblsl";
#elif (UNITY_EDITOR_LINUX && UNITY_EDITOR_64) || UNITY_STANDALONE_LINUX
			const string LIBNAME = "liblsl64.so";
#elif UNITY_EDITOR_LINUX
			const string LIBNAME = "liblsl32.so";
#elif UNITY_STANDALONE_LINUX
			const string LIBNAME = "liblsl.so";
#elif Unity_EDITOR_OSX || UNITY_STANDALONE_OSX
			//32-bit dylib no longer provided.
			const string LIBNAME = "liblsl64";
#elif UNITY_STANDALONE_OSX
			const string LIBNAME = "liblsl";
#elif UNITY_ANDROID
			const string LIBNAME = "lslAndroid";
#endif

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_protocol_version();

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_library_version();

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern double lsl_local_clock();

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_create_streaminfo(string name, string type, int channelCount, double sampling, channel_format_t channelFormat,
															  string sourceId);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_destroy_streaminfo(IntPtr info);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_get_name(IntPtr info);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_get_type(IntPtr info);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_get_channel_count(IntPtr info);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern double lsl_get_nominal_srate(IntPtr info);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern channel_format_t lsl_get_channel_format(IntPtr info);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_get_source_id(IntPtr info);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_get_version(IntPtr info);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern double lsl_get_created_at(IntPtr info);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_get_uid(IntPtr info);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_get_session_id(IntPtr info);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_get_hostname(IntPtr info);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_get_desc(IntPtr info);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_get_xml(IntPtr info);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_create_outlet(IntPtr info, int chunkSize, int maxBuffered);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern void lsl_destroy_outlet(IntPtr obj);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_sample_ftp(IntPtr obj, float[] data, double timestamp, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_sample_dtp(IntPtr obj, double[] data, double timestamp, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_sample_itp(IntPtr obj, int[] data, double timestamp, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_sample_stp(IntPtr obj, short[] data, double timestamp, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_sample_ctp(IntPtr obj, char[] data, double timestamp, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_sample_strtp(IntPtr obj, string[] data, double timestamp, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_sample_buftp(IntPtr obj, char[][] data, uint[] lengths, double timestamp, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_chunk_ftp(IntPtr obj, float[,] data, uint dataElements, double timestamp, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_chunk_ftnp(IntPtr obj, float[,] data, uint dataElements, double[] timestamps, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_chunk_dtp(IntPtr obj, double[,] data, uint dataElements, double timestamp, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_chunk_dtnp(IntPtr obj, double[,] data, uint dataElements, double[] timestamps, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_chunk_itp(IntPtr obj, int[,] data, uint dataElements, double timestamp, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_chunk_itnp(IntPtr obj, int[,] data, uint dataElements, double[] timestamps, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_chunk_stp(IntPtr obj, short[,] data, uint dataElements, double timestamp, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_chunk_stnp(IntPtr obj, short[,] data, uint dataElements, double[] timestamps, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_chunk_ctp(IntPtr obj, char[,] data, uint dataElements, double timestamp, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_chunk_ctnp(IntPtr obj, char[,] data, uint dataElements, double[] timestamps, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_chunk_strtp(IntPtr obj, string[,] data, uint dataElements, double timestamp, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_chunk_strtnp(IntPtr obj, string[,] data, uint dataElements, double[] timestamps, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_chunk_buftp(IntPtr obj, char[][] data, uint[] lengths, uint dataElements, double timestamp, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_push_chunk_buftnp(IntPtr obj, char[][] data, uint[] lengths, uint dataElements, double[] timestamps, int pushthrough);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_have_consumers(IntPtr obj);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_wait_for_consumers(IntPtr obj);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_get_info(IntPtr obj);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_resolve_all(IntPtr[] buffer, uint bufferElements, double waitTime);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_resolve_byprop(IntPtr[] buffer, uint bufferElements, string prop, string value, int minimum, double waitTime);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_resolve_bypred(IntPtr[] buffer, uint bufferElements, string pred, int minimum, double waitTime);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_create_inlet(IntPtr info, int maxBuflen, int maxChunklen, int recover);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern void lsl_destroy_inlet(IntPtr obj);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_get_fullinfo(IntPtr obj, double timeout, ref int ec);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern void lsl_open_stream(IntPtr obj, double timeout, ref int ec);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern void lsl_set_postprocessing(IntPtr obj, processing_options_t processingFlags);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern void lsl_close_stream(IntPtr obj);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern double lsl_time_correction(IntPtr obj, double timeout, ref int ec);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern double lsl_pull_sample_f(IntPtr obj, float[] buffer, int bufferElements, double timeout, ref int ec);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern double lsl_pull_sample_d(IntPtr obj, double[] buffer, int bufferElements, double timeout, ref int ec);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern double lsl_pull_sample_i(IntPtr obj, int[] buffer, int bufferElements, double timeout, ref int ec);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern double lsl_pull_sample_s(IntPtr obj, short[] buffer, int bufferElements, double timeout, ref int ec);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern double lsl_pull_sample_c(IntPtr obj, char[] buffer, int bufferElements, double timeout, ref int ec);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern double lsl_pull_sample_str(IntPtr obj, IntPtr[] buffer, int bufferElements, double timeout, ref int ec);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern double lsl_pull_sample_buf(IntPtr obj, char[][] buffer, uint[] bufferLengths, int bufferElements, double timeout, ref int ec);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern void lsl_destroy_string(IntPtr str);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern uint lsl_pull_chunk_f(IntPtr obj,                     float[,] dataBuffer, double[] timestampBuffer, uint dataBufferElements,
													   uint   timestampBufferElements, double   timeout,    ref int  ec);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern uint lsl_pull_chunk_d(IntPtr obj,                     double[,] dataBuffer, double[] timestampBuffer, uint dataBufferElements,
													   uint   timestampBufferElements, double    timeout,    ref int  ec);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern uint lsl_pull_chunk_i(IntPtr obj,                     int[,] dataBuffer, double[] timestampBuffer, uint dataBufferElements,
													   uint   timestampBufferElements, double timeout,    ref int  ec);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern uint lsl_pull_chunk_s(IntPtr obj,                     short[,] dataBuffer, double[] timestampBuffer, uint dataBufferElements,
													   uint   timestampBufferElements, double   timeout,    ref int  ec);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern uint lsl_pull_chunk_c(IntPtr obj,                     char[,] dataBuffer, double[] timestampBuffer, uint dataBufferElements,
													   uint   timestampBufferElements, double  timeout,    ref int  ec);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern uint lsl_pull_chunk_str(IntPtr obj, IntPtr[,] dataBuffer, double[] timestampBuffer,
														 uint   dataBufferElements,
														 uint   timestampBufferElements, double timeout, ref int ec);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern uint lsl_pull_chunk_buf(IntPtr   obj, char[][,] dataBuffer, uint[,] lengthsBuffer,
														 double[] timestampBuffer,
														 uint     dataBufferElements, uint timestampBufferElements, double timeout, ref int ec);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern uint lsl_samples_available(IntPtr obj);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern uint lsl_was_clock_reset(IntPtr obj);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_first_child(IntPtr e);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_last_child(IntPtr e);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_next_sibling(IntPtr e);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_previous_sibling(IntPtr e);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_parent(IntPtr e);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_child(IntPtr e, string name);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_next_sibling_n(IntPtr e, string name);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_previous_sibling_n(IntPtr e, string name);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_empty(IntPtr e);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_is_text(IntPtr e);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_name(IntPtr e);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_value(IntPtr e);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_child_value(IntPtr e);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_child_value_n(IntPtr e, string name);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_append_child_value(IntPtr e, string name, string value);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_prepend_child_value(IntPtr e, string name, string value);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_set_child_value(IntPtr e, string name, string value);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_set_name(IntPtr e, string rhs);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_set_value(IntPtr e, string rhs);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_append_child(IntPtr e, string name);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_prepend_child(IntPtr e, string name);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_append_copy(IntPtr e, IntPtr e2);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_prepend_copy(IntPtr e, IntPtr e2);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern void lsl_remove_child_n(IntPtr e, string name);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern void lsl_remove_child(IntPtr e, IntPtr e2);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_create_continuous_resolver(double forgetAfter);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_create_continuous_resolver_byprop(string prop, string value, double forgetAfter);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern IntPtr lsl_create_continuous_resolver_bypred(string pred, double forgetAfter);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern int lsl_resolver_results(IntPtr obj, IntPtr[] buffer, uint bufferElements);

			[DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
			public static extern void lsl_destroy_continuous_resolver(IntPtr obj);
		}
	}
}
