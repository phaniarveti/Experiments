﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using ClientDependency.Core;


namespace umbraco.cms.businesslogic.skinning.controls
{
    [ClientDependency(ClientDependencyType.Css, "modal/style.css", "UmbracoClient")]
    [ClientDependency(500,ClientDependencyType.Javascript, "modal/modal.js", "UmbracoClient")]
    public class ImageUploader : TextBox
    {
        public int ImageHeight { get; set; }
        public int ImageWidth { get; set; }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            base.Render(writer);

            writer.WriteLine(
                string.Format(
                "&nbsp;<a href=\"#\" onclick=\"{0}\">Upload image</a>",
                "Umbraco.Controls.ModalWindow().open('" + this.ResolveUrl(GlobalSettings.Path) + "/LiveEditing/Modules/SkinModule/ImageUploader.aspx?ctrl=" + this.ClientID + "&w="+this.ImageWidth+"&h="+this.ImageHeight +"','Upload image',true,750,550,50,0, ['.modalbuton'], null);return false;"));
        }
    }
}
