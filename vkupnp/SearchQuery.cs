using System;
using System.Text.RegularExpressions;
using Vk.Music.Upnp.ConsoleServer;
using System.Diagnostics;

namespace Vk.Upnp
{
	public class SearchQuery
	{
		public bool SearchArtist;
		public string Term;
		public string ObjectType;

		public static SearchQuery Parse(string query, string filter){
			Trace.TraceInformation (query);


			var typeMatch = Regex.Match(query, "upnp:class derivedfrom \"([^\"]+)\"");
			string type = String.Empty;

			if(typeMatch.Groups.Count > 1){
				type = typeMatch.Groups [1].Value;
			}

			var term = Regex.Match(query, "title|artist.+\"([^\"]+)\"");

			if (term.Groups.Count < 2){
				throw new Mono.Upnp.UpnpException("Cannot parse "+query);
			}

			return new SearchQuery{
				Term = term.Groups[1].Value,
				ObjectType = type,
				SearchArtist = type == MediaTypes.MusicArtist,
			};
		}
	}
}

