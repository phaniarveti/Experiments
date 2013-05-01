﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;
using Umbraco.Core.IO;
using Umbraco.Core;
using umbraco.cms.businesslogic.macro;

namespace Umbraco.Web.UI.Umbraco.Developer.Macros
{
	public partial class EditMacro : global::umbraco.cms.presentation.developer.editMacro
	{

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			PopulatePartialViewFiles();
		}

		/// <summary>
		/// This ensures that the SelectedPartialView txt box value is set correctly when the m_macro object's 
		/// ScriptingFile property contains a full virtual path beginning with the MacroPartials path
		/// </summary>
		/// <param name="macro"> </param>
		/// <param name="macroAssemblyValue"></param>
		/// <param name="macroTypeValue"></param>
		protected override void PopulateFieldsOnLoad(Macro macro, string macroAssemblyValue, string macroTypeValue)
		{
			base.PopulateFieldsOnLoad(macro, macroAssemblyValue, macroTypeValue);
			//check if the ScriptingFile property contains the MacroPartials path
			if (macro.ScriptingFile.IsNullOrWhiteSpace() == false &&
                (macro.ScriptingFile.StartsWith(SystemDirectories.MvcViews + "/MacroPartials/")
				|| (Regex.IsMatch(macro.ScriptingFile, "~/App_Plugins/.+?/Views/MacroPartials", RegexOptions.Compiled))))
			{
				macroPython.Text = "";
				SelectedPartialView.Text = macro.ScriptingFile;
			}
		}

		/// <summary>
		/// This changes the macro type to a PartialViewMacro if the SelectedPartialView txt box has a value.
		/// This then also updates the file path saved for the partial view to be the full virtual path, not just the file name.
		/// </summary>
		/// <param name="macro"> </param>
		/// <param name="macroCachePeriod"></param>
		/// <param name="macroAssemblyValue"></param>
		/// <param name="macroTypeValue"></param>
		protected override void SetMacroValuesFromPostBack(Macro macro, int macroCachePeriod, string macroAssemblyValue, string macroTypeValue)
		{
			base.SetMacroValuesFromPostBack(macro, macroCachePeriod, macroAssemblyValue, macroTypeValue);
			if (!SelectedPartialView.Text.IsNullOrWhiteSpace())
			{
				macro.ScriptingFile = SelectedPartialView.Text;
			}
		}

		/// <summary>
		/// Populate the drop down list for partial view files
		/// </summary>
		private void PopulatePartialViewFiles()
		{
			var partialsDir = IOHelper.MapPath(SystemDirectories.MvcViews + "/MacroPartials");
			//get all the partials in the normal /MacroPartials folder
			var foundMacroPartials = GetPartialViewFiles(partialsDir, partialsDir, SystemDirectories.MvcViews + "/MacroPartials");
			//now try to find all of them int he App_Plugins/[PackageName]/Views/MacroPartials folder
			var partialPluginsDir = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.AppPlugins));
			foreach(var d in partialPluginsDir.GetDirectories())
			{
				var viewsFolder = d.GetDirectories("Views");
				if (viewsFolder.Any())
				{
					var macroPartials = viewsFolder.First().GetDirectories("MacroPartials");
					if (macroPartials.Any())
					{
						foundMacroPartials = foundMacroPartials.Concat(
							GetPartialViewFiles(macroPartials.First().FullName, macroPartials.First().FullName, SystemDirectories.AppPlugins + "/" + d.Name + "/Views/MacroPartials"));
					}
				}
			}
			
			
			
			PartialViewList.DataSource = foundMacroPartials;
			PartialViewList.DataBind();
			PartialViewList.Items.Insert(0, new ListItem("Browse partial view files on server...", string.Empty));			
		}

		/// <summary>
		/// Get the list of partial view files in the ~/Views/MacroPartials folder and in all 
		/// folders of ~/App_Plugins/[PackageName]/Views/MacroPartials
		/// </summary>
		/// <param name="orgPath"></param>
		/// <param name="path"></param>
		/// <param name="prefixVirtualPath"> </param>
		/// <returns></returns>
		private IEnumerable<string> GetPartialViewFiles(string orgPath, string path, string prefixVirtualPath)
		{
			var files = new List<string>();
			var dirInfo = new DirectoryInfo(path);

			// Populate subdirectories
			var dirInfos = dirInfo.GetDirectories();
			foreach (var dir in dirInfos)
			{
				files.AddRange(GetPartialViewFiles(orgPath, path + "/" + dir.Name, prefixVirtualPath));
			}

			var fileInfo = dirInfo.GetFiles("*.*");

			files.AddRange(
				fileInfo.Select(file => 
					prefixVirtualPath.TrimEnd('/') + "/" + (path.Replace(orgPath, string.Empty).Trim('/') + "/" + file.Name).Trim('/')));
			return files;
		}

	}
}