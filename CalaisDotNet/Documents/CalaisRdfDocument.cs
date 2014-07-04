using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Calais.Entity;
using Calais.Interfaces;

namespace Calais
{
    public class CalaisRdfDocument : ICalaisDocument
    {
        #region Private Variables

        //Key namespaces needed for processing.
        readonly XNamespace rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        readonly XNamespace c = "http://s.opencalais.com/1/pred/";

        //URIs for various types.
        private const string _relationshipUri = "http://s.opencalais.com/1/type/em/r/";
        private const string _entityUri = "http://s.opencalais.com/1/type/em/e/";
        private const string _docInfoUri = "http://s.opencalais.com/1/type/sys/DocInfo";
        private const string _docMetaUri = "http://s.opencalais.com/1/type/sys/DocInfoMeta";
        private const string _instanceUri = "http://s.opencalais.com/1/type/sys/InstanceInfo";

        #endregion

        #region Public Variables

        #endregion

        #region Public Fields

        public IEnumerable<CalaisRdfEntity> Entities { get; set; }
        public CalaisRdfDocumentDescription Description { get; set; }
        public IEnumerable<CalaisRdfRelationship> Relationships { get; set; }

        #endregion

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
            Description = ProcessRdfDescription(doc);
            Entities = ProcessRdfEntities(doc);
            Relationships = ProcessRdfRelationships(doc);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Processes any RDF relationships.
        /// </summary>
        /// <param name="doc">Parsed response from the server.</param>
        /// <returns>A collection of RDF relationships.</returns>
        private IEnumerable<CalaisRdfRelationship> ProcessRdfRelationships(XDocument doc)
        {
            //Contract
            this.Require(x => doc != null, new ArgumentNullException("doc"));

            //Select elements that conatain a relationship URI
            var results = from item in doc.Root.Descendants(rdf + "Description")
                          where item.Element(rdf + "type").Attribute(rdf + "resource").Value.Contains(_relationshipUri)
                          select item;

            foreach (var result in results)
            {
                var relationship = new CalaisRdfRelationship
                                       {
                                           Id = result.Attribute(rdf + "about").Value,
                                           RelationshipType = ((CalaisRdfRelationshipType) Enum.Parse(typeof (CalaisRdfRelationshipType), result.Element(rdf + "type").Attribute(rdf + "resource").Value.Replace(_relationshipUri, string.Empty))),
                                           RelationshipDetails = ProcessRdfRelationshipDetails(result.Elements().Where(item => item.Name.Namespace == c)).ToDictionary(item => item.Key, item => item.Value),
                                           Instances = ProcessRdfInstances(result.Attribute(rdf + "about").Value, doc)
                                       };

                //Check that for each relationship there is at least one corresponding instance (otherwise something is seriously broken!)
                this.Ensure(item => relationship.Instances.Count() > 0, new ApplicationException("No instances of entity found in the document"));

                yield return relationship;
            }
        }

        /// <summary>
        /// Processes the RDF relationship details into key/value pairs so they can be added to an IDictionary
        /// </summary>
        /// <param name="elements">Collection of elements conatining details.</param>
        /// <returns>A collection of keyvalue pairs containing property name (key) and associated value.</returns>
        private IEnumerable<KeyValuePair<string, string>> ProcessRdfRelationshipDetails(IEnumerable<XElement> elements)
        {
            foreach (var element in elements)
            {
                if (element.Attribute(rdf + "resource") != null)
                {
                    yield return new KeyValuePair<string, string>(element.Name.LocalName, ResolveRdfEntity(element.Attribute(rdf + "resource").Value));
                }
                else
                {
                    yield return new KeyValuePair<string, string>(element.Name.LocalName, element.Value);
                }
            }
        }

        /// <summary>
        /// Resolves the RDF entity from the existsing collection of entities.
        /// </summary>
        /// <param name="resourceId">The resource id.</param>
        /// <returns></returns>
        private string ResolveRdfEntity(string resourceId)
        {
            var result = from item in Entities
                         where item.Id == resourceId
                         select item.Value;

            if (result.Count() > 0)
            {
                return result.Single();
            }

            return string.Empty;
        }

        /// <summary>
        /// Takes the DocumentInfo and DocumentMetadata and combines them into one description object
        /// </summary>
        /// <param name="doc">Parsed response from the server.</param>
        /// <returns>RDF Description object</returns>
        private CalaisRdfDocumentDescription ProcessRdfDescription(XDocument doc)
        {
            var docInfo = (from item in doc.Root.Descendants(rdf + "Description")
                           where item.Element(rdf + "type").Attribute(rdf + "resource").Value == _docInfoUri
                           select item).Single();

            var meta = (from item in doc.Root.Descendants(rdf + "Description")
                        where item.Element(rdf + "type").Attribute(rdf + "resource").Value == _docMetaUri
                        select item).Single();

            var description = new CalaisRdfDocumentDescription
                                  {
                                      Id = docInfo.Attribute(c + "id").Value,
                                      About = docInfo.Attribute(rdf + "about").Value,
                                      AllowDistribution = bool.Parse(docInfo.Attribute(c + "allowDistribution").Value),
                                      AllowSearch = bool.Parse(docInfo.Attribute(c + "allowSearch").Value),
                                      ExternalId = docInfo.Attribute(c + "externalID").Value,
                                      Document = docInfo.Element(c + "document").Value,
                                      Submitter = docInfo.Element(c + "submitter").Value,
                                      ContentType = meta.Attribute(c + "contentType").Value,
                                      EmVer = meta.Attribute(c + "emVer").Value,
                                      Language = meta.Attribute(c + "language").Value,
                                      LanguageIdVer = meta.Attribute(c + "langIdVer").Value,
                                      ProcessingVersion = meta.Attribute(c + "processingVer").Value,
                                      SubmitionDate = meta.Attribute(c + "submissionDate").Value,
                                      SubmitterCode = meta.Element(c + "submitterCode").Value,
                                      Signature = meta.Element(c + "signature").Value
                                  };

            return description;
        }

        /// <summary>
        /// Processes the RDF entities in the response docuement.
        /// </summary>
        /// <param name="doc">Parsed response from the server.</param>
        /// <returns>A collection of RDF entities.</returns>
        private IEnumerable<CalaisRdfEntity> ProcessRdfEntities(XDocument doc)
        {
            //Contract
            this.Require(x => doc != null, new ArgumentNullException("doc"));

            //Select elements that conatin an entity URI
            var results = from item in doc.Root.Descendants(rdf + "Description")
                          where item.Element(rdf + "type").Attribute(rdf + "resource").Value.Contains(_entityUri)
                          select item;

            foreach (var result in results)
            {
                // Annoyingly some entities now have subtypes. e.g. Person + PersonType
                // The design now allows for one subtype per entity
                // (if this changes this will need to be re-written to work like relationship details)
                var subElements = result.Elements().Where(item => item.Name.Namespace == c).ToList();

                //Check that each element has (at most) one subtype
                this.Ensure(item => subElements.Count >= 1 || subElements.Count < 3, new ApplicationException("Unknown Calais Entity format .. bailing out! Count=" + subElements.Count));

                var entity = new CalaisRdfEntity
                                 {
                                     Id = result.Attribute(rdf + "about").Value,
                                     EntityType = ((CalaisRdfEntityType) Enum.Parse(typeof (CalaisRdfEntityType), result.Element(rdf + "type").Attribute(rdf + "resource").Value.Replace(_entityUri, string.Empty))),
                                     Value = (subElements[0].Value ?? string.Empty),
                                     Instances = ProcessRdfInstances(result.Attribute(rdf + "about").Value, doc)
                                 };

                if (subElements.Count == 2)
                {
                    entity.EntitySubType = (CalaisRdfEntitySubType)Enum.Parse(typeof(CalaisRdfEntitySubType), subElements[1].Name.LocalName, true);
                    entity.SubValue = subElements[1].Value ?? string.Empty;
                }

                //Check that for each entity there is at least one corresponding instance (otherwise something is seriously broken!)
                this.Ensure(item => entity.Instances.Count() > 0, new ApplicationException("No instances of entity found in the document"));

                yield return entity;
            }
        }

        /// <summary>
        /// Processes all instances of a certain entity or relationship (based on its id)
        /// </summary>
        /// <param name="id">The id of the element.</param>
        /// <param name="doc">Parsed response from the server.</param>
        /// <returns></returns>
        private IEnumerable<CalaisRdfResourceInstance> ProcessRdfInstances(string id, XDocument doc)
        {
            var results = from item in doc.Root.Descendants(rdf + "Description")
                          where item.Element(rdf + "type").Attribute(rdf + "resource").Value == _instanceUri && item.Element(c + "subject").Attribute(rdf + "resource").Value == id
                          select new CalaisRdfResourceInstance
                                     {
                                         Detection = item.Element(c + "detection").Value,
                                         Length = int.Parse(item.Element(c + "length").Value),
                                         Offset = int.Parse(item.Element(c + "offset").Value)
                                     };
            return results;
        }

        #endregion
    }
}