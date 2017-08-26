﻿
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core;
using Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using X.Web.MetaExtractor;

namespace WebSite.Controllers
{
    public class ApiController : Controller
    {
        private readonly PublicationManager _publicationManager;
        private readonly UserManager _userManager;

        public ApiController(IMemoryCache cache)
        {
            _publicationManager = new PublicationManager(Core.Settings.Current.ConnectionString, cache);
            _userManager = new UserManager(Core.Settings.Current.ConnectionString);
        }

        [HttpGet]
        [Route("api/publications/new")]
        public async Task<IActionResult> AddPublicaton(string url, Guid key, int categoryId = 1)
        {
            DAL.User user = _userManager.GetBySecretKey(key);

            if (user == null)
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var extractor = new X.Web.MetaExtractor.Extractor();

            var metadata = await extractor.Extract(new Uri(url));


            var publication = new DAL.Publication
            {
                Title = metadata.Title,
                Description = metadata.Description,
                Link = metadata.Url,
                Image = metadata.Image.FirstOrDefault(),
                Type = metadata.Type,
                DateTime = DateTime.Now,
                UserId = user.Id,
                CategoryId = categoryId
            };

            publication = await _publicationManager.Save(publication);

            if (publication != null)
            {
                var model = new PublicationViewModel(publication, Settings.Current.WebSiteUrl);
                return Created(new Uri($"{Core.Settings.Current.WebSiteUrl}post/{publication.Id}"), model);
            }

            return StatusCode((int)HttpStatusCode.BadRequest);
        }

    }
}
