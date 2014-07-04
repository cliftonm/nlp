using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.ExtensionMethods;

using Calais;

namespace NlpComparison
{
	public class CalaisEvent
	{
		public string EventName { get; set; }
	}

	public class CalaisWrapper
	{
		protected string apikey;
		protected CalaisSimpleDocument document;

		public CalaisWrapper()
		{
		}

		public void Initialize()
		{
			apikey = File.ReadAllText("calaisapikey.txt");
		}

		public void ParseUrl(string content)
		{
			// A couple options: http://stackoverflow.com/questions/123336/how-can-you-strip-non-ascii-characters-from-a-string-in-c
			string asAscii = Encoding.ASCII.GetString(
						Encoding.Convert(
							Encoding.UTF8,
							Encoding.GetEncoding(
								Encoding.ASCII.EncodingName,
								new EncoderReplacementFallback(string.Empty),
								new DecoderExceptionFallback()
								),
							Encoding.UTF8.GetBytes(content)
						)
					); 
			
			CalaisDotNet calais = new CalaisDotNet(apikey, asAscii);
			document = calais.Call<CalaisSimpleDocument>();
		}

		public IList GetEntities()
		{
			return document.Entities.ToList();
		}

		public IList GetTopics()
		{
			return document.Topics.ToList();
		}

		public IList GetEvents()
		{
			List<CalaisEvent> events = new List<CalaisEvent>();
			document.Events.ForEach(e => events.Add(new CalaisEvent() { EventName = e }));
			return events;
		}
	}
}
