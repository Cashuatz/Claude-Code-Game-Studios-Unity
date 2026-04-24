using System;
using System.Globalization;
using System.Text;

namespace Proto.TD.Level
{
    /// <summary>
    /// 순수 C# 핸드롤 JSON 작성기. Unity / Newtonsoft 의존성 회피.
    /// level.schema.json 직렬화에 필요한 최소 기능만 제공.
    /// </summary>
    internal sealed class JsonWriter
    {
        private readonly StringBuilder _sb = new StringBuilder(4096);
        private readonly bool _pretty;
        private int _indent;
        private enum State { Root, ObjectStart, ObjectNext, ArrayStart, ArrayNext }
        private readonly System.Collections.Generic.Stack<State> _stack = new System.Collections.Generic.Stack<State>();

        public JsonWriter(bool pretty = true)
        {
            _pretty = pretty;
            _stack.Push(State.Root);
        }

        public override string ToString() => _sb.ToString();

        public void BeginObject()
        {
            PrefixValue();
            _sb.Append('{');
            _indent++;
            _stack.Push(State.ObjectStart);
        }

        public void EndObject()
        {
            var s = _stack.Pop();
            if (s != State.ObjectStart && s != State.ObjectNext)
                throw new InvalidOperationException("EndObject without BeginObject");
            _indent--;
            if (_pretty && s == State.ObjectNext) NewLine();
            _sb.Append('}');
            MarkAfterValue();
        }

        public void BeginArray()
        {
            PrefixValue();
            _sb.Append('[');
            _indent++;
            _stack.Push(State.ArrayStart);
        }

        public void EndArray()
        {
            var s = _stack.Pop();
            if (s != State.ArrayStart && s != State.ArrayNext)
                throw new InvalidOperationException("EndArray without BeginArray");
            _indent--;
            if (_pretty && s == State.ArrayNext) NewLine();
            _sb.Append(']');
            MarkAfterValue();
        }

        public void Key(string key)
        {
            var s = _stack.Peek();
            if (s != State.ObjectStart && s != State.ObjectNext)
                throw new InvalidOperationException("Key only valid inside object");
            if (s == State.ObjectNext) _sb.Append(',');
            if (_pretty) NewLine();
            WriteStringLiteral(key);
            _sb.Append(':');
            if (_pretty) _sb.Append(' ');
            _stack.Pop();
            _stack.Push(State.ObjectStart); // expecting value; after value MarkAfterValue flips to ObjectNext
        }

        public void Value(string v)
        {
            PrefixValue();
            if (v == null) _sb.Append("null");
            else WriteStringLiteral(v);
            MarkAfterValue();
        }

        public void Value(int v) { PrefixValue(); _sb.Append(v.ToString(CultureInfo.InvariantCulture)); MarkAfterValue(); }
        public void Value(long v) { PrefixValue(); _sb.Append(v.ToString(CultureInfo.InvariantCulture)); MarkAfterValue(); }
        public void Value(double v)
        {
            PrefixValue();
            if (double.IsNaN(v) || double.IsInfinity(v)) _sb.Append("null");
            else _sb.Append(v.ToString("R", CultureInfo.InvariantCulture));
            MarkAfterValue();
        }
        public void Value(bool v) { PrefixValue(); _sb.Append(v ? "true" : "false"); MarkAfterValue(); }
        public void ValueNull() { PrefixValue(); _sb.Append("null"); MarkAfterValue(); }

        private void PrefixValue()
        {
            var s = _stack.Peek();
            switch (s)
            {
                case State.ObjectStart:
                    // value expected after key; no prefix needed
                    break;
                case State.ArrayStart:
                    if (_pretty) NewLine();
                    break;
                case State.ArrayNext:
                    _sb.Append(',');
                    if (_pretty) NewLine();
                    break;
                case State.ObjectNext:
                    throw new InvalidOperationException("expected Key() after previous entry");
                case State.Root:
                    break;
            }
        }

        private void MarkAfterValue()
        {
            var s = _stack.Pop();
            switch (s)
            {
                case State.ObjectStart:
                    _stack.Push(State.ObjectNext);
                    break;
                case State.ArrayStart:
                    _stack.Push(State.ArrayNext);
                    break;
                case State.ArrayNext:
                    _stack.Push(State.ArrayNext);
                    break;
                case State.ObjectNext:
                    _stack.Push(State.ObjectNext);
                    break;
                case State.Root:
                    _stack.Push(State.Root);
                    break;
            }
        }

        private void NewLine()
        {
            _sb.Append('\n');
            for (int i = 0; i < _indent; i++) _sb.Append("  ");
        }

        private void WriteStringLiteral(string s)
        {
            _sb.Append('"');
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                switch (c)
                {
                    case '"': _sb.Append("\\\""); break;
                    case '\\': _sb.Append("\\\\"); break;
                    case '\b': _sb.Append("\\b"); break;
                    case '\f': _sb.Append("\\f"); break;
                    case '\n': _sb.Append("\\n"); break;
                    case '\r': _sb.Append("\\r"); break;
                    case '\t': _sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) _sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else _sb.Append(c);
                        break;
                }
            }
            _sb.Append('"');
        }
    }
}
