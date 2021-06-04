using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace CourseLibrary.API.Controllers
{
    [Route("api/authors/{authorId}/courses")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseLibraryRepository repository;
        private readonly IMapper mapper;

        public CoursesController(ICourseLibraryRepository repository, IMapper mapper)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor(Guid authorId)
        {
            if (!repository.AuthorExists(authorId))
                return NotFound();

            var courses = repository.GetCourses(authorId);
            return Ok(mapper.Map<IEnumerable<CourseDto>>(courses));
        }

        [HttpGet("{courseId}", Name = "GetCourseForAuthor")]
        public ActionResult<IEnumerable<CourseDto>> GetCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!repository.AuthorExists(authorId))
                return NotFound();

            var course = repository.GetCourse(authorId, courseId);

            if (course == null)
                return NotFound();

            return Ok(mapper.Map<CourseDto>(course));
        }

        [HttpPost]
        public ActionResult<CourseDto> CreateCourseForAuthor(Guid authorId, CourseForCreationDto course)
        {
            if (!repository.AuthorExists(authorId))
                return NotFound();

            var courseEntity = mapper.Map<Course>(course);
            repository.AddCourse(authorId, courseEntity);
            repository.Save();

            var courseToReturn = mapper.Map<CourseDto>(courseEntity);
            return CreatedAtRoute("GetCourseForAuthor", new { authorId = authorId, courseId = courseToReturn.Id }, courseToReturn);
        }

        [HttpPut("{courseId}")]
        public IActionResult UpdateCourseForAuthor(Guid authorId, Guid courseId, CourseForUpdateDto course)
        {
            if (!repository.AuthorExists(authorId))
                return NotFound();

            var courseForAuthorFromRepo = repository.GetCourse(authorId, courseId);

            if (courseForAuthorFromRepo == null)
            {
                var courseToAdd = mapper.Map<Course>(course);
                courseToAdd.Id = courseId;

                repository.AddCourse(authorId, courseToAdd);
                repository.Save();

                var courseToReturn = mapper.Map<CourseDto>(courseToAdd);

                return CreatedAtRoute("GetCourseForAuthor", new { authorId, courseId = courseToReturn.Id }, courseToReturn);
            }

            mapper.Map(course, courseForAuthorFromRepo);

            repository.UpdateCourse(courseForAuthorFromRepo);
            repository.Save();
            return NoContent();
        }

        [HttpPatch("{courseId}")]
        public ActionResult PartiallyUpdateCourseForAuthor(Guid authorId, Guid courseId, JsonPatchDocument<CourseForUpdateDto> patchDocument)
        {
            if (!repository.AuthorExists(authorId))
                return NotFound();

            var course = repository.GetCourse(authorId, courseId);

            if (course == null)
            {
                var courseDto = new CourseForUpdateDto();
                patchDocument.ApplyTo(courseDto, ModelState);

                if (!TryValidateModel(courseDto))
                    return ValidationProblem(ModelState);

                var courseToAdd = mapper.Map<Course>(courseDto);
                courseToAdd.Id = courseId;

                repository.AddCourse(authorId, courseToAdd);
                repository.Save();

                var courseToReturn = mapper.Map<CourseDto>(courseToAdd);

                return CreatedAtRoute("GetCourseForAuthor", new { authorId, courseId = courseToReturn.Id }, courseToReturn);
            }

            var courseToPatch = mapper.Map<CourseForUpdateDto>(course);
            patchDocument.ApplyTo(courseToPatch, ModelState);

            if (!TryValidateModel(courseToPatch))
                return ValidationProblem(ModelState);

            mapper.Map(courseToPatch, course);

            repository.UpdateCourse(course);
            repository.Save();
            return NoContent();
        }

        [HttpDelete("{courseId}")]
        public ActionResult DeleteCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!repository.AuthorExists(authorId))
                return NotFound();

            var courseForAuthorFromRepo = repository.GetCourse(authorId, courseId);

            if (courseForAuthorFromRepo == null)
                return NotFound();

            repository.DeleteCourse(courseForAuthorFromRepo);
            repository.Save();

            return NoContent();
        }

        public override ActionResult ValidationProblem([ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices
                .GetRequiredService<IOptions<ApiBehaviorOptions>>();

            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }
    }
}
