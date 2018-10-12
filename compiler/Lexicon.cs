using System;
using System.Text;
using static compiler.CompilerCodes;

namespace compiler
{
    public class Lexicon
    {
        char lookAhead;
        CompilerCodes token;
        string lexema;
        int ponteiro;
        string linhaFonte;
        int linhaAtual;
        int colunaAtual;
        StringBuilder tokensIdentificados = new StringBuilder();
        private Compiler compiler;

        public Lexicon(Compiler compiler)
        {
            this.linhaAtual = 0;
            this.colunaAtual = 0;
            this.ponteiro = 0;
            this.linhaFonte = "";
            this.token = T_NULO;
            this.compiler = compiler;
        }

        public void movelookAhead()
        {

            if ((ponteiro + 1) > linhaFonte.Length)
            {

                linhaAtual++;
                ponteiro = 0;

                if ((linhaFonte = compiler.streamReader.ReadLine()) == null)
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

        public void buscaProximoToken()
        {
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
                    compiler.errorMessage = "Erro Léxico na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nToken desconhecido: " + lookAhead;
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
                compiler.errorMessage = "Erro Léxico na linha: " + linhaAtual + "\nReconhecido ao atingir a coluna: " + colunaAtual + "\nLinha do Erro: <" + linhaFonte + ">\nToken desconhecido: " + lookAhead;
                sbLexema.Append(lookAhead);
            }

            lexema = sbLexema.ToString();
        }

        private void acumulaToken(string tokenIdentificado)
        {
            tokensIdentificados.Append(tokenIdentificado);
            tokensIdentificados.Append("\n");
        }
    }
}