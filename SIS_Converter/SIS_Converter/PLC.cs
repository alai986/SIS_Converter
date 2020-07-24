using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sharp7;

namespace SIS_Converter
{
    public class PLC
    {
        private byte[] Buffer = new byte[65536];
        // private byte[] Temp = new byte[65536];      
        private S7Client Client;     
        int[] Area =
           {
                 S7Consts.S7AreaPE,
                 S7Consts.S7AreaPA,
                 S7Consts.S7AreaMK,
                 S7Consts.S7AreaDB,
                 S7Consts.S7AreaCT,
                 S7Consts.S7AreaTM
            };
        int[] WordLen =
        {
                 S7Consts.S7WLBit,
                 S7Consts.S7WLByte,
                 S7Consts.S7WLChar,
                 S7Consts.S7WLWord,
                 S7Consts.S7WLInt,
                 S7Consts.S7WLDWord,
                 S7Consts.S7WLDInt,
                 S7Consts.S7WLReal,
                 S7Consts.S7WLCounter,
                 S7Consts.S7WLTimer
            };
      
        public void ReadData(byte type, byte datatype, int DBNumber, int Start, int Length, bool showMsg,ref byte[] ReadData)//读取浮点数
        {
            //ReadData = new PLC().Read(0x84, 16, 0, 3, "byte");
            // MessageBox.Show("nihao");
            Client = new S7Client();           
            UserConfig.ReadAppSettings("IP", out string IP);                        
            try

            {
                int Result;
                int num = 0;
                while (true)
                {
                    
                    int SizeRead = 0;                   
                    //Result = Client.ReadArea(S7Consts.S7AreaDB, 500, Begin, Length, S7Consts.S7WLReal, Buffer, ref SizeRead);

                    Result = Client.ReadArea(type, DBNumber, Start, Length, datatype, Buffer, ref SizeRead);

                    if (Result == 0)
                    {
                        for (int i = 0; i < SizeRead; i++)
                        {
                            ReadData[i] = Buffer[i];

                        }
                        if(showMsg) MessageBox.Show("读取成功！");
                        break;
                    }
                    else
                    {
                        Client.Disconnect();
                        Thread.Sleep(200);
                        Result = Client.ConnectTo(IP, 0, 0);
                        num++;
                        if (num > 5)
                        {
                           if(showMsg) MessageBox.Show("读取失败");
                           break;
                        }
                    }                   
                }
            }
            catch (Exception ex)
            {
                if(showMsg) MessageBox.Show(ex.ToString()+ "读取失败");

            }
        }
        public void WriteData(byte type, byte datatype, int DBNumber, int Start, int Length,ref byte []WriteData)// 主进程用
        {
            int Result;
            int SizeRead = 0;
            Client = new S7Client();
            // UserConfig 是配置config.cs 里面的读取函数
            UserConfig.ReadAppSettings("IP", out string IP);
            {
                Result = Client.ConnectTo(IP, 0, 0);
                try
                {
                    // Result = Client.WriteArea(S7Consts.S7AreaDB, address, start, lenth, S7Consts.S7WLReal, WriteData, ref SizeRead);
                    Result = Client.WriteArea(type, DBNumber, Start, Length, datatype, WriteData, ref SizeRead);

                    if (Result == 0) MessageBox.Show("写入成功");
                    else MessageBox.Show(Client.ErrorText(Result));

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    //TxtDump.Text = "ok2";
                }

            }
        }
    }
}
