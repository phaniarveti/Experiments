﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces.skinning;
using System.IO;
using umbraco.IO;
using System.Web;

namespace umbraco.cms.businesslogic.skinning.tasks
{
    public class ModifyCss : TaskType
    {
        public string TargetFile { get; set; }
        public string TargetRule { get; set; }
        public string TargetParameter { get; set; }

        public ModifyCss()
        {
            this.Name = "Modify Css";
            this.Description = "Will modify the stylesheet";
        }

        public override TaskExecutionDetails Execute(string Value)
        {
            TaskExecutionDetails d = new TaskExecutionDetails();

            //currently just appending it to the end of the css file
            StreamWriter sw = File.AppendText(IO.IOHelper.MapPath(SystemDirectories.Css) + "/" + TargetFile);
            sw.WriteLine(string.Format("{0}{{ {1}:{2};}}", TargetRule, TargetParameter, Value));
            sw.Close();

            d.TaskExecutionStatus = TaskExecutionStatus.Completed;
            d.OriginalValue = "";
            d.NewValue = Value;

            return d;
        }

        public override TaskExecutionStatus RollBack(string OriginalValue)
        {
            string[] filecontents = File.ReadAllLines(IO.IOHelper.MapPath(SystemDirectories.Css) + "/" + TargetFile);

            for (int i = filecontents.Length; i > 0; i--)
            {
                if (filecontents[i - 1].Contains(string.Format("{0}{{ {1}:", TargetRule, TargetParameter)))
                {
                    filecontents[i - 1] = string.Empty;
                    break;
                }
            }

            StringBuilder s = new StringBuilder();

            foreach (string line in filecontents)
                s.AppendLine(line);

            System.IO.StreamWriter file = new System.IO.StreamWriter(IO.IOHelper.MapPath(SystemDirectories.Css) + "/" + TargetFile);
            file.WriteLine(s.ToString());

            file.Close();


            return TaskExecutionStatus.Completed;
        }

        public override string PreviewClientScript(string ControlClientId,string ClientSidePreviewEventType, string ClientSideGetValueScript)
        {
           
            return string.Format(
                @"jQuery('#{0}').bind('{5}', function() {{ 
                        var val = '{3}'; 
                        jQuery('{1}').css('{2}', val.replace('${{Output}}',{4})); 
                }});


                //cancel support
                var init{6} = jQuery('{1}').css('{2}');
                jQuery('#cancelSkinCustomization').click(function () {{ 
                    jQuery('{1}').css('{2}',init{6}); 
                }});

                ",
                ControlClientId,
                TargetRule,
                TargetParameter,
                Value,
                ClientSideGetValueScript,
                ClientSidePreviewEventType,
                Guid.NewGuid().ToString().Replace("-", ""));
        }
    }
}
