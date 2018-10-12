using System;
using System.Collections;
using System.Text;

namespace Compiler
{
    public class SemanticStack
    {
        private Stack _stack;
        private StringBuilder _semanticStackOut;

        public SemanticStack()
        {
            _stack = new Stack();
        }

        public SemanticStackNode Pop()
        {
            var nodo = (SemanticStackNode)_stack.Pop();
            Console.WriteLine("Desempilhou " + nodo.Code);

            return (nodo);
        }

        public SemanticStackNode Push(string c, int r)
        {
            return this.Push(new SemanticStackNode(c, r));
        }

        public SemanticStackNode Push(SemanticStackNode nodo)
        {
            Console.WriteLine("Empilhou: " + nodo.Code);
            _stack.Push(nodo);

            return (nodo);
        }
    }

    public class SemanticStackNode
    {

        public string Code { get; set; }
        public int PushedRule { get; set; }

        public SemanticStackNode(string code, int rule)
        {
            this.Code = code;
            this.PushedRule = rule;
        }
    }
}
