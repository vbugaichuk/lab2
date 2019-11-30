using System;
using SmartTable;
namespace SmartTable
{
    public class ParserException : ApplicationException
    {
        public int row;
        public int col;

        public ParserException(string str, int row, int col) : base(str)
        {
            this.row = row;
            this.col = col;
        }
        public override string ToString()
        {
            return Message;
        }
    }

    public class Parser
    {
        enum Types { NONE, DELIMITER, VARIABLE, NUMBER }; // лексеми
        enum Errors { SYNTAX, UNBALPARENS, NOEXP, DIVBYZERP, RECURCELLS, REFMITHERROR, NOTEXTCELL }; //помилки
        string exp; //рядок виразу
        int expIdx; //поточний індекс у виразі
        string token; //поточна лексема
        Types tokType; //тип лексеми
        Class26BaseSys sys= new Class26BaseSys();

        Cell currentCell;
        int currentRowCell;
        int currentColCell;

        public Parser()
        {
        }
        public Cell Cell
        {
            get => default(Cell);
            set { }
        }

        public double Evaluate(string expstr, Cell _currentCell)
        {
            currentCell = _currentCell;
            currentColCell = sys.FromSys(_currentCell.ColumnLetter);

            currentRowCell = _currentCell.RowNumber - 1;
            currentCell.References.Clear();
            double result;
            exp = expstr;
            expIdx = 0;

            GetToken();
            if (token == "")
            {
                SyntaxErr(Errors.NOEXP);
                return 0.0;
            }
            EvalExp1(out result);
            if (token != "")
                SyntaxErr(Errors.SYNTAX);
            if (CheckRecurInCells(currentCell))
            {
                SyntaxErr(Errors.RECURCELLS);
            }
            return result;
        }

        bool CheckRecurInCells(Cell cell)
        {
            foreach (var i in cell.References)
            {
                if (i == currentCell)
                    return true;
                if (CheckRecurInCells(i) == true)
                    return true;
            }
            return false;
        }

        void EvalExp1(out double result)
        {
            string op;
            double partialResult;
            EvalExp2(out result);
            while ((op = token) == "<" || op == ">" || op == "<=" || op == ">=" || op == "<>" || op == "=")
            {
                GetToken();
                EvalExp2(out partialResult);
                switch (op)
                {
                    case "<":
                        result = result < partialResult ? 1 : 0;
                        break;
                    case ">":
                        result = result > partialResult ? 1 : 0;
                        break;
                    case "<=":
                        result = result <= partialResult ? 1 : 0;
                        break;
                    case ">=":
                        result = result >= partialResult ? 1 : 0;
                        break;
                    case "<>":
                        result = result != partialResult ? 1 : 0;
                        break;
                    case "=":
                        result = result == partialResult ? 1 : 0;
                        break;
                }
            }
        }

        void EvalExp2(out double result)
        {
            string op;
            double partialResult;

            EvalExp3(out result);
            while ((op = token) == "+" || op == "-")
            {
                GetToken();
                EvalExp2(out partialResult);
                switch (op)
                {
                    case "-":
                        result = result - partialResult;
                        break;
                    case "+":
                        result = result + partialResult;
                        break;
                }
            }
        }

        void EvalExp3(out double result)
        {
            string op;
            double partialResult = 0.0;
            EvalExp4(out result);
            while ((op = token) == "*" || op == "/" || op == "%")
            {
                GetToken();
                EvalExp4(out partialResult);
                switch (op)
                {
                    case "*":
                        result = result * partialResult;
                        break;
                    case "/":
                        if (partialResult == 0.0)
                        {
                            SyntaxErr(Errors.DIVBYZERP);
                        }
                        result = result / partialResult;
                        break;
                    case "%":
                        if (partialResult == 0.0)
                            SyntaxErr(Errors.DIVBYZERP);
                        result = (int)result % (int)partialResult;
                        break;
                }
            }
        }

        void EvalExp4(out double result)
        {
            double partialResult, ex;
            //int t;
            EvalExp5(out result);
            if (token == "^")
            {
                GetToken();
                EvalExp4(out partialResult);
                ex = result;
                /*if (partialResult == 0.0)
                {
                    result = 1.0;
                    return;
                }
                */
                /*if (partialResult < 0.0)
                {
                    for (t = (int)partialResult - 1; t < 0; t++)
                        result = 1 / (result * (double)ex);
                    return;
                }
                */
                result = Math.Pow(result, partialResult);
                //for (t = (int)partialResult - 1; t > 0; t--)
                  //  result = result * (double)ex;
            }
        }

        void EvalExp5(out double result)
        {
            string op;
            op = "";
            if ((tokType == Types.DELIMITER) && token == "+" || token == "-")
            {
                op = token;
                GetToken();
            }
            EvalExp6(out result);
            if (op == "-") result = -result;
        }
        void EvalExp6(out double result)
        {
            if ((token == "("))
            {
                GetToken();
                EvalExp1(out result);
                if (token != ")")
                    SyntaxErr(Errors.UNBALPARENS);
                GetToken();
            }
            else Atom(out result);
        }

        void Atom(out double result)
        {
            switch (tokType)
            {
                case Types.NUMBER:
                    try
                    {
                        result = Double.Parse(token);
                    }
                    catch (FormatException)
                    {
                        result = 0.0;
                        SyntaxErr(Errors.SYNTAX);
                    }
                    GetToken();
                    return;
                default:
                    result = 0.0;
                    SyntaxErr(Errors.SYNTAX);
                    break;
            }
        }

        void PutBack()
        {
            for (int i = 0; i < token.Length; i++) expIdx--;
        }

        void SyntaxErr(Errors error)
        {
            string[] err = {
            "Синтаксична помилка",
            "Дисбаланс дужок",
            "Вираз вiдсутнiй",
            "Дiлення на нуль",
            "Рекурсивнi посилання",
            "Посилання на клiтинку з помилкою",
            "Посилання на неiснуючу клiтинку"
        };
            throw new ParserException(err[(int)error], currentRowCell, currentColCell);
        }
        void GetToken()
        {
            tokType = Types.NONE;
            token = "";

            while (expIdx < exp.Length && Char.IsWhiteSpace(exp[expIdx])) ++expIdx;

            if (expIdx == exp.Length) return;

            if (IsDelim(exp[expIdx]))
            {
                token += exp[expIdx];
                expIdx++;
                if (token == "<" && (exp[expIdx] == '=' || exp[expIdx] == '>') ||
                    (token == ">" && exp[expIdx] == '='))
                {
                    token += exp[expIdx];
                    expIdx++;
                }

                tokType = Types.DELIMITER;
            }
            else if (Char.IsLetter(exp[expIdx]))
            {
                if (expIdx < exp.Length)
                    while (Char.IsLetter(exp[expIdx]))
                    {
                        token += exp[expIdx];
                        expIdx++;
                        if (expIdx >= exp.Length) break;
                    }
                string Column = token;
                int LetStop = token.Length;
                if (expIdx < exp.Length)
                    while (!IsDelim(exp[expIdx]))
                    {
                        token += exp[expIdx];
                        expIdx++;
                        if (expIdx > exp.Length- 1) break;
                    }
                
                string Row = token.Substring(LetStop);
                int RowIndex;
                if (!int.TryParse(Row, out RowIndex))
                {
                    SyntaxErr(Errors.SYNTAX);
                }
                int rowIndex = RowIndex - 1;
                int columnIndex = sys.FromSys(Column);

                if (rowIndex >= Data.cells.Count || columnIndex >= Data.cells[rowIndex].Count)
                {
                    Cell notExistCell = new Cell() { RowNumber = rowIndex + 1, ColumnLetter = Column };
                    currentCell.References.Add(notExistCell);
                    SyntaxErr(Errors.NOTEXTCELL);
                }

                Cell parsedCell = Data.cells[rowIndex][columnIndex];
                currentCell.References.Add(parsedCell);
                if (!string.IsNullOrEmpty(parsedCell.Error))
                    SyntaxErr(Errors.REFMITHERROR);
                token = parsedCell.Value.ToString();

                tokType = Types.NUMBER;
            }
            else if (Char.IsDigit(exp[expIdx]))
            {
                while (!IsDelim(exp[expIdx]))
                {
                    token += exp[expIdx];
                    expIdx++;
                    if (expIdx >= exp.Length) break;
                }
                tokType = Types.NUMBER;
            }
        }
        bool IsDelim(char c)
        {
            if (("+-/*%^<>=()".IndexOf(c) != -1))
                return true;
            return false;
        }
    }
}