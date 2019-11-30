using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using SpreadSheet;
namespace SpreadSheet
{

    enum Mode { Expression, Value }

    public class Data
    {
        private int columnCount = 10;
        private int rowCount = 10;
        Parser parser = new Parser();
        DataGridView dataGridView;

        public static List<List<Cell>> cells = new List<List<Cell>>();

        public Data(DataGridView _dataGridView)
        {
            dataGridView = _dataGridView;
            cells.Clear();
            for(int i = 0; i< rowCount; i++)
            {
                cells.Add(new List<Cell>());
                for(int j = 0; j < columnCount; j++)
                {
                    cells[i].Add(new Cell() { RowNumber = i + 1, ColumnLetter = Convert.ToChar('A' + j) });
                }
            }
        }
        public Cell Cell
        {
            get => default(Cell);
            set { }
        }
        public Praser Parser
        {
            get => default(Parser);
            set { }
        }

        public void AddRow()
        {
            cells.Add(new List<Cell>());
            for(int j = 0; j< columnCount; j++)
            {
                cells[cells.Count - 1].Add(new Cell() { RowNumber = rowCount + 1, ColumnLetter = Convert.ToChar('A' + j) });
            }
            dataGridView.Rows.Add(1);
            dataGridView.Rows[dataGridView.Rows.Count - 1].HeaderCell.Value = (dataGridView.Rows.Count).toString();
            rowCount++;
        }
        public void AddColumn()
        {
            if (columnCount > 25)
            {
                columnCount++;
                DataGridViewColumn column = new DataGridViewColumn();
                DataGridViewCell cell = new DataGridViewTextBoxCell();
                column.CellTemplate = cell;
                int k = dataGridView.ColumnCount - 1;
                string n = dataGridView.Columns[k].Name;
                Class26BaseSys f2 = new Class26BaseSys();
                column.Name = f2.ToSys(columnCount - 1);
                dataGridView.Columns.Add(column);
                dataGridView.Refresh();
            }
            else
            {
                for(int i = 0; i < rowCount; i++)
                {
                    cells[i].Add(new Cell() { RowNumber = i + 1, ColumnLetter = Convert.ToChar('A' + columnCount) });
                }

                dataGridView.Columns.Add(((char)('A' + columnCount)).ToString(), ((char)('A' + columnCount)).ToString());
                columnCount++;
            }
        }
            
        public void RemoveRow()
        {
            dataGridView.Rows.RemoveAt(rowCount - 1);
            cells.RemoveAt(rowCount - 1);

            for(int i = 0; i < rowCount - 1; i++)
            {
                for(int j = 0; j<columnCount; j++)
                {
                    if (cells[i][j].References.Where(a => a.RowNumber == rowCount).Count() != 0)
                    {
                        ChangeData(cells[i][j].Expression, i, j);
                    }
                }
            }

            rowCount--;
        }

        public void RemoveColumn()
        {
            dataGridView.Columns.RemoveAt(columnCount - 1);
            for(int i = 0; i<rowCount; i++)
            {
                cells[i].RemoveAt(columnCount - 1);
            }

            for(int i = 0; i < rowCount; i++)
            {
                for(int j = 0; j < columnCount - 1; j++)
                {
                    if (cells[i][j].References.Where(a=>a.ColumnLetter == 'A' + columnCount - 1).Count() != 0)
                    {
                        ChangeData(cells[i][j].Expression, i, j);
                    }
                }
            }

            columnCount--;
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
                cells[ex.row][ex.row].Error = ex.Message;
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
                        for(int k = 0; k <cells[i][j].References.Count; k++)
                        {
                            if (cells[i][j].References[k].RowNumber == cell.RowNumber && cells[i][j].References[k].ColumnLetter == cell.ColumnLetter)
                            {
                                cells[i][j].Value = pareser.Evaluate(cells[i][j].Expression, cells[i][j]);
                                cells[i][j].Error = null;
                                dataGridView.Rows[i].Cells[j].Value = cells[i][j].Value.Tostring();
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

            for (char i = 'A'; i < 'A' + columnCount; i++)
                dataGridView.Column.ADd(i.ToString(), i.ToString());

            dataGridView.Rows.Add(rowCount);

            for(int i = 0; i<rowCount; i++)
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