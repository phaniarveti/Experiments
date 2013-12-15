﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Umbraco.Web.Install
{
    internal class InstallerStepCollection : Dictionary<string, InstallerStep>
    {
        public void Add(InstallerStep step){
            step.Index = this.Count;
            this.Add(step.Alias, step);
        }
  
        public InstallerStep Get(string key)
        {
            return this.First(item => item.Key == key).Value;
        }

        public bool StepExists(string key)
        {
            return this.ContainsKey(key);
        }

        public InstallerStep GotoNextStep(string key)
        {
          var s = this[key];
          foreach(var i in this.Values){

             // System.Web.HttpContext.Current.Response.Write(i.Index.ToString() + i.Alias);

            if (i.Index > s.Index && !i.Completed()) {
               // System.Web.HttpContext.Current.Response.Write( "FOUND" +  i.Index.ToString() + i.Alias);
                return i;
            }
          }
  
          return null;
        }
    

        public InstallerStep FirstAvailableStep()
        {
            return this.First(item => item.Value.Completed() == false ).Value;
        }
    }
}

