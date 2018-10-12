using System;
using System.IO;
using System.Text;
using static compiler.CompilerCodes;

namespace compiler
{
    public class Semantic
    {
        StreamReader streamReader;
        char lookAhead;
        CompilerCodes token;
        string lexema;
        int ponteiro;
        string linhaFonte;
        int linhaAtual;
        int colunaAtual;
        string mensagemDeErro;
        StringBuilder tokensIdentificados = new StringBuilder();

        // Variaveis acrescentadas para o Sintatico
        StringBuilder regrasReconhecidas = new StringBuilder();
        CompilerCodes estadoCompilacao;

        // Variaveis criadas para o semantico
        string ultimoLexema;
        StringBuilder codigoPython = new StringBuilder();
        int nivelIdentacao = 0;
        string exp_0;
        string exp_1;
        string exp_2;
        string exp_3;
        string exp_alvo;
        SemanticStack pilhaSemantica = new SemanticStack();

        public Stream Run(Stream arqFonte)
        {
            var arqDestino = new StreamWriter(new MemoryStream());

            linhaAtual = 0;
            colunaAtual = 0;
            ponteiro = 0;
            linhaFonte = "";
            token = T_NULO;
            estadoCompilacao = E_SEM_ERROS;
            mensagemDeErro = "";
            tokensIdentificados.Append("Tokens reconhecidos: \n\n");
            regrasReconhecidas.Append("\n\nRegras reconhecidas: \n\n");
            streamReader = new StreamReader(arqFonte);

            // posiciono no primeiro token	
            movelookAhead();
            buscaProximoToken();
            analiseSintatica();
            arqDestino.Write(tokensIdentificados.ToString());
            arqDestino.Write(regrasReconhecidas.ToString());
            arqDestino.Write("\n\nStatus da Compilação:\n\n");
            arqDestino.Write(mensagemDeErro);
            var baseStream = new MemoryStream();
            arqDestino.BaseStream.CopyTo(baseStream);
            arqDestino.Close();

            showMessageDialog("Arquivo Salvo: " + arqDestino, "Salvando Arquivo");

            return baseStream;
        }

        void analiseSintatica()
        {

            p();

            if (estadoCompilacao == E_ERRO_LEXICO)
            {
                showMessageDialog(mensagemDeErro, "Erro Léxico!");
            }
            else if (estadoCompilacao == E_ERRO_SINTATICO)
            {
                showMessageDialog(mensagemDeErro, "Erro Sintático!");
            }
            else
            {
                showMessageDialog("Análise Sintática terminada sem erros", "Análise Sintática terminada!");
            }
        }

        // <P> ::= '}}' <comandos> '{{'  
        private void p()
        {
            if (token == T_ABREPROG)
            {
                buscaProximoToken();
                comandos();
                if (token == T_FECHAPROG)
                {
                    buscaProximoToken();
                }
                else
                {
                    registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nFECHA PROG {{ esperado, mas encontrei: " + lexema);
                }
            }
            else
            {
                registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nABRE PROG }} esperado, mas encontrei: " + lexema);
            }
        }

        // <comandos> ::= <comando> 'CABO' <comandos>
        //            |   <comando>    
        private void comandos()
        {
            comando();
            if (token == T_CABO)
            {
                buscaProximoToken();
                comandos();
            }
        }

        // Exemplo sem recursividade
        private void comandosAlternativo()
        {
            comando();

            while (token != T_CABO)
            {
                buscaProximoToken();
                comando();
            }
        }

        // <comando> ::= <cmd_atribuicao>
        //           |   <cmd_escrita> 
        //           |   <cmd_leitura>
        private void comando()
        {
            switch (token)
            {
                case T_ID: comandoAtribuicao(); break;
                case T_MEMOSTRA: comandoEscrita(); break;
                case T_MECAPTURA: comandoLeitura(); break;
                case T_MEREPETE: comandoPara(); break;
                default:
                    registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nComando não identificado va aprender a programar pois encontrei: " + lexema); break;
            }
        }

        //  <cmd_atribuicao> ::= <id> 'VIRA' <exp>
        private void comandoAtribuicao()
        {

            id();
            if (token == T_VIRA)
            {
                buscaProximoToken();
                expressao();
            }
            else
            {
                registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nVIRA esperado, mas encontrei: " + lexema);
            }
        }

        // <cmd_escrita> ::= 'MEMOSTRA'')' <exp> '('
        private void comandoEscrita()
        {
            if (token == T_MEMOSTRA)
            {
                buscaProximoToken();
                if (token == T_ABRE_PAR)
                {
                    buscaProximoToken();
                    expressao();
                    if (token == T_FECHA_PAR)
                    {
                        buscaProximoToken();
                    }
                    else
                    {
                        registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\n(*)Fecha parentesis esperado, mas encontrei: " + lexema);
                    }
                }
                else
                {
                    registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nAbre parentesis esperado, mas encontrei: " + lexema);
                }
            }
            else
            {
                registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nMEMOSTRA esperado, mas encontrei: " + lexema);
            }
        }

        // <cmd_leitura> ::= 'MECAPTURA' ')' <id> '('
        private void comandoLeitura()
        {
            if (token == T_MECAPTURA)
            {
                buscaProximoToken();
                if (token == T_ABRE_PAR)
                {
                    buscaProximoToken();
                    id();
                    if (token == T_FECHA_PAR)
                    {
                        buscaProximoToken();
                    }
                    else
                    {
                        registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\n(1)Fecha parentesis esperado, mas encontrei: " + lexema);
                    }
                }
                else
                {
                    registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nAbre parentesis esperado, mas encontrei: " + lexema);
                }
            }
            else
            {
                registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nMECAPTURA esperado, mas encontrei: " + lexema);
            }
        }

        // <cmd_para>       ::= 'MEREPETE' <id> '=>' <exp> '==>' <exp> ']' <comandos> '['
        private void comandoPara()
        {
            if (token == T_MEREPETE)
            {
                buscaProximoToken();
                id();
                if (token == T_REPETE_DE)
                {
                    buscaProximoToken();
                    expressao();
                    if (token == T_REPETE_PARA)
                    {
                        buscaProximoToken();
                        expressao();
                        if (token == T_ABRE_BLOCO)
                        {
                            buscaProximoToken();
                            comandos();
                            if (token == T_FECHA_BLOCO)
                            {
                                buscaProximoToken();
                            }
                            else
                            {
                                registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nFecha bloco esperado, mas encontrei: " + lexema);
                            }
                        }
                        else
                        {
                            registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nAbre bloco esperado, mas encontrei: " + lexema);
                        }
                    }
                    else
                    {
                        registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nRepete para esperado, mas encontrei: " + lexema);
                    }
                }
                else
                {
                    registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nRepete de esperado, mas encontrei: " + lexema);
                }
            }
            else
            {
                registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nMEREPETE esperado, mas encontrei: " + lexema);
            }
        }

        /*
         * <expcond>        ::= <exp> '<' <exp>
         *                  |   <exp> '<=' <exp>   
         *                  |   <exp> '<>' <exp>   
         *                  |   <exp> '>=' <exp>   
         *                  |   <exp> '>'  <exp>   
         *                  |   <exp> '==' <exp> 
         */
        private void expressaoCondicional()
        {
            expressao();
            if (token == T_MENOR || token == T_MENORIGUAL ||
                 token == T_DIFERENTE || token == T_MAIOR ||
                 token == T_MAIORIGUAL || token == T_IGUAL)
            {
                buscaProximoToken();
                expressao();
            }
            else
            {
                registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nExpressao esperada, mas encontrei: " + lexema);
            }
        }

        // <exp> ::= <exp> '+' <termo>
        //       |   <exp> '-' <termo>
        //       |   <termo>
        private void expressao()
        {
            termo();
            if (token == T_SOMA || token == T_SUBTRAI)
            {
                buscaProximoToken();
                expressao();
            }
        }

        private void expressaoAlternativa()
        {
            termo();
            while (token == T_SOMA || token == T_SUBTRAI)
            {
                buscaProximoToken();
                termo();
            }
        }

        // <termo> ::= <termo> '*' <fator>
        //         |   <termo> '/' <fator>
        //         |   <fator>
        private void termo()
        {
            fator();
            if (token == T_MULTIPLICA || token == T_DIVIDE)
            {
                buscaProximoToken();
                termo();
            }
        }

        // <fator> ::= <numero> | <id>  
        private void fator()
        {
            if (token == T_NUMERO)
            {
                numero();
            }
            else
            {
                id();
            }
        }

        // <id> ::= [A-Z][A-Z,0-9,_]*
        private void id()
        {
            if (token == T_ID)
            {
                buscaProximoToken();
            }
            else
            {
                registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nIdentificador esperado, mas encontrei: " + lexema);
            }
        }

        // <numero> ::= [0-9]+
        private void numero()
        {
            if (token == T_NUMERO)
            {
                buscaProximoToken();
            }
            else
            {
                registraErroSintatico("Erro Sintático na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nNUMERO esperado, mas encontrei: " + lexema);
            }
        }

        void movelookAhead()
        {

            if ((ponteiro + 1) > linhaFonte.Length)
            {

                linhaAtual++;
                ponteiro = 0;


                if ((linhaFonte = streamReader.ReadLine()) == null)
                {
                    lookAhead = (char)FIM_ARQUIVO;
                }
                else
                {

                    StringBuilder sbLinhaFonte = new StringBuilder(linhaFonte);
                    sbLinhaFonte.Append((char)13).Append((char)10);
                    linhaFonte = sbLinhaFonte.ToString();

                    lookAhead = linhaFonte[ponteiro];
                }
            }
            else
            {
                lookAhead = linhaFonte[ponteiro];
            }

            if ((lookAhead >= 'a') &&
                 (lookAhead <= 'z'))
            {
                lookAhead = (char)(lookAhead - 'a' + 'A');
            }

            ponteiro++;
            colunaAtual = ponteiro + 1;
        }

        void buscaProximoToken()
        {
            int i, j;

            StringBuilder sbLexema = new StringBuilder("");

            while ((lookAhead == 9) ||
                  (lookAhead == '\n') ||
                  (lookAhead == 8) ||
                  (lookAhead == 11) ||
                  (lookAhead == 12) ||
                  (lookAhead == '\r') ||
                  (lookAhead == 32))
            {
                movelookAhead();
            }

            /*--------------------------------------------------------------*
             * Caso o primeiro caracter seja alfabetico, procuro capturar a *
             * sequencia de caracteres que se segue a ele e classifica-la   *
             *--------------------------------------------------------------*/
            if ((lookAhead >= 'A') && (lookAhead <= 'Z'))
            {
                sbLexema.Append(lookAhead);
                movelookAhead();

                while (((lookAhead >= 'A') && (lookAhead <= 'Z')) ||
                        ((lookAhead >= '0') && (lookAhead <= '9')) ||
                          (lookAhead == '_'))
                {
                    sbLexema.Append(lookAhead);
                    movelookAhead();
                }

                lexema = sbLexema.ToString();

                /* Classifico o meu token como palavra reservada ou id */
                if (lexema.Equals("CABO"))
                    token = T_CABO;
                else if (lexema.Equals("MEMOSTRA"))
                    token = T_MEMOSTRA;
                else if (lexema.Equals("MECAPTURA"))
                    token = T_MECAPTURA;
                else if (lexema.Equals("MEREPETE"))
                    token = T_MEREPETE;
                else if (lexema.Equals("VIRA"))
                    token = T_VIRA;
                else
                {
                    token = T_ID;
                }
            }
            else if ((lookAhead >= '0') && (lookAhead <= '9'))
            {
                sbLexema.Append(lookAhead);
                movelookAhead();
                while ((lookAhead >= '0') && (lookAhead <= '9'))
                {
                    sbLexema.Append(lookAhead);
                    movelookAhead();
                }
                if (lookAhead == '.')
                {
                    sbLexema.Append(lookAhead);
                    movelookAhead();
                    while ((lookAhead >= '0') && (lookAhead <= '9'))
                    {
                        sbLexema.Append(lookAhead);
                        movelookAhead();
                    }
                }
                token = T_NUMERO;
            }
            else if (lookAhead == '}')
            {
                sbLexema.Append(lookAhead);
                movelookAhead();
                if (lookAhead == '}')
                {
                    sbLexema.Append(lookAhead);
                    movelookAhead();
                    token = T_ABREPROG;
                }
                else
                {
                    token = T_ERRO_LEX;
                }
            }
            else if (lookAhead == '{')
            {
                sbLexema.Append(lookAhead);
                movelookAhead();
                if (lookAhead == '{')
                {
                    sbLexema.Append(lookAhead);
                    movelookAhead();
                    token = T_FECHAPROG;
                }
                else
                {
                    token = T_ERRO_LEX;
                }
            }
            else if (lookAhead == '>')
            {
                sbLexema.Append(lookAhead);
                movelookAhead();
                if (lookAhead == '=')
                {
                    sbLexema.Append(lookAhead);
                    movelookAhead();
                    token = T_MAIORIGUAL;
                }
                else
                {
                    token = T_MAIOR;
                }
            }
            else if (lookAhead == '<')
            {
                sbLexema.Append(lookAhead);
                movelookAhead();
                if (lookAhead == '>')
                {
                    sbLexema.Append(lookAhead);
                    movelookAhead();
                    token = T_DIFERENTE;
                }
                else
                {
                    if (lookAhead == '=')
                    {
                        sbLexema.Append(lookAhead);
                        movelookAhead();
                        token = T_MENORIGUAL;
                    }
                    else
                    {
                        token = T_MENOR;
                    }
                }
            }
            else if (lookAhead == '=')
            {
                sbLexema.Append(lookAhead);
                movelookAhead();
                if (lookAhead == '=')
                {
                    sbLexema.Append(lookAhead);
                    movelookAhead();
                    if (lookAhead == '>')
                    {
                        sbLexema.Append(lookAhead);
                        movelookAhead();
                        token = T_REPETE_PARA;
                    }
                    else
                    {
                        token = T_IGUAL;
                    }
                }
                else if (lookAhead == '>')
                {
                    sbLexema.Append(lookAhead);
                    movelookAhead();
                    token = T_REPETE_DE;
                }
                else
                {
                    token = T_ERRO_LEX;
                    mensagemDeErro = "Erro Léxico na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nToken desconhecido: " + lookAhead;
                    sbLexema.Append(lookAhead);
                }
            }
            else if (lookAhead == ']')
            {
                sbLexema.Append(lookAhead);
                token = T_ABRE_BLOCO;
                movelookAhead();
            }
            else if (lookAhead == '[')
            {
                sbLexema.Append(lookAhead);
                token = T_FECHA_BLOCO;
                movelookAhead();
            }
            else if (lookAhead == '(')
            {
                sbLexema.Append(lookAhead);
                token = T_FECHA_PAR;
                movelookAhead();
            }
            else if (lookAhead == ')')
            {
                sbLexema.Append(lookAhead);
                token = T_ABRE_PAR;
                movelookAhead();
            }
            else if (lookAhead == '+')
            {
                sbLexema.Append(lookAhead);
                token = T_SOMA;
                movelookAhead();
            }
            else if (lookAhead == '-')
            {
                sbLexema.Append(lookAhead);
                token = T_SUBTRAI;
                movelookAhead();
            }
            else if (lookAhead == '*')
            {
                sbLexema.Append(lookAhead);
                token = T_MULTIPLICA;
                movelookAhead();
            }
            else if (lookAhead == '/')
            {
                sbLexema.Append(lookAhead);
                token = T_DIVIDE;
                movelookAhead();
            }
            else if (((int)lookAhead) == (int)FIM_ARQUIVO)
            {
                token = T_FIM_FONTE;
            }
            else
            {
                token = T_ERRO_LEX;
                mensagemDeErro = "Erro Léxico na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nToken desconhecido: " + lookAhead;
                sbLexema.Append(lookAhead);
            }

            lexema = sbLexema.ToString();
        }

        void mostraToken()
        {

            StringBuilder tokenLexema = new StringBuilder("");

            switch (token)
            {
                case T_ABREPROG: tokenLexema.Append("T_ABREPROG"); break;
                case T_FECHAPROG: tokenLexema.Append("T_FECHAPROG"); break;
                case T_ABRE_PAR: tokenLexema.Append("T_ABRE_PAR"); break;
                case T_FECHA_PAR: tokenLexema.Append("T_FECHA_PAR"); break;
                case T_ID: tokenLexema.Append("T_ID"); break;
                case T_NUMERO: tokenLexema.Append("T_NUMERO"); break;
                case T_CABO: tokenLexema.Append("T_CABO"); break;
                case T_MEMOSTRA: tokenLexema.Append("T_MEMOSTRA"); break;
                case T_MECAPTURA: tokenLexema.Append("T_MECAPTURA"); break;
                case T_VIRA: tokenLexema.Append("T_VIRA"); break;
                case T_SOMA: tokenLexema.Append("T_SOMA"); break;
                case T_SUBTRAI: tokenLexema.Append("T_SUBTRAI"); break;
                case T_DIVIDE: tokenLexema.Append("T_DIVIDE"); break;
                case T_MULTIPLICA: tokenLexema.Append("T_MULTIPLICA"); break;
                case T_MENOR: tokenLexema.Append("T_MENOR"); break;
                case T_MENORIGUAL: tokenLexema.Append("T_MENORIGUAL"); break;
                case T_DIFERENTE: tokenLexema.Append("T_DIFERENTE"); break;
                case T_MAIOR: tokenLexema.Append("T_MAIOR"); break;
                case T_MAIORIGUAL: tokenLexema.Append("T_MAIORIGUAL"); break;
                case T_IGUAL: tokenLexema.Append("T_IGUAL"); break;

                case T_MEREPETE: tokenLexema.Append("T_MEREPETE"); break;
                case T_REPETE_DE: tokenLexema.Append("T_REPETE_DE"); break;
                case T_REPETE_PARA: tokenLexema.Append("T_REPETE_PARA"); break;
                case T_ABRE_BLOCO: tokenLexema.Append("T_ABRE_BLOCO"); break;
                case T_FECHA_BLOCO: tokenLexema.Append("T_FECHA_BLOCO"); break;

                case T_FIM_FONTE: tokenLexema.Append("T_FIM_FONTE"); break;
                case T_ERRO_LEX: tokenLexema.Append("T_ERRO_LEX"); break;
                case T_NULO: tokenLexema.Append("T_NULO"); break;
                default: tokenLexema.Append("N/A"); break;
            }

            Console.WriteLine(tokenLexema.ToString() + " ( " + lexema + " )");

            acumulaToken(tokenLexema.ToString() + " ( " + lexema + " )");
            tokenLexema.Append(lexema);
        }

        private void acumulaToken(string tokenIdentificado)
        {
            tokensIdentificados.Append(tokenIdentificado);
            tokensIdentificados.Append("\n");
        }

        void registraErroSintatico(string msg)
        {
            if (estadoCompilacao == E_SEM_ERROS)
            {
                estadoCompilacao = E_ERRO_SINTATICO;
                mensagemDeErro = msg;
            }
        }

        private void showMessageDialog(string mensagemDeErro, string message)
        {
            Console.WriteLine(mensagemDeErro, message);
        }

        private void regraSemantica(int numeroRegra)
        {

            Console.WriteLine("Regra Semantica " + numeroRegra);

            switch (numeroRegra)
            {
                case 0:
                    codigoPython.Append("\n\nimport os\nimport sys\nimport glob\nimport string\n\n\n");
                    nivelIdentacao++;
                    codigoPython.Append("def main( ):\n");
                    codigoPython.Append(tabulacao(nivelIdentacao));
                    codigoPython.Append("'''ME2Python compiler�'''\n\n\n");
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
            }
        }

        private string tabulacao(int qtd)
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