using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NeoEAV.Data.DataClasses;
using NeoEAV.Windows.UI;

using Container = NeoEAV.Data.DataClasses.Container;
using Attribute = NeoEAV.Data.DataClasses.Attribute;


namespace NeoEAVWindows
{
    public partial class NeoEAVForm : Form
    {
        EAVEntityContext ctx = new EAVEntityContext();
        int nCurrentAttributeIndex = 0;

        public NeoEAVForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ctx.Projects.Load();

            ctlProject.DataSource = ctx.Projects.Local;

            ctlProject.ContextKey = "Test Project 1";
            ctlSubject.ContextKey = "Subject 1";
            ctlContainer.ContextKey = "Test Root Container 1";
            ctlInstance.ContextKey = "0";
        }

        private void ctlGoButton_Click(object sender, EventArgs e)
        {
            var attributes = ((Container)ctlContainer.DataItem).Attributes.Select(it => it.Name);
            int max = ((Container)ctlContainer.DataItem).Attributes.Count();

            ctlAttribute.ContextKey = attributes.ElementAt(nCurrentAttributeIndex);
            ctlProject.DataBind();

            nCurrentAttributeIndex = (nCurrentAttributeIndex + 1) % max;
        }
    }
}
