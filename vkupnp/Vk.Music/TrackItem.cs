using System;
using CDObject = Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.Object;
using CDClass  = Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.Class;
using Mono.Upnp.Dcp.MediaServer1.ContentDirectory1;
using System.Collections.Generic;
using Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.AV;
using Mono.Upnp.Dcp.MediaServer1.ConnectionManager1;
using Vk.Music.Upnp.ConsoleServer;
using System.Diagnostics;

namespace Vk.Music.Upnp
{
	public class TrackItem : MusicTrack
	{
		public TrackItem(string id, string parentId, MusicTrackOptions options) : base(id, parentId, options){}

		public static implicit operator TrackItem(VkNet.Model.Attachments.Audio audio)
		{
			var resources = new List<Resource> ();
			resources.Add (
				new Resource (audio.Url, new ResourceOptions {
					Duration = TimeSpan.FromSeconds (audio.Duration),
					NrAudioChannels = 2,
					BitRate = 320000,
					ProtocolInfo = MediaTypes.Mp3,
					SampleFrequency = 48000
				})
			);

			var options = new MusicTrackOptions{
				Title = audio.Title,
				Creator = audio.Artist,
				Resources = resources,
				IsRestricted = true
			};

			return new TrackItem (audio.Id.ToString(), audio.OwnerId.ToString(), options);
		}
	}
}

