

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper,
            IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _cloudinaryConfig = cloudinaryConfig;
            _repo = repo;
            _mapper = mapper;

            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }
        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);

            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId,
            [FromForm]PhotoForCreationDto photoForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(userId);

            var file = photoForCreationDto.File;

            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreationDto);

            if (!userFromRepo.Photos.Any(u => u.IsMain))
            {
                photo.IsMain = true;
                FaceDto assetsFromPhoto;
                using (var faceClient = new HttpClient())
                {
                    Uri uri = new Uri("https://face-finders.cognitiveservices.azure.com/face/v1.0/detect?returnFaceId=false&returnFaceAttributes=facialHair,glasses,hair,makeup");
                    var photoParameter = "{\"url\": \"" + photo.Url + "\"}";
                    HttpContent content = new StringContent(photoParameter, Encoding.UTF8, "application/json");
                    content.Headers.Add("Ocp-Apim-Subscription-Key", "a18f920438d74a0fa740f4532d342fb4");
                    faceClient.DefaultRequestHeaders.Add("Host", "face-finders.cognitiveservices.azure.com");

                    HttpResponseMessage response = await faceClient.PostAsync(uri, content);

                    string body = await response.Content.ReadAsStringAsync();
                    List<FaceDto> facesDto = JsonConvert.DeserializeObject<List<FaceDto>>(body);
                    assetsFromPhoto = facesDto[0];
                }

                var templateFromRepo = await _repo.GetUsersTemplate(userId);

                var template = _mapper.Map<FaceForTemplateDto>(assetsFromPhoto);

                templateFromRepo.FacialHair = template.FacialHair;
                templateFromRepo.Glasses = template.Glasses;
                templateFromRepo.MakeUp = template.MakeUp;
                templateFromRepo.Hair = template.Hair;
            }

            userFromRepo.Photos.Add(photo);


            if (await _repo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
            }

            return BadRequest("Nie udało się dodać zdjęcia");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId);

            if (!user.Photos.Any(p => p.Id == id)) return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);

            if (photoFromRepo.IsMain) return BadRequest("To zdjęcie jest już ustawione jako główne");

            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
            currentMainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;

            FaceDto assetsFromPhoto;

            using (var faceClient = new HttpClient())
            {
                Uri uri = new Uri("https://face-finders.cognitiveservices.azure.com/face/v1.0/detect?returnFaceId=false&returnFaceAttributes=facialHair,glasses,hair,makeup");
                var photoParameter = "{\"url\": \"" + photoFromRepo.Url + "\"}";
                HttpContent content = new StringContent(photoParameter, Encoding.UTF8, "application/json");
                content.Headers.Add("Ocp-Apim-Subscription-Key", "a18f920438d74a0fa740f4532d342fb4");
                faceClient.DefaultRequestHeaders.Add("Host", "face-finders.cognitiveservices.azure.com");

                HttpResponseMessage response = await faceClient.PostAsync(uri, content);

                string body = await response.Content.ReadAsStringAsync();
                List<FaceDto> facesDto = JsonConvert.DeserializeObject<List<FaceDto>>(body);
                assetsFromPhoto = facesDto[0];
            }

            var templateFromRepo = await _repo.GetUsersTemplate(userId);

            var template = _mapper.Map<FaceForTemplateDto>(assetsFromPhoto);

            templateFromRepo.FacialHair = template.FacialHair;
            templateFromRepo.Glasses = template.Glasses;
            templateFromRepo.MakeUp = template.MakeUp;
            templateFromRepo.Hair = template.Hair;

            if (await _repo.SaveAll()) return NoContent();
            return BadRequest("Nie można ustawić zdjęcia jako główne");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId);

            if (!user.Photos.Any(p => p.Id == id)) return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);

            if (photoFromRepo.IsMain) return BadRequest("Nie możesz usunąć głównego zdjęcia");

            if (photoFromRepo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);
                var result = _cloudinary.Destroy(deleteParams);

                if (result.Result == "ok") _repo.Delete(photoFromRepo);

                if (await _repo.SaveAll()) return Ok();
            }

            if (photoFromRepo.PublicId == null)
            {
                _repo.Delete(photoFromRepo);
            }

            if (await _repo.SaveAll()) return Ok();

            return BadRequest("Nie udało sie usunąć zdjęcia");
        }

    }
}