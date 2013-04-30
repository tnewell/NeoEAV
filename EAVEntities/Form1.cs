using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NeoEAV.Data.DataClasses;


namespace EAVEntities
{
    public partial class Form1 : Form
    {
        EAVEntityContext ctx = new EAVEntityContext();

        public Form1()
        {
            InitializeComponent();
        }

        private void ctlGoButton_Click(object sender, EventArgs e)
        {
            Project p = new Project() { Name = "Test Project" };

            ctx.Projects.Add(p);

            p.Containers.Add(new Container() { Name = "Test Container", DisplayName = "Test Container", HasFixedInstances = false, IsRepeating = false, Sequence = 0 });

            ctx.SaveChanges();

            ctlResults.AppendText("\r\nDone\r\n");
        }
    }
}
