using System;
using Mono.Upnp.Dcp.MediaServer1.ContentDirectory1;
using Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.AV;
using CDObject = Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.Object;
using System.Collections.Generic;
using System.Diagnostics;
using VkNet;
using Mono.Upnp.Control;
using Mono.Upnp.Xml;
using System.Text;
using System.Xml;
using Mono.Upnp.Dcp.MediaServer1;
using VkNet.Enums;
using Vk.Music.Upnp;
using Vk.Music.Upnp.ConsoleServer;

namespace Vk.Upnp.ConsoleServer
{
	public class VkMusicDirectory : ObjectBasedContentDirectory
	{
		const string rootObjectId = "0";
		const int defaultCount = 100;
		const int maximumSearchCount = 100;

		XmlSerializer serializer = new XmlSerializer ();
		protected VkApi vk;
		long currentUserId;

		protected Dictionary<string, TrackItem> objects = new Dictionary<string, TrackItem>();

		public VkMusicDirectory (Uri baseUrl, VkApi vk, long currentUserId)
		{
			this.vk = vk;
			this.currentUserId = currentUserId;

		}

		protected override string SearchCapabilities {
			get { return "upnp:class,dc:title,upnp:artist"; }
		}

		protected override string SortCapabilities {
			get { return string.Empty; }
		}


		[UpnpAction (OmitUnless = "CanSearch")]
		public override void Search ([UpnpArgument ("ContainerID")] string containerId,
			[UpnpArgument ("SearchCriteria")] string searchCriteria,
			[UpnpArgument ("Filter")] string filter,
			[UpnpArgument ("StartingIndex")] int startingIndex,
			[UpnpArgument ("RequestedCount")] int requestCount,
			[UpnpArgument ("SortCriteria")] string sortCriteria,
			[UpnpArgument ("Result")] out string result,
			[UpnpArgument ("NumberReturned")] out int numberReturned,
			[UpnpArgument ("TotalMatches")] out int totalMatches,
			[UpnpArgument ("UpdateID")] out string updateId)
		{
			updateId = "0";
			var serializer = new ResultsSerializer (this.serializer);
			Search (res => serializer.Serialize (res), containerId, SearchQuery.Parse(searchCriteria, filter), startingIndex,
				requestCount, sortCriteria, out numberReturned, out totalMatches);
			result = serializer.ToString ();
		}

		protected void Search (Action<CDObject> resultConsumer, string containerId, SearchQuery query, int startingIndex,
			int requestCount, string sortCriteria, out int numberReturned, out int totalMatches)
		{
			if (query.ObjectType != MediaTypes.AudioItem) { 
				totalMatches = numberReturned = 0;
				return;
			}

			var audios = vk.Audio.Search (
				query.Term, out totalMatches, true, default(AudioSort), false, 
				requestCount == 0 ? defaultCount : requestCount, 
				startingIndex
			);

			numberReturned = audios.Count;
			foreach (var audio in audios) {
				if (audio.Url != null) {
					resultConsumer ((TrackItem)audio);
				}
			}

			totalMatches = Math.Min(totalMatches, maximumSearchCount);
		}
			
		protected override CDObject GetObject (string objectId)
		{
			int uid;
			if (!Int32.TryParse (objectId, out uid)) {
				objectId = "";
			}
			return new CDObject (objectId, "", new ObjectOptions ());
		}

		protected override int VisitChildren (Action<CDObject> consumer,
			string objectId,
			int startIndex,
			int requestCount,
			string sortCriteria,
			out int totalMatches)
		{
			try{
				if (objectId == rootObjectId || objectId == String.Empty) {
					return VisitRootChildren (consumer, startIndex, requestCount, out totalMatches);
				}
				return VisitDirectoryChildren (consumer, objectId, startIndex, requestCount, out totalMatches);
			}catch(Exception e){
				Console.WriteLine (e.Message + Environment.NewLine + e.StackTrace.ToString ());
				throw e;
			}
		}

		int VisitRootChildren (
			Action<CDObject> consumer,
			int startIndex,
			int requestCount,
			out int totalMatches)
		{
			var userId = vk.UserId.GetValueOrDefault (this.currentUserId);

			consumer (new Container(userId.ToString(), rootObjectId, new ContainerOptions{Title = "My Music", IsRestricted = true}));

			var friendIds = vk.Friends.GetRecent(10);
			var friends = vk.Users.Get (friendIds, VkNet.Enums.Filters.ProfileFields.BirthDate);
			foreach (var friend in friends) {
				consumer (new Container(friend.Id.ToString(), rootObjectId, new ContainerOptions{
					Title = string.Format("{0} {1}", friend.FirstName, friend.LastName), 
					IsRestricted = true
				}));
			}

			totalMatches = friends.Count + 1;
			return totalMatches;
		}

		int VisitDirectoryChildren (
			Action<CDObject> consumer,
			string objectId,
			int startIndex,
			int requestCount,
			out int totalMatches)
		{
			var audios = vk.Audio.Get(
				Int32.Parse(objectId), null, null, 
				requestCount == 0 ? defaultCount : requestCount, 
				startIndex);

			foreach (var audio in audios) {
				consumer ((TrackItem)audio);
			}

			totalMatches = audios.Count;
			return totalMatches;
		}

		class ResultsSerializer
		{
			XmlSerializer serializer;
			StringBuilder builder = new StringBuilder ();
			XmlWriter writer;

			public ResultsSerializer (XmlSerializer serializer)
			{
				this.serializer = serializer;
				writer = XmlWriter.Create (builder);
				writer.WriteStartElement ("DIDL-Lite", Schemas.DidlLiteSchema);
				writer.WriteAttributeString ("xmlns", "dc", null, Schemas.DublinCoreSchema);
				writer.WriteAttributeString ("xmlns", "upnp", null, Schemas.UpnpSchema);
			}

			public void Serialize<T> (T item)
			{
				serializer.Serialize (item, writer);
			}

			public override string ToString ()
			{
				writer.WriteEndElement ();
				return builder.ToString ();
			}
		}
	}
}

