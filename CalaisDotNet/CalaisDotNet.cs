using System;
using System.Net;
using System.Xml.Linq;
using Calais.Interfaces;

namespace Calais
{
    /// <summary>
    /// A proxy and adapter to the Calais web service
    /// http://opencalais.com/
    /// </summary>
    public class CalaisDotNet
    {
        #region Private Fields

        private readonly CalaisServiceProxy _webServiceProxy;

        #endregion

        #region Public Fields

        public string ApiKey;
        public CalaisInputFormat InputFormat;
        public string Content;
        public CalaisOutputFormat OutputFormat;
        public bool AllowDistribution;
        public bool AllowSearch;
        public string ExternalId;
        public string Submitter;
        public string BaseUrl;
        public string EnableMetadataType;
        public bool CalculateRelevanceScore;

        /// <summary>
        /// Web proxy to go through when calling the Calais web service
        /// </summary>
        public IWebProxy Proxy
        {
            // assign web proxy to the web service proxy
            set { _webServiceProxy.Proxy = value; }
        }

        #endregion

        #region Constructors

        public CalaisDotNet(string apiKey, string content) : this(apiKey, content, DetectInputFormat(content), string.Empty, true, true, string.Empty, string.Empty, false, true) { }

        public CalaisDotNet(string apiKey, string content, CalaisInputFormat inputFormat) : this(apiKey, content, inputFormat, string.Empty, true, true, string.Empty, string.Empty, false, true) { }

        public CalaisDotNet(string apiKey, string content, CalaisInputFormat inputFormat, string baseUrl) : this(apiKey, content, inputFormat, baseUrl, true, true, string.Empty, string.Empty, false, true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalaisDotNet"/> class.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="content">The content.</param>
        /// <param name="inputFormat">Format of the input content.</param>
        /// <param name="baseUrl">Base URL to be put in rel-tag microformats.</param>
        /// <param name="allowDistribution">if set to <c>true</c> Indicates whether the extracted metadata can be distributed.</param>
        /// <param name="allowSearch">if set to <c>true</c> Indicates whether future searches can be performed on the extracted metadata.</param>
        /// <param name="externalId">User-generated ID for the submission.</param>
        /// <param name="submitter">Identifier for the content submitter.</param>
        /// <param name="enableMetadataType">if set to <c>true</c> Indicates whether the output (RDF only) will include Generic Relation extractions.</param>
        /// <param name="calculateRelevanceScore">if set to <c>true</c> Indicates whether the extracted metadata will include relevance score for each unique entity.</param>
        public CalaisDotNet(string apiKey, string content, CalaisInputFormat inputFormat, string baseUrl, bool allowDistribution, bool allowSearch, string externalId, string submitter, bool enableMetadataType, bool calculateRelevanceScore)
        {
            //Contract
            this.Require(x => apiKey.Length == 24, new ArgumentException("API Key must be 24 characters long (yours was" + apiKey.Length + ")"));
            this.Require(x => !string.IsNullOrEmpty(content), new ArgumentException("Content cannot be empty"));

            // initialise inputs required to call web service
            ApiKey = apiKey;
            Content = content;
            InputFormat = inputFormat;
            OutputFormat = CalaisOutputFormat.Simple;
            BaseUrl = baseUrl;
            AllowDistribution = allowDistribution;
            AllowSearch = allowSearch;
            ExternalId = externalId;
            Submitter = submitter;
            CalculateRelevanceScore = calculateRelevanceScore;
            EnableMetadataType = string.Empty;

            if (enableMetadataType)
            {
                EnableMetadataType = "GenericRelations";
            }

            // create a new web service proxy to Calais
            _webServiceProxy = new CalaisServiceProxy();
        }

        #endregion

        public static T Parse<T>(string xmlString) where T : ICalaisDocument
        {
            T document;

            //Check response is not empty
            if (string.IsNullOrEmpty(xmlString))
            {
                throw new ArgumentException("xmlString cannot be empty");
            }

            switch (typeof(T).Name)
            {
                case ("CalaisRdfDocument"):
                    {
                        document = ObjectFactory.Create<T>("Calais.CalaisRdfDocument");
                        break;
                    }

                case ("CalaisMicroFormatsDocument"):
                    {
                        document = ObjectFactory.Create<T>("Calais.CalaisMicroFormatsDocument");
                        break;
                    }

                case ("CalaisSimpleDocument"):
                    {
                        document = ObjectFactory.Create<T>("Calais.CalaisSimpleDocument");
                        break;
                    }

                case ("CalaisJsonDocument"):
                    {
                        document = ObjectFactory.Create<T>("Calais.CalaisJsonDocument");
                        break;
                    }

                default:
                    {
                        throw new ArgumentException("Unknown type");
                    }
            }

            ((ICalaisDocument)document).ProcessResponse(xmlString);

            return document;
        }

        public T Call<T>() where T : ICalaisDocument
        {
            T document;

            // Switch output type to version based on the object being returned
            // Call object factory to create concrete implimentation

            switch (typeof(T).Name)
            {
                case ("CalaisRdfDocument"):
                    {
                        OutputFormat = CalaisOutputFormat.Rdf;
                        document = ObjectFactory.Create<T>("Calais.CalaisRdfDocument");
                        break;
                    }

                case ("CalaisMicroFormatsDocument"):
                    {
                        OutputFormat = CalaisOutputFormat.MicroFormats;
                        document = ObjectFactory.Create<T>("Calais.CalaisMicroFormatsDocument");
                        break;
                    }

                case ("CalaisSimpleDocument"):
                    {
                        OutputFormat = CalaisOutputFormat.Simple;
                        document = ObjectFactory.Create<T>("Calais.CalaisSimpleDocument");
                        break;
                    }

                case ("CalaisJsonDocument"):
                    {
                        OutputFormat = CalaisOutputFormat.JSON;
                        document = ObjectFactory.Create<T>("Calais.CalaisJsonDocument");
                        break;
                    }

                default:
                    {
                        throw new ArgumentException("Unknown type");
                    }
            }

            // Get correctly formatted input XML
            string paramsXml = BuildInputParamsXml();

            //Check XML was built correctly
            this.Ensure(x => !string.IsNullOrEmpty(paramsXml), new ApplicationException("Building parameters XML failed!"));

            // call web service to get response
            string response = _webServiceProxy.Enlighten(ApiKey, Content, paramsXml);

            //Check response is not empty
            this.Ensure(x => !string.IsNullOrEmpty(response), new ApplicationException("Server response is empty!"));

            //Check for error message
            this.Ensure(x => !response.Contains("<Error>"), new ApplicationException("Server reported an error"));

            //TODO: Process <Error> message here !

            ((ICalaisDocument)document).ProcessResponse(response);

            return document;
        }

        #region Helpers

        /// <summary>
        /// Builds XML input content expected by web service
        /// </summary>
        /// <remarks>
        /// Input Paramaters Reference
        /// http://opencalais.mashery.com/page/documentation#inputparameters
        /// </remarks>
        private string BuildInputParamsXml()
        {
            XNamespace c = "http://s.opencalais.com/1/pred/";
            var xdoc = new XDocument(
                new XElement(
                    c + "params",
                    new XAttribute(XNamespace.Xmlns + "c", "http://s.opencalais.com/1/pred/"),
                    new XElement(c + "processingDirectives",
                        new XAttribute(c + "contentType", StringEnum.GetString(InputFormat)),
                        new XAttribute(c + "outputFormat", StringEnum.GetString(OutputFormat)),
                        new XAttribute(c + "reltagBaseURL", BaseUrl),
                        new XAttribute(c + "enableMetadataType", EnableMetadataType),
                        new XAttribute(c + "calculateRelevanceScore", CalculateRelevanceScore)
                    ),
                    new XElement(c + "userDirectives",
                        new XAttribute(c + "allowDistribution", AllowDistribution),
                        new XAttribute(c + "allowSearch", AllowSearch),
                        new XAttribute(c + "externalID", ExternalId),
                        new XAttribute(c + "submitter", Submitter)
                    ),
                    new XElement(c + "externalMetadata")
                )
            );

            return xdoc.ToString();
        }

        /// <summary>
        /// Checks input string to see if it is HTML / XML or plain text, defaults to RawText
        /// </summary>
        /// <param name="content">Content string to be analysed</param>
        /// <returns>Input format enum</returns>
        /// <remarks>This is a little crude and could do with reworking.</remarks>
        private static CalaisInputFormat DetectInputFormat(string content)
        {
            if (content.Contains("<html>"))
            {
                return CalaisInputFormat.Html;
            }

            if (content.StartsWith("<?xml") && !content.Contains("<html>"))
            {
                return CalaisInputFormat.Xml;
            }

            return CalaisInputFormat.RawText;
        }

        #endregion


    }
}

