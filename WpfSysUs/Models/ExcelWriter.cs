using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ExcelStreamLateBinding
{
    public class ExcelWriter
    {
        // Fields
        private object m_Application;
        private int m_ProcessId;
        private object m_Workbooks;
        private object m_Workbook;
        private object m_Worksheet;
        private object m_Range;
        private object[] m_params;
        private int m_ColumnCount;
        private int m_ColumnIndex;
        private int m_RowCount;
        private int m_RowIndex;
        private string m_Value;
        private bool m_StreamEnded;

        // Constructor
        public ExcelWriter()
        {
            m_ProcessId = -1;
            m_StreamEnded = false;
            m_ColumnCount = -1;
            m_ColumnIndex = -1;
            m_RowCount = -1;
            m_RowIndex = -1;
        }

        // Properties
        public int ProcessId
        {
            get { return m_ProcessId; }
        }
        public bool StreamEnded
        {
            get { return m_StreamEnded; }
        }

        public string Version
        {
            get
            {
                return (string)m_Application.GetType().InvokeMember(
                  "Version", BindingFlags.GetProperty, null, m_Application, null);
            }
        }

        // Methods
        public void Close()
        {
            m_Worksheet = null;
            m_Workbook.GetType().InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, m_Workbook, null);
            m_params = new object[] { false, Type.Missing, Type.Missing };
            m_Workbook.GetType().InvokeMember("Close", BindingFlags.InvokeMethod, null, m_Workbook, m_params);
            m_Workbook = null;
            m_params = new object[1] { true };
            m_Application.GetType().InvokeMember("DisplayAlerts", BindingFlags.SetProperty, null, m_Application, m_params);
            m_Application.GetType().InvokeMember("Quit", BindingFlags.InvokeMethod, null, m_Application, null);
            m_Application = null;
            Process.GetProcessById(m_ProcessId).Kill();
            m_ProcessId = -1;
        }

        public void Open(string filename)
        {
            Open(filename, null);
        }

        public void Open(string filename, string worksheetName)
        {
            Type objClassType = Type.GetTypeFromProgID("Excel.Application");
            Process[] excelProcessBefore = Process.GetProcessesByName("EXCEL");
            m_Application = Activator.CreateInstance(objClassType);
            Process[] excelProcessAfter = Process.GetProcessesByName("EXCEL");
            m_ProcessId = GetProcessId(excelProcessBefore, excelProcessAfter);

            m_params = new object[1] { false };
            m_Application.GetType().InvokeMember("DisplayAlerts", BindingFlags.SetProperty, null, m_Application, m_params);

            m_Workbooks = m_Application.GetType().InvokeMember("Workbooks", BindingFlags.GetProperty, null, m_Application, null);

            if (File.Exists(filename))
            {
                m_params = new object[1] { filename };
                m_Workbook = m_Workbooks.GetType().InvokeMember("Open", BindingFlags.InvokeMethod, null, m_Workbooks, m_params);
            }
            else
            {
                m_params = new object[1] { Type.Missing };
                m_Workbook = m_Workbooks.GetType().InvokeMember("Add", BindingFlags.InvokeMethod, null, m_Workbooks, m_params);
                m_params = new object[1] { filename };
                m_Workbook.GetType().InvokeMember("SaveAs", BindingFlags.InvokeMethod, null, m_Workbook, m_params);
            }

            if (string.IsNullOrEmpty(worksheetName))
                m_Worksheet = m_Workbook.GetType().InvokeMember("Activesheet", BindingFlags.GetProperty, null, m_Workbook, null);
            else
                m_Worksheet = GetWorksheet(worksheetName);

            SetWorksheetAttributes();
        }

        public void NewLine()
        {
            if (!m_StreamEnded)
            {
                if (m_RowIndex < m_RowCount)
                {
                    m_RowIndex++;
                    m_ColumnIndex = 1;
                }
                else
                    m_StreamEnded = true;
            }
            else
                throw new EndOfStreamException();
        }

        public void Write(string value)
        {
            if (!m_StreamEnded)
            {
                object cells = m_Worksheet.GetType().InvokeMember("Cells", BindingFlags.GetProperty, null, m_Worksheet, null);
                m_params = new object[2] { m_RowIndex, m_ColumnIndex };
                m_Range = cells.GetType().InvokeMember("Item", BindingFlags.GetProperty, null, cells, m_params);
                m_params = new object[1] { value };
                m_Range.GetType().InvokeMember("Value2", BindingFlags.SetProperty, null, m_Range, m_params);
                if (m_ColumnIndex == m_ColumnCount)
                {
                    if (m_RowIndex < m_RowCount)
                    {
                        m_RowIndex++;
                        m_ColumnIndex = 1;
                    }
                    else
                        m_StreamEnded = true;
                }
                else
                    m_ColumnIndex++;
            }
            else
                throw new EndOfStreamException();
        }

        public void WriteLine(string[] values)
        {
            if (!m_StreamEnded)
            {
                foreach (string str in values)
                {
                    if (m_ColumnIndex <= m_ColumnCount)
                    {
                        object cells = m_Worksheet.GetType().InvokeMember("Cells", BindingFlags.GetProperty, null, m_Worksheet, null);
                        m_params = new object[2] { m_RowIndex, m_ColumnIndex };
                        m_Range = cells.GetType().InvokeMember("Item", BindingFlags.GetProperty, null, cells, m_params);
                        m_params = new object[1] { str };
                        m_Range.GetType().InvokeMember("Value2", BindingFlags.SetProperty, null, m_Range, m_params);
                        if (m_ColumnIndex < m_ColumnCount)
                            m_ColumnIndex++;
                    }
                }
                if (m_RowIndex < m_RowCount)
                {
                    m_RowIndex++;
                    m_ColumnIndex = 1;
                }
            }
            else
                throw new EndOfStreamException();
        }

        public void PasteFromClipboard()
        {
            m_params = new object[2] { 1, 1 };
            object rng = m_Worksheet.GetType().InvokeMember("Cells", BindingFlags.GetProperty, null, m_Worksheet, m_params);
            m_params = new object[2] { rng, false };
            m_Worksheet.GetType().InvokeMember("Paste", BindingFlags.InvokeMethod, null, m_Worksheet, m_params);
        }

        private int GetProcessId(Process[] ExcelProcessBefore, Process[] ExcelProcessAfter)
        {
            bool isThisProcess = false;
            int result = -1;
            if (ExcelProcessBefore.Length == 0 && ExcelProcessAfter.Length == 1)
                result = ExcelProcessAfter[0].Id;
            else
            {
                foreach (Process processAfter in ExcelProcessAfter)
                {
                    isThisProcess = true;
                    foreach (Process processBefore in ExcelProcessBefore)
                    {
                        if (processAfter.Id == processBefore.Id)
                        {
                            isThisProcess = false;
                        }
                    }
                    if (isThisProcess)
                        result = processAfter.Id;
                }
            }
            return result;
        }

        private object GetWorksheet(string worksheetName)
        {
            object worksheets = m_Workbook.GetType().InvokeMember("Worksheets", BindingFlags.GetProperty, null, m_Workbook, null);
            try
            {
                m_params = new object[1] { worksheetName };
                return worksheets.GetType().InvokeMember("Item", BindingFlags.GetProperty, null, worksheets, m_params);
            }
            catch
            {
                m_params = new object[1] { Type.Missing };
                object newSheet = worksheets.GetType().InvokeMember("Add", BindingFlags.InvokeMethod, null, worksheets, m_params);
                m_params = new object[1] { worksheetName };
                newSheet.GetType().InvokeMember("Name", BindingFlags.SetProperty, null, newSheet, m_params);
                return newSheet;
            }
        }

        private void SetWorksheetAttributes()
        {
            m_RowIndex = 1;
            m_ColumnIndex = 1;
            object Rows = m_Worksheet.GetType().InvokeMember("Rows", BindingFlags.GetProperty, null, m_Worksheet, null);
            m_RowCount = (int)Rows.GetType().InvokeMember("Count", BindingFlags.GetProperty, null, Rows, null);
            object Cols = m_Worksheet.GetType().InvokeMember("Columns", BindingFlags.GetProperty, null, m_Worksheet, null);
            m_ColumnCount = (int)Cols.GetType().InvokeMember("Count", BindingFlags.GetProperty, null, Cols, null);
        }
    }
}
