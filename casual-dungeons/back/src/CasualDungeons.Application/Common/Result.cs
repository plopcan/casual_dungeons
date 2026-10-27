namespace CasualDungeons.Application.Common;

/// <summary>
/// Resultado de una operación de negocio que puede fallar por
/// motivos esperados (no encontrado, sin permiso, validación).
/// Evita usar excepciones para control de flujo en los servicios.
/// </summary>
public class Result
{
    public bool EsExitoso { get; }
    public string? Error { get; }
    public ResultErrorTipo? TipoError { get; }

    protected Result(bool esExitoso, string? error, ResultErrorTipo? tipoError)
    {
        EsExitoso = esExitoso;
        Error = error;
        TipoError = tipoError;
    }

    public static Result Exito() => new(true, null, null);
    public static Result Fallo(string error, ResultErrorTipo tipo = ResultErrorTipo.Validacion)
        => new(false, error, tipo);
}

public class Result<T> : Result
{
    public T? Valor { get; }

    private Result(bool esExitoso, T? valor, string? error, ResultErrorTipo? tipoError)
        : base(esExitoso, error, tipoError)
    {
        Valor = valor;
    }

    public static Result<T> Exito(T valor) => new(true, valor, null, null);

    public static new Result<T> Fallo(string error, ResultErrorTipo tipo = ResultErrorTipo.Validacion)
        => new(false, default, error, tipo);
}

public enum ResultErrorTipo
{
    Validacion,
    NoEncontrado,
    SinPermiso,
    Conflicto
}
