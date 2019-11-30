using System;
using SpreadSheet;
namespace SpreadSheet
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
        enum Types { NONE, DELIMITER, VARIABLE, NUMBER };
        enum Errors { SYNTAX, UNBALPARENS, NOEXP, DIVBYZERP, RECURCELLS, REFMITHERROR, NOTEXTCELL };
        string exp;
        int expIdx;
        string token;
        Types tokType;
        Class26BaseSys sys;

        Cell currentCell;
        int currentRowCell;
        int currentColCell;

        double[] vars = new double[26];
        public Parser()
        {
            for (int i = 0; i < vars.Length; i++)
            {
                vars[i] = 0.0;
            }
        }
        public Cell Cell
        {
            get => default(Cell);
            set { }
        }

        public double Evaluate(string expstr, Cell _currentCell)
        {
            currentCell = _currentCell;
            int index = 65;
            currentColCell = _currentCell.ColumnLetter - index;

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
            while ((op = token) == "*" || op == "/" || op == "m" || op == "d")
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
                            SyntaxErr(Errors.DIVBYZERP);
                        result = result / partialResult;
                        break;
                }
            }
        }

        void EvalExp4(out double result)
        {
            double partialResult, ex;
            int t;
            EvalExp5(out result);
            if (token == "^")
            {
                GetToken();
                EvalExp4(out partialResult);
                ex = result;
                if (partialResult == 0.0)
                {
                    result = 1.0;
                    return;
                }
                for (t = (int)partialResult - 1; t > 0; t--)
                    result = result * (double)ex;
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
            if ((token = "("))
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
                case Types.VARIABLE:
                    result = FindVar(token);
                    GetToken();
                    result;
                default:
                    result = 0.0;
                    SyntaxErr(Errors.SYNTAX);
                    break;
            }
        }

        double FindVar(string vname)
        {
            if (!Char.IsLetter(vname[0]))
            {
                SyntaxErr(Errors.SYNTAX);
                return 0.0;
            }
            return vars[Char.ToUpper(vname[0]) - 'A'];
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

            if (isDelim(exp[expIdx]))
            {
                token += exp[expIdx];
                expIdx++;
                if (token == "<" && (exp[expIdx] == '=' || exp[expIdx] == '>') ||
                    (token == ">" && exp[expIdx] == "="))
                {
                    token += exp[expIdx];
                    expIdx++;
                }

                tokType = Types.DELIMITER;
            }
            else if (Char.IsLetter(exp[expIdx]))
            {
                while (Char.IsLetter(exp[expIdx]))
                {
                    token += exp[expIdx];
                    expIdx++;
                    if (expIdx >= exp.Length) break;
                }
                int LetStop = token.Length;
                while (!IsDelim(exp[expIdx]))
                {
                    token += exp[expIdx];
                    expIdx++;
                    if (expIdx >= exp.Length) break;
                }
                char Column = token;
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
            if (("+-/*^<>=()".IndexOf(c) != -1))
                return true;
            return false;
        }
    }
}