using Data.Entities;
using Dolores.Http;
using Dolores.Requests;
using Dolores.Responses;
using nitwitapi.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nitwitapi.Controllers
{
    public class ToDoController : ControllerBase
    {
        public Response AddToDo()
        {
            // Deserialize
            ToDo newToDo;
            try
            {
                newToDo = Request.MessageBody.DeserializeJson<ToDo>();
            }
            catch (Exception)
            {
                var response = new Response(HttpStatusCode.BadRequest);
                response.Json(new { Message = "Request body is not a valid ToDo" });
                return response;
            }

            // Validate
            if (!IsPostContentValid(newToDo.Description))
            {
                var response = new Response(HttpStatusCode.BadRequest);
                response.Json(new { Message = "ToDo Description invalid" });
                return response;
            }

            using (var todoRepository = CreateToDoRepository())
            {
                // Save to database
                todoRepository.Insert(newToDo);
            }

            var createdResponse= new CreatedResponse(string.Empty);
            createdResponse.AddAccessControlAllowOriginHeader();

            return createdResponse;
        }

        public Response GetAllToDos()
        {
            // Read from database
            var toDos = new List<ToDo>();
            using (var todoRepository = CreateToDoRepository())
            {
                toDos = todoRepository.GetAll().ToList();
            }

            var response = GetJsonResponse(toDos);

            return response;
        }

        public Response DeleteAllToDos()
        {
            CheckPassword();

            using (var todoRepository = CreateToDoRepository())
            {
                todoRepository.DeleteAll();
            }

            return new Response(HttpStatusCode.NoContent);
        }
    }
}
