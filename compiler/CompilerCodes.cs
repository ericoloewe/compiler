namespace compiler
{
    public enum CompilerCodes
    {
        T_ABREPROG = 1,
        T_FECHAPROG = 2,
        T_ABRE_PAR = 3,
        T_FECHA_PAR = 4,
        T_ID = 5,
        T_NUMERO = 6,
        T_CABO = 7,
        T_MEMOSTRA = 8,
        T_MECAPTURA = 9,
        T_VIRA = 10,
        T_SOMA = 11,
        T_SUBTRAI = 12,
        T_DIVIDE = 13,
        T_MULTIPLICA = 14,
        T_MENOR = 15,
        T_MENORIGUAL = 16,
        T_DIFERENTE = 17,

        T_MEREPETE = 18,
        T_REPETE_DE = 19,
        T_REPETE_PARA = 20,
        T_ABRE_BLOCO = 21,
        T_FECHA_BLOCO = 22,

        T_MAIOR = 23,
        T_MAIORIGUAL = 24,
        T_IGUAL = 25,

        T_MECOMPARA = 26,
        T_ENTAO = 27,
        T_SENAO = 28,

        T_MECONTINUA = 29,
        T_ELEVADO = 30,

        T_FIM_FONTE = 90,
        T_ERRO_LEX = 98,
        T_NULO = 99,

        E_SEM_ERROS = 1,
        E_ERRO_LEXICO = 2,
        E_ERRO_SINTATICO = 3,
        E_ERRO_SEMANTICO = 4,

        FIM_ARQUIVO = 26,
    }
}
