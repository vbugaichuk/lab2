using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using SmartTable;
namespace SmartTable
{

    enum Mode { Expression, Value }

    class Data
    {
        private int columnCount = 10;
        private int rowCount = 10;
        Parser parser = new Parser();
        DataGridView dataGridView;
        Class26BaseSys sys= new Class26BaseSys();

        public static List<List<Cell>> cells = new List<List<Cell>>();

        public Data(DataGridView _dataGridView)
        {
            
            dataGridView = _dataGridView;
            cells.Clear();
            for (int i = 0; i < rowCount; i++)
            {
                cells.Add(new List<Cell>());
                for(int j = 0; j < columnCount; j++)
                {
                    cells[i].Add(new Cell() { RowNumber = i + 1, ColumnLetter = sys.ToSys(j) });
                }
            }
        }
        public Cell Cell
        {
            get => default(Cell);
            set { }
        }
        public Parser Parser
        {
            get => default(Parser);
            set { }
        }

        public void AddRow()
        {
            cells.Add(new List<Cell>());
            for(int j = 0; j < columnCount; j++)
            {
                cells[cells.Count - 1].Add(new Cell() { RowNumber = rowCount + 1, ColumnLetter = sys.ToSys(j) });
            }
            dataGridView.Rows.Add(1);
            dataGridView.Rows[rowCount].HeaderCell.Value = (rowCount+1).ToString();
            rowCount++;
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    for (int k = 0; k < cells[i][j].References.Count; k++)
                    {
                        if (cells[i][j].References[k].RowNumber == rowCount)
                        {
                            cells[i][j].Value = 0;
                            cells[i][j].Error = null;
                            dataGridView.Rows[i].Cells[j].Value = cells[i][j].Value.ToString();
                            cchangeData(cells[i][j].References[k].RowNumber, cells[i][j].References[k].ColumnLetter, false, false);
                        }
                    }

                }
            }

        }
        public void AddColumn()
        {
                for(int i = 0; i < rowCount; i++)
                {
                    cells[i].Add(new Cell() { RowNumber = i + 1, ColumnLetter = sys.ToSys(columnCount) });
                }

                dataGridView.Columns.Add(sys.ToSys(columnCount), sys.ToSys(columnCount));
                columnCount++;
                for (int i = 0; i < rowCount; i++)
                {
                    for (int j = 0; j < columnCount; j++)
                    {
                        for(int k = 0; k < cells[i][j].References.Count; k++)
                        {
                            if (cells[i][j].References[k].ColumnLetter == sys.ToSys(columnCount - 1))
                            {
                                cells[i][j].Value = 0;
                                cells[i][j].Error = null;
                                dataGridView.Rows[i].Cells[j].Value = cells[i][j].Value.ToString();
                                cchangeData(cells[i][j].References[k].RowNumber, cells[i][j].References[k].ColumnLetter, false, false);
                            }
                        }
                        
                    }
                }
        }
            
        public void RemoveRow()
        {
            if (rowCount == 1) return;
            for(int i = 0; i < columnCount; i++)
            {
                if (cells[rowCount - 1][i].Value != 0)
                {
                    DialogResult res = MessageBox.Show("Ви дiйсно хочете видалити рядок? Всі незбережені дані можуть бути втрачені.", "Повідомлення", MessageBoxButtons.YesNo);
                    if (res == DialogResult.No) return;
                }
            }

            dataGridView.Rows.RemoveAt(rowCount - 1);
            cells.RemoveAt(rowCount - 1);

            for(int i = 0; i < rowCount - 1; i++)
            {
                for(int j = 0; j < columnCount; j++)
                {
                    if (cells[i][j].References.Where(a => a.RowNumber == rowCount).Count() != 0)
                    {
                        ChangeData(cells[i][j].Expression, i, j);
                        cchangeData(cells[i][j].RowNumber, cells[i][j].ColumnLetter, false, true);
                    }
                }
            }
            rowCount--;            
        }

        public void RemoveColumn()
        {
            if (columnCount == 1) return;
            for (int i = 0; i < rowCount; i++)
            {
                if (cells[i][columnCount - 1].Value != 0)
                {
                    DialogResult res = MessageBox.Show("Ви дiйсно хочете видалити стовпчик? Всі незбережені дані можуть бути втрачені.", "Повідомлення", MessageBoxButtons.YesNo);
                    if (res == DialogResult.No) return;
                }
            }
            dataGridView.Columns.RemoveAt(columnCount - 1);
            for(int i = 0; i<rowCount; i++)
            {
                cells[i].RemoveAt(columnCount - 1);
            }

            for(int i = 0; i < rowCount; i++)
            {
                for(int j = 0; j < columnCount - 1; j++)
                {
                    if (cells[i][j].References.Where(a => a.ColumnLetter == sys.ToSys(columnCount - 1)).Count() != 0)
                    {
                        ChangeData(cells[i][j].Expression, i, j);
                        cchangeData(cells[i][j].RowNumber, cells[i][j].ColumnLetter, true, true);
                    }
                }
            }
            columnCount--;
        }
        
        public void cchangeData(int Ri, string Cl, bool qq, bool qqq)
        {
            int a = 0, b = 0;
            if (qqq == true)
            {
                if (qq == true)
                {
                    a = 1;
                }
                else
                {
                    b = 1;
                }
            }
            for (int i = 0; i < rowCount - b; i++)
            {
                for (int j = 0; j < columnCount - a; j++)
                {
                    if (cells[i][j].References.Where(s => s.ColumnLetter == Cl && s.RowNumber == Ri).Count() != 0)
                    {
                        ChangeData(cells[i][j].Expression, i, j);
                    }
                }
            }
        }
        public void RefreshAll()
        {
            for(int i = 0; i < columnCount; i++)
            {
                for(int j = 0; j < columnCount; j++)
                {
                    ChangeData1(cells[i][j].Expression, i, j);
                }
            }
        }
        public void ChangeData(string expression,int row,int col)
        {
            try
            {
                cells[row][col].Expression = expression;
                cells[row][col].Value = parser.Evaluate(expression, cells[row][col]);
                cells[row][col].Error = null;
                RecalcReferenceCell(cells[row][col]);
            }
            catch(ParserException ex)
            {
                if (ex.Message == "Рекурсивнi посилання")
                {
                    Cell fakeCell = cells[ex.row][ex.col].References[0];
                    while (fakeCell != cells[ex.row][ex.col])
                    {
                        fakeCell.Error = ex.Message;
                        dataGridView.Rows[fakeCell.RowNumber - 1].Cells[sys.FromSys(fakeCell.ColumnLetter)].Value = ex.Message;
                        fakeCell = fakeCell.References[0];
                    }
                    fakeCell.Error = ex.Message;
                    dataGridView.Rows[fakeCell.RowNumber - 1].Cells[sys.FromSys(fakeCell.ColumnLetter)].Value = ex.Message;
                }
                
                cells[ex.row][ex.col].Error = ex.Message;
                dataGridView.Rows[ex.row].Cells[ex.col].Value = cells[ex.row][ex.col].Error;
            }
        }
        public void ChangeData1(string expression, int row, int col)
        {
            try
            {
                cells[row][col].Expression = expression;
                cells[row][col].Value = parser.Evaluate(expression, cells[row][col]);
                cells[row][col].Error = null;
            }
            catch (ParserException ex)
            {
                if (ex.Message == "Рекурсивнi посилання")
                {
                    Cell fakeCell = cells[ex.row][ex.col].References[0];
                    while (fakeCell != cells[ex.row][ex.col])
                    {
                        fakeCell.Error = ex.Message;
                        dataGridView.Rows[fakeCell.RowNumber - 1].Cells[sys.FromSys(fakeCell.ColumnLetter)].Value = ex.Message;
                        fakeCell = fakeCell.References[0];
                    }
                    fakeCell.Error = ex.Message;
                    dataGridView.Rows[fakeCell.RowNumber - 1].Cells[sys.FromSys(fakeCell.ColumnLetter)].Value = ex.Message;
                }

                cells[ex.row][ex.col].Error = ex.Message;
                dataGridView.Rows[ex.row].Cells[ex.col].Value = cells[ex.row][ex.col].Error;
            }
        }

        void RecalcReferenceCell(Cell cell)
        {
            for(int i = 0; i < rowCount; i++)
            {
                for(int j = 0; j <columnCount; j++)
                {
                    if (cells[i][j].Expression != null)
                    {
                        for(int k = 0; k < cells[i][j].References.Count; k++)
                        {
                            if (cells[i][j].References[k].RowNumber == cell.RowNumber && cells[i][j].References[k].ColumnLetter == cell.ColumnLetter)
                            {
                                cells[i][j].Value = parser.Evaluate(cells[i][j].Expression, cells[i][j]);
                                cells[i][j].Error = null;
                                dataGridView.Rows[i].Cells[j].Value = cells[i][j].Value.ToString();
                                RecalcReferenceCell(cells[i][j]);
                            }
                        }
                    }
                }
            }
        }
        public void FillData(Mode mode)
        {
            dataGridView.Rows.Clear();
            dataGridView.Columns.Clear();

            for (int i = 0; i < columnCount; i++)
                dataGridView.Columns.Add(sys.ToSys(i), sys.ToSys(i));

            dataGridView.Rows.Add(rowCount);

            for(int i = 0; i < rowCount; i++)
            {
                dataGridView.Rows[i].HeaderCell.Value = (i + 1).ToString();

                for(int j = 0; j < columnCount; j++)
                {
                    if (cells[i][j].Expression != null)
                    {
                        if (cells[i][j].Error != null)
                            dataGridView.Rows[i].Cells[j].Value = cells[i][j].Error.ToString();
                        else
                            dataGridView.Rows[i].Cells[j].Value = mode == Mode.Expression ? cells[i][j].Expression.ToString() : cells[i][j].Value.ToString();

                    }
                }
            }
        }
        public void SaveToFile(string path)
        {
            StreamWriter stream = new StreamWriter(path);
            stream.WriteLine(rowCount);
            stream.WriteLine(columnCount);
            for(int i = 0; i <rowCount; i++)
            {
                for(int j = 0; j <columnCount; j++)
                {
                    if (cells[i][j].Expression != null)
                    {
                        stream.WriteLine(i);
                        stream.WriteLine(j);
                        stream.WriteLine(cells[i][j].Expression);
                        stream.WriteLine(cells[i][j].Value);
                        if (cells[i][j].Error == null)
                            stream.WriteLine();
                        else
                            stream.WriteLine(cells[i][j].Error);
                    }
                }
            }
            stream.Close();
        }
        public void OpenFile(string path)
        {
            StreamReader stream = new StreamReader(path);
            DataGridView fileDataGridView = new DataGridView();
            rowCount = Convert.ToInt32(stream.ReadLine());
            columnCount = Convert.ToInt32(stream.ReadLine());
            fileDataGridView.ColumnCount = columnCount;
            fileDataGridView.RowCount = rowCount;
            while (!stream.EndOfStream)
            {
                int i = Convert.ToInt32(stream.ReadLine());
                int j = Convert.ToInt32(stream.ReadLine());
                cells[i][j].Expression = stream.ReadLine();
                cells[i][j].Value = Convert.ToDouble(stream.ReadLine());
                string error = stream.ReadLine();
                if (!string.IsNullOrEmpty(error))
                {
                    cells[i][j].Error = error;
                }
            }
            stream.Close();
        }
    }
}