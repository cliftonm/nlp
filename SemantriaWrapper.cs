using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Semantria.Com;
using Semantria.Com.Serializers;
using Semantria.Com.Mapping;
using Semantria.Com.Mapping.Configuration;
using Semantria.Com.Mapping.Output;

using Clifton.Assertions;
using Clifton.ExtensionMethods;

namespace NlpComparison
{
	public class SemantriaWrapper
	{
		protected string consumerKey;
		protected string consumerSecret;

		protected JsonSerializer serializer;
		protected Session session;
		protected CollAnalyticData result;

		public SemantriaWrapper()
		{
		}

		public void Initialize()
		{
			string apikey = File.ReadAllText("semantriaapikey.txt");
			string[] keys = apikey.Split('\r');
			consumerKey = keys[0].Trim();
			consumerSecret = keys[1].Trim();
			serializer = new JsonSerializer();
			session = Session.CreateSession(consumerKey, consumerSecret, serializer);
			IncreaseLimits();
		}

		public void ParseUrl(string content)
		{
			Collection collection = SplitContent(content);
			int queueRet = session.QueueCollection(collection);
			Assert.That(queueRet != -1, "Problem queuing data.");			
			
			do
			{
				Thread.Sleep(1000);			// wait some arbitrary time for results to appear.
				// TODO: Is there a notification callback when the processing is done or do we need to implement it ourselves by wrapping this in an awaitable task?
				result = session.GetCollection(collection.Id);
			} while (result.Status == Semantria.Com.TaskStatus.QUEUED);
		}

		public IList GetEntities()
		{
			return result.Entities;
		}

		public IList GetFacets()
		{
			return result.Facets;
		}

		public IList GetThemes()
		{
			return result.Themes;
		}

		public IList GetTopics()
		{
			return result.Topics;
		}

		protected void IncreaseLimits()
		{
			List<Configuration> configurations = session.GetConfigurations();
			Configuration config = configurations.FirstOrDefault(item => item.Language.Equals("English"));

			if (config != null)
			{
				config.Document.NamedEntitiesLimit = 20;
				config.Document.ConceptTopicsLimit = 20;
				config.Document.EntityThemesLimit = 20;
				session.UpdateConfigurations(new List<Configuration>() { config });
			}
		}

		/// <summary>
		/// The content needs to be split into small chunks, otherwise an exception is thrown (line too long.)
		/// </summary>
		protected Collection SplitContent(string content)
		{
			Collection collection = new Collection() { Id = Guid.NewGuid().ToString(), Documents = new List<string>() };

			content.Split('\n').ForEach(s =>
			{
				string trimmed = s.Trim();

				// Ignore empty lines.
				if (!String.IsNullOrEmpty(trimmed))
				{
					collection.Documents.Add(trimmed);
				}
			});

			return collection;
		}
	}
}
