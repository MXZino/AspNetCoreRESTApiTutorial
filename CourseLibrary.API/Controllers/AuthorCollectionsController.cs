using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseLibrary.API.Controllers
{
    [Route("api/authorcollections")]
    [ApiController]
    public class AuthorCollectionsController : ControllerBase
    {
        private readonly ICourseLibraryRepository repository;
        private readonly IMapper mapper;

        public AuthorCollectionsController(ICourseLibraryRepository repository, IMapper mapper)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        //array key: 1,2,3
        //composite key: key1=value1,key2=value2

        [HttpGet("({ids})", Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection([FromRoute] [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
                return BadRequest();

            var authorEntities = repository.GetAuthors(ids);

            if (ids.Count() != authorEntities.Count())
                return NotFound();

            var authorsToReturn = mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

            return Ok(authorsToReturn);
        }

        [HttpPost]
        public ActionResult<IEnumerable<AuthorDto>> CreateAuthorCollection(IEnumerable<AuthorForCreationDto> authorCollection)
        {
            var authorEntities = mapper.Map<IEnumerable<Author>>(authorCollection);

            foreach (var author in authorEntities)
                repository.AddAuthor(author);

            repository.Save();

            var authorsToReturn = mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            var idsAsString = string.Join(",", authorsToReturn.Select(a => a.Id));

            return CreatedAtRoute("GetAuthorCollection", new { ids = idsAsString}, authorsToReturn);
        }
    }
}
