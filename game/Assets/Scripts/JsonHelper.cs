using System;
using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json.Linq;
using Unity.Plastic.Newtonsoft.Json.Schema;
using UnityEngine;

public static class JsonHelper
{
    static JsonSchema schema = JsonSchema.Parse(
        @"{
        '$ref': '#/definitions/Container',
        'definitions': {
            'Container': {
                'type': 'object',
                'properties': {
                    'questions': {
                        'type': 'array',
                        'required': true,
                        'minItems': 1,
                        'items': {
                            '$ref': '#/definitions/Question'
                        }
                    }
                },
                'additionalProperties': false,
                'title': 'Container'
            },
            'Question': {
                'type': 'object',
                'properties': {
                    'questionText': {
                        'type': 'string',
                        'required': true
                    },
                    'answers': {
                        'type': 'array',
                        'items': {'type': 'string'},
                        'minItems': 4,
                        'maxItems': 4,
                        'required': true
                    },
                    'correctAnswers': {
                        'type': 'array',
                        'items': {'type': 'integer'},
                        'minItems': 4,
                        'maxItems': 4,
                        'required': true
                    }
                },
                'additionalProperties': false,
                'title': 'Question'
            }
        }
    }");

    public static List<T> FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.questions;
    }
    public static string ToJson<T>(List<T> list)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.questions = list;
        return JsonUtility.ToJson(wrapper, true);
    }

    public static bool ValidateJson(string json)
    {
        var jsonObject = JObject.Parse(json);
        Debug.Log(json);
        IList<string> errors;
        bool validationResult = jsonObject.IsValid(schema, out errors);
        foreach (var error in errors)
        {
            Debug.LogError("JSON Validation Error: " + error);
        }
        return validationResult;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> questions;
    }
}