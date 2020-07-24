using Serilog;
using Sharp7;
using SimpleTCP;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using unvell.ReoGrid.Events;

namespace SIS_Converter
{


    public partial class MainForm : Form
    {

        private S7Client Client;
        // private Worksheet sheet;      
        private string SequenceNumber = "0001";//发送的序列号
        private string AssignmentID = "00000001";//任务ID
        private string Position = "A1";
        private string Row = "000";
        private string Column = "00";
        private string Rack = "001";
        private string Depth = "02";
        private int Step = 0;
        // private string[] craneData = new string[17];
        System.Timers.Timer IdleTime;//60s
        System.Timers.Timer LinkLossTime;//70s
        System.Timers.Timer BufferFullTime;//10s
        System.Timers.Timer ConfirmTime;//10s
        private int IdleTimeout, LinkLossTimeout, BufferFullTimeout, ConfirmTimeout;
        SimpleTcpServer server1, server2;
        private string server1SendData = "";//发送的数据                      
        private string server1RecData = "";//接收的数据      
        Event recData = new Event();
        Event sendData = new Event();
        public MainForm()
        {
            InitializeComponent();
            Client = new S7Client();
            Thread t1 = new Thread(new ThreadStart(ReadThread));
            t1.Start();
            recData.ValueChange += new Event.tempChange(recDataValueChange);
            sendData.ValueChange += new Event.tempChange(sendDataValueChange);

        }
        private void recDataValueChange(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(() =>
            {
                string data = server1RecData;
                //listBox1.Items.Add(data + "\r\n");
                Log.Information(data);
                listBox1.Text += data + "\r\n";
            }));
        }
        private void sendDataValueChange(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(() =>
            {
                string data = server1SendData;
                // listBox2.Items.Add(data + "\r\n");
                Log.Information(data);
            }));
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            Rack = "001";
            Depth = "02";
            DemoManager tables = new DemoManager();
            tables.InitTable();
            var Db = new DbContext().Db;//获取操作对象
            pictureBox1.Image = Image.FromFile(@"red.png");
            cmbType.Text = "PO";
            rdbRack1.Checked = true;
            IdleTime = new System.Timers.Timer(1000);//实例化Timer类，设置间隔时间为1000毫秒；
            IdleTime.Elapsed += new System.Timers.ElapsedEventHandler(idleFunc);//到达时间的时候执行事件；
            IdleTime.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
            BufferFullTime = new System.Timers.Timer(1000);//实例化Timer类，设置间隔时间为1000毫秒；
            BufferFullTime.Elapsed += new System.Timers.ElapsedEventHandler(bufferFunc);//到达时间的时候执行事件；
            BufferFullTime.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
            UpdateReogrid(1);//更新excel表格
            //写日志模块
            Log.Logger = new LoggerConfiguration().MinimumLevel.Information().WriteTo.Console()
               .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
               .CreateLogger();
        }

        private void ReadThread()
        {

            UserConfig.ReadAppSettings("Crane1Port", out string Port);
            server1 = new SimpleTcpServer().Start(int.Parse(Port));
            server1.DataReceived += (sender, msg) =>
            {
                // msg.Reply(msg.MessageString);   
                server1RecData = msg.MessageString;
                recData.Data = msg.MessageString;
                this.BeginInvoke(new Action(() =>
                {

                    //如果起始和终止符不是<>,那么返回                  
                    if (msg.MessageString.Substring(0, 1) != "<") return;
                    if (msg.MessageString.Substring(msg.MessageString.Length - 1, 1) != ">") return;
                    IdleTimeout = 0;
                    //如果第二个字符是1，那么要返回ACK，刚连上时，Cranebox会先发送CSR                  
                    if (msg.MessageString.Substring(1, 1) == "1")
                    {

                        Telegram.ACK(server1RecData, out server1SendData);
                        server1.Broadcast(server1SendData);
                        Log.Information(server1SendData);
                        listBox1.Text += server1SendData + "\r\n";
                        // sendData.Data = server1SendData;//触发委托监视事件,异步委托数据有可能会变化

                    }
                    if (msg.MessageString.Substring(18, 3) == "CSR")
                    {

                        if (msg.MessageString.Substring(48, 3) != "000")//如果返回错误
                        {
                            string[] craneData = new string[17];
                            craneData[0] = "Crane1";
                            craneData[1] = SequenceNumber;
                            craneData[2] = (int.Parse(AssignmentID) - 1).ToString("00000000");
                            Telegram.DER1(craneData, out server1SendData);
                            server1.Broadcast(server1SendData);
                            Log.Information(server1SendData);
                            listBox1.Text += server1SendData + "\r\n";
                            // sendData.Data = server1SendData;//触发委托监视事件
                            changeNumber(ref SequenceNumber, ref AssignmentID);
                        }
                        // if (!Server1SendData.Contains("ARQ")) return;



                    }


                    //如果是DUM，那么要返回DUA ，进行心跳
                    if (msg.MessageString.Substring(18, 3) == "DUM")
                    {
                        Telegram.DUA(server1RecData, out server1SendData);
                        server1.Broadcast(server1SendData);
                        //  msg.Reply(server1SendData);
                        Log.Information(server1SendData);
                        listBox1.Text += server1SendData + "\r\n";


                    }

                    //如果是NCK1 ，那么要发送SYN复位
                    //如果是NCK2，那么要等待Buffer timeout 时间重新发
                    if (msg.MessageString.Substring(18, 3) == "NCK")
                    {
                        if (msg.MessageString.Substring(21, 1) == "1")
                        {
                            SequenceNumber = "0001";
                            Telegram.SYN(server1RecData, out server1SendData);
                            server1.Broadcast(server1SendData);
                            Log.Information(server1SendData);
                            listBox1.Text += server1SendData + "\r\n";
                        }
                        if (msg.MessageString.Substring(21, 1) == "2")
                        {
                            BufferFullTime.Start();
                        }
                    }

                    if (msg.MessageString.Substring(18, 3) == "ACP")
                    {
                        if (msg.MessageString.Substring(51, 3) != "000")
                        {
                            string[] craneData = new string[17];
                            craneData[0] = "Crane1";
                            craneData[1] = SequenceNumber;
                            Telegram.STA1(craneData, out server1SendData);
                            server1.Broadcast(server1SendData);
                            Log.Information(server1SendData);
                            listBox1.Text += server1SendData + "\r\n";
                            changeNumber(ref SequenceNumber, ref AssignmentID);

                        }
                        else
                        {
                            listBox1.Text += "任务已完成！" + "\r\n";

                        }

                    }


                }));



            };
            server1.ClientConnected += (sender, msg) =>
            {

                pictureBox1.Image = Image.FromFile(@"green.png");

            };
            server1.ClientDisconnected += (sender, msg) =>
            {

                pictureBox1.Image = Image.FromFile(@"red.png");
            };
        }



        // 连接存在没有接收到报文，60s后，发送心跳，Client 先发送DUM
        public void idleFunc(object source, System.Timers.ElapsedEventArgs e)

        {

            IdleTimeout += 1;

        }
        public void bufferFunc(object source, System.Timers.ElapsedEventArgs e)

        {
            //如果缓冲区NCK，那么等待10s钟后重新发送报文
            BufferFullTimeout += 1;
            if (BufferFullTimeout >= 10)
            {
                BufferFullTimeout = 0;
                server1.Broadcast(server1SendData);
                sendData.Data = server1SendData;//触发委托监视事件
                BufferFullTime.Stop();
            }

        }

        private void 托盘数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataShow frm = new DataShow();
            frm.Show();
        }
        private void reoGridControl1_MouseClick(object sender, MouseEventArgs e)
        {
            //reoGridControl1.Load(@"C: \Users\lifl2.CN\Desktop\xlwingsdemo.xlsx");


            var worksheet = reoGridControl1.CurrentWorksheet;//获取当前工作表
            worksheet.CellMouseUp += Grid_CellMouseUp;
            if (e.Button != MouseButtons.Right) return;
            contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);

            // var cell = worksheet.Cells[0, 0]; //存取单元数据
            // var data = worksheet.GetColumnHeader(5);

            //  cell.Style.BackColor = Color.Tomato;
        }

        private void Grid_CellMouseUp(object sender, CellMouseEventArgs e)
        {

            Position = e.CellPosition.ToString();
            var column = e.CellPosition.ToString().Substring(0, 1);
            var row = e.CellPosition.ToString().Substring(1, 1);
            byte[] array = new byte[2]; //定义一组bai数组array
            array = System.Text.Encoding.ASCII.GetBytes(column); //将ABCD 转成对应1、2、3、4
            int asciicode = (short)(array[0]) - 64;
            Row = int.Parse(row).ToString("000");
            Column = asciicode.ToString("00"); //将转zhi换一的ASCII码转换成string型

        }



        private void telegramARQ(string[] cranedata)
        {

            Telegram.ARQ1(cranedata, out server1SendData);
            server1.Broadcast(server1SendData);
            Log.Information(server1SendData);
            changeNumber(ref SequenceNumber, ref AssignmentID);


        }

        private void 进货到货架ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Column == "") return;
            if (Row == "") return;
            var Db = new DbContext().Db;//获取操作对象
                                        //  AssignmentModel assignment = new AssignmentModel();
            string ID = ID = Rack + Row + Column + Depth;
            var getByPrimaryKey = Db.Queryable<AssignmentModel>().InSingle(ID);
            string[] craneData = new string[17];
            craneData[0] = "Crane1";
            craneData[1] = SequenceNumber;
            craneData[2] = AssignmentID;
            craneData[3] = "CM";
            craneData[4] = "01";
            craneData[5] = "001";//Rack1
            craneData[8] = "01";// 起点单双深
            craneData[9] = Rack;
            craneData[12] = Depth;// 终点单双深
            craneData[6] = "901";
            craneData[7] = "01";
            craneData[10] = Row;
            craneData[11] = Column;
            telegramARQ(craneData);

            //更新状态到excel表
            var worksheet = reoGridControl1.CurrentWorksheet;//获取当前工作表    
            worksheet[Position] = "Load";
            UpdateSqlData();
        }

        private void 出货到站台ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Column == "") return;
            if (Row == "") return;
            string[] craneData = new string[17];
            craneData[0] = "Crane1";
            craneData[1] = SequenceNumber;
            craneData[2] = AssignmentID;
            craneData[3] = "CM";
            craneData[4] = "01";
            craneData[5] = Rack;//Rack1
            craneData[8] = Depth;// 起点单双深
            craneData[9] = "002";
            craneData[12] = "01";// 终点单双深            
            craneData[6] = Row;
            craneData[7] = Column;
            // craneData[8] = tbDepth1.Text.ToString();
            craneData[10] = "902";
            craneData[11] = "01";
            telegramARQ(craneData);
            var worksheet = reoGridControl1.CurrentWorksheet;//获取当前工作表          
            worksheet[Position] = "";
            UpdateSqlData();
        }
        private void 定位ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Column == "") return;
            if (Row == "") return;
            string[] craneData = new string[17];
            craneData[0] = "Crane1";
            craneData[1] = SequenceNumber;
            craneData[2] = AssignmentID;
            craneData[3] = "PO";
            craneData[4] = "01";
            craneData[5] = Rack;//Rack1
            craneData[8] = Depth;// 起点单双深
            craneData[9] = Rack;
            craneData[12] = Depth;// 终点单双深       

            craneData[6] = Row;
            craneData[7] = Column;
            // craneData[8] = tbDepth1.Text.ToString();

            craneData[10] = Row;
            craneData[11] = Column;
            telegramARQ(craneData);


        }
        private void 取货ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Column == "") return;
            if (Row == "") return;
            string[] craneData = new string[17];
            craneData[0] = "Crane1";
            craneData[1] = SequenceNumber;
            craneData[2] = AssignmentID;
            craneData[3] = "PI";
            craneData[4] = "01";
            craneData[5] = Rack;//Rack1
            craneData[8] = Depth;// 起点单双深
            craneData[9] = Rack;
            craneData[12] = Depth;// 终点单双深       

            craneData[6] = Row;
            craneData[7] = Column;
            // craneData[8] = tbDepth1.Text.ToString();

            craneData[10] = Row;
            craneData[11] = Column;
            telegramARQ(craneData);
            var worksheet = reoGridControl1.CurrentWorksheet;//获取当前工作表       
            worksheet[Position] = "";
            UpdateSqlData();
        }
        private void 放货ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Column == "") return;
            if (Row == "") return;
            string[] craneData = new string[17];
            craneData[0] = "Crane1";
            craneData[1] = SequenceNumber;
            craneData[2] = AssignmentID;
            craneData[3] = "DE";
            craneData[4] = "01";
            craneData[5] = Rack;//Rack1
            craneData[8] = Depth;// 起点单双深
            craneData[9] = Rack;
            craneData[12] = Depth;// 终点单双深            
            craneData[6] = Row;
            craneData[7] = Column;
            craneData[10] = Row;
            craneData[11] = Column;
            telegramARQ(craneData);
            var worksheet = reoGridControl1.CurrentWorksheet;//获取当前工作表 
            worksheet[Position] = "Load";
            UpdateSqlData();
        }


        private void UpdateReogrid(int data)
        {

        }

        private void rdbRack1_CheckedChanged(object sender, EventArgs e)
        {
            Rack = "001";
            Depth = "02";
            UpdateExcel(Rack, Depth, "Load");

        }

        private void rdbRack2_CheckedChanged(object sender, EventArgs e)
        {
            Rack = "001";
            Depth = "01";
            UpdateExcel(Rack, Depth, "Load");
        }

        private void rdbRack3_CheckedChanged(object sender, EventArgs e)
        {
            Rack = "002";
            Depth = "01";
            UpdateExcel(Rack, Depth, "Load");
        }

        private void rdbRack4_CheckedChanged(object sender, EventArgs e)
        {
            Rack = "002";
            Depth = "02";
            UpdateExcel(Rack, Depth, "Load");
        }
        private void UpdateExcel(string rack, string depth, string data)
        {
            var worksheet = reoGridControl1.CurrentWorksheet;//获取当前工作表 
            var Db = new DbContext().Db;
            for (int i = 0; i < 26; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    string ID = Rack + (i + 1).ToString("000") + (j + 1).ToString("00") + Depth;
                    var isAny = Db.Queryable<AssignmentModel>().Where(it => it.ID == ID).Any();
                    if (isAny)
                    {
                        var getPrimaryValue = Db.Queryable<AssignmentModel>().Where(it => it.ID == ID).First();
                        worksheet[i, j] = getPrimaryValue.Name;
                    }
                    else
                    {
                        worksheet[i, j] = "";
                    }
                }
            }
            worksheet[0, 1] = data;
        }
        private void UpdateSqlData()//手动状态下更新数据库
        {
            var Db = new DbContext().Db;//获取操作对象
            AssignmentModel assignment = new AssignmentModel();
            assignment.ID = Rack + Row + Column + Depth;
            assignment.Rack = Rack;
            assignment.X_Num = Row;
            assignment.Y_Num = Column;
            assignment.Depth = Depth;
            assignment.Status = "OK";
            var worksheet = reoGridControl1.CurrentWorksheet;//获取当前工作表         
            if (worksheet[Position] != null)
            {
                assignment.Name = worksheet[Position].ToString();
                assignment.Code = "";
                assignment.CreateTime = DateTime.Now;
                Db.Saveable<AssignmentModel>(assignment).ExecuteReturnEntity();
            }
            else
            {
                // var t=Db.Deleteable<AssignmentModel>.In(assignment.ID).ExecuteCommand();
            }

        }
        //单元格执行编辑完毕
        private void reoGridControl1_ActionPerformed(object sender, WorkbookActionEventArgs e)
        {
            UpdateSqlData();

        }

        private void cmbType_SelectedValueChanged(object sender, EventArgs e)
        {
            bool tag1 = false, tag2 = false;
            if (cmbType.Text == "PO")
            {
                tag1 = true;
                tag2 = false;
            }
            if (cmbType.Text == "PI")
            {
                tag1 = true;
                tag2 = false;
            }
            if (cmbType.Text == "DE")
            {
                tag1 = false;
                tag2 = true;
            }
            if (cmbType.Text == "CM")
            {
                tag1 = true;
                tag2 = true;
            }
            tbRack1.Enabled = tag1;
            tbX_Number1.Enabled = tag1;
            tbY_Number1.Enabled = tag1;
            tbDepth1.Enabled = tag1;
            tbRack2.Enabled = tag2;
            tbX_Number2.Enabled = tag2;
            tbY_Number2.Enabled = tag2;
            tbDepth2.Enabled = tag2;
        }

        private void btRuku_Click(object sender, EventArgs e)
        {


            if (tbRuku.Text == "") return;
            if (InsertPallet(2))
            {
                MessageBox.Show("双深托盘入库成功");
            }
            else
            {
                if (InsertPallet(1))
                {
                    MessageBox.Show("单深托盘入库成功");
                }
                else
                {
                    MessageBox.Show("库位已满!");
                }
            }

        }
        private void btChuku_Click(object sender, EventArgs e)
        {
            if (tbChuku.Text == "") return;
            if (DelletPallet(2))
            {
                MessageBox.Show("双深托盘出库成功");
            }
            else
            {
                if (DelletPallet(1))
                {
                    MessageBox.Show("单深托盘出库成功");
                }
                else
                {
                    MessageBox.Show("库位已空!");
                }
            }

        }

        private void btChaxun_Click(object sender, EventArgs e)
        {

        }

        private bool InsertPallet(int depth)
        {
            var Db = new DbContext().Db;
            string ID;
            for (int rack = 1; rack < 3; rack++)//Rack
            {
                for (int row = 1; row < 27; row++)
                {
                    for (int column = 1; column < 5; column++)
                    {
                        ID = rack.ToString("000") + row.ToString("000") + column.ToString("00") + depth.ToString("00");
                        var isAny = Db.Queryable<AssignmentModel>().Where(it => it.ID == ID).Any();
                        if (isAny) continue;
                        else
                        {
                            string[] craneData = new string[17];
                            craneData[0] = "Crane1";
                            craneData[1] = SequenceNumber;
                            craneData[2] = AssignmentID;
                            craneData[3] = "CM";
                            craneData[4] = "01";
                            craneData[5] = "001";//Rack1
                            craneData[8] = "01";// 起点单双深
                            craneData[9] = rack.ToString("000");
                            craneData[12] = depth.ToString("00");// 终点单双深
                            craneData[6] = "901";//起点X
                            craneData[7] = "01";//起点Y
                            craneData[10] = row.ToString("000");//终点X
                            craneData[11] = column.ToString("00");//终点Y
                            telegramARQ(craneData);
                            AssignmentModel model = new AssignmentModel();
                            model.ID = ID;
                            model.Rack = rack.ToString("000");
                            model.X_Num = row.ToString("000");
                            model.Y_Num = column.ToString("00");
                            model.Depth = depth.ToString("00");
                            model.Status = "OK";
                            model.Name = tbRuku.Text;
                            model.Code = "";
                            model.CreateTime = DateTime.Now;
                            var t2 = Db.Insertable(model).ExecuteCommand();
                            UpdateExcel(model.X_Num, model.Y_Num, tbRuku.Text);
                            return true;
                        }
                    }
                }

            }
            return false;

        }
        private bool DelletPallet(int depth)
        {
            var Db = new DbContext().Db;
            string ID;
            for (int rack = 1; rack < 3; rack++)//Rack
            {
                for (int row = 1; row < 27; row++)
                {
                    for (int column = 1; column < 5; column++)
                    {
                        ID = rack.ToString("000") + row.ToString("000") + column.ToString("00") + depth.ToString("00");
                        var isAny = Db.Queryable<AssignmentModel>().Where(it => it.ID == ID).Any();
                        if (!isAny) continue;
                        else
                        {
                            string[] craneData = new string[17];

                            craneData[0] = "Crane1";
                            craneData[1] = SequenceNumber;
                            craneData[2] = AssignmentID;
                            craneData[3] = "CM";
                            craneData[4] = "01";
                            craneData[5] = rack.ToString("001");//Rack1
                            craneData[8] = depth.ToString("00");// 起点单双深
                            craneData[9] = "002";
                            craneData[12] = "01";// 终点单双深            
                            craneData[6] = row.ToString("000");
                            craneData[7] = column.ToString("00");
                            // craneData[8] = tbDepth1.Text.ToString();
                            craneData[10] = "902";
                            craneData[11] = "01";
                            telegramARQ(craneData);
                            var t3 = Db.Deleteable<AssignmentModel>().In(ID).ExecuteCommand();
                            UpdateExcel(row.ToString("000"), column.ToString("00"), tbChuku.Text);
                            return true;
                        }
                    }
                }

            }
            return false;

        }

        private void btSend_Click(object sender, EventArgs e)
        {
            //craneData[0] crane number: Crane1 //craneData[1] sequence number: 0001
            //craneData[2]  assignment id :00000000, //craneData[3] assignmetn type: PO
            //  //craneData[4] TU type :01
            //craneData[5] start RRR 货架编号//craneData[6] start sss X编号, 
            //craneData[7] start HH Y编号//craneData[8] start DD 单双深, 
            //craneData[9] end RRR 货架编号//craneData[10] end sss X编号, 
            //craneData[11] end HH Y编号//craneData[12] end DD 单双深, 
            //craneData[13] fork RE//craneData[14] Speed HI, 
            //craneData[15] Rear Fork Side FU//craneData[16] front fork side FU, 

            string[] craneData = new string[17];
            craneData[0] = "Crane1";
            craneData[1] = SequenceNumber;
            craneData[2] = AssignmentID;
            craneData[3] = cmbType.Text.ToString();
            craneData[4] = "01";
            craneData[5] = tbRack1.Text.ToString();
            craneData[6] = tbX_Number1.Text.ToString();
            craneData[7] = tbY_Number1.Text.ToString();
            craneData[8] = tbDepth1.Text.ToString();
            craneData[9] = tbRack2.Text.ToString();
            craneData[10] = tbX_Number2.Text.ToString();
            craneData[11] = tbY_Number2.Text.ToString();
            craneData[12] = tbDepth2.Text.ToString();
            Telegram.ARQ1(craneData, out server1SendData);
            //  listBox2.Items.Add(server1SendData);
            server1.Broadcast(server1SendData);
            sendData.Data = server1SendData;//触发委托监视事件
            changeNumber(ref SequenceNumber, ref AssignmentID);
            listBox1.Text = server1SendData + "\r\n";


        }

        private void btSYN_Click(object sender, EventArgs e)
        {

            Telegram.SYN(server1RecData, out server1SendData);

            server1.Broadcast(server1SendData);
            Log.Information(server1SendData);
            listBox1.Text += server1SendData + "\r\n";
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Log.CloseAndFlush();//格式化
        }
        private void changeNumber(ref string sequence, ref string assignment)
        {
            sequence = (int.Parse(sequence) + 1).ToString("0000");
            if (int.Parse(sequence) >= 9999) sequence = "0001";
            assignment = (int.Parse(assignment) + 1).ToString("00000000");
            if (int.Parse(assignment) >= 99999999) assignment = "00000001";
        }





    }

    public class Event//监听字符串变化
    {
        public delegate void tempChange(object sender, EventArgs e);
        public event tempChange ValueChange;
        string data;
        public string Data
        {
            get
            {
                return data;
            }
            set
            {
                if (data != value)
                {
                    ValueChange(this, new EventArgs());
                }
                data = value;

            }
        }
    }


}


