using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
#pragma warning disable MA0048, MA0016, MA0051, CS1591

namespace uGuardian.WAV {
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public readonly struct WAV {
		private static float BytesToFloat(byte firstByte, byte secondByte)
		{
			var num = (short)(secondByte << 8 | firstByte);
			return num / 32768f;
		}
		private static Stream FileToStream(string file) {
			return File.OpenRead(file);
		}
		private static Stream BytesToStream(byte[] bytes) {
			return new MemoryStream(bytes);
		}
		private readonly BinaryReader BinReader;
		
		[StructLayout(LayoutKind.Sequential, Pack=1)]
		public readonly struct RIFF {
			public RIFF(BinaryReader BinReader) {
				ChunkID = BinReader.ReadUInt32();
				const uint expectedChunkID = 1179011410u;
				if (ChunkID != expectedChunkID) {
					Debug.LogWarning("Starting Header isn't RIFF, audio file is probably corrupt");
					Debug.LogWarning("Should be ("+expectedChunkID+") but it's ("+ChunkID+")");
					try {
						ushort check;
						retry:
						do {
							check = BinReader.ReadUInt16();
						} while (check != 18770u);
						if (BinReader.ReadUInt16() != 17990u) {
							goto retry;
						}
						BinReader.BaseStream.Seek(-8, SeekOrigin.Current);
						ChunkID = BinReader.ReadUInt32();
					} catch (EndOfStreamException) {
						Debug.LogError("Invalid audio file");
					}
				}
				ChunkSize = BinReader.ReadUInt32();
				Format = BinReader.ReadUInt32();
			}
			public readonly uint ChunkID; // Offset: 0
			public readonly uint ChunkSize; // Offset: 4
			public readonly uint Format; // Offset: 8
		}
		[StructLayout(LayoutKind.Sequential, Pack=1)]
		public readonly struct Fmt {
			public Fmt(BinaryReader BinReader) {
				ushort check;
				
				retry:
				do {
					check = BinReader.ReadUInt16();
				} while (check != 28006u);
				if (BinReader.ReadUInt16() != 8308) {
					goto retry;
				}
				
				BinReader.BaseStream.Seek(-4, SeekOrigin.Current);
				Subchunk1ID = BinReader.ReadUInt32();
				Subchunk1Size = BinReader.ReadUInt32();
				AudioFormat = BinReader.ReadUInt16();
				NumChannels = BinReader.ReadUInt16();
				SampleRate = BinReader.ReadUInt32();
				ByteRate = BinReader.ReadUInt32();
				BlockAlign = BinReader.ReadUInt16();
				BitsPerSample = BinReader.ReadUInt16();
				if (AudioFormat != 0x1u) {
					ExtraParamSize = BinReader.ReadUInt16(); 
					ExtraParams = BinReader.ReadBytes((int)ExtraParamSize);
				} else {
					ExtraParamSize = null;
					ExtraParams = new byte[1];
				}
			}
			public readonly uint Subchunk1ID; // Offset: 12
			public readonly uint Subchunk1Size; // Offset: 16
			public readonly ushort AudioFormat; // Offset: 20
			public enum Tag : ushort {
				Microsoft_PCM = 0x1 ,
				Microsoft_ADPCM = 0x2 ,
				Microsoft_IEEE_float = 0x3 ,
				Compaq_VSELP = 0x4 ,
				IBM_CVSD = 0x5 ,
				Microsoft_a_Law = 0x6 ,
				Microsoft_u_Law = 0x7 ,
				Microsoft_DTS = 0x8 ,
				DRM = 0x9 ,
				WMA_9_Speech = 0xa ,
				Microsoft_Windows_Media_RT_Voice = 0xb   ,
				OKI_ADPCM = 0x10,
				Intel_IMA_DVI_ADPCM = 0x11,
				Videologic_Mediaspace_ADPCM = 0x12,
				Sierra_ADPCM = 0x13,
				Antex_G_723_ADPCM = 0x14,
				DSP_Solutions_DIGISTD = 0x15,
				DSP_Solutions_DIGIFIX = 0x16,
				Dialoic_OKI_ADPCM = 0x17,
				Media_Vision_ADPCM = 0x18,
				HP_CU = 0x19,
				HP_Dynamic_Voice = 0x1a,
				Yamaha_ADPCM = 0x20,
				SONARC_Speech_Compression = 0x21,
				DSP_Group_True_Speech = 0x22,
				Echo_Speech_Corp_ = 0x23,
				Virtual_Music_Audiofile_AF36 = 0x24,
				Audio_Processing_Tech_ = 0x25,
				Virtual_Music_Audiofile_AF10 = 0x26,
				Aculab_Prosody_1612 = 0x27,
				Merging_Tech_LRC = 0x28,
				Dolby_AC2 = 0x30,
				Microsoft_GSM610 = 0x31,
				MSN_Audio = 0x32,
				Antex_ADPCME = 0x33,
				Control_Resources_VQLPC = 0x34,
				DSP_Solutions_DIGIREAL = 0x35,
				DSP_Solutions_DIGIADPCM = 0x36,
				Control_Resources_CR10 = 0x37,
				Natural_MicroSystems_VBX_ADPCM = 0x38,
				Crystal_Semiconductor_IMA_ADPCM = 0x39,
				Echo_Speech_ECHOSC3 = 0x3a,
				Rockwell_ADPCM = 0x3b,
				Rockwell_DIGITALK = 0x3c,
				Xebec_Multimedia = 0x3d,
				Antex_G_721_ADPCM = 0x40,
				Antex_G_728_CELP = 0x41,
				Microsoft_MSG723 = 0x42,
				IBM_AVC_ADPCM = 0x43,
				ITU_T_G_726 = 0x45,
				Microsoft_MPEG = 0x50,
				RT23_or_PAC = 0x51,
				InSoft_RT24 = 0x52,
				InSoft_PAC = 0x53,
				MP3 = 0x55,
				Cirrus = 0x59,
				Cirrus_Logic = 0x60,
				ESS_Tech_PCM = 0x61,
				Voxware_Inc = 0x62,
				Canopus_ATRAC = 0x63,
				APICOM_G_726_ADPCM = 0x64,
				APICOM_G_722_ADPCM = 0x65,
				Microsoft_DSAT = 0x66,
				Microsoft_DSAT_DISPLAY = 0x67,
				Voxware_Byte_Aligned = 0x69,
				Voxware_AC8 = 0x70,
				Voxware_AC10 = 0x71,
				Voxware_AC16 = 0x72,
				Voxware_AC20 = 0x73,
				Voxware_MetaVoice = 0x74,
				Voxware_MetaSound = 0x75,
				Voxware_RT29HW = 0x76,
				Voxware_VR12 = 0x77,
				Voxware_VR18 = 0x78,
				Voxware_TQ40 = 0x79,
				Voxware_SC3 = 0x7a,
				Voxware_SC3_1 = 0x7b,
				Soundsoft = 0x80,
				Voxware_TQ60 = 0x81,
				Microsoft_MSRT24 = 0x82,
				ATandT_G_729A = 0x83,
				Motion_Pixels_MVI_MV12 = 0x84,
				DataFusion_G_726 = 0x85,
				DataFusion_GSM610 = 0x86,
				Iterated_Systems_Audio = 0x88,
				Onlive = 0x89,
				Multitude_Inc_FT_SX20 = 0x8a,
				Infocom_ITS_A_S_G_721_ADPCM = 0x8b,
				Convedia_G729 = 0x8c,
				Not_specified_congruency_Inc = 0x8d,
				Siemens_SBC24 = 0x91,
				Sonic_Foundry_Dolby_AC3_APDIF = 0x92,
				MediaSonic_G_723 = 0x93,
				Aculab_Prosody_8kbps = 0x94,
				ZyXEL_ADPCM = 0x97,
				Philips_LPCBB = 0x98,
				Studer_Professional_Audio_Packed = 0x99,
				Malden_PhonyTalk = 0xa0,
				Racal_Recorder_GSM = 0xa1,
				Racal_Recorder_G720_a = 0xa2,
				Racal_G723_1 = 0xa3,
				Racal_Tetra_ACELP = 0xa4,
				NEC_AAC_NEC_Corporation = 0xb0,
				AAC = 0xff,
				Rhetorex_ADPCM = 0x100,
				IBM_u_Law = 0x101,
				IBM_a_Law = 0x102,
				IBM_ADPCM = 0x103,
				Vivo_G_723 = 0x111,
				Vivo_Siren = 0x112,
				Philips_Speech_Processing_CELP = 0x120,
				Philips_Speech_Processing_GRUNDIG = 0x121,
				Digital_G_723 = 0x123,
				Sanyo_LD_ADPCM = 0x125,
				Sipro_Lab_ACEPLNET = 0x130,
				Sipro_Lab_ACELP4800 = 0x131,
				Sipro_Lab_ACELP8V3 = 0x132,
				Sipro_Lab_G_729 = 0x133,
				Sipro_Lab_G_729A = 0x134,
				Sipro_Lab_Kelvin = 0x135,
				VoiceAge_AMR = 0x136,
				Dictaphone_G_726_ADPCM = 0x140,
				Qualcomm_PureVoice = 0x150,
				Qualcomm_HalfRate = 0x151,
				Ring_Zero_Systems_TUBGSM = 0x155,
				Microsoft_Audio1 = 0x160,
				Windows_Media_Audio_V2_V7_V8_V9_DivX_audio_WMA_Alex_AC3_Audio = 0x161,
				Windows_Media_Audio_Professional_V9 = 0x162,
				Windows_Media_Audio_Lossless_V9 = 0x163,
				WMA_Pro_over_S_PDIF = 0x164,
				UNISYS_NAP_ADPCM = 0x170,
				UNISYS_NAP_ULAW = 0x171,
				UNISYS_NAP_ALAW = 0x172,
				UNISYS_NAP_16K = 0x173,
				MM_SYCOM_ACM_SYC008_SyCom_Technologies = 0x174,
				MM_SYCOM_ACM_SYC701_G726L_SyCom_Technologies = 0x175,
				MM_SYCOM_ACM_SYC701_CELP54_SyCom_Technologies = 0x176,
				MM_SYCOM_ACM_SYC701_CELP68_SyCom_Technologies = 0x177,
				Knowledge_Adventure_ADPCM = 0x178,
				Fraunhofer_IIS_MPEG2AAC = 0x180,
				Digital_Theater_Systems_DTS_DS = 0x190,
				Creative_Labs_ADPCM = 0x200,
				Creative_Labs_FASTSPEECH8 = 0x202,
				Creative_Labs_FASTSPEECH10 = 0x203,
				UHER_ADPCM = 0x210,
				Ulead_DV_ACM = 0x215,
				Ulead_DV_ACM_1 = 0x216,
				Quarterdeck_Corp_ = 0x220,
				I_Link_VC = 0x230,
				Aureal_Semiconductor_Raw_Sport = 0x240,
				ESST_AC3 = 0x241,
				Interactive_Products_HSX = 0x250,
				Interactive_Products_RPELP = 0x251,
				Consistent_CS2 = 0x260,
				Sony_SCX = 0x270,
				Sony_SCY = 0x271,
				Sony_ATRAC3 = 0x272,
				Sony_SPC = 0x273,
				TELUM_Telum_Inc = 0x280,
				TELUMIA_Telum_Inc = 0x281,
				Norcom_Voice_Systems_ADPCM = 0x285,
				Fujitsu_FM_TOWNS_SND = 0x300,
				Fujitsu_1 = 0x301,
				Fujitsu_2 = 0x302,
				Fujitsu_3 = 0x303,
				Fujitsu_4 = 0x304,
				Fujitsu_5 = 0x305,
				Fujitsu_6 = 0x306,
				Fujitsu_7 = 0x307,
				Fujitsu_8 = 0x308,
				Micronas_Semiconductors_Inc_Development = 0x350,
				Micronas_Semiconductors_Inc_CELP833 = 0x351,
				Brooktree_Digital = 0x400,
				Intel_Music_Coder_IMC = 0x401,
				Ligos_Indeo_Audio = 0x402,
				QDesign_Music = 0x450,
				On2_VP7_On2_Technologies = 0x500,
				On2_VP6_On2_Technologies = 0x501,
				ATandT_VME_VMPCM = 0x680,
				ATandT_TCP = 0x681,
				YMPEG_Alpha_dummy_for_MPEG_2_compressor = 0x700,
				ClearJump_LiteWave_lossless = 0x8ae,
				Olivetti_GSM = 0x1000,
				Olivetti_ADPCM = 0x1001,
				Olivetti_CELP = 0x1002,
				Olivetti_SBC = 0x1003,
				Olivetti_OPR = 0x1004,
				Lernout_and_Hauspie = 0x1100,
				Lernout_and_Hauspie_CELP_codec = 0x1101,
				Lernout_and_Hauspie_SBC_codec = 0x1102,
				Lernout_and_Hauspie_SBC_codec_1 = 0x1103,
				Lernout_and_Hauspie_SBC_codec_2 = 0x1104,
				Norris_Comm_Inc = 0x1400,
				ISIAudio = 0x1401,
				ATandT_Soundspace_Music_Compression = 0x1500,
				VoxWare_RT24_speech_codec = 0x181c,
				Lucent_elemedia_AX24000P_Music_codec = 0x181e,
				Sonic_Foundry_LOSSLESS = 0x1971,
				Innings_Telecom_Inc_ADPCM = 0x1979,
				Lucent_SX8300P_speech_codec = 0x1c07,
				Lucent_SX5363S_G_723_compliant_codec = 0x1c0c,
				CUseeMe_DigiTalk_ex_Rocwell = 0x1f03,
				NCT_Soft_ALF2CD_ACM = 0x1fc4,
				FAST_Multimedia_DVM = 0x2000,
				Dolby_DTS_Digital_Theater_System = 0x2001,
				RealAudio_1_2_14_4 = 0x2002,
				RealAudio_1_2_28_8 = 0x2003,
				RealAudio_G2_8_Cook_low_bitrate = 0x2004,
				RealAudio_3_4_5_Music_DNET = 0x2005,
				RealAudio_10_AAC_RAAC = 0x2006,
				RealAudio_10_AAC_RACP = 0x2007,
				Reserved_range_to_0x2600_Microsoft = 0x2500,
				makeAVIS_ffvfw_fake_AVI_sound_from_AviSynth_scripts = 0x3313,
				Divio_MPEG_4_AAC_audio = 0x4143,
				Nokia_adaptive_multirate = 0x4201,
				Divio_G726_Divio_Inc = 0x4243,
				LEAD_Speech = 0x434c,
				LEAD_Vorbis = 0x564c,
				WavPack_Audio = 0x5756,
				Ogg_Vorbis_mode_1 = 0x674f,
				Ogg_Vorbis_mode_2 = 0x6750,
				Ogg_Vorbis_mode_3 = 0x6751,
				Ogg_Vorbis_mode_1_1 = 0x676f,
				Ogg_Vorbis_mode_2_1 = 0x6770,
				Ogg_Vorbis_mode_3_1 = 0x6771,
				COM_NBX_3Com_Corporation = 0x7000,
				FAAD_AAC = 0x706d,
				GSM_AMR_CBR_no_SID = 0x7a21,
				GSM_AMR_VBR_including_SID = 0x7a22,
				Comverse_Infosys_Ltd_G723_1 = 0xa100,
				Comverse_Infosys_Ltd_AVQSBC = 0xa101,
				Comverse_Infosys_Ltd_OLDSBC = 0xa102,
				Symbol_Technologies_G729A = 0xa103,
				VoiceAge_AMR_WB_VoiceAge_Corporation = 0xa104,
				Ingenient_Technologies_Inc_G726 = 0xa105,
				ISO_MPEG_4_advanced_audio_Coding = 0xa106,
				Encore_Software_Ltd_G726 = 0xa107,
				Speex_ACM_Codec_xiph_org = 0xa109,
				DebugMode_SonicFoundry_Vegas_FrameServer_ACM_Codec = 0xdfac,
				Unknown = 0xe708,
				Free_Lossless_Audio_Codec_FLAC = 0xf1ac,
				Extensible = 0xfffe,
				Development = 0xffff,
			}
			public readonly ushort NumChannels; // Offset: 22
			public readonly uint SampleRate; // Offset: 24
			public readonly uint ByteRate; // Offset: 28
			public readonly ushort BlockAlign; // Offset: 32
			public readonly ushort BitsPerSample; // Offset: 34
			public readonly ushort? ExtraParamSize; // Offset: 36?
			public readonly byte[] ExtraParams; // Offset: ?
		}
		[StructLayout(LayoutKind.Sequential, Pack=1)]
		public readonly struct Data {
			public Data(BinaryReader BinReader) {
				ushort check;
				retry:
				do {
					check = BinReader.ReadUInt16();
				} while (check != 24932u);
				if (BinReader.ReadUInt16() != 24948u) {
					goto retry;
				}
				BinReader.BaseStream.Seek(-4, SeekOrigin.Current);
				Subchunk2ID = BinReader.ReadUInt32();
				Subchunk2Size = BinReader.ReadUInt32();
				data = BinReader.ReadBytes((int)Subchunk2Size);
			}
			public readonly uint Subchunk2ID; // Offset: 36
			public readonly uint Subchunk2Size; // Offset: 40
			public readonly byte[] data; // Offset: 44
		}
		public WAV(string filename) : this(FileToStream(filename)) {}
		public WAV(byte[] wav) : this(BytesToStream(wav)) {}
		public WAV(Stream wav) {
			BinReader = new BinaryReader(wav);
			riffBlock = new RIFF(BinReader);
			fmtBlock = new Fmt(BinReader);
			dataBlock = new Data(BinReader);
			
			var bytesPerSample = fmtBlock.BitsPerSample/8u;

			LeftChannel = new float[dataBlock.Subchunk2Size/bytesPerSample/fmtBlock.NumChannels];
			RightChannel = new float[dataBlock.Subchunk2Size/bytesPerSample/fmtBlock.NumChannels];
			InterleavedAudio = new float[dataBlock.Subchunk2Size/bytesPerSample];
			var channel = 0;
			int[] iteration = new int[3];
			var total = dataBlock.Subchunk2Size;
			if (bytesPerSample != 2) {
				throw new NotImplementedException("Wav reader currently only supports 16 bit PCM");
			}
			for (var i = 0; i < total; i += (int)bytesPerSample) {
				float audioData = BytesToFloat(dataBlock.data[i], dataBlock.data[i+1]);
				if (channel == 0) {
					LeftChannel[iteration[0]] = audioData;
					iteration[0]++;
				} else if (channel == 1) {
					LeftChannel[iteration[1]] = audioData;
					iteration[1]++;
				}
				InterleavedAudio[iteration[2]] = audioData;
				iteration[2]++;
				channel++;
				if (channel >= fmtBlock.NumChannels) {
					channel = 0;
				}
			}
			BinReader.Dispose();
		}
		public readonly RIFF riffBlock;
		public readonly Fmt fmtBlock;
		public readonly Data dataBlock;

		public readonly float[] LeftChannel;
		public readonly float[] RightChannel;
		public readonly float[] InterleavedAudio;

		public ushort NumChannels {get {return this.fmtBlock.NumChannels;}}
		public uint FileSize {get {return this.riffBlock.ChunkSize + 8u;}}
		public uint FileSize_Audio {get {return this.dataBlock.Subchunk2Size;}}
		public uint SampleRate {get {return this.fmtBlock.SampleRate;}}
		public uint BytesPerSample {get {return this.fmtBlock.BitsPerSample/8u;}}
		public uint SampleCount {get {return FileSize_Audio/BytesPerSample/NumChannels;}}
		public Fmt.Tag AudioFormat {get {return (Fmt.Tag)this.fmtBlock.AudioFormat;}}

		public override string ToString()
		{
			#pragma warning disable MA0011
			return string.Format("[WAV: RIFFtag={4}, NumChannels={0}, FileSize={1}, SampleRate={2}, SampleCount={3}]", new object[]
			{
				this.NumChannels,
				this.FileSize,
				this.SampleRate,
				this.SampleCount,
				this.AudioFormat,
			});
			#pragma warning restore MA0011
		}
	}
}