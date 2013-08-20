using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using idseefeld.de.imagecropper;
using umbraco.DataLayer;
using umbraco.interfaces;
using System.Text;
using umbraco.macroRenderings;
using umbraco.editorControls;
using System.Drawing;
using umbraco.BasePages;

[assembly: WebResource("idseefeld.de.imagecropper.imagecropper.Resources.product-logo.png", "image/png")]
namespace idseefeld.de.imagecropper.imagecropper {
	public class PrevalueEditor : PlaceHolder, IDataPrevalue {
		private readonly umbraco.cms.businesslogic.datatype.BaseDataType _dataType;

		private Config config;

		//private TextBox txtPropertyAlias;
		private propertyTypePicker imagePropertyTypePicker; // this has replaced txtPropertyAlias (a textbox used to enter a property alias)
		private RequiredFieldValidator imagePropertyRequiredFieldValidator;

		private TextBox txtBackgroungColor;
		private CheckBox chkIgnoreICC;
		private CheckBox chkShowIgnoreICC;
		private CheckBox chkAutoGenerateCrops;
		private CheckBox chkGenerateCrops;
		private CheckBox chkCompatibilityModeJpeg;
		private CheckBox chkShowLabel;
		private Literal litQuality;
		private TextBox txtQuality;
		private TextBox txtResizeMax;

		private SmartListBox slbPresets;
		private TextBox txtCropName;
		private TextBox txtTargetWidth;
		private TextBox txtTargetHeight;
		private CheckBox chkKeepAspect;
		private DropDownList ddlDefaultPosH;
		private DropDownList ddlDefaultPosV;

		private Button btnUp;
		private Button btnDown;
		private Button btnAddUpdate;
		private Button btnRemove;
		private Button btnEdit;

		private Button btnGenerateAll;
		private Panel pnlProgress;
		private Panel pnlProgressBar;
		private Literal litProgressText;


		public PrevalueEditor(umbraco.cms.businesslogic.datatype.BaseDataType dataType)
		{
			_dataType = dataType;

			SetupChildControls();
		}

		public void SetupChildControls()
		{
			//txtPropertyAlias = new TextBox { ID = "upload", Width = Unit.Pixel(200) };
			this.imagePropertyTypePicker = new propertyTypePicker() { ID = "imagePropertyTypePicker" };
			this.imagePropertyRequiredFieldValidator = new RequiredFieldValidator()
			{
				ID = "imagePropertyRequiredFieldValidator",
				Text = " Required",
				InitialValue = string.Empty,
				ControlToValidate = this.imagePropertyTypePicker.ID
			};

			txtBackgroungColor = new TextBox { ID = "backgroungColor", Width = Unit.Pixel(200) };
			chkGenerateCrops = new CheckBox { ID = "generateimg", AutoPostBack = true };
			chkAutoGenerateCrops = new CheckBox { ID = "autogenerateimg", AutoPostBack = false };
			chkIgnoreICC = new CheckBox { ID = "ignoreICC", AutoPostBack = false };
			chkShowIgnoreICC = new CheckBox { ID = "showIgnoreICC", AutoPostBack = false };
			chkCompatibilityModeJpeg = new CheckBox { ID = "compatibilityMode", AutoPostBack = false };
			litQuality = new Literal { ID = "qualityLiteral", Text = " Quality ", Visible = false };
			txtQuality = new TextBox { ID = "quality", Width = Unit.Pixel(30), Visible = false };
			txtQuality.Text = "90";
			chkShowLabel = new CheckBox { ID = "label" };
			txtResizeMax = new TextBox { ID = "resizeMax", Width = Unit.Pixel(200) };
			slbPresets = new SmartListBox
							 {
								 ID = "presets",
								 SelectionMode = ListSelectionMode.Multiple,
								 Height = Unit.Pixel(123),
								 Width = Unit.Pixel(350)
							 };

			txtCropName = new TextBox { ID = "presetname", Width = Unit.Pixel(100) };
			txtTargetWidth = new TextBox { ID = "presetw", Width = Unit.Pixel(50) };
			txtTargetHeight = new TextBox { ID = "preseth", Width = Unit.Pixel(50) };
			chkKeepAspect = new CheckBox { ID = "aspect", Checked = true };

			ddlDefaultPosH = new DropDownList { ID = "posh" };
			ddlDefaultPosH.Items.Add(new ListItem("Left", "L"));
			ddlDefaultPosH.Items.Add(new ListItem("Center", "C"));
			ddlDefaultPosH.Items.Add(new ListItem("Right", "R"));

			ddlDefaultPosV = new DropDownList { ID = "posv" };
			ddlDefaultPosV.Items.Add(new ListItem("Top", "T"));
			ddlDefaultPosV.Items.Add(new ListItem("Middle", "M"));
			ddlDefaultPosV.Items.Add(new ListItem("Bottom", "B"));

			btnUp = new Button { ID = "up", Text = "Up", Width = Unit.Pixel(60) };
			btnDown = new Button { ID = "down", Text = "Down", Width = Unit.Pixel(60) };
			btnAddUpdate = new Button { ID = "add", Text = "Add / Update", Width = Unit.Pixel(100) };
			btnRemove = new Button { ID = "remove", Text = "Remove", Width = Unit.Pixel(60) };
			btnEdit = new Button { ID = "edit", Text = "Edit", Width = Unit.Pixel(60) };

			btnGenerateAll = new Button { ID = "generate", Text = "Generate all crops", Width = Unit.Pixel(120) };
			pnlProgress = new Panel
			{
				ID = "progress",
				Width = Unit.Pixel(300),
				Height = Unit.Pixel(20),
				BackColor = Color.FromArgb(255, 130, 130),
				BorderWidth = Unit.Pixel(1),
				Visible = false
			};
			pnlProgressBar = new Panel
			{
				ID = "progressBar",
				Width = Unit.Pixel(3),
				Height = Unit.Pixel(20),
				BackColor = Color.FromArgb(0, 130, 0),
			};
			//litProgressText = new Literal
			//{
			//    ID = "progressText",
			//    Text = "0 %"
			//};
			pnlProgress.Controls.Add(pnlProgressBar);
			//pnlProgress.Controls.Add(litProgressText);


			//Controls.Add(txtPropertyAlias);
			Controls.Add(this.imagePropertyTypePicker);
			Controls.Add(this.imagePropertyRequiredFieldValidator);

			Controls.Add(txtBackgroungColor);
			Controls.Add(txtResizeMax);
			Controls.Add(chkGenerateCrops);
			Controls.Add(chkAutoGenerateCrops);
			Controls.Add(chkIgnoreICC);
			Controls.Add(chkShowIgnoreICC);
			Controls.Add(chkCompatibilityModeJpeg);
			Controls.Add(litQuality);
			Controls.Add(txtQuality);
			Controls.Add(chkShowLabel);

			Controls.Add(slbPresets);
			Controls.Add(txtCropName);
			Controls.Add(txtTargetWidth);
			Controls.Add(txtTargetHeight);
			Controls.Add(chkKeepAspect);
			Controls.Add(ddlDefaultPosH);
			Controls.Add(ddlDefaultPosV);

			Controls.Add(btnUp);
			Controls.Add(btnDown);
			Controls.Add(btnAddUpdate);
			Controls.Add(btnRemove);
			Controls.Add(btnEdit);

			Controls.Add(btnGenerateAll);
			Controls.Add(pnlProgress);

			btnUp.Click += _upButton_Click;
			btnDown.Click += _downButton_Click;
			btnAddUpdate.Click += _addButton_Click;
			btnRemove.Click += _removeButton_Click;
			btnEdit.Click += _editButton_Click;

			btnGenerateAll.Click += _generateButton_Click;

			chkGenerateCrops.CheckedChanged += _generateImagesCheckBox_CheckedChanged;


		}
		void _generateButton_Click(object sender, EventArgs e)
		{
			//TODO: implement crop generator with progress bar
			//pnlProgress.Visible = true;
			//pnlProgressBar.Width = Unit.Pixel(3 * 60);
		}

		void _generateImagesCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			txtQuality.Visible = chkGenerateCrops.Checked;
			litQuality.Visible = chkGenerateCrops.Checked;
		}

		void _upButton_Click(object sender, EventArgs e)
		{
			slbPresets.MoveUp();
		}

		void _downButton_Click(object sender, EventArgs e)
		{
			slbPresets.MoveDown();
		}

		void _removeButton_Click(object sender, EventArgs e)
		{
			slbPresets.RemoveSelected();
		}

		void _editButton_Click(object sender, EventArgs e)
		{
			ListItem item = slbPresets.GetSelectedItem();
			if (item == null)
				return;

			string[] values = item.Value.Split(',');
			if (values.Length < 3)
				return;

			txtCropName.Text = values[0];
			txtTargetWidth.Text = values[1];
			txtTargetHeight.Text = values[2];
			if (values.Length > 3)
				chkKeepAspect.Checked = values[3] == "1";
			if (values.Length > 4)
			{
				ddlDefaultPosH.SelectedValue = values[4].Substring(0, 1);
				ddlDefaultPosV.SelectedValue = values[4].Substring(1, 1);
			}
		}

		void _addButton_Click(object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty(txtCropName.Text)
				&& (!String.IsNullOrEmpty(txtTargetWidth.Text) || !String.IsNullOrEmpty(txtTargetHeight.Text)))
			{
				string itemText = getListItemDisplayName(
							txtCropName.Text,
							txtTargetWidth.Text,
							txtTargetHeight.Text,
							chkKeepAspect.Checked ? "1" : "0",
							String.Concat(ddlDefaultPosH.SelectedValue, ddlDefaultPosV.SelectedValue));
				string newValue = String.Format("{0},{1},{2},{3},{4}",
									  txtCropName.Text,
									  String.IsNullOrEmpty(txtTargetWidth.Text) ? "0" : txtTargetWidth.Text,
									  String.IsNullOrEmpty(txtTargetHeight.Text) ? "0" : txtTargetHeight.Text,
									  chkKeepAspect.Checked ? "1" : "0",
									  String.Concat(ddlDefaultPosH.SelectedValue, ddlDefaultPosV.SelectedValue));

				ListItem item = slbPresets.FindItem(txtCropName.Text); // slbPresets.GetSelectedItem();//
				if (item == null)
				{
					slbPresets.Items.Add(new ListItem(itemText, newValue));
				}
				else
				{
					item.Text = itemText;
					item.Value = newValue;
				}
				txtCropName.Text = "";
				txtTargetWidth.Text = "";
				txtTargetHeight.Text = "";
				chkKeepAspect.Checked = true;

				Save();
				//shoh saved bubble
				BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.save, "Datatype saved", "");
			}
			else
			{
				//show error bubble
				BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.error, "Not added", "The values are not vaild.");
			}

		}

		public Control Editor
		{
			get
			{
				return this;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			string _propertyDefault = "umbracoFile";
			if (this.imagePropertyTypePicker.Items.Contains(new ListItem(_propertyDefault)))
			{
				this.imagePropertyTypePicker.SelectedValue = _propertyDefault;
			}

			if (!Page.IsPostBack)
				LoadData();

			if (config == null)
			{
				config = new Config(Configuration);
			}

		}

		private void LoadData()
		{
			if (!string.IsNullOrEmpty(Configuration))
			{
				if (config == null)
				{
					config = new Config(Configuration);
				}
				//txtPropertyAlias.Text = config.UploadPropertyAlias;
				if (this.imagePropertyTypePicker.Items.Contains(new ListItem(config.UploadPropertyAlias)))
				{
					this.imagePropertyTypePicker.SelectedValue = config.UploadPropertyAlias;
				}

				txtBackgroungColor.Text = config.BackgroundColor;
				txtResizeMax.Text = config.ResizeMax > 0 ? config.ResizeMax.ToString() : "";
				chkGenerateCrops.Checked = config.GenerateImages;
				chkAutoGenerateCrops.Checked = config.AutoGenerateImages;
				chkIgnoreICC.Checked = config.IgnoreICC;
				chkShowIgnoreICC.Checked = config.ShowIgnoreICC;
				chkCompatibilityModeJpeg.Checked = config.CompatibilityModeJpeg;
				chkShowLabel.Checked = config.ShowLabel;
				txtQuality.Visible = chkGenerateCrops.Checked;
				txtQuality.Text = config.Quality.ToString();
				litQuality.Visible = chkGenerateCrops.Checked;

				foreach (Preset preset in config.presets)
				{
					if (!String.IsNullOrEmpty(preset.Name))
					{
						slbPresets.Items.Add(
							new ListItem(
								getListItemDisplayName(
									preset.Name,
									preset.TargetWidth.ToString(),
									preset.TargetHeight.ToString(),
									preset.KeepAspect ? "1" : "0",
									String.Concat(preset.PositionH, preset.PositionV)),
								String.Format("{0},{1},{2},{3},{4}{5}",
											  preset.Name,
											  preset.TargetWidth,
											  preset.TargetHeight,
											  preset.KeepAspect ? "1" : "0",
											  preset.PositionH, preset.PositionV)));
					}
				}
			}
		}

		private static string getListItemDisplayName(string presetTemplateName, string width, string height, string keepAspect, string position)
		{
			string rVal = String.Empty;
			if (String.IsNullOrEmpty(width) || String.IsNullOrEmpty(height) || width.Equals("0") || height.Equals("0"))
			{
				rVal = String.Format("{0}, width: {1}px, height: {2}px",
								 presetTemplateName,
								 String.IsNullOrEmpty(width) ? "0" : width,
								 String.IsNullOrEmpty(height) ? "0" : height);
			}
			else
			{
				rVal = String.Format("{0}, width: {1}px, height: {2}px, keep aspect: {3}, {4}",
												 presetTemplateName,
												 width,
												 height,
												 keepAspect == "1" ? "yes" : "no",
												 position);
			}
			return rVal;
		}

		/// <summary>
		/// Serialize configuration to:
		/// uploadPropertyAlias,generateImages,showLabel|presetTemplateName,targetWidth,targetHeight,keepAspect;
		/// </summary>
		public void Save()
		{
			_dataType.DBType = (umbraco.cms.businesslogic.datatype.DBTypes)Enum.Parse(
																			   typeof(umbraco.cms.businesslogic.datatype.DBTypes), DBTypes.Ntext.ToString(), true);
			string generalData = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
				//txtPropertyAlias.Text,
											   this.imagePropertyTypePicker.SelectedValue,
											   chkGenerateCrops.Checked ? "1" : "0",
											   chkShowLabel.Checked ? "1" : "0",
											   txtQuality.Text,
											   chkCompatibilityModeJpeg.Checked ? "1" : "0",
											   txtBackgroungColor.Text,
											   txtResizeMax.Text,
											   chkAutoGenerateCrops.Checked ? "1" : "0",
											   chkIgnoreICC.Checked ? "1" : "0",
											   chkShowIgnoreICC.Checked ? "1" : "0"
				);

			string templateData = "";

			for (int i = 0; i < slbPresets.Items.Count; i++)
			{
				templateData += slbPresets.Items[i].Value;
				if (i < slbPresets.Items.Count - 1) templateData += ";";
			}

			string data = String.Format("{0}|{1}", generalData, templateData);

			SqlHelper.ExecuteNonQuery("delete from cmsDataTypePreValues where datatypenodeid = @dtdefid",
									  SqlHelper.CreateParameter("@dtdefid", _dataType.DataTypeDefinitionId));

			SqlHelper.ExecuteNonQuery("insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')",
									  SqlHelper.CreateParameter("@dtdefid", _dataType.DataTypeDefinitionId), SqlHelper.CreateParameter("@value", data));
		}

		protected override void Render(HtmlTextWriter writer)
		{
			writer.Write("<style type=\"text/css\">");
			writer.Write("div.about{color:#006AB3;padding:10px;font-style:italic;}");
			writer.Write("div.about a{color:#006AB3;}");
			writer.Write("td.hintText{font-size:90%;color:#999;padding-bottom:10px;}");
			writer.Write("#progress{margin-bottom:10px;}");
			writer.Write("</style>");

			//TODO: show progress of crop generation
			//pnlProgress.RenderControl(writer);

			if (!config.CustomProvider)
			{
				writer.Write("<div style=\"background-color:red;color:#fff;padding:10px;\">ImageResizer.dll is missing. Cropper uses default render engine. Please reinstall Image Cropper Extended.</div>");
			}
			//writer.Write("<div style=\"float:left;overflow:hidden;\"><p><strong>General</strong></p></div>");
			writer.Write(String.Format("<div style=\"float:left;overflow:hidden;width:100%;\"><a href='https://github.com/idseefeld/imagecropper4umbraco' target='_blank'><img src='{0}' align='right' /></a>{1}</div>",
			Page.ClientScript.GetWebResourceUrl(
				typeof(PrevalueEditor),
				"idseefeld.de.imagecropper.imagecropper.Resources.product-logo.png"),
			config.CustomProvider ? "<div class=\"about\">Using ImageResizer as image render engine.<br /> For more information visit <a href=\"http://imageresizing.net\" target=\"_blank\">http://imageresizing.net</a>.</div>" : "")
			);

			writer.Write("<table style=\"clear:both\">");


			writer.Write("<tr><td>Property alias:</td><td>");

			this.imagePropertyTypePicker.RenderControl(writer);
			this.imagePropertyRequiredFieldValidator.RenderControl(writer);
			writer.Write("</td></tr>");
			writer.Write("<tr><td colspan=\"2\" class='hintText'>As document type property any image cropper data type can only connect to one upload property.<br />But you can define another data type for a second upload.</td></tr>");


			writer.Write("  <tr><td>Save crop images as files:</td><td>");
			chkGenerateCrops.RenderControl(writer);
			litQuality.RenderControl(writer);
			txtQuality.RenderControl(writer);
			writer.Write("  </td></tr>");
			writer.Write("  <tr><td class='hintText'>(/media/(imageid)/(filename)_(cropname).*)</td><td></td></tr>");

			writer.Write("  <tr><td>Ignore ICC </td><td>");
			chkIgnoreICC.RenderControl(writer);
			writer.Write("  </td></tr>");
			writer.Write("  <tr><td colspan=\"2\" class='hintText'>If checked the ICC profile embedded in the source image will be ignored.</td></tr>");
			writer.Write("  <tr><td>Show ignore ICC in editor</td><td>");
			chkShowIgnoreICC.RenderControl(writer);
			writer.Write("  </td></tr>");
			writer.Write("  <tr><td colspan=\"2\" class='hintText'>If checked you can set the <em>ignore ICC</em> option for each image item.</td></tr>");

			writer.Write("  <tr><td>Show Label:</td><td>");
			chkShowLabel.RenderControl(writer);
			writer.Write("  </td></tr>");

			writer.Write("</table>");

			writer.Write("<p><strong>Crops</strong></p>");

			writer.Write("<table>");
			writer.Write("  <tr><td colspan='3' class='hintText'>To constrain only one dimension set the other one to 0.<br />e.g. <em>Target width = 200 px, Target height: 0 px</em>. In this case <em>Default postion</em> and <em>Keep aspect</em> are ignored.</td></tr>");
			writer.Write("  <tr><td valign=\"top\">");

			writer.Write("      <table>");
			writer.Write("          <tr><td>Name</td><td>");
			txtCropName.RenderControl(writer);
			writer.Write("          </td></tr>");
			writer.Write("          <tr><td>Target width</td><td>");
			txtTargetWidth.RenderControl(writer);
			writer.Write("          px</td></tr>");
			writer.Write("          <tr><td>Target height</td><td>");
			txtTargetHeight.RenderControl(writer);
			writer.Write("          px</td></tr>");
			writer.Write("          <tr><td>Default position&nbsp;</td><td>");
			ddlDefaultPosH.RenderControl(writer);
			writer.Write(" ");
			ddlDefaultPosV.RenderControl(writer);
			writer.Write("          </td></tr>");
			writer.Write("          <tr><td>Keep aspect</td><td>");
			chkKeepAspect.RenderControl(writer);
			writer.Write("          </td></tr>");
			writer.Write("      </table><br />");
			btnAddUpdate.RenderControl(writer);
			writer.Write("<br /><br />");

			//TODO: id crop generator is implemented then show button
			//btnGenerateAll.RenderControl(writer);

			writer.Write("  </td><td valign=\"top\">&nbsp;&nbsp;");
			slbPresets.RenderControl(writer);
			writer.Write("  </td><td valign=\"top\">");
			btnUp.RenderControl(writer);
			writer.Write("  <br />");
			btnDown.RenderControl(writer);
			writer.Write("  <br /><br /><br />");
			btnEdit.RenderControl(writer);
			writer.Write("  <br />");
			btnRemove.RenderControl(writer);
			writer.Write("  </td></tr>");
			writer.Write("</table>");

		}

		public string Configuration
		{
			get
			{
				object conf =
					SqlHelper.ExecuteScalar<object>("select value from cmsDataTypePreValues where datatypenodeid = @datatypenodeid",
													SqlHelper.CreateParameter("@datatypenodeid", _dataType.DataTypeDefinitionId));

				if (conf != null)
					return conf.ToString();

				return string.Empty;
			}
		}

		public static ISqlHelper SqlHelper
		{
			get
			{
				return Application.SqlHelper;
			}
		}
	}
}