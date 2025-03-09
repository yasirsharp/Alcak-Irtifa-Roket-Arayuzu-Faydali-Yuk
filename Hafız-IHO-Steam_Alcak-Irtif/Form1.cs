using GMap.NET.MapProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hafız_IHO_Steam_Alcak_Irtif
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Unnamed Variables
        string dataIn;
        string LogFilePath = "../../../Logs/Data-Log.txt";
        string errLogFilePath = "../../../Logs/Error-Log.txt";

        sbyte index_of_enlem, index_of_boylam,index_of_gps_err;
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] portNames = SerialPort.GetPortNames();
            ComPortBox.Items.AddRange(portNames);
            ComPortBox.Text = portNames[0];
            map.MapProvider = GMapProviders.GoogleMap;
            //map.DragButton = MouseButtons.Left;
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        #region Com Port Open
        private void ComPortOpenButton_Click_1(object sender, EventArgs e)
        {
            ComPortOpen();
        }

        private void ComPortOpen()
        {
            try
            {
                seriP1.PortName = ComPortBox.Text;
                seriP1.Open();
                ComPortStatus.Value = 100;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Bir hatanız var!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Com Port Close
        private void ComPortCloseButton_Click(object sender, EventArgs e)
        {
            ComPortClose();
        }

        private void ComPortClose()
        {
            if (seriP1.IsOpen){
                seriP1.Close();
            }
            else
            {
                MessageBox.Show("Seri port zaten kapalı.", "Seri Port", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            ComPortStatus.Value = 0;
        }
        #endregion

        #region Data in, print and processing

        #region Veri alma
        private void seriP1_DataReceived(object sender, SerialDataReceivedEventArgs e) {
            dataIn = seriP1.ReadExisting(); //Örnek gelen veri - Q40,8401W31,1512Z

            #region index of ...
            index_of_enlem = Convert.ToSByte(dataIn.IndexOf('Q'));
            index_of_boylam = Convert.ToSByte(dataIn.IndexOf('W'));
            index_of_gps_err = Convert.ToSByte(dataIn.IndexOf('Z'));
            #endregion

            DataProcessing();
            LogData();
            this.Invoke(new EventHandler(ShowData));
        }
        #endregion

        #region Veri ekrana yazdırma
        private void ShowData(object sender, EventArgs e) {

            #region İrtifa
            //try
            //{
            //    if (index_of_enlem <= 0 && index_of_irtifa >= 0)
            //    {
            //        irtifa = dataIn.Substring(index_of_irtifa + 1);
            //        lblIrtifa.Text = $"İrtifa: {irtifa}";
            //    }
            //    if (index_of_enlem - index_of_irtifa > 7)
            //    {
            //        irtifa = dataIn.Substring(index_of_irtifa + 1, 4);
            //        lblIrtifa.Text = $"İrtifa: {irtifa}";
            //    }
            //    if (index_of_irtifa >= 0)
            //    {
            //        irtifa = dataIn.Substring(index_of_irtifa + 1, (index_of_enlem - index_of_irtifa) - 1);
            //        lblIrtifa.Text = $"İrtifa: {irtifa}";
            //    }
            //}
            //catch (Exception err)
            //{
            //    File.AppendAllText(errLogFilePath, $"İrtifa - {err.Message}  |  {DateTime.Now} \n");
            //}
            #endregion
        }
        #endregion

        #region Veri işleme
        private void DataProcessing() {

            #region GMap
			try
            {
				if (index_of_enlem >= 0 && index_of_boylam >= 0) { 
					lblenlem.Text = dataIn.Substring(index_of_enlem + 1, (index_of_boylam - index_of_enlem) - 1);
					lblboylam.Text = dataIn.Substring(index_of_boylam + 1, (index_of_boylam - index_of_enlem) - 1);
					lblKonum.Text = $"Konum: {lblenlem.Text}/{lblboylam.Text}";
				}
				if (lblenlem.Text != "Enlem" && lblboylam.Text != "Boylam") {
					map.MinZoom = 10;
					map.MaxZoom = 1000;
					map.Zoom = 15;
					map.Position = new GMap.NET.PointLatLng(
                        Convert.ToDouble(lblenlem.Text.Replace('.',',')), 
                        Convert.ToDouble(lblboylam.Text.Replace('.', ',')));
                    label2.Text = map.Position.ToString();
				}
			}
			catch(Exception err){
                File.AppendAllText(errLogFilePath, $"GMap.Net - {err.Message}  |  {DateTime.Now} \n");
			}
            #endregion

            #region gps error
            if (index_of_gps_err >= 0)
            {
                btnGpsErr.BackColor = Color.DarkOrange;
                btnGpsErr.Text = "GPS Bağlanmadı";
            }
            #endregion

        }
        #endregion

        #region Veri kayıt etme
        private void LogData() {
            try {
                File.AppendAllText(LogFilePath, $"{dataIn}  |  {DateTime.Now} \n");
            }
            catch (Exception err) {
                File.AppendAllText(errLogFilePath, $"Log error - {err.Message} | {DateTime.Now} \n");
            }
        }
        #endregion

        #endregion
    }
}