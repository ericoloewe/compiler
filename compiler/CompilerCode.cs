namespace Compiler
{
    internal enum CompilerCode
    {
        Abreprog = 1,
        Fechaprog = 2,
        AbrePar = 3,
        FechaPar = 4,
        Id = 5,
        Numero = 6,
        Cabo = 7,
        Memostra = 8,
        Mecaptura = 9,
        Vira = 10,
        Soma = 11,
        Subtrai = 12,
        Divide = 13,
        Multiplica = 14,
        Menor = 15,
        Menorigual = 16,
        Diferente = 17,

        Merepete = 18,
        RepeteDe = 19,
        RepetePara = 20,
        AbreBloco = 21,
        FechaBloco = 22,

        Maior = 23,
        Maiorigual = 24,
        Igual = 25,

        Mecompara = 26,
        Entao = 27,
        Senao = 28,

        Mecontinua = 29,

        FimFonte = 90,
        ErroLex = 98,
        Nulo = 99,

        ESemErros = 1,
        EErroLexico = 2,
        EErroSintatico = 3,
        EErroSemantico = 4,

        FimArquivo = 26,
    }
}
