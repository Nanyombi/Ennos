﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using YAT.View;

namespace YAT
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private class MacroTab
        {

            public MacroTab()
            {
                elements = new List<MacroData>();
            }

            public string name;
            public List<MacroData> elements;
        }

        private SerialPort m_serialPort = new SerialPort();
        private Queue<string> m_ToSendList = new Queue<string>();
        private TabPage m_tabPagePlus = new TabPage("  +");
        private string m_filename = "";
        private bool m_configurationIsDirty = false;
        private List<MacroTab> m_ConfiguredMacro = new List<MacroTab>();
        private System.Data.DataTable m_dataTableLog = new DataTable();

        private void Form1_Shown(Object sender, EventArgs e)
        {
            //set the correct name and version
            this.Text = Application.ProductName + " " + Application.ProductVersion;

            cboBaudRate.Items.Clear();

            cboBaudRate.Items.Add(19200);
            cboBaudRate.Items.Add(115200);
            cboBaudRate.Items.Add(921600);
            cboBaudRate.SelectedIndex = 1;

            //set the callback
            m_serialPort.DataReceived += new SerialDataReceivedEventHandler(dataReceived);

            //remove the current items
            ScanForSerialPorts();


            cboCommandTerminator.Items.Clear();


            cboCommandTerminator.Items.Clear();
            object[] list =
                {
                new ComboBoxItem<string>("None", ""),
                new ComboBoxItem<string>("CR", "\n"),
                new ComboBoxItem<string>("CR + LF", "\r\n"),
                new ComboBoxItem<string>("LF", "\r"),
            };

            cboCommandTerminator.Items.AddRange(list);
            cboCommandTerminator.SelectedIndex = 2;

            cboTimerSendSelected.Items.Clear();
            object[] listTimer =
               {
                new ComboBoxItem<int>("No Timer", 0),
                new ComboBoxItem<int>("1 Hz int.", 1000),
                new ComboBoxItem<int>("2 Hz int", 500),
                new ComboBoxItem<int>("5 Hz int", 200),
                new ComboBoxItem<int>("2 Sec int.", 2000),
                new ComboBoxItem<int>("5 Sec int.", 5000),
            };
            cboTimerSendSelected.Items.AddRange(listTimer);

            cboDecodeType.Items.Clear();
            cboDecodeType.Items.Add("Ascii");
            cboDecodeType.Items.Add("Hex");
            cboDecodeType.SelectedIndex = 0;

            SetupDefaultTab();

            m_dataTableLog.Columns.Add("Time");
            m_dataTableLog.Columns.Add("Data");
            m_dataTableLog.Columns.Add("dataIndex", typeof(Direction));



            dataGridViewLog.DataSource = new System.Windows.Forms.BindingSource { DataSource = m_dataTableLog };

            foreach (DataGridViewColumn column in dataGridViewLog.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            // hide the 3th collumn, as this is only an index
            dataGridViewLog.Columns[2].Visible = false;

            dataGridViewLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dataGridViewLog.ColumnHeadersVisible = true;
            dataGridViewLog.RowHeadersVisible = false;


        }

        void SetupDefaultTab()
        {
            tabMacro.TabPages.Clear();
            m_ConfiguredMacro.Clear();
            m_filename = "";

            tabMacro.TabPages.Add(m_tabPagePlus);
            tabMacro.SelectedTab = CreateNewAndAddTabPage("Default", false);

            AddMacroToPanel(GetCurrentSelectedTab(), true);
            UpdateButtonsAndStatus(true);
        }

        private enum Direction
        {
            Unknown,
            Sending,
            Receiving
        }


        void AddToLog(string data, Direction inOut)
        {
            string header = "";

            //header = DateTime.Now.ToString("yyyyMMddHHmmss.fff"); // case sensitive
            header = DateTime.Now.ToString("HH:mm:ss.fff"); // case sensitive

            if (inOut == Direction.Unknown)
            {
                header = "";
            }


            DataRow newData = m_dataTableLog.NewRow();
            newData[0] = header;
            newData[1] = data;
            newData[2] = inOut;
            m_dataTableLog.Rows.Add(newData);



            if (dataGridViewLog.Rows[m_dataTableLog.Rows.Count - 1].Displayed == false)
            {

                dataGridViewLog.FirstDisplayedScrollingRowIndex = m_dataTableLog.Rows.Count - 1;

            }

        }

        // This delegate enables asynchronous calls for setting  
        // the text property on a TextBox control.  
        delegate void StringArgReturningVoidDelegate(string text);

        private void UpdateReceivedInfo(string text)
        {
            if (dataGridViewLog.InvokeRequired == true)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(UpdateReceivedInfo);
                this.Invoke(d, new object[] { text });
            }
            else
            {

                if (cboDecodeType.SelectedIndex == 1)
                {
                    char[] arrayChars = text.ToCharArray();
                    string addData = "";
                    // add 
                    for (int counter = 0; counter < arrayChars.Length; counter++)
                    {
                        int value = arrayChars[counter];

                        addData += " 0x";
                        addData += value.ToString("X2");
                    }

                    AddToLog(addData, Direction.Receiving);
                }
                else
                {
                    AddToLog(text, Direction.Receiving);
                }

                int count = 0;

                /*
                char[] testchars = txtOutput.Text.ToCharArray();
                char testChar = '\n';
                int length = testchars.Length;
                for (int counter = length - 1; counter >= 0; counter--)
                {
                    if (testchars[counter] == testChar)
                    {
                        count++;
                    }
                }
                */

                lblCountTerminator.Text = count.ToString();

                DecodeToLoggingGraph(text);
            }
        }

        //callback for the serial port
        private void dataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = m_serialPort.ReadExisting();
            
            UpdateReceivedInfo(data);

            if(chkBoxLogValue.Checked == true)
            {

            }
        }

        public void MacroElementChanged(object sender, EventArgs e)
        {
            this.ReportDataDirty();
        }


        private MacroTab GetCurrentMacroTab()

        {
            MacroTab tab = null;

            int index = GetCurrentSelectedTab();

            if (index < m_ConfiguredMacro.Count)
            {
                tab = m_ConfiguredMacro[index];
            }

            return tab;
        }


        public void MacroSetFocusBefore(object sender, EventArgs e)
        {
            if(sender is MacroData)
            {
                MacroTab tab = GetCurrentMacroTab();

                if(tab is not null)
                {
                    int index = tab.elements.IndexOf((MacroData)sender);

                    if(index > 0)
                    {
                        index--;
                    }

                    if(index >= 0)
                    {
                        tab.elements[index].SetFocusToTextBox();
                    }
                }

                
            }
        }

        public void MacroSetFocusAfter(object sender, EventArgs e)
        {
            if (sender is MacroData)
            {
                MacroTab tab = GetCurrentMacroTab();

                if (tab is not null)
                {
                    int index = tab.elements.IndexOf((MacroData)sender);

                    if ((index >= 0) && (index < tab.elements.Count-1))
                    {
                        index++;
                    }

                    if (index >= 0)
                    {
                        tab.elements[index].SetFocusToTextBox();
                    }

                }
            }
        }

        private void UpdateGrid(int indexValue)
        {
            TableLayoutPanel layout = GetTableLayoutPanelOnTab(indexValue);

            if (layout != null)
            {   
                layout.Visible = false;
                layout.SuspendLayout();
                layout.Controls.Clear();

                layout.ColumnCount = 3;
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100.0F));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75));

                MacroData[] elements = m_ConfiguredMacro[indexValue].elements.ToArray();

                layout.RowCount = elements.Length;

                for (int counter = 0; counter < layout.RowCount; counter++)
                {

                    // layout.Controls.Add(elements[counter], 0, counter);                    
                    TextBox localTxtBox = new TextBox();
                    localTxtBox.Dock = DockStyle.Fill;

                    CheckBox chkBox = new CheckBox();
                    chkBox.Dock = DockStyle.Fill;
                    chkBox.Text = "Select";

                    Button btnSend = new Button();
                    btnSend.Text = "Send";
                    btnSend.Dock = DockStyle.Fill;

                    layout.Controls.Add(localTxtBox, 0, counter);                    
                    layout.Controls.Add(btnSend, 1, counter);
                    layout.Controls.Add(chkBox, 2, counter);

                    elements[counter].AttachTo(btnSend, chkBox, localTxtBox);

                }

                layout.RowCount = layout.RowCount + 1;
                Button button = CreateAddOneButton();
                layout.Controls.Add(button, 0, layout.RowCount-1);
                layout.SetColumnSpan(button, 1);

                button = CreateAddTenButton();
                layout.Controls.Add(button, 1, layout.RowCount - 1);
                layout.SetColumnSpan(button, 2);

                layout.ResumeLayout();
                layout.Visible = true;                
            }

        }


        public void MacroElementRemoveMe(object sender, EventArgs e)
        {
            //test the object
            if(sender is MacroData)
            {
                int index = GetCurrentSelectedTab();

                if (index < m_ConfiguredMacro.Count)
                {
                    MacroTab tab = m_ConfiguredMacro[index];

                    for (Int32 counter = 0; counter < tab.elements.Count; counter++)
                    {
                        if (tab.elements[counter] is MacroData)
                        {
                            if (((MacroData)tab.elements[counter]) == ((MacroData)sender))
                            {
                                tab.elements.RemoveAt(counter);
                                counter = Int32.MaxValue - 2;
                                UpdateGrid(index);
                            }


                            //set the counter to the max
                            
                        }
                    }                   
                }             
            }
        }

        public void MacroElementInsertBeforeMe(object sender, EventArgs e)
        {
            int index = GetCurrentSelectedTab();

            if (index < m_ConfiguredMacro.Count)
            {
                MacroTab tab = m_ConfiguredMacro[index];

                for (Int32 counter = 0; counter < tab.elements.Count; counter++)
                {
                    if (tab.elements[counter] is MacroData)
                    {
                        if (((MacroData)tab.elements[counter]) == ((MacroData)sender))
                        {
                            tab.elements.Insert(counter, CreateNewMacro());
                           
                            UpdateGrid(index);
                            counter++;
                        }
                    }
				}
            }
		}
        private void btnLoadMacro_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            openFileDialog1.Filter = "YAT Macro files (*.ymf)|*.ymf";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            //check for the correct result
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Stream myStream = null;
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            //remove the current elements
                            tabMacro.SuspendLayout();
                            tabMacro.TabPages.Clear();
                            m_ConfiguredMacro.Clear();

                            XmlReaderSettings settings = new XmlReaderSettings();
                            settings.Async = false;
                            
                            // Insert code to read the stream here.
                            XmlReader reader = XmlReader.Create(myStream, settings);
                                                        
                            while (reader.Read())
                            {
                                switch (reader.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        switch(reader.Name)
                                        {
                                            case "Tab":                                                
                                                // create a new tab
                                                CreateNewAndAddTabPage(reader.GetAttribute("Name"), true);
                                                
                                                break;
                                            case "Macro":
                                                //add to the last one
                                                MacroData macroSetting = AddMacroToPanel(tabMacro.TabPages.Count - 1,false);
                                                macroSetting.ReadXml(reader);                                                
                                                break;
                                            default:
                                                Console.WriteLine("Start Element {0}", reader.Name);
                                                break;
                                        }

                                        
                                        break;
                                    case XmlNodeType.Text:
                                        Console.WriteLine("Text Node: {0}", reader.Value);
                                        break;
                                    case XmlNodeType.EndElement:
                                        Console.WriteLine("End Element {0}", reader.Name);
                                        break;
                                    default:
                                        Console.WriteLine("Other node {0} with value {1}",
                                                        reader.NodeType, reader.Value);
                                        break;
                                }
                            }
                            
                            //close the stream
                            reader.Close();

                            tabMacro.TabPages.Add(m_tabPagePlus);
                            tabMacro.SelectedIndex = 0;
                            
                            for(int counter = 0; counter < tabMacro.TabPages.Count - 1; counter++)
                            {
                                UpdateGrid(counter);
                            }



                            tabMacro.ResumeLayout();
                        }
                    }
                    UpdateFileName(openFileDialog1.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }                
            }
        }

        private void UpdateTitleBar()
        {
            this.Text = Application.ProductName + " " + Application.ProductVersion;

            this.Text += " - ";
            
            if (Path.GetFileName(m_filename).Length > 0)
            {
                this.Text += Path.GetFileName(m_filename);
            }
            
            if (m_configurationIsDirty== true)
            {
                this.Text += "*";
            }
        }


        private void UpdateFileName(string filename)
        {
            m_filename = filename;
            m_configurationIsDirty = false;
            UpdateTitleBar();
        }


        void SaveFileInStream(Stream myStream)
        {
            if(myStream != null)
            {                
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.OmitXmlDeclaration = true;
                settings.NewLineOnAttributes = true;
                // Serialize the object to a file.
                XmlWriter writer = XmlWriter.Create(myStream, settings);
                writer.WriteStartDocument();
                writer.WriteStartElement("YatMacro");

                // load the items
                foreach (MacroTab foundtab in m_ConfiguredMacro)
                {
                    if (foundtab.elements.Count > 0)
                    {

                        writer.WriteStartElement("Tab");
                        writer.WriteAttributeString("Name", foundtab.name);

                        //foreach(macro macroSetting in panel.Controls)
                        for (Int32 counter = 0; counter < foundtab.elements.Count; counter++)
                        {
                            foundtab.elements[counter].WriteXml(writer);                                
                        }
  

                        //XmlWriter
                        writer.WriteEndElement();
                    }
                }
                //close the document
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Close();
            }
        }

        void BackUpCurrentFile(string pathAndFilename)
        {
            if(File.Exists(pathAndFilename))
            {
                string backupFolder = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "Backup");

                //check if the dir is existing
                if(Directory.Exists(backupFolder) == false)
                {
                    Directory.CreateDirectory(backupFolder);
                }

                // the directory now exists
                string newFile = Path.GetFileNameWithoutExtension(pathAndFilename) + "_" + DateTime.Now.ToString("s")  + Path.GetExtension(pathAndFilename);

                newFile = newFile.Replace(":", "_");

                File.Copy(pathAndFilename, Path.Combine(backupFolder, newFile));
            }
        }

        private void SaveFile(bool isSaveAS)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.InitialDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            saveFile.RestoreDirectory = true;
            saveFile.Filter = "YAT Macro files (*.ymf)|*.ymf";
            bool saveData = false;

            if ((m_filename.Length > 0) && (isSaveAS == false))
            {
                saveFile.FileName = m_filename;
                saveFile.InitialDirectory = Path.GetDirectoryName(m_filename);
                saveData = true;
            }
            else if (isSaveAS == true)
            {
                saveFile.FileName = Path.GetFileName(m_filename);
                saveFile.InitialDirectory = Path.GetDirectoryName(m_filename);
                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    saveData = true;
                }
            }
            else if (saveFile.ShowDialog() == DialogResult.OK)
            {
                saveData = true;
            }

            if (saveData == true)
            {
                Stream myStream = null;

                BackUpCurrentFile(m_filename);
                if ((myStream = saveFile.OpenFile()) != null)
                {
                    FileStream fs = myStream as FileStream;
                    if (fs != null) Console.WriteLine(fs.Name);

                    SaveFileInStream(myStream);
                }
                myStream.Close();
                UpdateFileName(saveFile.FileName);

            }
        }


        private void btnSaveMacro_Click(object sender, EventArgs e)
        {
            btnSaveMacro.Enabled = false;
            SaveFile(false);
            btnSaveMacro.Enabled = true;
        }


        private  int GetCurrentSelectedTab()
        {
            return tabMacro.SelectedIndex;
        }

        private TableLayoutPanel GetTableLayoutPanelOnTab(int index)
        {
            TableLayoutPanel foundItem = null;
            if (index < tabMacro.TabCount)
            {
                TabPage selectPage = tabMacro.TabPages[index];
                //check element
                if (selectPage != null)
                {
                    foundItem = (TableLayoutPanel)selectPage.Controls[0];
                }
            }

            return foundItem;
        }


       // private TableLayoutPanel GetTableLayoutPanelOnCurrentTab()
        //{            
            //return GetTableLayoutPanelOnTab(tabMacro.SelectedIndex); 
        //}

        private List<MacroData> GetMacroLayoutOnCurrentTab()
        {
            return m_ConfiguredMacro[tabMacro.SelectedIndex].elements;
        }


        private MacroData CreateNewMacro()
        {
            MacroData myobject = new MacroData();            
            myobject.Datachanged += MacroElementChanged;
            myobject.RemoveMe += MacroElementRemoveMe;
            myobject.InsertBeforeMe += MacroElementInsertBeforeMe;
            myobject.SetFocusAfterMe += MacroSetFocusAfter;
            myobject.SetFocusBeforeMe += MacroSetFocusBefore;
            return myobject;
        }

        private MacroData AddMacroToPanel(int index, bool updateView)
        {
            MacroData myobject =null;

            if (index < m_ConfiguredMacro.Count)
            {
                // testing the adding off the user commands
                myobject = CreateNewMacro();
                m_ConfiguredMacro[index].elements.Add(myobject);
            }

            if(updateView == true)
            {
                UpdateGrid(index);
            }

            return myobject;
        }

        private void AddElements(int count)
        {
            for(int counter = 0; counter < count -1; counter++)
            {
                AddMacroToPanel(GetCurrentSelectedTab(), false);
            }

            AddMacroToPanel(GetCurrentSelectedTab(), true);
            ReportDataDirty();

            TableLayoutPanel layout = GetTableLayoutPanelOnTab(GetCurrentSelectedTab());
            if (layout != null)
            {
                layout.VerticalScroll.Value = layout.VerticalScroll.Maximum;
                layout.Update();
            }
        }

        private void btn10NewMacro_Click(object sender, EventArgs e)
        {
            AddElements(10);
        }

        private void btnNewMacro_Click(object sender, EventArgs e)
        {
            AddElements(1);
        }

        private void ScanForSerialPorts()
        {
            //scan all the serial ports
            string[] ports = SerialPort.GetPortNames();

            //remove the current items
            cboSerialPorts.Items.Clear();

            if (ports.Count() == 0)
            {
                //remove the current items
                cboSerialPorts.Items.Add("No port");

            }
            else
            {
                cboSerialPorts.Items.AddRange(ports);
            }
            cboSerialPorts.SelectedIndex = 0;
        }

        private void btnRescan_Click(object sender, EventArgs e)
        {
            ScanForSerialPorts();
        }

        private Button CreateAddOneButton()
        {
            Button macroAddButton = new Button();
            macroAddButton.Click += new System.EventHandler(this.btnNewMacro_Click);
            macroAddButton.Dock = DockStyle.Fill;            
            macroAddButton.Height = 30;
            macroAddButton.Text = "+1";                
            return macroAddButton;
        }

        private Button CreateAddTenButton()
        {
            Button macroAddButton = new Button();
            macroAddButton.Click += new System.EventHandler(this.btn10NewMacro_Click);
            macroAddButton.Dock = DockStyle.Fill;
            macroAddButton.Height = 30;
            macroAddButton.Text = "+10";
            return macroAddButton;
        }

        private TabPage CreateNewAndAddTabPage(string nameTab, bool appendToEnd)
        {
            
            TabPage tp = new TabPage(nameTab);
            //FlowLayoutPanel fl_panel = new FlowLayoutPanel();
            TableLayoutPanel tbPanel = new TableLayoutPanel();
            tbPanel.Dock = DockStyle.Fill;
            tbPanel.VerticalScroll.Visible = true;
            tbPanel.AutoScroll = true;
            
            tbPanel.BringToFront();

            MacroTab newElement = new MacroTab();
            newElement.name = nameTab;

            m_ConfiguredMacro.Add(newElement);

            //add the panel
            tp.Controls.Add(tbPanel);
            //make the back ground nice
            tp.UseVisualStyleBackColor = true;
            if (appendToEnd == false)
            {
                tabMacro.TabPages.Insert(tabMacro.TabPages.Count - 1, tp);
            }
            else
            {
                tabMacro.TabPages.Add(tp);
            }

            return tp;      
         }


        private void btnRenameTab_Click(object sender, EventArgs e)
        {
            AskController getNewName = new AskController(this);
            //get the new name
            string newName = getNewName.GetNewName(tabMacro.SelectedTab.Text);

            //check the length
            if(newName.Length > 0)
            {
                //copy the new name
                tabMacro.SelectedTab.Text = newName;
                m_ConfiguredMacro[tabMacro.SelectedIndex].name = newName;
                ReportDataDirty();
            }
        }

        private void btnRemoveTab_Click(object sender, EventArgs e)
        {
            //check for at least one tab
            if (tabMacro.TabCount > 1)
            {
                if (MessageBox.Show(this, "Are you sure you want to remove \"" +  tabMacro.SelectedTab.Text + "\"?", "Remove tab", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    m_ConfiguredMacro.RemoveAt(tabMacro.SelectedIndex);
                    //remove the selected tab
                    tabMacro.TabPages.Remove(tabMacro.SelectedTab);
                    ReportDataDirty();
                }
            }
        }

        private void ReportConnectionStatus(string status)
        {
            toolStripCurrentStatusLabel.Text = status;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            //connect with the serial port
            if(m_serialPort.IsOpen)
            {
                m_serialPort.Close();                
            }


            if (cboSerialPorts.SelectedItem != null)
            {
                //get the data from the combo port
                m_serialPort.PortName = cboSerialPorts.SelectedItem.ToString();
                m_serialPort.BaudRate = Convert.ToInt32(cboBaudRate.SelectedItem.ToString());                
                try
                {
                    m_serialPort.Open();
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message);
                }
                
            }

            //update the info
            UpdateButtonsAndStatus(false);

        }

        private void UpdateButtonsAndStatus(bool changeTimer)
        {
            if(m_serialPort.IsOpen)
            {
                ReportConnectionStatus("Connected: " + m_serialPort.PortName + ", " + m_serialPort.BaudRate.ToString() + ", " + m_serialPort.DataBits.ToString() + ", " + m_serialPort.Parity.ToString() + ", " + m_serialPort.StopBits.ToString());            
            }
            else
            {
                ReportConnectionStatus("Disconnected");            
            }

            btnDisconnect.Enabled = m_serialPort.IsOpen;
            btnConnect.Enabled = !m_serialPort.IsOpen;
            btnSendAll.Enabled = m_serialPort.IsOpen;
            if (changeTimer == true)
            {
                cboTimerSendSelected.SelectedIndex = 0;
            }
            cboTimerSendSelected.Enabled = m_serialPort.IsOpen;
        }        

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if(m_serialPort.IsOpen)
            {
                m_serialPort.Close();
            }
            //update the view
            UpdateButtonsAndStatus(false);
        }

        void ReportDataDirty()
        {
            if(m_configurationIsDirty == false)
            {
                m_configurationIsDirty = true;
                UpdateTitleBar();
            }
        }


        private void btnDuplicateTab_Click(object sender, EventArgs e)
        {
            AskController getTabName = new AskController(this);

            //get the new name
            string nameTab = getTabName.GetNewName("");

            if (nameTab.Length > 0)
            {
                //select the tab
                TabPage clone = CreateNewAndAddTabPage(nameTab, false);

                int currentIndex = GetCurrentSelectedTab();
                // -2 as + tab = 1, and the count is 0 based.
                int cloneIndex = tabMacro.TabPages.Count - 2;

                for (Int32 counter = 0; counter < m_ConfiguredMacro[currentIndex].elements.Count; counter++)
                {
                    MacroData local = CreateNewMacro(); ;
                    local.CloneSettings(m_ConfiguredMacro[currentIndex].elements[counter]);
                    m_ConfiguredMacro[cloneIndex].elements.Add(local);
                }

                UpdateGrid(cloneIndex);

                //select the clone
                tabMacro.SelectedIndex = cloneIndex;

                ReportDataDirty();                

            }
        }

        public void SendCommand(string command)
        {
            if (m_serialPort.IsOpen == true)
            {

                if (m_ToSendList.Count() > 0)
                {
                    m_ToSendList.Enqueue(command);
                }
                else
                {
                    if(m_serialPort.BytesToWrite > 0)
                    {
                        m_ToSendList.Enqueue(command);
                    }
                    else
                    {
                        WriteStringToSerial(command);
                    }
                }

            }
        }

        private string GetTerminationString()
        {
            string terminator = "";
            if (cboCommandTerminator.SelectedItem is ComboBoxItem<string>)
            {
                ComboBoxItem<string> cast = cboCommandTerminator.SelectedItem as ComboBoxItem<string>;

                terminator = cast.Value;
            }
            return terminator;
        }

        private void WriteStringToSerial(string command)
        {
            if(m_serialPort.IsOpen == true)
            {
                string toSend = txtBoxPreFix.Text + command + GetTerminationString();
                try
                {
                    m_serialPort.Write(toSend);
                    AddToLog(toSend, Direction.Sending);
                } catch
                {
                    UpdateButtonsAndStatus(false);
                }
            }
        }

        private void btnSendAll_Click(object sender, EventArgs e)
        {
            List<MacroData> macroDataList = GetMacroLayoutOnCurrentTab();

            if (m_serialPort.IsOpen == true)
            {

                if (macroDataList != null)
                {
                    if (macroDataList.Count > 0)
                    {
                        for (Int32 counter = 0; counter < macroDataList.Count; counter++)
                        {
                            if (macroDataList[counter] is not null)
                            {
                                macroDataList[counter].SendIfChecked();
                            }
                        }
                    }

                }
            }
            
        }

        private void cboSerialPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(m_serialPort.IsOpen == true)
            {
                btnDisconnect.PerformClick();
                btnConnect.PerformClick();
            }
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            m_dataTableLog.Clear();            
            lblCountTerminator.Text = "0";
        }

        private void cboTimerSendSelected_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            if(cboTimerSendSelected.SelectedIndex == 0)
            {
                tmrSendAllCommands.Enabled = false;
            }
            else
            {
                tmrSendAllCommands.Enabled = false;

                if (cboTimerSendSelected.SelectedItem is ComboBoxItem<int>)
                {
                    ComboBoxItem<int> cast = cboTimerSendSelected.SelectedItem as ComboBoxItem<int>;

                    tmrSendAllCommands.Interval = cast.Value;
                }


                tmrSendAllCommands.Enabled = true;
            }
        }

        private int m_tabCounter = -1;
        private int m_lastMacroIndex = 0;

        private void FindNextElementAndUpdateVar()
        {
            bool foundItem = false;

            while(foundItem == false)
            {
               
                if (m_tabCounter == -1)
                {
                    if(m_ConfiguredMacro.Count > 0)
                    {
                        m_tabCounter++;
                    }
                    else
                    {
                        // done 
                        foundItem = true;
                    }
                }
                else if(m_tabCounter >= m_ConfiguredMacro.Count)
                {
                    m_tabCounter = -1;
                    // done 
                    foundItem = true;
                }
                else if (m_lastMacroIndex >= m_ConfiguredMacro[m_tabCounter].elements.Count)
                {
                    m_tabCounter++;
                    m_lastMacroIndex = 0;
                }
                else
                {
                   // check if possible
                   if(m_ConfiguredMacro[m_tabCounter].elements[m_lastMacroIndex].GetChecked() == true)
                    {
                        m_ConfiguredMacro[m_tabCounter].elements[m_lastMacroIndex].SendIfChecked();
                        foundItem = true;
                    }
                  
                    m_lastMacroIndex++;
                   

                }
            }
            
            
            
        }

        private void tmrSendAllCommands_Tick(object sender, EventArgs e)
        {
            if (m_serialPort.IsOpen == true)
            {
                if (m_tabCounter == -1)
                {
                    AddToLog("------------------------- Send selected -------------------------", Direction.Unknown);
                    
                }

                FindNextElementAndUpdateVar();


            }
        }

        private void tabMacro_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(tabMacro.SelectedTab == m_tabPagePlus)
            {
                AskController getTabName = new AskController(this);

                //get the new name
                string nameTab = getTabName.GetNewName("");

                if (nameTab.Length > 0)
                {
                    //select the tab
                    tabMacro.SelectedTab = CreateNewAndAddTabPage(nameTab, false);
                    AddMacroToPanel(GetCurrentSelectedTab(), true);
                    ReportDataDirty();
                }
                else
                {
                    tabMacro.SelectedIndex = 0;
                }
            }
        }


        void CheckConfigurationAndSave()
        {
            if (m_configurationIsDirty == true)
            {
                if (MessageBox.Show("Configuration changed, do you want to save?", Application.ProductName, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    btnSaveMacro.PerformClick();
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CheckConfigurationAndSave();
        }



        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
              List<MacroData> macroDataList = GetMacroLayoutOnCurrentTab();          

                if (macroDataList != null)
                {
                    if (macroDataList.Count > 0)
                    {
                        for (Int32 counter = 0; counter < macroDataList.Count; counter++)
                        {
                            if (macroDataList[counter] is not null)
                            {
                                macroDataList[counter].SetChecked(chkSelectAll.Checked);
                            }
                        }
                    }

                }
           

        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            CheckConfigurationAndSave();
            SetupDefaultTab();
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            string backupFile = m_filename;            
            ///
            SaveFile(true);
            

            if(m_filename.Length == 0)
            {
                m_filename = backupFile;
            }


        }

        private TabPage m_DraggedTab;
        private void tabMacro_MouseDown(object sender, MouseEventArgs e)
        {
            m_DraggedTab = TabAt(e.Location);
        }

        private void tabMacro_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || m_DraggedTab == null)
            {
                return;
            }

            TabPage tab = TabAt(e.Location);

            if (tab == null || tab == m_DraggedTab)
            {
                return;
            }

            if (Swap(m_DraggedTab, tab) == true)
            {
                tabMacro.SelectedTab = m_DraggedTab;
                ReportDataDirty();
            }
        }

        private TabPage TabAt(Point position)
        {
            // -1 as the last one is the +
            int count = tabMacro.TabCount - 1;

            for (int i = 0; i < count; i++)
            {
                if (tabMacro.GetTabRect(i).Contains(position))
                {
                    return tabMacro.TabPages[i];
                }
            }

            return null;
        }

        private bool Swap(TabPage a, TabPage b)
        {
            int i = tabMacro.TabPages.IndexOf(a);
            int j = tabMacro.TabPages.IndexOf(b);
            bool swapped = false;

            if ((i >= 0) && (j >= 0))
            {
                // swap the inter data as well
                MacroTab copy =  m_ConfiguredMacro[i];
                m_ConfiguredMacro[i] = m_ConfiguredMacro[j];
                m_ConfiguredMacro[j] = copy;

                tabMacro.TabPages[i] = b;
                tabMacro.TabPages[j] = a;
                swapped = true;
            }

            return swapped;
        }

        private void dataGridViewLog_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataRowView row = dataGridViewLog.Rows[e.RowIndex].DataBoundItem as DataRowView;

            if (row is not null)
            {
                if (row.Row.ItemArray[2] is not null)
                {
                    switch ((Direction)row.Row.ItemArray[2])
                    {
                        case Direction.Sending:

                            e.CellStyle.ForeColor = Color.Black;
                            e.CellStyle.BackColor = Color.White;
                            break;
                        case Direction.Receiving:
                            e.CellStyle.ForeColor = Color.Black;
                            e.CellStyle.BackColor = Color.LightGray;
                            break;
                        case Direction.Unknown:
                            e.CellStyle.ForeColor = Color.LightBlue;
                            e.CellStyle.BackColor = Color.Gray;
                            break;

                    }
                }
            }
        }

        void SetupLoggingGraph()
        {
            //clear the points
            chrtLoggingData.Series[0].Points.Clear();
            chrtLoggingData.ChartAreas[0].AxisX.Maximum = 10;
            chrtLoggingData.ChartAreas[0].AxisY.IsStartedFromZero = false;
        }


        private void chkBoxLogValue_CheckedChanged(object sender, EventArgs e)
        {
            tmrLog.Enabled = chkBoxLogValue.Checked;

            if(chkBoxLogValue.Checked == true)
            {
                if (tmrSendAllCommands.Enabled == true)
                {
                    cboTimerSendSelected.SelectedIndex = 0;
                }

                SetupLoggingGraph();
            }
            //clear the data
            m_unusedData = "";
            
        }

        private void tmrLog_Tick(object sender, EventArgs e)
        {
            if (m_serialPort.IsOpen == false)
            {
                chkBoxLogValue.Checked = false;
            }
            else
            {
                SendCommand(txtSendGraphCommand.Text);
            }
        }
        string m_unusedData = "";
        private void DecodeToLoggingGraph(string dataToDecode)
        {
            if (chkBoxLogValue.Checked == true)
            {
                m_unusedData += dataToDecode;

                if (m_unusedData.Contains(GetTerminationString()) == true)
                {
                    //check if we can decode
                    if (txtDecodeValue.TextLength > 0)
                    {
                        try
                        {
                            
                            Match m = Regex.Match(m_unusedData, txtDecodeValue.Text, RegexOptions.IgnoreCase);
                            if (m.Success == true)
                            {
                                string data = m.Groups[0].Value;
                                double value = 0;
                                try
                                {
                                    value = double.Parse(data);

                                    // add point to the chart
                                    chrtLoggingData.Series[0].Points.AddY(value);

                                    if(chrtLoggingData.Series[0].Points.Count > chrtLoggingData.ChartAreas[0].AxisX.Maximum)
                                    {
                                        chrtLoggingData.ChartAreas[0].AxisX.Maximum += 10;
                                    }

                                }
                                catch(Exception)
                                {

                                }

                            }
                        }
                        catch (Exception)
                        {

                        }
                    }

                    m_unusedData = m_unusedData.Substring(m_unusedData.LastIndexOf(GetTerminationString()));
                }
            }
        }

    }
}
