using System;
using System.Collections;
using System.Text;

namespace compiler
{
    public class SemanticStack
    {
        private Stack stack;
        private StringBuilder semanticStackOut;

        public SemanticStack()
        {
            stack = new Stack();
        }

        public SemanticStackNode Pop()
        {
            var nodo = (SemanticStackNode)stack.Pop();
            Console.WriteLine("Desempilhou " + nodo.Code);

            return (nodo);
        }

        public SemanticStackNode Push(String c, int r)
        {
            return this.Push(new SemanticStackNode(c, r));
        }

        public SemanticStackNode Push(SemanticStackNode nodo)
        {
            Console.WriteLine("Empilhou: " + nodo.Code);
            stack.Push(nodo);

            return (nodo);
        }
    }

    public class SemanticStackNode
    {

        public String Code { get; set; }
        public int PushedRule { get; set; }

        public SemanticStackNode(String code, int rule)
        {
            this.Code = code;
            this.PushedRule = rule;
        }
    }
}
