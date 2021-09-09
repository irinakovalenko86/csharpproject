using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swr.Configurator.Common.Data;
using Swr.Configurator.Common.Data.Api;
using SWR.ConfiguratorApi.Data;
using SWR.ConfiguratorApi.Data.Db;
using SWR.ConfiguratorApi.Logic;
using SWR.ConfiguratorApi.Logic.Converters;
using System.Collections.Generic;
using System.Net;
using Swr.Configurator.Common.Logic;
using SWR.ConfiguratorApi.Logic.Elastic;

namespace SWR.ConfiguratorApi.Controllers
{
	[Route(RouteHelper.RoutePrefix + RouteHelper.RouteVersion + "[controller]/[action]")]
	[ApiController]
	[Authorize]
	public class ComponentController : ControllerBase
	{
        private readonly ISettingsService _settingsService;

        public ComponentController(ISettingsService settingService)
        {
            _settingsService = settingService;
        }
		/// <summary>
		/// Добавление компонента
		/// </summary>
		/// <remarks>
		/// Простой запрос:
		///
		/// </remarks>
		/// <param name="components">Список компонентов</param>
		/// <returns>Ok</returns>
		/// <response code="200">Новые компоненты добавлены</response>
		/// <response code="400">Ошибка создания компонента с сообщением об ошибке</response>
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public IActionResult Create([FromBody] ComponentApiNew[] components)
		{
            Log(LogTypeEnum.Info, EventEnum.Create, null, "Components creation started");
            //var r = User.Claims;
            var savedComponents = new List<ComponentApiExist>();

			using (var dbController = new DbController())
			{
				dbController.Connect();

				var componentConverter = new ComponentConverter(dbController, _settingsService);

				var dbComponents = new List<Component>();

				var errorMessages = new List<string>();

				foreach (var componentObject in components)
				{
					var error = string.Empty;
					var dbComponent = componentConverter.ApiInToDbNew(componentObject, out error);

					if (dbComponent == null)
					{
                        Log(LogTypeEnum.Warning, EventEnum.Create, false, $"Component creation faild: {error}");

                        errorMessages.Add(error);
					}
					else
					{
                        dbComponents.Add(dbComponent);

						var savedComponent = new ComponentApiExist
						{
							Id = dbComponent.Id,
							CreateDate = dbComponent.CreateDate,
							ChangeDate = dbComponent.CreateDate,
							TypeName = componentObject.TypeName,
							Attributes = componentObject.Attributes
						};

						savedComponents.Add(savedComponent);

                        Log(LogTypeEnum.Info, EventEnum.Create, true, $"Component creation with id '{dbComponent.Id}' is ready");
                    }
				}

				if (errorMessages.Count != 0)
				{
					var errorMessage = string.Join(';', errorMessages);

					return new BadRequestObjectResult(new ErrorDetails { StatusCode = (int)HttpStatusCode.BadRequest, Message = errorMessage });
				}

				dbController.AddComponents(dbComponents);

				dbController.SaveClose();

                Log(LogTypeEnum.Info, EventEnum.Create, true, $"Components creation finished. Count {savedComponents.Count}");
            }

			return new JsonResult(savedComponents);
		}

		/// <summary>
		/// Поиск компонентов
		/// </summary>
		/// <param name="searchParams">Параметры поиска</param>
		/// <response code="200">Получили найденные компоненты</response>
		/// <response code="404">Недействительный URL</response>
		/// <response code="400">Неправильный формат запроса</response>
		/// <returns>Список найденных компонентов в формате Json</returns>
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public IActionResult Search([FromBody] SearchParams searchParams)
		{
            Log(LogTypeEnum.Info, EventEnum.Search, null, $"Components search started");

            // Проверки на exception в Middleware
            using (var dbController = new DbController())
			{
				dbController.Connect();

				var searchComponents = dbController.SearchComponents(searchParams, out var errorMessages);

				if (errorMessages.Count > 0)
				{
					var errorMessage = string.Join(';', errorMessages);

                    Log(LogTypeEnum.Warning, EventEnum.Search, false, $"Components search faild: {errorMessage}");

                    return new BadRequestObjectResult(new ErrorDetails { StatusCode = (int)HttpStatusCode.BadRequest, Message = errorMessage });
				}

				if (searchComponents == null) return NotFound();

				dbController.SaveClose();

                Log(LogTypeEnum.Info, EventEnum.Search, true, $"Components search finished. Count {searchComponents.Components.Count}");

                return new JsonResult(searchComponents);
			}
		}

		/// <summary>
		/// Обновление компонента
		/// </summary>
		/// <param name="components">Список обновляемых компонентов</param>
		/// <response code="200">Компоненты успешно обновлены</response>
		/// <response code="400">Ошибка обновления компонента с сообщением об ошибке</response>
		/// <response code="404">Недействительный URL</response>
		/// <returns>Ок</returns>
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public IActionResult Update([FromBody]ComponentApiExist[] components)
		{
			var savedComponents = new List<ComponentApiExist>();

            Log(LogTypeEnum.Info, EventEnum.Update, null, $"Components update started");

            using (var dbController = new DbController())
			{
				dbController.Connect();

				var componentConverter = new ComponentConverter(dbController, _settingsService);

				var dbComponents = new List<Component>();

				var errorMessages = new List<string>();

				foreach (var componentApiOut in components)
				{
					var error = string.Empty;
					var dbComponent = componentConverter.ApiToDbExists(componentApiOut, out error);

					if (dbComponent == null)
					{
						errorMessages.Add(error);

                        Log(LogTypeEnum.Warning, EventEnum.Update, false, $"Component update faild: {error}");
                    }
					else
					{
						dbComponents.Add(dbComponent);

						componentApiOut.ChangeDate = dbComponent.ChangeDate;
						componentApiOut.CreateDate = dbComponent.CreateDate;

						savedComponents.Add(componentApiOut);

                        Log(LogTypeEnum.Info, EventEnum.Update, true, $"Component update with id '{dbComponent.Id}' is ready");
                    }
				}

				if (errorMessages.Count != 0)
				{
					var errorMessage = string.Join(';', errorMessages);

					return new BadRequestObjectResult(new ErrorDetails { StatusCode = (int)HttpStatusCode.BadRequest, Message = errorMessage });
				}

				dbController.SaveComponents(dbComponents);

				dbController.SaveClose();

                Log(LogTypeEnum.Info, EventEnum.Create, true, $"Components update finished. Count {savedComponents.Count}");
            }

			return new JsonResult(savedComponents);
		}

        private void Log(LogTypeEnum logType, EventEnum eventEnum, bool? result, string message)
        {
            ElasticLogController.Log(logType, eventEnum, ObjectTypeEnum.Component, User.GetUserId(), result, this.GetIp(), message);
        }
    }
}