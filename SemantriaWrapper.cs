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
		// https://semantria.com/support/developer/overview/processing
		protected List<DocAnalyticData> docResults;
		protected Configuration config;
		protected string configID = null;

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
			// Document process rather than collection processing.
			string docId = Guid.NewGuid().ToString();
			Document doc = new Document() {Id = docId, Text = content};
			docResults = new List<DocAnalyticData>();
			int result = session.QueueDocument(doc, configID);
			DocAnalyticData ret;
			DateTime start = DateTime.Now;

			do
			{
				// Semantria guarantees a result within 10 seconds.  But how fast is it really?
				Thread.Sleep(100);
				ret = session.GetDocument(doc.Id, configID);

				if ((DateTime.Now - start).TotalSeconds > 15)
				{
					throw new ApplicationException("Semantria did not return with 15 seconds.");
				}
			} while (ret.Status == Semantria.Com.TaskStatus.QUEUED);

			if (ret.Status == Semantria.Com.TaskStatus.PROCESSED)
			{
				docResults.Add(ret);
			}
			else
			{
				throw new ApplicationException("Error processing document: " + ret.Status.ToString());
			}
		}

		public IList GetEntities()
		{
			List<DocEntity> entities = new List<DocEntity>();
			docResults.ForEach(d => entities.AddRange(d.Entities));

			return entities;
		}

		public IList GetThemes()
		{
			List<DocTheme> themes = new List<DocTheme>();
			docResults.ForEach(d => themes.AddRange(d.Themes));

			return themes;
		}

		public IList GetTopics()
		{
			List<DocTopic> topics = new List<DocTopic>();
			docResults.ForEach(d => topics.AddRange(d.Topics));

			return topics;
		}

		protected void IncreaseLimits()
		{
			// This takes considerable time to get the configurations back from the server.
			List<Configuration> configurations = session.GetConfigurations();
			config = configurations.FirstOrDefault(item => item.Language.Equals("English"));

			if (config != null)
			{
				config.Document.NamedEntitiesLimit = 50;
				config.Document.ConceptTopicsLimit = 50;
				config.Document.EntityThemesLimit = 50;
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
