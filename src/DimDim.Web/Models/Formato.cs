using System.Globalization;

namespace DimDim.Web.Models;

// Formatação pt-BR usada nas telas
public static class Formato
{
    private static readonly CultureInfo Br = new("pt-BR");

    public static string Moeda(decimal valor) => valor.ToString("C", Br);
    public static string Data(DateTime data) => data.ToString("dd/MM/yyyy HH:mm", Br);
    public static string Cpf(string cpf) =>
        cpf.Length == 11 ? $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}" : cpf;
    public static string Tipo(string tipo) => tipo == "Poupanca" ? "Poupança" : tipo;
}
