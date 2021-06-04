using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using AutoMapper;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Entities;

namespace CourseLibrary.API.Controllers
{
    [Route("api/authors")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository repository;
        private readonly IMapper mapper;

        public AuthorsController(ICourseLibraryRepository repository, IMapper mapper)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<AuthorDto>> GetAuthors([FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {
            var authorsFromRepo = repository.GetAuthors(authorsResourceParameters);
            return Ok(mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
        }

        [HttpGet("{authorId}", Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid authorId)
        {
            var author = repository.GetAuthor(authorId);

            if (author == null)
                return NotFound();

            return Ok(mapper.Map<AuthorDto>(author));
        }

        [HttpPost]
        public ActionResult<AuthorDto> CreateAuthor(AuthorForCreationDto author)
        {
            var authorEntity = mapper.Map<Author>(author);

            repository.AddAuthor(authorEntity);
            repository.Save();

            var authorToReturn = mapper.Map<AuthorDto>(authorEntity);
            return CreatedAtRoute("GetAuthor", new { authorId = authorToReturn.Id }, authorToReturn);
        }

        [HttpOptions]
        public IActionResult GetAuthorsOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }

        [HttpDelete("{authorId}")]
        public ActionResult DeleteAuthor(Guid authorId)
        {
            var author = repository.GetAuthor(authorId);

            if (author == null)
                return NotFound();

            repository.DeleteAuthor(author);
            repository.Save();

            return NoContent();
        }
    }
}
