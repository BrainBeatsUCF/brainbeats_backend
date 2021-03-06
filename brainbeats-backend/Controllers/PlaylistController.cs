using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using static brainbeats_backend.QueryStrings;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.Controllers
{
  public class Playlist {
    public string email { get; set; }
    public string owner { get; set; }
    public string id { get; set; }
    public string name { get; set; }
    public IFormFile image { get; set; }
    public bool isPrivate { get; set; }
    public string seed { get; set; }
  }

  [Route("api/[controller]")]
  [ApiController]
  public class PlaylistController : ControllerBase
  {
    [HttpPost]
    [Route("create_playlist")]
    public async Task<IActionResult> CreatePlaylist([FromForm] Playlist request) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }
      
      string queryString;

      try {
        string email = request.email.ToLowerInvariant();

        List<KeyValuePair<string, string>> edges = new List<KeyValuePair<string, string>> {
          new KeyValuePair<string, string>("OWNED_BY", email)
        };

        request.owner = email;

        queryString = await CreateVertexQueryAsync(request, edges);
      } catch (Exception e) {
        return BadRequest($"Malformed request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("read_playlist")]
    public async Task<IActionResult> ReadPlaylist(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      // Verify ownership
      try {
        if (!await ValidateVertexOwnershipAsync(body.GetValue("email").ToString().ToLowerInvariant(), body.GetValue("id").ToString())) {
          return BadRequest("User is not the owner of this private Playlist");
        }
      } catch (Exception e) {
        return BadRequest($"Error validating ownership: {e}");
      }

      try {
        queryString = ReadVertexQuery(body.GetValue("id").ToString());
      } catch (Exception e) {
        return BadRequest($"Malformed request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("search_playlist")]
    public async Task<IActionResult> SearchPlaylist(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = SearchVertexQuery("playlist", body.GetValue("name").ToString().ToLower());
      } catch (Exception e) {
        return BadRequest($"Malformed request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("search_public_playlists")]
    public async Task<IActionResult> SearchPublicPlaylists(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = SearchPublicVertexQuery("playlist", body.GetValue("name").ToString().ToLowerInvariant());
      } catch (Exception e) {
        return BadRequest($"Malformed request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("search_owned_playlists")]
    public async Task<IActionResult> SearchOwnedPlaylists(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = SearchOwnedVertexQuery("playlist", body.GetValue("email").ToString().ToLowerInvariant(), body.GetValue("name").ToString().ToLowerInvariant());
      } catch (Exception e) {
        return BadRequest($"Malformed request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("read_playlist_beats")]
    public async Task<IActionResult> ReadPlaylistBeats(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      // Verify ownership
      try {
        if (!await ValidateVertexOwnershipAsync(body.GetValue("email").ToString().ToLowerInvariant(), body.GetValue("id").ToString())) {
          return BadRequest("User is not the owner of this private Playlist");
        }
      } catch (Exception e) {
        return BadRequest($"Error validating ownership: {e}");
      }

      try {
        queryString = GetOutNeighborsQuery("beat", "CONTAINS", body.GetValue("id").ToString());
      } catch (Exception e) {
        return BadRequest($"Malformed request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("get_all_playlists")]
    public async Task<IActionResult> GetAllPlaylists(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryStringPublic;
      string queryStringPrivate;

      try {
        queryStringPublic = GetAllPublicVerticesQuery("playlist");
        queryStringPrivate = GetAllPrivateVerticesQuery("playlist", body.GetValue("email").ToString().ToLowerInvariant());
      } catch (Exception e) {
        return BadRequest($"Malformed Request: {e}");
      }

      try {
        var resultsPublic = await DatabaseConnection.Instance.ExecuteQuery(queryStringPublic);
        var resultsPrivate = await DatabaseConnection.Instance.ExecuteQuery(queryStringPrivate);
        return Ok(resultsPublic.Concat(resultsPrivate));
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("update_playlist")]
    public async Task<IActionResult> UpdatePlaylist([FromForm] Playlist p) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      string queryString;

      // Verify ownership
      try {
        if (!await ValidateVertexOwnershipAsync(p.email, p.id)) {
          return BadRequest("User is not the owner of this private Playlist");
        }
      } catch (Exception e) {
        return BadRequest($"Error validating ownership: {e}");
      }

      try {
        queryString = await UpdateVertexQueryAsync(p);
      } catch (Exception e) {
        return BadRequest($"Malformed Request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("update_playlist_delete_beat")]
    public async Task<IActionResult> UpdatePlaylistDeleteBeat(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      // Verify ownership
      try {
        if (!await ValidateVertexOwnershipAsync(body.GetValue("email").ToString().ToLowerInvariant(), body.GetValue("playlistId").ToString())) {
          return BadRequest("User is not the owner of this private Playlist");
        }

        if (!await ValidateVertexOwnershipAsync(body.GetValue("email").ToString().ToLowerInvariant(), body.GetValue("beatId").ToString())) {
          return BadRequest("User is not the owner of this private Beat");
        }
      } catch (Exception e) {
        return BadRequest($"Error validating ownership: {e}");
      }

      try {
        queryString = DeleteOutNeighborQuery("CONTAINS", body.GetValue("playlistId").ToString(), body.GetValue("beatId").ToString());
      } catch (Exception e) {
        return BadRequest($"Malformed Request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("update_playlist_add_beat")]
    public async Task<IActionResult> UpdatePlaylistAddBeat(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      // Verify ownership
      try {
        if (!await ValidateVertexOwnershipAsync(body.GetValue("email").ToString().ToLowerInvariant(), body.GetValue("playlistId").ToString())) {
          return BadRequest("User is not the owner of this private Playlist");
        }

        if (!await ValidateVertexOwnershipAsync(body.GetValue("email").ToString().ToLowerInvariant(), body.GetValue("beatId").ToString())) {
          return BadRequest("User is not the owner of this private Beat");
        }
      } catch (Exception e) {
        return BadRequest($"Error validating ownership: {e}");
      }

      try {
        queryString = CreateOutNeighborQuery("CONTAINS", body.GetValue("playlistId").ToString(), body.GetValue("beatId").ToString());
      } catch (Exception e) {
        return BadRequest($"Malformed request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("delete_playlist")]
    public async Task<IActionResult> DeletePlaylist(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      // Verify ownership
      try {
        if (!await ValidateVertexOwnershipAsync(body.GetValue("email").ToString().ToLowerInvariant(), body.GetValue("id").ToString())) {
          return BadRequest("User is not the owner of this private Playlist");
        }
      } catch (Exception e) {
        return BadRequest($"Error validating ownership: {e}");
      }

      try {
        // Delete the png or jpg picture associated with this Playlist
        await StorageConnection.Instance.DeleteFileAsync("playlist", body.GetValue("id").ToString() + "_image.png");
        await StorageConnection.Instance.DeleteFileAsync("playlist", body.GetValue("id").ToString() + "_image.jpg");
      } catch (Exception e) {
        return BadRequest($"Error deleting associated storage uploads for Playlist: {e}");
      }

      try {
        queryString = DeleteVertexQuery(body.GetValue("id").ToString());
      } catch (Exception e) {
        return BadRequest($"Malformed request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }
  }
}