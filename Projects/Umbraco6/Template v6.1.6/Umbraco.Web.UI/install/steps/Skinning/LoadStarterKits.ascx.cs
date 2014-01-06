﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Logging;
using Umbraco.Web.Install;

namespace Umbraco.Web.UI.Install.Steps.Skinning
{
    public delegate void StarterKitInstalledEventHandler();

    public partial class LoadStarterKits : StepUserControl
    {
        /// <summary>
        /// Returns the string for the package installer web service base url
        /// </summary>
        protected string PackageInstallServiceBaseUrl { get; private set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //Get the URL for the package install service base url
            var umbracoPath = Core.Configuration.GlobalSettings.UmbracoMvcArea;
            var urlHelper = new UrlHelper(Context.Request.RequestContext);
            PackageInstallServiceBaseUrl = urlHelper.Action("Index", "InstallPackage", new { area = umbracoPath });
        }

        /// <summary>
		/// Flag to show if we can connect to the repo or not
		/// </summary>
		protected bool CannotConnect { get; private set; }

		public event StarterKitInstalledEventHandler StarterKitInstalled;

		protected virtual void OnStarterKitInstalled()
		{
			StarterKitInstalled();
		}


		private readonly global::umbraco.cms.businesslogic.packager.repositories.Repository _repo;
        private const string RepoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";

        public LoadStarterKits()
		{
            _repo = global::umbraco.cms.businesslogic.packager.repositories.Repository.getByGuid(RepoGuid);
		}

		protected void Page_Load(object sender, EventArgs e)
		{

		}

		protected void NextStep(object sender, EventArgs e)
		{
            var p = (Default)this.Page;
			InstallHelper.RedirectToNextStep(Page, Request.GetItemAsString("installStep"));
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			//clear progressbar cache
            InstallHelper.ClearProgress();

			if (_repo.HasConnection())
			{
				try
				{
					rep_starterKits.DataSource = _repo.Webservice.StarterKits();
					rep_starterKits.DataBind();
				}
				catch (Exception ex)
				{
					LogHelper.Error<LoadStarterKits>("Cannot connect to package repository", ex);
					CannotConnect = true;

				}
			}
			else
			{
				CannotConnect = true;
			}
		}


		protected void GotoLastStep(object sender, EventArgs e)
		{
            InstallHelper.RedirectToLastStep(Page);
		}

    }
}