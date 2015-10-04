using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

using NDesk.Options;

using Mono.Upnp.Control;
using Mono.Upnp.Dcp.MediaServer1;
using Mono.Upnp.Dcp.MediaServer1.ContentDirectory1;

using VkNet;
using VkNet.Enums.Filters;
using Vk.Upnp.ConsoleServer;
using Mono.Upnp;
using System.Diagnostics;

namespace Vk.Music.Upnp.ConsoleServer
{
	class Program
	{
		protected static System.Reflection.Assembly Binary{
			get{ 
				return System.Reflection.Assembly.GetExecutingAssembly ();
			}
		}

		public static void Main (string[] args)
		{
			var udn = "uuid:" + Guid.NewGuid ().ToString ();
			var friendly_name = "vk.com";
			var manufacturer = "synchrone";
			var model_name = "vk.upnp";
			var manufacturer_url = new Uri ("https://github.com/synchrone/vk.upnp");

			var model_description = "UPnP Access to vk.com media";
			var model_number = Binary.GetName().Version.ToString();
			var model_url = new Uri ("http://vk.com");

			string uid = String.Empty;
			var ignoreTlsErrors = false;
			var help = false;

			//token is:
			// https://oauth.vk.com/authorize?client_id=<standalone application client id>&scope=audio,offline,friends&redirect_uri=https://oauth.vk.com/blank.html&response_type=token
			var vkToken = String.Empty;

			var options = new OptionSet {
				{ "t|token=", "vk api token" , t => vkToken = t },
				{ "u|userid=", "vk user id", u => uid = u},
				{ "k|ignoretls", "ignore TLS errors", k => ignoreTlsErrors = true },
				{ "h|?|help", "show this help message and exit.", v => help = v != null }
			};

			try {
				options.Parse (args);
			} catch (Exception e) {
				var binaryName = Binary.GetName ().Name;
				Console.WriteLine (binaryName + ":");
				Console.WriteLine (e.Message);
				Console.WriteLine ("Try "+binaryName +" --help for more info.");
				return;
			}
			
			if (help) {
				ShowHelp (options);
				return;
			}

			if (vkToken == String.Empty || uid == String.Empty) {
				Console.Error.WriteLine ("Please specify both --userid and --token");
				return;
			}

			if (ignoreTlsErrors) {
				ServicePointManager.ServerCertificateValidationCallback += 
						(sender, cert, chain, sslPolicyErrors) => true;
			}

			var connection_manager = new DummyConnectionManager();

			var content_directory = BuildContentDirectory(BuildVkApi(vkToken), long.Parse(uid));

			var media_server = new MediaServer (
				udn,
				friendly_name,
				manufacturer,
				model_name,
				new DeviceOptions{
					ManufacturerUrl = manufacturer_url,
					ModelNumber = model_number,
					ModelDescription = model_description,
					ModelUrl = model_url
				},
				connection_manager,
				content_directory
			);

			using (media_server) {
				media_server.Start ();
				Trace.Listeners.Add(new ConsoleTraceListener(true));
				Trace.Listeners.Add(new ConsoleTraceListener());
				Console.WriteLine ("Press ENTER to exit.");
				Console.ReadLine ();
			}
		}

		static ObjectBasedContentDirectory BuildContentDirectory (VkApi vk, long userId)
		{
			return new VkMusicDirectory(GenerateUrl(), vk, userId);
		}


		static VkApi BuildVkApi(string vkToken){
			var vk = new VkApi();
			vk.Authorize(vkToken);

			return vk;
		}

		static Uri GenerateUrl ()
		{
			foreach (var address in Dns.GetHostAddresses (Dns.GetHostName ())) {
				if (address.AddressFamily == AddressFamily.InterNetwork) {
					return new Uri (string.Format (
						"http://{0}:{1}/upnp/media-server/", address, new Random ().Next (1024, 5000)));
				}
			}

			return null;
		}

		static void ShowHelp (OptionSet options)
		{
			Console.WriteLine ("Usage: vkupnp [OPTIONS]");
			Console.WriteLine ("Options:");
			options.WriteOptionDescriptions (Console.Out);
		}
	}
}
