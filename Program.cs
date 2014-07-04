using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using AlchemyAPI;

using Clifton.MycroParser;

namespace NlpComparison
{
    public class Program
    {
		protected TextBox tbUrl;
		protected StatusBarPanel sbStatus;

		// AlchemyAPI views and grids:
		protected DataView dvEntities;
		protected DataView dvKeywords;
		protected DataView dvConcepts;

		protected DataGridView dgvEntities;
		protected DataGridView dgvKeywords;
		protected DataGridView dgvConcepts;

		// OpenCalais views and grids:
		protected DataView dvCalaisEntities;
		protected DataView dvCalaisTopics;
		protected DataView dvCalaisEvents;

		protected DataGridView dgvCalaisEntities;
		protected DataGridView dgvCalaisTopics;
		protected DataGridView dgvCalaisEvents;

		// Semantria views and grids:
		protected DataView dvSemantriaEntities;
		protected DataView dvSemantriaFacets;
		protected DataView dvSemantriaThemes;
		protected DataView dvSemantriaTopics;

		protected DataGridView dgvSemantriaEntities;
		protected DataGridView dgvSemantriaFacets;
		protected DataGridView dgvSemantriaThemes;
		protected DataGridView dgvSemantriaTopics;

		// Labels:
		protected Label lblAlchemyEntities;
		protected Label lblAlchemyKeywords;
		protected Label lblAlchemyConcepts;
		protected Label lblCalaisEntities;
		protected Label lblCalaisTopics;
		protected Label lblCalaisEvents;
		protected Label lblSemantriaEntities;
		protected Label lblSemantriaFacets;
		protected Label lblSemantriaThemes;
		protected Label lblSemantriaTopics;

		public Program()
		{
		}

		public void Initialize()
		{
			MycroParser mp = new MycroParser();
			XmlDocument doc = new XmlDocument();
			doc.Load("MainForm.xml");
			mp.Load(doc, "Form", this);
			Form form = (Form)mp.Process();

			// Controls we need to use:
			tbUrl = mp.ObjectCollection["tbUrl"] as TextBox;
			sbStatus = mp.ObjectCollection["sbStatus"] as StatusBarPanel;

			// AlchemyAPI grids:
			dgvEntities = (DataGridView)mp.ObjectCollection["dgvEntities"];
			dgvKeywords = (DataGridView)mp.ObjectCollection["dgvKeywords"];
			dgvConcepts = (DataGridView)mp.ObjectCollection["dgvConcepts"];

			// Calais grids:
			dgvCalaisEntities = (DataGridView)mp.ObjectCollection["dgvCalaisEntities"];
			dgvCalaisTopics = (DataGridView)mp.ObjectCollection["dgvCalaisTopics"];
			dgvCalaisEvents = (DataGridView)mp.ObjectCollection["dgvCalaisEvents"];

			// Semantria grids:
			dgvSemantriaEntities = (DataGridView)mp.ObjectCollection["dgvSemantriaEntities"];
			dgvSemantriaFacets = (DataGridView)mp.ObjectCollection["dgvSemantriaFacets"];
			dgvSemantriaThemes = (DataGridView)mp.ObjectCollection["dgvSemantriaThemes"];
			dgvSemantriaTopics = (DataGridView)mp.ObjectCollection["dgvSemantriaTopics"];

			lblAlchemyEntities = (Label)mp.ObjectCollection["lblAlchemyEntities"]; ;
			lblAlchemyKeywords = (Label)mp.ObjectCollection["lblAlchemyKeywords"];
			lblAlchemyConcepts = (Label)mp.ObjectCollection["lblAlchemyConcepts"];
			lblCalaisEntities = (Label)mp.ObjectCollection["lblCalaisEntities"];
			lblCalaisTopics = (Label)mp.ObjectCollection["lblCalaisTopics"];
			lblCalaisEvents = (Label)mp.ObjectCollection["lblCalaisEvents"];
			lblSemantriaEntities = (Label)mp.ObjectCollection["lblSemantriaEntities"];
			lblSemantriaFacets = (Label)mp.ObjectCollection["lblSemantriaFacets"];
			lblSemantriaThemes = (Label)mp.ObjectCollection["lblSemantriaThemes"];
			lblSemantriaTopics = (Label)mp.ObjectCollection["lblSemantriaTopics"];

			Application.Run(form);
		}

		[STAThread]
		public static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Program program = new Program();
			program.Initialize();
		}

		/// <summary>
		/// Process the URL with AlchemyAPI, OpenCalais, and Semantra NLP's.
		/// </summary>
		protected void Process(object sender, EventArgs args)
		{
			ClearAllGrids();
			Application.DoEvents();

			string url = tbUrl.Text;
			sbStatus.Text = "Acquiring page content...";
			string pageText = GetPageText(url);
			
			sbStatus.Text = "Processing results with Alchemy...";
			StartTimer();
			LoadAlchemyResults(pageText);
			double alchemyTime = StopTimer();
			Application.DoEvents();

			StartTimer();
			sbStatus.Text = "Processing results with OpenCalais...";
			LoadCalaisResults(pageText);
			double calaisTime = StopTimer();
			Application.DoEvents();

			StartTimer();
			sbStatus.Text = "Processing results with Semantria...";
			LoadSemantriaResults(pageText);
			double semantriaTime = StopTimer();
			sbStatus.Text = "Done processing.";

			ReportTimes(alchemyTime, calaisTime, semantriaTime);
		}

		protected void ClearAllGrids()
		{
			dgvEntities.DataSource = null;
			dgvKeywords.DataSource = null;
			dgvConcepts.DataSource = null;

			dgvCalaisEntities.DataSource = null;
			dgvCalaisTopics.DataSource = null;
			dgvCalaisEvents.DataSource = null;

			dgvSemantriaEntities.DataSource = null;
			dgvSemantriaFacets.DataSource = null;
			dgvSemantriaThemes.DataSource = null;
			dgvSemantriaTopics.DataSource = null;
		}

		protected DateTime start;

		protected void StartTimer()
		{
			start = DateTime.Now;
		}

		/// <summary>
		/// Returns elapsed time in milliseconds.
		/// </summary>
		protected double StopTimer()
		{
			return (DateTime.Now - start).TotalSeconds;
		}

		protected void ReportTimes(double alchemyTime, double calaisTime, double semantriaTime)
		{
			string results = String.Format("AlchemyAPI: {0}\r\nOpenCalais: {1}\r\nSemantria: {2}", alchemyTime, calaisTime, semantriaTime);
			MessageBox.Show(results, "Processing Time (in seconds)", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		/// <summary>
		/// We use AlchemyAPI to get the page text for OpenCalais and Semantria.
		/// </summary>
		protected string GetPageText(string url)
		{
			AlchemyWrapper alchemy = new AlchemyWrapper();
			alchemy.Initialize();

			string xml = alchemy.GetUrlText(url);
			XmlDocument xdoc = new XmlDocument();
			xdoc.LoadXml(xml);

			return xdoc.SelectSingleNode("//text").InnerText;
		}

		/// <summary>
		/// Have Alchemy process the URL and then we populate the DataGridView controls with views of the desired tables.
		/// </summary>
		protected void LoadAlchemyResults(string text)
		{
			try
			{
				AlchemyWrapper alchemy = new AlchemyWrapper();
				alchemy.Initialize();
				DataSet dsEntities = alchemy.LoadEntities(text);
				DataSet dsKeywords = alchemy.LoadKeywords(text);
				DataSet dsConcepts = alchemy.LoadConcepts(text);

				dvEntities = new DataView(dsEntities.Tables["entity"]);
				dgvEntities.DataSource = dvEntities;

				dvKeywords = new DataView(dsKeywords.Tables["keyword"]);
				dgvKeywords.DataSource = dvKeywords;

				dvConcepts = new DataView(dsConcepts.Tables["concept"]);
				dgvConcepts.DataSource = dvConcepts;

				lblAlchemyEntities.Text = String.Format("Entities: {0}", dvEntities.Count);
				lblAlchemyKeywords.Text = String.Format("Keywords: {0}", dvKeywords.Count);
				lblAlchemyConcepts.Text = String.Format("Concepts: {0}", dvConcepts.Count);
			}
			catch (Exception ex)
			{
				ReportException(ex, "Alchemy: ");
			}
		}

		/// <summary>
		/// Have OpenCalais process the URL.
		/// </summary>
		protected void LoadCalaisResults(string text)
		{
			try
			{
				CalaisWrapper calais = new CalaisWrapper();
				calais.Initialize();
				calais.ParseUrl(text);
				dgvCalaisEntities.DataSource = calais.GetEntities();
				dgvCalaisTopics.DataSource = calais.GetTopics();
				dgvCalaisEvents.DataSource = calais.GetEvents();

				lblCalaisEntities.Text = String.Format("Entities: {0}", ((IList)dgvCalaisEntities.DataSource).Count);
				lblCalaisTopics.Text = String.Format("Topics: {0}", ((IList)dgvCalaisTopics.DataSource).Count);
				lblCalaisEvents.Text = String.Format("Events: {0}", ((IList)dgvCalaisEvents.DataSource).Count);
			}
			catch (Exception ex)
			{
				ReportException(ex, "OpenCalais: ");
			}
		}

		/// <summary>
		/// Have Semantria process the URL.
		/// </summary>
		protected void LoadSemantriaResults(string text)
		{
			try
			{
				SemantriaWrapper semantria = new SemantriaWrapper();
				semantria.Initialize();
				semantria.ParseUrl(text);
				dgvSemantriaEntities.DataSource = semantria.GetEntities();
				dgvSemantriaFacets.DataSource = semantria.GetFacets();
				dgvSemantriaThemes.DataSource = semantria.GetThemes();
				dgvSemantriaTopics.DataSource = semantria.GetTopics();

				lblSemantriaEntities.Text = String.Format("Entities: {0}", ((IList)dgvSemantriaEntities.DataSource).Count);
				lblSemantriaFacets.Text = String.Format("Facets: {0}", ((IList)dgvSemantriaFacets.DataSource).Count);
				lblSemantriaThemes.Text = String.Format("Themes: {0}", ((IList)dgvSemantriaThemes.DataSource).Count);
				lblSemantriaTopics.Text = String.Format("Topics: {0}", ((IList)dgvSemantriaTopics.DataSource).Count);
			}
			catch (Exception ex)
			{
				ReportException(ex, "Semantria: ");
			}
		}

		protected void ReportException(Exception ex, string source)
		{
			MessageBox.Show(source + "\r\n" + ex.Message, "Error in processing", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
    }
}
