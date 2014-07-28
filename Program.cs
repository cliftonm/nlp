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
		protected Form form;
		protected TextBox tbUrl;
		protected StatusBarPanel sbStatus;
		protected Button btnProcess;

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
		protected DataView dvSemantriaThemes;
		protected DataView dvSemantriaTopics;

		protected DataGridView dgvSemantriaEntities;
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
		protected Label lblSemantriaThemes;
		protected Label lblSemantriaTopics;

		// NLP's:
		protected AlchemyWrapper alchemy;
		protected CalaisWrapper calais;
		protected SemantriaWrapper semantria;

		public Program()
		{
		}

		public void Initialize()
		{
			MycroParser mp = new MycroParser();
			XmlDocument doc = new XmlDocument();
			doc.Load("MainForm.xml");
			mp.Load(doc, "Form", this);
			form = (Form)mp.Process();

			// Controls we need to use:
			tbUrl = mp.ObjectCollection["tbUrl"] as TextBox;
			sbStatus = mp.ObjectCollection["sbStatus"] as StatusBarPanel;
			btnProcess = mp.ObjectCollection["btnProcess"] as Button;

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
			dgvSemantriaThemes = (DataGridView)mp.ObjectCollection["dgvSemantriaThemes"];
			dgvSemantriaTopics = (DataGridView)mp.ObjectCollection["dgvSemantriaTopics"];

			lblAlchemyEntities = (Label)mp.ObjectCollection["lblAlchemyEntities"]; ;
			lblAlchemyKeywords = (Label)mp.ObjectCollection["lblAlchemyKeywords"];
			lblAlchemyConcepts = (Label)mp.ObjectCollection["lblAlchemyConcepts"];
			lblCalaisEntities = (Label)mp.ObjectCollection["lblCalaisEntities"];
			lblCalaisTopics = (Label)mp.ObjectCollection["lblCalaisTopics"];
			lblCalaisEvents = (Label)mp.ObjectCollection["lblCalaisEvents"];
			lblSemantriaEntities = (Label)mp.ObjectCollection["lblSemantriaEntities"];
			lblSemantriaThemes = (Label)mp.ObjectCollection["lblSemantriaThemes"];
			lblSemantriaTopics = (Label)mp.ObjectCollection["lblSemantriaTopics"];

			btnProcess.Enabled = false;
			InitializeNLPs();

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

		protected async void InitializeNLPs()
		{
			sbStatus.Text = "Initializing NLP's...";

			alchemy = await Task.Run(() =>
				{
					AlchemyWrapper api = new AlchemyWrapper();
					api.Initialize();
					return api;
				});

			calais = await Task.Run(() =>
			{
				CalaisWrapper api = new CalaisWrapper();
				api.Initialize();

				return api;
			});

			semantria = await Task.Run(() =>
				{
					SemantriaWrapper api = new SemantriaWrapper();
					api.Initialize();

					return api;
				});

			btnProcess.Enabled = true;
			sbStatus.Text = "Ready";
		}

		/// <summary>
		/// Process the URL with AlchemyAPI, OpenCalais, and Semantra NLP's.
		/// </summary>
		protected async void Process(object sender, EventArgs args)
		{
			btnProcess.Enabled = false;
			ClearAllGrids();
			string url = tbUrl.Text;
			sbStatus.Text = "Acquiring page content...";

			// Eases debugging when we comment out one or more of the NLP's to test the other.
			double alchemyTime = 0;
			double calaisTime = 0;
			double semantriaTime = 0;

			string pageText = await Task.Run(() => GetUrlText(url));

			sbStatus.Text = "Processing results with Alchemy...";

			alchemyTime = await Task.Run(() =>
				{
					LoadAlchemyResults(pageText);
					return ElapsedTime();
				});

			sbStatus.Text = "Processing results with OpenCalais...";

			calaisTime = await Task.Run(() =>
				{
					LoadCalaisResults(pageText);
					return ElapsedTime();
				});

			sbStatus.Text = "Processing results with Semantria...";

			semantriaTime = await Task.Run(() =>
				{
					LoadSemantriaResults(pageText);
					return ElapsedTime();
				});

			sbStatus.Text = "Done processing.";

			ReportTimes(alchemyTime, calaisTime, semantriaTime);
			btnProcess.Enabled = true;
		}

		/// <summary>
		/// Uses AlchemyAPI to scrape the URL.  Also caches the URL, so
		/// we don't hit AlchemyAPI's servers for repeat queries.
		/// </summary>
		protected string GetUrlText(string url)
		{
			string urlHash = url.GetHashCode().ToString();
			string textFilename = urlHash + ".txt";
			string pageText;

			if (File.Exists(textFilename))
			{
				pageText = File.ReadAllText(textFilename);
			}
			else
			{
				pageText = GetPageText(url); 
			}

			File.WriteAllText(textFilename, pageText);

			return pageText;
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
			dgvSemantriaThemes.DataSource = null;
			dgvSemantriaTopics.DataSource = null;
		}

		protected DateTime start;
		protected DateTime end;

		protected void StartTimer()
		{
			start = DateTime.Now;
			end = DateTime.Now;
		}

		/// <summary>
		/// Returns elapsed time in milliseconds.
		/// </summary>
		protected double StopTimer()
		{
			end = DateTime.Now;

			return ElapsedTime();
		}

		protected double ElapsedTime()
		{
			return (end - start).TotalSeconds;
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
				StartTimer();
				DataSet dsEntities = alchemy.LoadEntities(text);
				DataSet dsKeywords = alchemy.LoadKeywords(text);
				DataSet dsConcepts = alchemy.LoadConcepts(text);
				StopTimer();

				form.BeginInvoke((Action)(() =>
					{
						dvEntities = new DataView(dsEntities.Tables["entity"]);
						dgvEntities.DataSource = dvEntities;

						dvKeywords = new DataView(dsKeywords.Tables["keyword"]);
						dgvKeywords.DataSource = dvKeywords;

						dvConcepts = new DataView(dsConcepts.Tables["concept"]);
						dgvConcepts.DataSource = dvConcepts;

						lblAlchemyEntities.Text = String.Format("Entities: {0}", dvEntities.Count);
						lblAlchemyKeywords.Text = String.Format("Keywords: {0}", dvKeywords.Count);
						lblAlchemyConcepts.Text = String.Format("Concepts: {0}", dvConcepts.Count);
					}));
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
				StartTimer();
				calais.ParseUrl(text);
				StopTimer();

				form.BeginInvoke((Action)(() =>
					{
						dgvCalaisEntities.DataSource = calais.GetEntities();
						dgvCalaisTopics.DataSource = calais.GetTopics();
						dgvCalaisEvents.DataSource = calais.GetEvents();

						lblCalaisEntities.Text = String.Format("Entities: {0}", ((IList)dgvCalaisEntities.DataSource).Count);
						lblCalaisTopics.Text = String.Format("Topics: {0}", ((IList)dgvCalaisTopics.DataSource).Count);
						lblCalaisEvents.Text = String.Format("Events: {0}", ((IList)dgvCalaisEvents.DataSource).Count);
					}));
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
				StartTimer();
				semantria.ParseUrl(text);
				StopTimer();

				form.BeginInvoke((Action)(() =>
					{
						dgvSemantriaEntities.DataSource = semantria.GetEntities();
						dgvSemantriaThemes.DataSource = semantria.GetThemes();
						dgvSemantriaTopics.DataSource = semantria.GetTopics();

						lblSemantriaEntities.Text = String.Format("Entities: {0}", ((IList)dgvSemantriaEntities.DataSource).Count);
						lblSemantriaThemes.Text = String.Format("Themes: {0}", ((IList)dgvSemantriaThemes.DataSource).Count);
						lblSemantriaTopics.Text = String.Format("Topics: {0}", ((IList)dgvSemantriaTopics.DataSource).Count);
					}));
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
