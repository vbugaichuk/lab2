using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartTable
{
    public partial class FormSmartTable : Form
    {
        Data data;
        string OldTextBoxExpression = String.Empty;
        string currentFileName;
        Class26BaseSys f2 = new Class26BaseSys();
        public FormSmartTable()
        {
            InitializeComponent();
            data = new Data(dataGridView);
            this.Text = "MySmartTable";
            
        }
        internal Data Data
        {
            get => default(Data);
            set { }
        }
        private void FormSmartTable_Load(object sender, EventArgs e)
        {
            data.FillData(Mode.Value);
        }

        private void FormSmartTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Чи дійсно ви хочете закрити програму? Всі незбережені дані будуть втрачені", "Повідомлення", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                e.Cancel = true;
            else
                e.Cancel = false;
        }

        
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView_CellParsing(object sender, DataGridViewCellParsingEventArgs e) 
        {
            if (!string.IsNullOrEmpty(e.Value.ToString()))
            {
                data.ChangeData(e.Value.ToString(), e.RowIndex, e.ColumnIndex);
            }
        }

        

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }


        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

 
        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView.SelectedCells.Count == 1)
            {
                var selectedCell = dataGridView.SelectedCells[0];
                textBox1.Text = Data.cells[selectedCell.RowIndex][selectedCell.ColumnIndex].Expression;
                //textBox1.Text = selectedCell.ColumnIndex.ToString();
                OldTextBoxExpression = textBox1.Text;
            }
        }
        private void dataGridView_CellEnter(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e) //обробляємо комірку після введення
        {
            if (Data.cells[e.RowIndex][e.ColumnIndex].Expression != null)
                if (!String.IsNullOrEmpty(Data.cells[e.RowIndex][e.ColumnIndex].Error))
                    dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Data.cells[e.RowIndex][e.ColumnIndex].Error;
                else
                    dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Data.cells[e.RowIndex][e.ColumnIndex].Value.ToString();
        }

        private void dataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Data.cells[e.RowIndex][e.ColumnIndex].Expression;
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            if (dataGridView.SelectedCells.Count == 1)
            {
                var selectedCell = dataGridView.SelectedCells[0];
                if (textBox1.Text == String.Empty)
                {
                    Data.cells[selectedCell.RowIndex][selectedCell.ColumnIndex].Expression = null;
                    Data.cells[selectedCell.RowIndex][selectedCell.ColumnIndex].Value = 0;
                    dataGridView.Rows[selectedCell.RowIndex].Cells[selectedCell.ColumnIndex].Value = "";
                }
                else
                {
                    data.ChangeData(textBox1.Text, selectedCell.RowIndex, selectedCell.ColumnIndex);
                    if (!String.IsNullOrEmpty(Data.cells[selectedCell.RowIndex][selectedCell.ColumnIndex].Error))
                        dataGridView.Rows[selectedCell.RowIndex].Cells[selectedCell.ColumnIndex].Value = Data.cells[selectedCell.RowIndex][selectedCell.ColumnIndex].Error;
                    else
                        dataGridView.Rows[selectedCell.RowIndex].Cells[selectedCell.ColumnIndex].Value = Data.cells[selectedCell.RowIndex][selectedCell.ColumnIndex].Value.ToString();
                }
            }
        }

        private void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void datagridView_Edit_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e) // Дело сделано
        {
            TextBox tb = (TextBox)e.Control;

            tb.TextChanged += Tb_TextChanged;
        }

        private void Tb_TextChanged(object sender, EventArgs e) 
        {
            textBox1.Text = ((TextBox)sender).Text;
            OldTextBoxExpression = textBox1.Text;
        }

        private void Button2_Click_1(object sender, EventArgs e)
        {
            textBox1.Text = OldTextBoxExpression;
        }
        private void Button3_Click_1(object sender, EventArgs e)
        {
            data.AddRow();
        }
        private void Button4_Click_1(object sender, EventArgs e)
        {
            data.AddColumn();
        }


        private void Button6_Click_1(object sender, EventArgs e)
        {
            data.RemoveRow();
        }
        private void Button5_Click_1(object sender, EventArgs e)
        {
            data.RemoveColumn();
        }

        

        private void ToolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Список доступних операцій: \n" + "1) + - * / (бінарні операції \n" 
                + "2) % (остача від ділення) \n" + "3) ^ (піднесення у степінь) \n" + "4) =, <, > \n" + "5) <=, >=, <>", "Інформація");
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Чи дійсно ви хочете відкрити новий файл? Всі незбережені дані будуть втрачені", "Повідомлення", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                return;
            //else
              //  e.Cancel = false;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                data = new Data(dataGridView);
                data.OpenFile(openFileDialog1.FileName);
                data.FillData(Mode.Value);
                currentFileName = openFileDialog1.FileName;
                this.Text = currentFileName + " - MySmartTable";
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                data.SaveToFile(saveFileDialog1.FileName);
                currentFileName = saveFileDialog1.FileName;
                this.Text = currentFileName + " - MySmartTable";
            }
        }

        private void FileMenu_Click(object sender, EventArgs e)
        {

        }
    }
}
