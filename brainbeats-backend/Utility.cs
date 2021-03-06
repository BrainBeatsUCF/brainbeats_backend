﻿using brainbeats_backend.Controllers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using static brainbeats_backend.QueryStrings;
using static brainbeats_backend.QueryBuilder;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.IO;
using Gremlin.Net.Driver;

namespace brainbeats_backend {
  public static class Utility {
    // Ensures the incoming controller request is of type String
    public static string GetRequest(dynamic req) {
      return req.GetType().Equals(typeof(string)) ? req : req.ToString();
    }

    // Returns true if the email has access permissions to the specified vertex
    public static async Task<bool> ValidateVertexOwnershipAsync(string email, IDictionary<string, dynamic> vertex) {
      return await ValidateVertexOwnershipAsync(email, vertex["id"]);
    }

    public static async Task<bool> ValidateVertexOwnershipAsync(string email, string vertexId) {
      string queryString = ReadVertexQuery(vertexId);
      var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
      foreach (var itemVertex in result) {
        foreach (var field in itemVertex["properties"]["isPrivate"]) {
          if (field["value"].ToString().ToLowerInvariant().Equals("true")) {
            queryString = GetOutNeighborsQuery("user", "OWNED_BY", vertexId);
            result = await DatabaseConnection.Instance.ExecuteQuery(queryString);

            foreach (var itemOwner in result) {
              if (!itemOwner["id"].ToString().ToLowerInvariant().Equals(email.ToLowerInvariant())) {
                return false;
              }
            }
          }
        }
      }

      return true;
    }

    public static async Task<List<dynamic>> PopulateVertexOwners(dynamic vertices) {
      List<dynamic> resultList = new List<dynamic>();

      foreach (var vertex in vertices) {
        string queryString = GetOutNeighborsQuery("user", "OWNED_BY", vertex["id"].ToString());
        var owners = await DatabaseConnection.Instance.ExecuteQuery(queryString);

        foreach (var owner in owners) {
          vertex["owner"] = owner;
        }

        resultList.Add(vertex);
      }

      return resultList;
    }

    public static async Task<List<dynamic>> PopulatePlaylistLength(dynamic vertices) {
      List<dynamic> resultList = new List<dynamic>();

      foreach (var vertex in vertices) {
        if (vertex["label"].ToLowerInvariant().Equals("playlist")) {
          string queryString = GetOutNeighborsQuery("beat", "CONTAINS", vertex["id"]);
          ResultSet<dynamic> beatNeighbors = await DatabaseConnection.Instance.ExecuteQuery(queryString);
          vertex["length"] = beatNeighbors.Count;
        }

        resultList.Add(vertex);
      }

      return resultList;
    }

    // Gets the current UNIX time
    public static string GetCurrentTime() {
      int unixTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
      return unixTimestamp.ToString();
    }

    public static PropertyInfo [] GetSchema(string vertexType) {
      string type = vertexType.ToLowerInvariant().Trim();

      return type switch {
        "user" => new User().GetType().GetProperties(),
        "beat" => new Beat().GetType().GetProperties(),
        "sample" => new Sample().GetType().GetProperties(),
        "playlist" => new Playlist().GetType().GetProperties(),
        _ => null,
      };
    }

    public static dynamic DeserializeRequest(dynamic req) {
      return DeserializeRequest(req, null);
    }

    public static dynamic DeserializeRequest(dynamic req, object obj) {
      string request = GetRequest(req);

      return obj switch
      {
        Beat _ => JsonConvert.DeserializeObject<Beat>(request),
        Playlist _ => JsonConvert.DeserializeObject<Playlist>(request),
        Sample _ => JsonConvert.DeserializeObject<Sample>(request),
        User _ => JsonConvert.DeserializeObject<User>(request),
        UserProfileSettings _ => JsonConvert.DeserializeObject<UserProfileSettings>(request),
        _ => JsonConvert.DeserializeObject<dynamic>(request) as JObject,
      };
    }
  }
}
