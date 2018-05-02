﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MExcel = Microsoft.Office.Interop.Excel;
using System.IO;
using Accord.IO;

namespace DecisionTree
{
    public partial class MainForm : Form
    {
        public string[,] data;
        public string[] inputs;
        public string[] outputs;
        public MainForm()
        {
            InitializeComponent();
            data = new string[10, 10];
            inputs = new string[1];
            outputs = new string[1];
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            int sizeFormX = 800;
            int sizeFormY = 500;
            this.Size = new Size(sizeFormX, sizeFormY);

        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.Title = "Browse Excel File";
            openFileDialog1.Filter = "Excel Worksheets (*.xls;*.xlsx)|*.xls;*.xlsx|All File (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.ShowReadOnly = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var nameFile = openFileDialog1.FileName;

                    //form an object to work with an Excel file
                    string extension = Path.GetExtension(nameFile);
                    if (extension == ".xls" || extension == ".xlsx")
                    {
                        MExcel.Application ObjExcel = new MExcel.Application();
                        MExcel.Workbook ExcelBook = ObjExcel.Workbooks.Open(nameFile);

                        List<string> worksheets = new List<string>();
                        foreach(MExcel.Worksheet each in ExcelBook.Worksheets)
                        {
                            worksheets.Add(each.Name);
                        }
                        SelectTableSheet table = new SelectTableSheet(worksheets.ToArray());
                        if (table.ShowDialog(this) == DialogResult.OK)
                        {
                            MExcel.Worksheet ExcSheet = ExcelBook.Sheets[worksheets.Count() - table.Selection];

                            //determining the range of data storage in a file
                            Dictionary<int, int> RC = Utilities.RangeOfData(ExcSheet);
                            int rows = RC.First().Key;
                            int column = RC.First().Value;
                            int firstRow = RC.Last().Key;
                            int firstColumn = RC.Last().Value;

                            //recording in the program memory arrays and matrix from the original data for further work
                            data = new string[rows, column];
                            inputs = new string[column];
                            outputs = new string[rows];
                            for (int j = 0; j < column; j++)
                            {
                                inputs[j] = ExcSheet.Cells[firstRow, j + firstColumn].Text;
                            }
                            for (int i = 0; i < rows; i++)
                            {
                                for (int j = 0; j < column; j++)
                                {
                                    data[i, j] = ExcSheet.Cells[i + firstRow + 1, j + firstColumn].Text;
                                }
                            }
                            for (int i = 0; i < rows; i++)
                            {
                                outputs[i] = ExcSheet.Cells[i + firstRow + 1, column + firstColumn].Text;
                            }
                            ExcelBook.Close();
                            ObjExcel.Quit();

                            //difine type of inputs
                            Dictionary<string, string> typeOfInputs = Utilities.TypeOfInputs(data, inputs);                            

                            for (int j = 0; j < column; j++)
                            {
                                if(typeOfInputs[inputs[j]] == "string")
                                {
                                    //строим ФП как для ранговых
                                    string[] attributeValues = new string[rows];
                                    for (int i=0; i < rows; i++)
                                    {
                                        attributeValues[i] = data[i, j];
                                    }
                                    SortRanks ranks = new SortRanks(inputs[j], attributeValues);
                                    if (ranks.ShowDialog(this) == DialogResult.OK)
                                    {
                                        label1.Text = inputs[j];
                                    }
                                    //упорядочить значения (причем с формы ручками)
                                    Dictionary<string, double> centersFP = Utilities.CentersOfFP(attributeValues);
                                }
                                else
                                {
                                    //строим ФП каким-нибудь способом для чисел
                                }
                            }
                            //label1.Text = "";
                            //foreach (var each in typeOfInputs)
                            //{
                            //    label1.Text += each.Key + "->" + each.Value + "; ";
                            //}
                            //НАДО РИСОВАТЬ ГРАФИКИ
                        }
                    }
                }

                catch (Exception excp)
                {
                    MessageBox.Show(excp.Message.ToString());
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    } 
}
