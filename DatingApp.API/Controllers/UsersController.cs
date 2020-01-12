using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var userFromRepo = await _repo.GetUser(currentUserId);

            userParams.UserId = currentUserId;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }

            var users = await _repo.GetUsers(userParams);

            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repo.GetUser(id);

            var userToReturn = _mapper.Map<UserForDetailedDto>(user);

            return Ok(userToReturn);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(id);

            _mapper.Map(userForUpdateDto, userFromRepo);

            if (await _repo.SaveAll())
                return NoContent();

            throw new Exception($"Aktualizacja użytkownika {id} się nie powiodła");
        }

        [HttpGet("{id}/preferences")]
        public async Task<IActionResult> GetUsersPreferences(int id)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var preferencesFromRepo = await _repo.GetUsersPreferences(id);

            if (preferencesFromRepo == null) return BadRequest("Użytkownik nie ma ustawionych preferencji");

            var preferencesToReturn = _mapper.Map<PreferencesForUpdateDto>(preferencesFromRepo);

            return Ok(preferencesToReturn);
        }

        [HttpGet("{id}/template")]
        public async Task<IActionResult> GetUsersTemplate(int id)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var templateFromRepo = await _repo.GetUsersTemplate(id);

            if (templateFromRepo == null) return BadRequest("Użytkownik nie ma ustawionych cech");

            var templateToReturn = _mapper.Map<UsersTemplateForUpdateDto>(templateFromRepo);

            return Ok(templateToReturn);
        }

        [HttpPut("{id}/preferences")]
        public async Task<IActionResult> UpdateUsersPreferences(int id, PreferencesForUpdateDto preferenceForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var preferencesFromRepo = await _repo.GetUsersPreferences(id);

            if (preferencesFromRepo != null)
            {
                preferencesFromRepo = _mapper.Map(preferenceForUpdateDto, preferencesFromRepo);
            }
            else
            {
                preferencesFromRepo = new Preferences();
                preferencesFromRepo = _mapper.Map(preferenceForUpdateDto, preferencesFromRepo);
                preferencesFromRepo.UserId = id;
                _repo.Add(preferencesFromRepo);
            }

            if (await _repo.SaveAll())
                return Ok();

            throw new Exception($"Aktualizacja preferencji się nie powiodła");
        }

        [HttpPut("{id}/template")]
        public async Task<IActionResult> UpdateUsersTemplate(int id, UsersTemplateForUpdateDto usersTemplateForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var usersTemplateFromRepo = await _repo.GetUsersTemplate(id);

            if (usersTemplateFromRepo != null)
            {
                usersTemplateFromRepo = _mapper.Map(usersTemplateForUpdateDto, usersTemplateFromRepo);
            }
            else
            {
                usersTemplateFromRepo = new UsersTemplate();
                usersTemplateFromRepo = _mapper.Map(usersTemplateForUpdateDto, usersTemplateFromRepo);
                usersTemplateFromRepo.UserId = id;
                _repo.Add(usersTemplateFromRepo);
            }

            if (await _repo.SaveAll())
                return Ok();

            throw new Exception($"Aktualizacja cech się nie powiodła");
        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var like = await _repo.GetLike(id, recipientId);

            if (like != null)
                return BadRequest("Już lubisz tego użytkownika");

            if (await _repo.GetUser(recipientId) == null)
                return NotFound();

            like = new Like
            {
                LikerId = id,
                LikeeId = recipientId
            };

            _repo.Add<Like>(like);

            if (await _repo.SaveAll())
                return Ok();

            return BadRequest("Nie udało się polubić użytkownika");
        }
    }
}