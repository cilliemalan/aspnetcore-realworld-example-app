using ConduitApi.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Controllers
{
    public class TagsController : ApiController
    {
        private readonly Data.ConduitDbContext _dbContext;

        public TagsController(Data.ConduitDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("tags")]
        public Envelope<string[]> GetTags() =>
            new Envelope<string[]>
            {
                EnvelopePropertyName = "tags",
                Content = _dbContext.Tags.Select(x => x.TagName).Distinct().ToArray()
            };
    }
}
