using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Data;
using System;

    /* Unused Code - Might be useful for parsing strategies
    ** Source code at https://github.com/Bunny83/LSystem/blob/master/LogicExpressionParser.cs

    public interface ILogicResult
    {
        bool GetResult();
    }
    public interface INumberProvider
    {
        double GetNumber();
    }
    public interface ICommandParser
    {
        bool Parse(Parser aParser, string aCommand, out ValueProvider aResult);
    }

    #region logic gates
    public class CombineAnd : ILogicResult
    {
        public List<ILogicResult> inputs = new List<ILogicResult>();

        public bool GetResult()
        {
            for (int i = 0; i < inputs.Count; i++)
                if (!inputs[i].GetResult())
                    return false;
            return true;
        }
    }
    public class CombineOr : ILogicResult
    {
        public List<ILogicResult> inputs = new List<ILogicResult>();

        public bool GetResult()
        {
            for (int i = 0; i < inputs.Count; i++)
                if (inputs[i].GetResult())
                    return true;
            return false;
        }
    }
    public class CombineXor : ILogicResult
    {
        public List<ILogicResult> inputs = new List<ILogicResult>();

        public bool GetResult()
        {
            bool res = false;
            for (int i = 0; i < inputs.Count; i++)
                res ^= inputs[i].GetResult();
            return res;
        }
    }
    public class CombineNot : ILogicResult
    {
        public ILogicResult input;
        public bool GetResult() { return !input.GetResult(); }
    }
    #endregion logic gates

    #region Arithmetic gates

    public class OperationAdd : INumberProvider
    {
        public List<INumberProvider> inputs = new List<INumberProvider>();

        public double GetNumber()
        {
            double res = 0d;
            for(int i = 0; i < inputs.Count; i++)
                res += inputs[i].GetNumber();
            return res;
        }
        public OperationAdd(params INumberProvider[] aInputs)
        {
            for(int i = 0;i < aInputs.Length; i++)
            {
                var add = aInputs[i] as OperationAdd;
                if (add != null)
                    inputs.AddRange(add.inputs);
                else
                    inputs.Add(aInputs[i]);
            }
        }
    }

    public class OperationNegate : INumberProvider
    {
        public INumberProvider input;
        public double GetNumber()
        {
            return -input.GetNumber();
        }
        public OperationNegate(INumberProvider aInput)
        {
            input = aInput;
        }
    }

    public class OperationProduct : INumberProvider
    {
        public List<INumberProvider> inputs = new List<INumberProvider>();

        public double GetNumber()
        {
            double res = 1d;
            for (int i = 0; i < inputs.Count; i++)
                res *= inputs[i].GetNumber();
            return res;
        }
        public OperationProduct(params INumberProvider[] aInputs)
        {
            for (int i = 0; i < aInputs.Length; i++)
            {
                var add = aInputs[i] as OperationProduct;
                if (add != null)
                    inputs.AddRange(add.inputs);
                else
                    inputs.Add(aInputs[i]);
            }
        }
    }


    public class OperationReciprocal : INumberProvider
    {
        public INumberProvider input;
        public double GetNumber()
        {
            return 1d / input.GetNumber();
        }
        public OperationReciprocal(INumberProvider aInput)
        {
            input = aInput;
        }
    }

    public class OperationPower : INumberProvider
    {
        public INumberProvider value;
        public INumberProvider power;
        public double GetNumber()
        {
            return Math.Pow(value.GetNumber(), power.GetNumber());
        }
        public OperationPower(INumberProvider aValue, INumberProvider aPower)
        {
            value = aValue;
            power = aPower;
        }
    }
    public class CustomFunction : INumberProvider
    {
        private Func<ParameterList, double> m_Func;
        private ParameterList m_Params;
        public CustomFunction(Func<ParameterList, double> aFunc, ParameterList aParams)
        {
            m_Func = aFunc;
            m_Params = aParams;
        }
        public double GetNumber()
        {
            return m_Func(m_Params);
        }
    }



    #endregion Arithmetic gates

    #region compare gates
    public abstract class CompareStatement : ILogicResult
    {
        public bool GetResult() {  return Compare(op1.GetNumber(), op2.GetNumber()); }
        public INumberProvider op1;
        public INumberProvider op2;
        protected abstract bool Compare(double aOp1, double aOp2);
    }
    public class CompareEqual : CompareStatement
    {
        protected override bool Compare(double aOp1, double aOp2) { return aOp1 == aOp2; }
    }
    public class CompareNotEqual : CompareStatement
    {
        protected override bool Compare(double aOp1, double aOp2) { return aOp1 != aOp2; }
    }
    public class CompareGreater : CompareStatement
    {
        protected override bool Compare(double aOp1, double aOp2) { return aOp1 > aOp2; }
    }
    public class CompareGreaterOrEqual : CompareStatement
    {
        protected override bool Compare(double aOp1, double aOp2) { return aOp1 >= aOp2; }
    }
    public class CompareLower : CompareStatement
    {
        protected override bool Compare(double aOp1, double aOp2) { return aOp1 < aOp2; }
    }
    public class CompareLowerOrEqual : CompareStatement
    {
        protected override bool Compare(double aOp1, double aOp2) { return aOp1 <= aOp2; }
    }
    #endregion compare gates

    #region value providers
    public class ConstantNumber : INumberProvider
    {
        public double constantValue;
        public double GetNumber() { return constantValue; }
    }
    public class ConstantBool : ILogicResult
    {
        public bool constantValue;
        public bool GetResult() { return constantValue; }
    }
    public class DelegateNumber : INumberProvider
    {
        public System.Func<double> callback;
        public double GetNumber() { return callback(); }
    }
    public class DelegateBool : ILogicResult
    {
        public Func<bool> callback;
        public bool GetResult() { return callback(); }
    }
    public class NumberToBool : ILogicResult
    {
        public INumberProvider val;
        public bool GetResult() { return val.GetNumber() > 0; }
    }
    public class BoolToNumber : INumberProvider
    {
        public ILogicResult val;
        public double GetNumber() { return val.GetResult() ? 1d : 0d; }
    }

    public class ValueProvider : INumberProvider, ILogicResult
    {
        protected ILogicResult m_BoolVal = null;
        protected INumberProvider m_NumberVal = null;
        public virtual double GetNumber()
        {
            if (m_NumberVal != null)
                return m_NumberVal.GetNumber();
            if (m_BoolVal != null)
                return m_BoolVal.GetResult() ? 1 : 0;
            return 0d;
        }
        public virtual bool GetResult()
        {
            if (m_BoolVal != null)
                return m_BoolVal.GetResult();
            if (m_NumberVal != null)
                return m_NumberVal.GetNumber() > 0d;
            return false;
        }
        public virtual void Set(bool aValue)
        {
            var tmp = m_BoolVal as ConstantBool;
            if (tmp == null)
                tmp = new ConstantBool();
            tmp.constantValue = aValue;
            m_BoolVal = tmp;
            m_NumberVal = null;
        }
        public virtual void Set(double aValue)
        {
            var tmp = m_NumberVal as ConstantNumber;
            if (tmp == null)
                tmp = new ConstantNumber();
            tmp.constantValue = aValue;
            m_NumberVal = tmp;
            m_BoolVal = null;
        }
        public virtual void Set(Func<bool> aValue)
        {
            var tmp = m_BoolVal as DelegateBool;
            if (tmp == null)
                tmp = new DelegateBool();
            tmp.callback = aValue;
            m_BoolVal = tmp;
            m_NumberVal = null;
        }
        public virtual void Set(Func<double> aValue)
        {
            var tmp = m_NumberVal as DelegateNumber;
            if (tmp == null)
                tmp = new DelegateNumber();
            tmp.callback = aValue;
            m_NumberVal = tmp;
            m_BoolVal = null;
        }
    }

    public class ExpressionVariable : ValueProvider
    {
        public string Name { get; private set; }
        public ExpressionVariable(string aName)
        {
            Name = aName;
        }
    }

    public class ParameterList : INumberProvider
    {
        public List<INumberProvider> inputs = new List<INumberProvider>();
        public ParameterList() { }
        public ParameterList(INumberProvider aNumber)
        {
            inputs.Add(aNumber);
        }
        public double GetNumber()
        {
            if (inputs.Count > 0)
                return inputs[0].GetNumber();
            return 0d;
        }
        public double this[int aIndex]
        {
            get
            {
                if (aIndex < 0 || aIndex >= inputs.Count)
                    return 0d;
                return inputs[aIndex].GetNumber();
            }
        }
        public bool Exists(int aIndex)
        {
            return aIndex < inputs.Count && aIndex >= 0;
        }
    }

    public class LogicExpression : ILogicResult
    {
        public bool GetResult() { return expressionTree.GetResult(); }
        public ExpressionContext Context { get { return context; } }
        protected ILogicResult expressionTree;
        protected ExpressionContext context;
        public ExpressionVariable this[string aVarName]
        {
            get
            {
                return context[aVarName];
            }
        }
        public LogicExpression(ILogicResult aExpressionTree, ExpressionContext aContext)
        {
            expressionTree = aExpressionTree;
            context = aContext;
        }
    }

    public class NumberExpression : INumberProvider
    {
        public double GetNumber() { return expressionTree.GetNumber(); }
        public ExpressionContext Context { get { return context; } }
        protected INumberProvider expressionTree;
        protected ExpressionContext context;
        public ExpressionVariable this[string aVarName]
        {
            get
            {
                return context[aVarName];
            }
        }
        public NumberExpression(INumberProvider aExpressionTree, ExpressionContext aContext)
        {
            expressionTree = aExpressionTree;
            context = aContext;
        }
    }

    #endregion value providers

    #region Expression & Parsing Context
    public class ExpressionContext
    {
        protected Dictionary<string, ExpressionVariable> variables = new Dictionary<string, ExpressionVariable>();
        public ExpressionContext() : this(true) { }
        public ExpressionContext(bool aAddDefaultConstants)
        {
            if (aAddDefaultConstants)
                AddMathConstants();
        }
        public void AddMathConstants()
        {
            this["e"].Set(Math.E);
            this["pi"].Set(Math.PI);
            this["r2d"].Set(180d / Math.PI);
            this["d2r"].Set(Math.PI / 180d);
        }
        public virtual ExpressionVariable FindVariable(string aVarName)
        {
            ExpressionVariable res;
            if (variables.TryGetValue(aVarName, out res))
            {
                return res;
            }
            return null;
        }
        public virtual ExpressionVariable GetVariable(string aVarName)
        {
            ExpressionVariable res;
            if (!variables.TryGetValue(aVarName, out res))
            {
                res = new ExpressionVariable(aVarName);
                variables.Add(res.Name, res);
            }
            return res;
        }
        public virtual ExpressionVariable this[string aVarName]
        {
            get
            {
                return GetVariable(aVarName);
            }
        }
    }

    public class ParsingContext
    {
        private List<string> m_BracketHeap = new List<string>();
        private List<ValueProvider> m_Commands = new List<ValueProvider>();
        private List<ICommandParser> m_CommandParser = new List<ICommandParser>();
        private Dictionary<string, Func<ParameterList, double>> m_Functions = new Dictionary<string, Func<ParameterList, double>>();
        public ParsingContext() : this(true) { }
        public ParsingContext(bool aAddMathMethods)
        {
            if (aAddMathMethods)
                AddMathFunctions();
        }
        public void AddMathFunctions()
        {
            var rnd = new System.Random();
            AddFunction("sin", (p) => Math.Sin(p[0]));
            AddFunction("cos", (p) => Math.Cos(p[0]));
            AddFunction("tan", (p) => Math.Tan(p[0]));
            AddFunction("asin", (p) => Math.Asin(p[0]));
            AddFunction("acos", (p) => Math.Acos(p[0]));
            AddFunction("atan", (p) => Math.Atan(p[0]));
            AddFunction("atan2", (p) => Math.Atan2(p[0], p[1]));

            AddFunction("abs", (p) => Math.Abs(p[0]));
            AddFunction("floor", (p) => Math.Floor(p[0]));
            AddFunction("ceil", (p) => Math.Ceiling(p[0]));
            AddFunction("round", (p) => Math.Round(p[0]));
            AddFunction("min", (p) => Math.Min(p[0], p[1]));
            AddFunction("max", (p) => Math.Max(p[0], p[1]));

            AddFunction("exp", (p) => Math.Exp(p[0]));
            AddFunction("ln", (p) => Math.Log(p[0]));
            AddFunction("log10", (p) => Math.Log10(p[0]));
            AddFunction("sqrt", (p) => Math.Sqrt(p[0]));
            AddFunction("pow", (p) => Math.Pow(p[0], p[1]));

            AddFunction("lerp", (p) => { var s = p[0]; return s + (p[1] - s) * p[2]; });
            AddFunction("rand", (p) => {
                var p0 = p[0];
                if (p.Exists(1))
                    return p0 + rnd.NextDouble()*(p[1]-p0);
                else
                    return rnd.NextDouble()*p[0];
            });
            AddFunction("clamp", (p) => {
                var p0 = p[0];
                var p1 = p[0];
                var p2 = p[0];
                return p0 < p1 ? p1 : (p0 > p2) ? p2 : p0;
            });
            AddFunction("clamp01", (p) => {
                var p0 = p[0];
                return p0 < 0d ? 0d : (p0 > 1d) ? 1d : p0;
            });
        }
        public static int FindClosingBracket(string aText, int aStart, char aOpen, char aClose)
        {
            int counter = 0;
            for (int i = aStart; i < aText.Length; i++)
            {
                if (aText[i] == aOpen)
                    counter++;
                if (aText[i] == aClose)
                    counter--;
                if (counter == 0)
                    return i;
            }
            return -1;
        }
        private void SubstitudeBracket(ref string aExpression, int aIndex)
        {
            int closing = FindClosingBracket(aExpression, aIndex, '(', ')');
            if (closing > aIndex + 1)
            {
                string inner = aExpression.Substring(aIndex + 1, closing - aIndex - 1);
                m_BracketHeap.Add(inner);
                string sub = "$B" + (m_BracketHeap.Count - 1) + ";";
                aExpression = aExpression.Substring(0, aIndex) + sub + aExpression.Substring(closing + 1);
            }
            else throw new ParseException("Bracket not closed!");
        }
        private void SubstitudeCommand(Parser aParser, ref string aExpression, int aIndex)
        {
            int closing = FindClosingBracket(aExpression, aIndex, '{', '}');
            if (closing > aIndex + 1)
            {
                string inner = aExpression.Substring(aIndex + 1, closing - aIndex - 1).Trim();
                string sub = "$C" + (m_Commands.Count) + ";";
                m_Commands.Add(ParseCommand(aParser, inner));
                aExpression = aExpression.Substring(0, aIndex) + sub + aExpression.Substring(closing + 1);
            }
            else throw new ParseException("Bracket not closed!");
        }

        protected virtual ValueProvider ParseCommand(Parser aParser, string aCommand)
        {
            for (int i = 0; i < m_CommandParser.Count; i++)
            {
                ValueProvider result;
                if (m_CommandParser[i].Parse(aParser, aCommand, out result))
                    return result;
            }
            return new ValueProvider();
        }

        public virtual void Preprocess(Parser aParser, ref string aExpression)
        {
            aExpression = aExpression.Trim();
            int index = aExpression.IndexOf('{');
            while (index >= 0)
            {
                SubstitudeCommand(aParser, ref aExpression, index);
                index = aExpression.IndexOf('{');
            }
            index = aExpression.IndexOf('(');
            while (index >= 0)
            {
                SubstitudeBracket(ref aExpression, index);
                index = aExpression.IndexOf('(');
            }
        }
        private bool ParseToken(ref string aExpression, out char aTokenType, out int aIndex)
        {
            int index2a = aExpression.IndexOf("$");
            int index2b = aExpression.IndexOf(';');
            if (index2a >= 0 && index2b >= 3)
            {
                aTokenType = aExpression[index2a + 1];
                var inner = aExpression.Substring(index2a + 2, index2b - index2a - 2);

                if (int.TryParse(inner, out aIndex) && aIndex >= 0)
                {
                    return true;
                }
                else
                    throw new ParseException("Can't parse bracket substitution token");
            }
            aTokenType = '\0';
            aIndex = -1;
            return false;
        }
        public string GetBracket(ref string aExpression)
        {
            char type;
            int index;
            if (ParseToken(ref aExpression, out type, out index) && type == 'B' && index < m_BracketHeap.Count)
                return m_BracketHeap[index];
            return null;
        }
        public ValueProvider GetCommand(ref string aExpression)
        {
            char type;
            int index;
            if (ParseToken(ref aExpression, out type, out index) && type == 'C' && index < m_Commands.Count)
                return m_Commands[index];
            return null;
        }

        public Func<ParameterList, double> GetFunction(string fName)
        {
            Func<ParameterList, double> f;
            if (m_Functions.TryGetValue(fName, out f))
                return f;
            return null;
        }
        public void AddFunction(string aName, Func<ParameterList, double> aFunc)
        {
            if (m_Functions.ContainsKey(aName))
                m_Functions[aName] = aFunc;
            else
                m_Functions.Add(aName, aFunc);
        }
    }
    #endregion Expression & Parsing Context

    public class Parser
    {
        private ParsingContext m_ParsingContext;
        private ExpressionContext context;
        public ParsingContext ParsingContext { get { return m_ParsingContext; } set { m_ParsingContext = value; } }
        public ExpressionContext ExpressionContext { get { return context; } set { context = value; } }

        public Parser() : this(new ParsingContext()) { }
        public Parser(ParsingContext aParsingContext)
        {
            context = new ExpressionContext();
            m_ParsingContext = aParsingContext;
        }

        private ILogicResult ParseLogicResult(string aExpression, int aMaxRecursion)
        {
            --aMaxRecursion;
            m_ParsingContext.Preprocess(this, ref aExpression);
            aExpression = aExpression.Trim();
            if (aExpression.Contains(" or "))
            {
                string[] parts = aExpression.Split(new string[] { " or " }, StringSplitOptions.None);
                List<ILogicResult> exp = new List<ILogicResult>(parts.Length);
                for (int i = 0; i < parts.Length; i++)
                {
                    string s = parts[i].Trim();
                    if (!string.IsNullOrEmpty(s))
                        exp.Add(ParseLogicResult(s, aMaxRecursion));
                }
                if (exp.Count == 1)
                    return exp[0];
                return new CombineOr { inputs = exp };
            }
            else if (aExpression.Contains("||"))
            {
                string[] parts = aExpression.Split(new string[] { "||" }, StringSplitOptions.None);
                List<ILogicResult> exp = new List<ILogicResult>(parts.Length);
                for (int i = 0; i < parts.Length; i++)
                {
                    string s = parts[i].Trim();
                    if (!string.IsNullOrEmpty(s))
                        exp.Add(ParseLogicResult(s, aMaxRecursion));
                }
                if (exp.Count == 1)
                    return exp[0];
                return new CombineOr { inputs = exp };
            }
            if (aExpression.Contains(" xor "))
            {
                string[] parts = aExpression.Split(new string[] { " xor " }, StringSplitOptions.None);
                List<ILogicResult> exp = new List<ILogicResult>(parts.Length);
                for (int i = 0; i < parts.Length; i++)
                {
                    string s = parts[i].Trim();
                    if (!string.IsNullOrEmpty(s))
                        exp.Add(ParseLogicResult(s, aMaxRecursion));
                }
                if (exp.Count == 1)
                    return exp[0];
                return new CombineXor { inputs = exp };
            }
            else if (aExpression.Contains("^"))
            {
                string[] parts = aExpression.Split(new string[] { "^" }, StringSplitOptions.None);
                List<ILogicResult> exp = new List<ILogicResult>(parts.Length);
                for (int i = 0; i < parts.Length; i++)
                {
                    string s = parts[i].Trim();
                    if (!string.IsNullOrEmpty(s))
                        exp.Add(ParseLogicResult(s, aMaxRecursion));
                }
                if (exp.Count == 1)
                    return exp[0];
                return new CombineXor { inputs = exp };
            }
            else if (aExpression.Contains(" and "))
            {
                string[] parts = aExpression.Split(new string[] { " and " }, StringSplitOptions.None);
                List<ILogicResult> exp = new List<ILogicResult>(parts.Length);
                for (int i = 0; i < parts.Length; i++)
                {
                    string s = parts[i].Trim();
                    if (!string.IsNullOrEmpty(s))
                        exp.Add(ParseLogicResult(s, aMaxRecursion));
                }
                if (exp.Count == 1)
                    return exp[0];
                return new CombineAnd { inputs = exp };
            }
            else if (aExpression.Contains("&&"))
            {
                string[] parts = aExpression.Split(new string[] { "&&" }, StringSplitOptions.None);
                List<ILogicResult> exp = new List<ILogicResult>(parts.Length);
                for (int i = 0; i < parts.Length; i++)
                {
                    string s = parts[i].Trim();
                    if (!string.IsNullOrEmpty(s))
                        exp.Add(ParseLogicResult(s, aMaxRecursion));
                }
                if (exp.Count == 1)
                    return exp[0];
                return new CombineAnd { inputs = exp };
            }
            else if (aExpression.Contains("=="))
            {
                string[] parts = aExpression.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    throw new ParseException("== operator needs an expression on either side");
                return new CompareEqual { op1 = ParseNumber(parts[0].Trim(), aMaxRecursion), op2 = ParseNumber(parts[1].Trim(), aMaxRecursion) };
            }
            else if (aExpression.Contains("!="))
            {
                string[] parts = aExpression.Split(new string[] { "!=" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    throw new ParseException("!= operator needs an expression on either side");
                return new CompareNotEqual { op1 = ParseNumber(parts[0].Trim(), aMaxRecursion), op2 = ParseNumber(parts[1].Trim(), aMaxRecursion) };
            }
            else if (aExpression.Contains(">="))
            {
                string[] parts = aExpression.Split(new string[] { ">=" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    throw new ParseException(">= operator needs an expression on either side");
                return new CompareGreaterOrEqual { op1 = ParseNumber(parts[0].Trim(), aMaxRecursion), op2 = ParseNumber(parts[1].Trim(), aMaxRecursion) };
            }
            else if (aExpression.Contains(">"))
            {
                string[] parts = aExpression.Split(new string[] { ">" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    throw new ParseException("> operator needs an expression on either side");
                return new CompareGreater { op1 = ParseNumber(parts[0].Trim(), aMaxRecursion), op2 = ParseNumber(parts[1].Trim(), aMaxRecursion) };
            }
            else if (aExpression.Contains("<="))
            {
                string[] parts = aExpression.Split(new string[] { "<=" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    throw new ParseException("<= operator needs an expression on either side");
                return new CompareLowerOrEqual { op1 = ParseNumber(parts[0].Trim(), aMaxRecursion), op2 = ParseNumber(parts[1].Trim(), aMaxRecursion) };
            }
            else if (aExpression.Contains("<"))
            {
                string[] parts = aExpression.Split(new string[] { "<" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    throw new ParseException("< operator needs an expression on either side");
                return new CompareLower { op1 = ParseNumber(parts[0].Trim(), aMaxRecursion), op2 = ParseNumber(parts[1].Trim(), aMaxRecursion) };
            }
            else if (aExpression.StartsWith("not "))
            {
                return new CombineNot {  input = ParseLogicResult(aExpression.Substring(4), aMaxRecursion) };
            }
            else if (aExpression.StartsWith("!"))
            {
                return new CombineNot { input = ParseLogicResult(aExpression.Substring(1), aMaxRecursion) };
            }
            else if (aExpression == "true")
            {
                return new ConstantBool { constantValue = true };
            }
            else if (aExpression == "false")
            {
                return new ConstantBool { constantValue = false };
            }


            string bracketContent = m_ParsingContext.GetBracket(ref aExpression);
            if (!string.IsNullOrEmpty(bracketContent))
            {
                return ParseLogicResult(bracketContent, aMaxRecursion);
            }
            ValueProvider value = m_ParsingContext.GetCommand(ref aExpression);
            if (value != null)
            {
                return value;
            }

            if (ValidIdentifier(aExpression))
            {
                return context.GetVariable(aExpression.Trim());
            }

            if (aMaxRecursion > 0)
                return new NumberToBool { val = ParseNumber(aExpression, aMaxRecursion) };
            throw new ParseException("Unexpected end / expression");
        } // ParseLogicResult(string, int)

        private INumberProvider ParseNumber(string aExpression, int aMaxRecursion)
        {
            --aMaxRecursion;
            m_ParsingContext.Preprocess(this, ref aExpression);
            aExpression = aExpression.Trim();
            if (aExpression.Contains(","))
            {
                string[] parts = aExpression.Split(',');
                var paramList = new ParameterList();
                paramList.inputs.Capacity = parts.Length;
                for (int i = 0; i < parts.Length; i++)
                {
                    string s = parts[i].Trim();
                    if (!string.IsNullOrEmpty(s))
                        paramList.inputs.Add(ParseNumber(s, aMaxRecursion));
                }
                if (paramList.inputs.Count == 1)
                    return paramList.inputs[0];
                return paramList;
            }
            else if (aExpression.Contains("+"))
            {
                string[] parts = aExpression.Split('+');
                List<INumberProvider> exp = new List<INumberProvider>(parts.Length);
                for (int i = 0; i < parts.Length; i++)
                {
                    string s = parts[i].Trim();
                    if (!string.IsNullOrEmpty(s))
                        exp.Add(ParseNumber(s, aMaxRecursion));
                }
                if (exp.Count == 1)
                    return exp[0];
                return new OperationAdd(exp.ToArray());
            }
            else if (aExpression.Contains("-"))
            {
                string[] parts = aExpression.Split('-');
                List<INumberProvider> exp = new List<INumberProvider>(parts.Length);
                if (!string.IsNullOrEmpty(parts[0].Trim()))
                    exp.Add(ParseNumber(parts[0], aMaxRecursion));
                for (int i = 1; i < parts.Length; i++)
                {
                    string s = parts[i].Trim();
                    if (!string.IsNullOrEmpty(s))
                        exp.Add(new OperationNegate(ParseNumber(s, aMaxRecursion)));
                }
                if (exp.Count == 1)
                    return exp[0];
                return new OperationAdd(exp.ToArray());
            }
            else if (aExpression.Contains("*"))
            {
                string[] parts = aExpression.Split('*');
                List<INumberProvider> exp = new List<INumberProvider>(parts.Length);
                for (int i = 0; i < parts.Length; i++)
                {
                    exp.Add(ParseNumber(parts[i], aMaxRecursion));
                }
                if (exp.Count == 1)
                    return exp[0];
                return new OperationProduct(exp.ToArray());
            }
            else if (aExpression.Contains("/"))
            {
                string[] parts = aExpression.Split('/');
                List<INumberProvider> exp = new List<INumberProvider>(parts.Length);
                if (!string.IsNullOrEmpty(parts[0].Trim()))
                    exp.Add(ParseNumber(parts[0], aMaxRecursion));
                for (int i = 1; i < parts.Length; i++)
                {
                    string s = parts[i].Trim();
                    if (!string.IsNullOrEmpty(s))
                        exp.Add(new OperationReciprocal(ParseNumber(s, aMaxRecursion)));
                }
                return new OperationProduct(exp.ToArray());
            }
            else if (aExpression.Contains("^"))
            {
                int pos = aExpression.IndexOf('^');
                var val = ParseNumber(aExpression.Substring(0, pos), aMaxRecursion);
                var pow = ParseNumber(aExpression.Substring(pos + 1), aMaxRecursion);
                return new OperationPower(val, pow);
            }

            int p = aExpression.IndexOf("$B");
            if (p > 0)
            {
                string fName = aExpression.Substring(0, p).Trim();
                Func<ParameterList, double> func = m_ParsingContext.GetFunction(fName);
                if (func != null)
                {
                    aExpression = aExpression.Substring(p);
                    string inner = m_ParsingContext.GetBracket(ref aExpression);
                    var param = ParseNumber(inner, aMaxRecursion);
                    if (param is ParameterList)
                        return new CustomFunction(func, (ParameterList)param);
                    return new CustomFunction(func, new ParameterList(param));
                }
            }
            string bracketContent = m_ParsingContext.GetBracket(ref aExpression);

            if (!string.IsNullOrEmpty(bracketContent))
            {
                return ParseNumber(bracketContent, aMaxRecursion);
            }
            ValueProvider value = m_ParsingContext.GetCommand(ref aExpression);
            if (value != null)
            {
                return value;
            }

            double doubleValue;
            if (double.TryParse(aExpression.Trim(), out doubleValue))
            {
                return new ConstantNumber { constantValue = doubleValue };
            }

            if (ValidIdentifier(aExpression))
            {
                return context.GetVariable(aExpression.Trim());
            }

            if (aMaxRecursion > 0)
                return new BoolToNumber { val = ParseLogicResult(aExpression, aMaxRecursion) };
            throw new ParseException("Unexpected end / expression");
        } //ParseNumber(string, int)

        public LogicExpression Parse(string aExpressionString, ExpressionContext aContext = null)
        {
            var old = context;
            if (aContext != null)
                context = aContext;
            ILogicResult tree = ParseLogicResult(aExpressionString, 20);
            LogicExpression res = new LogicExpression(tree, context);
            if (aContext != null)
                context = old;
            return res;
        }

        public NumberExpression ParseNumber(string aExpressionString, ExpressionContext aContext = null)
        {
            var old = context;
            if (aContext != null)
                context = aContext;
            INumberProvider tree = ParseNumber(aExpressionString, 20);
            NumberExpression res = new NumberExpression(tree, context);
            if (aContext != null)
                context = old;
            return res;
        }

        private static bool ValidIdentifier(string aExpression)
        {
            aExpression = aExpression.Trim();
            if (string.IsNullOrEmpty(aExpression))
                return false;
            if (aExpression.Length < 1)
                return false;
            if (aExpression.Contains(" "))
                return false;
            //if (!"abcdefghijklmnopqrstuvwxyz_".Contains(aExpression.Substring(0, 1).ToLower()))
            //    return false;
            // this is better for performance and garbage generation
            char firstLetter = char.ToLower(aExpression[0]);
            if (firstLetter != '_' && (firstLetter < 'a' || firstLetter > 'z'))
                return false;
            return true;
        }
    }

    public class ParseException : Exception
    {
        public ParseException(string aMessage) : base(aMessage) { }
    } */

public class MainScript : MonoBehaviour
{
    public GameObject Bar;
    public GameObject Circle;
    public GameObject Canv;
    public InputField XStrategy;
    public InputField YStrategy;
    float sq3 = Mathf.Sqrt(3);
    float camSize = 7.5f;
    float minCamSize = 1.5f;
    Dictionary<Vector2Int, SpriteRenderer> CircleDict = new Dictionary<Vector2Int, SpriteRenderer>();
    HashSet<Vector2Int> Vertices = new HashSet<Vector2Int>();
    Stack<HashSet<Vector2Int>> OnFireStack = new Stack<HashSet<Vector2Int>>();
    Stack<HashSet<Vector2Int>> MoveStack = new Stack<HashSet<Vector2Int>>();
    Stack<char> MoveTypes = new Stack<char>();

    Vector2Int CircleToVec2(GameObject c) {
        float x = c.transform.position.x;
        float y = c.transform.position.y;
        return Vec(Mathf.RoundToInt(x * 2f / sq3),Mathf.RoundToInt(2*y));
    }

    int bmod(int a, int b) {
        return ((a % b) + b) % b;
    }

    void RegisterCircle(GameObject c) {
        CircleDict[CircleToVec2(c)] = (SpriteRenderer) c.GetComponent(typeof(SpriteRenderer));
    }

    void Neighborize(Vector2Int pos) {
        List<Vector2Int> Neigh = Neighbors(pos);
        foreach (Vector2Int neigh in Neigh) {
            if (CircleDict.ContainsKey(neigh)) {
                float midx = (float)(pos.x + neigh.x) / 2;
                float midy = (float)(pos.y + neigh.y) / 2;
                int orient = (pos.x - neigh.x) * (pos.y - neigh.y);
                float angle = orient == 0 ? 90 : (orient > 0 ? 30 : 150);
                Instantiate(Bar, new Vector3(midx * sq3 / 2f, midy / 2f,0),Quaternion.Euler(0,0,angle));
            }
        }
    }

    int growCount = 0;
    public void Grow() {
        growCount += 1;
        if (growCount % 2 == 0) {
            minCamSize += 1.5f;
            if (camSize < minCamSize) {
                camSize = minCamSize;
                Camera.main.orthographicSize = camSize;
                Canv.transform.localScale = new Vector3(camSize / 420f, camSize / 420f, 0);
            }
        }
        HashSet<Vector2Int> NewVertices = new HashSet<Vector2Int>();
        foreach (Vector2Int pos in Vertices) {
            List<Vector2Int> Neigh = Neighbors(pos);
            foreach (Vector2Int neigh in Neigh) {
                if (!CircleDict.ContainsKey(neigh)) {
                    float midx = (float)(pos.x + neigh.x) / 2;
                    float midy = (float)(pos.y + neigh.y) / 2;
                    float angle = pos.y == neigh.y ? 0 : 0;
                    RegisterCircle(Instantiate(Circle, new Vector3(neigh.x * sq3 / 2f, neigh.y / 2f,0),Quaternion.identity));
                    Vector2Int neighPos = Vec(neigh.x,neigh.y);
                    NewVertices.Add(neighPos);
                    Neighborize(neighPos);
                }
            }
        }
        Vertices.UnionWith(NewVertices);
    }

    List<Vector2Int> Neighbors(Vector2Int pos) {
        int x = pos.x;
        int y = pos.y;
        List<Vector2Int> result;
        if ((bmod(x,2) == 0 && bmod(y,6) == 0) || (bmod(x,2) == 1 && bmod(y,6) == 3)) {
            result = new List<Vector2Int>{Vec(x-1,y-1),Vec(x+1,y-1),Vec(x,y+2)};
        }
        else {
            result = new List<Vector2Int>{Vec(x-1,y+1),Vec(x+1,y+1),Vec(x,y-2)};
        }
        return result;
    }
    Vector2Int Vec(int x, int y) {
        Vector2Int ret = new Vector2Int(x,y);
        return ret;
    }

    public void Reset() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Start() {
        RegisterCircle(Instantiate(Circle, new Vector3(0,0,0),Quaternion.identity));
        Vertices.Add(Vec(0,0));
        for (int i = 0; i < 9; ++i) {
            Grow();
        }
        OnFireStack.Push(new HashSet<Vector2Int>{Vec(0,0)});
        MoveStack.Push(new HashSet<Vector2Int>());
        Render();
    }

    void Render() {
        foreach(var entry in CircleDict) {
            CircleDict[entry.Key].color = Color.gray;
        }
        HashSet<Vector2Int> OnFire = OnFireStack.Peek();
        foreach (Vector2Int pos in OnFire) {
            if (!CircleDict.ContainsKey(pos)) {
                Grow();
            }
            CircleDict[pos].color = Color.red;
        }
        HashSet<Vector2Int> Moves = MoveStack.Peek();
        foreach (Vector2Int pos in Moves) {
            CircleDict[pos].color = Color.green;
        }
    }

    public void Undo() {
        if (MoveTypes.Count != 0) {
            char last = MoveTypes.Pop();
            if (last == 'F') {
                OnFireStack.Pop();
            } else {
                MoveStack.Pop();
            }
            Render();
        }
    }

    public void SpreadFire() {
        HashSet<Vector2Int> NextFire = new HashSet<Vector2Int>();
        HashSet<Vector2Int> OnFire = OnFireStack.Peek();
        foreach (Vector2Int pos in OnFire) {
            NextFire.Add(pos);
            List<Vector2Int> Neigh = Neighbors(pos);
            foreach (Vector2Int n in Neigh) {
                if (!MoveStack.Peek().Contains(n)) {
                    NextFire.Add(n);
                }
            }
        }
        if (NextFire.Count > OnFire.Count) {
            MoveTypes.Push('F');
            OnFireStack.Push(NextFire);
            Render();
        }
    }

    void Block(Vector2Int nearestCircle) {
        if (!OnFireStack.Peek().Contains(nearestCircle) && !MoveStack.Peek().Contains(nearestCircle)) {
            MoveTypes.Push('M');
            CircleDict[nearestCircle].color = Color.green;
            HashSet<Vector2Int> Moves = MoveStack.Peek();
            HashSet<Vector2Int> NextMoves = new HashSet<Vector2Int>();
            foreach (Vector2Int pos in Moves) {
                NextMoves.Add(pos);
            }
            NextMoves.Add(nearestCircle);
            MoveStack.Push(NextMoves);
        }
    }

    /* Unused Code - Might be useful for applying strategies
    
    bool LogicEvaluate(string expr) {
        Parser parser = new Parser();
        bool val = ((LogicExpression)(parser.Parse(expr))).GetResult();
        return val;
    }

    int IntEvaluate(string expr) {
        Parser parser = new Parser();
        int val = (int)(((NumberExpression)(parser.ParseNumber(expr))).GetNumber());
        return val;
    }

    int Evaluate(string strat, int t) {
        try {
            string[] exprs = strat.Split('\n');
            foreach (string expold in exprs) {
                string expr = expold.Replace("t", t.ToString());
                int cut = -1;
                for (int j = 0; j < expr.Length; ++j) {
                    if (expr[j] == '-' && expr[j+1] == '>') {
                        cut = j;
                        break;
                    }
                }
                if (cut >= 0) {
                    string s1 = expr.Substring(0, cut);
                    string s2 = expr.Substring(cut + 2);
                    if (LogicEvaluate(s1)) {
                        return IntEvaluate(s2);
                    }
                } else {
                    return IntEvaluate(expr);
                }
            }
            return 0;
        } catch {
            return 0;
        }
    }

    public void ApplyStrategy() {
        int n = MoveStack.Count - 1;
        int x = Evaluate(XStrategy.text, n);
        int y = Evaluate(YStrategy.text, n);
        if (CircleDict.ContainsKey(Vec(x,y))) {
            Block(Vec(x,y));
        }
    } */

    void Update()
    {
        GameObject g = EventSystem.current.currentSelectedGameObject;
        if (g != null && g.GetComponent(typeof(InputField)) != null) {
            return;
        }
        if (Input.GetKeyDown("right")) {
            SpreadFire();
        } else if (Input.GetKeyDown("left")) {
            Undo();
        } else if (Input.GetKeyDown("r")) {
            Reset();
        } else if (Input.GetKeyDown("a") && false) {
            ApplyStrategy();
        } else if (Input.GetKeyDown("g")) {
            Grow();
        } else if (Input.GetMouseButton(0)) {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float x = worldPoint.x;
            float y = worldPoint.y;
            Vector2Int nearestCircle = Vec(Mathf.RoundToInt(x * 2f / sq3),Mathf.RoundToInt(2*y));
            if (CircleDict.ContainsKey(nearestCircle)) {
                float xt = nearestCircle.x * sq3 / 2f;
                float yt = nearestCircle.y / 2f;
                if ((x - xt) * (x - xt) + (y - yt) * (y - yt) <= 0.0625) { // A bit of clicking leeway is given
                    Block(nearestCircle);
                }
            }
        }
    }
}