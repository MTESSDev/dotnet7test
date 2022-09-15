using FRW.TR.Contrats.Composants;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace FRW.TR.Commun.Utils
{

    /// <summary>
    /// Convertisseur JSON maison pour Json.NET, force la conversion récursive en Dictionnaire.
    /// Peut aussi parser les valeurs du modèle selon le type de la configuration (liste de composants reçue en entrée)
    /// </summary>
    public class ConvertisseurFRW : CustomCreationConverter<IDictionary<object, object>>
    {
        private readonly List<ComponentBinding>? _composants;

        /// <summary>
        /// Convertisseur JSON de dictionnaires
        /// </summary>
        /// <param name="composants">Liste des composants (optionnels) pour changer le type des données selon la config.</param>
        public ConvertisseurFRW(List<ComponentBinding>? composants = null)
        {
            _composants = composants;
        }

        public override IDictionary<object, object> Create(Type objectType)
        {
            return new Dictionary<object, object>();
        }

        public override bool CanConvert(Type objectType)
        {
            // in addition to handling IDictionary<string, object>
            // we want to handle the deserialization of dict value
            // which is of type object
            return objectType == typeof(object) || base.CanConvert(objectType);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject
                || reader.TokenType == JsonToken.Null)
                return base.ReadJson(reader, objectType, existingValue, serializer);

            if (reader.TokenType == JsonToken.StartArray)
            {
                return serializer.Deserialize<object[]?>(reader);
            }

            if (_composants is { })
            {
                if (reader.TokenType == JsonToken.String)
                {
                    if (reader.Value is null)
                        return null;

                    // Enlever les [*]
                    var flattenPath = Regex.Replace(reader.Path, @"\[[0-9]*\]", "");

                    flattenPath = flattenPath.StartsWith("form")? flattenPath[5..]: flattenPath;

                    // On va tenter de parser selon le type du composant
                    switch (_composants.FirstOrDefault(e => e.FlatPath.Equals(flattenPath))?.OriginalType.ToUpper())
                    {
                        case "NUMBER":
                        case "NOMBRE":
                            if (int.TryParse((string)reader.Value!, out int num))
                                return num;

                            if (decimal.TryParse((string)reader.Value!, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal dec))
                                return dec;
                            break;
                        case "DATE":
                            if (DateTime.TryParseExact((string)reader.Value!, "yyyy-MM-dd", null, DateTimeStyles.None, out var date))
                                return date;
                            break;
                        case "DATETIME-LOCAL":
                            if (DateTime.TryParseExact((string)reader.Value!, "yyyy-MM-ddTHH:mm:ss", null, DateTimeStyles.None, out var date_local))
                                return date_local;
                            break;
                        case "MONTH":
                            if (DateTime.TryParseExact((string)reader.Value!, "yyyy-MM", null, DateTimeStyles.None, out var mois))
                                return mois;
                            break;
                        case "TIME":
                            if (DateTime.TryParseExact((string)reader.Value!, "HH:mm", null, DateTimeStyles.None, out var time))
                                return time;
                            break;
                        default:
                            if ((string)reader.Value == "true")
                                return true;

                            if ((string)reader.Value == "false")
                                return false;

                            break;
                    }
                }
            }

            return serializer.Deserialize(reader);
        }
    }
}