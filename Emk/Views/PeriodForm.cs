using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Emk.Views
{
    public partial class PeriodForm : Form
    {
        public DateTime PeriodBegin;
        public DateTime PeriodEnd;

        public PeriodForm()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            PeriodBegin = dtpBegin.Value;
            PeriodEnd = dtpEnd.Value;
        }
    }
}
