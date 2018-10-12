using System;
using System.IO;
using System.Text;
using static compiler.CompilerCodes;

namespace compiler
{
    public class Semantic
    {
        // Variaveis criadas para o semantico
        string ultimoLexema;
        StringBuilder codigoPython = new StringBuilder();
        int nivelIdentacao = 0;
        string exp_0;
        string exp_1;
        string exp_2;
        string exp_3;
        SemanticStack pilhaSemantica = new SemanticStack();
        private Compiler compiler;
        String simboloComparacao;

        public Semantic(Compiler compiler)
        {
            this.compiler = compiler;
        }

        public void regraSemantica(int numeroRegra)
        {
            Console.WriteLine("Regra Semantica " + numeroRegra);

            switch (numeroRegra)
            {
                case 0:
                    codigoPython.Append("\n\nimport os\nimport sys\nimport glob\nimport string\n\n\n");
                    nivelIdentacao++;
                    codigoPython.Append("def main( ):\n");
                    codigoPython.Append(tabulacao(nivelIdentacao));
                    codigoPython.Append("'''ME2Python compiler®'''\n\n\n");
                    break;
                case 1:
                    codigoPython.Append(tabulacao(nivelIdentacao));
                    codigoPython.Append("pass\n\n");
                    codigoPython.Append("if __name__ == '__main__':\n");
                    codigoPython.Append(tabulacao(nivelIdentacao));
                    codigoPython.Append("main( )\n");
                    break;
                case 2:
                    pilhaSemantica.Push(ultimoLexema.ToLower(), 2); // Empilha Identificador (variavel encontrada)
                    break;
                case 3:
                    pilhaSemantica.Push(ultimoLexema.ToLower(), 3); // Empilha Numero   		             
                    break;
                case 4:
                    exp_2 = pilhaSemantica.Pop().Code;
                    exp_1 = pilhaSemantica.Pop().Code;
                    exp_0 = exp_1 + " * " + exp_2;
                    pilhaSemantica.Push(exp_0, 4); // Empilha Termo
                    break;
                case 5:
                    exp_2 = pilhaSemantica.Pop().Code;
                    exp_1 = pilhaSemantica.Pop().Code;
                    exp_0 = exp_1 + " / " + exp_2;
                    pilhaSemantica.Push(exp_0, 5); // Empilha Termo
                    break;
                case 6:
                    exp_2 = pilhaSemantica.Pop().Code;
                    exp_1 = pilhaSemantica.Pop().Code;
                    exp_0 = exp_1 + " + " + exp_2;
                    pilhaSemantica.Push(exp_0, 6); // Empilha Termo
                    break;
                case 7:
                    exp_2 = pilhaSemantica.Pop().Code;
                    exp_1 = pilhaSemantica.Pop().Code;
                    exp_0 = exp_1 + " - " + exp_2;
                    pilhaSemantica.Push(exp_0, 7); // Empilha Termo
                    break;
                case 8:
                    exp_2 = pilhaSemantica.Pop().Code;
                    exp_1 = pilhaSemantica.Pop().Code;
                    exp_0 = exp_1 + " = " + exp_2 + "\n";
                    codigoPython.Append(tabulacao(nivelIdentacao));
                    codigoPython.Append(exp_0);
                    break;
                case 9:
                    exp_1 = pilhaSemantica.Pop().Code;
                    exp_0 = exp_1 + " = int ( raw_input( \"Informe a variavel " + exp_1 + ": \" ) ) \n";
                    codigoPython.Append(tabulacao(nivelIdentacao));
                    codigoPython.Append(exp_0);
                    break;
                case 10:
                    exp_3 = pilhaSemantica.Pop().Code;
                    exp_2 = pilhaSemantica.Pop().Code;
                    exp_1 = pilhaSemantica.Pop().Code;
                    exp_0 = "for " + exp_1 + " in range( " + exp_2 + ", " + exp_3 + "+1 ): \n";
                    codigoPython.Append(tabulacao(nivelIdentacao));
                    codigoPython.Append(exp_0);
                    nivelIdentacao++;
                    break;
                case 11:
                    nivelIdentacao--;
                    break;
                case 12:
                    exp_1 = pilhaSemantica.Pop().Code;
                    exp_0 = "print( " + exp_1 + " )\n";
                    codigoPython.Append(tabulacao(nivelIdentacao));
                    codigoPython.Append(exp_0);
                    break; // tipoDeComparacao
                case 13:
                    exp_2 = pilhaSemantica.Pop().Code;
                    exp_1 = pilhaSemantica.Pop().Code;
                    simboloComparacao = traduzSimbolo(compiler.tipoDeComparacao);
                    exp_0 = "( " + exp_1 + simboloComparacao + exp_2 + " )";
                    pilhaSemantica.Push(exp_0, 13); // Empilha expressao condicional
                    break;
                case 14:
                    exp_1 = pilhaSemantica.Pop().Code;
                    exp_0 = "if ( " + exp_1 + " ):\n";
                    codigoPython.Append(tabulacao(nivelIdentacao));
                    codigoPython.Append(exp_0);
                    nivelIdentacao++;
                    break;
                case 15:
                    nivelIdentacao--;
                    break;
                case 16:
                    codigoPython.Append(tabulacao(nivelIdentacao));
                    codigoPython.Append("else: \n");
                    nivelIdentacao++;
                    break;
                case 17:
                    nivelIdentacao--;
                    break;
                case 18:
                    exp_1 = pilhaSemantica.Pop().Code;
                    exp_0 = "while ( " + exp_1 + " ):\n";
                    codigoPython.Append(tabulacao(nivelIdentacao));
                    codigoPython.Append(exp_0);
                    nivelIdentacao++;
                    break;
                case 19:
                    nivelIdentacao--;
                    break;
                case 20:
                    exp_2 = pilhaSemantica.Pop().Code;
                    exp_1 = pilhaSemantica.Pop().Code;
                    exp_0 = exp_1 + " ** " + exp_2;
                    pilhaSemantica.Push(exp_0, 20); // Empilha Termo
                    break;
            }
        }

        private String traduzSimbolo(CompilerCodes tipoDeComparacao)
        {
            switch (tipoDeComparacao)
            {
                case T_MAIOR: return " > ";
                case T_MENOR: return " < ";
                case T_MAIORIGUAL: return " >= ";
                case T_MENORIGUAL: return " <= ";
                case T_IGUAL: return " == ";
                case T_DIFERENTE: return " != ";
            }

            return null;
        }

        String tabulacao(int qtd)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < qtd; i++)
            {
                sb.Append("    ");
            }

            return sb.ToString();
        }
    }
}