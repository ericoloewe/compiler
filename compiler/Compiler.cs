using System;
using System.IO;
using System.Text;
using static compiler.CompilerCodes;

namespace compiler
{
    public class Compiler
    {
        public StreamReader streamReader { get; set; }
        public string errorMessage { get; set; }
        public CompilerCodes compilationState { get; set; }
        StringBuilder tokensIdentificados = new StringBuilder();
        StringBuilder regrasReconhecidas = new StringBuilder();

        public Lexicon lexicon { get; set; }
        public Syntactic syntactic { get; set; }
        public Semantic semantic { get; set; }

        public Compiler()
        {
            lexicon = new Lexicon(this);
            syntactic = new Syntactic(this);
            semantic = new Semantic(this);
        }

        public Stream Run(Stream arqFonte)
        {
            var arqDestino = new StreamWriter(new MemoryStream());

            errorMessage = "";
            compilationState = E_SEM_ERROS;
            tokensIdentificados.Append("Tokens reconhecidos: \n\n");
            regrasReconhecidas.Append("\n\nRegras reconhecidas: \n\n");
            streamReader = new StreamReader(arqFonte);

            // posiciono no primeiro token	
            lexicon.movelookAhead();
            lexicon.buscaProximoToken();
            syntactic.analiseSintatica();
            checkCompilationErrors();
            arqDestino.Write(tokensIdentificados.ToString());
            arqDestino.Write(regrasReconhecidas.ToString());
            arqDestino.Write("\n\nStatus da Compilação:\n\n");
            arqDestino.Write(errorMessage);
            var baseStream = new MemoryStream();
            arqDestino.BaseStream.CopyTo(baseStream);
            arqDestino.Close();

            showMessageDialog("Arquivo Salvo: " + arqDestino, "Salvando Arquivo");

            return baseStream;
        }

        private void checkCompilationErrors()
        {
            if (compilationState == E_ERRO_LEXICO)
            {
                showMessageDialog(errorMessage, "Erro Léxico!");
            }
            else if (compilationState == E_ERRO_SINTATICO)
            {
                showMessageDialog(errorMessage, "Erro Sintático!");
            }
            else
            {
                showMessageDialog("Análise Sintática terminada sem erros", "Análise Sintática terminada!");
            }
        }

        public void showMessageDialog(string mensagemDeErro, string message)
        {
            Console.WriteLine(mensagemDeErro, message);
        }
    }
}