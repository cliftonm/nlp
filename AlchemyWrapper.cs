using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using AlchemyAPI;

namespace NlpComparison
{
	public class AlchemyWrapper
	{
		protected AlchemyAPI.AlchemyAPI alchemyObj;
		protected AlchemyAPI_EntityParams eparams;
		protected AlchemyAPI_KeywordParams kparams;
		protected AlchemyAPI_ConceptParams cparams;

		public void Initialize()
		{
			alchemyObj = new AlchemyAPI.AlchemyAPI();
			alchemyObj.LoadAPIKey("alchemyapikey.txt");

			eparams = new AlchemyAPI_EntityParams();
			eparams.setMaxRetrieve(250);

			kparams = new AlchemyAPI_KeywordParams();
			kparams.setMaxRetrieve(250);

			cparams = new AlchemyAPI_ConceptParams();
			cparams.setMaxRetrieve(250);
		}

		public string GetUrlText(string url)
		{
			return alchemyObj.URLGetText(url);
		}

		public DataSet LoadEntities(string text)
		{
			DataSet dsEntities = new DataSet();
			string xml = alchemyObj.TextGetRankedNamedEntities(text, eparams);
			TextReader tr = new StringReader(xml);
			XmlReader xr = XmlReader.Create(tr);
			dsEntities.ReadXml(xr);
			xr.Close();
			tr.Close();

			return dsEntities;
		}

		public DataSet LoadKeywords(string text)
		{
			DataSet dsKeywords = new DataSet();
			string xml = alchemyObj.TextGetRankedKeywords(text, kparams);
			TextReader tr = new StringReader(xml);
			XmlReader xr = XmlReader.Create(tr);
			dsKeywords.ReadXml(xr);
			xr.Close();
			tr.Close();

			return dsKeywords;
		}

		public DataSet LoadConcepts(string text)
		{
			DataSet dsConcepts = new DataSet();
			string xml = alchemyObj.TextGetRankedConcepts(text, cparams);
			TextReader tr = new StringReader(xml);
			XmlReader xr = XmlReader.Create(tr);
			dsConcepts.ReadXml(xr);
			xr.Close();
			tr.Close();

			return dsConcepts;
		}

		public DataSet LoadEntitiesFromUrl(string url)
		{
			DataSet dsEntities = new DataSet();
			string xml = alchemyObj.URLGetRankedNamedEntities(url, eparams);
			TextReader tr = new StringReader(xml);
			XmlReader xr = XmlReader.Create(tr);
			dsEntities.ReadXml(xr);
			xr.Close();
			tr.Close();

			return dsEntities;
		}

		public DataSet LoadKeywordsFromUrl(string url)
		{
			DataSet dsKeywords = new DataSet();
			string xml = alchemyObj.URLGetRankedKeywords(url, kparams);
			TextReader tr = new StringReader(xml);
			XmlReader xr = XmlReader.Create(tr);
			dsKeywords.ReadXml(xr);
			xr.Close();
			tr.Close();

			return dsKeywords;
		}

		public DataSet LoadConceptsFromUrl(string url)
		{
			DataSet dsConcepts = new DataSet();
			string xml = alchemyObj.URLGetRankedConcepts(url, cparams);
			TextReader tr = new StringReader(xml);
			XmlReader xr = XmlReader.Create(tr);
			dsConcepts.ReadXml(xr);
			xr.Close();
			tr.Close();

			return dsConcepts;
		}
	}
}
