using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIS_Converter
{
     
    public static class Telegram
    {
     
       
        public static void ACK( string telegram, out string command)
        {
            //UserConfig.ReadAppSettings(craneNumber+"Destination", out string destination);
            //UserConfig.ReadAppSettings(craneNumber+"Source", out string source);
            //UserConfig.ReadAppSettings(craneNumber + "Sequence", out string sequence);
            string STX = "<";
            string ConfirmFlag = "0";//0 unconfirmed;
            string SequenceNumber = "0000";//0000 unconfirmed;0001-9999 confirmed;0001-9999 confirmed;   
            string Destination = telegram.Substring(12, 6);
            string Source = telegram.Substring(6, 6);
            string Command = "ACK0" + telegram.Substring(2, 4);
            string CRC = "00";
            string ETX = ">";
            command = STX + ConfirmFlag + SequenceNumber + Destination + Source + Command + CRC + ETX;

        }
        public static void DUA( string telegram, out string command)
        {
            string STX = "<";
            string ConfirmFlag = "0";//0 unconfirmed;
            string SequenceNumber = "0000";//0000 unconfirmed;0001-9999 confirmed;0001-9999 confirmed;   
            string Destination = telegram.Substring(12, 6);
            string Source = telegram.Substring(6, 6);
            string Command = "DUA00000";
            string CRC = "00";
            string ETX = ">";
            command = STX + ConfirmFlag + SequenceNumber + Destination + Source + Command + CRC + ETX;
        }
        public static void ARQ1(string[] craneData, out string command)
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
            UserConfig.ReadAppSettings(craneData[0]+"Destination", out string destination);
            UserConfig.ReadAppSettings(craneData[0]+"Source", out string source);
            string STX = "<";
            string ConfirmFlag = "1";//0 unconfirmed;           
            string SequenceNumber = craneData[1];//0000 unconfirmed;0001-9999 confirmed;   
            string Destination = destination;
            string Source = source;
            string Command = "ARQ" + "01" + craneData[2];//(ARQ01 01 是指堆垛机编号)
            string AssigmentType = craneData[3];//任务类型
            string TUType = craneData[4];
            string MM = destination.Substring(4, 2);//CraneBox编号(CRAN30是指Cranebox叫30)         
            string StartPosition = MM + craneData[5] + craneData[6] + craneData[7] + craneData[8];
            string DestinationPosition = MM +craneData[9] +craneData[10] +craneData[11] +craneData[12];
            string Fork = "RE";//前后叉
            string Speed = "HI";//速度
            string RearForkSide = "FU";
            string FrontForkSide = "FU";
            string CRC = "00";
            string ETX = ">";
            command = STX + ConfirmFlag + SequenceNumber + Destination + Source + Command + AssigmentType + TUType + StartPosition + DestinationPosition + Fork + Speed + RearForkSide + FrontForkSide + CRC + ETX;
        }
        public static void SYN(string telegram,out string command)
        {
            string STX = "<";
            string ConfirmFlag = "0";//0 unconfirmed;
            string SequenceNumber = "0000";//0000 unconfirmed;0001-9999 confirmed;0001-9999 confirmed;   
            string Destination = telegram.Substring(12, 6);;
            string Source = telegram.Substring(6, 6);
            string Command = "SYN00000";
            string CRC = "00";
            string ETX = ">";
            command = STX + ConfirmFlag + SequenceNumber + Destination + Source + Command + CRC + ETX;
        }
        public static void DER1(string[] craneData, out string command)
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
            UserConfig.ReadAppSettings(craneData[0] + "Destination", out string destination);
            UserConfig.ReadAppSettings(craneData[0] + "Source", out string source);
            string STX = "<";
            string ConfirmFlag = "1";//0 unconfirmed;           
            string SequenceNumber = craneData[1];//0000 unconfirmed;0001-9999 confirmed;   
            string Destination = destination;
            string Source = source;
            string Command = "DER" + "01" ;//(ARQ01 01 是指堆垛机编号)
            string AssigmentID = craneData[2];//任务类型          
            string CRC = "00";
            string ETX = ">";
            command = STX + ConfirmFlag + SequenceNumber + Destination + Source + Command + AssigmentID  + CRC + ETX;
        }

        public static void STA1(string[] craneData, out string command)
        {
    
            UserConfig.ReadAppSettings(craneData[0] + "Destination", out string destination);
            UserConfig.ReadAppSettings(craneData[0] + "Source", out string source);
            string STX = "<";
            string ConfirmFlag = "1";//0 unconfirmed;           
            string SequenceNumber = craneData[1];//0000 unconfirmed;0001-9999 confirmed;   
            string Destination = destination;
            string Source = source;
            string Command = "STA" + "01" ;//(ARQ01 01 是指堆垛机编号)                    
            string CRC = "00";
            string ETX = ">";
            command = STX + ConfirmFlag + SequenceNumber + Destination + Source + Command + CRC + ETX;
        }


    }
}
