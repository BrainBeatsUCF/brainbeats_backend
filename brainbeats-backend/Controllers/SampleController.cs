﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using static brainbeats_backend.QueryStrings;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.Controllers {
  public class Sample {
    public string email { get; set; }
    public string name { get; set; }
    public bool isPrivate { get; set; }
    public string attributes { get; set; }
    public IFormFile audio { get; set; }
  }

  [Route("api/[controller]")]
  [ApiController]
  public class SampleController : ControllerBase {
    [HttpPost]
    [Route("create_sample")]
    public async Task<IActionResult> CreateSample([FromForm] Sample request) {
      string queryString;

      try {
        List<KeyValuePair<string, string>> edges = new List<KeyValuePair<string, string>> {
          new KeyValuePair<string, string>("OWNED_BY", request.email)
        };

        queryString = await CreateVertexQueryAsync("sample", request, edges);
      } catch {
        return BadRequest("Malformed request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("read_sample")]
    public async Task<IActionResult> ReadSample(dynamic req) {
      JObject body = DeserializeRequest(req);

      string queryString;

      try {
        queryString = ReadVertexQuery(body.GetValue("sampleId").ToString());
      } catch {
        return BadRequest("Malformed request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("get_all_samples")]
    public async Task<IActionResult> GetAllSamples(dynamic req) {
      JObject body = DeserializeRequest(req);

      string queryStringPublic;
      string queryStringPrivate;

      try {
        queryStringPublic = GetAllPublicVerticesQuery("sample");
        queryStringPrivate = GetAllPrivateVerticesQuery("sample", body.GetValue("email").ToString());
      } catch {
        return BadRequest("Malformed Request");
      }

      try {
        var resultsPublic = await DatabaseConnection.Instance.ExecuteQuery(queryStringPublic);
        var resultsPrivate = await DatabaseConnection.Instance.ExecuteQuery(queryStringPrivate);

        List<dynamic> resultList = new List<dynamic>();

        foreach (var item in resultsPublic) {
          resultList.Add(item);
        }

        foreach (var item in resultsPrivate) {
          resultList.Add(item);
        }

        return Ok(resultList);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("update_sample")]
    public async Task<IActionResult> UpdateSample(dynamic req) {
      JObject body = DeserializeRequest(req);

      string queryString;

      try {
        queryString = UpdateVertexQuery("sample", body.GetValue("sampleId").ToString(), body);
      } catch {
        return BadRequest("Malformed request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("delete_sample")]
    public async Task<IActionResult> DeleteSample(dynamic req) {
      JObject body = DeserializeRequest(req);

      string queryString;

      try {
        queryString = DeleteVertexQuery(body.GetValue("sampleId").ToString());
      } catch {
        return BadRequest("Malformed request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }
  }
}
