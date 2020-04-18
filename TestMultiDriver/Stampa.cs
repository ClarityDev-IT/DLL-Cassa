using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.PointOfService;
using Microsoft.PointOfService.BaseServiceObjects;
using System.IO;

using System.Runtime.InteropServices;

namespace TestMultiDriver
{
    [ComVisible(true)]

    [ClassInterface(ClassInterfaceType.AutoDual)]

    [ProgId("TestMultiDriver.Stampa")]
    public  class Stampa : IDisposable
    {
        // Pointer to an external unmanaged resource.
        private IntPtr handle;
        // Other managed resource this class uses.
        private Component component = new Component();
        // Track whether Dispose has been called.
        private bool disposed = false;

        Microsoft.PointOfService.RCHGROUP.UPOS.MULTIFiscalPrinter MySerialTest;
        Microsoft.PointOfService.RCHGROUP.UPOS.MULTIFiscalPrinter _MySerialTest;
        DirectIOData IOUserData;
        delegate void updateFormDelegate(string newText);  //Per gestione di operazioni cross-thread
        private delegate void ControlUpdater(System.Windows.Forms.ToolStripLabel crtl, string text, Color MyColor);
        private delegate void DisplayFWVersion();
        private bool ISConnected = false; //Solo per connessioni LAN
        private int TipoLettura = 1;
        public Stampa()
        {
           
            _MySerialTest = new Microsoft.PointOfService.RCHGROUP.UPOS.MULTIFiscalPrinter();
            _MySerialTest.DirectIOEvent += new DirectIOEventHandler(RispondiAEvento);
            _MySerialTest.StatusUpdateEvent += new StatusUpdateEventHandler(RispondiAEventoStaus);


            MySerialTest = new Microsoft.PointOfService.RCHGROUP.UPOS.MULTIFiscalPrinter();
            MySerialTest.DirectIOEvent += new DirectIOEventHandler(RispondiAEvento);
            MySerialTest.StatusUpdateEvent += new StatusUpdateEventHandler(RispondiAEventoStausUpdate);



            MySerialTest.ErrorEvent += new DeviceErrorEventHandler(RispondiAErrorEvent);
            MySerialTest.TimeOutEvent += new Microsoft.PointOfService.RCHGROUP.UPOS.MULTIFiscalPrinter.TimeOUTEventHandler(RispondiATimeOutEvent);
            MySerialTest.LANSocketEvent += new Microsoft.PointOfService.RCHGROUP.UPOS.MULTIFiscalPrinter.LANSocketEventHandler(RispondiALANSocketEvent);

            //cmBOXModels.Items.Add(Microsoft.PointOfService.RCHGROUP.UPOS.MULTIFiscalPrinter.CurrentECRModels.PRINTF.ToString());
            //cmBOXModels.Items.Add(Microsoft.PointOfService.RCHGROUP.UPOS.MULTIFiscalPrinter.CurrentECRModels.GLOBE.ToString());            
            //cmBOXModels.Items.Add(Microsoft.PointOfService.RCHSPA.UPOS.MULTIFiscalPrinter.CurrentECRModels.ONDA.ToString());
            //cmBOXModels.Items.Add(Microsoft.PointOfService.RCHSPA.UPOS.MULTIFiscalPrinter.CurrentECRModels.SWING.ToString());            
            //cmBOXModels.Items.Add(Microsoft.PointOfService.RCHSPA.UPOS.MULTIFiscalPrinter.CurrentECRModels.ISWING.ToString());
            //cmBOXModels.Items.Add("PRINTF");
            //radBut_Serial.Checked = true;
            //radBut_USB.Enabled = false;

            //Enable SetEvent on GE/MF DUMP only on Form Application
            MySerialTest.FormMode = true;

            //this.handle = handle;
        }


        // The class constructor.
        //public Stampa(IntPtr handle)
        //{
        //    this.handle = handle;
        //}

        private void RispondiATimeOutEvent(string Messaggio)
        {
            try
            {
                ControlUpdater updater = UpdateControl;
                //Invoke(updater, toolStripStatusLabel1, Messaggio, Color.Red);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void RispondiALANSocketEvent(string Messaggio)
        {
            try
            {
                ControlUpdater updater = UpdateControl;
               // Invoke(updater, toolStripStatusLabel1, Messaggio, Color.Green);
                ISConnected = true;
            }
            catch (System.Exception ex)
            {
                ISConnected = false;
                MessageBox.Show(ex.ToString());
            }
        }


        private void RispondiAEvento(object o, DirectIOEventArgs e)
        {
            try
            {
                string risposta = "";
                risposta = e.Object.ToString();
                updateForm(risposta);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void RispondiAEventoStaus(object o, StatusUpdateEventArgs e)
        {
            try
            {
                string risposta = "";
                risposta = "Status Update: " + e.Status.ToString(); // e.ToString();
                updateForm(risposta);
                switch (e.Status.ToString())
                {
                    case "25":
                        MessageBox.Show("Rotolo Carta Esaurito");
                        break;
                    case "27":
                        MessageBox.Show("Rotolo Carta Ripristinato");
                        break;
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void RispondiAEventoStausUpdate(object o, StatusUpdateEventArgs e)
        {
            try
            {
                string risposta = "";
                risposta = "Status Update: " + e.Status.ToString(); // e.ToString();
                updateForm(risposta);
                switch (e.Status.ToString())
                {
                    case "25":
                        MessageBox.Show("Rotolo Carta Esaurito");
                        break;
                    case "27":
                        MessageBox.Show("Rotolo Carta Ripristinato");
                        break;
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void RispondiAErrorEvent(object o, DeviceErrorEventArgs e)
        {
            //ErrorEvent su gestione asincrona.
            string risposta;
            string rispostaEX;
            risposta = e.ErrorCode.ToString();
            rispostaEX = e.ErrorCodeExtended.ToString();
            updateForm("ErrorEvent: " + risposta + " " + rispostaEX);
        }

        //Per gestione di operazioni cross-thread
        private void updateForm(string newText)
        {
            //if (this.listBox1.InvokeRequired)
            //{
            //    // this is worker thread
            //    updateFormDelegate del = new updateFormDelegate(updateForm);
            //    this.Invoke(del, new object[] { newText });
            //}
            //else
            {
                // this is UI thread

                Byte[] Byte = Encoding.GetEncoding(1252).GetBytes(newText);
                String exAscii = System.Text.Encoding.GetEncoding(936).GetString(Byte, 0, Byte.Length);
                //this.listBox1.Items.Add(exAscii);

                //this.listBox1.Items.Add(newText);
               // this.listBox1.Refresh();
            }
        }

        private void UpdateControl(System.Windows.Forms.ToolStripLabel ctrl, string text, Color Mycolor)
        {
            ctrl.Text = text;
            ctrl.BackColor = Mycolor;
        }

        private void UpdateFWVersion()
        {
            IOUserData = MySerialTest.DirectIO(0, 0, "<</?f");
            string DataFW = IOUserData.Object.ToString();
            DataFW = DataFW.Substring(7, 20);
           // FWVersion.Text = DataFW;
        }

        private void btnClearList_Click(object sender, EventArgs e)
        {
            try
            {
                //listBox1.Items.Clear();
                //this.Refresh();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }

        private void btnSend7_Click()
        {
            try
            {
                for (int i = 1; i <= 1; i++)
                {
                    IOUserData = MySerialTest.DirectIO(0, 0, "=K");
                    IOUserData = MySerialTest.DirectIO(0, 0, "=C1");
                    IOUserData = MySerialTest.DirectIO(0, 0, "=R1/$100/(Descrizione)");
                    //updateForm(IOUserData.Object.ToString());
                    IOUserData = MySerialTest.DirectIO(0, 0, "=R2/$1620/(Descrizione)");
                    //updateForm(IOUserData.Object.ToString());
                    IOUserData = MySerialTest.DirectIO(0, 0, "=R3/$250/(Descrizione)");
                    //updateForm(IOUserData.Object.ToString());
                    IOUserData = MySerialTest.DirectIO(0, 0, "=R5/$120/(Descrizione)");
                    //updateForm(IOUserData.Object.ToString());
                    IOUserData = MySerialTest.DirectIO(0, 0, "=R1/*2/$100/(Descrizione)");
                    //updateForm(IOUserData.Object.ToString());
                    IOUserData = MySerialTest.DirectIO(0, 0, "=R4/$650/(Descrizione)");
                    //updateForm(IOUserData.Object.ToString());
                    IOUserData = MySerialTest.DirectIO(0, 0, "=s");
                    //updateForm(IOUserData.Object.ToString());
                    IOUserData = MySerialTest.DirectIO(0, 0, "=R3/$250/(Descrizione)");
                    //updateForm(IOUserData.Object.ToString());
                    IOUserData = MySerialTest.DirectIO(0, 0, "=R3/$7000/(Descrizione)");
                    //updateForm(IOUserData.Object.ToString());
                    IOUserData = MySerialTest.DirectIO(0, 0, "=S");
                    //updateForm(IOUserData.Object.ToString());
                    IOUserData = MySerialTest.DirectIO(0, 0, "=T");
                    //updateForm(IOUserData.Object.ToString());
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }

        private void btnSend1_Click(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "=R1/$100");
                if (IOUserData.Object.ToString() == "")
                {
                    MessageBox.Show("Comunicazione assente");
                }
                //updateForm(IOUserData.Object.ToString());
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "=a");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "=k");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }

        private void btnSend2_Click(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "=S");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }

        private void btnSend3_Click(object sender, EventArgs e)
        {
            try
            {


                IOUserData = MySerialTest.DirectIO(0, 0, "=T1");

                if (IOUserData.Object.ToString() == "")
                {
                    MessageBox.Show("Comunicazione assente");
                }

                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore A" + ex.ToString());
            }
        }




        private void btnSend4_Click(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "<</?s");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }

        private void btnSend5_Click(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "<</?f");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }

        private void btnSend6_Click_1(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "<</?m");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }


        public string open(string txtBoxIP, string txtBoxPORT)
        {

            try
            {
                MySerialTest.ECRModels = Microsoft.PointOfService.RCHGROUP.UPOS.MULTIFiscalPrinter.CurrentECRModels.PRINTF;
                MySerialTest.IsServer = false;
                MySerialTest.IsLAN = true;
                MySerialTest.OpenIP(txtBoxIP, txtBoxPORT);
                return "OKK";
            }
            catch (System.Exception)
            {
                return ("Errore Apertura Comunicazione");
            }
        }

        public string close()
        {
            try
            {
                MySerialTest.Close();
                ISConnected = false;
            }
            catch (System.Exception ex)
            {
                return ("Errore " + ex.ToString());
            }
            return "OK";
        }

        //private void btnOPEN_Click_1(object sender, EventArgs e)
        //{

        //    try
        //    {
        //        switch (cmBOXModels.Text)
        //        {
        //            case "PRINTF":
        //                MySerialTest.ECRModels = Microsoft.PointOfService.RCHGROUP.UPOS.MULTIFiscalPrinter.CurrentECRModels.PRINTF;
        //                break;
        //            case "ONDA":
        //                MySerialTest.ECRModels = Microsoft.PointOfService.RCHGROUP.UPOS.MULTIFiscalPrinter.CurrentECRModels.ONDA;
        //                break;
        //            case "IGLOBE":
        //                MySerialTest.ECRModels = Microsoft.PointOfService.RCHGROUP.UPOS.MULTIFiscalPrinter.CurrentECRModels.IGLOBE;
        //                break;
        //            default:
        //                MySerialTest.ECRModels = Microsoft.PointOfService.RCHGROUP.UPOS.MULTIFiscalPrinter.CurrentECRModels.PRINTF;
        //                break;
        //        }
        //        if (checkBoxLOG.Checked)
        //            MySerialTest.WriteLOG = true;
        //        else
        //            MySerialTest.WriteLOG = false;

        //        if (chkBoxOldProt.Checked)
        //            MySerialTest.OldProtocol = true;
        //        else
        //            MySerialTest.OldProtocol = false;

        //        if (radBut_LAN.Checked)
        //        {
        //            MySerialTest.IsServer = false;
        //            MySerialTest.IsLAN = true;
        //            MySerialTest.OpenIP(txtBoxIP.Text, txtBoxPORT.Text);
        //            TimerLAN.Enabled = true;
        //        }

        //        if (radBut_USB.Checked)
        //        {
        //            MySerialTest.USBComMode = true;
        //            MySerialTest.Open();
        //            FirstCom();

        //        }

        //        if (radBut_Serial.Checked)
        //        {
        //            MySerialTest.USBComMode = false;
        //            MySerialTest.ECRPortCOM = cmBoxCOM.Text;
        //            MySerialTest.ECRBaudRateCOM = Int32.Parse(cmBOXBaudRate.Text);
        //            MySerialTest.Open();
        //            FirstCom();
        //        }

        //    }
        //    catch (System.Exception)
        //    {
        //        ISConnected = false;
        //        toolStripStatusLabel1.BackColor = Color.Red;
        //        toolStripStatusLabel1.Text = "ECR NOT CONNECTED";
        //        MessageBox.Show("Errore Apertura Comunicazione");
        //    }
        //}

        //private void FirstCom()
        //{
        //    IOUserData = MySerialTest.DirectIO(0, 0, "<</?s");
        //    toolStripStatusLabel1.BackColor = Color.Green;
        //    toolStripStatusLabel1.Text = "ECR CONNECTED";
        //    IOUserData = MySerialTest.DirectIO(0, 0, "<</?f");
        //    string DataFW = IOUserData.Object.ToString();

        //    switch (cmBOXModels.Text)
        //    {
        //        case "PRINTF":
        //            DataFW = DataFW.Substring(7, (DataFW.Length - 10));
        //            break;
        //        case "GLOBE":
        //            DataFW = DataFW.Substring(7, (DataFW.Length - 10));
        //            break;
        //        case "ONDA":
        //            DataFW = DataFW.Substring(7, (DataFW.Length - 10));
        //            break;
        //        case "SWING":
        //            DataFW = DataFW.Substring(7, (DataFW.Length - 10));
        //            break;
        //        case "ISWING":
        //            DataFW = DataFW.Substring(7, (DataFW.Length - 10));
        //            break;
        //        case "IGLOBE":
        //            DataFW = DataFW.Substring(7, (DataFW.Length - 10));
        //            break;
        //        default:
        //            DataFW = DataFW.Substring(7, (DataFW.Length - 10));
        //            break;
        //    }
        //    FWVersion.Text = DataFW;
        //}

        //private void btnCLOSE_Click_1(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        MySerialTest.Close();
        //        toolStripStatusLabel1.BackColor = Color.Red;
        //        toolStripStatusLabel1.Text = "ECR NOT CONNECTED";
        //        FWVersion.Text = "";
        //        ISConnected = false;
        //    }
        //    catch (System.Exception ex)
        //    {
        //        MessageBox.Show("Errore " + ex.ToString());
        //    }
        //}

        private void btnClear_Click_1(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "=K");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }



        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "<</?8");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "<</?7");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }



        private void txtSend_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                try
                {
                    IOUserData = MySerialTest.DirectIO(0, 0, "txtSend.Text");

                    updateForm(IOUserData.Object.ToString());
                    //int i= IOUserData.Data; 
                    //txtSend.Clear();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Errore " + ex.ToString());
                }
            }

        }

        private void btnC1_Click_1(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "=C1");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }

        //private void cmBOXModels_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    switch (cmBOXModels.Text)
        //    {
        //        case "GLOBE":
        //            radBut_Serial.Enabled = true;
        //            radBut_USB.Enabled = false;
        //            radBut_LAN.Enabled = true;
        //            cmBoxCOM.Enabled = true;
        //            cmBOXBaudRate.Enabled = true;
        //            radBut_Serial.Checked = true;
        //            btnRead.Enabled = true;
        //            chkBoxOldProt.Visible = false;
        //            break;
        //        case "ONDA":
        //            radBut_Serial.Enabled = true;
        //            radBut_USB.Enabled = false;
        //            radBut_LAN.Enabled = true;
        //            cmBoxCOM.Enabled = true;
        //            cmBOXBaudRate.Enabled = true;
        //            radBut_Serial.Checked = true;
        //            btnRead.Enabled = false;
        //            chkBoxOldProt.Visible = true;
        //            break;
        //        case "SWING":
        //            radBut_Serial.Enabled = true;
        //            radBut_USB.Enabled = false;
        //            radBut_LAN.Enabled = false;
        //            cmBoxCOM.Enabled = true;
        //            cmBOXBaudRate.Enabled = true;
        //            radBut_Serial.Checked = true;
        //            btnRead.Enabled = true;
        //            chkBoxOldProt.Visible = false;
        //            break;
        //        case "XOVER":
        //            radBut_Serial.Enabled = true;
        //            radBut_USB.Enabled = false;
        //            radBut_LAN.Enabled = true;
        //            cmBoxCOM.Enabled = true;
        //            cmBOXBaudRate.Enabled = true;
        //            radBut_Serial.Checked = true;
        //            btnRead.Enabled = true;
        //            chkBoxOldProt.Visible = false;
        //            break;
        //        case "IGLOBE":
        //            radBut_Serial.Enabled = true;
        //            radBut_USB.Enabled = true;
        //            radBut_LAN.Enabled = true;
        //            cmBoxCOM.Enabled = true;
        //            cmBOXBaudRate.Enabled = true;
        //            radBut_Serial.Checked = true;
        //            btnRead.Enabled = true;
        //            chkBoxOldProt.Visible = false;
        //            break;
        //        default:
        //            chkBoxOldProt.Visible = false;
        //            break;
        //    }

        //}

        //private void radBut_Serial_CheckedChanged(object sender, EventArgs e)
        //{
        //    cmBoxCOM.Enabled = true;
        //    cmBOXBaudRate.Enabled = true;
        //    txtBoxIP.Enabled = false;
        //    txtBoxPORT.Enabled = false;
        //}

        //private void radBut_LAN_CheckedChanged(object sender, EventArgs e)
        //{
        //    cmBoxCOM.Enabled = false;
        //    cmBOXBaudRate.Enabled = false;
        //    txtBoxIP.Enabled = true;
        //    txtBoxPORT.Enabled = true;
        //}

        //private void radBut_LANServer_CheckedChanged(object sender, EventArgs e)
        //{
        //    cmBoxCOM.Enabled = false;
        //    cmBOXBaudRate.Enabled = false;
        //    txtBoxIP.Enabled = false;
        //    txtBoxPORT.Enabled = true;
        //}

        //private void comboBoxLetture_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        TipoLettura = comboBoxLetture.SelectedIndex + 1;
        //        switch (TipoLettura)
        //        {
        //            case 1:
        //                //GE For Data From Sct to SCT
        //                dateTimePickerFrom.Enabled = true;
        //                dateTimePickerTo.Enabled = false;
        //                FromRcpt.Enabled = true;
        //                ToRcpt.Enabled = true;
        //                rBVideo.Enabled = true;
        //                rBVideo.Enabled = true;
        //                rBVideo.Checked = true;
        //                break;
        //            case 2:
        //                //GE From Data to Data
        //                dateTimePickerFrom.Enabled = true;
        //                dateTimePickerTo.Enabled = true;
        //                FromRcpt.Enabled = false;
        //                ToRcpt.Enabled = false;
        //                rBVideo.Enabled = true;
        //                rBVideo.Enabled = true;
        //                rBVideo.Checked = true;
        //                break;
        //            case 3:
        //                //GE Full
        //                dateTimePickerFrom.Enabled = false;
        //                dateTimePickerTo.Enabled = false;
        //                FromRcpt.Enabled = false;
        //                ToRcpt.Enabled = false;
        //                rBVideo.Enabled = true;
        //                rBVideo.Enabled = true;
        //                rBVideo.Checked = true;
        //                break;
        //            case 4:
        //                //MF
        //                rBVideo.Enabled = false;
        //                rBPrint.Checked = true;
        //                break;
        //            case 5:
        //                //MF
        //                rBVideo.Enabled = false;
        //                rBPrint.Checked = true;
        //                break;
        //            case 6:
        //                //MF
        //                rBVideo.Enabled = false;
        //                rBPrint.Checked = true;
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        MessageBox.Show("Errore " + ex.ToString());
        //    }
        //}

        //private void btnRead_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        DateTime DataFrom;
        //        DateTime DataTo;
        //        string strFromData, strToData;
        //        string strFromReceipt, strToReceipt;
        //        //TipoLettura = comboBoxLetture.SelectedIndex + 1;

        //        switch (cmBOXModels.Text)
        //        {
        //            case "IGLOBE":
        //                if (rBVideo.Checked)
        //                {
        //                    IOUserData = MySerialTest.DirectIO(0, 0, "=C4");
        //                    IOUserData = MySerialTest.DirectIO(0, 0, "=C120");
        //                    IOUserData = MySerialTest.DirectIO(0, 0, "=R1");
        //                }
        //                if (rBPrint.Checked)
        //                {
        //                    IOUserData = MySerialTest.DirectIO(0, 0, "=C4");
        //                    IOUserData = MySerialTest.DirectIO(0, 0, "=C120");
        //                    IOUserData = MySerialTest.DirectIO(0, 0, "=R0");
        //                }

        //                switch (TipoLettura)
        //                {
        //                    case 1:
        //                        //GE For Data From Sct to SCT
        //                        DataFrom = dateTimePickerFrom.Value;
        //                        strFromData = "=\"" + DataFrom.ToString("ddMMyy") + "S";
        //                        strFromReceipt = "=\"" + FromRcpt.Value.ToString() + "S";
        //                        strToReceipt = "=\"" + ToRcpt.Value.ToString() + "S";
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C4");
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C452");
        //                        IOUserData = MySerialTest.DirectIO(0, 0, strFromData);
        //                        IOUserData = MySerialTest.DirectIO(0, 0, strFromReceipt);
        //                        IOUserData = MySerialTest.DirectIO(0, 0, strToReceipt);
        //                        break;
        //                    case 2:
        //                        //GE From Data to Data
        //                        DataFrom = dateTimePickerFrom.Value;
        //                        strFromData = "=\"" + DataFrom.ToString("ddMMyy") + "S";
        //                        DataTo = dateTimePickerTo.Value;
        //                        strToData = "=\"" + DataTo.ToString("ddMMyy") + "S";
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C4");
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C451");
        //                        IOUserData = MySerialTest.DirectIO(0, 0, strFromData);
        //                        IOUserData = MySerialTest.DirectIO(0, 0, strToData);
        //                        break;
        //                    case 3:
        //                        //GE Full
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C4");
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C450");
        //                        break;
        //                    case 4:
        //                        //MF
        //                        break;
        //                    case 5:
        //                        //MF
        //                        break;
        //                    case 6:
        //                        //MF
        //                        break;
        //                    default:
        //                        break;
        //                }
        //                break;
        //            case "ONDA":
        //                break;
        //            case "SWING":
        //                switch (TipoLettura)
        //                {
        //                    case 1:
        //                        //GE For Data From Sct to SCT
        //                        DataFrom = dateTimePickerFrom.Value;
        //                        strFromData = "&" + DataFrom.ToString("ddMMyy");
        //                        strFromReceipt = "[" + FromRcpt.Value.ToString();
        //                        strToReceipt = "]" + ToRcpt.Value.ToString();
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C3");
        //                        if (rBVideo.Checked)
        //                        {
        //                            IOUserData = MySerialTest.DirectIO(0, 0, "=C452/$0/" + strFromData + "/" + strFromReceipt + "/" + strToReceipt);
        //                        }
        //                        if (rBPrint.Checked)
        //                        {
        //                            IOUserData = MySerialTest.DirectIO(0, 0, "=C452/$1/" + strFromData + "/" + strFromReceipt + "/" + strToReceipt);
        //                        }
        //                        break;
        //                    case 2:
        //                        //GE From Data to Data
        //                        DataFrom = dateTimePickerFrom.Value;
        //                        strFromData = "&" + DataFrom.ToString("ddMMyy");
        //                        DataTo = dateTimePickerTo.Value;
        //                        strToData = "[" + DataTo.ToString("ddMMyy");
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C3");
        //                        if (rBVideo.Checked)
        //                        {
        //                            IOUserData = MySerialTest.DirectIO(0, 0, "=C451/$0/" + strFromData + "/" + strToData);
        //                        }
        //                        if (rBPrint.Checked)
        //                        {
        //                            IOUserData = MySerialTest.DirectIO(0, 0, "=C451/$1/" + strFromData + "/" + strToData);
        //                        }
        //                        break;
        //                    case 3:
        //                        //GE Full
        //                        if (rBVideo.Checked)
        //                        {
        //                            IOUserData = MySerialTest.DirectIO(0, 0, "=C3");
        //                            IOUserData = MySerialTest.DirectIO(0, 0, "=C450/$0");
        //                        }
        //                        if (rBPrint.Checked)
        //                        {
        //                            IOUserData = MySerialTest.DirectIO(0, 0, "=C3");
        //                            IOUserData = MySerialTest.DirectIO(0, 0, "=C450/$1");
        //                        }
        //                        break;
        //                    case 4:
        //                        //MF
        //                        break;
        //                    case 5:
        //                        //MF From Data to Data
        //                        DataFrom = dateTimePickerFrom.Value;
        //                        strFromData = "&" + DataFrom.ToString("ddMMyy");
        //                        DataTo = dateTimePickerTo.Value;
        //                        strToData = "[" + DataTo.ToString("ddMMyy");
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C3");
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C401/" + strFromData + "/" + strToData);
        //                        break;
        //                    case 6:
        //                        //MF FULL
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C3");
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C400");
        //                        break;
        //                    default:
        //                        break;
        //                }
        //                break;
        //            case "PRINTF":
        //                switch (TipoLettura)
        //                {
        //                    case 1:
        //                        //GE For Data From Sct to SCT
        //                        DataFrom = dateTimePickerFrom.Value;
        //                        strFromData = "&" + DataFrom.ToString("ddMMyy");
        //                        strFromReceipt = "[" + FromRcpt.Value.ToString();
        //                        strToReceipt = "]" + ToRcpt.Value.ToString();
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C3");
        //                        if (rBVideo.Checked)
        //                        {
        //                            IOUserData = MySerialTest.DirectIO(0, 0, "=C452/$0/" + strFromData + "/" + strFromReceipt + "/" + strToReceipt);
        //                        }
        //                        if (rBPrint.Checked)
        //                        {
        //                            IOUserData = MySerialTest.DirectIO(0, 0, "=C452/$1/" + strFromData + "/" + strFromReceipt + "/" + strToReceipt);
        //                        }
        //                        break;
        //                    case 2:
        //                        //GE From Data to Data
        //                        DataFrom = dateTimePickerFrom.Value;
        //                        strFromData = "&" + DataFrom.ToString("ddMMyy");
        //                        DataTo = dateTimePickerTo.Value;
        //                        strToData = "[" + DataTo.ToString("ddMMyy");
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C3");
        //                        if (rBVideo.Checked)
        //                        {
        //                            IOUserData = MySerialTest.DirectIO(0, 0, "=C451/$0/" + strFromData + "/" + strToData);
        //                        }
        //                        if (rBPrint.Checked)
        //                        {
        //                            IOUserData = MySerialTest.DirectIO(0, 0, "=C451/$1/" + strFromData + "/" + strToData);
        //                        }
        //                        break;
        //                    case 3:
        //                        //GE Full
        //                        if (rBVideo.Checked)
        //                        {
        //                            IOUserData = MySerialTest.DirectIO(0, 0, "=C3");
        //                            IOUserData = MySerialTest.DirectIO(0, 0, "=C450/$0");
        //                        }
        //                        if (rBPrint.Checked)
        //                        {
        //                            IOUserData = MySerialTest.DirectIO(0, 0, "=C3");
        //                            IOUserData = MySerialTest.DirectIO(0, 0, "=C450/$1");
        //                        }
        //                        break;
        //                    case 4:
        //                        //MF
        //                        break;
        //                    case 5:
        //                        //MF From Data to Data
        //                        DataFrom = dateTimePickerFrom.Value;
        //                        strFromData = "&" + DataFrom.ToString("ddMMyy");
        //                        DataTo = dateTimePickerTo.Value;
        //                        strToData = "[" + DataTo.ToString("ddMMyy");
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C3");
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C401/" + strFromData + "/" + strToData);
        //                        break;
        //                    case 6:
        //                        //MF FULL
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C3");
        //                        IOUserData = MySerialTest.DirectIO(0, 0, "=C400");
        //                        break;
        //                    default:
        //                        break;
        //                }
        //                break;
        //            default:
        //                break;
        //        }
        //        //IOUserData = MySerialTest.DirectIO(0, 0, "=C1");
        //    }
        //    catch (System.Exception ex)
        //    {
        //        MessageBox.Show("Errore " + ex.ToString());
        //    }
        //}

        //private void TimerLAN_Tick(object sender, EventArgs e)
        //{
        //    //Only for LAN
        //    if (ISConnected)
        //    {
        //        TimerLAN.Enabled = false;
        //        //CINA non va con XOVER (ISWING)
        //        //DisplayFWVersion DisplayFW = UpdateFWVersion;
        //        //Invoke(DisplayFW);
        //    }
        //}

        private void btnRunCmdFiles_Click(object sender, EventArgs e)
        {
            string strCommand = "";
            try
            {
                Stream myStream;
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.InitialDirectory = "c:\\";
                openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                //openFileDialog1.FilterIndex = 2;
                openFileDialog1.RestoreDirectory = true;
                openFileDialog1.Title = "SELEZIONARE FILE DA ESEGUIRE";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        // Insert code to read the stream here.
                        StreamReader reader = new StreamReader(myStream);
                        while (!reader.EndOfStream)
                        {
                            strCommand = reader.ReadLine();
                            //textBoxCmdExec.Text = strCommand;
                           // textBoxCmdExec.Refresh();
                            IOUserData = MySerialTest.DirectIO(0, 0, strCommand);
                            updateForm(IOUserData.Object.ToString());
                        }
                        myStream.Close();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Comando Errato: " + strCommand + ex.ToString());
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                //System.Diagnostics.Process.Start("c:\\windows\\notepad.exe", "c:\\output.txt");
                System.Diagnostics.Process.Start("c:\\windows\\notepad.exe");
            }
            catch (System.Exception)
            {
                MessageBox.Show("Errore Apertura ");
            }
        }

        private void radBut_USB_CheckedChanged(object sender, EventArgs e)
        {
            //cmBoxCOM.Enabled = false;
            //cmBOXBaudRate.Enabled = false;
            //txtBoxIP.Enabled = false;
            //txtBoxPORT.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IOUserData = MySerialTest.DirectIO(0, 0, "=C2");
            String strResp = IOUserData.Object.ToString();
            if (strResp == "")
            {
                MessageBox.Show("COLLEGAMENTO ECR ASSENTE, VERIFICARE LA STAMPANTE FISCALE");
            }
            IOUserData = MySerialTest.DirectIO(0, 0, "=C10");
            strResp = IOUserData.Object.ToString();
            if (strResp == "")
            {
                MessageBox.Show("COLLEGAMENTO ECR ASSENTE, VERIFICARE LA STAMPANTE FISCALE");
            }
            IOUserData = MySerialTest.DirectIO(0, 0, "=C1");
            strResp = IOUserData.Object.ToString();
            if (strResp == "")
            {
                MessageBox.Show("COLLEGAMENTO ECR ASSENTE, VERIFICARE LA STAMPANTE FISCALE");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "<</?d");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }





        private void button4_Click(object sender, EventArgs e)
        {
            //Programmazione Display
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "=D1/(Messaggio1)");
                IOUserData = MySerialTest.DirectIO(0, 0, "=D2/(Messaggio2)");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error " + ex.ToString());
            }
        }



        private void button21_Click(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "=C2");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "=C3");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "=C4");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }

        private void button23_Click(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "=C2");
                updateForm(IOUserData.Object.ToString());
                IOUserData = MySerialTest.DirectIO(0, 0, "=C10");
                updateForm(IOUserData.Object.ToString());
                IOUserData = MySerialTest.DirectIO(0, 0, "=C1");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }

        private void button24_Click(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "=C3");
                updateForm(IOUserData.Object.ToString());
                IOUserData = MySerialTest.DirectIO(0, 0, "=C10");
                updateForm(IOUserData.Object.ToString());
                IOUserData = MySerialTest.DirectIO(0, 0, "=C1");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "=T2");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore A" + ex.ToString());
            }
        }

        private void button25_Click(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "=T3/$10000/(Contanti            )");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore A" + ex.ToString());
            }
        }

        private void button26_Click(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "=T4");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore A" + ex.ToString());
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                IOUserData = MySerialTest.DirectIO(0, 0, "=c");
                updateForm(IOUserData.Object.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Errore " + ex.ToString());
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtSend_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public  void button6_Click()
        {
            for (int i = 0; i < 3; i++)
            {
                btnSend7_Click();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    component.Dispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                CloseHandle(handle);
                handle = IntPtr.Zero;

                // Note disposing has been done.
                disposed = true;

            }


        }

        // Use interop to call the method necessary
        // to clean up the unmanaged resource.
        [System.Runtime.InteropServices.DllImport("Kernel32")]
        private extern static Boolean CloseHandle(IntPtr handle);

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~Stampa()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        public void message(string _message)
        {
            MySerialTest.DirectIO(0, 0, _message);
        }

    }
}
