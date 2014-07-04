using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Calais.Entity;
using Calais.Interfaces;

namespace Calais
{
    public class CalaisSimpleDocument : ICalaisDocument
    {
        public IEnumerable<CalaisSimpleEntity> Entities;
        public CalaisSimpleDocumentDescription Description { get; set; }
        public IEnumerable<string> Events;
        public IEnumerable<CalaisSimpleTopic> Topics;

        #region ICalaisDocument Members

        public string RawOutput { get; set; }

        /// <summary>
        /// Processes the response from the server.
        /// </summary>
        /// <param name="response">The response.</param>
        public void ProcessResponse(string response)
        {
            //Contract
            this.Require(item => response != null, new ArgumentNullException("response"));

            RawOutput = response;

            var doc = XDocument.Parse(response);

            this.Ensure(item => doc != null, new Exception("Unable to process response!"));

            //Process each part of the document in order.
            Description = ProcessSimpleDescription(doc);
            Entities = ProcessSimpleEntities(doc);
            Events = ProcessSimpleEvents(doc);
            Topics = ProcessSimpleTopics(doc);
        }

        /// <summary>
        /// Processes the simple topics.
        /// </summary>
        /// <param name="doc">Parsed response from the server.</param>
        /// <returns>A collection of CalaisSimpleTopic objects.</returns>
        private static IEnumerable<CalaisSimpleTopic> ProcessSimpleTopics(XDocument doc)
        {
            var results = from item in doc.Root.Element("CalaisSimpleOutputFormat").Descendants("Topic")
                          select item;

            foreach (var result in results)
            {
                var newTopic = new CalaisSimpleTopic
                                   {
                                       Score = result.Attribute("Score").Value,
                                       Value = result.Value,
                                       Taxonomy = result.Attribute("Taxonomy").Value
                                   };

                yield return newTopic;
            }
        }

        /// <summary>
        /// Processes the simple events.
        /// </summary>
        /// <param name="doc">Parsed response from the server.</param>
        /// <returns>A collection of Events.</returns>
        private static IEnumerable<string> ProcessSimpleEvents(XDocument doc)
        {
            var results = from item in doc.Root.Element("CalaisSimpleOutputFormat").Descendants("Event")
                          select item;

            foreach (var result in results)
            {
                yield return result.Value;
            }
        }

        #endregion

        /// <summary>
        /// Processes the simple entities.
        /// </summary>
        /// <param name="doc">Parsed response from the server.</param>
        /// <returns>A collection of 'Simple' entities.</returns>
        private static IEnumerable<CalaisSimpleEntity> ProcessSimpleEntities(XDocument doc)
        {
            var results = from item in doc.Root.Element("CalaisSimpleOutputFormat").Descendants()
                          select item;

            foreach (var result in results)
            {
                var newSimpleEntity = new CalaisSimpleEntity();

                newSimpleEntity.Value = result.Value;
                newSimpleEntity.Relevance = "0";
                newSimpleEntity.Frequency = 0;

                if (result.Attribute("count") != null)
                {
                    newSimpleEntity.Frequency = int.Parse(result.Attribute("count").Value);
                }

                if (result.Attribute("relevance") != null)
                {
                    newSimpleEntity.Relevance = result.Attribute("relevance").Value;
                }

                var elementName = result.Name.ToString();

                // Ignore topics and events are they are processed seperately
                if (elementName != "Topics" && elementName != "Event" && elementName != "Topic")
                {
                    newSimpleEntity.Type = result.Name.ToString();
                    yield return newSimpleEntity;
                }
            }
        }

        /// <summary>
        /// Processes the document info into a simple description object.
        /// </summary>
        /// <param name="doc">Parsed response from the server.</param>
        /// <returns>A 'Simple' description object.</returns>
        private static CalaisSimpleDocumentDescription ProcessSimpleDescription(XDocument doc)
        {
            return new CalaisSimpleDocumentDescription
            {
                AllowDistribution = bool.Parse(doc.Root.Element("Description").Element("allowDistribution").Value),
                AllowSearch = bool.Parse(doc.Root.Element("Description").Element("allowSearch").Value),
                About = doc.Root.Element("Description").Element("about").Value,
                ExternalId = doc.Root.Element("Description").Element("externalID").Value,
                Id = doc.Root.Element("Description").Element("id").Value,
                CalaisRequestId = doc.Root.Element("Description").Element("calaisRequestID").Value
            };
        }
    }
}
