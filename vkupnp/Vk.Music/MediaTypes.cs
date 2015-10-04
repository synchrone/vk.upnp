using System;
using Mono.Upnp.Dcp.MediaServer1.ConnectionManager1;

namespace Vk.Music.Upnp.ConsoleServer
{
	public class MediaTypes
	{
		public const string AudioItem = "object.item.audioItem";
		public const string MusicArtist = "object.item.musicArtist";
		public const string VideoItem = "object.item.videoItem";

		public static ProtocolInfo Mp3{
			get{
				return ProtocolInfo.Parse ("http-get:*:audio/mpeg:DLNA.ORG_PN=MP3;DLNA.ORG_OP=01;DLNA.ORG_FLAGS=01100000000000000000000000000000");
			}
		}
	}
}

