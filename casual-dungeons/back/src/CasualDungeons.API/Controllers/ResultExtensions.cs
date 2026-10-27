using CasualDungeons.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace CasualDungeons.API.Controllers;

/// <summary>
/// Traduce el tipo Result/Result&lt;T&gt; de Application al código
/// HTTP correspondiente, evitando repetir el mismo switch en cada
/// acción de cada controlador.
/// </summary>
public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.EsExitoso)
            return new NoContentResult();

        return result.TipoError switch
        {
            ResultErrorTipo.NoEncontrado => new NotFoundObjectResult(new { error = result.Error }),
            ResultErrorTipo.SinPermiso => new ObjectResult(new { error = result.Error }) { StatusCode = 403 },
            ResultErrorTipo.Conflicto => new ConflictObjectResult(new { error = result.Error }),
            _ => new BadRequestObjectResult(new { error = result.Error })
        };
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.EsExitoso)
            return new OkObjectResult(result.Valor);

        return result.TipoError switch
        {
            ResultErrorTipo.NoEncontrado => new NotFoundObjectResult(new { error = result.Error }),
            ResultErrorTipo.SinPermiso => new ObjectResult(new { error = result.Error }) { StatusCode = 403 },
            ResultErrorTipo.Conflicto => new ConflictObjectResult(new { error = result.Error }),
            _ => new BadRequestObjectResult(new { error = result.Error })
        };
    }

    /// <summary>
    /// Variante para creación exitosa (201 Created) con ubicación
    /// del recurso, usada en los endpoints POST que crean entidades.
    /// </summary>
    public static IActionResult ToCreatedResult<T>(this Result<T> result, string actionName, object routeValues)
    {
        if (!result.EsExitoso)
            return result.ToActionResult();

        return new CreatedAtActionResult(actionName, null, routeValues, result.Valor);
    }
}
