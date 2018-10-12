using System;
using System.IO;
using System.Text;
using static Compiler.CompilerCode;

namespace Compiler
{
    public class Syntactic
    {
        private StreamReader _streamReader;
        private char _lookAhead;
        private CompilerCode _token;
        private string _lexema;
        private int _ponteiro;
        private string _linhaFonte;
        private int _linhaAtual;
        private int _colunaAtual;
        private string _mensagemDeErro;
        private StringBuilder _tokensIdentificados = new StringBuilder();

        // Variaveis acrescentadas para o Sintatico
        private StringBuilder _regrasReconhecidas = new StringBuilder();
        private CompilerCode _estadoCompilacao;

        public Stream Run(Stream arqFonte)
        {
            var arqDestino = new StreamWriter(new MemoryStream());

            _linhaAtual = 0;
            _colunaAtual = 0;
            _ponteiro = 0;
            _linhaFonte = "";
            _token = Nulo;
            _estadoCompilacao = ESemErros;
            _mensagemDeErro = "";
            _tokensIdentificados.Append("Tokens reconhecidos: \n\n");
            _regrasReconhecidas.Append("\n\nRegras reconhecidas: \n\n");
            _streamReader = new StreamReader(arqFonte);

            // posiciono no primeiro token	
            MovelookAhead();
            BuscaProximoToken();
            AnaliseSintatica();
            arqDestino.Write(_tokensIdentificados.ToString());
            arqDestino.Write(_regrasReconhecidas.ToString());
            arqDestino.Write("\n\nStatus da Compilação:\n\n");
            arqDestino.Write(_mensagemDeErro);
            arqDestino.Close();

            ShowMessageDialog("Arquivo Salvo: " + arqDestino, "Salvando Arquivo");

            return arqDestino.BaseStream;
        }

        private void AnaliseSintatica()
        {

            P();

            if (_estadoCompilacao == EErroLexico)
            {
                ShowMessageDialog(_mensagemDeErro, "Erro Léxico!");
            }
            else if (_estadoCompilacao == EErroSintatico)
            {
                ShowMessageDialog(_mensagemDeErro, "Erro Sintático!");
            }
            else
            {
                ShowMessageDialog("Análise Sintática terminada sem erros", "Análise Sintática terminada!");
            }
        }

        // <P> ::= '}}' <comandos> '{{'  
        private void P()
        {
            if (_token == Abreprog)
            {
                BuscaProximoToken();
                Comandos();
                if (_token == Fechaprog)
                {
                    BuscaProximoToken();
                }
                else
                {
                    RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nFECHA PROG {{ esperado, mas encontrei: " + _lexema);
                }
            }
            else
            {
                RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nABRE PROG }} esperado, mas encontrei: " + _lexema);
            }
        }

        // <comandos> ::= <comando> 'CABO' <comandos>
        //            |   <comando>    
        private void Comandos()
        {
            Comando();
            if (_token == Cabo)
            {
                BuscaProximoToken();
                Comandos();
            }
        }

        // Exemplo sem recursividade
        private void ComandosAlternativo()
        {
            Comando();

            while (_token != Cabo)
            {
                BuscaProximoToken();
                Comando();
            }
        }

        // <comando> ::= <cmd_atribuicao>
        //           |   <cmd_escrita> 
        //           |   <cmd_leitura>
        private void Comando()
        {
            switch (_token)
            {
                case CompilerCode.Id: ComandoAtribuicao(); break;
                case Memostra: ComandoEscrita(); break;
                case Mecaptura: ComandoLeitura(); break;
                case Merepete: ComandoPara(); break;
                default:
                    RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nComando não identificado va aprender a programar pois encontrei: " + _lexema); break;
            }
        }

        //  <cmd_atribuicao> ::= <id> 'VIRA' <exp>
        private void ComandoAtribuicao()
        {

            Id();
            if (_token == Vira)
            {
                BuscaProximoToken();
                Expressao();
            }
            else
            {
                RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nVIRA esperado, mas encontrei: " + _lexema);
            }
        }

        // <cmd_escrita> ::= 'MEMOSTRA'')' <exp> '('
        private void ComandoEscrita()
        {
            if (_token == Memostra)
            {
                BuscaProximoToken();
                if (_token == AbrePar)
                {
                    BuscaProximoToken();
                    Expressao();
                    if (_token == FechaPar)
                    {
                        BuscaProximoToken();
                    }
                    else
                    {
                        RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\n(*)Fecha parentesis esperado, mas encontrei: " + _lexema);
                    }
                }
                else
                {
                    RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nAbre parentesis esperado, mas encontrei: " + _lexema);
                }
            }
            else
            {
                RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nMEMOSTRA esperado, mas encontrei: " + _lexema);
            }
        }

        // <cmd_leitura> ::= 'MECAPTURA' ')' <id> '('
        private void ComandoLeitura()
        {
            if (_token == Mecaptura)
            {
                BuscaProximoToken();
                if (_token == AbrePar)
                {
                    BuscaProximoToken();
                    Id();
                    if (_token == FechaPar)
                    {
                        BuscaProximoToken();
                    }
                    else
                    {
                        RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\n(1)Fecha parentesis esperado, mas encontrei: " + _lexema);
                    }
                }
                else
                {
                    RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nAbre parentesis esperado, mas encontrei: " + _lexema);
                }
            }
            else
            {
                RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nMECAPTURA esperado, mas encontrei: " + _lexema);
            }
        }

        // <cmd_para>       ::= 'MEREPETE' <id> '=>' <exp> '==>' <exp> ']' <comandos> '['
        private void ComandoPara()
        {
            if (_token == Merepete)
            {
                BuscaProximoToken();
                Id();
                if (_token == RepeteDe)
                {
                    BuscaProximoToken();
                    Expressao();
                    if (_token == RepetePara)
                    {
                        BuscaProximoToken();
                        Expressao();
                        if (_token == AbreBloco)
                        {
                            BuscaProximoToken();
                            Comandos();
                            if (_token == FechaBloco)
                            {
                                BuscaProximoToken();
                            }
                            else
                            {
                                RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nFecha bloco esperado, mas encontrei: " + _lexema);
                            }
                        }
                        else
                        {
                            RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nAbre bloco esperado, mas encontrei: " + _lexema);
                        }
                    }
                    else
                    {
                        RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nRepete para esperado, mas encontrei: " + _lexema);
                    }
                }
                else
                {
                    RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nRepete de esperado, mas encontrei: " + _lexema);
                }
            }
            else
            {
                RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nMEREPETE esperado, mas encontrei: " + _lexema);
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
        private void ExpressaoCondicional()
        {
            Expressao();
            if (_token == Menor || _token == Menorigual ||
                 _token == Diferente || _token == Maior ||
                 _token == Maiorigual || _token == Igual)
            {
                BuscaProximoToken();
                Expressao();
            }
            else
            {
                RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nExpressao esperada, mas encontrei: " + _lexema);
            }
        }

        // <exp> ::= <exp> '+' <termo>
        //       |   <exp> '-' <termo>
        //       |   <termo>
        private void Expressao()
        {
            Termo();
            if (_token == Soma || _token == Subtrai)
            {
                BuscaProximoToken();
                Expressao();
            }
        }

        private void ExpressaoAlternativa()
        {
            Termo();
            while (_token == Soma || _token == Subtrai)
            {
                BuscaProximoToken();
                Termo();
            }
        }

        // <termo> ::= <termo> '*' <fator>
        //         |   <termo> '/' <fator>
        //         |   <fator>
        private void Termo()
        {
            Fator();
            if (_token == Multiplica || _token == Divide)
            {
                BuscaProximoToken();
                Termo();
            }
        }

        // <fator> ::= <numero> | <id>  
        private void Fator()
        {
            if (_token == CompilerCode.Numero)
            {
                Numero();
            }
            else
            {
                Id();
            }
        }

        // <id> ::= [A-Z][A-Z,0-9,_]*
        private void Id()
        {
            if (_token == CompilerCode.Id)
            {
                BuscaProximoToken();
            }
            else
            {
                RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nIdentificador esperado, mas encontrei: " + _lexema);
            }
        }

        // <numero> ::= [0-9]+
        private void Numero()
        {
            if (_token == CompilerCode.Numero)
            {
                BuscaProximoToken();
            }
            else
            {
                RegistraErroSintatico("Erro Sintático na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nNUMERO esperado, mas encontrei: " + _lexema);
            }
        }

        private void MovelookAhead()
        {

            if ((_ponteiro + 1) > _linhaFonte.Length)
            {

                _linhaAtual++;
                _ponteiro = 0;


                if ((_linhaFonte = _streamReader.ReadLine()) == null)
                {
                    _lookAhead = (char)FimArquivo;
                }
                else
                {

                    StringBuilder sbLinhaFonte = new StringBuilder(_linhaFonte);
                    sbLinhaFonte.Append((char)13).Append((char)10);
                    _linhaFonte = sbLinhaFonte.ToString();

                    _lookAhead = _linhaFonte[_ponteiro];
                }
            }
            else
            {
                _lookAhead = _linhaFonte[_ponteiro];
            }

            if ((_lookAhead >= 'a') &&
                 (_lookAhead <= 'z'))
            {
                _lookAhead = (char)(_lookAhead - 'a' + 'A');
            }

            _ponteiro++;
            _colunaAtual = _ponteiro + 1;
        }

        private void BuscaProximoToken()
        {
            int i, j;

            StringBuilder sbLexema = new StringBuilder("");

            while ((_lookAhead == 9) ||
                  (_lookAhead == '\n') ||
                  (_lookAhead == 8) ||
                  (_lookAhead == 11) ||
                  (_lookAhead == 12) ||
                  (_lookAhead == '\r') ||
                  (_lookAhead == 32))
            {
                MovelookAhead();
            }

            /*--------------------------------------------------------------*
             * Caso o primeiro caracter seja alfabetico, procuro capturar a *
             * sequencia de caracteres que se segue a ele e classifica-la   *
             *--------------------------------------------------------------*/
            if ((_lookAhead >= 'A') && (_lookAhead <= 'Z'))
            {
                sbLexema.Append(_lookAhead);
                MovelookAhead();

                while (((_lookAhead >= 'A') && (_lookAhead <= 'Z')) ||
                        ((_lookAhead >= '0') && (_lookAhead <= '9')) ||
                          (_lookAhead == '_'))
                {
                    sbLexema.Append(_lookAhead);
                    MovelookAhead();
                }

                _lexema = sbLexema.ToString();

                /* Classifico o meu token como palavra reservada ou id */
                if (_lexema.Equals("CABO"))
                    _token = Cabo;
                else if (_lexema.Equals("MEMOSTRA"))
                    _token = Memostra;
                else if (_lexema.Equals("MECAPTURA"))
                    _token = Mecaptura;
                else if (_lexema.Equals("MEREPETE"))
                    _token = Merepete;
                else if (_lexema.Equals("VIRA"))
                    _token = Vira;
                else
                {
                    _token = CompilerCode.Id;
                }
            }
            else if ((_lookAhead >= '0') && (_lookAhead <= '9'))
            {
                sbLexema.Append(_lookAhead);
                MovelookAhead();
                while ((_lookAhead >= '0') && (_lookAhead <= '9'))
                {
                    sbLexema.Append(_lookAhead);
                    MovelookAhead();
                }
                if (_lookAhead == '.')
                {
                    sbLexema.Append(_lookAhead);
                    MovelookAhead();
                    while ((_lookAhead >= '0') && (_lookAhead <= '9'))
                    {
                        sbLexema.Append(_lookAhead);
                        MovelookAhead();
                    }
                }
                _token = CompilerCode.Numero;
            }
            else if (_lookAhead == '}')
            {
                sbLexema.Append(_lookAhead);
                MovelookAhead();
                if (_lookAhead == '}')
                {
                    sbLexema.Append(_lookAhead);
                    MovelookAhead();
                    _token = Abreprog;
                }
                else
                {
                    _token = ErroLex;
                }
            }
            else if (_lookAhead == '{')
            {
                sbLexema.Append(_lookAhead);
                MovelookAhead();
                if (_lookAhead == '{')
                {
                    sbLexema.Append(_lookAhead);
                    MovelookAhead();
                    _token = Fechaprog;
                }
                else
                {
                    _token = ErroLex;
                }
            }
            else if (_lookAhead == '>')
            {
                sbLexema.Append(_lookAhead);
                MovelookAhead();
                if (_lookAhead == '=')
                {
                    sbLexema.Append(_lookAhead);
                    MovelookAhead();
                    _token = Maiorigual;
                }
                else
                {
                    _token = Maior;
                }
            }
            else if (_lookAhead == '<')
            {
                sbLexema.Append(_lookAhead);
                MovelookAhead();
                if (_lookAhead == '>')
                {
                    sbLexema.Append(_lookAhead);
                    MovelookAhead();
                    _token = Diferente;
                }
                else
                {
                    if (_lookAhead == '=')
                    {
                        sbLexema.Append(_lookAhead);
                        MovelookAhead();
                        _token = Menorigual;
                    }
                    else
                    {
                        _token = Menor;
                    }
                }
            }
            else if (_lookAhead == '=')
            {
                sbLexema.Append(_lookAhead);
                MovelookAhead();
                if (_lookAhead == '=')
                {
                    sbLexema.Append(_lookAhead);
                    MovelookAhead();
                    if (_lookAhead == '>')
                    {
                        sbLexema.Append(_lookAhead);
                        MovelookAhead();
                        _token = RepetePara;
                    }
                    else
                    {
                        _token = Igual;
                    }
                }
                else if (_lookAhead == '>')
                {
                    sbLexema.Append(_lookAhead);
                    MovelookAhead();
                    _token = RepeteDe;
                }
                else
                {
                    _token = ErroLex;
                    _mensagemDeErro = "Erro Léxico na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nToken desconhecido: " + _lookAhead;
                    sbLexema.Append(_lookAhead);
                }
            }
            else if (_lookAhead == ']')
            {
                sbLexema.Append(_lookAhead);
                _token = AbreBloco;
                MovelookAhead();
            }
            else if (_lookAhead == '[')
            {
                sbLexema.Append(_lookAhead);
                _token = FechaBloco;
                MovelookAhead();
            }
            else if (_lookAhead == '(')
            {
                sbLexema.Append(_lookAhead);
                _token = FechaPar;
                MovelookAhead();
            }
            else if (_lookAhead == ')')
            {
                sbLexema.Append(_lookAhead);
                _token = AbrePar;
                MovelookAhead();
            }
            else if (_lookAhead == '+')
            {
                sbLexema.Append(_lookAhead);
                _token = Soma;
                MovelookAhead();
            }
            else if (_lookAhead == '-')
            {
                sbLexema.Append(_lookAhead);
                _token = Subtrai;
                MovelookAhead();
            }
            else if (_lookAhead == '*')
            {
                sbLexema.Append(_lookAhead);
                _token = Multiplica;
                MovelookAhead();
            }
            else if (_lookAhead == '/')
            {
                sbLexema.Append(_lookAhead);
                _token = Divide;
                MovelookAhead();
            }
            else if (((int)_lookAhead) == (int)FimArquivo)
            {
                _token = FimFonte;
            }
            else
            {
                _token = ErroLex;
                _mensagemDeErro = "Erro Léxico na linha: " + _linhaAtual + "\nReconhecido ao atingir a coluna: " + _colunaAtual + "\nLinha do Erro: <" + _linhaFonte + ">\nToken desconhecido: " + _lookAhead;
                sbLexema.Append(_lookAhead);
            }

            _lexema = sbLexema.ToString();
        }

        private void MostraToken()
        {

            StringBuilder tokenLexema = new StringBuilder("");

            switch (_token)
            {
                case Abreprog: tokenLexema.Append("T_ABREPROG"); break;
                case Fechaprog: tokenLexema.Append("T_FECHAPROG"); break;
                case AbrePar: tokenLexema.Append("T_ABRE_PAR"); break;
                case FechaPar: tokenLexema.Append("T_FECHA_PAR"); break;
                case CompilerCode.Id: tokenLexema.Append("T_ID"); break;
                case CompilerCode.Numero: tokenLexema.Append("T_NUMERO"); break;
                case Cabo: tokenLexema.Append("T_CABO"); break;
                case Memostra: tokenLexema.Append("T_MEMOSTRA"); break;
                case Mecaptura: tokenLexema.Append("T_MECAPTURA"); break;
                case Vira: tokenLexema.Append("T_VIRA"); break;
                case Soma: tokenLexema.Append("T_SOMA"); break;
                case Subtrai: tokenLexema.Append("T_SUBTRAI"); break;
                case Divide: tokenLexema.Append("T_DIVIDE"); break;
                case Multiplica: tokenLexema.Append("T_MULTIPLICA"); break;
                case Menor: tokenLexema.Append("T_MENOR"); break;
                case Menorigual: tokenLexema.Append("T_MENORIGUAL"); break;
                case Diferente: tokenLexema.Append("T_DIFERENTE"); break;
                case Maior: tokenLexema.Append("T_MAIOR"); break;
                case Maiorigual: tokenLexema.Append("T_MAIORIGUAL"); break;
                case Igual: tokenLexema.Append("T_IGUAL"); break;

                case Merepete: tokenLexema.Append("T_MEREPETE"); break;
                case RepeteDe: tokenLexema.Append("T_REPETE_DE"); break;
                case RepetePara: tokenLexema.Append("T_REPETE_PARA"); break;
                case AbreBloco: tokenLexema.Append("T_ABRE_BLOCO"); break;
                case FechaBloco: tokenLexema.Append("T_FECHA_BLOCO"); break;

                case FimFonte: tokenLexema.Append("T_FIM_FONTE"); break;
                case ErroLex: tokenLexema.Append("T_ERRO_LEX"); break;
                case Nulo: tokenLexema.Append("T_NULO"); break;
                default: tokenLexema.Append("N/A"); break;
            }

            Console.WriteLine(tokenLexema.ToString() + " ( " + _lexema + " )");

            AcumulaToken(tokenLexema.ToString() + " ( " + _lexema + " )");
            tokenLexema.Append(_lexema);
        }

        private void AcumulaToken(string tokenIdentificado)
        {
            _tokensIdentificados.Append(tokenIdentificado);
            _tokensIdentificados.Append("\n");
        }

        private void RegistraErroSintatico(string msg)
        {
            if (_estadoCompilacao == ESemErros)
            {
                _estadoCompilacao = EErroSintatico;
                _mensagemDeErro = msg;
            }
        }

        private void ShowMessageDialog(string mensagemDeErro, string message)
        {
            Console.WriteLine(mensagemDeErro, message);
        }
    }
}