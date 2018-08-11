using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTCTickSim
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Form1Instance = this;
        }
        private static Form1 _form1Instance;
        public static Form1 Form1Instance
        {
            set { _form1Instance = value; }
            get { return _form1Instance; }
        }



        private void buttonStart_Click(object sender, EventArgs e)
        {
            ThreadMaster tm = new ThreadMaster();
            tm.start();
        }

        private void buttonGA_Click(object sender, EventArgs e)
        {
            ThreadMaster tm = new ThreadMaster();
            tm.startGA();
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            TickData.readTickData();
            TickData.writeData(0, 10000);
        }

        private void buttonMultiGASIM_Click(object sender, EventArgs e)
        {
            ThreadMaster tm = new ThreadMaster();
            tm.startMultiGA();
        }

        private void buttonContiGASim_Click(object sender, EventArgs e)
        {
            ThreadMaster th = new ThreadMaster();
            th.startContiGASIm();
        }

        #region Delegate
        private delegate void setLabel1Delegate(string text);
        public void setLabel(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new setLabel1Delegate(setLabel), text);
                return;
            }
            this.label1.Text = text;
        }

        private delegate void setLabel2Delegate(string text);
        public void setLabel2(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new setLabel2Delegate(setLabel2), text);
                return;
            }
            this.label2.Text = text;
        }

        private delegate void setLabel3Delegate(string text);
        public void setLabel3(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new setLabel3Delegate(setLabel3), text);
                return;
            }
            this.label3.Text = text;
        }

        private delegate void addListBoxDelegate(string text);
        public void addListBox(string text)
        {
            if(InvokeRequired)
            {
                Invoke(new addListBoxDelegate(addListBox), text);
                return;
            }
            this.listBox1.Items.Add(text);
            this.listBox1.TopIndex = this.listBox1.Items.Count-1;
        }

        private delegate void initializeListBoxDelegate();
        public void initializeListBox()
        {
            if(InvokeRequired)
            {
                Invoke(new initializeListBoxDelegate(initializeListBox));
                    this.listBox1.Items.Clear();
            }
        }

        private delegate void addListBox2Delegate(string text);
        public void addListBox2(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new addListBox2Delegate(addListBox2), text);
                return;
            }
            this.listBox2.Items.Add(text);
            this.listBox2.TopIndex = this.listBox2.Items.Count - 1;
        }


        #endregion

        
    }
}
