﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;

namespace umbraco.uicontrols.TreePicker
{
    public class SimpleMediaPicker : BaseTreePicker
    {
        public override string TreePickerUrl
        {
            get
            {
                return TreeUrlGenerator.GetPickerUrl(Constants.Applications.Media, "media");
            }
        }

        public override string ModalWindowTitle
        {
            get
            {
                return ui.GetText("general", "choose") + " " + ui.GetText("sections", "media");
            }
        }
    }
}
