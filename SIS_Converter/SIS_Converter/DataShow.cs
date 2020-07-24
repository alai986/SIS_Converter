using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SIS_Converter
{
    public partial class DataShow : Form
    {
        public DataShow()
        {
            InitializeComponent();
        }

        private void DataShow_Load(object sender, EventArgs e)
        {
            this.reoGridControl1.CellsSelectionCursor = System.Windows.Forms.Cursors.Default;
        }
    }
}
